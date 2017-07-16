using System.Windows.Forms;
using Chummer.UI.Powers;
using Chummer.UI.Skills;
using ComboBox = Chummer.helpers.ComboBox;
using TreeView = Chummer.helpers.TreeView;

namespace Chummer
{
    partial class frmCareer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmCareer));
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
            System.Windows.Forms.TreeNode treeNode16 = new System.Windows.Forms.TreeNode("Critter Powers");
            System.Windows.Forms.TreeNode treeNode17 = new System.Windows.Forms.TreeNode("Weaknesses");
            System.Windows.Forms.TreeNode treeNode18 = new System.Windows.Forms.TreeNode("Selected AI Programs and Advanced Programs");
            System.Windows.Forms.TreeNode treeNode19 = new System.Windows.Forms.TreeNode("Selected Cyberware");
            System.Windows.Forms.TreeNode treeNode20 = new System.Windows.Forms.TreeNode("Selected Bioware");
            System.Windows.Forms.TreeNode treeNode21 = new System.Windows.Forms.TreeNode("Selected Lifestyles");
            System.Windows.Forms.TreeNode treeNode22 = new System.Windows.Forms.TreeNode("Selected Armor");
            System.Windows.Forms.TreeNode treeNode23 = new System.Windows.Forms.TreeNode("Selected Weapons");
            System.Windows.Forms.TreeNode treeNode24 = new System.Windows.Forms.TreeNode("Selected Gear");
            System.Windows.Forms.TreeNode treeNode25 = new System.Windows.Forms.TreeNode("Selected Vehicles");
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.TreeNode treeNode26 = new System.Windows.Forms.TreeNode("Selected Improvements");
            this.lblTraditionSource = new System.Windows.Forms.Label();
            this.lblTraditionSourceLabel = new System.Windows.Forms.Label();
            this.tabPowerUc = new Chummer.UI.Powers.PowersTabUserControl();
            this.StatusStrip = new System.Windows.Forms.StatusStrip();
            this.tssKarmaLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tssKarma = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tssEssence = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel4 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tssNuyen = new System.Windows.Forms.ToolStripStatusLabel();
            this.pgbProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.dlgSaveFile = new System.Windows.Forms.SaveFileDialog();
            this.tipTooltip = new TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip();
            this.lblCMStunLabel = new System.Windows.Forms.Label();
            this.lblCMPhysicalLabel = new System.Windows.Forms.Label();
            this.lblRemainingNuyenLabel = new System.Windows.Forms.Label();
            this.lblESS = new System.Windows.Forms.Label();
            this.lblCareerKarmaLabel = new System.Windows.Forms.Label();
            this.lblMemoryLabel = new System.Windows.Forms.Label();
            this.lblLiftCarryLabel = new System.Windows.Forms.Label();
            this.lblJudgeIntentionsLabel = new System.Windows.Forms.Label();
            this.lblComposureLabel = new System.Windows.Forms.Label();
            this.lblCMPenaltyLabel = new System.Windows.Forms.Label();
            this.lblCMArmorLabel = new System.Windows.Forms.Label();
            this.lblCMDamageResistancePoolLabel = new System.Windows.Forms.Label();
            this.lblCareerNuyenLabel = new System.Windows.Forms.Label();
            this.lblArmorLabel = new System.Windows.Forms.Label();
            this.lblRiggingINILabel = new System.Windows.Forms.Label();
            this.lblMatrixINIHotLabel = new System.Windows.Forms.Label();
            this.lblMatrixINIColdLabel = new System.Windows.Forms.Label();
            this.lblAstralINILabel = new System.Windows.Forms.Label();
            this.lblMatrixINILabel = new System.Windows.Forms.Label();
            this.lblINILabel = new System.Windows.Forms.Label();
            this.cmdEdgeGained = new System.Windows.Forms.Button();
            this.cmdEdgeSpent = new System.Windows.Forms.Button();
            this.lblCounterspellingDiceLabel = new System.Windows.Forms.Label();
            this.lbllSpellDefenceManipPhysicalLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceManipMentalLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceIllusionPhysicalLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceIllusionManaLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceDecAttWILLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceDecAttLOGLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceDecAttINTLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceDecAttCHALabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceDecAttSTRLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceDecAttREALabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceDecAttAGILabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceDecAttBODLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceDetectionLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceDirectSoakPhysicalLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceDirectSoakManaLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceIndirectSoakLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceIndirectDodgeLabel = new System.Windows.Forms.Label();
            this.lblStreetCred = new System.Windows.Forms.Label();
            this.lblNotoriety = new System.Windows.Forms.Label();
            this.cmdBurnStreetCred = new System.Windows.Forms.Button();
            this.cmdVehicleGearReduceQty = new System.Windows.Forms.Button();
            this.cmdVehicleMoveToInventory = new System.Windows.Forms.Button();
            this.chkVehicleWeaponAccessoryInstalled = new System.Windows.Forms.CheckBox();
            this.cmdGearReduceQty = new System.Windows.Forms.Button();
            this.cmdGearIncreaseQty = new System.Windows.Forms.Button();
            this.cmdGearSplitQty = new System.Windows.Forms.Button();
            this.cmdGearMergeQty = new System.Windows.Forms.Button();
            this.cmdGearMoveToVehicle = new System.Windows.Forms.Button();
            this.lblFoci = new System.Windows.Forms.Label();
            this.cmdWeaponBuyAmmo = new System.Windows.Forms.Button();
            this.cmdWeaponMoveToVehicle = new System.Windows.Forms.Button();
            this.chkWeaponAccessoryInstalled = new System.Windows.Forms.CheckBox();
            this.cmdArmorDecrease = new System.Windows.Forms.Button();
            this.cmdArmorIncrease = new System.Windows.Forms.Button();
            this.chkArmorEquipped = new System.Windows.Forms.CheckBox();
            this.cmdDecreaseLifestyleMonths = new System.Windows.Forms.Button();
            this.cmdIncreaseLifestyleMonths = new System.Windows.Forms.Button();
            this.lblAttributesAug = new System.Windows.Forms.Label();
            this.lblAttributesMetatype = new System.Windows.Forms.Label();
            this.lblAttributes = new System.Windows.Forms.Label();
            this.lblMovementLabel = new System.Windows.Forms.Label();
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
            this.cmsDeleteCyberware = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsCyberwareSell = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsLifestyle = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsAdvancedLifestyle = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsArmor = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsAddArmorMod = new System.Windows.Forms.ToolStripMenuItem();
            this.tsAddArmorGear = new System.Windows.Forms.ToolStripMenuItem();
            this.tsArmorName = new System.Windows.Forms.ToolStripMenuItem();
            this.tsArmorNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsDeleteArmor = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsArmorSell = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsWeapon = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsCreateNaturalWeapon = new System.Windows.Forms.ToolStripMenuItem();
            this.tsWeaponAddAccessory = new System.Windows.Forms.ToolStripMenuItem();
            this.tsWeaponAddUnderbarrel = new System.Windows.Forms.ToolStripMenuItem();
            this.tsWeaponName = new System.Windows.Forms.ToolStripMenuItem();
            this.tsWeaponNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsDeleteWeapon = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsWeaponSell = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsAmmoExpense = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cmsAmmoSingleShot = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsAmmoShortBurst = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsAmmoLongBurst = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsAmmoFullBurst = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsAmmoSuppressiveFire = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsGearButton = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsGearButtonAddAccessory = new System.Windows.Forms.ToolStripMenuItem();
            this.tsGearAddNexus = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsDeleteGear = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.sellItemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsVehicle = new System.Windows.Forms.ContextMenuStrip(this.components);
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
            this.cmdVehicleAmmoExpense = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cmsVehicleAmmoSingleShot = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsVehicleAmmoShortBurst = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsVehicleAmmoLongBurst = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsVehicleAmmoFullBurst = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsVehicleAmmoSuppressiveFire = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsDeleteVehicle = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsVehicleSell = new System.Windows.Forms.ToolStripMenuItem();
            this.tsWeaponAddModification = new System.Windows.Forms.ToolStripMenuItem();
            this.tsBoltHole = new System.Windows.Forms.ToolStripMenuItem();
            this.tsSafehouse = new System.Windows.Forms.ToolStripMenuItem();
            this.lblArmor = new System.Windows.Forms.Label();
            this.panStunCM = new System.Windows.Forms.Panel();
            this.lblStunCMLabel = new System.Windows.Forms.Label();
            this.chkStunCM18 = new System.Windows.Forms.CheckBox();
            this.chkStunCM17 = new System.Windows.Forms.CheckBox();
            this.chkStunCM16 = new System.Windows.Forms.CheckBox();
            this.chkStunCM15 = new System.Windows.Forms.CheckBox();
            this.chkStunCM14 = new System.Windows.Forms.CheckBox();
            this.chkStunCM13 = new System.Windows.Forms.CheckBox();
            this.chkStunCM12 = new System.Windows.Forms.CheckBox();
            this.chkStunCM11 = new System.Windows.Forms.CheckBox();
            this.chkStunCM10 = new System.Windows.Forms.CheckBox();
            this.chkStunCM9 = new System.Windows.Forms.CheckBox();
            this.chkStunCM8 = new System.Windows.Forms.CheckBox();
            this.chkStunCM7 = new System.Windows.Forms.CheckBox();
            this.chkStunCM6 = new System.Windows.Forms.CheckBox();
            this.chkStunCM5 = new System.Windows.Forms.CheckBox();
            this.chkStunCM4 = new System.Windows.Forms.CheckBox();
            this.chkStunCM3 = new System.Windows.Forms.CheckBox();
            this.chkStunCM2 = new System.Windows.Forms.CheckBox();
            this.chkStunCM1 = new System.Windows.Forms.CheckBox();
            this.panPhysicalCM = new System.Windows.Forms.Panel();
            this.chkPhysicalCM24 = new System.Windows.Forms.CheckBox();
            this.chkPhysicalCM23 = new System.Windows.Forms.CheckBox();
            this.chkPhysicalCM22 = new System.Windows.Forms.CheckBox();
            this.chkPhysicalCM21 = new System.Windows.Forms.CheckBox();
            this.chkPhysicalCM20 = new System.Windows.Forms.CheckBox();
            this.chkPhysicalCM19 = new System.Windows.Forms.CheckBox();
            this.chkPhysicalCM18 = new System.Windows.Forms.CheckBox();
            this.lblPhysicalCMLabel = new System.Windows.Forms.Label();
            this.chkPhysicalCM17 = new System.Windows.Forms.CheckBox();
            this.chkPhysicalCM16 = new System.Windows.Forms.CheckBox();
            this.chkPhysicalCM15 = new System.Windows.Forms.CheckBox();
            this.chkPhysicalCM14 = new System.Windows.Forms.CheckBox();
            this.chkPhysicalCM13 = new System.Windows.Forms.CheckBox();
            this.chkPhysicalCM12 = new System.Windows.Forms.CheckBox();
            this.chkPhysicalCM11 = new System.Windows.Forms.CheckBox();
            this.chkPhysicalCM10 = new System.Windows.Forms.CheckBox();
            this.chkPhysicalCM9 = new System.Windows.Forms.CheckBox();
            this.chkPhysicalCM8 = new System.Windows.Forms.CheckBox();
            this.chkPhysicalCM7 = new System.Windows.Forms.CheckBox();
            this.chkPhysicalCM6 = new System.Windows.Forms.CheckBox();
            this.chkPhysicalCM5 = new System.Windows.Forms.CheckBox();
            this.chkPhysicalCM4 = new System.Windows.Forms.CheckBox();
            this.chkPhysicalCM3 = new System.Windows.Forms.CheckBox();
            this.chkPhysicalCM2 = new System.Windows.Forms.CheckBox();
            this.chkPhysicalCM1 = new System.Windows.Forms.CheckBox();
            this.tabInfo = new System.Windows.Forms.TabControl();
            this.tabOtherInfo = new System.Windows.Forms.TabPage();
            this.lblRiggingINI = new System.Windows.Forms.Label();
            this.lblMatrixINIHot = new System.Windows.Forms.Label();
            this.lblMatrixINICold = new System.Windows.Forms.Label();
            this.lblAstralINI = new System.Windows.Forms.Label();
            this.lblMatrixINI = new System.Windows.Forms.Label();
            this.lblINI = new System.Windows.Forms.Label();
            this.lblCareerNuyen = new System.Windows.Forms.Label();
            this.lblFly = new System.Windows.Forms.Label();
            this.lblFlyLabel = new System.Windows.Forms.Label();
            this.lblSwim = new System.Windows.Forms.Label();
            this.lblSwimLabel = new System.Windows.Forms.Label();
            this.lblMemory = new System.Windows.Forms.Label();
            this.lblLiftCarry = new System.Windows.Forms.Label();
            this.lblJudgeIntentions = new System.Windows.Forms.Label();
            this.lblComposure = new System.Windows.Forms.Label();
            this.lblMovement = new System.Windows.Forms.Label();
            this.lblCareerKarma = new System.Windows.Forms.Label();
            this.lblRemainingNuyen = new System.Windows.Forms.Label();
            this.lblESSMax = new System.Windows.Forms.Label();
            this.lblCMStun = new System.Windows.Forms.Label();
            this.lblCMPhysical = new System.Windows.Forms.Label();
            this.tabConditionMonitor = new System.Windows.Forms.TabPage();
            this.lblEDGInfo = new System.Windows.Forms.Label();
            this.lblCMDamageResistancePool = new System.Windows.Forms.Label();
            this.lblCMArmor = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblCMPenalty = new System.Windows.Forms.Label();
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
            this.mnuCreateMenu = new System.Windows.Forms.MenuStrip();
            this.mnuCreateFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileSave = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFileClose = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFilePrint = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileExport = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCreateEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCreateSpecial = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialAddCyberwareSuite = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialAddBiowareSuite = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialCyberzombie = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialConvertToFreeSprite = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialReduceAttribute = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialPossess = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialPossessInanimate = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialReapplyImprovements = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialCloningMachine = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.tsbSave = new System.Windows.Forms.ToolStripButton();
            this.tsbPrint = new System.Windows.Forms.ToolStripButton();
            this.tsbSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.tsbCopy = new System.Windows.Forms.ToolStripButton();
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
            this.cmsUndoKarmaExpense = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsUndoKarmaExpense = new System.Windows.Forms.ToolStripMenuItem();
            this.tsEditKarmaExpense = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsUndoNuyenExpense = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsUndoNuyenExpense = new System.Windows.Forms.ToolStripMenuItem();
            this.tsEditNuyenExpense = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsArmorGear = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsArmorGearAddAsPlugin = new System.Windows.Forms.ToolStripMenuItem();
            this.tsArmorGearNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsArmorMod = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsArmorModNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsQuality = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsQualityNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsMartialArtManeuver = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsMartialArtManeuverNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsSpell = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsSpellNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsCritterPowers = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsCritterPowersNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsMetamagic = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsMetamagicAddArt = new System.Windows.Forms.ToolStripMenuItem();
            this.tsMetamagicAddEnchantment = new System.Windows.Forms.ToolStripMenuItem();
            this.tsMetamagicAddEnhancement = new System.Windows.Forms.ToolStripMenuItem();
            this.tsMetamagicAddMetamagic = new System.Windows.Forms.ToolStripMenuItem();
            this.tsMetamagicAddRitual = new System.Windows.Forms.ToolStripMenuItem();
            this.tsMetamagicNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsLifestyleNotes = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsEditLifestyle = new System.Windows.Forms.ToolStripMenuItem();
            this.tsLifestyleName = new System.Windows.Forms.ToolStripMenuItem();
            this.tsLifestyleNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsWeaponMod = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsWeaponModNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsWeaponAccessory = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsWeaponAccessoryAddGear = new System.Windows.Forms.ToolStripMenuItem();
            this.tsWeaponAccessoryNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsGearPlugin = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsGearPluginNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsComplexFormPlugin = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsComplexFormPluginNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.tabCharacterTabs = new System.Windows.Forms.TabControl();
            this.tabCommon = new System.Windows.Forms.TabPage();
            this.pnlAttributes = new System.Windows.Forms.FlowLayoutPanel();
            this.tabPeople = new System.Windows.Forms.TabControl();
            this.tabContacts = new System.Windows.Forms.TabPage();
            this.panContacts = new System.Windows.Forms.FlowLayoutPanel();
            this.cmdAddContact = new System.Windows.Forms.Button();
            this.lblContactArchtypeLabel = new System.Windows.Forms.Label();
            this.lblContactNameLabel = new System.Windows.Forms.Label();
            this.lblContactLocationLabel = new System.Windows.Forms.Label();
            this.tabEnemies = new System.Windows.Forms.TabPage();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.panEnemies = new System.Windows.Forms.FlowLayoutPanel();
            this.cmdAddEnemy = new System.Windows.Forms.Button();
            this.lblPossessed = new System.Windows.Forms.Label();
            this.txtAlias = new System.Windows.Forms.TextBox();
            this.lblAlias = new System.Windows.Forms.Label();
            this.lblMetatypeSource = new System.Windows.Forms.Label();
            this.lblMetatypeSourceLabel = new System.Windows.Forms.Label();
            this.cmdSwapQuality = new System.Windows.Forms.Button();
            this.lblQualityBP = new System.Windows.Forms.Label();
            this.lblQualityBPLabel = new System.Windows.Forms.Label();
            this.lblQualitySource = new System.Windows.Forms.Label();
            this.lblQualitySourceLabel = new System.Windows.Forms.Label();
            this.cmdDeleteQuality = new System.Windows.Forms.Button();
            this.cmdAddQuality = new System.Windows.Forms.Button();
            this.treQualities = new Chummer.helpers.TreeView();
            this.lblMysticAdeptAssignment = new System.Windows.Forms.Label();
            this.lblMysticAdeptMAGAdept = new System.Windows.Forms.Label();
            this.lblMetatype = new System.Windows.Forms.Label();
            this.lblMetatypeLabel = new System.Windows.Forms.Label();
            this.tabSkills = new System.Windows.Forms.TabPage();
            this.tabSkillsUc = new Chummer.UI.Skills.SkillsTabUserControl();
            this.tabLimits = new System.Windows.Forms.TabPage();
            this.lblAstralLabel = new System.Windows.Forms.Label();
            this.lblAstral = new System.Windows.Forms.Label();
            this.lblSocialLimitLabel = new System.Windows.Forms.Label();
            this.lblSocial = new System.Windows.Forms.Label();
            this.lblMentalLimitLabel = new System.Windows.Forms.Label();
            this.lblMental = new System.Windows.Forms.Label();
            this.lblPhysicalLimitLabel = new System.Windows.Forms.Label();
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
            this.cboSpiritManipulation = new System.Windows.Forms.ComboBox();
            this.lblSpiritManipulation = new System.Windows.Forms.Label();
            this.cboSpiritIllusion = new System.Windows.Forms.ComboBox();
            this.lblSpiritIllusion = new System.Windows.Forms.Label();
            this.cboSpiritHealth = new System.Windows.Forms.ComboBox();
            this.lblSpiritHealth = new System.Windows.Forms.Label();
            this.cboSpiritDetection = new System.Windows.Forms.ComboBox();
            this.lblSpiritDetection = new System.Windows.Forms.Label();
            this.cboSpiritCombat = new System.Windows.Forms.ComboBox();
            this.lblSpiritCombat = new System.Windows.Forms.Label();
            this.cboDrain = new System.Windows.Forms.ComboBox();
            this.txtTraditionName = new System.Windows.Forms.TextBox();
            this.lblTraditionName = new System.Windows.Forms.Label();
            this.cmdQuickenSpell = new System.Windows.Forms.Button();
            this.lblSpellDicePool = new System.Windows.Forms.Label();
            this.lblSpellDicePoolLabel = new System.Windows.Forms.Label();
            this.lblMentorSpirit = new System.Windows.Forms.Label();
            this.lblMentorSpiritLabel = new System.Windows.Forms.Label();
            this.lblMentorSpiritInformation = new System.Windows.Forms.Label();
            this.cboTradition = new System.Windows.Forms.ComboBox();
            this.lblDrainAttributesValue = new System.Windows.Forms.Label();
            this.lblDrainAttributes = new System.Windows.Forms.Label();
            this.lblDrainAttributesLabel = new System.Windows.Forms.Label();
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
            this.lblSpirits = new System.Windows.Forms.Label();
            this.panSpirits = new System.Windows.Forms.Panel();
            this.lblSelectedSpells = new System.Windows.Forms.Label();
            this.cmdRollDrain = new System.Windows.Forms.Button();
            this.cmdRollSpell = new System.Windows.Forms.Button();
            this.cmdAddSpell = new SplitButton();
            this.tabAdept = new System.Windows.Forms.TabPage();
            this.tabTechnomancer = new System.Windows.Forms.TabPage();
            this.lblFV = new System.Windows.Forms.Label();
            this.lblFVLabel = new System.Windows.Forms.Label();
            this.lblDuration = new System.Windows.Forms.Label();
            this.lblDurationLabel = new System.Windows.Forms.Label();
            this.lblTarget = new System.Windows.Forms.Label();
            this.lblTargetLabel = new System.Windows.Forms.Label();
            this.lblComplexFormSource = new System.Windows.Forms.Label();
            this.lblComplexFormSourceLabel = new System.Windows.Forms.Label();
            this.lblLivingPersonaFirewall = new System.Windows.Forms.Label();
            this.lblLivingPersonaFirewallLabel = new System.Windows.Forms.Label();
            this.lblLivingPersonaDataProcessing = new System.Windows.Forms.Label();
            this.lblLivingPersonaDataProcessingLabel = new System.Windows.Forms.Label();
            this.lblLivingPersonaSleaze = new System.Windows.Forms.Label();
            this.lblLivingPersonaSleazeLabel = new System.Windows.Forms.Label();
            this.lblLivingPersonaAttack = new System.Windows.Forms.Label();
            this.lblLivingPersonaAttackLabel = new System.Windows.Forms.Label();
            this.lblLivingPersonaLabel = new System.Windows.Forms.Label();
            this.lblLivingPersonaDeviceRating = new System.Windows.Forms.Label();
            this.lblLivingPersonaDeviceRatingLabel = new System.Windows.Forms.Label();
            this.cmdRollFading = new System.Windows.Forms.Button();
            this.cboStream = new System.Windows.Forms.ComboBox();
            this.lblFadingAttributesValue = new System.Windows.Forms.Label();
            this.lblFadingAttributes = new System.Windows.Forms.Label();
            this.lblFadingAttributesLabel = new System.Windows.Forms.Label();
            this.lblStreamLabel = new System.Windows.Forms.Label();
            this.treComplexForms = new Chummer.helpers.TreeView();
            this.cmdDeleteComplexForm = new System.Windows.Forms.Button();
            this.lblComplexForms = new System.Windows.Forms.Label();
            this.cmdAddSprite = new System.Windows.Forms.Button();
            this.lblSprites = new System.Windows.Forms.Label();
            this.panSprites = new System.Windows.Forms.Panel();
            this.cmdAddComplexForm = new SplitButton();
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
            this.tabAdvancedPrograms = new System.Windows.Forms.TabPage();
            this.cmdAddAIProgram = new System.Windows.Forms.Button();
            this.lblAIProgramsRequires = new System.Windows.Forms.Label();
            this.lblAIProgramsRequiresLabel = new System.Windows.Forms.Label();
            this.lblAIProgramsSource = new System.Windows.Forms.Label();
            this.lblAIProgramsSourceLabel = new System.Windows.Forms.Label();
            this.treAIPrograms = new Chummer.helpers.TreeView();
            this.cmdDeleteAIProgram = new System.Windows.Forms.Button();
            this.lblAIProgramsAdvancedPrograms = new System.Windows.Forms.Label();
            this.tabInitiation = new System.Windows.Forms.TabPage();
            this.chkInitiationSchooling = new System.Windows.Forms.CheckBox();
            this.chkInitiationOrdeal = new System.Windows.Forms.CheckBox();
            this.chkInitiationGroup = new System.Windows.Forms.CheckBox();
            this.chkJoinGroup = new System.Windows.Forms.CheckBox();
            this.txtGroupNotes = new System.Windows.Forms.TextBox();
            this.txtGroupName = new System.Windows.Forms.TextBox();
            this.lblGroupNotes = new System.Windows.Forms.Label();
            this.lblGroupName = new System.Windows.Forms.Label();
            this.lblMetamagicSource = new System.Windows.Forms.Label();
            this.lblMetamagicSourceLabel = new System.Windows.Forms.Label();
            this.treMetamagic = new System.Windows.Forms.TreeView();
            this.cmdAddMetamagic = new System.Windows.Forms.Button();
            this.tabCyberware = new System.Windows.Forms.TabPage();
            this.lblCyberlimbSTR = new System.Windows.Forms.Label();
            this.lblCyberlimbAGI = new System.Windows.Forms.Label();
            this.lblCyberlimbSTRLabel = new System.Windows.Forms.Label();
            this.lblCyberlimbAGILabel = new System.Windows.Forms.Label();
            this.cboCyberwareGearOverclocker = new System.Windows.Forms.ComboBox();
            this.lblCyberwareGearOverclocker = new System.Windows.Forms.Label();
            this.cboCyberwareGearDataProcessing = new System.Windows.Forms.ComboBox();
            this.cboCyberwareGearFirewall = new System.Windows.Forms.ComboBox();
            this.cboCyberwareGearSleaze = new System.Windows.Forms.ComboBox();
            this.cboCyberwareGearAttack = new System.Windows.Forms.ComboBox();
            this.tabCyberwareCM = new System.Windows.Forms.TabControl();
            this.tabCyberwareMatrixCM = new System.Windows.Forms.TabPage();
            this.chkCyberwareMatrixCM1 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM2 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM3 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM4 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM5 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM6 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM7 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM8 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM9 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM10 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM11 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM12 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM13 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM14 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM15 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM16 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM17 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM18 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM19 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM20 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM21 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM22 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM23 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM24 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM25 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM26 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM27 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM28 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM29 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM30 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM31 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM32 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM33 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM34 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM35 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM36 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM37 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM38 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM39 = new System.Windows.Forms.CheckBox();
            this.chkCyberwareMatrixCM40 = new System.Windows.Forms.CheckBox();
            this.lblCyberFirewallLabel = new System.Windows.Forms.Label();
            this.lblCyberDataProcessingLabel = new System.Windows.Forms.Label();
            this.lblCyberSleazeLabel = new System.Windows.Forms.Label();
            this.lblCyberAttackLabel = new System.Windows.Forms.Label();
            this.lblCyberDeviceRating = new System.Windows.Forms.Label();
            this.lblCyberDeviceRatingLabel = new System.Windows.Forms.Label();
            this.lblEssenceHoleESS = new System.Windows.Forms.Label();
            this.lblEssenceHoleESSLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblBiowareESS = new System.Windows.Forms.Label();
            this.lblCyberwareESS = new System.Windows.Forms.Label();
            this.lblBiowareESSLabel = new System.Windows.Forms.Label();
            this.lblCyberwareESSLabel = new System.Windows.Forms.Label();
            this.lblCyberwareRating = new System.Windows.Forms.Label();
            this.lblCyberwareGrade = new System.Windows.Forms.Label();
            this.lblCyberwareSource = new System.Windows.Forms.Label();
            this.lblCyberwareSourceLabel = new System.Windows.Forms.Label();
            this.cmdAddBioware = new System.Windows.Forms.Button();
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
            this.cmdDeleteCyberware = new SplitButton();
            this.tabStreetGear = new System.Windows.Forms.TabPage();
            this.tabStreetGearTabs = new System.Windows.Forms.TabControl();
            this.tabLifestyle = new System.Windows.Forms.TabPage();
            this.cmdAddLifestyle = new SplitButton();
            this.lblBaseLifestyle = new System.Windows.Forms.Label();
            this.lblLifestyleComfortsLabel = new System.Windows.Forms.Label();
            this.lblLifestyleQualities = new System.Windows.Forms.Label();
            this.lblLifestyleQualitiesLabel = new System.Windows.Forms.Label();
            this.lblLifestyleMonths = new System.Windows.Forms.Label();
            this.lblLifestyleSource = new System.Windows.Forms.Label();
            this.lblLifestyleSourceLabel = new System.Windows.Forms.Label();
            this.lblLifestyleCostLabel = new System.Windows.Forms.Label();
            this.treLifestyles = new Chummer.helpers.TreeView();
            this.lblLifestyleCost = new System.Windows.Forms.Label();
            this.cmdDeleteLifestyle = new System.Windows.Forms.Button();
            this.lblLifestyleMonthsLabel = new System.Windows.Forms.Label();
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
            this.lblArmorRating = new System.Windows.Forms.Label();
            this.lblArmorRatingLabel = new System.Windows.Forms.Label();
            this.lblArmorSource = new System.Windows.Forms.Label();
            this.lblArmorSourceLabel = new System.Windows.Forms.Label();
            this.lblArmorCost = new System.Windows.Forms.Label();
            this.lblArmorCostLabel = new System.Windows.Forms.Label();
            this.lblArmorAvail = new System.Windows.Forms.Label();
            this.treArmor = new Chummer.helpers.TreeView();
            this.lblArmorAvailLabel = new System.Windows.Forms.Label();
            this.cmdAddArmor = new SplitButton();
            this.cmdDeleteArmor = new SplitButton();
            this.tabWeapons = new System.Windows.Forms.TabPage();
            this.cboWeaponGearDataProcessing = new System.Windows.Forms.ComboBox();
            this.cboWeaponGearFirewall = new System.Windows.Forms.ComboBox();
            this.cboWeaponGearSleaze = new System.Windows.Forms.ComboBox();
            this.cboWeaponGearAttack = new System.Windows.Forms.ComboBox();
            this.lblWeaponRating = new System.Windows.Forms.Label();
            this.lblWeaponRatingLabel = new System.Windows.Forms.Label();
            this.lblWeaponFirewallLabel = new System.Windows.Forms.Label();
            this.lblWeaponDataProcessingLabel = new System.Windows.Forms.Label();
            this.lblWeaponSleazeLabel = new System.Windows.Forms.Label();
            this.lblWeaponAttackLabel = new System.Windows.Forms.Label();
            this.lblWeaponDeviceRating = new System.Windows.Forms.Label();
            this.lblWeaponDeviceRatingLabel = new System.Windows.Forms.Label();
            this.lblWeaponAccuracyLabel = new System.Windows.Forms.Label();
            this.lblWeaponAccuracy = new System.Windows.Forms.Label();
            this.cmdAddWeaponLocation = new System.Windows.Forms.Button();
            this.cboWeaponAmmo = new System.Windows.Forms.ComboBox();
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
            this.cmdReloadWeapon = new System.Windows.Forms.Button();
            this.lblWeaponAmmoTypeLabel = new System.Windows.Forms.Label();
            this.lblWeaponAmmoRemaining = new System.Windows.Forms.Label();
            this.lblWeaponAmmoRemainingLabel = new System.Windows.Forms.Label();
            this.lblWeaponSlots = new System.Windows.Forms.Label();
            this.lblWeaponSlotsLabel = new System.Windows.Forms.Label();
            this.lblWeaponSource = new System.Windows.Forms.Label();
            this.lblWeaponSourceLabel = new System.Windows.Forms.Label();
            this.lblWeaponAmmo = new System.Windows.Forms.Label();
            this.lblWeaponAmmoLabel = new System.Windows.Forms.Label();
            this.treWeapons = new Chummer.helpers.TreeView();
            this.lblWeaponMode = new System.Windows.Forms.Label();
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
            this.cmdRollWeapon = new System.Windows.Forms.Button();
            this.cmdAddWeapon = new SplitButton();
            this.cmdDeleteWeapon = new SplitButton();
            this.cmdFireWeapon = new SplitButton();
            this.tabGear = new System.Windows.Forms.TabPage();
            this.cboGearOverclocker = new System.Windows.Forms.ComboBox();
            this.lblGearOverclocker = new System.Windows.Forms.Label();
            this.tabGearMatrixCM = new System.Windows.Forms.TabControl();
            this.tabMatrixCM = new System.Windows.Forms.TabPage();
            this.chkGearMatrixCM1 = new System.Windows.Forms.CheckBox();
            this.chkGearMatrixCM2 = new System.Windows.Forms.CheckBox();
            this.chkGearMatrixCM3 = new System.Windows.Forms.CheckBox();
            this.chkGearMatrixCM4 = new System.Windows.Forms.CheckBox();
            this.chkGearMatrixCM5 = new System.Windows.Forms.CheckBox();
            this.chkGearMatrixCM6 = new System.Windows.Forms.CheckBox();
            this.chkGearMatrixCM7 = new System.Windows.Forms.CheckBox();
            this.chkGearMatrixCM8 = new System.Windows.Forms.CheckBox();
            this.chkGearMatrixCM9 = new System.Windows.Forms.CheckBox();
            this.chkGearMatrixCM10 = new System.Windows.Forms.CheckBox();
            this.chkGearMatrixCM11 = new System.Windows.Forms.CheckBox();
            this.chkGearMatrixCM12 = new System.Windows.Forms.CheckBox();
            this.chkGearMatrixCM13 = new System.Windows.Forms.CheckBox();
            this.chkGearMatrixCM14 = new System.Windows.Forms.CheckBox();
            this.chkGearMatrixCM15 = new System.Windows.Forms.CheckBox();
            this.chkGearMatrixCM16 = new System.Windows.Forms.CheckBox();
            this.chkGearMatrixCM17 = new System.Windows.Forms.CheckBox();
            this.chkGearMatrixCM18 = new System.Windows.Forms.CheckBox();
            this.chkGearMatrixCM19 = new System.Windows.Forms.CheckBox();
            this.chkGearMatrixCM20 = new System.Windows.Forms.CheckBox();
            this.chkGearMatrixCM21 = new System.Windows.Forms.CheckBox();
            this.chkGearMatrixCM22 = new System.Windows.Forms.CheckBox();
            this.chkGearMatrixCM23 = new System.Windows.Forms.CheckBox();
            this.chkGearMatrixCM24 = new System.Windows.Forms.CheckBox();
            this.cboGearDataProcessing = new System.Windows.Forms.ComboBox();
            this.cboGearFirewall = new System.Windows.Forms.ComboBox();
            this.cboGearSleaze = new System.Windows.Forms.ComboBox();
            this.cboGearAttack = new System.Windows.Forms.ComboBox();
            this.lblGearFirewallLabel = new System.Windows.Forms.Label();
            this.lblGearDataProcessingLabel = new System.Windows.Forms.Label();
            this.lblGearSleazeLabel = new System.Windows.Forms.Label();
            this.lblGearAttackLabel = new System.Windows.Forms.Label();
            this.lblGearDeviceRating = new System.Windows.Forms.Label();
            this.lblGearDeviceRatingLabel = new System.Windows.Forms.Label();
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
            this.lblGearRating = new System.Windows.Forms.Label();
            this.lblGearQty = new System.Windows.Forms.Label();
            this.treFoci = new System.Windows.Forms.TreeView();
            this.lblGearSource = new System.Windows.Forms.Label();
            this.lblGearSourceLabel = new System.Windows.Forms.Label();
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
            this.lblGearRatingLabel = new System.Windows.Forms.Label();
            this.treGear = new Chummer.helpers.TreeView();
            this.cmdAddGear = new SplitButton();
            this.cmdDeleteGear = new SplitButton();
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
            this.panVehicleCM = new System.Windows.Forms.TabControl();
            this.tabVehiclePhysicalCM = new System.Windows.Forms.TabPage();
            this.chkVehiclePhysicalCM40 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM1 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM39 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM2 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM38 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM3 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM37 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM4 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM36 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM5 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM35 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM6 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM34 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM7 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM33 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM8 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM32 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM9 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM31 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM10 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM30 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM11 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM29 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM12 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM28 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM13 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM27 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM14 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM26 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM15 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM25 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM16 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM24 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM17 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM23 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM18 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM22 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM19 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM21 = new System.Windows.Forms.CheckBox();
            this.chkVehiclePhysicalCM20 = new System.Windows.Forms.CheckBox();
            this.tabVehicleMatrixCM = new System.Windows.Forms.TabPage();
            this.chkVehicleMatrixCM1 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM2 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM3 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM4 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM5 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM6 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM7 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM8 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM9 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM10 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM11 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM12 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM13 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM14 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM15 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM16 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM17 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM18 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM19 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM20 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM21 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM22 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM23 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM24 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM25 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM26 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM27 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM28 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM29 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM30 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM31 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM32 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM33 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM34 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM35 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM36 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM37 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM38 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM39 = new System.Windows.Forms.CheckBox();
            this.chkVehicleMatrixCM40 = new System.Windows.Forms.CheckBox();
            this.lblVehicleFirewallLabel = new System.Windows.Forms.Label();
            this.lblVehicleDataProcessingLabel = new System.Windows.Forms.Label();
            this.lblVehicleSleazeLabel = new System.Windows.Forms.Label();
            this.lblVehicleAttackLabel = new System.Windows.Forms.Label();
            this.cmdAddVehicleLocation = new System.Windows.Forms.Button();
            this.chkVehicleHomeNode = new System.Windows.Forms.CheckBox();
            this.lblVehicleWeaponDicePool = new System.Windows.Forms.Label();
            this.lblVehicleWeaponDicePoolLabel = new System.Windows.Forms.Label();
            this.lblVehicleDevice = new System.Windows.Forms.Label();
            this.lblVehicleDeviceLabel = new System.Windows.Forms.Label();
            this.cboVehicleWeaponAmmo = new System.Windows.Forms.ComboBox();
            this.lblVehicleGearQty = new System.Windows.Forms.Label();
            this.lblVehicleGearQtyLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeExtreme = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeLong = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeMedium = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeShort = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeExtremeLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeLongLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeMediumLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeShortLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeLabel = new System.Windows.Forms.Label();
            this.chkVehicleIncludedInWeapon = new System.Windows.Forms.CheckBox();
            this.lblVehicleWeaponAmmo = new System.Windows.Forms.Label();
            this.lblVehicleWeaponAmmoLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponMode = new System.Windows.Forms.Label();
            this.lblVehicleWeaponModeLabel = new System.Windows.Forms.Label();
            this.cmdReloadVehicleWeapon = new System.Windows.Forms.Button();
            this.lblVehicleWeaponAmmoTypeLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponAmmoRemaining = new System.Windows.Forms.Label();
            this.lblVehicleWeaponAmmoRemainingLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponNameLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponName = new System.Windows.Forms.Label();
            this.lblVehicleWeaponCategoryLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponAP = new System.Windows.Forms.Label();
            this.lblVehicleWeaponCategory = new System.Windows.Forms.Label();
            this.lblVehicleWeaponAPLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponDamageLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponDamage = new System.Windows.Forms.Label();
            this.lblVehicleRating = new System.Windows.Forms.Label();
            this.lblVehicleSource = new System.Windows.Forms.Label();
            this.lblVehicleSourceLabel = new System.Windows.Forms.Label();
            this.lblVehicleSlots = new System.Windows.Forms.Label();
            this.lblVehicleSlotsLabel = new System.Windows.Forms.Label();
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
            this.treVehicles = new Chummer.helpers.TreeView();
            this.cmdRollVehicleWeapon = new System.Windows.Forms.Button();
            this.cmdAddVehicle = new SplitButton();
            this.cmdFireVehicleWeapon = new SplitButton();
            this.cmdDeleteVehicle = new SplitButton();
            this.tabCharacterInfo = new System.Windows.Forms.TabPage();
            this.cboPrimaryArm = new Chummer.helpers.ComboBox();
            this.lblHandedness = new System.Windows.Forms.Label();
            this.chkIsMainMugshot = new System.Windows.Forms.CheckBox();
            this.lblNumMugshots = new System.Windows.Forms.Label();
            this.nudMugshotIndex = new System.Windows.Forms.NumericUpDown();
            this.lblMugshotDimensions = new System.Windows.Forms.Label();
            this.lblPublicAwareTotal = new System.Windows.Forms.Label();
            this.lblNotorietyTotal = new System.Windows.Forms.Label();
            this.lblStreetCredTotal = new System.Windows.Forms.Label();
            this.lblCharacterName = new System.Windows.Forms.Label();
            this.txtCharacterName = new System.Windows.Forms.TextBox();
            this.txtPlayerName = new System.Windows.Forms.TextBox();
            this.txtNotes = new System.Windows.Forms.TextBox();
            this.txtConcept = new System.Windows.Forms.TextBox();
            this.txtBackground = new System.Windows.Forms.TextBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.txtSkin = new System.Windows.Forms.TextBox();
            this.txtWeight = new System.Windows.Forms.TextBox();
            this.txtHeight = new System.Windows.Forms.TextBox();
            this.txtHair = new System.Windows.Forms.TextBox();
            this.txtEyes = new System.Windows.Forms.TextBox();
            this.txtAge = new System.Windows.Forms.TextBox();
            this.txtSex = new System.Windows.Forms.TextBox();
            this.nudPublicAware = new System.Windows.Forms.NumericUpDown();
            this.lblPublicAware = new System.Windows.Forms.Label();
            this.nudNotoriety = new System.Windows.Forms.NumericUpDown();
            this.nudStreetCred = new System.Windows.Forms.NumericUpDown();
            this.lblPlayerName = new System.Windows.Forms.Label();
            this.lblNotes = new System.Windows.Forms.Label();
            this.cmdDeleteMugshot = new System.Windows.Forms.Button();
            this.cmdAddMugshot = new System.Windows.Forms.Button();
            this.lblMugshot = new System.Windows.Forms.Label();
            this.lblConcept = new System.Windows.Forms.Label();
            this.lblBackground = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblSkin = new System.Windows.Forms.Label();
            this.lblWeight = new System.Windows.Forms.Label();
            this.lblHeight = new System.Windows.Forms.Label();
            this.lblHair = new System.Windows.Forms.Label();
            this.lblEyes = new System.Windows.Forms.Label();
            this.lblAge = new System.Windows.Forms.Label();
            this.lblSex = new System.Windows.Forms.Label();
            this.picMugshot = new System.Windows.Forms.PictureBox();
            this.tabKarma = new System.Windows.Forms.TabPage();
            this.splitKarmaNuyen = new System.Windows.Forms.SplitContainer();
            this.chkShowFreeKarma = new System.Windows.Forms.CheckBox();
            this.chtKarma = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.cmdKarmaEdit = new System.Windows.Forms.Button();
            this.cmdKarmaGained = new System.Windows.Forms.Button();
            this.lstKarma = new System.Windows.Forms.ListView();
            this.colKarmaDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colKarmaAmount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colKarmaReason = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmdKarmaSpent = new System.Windows.Forms.Button();
            this.chkShowFreeNuyen = new System.Windows.Forms.CheckBox();
            this.chtNuyen = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.cmdNuyenEdit = new System.Windows.Forms.Button();
            this.lstNuyen = new System.Windows.Forms.ListView();
            this.colNuyenDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colNuyenAmount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colNuyenReason = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmdNuyenSpent = new System.Windows.Forms.Button();
            this.cmdNuyenGained = new System.Windows.Forms.Button();
            this.tabCalendar = new System.Windows.Forms.TabPage();
            this.cmdDeleteWeek = new System.Windows.Forms.Button();
            this.cmdChangeStartWeek = new System.Windows.Forms.Button();
            this.cmdEditWeek = new System.Windows.Forms.Button();
            this.cmdAddWeek = new System.Windows.Forms.Button();
            this.lstCalendar = new System.Windows.Forms.ListView();
            this.colCalendarDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colCalendarNotes = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabNotes = new System.Windows.Forms.TabPage();
            this.txtGameNotes = new System.Windows.Forms.TextBox();
            this.tabImprovements = new System.Windows.Forms.TabPage();
            this.cmdImprovementsDisableAll = new System.Windows.Forms.Button();
            this.cmdImprovementsEnableAll = new System.Windows.Forms.Button();
            this.cmdAddImprovementGroup = new System.Windows.Forms.Button();
            this.cmdDeleteImprovement = new System.Windows.Forms.Button();
            this.cmdEditImprovement = new System.Windows.Forms.Button();
            this.chkImprovementActive = new System.Windows.Forms.CheckBox();
            this.lblImprovementValue = new System.Windows.Forms.Label();
            this.lblImprovementType = new System.Windows.Forms.Label();
            this.lblImprovementTypeLabel = new System.Windows.Forms.Label();
            this.treImprovements = new Chummer.helpers.TreeView();
            this.cmdAddImprovement = new System.Windows.Forms.Button();
            this.panAttributes = new System.Windows.Forms.Panel();
            this.cmsBioware = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsBiowareNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsAdvancedLifestyle = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsEditAdvancedLifestyle = new System.Windows.Forms.ToolStripMenuItem();
            this.tsAdvancedLifestyleNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsGearLocation = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsGearRenameLocation = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsImprovement = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsImprovementNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsArmorLocation = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsArmorRenameLocation = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsImprovementLocation = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsImprovementRenameLocation = new System.Windows.Forms.ToolStripMenuItem();
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
            this.cmsVehicleWeaponMod = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsVehicleWeaponModNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsWeaponLocation = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsWeaponRenameLocation = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsLimitModifier = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tssLimitModifierEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.tssLimitModifierNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsInitiationNotes = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsInitiationNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsTechnique = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsAddTechniqueNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsAdvancedProgram = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsAddAdvancedProgramOption = new System.Windows.Forms.ToolStripMenuItem();
            this.tsAIProgramNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmdIncreasePowerPoints = new Chummer.helpers.Button();
            this.StatusStrip.SuspendLayout();
            this.cmsMartialArts.SuspendLayout();
            this.cmsSpellButton.SuspendLayout();
            this.cmsComplexForm.SuspendLayout();
            this.cmsCyberware.SuspendLayout();
            this.cmsDeleteCyberware.SuspendLayout();
            this.cmsLifestyle.SuspendLayout();
            this.cmsArmor.SuspendLayout();
            this.cmsDeleteArmor.SuspendLayout();
            this.cmsWeapon.SuspendLayout();
            this.cmsDeleteWeapon.SuspendLayout();
            this.cmsAmmoExpense.SuspendLayout();
            this.cmsGearButton.SuspendLayout();
            this.cmsDeleteGear.SuspendLayout();
            this.cmsVehicle.SuspendLayout();
            this.cmdVehicleAmmoExpense.SuspendLayout();
            this.cmsDeleteVehicle.SuspendLayout();
            this.panStunCM.SuspendLayout();
            this.panPhysicalCM.SuspendLayout();
            this.tabInfo.SuspendLayout();
            this.tabOtherInfo.SuspendLayout();
            this.tabConditionMonitor.SuspendLayout();
            this.tabDefences.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCounterspellingDice)).BeginInit();
            this.mnuCreateMenu.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.cmsGear.SuspendLayout();
            this.cmsVehicleWeapon.SuspendLayout();
            this.cmsVehicleGear.SuspendLayout();
            this.cmsUndoKarmaExpense.SuspendLayout();
            this.cmsUndoNuyenExpense.SuspendLayout();
            this.cmsArmorGear.SuspendLayout();
            this.cmsArmorMod.SuspendLayout();
            this.cmsQuality.SuspendLayout();
            this.cmsMartialArtManeuver.SuspendLayout();
            this.cmsSpell.SuspendLayout();
            this.cmsCritterPowers.SuspendLayout();
            this.cmsMetamagic.SuspendLayout();
            this.cmsLifestyleNotes.SuspendLayout();
            this.cmsWeaponAccessory.SuspendLayout();
            this.cmsGearPlugin.SuspendLayout();
            this.cmsComplexFormPlugin.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.tabCharacterTabs.SuspendLayout();
            this.tabCommon.SuspendLayout();
            this.tabPeople.SuspendLayout();
            this.tabContacts.SuspendLayout();
            this.tabEnemies.SuspendLayout();
            this.tabSkills.SuspendLayout();
            this.tabLimits.SuspendLayout();
            this.tabMartialArts.SuspendLayout();
            this.tabMagician.SuspendLayout();
            this.tabAdept.SuspendLayout();
            this.tabTechnomancer.SuspendLayout();
            this.tabCritter.SuspendLayout();
            this.tabAdvancedPrograms.SuspendLayout();
            this.tabInitiation.SuspendLayout();
            this.tabCyberware.SuspendLayout();
            this.tabCyberwareCM.SuspendLayout();
            this.tabCyberwareMatrixCM.SuspendLayout();
            this.tabStreetGear.SuspendLayout();
            this.tabStreetGearTabs.SuspendLayout();
            this.tabLifestyle.SuspendLayout();
            this.tabArmor.SuspendLayout();
            this.tabWeapons.SuspendLayout();
            this.tabGear.SuspendLayout();
            this.tabGearMatrixCM.SuspendLayout();
            this.tabMatrixCM.SuspendLayout();
            this.tabPets.SuspendLayout();
            this.tabVehicles.SuspendLayout();
            this.panVehicleCM.SuspendLayout();
            this.tabVehiclePhysicalCM.SuspendLayout();
            this.tabVehicleMatrixCM.SuspendLayout();
            this.tabCharacterInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMugshotIndex)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPublicAware)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNotoriety)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudStreetCred)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMugshot)).BeginInit();
            this.tabKarma.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitKarmaNuyen)).BeginInit();
            this.splitKarmaNuyen.Panel1.SuspendLayout();
            this.splitKarmaNuyen.Panel2.SuspendLayout();
            this.splitKarmaNuyen.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chtKarma)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chtNuyen)).BeginInit();
            this.tabCalendar.SuspendLayout();
            this.tabNotes.SuspendLayout();
            this.tabImprovements.SuspendLayout();
            this.cmsBioware.SuspendLayout();
            this.cmsAdvancedLifestyle.SuspendLayout();
            this.cmsGearLocation.SuspendLayout();
            this.cmsImprovement.SuspendLayout();
            this.cmsArmorLocation.SuspendLayout();
            this.cmsImprovementLocation.SuspendLayout();
            this.cmsCyberwareGear.SuspendLayout();
            this.cmsWeaponAccessoryGear.SuspendLayout();
            this.cmsVehicleLocation.SuspendLayout();
            this.cmsVehicleWeaponAccessory.SuspendLayout();
            this.cmsVehicleWeaponAccessoryGear.SuspendLayout();
            this.cmsVehicleWeaponMod.SuspendLayout();
            this.cmsWeaponLocation.SuspendLayout();
            this.cmsLimitModifier.SuspendLayout();
            this.cmsInitiationNotes.SuspendLayout();
            this.cmsTechnique.SuspendLayout();
            this.cmsAdvancedProgram.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTraditionSource
            // 
            this.lblTraditionSource.AutoSize = true;
            this.lblTraditionSource.Location = new System.Drawing.Point(404, 260);
            this.lblTraditionSource.Name = "lblTraditionSource";
            this.lblTraditionSource.Size = new System.Drawing.Size(47, 13);
            this.lblTraditionSource.TabIndex = 168;
            this.lblTraditionSource.Text = "[Source]";
            this.lblTraditionSource.Click += new System.EventHandler(this.lblTraditionSource_Click);
            // 
            // lblTraditionSourceLabel
            // 
            this.lblTraditionSourceLabel.AutoSize = true;
            this.lblTraditionSourceLabel.Location = new System.Drawing.Point(309, 260);
            this.lblTraditionSourceLabel.Name = "lblTraditionSourceLabel";
            this.lblTraditionSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblTraditionSourceLabel.TabIndex = 167;
            this.lblTraditionSourceLabel.Tag = "Label_Source";
            this.lblTraditionSourceLabel.Text = "Source:";
            // 
            // tabPowerUc
            // 
            this.tabPowerUc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabPowerUc.Location = new System.Drawing.Point(0, 0);
            this.tabPowerUc.Name = "tabPowerUc";
            this.tabPowerUc.ObjCharacter = null;
            this.tabPowerUc.Size = new System.Drawing.Size(861, 586);
            this.tabPowerUc.TabIndex = 1;
            // 
            // StatusStrip
            // 
            this.StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tssKarmaLabel,
            this.tssKarma,
            this.toolStripStatusLabel3,
            this.tssEssence,
            this.toolStripStatusLabel4,
            this.tssNuyen,
            this.pgbProgress});
            this.StatusStrip.Location = new System.Drawing.Point(0, 615);
            this.StatusStrip.Name = "StatusStrip";
            this.StatusStrip.Size = new System.Drawing.Size(1066, 24);
            this.StatusStrip.TabIndex = 24;
            this.StatusStrip.Text = "StatusStrip1";
            // 
            // tssKarmaLabel
            // 
            this.tssKarmaLabel.Name = "tssKarmaLabel";
            this.tssKarmaLabel.Size = new System.Drawing.Size(44, 19);
            this.tssKarmaLabel.Tag = "Label_Karma";
            this.tssKarmaLabel.Text = "Karma:";
            // 
            // tssKarma
            // 
            this.tssKarma.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.tssKarma.Name = "tssKarma";
            this.tssKarma.Size = new System.Drawing.Size(17, 19);
            this.tssKarma.Text = "0";
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(51, 19);
            this.toolStripStatusLabel3.Tag = "Label_Essence";
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
            this.toolStripStatusLabel4.Size = new System.Drawing.Size(45, 19);
            this.toolStripStatusLabel4.Tag = "Label_Nuyen";
            this.toolStripStatusLabel4.Text = "Nuyen:";
            // 
            // tssNuyen
            // 
            this.tssNuyen.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.tssNuyen.Name = "tssNuyen";
            this.tssNuyen.Size = new System.Drawing.Size(23, 19);
            this.tssNuyen.Text = "0¥";
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
            // lblRemainingNuyenLabel
            // 
            this.lblRemainingNuyenLabel.AutoSize = true;
            this.lblRemainingNuyenLabel.Location = new System.Drawing.Point(6, 235);
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
            this.lblESS.Location = new System.Drawing.Point(6, 212);
            this.lblESS.Name = "lblESS";
            this.lblESS.Size = new System.Drawing.Size(51, 13);
            this.lblESS.TabIndex = 34;
            this.lblESS.Tag = "Label_OtherEssence";
            this.lblESS.Text = "Essence:";
            this.tipTooltip.SetToolTip(this.lblESS, "Characters start with 6 Essence which is decreased by adding Cyberware and Biowar" +
        "e.");
            // 
            // lblCareerKarmaLabel
            // 
            this.lblCareerKarmaLabel.AutoSize = true;
            this.lblCareerKarmaLabel.Location = new System.Drawing.Point(6, 258);
            this.lblCareerKarmaLabel.Name = "lblCareerKarmaLabel";
            this.lblCareerKarmaLabel.Size = new System.Drawing.Size(74, 13);
            this.lblCareerKarmaLabel.TabIndex = 42;
            this.lblCareerKarmaLabel.Tag = "Label_OtherCareerKarma";
            this.lblCareerKarmaLabel.Text = "Career Karma:";
            this.tipTooltip.SetToolTip(this.lblCareerKarmaLabel, "The amount of Nuyen you have left to purchase gear.");
            // 
            // lblMemoryLabel
            // 
            this.lblMemoryLabel.AutoSize = true;
            this.lblMemoryLabel.Location = new System.Drawing.Point(6, 373);
            this.lblMemoryLabel.Name = "lblMemoryLabel";
            this.lblMemoryLabel.Size = new System.Drawing.Size(47, 13);
            this.lblMemoryLabel.TabIndex = 58;
            this.lblMemoryLabel.Tag = "Label_OtherMemory";
            this.lblMemoryLabel.Text = "Memory:";
            this.tipTooltip.SetToolTip(this.lblMemoryLabel, "Memory is calculated as LOG + WIL.");
            // 
            // lblLiftCarryLabel
            // 
            this.lblLiftCarryLabel.AutoSize = true;
            this.lblLiftCarryLabel.Location = new System.Drawing.Point(6, 350);
            this.lblLiftCarryLabel.Name = "lblLiftCarryLabel";
            this.lblLiftCarryLabel.Size = new System.Drawing.Size(72, 13);
            this.lblLiftCarryLabel.TabIndex = 56;
            this.lblLiftCarryLabel.Tag = "Label_OtherLiftAndCarry";
            this.lblLiftCarryLabel.Text = "Lift and Carry:";
            this.tipTooltip.SetToolTip(this.lblLiftCarryLabel, "Lift and Carry is calculated as STR + BOD.");
            // 
            // lblJudgeIntentionsLabel
            // 
            this.lblJudgeIntentionsLabel.AutoSize = true;
            this.lblJudgeIntentionsLabel.Location = new System.Drawing.Point(6, 327);
            this.lblJudgeIntentionsLabel.Name = "lblJudgeIntentionsLabel";
            this.lblJudgeIntentionsLabel.Size = new System.Drawing.Size(88, 13);
            this.lblJudgeIntentionsLabel.TabIndex = 54;
            this.lblJudgeIntentionsLabel.Tag = "Label_OtherJudgeIntention";
            this.lblJudgeIntentionsLabel.Text = "Judge Intentions:";
            this.tipTooltip.SetToolTip(this.lblJudgeIntentionsLabel, "Judge Intentions is calculated as INT + CHA.");
            // 
            // lblComposureLabel
            // 
            this.lblComposureLabel.AutoSize = true;
            this.lblComposureLabel.Location = new System.Drawing.Point(6, 304);
            this.lblComposureLabel.Name = "lblComposureLabel";
            this.lblComposureLabel.Size = new System.Drawing.Size(63, 13);
            this.lblComposureLabel.TabIndex = 52;
            this.lblComposureLabel.Tag = "Label_OtherCmposure";
            this.lblComposureLabel.Text = "Composure:";
            this.tipTooltip.SetToolTip(this.lblComposureLabel, "Composure is calculated as WIL + CHA.");
            // 
            // lblCMPenaltyLabel
            // 
            this.lblCMPenaltyLabel.AutoSize = true;
            this.lblCMPenaltyLabel.Location = new System.Drawing.Point(3, 10);
            this.lblCMPenaltyLabel.Name = "lblCMPenaltyLabel";
            this.lblCMPenaltyLabel.Size = new System.Drawing.Size(64, 13);
            this.lblCMPenaltyLabel.TabIndex = 37;
            this.lblCMPenaltyLabel.Tag = "Label_CMCMPenalty";
            this.lblCMPenaltyLabel.Text = "CM Penalty:";
            this.tipTooltip.SetToolTip(this.lblCMPenaltyLabel, "Dice pool penalty from Condition Monitor damage.");
            // 
            // lblCMArmorLabel
            // 
            this.lblCMArmorLabel.AutoSize = true;
            this.lblCMArmorLabel.Location = new System.Drawing.Point(3, 31);
            this.lblCMArmorLabel.Name = "lblCMArmorLabel";
            this.lblCMArmorLabel.Size = new System.Drawing.Size(37, 13);
            this.lblCMArmorLabel.TabIndex = 66;
            this.lblCMArmorLabel.Tag = "Label_CMArmor";
            this.lblCMArmorLabel.Text = "Armor:";
            this.tipTooltip.SetToolTip(this.lblCMArmorLabel, resources.GetString("lblCMArmorLabel.ToolTip"));
            // 
            // lblCMDamageResistancePoolLabel
            // 
            this.lblCMDamageResistancePoolLabel.AutoSize = true;
            this.lblCMDamageResistancePoolLabel.Location = new System.Drawing.Point(3, 73);
            this.lblCMDamageResistancePoolLabel.Name = "lblCMDamageResistancePoolLabel";
            this.lblCMDamageResistancePoolLabel.Size = new System.Drawing.Size(112, 13);
            this.lblCMDamageResistancePoolLabel.TabIndex = 70;
            this.lblCMDamageResistancePoolLabel.Tag = "Label_CMResistancePool";
            this.lblCMDamageResistancePoolLabel.Text = "Dmg Resistance Pool:";
            this.tipTooltip.SetToolTip(this.lblCMDamageResistancePoolLabel, "Number of dice used to make Damage Resistance Tests.");
            // 
            // lblCareerNuyenLabel
            // 
            this.lblCareerNuyenLabel.AutoSize = true;
            this.lblCareerNuyenLabel.Location = new System.Drawing.Point(6, 281);
            this.lblCareerNuyenLabel.Name = "lblCareerNuyenLabel";
            this.lblCareerNuyenLabel.Size = new System.Drawing.Size(75, 13);
            this.lblCareerNuyenLabel.TabIndex = 64;
            this.lblCareerNuyenLabel.Tag = "Label_OtherCareerNuyen";
            this.lblCareerNuyenLabel.Text = "Career Nuyen:";
            this.tipTooltip.SetToolTip(this.lblCareerNuyenLabel, "The amount of Nuyen you have left to purchase gear.");
            // 
            // lblArmorLabel
            // 
            this.lblArmorLabel.AutoSize = true;
            this.lblArmorLabel.Location = new System.Drawing.Point(6, 189);
            this.lblArmorLabel.Name = "lblArmorLabel";
            this.lblArmorLabel.Size = new System.Drawing.Size(37, 13);
            this.lblArmorLabel.TabIndex = 71;
            this.lblArmorLabel.Tag = "Label_ArmorValueShort";
            this.lblArmorLabel.Text = "Armor:";
            this.tipTooltip.SetToolTip(this.lblArmorLabel, resources.GetString("lblArmorLabel.ToolTip"));
            // 
            // lblRiggingINILabel
            // 
            this.lblRiggingINILabel.AutoSize = true;
            this.lblRiggingINILabel.Location = new System.Drawing.Point(6, 166);
            this.lblRiggingINILabel.Name = "lblRiggingINILabel";
            this.lblRiggingINILabel.Size = new System.Drawing.Size(112, 13);
            this.lblRiggingINILabel.TabIndex = 87;
            this.lblRiggingINILabel.Tag = "Label_OtherRiggingInit";
            this.lblRiggingINILabel.Text = "Rigging Initiative (AR):";
            this.tipTooltip.SetToolTip(this.lblRiggingINILabel, "Matrix Initiative is calculated as Commlink Response + INT.");
            // 
            // lblMatrixINIHotLabel
            // 
            this.lblMatrixINIHotLabel.AutoSize = true;
            this.lblMatrixINIHotLabel.Location = new System.Drawing.Point(6, 143);
            this.lblMatrixINIHotLabel.Name = "lblMatrixINIHotLabel";
            this.lblMatrixINIHotLabel.Size = new System.Drawing.Size(106, 13);
            this.lblMatrixINIHotLabel.TabIndex = 85;
            this.lblMatrixINIHotLabel.Tag = "Label_OtherMatrixInitVRHot";
            this.lblMatrixINIHotLabel.Text = "Matrix Initiative (Hot):";
            this.tipTooltip.SetToolTip(this.lblMatrixINIHotLabel, "Matrix Initiative is calculated as Commlink Response + INT.");
            // 
            // lblMatrixINIColdLabel
            // 
            this.lblMatrixINIColdLabel.AutoSize = true;
            this.lblMatrixINIColdLabel.Location = new System.Drawing.Point(6, 120);
            this.lblMatrixINIColdLabel.Name = "lblMatrixINIColdLabel";
            this.lblMatrixINIColdLabel.Size = new System.Drawing.Size(110, 13);
            this.lblMatrixINIColdLabel.TabIndex = 83;
            this.lblMatrixINIColdLabel.Tag = "Label_OtherMatrixInitVRCold";
            this.lblMatrixINIColdLabel.Text = "Matrix Initiative (Cold):";
            this.tipTooltip.SetToolTip(this.lblMatrixINIColdLabel, "Matrix Initiative is calculated as Commlink Response + INT.");
            // 
            // lblAstralINILabel
            // 
            this.lblAstralINILabel.AutoSize = true;
            this.lblAstralINILabel.Location = new System.Drawing.Point(6, 74);
            this.lblAstralINILabel.Name = "lblAstralINILabel";
            this.lblAstralINILabel.Size = new System.Drawing.Size(78, 13);
            this.lblAstralINILabel.TabIndex = 79;
            this.lblAstralINILabel.Tag = "Label_OtherAstralInit";
            this.lblAstralINILabel.Text = "Astral Initiative:";
            this.tipTooltip.SetToolTip(this.lblAstralINILabel, "Astral Initiative is calculated as INT x 2.");
            // 
            // lblMatrixINILabel
            // 
            this.lblMatrixINILabel.AutoSize = true;
            this.lblMatrixINILabel.Location = new System.Drawing.Point(6, 97);
            this.lblMatrixINILabel.Name = "lblMatrixINILabel";
            this.lblMatrixINILabel.Size = new System.Drawing.Size(104, 13);
            this.lblMatrixINILabel.TabIndex = 78;
            this.lblMatrixINILabel.Tag = "Label_OtherMatrixInit";
            this.lblMatrixINILabel.Text = "Matrix Initiative (AR):";
            this.tipTooltip.SetToolTip(this.lblMatrixINILabel, "Matrix Initiative is calculated as Commlink Response + INT.");
            // 
            // lblINILabel
            // 
            this.lblINILabel.AutoSize = true;
            this.lblINILabel.Location = new System.Drawing.Point(6, 52);
            this.lblINILabel.Name = "lblINILabel";
            this.lblINILabel.Size = new System.Drawing.Size(49, 13);
            this.lblINILabel.TabIndex = 77;
            this.lblINILabel.Tag = "Label_OtherInit";
            this.lblINILabel.Text = "Initiative:";
            this.tipTooltip.SetToolTip(this.lblINILabel, "Initiative is calculated as REA + INT.");
            // 
            // cmdEdgeGained
            // 
            this.cmdEdgeGained.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdEdgeGained.Image = global::Chummer.Properties.Resources.add;
            this.cmdEdgeGained.Location = new System.Drawing.Point(30, 537);
            this.cmdEdgeGained.Name = "cmdEdgeGained";
            this.cmdEdgeGained.Size = new System.Drawing.Size(24, 24);
            this.cmdEdgeGained.TabIndex = 64;
            this.cmdEdgeGained.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.tipTooltip.SetToolTip(this.cmdEdgeGained, "Regain a point of Edge");
            this.cmdEdgeGained.UseVisualStyleBackColor = true;
            this.cmdEdgeGained.Click += new System.EventHandler(this.cmdEdgeGained_Click);
            // 
            // cmdEdgeSpent
            // 
            this.cmdEdgeSpent.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdEdgeSpent.Image = global::Chummer.Properties.Resources.delete;
            this.cmdEdgeSpent.Location = new System.Drawing.Point(60, 537);
            this.cmdEdgeSpent.Name = "cmdEdgeSpent";
            this.cmdEdgeSpent.Size = new System.Drawing.Size(24, 24);
            this.cmdEdgeSpent.TabIndex = 63;
            this.cmdEdgeSpent.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.tipTooltip.SetToolTip(this.cmdEdgeSpent, "Spend a point of Edge");
            this.cmdEdgeSpent.UseVisualStyleBackColor = true;
            this.cmdEdgeSpent.Click += new System.EventHandler(this.cmdEdgeSpent_Click);
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
            // lblStreetCred
            // 
            this.lblStreetCred.AutoSize = true;
            this.lblStreetCred.Location = new System.Drawing.Point(654, 97);
            this.lblStreetCred.Name = "lblStreetCred";
            this.lblStreetCred.Size = new System.Drawing.Size(63, 13);
            this.lblStreetCred.TabIndex = 71;
            this.lblStreetCred.Tag = "Label_StreetCred";
            this.lblStreetCred.Text = "Street Cred:";
            this.tipTooltip.SetToolTip(this.lblStreetCred, "Street Cred is calculated as Career Karma ÷ 10, rounded up, and can be further ad" +
        "justed by Game Masters.");
            // 
            // lblNotoriety
            // 
            this.lblNotoriety.AutoSize = true;
            this.lblNotoriety.Location = new System.Drawing.Point(654, 119);
            this.lblNotoriety.Name = "lblNotoriety";
            this.lblNotoriety.Size = new System.Drawing.Size(52, 13);
            this.lblNotoriety.TabIndex = 73;
            this.lblNotoriety.Tag = "Label_Notoriety";
            this.lblNotoriety.Text = "Notoriety:";
            this.tipTooltip.SetToolTip(this.lblNotoriety, "Notoriety is typically gained through Qualities and can be further adjusted by Ga" +
        "me Masters. Notoriety can be reduced by 1 point by burning 2 points of Street Cr" +
        "ed.");
            // 
            // cmdBurnStreetCred
            // 
            this.cmdBurnStreetCred.Enabled = false;
            this.cmdBurnStreetCred.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdBurnStreetCred.Image = global::Chummer.Properties.Resources.delete;
            this.cmdBurnStreetCred.Location = new System.Drawing.Point(801, 92);
            this.cmdBurnStreetCred.Name = "cmdBurnStreetCred";
            this.cmdBurnStreetCred.Size = new System.Drawing.Size(24, 24);
            this.cmdBurnStreetCred.TabIndex = 82;
            this.cmdBurnStreetCred.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.tipTooltip.SetToolTip(this.cmdBurnStreetCred, "Burn 2 points of Street Cred to reduce Notoriety by 1.");
            this.cmdBurnStreetCred.UseVisualStyleBackColor = true;
            this.cmdBurnStreetCred.Click += new System.EventHandler(this.cmdBurnStreetCred_Click);
            // 
            // cmdVehicleGearReduceQty
            // 
            this.cmdVehicleGearReduceQty.Enabled = false;
            this.cmdVehicleGearReduceQty.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdVehicleGearReduceQty.Image = global::Chummer.Properties.Resources.delete;
            this.cmdVehicleGearReduceQty.Location = new System.Drawing.Point(538, 243);
            this.cmdVehicleGearReduceQty.Name = "cmdVehicleGearReduceQty";
            this.cmdVehicleGearReduceQty.Size = new System.Drawing.Size(24, 24);
            this.cmdVehicleGearReduceQty.TabIndex = 112;
            this.cmdVehicleGearReduceQty.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.tipTooltip.SetToolTip(this.cmdVehicleGearReduceQty, "Reduce the quantity of the Gear by 1.");
            this.cmdVehicleGearReduceQty.UseVisualStyleBackColor = true;
            this.cmdVehicleGearReduceQty.Click += new System.EventHandler(this.cmdVehicleGearReduceQty_Click);
            // 
            // cmdVehicleMoveToInventory
            // 
            this.cmdVehicleMoveToInventory.Enabled = false;
            this.cmdVehicleMoveToInventory.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdVehicleMoveToInventory.Image = global::Chummer.Properties.Resources.car_go;
            this.cmdVehicleMoveToInventory.Location = new System.Drawing.Point(576, 266);
            this.cmdVehicleMoveToInventory.Name = "cmdVehicleMoveToInventory";
            this.cmdVehicleMoveToInventory.Size = new System.Drawing.Size(24, 24);
            this.cmdVehicleMoveToInventory.TabIndex = 136;
            this.cmdVehicleMoveToInventory.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.tipTooltip.SetToolTip(this.cmdVehicleMoveToInventory, "Transfer to Inventory.");
            this.cmdVehicleMoveToInventory.UseVisualStyleBackColor = true;
            this.cmdVehicleMoveToInventory.Click += new System.EventHandler(this.cmdVehicleMoveToInventory_Click);
            // 
            // chkVehicleWeaponAccessoryInstalled
            // 
            this.chkVehicleWeaponAccessoryInstalled.AutoSize = true;
            this.chkVehicleWeaponAccessoryInstalled.Enabled = false;
            this.chkVehicleWeaponAccessoryInstalled.Location = new System.Drawing.Point(729, 222);
            this.chkVehicleWeaponAccessoryInstalled.Name = "chkVehicleWeaponAccessoryInstalled";
            this.chkVehicleWeaponAccessoryInstalled.Size = new System.Drawing.Size(65, 17);
            this.chkVehicleWeaponAccessoryInstalled.TabIndex = 89;
            this.chkVehicleWeaponAccessoryInstalled.Tag = "Checkbox_Installed";
            this.chkVehicleWeaponAccessoryInstalled.Text = "Installed";
            this.tipTooltip.SetToolTip(this.chkVehicleWeaponAccessoryInstalled, "Installed Weapon Accessories and Mods count towards a Weapon\'s stats.");
            this.chkVehicleWeaponAccessoryInstalled.UseVisualStyleBackColor = true;
            this.chkVehicleWeaponAccessoryInstalled.CheckedChanged += new System.EventHandler(this.chkVehicleWeaponAccessoryInstalled_CheckedChanged);
            // 
            // cmdGearReduceQty
            // 
            this.cmdGearReduceQty.Enabled = false;
            this.cmdGearReduceQty.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdGearReduceQty.Image = global::Chummer.Properties.Resources.delete;
            this.cmdGearReduceQty.Location = new System.Drawing.Point(452, 122);
            this.cmdGearReduceQty.Name = "cmdGearReduceQty";
            this.cmdGearReduceQty.Size = new System.Drawing.Size(24, 24);
            this.cmdGearReduceQty.TabIndex = 96;
            this.cmdGearReduceQty.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.tipTooltip.SetToolTip(this.cmdGearReduceQty, "Reduce the quantity of the Gear by 1.");
            this.cmdGearReduceQty.UseVisualStyleBackColor = true;
            this.cmdGearReduceQty.Click += new System.EventHandler(this.cmdGearReduceQty_Click);
            // 
            // cmdGearIncreaseQty
            // 
            this.cmdGearIncreaseQty.Enabled = false;
            this.cmdGearIncreaseQty.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdGearIncreaseQty.Image = global::Chummer.Properties.Resources.add;
            this.cmdGearIncreaseQty.Location = new System.Drawing.Point(422, 122);
            this.cmdGearIncreaseQty.Name = "cmdGearIncreaseQty";
            this.cmdGearIncreaseQty.Size = new System.Drawing.Size(24, 24);
            this.cmdGearIncreaseQty.TabIndex = 105;
            this.cmdGearIncreaseQty.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.tipTooltip.SetToolTip(this.cmdGearIncreaseQty, "Increase the number of pre-paid months for the Lifestyle");
            this.cmdGearIncreaseQty.UseVisualStyleBackColor = true;
            this.cmdGearIncreaseQty.Click += new System.EventHandler(this.cmdGearIncreaseQty_Click);
            // 
            // cmdGearSplitQty
            // 
            this.cmdGearSplitQty.Enabled = false;
            this.cmdGearSplitQty.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdGearSplitQty.Image = global::Chummer.Properties.Resources.arrow_divide;
            this.cmdGearSplitQty.Location = new System.Drawing.Point(531, 122);
            this.cmdGearSplitQty.Name = "cmdGearSplitQty";
            this.cmdGearSplitQty.Size = new System.Drawing.Size(24, 24);
            this.cmdGearSplitQty.TabIndex = 112;
            this.cmdGearSplitQty.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.tipTooltip.SetToolTip(this.cmdGearSplitQty, "Reduce the quantity of the Gear by 1.");
            this.cmdGearSplitQty.UseVisualStyleBackColor = true;
            this.cmdGearSplitQty.Click += new System.EventHandler(this.cmdGearSplitQty_Click);
            // 
            // cmdGearMergeQty
            // 
            this.cmdGearMergeQty.Enabled = false;
            this.cmdGearMergeQty.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdGearMergeQty.Image = global::Chummer.Properties.Resources.arrow_join;
            this.cmdGearMergeQty.Location = new System.Drawing.Point(561, 122);
            this.cmdGearMergeQty.Name = "cmdGearMergeQty";
            this.cmdGearMergeQty.Size = new System.Drawing.Size(24, 24);
            this.cmdGearMergeQty.TabIndex = 113;
            this.cmdGearMergeQty.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.tipTooltip.SetToolTip(this.cmdGearMergeQty, "Reduce the quantity of the Gear by 1.");
            this.cmdGearMergeQty.UseVisualStyleBackColor = true;
            this.cmdGearMergeQty.Click += new System.EventHandler(this.cmdGearMergeQty_Click);
            // 
            // cmdGearMoveToVehicle
            // 
            this.cmdGearMoveToVehicle.Enabled = false;
            this.cmdGearMoveToVehicle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdGearMoveToVehicle.Image = global::Chummer.Properties.Resources.car_go;
            this.cmdGearMoveToVehicle.Location = new System.Drawing.Point(617, 122);
            this.cmdGearMoveToVehicle.Name = "cmdGearMoveToVehicle";
            this.cmdGearMoveToVehicle.Size = new System.Drawing.Size(24, 24);
            this.cmdGearMoveToVehicle.TabIndex = 115;
            this.cmdGearMoveToVehicle.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.tipTooltip.SetToolTip(this.cmdGearMoveToVehicle, "Transfer to Vehicle.");
            this.cmdGearMoveToVehicle.UseVisualStyleBackColor = true;
            this.cmdGearMoveToVehicle.Click += new System.EventHandler(this.cmdGearMoveToVehicle_Click);
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
            this.tipTooltip.SetToolTip(this.lblFoci, "Each bonded Focus costs a number of BP equal to its Force.");
            // 
            // cmdWeaponBuyAmmo
            // 
            this.cmdWeaponBuyAmmo.Enabled = false;
            this.cmdWeaponBuyAmmo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdWeaponBuyAmmo.Image = global::Chummer.Properties.Resources.basket_add;
            this.cmdWeaponBuyAmmo.Location = new System.Drawing.Point(699, 280);
            this.cmdWeaponBuyAmmo.Name = "cmdWeaponBuyAmmo";
            this.cmdWeaponBuyAmmo.Size = new System.Drawing.Size(24, 24);
            this.cmdWeaponBuyAmmo.TabIndex = 107;
            this.cmdWeaponBuyAmmo.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.tipTooltip.SetToolTip(this.cmdWeaponBuyAmmo, "Buy additional Ammo for this Weapon");
            this.cmdWeaponBuyAmmo.UseVisualStyleBackColor = true;
            this.cmdWeaponBuyAmmo.Click += new System.EventHandler(this.cmdWeaponBuyAmmo_Click);
            // 
            // cmdWeaponMoveToVehicle
            // 
            this.cmdWeaponMoveToVehicle.Enabled = false;
            this.cmdWeaponMoveToVehicle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdWeaponMoveToVehicle.Image = global::Chummer.Properties.Resources.car_go;
            this.cmdWeaponMoveToVehicle.Location = new System.Drawing.Point(657, 145);
            this.cmdWeaponMoveToVehicle.Name = "cmdWeaponMoveToVehicle";
            this.cmdWeaponMoveToVehicle.Size = new System.Drawing.Size(24, 24);
            this.cmdWeaponMoveToVehicle.TabIndex = 116;
            this.cmdWeaponMoveToVehicle.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.tipTooltip.SetToolTip(this.cmdWeaponMoveToVehicle, "Transfer to Vehicle.");
            this.cmdWeaponMoveToVehicle.UseVisualStyleBackColor = true;
            this.cmdWeaponMoveToVehicle.Click += new System.EventHandler(this.cmdWeaponMoveToVehicle_Click);
            // 
            // chkWeaponAccessoryInstalled
            // 
            this.chkWeaponAccessoryInstalled.AutoSize = true;
            this.chkWeaponAccessoryInstalled.Enabled = false;
            this.chkWeaponAccessoryInstalled.Location = new System.Drawing.Point(586, 150);
            this.chkWeaponAccessoryInstalled.Name = "chkWeaponAccessoryInstalled";
            this.chkWeaponAccessoryInstalled.Size = new System.Drawing.Size(65, 17);
            this.chkWeaponAccessoryInstalled.TabIndex = 79;
            this.chkWeaponAccessoryInstalled.Tag = "Checkbox_Installed";
            this.chkWeaponAccessoryInstalled.Text = "Installed";
            this.tipTooltip.SetToolTip(this.chkWeaponAccessoryInstalled, "Installed Weapon Accessories and Mods count towards a Weapon\'s stats.");
            this.chkWeaponAccessoryInstalled.UseVisualStyleBackColor = true;
            this.chkWeaponAccessoryInstalled.CheckedChanged += new System.EventHandler(this.chkWeaponAccessoryInstalled_CheckedChanged);
            // 
            // cmdArmorDecrease
            // 
            this.cmdArmorDecrease.Enabled = false;
            this.cmdArmorDecrease.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdArmorDecrease.Image = global::Chummer.Properties.Resources.delete;
            this.cmdArmorDecrease.Location = new System.Drawing.Point(433, 30);
            this.cmdArmorDecrease.Name = "cmdArmorDecrease";
            this.cmdArmorDecrease.Size = new System.Drawing.Size(24, 24);
            this.cmdArmorDecrease.TabIndex = 94;
            this.cmdArmorDecrease.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.tipTooltip.SetToolTip(this.cmdArmorDecrease, "Damage Armor Rating");
            this.cmdArmorDecrease.UseVisualStyleBackColor = true;
            this.cmdArmorDecrease.Click += new System.EventHandler(this.cmdArmorDecrease_Click);
            // 
            // cmdArmorIncrease
            // 
            this.cmdArmorIncrease.Enabled = false;
            this.cmdArmorIncrease.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdArmorIncrease.Image = global::Chummer.Properties.Resources.add;
            this.cmdArmorIncrease.Location = new System.Drawing.Point(403, 30);
            this.cmdArmorIncrease.Name = "cmdArmorIncrease";
            this.cmdArmorIncrease.Size = new System.Drawing.Size(24, 24);
            this.cmdArmorIncrease.TabIndex = 95;
            this.cmdArmorIncrease.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.tipTooltip.SetToolTip(this.cmdArmorIncrease, "Repair Armor Rating");
            this.cmdArmorIncrease.UseVisualStyleBackColor = true;
            this.cmdArmorIncrease.Click += new System.EventHandler(this.cmdArmorIncrease_Click);
            // 
            // chkArmorEquipped
            // 
            this.chkArmorEquipped.AutoSize = true;
            this.chkArmorEquipped.Enabled = false;
            this.chkArmorEquipped.Location = new System.Drawing.Point(310, 155);
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
            // cmdDecreaseLifestyleMonths
            // 
            this.cmdDecreaseLifestyleMonths.Enabled = false;
            this.cmdDecreaseLifestyleMonths.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdDecreaseLifestyleMonths.Image = global::Chummer.Properties.Resources.delete;
            this.cmdDecreaseLifestyleMonths.Location = new System.Drawing.Point(374, 58);
            this.cmdDecreaseLifestyleMonths.Name = "cmdDecreaseLifestyleMonths";
            this.cmdDecreaseLifestyleMonths.Size = new System.Drawing.Size(24, 24);
            this.cmdDecreaseLifestyleMonths.TabIndex = 92;
            this.cmdDecreaseLifestyleMonths.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.tipTooltip.SetToolTip(this.cmdDecreaseLifestyleMonths, "Decrease the number of pre-paid months for the Lifestyle");
            this.cmdDecreaseLifestyleMonths.UseVisualStyleBackColor = true;
            this.cmdDecreaseLifestyleMonths.Click += new System.EventHandler(this.cmdDecreaseLifestyleMonths_Click);
            // 
            // cmdIncreaseLifestyleMonths
            // 
            this.cmdIncreaseLifestyleMonths.Enabled = false;
            this.cmdIncreaseLifestyleMonths.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdIncreaseLifestyleMonths.Image = global::Chummer.Properties.Resources.add;
            this.cmdIncreaseLifestyleMonths.Location = new System.Drawing.Point(344, 58);
            this.cmdIncreaseLifestyleMonths.Name = "cmdIncreaseLifestyleMonths";
            this.cmdIncreaseLifestyleMonths.Size = new System.Drawing.Size(24, 24);
            this.cmdIncreaseLifestyleMonths.TabIndex = 93;
            this.cmdIncreaseLifestyleMonths.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.tipTooltip.SetToolTip(this.cmdIncreaseLifestyleMonths, "Increase the number of pre-paid months for the Lifestyle");
            this.cmdIncreaseLifestyleMonths.UseVisualStyleBackColor = true;
            this.cmdIncreaseLifestyleMonths.Click += new System.EventHandler(this.cmdIncreaseLifestyleMonths_Click);
            // 
            // lblAttributesAug
            // 
            this.lblAttributesAug.AutoSize = true;
            this.lblAttributesAug.Location = new System.Drawing.Point(486, 29);
            this.lblAttributesAug.Name = "lblAttributesAug";
            this.lblAttributesAug.Size = new System.Drawing.Size(50, 13);
            this.lblAttributesAug.TabIndex = 105;
            this.lblAttributesAug.Tag = "Label_ValAugmented";
            this.lblAttributesAug.Text = "Val (Aug)";
            this.tipTooltip.SetToolTip(this.lblAttributesAug, "Augmented Attribute value.");
            // 
            // lblAttributesMetatype
            // 
            this.lblAttributesMetatype.AutoSize = true;
            this.lblAttributesMetatype.Location = new System.Drawing.Point(542, 29);
            this.lblAttributesMetatype.Name = "lblAttributesMetatype";
            this.lblAttributesMetatype.Size = new System.Drawing.Size(80, 13);
            this.lblAttributesMetatype.TabIndex = 104;
            this.lblAttributesMetatype.Tag = "Label_MetatypeLimits";
            this.lblAttributesMetatype.Text = "Metatype Limits";
            this.tipTooltip.SetToolTip(this.lblAttributesMetatype, "Metatype Minimum / Maximum (Augmented Maximum) values.");
            // 
            // lblAttributes
            // 
            this.lblAttributes.AutoSize = true;
            this.lblAttributes.Location = new System.Drawing.Point(286, 29);
            this.lblAttributes.Name = "lblAttributes";
            this.lblAttributes.Size = new System.Drawing.Size(51, 13);
            this.lblAttributes.TabIndex = 103;
            this.lblAttributes.Tag = "Label_Attributes";
            this.lblAttributes.Text = "Attributes";
            this.tipTooltip.SetToolTip(this.lblAttributes, "Only one attribute may be at its Maximum value during character creation (not inc" +
        "luding EDG, MAG, and RES).");
            // 
            // lblMovementLabel
            // 
            this.lblMovementLabel.AutoSize = true;
            this.lblMovementLabel.Location = new System.Drawing.Point(6, 396);
            this.lblMovementLabel.Name = "lblMovementLabel";
            this.lblMovementLabel.Size = new System.Drawing.Size(60, 13);
            this.lblMovementLabel.TabIndex = 44;
            this.lblMovementLabel.Tag = "Label_OtherMovement";
            this.lblMovementLabel.Text = "Movement:";
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
            // cmsDeleteCyberware
            // 
            this.cmsDeleteCyberware.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsCyberwareSell});
            this.cmsDeleteCyberware.Name = "cmsCyberware";
            this.cmsDeleteCyberware.Size = new System.Drawing.Size(120, 26);
            this.cmsDeleteCyberware.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
            // 
            // tsCyberwareSell
            // 
            this.tsCyberwareSell.Image = global::Chummer.Properties.Resources.brick_delete;
            this.tsCyberwareSell.Name = "tsCyberwareSell";
            this.tsCyberwareSell.Size = new System.Drawing.Size(119, 22);
            this.tsCyberwareSell.Tag = "Menu_SellItem";
            this.tsCyberwareSell.Text = "&Sell Item";
            this.tsCyberwareSell.Click += new System.EventHandler(this.tsCyberwareSell_Click);
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
            // cmsDeleteArmor
            // 
            this.cmsDeleteArmor.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsArmorSell});
            this.cmsDeleteArmor.Name = "cmsDeleteArmor";
            this.cmsDeleteArmor.Size = new System.Drawing.Size(120, 26);
            this.cmsDeleteArmor.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
            // 
            // tsArmorSell
            // 
            this.tsArmorSell.Image = global::Chummer.Properties.Resources.brick_delete;
            this.tsArmorSell.Name = "tsArmorSell";
            this.tsArmorSell.Size = new System.Drawing.Size(119, 22);
            this.tsArmorSell.Tag = "Menu_SellItem";
            this.tsArmorSell.Text = "&Sell Item";
            this.tsArmorSell.Click += new System.EventHandler(this.tsArmorSell_Click);
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
            // cmsDeleteWeapon
            // 
            this.cmsDeleteWeapon.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsWeaponSell});
            this.cmsDeleteWeapon.Name = "cmsDeleteWeapon";
            this.cmsDeleteWeapon.Size = new System.Drawing.Size(120, 26);
            this.cmsDeleteWeapon.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
            // 
            // tsWeaponSell
            // 
            this.tsWeaponSell.Image = global::Chummer.Properties.Resources.brick_delete;
            this.tsWeaponSell.Name = "tsWeaponSell";
            this.tsWeaponSell.Size = new System.Drawing.Size(119, 22);
            this.tsWeaponSell.Tag = "Menu_SellItem";
            this.tsWeaponSell.Text = "&Sell Item";
            this.tsWeaponSell.Click += new System.EventHandler(this.tsWeaponSell_Click);
            // 
            // cmsAmmoExpense
            // 
            this.cmsAmmoExpense.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmsAmmoSingleShot,
            this.cmsAmmoShortBurst,
            this.cmsAmmoLongBurst,
            this.cmsAmmoFullBurst,
            this.cmsAmmoSuppressiveFire});
            this.cmsAmmoExpense.Name = "cmsAmmoExpense";
            this.cmsAmmoExpense.Size = new System.Drawing.Size(220, 114);
            this.cmsAmmoExpense.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
            // 
            // cmsAmmoSingleShot
            // 
            this.cmsAmmoSingleShot.Name = "cmsAmmoSingleShot";
            this.cmsAmmoSingleShot.Size = new System.Drawing.Size(219, 22);
            this.cmsAmmoSingleShot.Tag = "String_SingleShot";
            this.cmsAmmoSingleShot.Text = "Single Shot (1 bullet)";
            this.cmsAmmoSingleShot.Click += new System.EventHandler(this.cmsAmmoSingleShot_Click);
            // 
            // cmsAmmoShortBurst
            // 
            this.cmsAmmoShortBurst.Name = "cmsAmmoShortBurst";
            this.cmsAmmoShortBurst.Size = new System.Drawing.Size(219, 22);
            this.cmsAmmoShortBurst.Tag = "String_ShortBurst";
            this.cmsAmmoShortBurst.Text = "Short Burst (3 bullets)";
            this.cmsAmmoShortBurst.Click += new System.EventHandler(this.cmsAmmoShortBurst_Click);
            // 
            // cmsAmmoLongBurst
            // 
            this.cmsAmmoLongBurst.Name = "cmsAmmoLongBurst";
            this.cmsAmmoLongBurst.Size = new System.Drawing.Size(219, 22);
            this.cmsAmmoLongBurst.Tag = "String_LongBurst";
            this.cmsAmmoLongBurst.Text = "Long Burst (6 bullets)";
            this.cmsAmmoLongBurst.Click += new System.EventHandler(this.cmsAmmoLongBurst_Click);
            // 
            // cmsAmmoFullBurst
            // 
            this.cmsAmmoFullBurst.Name = "cmsAmmoFullBurst";
            this.cmsAmmoFullBurst.Size = new System.Drawing.Size(219, 22);
            this.cmsAmmoFullBurst.Text = "Full Burst (10 bullets)";
            this.cmsAmmoFullBurst.Click += new System.EventHandler(this.cmsAmmoFullBurst_Click);
            // 
            // cmsAmmoSuppressiveFire
            // 
            this.cmsAmmoSuppressiveFire.Name = "cmsAmmoSuppressiveFire";
            this.cmsAmmoSuppressiveFire.Size = new System.Drawing.Size(219, 22);
            this.cmsAmmoSuppressiveFire.Text = "Suppressive Fire (20 bullets)";
            this.cmsAmmoSuppressiveFire.Click += new System.EventHandler(this.cmsAmmoSuppressiveFire_Click);
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
            // cmsDeleteGear
            // 
            this.cmsDeleteGear.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sellItemToolStripMenuItem});
            this.cmsDeleteGear.Name = "cmsDeleteGear";
            this.cmsDeleteGear.Size = new System.Drawing.Size(120, 26);
            this.cmsDeleteGear.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
            // 
            // sellItemToolStripMenuItem
            // 
            this.sellItemToolStripMenuItem.Image = global::Chummer.Properties.Resources.brick_delete;
            this.sellItemToolStripMenuItem.Name = "sellItemToolStripMenuItem";
            this.sellItemToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.sellItemToolStripMenuItem.Tag = "Menu_SellItem";
            this.sellItemToolStripMenuItem.Text = "&Sell Item";
            this.sellItemToolStripMenuItem.Click += new System.EventHandler(this.sellItemToolStripMenuItem_Click);
            // 
            // cmsVehicle
            // 
            this.cmsVehicle.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsVehicleAddMod,
            this.tsVehicleAddCyberware,
            this.tsVehicleAddSensor,
            this.tsVehicleAddWeapon,
            this.tsVehicleName,
            this.tsVehicleNotes});
            this.cmsVehicle.Name = "cmsWeapon";
            this.cmsVehicle.Size = new System.Drawing.Size(193, 136);
            this.cmsVehicle.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
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
            // cmdVehicleAmmoExpense
            // 
            this.cmdVehicleAmmoExpense.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmsVehicleAmmoSingleShot,
            this.cmsVehicleAmmoShortBurst,
            this.cmsVehicleAmmoLongBurst,
            this.cmsVehicleAmmoFullBurst,
            this.cmsVehicleAmmoSuppressiveFire});
            this.cmdVehicleAmmoExpense.Name = "contextMenuStrip1";
            this.cmdVehicleAmmoExpense.Size = new System.Drawing.Size(220, 114);
            this.cmdVehicleAmmoExpense.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
            // 
            // cmsVehicleAmmoSingleShot
            // 
            this.cmsVehicleAmmoSingleShot.Name = "cmsVehicleAmmoSingleShot";
            this.cmsVehicleAmmoSingleShot.Size = new System.Drawing.Size(219, 22);
            this.cmsVehicleAmmoSingleShot.Tag = "String_SingleShot";
            this.cmsVehicleAmmoSingleShot.Text = "Single Shot (1 bullet)";
            this.cmsVehicleAmmoSingleShot.Click += new System.EventHandler(this.cmsVehicleAmmoSingleShot_Click);
            // 
            // cmsVehicleAmmoShortBurst
            // 
            this.cmsVehicleAmmoShortBurst.Name = "cmsVehicleAmmoShortBurst";
            this.cmsVehicleAmmoShortBurst.Size = new System.Drawing.Size(219, 22);
            this.cmsVehicleAmmoShortBurst.Tag = "String_ShortBurst";
            this.cmsVehicleAmmoShortBurst.Text = "Short Burst (3 bullets)";
            this.cmsVehicleAmmoShortBurst.Click += new System.EventHandler(this.cmsVehicleAmmoShortBurst_Click);
            // 
            // cmsVehicleAmmoLongBurst
            // 
            this.cmsVehicleAmmoLongBurst.Name = "cmsVehicleAmmoLongBurst";
            this.cmsVehicleAmmoLongBurst.Size = new System.Drawing.Size(219, 22);
            this.cmsVehicleAmmoLongBurst.Tag = "String_LongBurst";
            this.cmsVehicleAmmoLongBurst.Text = "Long Burst (6 bullets)";
            this.cmsVehicleAmmoLongBurst.Click += new System.EventHandler(this.cmsVehicleAmmoLongBurst_Click);
            // 
            // cmsVehicleAmmoFullBurst
            // 
            this.cmsVehicleAmmoFullBurst.Name = "cmsVehicleAmmoFullBurst";
            this.cmsVehicleAmmoFullBurst.Size = new System.Drawing.Size(219, 22);
            this.cmsVehicleAmmoFullBurst.Text = "Full Burst (10 bullets)";
            this.cmsVehicleAmmoFullBurst.Click += new System.EventHandler(this.cmsVehicleAmmoFullBurst_Click);
            // 
            // cmsVehicleAmmoSuppressiveFire
            // 
            this.cmsVehicleAmmoSuppressiveFire.Name = "cmsVehicleAmmoSuppressiveFire";
            this.cmsVehicleAmmoSuppressiveFire.Size = new System.Drawing.Size(219, 22);
            this.cmsVehicleAmmoSuppressiveFire.Text = "Suppressive Fire (20 bullets)";
            this.cmsVehicleAmmoSuppressiveFire.Click += new System.EventHandler(this.cmsVehicleAmmoSuppressiveFire_Click);
            // 
            // cmsDeleteVehicle
            // 
            this.cmsDeleteVehicle.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsVehicleSell});
            this.cmsDeleteVehicle.Name = "cmsDeleteVehicle";
            this.cmsDeleteVehicle.Size = new System.Drawing.Size(120, 26);
            this.cmsDeleteVehicle.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
            // 
            // tsVehicleSell
            // 
            this.tsVehicleSell.Image = global::Chummer.Properties.Resources.brick_delete;
            this.tsVehicleSell.Name = "tsVehicleSell";
            this.tsVehicleSell.Size = new System.Drawing.Size(119, 22);
            this.tsVehicleSell.Tag = "Menu_SellItem";
            this.tsVehicleSell.Text = "&Sell Item";
            this.tsVehicleSell.Click += new System.EventHandler(this.tsVehicleSell_Click);
            // 
            // tsWeaponAddModification
            // 
            this.tsWeaponAddModification.Name = "tsWeaponAddModification";
            this.tsWeaponAddModification.Size = new System.Drawing.Size(32, 19);
            // 
            // tsBoltHole
            // 
            this.tsBoltHole.Name = "tsBoltHole";
            this.tsBoltHole.Size = new System.Drawing.Size(32, 19);
            // 
            // tsSafehouse
            // 
            this.tsSafehouse.Name = "tsSafehouse";
            this.tsSafehouse.Size = new System.Drawing.Size(32, 19);
            // 
            // lblArmor
            // 
            this.lblArmor.AutoSize = true;
            this.lblArmor.Location = new System.Drawing.Point(139, 189);
            this.lblArmor.Name = "lblArmor";
            this.lblArmor.Size = new System.Drawing.Size(13, 13);
            this.lblArmor.TabIndex = 31;
            this.lblArmor.Text = "0";
            // 
            // panStunCM
            // 
            this.panStunCM.Controls.Add(this.lblStunCMLabel);
            this.panStunCM.Controls.Add(this.chkStunCM18);
            this.panStunCM.Controls.Add(this.chkStunCM17);
            this.panStunCM.Controls.Add(this.chkStunCM16);
            this.panStunCM.Controls.Add(this.chkStunCM15);
            this.panStunCM.Controls.Add(this.chkStunCM14);
            this.panStunCM.Controls.Add(this.chkStunCM13);
            this.panStunCM.Controls.Add(this.chkStunCM12);
            this.panStunCM.Controls.Add(this.chkStunCM11);
            this.panStunCM.Controls.Add(this.chkStunCM10);
            this.panStunCM.Controls.Add(this.chkStunCM9);
            this.panStunCM.Controls.Add(this.chkStunCM8);
            this.panStunCM.Controls.Add(this.chkStunCM7);
            this.panStunCM.Controls.Add(this.chkStunCM6);
            this.panStunCM.Controls.Add(this.chkStunCM5);
            this.panStunCM.Controls.Add(this.chkStunCM4);
            this.panStunCM.Controls.Add(this.chkStunCM3);
            this.panStunCM.Controls.Add(this.chkStunCM2);
            this.panStunCM.Controls.Add(this.chkStunCM1);
            this.panStunCM.Location = new System.Drawing.Point(6, 321);
            this.panStunCM.Name = "panStunCM";
            this.panStunCM.Size = new System.Drawing.Size(134, 163);
            this.panStunCM.TabIndex = 0;
            // 
            // lblStunCMLabel
            // 
            this.lblStunCMLabel.AutoSize = true;
            this.lblStunCMLabel.Location = new System.Drawing.Point(0, 0);
            this.lblStunCMLabel.Name = "lblStunCMLabel";
            this.lblStunCMLabel.Size = new System.Drawing.Size(29, 13);
            this.lblStunCMLabel.TabIndex = 19;
            this.lblStunCMLabel.Tag = "Label_CMStun";
            this.lblStunCMLabel.Text = "Stun";
            // 
            // chkStunCM18
            // 
            this.chkStunCM18.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkStunCM18.Location = new System.Drawing.Point(51, 136);
            this.chkStunCM18.Name = "chkStunCM18";
            this.chkStunCM18.Size = new System.Drawing.Size(24, 24);
            this.chkStunCM18.TabIndex = 17;
            this.chkStunCM18.Tag = "18";
            this.chkStunCM18.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkStunCM18.UseVisualStyleBackColor = true;
            this.chkStunCM18.CheckedChanged += new System.EventHandler(this.chkStunCM_CheckedChanged);
            // 
            // chkStunCM17
            // 
            this.chkStunCM17.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkStunCM17.Location = new System.Drawing.Point(27, 136);
            this.chkStunCM17.Name = "chkStunCM17";
            this.chkStunCM17.Size = new System.Drawing.Size(24, 24);
            this.chkStunCM17.TabIndex = 16;
            this.chkStunCM17.Tag = "17";
            this.chkStunCM17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkStunCM17.UseVisualStyleBackColor = true;
            this.chkStunCM17.CheckedChanged += new System.EventHandler(this.chkStunCM_CheckedChanged);
            // 
            // chkStunCM16
            // 
            this.chkStunCM16.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkStunCM16.Location = new System.Drawing.Point(3, 136);
            this.chkStunCM16.Name = "chkStunCM16";
            this.chkStunCM16.Size = new System.Drawing.Size(24, 24);
            this.chkStunCM16.TabIndex = 15;
            this.chkStunCM16.Tag = "16";
            this.chkStunCM16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkStunCM16.UseVisualStyleBackColor = true;
            this.chkStunCM16.CheckedChanged += new System.EventHandler(this.chkStunCM_CheckedChanged);
            // 
            // chkStunCM15
            // 
            this.chkStunCM15.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkStunCM15.Location = new System.Drawing.Point(51, 112);
            this.chkStunCM15.Name = "chkStunCM15";
            this.chkStunCM15.Size = new System.Drawing.Size(24, 24);
            this.chkStunCM15.TabIndex = 14;
            this.chkStunCM15.Tag = "15";
            this.chkStunCM15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkStunCM15.UseVisualStyleBackColor = true;
            this.chkStunCM15.CheckedChanged += new System.EventHandler(this.chkStunCM_CheckedChanged);
            // 
            // chkStunCM14
            // 
            this.chkStunCM14.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkStunCM14.Location = new System.Drawing.Point(27, 112);
            this.chkStunCM14.Name = "chkStunCM14";
            this.chkStunCM14.Size = new System.Drawing.Size(24, 24);
            this.chkStunCM14.TabIndex = 13;
            this.chkStunCM14.Tag = "14";
            this.chkStunCM14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkStunCM14.UseVisualStyleBackColor = true;
            this.chkStunCM14.CheckedChanged += new System.EventHandler(this.chkStunCM_CheckedChanged);
            // 
            // chkStunCM13
            // 
            this.chkStunCM13.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkStunCM13.Location = new System.Drawing.Point(3, 112);
            this.chkStunCM13.Name = "chkStunCM13";
            this.chkStunCM13.Size = new System.Drawing.Size(24, 24);
            this.chkStunCM13.TabIndex = 12;
            this.chkStunCM13.Tag = "13";
            this.chkStunCM13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkStunCM13.UseVisualStyleBackColor = true;
            this.chkStunCM13.CheckedChanged += new System.EventHandler(this.chkStunCM_CheckedChanged);
            // 
            // chkStunCM12
            // 
            this.chkStunCM12.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkStunCM12.Location = new System.Drawing.Point(51, 88);
            this.chkStunCM12.Name = "chkStunCM12";
            this.chkStunCM12.Size = new System.Drawing.Size(24, 24);
            this.chkStunCM12.TabIndex = 11;
            this.chkStunCM12.Tag = "12";
            this.chkStunCM12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkStunCM12.UseVisualStyleBackColor = true;
            this.chkStunCM12.CheckedChanged += new System.EventHandler(this.chkStunCM_CheckedChanged);
            // 
            // chkStunCM11
            // 
            this.chkStunCM11.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkStunCM11.Location = new System.Drawing.Point(27, 88);
            this.chkStunCM11.Name = "chkStunCM11";
            this.chkStunCM11.Size = new System.Drawing.Size(24, 24);
            this.chkStunCM11.TabIndex = 10;
            this.chkStunCM11.Tag = "11";
            this.chkStunCM11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkStunCM11.UseVisualStyleBackColor = true;
            this.chkStunCM11.CheckedChanged += new System.EventHandler(this.chkStunCM_CheckedChanged);
            // 
            // chkStunCM10
            // 
            this.chkStunCM10.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkStunCM10.Location = new System.Drawing.Point(3, 88);
            this.chkStunCM10.Name = "chkStunCM10";
            this.chkStunCM10.Size = new System.Drawing.Size(24, 24);
            this.chkStunCM10.TabIndex = 9;
            this.chkStunCM10.Tag = "10";
            this.chkStunCM10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkStunCM10.UseVisualStyleBackColor = true;
            this.chkStunCM10.CheckedChanged += new System.EventHandler(this.chkStunCM_CheckedChanged);
            // 
            // chkStunCM9
            // 
            this.chkStunCM9.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkStunCM9.Location = new System.Drawing.Point(51, 64);
            this.chkStunCM9.Name = "chkStunCM9";
            this.chkStunCM9.Size = new System.Drawing.Size(24, 24);
            this.chkStunCM9.TabIndex = 8;
            this.chkStunCM9.Tag = "9";
            this.chkStunCM9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkStunCM9.UseVisualStyleBackColor = true;
            this.chkStunCM9.CheckedChanged += new System.EventHandler(this.chkStunCM_CheckedChanged);
            // 
            // chkStunCM8
            // 
            this.chkStunCM8.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkStunCM8.Location = new System.Drawing.Point(27, 64);
            this.chkStunCM8.Name = "chkStunCM8";
            this.chkStunCM8.Size = new System.Drawing.Size(24, 24);
            this.chkStunCM8.TabIndex = 7;
            this.chkStunCM8.Tag = "8";
            this.chkStunCM8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkStunCM8.UseVisualStyleBackColor = true;
            this.chkStunCM8.CheckedChanged += new System.EventHandler(this.chkStunCM_CheckedChanged);
            // 
            // chkStunCM7
            // 
            this.chkStunCM7.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkStunCM7.Location = new System.Drawing.Point(3, 64);
            this.chkStunCM7.Name = "chkStunCM7";
            this.chkStunCM7.Size = new System.Drawing.Size(24, 24);
            this.chkStunCM7.TabIndex = 6;
            this.chkStunCM7.Tag = "7";
            this.chkStunCM7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkStunCM7.UseVisualStyleBackColor = true;
            this.chkStunCM7.CheckedChanged += new System.EventHandler(this.chkStunCM_CheckedChanged);
            // 
            // chkStunCM6
            // 
            this.chkStunCM6.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkStunCM6.Location = new System.Drawing.Point(51, 40);
            this.chkStunCM6.Name = "chkStunCM6";
            this.chkStunCM6.Size = new System.Drawing.Size(24, 24);
            this.chkStunCM6.TabIndex = 5;
            this.chkStunCM6.Tag = "6";
            this.chkStunCM6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkStunCM6.UseVisualStyleBackColor = true;
            this.chkStunCM6.CheckedChanged += new System.EventHandler(this.chkStunCM_CheckedChanged);
            // 
            // chkStunCM5
            // 
            this.chkStunCM5.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkStunCM5.Location = new System.Drawing.Point(27, 40);
            this.chkStunCM5.Name = "chkStunCM5";
            this.chkStunCM5.Size = new System.Drawing.Size(24, 24);
            this.chkStunCM5.TabIndex = 4;
            this.chkStunCM5.Tag = "5";
            this.chkStunCM5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkStunCM5.UseVisualStyleBackColor = true;
            this.chkStunCM5.CheckedChanged += new System.EventHandler(this.chkStunCM_CheckedChanged);
            // 
            // chkStunCM4
            // 
            this.chkStunCM4.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkStunCM4.Location = new System.Drawing.Point(3, 40);
            this.chkStunCM4.Name = "chkStunCM4";
            this.chkStunCM4.Size = new System.Drawing.Size(24, 24);
            this.chkStunCM4.TabIndex = 3;
            this.chkStunCM4.Tag = "4";
            this.chkStunCM4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkStunCM4.UseVisualStyleBackColor = true;
            this.chkStunCM4.CheckedChanged += new System.EventHandler(this.chkStunCM_CheckedChanged);
            // 
            // chkStunCM3
            // 
            this.chkStunCM3.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkStunCM3.Location = new System.Drawing.Point(51, 16);
            this.chkStunCM3.Name = "chkStunCM3";
            this.chkStunCM3.Size = new System.Drawing.Size(24, 24);
            this.chkStunCM3.TabIndex = 2;
            this.chkStunCM3.Tag = "3";
            this.chkStunCM3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkStunCM3.UseVisualStyleBackColor = true;
            this.chkStunCM3.CheckedChanged += new System.EventHandler(this.chkStunCM_CheckedChanged);
            // 
            // chkStunCM2
            // 
            this.chkStunCM2.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkStunCM2.Location = new System.Drawing.Point(27, 16);
            this.chkStunCM2.Name = "chkStunCM2";
            this.chkStunCM2.Size = new System.Drawing.Size(24, 24);
            this.chkStunCM2.TabIndex = 1;
            this.chkStunCM2.Tag = "2";
            this.chkStunCM2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkStunCM2.UseVisualStyleBackColor = true;
            this.chkStunCM2.CheckedChanged += new System.EventHandler(this.chkStunCM_CheckedChanged);
            // 
            // chkStunCM1
            // 
            this.chkStunCM1.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkStunCM1.Location = new System.Drawing.Point(3, 16);
            this.chkStunCM1.Name = "chkStunCM1";
            this.chkStunCM1.Size = new System.Drawing.Size(24, 24);
            this.chkStunCM1.TabIndex = 0;
            this.chkStunCM1.Tag = "1";
            this.chkStunCM1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkStunCM1.UseVisualStyleBackColor = true;
            this.chkStunCM1.CheckedChanged += new System.EventHandler(this.chkStunCM_CheckedChanged);
            // 
            // panPhysicalCM
            // 
            this.panPhysicalCM.Controls.Add(this.chkPhysicalCM24);
            this.panPhysicalCM.Controls.Add(this.chkPhysicalCM23);
            this.panPhysicalCM.Controls.Add(this.chkPhysicalCM22);
            this.panPhysicalCM.Controls.Add(this.chkPhysicalCM21);
            this.panPhysicalCM.Controls.Add(this.chkPhysicalCM20);
            this.panPhysicalCM.Controls.Add(this.chkPhysicalCM19);
            this.panPhysicalCM.Controls.Add(this.chkPhysicalCM18);
            this.panPhysicalCM.Controls.Add(this.lblPhysicalCMLabel);
            this.panPhysicalCM.Controls.Add(this.chkPhysicalCM17);
            this.panPhysicalCM.Controls.Add(this.chkPhysicalCM16);
            this.panPhysicalCM.Controls.Add(this.chkPhysicalCM15);
            this.panPhysicalCM.Controls.Add(this.chkPhysicalCM14);
            this.panPhysicalCM.Controls.Add(this.chkPhysicalCM13);
            this.panPhysicalCM.Controls.Add(this.chkPhysicalCM12);
            this.panPhysicalCM.Controls.Add(this.chkPhysicalCM11);
            this.panPhysicalCM.Controls.Add(this.chkPhysicalCM10);
            this.panPhysicalCM.Controls.Add(this.chkPhysicalCM9);
            this.panPhysicalCM.Controls.Add(this.chkPhysicalCM8);
            this.panPhysicalCM.Controls.Add(this.chkPhysicalCM7);
            this.panPhysicalCM.Controls.Add(this.chkPhysicalCM6);
            this.panPhysicalCM.Controls.Add(this.chkPhysicalCM5);
            this.panPhysicalCM.Controls.Add(this.chkPhysicalCM4);
            this.panPhysicalCM.Controls.Add(this.chkPhysicalCM3);
            this.panPhysicalCM.Controls.Add(this.chkPhysicalCM2);
            this.panPhysicalCM.Controls.Add(this.chkPhysicalCM1);
            this.panPhysicalCM.Location = new System.Drawing.Point(6, 100);
            this.panPhysicalCM.Name = "panPhysicalCM";
            this.panPhysicalCM.Size = new System.Drawing.Size(134, 215);
            this.panPhysicalCM.TabIndex = 36;
            // 
            // chkPhysicalCM24
            // 
            this.chkPhysicalCM24.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPhysicalCM24.Location = new System.Drawing.Point(51, 184);
            this.chkPhysicalCM24.Name = "chkPhysicalCM24";
            this.chkPhysicalCM24.Size = new System.Drawing.Size(24, 24);
            this.chkPhysicalCM24.TabIndex = 24;
            this.chkPhysicalCM24.Tag = "24";
            this.chkPhysicalCM24.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkPhysicalCM24.UseVisualStyleBackColor = true;
            // 
            // chkPhysicalCM23
            // 
            this.chkPhysicalCM23.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPhysicalCM23.Location = new System.Drawing.Point(27, 184);
            this.chkPhysicalCM23.Name = "chkPhysicalCM23";
            this.chkPhysicalCM23.Size = new System.Drawing.Size(24, 24);
            this.chkPhysicalCM23.TabIndex = 23;
            this.chkPhysicalCM23.Tag = "23";
            this.chkPhysicalCM23.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkPhysicalCM23.UseVisualStyleBackColor = true;
            // 
            // chkPhysicalCM22
            // 
            this.chkPhysicalCM22.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPhysicalCM22.Location = new System.Drawing.Point(3, 184);
            this.chkPhysicalCM22.Name = "chkPhysicalCM22";
            this.chkPhysicalCM22.Size = new System.Drawing.Size(24, 24);
            this.chkPhysicalCM22.TabIndex = 22;
            this.chkPhysicalCM22.Tag = "22";
            this.chkPhysicalCM22.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkPhysicalCM22.UseVisualStyleBackColor = true;
            // 
            // chkPhysicalCM21
            // 
            this.chkPhysicalCM21.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPhysicalCM21.Location = new System.Drawing.Point(51, 160);
            this.chkPhysicalCM21.Name = "chkPhysicalCM21";
            this.chkPhysicalCM21.Size = new System.Drawing.Size(24, 24);
            this.chkPhysicalCM21.TabIndex = 21;
            this.chkPhysicalCM21.Tag = "21";
            this.chkPhysicalCM21.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkPhysicalCM21.UseVisualStyleBackColor = true;
            // 
            // chkPhysicalCM20
            // 
            this.chkPhysicalCM20.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPhysicalCM20.Location = new System.Drawing.Point(27, 160);
            this.chkPhysicalCM20.Name = "chkPhysicalCM20";
            this.chkPhysicalCM20.Size = new System.Drawing.Size(24, 24);
            this.chkPhysicalCM20.TabIndex = 20;
            this.chkPhysicalCM20.Tag = "20";
            this.chkPhysicalCM20.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkPhysicalCM20.UseVisualStyleBackColor = true;
            // 
            // chkPhysicalCM19
            // 
            this.chkPhysicalCM19.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPhysicalCM19.Location = new System.Drawing.Point(3, 160);
            this.chkPhysicalCM19.Name = "chkPhysicalCM19";
            this.chkPhysicalCM19.Size = new System.Drawing.Size(24, 24);
            this.chkPhysicalCM19.TabIndex = 19;
            this.chkPhysicalCM19.Tag = "19";
            this.chkPhysicalCM19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkPhysicalCM19.UseVisualStyleBackColor = true;
            // 
            // chkPhysicalCM18
            // 
            this.chkPhysicalCM18.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPhysicalCM18.Location = new System.Drawing.Point(51, 136);
            this.chkPhysicalCM18.Name = "chkPhysicalCM18";
            this.chkPhysicalCM18.Size = new System.Drawing.Size(24, 24);
            this.chkPhysicalCM18.TabIndex = 17;
            this.chkPhysicalCM18.Tag = "18";
            this.chkPhysicalCM18.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkPhysicalCM18.UseVisualStyleBackColor = true;
            this.chkPhysicalCM18.CheckedChanged += new System.EventHandler(this.chkPhysicalCM_CheckedChanged);
            // 
            // lblPhysicalCMLabel
            // 
            this.lblPhysicalCMLabel.AutoSize = true;
            this.lblPhysicalCMLabel.Location = new System.Drawing.Point(0, 0);
            this.lblPhysicalCMLabel.Name = "lblPhysicalCMLabel";
            this.lblPhysicalCMLabel.Size = new System.Drawing.Size(46, 13);
            this.lblPhysicalCMLabel.TabIndex = 18;
            this.lblPhysicalCMLabel.Tag = "Label_CMPhysical";
            this.lblPhysicalCMLabel.Text = "Physical";
            // 
            // chkPhysicalCM17
            // 
            this.chkPhysicalCM17.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPhysicalCM17.Location = new System.Drawing.Point(27, 136);
            this.chkPhysicalCM17.Name = "chkPhysicalCM17";
            this.chkPhysicalCM17.Size = new System.Drawing.Size(24, 24);
            this.chkPhysicalCM17.TabIndex = 16;
            this.chkPhysicalCM17.Tag = "17";
            this.chkPhysicalCM17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkPhysicalCM17.UseVisualStyleBackColor = true;
            this.chkPhysicalCM17.CheckedChanged += new System.EventHandler(this.chkPhysicalCM_CheckedChanged);
            // 
            // chkPhysicalCM16
            // 
            this.chkPhysicalCM16.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPhysicalCM16.Location = new System.Drawing.Point(3, 136);
            this.chkPhysicalCM16.Name = "chkPhysicalCM16";
            this.chkPhysicalCM16.Size = new System.Drawing.Size(24, 24);
            this.chkPhysicalCM16.TabIndex = 15;
            this.chkPhysicalCM16.Tag = "16";
            this.chkPhysicalCM16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkPhysicalCM16.UseVisualStyleBackColor = true;
            this.chkPhysicalCM16.CheckedChanged += new System.EventHandler(this.chkPhysicalCM_CheckedChanged);
            // 
            // chkPhysicalCM15
            // 
            this.chkPhysicalCM15.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPhysicalCM15.Location = new System.Drawing.Point(51, 112);
            this.chkPhysicalCM15.Name = "chkPhysicalCM15";
            this.chkPhysicalCM15.Size = new System.Drawing.Size(24, 24);
            this.chkPhysicalCM15.TabIndex = 14;
            this.chkPhysicalCM15.Tag = "15";
            this.chkPhysicalCM15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkPhysicalCM15.UseVisualStyleBackColor = true;
            this.chkPhysicalCM15.CheckedChanged += new System.EventHandler(this.chkPhysicalCM_CheckedChanged);
            // 
            // chkPhysicalCM14
            // 
            this.chkPhysicalCM14.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPhysicalCM14.Location = new System.Drawing.Point(27, 112);
            this.chkPhysicalCM14.Name = "chkPhysicalCM14";
            this.chkPhysicalCM14.Size = new System.Drawing.Size(24, 24);
            this.chkPhysicalCM14.TabIndex = 13;
            this.chkPhysicalCM14.Tag = "14";
            this.chkPhysicalCM14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkPhysicalCM14.UseVisualStyleBackColor = true;
            this.chkPhysicalCM14.CheckedChanged += new System.EventHandler(this.chkPhysicalCM_CheckedChanged);
            // 
            // chkPhysicalCM13
            // 
            this.chkPhysicalCM13.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPhysicalCM13.Location = new System.Drawing.Point(3, 112);
            this.chkPhysicalCM13.Name = "chkPhysicalCM13";
            this.chkPhysicalCM13.Size = new System.Drawing.Size(24, 24);
            this.chkPhysicalCM13.TabIndex = 12;
            this.chkPhysicalCM13.Tag = "13";
            this.chkPhysicalCM13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkPhysicalCM13.UseVisualStyleBackColor = true;
            this.chkPhysicalCM13.CheckedChanged += new System.EventHandler(this.chkPhysicalCM_CheckedChanged);
            // 
            // chkPhysicalCM12
            // 
            this.chkPhysicalCM12.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPhysicalCM12.Location = new System.Drawing.Point(51, 88);
            this.chkPhysicalCM12.Name = "chkPhysicalCM12";
            this.chkPhysicalCM12.Size = new System.Drawing.Size(24, 24);
            this.chkPhysicalCM12.TabIndex = 11;
            this.chkPhysicalCM12.Tag = "12";
            this.chkPhysicalCM12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkPhysicalCM12.UseVisualStyleBackColor = true;
            this.chkPhysicalCM12.CheckedChanged += new System.EventHandler(this.chkPhysicalCM_CheckedChanged);
            // 
            // chkPhysicalCM11
            // 
            this.chkPhysicalCM11.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPhysicalCM11.Location = new System.Drawing.Point(27, 88);
            this.chkPhysicalCM11.Name = "chkPhysicalCM11";
            this.chkPhysicalCM11.Size = new System.Drawing.Size(24, 24);
            this.chkPhysicalCM11.TabIndex = 10;
            this.chkPhysicalCM11.Tag = "11";
            this.chkPhysicalCM11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkPhysicalCM11.UseVisualStyleBackColor = true;
            this.chkPhysicalCM11.CheckedChanged += new System.EventHandler(this.chkPhysicalCM_CheckedChanged);
            // 
            // chkPhysicalCM10
            // 
            this.chkPhysicalCM10.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPhysicalCM10.Location = new System.Drawing.Point(3, 88);
            this.chkPhysicalCM10.Name = "chkPhysicalCM10";
            this.chkPhysicalCM10.Size = new System.Drawing.Size(24, 24);
            this.chkPhysicalCM10.TabIndex = 9;
            this.chkPhysicalCM10.Tag = "10";
            this.chkPhysicalCM10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkPhysicalCM10.UseVisualStyleBackColor = true;
            this.chkPhysicalCM10.CheckedChanged += new System.EventHandler(this.chkPhysicalCM_CheckedChanged);
            // 
            // chkPhysicalCM9
            // 
            this.chkPhysicalCM9.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPhysicalCM9.Location = new System.Drawing.Point(51, 64);
            this.chkPhysicalCM9.Name = "chkPhysicalCM9";
            this.chkPhysicalCM9.Size = new System.Drawing.Size(24, 24);
            this.chkPhysicalCM9.TabIndex = 8;
            this.chkPhysicalCM9.Tag = "9";
            this.chkPhysicalCM9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkPhysicalCM9.UseVisualStyleBackColor = true;
            this.chkPhysicalCM9.CheckedChanged += new System.EventHandler(this.chkPhysicalCM_CheckedChanged);
            // 
            // chkPhysicalCM8
            // 
            this.chkPhysicalCM8.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPhysicalCM8.Location = new System.Drawing.Point(27, 64);
            this.chkPhysicalCM8.Name = "chkPhysicalCM8";
            this.chkPhysicalCM8.Size = new System.Drawing.Size(24, 24);
            this.chkPhysicalCM8.TabIndex = 7;
            this.chkPhysicalCM8.Tag = "8";
            this.chkPhysicalCM8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkPhysicalCM8.UseVisualStyleBackColor = true;
            this.chkPhysicalCM8.CheckedChanged += new System.EventHandler(this.chkPhysicalCM_CheckedChanged);
            // 
            // chkPhysicalCM7
            // 
            this.chkPhysicalCM7.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPhysicalCM7.Location = new System.Drawing.Point(3, 64);
            this.chkPhysicalCM7.Name = "chkPhysicalCM7";
            this.chkPhysicalCM7.Size = new System.Drawing.Size(24, 24);
            this.chkPhysicalCM7.TabIndex = 6;
            this.chkPhysicalCM7.Tag = "7";
            this.chkPhysicalCM7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkPhysicalCM7.UseVisualStyleBackColor = true;
            this.chkPhysicalCM7.CheckedChanged += new System.EventHandler(this.chkPhysicalCM_CheckedChanged);
            // 
            // chkPhysicalCM6
            // 
            this.chkPhysicalCM6.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPhysicalCM6.Location = new System.Drawing.Point(51, 40);
            this.chkPhysicalCM6.Name = "chkPhysicalCM6";
            this.chkPhysicalCM6.Size = new System.Drawing.Size(24, 24);
            this.chkPhysicalCM6.TabIndex = 5;
            this.chkPhysicalCM6.Tag = "6";
            this.chkPhysicalCM6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkPhysicalCM6.UseVisualStyleBackColor = true;
            this.chkPhysicalCM6.CheckedChanged += new System.EventHandler(this.chkPhysicalCM_CheckedChanged);
            // 
            // chkPhysicalCM5
            // 
            this.chkPhysicalCM5.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPhysicalCM5.Location = new System.Drawing.Point(27, 40);
            this.chkPhysicalCM5.Name = "chkPhysicalCM5";
            this.chkPhysicalCM5.Size = new System.Drawing.Size(24, 24);
            this.chkPhysicalCM5.TabIndex = 4;
            this.chkPhysicalCM5.Tag = "5";
            this.chkPhysicalCM5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkPhysicalCM5.UseVisualStyleBackColor = true;
            this.chkPhysicalCM5.CheckedChanged += new System.EventHandler(this.chkPhysicalCM_CheckedChanged);
            // 
            // chkPhysicalCM4
            // 
            this.chkPhysicalCM4.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPhysicalCM4.Location = new System.Drawing.Point(3, 40);
            this.chkPhysicalCM4.Name = "chkPhysicalCM4";
            this.chkPhysicalCM4.Size = new System.Drawing.Size(24, 24);
            this.chkPhysicalCM4.TabIndex = 3;
            this.chkPhysicalCM4.Tag = "4";
            this.chkPhysicalCM4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkPhysicalCM4.UseVisualStyleBackColor = true;
            this.chkPhysicalCM4.CheckedChanged += new System.EventHandler(this.chkPhysicalCM_CheckedChanged);
            // 
            // chkPhysicalCM3
            // 
            this.chkPhysicalCM3.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPhysicalCM3.Location = new System.Drawing.Point(51, 16);
            this.chkPhysicalCM3.Name = "chkPhysicalCM3";
            this.chkPhysicalCM3.Size = new System.Drawing.Size(24, 24);
            this.chkPhysicalCM3.TabIndex = 2;
            this.chkPhysicalCM3.Tag = "3";
            this.chkPhysicalCM3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkPhysicalCM3.UseVisualStyleBackColor = true;
            this.chkPhysicalCM3.CheckedChanged += new System.EventHandler(this.chkPhysicalCM_CheckedChanged);
            // 
            // chkPhysicalCM2
            // 
            this.chkPhysicalCM2.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPhysicalCM2.Location = new System.Drawing.Point(27, 16);
            this.chkPhysicalCM2.Name = "chkPhysicalCM2";
            this.chkPhysicalCM2.Size = new System.Drawing.Size(24, 24);
            this.chkPhysicalCM2.TabIndex = 1;
            this.chkPhysicalCM2.Tag = "2";
            this.chkPhysicalCM2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkPhysicalCM2.UseVisualStyleBackColor = true;
            this.chkPhysicalCM2.CheckedChanged += new System.EventHandler(this.chkPhysicalCM_CheckedChanged);
            // 
            // chkPhysicalCM1
            // 
            this.chkPhysicalCM1.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPhysicalCM1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.chkPhysicalCM1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkPhysicalCM1.Location = new System.Drawing.Point(3, 16);
            this.chkPhysicalCM1.Name = "chkPhysicalCM1";
            this.chkPhysicalCM1.Size = new System.Drawing.Size(24, 24);
            this.chkPhysicalCM1.TabIndex = 0;
            this.chkPhysicalCM1.Tag = "1";
            this.chkPhysicalCM1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkPhysicalCM1.UseVisualStyleBackColor = false;
            this.chkPhysicalCM1.CheckedChanged += new System.EventHandler(this.chkPhysicalCM_CheckedChanged);
            // 
            // tabInfo
            // 
            this.tabInfo.Controls.Add(this.tabOtherInfo);
            this.tabInfo.Controls.Add(this.tabConditionMonitor);
            this.tabInfo.Controls.Add(this.tabDefences);
            this.tabInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabInfo.Location = new System.Drawing.Point(0, 0);
            this.tabInfo.Name = "tabInfo";
            this.tabInfo.SelectedIndex = 0;
            this.tabInfo.Size = new System.Drawing.Size(193, 612);
            this.tabInfo.TabIndex = 50;
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
            this.tabOtherInfo.Controls.Add(this.lblAstralINI);
            this.tabOtherInfo.Controls.Add(this.lblMatrixINI);
            this.tabOtherInfo.Controls.Add(this.lblINI);
            this.tabOtherInfo.Controls.Add(this.lblAstralINILabel);
            this.tabOtherInfo.Controls.Add(this.lblMatrixINILabel);
            this.tabOtherInfo.Controls.Add(this.lblINILabel);
            this.tabOtherInfo.Controls.Add(this.lblArmorLabel);
            this.tabOtherInfo.Controls.Add(this.lblCareerNuyen);
            this.tabOtherInfo.Controls.Add(this.lblCareerNuyenLabel);
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
            this.tabOtherInfo.Controls.Add(this.lblCareerKarma);
            this.tabOtherInfo.Controls.Add(this.lblCareerKarmaLabel);
            this.tabOtherInfo.Controls.Add(this.lblRemainingNuyen);
            this.tabOtherInfo.Controls.Add(this.lblRemainingNuyenLabel);
            this.tabOtherInfo.Controls.Add(this.lblESSMax);
            this.tabOtherInfo.Controls.Add(this.lblESS);
            this.tabOtherInfo.Controls.Add(this.lblArmor);
            this.tabOtherInfo.Controls.Add(this.lblCMStun);
            this.tabOtherInfo.Controls.Add(this.lblCMPhysical);
            this.tabOtherInfo.Controls.Add(this.lblCMStunLabel);
            this.tabOtherInfo.Controls.Add(this.lblCMPhysicalLabel);
            this.tabOtherInfo.Location = new System.Drawing.Point(4, 22);
            this.tabOtherInfo.Name = "tabOtherInfo";
            this.tabOtherInfo.Padding = new System.Windows.Forms.Padding(3);
            this.tabOtherInfo.Size = new System.Drawing.Size(185, 586);
            this.tabOtherInfo.TabIndex = 1;
            this.tabOtherInfo.Tag = "Tab_OtherInfo";
            this.tabOtherInfo.Text = "Other Info";
            // 
            // lblRiggingINI
            // 
            this.lblRiggingINI.AutoSize = true;
            this.lblRiggingINI.Location = new System.Drawing.Point(139, 166);
            this.lblRiggingINI.Name = "lblRiggingINI";
            this.lblRiggingINI.Size = new System.Drawing.Size(13, 13);
            this.lblRiggingINI.TabIndex = 88;
            this.lblRiggingINI.Text = "0";
            // 
            // lblMatrixINIHot
            // 
            this.lblMatrixINIHot.AutoSize = true;
            this.lblMatrixINIHot.Location = new System.Drawing.Point(139, 143);
            this.lblMatrixINIHot.Name = "lblMatrixINIHot";
            this.lblMatrixINIHot.Size = new System.Drawing.Size(13, 13);
            this.lblMatrixINIHot.TabIndex = 86;
            this.lblMatrixINIHot.Text = "0";
            // 
            // lblMatrixINICold
            // 
            this.lblMatrixINICold.AutoSize = true;
            this.lblMatrixINICold.Location = new System.Drawing.Point(139, 120);
            this.lblMatrixINICold.Name = "lblMatrixINICold";
            this.lblMatrixINICold.Size = new System.Drawing.Size(13, 13);
            this.lblMatrixINICold.TabIndex = 84;
            this.lblMatrixINICold.Text = "0";
            // 
            // lblAstralINI
            // 
            this.lblAstralINI.AutoSize = true;
            this.lblAstralINI.Location = new System.Drawing.Point(139, 74);
            this.lblAstralINI.Name = "lblAstralINI";
            this.lblAstralINI.Size = new System.Drawing.Size(13, 13);
            this.lblAstralINI.TabIndex = 82;
            this.lblAstralINI.Text = "0";
            // 
            // lblMatrixINI
            // 
            this.lblMatrixINI.AutoSize = true;
            this.lblMatrixINI.Location = new System.Drawing.Point(139, 97);
            this.lblMatrixINI.Name = "lblMatrixINI";
            this.lblMatrixINI.Size = new System.Drawing.Size(13, 13);
            this.lblMatrixINI.TabIndex = 81;
            this.lblMatrixINI.Text = "0";
            // 
            // lblINI
            // 
            this.lblINI.AutoSize = true;
            this.lblINI.Location = new System.Drawing.Point(139, 52);
            this.lblINI.Name = "lblINI";
            this.lblINI.Size = new System.Drawing.Size(13, 13);
            this.lblINI.TabIndex = 80;
            this.lblINI.Text = "0";
            // 
            // lblCareerNuyen
            // 
            this.lblCareerNuyen.AutoSize = true;
            this.lblCareerNuyen.Location = new System.Drawing.Point(139, 281);
            this.lblCareerNuyen.Name = "lblCareerNuyen";
            this.lblCareerNuyen.Size = new System.Drawing.Size(13, 13);
            this.lblCareerNuyen.TabIndex = 65;
            this.lblCareerNuyen.Text = "0";
            // 
            // lblFly
            // 
            this.lblFly.AutoSize = true;
            this.lblFly.Location = new System.Drawing.Point(139, 442);
            this.lblFly.Name = "lblFly";
            this.lblFly.Size = new System.Drawing.Size(13, 13);
            this.lblFly.TabIndex = 63;
            this.lblFly.Text = "0";
            // 
            // lblFlyLabel
            // 
            this.lblFlyLabel.AutoSize = true;
            this.lblFlyLabel.Location = new System.Drawing.Point(6, 442);
            this.lblFlyLabel.Name = "lblFlyLabel";
            this.lblFlyLabel.Size = new System.Drawing.Size(23, 13);
            this.lblFlyLabel.TabIndex = 62;
            this.lblFlyLabel.Tag = "Label_OtherFly";
            this.lblFlyLabel.Text = "Fly:";
            // 
            // lblSwim
            // 
            this.lblSwim.AutoSize = true;
            this.lblSwim.Location = new System.Drawing.Point(139, 419);
            this.lblSwim.Name = "lblSwim";
            this.lblSwim.Size = new System.Drawing.Size(13, 13);
            this.lblSwim.TabIndex = 61;
            this.lblSwim.Text = "0";
            // 
            // lblSwimLabel
            // 
            this.lblSwimLabel.AutoSize = true;
            this.lblSwimLabel.Location = new System.Drawing.Point(6, 419);
            this.lblSwimLabel.Name = "lblSwimLabel";
            this.lblSwimLabel.Size = new System.Drawing.Size(35, 13);
            this.lblSwimLabel.TabIndex = 60;
            this.lblSwimLabel.Tag = "Label_OtherSwim";
            this.lblSwimLabel.Text = "Swim:";
            // 
            // lblMemory
            // 
            this.lblMemory.AutoSize = true;
            this.lblMemory.Location = new System.Drawing.Point(139, 373);
            this.lblMemory.Name = "lblMemory";
            this.lblMemory.Size = new System.Drawing.Size(13, 13);
            this.lblMemory.TabIndex = 59;
            this.lblMemory.Text = "0";
            // 
            // lblLiftCarry
            // 
            this.lblLiftCarry.AutoSize = true;
            this.lblLiftCarry.Location = new System.Drawing.Point(139, 350);
            this.lblLiftCarry.Name = "lblLiftCarry";
            this.lblLiftCarry.Size = new System.Drawing.Size(13, 13);
            this.lblLiftCarry.TabIndex = 57;
            this.lblLiftCarry.Text = "0";
            // 
            // lblJudgeIntentions
            // 
            this.lblJudgeIntentions.AutoSize = true;
            this.lblJudgeIntentions.Location = new System.Drawing.Point(139, 327);
            this.lblJudgeIntentions.Name = "lblJudgeIntentions";
            this.lblJudgeIntentions.Size = new System.Drawing.Size(13, 13);
            this.lblJudgeIntentions.TabIndex = 55;
            this.lblJudgeIntentions.Text = "0";
            // 
            // lblComposure
            // 
            this.lblComposure.AutoSize = true;
            this.lblComposure.Location = new System.Drawing.Point(139, 304);
            this.lblComposure.Name = "lblComposure";
            this.lblComposure.Size = new System.Drawing.Size(13, 13);
            this.lblComposure.TabIndex = 53;
            this.lblComposure.Text = "0";
            // 
            // lblMovement
            // 
            this.lblMovement.AutoSize = true;
            this.lblMovement.Location = new System.Drawing.Point(139, 396);
            this.lblMovement.Name = "lblMovement";
            this.lblMovement.Size = new System.Drawing.Size(13, 13);
            this.lblMovement.TabIndex = 45;
            this.lblMovement.Text = "0";
            // 
            // lblCareerKarma
            // 
            this.lblCareerKarma.AutoSize = true;
            this.lblCareerKarma.Location = new System.Drawing.Point(139, 258);
            this.lblCareerKarma.Name = "lblCareerKarma";
            this.lblCareerKarma.Size = new System.Drawing.Size(13, 13);
            this.lblCareerKarma.TabIndex = 43;
            this.lblCareerKarma.Text = "0";
            // 
            // lblRemainingNuyen
            // 
            this.lblRemainingNuyen.AutoSize = true;
            this.lblRemainingNuyen.Location = new System.Drawing.Point(139, 235);
            this.lblRemainingNuyen.Name = "lblRemainingNuyen";
            this.lblRemainingNuyen.Size = new System.Drawing.Size(13, 13);
            this.lblRemainingNuyen.TabIndex = 37;
            this.lblRemainingNuyen.Text = "0";
            // 
            // lblESSMax
            // 
            this.lblESSMax.AutoSize = true;
            this.lblESSMax.Location = new System.Drawing.Point(139, 212);
            this.lblESSMax.Name = "lblESSMax";
            this.lblESSMax.Size = new System.Drawing.Size(13, 13);
            this.lblESSMax.TabIndex = 35;
            this.lblESSMax.Text = "0";
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
            // tabConditionMonitor
            // 
            this.tabConditionMonitor.BackColor = System.Drawing.SystemColors.Control;
            this.tabConditionMonitor.Controls.Add(this.lblEDGInfo);
            this.tabConditionMonitor.Controls.Add(this.lblCMDamageResistancePool);
            this.tabConditionMonitor.Controls.Add(this.lblCMDamageResistancePoolLabel);
            this.tabConditionMonitor.Controls.Add(this.lblCMArmor);
            this.tabConditionMonitor.Controls.Add(this.lblCMArmorLabel);
            this.tabConditionMonitor.Controls.Add(this.label1);
            this.tabConditionMonitor.Controls.Add(this.panStunCM);
            this.tabConditionMonitor.Controls.Add(this.lblCMPenalty);
            this.tabConditionMonitor.Controls.Add(this.panPhysicalCM);
            this.tabConditionMonitor.Controls.Add(this.lblCMPenaltyLabel);
            this.tabConditionMonitor.Controls.Add(this.cmdEdgeGained);
            this.tabConditionMonitor.Controls.Add(this.cmdEdgeSpent);
            this.tabConditionMonitor.Location = new System.Drawing.Point(4, 22);
            this.tabConditionMonitor.Name = "tabConditionMonitor";
            this.tabConditionMonitor.Padding = new System.Windows.Forms.Padding(3);
            this.tabConditionMonitor.Size = new System.Drawing.Size(185, 586);
            this.tabConditionMonitor.TabIndex = 2;
            this.tabConditionMonitor.Tag = "Tab_ConditionMonitor";
            this.tabConditionMonitor.Text = "Condition Monitor";
            // 
            // lblEDGInfo
            // 
            this.lblEDGInfo.AutoSize = true;
            this.lblEDGInfo.Location = new System.Drawing.Point(6, 516);
            this.lblEDGInfo.Name = "lblEDGInfo";
            this.lblEDGInfo.Size = new System.Drawing.Size(105, 13);
            this.lblEDGInfo.TabIndex = 72;
            this.lblEDGInfo.Tag = "Label_CMEdge";
            this.lblEDGInfo.Text = "Regain/Spend Edge";
            // 
            // lblCMDamageResistancePool
            // 
            this.lblCMDamageResistancePool.AutoSize = true;
            this.lblCMDamageResistancePool.Location = new System.Drawing.Point(121, 73);
            this.lblCMDamageResistancePool.Name = "lblCMDamageResistancePool";
            this.lblCMDamageResistancePool.Size = new System.Drawing.Size(19, 13);
            this.lblCMDamageResistancePool.TabIndex = 71;
            this.lblCMDamageResistancePool.Text = "[0]";
            // 
            // lblCMArmor
            // 
            this.lblCMArmor.AutoSize = true;
            this.lblCMArmor.Location = new System.Drawing.Point(121, 31);
            this.lblCMArmor.Name = "lblCMArmor";
            this.lblCMArmor.Size = new System.Drawing.Size(19, 13);
            this.lblCMArmor.TabIndex = 67;
            this.lblCMArmor.Text = "[0]";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 497);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(105, 13);
            this.label1.TabIndex = 65;
            this.label1.Tag = "Label_CMEdge";
            this.label1.Text = "Regain/Spend Edge";
            // 
            // lblCMPenalty
            // 
            this.lblCMPenalty.AutoSize = true;
            this.lblCMPenalty.Location = new System.Drawing.Point(121, 10);
            this.lblCMPenalty.Name = "lblCMPenalty";
            this.lblCMPenalty.Size = new System.Drawing.Size(19, 13);
            this.lblCMPenalty.TabIndex = 38;
            this.lblCMPenalty.Text = "[0]";
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
            this.tabDefences.Size = new System.Drawing.Size(185, 586);
            this.tabDefences.TabIndex = 4;
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
            this.mnuFilePrint,
            this.mnuFileExport});
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
            this.toolStripSeparator2.MergeAction = System.Windows.Forms.MergeAction.Insert;
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
            // mnuFileExport
            // 
            this.mnuFileExport.Image = global::Chummer.Properties.Resources.export;
            this.mnuFileExport.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.mnuFileExport.MergeIndex = 9;
            this.mnuFileExport.Name = "mnuFileExport";
            this.mnuFileExport.Size = new System.Drawing.Size(148, 22);
            this.mnuFileExport.Tag = "Menu_FileExport";
            this.mnuFileExport.Text = "Export";
            this.mnuFileExport.Click += new System.EventHandler(this.mnuFileExport_Click);
            // 
            // mnuCreateEdit
            // 
            this.mnuCreateEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuEditCopy});
            this.mnuCreateEdit.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.mnuCreateEdit.MergeIndex = 1;
            this.mnuCreateEdit.Name = "mnuCreateEdit";
            this.mnuCreateEdit.Size = new System.Drawing.Size(39, 20);
            this.mnuCreateEdit.Tag = "Menu_Main_Edit";
            this.mnuCreateEdit.Text = "&Edit";
            this.mnuCreateEdit.Visible = false;
            this.mnuCreateEdit.DropDownOpening += new System.EventHandler(this.Menu_DropDownOpening);
            // 
            // mnuEditCopy
            // 
            this.mnuEditCopy.Image = global::Chummer.Properties.Resources.page_copy;
            this.mnuEditCopy.Name = "mnuEditCopy";
            this.mnuEditCopy.Size = new System.Drawing.Size(102, 22);
            this.mnuEditCopy.Tag = "Menu_EditCopy";
            this.mnuEditCopy.Text = "&Copy";
            this.mnuEditCopy.Click += new System.EventHandler(this.mnuEditCopy_Click);
            // 
            // mnuCreateSpecial
            // 
            this.mnuCreateSpecial.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuSpecialAddCyberwareSuite,
            this.mnuSpecialAddBiowareSuite,
            this.mnuSpecialCyberzombie,
            this.mnuSpecialConvertToFreeSprite,
            this.mnuSpecialReduceAttribute,
            this.mnuSpecialPossess,
            this.mnuSpecialPossessInanimate,
            this.mnuSpecialReapplyImprovements,
            this.mnuSpecialCloningMachine});
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
            this.mnuSpecialAddCyberwareSuite.Size = new System.Drawing.Size(246, 22);
            this.mnuSpecialAddCyberwareSuite.Tag = "Menu_SpecialAddCyberwareSuite";
            this.mnuSpecialAddCyberwareSuite.Text = "Add &Cyberware Suite";
            this.mnuSpecialAddCyberwareSuite.Click += new System.EventHandler(this.mnuSpecialAddCyberwareSuite_Click);
            // 
            // mnuSpecialAddBiowareSuite
            // 
            this.mnuSpecialAddBiowareSuite.Image = global::Chummer.Properties.Resources.briefcase_add;
            this.mnuSpecialAddBiowareSuite.Name = "mnuSpecialAddBiowareSuite";
            this.mnuSpecialAddBiowareSuite.Size = new System.Drawing.Size(246, 22);
            this.mnuSpecialAddBiowareSuite.Tag = "Menu_SpecialAddBiowareSuite";
            this.mnuSpecialAddBiowareSuite.Text = "Add &Bioware Suite";
            this.mnuSpecialAddBiowareSuite.Click += new System.EventHandler(this.mnuSpecialAddBiowareSuite_Click);
            // 
            // mnuSpecialCyberzombie
            // 
            this.mnuSpecialCyberzombie.Image = global::Chummer.Properties.Resources.emoticon_evilgrin;
            this.mnuSpecialCyberzombie.Name = "mnuSpecialCyberzombie";
            this.mnuSpecialCyberzombie.Size = new System.Drawing.Size(246, 22);
            this.mnuSpecialCyberzombie.Tag = "Menu_SpecialConverToCyberzombie";
            this.mnuSpecialCyberzombie.Text = "Convert to Cyberzombie";
            this.mnuSpecialCyberzombie.Click += new System.EventHandler(this.mnuSpecialCyberzombie_Click);
            // 
            // mnuSpecialConvertToFreeSprite
            // 
            this.mnuSpecialConvertToFreeSprite.Image = global::Chummer.Properties.Resources.emoticon_waii;
            this.mnuSpecialConvertToFreeSprite.Name = "mnuSpecialConvertToFreeSprite";
            this.mnuSpecialConvertToFreeSprite.Size = new System.Drawing.Size(246, 22);
            this.mnuSpecialConvertToFreeSprite.Tag = "Menu_SpecialConvertToFreeSprite";
            this.mnuSpecialConvertToFreeSprite.Text = "Convert to Free Sprite";
            this.mnuSpecialConvertToFreeSprite.Visible = false;
            this.mnuSpecialConvertToFreeSprite.Click += new System.EventHandler(this.mnuSpecialConvertToFreeSprite_Click);
            // 
            // mnuSpecialReduceAttribute
            // 
            this.mnuSpecialReduceAttribute.Image = global::Chummer.Properties.Resources.emoticon_unhappy;
            this.mnuSpecialReduceAttribute.Name = "mnuSpecialReduceAttribute";
            this.mnuSpecialReduceAttribute.Size = new System.Drawing.Size(246, 22);
            this.mnuSpecialReduceAttribute.Tag = "Menu_SpecialReduceAttribute";
            this.mnuSpecialReduceAttribute.Text = "Reduce Attribute";
            this.mnuSpecialReduceAttribute.Click += new System.EventHandler(this.mnuSpecialReduceAttribute_Click);
            // 
            // mnuSpecialPossess
            // 
            this.mnuSpecialPossess.Image = global::Chummer.Properties.Resources.possession;
            this.mnuSpecialPossess.Name = "mnuSpecialPossess";
            this.mnuSpecialPossess.Size = new System.Drawing.Size(246, 22);
            this.mnuSpecialPossess.Tag = "Menu_SpecialPossessLiving";
            this.mnuSpecialPossess.Text = "Possess/Inhabit Living Vessel";
            this.mnuSpecialPossess.Click += new System.EventHandler(this.mnuSpecialPossess_Click);
            // 
            // mnuSpecialPossessInanimate
            // 
            this.mnuSpecialPossessInanimate.Image = global::Chummer.Properties.Resources.possessinanimate;
            this.mnuSpecialPossessInanimate.Name = "mnuSpecialPossessInanimate";
            this.mnuSpecialPossessInanimate.Size = new System.Drawing.Size(246, 22);
            this.mnuSpecialPossessInanimate.Tag = "Menu_SpecialPossessInanimate";
            this.mnuSpecialPossessInanimate.Text = "Possess/Inhabit Inanimate Vessel";
            this.mnuSpecialPossessInanimate.Click += new System.EventHandler(this.mnuSpecialPossessInanimate_Click);
            // 
            // mnuSpecialReapplyImprovements
            // 
            this.mnuSpecialReapplyImprovements.Image = global::Chummer.Properties.Resources.arrow_redo;
            this.mnuSpecialReapplyImprovements.Name = "mnuSpecialReapplyImprovements";
            this.mnuSpecialReapplyImprovements.Size = new System.Drawing.Size(246, 22);
            this.mnuSpecialReapplyImprovements.Tag = "Menu_SpecialReapplyImprovements";
            this.mnuSpecialReapplyImprovements.Text = "Re-apply Improvements";
            this.mnuSpecialReapplyImprovements.Click += new System.EventHandler(this.mnuSpecialReapplyImprovements_Click);
            // 
            // mnuSpecialCloningMachine
            // 
            this.mnuSpecialCloningMachine.Image = global::Chummer.Properties.Resources.user_add;
            this.mnuSpecialCloningMachine.Name = "mnuSpecialCloningMachine";
            this.mnuSpecialCloningMachine.Size = new System.Drawing.Size(246, 22);
            this.mnuSpecialCloningMachine.Tag = "Menu_SpecialCloningMachine";
            this.mnuSpecialCloningMachine.Text = "Cloning Machine";
            this.mnuSpecialCloningMachine.Click += new System.EventHandler(this.mnuSpecialCloningMachine_Click);
            // 
            // toolStrip
            // 
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbSave,
            this.tsbPrint,
            this.tsbSeparator,
            this.tsbCopy});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(1066, 25);
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
            this.tsbCopy.Visible = false;
            this.tsbCopy.Click += new System.EventHandler(this.tsbCopy_Click);
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
            // cmsUndoKarmaExpense
            // 
            this.cmsUndoKarmaExpense.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsUndoKarmaExpense,
            this.tsEditKarmaExpense});
            this.cmsUndoKarmaExpense.Name = "contextMenuStrip1";
            this.cmsUndoKarmaExpense.Size = new System.Drawing.Size(149, 48);
            this.cmsUndoKarmaExpense.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
            // 
            // tsUndoKarmaExpense
            // 
            this.tsUndoKarmaExpense.Image = global::Chummer.Properties.Resources.arrow_undo;
            this.tsUndoKarmaExpense.Name = "tsUndoKarmaExpense";
            this.tsUndoKarmaExpense.Size = new System.Drawing.Size(148, 22);
            this.tsUndoKarmaExpense.Tag = "Menu_UndoExpense";
            this.tsUndoKarmaExpense.Text = "Undo Expense";
            this.tsUndoKarmaExpense.Click += new System.EventHandler(this.tsUndoKarmaExpense_Click);
            // 
            // tsEditKarmaExpense
            // 
            this.tsEditKarmaExpense.Image = global::Chummer.Properties.Resources.pencil;
            this.tsEditKarmaExpense.Name = "tsEditKarmaExpense";
            this.tsEditKarmaExpense.Size = new System.Drawing.Size(148, 22);
            this.tsEditKarmaExpense.Tag = "Button_EditExpense";
            this.tsEditKarmaExpense.Text = "Edit Expense";
            this.tsEditKarmaExpense.Click += new System.EventHandler(this.tsEditKarmaExpense_Click);
            // 
            // cmsUndoNuyenExpense
            // 
            this.cmsUndoNuyenExpense.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsUndoNuyenExpense,
            this.tsEditNuyenExpense});
            this.cmsUndoNuyenExpense.Name = "cmsUndoNuyenExpense";
            this.cmsUndoNuyenExpense.Size = new System.Drawing.Size(149, 48);
            this.cmsUndoNuyenExpense.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
            // 
            // tsUndoNuyenExpense
            // 
            this.tsUndoNuyenExpense.Image = global::Chummer.Properties.Resources.arrow_undo;
            this.tsUndoNuyenExpense.Name = "tsUndoNuyenExpense";
            this.tsUndoNuyenExpense.Size = new System.Drawing.Size(148, 22);
            this.tsUndoNuyenExpense.Tag = "Menu_UndoExpense";
            this.tsUndoNuyenExpense.Text = "Undo Expense";
            this.tsUndoNuyenExpense.Click += new System.EventHandler(this.tsUndoNuyenExpense_Click);
            // 
            // tsEditNuyenExpense
            // 
            this.tsEditNuyenExpense.Image = global::Chummer.Properties.Resources.pencil;
            this.tsEditNuyenExpense.Name = "tsEditNuyenExpense";
            this.tsEditNuyenExpense.Size = new System.Drawing.Size(148, 22);
            this.tsEditNuyenExpense.Tag = "Button_EditExpense";
            this.tsEditNuyenExpense.Text = "Edit Expense";
            this.tsEditNuyenExpense.Click += new System.EventHandler(this.tsEditNuyenExpense_Click);
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
            // cmsMartialArtManeuver
            // 
            this.cmsMartialArtManeuver.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsMartialArtManeuverNotes});
            this.cmsMartialArtManeuver.Name = "cmsMartialArtManeuver";
            this.cmsMartialArtManeuver.Size = new System.Drawing.Size(106, 26);
            this.cmsMartialArtManeuver.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
            // 
            // tsMartialArtManeuverNotes
            // 
            this.tsMartialArtManeuverNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsMartialArtManeuverNotes.Name = "tsMartialArtManeuverNotes";
            this.tsMartialArtManeuverNotes.Size = new System.Drawing.Size(105, 22);
            this.tsMartialArtManeuverNotes.Tag = "Menu_Notes";
            this.tsMartialArtManeuverNotes.Text = "&Notes";
            this.tsMartialArtManeuverNotes.Click += new System.EventHandler(this.tsMartialArtManeuverNotes_Click);
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
            this.cmsMetamagic.Opening += new System.ComponentModel.CancelEventHandler(this.InitiationContextMenu_Opening);
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
            // cmsWeaponMod
            // 
            this.cmsWeaponMod.Name = "cmsWeaponMod";
            this.cmsWeaponMod.Size = new System.Drawing.Size(61, 4);
            // 
            // tsWeaponModNotes
            // 
            this.tsWeaponModNotes.Name = "tsWeaponModNotes";
            this.tsWeaponModNotes.Size = new System.Drawing.Size(32, 19);
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
            this.splitMain.Size = new System.Drawing.Size(1066, 612);
            this.splitMain.SplitterDistance = 869;
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
            this.tabCharacterTabs.Controls.Add(this.tabCritter);
            this.tabCharacterTabs.Controls.Add(this.tabAdvancedPrograms);
            this.tabCharacterTabs.Controls.Add(this.tabInitiation);
            this.tabCharacterTabs.Controls.Add(this.tabCyberware);
            this.tabCharacterTabs.Controls.Add(this.tabStreetGear);
            this.tabCharacterTabs.Controls.Add(this.tabVehicles);
            this.tabCharacterTabs.Controls.Add(this.tabCharacterInfo);
            this.tabCharacterTabs.Controls.Add(this.tabKarma);
            this.tabCharacterTabs.Controls.Add(this.tabCalendar);
            this.tabCharacterTabs.Controls.Add(this.tabNotes);
            this.tabCharacterTabs.Controls.Add(this.tabImprovements);
            this.tabCharacterTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabCharacterTabs.Location = new System.Drawing.Point(0, 0);
            this.tabCharacterTabs.Name = "tabCharacterTabs";
            this.tabCharacterTabs.SelectedIndex = 0;
            this.tabCharacterTabs.Size = new System.Drawing.Size(869, 612);
            this.tabCharacterTabs.TabIndex = 33;
            this.tabCharacterTabs.Tag = "";
            this.tabCharacterTabs.SelectedIndexChanged += new System.EventHandler(this.tabCharacterTabs_SelectedIndexChanged);
            // 
            // tabCommon
            // 
            this.tabCommon.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabCommon.Controls.Add(this.cmdIncreasePowerPoints);
            this.tabCommon.Controls.Add(this.lblAttributesAug);
            this.tabCommon.Controls.Add(this.lblAttributesMetatype);
            this.tabCommon.Controls.Add(this.lblAttributes);
            this.tabCommon.Controls.Add(this.pnlAttributes);
            this.tabCommon.Controls.Add(this.tabPeople);
            this.tabCommon.Controls.Add(this.lblPossessed);
            this.tabCommon.Controls.Add(this.txtAlias);
            this.tabCommon.Controls.Add(this.lblAlias);
            this.tabCommon.Controls.Add(this.lblMetatypeSource);
            this.tabCommon.Controls.Add(this.lblMetatypeSourceLabel);
            this.tabCommon.Controls.Add(this.cmdSwapQuality);
            this.tabCommon.Controls.Add(this.lblQualityBP);
            this.tabCommon.Controls.Add(this.lblQualityBPLabel);
            this.tabCommon.Controls.Add(this.lblQualitySource);
            this.tabCommon.Controls.Add(this.lblQualitySourceLabel);
            this.tabCommon.Controls.Add(this.cmdDeleteQuality);
            this.tabCommon.Controls.Add(this.cmdAddQuality);
            this.tabCommon.Controls.Add(this.treQualities);
            this.tabCommon.Controls.Add(this.lblMysticAdeptAssignment);
            this.tabCommon.Controls.Add(this.lblMysticAdeptMAGAdept);
            this.tabCommon.Controls.Add(this.lblMetatype);
            this.tabCommon.Controls.Add(this.lblMetatypeLabel);
            this.tabCommon.Location = new System.Drawing.Point(4, 22);
            this.tabCommon.Name = "tabCommon";
            this.tabCommon.Padding = new System.Windows.Forms.Padding(3);
            this.tabCommon.Size = new System.Drawing.Size(861, 586);
            this.tabCommon.TabIndex = 0;
            this.tabCommon.Tag = "Tab_Common";
            this.tabCommon.Text = "Common";
            // 
            // pnlAttributes
            // 
            this.pnlAttributes.Location = new System.Drawing.Point(289, 48);
            this.pnlAttributes.Name = "pnlAttributes";
            this.pnlAttributes.Size = new System.Drawing.Size(333, 319);
            this.pnlAttributes.TabIndex = 102;
            // 
            // tabPeople
            // 
            this.tabPeople.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabPeople.Controls.Add(this.tabContacts);
            this.tabPeople.Controls.Add(this.tabEnemies);
            this.tabPeople.Location = new System.Drawing.Point(285, 373);
            this.tabPeople.Name = "tabPeople";
            this.tabPeople.SelectedIndex = 0;
            this.tabPeople.Size = new System.Drawing.Size(570, 207);
            this.tabPeople.TabIndex = 91;
            // 
            // tabContacts
            // 
            this.tabContacts.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabContacts.Controls.Add(this.panContacts);
            this.tabContacts.Controls.Add(this.cmdAddContact);
            this.tabContacts.Controls.Add(this.lblContactArchtypeLabel);
            this.tabContacts.Controls.Add(this.lblContactNameLabel);
            this.tabContacts.Controls.Add(this.lblContactLocationLabel);
            this.tabContacts.Location = new System.Drawing.Point(4, 22);
            this.tabContacts.Name = "tabContacts";
            this.tabContacts.Padding = new System.Windows.Forms.Padding(3);
            this.tabContacts.Size = new System.Drawing.Size(562, 181);
            this.tabContacts.TabIndex = 0;
            this.tabContacts.Text = "Contacts";
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
            this.panContacts.Size = new System.Drawing.Size(553, 129);
            this.panContacts.TabIndex = 25;
            this.panContacts.Click += new System.EventHandler(this.panContacts_Click);
            this.panContacts.DragDrop += new System.Windows.Forms.DragEventHandler(this.panContacts_DragDrop);
            this.panContacts.DragEnter += new System.Windows.Forms.DragEventHandler(this.panContacts_DragEnter);
            this.panContacts.DragOver += new System.Windows.Forms.DragEventHandler(this.panContacts_DragOver);
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
            // tabEnemies
            // 
            this.tabEnemies.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabEnemies.Controls.Add(this.label10);
            this.tabEnemies.Controls.Add(this.label11);
            this.tabEnemies.Controls.Add(this.label12);
            this.tabEnemies.Controls.Add(this.panEnemies);
            this.tabEnemies.Controls.Add(this.cmdAddEnemy);
            this.tabEnemies.Location = new System.Drawing.Point(4, 22);
            this.tabEnemies.Name = "tabEnemies";
            this.tabEnemies.Padding = new System.Windows.Forms.Padding(3);
            this.tabEnemies.Size = new System.Drawing.Size(562, 181);
            this.tabEnemies.TabIndex = 1;
            this.tabEnemies.Text = "Enemies";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(255, 32);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(52, 13);
            this.label10.TabIndex = 49;
            this.label10.Tag = "Label_Archtype";
            this.label10.Text = "Archtype:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(129, 32);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(51, 13);
            this.label11.TabIndex = 48;
            this.label11.Tag = "Label_Location";
            this.label11.Text = "Location:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 32);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(38, 13);
            this.label12.TabIndex = 47;
            this.label12.Tag = "Label_Name";
            this.label12.Text = "Name:";
            // 
            // panEnemies
            // 
            this.panEnemies.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panEnemies.AutoScroll = true;
            this.panEnemies.Location = new System.Drawing.Point(3, 48);
            this.panEnemies.Name = "panEnemies";
            this.panEnemies.Size = new System.Drawing.Size(554, 129);
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
            // lblPossessed
            // 
            this.lblPossessed.AutoSize = true;
            this.lblPossessed.Location = new System.Drawing.Point(628, 55);
            this.lblPossessed.Name = "lblPossessed";
            this.lblPossessed.Size = new System.Drawing.Size(33, 13);
            this.lblPossessed.TabIndex = 90;
            this.lblPossessed.Text = "None";
            // 
            // txtAlias
            // 
            this.txtAlias.Location = new System.Drawing.Point(326, 6);
            this.txtAlias.Name = "txtAlias";
            this.txtAlias.Size = new System.Drawing.Size(270, 20);
            this.txtAlias.TabIndex = 89;
            this.txtAlias.TextChanged += new System.EventHandler(this.txtAlias_TextChanged);
            // 
            // lblAlias
            // 
            this.lblAlias.AutoSize = true;
            this.lblAlias.Location = new System.Drawing.Point(288, 9);
            this.lblAlias.Name = "lblAlias";
            this.lblAlias.Size = new System.Drawing.Size(32, 13);
            this.lblAlias.TabIndex = 88;
            this.lblAlias.Tag = "Label_Alias";
            this.lblAlias.Text = "Alias:";
            // 
            // lblMetatypeSource
            // 
            this.lblMetatypeSource.AutoSize = true;
            this.lblMetatypeSource.Location = new System.Drawing.Point(688, 32);
            this.lblMetatypeSource.Name = "lblMetatypeSource";
            this.lblMetatypeSource.Size = new System.Drawing.Size(33, 13);
            this.lblMetatypeSource.TabIndex = 87;
            this.lblMetatypeSource.Text = "None";
            this.lblMetatypeSource.Click += new System.EventHandler(this.lblMetatypeSource_Click);
            // 
            // lblMetatypeSourceLabel
            // 
            this.lblMetatypeSourceLabel.AutoSize = true;
            this.lblMetatypeSourceLabel.Location = new System.Drawing.Point(628, 32);
            this.lblMetatypeSourceLabel.Name = "lblMetatypeSourceLabel";
            this.lblMetatypeSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblMetatypeSourceLabel.TabIndex = 86;
            this.lblMetatypeSourceLabel.Tag = "Label_Source";
            this.lblMetatypeSourceLabel.Text = "Source:";
            // 
            // cmdSwapQuality
            // 
            this.cmdSwapQuality.AutoSize = true;
            this.cmdSwapQuality.Location = new System.Drawing.Point(101, 9);
            this.cmdSwapQuality.Name = "cmdSwapQuality";
            this.cmdSwapQuality.Size = new System.Drawing.Size(86, 23);
            this.cmdSwapQuality.TabIndex = 71;
            this.cmdSwapQuality.Tag = "Button_SwapQuality";
            this.cmdSwapQuality.Text = "Swap Quality";
            this.cmdSwapQuality.UseVisualStyleBackColor = true;
            this.cmdSwapQuality.Click += new System.EventHandler(this.cmdSwapQuality_Click);
            // 
            // lblQualityBP
            // 
            this.lblQualityBP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblQualityBP.AutoSize = true;
            this.lblQualityBP.Location = new System.Drawing.Point(58, 564);
            this.lblQualityBP.Name = "lblQualityBP";
            this.lblQualityBP.Size = new System.Drawing.Size(43, 13);
            this.lblQualityBP.TabIndex = 70;
            this.lblQualityBP.Text = "[Karma]";
            // 
            // lblQualityBPLabel
            // 
            this.lblQualityBPLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblQualityBPLabel.AutoSize = true;
            this.lblQualityBPLabel.Location = new System.Drawing.Point(8, 564);
            this.lblQualityBPLabel.Name = "lblQualityBPLabel";
            this.lblQualityBPLabel.Size = new System.Drawing.Size(40, 13);
            this.lblQualityBPLabel.TabIndex = 69;
            this.lblQualityBPLabel.Tag = "Label_Karma";
            this.lblQualityBPLabel.Text = "Karma:";
            // 
            // lblQualitySource
            // 
            this.lblQualitySource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblQualitySource.AutoSize = true;
            this.lblQualitySource.Location = new System.Drawing.Point(232, 564);
            this.lblQualitySource.Name = "lblQualitySource";
            this.lblQualitySource.Size = new System.Drawing.Size(47, 13);
            this.lblQualitySource.TabIndex = 68;
            this.lblQualitySource.Text = "[Source]";
            this.lblQualitySource.Click += new System.EventHandler(this.lblQualitySource_Click);
            // 
            // lblQualitySourceLabel
            // 
            this.lblQualitySourceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblQualitySourceLabel.AutoSize = true;
            this.lblQualitySourceLabel.Location = new System.Drawing.Point(182, 564);
            this.lblQualitySourceLabel.Name = "lblQualitySourceLabel";
            this.lblQualitySourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblQualitySourceLabel.TabIndex = 67;
            this.lblQualitySourceLabel.Tag = "Label_Source";
            this.lblQualitySourceLabel.Text = "Source:";
            // 
            // cmdDeleteQuality
            // 
            this.cmdDeleteQuality.AutoSize = true;
            this.cmdDeleteQuality.Location = new System.Drawing.Point(193, 9);
            this.cmdDeleteQuality.Name = "cmdDeleteQuality";
            this.cmdDeleteQuality.Size = new System.Drawing.Size(80, 23);
            this.cmdDeleteQuality.TabIndex = 66;
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
            this.cmdAddQuality.TabIndex = 65;
            this.cmdAddQuality.Tag = "Button_AddQuality";
            this.cmdAddQuality.Text = "Add &Quality";
            this.cmdAddQuality.UseVisualStyleBackColor = true;
            this.cmdAddQuality.Click += new System.EventHandler(this.cmdAddQuality_Click);
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
            this.treQualities.Size = new System.Drawing.Size(273, 520);
            this.treQualities.TabIndex = 64;
            this.treQualities.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treQualities_AfterSelect);
            this.treQualities.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treQualities_KeyDown);
            this.treQualities.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            // 
            // lblMysticAdeptAssignment
            // 
            this.lblMysticAdeptAssignment.AutoSize = true;
            this.lblMysticAdeptAssignment.Location = new System.Drawing.Point(628, 289);
            this.lblMysticAdeptAssignment.Name = "lblMysticAdeptAssignment";
            this.lblMysticAdeptAssignment.Size = new System.Drawing.Size(133, 13);
            this.lblMysticAdeptAssignment.TabIndex = 56;
            this.lblMysticAdeptAssignment.Tag = "Label_MysticAdeptAssignment";
            this.lblMysticAdeptAssignment.Text = "Mystic Adept Power Points";
            this.lblMysticAdeptAssignment.Visible = false;
            // 
            // lblMysticAdeptMAGAdept
            // 
            this.lblMysticAdeptMAGAdept.AutoSize = true;
            this.lblMysticAdeptMAGAdept.Location = new System.Drawing.Point(780, 289);
            this.lblMysticAdeptMAGAdept.Name = "lblMysticAdeptMAGAdept";
            this.lblMysticAdeptMAGAdept.Size = new System.Drawing.Size(19, 13);
            this.lblMysticAdeptMAGAdept.TabIndex = 58;
            this.lblMysticAdeptMAGAdept.Text = "[0]";
            this.lblMysticAdeptMAGAdept.Visible = false;
            // 
            // lblMetatype
            // 
            this.lblMetatype.AutoSize = true;
            this.lblMetatype.Location = new System.Drawing.Point(688, 9);
            this.lblMetatype.Name = "lblMetatype";
            this.lblMetatype.Size = new System.Drawing.Size(33, 13);
            this.lblMetatype.TabIndex = 20;
            this.lblMetatype.Text = "None";
            // 
            // lblMetatypeLabel
            // 
            this.lblMetatypeLabel.AutoSize = true;
            this.lblMetatypeLabel.Location = new System.Drawing.Point(628, 9);
            this.lblMetatypeLabel.Name = "lblMetatypeLabel";
            this.lblMetatypeLabel.Size = new System.Drawing.Size(54, 13);
            this.lblMetatypeLabel.TabIndex = 19;
            this.lblMetatypeLabel.Tag = "Label_Metatype";
            this.lblMetatypeLabel.Text = "Metatype:";
            // 
            // tabSkills
            // 
            this.tabSkills.Controls.Add(this.tabSkillsUc);
            this.tabSkills.Location = new System.Drawing.Point(4, 22);
            this.tabSkills.Name = "tabSkills";
            this.tabSkills.Size = new System.Drawing.Size(861, 586);
            this.tabSkills.TabIndex = 17;
            this.tabSkills.Tag = "Tab_Skills";
            this.tabSkills.Text = "Skills";
            this.tabSkills.UseVisualStyleBackColor = true;
            // 
            // tabSkillsUc
            // 
            this.tabSkillsUc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabSkillsUc.Location = new System.Drawing.Point(0, 0);
            this.tabSkillsUc.Name = "tabSkillsUc";
            this.tabSkillsUc.ObjCharacter = null;
            this.tabSkillsUc.Size = new System.Drawing.Size(861, 586);
            this.tabSkillsUc.TabIndex = 0;
            // 
            // tabLimits
            // 
            this.tabLimits.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabLimits.Controls.Add(this.lblAstralLabel);
            this.tabLimits.Controls.Add(this.lblAstral);
            this.tabLimits.Controls.Add(this.lblSocialLimitLabel);
            this.tabLimits.Controls.Add(this.lblSocial);
            this.tabLimits.Controls.Add(this.lblMentalLimitLabel);
            this.tabLimits.Controls.Add(this.lblMental);
            this.tabLimits.Controls.Add(this.lblPhysicalLimitLabel);
            this.tabLimits.Controls.Add(this.lblPhysical);
            this.tabLimits.Controls.Add(this.cmdAddLimitModifier);
            this.tabLimits.Controls.Add(this.treLimit);
            this.tabLimits.Controls.Add(this.cmdDeleteLimitModifier);
            this.tabLimits.Location = new System.Drawing.Point(4, 22);
            this.tabLimits.Name = "tabLimits";
            this.tabLimits.Size = new System.Drawing.Size(861, 586);
            this.tabLimits.TabIndex = 16;
            this.tabLimits.Text = "Limits";
            // 
            // lblAstralLabel
            // 
            this.lblAstralLabel.AutoSize = true;
            this.lblAstralLabel.Location = new System.Drawing.Point(323, 112);
            this.lblAstralLabel.Name = "lblAstralLabel";
            this.lblAstralLabel.Size = new System.Drawing.Size(33, 13);
            this.lblAstralLabel.TabIndex = 87;
            this.lblAstralLabel.Tag = "Node_Astral";
            this.lblAstralLabel.Text = "Astral";
            // 
            // lblAstral
            // 
            this.lblAstral.AutoSize = true;
            this.lblAstral.Location = new System.Drawing.Point(406, 112);
            this.lblAstral.Name = "lblAstral";
            this.lblAstral.Size = new System.Drawing.Size(19, 13);
            this.lblAstral.TabIndex = 88;
            this.lblAstral.Text = "[0]";
            // 
            // lblSocialLimitLabel
            // 
            this.lblSocialLimitLabel.AutoSize = true;
            this.lblSocialLimitLabel.Location = new System.Drawing.Point(323, 86);
            this.lblSocialLimitLabel.Name = "lblSocialLimitLabel";
            this.lblSocialLimitLabel.Size = new System.Drawing.Size(36, 13);
            this.lblSocialLimitLabel.TabIndex = 85;
            this.lblSocialLimitLabel.Tag = "Node_Social";
            this.lblSocialLimitLabel.Text = "Social";
            // 
            // lblSocial
            // 
            this.lblSocial.AutoSize = true;
            this.lblSocial.Location = new System.Drawing.Point(406, 86);
            this.lblSocial.Name = "lblSocial";
            this.lblSocial.Size = new System.Drawing.Size(19, 13);
            this.lblSocial.TabIndex = 86;
            this.lblSocial.Text = "[0]";
            // 
            // lblMentalLimitLabel
            // 
            this.lblMentalLimitLabel.AutoSize = true;
            this.lblMentalLimitLabel.Location = new System.Drawing.Point(323, 60);
            this.lblMentalLimitLabel.Name = "lblMentalLimitLabel";
            this.lblMentalLimitLabel.Size = new System.Drawing.Size(39, 13);
            this.lblMentalLimitLabel.TabIndex = 83;
            this.lblMentalLimitLabel.Tag = "Node_Mental";
            this.lblMentalLimitLabel.Text = "Mental";
            // 
            // lblMental
            // 
            this.lblMental.AutoSize = true;
            this.lblMental.Location = new System.Drawing.Point(406, 60);
            this.lblMental.Name = "lblMental";
            this.lblMental.Size = new System.Drawing.Size(19, 13);
            this.lblMental.TabIndex = 84;
            this.lblMental.Text = "[0]";
            // 
            // lblPhysicalLimitLabel
            // 
            this.lblPhysicalLimitLabel.AutoSize = true;
            this.lblPhysicalLimitLabel.Location = new System.Drawing.Point(323, 34);
            this.lblPhysicalLimitLabel.Name = "lblPhysicalLimitLabel";
            this.lblPhysicalLimitLabel.Size = new System.Drawing.Size(46, 13);
            this.lblPhysicalLimitLabel.TabIndex = 81;
            this.lblPhysicalLimitLabel.Tag = "Node_Physical";
            this.lblPhysicalLimitLabel.Text = "Physical";
            // 
            // lblPhysical
            // 
            this.lblPhysical.AutoSize = true;
            this.lblPhysical.Location = new System.Drawing.Point(406, 34);
            this.lblPhysical.Name = "lblPhysical";
            this.lblPhysical.Size = new System.Drawing.Size(19, 13);
            this.lblPhysical.TabIndex = 82;
            this.lblPhysical.Text = "[0]";
            // 
            // cmdAddLimitModifier
            // 
            this.cmdAddLimitModifier.AutoSize = true;
            this.cmdAddLimitModifier.Location = new System.Drawing.Point(3, 6);
            this.cmdAddLimitModifier.Name = "cmdAddLimitModifier";
            this.cmdAddLimitModifier.Size = new System.Drawing.Size(100, 23);
            this.cmdAddLimitModifier.TabIndex = 80;
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
            this.treLimit.Location = new System.Drawing.Point(3, 35);
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
            this.treLimit.Size = new System.Drawing.Size(299, 548);
            this.treLimit.TabIndex = 79;
            this.treLimit.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treLimit_KeyDown);
            // 
            // cmdDeleteLimitModifier
            // 
            this.cmdDeleteLimitModifier.AutoSize = true;
            this.cmdDeleteLimitModifier.Location = new System.Drawing.Point(222, 6);
            this.cmdDeleteLimitModifier.Name = "cmdDeleteLimitModifier";
            this.cmdDeleteLimitModifier.Size = new System.Drawing.Size(80, 23);
            this.cmdDeleteLimitModifier.TabIndex = 78;
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
            this.tabMartialArts.Size = new System.Drawing.Size(861, 586);
            this.tabMartialArts.TabIndex = 8;
            this.tabMartialArts.Tag = "Tab_MartialArts";
            this.tabMartialArts.Text = "Martial Arts";
            // 
            // cmdAddMartialArt
            // 
            this.cmdAddMartialArt.AutoSize = true;
            this.cmdAddMartialArt.ContextMenuStrip = this.cmsMartialArts;
            this.cmdAddMartialArt.Location = new System.Drawing.Point(8, 7);
            this.cmdAddMartialArt.Name = "cmdAddMartialArt";
            this.cmdAddMartialArt.Size = new System.Drawing.Size(107, 23);
            this.cmdAddMartialArt.SplitMenuStrip = this.cmsMartialArts;
            this.cmdAddMartialArt.TabIndex = 65;
            this.cmdAddMartialArt.Tag = "Button_AddMartialArt";
            this.cmdAddMartialArt.Text = "&Add Martial Art";
            this.cmdAddMartialArt.UseVisualStyleBackColor = true;
            this.cmdAddMartialArt.Click += new System.EventHandler(this.cmdAddMartialArt_Click);
            // 
            // lblMartialArtSource
            // 
            this.lblMartialArtSource.AutoSize = true;
            this.lblMartialArtSource.Location = new System.Drawing.Point(380, 36);
            this.lblMartialArtSource.Name = "lblMartialArtSource";
            this.lblMartialArtSource.Size = new System.Drawing.Size(47, 13);
            this.lblMartialArtSource.TabIndex = 25;
            this.lblMartialArtSource.Text = "[Source]";
            this.lblMartialArtSource.Click += new System.EventHandler(this.lblMartialArtSource_Click);
            // 
            // lblMartialArtSourceLabel
            // 
            this.lblMartialArtSourceLabel.AutoSize = true;
            this.lblMartialArtSourceLabel.Location = new System.Drawing.Point(329, 36);
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
            treeNode7.Name = "nodQualities";
            treeNode7.Tag = "";
            treeNode7.Text = "Selected Qualities";
            this.treMartialArts.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode6,
            treeNode7});
            this.treMartialArts.ShowNodeToolTips = true;
            this.treMartialArts.ShowPlusMinus = false;
            this.treMartialArts.ShowRootLines = false;
            this.treMartialArts.Size = new System.Drawing.Size(315, 548);
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
            this.tabMagician.Controls.Add(this.cmdQuickenSpell);
            this.tabMagician.Controls.Add(this.lblSpellDicePool);
            this.tabMagician.Controls.Add(this.lblSpellDicePoolLabel);
            this.tabMagician.Controls.Add(this.lblMentorSpirit);
            this.tabMagician.Controls.Add(this.lblMentorSpiritLabel);
            this.tabMagician.Controls.Add(this.lblMentorSpiritInformation);
            this.tabMagician.Controls.Add(this.cboTradition);
            this.tabMagician.Controls.Add(this.lblDrainAttributesValue);
            this.tabMagician.Controls.Add(this.lblDrainAttributes);
            this.tabMagician.Controls.Add(this.lblDrainAttributesLabel);
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
            this.tabMagician.Controls.Add(this.cmdRollDrain);
            this.tabMagician.Controls.Add(this.cmdRollSpell);
            this.tabMagician.Controls.Add(this.cmdAddSpell);
            this.tabMagician.Location = new System.Drawing.Point(4, 22);
            this.tabMagician.Name = "tabMagician";
            this.tabMagician.Padding = new System.Windows.Forms.Padding(3);
            this.tabMagician.Size = new System.Drawing.Size(861, 586);
            this.tabMagician.TabIndex = 1;
            this.tabMagician.Tag = "Tab_Magician";
            this.tabMagician.Text = "Spells and Spirits";
            // 
            // cboSpiritManipulation
            // 
            this.cboSpiritManipulation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSpiritManipulation.FormattingEnabled = true;
            this.cboSpiritManipulation.Location = new System.Drawing.Point(639, 344);
            this.cboSpiritManipulation.Name = "cboSpiritManipulation";
            this.cboSpiritManipulation.Size = new System.Drawing.Size(122, 21);
            this.cboSpiritManipulation.TabIndex = 166;
            this.cboSpiritManipulation.Visible = false;
            this.cboSpiritManipulation.SelectedIndexChanged += new System.EventHandler(this.cboSpiritManipulation_SelectedIndexChanged);
            // 
            // lblSpiritManipulation
            // 
            this.lblSpiritManipulation.AutoSize = true;
            this.lblSpiritManipulation.Location = new System.Drawing.Point(566, 347);
            this.lblSpiritManipulation.Name = "lblSpiritManipulation";
            this.lblSpiritManipulation.Size = new System.Drawing.Size(70, 13);
            this.lblSpiritManipulation.TabIndex = 165;
            this.lblSpiritManipulation.Tag = "Label_SpiritManipulation";
            this.lblSpiritManipulation.Text = "Manipulation:";
            this.lblSpiritManipulation.Visible = false;
            // 
            // cboSpiritIllusion
            // 
            this.cboSpiritIllusion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSpiritIllusion.FormattingEnabled = true;
            this.cboSpiritIllusion.Location = new System.Drawing.Point(639, 317);
            this.cboSpiritIllusion.Name = "cboSpiritIllusion";
            this.cboSpiritIllusion.Size = new System.Drawing.Size(122, 21);
            this.cboSpiritIllusion.TabIndex = 164;
            this.cboSpiritIllusion.Visible = false;
            this.cboSpiritIllusion.SelectedIndexChanged += new System.EventHandler(this.cboSpiritIllusion_SelectedIndexChanged);
            // 
            // lblSpiritIllusion
            // 
            this.lblSpiritIllusion.AutoSize = true;
            this.lblSpiritIllusion.Location = new System.Drawing.Point(566, 320);
            this.lblSpiritIllusion.Name = "lblSpiritIllusion";
            this.lblSpiritIllusion.Size = new System.Drawing.Size(42, 13);
            this.lblSpiritIllusion.TabIndex = 163;
            this.lblSpiritIllusion.Tag = "Label_SpiritIllusion";
            this.lblSpiritIllusion.Text = "Illusion:";
            this.lblSpiritIllusion.Visible = false;
            // 
            // cboSpiritHealth
            // 
            this.cboSpiritHealth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSpiritHealth.FormattingEnabled = true;
            this.cboSpiritHealth.Location = new System.Drawing.Point(639, 290);
            this.cboSpiritHealth.Name = "cboSpiritHealth";
            this.cboSpiritHealth.Size = new System.Drawing.Size(122, 21);
            this.cboSpiritHealth.TabIndex = 162;
            this.cboSpiritHealth.Visible = false;
            this.cboSpiritHealth.SelectedIndexChanged += new System.EventHandler(this.cboSpiritHealth_SelectedIndexChanged);
            // 
            // lblSpiritHealth
            // 
            this.lblSpiritHealth.AutoSize = true;
            this.lblSpiritHealth.Location = new System.Drawing.Point(566, 293);
            this.lblSpiritHealth.Name = "lblSpiritHealth";
            this.lblSpiritHealth.Size = new System.Drawing.Size(41, 13);
            this.lblSpiritHealth.TabIndex = 161;
            this.lblSpiritHealth.Tag = "Label_SpiritHealth";
            this.lblSpiritHealth.Text = "Health:";
            this.lblSpiritHealth.Visible = false;
            // 
            // cboSpiritDetection
            // 
            this.cboSpiritDetection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSpiritDetection.FormattingEnabled = true;
            this.cboSpiritDetection.Location = new System.Drawing.Point(639, 263);
            this.cboSpiritDetection.Name = "cboSpiritDetection";
            this.cboSpiritDetection.Size = new System.Drawing.Size(122, 21);
            this.cboSpiritDetection.TabIndex = 160;
            this.cboSpiritDetection.Visible = false;
            this.cboSpiritDetection.SelectedIndexChanged += new System.EventHandler(this.cboSpiritDetection_SelectedIndexChanged);
            // 
            // lblSpiritDetection
            // 
            this.lblSpiritDetection.AutoSize = true;
            this.lblSpiritDetection.Location = new System.Drawing.Point(566, 266);
            this.lblSpiritDetection.Name = "lblSpiritDetection";
            this.lblSpiritDetection.Size = new System.Drawing.Size(56, 13);
            this.lblSpiritDetection.TabIndex = 159;
            this.lblSpiritDetection.Tag = "Label_SpiritDetection";
            this.lblSpiritDetection.Text = "Detection:";
            this.lblSpiritDetection.Visible = false;
            // 
            // cboSpiritCombat
            // 
            this.cboSpiritCombat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSpiritCombat.FormattingEnabled = true;
            this.cboSpiritCombat.Location = new System.Drawing.Point(639, 236);
            this.cboSpiritCombat.Name = "cboSpiritCombat";
            this.cboSpiritCombat.Size = new System.Drawing.Size(122, 21);
            this.cboSpiritCombat.TabIndex = 158;
            this.cboSpiritCombat.Visible = false;
            this.cboSpiritCombat.SelectedIndexChanged += new System.EventHandler(this.cboSpiritCombat_SelectedIndexChanged);
            // 
            // lblSpiritCombat
            // 
            this.lblSpiritCombat.AutoSize = true;
            this.lblSpiritCombat.Location = new System.Drawing.Point(566, 239);
            this.lblSpiritCombat.Name = "lblSpiritCombat";
            this.lblSpiritCombat.Size = new System.Drawing.Size(46, 13);
            this.lblSpiritCombat.TabIndex = 157;
            this.lblSpiritCombat.Tag = "Label_SpiritCombat";
            this.lblSpiritCombat.Text = "Combat:";
            this.lblSpiritCombat.Visible = false;
            // 
            // cboDrain
            // 
            this.cboDrain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDrain.FormattingEnabled = true;
            this.cboDrain.Location = new System.Drawing.Point(407, 236);
            this.cboDrain.Name = "cboDrain";
            this.cboDrain.Size = new System.Drawing.Size(82, 21);
            this.cboDrain.TabIndex = 156;
            this.cboDrain.Visible = false;
            this.cboDrain.SelectedIndexChanged += new System.EventHandler(this.cboDrain_SelectedIndexChanged);
            // 
            // txtTraditionName
            // 
            this.txtTraditionName.Location = new System.Drawing.Point(639, 213);
            this.txtTraditionName.Name = "txtTraditionName";
            this.txtTraditionName.Size = new System.Drawing.Size(122, 20);
            this.txtTraditionName.TabIndex = 155;
            this.txtTraditionName.Visible = false;
            this.txtTraditionName.TextChanged += new System.EventHandler(this.txtTraditionName_TextChanged);
            // 
            // lblTraditionName
            // 
            this.lblTraditionName.AutoSize = true;
            this.lblTraditionName.Location = new System.Drawing.Point(566, 216);
            this.lblTraditionName.Name = "lblTraditionName";
            this.lblTraditionName.Size = new System.Drawing.Size(38, 13);
            this.lblTraditionName.TabIndex = 154;
            this.lblTraditionName.Tag = "Label_TraditionName";
            this.lblTraditionName.Text = "Name:";
            this.lblTraditionName.Visible = false;
            // 
            // cmdQuickenSpell
            // 
            this.cmdQuickenSpell.AutoSize = true;
            this.cmdQuickenSpell.Location = new System.Drawing.Point(491, 167);
            this.cmdQuickenSpell.Name = "cmdQuickenSpell";
            this.cmdQuickenSpell.Size = new System.Drawing.Size(83, 23);
            this.cmdQuickenSpell.TabIndex = 108;
            this.cmdQuickenSpell.Tag = "Button_QuickenSpell";
            this.cmdQuickenSpell.Text = "Quicken Spell";
            this.cmdQuickenSpell.UseVisualStyleBackColor = true;
            this.cmdQuickenSpell.Visible = false;
            this.cmdQuickenSpell.Click += new System.EventHandler(this.cmdQuickenSpell_Click);
            // 
            // lblSpellDicePool
            // 
            this.lblSpellDicePool.AutoSize = true;
            this.lblSpellDicePool.Location = new System.Drawing.Point(378, 172);
            this.lblSpellDicePool.Name = "lblSpellDicePool";
            this.lblSpellDicePool.Size = new System.Drawing.Size(59, 13);
            this.lblSpellDicePool.TabIndex = 105;
            this.lblSpellDicePool.Text = "[Dice Pool]";
            // 
            // lblSpellDicePoolLabel
            // 
            this.lblSpellDicePoolLabel.AutoSize = true;
            this.lblSpellDicePoolLabel.Location = new System.Drawing.Point(309, 172);
            this.lblSpellDicePoolLabel.Name = "lblSpellDicePoolLabel";
            this.lblSpellDicePoolLabel.Size = new System.Drawing.Size(56, 13);
            this.lblSpellDicePoolLabel.TabIndex = 104;
            this.lblSpellDicePoolLabel.Tag = "Label_DicePool";
            this.lblSpellDicePoolLabel.Text = "Dice Pool:";
            // 
            // lblMentorSpirit
            // 
            this.lblMentorSpirit.AutoSize = true;
            this.lblMentorSpirit.Location = new System.Drawing.Point(404, 347);
            this.lblMentorSpirit.Name = "lblMentorSpirit";
            this.lblMentorSpirit.Size = new System.Drawing.Size(72, 13);
            this.lblMentorSpirit.TabIndex = 102;
            this.lblMentorSpirit.Text = "[Mentor Spirit]";
            this.lblMentorSpirit.Visible = false;
            // 
            // lblMentorSpiritLabel
            // 
            this.lblMentorSpiritLabel.AutoSize = true;
            this.lblMentorSpiritLabel.Location = new System.Drawing.Point(309, 347);
            this.lblMentorSpiritLabel.Name = "lblMentorSpiritLabel";
            this.lblMentorSpiritLabel.Size = new System.Drawing.Size(69, 13);
            this.lblMentorSpiritLabel.TabIndex = 101;
            this.lblMentorSpiritLabel.Tag = "Label_MentorSpirit";
            this.lblMentorSpiritLabel.Text = "Mentor Spirit:";
            this.lblMentorSpiritLabel.Visible = false;
            // 
            // lblMentorSpiritInformation
            // 
            this.lblMentorSpiritInformation.Location = new System.Drawing.Point(309, 368);
            this.lblMentorSpiritInformation.Name = "lblMentorSpiritInformation";
            this.lblMentorSpiritInformation.Size = new System.Drawing.Size(526, 75);
            this.lblMentorSpiritInformation.TabIndex = 100;
            this.lblMentorSpiritInformation.Text = "[Mentor Spirit Information]";
            this.lblMentorSpiritInformation.Visible = false;
            // 
            // cboTradition
            // 
            this.cboTradition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTradition.FormattingEnabled = true;
            this.cboTradition.Location = new System.Drawing.Point(407, 211);
            this.cboTradition.Name = "cboTradition";
            this.cboTradition.Size = new System.Drawing.Size(141, 21);
            this.cboTradition.TabIndex = 99;
            this.cboTradition.SelectedIndexChanged += new System.EventHandler(this.cboTradition_SelectedIndexChanged);
            // 
            // lblDrainAttributesValue
            // 
            this.lblDrainAttributesValue.AutoSize = true;
            this.lblDrainAttributesValue.Location = new System.Drawing.Point(495, 237);
            this.lblDrainAttributesValue.Name = "lblDrainAttributesValue";
            this.lblDrainAttributesValue.Size = new System.Drawing.Size(37, 13);
            this.lblDrainAttributesValue.TabIndex = 98;
            this.lblDrainAttributesValue.Text = "[Total]";
            // 
            // lblDrainAttributes
            // 
            this.lblDrainAttributes.AutoSize = true;
            this.lblDrainAttributes.Location = new System.Drawing.Point(404, 237);
            this.lblDrainAttributes.Name = "lblDrainAttributes";
            this.lblDrainAttributes.Size = new System.Drawing.Size(57, 13);
            this.lblDrainAttributes.TabIndex = 96;
            this.lblDrainAttributes.Text = "[Attributes]";
            // 
            // lblDrainAttributesLabel
            // 
            this.lblDrainAttributesLabel.AutoSize = true;
            this.lblDrainAttributesLabel.Location = new System.Drawing.Point(309, 237);
            this.lblDrainAttributesLabel.Name = "lblDrainAttributesLabel";
            this.lblDrainAttributesLabel.Size = new System.Drawing.Size(89, 13);
            this.lblDrainAttributesLabel.TabIndex = 95;
            this.lblDrainAttributesLabel.Tag = "Label_ResistDrain";
            this.lblDrainAttributesLabel.Text = "Resist Drain with:";
            // 
            // lblTraditionLabel
            // 
            this.lblTraditionLabel.AutoSize = true;
            this.lblTraditionLabel.Location = new System.Drawing.Point(309, 214);
            this.lblTraditionLabel.Name = "lblTraditionLabel";
            this.lblTraditionLabel.Size = new System.Drawing.Size(51, 13);
            this.lblTraditionLabel.TabIndex = 93;
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
            this.cmdAddSpirit.Location = new System.Drawing.Point(11, 420);
            this.cmdAddSpirit.Name = "cmdAddSpirit";
            this.cmdAddSpirit.Size = new System.Drawing.Size(75, 23);
            this.cmdAddSpirit.TabIndex = 23;
            this.cmdAddSpirit.Tag = "Button_AddSpirit";
            this.cmdAddSpirit.Text = "A&dd Spirit";
            this.cmdAddSpirit.UseVisualStyleBackColor = true;
            this.cmdAddSpirit.Click += new System.EventHandler(this.cmdAddSpirit_Click);
            // 
            // lblSpirits
            // 
            this.lblSpirits.AutoSize = true;
            this.lblSpirits.Location = new System.Drawing.Point(8, 404);
            this.lblSpirits.Name = "lblSpirits";
            this.lblSpirits.Size = new System.Drawing.Size(35, 13);
            this.lblSpirits.TabIndex = 0;
            this.lblSpirits.Tag = "Label_Spirits";
            this.lblSpirits.Text = "Spirits";
            // 
            // panSpirits
            // 
            this.panSpirits.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panSpirits.AutoScroll = true;
            this.panSpirits.Location = new System.Drawing.Point(11, 449);
            this.panSpirits.Name = "panSpirits";
            this.panSpirits.Size = new System.Drawing.Size(824, 131);
            this.panSpirits.TabIndex = 4;
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
            // 
            // cmdRollDrain
            // 
            this.cmdRollDrain.FlatAppearance.BorderSize = 0;
            this.cmdRollDrain.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdRollDrain.Image = global::Chummer.Properties.Resources.die;
            this.cmdRollDrain.Location = new System.Drawing.Point(538, 233);
            this.cmdRollDrain.Name = "cmdRollDrain";
            this.cmdRollDrain.Size = new System.Drawing.Size(24, 24);
            this.cmdRollDrain.TabIndex = 107;
            this.cmdRollDrain.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.cmdRollDrain.UseVisualStyleBackColor = true;
            this.cmdRollDrain.Visible = false;
            this.cmdRollDrain.Click += new System.EventHandler(this.cmdRollDrain_Click);
            // 
            // cmdRollSpell
            // 
            this.cmdRollSpell.FlatAppearance.BorderSize = 0;
            this.cmdRollSpell.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdRollSpell.Image = global::Chummer.Properties.Resources.die;
            this.cmdRollSpell.Location = new System.Drawing.Point(443, 166);
            this.cmdRollSpell.Name = "cmdRollSpell";
            this.cmdRollSpell.Size = new System.Drawing.Size(24, 24);
            this.cmdRollSpell.TabIndex = 106;
            this.cmdRollSpell.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.cmdRollSpell.UseVisualStyleBackColor = true;
            this.cmdRollSpell.Visible = false;
            this.cmdRollSpell.Click += new System.EventHandler(this.cmdRollSpell_Click);
            // 
            // cmdAddSpell
            // 
            this.cmdAddSpell.AutoSize = true;
            this.cmdAddSpell.ContextMenuStrip = this.cmsSpellButton;
            this.cmdAddSpell.Location = new System.Drawing.Point(8, 26);
            this.cmdAddSpell.Name = "cmdAddSpell";
            this.cmdAddSpell.Size = new System.Drawing.Size(80, 23);
            this.cmdAddSpell.SplitMenuStrip = this.cmsSpellButton;
            this.cmdAddSpell.TabIndex = 103;
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
            this.tabAdept.Size = new System.Drawing.Size(861, 586);
            this.tabAdept.TabIndex = 2;
            this.tabAdept.Tag = "Tab_Adept";
            this.tabAdept.Text = "Adept Powers";
            // 
            // tabTechnomancer
            // 
            this.tabTechnomancer.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabTechnomancer.Controls.Add(this.lblFV);
            this.tabTechnomancer.Controls.Add(this.lblFVLabel);
            this.tabTechnomancer.Controls.Add(this.lblDuration);
            this.tabTechnomancer.Controls.Add(this.lblDurationLabel);
            this.tabTechnomancer.Controls.Add(this.lblTarget);
            this.tabTechnomancer.Controls.Add(this.lblTargetLabel);
            this.tabTechnomancer.Controls.Add(this.lblComplexFormSource);
            this.tabTechnomancer.Controls.Add(this.lblComplexFormSourceLabel);
            this.tabTechnomancer.Controls.Add(this.lblLivingPersonaFirewall);
            this.tabTechnomancer.Controls.Add(this.lblLivingPersonaFirewallLabel);
            this.tabTechnomancer.Controls.Add(this.lblLivingPersonaDataProcessing);
            this.tabTechnomancer.Controls.Add(this.lblLivingPersonaDataProcessingLabel);
            this.tabTechnomancer.Controls.Add(this.lblLivingPersonaSleaze);
            this.tabTechnomancer.Controls.Add(this.lblLivingPersonaSleazeLabel);
            this.tabTechnomancer.Controls.Add(this.lblLivingPersonaAttack);
            this.tabTechnomancer.Controls.Add(this.lblLivingPersonaAttackLabel);
            this.tabTechnomancer.Controls.Add(this.lblLivingPersonaLabel);
            this.tabTechnomancer.Controls.Add(this.lblLivingPersonaDeviceRating);
            this.tabTechnomancer.Controls.Add(this.lblLivingPersonaDeviceRatingLabel);
            this.tabTechnomancer.Controls.Add(this.cmdRollFading);
            this.tabTechnomancer.Controls.Add(this.cboStream);
            this.tabTechnomancer.Controls.Add(this.lblFadingAttributesValue);
            this.tabTechnomancer.Controls.Add(this.lblFadingAttributes);
            this.tabTechnomancer.Controls.Add(this.lblFadingAttributesLabel);
            this.tabTechnomancer.Controls.Add(this.lblStreamLabel);
            this.tabTechnomancer.Controls.Add(this.treComplexForms);
            this.tabTechnomancer.Controls.Add(this.cmdDeleteComplexForm);
            this.tabTechnomancer.Controls.Add(this.lblComplexForms);
            this.tabTechnomancer.Controls.Add(this.cmdAddSprite);
            this.tabTechnomancer.Controls.Add(this.lblSprites);
            this.tabTechnomancer.Controls.Add(this.panSprites);
            this.tabTechnomancer.Controls.Add(this.cmdAddComplexForm);
            this.tabTechnomancer.Location = new System.Drawing.Point(4, 22);
            this.tabTechnomancer.Name = "tabTechnomancer";
            this.tabTechnomancer.Size = new System.Drawing.Size(861, 586);
            this.tabTechnomancer.TabIndex = 3;
            this.tabTechnomancer.Tag = "Tab_Technomancer";
            this.tabTechnomancer.Text = "Sprites and Complex Forms";
            // 
            // lblFV
            // 
            this.lblFV.AutoSize = true;
            this.lblFV.Location = new System.Drawing.Point(423, 99);
            this.lblFV.Name = "lblFV";
            this.lblFV.Size = new System.Drawing.Size(39, 13);
            this.lblFV.TabIndex = 180;
            this.lblFV.Text = "[None]";
            // 
            // lblFVLabel
            // 
            this.lblFVLabel.AutoSize = true;
            this.lblFVLabel.Location = new System.Drawing.Point(309, 99);
            this.lblFVLabel.Name = "lblFVLabel";
            this.lblFVLabel.Size = new System.Drawing.Size(23, 13);
            this.lblFVLabel.TabIndex = 179;
            this.lblFVLabel.Tag = "Label_SelectProgram_FV";
            this.lblFVLabel.Text = "FV:";
            // 
            // lblDuration
            // 
            this.lblDuration.AutoSize = true;
            this.lblDuration.Location = new System.Drawing.Point(423, 76);
            this.lblDuration.Name = "lblDuration";
            this.lblDuration.Size = new System.Drawing.Size(39, 13);
            this.lblDuration.TabIndex = 178;
            this.lblDuration.Text = "[None]";
            // 
            // lblDurationLabel
            // 
            this.lblDurationLabel.AutoSize = true;
            this.lblDurationLabel.Location = new System.Drawing.Point(309, 76);
            this.lblDurationLabel.Name = "lblDurationLabel";
            this.lblDurationLabel.Size = new System.Drawing.Size(50, 13);
            this.lblDurationLabel.TabIndex = 177;
            this.lblDurationLabel.Tag = "Label_SelectProgram_Duration";
            this.lblDurationLabel.Text = "Duration:";
            // 
            // lblTarget
            // 
            this.lblTarget.AutoSize = true;
            this.lblTarget.Location = new System.Drawing.Point(423, 54);
            this.lblTarget.Name = "lblTarget";
            this.lblTarget.Size = new System.Drawing.Size(39, 13);
            this.lblTarget.TabIndex = 176;
            this.lblTarget.Text = "[None]";
            // 
            // lblTargetLabel
            // 
            this.lblTargetLabel.AutoSize = true;
            this.lblTargetLabel.Location = new System.Drawing.Point(309, 54);
            this.lblTargetLabel.Name = "lblTargetLabel";
            this.lblTargetLabel.Size = new System.Drawing.Size(41, 13);
            this.lblTargetLabel.TabIndex = 175;
            this.lblTargetLabel.Tag = "Label_SelectProgram_Target";
            this.lblTargetLabel.Text = "Target:";
            // 
            // lblComplexFormSource
            // 
            this.lblComplexFormSource.AutoSize = true;
            this.lblComplexFormSource.Location = new System.Drawing.Point(388, 122);
            this.lblComplexFormSource.Name = "lblComplexFormSource";
            this.lblComplexFormSource.Size = new System.Drawing.Size(47, 13);
            this.lblComplexFormSource.TabIndex = 174;
            this.lblComplexFormSource.Text = "[Source]";
            // 
            // lblComplexFormSourceLabel
            // 
            this.lblComplexFormSourceLabel.AutoSize = true;
            this.lblComplexFormSourceLabel.Location = new System.Drawing.Point(309, 122);
            this.lblComplexFormSourceLabel.Name = "lblComplexFormSourceLabel";
            this.lblComplexFormSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblComplexFormSourceLabel.TabIndex = 173;
            this.lblComplexFormSourceLabel.Tag = "Label_Source";
            this.lblComplexFormSourceLabel.Text = "Source:";
            // 
            // lblLivingPersonaFirewall
            // 
            this.lblLivingPersonaFirewall.AutoSize = true;
            this.lblLivingPersonaFirewall.Location = new System.Drawing.Point(393, 338);
            this.lblLivingPersonaFirewall.Name = "lblLivingPersonaFirewall";
            this.lblLivingPersonaFirewall.Size = new System.Drawing.Size(19, 13);
            this.lblLivingPersonaFirewall.TabIndex = 172;
            this.lblLivingPersonaFirewall.Text = "[0]";
            // 
            // lblLivingPersonaFirewallLabel
            // 
            this.lblLivingPersonaFirewallLabel.AutoSize = true;
            this.lblLivingPersonaFirewallLabel.Location = new System.Drawing.Point(309, 338);
            this.lblLivingPersonaFirewallLabel.Name = "lblLivingPersonaFirewallLabel";
            this.lblLivingPersonaFirewallLabel.Size = new System.Drawing.Size(45, 13);
            this.lblLivingPersonaFirewallLabel.TabIndex = 171;
            this.lblLivingPersonaFirewallLabel.Tag = "Label_Firewall";
            this.lblLivingPersonaFirewallLabel.Text = "Firewall:";
            // 
            // lblLivingPersonaDataProcessing
            // 
            this.lblLivingPersonaDataProcessing.AutoSize = true;
            this.lblLivingPersonaDataProcessing.Location = new System.Drawing.Point(393, 318);
            this.lblLivingPersonaDataProcessing.Name = "lblLivingPersonaDataProcessing";
            this.lblLivingPersonaDataProcessing.Size = new System.Drawing.Size(19, 13);
            this.lblLivingPersonaDataProcessing.TabIndex = 170;
            this.lblLivingPersonaDataProcessing.Text = "[0]";
            // 
            // lblLivingPersonaDataProcessingLabel
            // 
            this.lblLivingPersonaDataProcessingLabel.AutoSize = true;
            this.lblLivingPersonaDataProcessingLabel.Location = new System.Drawing.Point(309, 318);
            this.lblLivingPersonaDataProcessingLabel.Name = "lblLivingPersonaDataProcessingLabel";
            this.lblLivingPersonaDataProcessingLabel.Size = new System.Drawing.Size(88, 13);
            this.lblLivingPersonaDataProcessingLabel.TabIndex = 169;
            this.lblLivingPersonaDataProcessingLabel.Tag = "Label_DataProcessing";
            this.lblLivingPersonaDataProcessingLabel.Text = "Data Processing:";
            // 
            // lblLivingPersonaSleaze
            // 
            this.lblLivingPersonaSleaze.AutoSize = true;
            this.lblLivingPersonaSleaze.Location = new System.Drawing.Point(393, 298);
            this.lblLivingPersonaSleaze.Name = "lblLivingPersonaSleaze";
            this.lblLivingPersonaSleaze.Size = new System.Drawing.Size(19, 13);
            this.lblLivingPersonaSleaze.TabIndex = 168;
            this.lblLivingPersonaSleaze.Text = "[0]";
            // 
            // lblLivingPersonaSleazeLabel
            // 
            this.lblLivingPersonaSleazeLabel.AutoSize = true;
            this.lblLivingPersonaSleazeLabel.Location = new System.Drawing.Point(309, 298);
            this.lblLivingPersonaSleazeLabel.Name = "lblLivingPersonaSleazeLabel";
            this.lblLivingPersonaSleazeLabel.Size = new System.Drawing.Size(42, 13);
            this.lblLivingPersonaSleazeLabel.TabIndex = 167;
            this.lblLivingPersonaSleazeLabel.Tag = "Label_Sleaze";
            this.lblLivingPersonaSleazeLabel.Text = "Sleaze:";
            // 
            // lblLivingPersonaAttack
            // 
            this.lblLivingPersonaAttack.AutoSize = true;
            this.lblLivingPersonaAttack.Location = new System.Drawing.Point(393, 278);
            this.lblLivingPersonaAttack.Name = "lblLivingPersonaAttack";
            this.lblLivingPersonaAttack.Size = new System.Drawing.Size(19, 13);
            this.lblLivingPersonaAttack.TabIndex = 166;
            this.lblLivingPersonaAttack.Text = "[0]";
            // 
            // lblLivingPersonaAttackLabel
            // 
            this.lblLivingPersonaAttackLabel.AutoSize = true;
            this.lblLivingPersonaAttackLabel.Location = new System.Drawing.Point(309, 278);
            this.lblLivingPersonaAttackLabel.Name = "lblLivingPersonaAttackLabel";
            this.lblLivingPersonaAttackLabel.Size = new System.Drawing.Size(41, 13);
            this.lblLivingPersonaAttackLabel.TabIndex = 165;
            this.lblLivingPersonaAttackLabel.Tag = "Label_Attack";
            this.lblLivingPersonaAttackLabel.Text = "Attack:";
            // 
            // lblLivingPersonaLabel
            // 
            this.lblLivingPersonaLabel.AutoSize = true;
            this.lblLivingPersonaLabel.Location = new System.Drawing.Point(309, 235);
            this.lblLivingPersonaLabel.Name = "lblLivingPersonaLabel";
            this.lblLivingPersonaLabel.Size = new System.Drawing.Size(77, 13);
            this.lblLivingPersonaLabel.TabIndex = 164;
            this.lblLivingPersonaLabel.Tag = "String_LivingPersona";
            this.lblLivingPersonaLabel.Text = "Living Persona";
            // 
            // lblLivingPersonaDeviceRating
            // 
            this.lblLivingPersonaDeviceRating.AutoSize = true;
            this.lblLivingPersonaDeviceRating.Location = new System.Drawing.Point(393, 258);
            this.lblLivingPersonaDeviceRating.Name = "lblLivingPersonaDeviceRating";
            this.lblLivingPersonaDeviceRating.Size = new System.Drawing.Size(19, 13);
            this.lblLivingPersonaDeviceRating.TabIndex = 163;
            this.lblLivingPersonaDeviceRating.Text = "[0]";
            // 
            // lblLivingPersonaDeviceRatingLabel
            // 
            this.lblLivingPersonaDeviceRatingLabel.AutoSize = true;
            this.lblLivingPersonaDeviceRatingLabel.Location = new System.Drawing.Point(309, 258);
            this.lblLivingPersonaDeviceRatingLabel.Name = "lblLivingPersonaDeviceRatingLabel";
            this.lblLivingPersonaDeviceRatingLabel.Size = new System.Drawing.Size(78, 13);
            this.lblLivingPersonaDeviceRatingLabel.TabIndex = 162;
            this.lblLivingPersonaDeviceRatingLabel.Tag = "Label_DeviceRating";
            this.lblLivingPersonaDeviceRatingLabel.Text = "Device Rating:";
            // 
            // cmdRollFading
            // 
            this.cmdRollFading.FlatAppearance.BorderSize = 0;
            this.cmdRollFading.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdRollFading.Image = global::Chummer.Properties.Resources.die;
            this.cmdRollFading.Location = new System.Drawing.Point(538, 185);
            this.cmdRollFading.Name = "cmdRollFading";
            this.cmdRollFading.Size = new System.Drawing.Size(24, 24);
            this.cmdRollFading.TabIndex = 132;
            this.cmdRollFading.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.cmdRollFading.UseVisualStyleBackColor = true;
            this.cmdRollFading.Visible = false;
            this.cmdRollFading.Click += new System.EventHandler(this.cmdRollFading_Click);
            // 
            // cboStream
            // 
            this.cboStream.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStream.FormattingEnabled = true;
            this.cboStream.Location = new System.Drawing.Point(407, 163);
            this.cboStream.Name = "cboStream";
            this.cboStream.Size = new System.Drawing.Size(141, 21);
            this.cboStream.TabIndex = 106;
            this.cboStream.Visible = false;
            this.cboStream.SelectedIndexChanged += new System.EventHandler(this.cboStream_SelectedIndexChanged);
            // 
            // lblFadingAttributesValue
            // 
            this.lblFadingAttributesValue.AutoSize = true;
            this.lblFadingAttributesValue.Location = new System.Drawing.Point(495, 189);
            this.lblFadingAttributesValue.Name = "lblFadingAttributesValue";
            this.lblFadingAttributesValue.Size = new System.Drawing.Size(37, 13);
            this.lblFadingAttributesValue.TabIndex = 104;
            this.lblFadingAttributesValue.Text = "[Total]";
            // 
            // lblFadingAttributes
            // 
            this.lblFadingAttributes.AutoSize = true;
            this.lblFadingAttributes.Location = new System.Drawing.Point(404, 189);
            this.lblFadingAttributes.Name = "lblFadingAttributes";
            this.lblFadingAttributes.Size = new System.Drawing.Size(57, 13);
            this.lblFadingAttributes.TabIndex = 102;
            this.lblFadingAttributes.Text = "[Attributes]";
            // 
            // lblFadingAttributesLabel
            // 
            this.lblFadingAttributesLabel.AutoSize = true;
            this.lblFadingAttributesLabel.Location = new System.Drawing.Point(309, 189);
            this.lblFadingAttributesLabel.Name = "lblFadingAttributesLabel";
            this.lblFadingAttributesLabel.Size = new System.Drawing.Size(96, 13);
            this.lblFadingAttributesLabel.TabIndex = 101;
            this.lblFadingAttributesLabel.Tag = "Label_ResistFading";
            this.lblFadingAttributesLabel.Text = "Resist Fading with:";
            // 
            // lblStreamLabel
            // 
            this.lblStreamLabel.AutoSize = true;
            this.lblStreamLabel.Location = new System.Drawing.Point(309, 166);
            this.lblStreamLabel.Name = "lblStreamLabel";
            this.lblStreamLabel.Size = new System.Drawing.Size(43, 13);
            this.lblStreamLabel.TabIndex = 100;
            this.lblStreamLabel.Tag = "Label_Stream";
            this.lblStreamLabel.Text = "Stream:";
            this.lblStreamLabel.Visible = false;
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
            this.treComplexForms.TabIndex = 94;
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
            this.cmdDeleteComplexForm.TabIndex = 93;
            this.cmdDeleteComplexForm.Tag = "String_Delete";
            this.cmdDeleteComplexForm.Text = "Delete";
            this.cmdDeleteComplexForm.UseVisualStyleBackColor = true;
            this.cmdDeleteComplexForm.Click += new System.EventHandler(this.cmdDeleteComplexForm_Click);
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
            // lblSprites
            // 
            this.lblSprites.AutoSize = true;
            this.lblSprites.Location = new System.Drawing.Point(8, 400);
            this.lblSprites.Name = "lblSprites";
            this.lblSprites.Size = new System.Drawing.Size(39, 13);
            this.lblSprites.TabIndex = 24;
            this.lblSprites.Tag = "Label_Sprites";
            this.lblSprites.Text = "Sprites";
            // 
            // panSprites
            // 
            this.panSprites.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panSprites.AutoScroll = true;
            this.panSprites.Location = new System.Drawing.Point(8, 445);
            this.panSprites.Name = "panSprites";
            this.panSprites.Size = new System.Drawing.Size(827, 138);
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
            this.cmdAddComplexForm.TabIndex = 105;
            this.cmdAddComplexForm.Tag = "Button_AddComplexForm";
            this.cmdAddComplexForm.Text = "Add Complex Form";
            this.cmdAddComplexForm.UseVisualStyleBackColor = true;
            this.cmdAddComplexForm.Click += new System.EventHandler(this.cmdAddComplexForm_Click);
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
            this.tabCritter.Size = new System.Drawing.Size(861, 586);
            this.tabCritter.TabIndex = 12;
            this.tabCritter.Tag = "Tab_Critter";
            this.tabCritter.Text = "Critter Powers";
            // 
            // chkCritterPowerCount
            // 
            this.chkCritterPowerCount.AutoSize = true;
            this.chkCritterPowerCount.Location = new System.Drawing.Point(350, 219);
            this.chkCritterPowerCount.Name = "chkCritterPowerCount";
            this.chkCritterPowerCount.Size = new System.Drawing.Size(182, 17);
            this.chkCritterPowerCount.TabIndex = 36;
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
            this.lblCritterPowerPointCost.TabIndex = 35;
            this.lblCritterPowerPointCost.Text = "[Power Points]";
            this.lblCritterPowerPointCost.Visible = false;
            // 
            // lblCritterPowerPointCostLabel
            // 
            this.lblCritterPowerPointCostLabel.AutoSize = true;
            this.lblCritterPowerPointCostLabel.Location = new System.Drawing.Point(347, 193);
            this.lblCritterPowerPointCostLabel.Name = "lblCritterPowerPointCostLabel";
            this.lblCritterPowerPointCostLabel.Size = new System.Drawing.Size(39, 13);
            this.lblCritterPowerPointCostLabel.TabIndex = 34;
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
            this.lblCritterPowerPoints.TabIndex = 33;
            this.lblCritterPowerPoints.Text = "0 (0 remaining)";
            this.lblCritterPowerPoints.Visible = false;
            // 
            // lblCritterPowerPointsLabel
            // 
            this.lblCritterPowerPointsLabel.AutoSize = true;
            this.lblCritterPowerPointsLabel.Location = new System.Drawing.Point(195, 11);
            this.lblCritterPowerPointsLabel.Name = "lblCritterPowerPointsLabel";
            this.lblCritterPowerPointsLabel.Size = new System.Drawing.Size(72, 13);
            this.lblCritterPowerPointsLabel.TabIndex = 32;
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
            this.cmdDeleteCritterPower.TabIndex = 31;
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
            this.cmdAddCritterPower.TabIndex = 30;
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
            this.lblCritterPowerSource.TabIndex = 29;
            this.lblCritterPowerSource.Text = "[Source]";
            this.lblCritterPowerSource.Click += new System.EventHandler(this.lblCritterPowerSource_Click);
            // 
            // lblCritterPowerSourceLabel
            // 
            this.lblCritterPowerSourceLabel.AutoSize = true;
            this.lblCritterPowerSourceLabel.Location = new System.Drawing.Point(347, 170);
            this.lblCritterPowerSourceLabel.Name = "lblCritterPowerSourceLabel";
            this.lblCritterPowerSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblCritterPowerSourceLabel.TabIndex = 28;
            this.lblCritterPowerSourceLabel.Tag = "Label_Source";
            this.lblCritterPowerSourceLabel.Text = "Source:";
            // 
            // lblCritterPowerDuration
            // 
            this.lblCritterPowerDuration.AutoSize = true;
            this.lblCritterPowerDuration.Location = new System.Drawing.Point(413, 147);
            this.lblCritterPowerDuration.Name = "lblCritterPowerDuration";
            this.lblCritterPowerDuration.Size = new System.Drawing.Size(53, 13);
            this.lblCritterPowerDuration.TabIndex = 27;
            this.lblCritterPowerDuration.Text = "[Duration]";
            // 
            // lblCritterPowerDurationLabel
            // 
            this.lblCritterPowerDurationLabel.AutoSize = true;
            this.lblCritterPowerDurationLabel.Location = new System.Drawing.Point(347, 147);
            this.lblCritterPowerDurationLabel.Name = "lblCritterPowerDurationLabel";
            this.lblCritterPowerDurationLabel.Size = new System.Drawing.Size(50, 13);
            this.lblCritterPowerDurationLabel.TabIndex = 26;
            this.lblCritterPowerDurationLabel.Tag = "Label_Duration";
            this.lblCritterPowerDurationLabel.Text = "Duration:";
            // 
            // lblCritterPowerRange
            // 
            this.lblCritterPowerRange.AutoSize = true;
            this.lblCritterPowerRange.Location = new System.Drawing.Point(413, 124);
            this.lblCritterPowerRange.Name = "lblCritterPowerRange";
            this.lblCritterPowerRange.Size = new System.Drawing.Size(45, 13);
            this.lblCritterPowerRange.TabIndex = 25;
            this.lblCritterPowerRange.Text = "[Range]";
            // 
            // lblCritterPowerRangeLabel
            // 
            this.lblCritterPowerRangeLabel.AutoSize = true;
            this.lblCritterPowerRangeLabel.Location = new System.Drawing.Point(347, 124);
            this.lblCritterPowerRangeLabel.Name = "lblCritterPowerRangeLabel";
            this.lblCritterPowerRangeLabel.Size = new System.Drawing.Size(42, 13);
            this.lblCritterPowerRangeLabel.TabIndex = 24;
            this.lblCritterPowerRangeLabel.Tag = "Label_Range";
            this.lblCritterPowerRangeLabel.Text = "Range:";
            // 
            // lblCritterPowerAction
            // 
            this.lblCritterPowerAction.AutoSize = true;
            this.lblCritterPowerAction.Location = new System.Drawing.Point(413, 101);
            this.lblCritterPowerAction.Name = "lblCritterPowerAction";
            this.lblCritterPowerAction.Size = new System.Drawing.Size(43, 13);
            this.lblCritterPowerAction.TabIndex = 23;
            this.lblCritterPowerAction.Text = "[Action]";
            // 
            // lblCritterPowerActionLabel
            // 
            this.lblCritterPowerActionLabel.AutoSize = true;
            this.lblCritterPowerActionLabel.Location = new System.Drawing.Point(347, 101);
            this.lblCritterPowerActionLabel.Name = "lblCritterPowerActionLabel";
            this.lblCritterPowerActionLabel.Size = new System.Drawing.Size(40, 13);
            this.lblCritterPowerActionLabel.TabIndex = 22;
            this.lblCritterPowerActionLabel.Tag = "Label_Action";
            this.lblCritterPowerActionLabel.Text = "Action:";
            // 
            // lblCritterPowerType
            // 
            this.lblCritterPowerType.AutoSize = true;
            this.lblCritterPowerType.Location = new System.Drawing.Point(413, 78);
            this.lblCritterPowerType.Name = "lblCritterPowerType";
            this.lblCritterPowerType.Size = new System.Drawing.Size(37, 13);
            this.lblCritterPowerType.TabIndex = 21;
            this.lblCritterPowerType.Text = "[Type]";
            // 
            // lblCritterPowerTypeLabel
            // 
            this.lblCritterPowerTypeLabel.AutoSize = true;
            this.lblCritterPowerTypeLabel.Location = new System.Drawing.Point(347, 78);
            this.lblCritterPowerTypeLabel.Name = "lblCritterPowerTypeLabel";
            this.lblCritterPowerTypeLabel.Size = new System.Drawing.Size(34, 13);
            this.lblCritterPowerTypeLabel.TabIndex = 20;
            this.lblCritterPowerTypeLabel.Tag = "Label_Type";
            this.lblCritterPowerTypeLabel.Text = "Type:";
            // 
            // lblCritterPowerCategory
            // 
            this.lblCritterPowerCategory.AutoSize = true;
            this.lblCritterPowerCategory.Location = new System.Drawing.Point(413, 55);
            this.lblCritterPowerCategory.Name = "lblCritterPowerCategory";
            this.lblCritterPowerCategory.Size = new System.Drawing.Size(55, 13);
            this.lblCritterPowerCategory.TabIndex = 19;
            this.lblCritterPowerCategory.Text = "[Category]";
            // 
            // lblCritterPowerCategoryLabel
            // 
            this.lblCritterPowerCategoryLabel.AutoSize = true;
            this.lblCritterPowerCategoryLabel.Location = new System.Drawing.Point(347, 55);
            this.lblCritterPowerCategoryLabel.Name = "lblCritterPowerCategoryLabel";
            this.lblCritterPowerCategoryLabel.Size = new System.Drawing.Size(52, 13);
            this.lblCritterPowerCategoryLabel.TabIndex = 18;
            this.lblCritterPowerCategoryLabel.Tag = "Label_Category";
            this.lblCritterPowerCategoryLabel.Text = "Category:";
            // 
            // lblCritterPowerName
            // 
            this.lblCritterPowerName.AutoSize = true;
            this.lblCritterPowerName.Location = new System.Drawing.Point(413, 32);
            this.lblCritterPowerName.Name = "lblCritterPowerName";
            this.lblCritterPowerName.Size = new System.Drawing.Size(41, 13);
            this.lblCritterPowerName.TabIndex = 17;
            this.lblCritterPowerName.Text = "[Name]";
            // 
            // lblCritterPowerNameLabel
            // 
            this.lblCritterPowerNameLabel.AutoSize = true;
            this.lblCritterPowerNameLabel.Location = new System.Drawing.Point(347, 32);
            this.lblCritterPowerNameLabel.Name = "lblCritterPowerNameLabel";
            this.lblCritterPowerNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblCritterPowerNameLabel.TabIndex = 16;
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
            treeNode16.Name = "nodCritterPowerRoot";
            treeNode16.Tag = "Node_CritterPowers";
            treeNode16.Text = "Critter Powers";
            treeNode17.Name = "nodCritterWeaknessRoot";
            treeNode17.Tag = "Node_CritterWeaknesses";
            treeNode17.Text = "Weaknesses";
            this.treCritterPowers.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode16,
            treeNode17});
            this.treCritterPowers.ShowNodeToolTips = true;
            this.treCritterPowers.ShowPlusMinus = false;
            this.treCritterPowers.ShowRootLines = false;
            this.treCritterPowers.Size = new System.Drawing.Size(333, 551);
            this.treCritterPowers.TabIndex = 15;
            this.treCritterPowers.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treCritterPowers_AfterSelect);
            this.treCritterPowers.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treCritterPowers_KeyDown);
            this.treCritterPowers.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
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
            this.tabAdvancedPrograms.Size = new System.Drawing.Size(861, 586);
            this.tabAdvancedPrograms.TabIndex = 19;
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
            treeNode18.Name = "nodAIProgramsRoot";
            treeNode18.Tag = "Node_SelectedAIPrograms";
            treeNode18.Text = "Selected AI Programs and Advanced Programs";
            this.treAIPrograms.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode18});
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
            // tabInitiation
            // 
            this.tabInitiation.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabInitiation.Controls.Add(this.chkInitiationSchooling);
            this.tabInitiation.Controls.Add(this.chkInitiationOrdeal);
            this.tabInitiation.Controls.Add(this.chkInitiationGroup);
            this.tabInitiation.Controls.Add(this.chkJoinGroup);
            this.tabInitiation.Controls.Add(this.txtGroupNotes);
            this.tabInitiation.Controls.Add(this.txtGroupName);
            this.tabInitiation.Controls.Add(this.lblGroupNotes);
            this.tabInitiation.Controls.Add(this.lblGroupName);
            this.tabInitiation.Controls.Add(this.lblMetamagicSource);
            this.tabInitiation.Controls.Add(this.lblMetamagicSourceLabel);
            this.tabInitiation.Controls.Add(this.treMetamagic);
            this.tabInitiation.Controls.Add(this.cmdAddMetamagic);
            this.tabInitiation.Location = new System.Drawing.Point(4, 22);
            this.tabInitiation.Name = "tabInitiation";
            this.tabInitiation.Padding = new System.Windows.Forms.Padding(3);
            this.tabInitiation.Size = new System.Drawing.Size(861, 586);
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
            this.chkInitiationSchooling.TabIndex = 128;
            this.chkInitiationSchooling.Tag = "Checkbox_InitiationSchooling";
            this.chkInitiationSchooling.Text = "Schooling (-10%)";
            this.chkInitiationSchooling.UseVisualStyleBackColor = true;
            this.chkInitiationSchooling.CheckedChanged += new System.EventHandler(this.chkInitiationSchooling_CheckedChanged);
            // 
            // chkInitiationOrdeal
            // 
            this.chkInitiationOrdeal.AutoSize = true;
            this.chkInitiationOrdeal.Location = new System.Drawing.Point(8, 6);
            this.chkInitiationOrdeal.Name = "chkInitiationOrdeal";
            this.chkInitiationOrdeal.Size = new System.Drawing.Size(131, 17);
            this.chkInitiationOrdeal.TabIndex = 127;
            this.chkInitiationOrdeal.Tag = "Checkbox_InitiationOrdeal";
            this.chkInitiationOrdeal.Text = "Initiatory Ordeal (-10%)";
            this.chkInitiationOrdeal.UseVisualStyleBackColor = true;
            // 
            // chkInitiationGroup
            // 
            this.chkInitiationGroup.AutoSize = true;
            this.chkInitiationGroup.Location = new System.Drawing.Point(8, 29);
            this.chkInitiationGroup.Name = "chkInitiationGroup";
            this.chkInitiationGroup.Size = new System.Drawing.Size(129, 17);
            this.chkInitiationGroup.TabIndex = 126;
            this.chkInitiationGroup.Tag = "Checkbox_GroupInitiation";
            this.chkInitiationGroup.Text = "Group Initiation (-10%)";
            this.chkInitiationGroup.UseVisualStyleBackColor = true;
            // 
            // chkJoinGroup
            // 
            this.chkJoinGroup.AutoSize = true;
            this.chkJoinGroup.Location = new System.Drawing.Point(365, 508);
            this.chkJoinGroup.Name = "chkJoinGroup";
            this.chkJoinGroup.Size = new System.Drawing.Size(77, 17);
            this.chkJoinGroup.TabIndex = 125;
            this.chkJoinGroup.Tag = "Checkbox_JoinedGroup";
            this.chkJoinGroup.Text = "Join Group";
            this.chkJoinGroup.UseVisualStyleBackColor = true;
            this.chkJoinGroup.Visible = false;
            this.chkJoinGroup.CheckedChanged += new System.EventHandler(this.chkJoinGroup_CheckedChanged);
            // 
            // txtGroupNotes
            // 
            this.txtGroupNotes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.txtGroupNotes.Location = new System.Drawing.Point(418, 554);
            this.txtGroupNotes.Multiline = true;
            this.txtGroupNotes.Name = "txtGroupNotes";
            this.txtGroupNotes.Size = new System.Drawing.Size(414, 16);
            this.txtGroupNotes.TabIndex = 117;
            this.txtGroupNotes.Visible = false;
            this.txtGroupNotes.TextChanged += new System.EventHandler(this.txtGroupNotes_TextChanged);
            this.txtGroupNotes.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtNotes_KeyDown);
            // 
            // txtGroupName
            // 
            this.txtGroupName.Location = new System.Drawing.Point(418, 531);
            this.txtGroupName.Name = "txtGroupName";
            this.txtGroupName.Size = new System.Drawing.Size(414, 20);
            this.txtGroupName.TabIndex = 115;
            this.txtGroupName.Visible = false;
            this.txtGroupName.TextChanged += new System.EventHandler(this.txtGroupName_TextChanged);
            // 
            // lblGroupNotes
            // 
            this.lblGroupNotes.AutoSize = true;
            this.lblGroupNotes.Location = new System.Drawing.Point(362, 557);
            this.lblGroupNotes.Name = "lblGroupNotes";
            this.lblGroupNotes.Size = new System.Drawing.Size(38, 13);
            this.lblGroupNotes.TabIndex = 116;
            this.lblGroupNotes.Tag = "Label_Notes";
            this.lblGroupNotes.Text = "Notes:";
            this.lblGroupNotes.Visible = false;
            // 
            // lblGroupName
            // 
            this.lblGroupName.AutoSize = true;
            this.lblGroupName.Location = new System.Drawing.Point(362, 534);
            this.lblGroupName.Name = "lblGroupName";
            this.lblGroupName.Size = new System.Drawing.Size(39, 13);
            this.lblGroupName.TabIndex = 114;
            this.lblGroupName.Tag = "Label_Group";
            this.lblGroupName.Text = "Group:";
            this.lblGroupName.Visible = false;
            // 
            // lblMetamagicSource
            // 
            this.lblMetamagicSource.AutoSize = true;
            this.lblMetamagicSource.Location = new System.Drawing.Point(434, 77);
            this.lblMetamagicSource.Name = "lblMetamagicSource";
            this.lblMetamagicSource.Size = new System.Drawing.Size(47, 13);
            this.lblMetamagicSource.TabIndex = 111;
            this.lblMetamagicSource.Text = "[Source]";
            this.lblMetamagicSource.Click += new System.EventHandler(this.lblMetamagicSource_Click);
            // 
            // lblMetamagicSourceLabel
            // 
            this.lblMetamagicSourceLabel.AutoSize = true;
            this.lblMetamagicSourceLabel.Location = new System.Drawing.Point(365, 77);
            this.lblMetamagicSourceLabel.Name = "lblMetamagicSourceLabel";
            this.lblMetamagicSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblMetamagicSourceLabel.TabIndex = 110;
            this.lblMetamagicSourceLabel.Tag = "Label_Source";
            this.lblMetamagicSourceLabel.Text = "Source:";
            // 
            // treMetamagic
            // 
            this.treMetamagic.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treMetamagic.HideSelection = false;
            this.treMetamagic.Location = new System.Drawing.Point(8, 77);
            this.treMetamagic.Name = "treMetamagic";
            this.treMetamagic.ShowNodeToolTips = true;
            this.treMetamagic.Size = new System.Drawing.Size(351, 503);
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
            this.tabCyberware.Controls.Add(this.lblCyberlimbSTR);
            this.tabCyberware.Controls.Add(this.lblCyberlimbAGI);
            this.tabCyberware.Controls.Add(this.lblCyberlimbSTRLabel);
            this.tabCyberware.Controls.Add(this.lblCyberlimbAGILabel);
            this.tabCyberware.Controls.Add(this.cboCyberwareGearOverclocker);
            this.tabCyberware.Controls.Add(this.lblCyberwareGearOverclocker);
            this.tabCyberware.Controls.Add(this.cboCyberwareGearDataProcessing);
            this.tabCyberware.Controls.Add(this.cboCyberwareGearFirewall);
            this.tabCyberware.Controls.Add(this.cboCyberwareGearSleaze);
            this.tabCyberware.Controls.Add(this.cboCyberwareGearAttack);
            this.tabCyberware.Controls.Add(this.tabCyberwareCM);
            this.tabCyberware.Controls.Add(this.lblCyberFirewallLabel);
            this.tabCyberware.Controls.Add(this.lblCyberDataProcessingLabel);
            this.tabCyberware.Controls.Add(this.lblCyberSleazeLabel);
            this.tabCyberware.Controls.Add(this.lblCyberAttackLabel);
            this.tabCyberware.Controls.Add(this.lblCyberDeviceRating);
            this.tabCyberware.Controls.Add(this.lblCyberDeviceRatingLabel);
            this.tabCyberware.Controls.Add(this.lblEssenceHoleESS);
            this.tabCyberware.Controls.Add(this.lblEssenceHoleESSLabel);
            this.tabCyberware.Controls.Add(this.label2);
            this.tabCyberware.Controls.Add(this.lblBiowareESS);
            this.tabCyberware.Controls.Add(this.lblCyberwareESS);
            this.tabCyberware.Controls.Add(this.lblBiowareESSLabel);
            this.tabCyberware.Controls.Add(this.lblCyberwareESSLabel);
            this.tabCyberware.Controls.Add(this.lblCyberwareRating);
            this.tabCyberware.Controls.Add(this.lblCyberwareGrade);
            this.tabCyberware.Controls.Add(this.lblCyberwareSource);
            this.tabCyberware.Controls.Add(this.lblCyberwareSourceLabel);
            this.tabCyberware.Controls.Add(this.cmdAddBioware);
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
            this.tabCyberware.Controls.Add(this.cmdDeleteCyberware);
            this.tabCyberware.Location = new System.Drawing.Point(4, 22);
            this.tabCyberware.Name = "tabCyberware";
            this.tabCyberware.Size = new System.Drawing.Size(861, 586);
            this.tabCyberware.TabIndex = 4;
            this.tabCyberware.Tag = "Tab_Cyberware";
            this.tabCyberware.Text = "Cyberware and Bioware";
            // 
            // lblCyberlimbSTR
            // 
            this.lblCyberlimbSTR.AutoSize = true;
            this.lblCyberlimbSTR.Location = new System.Drawing.Point(682, 174);
            this.lblCyberlimbSTR.Name = "lblCyberlimbSTR";
            this.lblCyberlimbSTR.Size = new System.Drawing.Size(19, 13);
            this.lblCyberlimbSTR.TabIndex = 220;
            this.lblCyberlimbSTR.Text = "[0]";
            this.lblCyberlimbSTR.Visible = false;
            // 
            // lblCyberlimbAGI
            // 
            this.lblCyberlimbAGI.AutoSize = true;
            this.lblCyberlimbAGI.Location = new System.Drawing.Point(682, 151);
            this.lblCyberlimbAGI.Name = "lblCyberlimbAGI";
            this.lblCyberlimbAGI.Size = new System.Drawing.Size(19, 13);
            this.lblCyberlimbAGI.TabIndex = 219;
            this.lblCyberlimbAGI.Text = "[0]";
            this.lblCyberlimbAGI.Visible = false;
            // 
            // lblCyberlimbSTRLabel
            // 
            this.lblCyberlimbSTRLabel.Location = new System.Drawing.Point(595, 174);
            this.lblCyberlimbSTRLabel.Name = "lblCyberlimbSTRLabel";
            this.lblCyberlimbSTRLabel.Size = new System.Drawing.Size(81, 13);
            this.lblCyberlimbSTRLabel.TabIndex = 218;
            this.lblCyberlimbSTRLabel.Tag = "";
            this.lblCyberlimbSTRLabel.Text = "Strength (STR):";
            this.lblCyberlimbSTRLabel.Visible = false;
            // 
            // lblCyberlimbAGILabel
            // 
            this.lblCyberlimbAGILabel.AutoSize = true;
            this.lblCyberlimbAGILabel.Location = new System.Drawing.Point(595, 151);
            this.lblCyberlimbAGILabel.Name = "lblCyberlimbAGILabel";
            this.lblCyberlimbAGILabel.Size = new System.Drawing.Size(64, 13);
            this.lblCyberlimbAGILabel.TabIndex = 217;
            this.lblCyberlimbAGILabel.Tag = "";
            this.lblCyberlimbAGILabel.Text = "Agility (AGI):";
            this.lblCyberlimbAGILabel.Visible = false;
            // 
            // cboCyberwareGearOverclocker
            // 
            this.cboCyberwareGearOverclocker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCyberwareGearOverclocker.FormattingEnabled = true;
            this.cboCyberwareGearOverclocker.Location = new System.Drawing.Point(446, 273);
            this.cboCyberwareGearOverclocker.Name = "cboCyberwareGearOverclocker";
            this.cboCyberwareGearOverclocker.Size = new System.Drawing.Size(139, 21);
            this.cboCyberwareGearOverclocker.TabIndex = 216;
            this.cboCyberwareGearOverclocker.Visible = false;
            this.cboCyberwareGearOverclocker.SelectedIndexChanged += new System.EventHandler(this.cboCyberwareGearOverclocker_SelectedIndexChanged);
            // 
            // lblCyberwareGearOverclocker
            // 
            this.lblCyberwareGearOverclocker.Location = new System.Drawing.Point(446, 258);
            this.lblCyberwareGearOverclocker.Name = "lblCyberwareGearOverclocker";
            this.lblCyberwareGearOverclocker.Size = new System.Drawing.Size(87, 13);
            this.lblCyberwareGearOverclocker.TabIndex = 215;
            this.lblCyberwareGearOverclocker.Tag = "Label_Firewall";
            this.lblCyberwareGearOverclocker.Text = "Overclocker:";
            this.lblCyberwareGearOverclocker.Visible = false;
            // 
            // cboCyberwareGearDataProcessing
            // 
            this.cboCyberwareGearDataProcessing.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCyberwareGearDataProcessing.FormattingEnabled = true;
            this.cboCyberwareGearDataProcessing.Location = new System.Drawing.Point(673, 300);
            this.cboCyberwareGearDataProcessing.Name = "cboCyberwareGearDataProcessing";
            this.cboCyberwareGearDataProcessing.Size = new System.Drawing.Size(30, 21);
            this.cboCyberwareGearDataProcessing.TabIndex = 214;
            this.cboCyberwareGearDataProcessing.Visible = false;
            this.cboCyberwareGearDataProcessing.SelectedIndexChanged += new System.EventHandler(this.cboCyberwareGearDataProcessing_SelectedIndexChanged);
            // 
            // cboCyberwareGearFirewall
            // 
            this.cboCyberwareGearFirewall.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCyberwareGearFirewall.FormattingEnabled = true;
            this.cboCyberwareGearFirewall.Location = new System.Drawing.Point(750, 300);
            this.cboCyberwareGearFirewall.Name = "cboCyberwareGearFirewall";
            this.cboCyberwareGearFirewall.Size = new System.Drawing.Size(30, 21);
            this.cboCyberwareGearFirewall.TabIndex = 213;
            this.cboCyberwareGearFirewall.Visible = false;
            this.cboCyberwareGearFirewall.SelectedIndexChanged += new System.EventHandler(this.cboCyberwareGearFirewall_SelectedIndexChanged);
            // 
            // cboCyberwareGearSleaze
            // 
            this.cboCyberwareGearSleaze.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCyberwareGearSleaze.FormattingEnabled = true;
            this.cboCyberwareGearSleaze.Location = new System.Drawing.Point(555, 300);
            this.cboCyberwareGearSleaze.Name = "cboCyberwareGearSleaze";
            this.cboCyberwareGearSleaze.Size = new System.Drawing.Size(30, 21);
            this.cboCyberwareGearSleaze.TabIndex = 212;
            this.cboCyberwareGearSleaze.Visible = false;
            this.cboCyberwareGearSleaze.SelectedIndexChanged += new System.EventHandler(this.cboCyberwareGearSleaze_SelectedIndexChanged);
            // 
            // cboCyberwareGearAttack
            // 
            this.cboCyberwareGearAttack.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCyberwareGearAttack.FormattingEnabled = true;
            this.cboCyberwareGearAttack.Location = new System.Drawing.Point(481, 300);
            this.cboCyberwareGearAttack.Name = "cboCyberwareGearAttack";
            this.cboCyberwareGearAttack.Size = new System.Drawing.Size(30, 21);
            this.cboCyberwareGearAttack.TabIndex = 211;
            this.cboCyberwareGearAttack.Visible = false;
            this.cboCyberwareGearAttack.SelectedIndexChanged += new System.EventHandler(this.cboCyberwareGearAttack_SelectedIndexChanged);
            // 
            // tabCyberwareCM
            // 
            this.tabCyberwareCM.Controls.Add(this.tabCyberwareMatrixCM);
            this.tabCyberwareCM.ItemSize = new System.Drawing.Size(176, 18);
            this.tabCyberwareCM.Location = new System.Drawing.Point(312, 327);
            this.tabCyberwareCM.Name = "tabCyberwareCM";
            this.tabCyberwareCM.SelectedIndex = 0;
            this.tabCyberwareCM.Size = new System.Drawing.Size(360, 113);
            this.tabCyberwareCM.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabCyberwareCM.TabIndex = 203;
            // 
            // tabCyberwareMatrixCM
            // 
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM1);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM2);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM3);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM4);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM5);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM6);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM7);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM8);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM9);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM10);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM11);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM12);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM13);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM14);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM15);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM16);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM17);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM18);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM19);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM20);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM21);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM22);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM23);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM24);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM25);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM26);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM27);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM28);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM29);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM30);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM31);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM32);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM33);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM34);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM35);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM36);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM37);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM38);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM39);
            this.tabCyberwareMatrixCM.Controls.Add(this.chkCyberwareMatrixCM40);
            this.tabCyberwareMatrixCM.Location = new System.Drawing.Point(4, 22);
            this.tabCyberwareMatrixCM.Name = "tabCyberwareMatrixCM";
            this.tabCyberwareMatrixCM.Padding = new System.Windows.Forms.Padding(3);
            this.tabCyberwareMatrixCM.Size = new System.Drawing.Size(352, 87);
            this.tabCyberwareMatrixCM.TabIndex = 1;
            this.tabCyberwareMatrixCM.Text = "Matrix Condition Monitor";
            this.tabCyberwareMatrixCM.UseVisualStyleBackColor = true;
            // 
            // chkCyberwareMatrixCM1
            // 
            this.chkCyberwareMatrixCM1.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM1.Location = new System.Drawing.Point(5, 6);
            this.chkCyberwareMatrixCM1.Name = "chkCyberwareMatrixCM1";
            this.chkCyberwareMatrixCM1.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM1.TabIndex = 49;
            this.chkCyberwareMatrixCM1.Tag = "1";
            this.chkCyberwareMatrixCM1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM1.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM1.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM2
            // 
            this.chkCyberwareMatrixCM2.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM2.Location = new System.Drawing.Point(29, 6);
            this.chkCyberwareMatrixCM2.Name = "chkCyberwareMatrixCM2";
            this.chkCyberwareMatrixCM2.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM2.TabIndex = 50;
            this.chkCyberwareMatrixCM2.Tag = "2";
            this.chkCyberwareMatrixCM2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM2.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM2.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM3
            // 
            this.chkCyberwareMatrixCM3.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM3.Location = new System.Drawing.Point(53, 6);
            this.chkCyberwareMatrixCM3.Name = "chkCyberwareMatrixCM3";
            this.chkCyberwareMatrixCM3.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM3.TabIndex = 51;
            this.chkCyberwareMatrixCM3.Tag = "3";
            this.chkCyberwareMatrixCM3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM3.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM3.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM4
            // 
            this.chkCyberwareMatrixCM4.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM4.Location = new System.Drawing.Point(77, 6);
            this.chkCyberwareMatrixCM4.Name = "chkCyberwareMatrixCM4";
            this.chkCyberwareMatrixCM4.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM4.TabIndex = 52;
            this.chkCyberwareMatrixCM4.Tag = "4";
            this.chkCyberwareMatrixCM4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM4.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM4.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM5
            // 
            this.chkCyberwareMatrixCM5.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM5.Location = new System.Drawing.Point(101, 6);
            this.chkCyberwareMatrixCM5.Name = "chkCyberwareMatrixCM5";
            this.chkCyberwareMatrixCM5.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM5.TabIndex = 53;
            this.chkCyberwareMatrixCM5.Tag = "5";
            this.chkCyberwareMatrixCM5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM5.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM5.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM6
            // 
            this.chkCyberwareMatrixCM6.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM6.Location = new System.Drawing.Point(125, 6);
            this.chkCyberwareMatrixCM6.Name = "chkCyberwareMatrixCM6";
            this.chkCyberwareMatrixCM6.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM6.TabIndex = 54;
            this.chkCyberwareMatrixCM6.Tag = "6";
            this.chkCyberwareMatrixCM6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM6.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM6.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM7
            // 
            this.chkCyberwareMatrixCM7.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM7.Location = new System.Drawing.Point(149, 6);
            this.chkCyberwareMatrixCM7.Name = "chkCyberwareMatrixCM7";
            this.chkCyberwareMatrixCM7.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM7.TabIndex = 55;
            this.chkCyberwareMatrixCM7.Tag = "7";
            this.chkCyberwareMatrixCM7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM7.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM7.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM8
            // 
            this.chkCyberwareMatrixCM8.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM8.Location = new System.Drawing.Point(173, 6);
            this.chkCyberwareMatrixCM8.Name = "chkCyberwareMatrixCM8";
            this.chkCyberwareMatrixCM8.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM8.TabIndex = 56;
            this.chkCyberwareMatrixCM8.Tag = "8";
            this.chkCyberwareMatrixCM8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM8.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM8.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM9
            // 
            this.chkCyberwareMatrixCM9.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM9.Location = new System.Drawing.Point(197, 6);
            this.chkCyberwareMatrixCM9.Name = "chkCyberwareMatrixCM9";
            this.chkCyberwareMatrixCM9.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM9.TabIndex = 57;
            this.chkCyberwareMatrixCM9.Tag = "9";
            this.chkCyberwareMatrixCM9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM9.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM9.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM10
            // 
            this.chkCyberwareMatrixCM10.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM10.Location = new System.Drawing.Point(221, 6);
            this.chkCyberwareMatrixCM10.Name = "chkCyberwareMatrixCM10";
            this.chkCyberwareMatrixCM10.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM10.TabIndex = 58;
            this.chkCyberwareMatrixCM10.Tag = "10";
            this.chkCyberwareMatrixCM10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM10.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM10.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM11
            // 
            this.chkCyberwareMatrixCM11.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM11.Location = new System.Drawing.Point(245, 6);
            this.chkCyberwareMatrixCM11.Name = "chkCyberwareMatrixCM11";
            this.chkCyberwareMatrixCM11.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM11.TabIndex = 59;
            this.chkCyberwareMatrixCM11.Tag = "11";
            this.chkCyberwareMatrixCM11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM11.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM11.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM12
            // 
            this.chkCyberwareMatrixCM12.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM12.Location = new System.Drawing.Point(269, 6);
            this.chkCyberwareMatrixCM12.Name = "chkCyberwareMatrixCM12";
            this.chkCyberwareMatrixCM12.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM12.TabIndex = 60;
            this.chkCyberwareMatrixCM12.Tag = "12";
            this.chkCyberwareMatrixCM12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM12.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM12.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM13
            // 
            this.chkCyberwareMatrixCM13.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM13.Location = new System.Drawing.Point(293, 6);
            this.chkCyberwareMatrixCM13.Name = "chkCyberwareMatrixCM13";
            this.chkCyberwareMatrixCM13.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM13.TabIndex = 61;
            this.chkCyberwareMatrixCM13.Tag = "13";
            this.chkCyberwareMatrixCM13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM13.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM13.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM14
            // 
            this.chkCyberwareMatrixCM14.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM14.Location = new System.Drawing.Point(317, 6);
            this.chkCyberwareMatrixCM14.Name = "chkCyberwareMatrixCM14";
            this.chkCyberwareMatrixCM14.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM14.TabIndex = 62;
            this.chkCyberwareMatrixCM14.Tag = "14";
            this.chkCyberwareMatrixCM14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM14.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM14.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM15
            // 
            this.chkCyberwareMatrixCM15.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM15.Location = new System.Drawing.Point(5, 31);
            this.chkCyberwareMatrixCM15.Name = "chkCyberwareMatrixCM15";
            this.chkCyberwareMatrixCM15.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM15.TabIndex = 63;
            this.chkCyberwareMatrixCM15.Tag = "15";
            this.chkCyberwareMatrixCM15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM15.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM15.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM16
            // 
            this.chkCyberwareMatrixCM16.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM16.Location = new System.Drawing.Point(29, 31);
            this.chkCyberwareMatrixCM16.Name = "chkCyberwareMatrixCM16";
            this.chkCyberwareMatrixCM16.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM16.TabIndex = 64;
            this.chkCyberwareMatrixCM16.Tag = "16";
            this.chkCyberwareMatrixCM16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM16.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM16.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM17
            // 
            this.chkCyberwareMatrixCM17.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM17.Location = new System.Drawing.Point(53, 31);
            this.chkCyberwareMatrixCM17.Name = "chkCyberwareMatrixCM17";
            this.chkCyberwareMatrixCM17.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM17.TabIndex = 65;
            this.chkCyberwareMatrixCM17.Tag = "17";
            this.chkCyberwareMatrixCM17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM17.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM17.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM18
            // 
            this.chkCyberwareMatrixCM18.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM18.Location = new System.Drawing.Point(77, 31);
            this.chkCyberwareMatrixCM18.Name = "chkCyberwareMatrixCM18";
            this.chkCyberwareMatrixCM18.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM18.TabIndex = 66;
            this.chkCyberwareMatrixCM18.Tag = "18";
            this.chkCyberwareMatrixCM18.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM18.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM18.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM19
            // 
            this.chkCyberwareMatrixCM19.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM19.Location = new System.Drawing.Point(101, 31);
            this.chkCyberwareMatrixCM19.Name = "chkCyberwareMatrixCM19";
            this.chkCyberwareMatrixCM19.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM19.TabIndex = 67;
            this.chkCyberwareMatrixCM19.Tag = "19";
            this.chkCyberwareMatrixCM19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM19.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM19.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM20
            // 
            this.chkCyberwareMatrixCM20.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM20.Location = new System.Drawing.Point(125, 31);
            this.chkCyberwareMatrixCM20.Name = "chkCyberwareMatrixCM20";
            this.chkCyberwareMatrixCM20.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM20.TabIndex = 68;
            this.chkCyberwareMatrixCM20.Tag = "20";
            this.chkCyberwareMatrixCM20.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM20.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM20.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM21
            // 
            this.chkCyberwareMatrixCM21.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM21.Location = new System.Drawing.Point(149, 31);
            this.chkCyberwareMatrixCM21.Name = "chkCyberwareMatrixCM21";
            this.chkCyberwareMatrixCM21.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM21.TabIndex = 69;
            this.chkCyberwareMatrixCM21.Tag = "21";
            this.chkCyberwareMatrixCM21.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM21.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM21.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM22
            // 
            this.chkCyberwareMatrixCM22.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM22.Location = new System.Drawing.Point(173, 31);
            this.chkCyberwareMatrixCM22.Name = "chkCyberwareMatrixCM22";
            this.chkCyberwareMatrixCM22.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM22.TabIndex = 70;
            this.chkCyberwareMatrixCM22.Tag = "22";
            this.chkCyberwareMatrixCM22.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM22.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM22.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM23
            // 
            this.chkCyberwareMatrixCM23.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM23.Location = new System.Drawing.Point(197, 31);
            this.chkCyberwareMatrixCM23.Name = "chkCyberwareMatrixCM23";
            this.chkCyberwareMatrixCM23.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM23.TabIndex = 71;
            this.chkCyberwareMatrixCM23.Tag = "23";
            this.chkCyberwareMatrixCM23.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM23.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM23.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM24
            // 
            this.chkCyberwareMatrixCM24.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM24.Location = new System.Drawing.Point(221, 31);
            this.chkCyberwareMatrixCM24.Name = "chkCyberwareMatrixCM24";
            this.chkCyberwareMatrixCM24.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM24.TabIndex = 72;
            this.chkCyberwareMatrixCM24.Tag = "24";
            this.chkCyberwareMatrixCM24.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM24.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM24.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM25
            // 
            this.chkCyberwareMatrixCM25.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM25.Location = new System.Drawing.Point(245, 31);
            this.chkCyberwareMatrixCM25.Name = "chkCyberwareMatrixCM25";
            this.chkCyberwareMatrixCM25.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM25.TabIndex = 73;
            this.chkCyberwareMatrixCM25.Tag = "25";
            this.chkCyberwareMatrixCM25.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM25.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM25.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM26
            // 
            this.chkCyberwareMatrixCM26.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM26.Location = new System.Drawing.Point(269, 31);
            this.chkCyberwareMatrixCM26.Name = "chkCyberwareMatrixCM26";
            this.chkCyberwareMatrixCM26.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM26.TabIndex = 74;
            this.chkCyberwareMatrixCM26.Tag = "26";
            this.chkCyberwareMatrixCM26.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM26.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM26.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM27
            // 
            this.chkCyberwareMatrixCM27.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM27.Location = new System.Drawing.Point(293, 31);
            this.chkCyberwareMatrixCM27.Name = "chkCyberwareMatrixCM27";
            this.chkCyberwareMatrixCM27.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM27.TabIndex = 75;
            this.chkCyberwareMatrixCM27.Tag = "27";
            this.chkCyberwareMatrixCM27.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM27.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM27.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM28
            // 
            this.chkCyberwareMatrixCM28.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM28.Location = new System.Drawing.Point(317, 31);
            this.chkCyberwareMatrixCM28.Name = "chkCyberwareMatrixCM28";
            this.chkCyberwareMatrixCM28.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM28.TabIndex = 76;
            this.chkCyberwareMatrixCM28.Tag = "28";
            this.chkCyberwareMatrixCM28.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM28.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM28.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM29
            // 
            this.chkCyberwareMatrixCM29.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM29.Location = new System.Drawing.Point(5, 55);
            this.chkCyberwareMatrixCM29.Name = "chkCyberwareMatrixCM29";
            this.chkCyberwareMatrixCM29.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM29.TabIndex = 77;
            this.chkCyberwareMatrixCM29.Tag = "29";
            this.chkCyberwareMatrixCM29.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM29.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM29.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM30
            // 
            this.chkCyberwareMatrixCM30.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM30.Location = new System.Drawing.Point(29, 55);
            this.chkCyberwareMatrixCM30.Name = "chkCyberwareMatrixCM30";
            this.chkCyberwareMatrixCM30.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM30.TabIndex = 78;
            this.chkCyberwareMatrixCM30.Tag = "30";
            this.chkCyberwareMatrixCM30.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM30.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM30.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM31
            // 
            this.chkCyberwareMatrixCM31.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM31.Location = new System.Drawing.Point(53, 55);
            this.chkCyberwareMatrixCM31.Name = "chkCyberwareMatrixCM31";
            this.chkCyberwareMatrixCM31.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM31.TabIndex = 79;
            this.chkCyberwareMatrixCM31.Tag = "31";
            this.chkCyberwareMatrixCM31.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM31.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM31.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM32
            // 
            this.chkCyberwareMatrixCM32.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM32.Location = new System.Drawing.Point(77, 55);
            this.chkCyberwareMatrixCM32.Name = "chkCyberwareMatrixCM32";
            this.chkCyberwareMatrixCM32.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM32.TabIndex = 80;
            this.chkCyberwareMatrixCM32.Tag = "32";
            this.chkCyberwareMatrixCM32.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM32.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM32.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM33
            // 
            this.chkCyberwareMatrixCM33.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM33.Location = new System.Drawing.Point(101, 55);
            this.chkCyberwareMatrixCM33.Name = "chkCyberwareMatrixCM33";
            this.chkCyberwareMatrixCM33.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM33.TabIndex = 81;
            this.chkCyberwareMatrixCM33.Tag = "33";
            this.chkCyberwareMatrixCM33.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM33.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM33.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM34
            // 
            this.chkCyberwareMatrixCM34.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM34.Location = new System.Drawing.Point(125, 55);
            this.chkCyberwareMatrixCM34.Name = "chkCyberwareMatrixCM34";
            this.chkCyberwareMatrixCM34.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM34.TabIndex = 82;
            this.chkCyberwareMatrixCM34.Tag = "34";
            this.chkCyberwareMatrixCM34.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM34.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM34.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM35
            // 
            this.chkCyberwareMatrixCM35.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM35.Location = new System.Drawing.Point(149, 55);
            this.chkCyberwareMatrixCM35.Name = "chkCyberwareMatrixCM35";
            this.chkCyberwareMatrixCM35.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM35.TabIndex = 83;
            this.chkCyberwareMatrixCM35.Tag = "35";
            this.chkCyberwareMatrixCM35.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM35.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM35.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM36
            // 
            this.chkCyberwareMatrixCM36.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM36.Location = new System.Drawing.Point(173, 55);
            this.chkCyberwareMatrixCM36.Name = "chkCyberwareMatrixCM36";
            this.chkCyberwareMatrixCM36.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM36.TabIndex = 84;
            this.chkCyberwareMatrixCM36.Tag = "36";
            this.chkCyberwareMatrixCM36.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM36.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM36.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM37
            // 
            this.chkCyberwareMatrixCM37.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM37.Location = new System.Drawing.Point(197, 55);
            this.chkCyberwareMatrixCM37.Name = "chkCyberwareMatrixCM37";
            this.chkCyberwareMatrixCM37.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM37.TabIndex = 85;
            this.chkCyberwareMatrixCM37.Tag = "37";
            this.chkCyberwareMatrixCM37.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM37.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM37.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM38
            // 
            this.chkCyberwareMatrixCM38.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM38.Location = new System.Drawing.Point(221, 55);
            this.chkCyberwareMatrixCM38.Name = "chkCyberwareMatrixCM38";
            this.chkCyberwareMatrixCM38.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM38.TabIndex = 86;
            this.chkCyberwareMatrixCM38.Tag = "38";
            this.chkCyberwareMatrixCM38.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM38.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM38.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM39
            // 
            this.chkCyberwareMatrixCM39.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM39.Location = new System.Drawing.Point(245, 55);
            this.chkCyberwareMatrixCM39.Name = "chkCyberwareMatrixCM39";
            this.chkCyberwareMatrixCM39.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM39.TabIndex = 87;
            this.chkCyberwareMatrixCM39.Tag = "39";
            this.chkCyberwareMatrixCM39.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM39.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM39.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // chkCyberwareMatrixCM40
            // 
            this.chkCyberwareMatrixCM40.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkCyberwareMatrixCM40.Location = new System.Drawing.Point(269, 55);
            this.chkCyberwareMatrixCM40.Name = "chkCyberwareMatrixCM40";
            this.chkCyberwareMatrixCM40.Size = new System.Drawing.Size(24, 24);
            this.chkCyberwareMatrixCM40.TabIndex = 88;
            this.chkCyberwareMatrixCM40.Tag = "40";
            this.chkCyberwareMatrixCM40.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCyberwareMatrixCM40.UseVisualStyleBackColor = true;
            this.chkCyberwareMatrixCM40.CheckedChanged += new System.EventHandler(this.chkCyberwareCM_CheckedChanged);
            // 
            // lblCyberFirewallLabel
            // 
            this.lblCyberFirewallLabel.AutoSize = true;
            this.lblCyberFirewallLabel.Location = new System.Drawing.Point(706, 304);
            this.lblCyberFirewallLabel.Name = "lblCyberFirewallLabel";
            this.lblCyberFirewallLabel.Size = new System.Drawing.Size(45, 13);
            this.lblCyberFirewallLabel.TabIndex = 184;
            this.lblCyberFirewallLabel.Tag = "Label_Firewall";
            this.lblCyberFirewallLabel.Text = "Firewall:";
            this.lblCyberFirewallLabel.Visible = false;
            // 
            // lblCyberDataProcessingLabel
            // 
            this.lblCyberDataProcessingLabel.AutoSize = true;
            this.lblCyberDataProcessingLabel.Location = new System.Drawing.Point(587, 304);
            this.lblCyberDataProcessingLabel.Name = "lblCyberDataProcessingLabel";
            this.lblCyberDataProcessingLabel.Size = new System.Drawing.Size(88, 13);
            this.lblCyberDataProcessingLabel.TabIndex = 182;
            this.lblCyberDataProcessingLabel.Tag = "Label_DataProcessing";
            this.lblCyberDataProcessingLabel.Text = "Data Processing:";
            this.lblCyberDataProcessingLabel.Visible = false;
            // 
            // lblCyberSleazeLabel
            // 
            this.lblCyberSleazeLabel.AutoSize = true;
            this.lblCyberSleazeLabel.Location = new System.Drawing.Point(515, 304);
            this.lblCyberSleazeLabel.Name = "lblCyberSleazeLabel";
            this.lblCyberSleazeLabel.Size = new System.Drawing.Size(42, 13);
            this.lblCyberSleazeLabel.TabIndex = 180;
            this.lblCyberSleazeLabel.Tag = "Label_Sleaze";
            this.lblCyberSleazeLabel.Text = "Sleaze:";
            this.lblCyberSleazeLabel.Visible = false;
            // 
            // lblCyberAttackLabel
            // 
            this.lblCyberAttackLabel.AutoSize = true;
            this.lblCyberAttackLabel.Location = new System.Drawing.Point(443, 304);
            this.lblCyberAttackLabel.Name = "lblCyberAttackLabel";
            this.lblCyberAttackLabel.Size = new System.Drawing.Size(41, 13);
            this.lblCyberAttackLabel.TabIndex = 178;
            this.lblCyberAttackLabel.Tag = "Label_Attack";
            this.lblCyberAttackLabel.Text = "Attack:";
            this.lblCyberAttackLabel.Visible = false;
            // 
            // lblCyberDeviceRating
            // 
            this.lblCyberDeviceRating.AutoSize = true;
            this.lblCyberDeviceRating.Location = new System.Drawing.Point(409, 304);
            this.lblCyberDeviceRating.Name = "lblCyberDeviceRating";
            this.lblCyberDeviceRating.Size = new System.Drawing.Size(19, 13);
            this.lblCyberDeviceRating.TabIndex = 177;
            this.lblCyberDeviceRating.Text = "[0]";
            this.lblCyberDeviceRating.Visible = false;
            // 
            // lblCyberDeviceRatingLabel
            // 
            this.lblCyberDeviceRatingLabel.AutoSize = true;
            this.lblCyberDeviceRatingLabel.Location = new System.Drawing.Point(310, 304);
            this.lblCyberDeviceRatingLabel.Name = "lblCyberDeviceRatingLabel";
            this.lblCyberDeviceRatingLabel.Size = new System.Drawing.Size(78, 13);
            this.lblCyberDeviceRatingLabel.TabIndex = 176;
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
            this.lblEssenceHoleESS.TabIndex = 68;
            this.lblEssenceHoleESS.Text = "[0]";
            // 
            // lblEssenceHoleESSLabel
            // 
            this.lblEssenceHoleESSLabel.AutoSize = true;
            this.lblEssenceHoleESSLabel.Location = new System.Drawing.Point(310, 269);
            this.lblEssenceHoleESSLabel.Name = "lblEssenceHoleESSLabel";
            this.lblEssenceHoleESSLabel.Size = new System.Drawing.Size(76, 13);
            this.lblEssenceHoleESSLabel.TabIndex = 67;
            this.lblEssenceHoleESSLabel.Tag = "Label_EssenceHole";
            this.lblEssenceHoleESSLabel.Text = "Essence Hole:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(310, 200);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 13);
            this.label2.TabIndex = 66;
            this.label2.Tag = "Label_EssenceConsumption";
            this.label2.Text = "Essence Consumption";
            // 
            // lblBiowareESS
            // 
            this.lblBiowareESS.AutoSize = true;
            this.lblBiowareESS.Location = new System.Drawing.Point(387, 246);
            this.lblBiowareESS.Name = "lblBiowareESS";
            this.lblBiowareESS.Size = new System.Drawing.Size(19, 13);
            this.lblBiowareESS.TabIndex = 65;
            this.lblBiowareESS.Text = "[0]";
            // 
            // lblCyberwareESS
            // 
            this.lblCyberwareESS.AutoSize = true;
            this.lblCyberwareESS.Location = new System.Drawing.Point(387, 223);
            this.lblCyberwareESS.Name = "lblCyberwareESS";
            this.lblCyberwareESS.Size = new System.Drawing.Size(19, 13);
            this.lblCyberwareESS.TabIndex = 64;
            this.lblCyberwareESS.Text = "[0]";
            // 
            // lblBiowareESSLabel
            // 
            this.lblBiowareESSLabel.AutoSize = true;
            this.lblBiowareESSLabel.Location = new System.Drawing.Point(310, 246);
            this.lblBiowareESSLabel.Name = "lblBiowareESSLabel";
            this.lblBiowareESSLabel.Size = new System.Drawing.Size(48, 13);
            this.lblBiowareESSLabel.TabIndex = 63;
            this.lblBiowareESSLabel.Tag = "Label_Bioware";
            this.lblBiowareESSLabel.Text = "Bioware:";
            // 
            // lblCyberwareESSLabel
            // 
            this.lblCyberwareESSLabel.AutoSize = true;
            this.lblCyberwareESSLabel.Location = new System.Drawing.Point(310, 223);
            this.lblCyberwareESSLabel.Name = "lblCyberwareESSLabel";
            this.lblCyberwareESSLabel.Size = new System.Drawing.Size(60, 13);
            this.lblCyberwareESSLabel.TabIndex = 62;
            this.lblCyberwareESSLabel.Tag = "Label_Cyberware";
            this.lblCyberwareESSLabel.Text = "Cyberware:";
            // 
            // lblCyberwareRating
            // 
            this.lblCyberwareRating.AutoSize = true;
            this.lblCyberwareRating.Location = new System.Drawing.Point(660, 82);
            this.lblCyberwareRating.Name = "lblCyberwareRating";
            this.lblCyberwareRating.Size = new System.Drawing.Size(44, 13);
            this.lblCyberwareRating.TabIndex = 50;
            this.lblCyberwareRating.Text = "[Rating]";
            // 
            // lblCyberwareGrade
            // 
            this.lblCyberwareGrade.AutoSize = true;
            this.lblCyberwareGrade.Location = new System.Drawing.Point(387, 82);
            this.lblCyberwareGrade.Name = "lblCyberwareGrade";
            this.lblCyberwareGrade.Size = new System.Drawing.Size(42, 13);
            this.lblCyberwareGrade.TabIndex = 49;
            this.lblCyberwareGrade.Text = "[Grade]";
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
            this.cmdAddBioware.Location = new System.Drawing.Point(121, 7);
            this.cmdAddBioware.Name = "cmdAddBioware";
            this.cmdAddBioware.Size = new System.Drawing.Size(90, 23);
            this.cmdAddBioware.TabIndex = 46;
            this.cmdAddBioware.Tag = "Button_AddBioware";
            this.cmdAddBioware.Text = "A&dd Bioware";
            this.cmdAddBioware.UseVisualStyleBackColor = true;
            this.cmdAddBioware.Click += new System.EventHandler(this.cmdAddBioware_Click);
            // 
            // lblCyberwareRatingLabel
            // 
            this.lblCyberwareRatingLabel.AutoSize = true;
            this.lblCyberwareRatingLabel.Location = new System.Drawing.Point(595, 82);
            this.lblCyberwareRatingLabel.Name = "lblCyberwareRatingLabel";
            this.lblCyberwareRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblCyberwareRatingLabel.TabIndex = 42;
            this.lblCyberwareRatingLabel.Tag = "Label_Rating";
            this.lblCyberwareRatingLabel.Text = "Rating:";
            // 
            // lblCyberwareCost
            // 
            this.lblCyberwareCost.AutoSize = true;
            this.lblCyberwareCost.Location = new System.Drawing.Point(660, 128);
            this.lblCyberwareCost.Name = "lblCyberwareCost";
            this.lblCyberwareCost.Size = new System.Drawing.Size(34, 13);
            this.lblCyberwareCost.TabIndex = 41;
            this.lblCyberwareCost.Text = "[Cost]";
            // 
            // lblCyberwareCostLabel
            // 
            this.lblCyberwareCostLabel.AutoSize = true;
            this.lblCyberwareCostLabel.Location = new System.Drawing.Point(595, 128);
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
            this.lblCyberwareCapacity.Location = new System.Drawing.Point(660, 105);
            this.lblCyberwareCapacity.Name = "lblCyberwareCapacity";
            this.lblCyberwareCapacity.Size = new System.Drawing.Size(54, 13);
            this.lblCyberwareCapacity.TabIndex = 36;
            this.lblCyberwareCapacity.Text = "[Capacity]";
            // 
            // lblCyberwareCapacityLabel
            // 
            this.lblCyberwareCapacityLabel.AutoSize = true;
            this.lblCyberwareCapacityLabel.Location = new System.Drawing.Point(595, 105);
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
            this.treCyberware.Size = new System.Drawing.Size(295, 547);
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
            this.cmdAddCyberware.TabIndex = 52;
            this.cmdAddCyberware.Tag = "Button_AddCyberware";
            this.cmdAddCyberware.Text = "&Add Cyberware";
            this.cmdAddCyberware.UseVisualStyleBackColor = true;
            this.cmdAddCyberware.Click += new System.EventHandler(this.cmdAddCyberware_Click);
            // 
            // cmdDeleteCyberware
            // 
            this.cmdDeleteCyberware.AutoSize = true;
            this.cmdDeleteCyberware.ContextMenuStrip = this.cmsDeleteCyberware;
            this.cmdDeleteCyberware.Location = new System.Drawing.Point(217, 7);
            this.cmdDeleteCyberware.Name = "cmdDeleteCyberware";
            this.cmdDeleteCyberware.Size = new System.Drawing.Size(80, 23);
            this.cmdDeleteCyberware.SplitMenuStrip = this.cmsDeleteCyberware;
            this.cmdDeleteCyberware.TabIndex = 51;
            this.cmdDeleteCyberware.Tag = "String_Delete";
            this.cmdDeleteCyberware.Text = "Delete";
            this.cmdDeleteCyberware.UseVisualStyleBackColor = true;
            this.cmdDeleteCyberware.Click += new System.EventHandler(this.cmdDeleteCyberware_Click);
            // 
            // tabStreetGear
            // 
            this.tabStreetGear.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabStreetGear.Controls.Add(this.tabStreetGearTabs);
            this.tabStreetGear.Location = new System.Drawing.Point(4, 22);
            this.tabStreetGear.Name = "tabStreetGear";
            this.tabStreetGear.Size = new System.Drawing.Size(861, 586);
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
            this.tabStreetGearTabs.Size = new System.Drawing.Size(859, 580);
            this.tabStreetGearTabs.TabIndex = 87;
            this.tabStreetGearTabs.SelectedIndexChanged += new System.EventHandler(this.tabStreetGearTabs_SelectedIndexChanged);
            // 
            // tabLifestyle
            // 
            this.tabLifestyle.BackColor = System.Drawing.SystemColors.Control;
            this.tabLifestyle.Controls.Add(this.cmdAddLifestyle);
            this.tabLifestyle.Controls.Add(this.lblBaseLifestyle);
            this.tabLifestyle.Controls.Add(this.lblLifestyleComfortsLabel);
            this.tabLifestyle.Controls.Add(this.lblLifestyleQualities);
            this.tabLifestyle.Controls.Add(this.lblLifestyleQualitiesLabel);
            this.tabLifestyle.Controls.Add(this.cmdIncreaseLifestyleMonths);
            this.tabLifestyle.Controls.Add(this.cmdDecreaseLifestyleMonths);
            this.tabLifestyle.Controls.Add(this.lblLifestyleMonths);
            this.tabLifestyle.Controls.Add(this.lblLifestyleSource);
            this.tabLifestyle.Controls.Add(this.lblLifestyleSourceLabel);
            this.tabLifestyle.Controls.Add(this.lblLifestyleCostLabel);
            this.tabLifestyle.Controls.Add(this.treLifestyles);
            this.tabLifestyle.Controls.Add(this.lblLifestyleCost);
            this.tabLifestyle.Controls.Add(this.cmdDeleteLifestyle);
            this.tabLifestyle.Controls.Add(this.lblLifestyleMonthsLabel);
            this.tabLifestyle.Location = new System.Drawing.Point(4, 22);
            this.tabLifestyle.Name = "tabLifestyle";
            this.tabLifestyle.Padding = new System.Windows.Forms.Padding(3);
            this.tabLifestyle.Size = new System.Drawing.Size(851, 554);
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
            // lblBaseLifestyle
            // 
            this.lblBaseLifestyle.AutoSize = true;
            this.lblBaseLifestyle.Location = new System.Drawing.Point(409, 128);
            this.lblBaseLifestyle.Name = "lblBaseLifestyle";
            this.lblBaseLifestyle.Size = new System.Drawing.Size(19, 13);
            this.lblBaseLifestyle.TabIndex = 117;
            this.lblBaseLifestyle.Text = "[0]";
            // 
            // lblLifestyleComfortsLabel
            // 
            this.lblLifestyleComfortsLabel.AutoSize = true;
            this.lblLifestyleComfortsLabel.Location = new System.Drawing.Point(308, 128);
            this.lblLifestyleComfortsLabel.Name = "lblLifestyleComfortsLabel";
            this.lblLifestyleComfortsLabel.Size = new System.Drawing.Size(48, 13);
            this.lblLifestyleComfortsLabel.TabIndex = 116;
            this.lblLifestyleComfortsLabel.Tag = "Label_SelectAdvancedLifestyle_Lifestyle";
            this.lblLifestyleComfortsLabel.Text = "Lifestyle:";
            // 
            // lblLifestyleQualities
            // 
            this.lblLifestyleQualities.Location = new System.Drawing.Point(480, 141);
            this.lblLifestyleQualities.Name = "lblLifestyleQualities";
            this.lblLifestyleQualities.Size = new System.Drawing.Size(323, 290);
            this.lblLifestyleQualities.TabIndex = 115;
            this.lblLifestyleQualities.Text = "[0]";
            // 
            // lblLifestyleQualitiesLabel
            // 
            this.lblLifestyleQualitiesLabel.AutoSize = true;
            this.lblLifestyleQualitiesLabel.Location = new System.Drawing.Point(480, 128);
            this.lblLifestyleQualitiesLabel.Name = "lblLifestyleQualitiesLabel";
            this.lblLifestyleQualitiesLabel.Size = new System.Drawing.Size(50, 13);
            this.lblLifestyleQualitiesLabel.TabIndex = 114;
            this.lblLifestyleQualitiesLabel.Tag = "Label_LifestyleQualities";
            this.lblLifestyleQualitiesLabel.Text = "Qualities:";
            // 
            // lblLifestyleMonths
            // 
            this.lblLifestyleMonths.AutoSize = true;
            this.lblLifestyleMonths.Location = new System.Drawing.Point(307, 64);
            this.lblLifestyleMonths.Name = "lblLifestyleMonths";
            this.lblLifestyleMonths.Size = new System.Drawing.Size(31, 13);
            this.lblLifestyleMonths.TabIndex = 91;
            this.lblLifestyleMonths.Text = "[100]";
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
            this.treLifestyles.Size = new System.Drawing.Size(295, 512);
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
            this.lblLifestyleMonthsLabel.Location = new System.Drawing.Point(404, 64);
            this.lblLifestyleMonthsLabel.Name = "lblLifestyleMonthsLabel";
            this.lblLifestyleMonthsLabel.Size = new System.Drawing.Size(138, 13);
            this.lblLifestyleMonthsLabel.TabIndex = 83;
            this.lblLifestyleMonthsLabel.Tag = "Label_LifestyleMonths";
            this.lblLifestyleMonthsLabel.Text = "Months (100 for Permanent)";
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
            this.tabArmor.Controls.Add(this.lblArmorRating);
            this.tabArmor.Controls.Add(this.lblArmorRatingLabel);
            this.tabArmor.Controls.Add(this.lblArmorSource);
            this.tabArmor.Controls.Add(this.lblArmorSourceLabel);
            this.tabArmor.Controls.Add(this.chkArmorEquipped);
            this.tabArmor.Controls.Add(this.lblArmorCost);
            this.tabArmor.Controls.Add(this.lblArmorCostLabel);
            this.tabArmor.Controls.Add(this.lblArmorAvail);
            this.tabArmor.Controls.Add(this.treArmor);
            this.tabArmor.Controls.Add(this.lblArmorAvailLabel);
            this.tabArmor.Controls.Add(this.cmdArmorIncrease);
            this.tabArmor.Controls.Add(this.cmdArmorDecrease);
            this.tabArmor.Controls.Add(this.cmdAddArmor);
            this.tabArmor.Controls.Add(this.cmdDeleteArmor);
            this.tabArmor.Location = new System.Drawing.Point(4, 22);
            this.tabArmor.Name = "tabArmor";
            this.tabArmor.Padding = new System.Windows.Forms.Padding(3);
            this.tabArmor.Size = new System.Drawing.Size(851, 554);
            this.tabArmor.TabIndex = 1;
            this.tabArmor.Tag = "Tab_Armor";
            this.tabArmor.Text = "Armor";
            // 
            // lblArmorFirewall
            // 
            this.lblArmorFirewall.AutoSize = true;
            this.lblArmorFirewall.Location = new System.Drawing.Point(750, 87);
            this.lblArmorFirewall.Name = "lblArmorFirewall";
            this.lblArmorFirewall.Size = new System.Drawing.Size(19, 13);
            this.lblArmorFirewall.TabIndex = 185;
            this.lblArmorFirewall.Text = "[0]";
            // 
            // lblArmorFirewallLabel
            // 
            this.lblArmorFirewallLabel.AutoSize = true;
            this.lblArmorFirewallLabel.Location = new System.Drawing.Point(703, 87);
            this.lblArmorFirewallLabel.Name = "lblArmorFirewallLabel";
            this.lblArmorFirewallLabel.Size = new System.Drawing.Size(45, 13);
            this.lblArmorFirewallLabel.TabIndex = 184;
            this.lblArmorFirewallLabel.Tag = "Label_Firewall";
            this.lblArmorFirewallLabel.Text = "Firewall:";
            // 
            // lblArmorDataProcessing
            // 
            this.lblArmorDataProcessing.AutoSize = true;
            this.lblArmorDataProcessing.Location = new System.Drawing.Point(678, 87);
            this.lblArmorDataProcessing.Name = "lblArmorDataProcessing";
            this.lblArmorDataProcessing.Size = new System.Drawing.Size(19, 13);
            this.lblArmorDataProcessing.TabIndex = 183;
            this.lblArmorDataProcessing.Text = "[0]";
            // 
            // lblArmorDataProcessingLabel
            // 
            this.lblArmorDataProcessingLabel.AutoSize = true;
            this.lblArmorDataProcessingLabel.Location = new System.Drawing.Point(584, 87);
            this.lblArmorDataProcessingLabel.Name = "lblArmorDataProcessingLabel";
            this.lblArmorDataProcessingLabel.Size = new System.Drawing.Size(88, 13);
            this.lblArmorDataProcessingLabel.TabIndex = 182;
            this.lblArmorDataProcessingLabel.Tag = "Label_DataProcessing";
            this.lblArmorDataProcessingLabel.Text = "Data Processing:";
            // 
            // lblArmorSleaze
            // 
            this.lblArmorSleaze.AutoSize = true;
            this.lblArmorSleaze.Location = new System.Drawing.Point(559, 87);
            this.lblArmorSleaze.Name = "lblArmorSleaze";
            this.lblArmorSleaze.Size = new System.Drawing.Size(19, 13);
            this.lblArmorSleaze.TabIndex = 181;
            this.lblArmorSleaze.Text = "[0]";
            // 
            // lblArmorSleazeLabel
            // 
            this.lblArmorSleazeLabel.AutoSize = true;
            this.lblArmorSleazeLabel.Location = new System.Drawing.Point(512, 87);
            this.lblArmorSleazeLabel.Name = "lblArmorSleazeLabel";
            this.lblArmorSleazeLabel.Size = new System.Drawing.Size(42, 13);
            this.lblArmorSleazeLabel.TabIndex = 180;
            this.lblArmorSleazeLabel.Tag = "Label_Sleaze";
            this.lblArmorSleazeLabel.Text = "Sleaze:";
            // 
            // lblArmorAttack
            // 
            this.lblArmorAttack.AutoSize = true;
            this.lblArmorAttack.Location = new System.Drawing.Point(487, 87);
            this.lblArmorAttack.Name = "lblArmorAttack";
            this.lblArmorAttack.Size = new System.Drawing.Size(19, 13);
            this.lblArmorAttack.TabIndex = 179;
            this.lblArmorAttack.Text = "[0]";
            // 
            // lblArmorAttackLabel
            // 
            this.lblArmorAttackLabel.AutoSize = true;
            this.lblArmorAttackLabel.Location = new System.Drawing.Point(440, 87);
            this.lblArmorAttackLabel.Name = "lblArmorAttackLabel";
            this.lblArmorAttackLabel.Size = new System.Drawing.Size(41, 13);
            this.lblArmorAttackLabel.TabIndex = 178;
            this.lblArmorAttackLabel.Tag = "Label_Attack";
            this.lblArmorAttackLabel.Text = "Attack:";
            // 
            // lblArmorDeviceRating
            // 
            this.lblArmorDeviceRating.AutoSize = true;
            this.lblArmorDeviceRating.Location = new System.Drawing.Point(406, 87);
            this.lblArmorDeviceRating.Name = "lblArmorDeviceRating";
            this.lblArmorDeviceRating.Size = new System.Drawing.Size(19, 13);
            this.lblArmorDeviceRating.TabIndex = 177;
            this.lblArmorDeviceRating.Text = "[0]";
            // 
            // lblArmorDeviceRatingLabel
            // 
            this.lblArmorDeviceRatingLabel.AutoSize = true;
            this.lblArmorDeviceRatingLabel.Location = new System.Drawing.Point(307, 87);
            this.lblArmorDeviceRatingLabel.Name = "lblArmorDeviceRatingLabel";
            this.lblArmorDeviceRatingLabel.Size = new System.Drawing.Size(78, 13);
            this.lblArmorDeviceRatingLabel.TabIndex = 176;
            this.lblArmorDeviceRatingLabel.Tag = "Label_DeviceRating";
            this.lblArmorDeviceRatingLabel.Text = "Device Rating:";
            // 
            // lblArmorValueLabel
            // 
            this.lblArmorValueLabel.AutoSize = true;
            this.lblArmorValueLabel.Location = new System.Drawing.Point(307, 41);
            this.lblArmorValueLabel.Name = "lblArmorValueLabel";
            this.lblArmorValueLabel.Size = new System.Drawing.Size(37, 13);
            this.lblArmorValueLabel.TabIndex = 118;
            this.lblArmorValueLabel.Tag = "Label_Armor";
            this.lblArmorValueLabel.Text = "Armor:";
            // 
            // lblArmorValue
            // 
            this.lblArmorValue.AutoSize = true;
            this.lblArmorValue.Location = new System.Drawing.Point(358, 41);
            this.lblArmorValue.Name = "lblArmorValue";
            this.lblArmorValue.Size = new System.Drawing.Size(20, 13);
            this.lblArmorValue.TabIndex = 119;
            this.lblArmorValue.Text = "[A]";
            // 
            // chkIncludedInArmor
            // 
            this.chkIncludedInArmor.AutoSize = true;
            this.chkIncludedInArmor.Enabled = false;
            this.chkIncludedInArmor.Location = new System.Drawing.Point(503, 130);
            this.chkIncludedInArmor.Name = "chkIncludedInArmor";
            this.chkIncludedInArmor.Size = new System.Drawing.Size(113, 17);
            this.chkIncludedInArmor.TabIndex = 114;
            this.chkIncludedInArmor.Tag = "Checkbox_BaseArmor";
            this.chkIncludedInArmor.Text = "Part of base Armor";
            this.chkIncludedInArmor.UseVisualStyleBackColor = true;
            this.chkIncludedInArmor.CheckedChanged += new System.EventHandler(this.chkIncludedInArmor_CheckedChanged);
            // 
            // lblArmorEquipped
            // 
            this.lblArmorEquipped.Location = new System.Drawing.Point(307, 221);
            this.lblArmorEquipped.Name = "lblArmorEquipped";
            this.lblArmorEquipped.Size = new System.Drawing.Size(514, 334);
            this.lblArmorEquipped.TabIndex = 113;
            this.lblArmorEquipped.Text = "[Armor Bundle Equipped Items]";
            this.lblArmorEquipped.Visible = false;
            // 
            // lblArmorEquippedLabel
            // 
            this.lblArmorEquippedLabel.AutoSize = true;
            this.lblArmorEquippedLabel.Location = new System.Drawing.Point(307, 198);
            this.lblArmorEquippedLabel.Name = "lblArmorEquippedLabel";
            this.lblArmorEquippedLabel.Size = new System.Drawing.Size(52, 13);
            this.lblArmorEquippedLabel.TabIndex = 112;
            this.lblArmorEquippedLabel.Tag = "Checkbox_Equipped";
            this.lblArmorEquippedLabel.Text = "Equipped";
            this.lblArmorEquippedLabel.Visible = false;
            // 
            // cmdArmorUnEquipAll
            // 
            this.cmdArmorUnEquipAll.AutoSize = true;
            this.cmdArmorUnEquipAll.Location = new System.Drawing.Point(475, 151);
            this.cmdArmorUnEquipAll.Name = "cmdArmorUnEquipAll";
            this.cmdArmorUnEquipAll.Size = new System.Drawing.Size(82, 23);
            this.cmdArmorUnEquipAll.TabIndex = 107;
            this.cmdArmorUnEquipAll.Tag = "Button_UnEquipAll";
            this.cmdArmorUnEquipAll.Text = "Un-Equip All";
            this.cmdArmorUnEquipAll.UseVisualStyleBackColor = true;
            this.cmdArmorUnEquipAll.Visible = false;
            this.cmdArmorUnEquipAll.Click += new System.EventHandler(this.cmdArmorUnEquipAll_Click);
            // 
            // cmdArmorEquipAll
            // 
            this.cmdArmorEquipAll.AutoSize = true;
            this.cmdArmorEquipAll.Location = new System.Drawing.Point(387, 151);
            this.cmdArmorEquipAll.Name = "cmdArmorEquipAll";
            this.cmdArmorEquipAll.Size = new System.Drawing.Size(82, 23);
            this.cmdArmorEquipAll.TabIndex = 106;
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
            this.cmdAddArmorBundle.TabIndex = 105;
            this.cmdAddArmorBundle.Tag = "Button_AddBundle";
            this.cmdAddArmorBundle.Text = "Add Armor Bundle";
            this.cmdAddArmorBundle.UseVisualStyleBackColor = true;
            this.cmdAddArmorBundle.Click += new System.EventHandler(this.cmdAddArmorBundle_Click);
            // 
            // lblArmorCapacity
            // 
            this.lblArmorCapacity.AutoSize = true;
            this.lblArmorCapacity.Location = new System.Drawing.Point(358, 108);
            this.lblArmorCapacity.Name = "lblArmorCapacity";
            this.lblArmorCapacity.Size = new System.Drawing.Size(54, 13);
            this.lblArmorCapacity.TabIndex = 85;
            this.lblArmorCapacity.Text = "[Capacity]";
            // 
            // lblArmorCapacityLabel
            // 
            this.lblArmorCapacityLabel.AutoSize = true;
            this.lblArmorCapacityLabel.Location = new System.Drawing.Point(307, 108);
            this.lblArmorCapacityLabel.Name = "lblArmorCapacityLabel";
            this.lblArmorCapacityLabel.Size = new System.Drawing.Size(51, 13);
            this.lblArmorCapacityLabel.TabIndex = 84;
            this.lblArmorCapacityLabel.Tag = "Label_Capacity";
            this.lblArmorCapacityLabel.Text = "Capacity:";
            // 
            // lblArmorRating
            // 
            this.lblArmorRating.AutoSize = true;
            this.lblArmorRating.Location = new System.Drawing.Point(358, 64);
            this.lblArmorRating.Name = "lblArmorRating";
            this.lblArmorRating.Size = new System.Drawing.Size(44, 13);
            this.lblArmorRating.TabIndex = 82;
            this.lblArmorRating.Text = "[Rating]";
            // 
            // lblArmorRatingLabel
            // 
            this.lblArmorRatingLabel.AutoSize = true;
            this.lblArmorRatingLabel.Location = new System.Drawing.Point(307, 64);
            this.lblArmorRatingLabel.Name = "lblArmorRatingLabel";
            this.lblArmorRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblArmorRatingLabel.TabIndex = 81;
            this.lblArmorRatingLabel.Tag = "Label_Rating";
            this.lblArmorRatingLabel.Text = "Rating:";
            // 
            // lblArmorSource
            // 
            this.lblArmorSource.AutoSize = true;
            this.lblArmorSource.Location = new System.Drawing.Point(358, 131);
            this.lblArmorSource.Name = "lblArmorSource";
            this.lblArmorSource.Size = new System.Drawing.Size(47, 13);
            this.lblArmorSource.TabIndex = 80;
            this.lblArmorSource.Text = "[Source]";
            this.lblArmorSource.Click += new System.EventHandler(this.lblArmorSource_Click);
            // 
            // lblArmorSourceLabel
            // 
            this.lblArmorSourceLabel.AutoSize = true;
            this.lblArmorSourceLabel.Location = new System.Drawing.Point(307, 131);
            this.lblArmorSourceLabel.Name = "lblArmorSourceLabel";
            this.lblArmorSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblArmorSourceLabel.TabIndex = 79;
            this.lblArmorSourceLabel.Tag = "Label_Source";
            this.lblArmorSourceLabel.Text = "Source:";
            // 
            // lblArmorCost
            // 
            this.lblArmorCost.AutoSize = true;
            this.lblArmorCost.Location = new System.Drawing.Point(551, 64);
            this.lblArmorCost.Name = "lblArmorCost";
            this.lblArmorCost.Size = new System.Drawing.Size(34, 13);
            this.lblArmorCost.TabIndex = 77;
            this.lblArmorCost.Text = "[Cost]";
            // 
            // lblArmorCostLabel
            // 
            this.lblArmorCostLabel.AutoSize = true;
            this.lblArmorCostLabel.Location = new System.Drawing.Point(500, 64);
            this.lblArmorCostLabel.Name = "lblArmorCostLabel";
            this.lblArmorCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblArmorCostLabel.TabIndex = 76;
            this.lblArmorCostLabel.Tag = "Label_Cost";
            this.lblArmorCostLabel.Text = "Cost:";
            // 
            // lblArmorAvail
            // 
            this.lblArmorAvail.AutoSize = true;
            this.lblArmorAvail.Location = new System.Drawing.Point(459, 64);
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
            this.lblArmorAvailLabel.Location = new System.Drawing.Point(408, 64);
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
            this.cmdAddArmor.Size = new System.Drawing.Size(88, 23);
            this.cmdAddArmor.SplitMenuStrip = this.cmsArmor;
            this.cmdAddArmor.TabIndex = 86;
            this.cmdAddArmor.Tag = "Button_AddArmor";
            this.cmdAddArmor.Text = "&Add Armor";
            this.cmdAddArmor.UseVisualStyleBackColor = true;
            this.cmdAddArmor.Click += new System.EventHandler(this.cmdAddArmor_Click);
            // 
            // cmdDeleteArmor
            // 
            this.cmdDeleteArmor.AutoSize = true;
            this.cmdDeleteArmor.ContextMenuStrip = this.cmsDeleteArmor;
            this.cmdDeleteArmor.Location = new System.Drawing.Point(100, 7);
            this.cmdDeleteArmor.Name = "cmdDeleteArmor";
            this.cmdDeleteArmor.Size = new System.Drawing.Size(80, 23);
            this.cmdDeleteArmor.SplitMenuStrip = this.cmsDeleteArmor;
            this.cmdDeleteArmor.TabIndex = 83;
            this.cmdDeleteArmor.Tag = "String_Delete";
            this.cmdDeleteArmor.Text = "Delete";
            this.cmdDeleteArmor.UseVisualStyleBackColor = true;
            this.cmdDeleteArmor.Click += new System.EventHandler(this.cmdDeleteArmor_Click);
            // 
            // tabWeapons
            // 
            this.tabWeapons.BackColor = System.Drawing.SystemColors.Control;
            this.tabWeapons.Controls.Add(this.cboWeaponGearDataProcessing);
            this.tabWeapons.Controls.Add(this.cboWeaponGearFirewall);
            this.tabWeapons.Controls.Add(this.cboWeaponGearSleaze);
            this.tabWeapons.Controls.Add(this.cboWeaponGearAttack);
            this.tabWeapons.Controls.Add(this.lblWeaponRating);
            this.tabWeapons.Controls.Add(this.lblWeaponRatingLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponFirewallLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponDataProcessingLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponSleazeLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponAttackLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponDeviceRating);
            this.tabWeapons.Controls.Add(this.lblWeaponDeviceRatingLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponAccuracyLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponAccuracy);
            this.tabWeapons.Controls.Add(this.cmdAddWeaponLocation);
            this.tabWeapons.Controls.Add(this.cboWeaponAmmo);
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
            this.tabWeapons.Controls.Add(this.cmdReloadWeapon);
            this.tabWeapons.Controls.Add(this.lblWeaponAmmoTypeLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponAmmoRemaining);
            this.tabWeapons.Controls.Add(this.lblWeaponAmmoRemainingLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponSlots);
            this.tabWeapons.Controls.Add(this.lblWeaponSlotsLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponSource);
            this.tabWeapons.Controls.Add(this.lblWeaponSourceLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponAmmo);
            this.tabWeapons.Controls.Add(this.lblWeaponAmmoLabel);
            this.tabWeapons.Controls.Add(this.treWeapons);
            this.tabWeapons.Controls.Add(this.lblWeaponMode);
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
            this.tabWeapons.Controls.Add(this.cmdRollWeapon);
            this.tabWeapons.Controls.Add(this.cmdWeaponMoveToVehicle);
            this.tabWeapons.Controls.Add(this.cmdWeaponBuyAmmo);
            this.tabWeapons.Controls.Add(this.cmdAddWeapon);
            this.tabWeapons.Controls.Add(this.cmdDeleteWeapon);
            this.tabWeapons.Controls.Add(this.cmdFireWeapon);
            this.tabWeapons.Location = new System.Drawing.Point(4, 22);
            this.tabWeapons.Name = "tabWeapons";
            this.tabWeapons.Size = new System.Drawing.Size(851, 554);
            this.tabWeapons.TabIndex = 2;
            this.tabWeapons.Tag = "Tab_Weapons";
            this.tabWeapons.Text = "Weapons";
            // 
            // cboWeaponGearDataProcessing
            // 
            this.cboWeaponGearDataProcessing.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboWeaponGearDataProcessing.FormattingEnabled = true;
            this.cboWeaponGearDataProcessing.Location = new System.Drawing.Point(674, 368);
            this.cboWeaponGearDataProcessing.Name = "cboWeaponGearDataProcessing";
            this.cboWeaponGearDataProcessing.Size = new System.Drawing.Size(30, 21);
            this.cboWeaponGearDataProcessing.TabIndex = 210;
            this.cboWeaponGearDataProcessing.Visible = false;
            this.cboWeaponGearDataProcessing.SelectedIndexChanged += new System.EventHandler(this.cboWeaponGearDataProcessing_SelectedIndexChanged);
            // 
            // cboWeaponGearFirewall
            // 
            this.cboWeaponGearFirewall.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboWeaponGearFirewall.FormattingEnabled = true;
            this.cboWeaponGearFirewall.Location = new System.Drawing.Point(755, 368);
            this.cboWeaponGearFirewall.Name = "cboWeaponGearFirewall";
            this.cboWeaponGearFirewall.Size = new System.Drawing.Size(30, 21);
            this.cboWeaponGearFirewall.TabIndex = 209;
            this.cboWeaponGearFirewall.Visible = false;
            this.cboWeaponGearFirewall.SelectedIndexChanged += new System.EventHandler(this.cboWeaponGearFirewall_SelectedIndexChanged);
            // 
            // cboWeaponGearSleaze
            // 
            this.cboWeaponGearSleaze.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboWeaponGearSleaze.FormattingEnabled = true;
            this.cboWeaponGearSleaze.Location = new System.Drawing.Point(556, 368);
            this.cboWeaponGearSleaze.Name = "cboWeaponGearSleaze";
            this.cboWeaponGearSleaze.Size = new System.Drawing.Size(30, 21);
            this.cboWeaponGearSleaze.TabIndex = 208;
            this.cboWeaponGearSleaze.Visible = false;
            this.cboWeaponGearSleaze.SelectedIndexChanged += new System.EventHandler(this.cboWeaponGearSleaze_SelectedIndexChanged);
            // 
            // cboWeaponGearAttack
            // 
            this.cboWeaponGearAttack.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboWeaponGearAttack.FormattingEnabled = true;
            this.cboWeaponGearAttack.Location = new System.Drawing.Point(482, 368);
            this.cboWeaponGearAttack.Name = "cboWeaponGearAttack";
            this.cboWeaponGearAttack.Size = new System.Drawing.Size(30, 21);
            this.cboWeaponGearAttack.TabIndex = 207;
            this.cboWeaponGearAttack.Visible = false;
            this.cboWeaponGearAttack.SelectedIndexChanged += new System.EventHandler(this.cboWeaponGearAttack_SelectedIndexChanged);
            // 
            // lblWeaponRating
            // 
            this.lblWeaponRating.AutoSize = true;
            this.lblWeaponRating.Location = new System.Drawing.Point(371, 195);
            this.lblWeaponRating.Name = "lblWeaponRating";
            this.lblWeaponRating.Size = new System.Drawing.Size(44, 13);
            this.lblWeaponRating.TabIndex = 177;
            this.lblWeaponRating.Text = "[Rating]";
            // 
            // lblWeaponRatingLabel
            // 
            this.lblWeaponRatingLabel.AutoSize = true;
            this.lblWeaponRatingLabel.Location = new System.Drawing.Point(308, 195);
            this.lblWeaponRatingLabel.Name = "lblWeaponRatingLabel";
            this.lblWeaponRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblWeaponRatingLabel.TabIndex = 176;
            this.lblWeaponRatingLabel.Tag = "Label_Rating";
            this.lblWeaponRatingLabel.Text = "Rating:";
            // 
            // lblWeaponFirewallLabel
            // 
            this.lblWeaponFirewallLabel.AutoSize = true;
            this.lblWeaponFirewallLabel.Location = new System.Drawing.Point(704, 371);
            this.lblWeaponFirewallLabel.Name = "lblWeaponFirewallLabel";
            this.lblWeaponFirewallLabel.Size = new System.Drawing.Size(45, 13);
            this.lblWeaponFirewallLabel.TabIndex = 174;
            this.lblWeaponFirewallLabel.Tag = "Label_Firewall";
            this.lblWeaponFirewallLabel.Text = "Firewall:";
            // 
            // lblWeaponDataProcessingLabel
            // 
            this.lblWeaponDataProcessingLabel.AutoSize = true;
            this.lblWeaponDataProcessingLabel.Location = new System.Drawing.Point(585, 371);
            this.lblWeaponDataProcessingLabel.Name = "lblWeaponDataProcessingLabel";
            this.lblWeaponDataProcessingLabel.Size = new System.Drawing.Size(88, 13);
            this.lblWeaponDataProcessingLabel.TabIndex = 172;
            this.lblWeaponDataProcessingLabel.Tag = "Label_DataProcessing";
            this.lblWeaponDataProcessingLabel.Text = "Data Processing:";
            // 
            // lblWeaponSleazeLabel
            // 
            this.lblWeaponSleazeLabel.AutoSize = true;
            this.lblWeaponSleazeLabel.Location = new System.Drawing.Point(513, 371);
            this.lblWeaponSleazeLabel.Name = "lblWeaponSleazeLabel";
            this.lblWeaponSleazeLabel.Size = new System.Drawing.Size(42, 13);
            this.lblWeaponSleazeLabel.TabIndex = 170;
            this.lblWeaponSleazeLabel.Tag = "Label_Sleaze";
            this.lblWeaponSleazeLabel.Text = "Sleaze:";
            // 
            // lblWeaponAttackLabel
            // 
            this.lblWeaponAttackLabel.AutoSize = true;
            this.lblWeaponAttackLabel.Location = new System.Drawing.Point(441, 371);
            this.lblWeaponAttackLabel.Name = "lblWeaponAttackLabel";
            this.lblWeaponAttackLabel.Size = new System.Drawing.Size(41, 13);
            this.lblWeaponAttackLabel.TabIndex = 168;
            this.lblWeaponAttackLabel.Tag = "Label_Attack";
            this.lblWeaponAttackLabel.Text = "Attack:";
            // 
            // lblWeaponDeviceRating
            // 
            this.lblWeaponDeviceRating.AutoSize = true;
            this.lblWeaponDeviceRating.Location = new System.Drawing.Point(407, 371);
            this.lblWeaponDeviceRating.Name = "lblWeaponDeviceRating";
            this.lblWeaponDeviceRating.Size = new System.Drawing.Size(19, 13);
            this.lblWeaponDeviceRating.TabIndex = 167;
            this.lblWeaponDeviceRating.Text = "[0]";
            // 
            // lblWeaponDeviceRatingLabel
            // 
            this.lblWeaponDeviceRatingLabel.AutoSize = true;
            this.lblWeaponDeviceRatingLabel.Location = new System.Drawing.Point(308, 371);
            this.lblWeaponDeviceRatingLabel.Name = "lblWeaponDeviceRatingLabel";
            this.lblWeaponDeviceRatingLabel.Size = new System.Drawing.Size(78, 13);
            this.lblWeaponDeviceRatingLabel.TabIndex = 166;
            this.lblWeaponDeviceRatingLabel.Tag = "Label_DeviceRating";
            this.lblWeaponDeviceRatingLabel.Text = "Device Rating:";
            // 
            // lblWeaponAccuracyLabel
            // 
            this.lblWeaponAccuracyLabel.AutoSize = true;
            this.lblWeaponAccuracyLabel.Location = new System.Drawing.Point(445, 219);
            this.lblWeaponAccuracyLabel.Name = "lblWeaponAccuracyLabel";
            this.lblWeaponAccuracyLabel.Size = new System.Drawing.Size(55, 13);
            this.lblWeaponAccuracyLabel.TabIndex = 119;
            this.lblWeaponAccuracyLabel.Tag = "Label_Accuracy";
            this.lblWeaponAccuracyLabel.Text = "Accuracy:";
            // 
            // lblWeaponAccuracy
            // 
            this.lblWeaponAccuracy.AutoSize = true;
            this.lblWeaponAccuracy.Location = new System.Drawing.Point(506, 219);
            this.lblWeaponAccuracy.Name = "lblWeaponAccuracy";
            this.lblWeaponAccuracy.Size = new System.Drawing.Size(58, 13);
            this.lblWeaponAccuracy.TabIndex = 120;
            this.lblWeaponAccuracy.Text = "[Accuracy]";
            // 
            // cmdAddWeaponLocation
            // 
            this.cmdAddWeaponLocation.AutoSize = true;
            this.cmdAddWeaponLocation.Location = new System.Drawing.Point(221, 7);
            this.cmdAddWeaponLocation.Name = "cmdAddWeaponLocation";
            this.cmdAddWeaponLocation.Size = new System.Drawing.Size(80, 23);
            this.cmdAddWeaponLocation.TabIndex = 118;
            this.cmdAddWeaponLocation.Tag = "Button_AddLocation";
            this.cmdAddWeaponLocation.Text = "Add Location";
            this.cmdAddWeaponLocation.UseVisualStyleBackColor = true;
            this.cmdAddWeaponLocation.Click += new System.EventHandler(this.cmdAddWeaponLocation_Click);
            // 
            // cboWeaponAmmo
            // 
            this.cboWeaponAmmo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboWeaponAmmo.FormattingEnabled = true;
            this.cboWeaponAmmo.Location = new System.Drawing.Point(409, 305);
            this.cboWeaponAmmo.Name = "cboWeaponAmmo";
            this.cboWeaponAmmo.Size = new System.Drawing.Size(283, 21);
            this.cboWeaponAmmo.TabIndex = 106;
            this.cboWeaponAmmo.SelectedIndexChanged += new System.EventHandler(this.cboWeaponAmmo_SelectedIndexChanged);
            // 
            // lblWeaponDicePool
            // 
            this.lblWeaponDicePool.AutoSize = true;
            this.lblWeaponDicePool.Location = new System.Drawing.Point(406, 333);
            this.lblWeaponDicePool.Name = "lblWeaponDicePool";
            this.lblWeaponDicePool.Size = new System.Drawing.Size(34, 13);
            this.lblWeaponDicePool.TabIndex = 105;
            this.lblWeaponDicePool.Text = "[Pool]";
            // 
            // lblWeaponDicePoolLabel
            // 
            this.lblWeaponDicePoolLabel.AutoSize = true;
            this.lblWeaponDicePoolLabel.Location = new System.Drawing.Point(308, 333);
            this.lblWeaponDicePoolLabel.Name = "lblWeaponDicePoolLabel";
            this.lblWeaponDicePoolLabel.Size = new System.Drawing.Size(56, 13);
            this.lblWeaponDicePoolLabel.TabIndex = 104;
            this.lblWeaponDicePoolLabel.Tag = "Label_DicePool";
            this.lblWeaponDicePoolLabel.Text = "Dice Pool:";
            // 
            // lblWeaponConceal
            // 
            this.lblWeaponConceal.AutoSize = true;
            this.lblWeaponConceal.Location = new System.Drawing.Point(640, 128);
            this.lblWeaponConceal.Name = "lblWeaponConceal";
            this.lblWeaponConceal.Size = new System.Drawing.Size(52, 13);
            this.lblWeaponConceal.TabIndex = 102;
            this.lblWeaponConceal.Text = "[Conceal]";
            // 
            // lblWeaponConcealLabel
            // 
            this.lblWeaponConcealLabel.AutoSize = true;
            this.lblWeaponConcealLabel.Location = new System.Drawing.Point(583, 128);
            this.lblWeaponConcealLabel.Name = "lblWeaponConcealLabel";
            this.lblWeaponConcealLabel.Size = new System.Drawing.Size(49, 13);
            this.lblWeaponConcealLabel.TabIndex = 101;
            this.lblWeaponConcealLabel.Tag = "Label_Conceal";
            this.lblWeaponConcealLabel.Text = "Conceal:";
            // 
            // lblWeaponRangeExtreme
            // 
            this.lblWeaponRangeExtreme.Location = new System.Drawing.Point(518, 259);
            this.lblWeaponRangeExtreme.Name = "lblWeaponRangeExtreme";
            this.lblWeaponRangeExtreme.Size = new System.Drawing.Size(64, 13);
            this.lblWeaponRangeExtreme.TabIndex = 89;
            this.lblWeaponRangeExtreme.Text = "[0]";
            this.lblWeaponRangeExtreme.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblWeaponRangeLong
            // 
            this.lblWeaponRangeLong.Location = new System.Drawing.Point(448, 259);
            this.lblWeaponRangeLong.Name = "lblWeaponRangeLong";
            this.lblWeaponRangeLong.Size = new System.Drawing.Size(64, 13);
            this.lblWeaponRangeLong.TabIndex = 88;
            this.lblWeaponRangeLong.Text = "[0]";
            this.lblWeaponRangeLong.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblWeaponRangeMedium
            // 
            this.lblWeaponRangeMedium.Location = new System.Drawing.Point(378, 259);
            this.lblWeaponRangeMedium.Name = "lblWeaponRangeMedium";
            this.lblWeaponRangeMedium.Size = new System.Drawing.Size(64, 13);
            this.lblWeaponRangeMedium.TabIndex = 87;
            this.lblWeaponRangeMedium.Text = "[0]";
            this.lblWeaponRangeMedium.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblWeaponRangeShort
            // 
            this.lblWeaponRangeShort.Location = new System.Drawing.Point(308, 259);
            this.lblWeaponRangeShort.Name = "lblWeaponRangeShort";
            this.lblWeaponRangeShort.Size = new System.Drawing.Size(64, 13);
            this.lblWeaponRangeShort.TabIndex = 86;
            this.lblWeaponRangeShort.Text = "[0]";
            this.lblWeaponRangeShort.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblWeaponRangeExtremeLabel
            // 
            this.lblWeaponRangeExtremeLabel.AutoSize = true;
            this.lblWeaponRangeExtremeLabel.Location = new System.Drawing.Point(520, 242);
            this.lblWeaponRangeExtremeLabel.Name = "lblWeaponRangeExtremeLabel";
            this.lblWeaponRangeExtremeLabel.Size = new System.Drawing.Size(63, 13);
            this.lblWeaponRangeExtremeLabel.TabIndex = 85;
            this.lblWeaponRangeExtremeLabel.Tag = "Label_RangeExtreme";
            this.lblWeaponRangeExtremeLabel.Text = "Extreme (-6)";
            this.lblWeaponRangeExtremeLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblWeaponRangeLongLabel
            // 
            this.lblWeaponRangeLongLabel.AutoSize = true;
            this.lblWeaponRangeLongLabel.Location = new System.Drawing.Point(457, 242);
            this.lblWeaponRangeLongLabel.Name = "lblWeaponRangeLongLabel";
            this.lblWeaponRangeLongLabel.Size = new System.Drawing.Size(49, 13);
            this.lblWeaponRangeLongLabel.TabIndex = 84;
            this.lblWeaponRangeLongLabel.Tag = "Label_RangeLong";
            this.lblWeaponRangeLongLabel.Text = "Long (-3)";
            this.lblWeaponRangeLongLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblWeaponRangeMediumLabel
            // 
            this.lblWeaponRangeMediumLabel.AutoSize = true;
            this.lblWeaponRangeMediumLabel.Location = new System.Drawing.Point(380, 242);
            this.lblWeaponRangeMediumLabel.Name = "lblWeaponRangeMediumLabel";
            this.lblWeaponRangeMediumLabel.Size = new System.Drawing.Size(62, 13);
            this.lblWeaponRangeMediumLabel.TabIndex = 83;
            this.lblWeaponRangeMediumLabel.Tag = "Label_RangeMedium";
            this.lblWeaponRangeMediumLabel.Text = "Medium (-1)";
            this.lblWeaponRangeMediumLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblWeaponRangeShortLabel
            // 
            this.lblWeaponRangeShortLabel.AutoSize = true;
            this.lblWeaponRangeShortLabel.Location = new System.Drawing.Point(316, 242);
            this.lblWeaponRangeShortLabel.Name = "lblWeaponRangeShortLabel";
            this.lblWeaponRangeShortLabel.Size = new System.Drawing.Size(50, 13);
            this.lblWeaponRangeShortLabel.TabIndex = 82;
            this.lblWeaponRangeShortLabel.Tag = "Label_RangeShort";
            this.lblWeaponRangeShortLabel.Text = "Short (-0)";
            this.lblWeaponRangeShortLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblWeaponRangeLabel
            // 
            this.lblWeaponRangeLabel.AutoSize = true;
            this.lblWeaponRangeLabel.Location = new System.Drawing.Point(307, 219);
            this.lblWeaponRangeLabel.Name = "lblWeaponRangeLabel";
            this.lblWeaponRangeLabel.Size = new System.Drawing.Size(39, 13);
            this.lblWeaponRangeLabel.TabIndex = 81;
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
            this.chkIncludedInWeapon.TabIndex = 80;
            this.chkIncludedInWeapon.Tag = "Checkbox_BaseWeapon";
            this.chkIncludedInWeapon.Text = "Part of base Weapon";
            this.chkIncludedInWeapon.UseVisualStyleBackColor = true;
            this.chkIncludedInWeapon.CheckedChanged += new System.EventHandler(this.chkIncludedInWeapon_CheckedChanged);
            // 
            // cmdReloadWeapon
            // 
            this.cmdReloadWeapon.Enabled = false;
            this.cmdReloadWeapon.Location = new System.Drawing.Point(614, 280);
            this.cmdReloadWeapon.Name = "cmdReloadWeapon";
            this.cmdReloadWeapon.Size = new System.Drawing.Size(79, 23);
            this.cmdReloadWeapon.TabIndex = 78;
            this.cmdReloadWeapon.Tag = "Button_Reload";
            this.cmdReloadWeapon.Text = "Reload";
            this.cmdReloadWeapon.UseVisualStyleBackColor = true;
            this.cmdReloadWeapon.Click += new System.EventHandler(this.cmdReloadWeapon_Click);
            // 
            // lblWeaponAmmoTypeLabel
            // 
            this.lblWeaponAmmoTypeLabel.AutoSize = true;
            this.lblWeaponAmmoTypeLabel.Location = new System.Drawing.Point(308, 308);
            this.lblWeaponAmmoTypeLabel.Name = "lblWeaponAmmoTypeLabel";
            this.lblWeaponAmmoTypeLabel.Size = new System.Drawing.Size(76, 13);
            this.lblWeaponAmmoTypeLabel.TabIndex = 76;
            this.lblWeaponAmmoTypeLabel.Tag = "Label_CurrentAmmo";
            this.lblWeaponAmmoTypeLabel.Text = "Current Ammo:";
            // 
            // lblWeaponAmmoRemaining
            // 
            this.lblWeaponAmmoRemaining.AutoSize = true;
            this.lblWeaponAmmoRemaining.Location = new System.Drawing.Point(406, 285);
            this.lblWeaponAmmoRemaining.Name = "lblWeaponAmmoRemaining";
            this.lblWeaponAmmoRemaining.Size = new System.Drawing.Size(95, 13);
            this.lblWeaponAmmoRemaining.TabIndex = 73;
            this.lblWeaponAmmoRemaining.Text = "[Ammo Remaining]";
            // 
            // lblWeaponAmmoRemainingLabel
            // 
            this.lblWeaponAmmoRemainingLabel.AutoSize = true;
            this.lblWeaponAmmoRemainingLabel.Location = new System.Drawing.Point(308, 285);
            this.lblWeaponAmmoRemainingLabel.Name = "lblWeaponAmmoRemainingLabel";
            this.lblWeaponAmmoRemainingLabel.Size = new System.Drawing.Size(92, 13);
            this.lblWeaponAmmoRemainingLabel.TabIndex = 72;
            this.lblWeaponAmmoRemainingLabel.Tag = "Label_AmmoRemaining";
            this.lblWeaponAmmoRemainingLabel.Text = "Ammo Remaining:";
            // 
            // lblWeaponSlots
            // 
            this.lblWeaponSlots.AutoSize = true;
            this.lblWeaponSlots.Location = new System.Drawing.Point(370, 174);
            this.lblWeaponSlots.Name = "lblWeaponSlots";
            this.lblWeaponSlots.Size = new System.Drawing.Size(36, 13);
            this.lblWeaponSlots.TabIndex = 71;
            this.lblWeaponSlots.Text = "[Slots]";
            // 
            // lblWeaponSlotsLabel
            // 
            this.lblWeaponSlotsLabel.AutoSize = true;
            this.lblWeaponSlotsLabel.Location = new System.Drawing.Point(307, 174);
            this.lblWeaponSlotsLabel.Name = "lblWeaponSlotsLabel";
            this.lblWeaponSlotsLabel.Size = new System.Drawing.Size(57, 13);
            this.lblWeaponSlotsLabel.TabIndex = 70;
            this.lblWeaponSlotsLabel.Tag = "Label_ModSlots";
            this.lblWeaponSlotsLabel.Text = "Mod Slots:";
            // 
            // lblWeaponSource
            // 
            this.lblWeaponSource.AutoSize = true;
            this.lblWeaponSource.Location = new System.Drawing.Point(370, 151);
            this.lblWeaponSource.Name = "lblWeaponSource";
            this.lblWeaponSource.Size = new System.Drawing.Size(47, 13);
            this.lblWeaponSource.TabIndex = 69;
            this.lblWeaponSource.Text = "[Source]";
            this.lblWeaponSource.Click += new System.EventHandler(this.lblWeaponSource_Click);
            // 
            // lblWeaponSourceLabel
            // 
            this.lblWeaponSourceLabel.AutoSize = true;
            this.lblWeaponSourceLabel.Location = new System.Drawing.Point(307, 151);
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
            this.treWeapons.Size = new System.Drawing.Size(295, 515);
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
            // cmdRollWeapon
            // 
            this.cmdRollWeapon.FlatAppearance.BorderSize = 0;
            this.cmdRollWeapon.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdRollWeapon.Image = global::Chummer.Properties.Resources.die;
            this.cmdRollWeapon.Location = new System.Drawing.Point(446, 328);
            this.cmdRollWeapon.Name = "cmdRollWeapon";
            this.cmdRollWeapon.Size = new System.Drawing.Size(24, 24);
            this.cmdRollWeapon.TabIndex = 117;
            this.cmdRollWeapon.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.cmdRollWeapon.UseVisualStyleBackColor = true;
            this.cmdRollWeapon.Visible = false;
            this.cmdRollWeapon.Click += new System.EventHandler(this.cmdRollWeapon_Click);
            // 
            // cmdAddWeapon
            // 
            this.cmdAddWeapon.AutoSize = true;
            this.cmdAddWeapon.ContextMenuStrip = this.cmsWeapon;
            this.cmdAddWeapon.Location = new System.Drawing.Point(6, 7);
            this.cmdAddWeapon.Name = "cmdAddWeapon";
            this.cmdAddWeapon.Size = new System.Drawing.Size(98, 23);
            this.cmdAddWeapon.SplitMenuStrip = this.cmsWeapon;
            this.cmdAddWeapon.TabIndex = 103;
            this.cmdAddWeapon.Tag = "Button_AddWeapon";
            this.cmdAddWeapon.Text = "&Add Weapon";
            this.cmdAddWeapon.UseVisualStyleBackColor = true;
            this.cmdAddWeapon.Click += new System.EventHandler(this.cmdAddWeapon_Click);
            // 
            // cmdDeleteWeapon
            // 
            this.cmdDeleteWeapon.AutoSize = true;
            this.cmdDeleteWeapon.ContextMenuStrip = this.cmsDeleteWeapon;
            this.cmdDeleteWeapon.Location = new System.Drawing.Point(110, 7);
            this.cmdDeleteWeapon.Name = "cmdDeleteWeapon";
            this.cmdDeleteWeapon.Size = new System.Drawing.Size(80, 23);
            this.cmdDeleteWeapon.SplitMenuStrip = this.cmsDeleteWeapon;
            this.cmdDeleteWeapon.TabIndex = 55;
            this.cmdDeleteWeapon.Tag = "String_Delete";
            this.cmdDeleteWeapon.Text = "Delete";
            this.cmdDeleteWeapon.UseVisualStyleBackColor = true;
            this.cmdDeleteWeapon.Click += new System.EventHandler(this.cmdDeleteWeapon_Click);
            // 
            // cmdFireWeapon
            // 
            this.cmdFireWeapon.AutoSize = true;
            this.cmdFireWeapon.ContextMenuStrip = this.cmsAmmoExpense;
            this.cmdFireWeapon.Enabled = false;
            this.cmdFireWeapon.Location = new System.Drawing.Point(529, 280);
            this.cmdFireWeapon.Name = "cmdFireWeapon";
            this.cmdFireWeapon.Size = new System.Drawing.Size(79, 23);
            this.cmdFireWeapon.SplitMenuStrip = this.cmsAmmoExpense;
            this.cmdFireWeapon.TabIndex = 75;
            this.cmdFireWeapon.Tag = "Button_Fire";
            this.cmdFireWeapon.Text = "FIRE!";
            this.cmdFireWeapon.UseVisualStyleBackColor = true;
            this.cmdFireWeapon.Click += new System.EventHandler(this.cmdFireWeapon_Click);
            // 
            // tabGear
            // 
            this.tabGear.BackColor = System.Drawing.SystemColors.Control;
            this.tabGear.Controls.Add(this.cboGearOverclocker);
            this.tabGear.Controls.Add(this.lblGearOverclocker);
            this.tabGear.Controls.Add(this.tabGearMatrixCM);
            this.tabGear.Controls.Add(this.cboGearDataProcessing);
            this.tabGear.Controls.Add(this.cboGearFirewall);
            this.tabGear.Controls.Add(this.cboGearSleaze);
            this.tabGear.Controls.Add(this.cboGearAttack);
            this.tabGear.Controls.Add(this.lblGearFirewallLabel);
            this.tabGear.Controls.Add(this.lblGearDataProcessingLabel);
            this.tabGear.Controls.Add(this.lblGearSleazeLabel);
            this.tabGear.Controls.Add(this.lblGearAttackLabel);
            this.tabGear.Controls.Add(this.lblGearDeviceRating);
            this.tabGear.Controls.Add(this.lblGearDeviceRatingLabel);
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
            this.tabGear.Controls.Add(this.lblGearRating);
            this.tabGear.Controls.Add(this.lblGearQty);
            this.tabGear.Controls.Add(this.lblFoci);
            this.tabGear.Controls.Add(this.treFoci);
            this.tabGear.Controls.Add(this.lblGearSource);
            this.tabGear.Controls.Add(this.lblGearSourceLabel);
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
            this.tabGear.Controls.Add(this.lblGearRatingLabel);
            this.tabGear.Controls.Add(this.treGear);
            this.tabGear.Controls.Add(this.cmdGearMoveToVehicle);
            this.tabGear.Controls.Add(this.cmdGearMergeQty);
            this.tabGear.Controls.Add(this.cmdGearSplitQty);
            this.tabGear.Controls.Add(this.cmdGearIncreaseQty);
            this.tabGear.Controls.Add(this.cmdGearReduceQty);
            this.tabGear.Controls.Add(this.cmdAddGear);
            this.tabGear.Controls.Add(this.cmdDeleteGear);
            this.tabGear.Location = new System.Drawing.Point(4, 22);
            this.tabGear.Name = "tabGear";
            this.tabGear.Size = new System.Drawing.Size(851, 554);
            this.tabGear.TabIndex = 3;
            this.tabGear.Tag = "Tab_Gear";
            this.tabGear.Text = "Gear";
            // 
            // cboGearOverclocker
            // 
            this.cboGearOverclocker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGearOverclocker.FormattingEnabled = true;
            this.cboGearOverclocker.Location = new System.Drawing.Point(485, 202);
            this.cboGearOverclocker.Name = "cboGearOverclocker";
            this.cboGearOverclocker.Size = new System.Drawing.Size(127, 21);
            this.cboGearOverclocker.TabIndex = 205;
            this.cboGearOverclocker.SelectedIndexChanged += new System.EventHandler(this.cboGearOverclocker_SelectedIndexChanged);
            // 
            // lblGearOverclocker
            // 
            this.lblGearOverclocker.Location = new System.Drawing.Point(485, 187);
            this.lblGearOverclocker.Name = "lblGearOverclocker";
            this.lblGearOverclocker.Size = new System.Drawing.Size(75, 13);
            this.lblGearOverclocker.TabIndex = 204;
            this.lblGearOverclocker.Tag = "";
            this.lblGearOverclocker.Text = "Overclocker:";
            // 
            // tabGearMatrixCM
            // 
            this.tabGearMatrixCM.Controls.Add(this.tabMatrixCM);
            this.tabGearMatrixCM.ItemSize = new System.Drawing.Size(176, 18);
            this.tabGearMatrixCM.Location = new System.Drawing.Point(498, 232);
            this.tabGearMatrixCM.Name = "tabGearMatrixCM";
            this.tabGearMatrixCM.SelectedIndex = 0;
            this.tabGearMatrixCM.Size = new System.Drawing.Size(213, 113);
            this.tabGearMatrixCM.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabGearMatrixCM.TabIndex = 203;
            this.tabGearMatrixCM.Visible = false;
            // 
            // tabMatrixCM
            // 
            this.tabMatrixCM.Controls.Add(this.chkGearMatrixCM1);
            this.tabMatrixCM.Controls.Add(this.chkGearMatrixCM2);
            this.tabMatrixCM.Controls.Add(this.chkGearMatrixCM3);
            this.tabMatrixCM.Controls.Add(this.chkGearMatrixCM4);
            this.tabMatrixCM.Controls.Add(this.chkGearMatrixCM5);
            this.tabMatrixCM.Controls.Add(this.chkGearMatrixCM6);
            this.tabMatrixCM.Controls.Add(this.chkGearMatrixCM7);
            this.tabMatrixCM.Controls.Add(this.chkGearMatrixCM8);
            this.tabMatrixCM.Controls.Add(this.chkGearMatrixCM9);
            this.tabMatrixCM.Controls.Add(this.chkGearMatrixCM10);
            this.tabMatrixCM.Controls.Add(this.chkGearMatrixCM11);
            this.tabMatrixCM.Controls.Add(this.chkGearMatrixCM12);
            this.tabMatrixCM.Controls.Add(this.chkGearMatrixCM13);
            this.tabMatrixCM.Controls.Add(this.chkGearMatrixCM14);
            this.tabMatrixCM.Controls.Add(this.chkGearMatrixCM15);
            this.tabMatrixCM.Controls.Add(this.chkGearMatrixCM16);
            this.tabMatrixCM.Controls.Add(this.chkGearMatrixCM17);
            this.tabMatrixCM.Controls.Add(this.chkGearMatrixCM18);
            this.tabMatrixCM.Controls.Add(this.chkGearMatrixCM19);
            this.tabMatrixCM.Controls.Add(this.chkGearMatrixCM20);
            this.tabMatrixCM.Controls.Add(this.chkGearMatrixCM21);
            this.tabMatrixCM.Controls.Add(this.chkGearMatrixCM22);
            this.tabMatrixCM.Controls.Add(this.chkGearMatrixCM23);
            this.tabMatrixCM.Controls.Add(this.chkGearMatrixCM24);
            this.tabMatrixCM.Location = new System.Drawing.Point(4, 22);
            this.tabMatrixCM.Name = "tabMatrixCM";
            this.tabMatrixCM.Padding = new System.Windows.Forms.Padding(3);
            this.tabMatrixCM.Size = new System.Drawing.Size(205, 87);
            this.tabMatrixCM.TabIndex = 1;
            this.tabMatrixCM.Text = "Matrix Condition Monitor";
            this.tabMatrixCM.UseVisualStyleBackColor = true;
            // 
            // chkGearMatrixCM1
            // 
            this.chkGearMatrixCM1.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkGearMatrixCM1.Location = new System.Drawing.Point(5, 6);
            this.chkGearMatrixCM1.Name = "chkGearMatrixCM1";
            this.chkGearMatrixCM1.Size = new System.Drawing.Size(24, 24);
            this.chkGearMatrixCM1.TabIndex = 49;
            this.chkGearMatrixCM1.Tag = "1";
            this.chkGearMatrixCM1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkGearMatrixCM1.UseVisualStyleBackColor = true;
            this.chkGearMatrixCM1.CheckedChanged += new System.EventHandler(this.chkGearCM_CheckedChanged);
            // 
            // chkGearMatrixCM2
            // 
            this.chkGearMatrixCM2.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkGearMatrixCM2.Location = new System.Drawing.Point(29, 6);
            this.chkGearMatrixCM2.Name = "chkGearMatrixCM2";
            this.chkGearMatrixCM2.Size = new System.Drawing.Size(24, 24);
            this.chkGearMatrixCM2.TabIndex = 50;
            this.chkGearMatrixCM2.Tag = "2";
            this.chkGearMatrixCM2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkGearMatrixCM2.UseVisualStyleBackColor = true;
            this.chkGearMatrixCM2.CheckedChanged += new System.EventHandler(this.chkGearCM_CheckedChanged);
            // 
            // chkGearMatrixCM3
            // 
            this.chkGearMatrixCM3.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkGearMatrixCM3.Location = new System.Drawing.Point(53, 6);
            this.chkGearMatrixCM3.Name = "chkGearMatrixCM3";
            this.chkGearMatrixCM3.Size = new System.Drawing.Size(24, 24);
            this.chkGearMatrixCM3.TabIndex = 51;
            this.chkGearMatrixCM3.Tag = "3";
            this.chkGearMatrixCM3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkGearMatrixCM3.UseVisualStyleBackColor = true;
            this.chkGearMatrixCM3.CheckedChanged += new System.EventHandler(this.chkGearCM_CheckedChanged);
            // 
            // chkGearMatrixCM4
            // 
            this.chkGearMatrixCM4.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkGearMatrixCM4.Location = new System.Drawing.Point(77, 6);
            this.chkGearMatrixCM4.Name = "chkGearMatrixCM4";
            this.chkGearMatrixCM4.Size = new System.Drawing.Size(24, 24);
            this.chkGearMatrixCM4.TabIndex = 52;
            this.chkGearMatrixCM4.Tag = "4";
            this.chkGearMatrixCM4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkGearMatrixCM4.UseVisualStyleBackColor = true;
            this.chkGearMatrixCM4.CheckedChanged += new System.EventHandler(this.chkGearCM_CheckedChanged);
            // 
            // chkGearMatrixCM5
            // 
            this.chkGearMatrixCM5.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkGearMatrixCM5.Location = new System.Drawing.Point(101, 6);
            this.chkGearMatrixCM5.Name = "chkGearMatrixCM5";
            this.chkGearMatrixCM5.Size = new System.Drawing.Size(24, 24);
            this.chkGearMatrixCM5.TabIndex = 53;
            this.chkGearMatrixCM5.Tag = "5";
            this.chkGearMatrixCM5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkGearMatrixCM5.UseVisualStyleBackColor = true;
            this.chkGearMatrixCM5.CheckedChanged += new System.EventHandler(this.chkGearCM_CheckedChanged);
            // 
            // chkGearMatrixCM6
            // 
            this.chkGearMatrixCM6.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkGearMatrixCM6.Location = new System.Drawing.Point(125, 6);
            this.chkGearMatrixCM6.Name = "chkGearMatrixCM6";
            this.chkGearMatrixCM6.Size = new System.Drawing.Size(24, 24);
            this.chkGearMatrixCM6.TabIndex = 54;
            this.chkGearMatrixCM6.Tag = "6";
            this.chkGearMatrixCM6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkGearMatrixCM6.UseVisualStyleBackColor = true;
            this.chkGearMatrixCM6.CheckedChanged += new System.EventHandler(this.chkGearCM_CheckedChanged);
            // 
            // chkGearMatrixCM7
            // 
            this.chkGearMatrixCM7.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkGearMatrixCM7.Location = new System.Drawing.Point(149, 6);
            this.chkGearMatrixCM7.Name = "chkGearMatrixCM7";
            this.chkGearMatrixCM7.Size = new System.Drawing.Size(24, 24);
            this.chkGearMatrixCM7.TabIndex = 55;
            this.chkGearMatrixCM7.Tag = "7";
            this.chkGearMatrixCM7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkGearMatrixCM7.UseVisualStyleBackColor = true;
            this.chkGearMatrixCM7.CheckedChanged += new System.EventHandler(this.chkGearCM_CheckedChanged);
            // 
            // chkGearMatrixCM8
            // 
            this.chkGearMatrixCM8.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkGearMatrixCM8.Location = new System.Drawing.Point(173, 6);
            this.chkGearMatrixCM8.Name = "chkGearMatrixCM8";
            this.chkGearMatrixCM8.Size = new System.Drawing.Size(24, 24);
            this.chkGearMatrixCM8.TabIndex = 56;
            this.chkGearMatrixCM8.Tag = "8";
            this.chkGearMatrixCM8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkGearMatrixCM8.UseVisualStyleBackColor = true;
            this.chkGearMatrixCM8.CheckedChanged += new System.EventHandler(this.chkGearCM_CheckedChanged);
            // 
            // chkGearMatrixCM9
            // 
            this.chkGearMatrixCM9.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkGearMatrixCM9.Location = new System.Drawing.Point(5, 31);
            this.chkGearMatrixCM9.Name = "chkGearMatrixCM9";
            this.chkGearMatrixCM9.Size = new System.Drawing.Size(24, 24);
            this.chkGearMatrixCM9.TabIndex = 57;
            this.chkGearMatrixCM9.Tag = "9";
            this.chkGearMatrixCM9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkGearMatrixCM9.UseVisualStyleBackColor = true;
            this.chkGearMatrixCM9.CheckedChanged += new System.EventHandler(this.chkGearCM_CheckedChanged);
            // 
            // chkGearMatrixCM10
            // 
            this.chkGearMatrixCM10.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkGearMatrixCM10.Location = new System.Drawing.Point(29, 31);
            this.chkGearMatrixCM10.Name = "chkGearMatrixCM10";
            this.chkGearMatrixCM10.Size = new System.Drawing.Size(24, 24);
            this.chkGearMatrixCM10.TabIndex = 58;
            this.chkGearMatrixCM10.Tag = "10";
            this.chkGearMatrixCM10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkGearMatrixCM10.UseVisualStyleBackColor = true;
            this.chkGearMatrixCM10.CheckedChanged += new System.EventHandler(this.chkGearCM_CheckedChanged);
            // 
            // chkGearMatrixCM11
            // 
            this.chkGearMatrixCM11.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkGearMatrixCM11.Location = new System.Drawing.Point(53, 31);
            this.chkGearMatrixCM11.Name = "chkGearMatrixCM11";
            this.chkGearMatrixCM11.Size = new System.Drawing.Size(24, 24);
            this.chkGearMatrixCM11.TabIndex = 59;
            this.chkGearMatrixCM11.Tag = "11";
            this.chkGearMatrixCM11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkGearMatrixCM11.UseVisualStyleBackColor = true;
            this.chkGearMatrixCM11.CheckedChanged += new System.EventHandler(this.chkGearCM_CheckedChanged);
            // 
            // chkGearMatrixCM12
            // 
            this.chkGearMatrixCM12.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkGearMatrixCM12.Location = new System.Drawing.Point(77, 31);
            this.chkGearMatrixCM12.Name = "chkGearMatrixCM12";
            this.chkGearMatrixCM12.Size = new System.Drawing.Size(24, 24);
            this.chkGearMatrixCM12.TabIndex = 60;
            this.chkGearMatrixCM12.Tag = "12";
            this.chkGearMatrixCM12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkGearMatrixCM12.UseVisualStyleBackColor = true;
            this.chkGearMatrixCM12.CheckedChanged += new System.EventHandler(this.chkGearCM_CheckedChanged);
            // 
            // chkGearMatrixCM13
            // 
            this.chkGearMatrixCM13.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkGearMatrixCM13.Location = new System.Drawing.Point(101, 31);
            this.chkGearMatrixCM13.Name = "chkGearMatrixCM13";
            this.chkGearMatrixCM13.Size = new System.Drawing.Size(24, 24);
            this.chkGearMatrixCM13.TabIndex = 63;
            this.chkGearMatrixCM13.Tag = "13";
            this.chkGearMatrixCM13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkGearMatrixCM13.UseVisualStyleBackColor = true;
            this.chkGearMatrixCM13.CheckedChanged += new System.EventHandler(this.chkGearCM_CheckedChanged);
            // 
            // chkGearMatrixCM14
            // 
            this.chkGearMatrixCM14.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkGearMatrixCM14.Location = new System.Drawing.Point(125, 31);
            this.chkGearMatrixCM14.Name = "chkGearMatrixCM14";
            this.chkGearMatrixCM14.Size = new System.Drawing.Size(24, 24);
            this.chkGearMatrixCM14.TabIndex = 64;
            this.chkGearMatrixCM14.Tag = "14";
            this.chkGearMatrixCM14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkGearMatrixCM14.UseVisualStyleBackColor = true;
            this.chkGearMatrixCM14.CheckedChanged += new System.EventHandler(this.chkGearCM_CheckedChanged);
            // 
            // chkGearMatrixCM15
            // 
            this.chkGearMatrixCM15.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkGearMatrixCM15.Location = new System.Drawing.Point(149, 31);
            this.chkGearMatrixCM15.Name = "chkGearMatrixCM15";
            this.chkGearMatrixCM15.Size = new System.Drawing.Size(24, 24);
            this.chkGearMatrixCM15.TabIndex = 65;
            this.chkGearMatrixCM15.Tag = "15";
            this.chkGearMatrixCM15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkGearMatrixCM15.UseVisualStyleBackColor = true;
            this.chkGearMatrixCM15.CheckedChanged += new System.EventHandler(this.chkGearCM_CheckedChanged);
            // 
            // chkGearMatrixCM16
            // 
            this.chkGearMatrixCM16.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkGearMatrixCM16.Location = new System.Drawing.Point(173, 31);
            this.chkGearMatrixCM16.Name = "chkGearMatrixCM16";
            this.chkGearMatrixCM16.Size = new System.Drawing.Size(24, 24);
            this.chkGearMatrixCM16.TabIndex = 66;
            this.chkGearMatrixCM16.Tag = "16";
            this.chkGearMatrixCM16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkGearMatrixCM16.UseVisualStyleBackColor = true;
            this.chkGearMatrixCM16.CheckedChanged += new System.EventHandler(this.chkGearCM_CheckedChanged);
            // 
            // chkGearMatrixCM17
            // 
            this.chkGearMatrixCM17.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkGearMatrixCM17.Location = new System.Drawing.Point(5, 57);
            this.chkGearMatrixCM17.Name = "chkGearMatrixCM17";
            this.chkGearMatrixCM17.Size = new System.Drawing.Size(24, 24);
            this.chkGearMatrixCM17.TabIndex = 67;
            this.chkGearMatrixCM17.Tag = "17";
            this.chkGearMatrixCM17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkGearMatrixCM17.UseVisualStyleBackColor = true;
            this.chkGearMatrixCM17.CheckedChanged += new System.EventHandler(this.chkGearCM_CheckedChanged);
            // 
            // chkGearMatrixCM18
            // 
            this.chkGearMatrixCM18.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkGearMatrixCM18.Location = new System.Drawing.Point(29, 57);
            this.chkGearMatrixCM18.Name = "chkGearMatrixCM18";
            this.chkGearMatrixCM18.Size = new System.Drawing.Size(24, 24);
            this.chkGearMatrixCM18.TabIndex = 68;
            this.chkGearMatrixCM18.Tag = "18";
            this.chkGearMatrixCM18.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkGearMatrixCM18.UseVisualStyleBackColor = true;
            this.chkGearMatrixCM18.CheckedChanged += new System.EventHandler(this.chkGearCM_CheckedChanged);
            // 
            // chkGearMatrixCM19
            // 
            this.chkGearMatrixCM19.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkGearMatrixCM19.Location = new System.Drawing.Point(53, 57);
            this.chkGearMatrixCM19.Name = "chkGearMatrixCM19";
            this.chkGearMatrixCM19.Size = new System.Drawing.Size(24, 24);
            this.chkGearMatrixCM19.TabIndex = 69;
            this.chkGearMatrixCM19.Tag = "19";
            this.chkGearMatrixCM19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkGearMatrixCM19.UseVisualStyleBackColor = true;
            this.chkGearMatrixCM19.CheckedChanged += new System.EventHandler(this.chkGearCM_CheckedChanged);
            // 
            // chkGearMatrixCM20
            // 
            this.chkGearMatrixCM20.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkGearMatrixCM20.Location = new System.Drawing.Point(77, 57);
            this.chkGearMatrixCM20.Name = "chkGearMatrixCM20";
            this.chkGearMatrixCM20.Size = new System.Drawing.Size(24, 24);
            this.chkGearMatrixCM20.TabIndex = 70;
            this.chkGearMatrixCM20.Tag = "20";
            this.chkGearMatrixCM20.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkGearMatrixCM20.UseVisualStyleBackColor = true;
            this.chkGearMatrixCM20.CheckedChanged += new System.EventHandler(this.chkGearCM_CheckedChanged);
            // 
            // chkGearMatrixCM21
            // 
            this.chkGearMatrixCM21.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkGearMatrixCM21.Location = new System.Drawing.Point(101, 57);
            this.chkGearMatrixCM21.Name = "chkGearMatrixCM21";
            this.chkGearMatrixCM21.Size = new System.Drawing.Size(24, 24);
            this.chkGearMatrixCM21.TabIndex = 71;
            this.chkGearMatrixCM21.Tag = "21";
            this.chkGearMatrixCM21.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkGearMatrixCM21.UseVisualStyleBackColor = true;
            this.chkGearMatrixCM21.CheckedChanged += new System.EventHandler(this.chkGearCM_CheckedChanged);
            // 
            // chkGearMatrixCM22
            // 
            this.chkGearMatrixCM22.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkGearMatrixCM22.Location = new System.Drawing.Point(125, 57);
            this.chkGearMatrixCM22.Name = "chkGearMatrixCM22";
            this.chkGearMatrixCM22.Size = new System.Drawing.Size(24, 24);
            this.chkGearMatrixCM22.TabIndex = 72;
            this.chkGearMatrixCM22.Tag = "22";
            this.chkGearMatrixCM22.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkGearMatrixCM22.UseVisualStyleBackColor = true;
            this.chkGearMatrixCM22.CheckedChanged += new System.EventHandler(this.chkGearCM_CheckedChanged);
            // 
            // chkGearMatrixCM23
            // 
            this.chkGearMatrixCM23.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkGearMatrixCM23.Location = new System.Drawing.Point(149, 57);
            this.chkGearMatrixCM23.Name = "chkGearMatrixCM23";
            this.chkGearMatrixCM23.Size = new System.Drawing.Size(24, 24);
            this.chkGearMatrixCM23.TabIndex = 73;
            this.chkGearMatrixCM23.Tag = "23";
            this.chkGearMatrixCM23.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkGearMatrixCM23.UseVisualStyleBackColor = true;
            this.chkGearMatrixCM23.CheckedChanged += new System.EventHandler(this.chkGearCM_CheckedChanged);
            // 
            // chkGearMatrixCM24
            // 
            this.chkGearMatrixCM24.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkGearMatrixCM24.Location = new System.Drawing.Point(173, 57);
            this.chkGearMatrixCM24.Name = "chkGearMatrixCM24";
            this.chkGearMatrixCM24.Size = new System.Drawing.Size(24, 24);
            this.chkGearMatrixCM24.TabIndex = 74;
            this.chkGearMatrixCM24.Tag = "24";
            this.chkGearMatrixCM24.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkGearMatrixCM24.UseVisualStyleBackColor = true;
            this.chkGearMatrixCM24.CheckedChanged += new System.EventHandler(this.chkGearCM_CheckedChanged);
            // 
            // cboGearDataProcessing
            // 
            this.cboGearDataProcessing.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGearDataProcessing.FormattingEnabled = true;
            this.cboGearDataProcessing.Location = new System.Drawing.Point(621, 163);
            this.cboGearDataProcessing.Name = "cboGearDataProcessing";
            this.cboGearDataProcessing.Size = new System.Drawing.Size(60, 21);
            this.cboGearDataProcessing.TabIndex = 167;
            this.cboGearDataProcessing.Visible = false;
            this.cboGearDataProcessing.SelectedIndexChanged += new System.EventHandler(this.cboGearDataProcessing_SelectedIndexChanged);
            // 
            // cboGearFirewall
            // 
            this.cboGearFirewall.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGearFirewall.FormattingEnabled = true;
            this.cboGearFirewall.Location = new System.Drawing.Point(687, 163);
            this.cboGearFirewall.Name = "cboGearFirewall";
            this.cboGearFirewall.Size = new System.Drawing.Size(60, 21);
            this.cboGearFirewall.TabIndex = 166;
            this.cboGearFirewall.Visible = false;
            this.cboGearFirewall.SelectedIndexChanged += new System.EventHandler(this.cboGearFirewall_SelectedIndexChanged);
            // 
            // cboGearSleaze
            // 
            this.cboGearSleaze.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGearSleaze.FormattingEnabled = true;
            this.cboGearSleaze.Location = new System.Drawing.Point(552, 163);
            this.cboGearSleaze.Name = "cboGearSleaze";
            this.cboGearSleaze.Size = new System.Drawing.Size(60, 21);
            this.cboGearSleaze.TabIndex = 165;
            this.cboGearSleaze.Visible = false;
            this.cboGearSleaze.SelectedIndexChanged += new System.EventHandler(this.cboGearSleaze_SelectedIndexChanged);
            // 
            // cboGearAttack
            // 
            this.cboGearAttack.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGearAttack.FormattingEnabled = true;
            this.cboGearAttack.Location = new System.Drawing.Point(485, 163);
            this.cboGearAttack.Name = "cboGearAttack";
            this.cboGearAttack.Size = new System.Drawing.Size(60, 21);
            this.cboGearAttack.TabIndex = 164;
            this.cboGearAttack.Visible = false;
            this.cboGearAttack.SelectedIndexChanged += new System.EventHandler(this.cboGearAttack_SelectedIndexChanged);
            // 
            // lblGearFirewallLabel
            // 
            this.lblGearFirewallLabel.Location = new System.Drawing.Point(687, 149);
            this.lblGearFirewallLabel.Name = "lblGearFirewallLabel";
            this.lblGearFirewallLabel.Size = new System.Drawing.Size(60, 13);
            this.lblGearFirewallLabel.TabIndex = 163;
            this.lblGearFirewallLabel.Tag = "Label_Firewall";
            this.lblGearFirewallLabel.Text = "Firewall:";
            this.lblGearFirewallLabel.Visible = false;
            // 
            // lblGearDataProcessingLabel
            // 
            this.lblGearDataProcessingLabel.Location = new System.Drawing.Point(621, 149);
            this.lblGearDataProcessingLabel.Name = "lblGearDataProcessingLabel";
            this.lblGearDataProcessingLabel.Size = new System.Drawing.Size(60, 13);
            this.lblGearDataProcessingLabel.TabIndex = 162;
            this.lblGearDataProcessingLabel.Tag = "Label_DataProcessing";
            this.lblGearDataProcessingLabel.Text = "Data Proc:";
            this.lblGearDataProcessingLabel.Visible = false;
            // 
            // lblGearSleazeLabel
            // 
            this.lblGearSleazeLabel.Location = new System.Drawing.Point(552, 149);
            this.lblGearSleazeLabel.Name = "lblGearSleazeLabel";
            this.lblGearSleazeLabel.Size = new System.Drawing.Size(60, 13);
            this.lblGearSleazeLabel.TabIndex = 161;
            this.lblGearSleazeLabel.Tag = "Label_Sleaze";
            this.lblGearSleazeLabel.Text = "Sleaze:";
            this.lblGearSleazeLabel.Visible = false;
            // 
            // lblGearAttackLabel
            // 
            this.lblGearAttackLabel.Location = new System.Drawing.Point(485, 149);
            this.lblGearAttackLabel.Name = "lblGearAttackLabel";
            this.lblGearAttackLabel.Size = new System.Drawing.Size(60, 13);
            this.lblGearAttackLabel.TabIndex = 160;
            this.lblGearAttackLabel.Tag = "Label_Attack";
            this.lblGearAttackLabel.Text = "Attack:";
            this.lblGearAttackLabel.Visible = false;
            // 
            // lblGearDeviceRating
            // 
            this.lblGearDeviceRating.AutoSize = true;
            this.lblGearDeviceRating.Location = new System.Drawing.Point(406, 164);
            this.lblGearDeviceRating.Name = "lblGearDeviceRating";
            this.lblGearDeviceRating.Size = new System.Drawing.Size(19, 13);
            this.lblGearDeviceRating.TabIndex = 157;
            this.lblGearDeviceRating.Text = "[0]";
            // 
            // lblGearDeviceRatingLabel
            // 
            this.lblGearDeviceRatingLabel.AutoSize = true;
            this.lblGearDeviceRatingLabel.Location = new System.Drawing.Point(307, 164);
            this.lblGearDeviceRatingLabel.Name = "lblGearDeviceRatingLabel";
            this.lblGearDeviceRatingLabel.Size = new System.Drawing.Size(78, 13);
            this.lblGearDeviceRatingLabel.TabIndex = 156;
            this.lblGearDeviceRatingLabel.Tag = "Label_DeviceRating";
            this.lblGearDeviceRatingLabel.Text = "Device Rating:";
            // 
            // chkActiveCommlink
            // 
            this.chkActiveCommlink.AutoSize = true;
            this.chkActiveCommlink.Location = new System.Drawing.Point(310, 276);
            this.chkActiveCommlink.Name = "chkActiveCommlink";
            this.chkActiveCommlink.Size = new System.Drawing.Size(104, 17);
            this.chkActiveCommlink.TabIndex = 118;
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
            this.chkCommlinks.TabIndex = 117;
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
            this.cmdCreateStackedFocus.TabIndex = 116;
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
            this.chkGearHomeNode.TabIndex = 114;
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
            this.lblGearAP.TabIndex = 111;
            this.lblGearAP.Text = "[0]";
            // 
            // lblGearAPLabel
            // 
            this.lblGearAPLabel.AutoSize = true;
            this.lblGearAPLabel.Location = new System.Drawing.Point(406, 189);
            this.lblGearAPLabel.Name = "lblGearAPLabel";
            this.lblGearAPLabel.Size = new System.Drawing.Size(24, 13);
            this.lblGearAPLabel.TabIndex = 110;
            this.lblGearAPLabel.Tag = "Label_AP";
            this.lblGearAPLabel.Text = "AP:";
            // 
            // lblGearDamage
            // 
            this.lblGearDamage.AutoSize = true;
            this.lblGearDamage.Location = new System.Drawing.Point(371, 189);
            this.lblGearDamage.Name = "lblGearDamage";
            this.lblGearDamage.Size = new System.Drawing.Size(19, 13);
            this.lblGearDamage.TabIndex = 109;
            this.lblGearDamage.Text = "[0]";
            // 
            // lblGearDamageLabel
            // 
            this.lblGearDamageLabel.AutoSize = true;
            this.lblGearDamageLabel.Location = new System.Drawing.Point(307, 189);
            this.lblGearDamageLabel.Name = "lblGearDamageLabel";
            this.lblGearDamageLabel.Size = new System.Drawing.Size(50, 13);
            this.lblGearDamageLabel.TabIndex = 108;
            this.lblGearDamageLabel.Tag = "Label_Damage";
            this.lblGearDamageLabel.Text = "Damage:";
            // 
            // cmdAddLocation
            // 
            this.cmdAddLocation.AutoSize = true;
            this.cmdAddLocation.Location = new System.Drawing.Point(221, 7);
            this.cmdAddLocation.Name = "cmdAddLocation";
            this.cmdAddLocation.Size = new System.Drawing.Size(80, 23);
            this.cmdAddLocation.TabIndex = 106;
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
            this.chkGearEquipped.TabIndex = 97;
            this.chkGearEquipped.Tag = "Checkbox_Equipped";
            this.chkGearEquipped.Text = "Equipped";
            this.chkGearEquipped.UseVisualStyleBackColor = true;
            this.chkGearEquipped.CheckedChanged += new System.EventHandler(this.chkGearEquipped_CheckedChanged);
            // 
            // lblGearRating
            // 
            this.lblGearRating.AutoSize = true;
            this.lblGearRating.Location = new System.Drawing.Point(365, 82);
            this.lblGearRating.Name = "lblGearRating";
            this.lblGearRating.Size = new System.Drawing.Size(44, 13);
            this.lblGearRating.TabIndex = 95;
            this.lblGearRating.Text = "[Rating]";
            // 
            // lblGearQty
            // 
            this.lblGearQty.AutoSize = true;
            this.lblGearQty.Location = new System.Drawing.Point(365, 128);
            this.lblGearQty.Name = "lblGearQty";
            this.lblGearQty.Size = new System.Drawing.Size(29, 13);
            this.lblGearQty.TabIndex = 93;
            this.lblGearQty.Text = "[Qty]";
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
            this.treFoci.Size = new System.Drawing.Size(308, 228);
            this.treFoci.TabIndex = 91;
            this.treFoci.BeforeCheck += new System.Windows.Forms.TreeViewCancelEventHandler(this.treFoci_BeforeCheck);
            this.treFoci.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treFoci_AfterCheck);
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
            // cmdAddGear
            // 
            this.cmdAddGear.AutoSize = true;
            this.cmdAddGear.ContextMenuStrip = this.cmsGearButton;
            this.cmdAddGear.Location = new System.Drawing.Point(6, 7);
            this.cmdAddGear.Name = "cmdAddGear";
            this.cmdAddGear.Size = new System.Drawing.Size(80, 23);
            this.cmdAddGear.SplitMenuStrip = this.cmsGearButton;
            this.cmdAddGear.TabIndex = 104;
            this.cmdAddGear.Tag = "Button_AddGear";
            this.cmdAddGear.Text = "&Add Gear";
            this.cmdAddGear.UseVisualStyleBackColor = true;
            this.cmdAddGear.Click += new System.EventHandler(this.cmdAddGear_Click);
            // 
            // cmdDeleteGear
            // 
            this.cmdDeleteGear.AutoSize = true;
            this.cmdDeleteGear.ContextMenuStrip = this.cmsDeleteGear;
            this.cmdDeleteGear.Location = new System.Drawing.Point(92, 7);
            this.cmdDeleteGear.Name = "cmdDeleteGear";
            this.cmdDeleteGear.Size = new System.Drawing.Size(80, 23);
            this.cmdDeleteGear.SplitMenuStrip = this.cmsDeleteGear;
            this.cmdDeleteGear.TabIndex = 94;
            this.cmdDeleteGear.Tag = "String_Delete";
            this.cmdDeleteGear.Text = "Delete";
            this.cmdDeleteGear.UseVisualStyleBackColor = true;
            this.cmdDeleteGear.Click += new System.EventHandler(this.cmdDeleteGear_Click);
            // 
            // tabPets
            // 
            this.tabPets.BackColor = System.Drawing.SystemColors.Control;
            this.tabPets.Controls.Add(this.panPets);
            this.tabPets.Controls.Add(this.cmdAddPet);
            this.tabPets.Location = new System.Drawing.Point(4, 22);
            this.tabPets.Name = "tabPets";
            this.tabPets.Padding = new System.Windows.Forms.Padding(3);
            this.tabPets.Size = new System.Drawing.Size(851, 554);
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
            this.panPets.TabIndex = 25;
            // 
            // cmdAddPet
            // 
            this.cmdAddPet.AutoSize = true;
            this.cmdAddPet.Location = new System.Drawing.Point(6, 7);
            this.cmdAddPet.Name = "cmdAddPet";
            this.cmdAddPet.Size = new System.Drawing.Size(76, 23);
            this.cmdAddPet.TabIndex = 24;
            this.cmdAddPet.Tag = "Button_AddPet";
            this.cmdAddPet.Text = "&Add Pet";
            this.cmdAddPet.UseVisualStyleBackColor = true;
            this.cmdAddPet.Click += new System.EventHandler(this.cmdAddPet_Click);
            // 
            // tabVehicles
            // 
            this.tabVehicles.BackColor = System.Drawing.SystemColors.ButtonFace;
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
            this.tabVehicles.Controls.Add(this.panVehicleCM);
            this.tabVehicles.Controls.Add(this.lblVehicleFirewallLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleDataProcessingLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleSleazeLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleAttackLabel);
            this.tabVehicles.Controls.Add(this.cmdAddVehicleLocation);
            this.tabVehicles.Controls.Add(this.chkVehicleHomeNode);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponDicePool);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponDicePoolLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleDevice);
            this.tabVehicles.Controls.Add(this.lblVehicleDeviceLabel);
            this.tabVehicles.Controls.Add(this.cboVehicleWeaponAmmo);
            this.tabVehicles.Controls.Add(this.lblVehicleGearQty);
            this.tabVehicles.Controls.Add(this.lblVehicleGearQtyLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponRangeExtreme);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponRangeLong);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponRangeMedium);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponRangeShort);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponRangeExtremeLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponRangeLongLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponRangeMediumLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponRangeShortLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponRangeLabel);
            this.tabVehicles.Controls.Add(this.chkVehicleIncludedInWeapon);
            this.tabVehicles.Controls.Add(this.chkVehicleWeaponAccessoryInstalled);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponAmmo);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponAmmoLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponMode);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponModeLabel);
            this.tabVehicles.Controls.Add(this.cmdReloadVehicleWeapon);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponAmmoTypeLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponAmmoRemaining);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponAmmoRemainingLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponNameLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponName);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponCategoryLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponAP);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponCategory);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponAPLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponDamageLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponDamage);
            this.tabVehicles.Controls.Add(this.lblVehicleRating);
            this.tabVehicles.Controls.Add(this.lblVehicleSource);
            this.tabVehicles.Controls.Add(this.lblVehicleSourceLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleSlots);
            this.tabVehicles.Controls.Add(this.lblVehicleSlotsLabel);
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
            this.tabVehicles.Controls.Add(this.treVehicles);
            this.tabVehicles.Controls.Add(this.cmdRollVehicleWeapon);
            this.tabVehicles.Controls.Add(this.cmdVehicleMoveToInventory);
            this.tabVehicles.Controls.Add(this.cmdVehicleGearReduceQty);
            this.tabVehicles.Controls.Add(this.cmdAddVehicle);
            this.tabVehicles.Controls.Add(this.cmdFireVehicleWeapon);
            this.tabVehicles.Controls.Add(this.cmdDeleteVehicle);
            this.tabVehicles.Location = new System.Drawing.Point(4, 22);
            this.tabVehicles.Name = "tabVehicles";
            this.tabVehicles.Size = new System.Drawing.Size(861, 586);
            this.tabVehicles.TabIndex = 7;
            this.tabVehicles.Tag = "Tab_Vehicles";
            this.tabVehicles.Text = "Vehicles & Drones";
            // 
            // lblVehicleSeats
            // 
            this.lblVehicleSeats.AutoSize = true;
            this.lblVehicleSeats.Location = new System.Drawing.Point(759, 152);
            this.lblVehicleSeats.Name = "lblVehicleSeats";
            this.lblVehicleSeats.Size = new System.Drawing.Size(40, 13);
            this.lblVehicleSeats.TabIndex = 226;
            this.lblVehicleSeats.Text = "[Seats]";
            // 
            // lblVehicleSeatsLabel
            // 
            this.lblVehicleSeatsLabel.AutoSize = true;
            this.lblVehicleSeatsLabel.Location = new System.Drawing.Point(720, 152);
            this.lblVehicleSeatsLabel.Name = "lblVehicleSeatsLabel";
            this.lblVehicleSeatsLabel.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleSeatsLabel.TabIndex = 225;
            this.lblVehicleSeatsLabel.Tag = "Label_Slots";
            this.lblVehicleSeatsLabel.Text = "Seats:";
            // 
            // lblVehicleDroneModSlots
            // 
            this.lblVehicleDroneModSlots.AutoSize = true;
            this.lblVehicleDroneModSlots.Location = new System.Drawing.Point(788, 176);
            this.lblVehicleDroneModSlots.Name = "lblVehicleDroneModSlots";
            this.lblVehicleDroneModSlots.Size = new System.Drawing.Size(57, 13);
            this.lblVehicleDroneModSlots.TabIndex = 224;
            this.lblVehicleDroneModSlots.Text = "[ModSlots]";
            this.lblVehicleDroneModSlots.Visible = false;
            // 
            // lblVehicleDroneModSlotsLabel
            // 
            this.lblVehicleDroneModSlotsLabel.AutoSize = true;
            this.lblVehicleDroneModSlotsLabel.Location = new System.Drawing.Point(725, 176);
            this.lblVehicleDroneModSlotsLabel.Name = "lblVehicleDroneModSlotsLabel";
            this.lblVehicleDroneModSlotsLabel.Size = new System.Drawing.Size(57, 13);
            this.lblVehicleDroneModSlotsLabel.TabIndex = 223;
            this.lblVehicleDroneModSlotsLabel.Tag = "Label_DroneModSlots";
            this.lblVehicleDroneModSlotsLabel.Text = "Mod Slots:";
            this.lblVehicleDroneModSlotsLabel.Visible = false;
            // 
            // lblVehicleCosmetic
            // 
            this.lblVehicleCosmetic.AutoSize = true;
            this.lblVehicleCosmetic.Location = new System.Drawing.Point(675, 198);
            this.lblVehicleCosmetic.Name = "lblVehicleCosmetic";
            this.lblVehicleCosmetic.Size = new System.Drawing.Size(56, 13);
            this.lblVehicleCosmetic.TabIndex = 222;
            this.lblVehicleCosmetic.Text = "[Cosmetic]";
            // 
            // lblVehicleElectromagnetic
            // 
            this.lblVehicleElectromagnetic.AutoSize = true;
            this.lblVehicleElectromagnetic.Location = new System.Drawing.Point(573, 198);
            this.lblVehicleElectromagnetic.Name = "lblVehicleElectromagnetic";
            this.lblVehicleElectromagnetic.Size = new System.Drawing.Size(34, 13);
            this.lblVehicleElectromagnetic.TabIndex = 221;
            this.lblVehicleElectromagnetic.Text = "[Elec]";
            // 
            // lblVehicleBodymod
            // 
            this.lblVehicleBodymod.AutoSize = true;
            this.lblVehicleBodymod.Location = new System.Drawing.Point(486, 198);
            this.lblVehicleBodymod.Name = "lblVehicleBodymod";
            this.lblVehicleBodymod.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleBodymod.TabIndex = 220;
            this.lblVehicleBodymod.Text = "[Body]";
            // 
            // lblVehicleWeaponsmod
            // 
            this.lblVehicleWeaponsmod.AutoSize = true;
            this.lblVehicleWeaponsmod.Location = new System.Drawing.Point(487, 176);
            this.lblVehicleWeaponsmod.Name = "lblVehicleWeaponsmod";
            this.lblVehicleWeaponsmod.Size = new System.Drawing.Size(42, 13);
            this.lblVehicleWeaponsmod.TabIndex = 219;
            this.lblVehicleWeaponsmod.Text = "[Weap]";
            // 
            // lblVehicleProtection
            // 
            this.lblVehicleProtection.AutoSize = true;
            this.lblVehicleProtection.Location = new System.Drawing.Point(573, 176);
            this.lblVehicleProtection.Name = "lblVehicleProtection";
            this.lblVehicleProtection.Size = new System.Drawing.Size(32, 13);
            this.lblVehicleProtection.TabIndex = 218;
            this.lblVehicleProtection.Text = "[Prot]";
            // 
            // lblVehiclePowertrain
            // 
            this.lblVehiclePowertrain.AutoSize = true;
            this.lblVehiclePowertrain.Location = new System.Drawing.Point(675, 176);
            this.lblVehiclePowertrain.Name = "lblVehiclePowertrain";
            this.lblVehiclePowertrain.Size = new System.Drawing.Size(43, 13);
            this.lblVehiclePowertrain.TabIndex = 217;
            this.lblVehiclePowertrain.Text = "[Power]";
            // 
            // lblVehicleCosmeticLabel
            // 
            this.lblVehicleCosmeticLabel.AutoSize = true;
            this.lblVehicleCosmeticLabel.Location = new System.Drawing.Point(626, 198);
            this.lblVehicleCosmeticLabel.Name = "lblVehicleCosmeticLabel";
            this.lblVehicleCosmeticLabel.Size = new System.Drawing.Size(53, 13);
            this.lblVehicleCosmeticLabel.TabIndex = 216;
            this.lblVehicleCosmeticLabel.Tag = "Label_Cosmetic";
            this.lblVehicleCosmeticLabel.Text = "Cosmetic:";
            // 
            // lblVehicleElectromagneticLabel
            // 
            this.lblVehicleElectromagneticLabel.AutoSize = true;
            this.lblVehicleElectromagneticLabel.Location = new System.Drawing.Point(533, 198);
            this.lblVehicleElectromagneticLabel.Name = "lblVehicleElectromagneticLabel";
            this.lblVehicleElectromagneticLabel.Size = new System.Drawing.Size(31, 13);
            this.lblVehicleElectromagneticLabel.TabIndex = 215;
            this.lblVehicleElectromagneticLabel.Tag = "Label_Electromagnetic";
            this.lblVehicleElectromagneticLabel.Text = "Elec:";
            // 
            // lblVehicleBodymodLabel
            // 
            this.lblVehicleBodymodLabel.AutoSize = true;
            this.lblVehicleBodymodLabel.Location = new System.Drawing.Point(417, 198);
            this.lblVehicleBodymodLabel.Name = "lblVehicleBodymodLabel";
            this.lblVehicleBodymodLabel.Size = new System.Drawing.Size(63, 13);
            this.lblVehicleBodymodLabel.TabIndex = 214;
            this.lblVehicleBodymodLabel.Tag = "Label_Bodymod";
            this.lblVehicleBodymodLabel.Text = "Body Mods:";
            // 
            // lblVehicleWeaponsmodLabel
            // 
            this.lblVehicleWeaponsmodLabel.AutoSize = true;
            this.lblVehicleWeaponsmodLabel.Location = new System.Drawing.Point(417, 176);
            this.lblVehicleWeaponsmodLabel.Name = "lblVehicleWeaponsmodLabel";
            this.lblVehicleWeaponsmodLabel.Size = new System.Drawing.Size(56, 13);
            this.lblVehicleWeaponsmodLabel.TabIndex = 213;
            this.lblVehicleWeaponsmodLabel.Tag = "Label_Weapons";
            this.lblVehicleWeaponsmodLabel.Text = "Weapons:";
            // 
            // lblVehicleProtectionLabel
            // 
            this.lblVehicleProtectionLabel.AutoSize = true;
            this.lblVehicleProtectionLabel.Location = new System.Drawing.Point(533, 176);
            this.lblVehicleProtectionLabel.Name = "lblVehicleProtectionLabel";
            this.lblVehicleProtectionLabel.Size = new System.Drawing.Size(29, 13);
            this.lblVehicleProtectionLabel.TabIndex = 212;
            this.lblVehicleProtectionLabel.Tag = "Label_Protection";
            this.lblVehicleProtectionLabel.Text = "Prot:";
            // 
            // lblVehiclePowertrainLabel
            // 
            this.lblVehiclePowertrainLabel.AutoSize = true;
            this.lblVehiclePowertrainLabel.Location = new System.Drawing.Point(626, 176);
            this.lblVehiclePowertrainLabel.Name = "lblVehiclePowertrainLabel";
            this.lblVehiclePowertrainLabel.Size = new System.Drawing.Size(40, 13);
            this.lblVehiclePowertrainLabel.TabIndex = 211;
            this.lblVehiclePowertrainLabel.Tag = "Label_Powertrain";
            this.lblVehiclePowertrainLabel.Text = "Power:";
            // 
            // cboVehicleGearDataProcessing
            // 
            this.cboVehicleGearDataProcessing.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboVehicleGearDataProcessing.FormattingEnabled = true;
            this.cboVehicleGearDataProcessing.Location = new System.Drawing.Point(693, 125);
            this.cboVehicleGearDataProcessing.Name = "cboVehicleGearDataProcessing";
            this.cboVehicleGearDataProcessing.Size = new System.Drawing.Size(30, 21);
            this.cboVehicleGearDataProcessing.TabIndex = 206;
            this.cboVehicleGearDataProcessing.Visible = false;
            this.cboVehicleGearDataProcessing.SelectedIndexChanged += new System.EventHandler(this.cboVehicleGearDataProcessing_SelectedIndexChanged);
            // 
            // cboVehicleGearFirewall
            // 
            this.cboVehicleGearFirewall.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboVehicleGearFirewall.FormattingEnabled = true;
            this.cboVehicleGearFirewall.Location = new System.Drawing.Point(777, 125);
            this.cboVehicleGearFirewall.Name = "cboVehicleGearFirewall";
            this.cboVehicleGearFirewall.Size = new System.Drawing.Size(30, 21);
            this.cboVehicleGearFirewall.TabIndex = 205;
            this.cboVehicleGearFirewall.Visible = false;
            this.cboVehicleGearFirewall.SelectedIndexChanged += new System.EventHandler(this.cboVehicleGearFirewall_SelectedIndexChanged);
            // 
            // cboVehicleGearSleaze
            // 
            this.cboVehicleGearSleaze.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboVehicleGearSleaze.FormattingEnabled = true;
            this.cboVehicleGearSleaze.Location = new System.Drawing.Point(576, 125);
            this.cboVehicleGearSleaze.Name = "cboVehicleGearSleaze";
            this.cboVehicleGearSleaze.Size = new System.Drawing.Size(30, 21);
            this.cboVehicleGearSleaze.TabIndex = 204;
            this.cboVehicleGearSleaze.Visible = false;
            this.cboVehicleGearSleaze.SelectedIndexChanged += new System.EventHandler(this.cboVehicleGearSleaze_SelectedIndexChanged);
            // 
            // cboVehicleGearAttack
            // 
            this.cboVehicleGearAttack.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboVehicleGearAttack.FormattingEnabled = true;
            this.cboVehicleGearAttack.Location = new System.Drawing.Point(489, 125);
            this.cboVehicleGearAttack.Name = "cboVehicleGearAttack";
            this.cboVehicleGearAttack.Size = new System.Drawing.Size(30, 21);
            this.cboVehicleGearAttack.TabIndex = 203;
            this.cboVehicleGearAttack.Visible = false;
            this.cboVehicleGearAttack.SelectedIndexChanged += new System.EventHandler(this.cboVehicleGearAttack_SelectedIndexChanged);
            // 
            // panVehicleCM
            // 
            this.panVehicleCM.Controls.Add(this.tabVehiclePhysicalCM);
            this.panVehicleCM.Controls.Add(this.tabVehicleMatrixCM);
            this.panVehicleCM.ItemSize = new System.Drawing.Size(176, 18);
            this.panVehicleCM.Location = new System.Drawing.Point(420, 491);
            this.panVehicleCM.Name = "panVehicleCM";
            this.panVehicleCM.SelectedIndex = 0;
            this.panVehicleCM.Size = new System.Drawing.Size(360, 113);
            this.panVehicleCM.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.panVehicleCM.TabIndex = 202;
            this.panVehicleCM.Visible = false;
            // 
            // tabVehiclePhysicalCM
            // 
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM40);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM1);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM39);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM2);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM38);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM3);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM37);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM4);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM36);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM5);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM35);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM6);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM34);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM7);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM33);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM8);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM32);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM9);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM31);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM10);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM30);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM11);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM29);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM12);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM28);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM13);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM27);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM14);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM26);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM15);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM25);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM16);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM24);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM17);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM23);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM18);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM22);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM19);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM21);
            this.tabVehiclePhysicalCM.Controls.Add(this.chkVehiclePhysicalCM20);
            this.tabVehiclePhysicalCM.Location = new System.Drawing.Point(4, 22);
            this.tabVehiclePhysicalCM.Name = "tabVehiclePhysicalCM";
            this.tabVehiclePhysicalCM.Padding = new System.Windows.Forms.Padding(3);
            this.tabVehiclePhysicalCM.Size = new System.Drawing.Size(352, 87);
            this.tabVehiclePhysicalCM.TabIndex = 0;
            this.tabVehiclePhysicalCM.Text = "Physical Condition Monitor";
            this.tabVehiclePhysicalCM.UseVisualStyleBackColor = true;
            // 
            // chkVehiclePhysicalCM40
            // 
            this.chkVehiclePhysicalCM40.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM40.Location = new System.Drawing.Point(269, 55);
            this.chkVehiclePhysicalCM40.Name = "chkVehiclePhysicalCM40";
            this.chkVehiclePhysicalCM40.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM40.TabIndex = 48;
            this.chkVehiclePhysicalCM40.Tag = "40";
            this.chkVehiclePhysicalCM40.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM40.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM40.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM1
            // 
            this.chkVehiclePhysicalCM1.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM1.Location = new System.Drawing.Point(5, 6);
            this.chkVehiclePhysicalCM1.Name = "chkVehiclePhysicalCM1";
            this.chkVehiclePhysicalCM1.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM1.TabIndex = 0;
            this.chkVehiclePhysicalCM1.Tag = "1";
            this.chkVehiclePhysicalCM1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM1.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM1.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM39
            // 
            this.chkVehiclePhysicalCM39.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM39.Location = new System.Drawing.Point(245, 55);
            this.chkVehiclePhysicalCM39.Name = "chkVehiclePhysicalCM39";
            this.chkVehiclePhysicalCM39.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM39.TabIndex = 47;
            this.chkVehiclePhysicalCM39.Tag = "39";
            this.chkVehiclePhysicalCM39.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM39.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM39.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM2
            // 
            this.chkVehiclePhysicalCM2.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM2.Location = new System.Drawing.Point(29, 6);
            this.chkVehiclePhysicalCM2.Name = "chkVehiclePhysicalCM2";
            this.chkVehiclePhysicalCM2.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM2.TabIndex = 1;
            this.chkVehiclePhysicalCM2.Tag = "2";
            this.chkVehiclePhysicalCM2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM2.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM2.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM38
            // 
            this.chkVehiclePhysicalCM38.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM38.Location = new System.Drawing.Point(221, 55);
            this.chkVehiclePhysicalCM38.Name = "chkVehiclePhysicalCM38";
            this.chkVehiclePhysicalCM38.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM38.TabIndex = 46;
            this.chkVehiclePhysicalCM38.Tag = "38";
            this.chkVehiclePhysicalCM38.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM38.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM38.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM3
            // 
            this.chkVehiclePhysicalCM3.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM3.Location = new System.Drawing.Point(53, 6);
            this.chkVehiclePhysicalCM3.Name = "chkVehiclePhysicalCM3";
            this.chkVehiclePhysicalCM3.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM3.TabIndex = 2;
            this.chkVehiclePhysicalCM3.Tag = "3";
            this.chkVehiclePhysicalCM3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM3.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM3.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM37
            // 
            this.chkVehiclePhysicalCM37.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM37.Location = new System.Drawing.Point(197, 55);
            this.chkVehiclePhysicalCM37.Name = "chkVehiclePhysicalCM37";
            this.chkVehiclePhysicalCM37.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM37.TabIndex = 45;
            this.chkVehiclePhysicalCM37.Tag = "37";
            this.chkVehiclePhysicalCM37.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM37.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM37.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM4
            // 
            this.chkVehiclePhysicalCM4.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM4.Location = new System.Drawing.Point(77, 6);
            this.chkVehiclePhysicalCM4.Name = "chkVehiclePhysicalCM4";
            this.chkVehiclePhysicalCM4.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM4.TabIndex = 3;
            this.chkVehiclePhysicalCM4.Tag = "4";
            this.chkVehiclePhysicalCM4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM4.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM4.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM36
            // 
            this.chkVehiclePhysicalCM36.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM36.Location = new System.Drawing.Point(173, 55);
            this.chkVehiclePhysicalCM36.Name = "chkVehiclePhysicalCM36";
            this.chkVehiclePhysicalCM36.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM36.TabIndex = 44;
            this.chkVehiclePhysicalCM36.Tag = "36";
            this.chkVehiclePhysicalCM36.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM36.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM36.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM5
            // 
            this.chkVehiclePhysicalCM5.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM5.Location = new System.Drawing.Point(101, 6);
            this.chkVehiclePhysicalCM5.Name = "chkVehiclePhysicalCM5";
            this.chkVehiclePhysicalCM5.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM5.TabIndex = 4;
            this.chkVehiclePhysicalCM5.Tag = "5";
            this.chkVehiclePhysicalCM5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM5.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM5.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM35
            // 
            this.chkVehiclePhysicalCM35.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM35.Location = new System.Drawing.Point(149, 55);
            this.chkVehiclePhysicalCM35.Name = "chkVehiclePhysicalCM35";
            this.chkVehiclePhysicalCM35.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM35.TabIndex = 43;
            this.chkVehiclePhysicalCM35.Tag = "35";
            this.chkVehiclePhysicalCM35.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM35.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM35.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM6
            // 
            this.chkVehiclePhysicalCM6.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM6.Location = new System.Drawing.Point(125, 6);
            this.chkVehiclePhysicalCM6.Name = "chkVehiclePhysicalCM6";
            this.chkVehiclePhysicalCM6.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM6.TabIndex = 5;
            this.chkVehiclePhysicalCM6.Tag = "6";
            this.chkVehiclePhysicalCM6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM6.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM6.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM34
            // 
            this.chkVehiclePhysicalCM34.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM34.Location = new System.Drawing.Point(125, 55);
            this.chkVehiclePhysicalCM34.Name = "chkVehiclePhysicalCM34";
            this.chkVehiclePhysicalCM34.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM34.TabIndex = 42;
            this.chkVehiclePhysicalCM34.Tag = "34";
            this.chkVehiclePhysicalCM34.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM34.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM34.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM7
            // 
            this.chkVehiclePhysicalCM7.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM7.Location = new System.Drawing.Point(149, 6);
            this.chkVehiclePhysicalCM7.Name = "chkVehiclePhysicalCM7";
            this.chkVehiclePhysicalCM7.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM7.TabIndex = 6;
            this.chkVehiclePhysicalCM7.Tag = "7";
            this.chkVehiclePhysicalCM7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM7.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM7.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM33
            // 
            this.chkVehiclePhysicalCM33.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM33.Location = new System.Drawing.Point(101, 55);
            this.chkVehiclePhysicalCM33.Name = "chkVehiclePhysicalCM33";
            this.chkVehiclePhysicalCM33.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM33.TabIndex = 41;
            this.chkVehiclePhysicalCM33.Tag = "33";
            this.chkVehiclePhysicalCM33.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM33.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM33.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM8
            // 
            this.chkVehiclePhysicalCM8.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM8.Location = new System.Drawing.Point(173, 6);
            this.chkVehiclePhysicalCM8.Name = "chkVehiclePhysicalCM8";
            this.chkVehiclePhysicalCM8.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM8.TabIndex = 7;
            this.chkVehiclePhysicalCM8.Tag = "8";
            this.chkVehiclePhysicalCM8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM8.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM8.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM32
            // 
            this.chkVehiclePhysicalCM32.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM32.Location = new System.Drawing.Point(77, 55);
            this.chkVehiclePhysicalCM32.Name = "chkVehiclePhysicalCM32";
            this.chkVehiclePhysicalCM32.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM32.TabIndex = 40;
            this.chkVehiclePhysicalCM32.Tag = "32";
            this.chkVehiclePhysicalCM32.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM32.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM32.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM9
            // 
            this.chkVehiclePhysicalCM9.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM9.Location = new System.Drawing.Point(197, 6);
            this.chkVehiclePhysicalCM9.Name = "chkVehiclePhysicalCM9";
            this.chkVehiclePhysicalCM9.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM9.TabIndex = 8;
            this.chkVehiclePhysicalCM9.Tag = "9";
            this.chkVehiclePhysicalCM9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM9.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM9.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM31
            // 
            this.chkVehiclePhysicalCM31.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM31.Location = new System.Drawing.Point(53, 55);
            this.chkVehiclePhysicalCM31.Name = "chkVehiclePhysicalCM31";
            this.chkVehiclePhysicalCM31.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM31.TabIndex = 39;
            this.chkVehiclePhysicalCM31.Tag = "31";
            this.chkVehiclePhysicalCM31.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM31.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM31.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM10
            // 
            this.chkVehiclePhysicalCM10.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM10.Location = new System.Drawing.Point(221, 6);
            this.chkVehiclePhysicalCM10.Name = "chkVehiclePhysicalCM10";
            this.chkVehiclePhysicalCM10.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM10.TabIndex = 9;
            this.chkVehiclePhysicalCM10.Tag = "10";
            this.chkVehiclePhysicalCM10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM10.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM10.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM30
            // 
            this.chkVehiclePhysicalCM30.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM30.Location = new System.Drawing.Point(29, 55);
            this.chkVehiclePhysicalCM30.Name = "chkVehiclePhysicalCM30";
            this.chkVehiclePhysicalCM30.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM30.TabIndex = 38;
            this.chkVehiclePhysicalCM30.Tag = "30";
            this.chkVehiclePhysicalCM30.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM30.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM30.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM11
            // 
            this.chkVehiclePhysicalCM11.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM11.Location = new System.Drawing.Point(245, 6);
            this.chkVehiclePhysicalCM11.Name = "chkVehiclePhysicalCM11";
            this.chkVehiclePhysicalCM11.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM11.TabIndex = 19;
            this.chkVehiclePhysicalCM11.Tag = "11";
            this.chkVehiclePhysicalCM11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM11.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM11.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM29
            // 
            this.chkVehiclePhysicalCM29.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM29.Location = new System.Drawing.Point(5, 55);
            this.chkVehiclePhysicalCM29.Name = "chkVehiclePhysicalCM29";
            this.chkVehiclePhysicalCM29.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM29.TabIndex = 37;
            this.chkVehiclePhysicalCM29.Tag = "29";
            this.chkVehiclePhysicalCM29.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM29.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM29.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM12
            // 
            this.chkVehiclePhysicalCM12.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM12.Location = new System.Drawing.Point(269, 6);
            this.chkVehiclePhysicalCM12.Name = "chkVehiclePhysicalCM12";
            this.chkVehiclePhysicalCM12.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM12.TabIndex = 20;
            this.chkVehiclePhysicalCM12.Tag = "12";
            this.chkVehiclePhysicalCM12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM12.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM12.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM28
            // 
            this.chkVehiclePhysicalCM28.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM28.Location = new System.Drawing.Point(317, 31);
            this.chkVehiclePhysicalCM28.Name = "chkVehiclePhysicalCM28";
            this.chkVehiclePhysicalCM28.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM28.TabIndex = 36;
            this.chkVehiclePhysicalCM28.Tag = "28";
            this.chkVehiclePhysicalCM28.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM28.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM28.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM13
            // 
            this.chkVehiclePhysicalCM13.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM13.Location = new System.Drawing.Point(293, 6);
            this.chkVehiclePhysicalCM13.Name = "chkVehiclePhysicalCM13";
            this.chkVehiclePhysicalCM13.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM13.TabIndex = 21;
            this.chkVehiclePhysicalCM13.Tag = "13";
            this.chkVehiclePhysicalCM13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM13.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM13.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM27
            // 
            this.chkVehiclePhysicalCM27.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM27.Location = new System.Drawing.Point(293, 31);
            this.chkVehiclePhysicalCM27.Name = "chkVehiclePhysicalCM27";
            this.chkVehiclePhysicalCM27.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM27.TabIndex = 35;
            this.chkVehiclePhysicalCM27.Tag = "27";
            this.chkVehiclePhysicalCM27.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM27.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM27.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM14
            // 
            this.chkVehiclePhysicalCM14.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM14.Location = new System.Drawing.Point(317, 6);
            this.chkVehiclePhysicalCM14.Name = "chkVehiclePhysicalCM14";
            this.chkVehiclePhysicalCM14.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM14.TabIndex = 22;
            this.chkVehiclePhysicalCM14.Tag = "14";
            this.chkVehiclePhysicalCM14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM14.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM14.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM26
            // 
            this.chkVehiclePhysicalCM26.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM26.Location = new System.Drawing.Point(269, 31);
            this.chkVehiclePhysicalCM26.Name = "chkVehiclePhysicalCM26";
            this.chkVehiclePhysicalCM26.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM26.TabIndex = 34;
            this.chkVehiclePhysicalCM26.Tag = "26";
            this.chkVehiclePhysicalCM26.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM26.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM26.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM15
            // 
            this.chkVehiclePhysicalCM15.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM15.Location = new System.Drawing.Point(5, 31);
            this.chkVehiclePhysicalCM15.Name = "chkVehiclePhysicalCM15";
            this.chkVehiclePhysicalCM15.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM15.TabIndex = 23;
            this.chkVehiclePhysicalCM15.Tag = "15";
            this.chkVehiclePhysicalCM15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM15.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM15.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM25
            // 
            this.chkVehiclePhysicalCM25.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM25.Location = new System.Drawing.Point(245, 31);
            this.chkVehiclePhysicalCM25.Name = "chkVehiclePhysicalCM25";
            this.chkVehiclePhysicalCM25.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM25.TabIndex = 33;
            this.chkVehiclePhysicalCM25.Tag = "25";
            this.chkVehiclePhysicalCM25.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM25.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM25.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM16
            // 
            this.chkVehiclePhysicalCM16.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM16.Location = new System.Drawing.Point(29, 31);
            this.chkVehiclePhysicalCM16.Name = "chkVehiclePhysicalCM16";
            this.chkVehiclePhysicalCM16.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM16.TabIndex = 24;
            this.chkVehiclePhysicalCM16.Tag = "16";
            this.chkVehiclePhysicalCM16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM16.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM16.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM24
            // 
            this.chkVehiclePhysicalCM24.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM24.Location = new System.Drawing.Point(221, 31);
            this.chkVehiclePhysicalCM24.Name = "chkVehiclePhysicalCM24";
            this.chkVehiclePhysicalCM24.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM24.TabIndex = 32;
            this.chkVehiclePhysicalCM24.Tag = "24";
            this.chkVehiclePhysicalCM24.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM24.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM24.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM17
            // 
            this.chkVehiclePhysicalCM17.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM17.Location = new System.Drawing.Point(53, 31);
            this.chkVehiclePhysicalCM17.Name = "chkVehiclePhysicalCM17";
            this.chkVehiclePhysicalCM17.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM17.TabIndex = 25;
            this.chkVehiclePhysicalCM17.Tag = "17";
            this.chkVehiclePhysicalCM17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM17.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM17.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM23
            // 
            this.chkVehiclePhysicalCM23.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM23.Location = new System.Drawing.Point(197, 31);
            this.chkVehiclePhysicalCM23.Name = "chkVehiclePhysicalCM23";
            this.chkVehiclePhysicalCM23.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM23.TabIndex = 31;
            this.chkVehiclePhysicalCM23.Tag = "23";
            this.chkVehiclePhysicalCM23.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM23.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM23.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM18
            // 
            this.chkVehiclePhysicalCM18.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM18.Location = new System.Drawing.Point(77, 31);
            this.chkVehiclePhysicalCM18.Name = "chkVehiclePhysicalCM18";
            this.chkVehiclePhysicalCM18.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM18.TabIndex = 26;
            this.chkVehiclePhysicalCM18.Tag = "18";
            this.chkVehiclePhysicalCM18.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM18.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM18.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM22
            // 
            this.chkVehiclePhysicalCM22.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM22.Location = new System.Drawing.Point(173, 31);
            this.chkVehiclePhysicalCM22.Name = "chkVehiclePhysicalCM22";
            this.chkVehiclePhysicalCM22.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM22.TabIndex = 30;
            this.chkVehiclePhysicalCM22.Tag = "22";
            this.chkVehiclePhysicalCM22.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM22.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM22.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM19
            // 
            this.chkVehiclePhysicalCM19.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM19.Location = new System.Drawing.Point(101, 31);
            this.chkVehiclePhysicalCM19.Name = "chkVehiclePhysicalCM19";
            this.chkVehiclePhysicalCM19.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM19.TabIndex = 27;
            this.chkVehiclePhysicalCM19.Tag = "19";
            this.chkVehiclePhysicalCM19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM19.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM19.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM21
            // 
            this.chkVehiclePhysicalCM21.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM21.Location = new System.Drawing.Point(149, 31);
            this.chkVehiclePhysicalCM21.Name = "chkVehiclePhysicalCM21";
            this.chkVehiclePhysicalCM21.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM21.TabIndex = 29;
            this.chkVehiclePhysicalCM21.Tag = "21";
            this.chkVehiclePhysicalCM21.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM21.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM21.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehiclePhysicalCM20
            // 
            this.chkVehiclePhysicalCM20.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehiclePhysicalCM20.Location = new System.Drawing.Point(125, 31);
            this.chkVehiclePhysicalCM20.Name = "chkVehiclePhysicalCM20";
            this.chkVehiclePhysicalCM20.Size = new System.Drawing.Size(24, 24);
            this.chkVehiclePhysicalCM20.TabIndex = 28;
            this.chkVehiclePhysicalCM20.Tag = "20";
            this.chkVehiclePhysicalCM20.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehiclePhysicalCM20.UseVisualStyleBackColor = true;
            this.chkVehiclePhysicalCM20.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // tabVehicleMatrixCM
            // 
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM1);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM2);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM3);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM4);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM5);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM6);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM7);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM8);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM9);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM10);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM11);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM12);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM13);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM14);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM15);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM16);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM17);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM18);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM19);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM20);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM21);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM22);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM23);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM24);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM25);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM26);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM27);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM28);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM29);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM30);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM31);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM32);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM33);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM34);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM35);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM36);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM37);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM38);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM39);
            this.tabVehicleMatrixCM.Controls.Add(this.chkVehicleMatrixCM40);
            this.tabVehicleMatrixCM.Location = new System.Drawing.Point(4, 22);
            this.tabVehicleMatrixCM.Name = "tabVehicleMatrixCM";
            this.tabVehicleMatrixCM.Padding = new System.Windows.Forms.Padding(3);
            this.tabVehicleMatrixCM.Size = new System.Drawing.Size(352, 87);
            this.tabVehicleMatrixCM.TabIndex = 1;
            this.tabVehicleMatrixCM.Text = "Matrix Condition Monitor";
            this.tabVehicleMatrixCM.UseVisualStyleBackColor = true;
            // 
            // chkVehicleMatrixCM1
            // 
            this.chkVehicleMatrixCM1.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM1.Location = new System.Drawing.Point(5, 6);
            this.chkVehicleMatrixCM1.Name = "chkVehicleMatrixCM1";
            this.chkVehicleMatrixCM1.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM1.TabIndex = 49;
            this.chkVehicleMatrixCM1.Tag = "1";
            this.chkVehicleMatrixCM1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM1.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM1.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM2
            // 
            this.chkVehicleMatrixCM2.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM2.Location = new System.Drawing.Point(29, 6);
            this.chkVehicleMatrixCM2.Name = "chkVehicleMatrixCM2";
            this.chkVehicleMatrixCM2.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM2.TabIndex = 50;
            this.chkVehicleMatrixCM2.Tag = "2";
            this.chkVehicleMatrixCM2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM2.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM2.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM3
            // 
            this.chkVehicleMatrixCM3.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM3.Location = new System.Drawing.Point(53, 6);
            this.chkVehicleMatrixCM3.Name = "chkVehicleMatrixCM3";
            this.chkVehicleMatrixCM3.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM3.TabIndex = 51;
            this.chkVehicleMatrixCM3.Tag = "3";
            this.chkVehicleMatrixCM3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM3.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM3.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM4
            // 
            this.chkVehicleMatrixCM4.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM4.Location = new System.Drawing.Point(77, 6);
            this.chkVehicleMatrixCM4.Name = "chkVehicleMatrixCM4";
            this.chkVehicleMatrixCM4.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM4.TabIndex = 52;
            this.chkVehicleMatrixCM4.Tag = "4";
            this.chkVehicleMatrixCM4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM4.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM4.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM5
            // 
            this.chkVehicleMatrixCM5.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM5.Location = new System.Drawing.Point(101, 6);
            this.chkVehicleMatrixCM5.Name = "chkVehicleMatrixCM5";
            this.chkVehicleMatrixCM5.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM5.TabIndex = 53;
            this.chkVehicleMatrixCM5.Tag = "5";
            this.chkVehicleMatrixCM5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM5.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM5.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM6
            // 
            this.chkVehicleMatrixCM6.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM6.Location = new System.Drawing.Point(125, 6);
            this.chkVehicleMatrixCM6.Name = "chkVehicleMatrixCM6";
            this.chkVehicleMatrixCM6.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM6.TabIndex = 54;
            this.chkVehicleMatrixCM6.Tag = "6";
            this.chkVehicleMatrixCM6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM6.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM6.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM7
            // 
            this.chkVehicleMatrixCM7.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM7.Location = new System.Drawing.Point(149, 6);
            this.chkVehicleMatrixCM7.Name = "chkVehicleMatrixCM7";
            this.chkVehicleMatrixCM7.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM7.TabIndex = 55;
            this.chkVehicleMatrixCM7.Tag = "7";
            this.chkVehicleMatrixCM7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM7.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM7.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM8
            // 
            this.chkVehicleMatrixCM8.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM8.Location = new System.Drawing.Point(173, 6);
            this.chkVehicleMatrixCM8.Name = "chkVehicleMatrixCM8";
            this.chkVehicleMatrixCM8.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM8.TabIndex = 56;
            this.chkVehicleMatrixCM8.Tag = "8";
            this.chkVehicleMatrixCM8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM8.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM8.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM9
            // 
            this.chkVehicleMatrixCM9.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM9.Location = new System.Drawing.Point(197, 6);
            this.chkVehicleMatrixCM9.Name = "chkVehicleMatrixCM9";
            this.chkVehicleMatrixCM9.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM9.TabIndex = 57;
            this.chkVehicleMatrixCM9.Tag = "9";
            this.chkVehicleMatrixCM9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM9.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM9.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM10
            // 
            this.chkVehicleMatrixCM10.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM10.Location = new System.Drawing.Point(221, 6);
            this.chkVehicleMatrixCM10.Name = "chkVehicleMatrixCM10";
            this.chkVehicleMatrixCM10.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM10.TabIndex = 58;
            this.chkVehicleMatrixCM10.Tag = "10";
            this.chkVehicleMatrixCM10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM10.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM10.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM11
            // 
            this.chkVehicleMatrixCM11.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM11.Location = new System.Drawing.Point(245, 6);
            this.chkVehicleMatrixCM11.Name = "chkVehicleMatrixCM11";
            this.chkVehicleMatrixCM11.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM11.TabIndex = 59;
            this.chkVehicleMatrixCM11.Tag = "11";
            this.chkVehicleMatrixCM11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM11.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM11.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM12
            // 
            this.chkVehicleMatrixCM12.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM12.Location = new System.Drawing.Point(269, 6);
            this.chkVehicleMatrixCM12.Name = "chkVehicleMatrixCM12";
            this.chkVehicleMatrixCM12.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM12.TabIndex = 60;
            this.chkVehicleMatrixCM12.Tag = "12";
            this.chkVehicleMatrixCM12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM12.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM12.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM13
            // 
            this.chkVehicleMatrixCM13.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM13.Location = new System.Drawing.Point(293, 6);
            this.chkVehicleMatrixCM13.Name = "chkVehicleMatrixCM13";
            this.chkVehicleMatrixCM13.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM13.TabIndex = 61;
            this.chkVehicleMatrixCM13.Tag = "13";
            this.chkVehicleMatrixCM13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM13.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM13.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM14
            // 
            this.chkVehicleMatrixCM14.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM14.Location = new System.Drawing.Point(317, 6);
            this.chkVehicleMatrixCM14.Name = "chkVehicleMatrixCM14";
            this.chkVehicleMatrixCM14.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM14.TabIndex = 62;
            this.chkVehicleMatrixCM14.Tag = "14";
            this.chkVehicleMatrixCM14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM14.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM14.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM15
            // 
            this.chkVehicleMatrixCM15.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM15.Location = new System.Drawing.Point(5, 31);
            this.chkVehicleMatrixCM15.Name = "chkVehicleMatrixCM15";
            this.chkVehicleMatrixCM15.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM15.TabIndex = 63;
            this.chkVehicleMatrixCM15.Tag = "15";
            this.chkVehicleMatrixCM15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM15.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM15.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM16
            // 
            this.chkVehicleMatrixCM16.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM16.Location = new System.Drawing.Point(29, 31);
            this.chkVehicleMatrixCM16.Name = "chkVehicleMatrixCM16";
            this.chkVehicleMatrixCM16.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM16.TabIndex = 64;
            this.chkVehicleMatrixCM16.Tag = "16";
            this.chkVehicleMatrixCM16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM16.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM16.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM17
            // 
            this.chkVehicleMatrixCM17.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM17.Location = new System.Drawing.Point(53, 31);
            this.chkVehicleMatrixCM17.Name = "chkVehicleMatrixCM17";
            this.chkVehicleMatrixCM17.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM17.TabIndex = 65;
            this.chkVehicleMatrixCM17.Tag = "17";
            this.chkVehicleMatrixCM17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM17.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM17.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM18
            // 
            this.chkVehicleMatrixCM18.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM18.Location = new System.Drawing.Point(77, 31);
            this.chkVehicleMatrixCM18.Name = "chkVehicleMatrixCM18";
            this.chkVehicleMatrixCM18.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM18.TabIndex = 66;
            this.chkVehicleMatrixCM18.Tag = "18";
            this.chkVehicleMatrixCM18.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM18.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM18.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM19
            // 
            this.chkVehicleMatrixCM19.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM19.Location = new System.Drawing.Point(101, 31);
            this.chkVehicleMatrixCM19.Name = "chkVehicleMatrixCM19";
            this.chkVehicleMatrixCM19.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM19.TabIndex = 67;
            this.chkVehicleMatrixCM19.Tag = "19";
            this.chkVehicleMatrixCM19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM19.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM19.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM20
            // 
            this.chkVehicleMatrixCM20.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM20.Location = new System.Drawing.Point(125, 31);
            this.chkVehicleMatrixCM20.Name = "chkVehicleMatrixCM20";
            this.chkVehicleMatrixCM20.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM20.TabIndex = 68;
            this.chkVehicleMatrixCM20.Tag = "20";
            this.chkVehicleMatrixCM20.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM20.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM20.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM21
            // 
            this.chkVehicleMatrixCM21.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM21.Location = new System.Drawing.Point(149, 31);
            this.chkVehicleMatrixCM21.Name = "chkVehicleMatrixCM21";
            this.chkVehicleMatrixCM21.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM21.TabIndex = 69;
            this.chkVehicleMatrixCM21.Tag = "21";
            this.chkVehicleMatrixCM21.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM21.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM21.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM22
            // 
            this.chkVehicleMatrixCM22.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM22.Location = new System.Drawing.Point(173, 31);
            this.chkVehicleMatrixCM22.Name = "chkVehicleMatrixCM22";
            this.chkVehicleMatrixCM22.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM22.TabIndex = 70;
            this.chkVehicleMatrixCM22.Tag = "22";
            this.chkVehicleMatrixCM22.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM22.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM22.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM23
            // 
            this.chkVehicleMatrixCM23.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM23.Location = new System.Drawing.Point(197, 31);
            this.chkVehicleMatrixCM23.Name = "chkVehicleMatrixCM23";
            this.chkVehicleMatrixCM23.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM23.TabIndex = 71;
            this.chkVehicleMatrixCM23.Tag = "23";
            this.chkVehicleMatrixCM23.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM23.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM23.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM24
            // 
            this.chkVehicleMatrixCM24.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM24.Location = new System.Drawing.Point(221, 31);
            this.chkVehicleMatrixCM24.Name = "chkVehicleMatrixCM24";
            this.chkVehicleMatrixCM24.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM24.TabIndex = 72;
            this.chkVehicleMatrixCM24.Tag = "24";
            this.chkVehicleMatrixCM24.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM24.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM24.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM25
            // 
            this.chkVehicleMatrixCM25.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM25.Location = new System.Drawing.Point(245, 31);
            this.chkVehicleMatrixCM25.Name = "chkVehicleMatrixCM25";
            this.chkVehicleMatrixCM25.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM25.TabIndex = 73;
            this.chkVehicleMatrixCM25.Tag = "25";
            this.chkVehicleMatrixCM25.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM25.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM25.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM26
            // 
            this.chkVehicleMatrixCM26.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM26.Location = new System.Drawing.Point(269, 31);
            this.chkVehicleMatrixCM26.Name = "chkVehicleMatrixCM26";
            this.chkVehicleMatrixCM26.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM26.TabIndex = 74;
            this.chkVehicleMatrixCM26.Tag = "26";
            this.chkVehicleMatrixCM26.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM26.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM26.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM27
            // 
            this.chkVehicleMatrixCM27.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM27.Location = new System.Drawing.Point(293, 31);
            this.chkVehicleMatrixCM27.Name = "chkVehicleMatrixCM27";
            this.chkVehicleMatrixCM27.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM27.TabIndex = 75;
            this.chkVehicleMatrixCM27.Tag = "27";
            this.chkVehicleMatrixCM27.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM27.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM27.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM28
            // 
            this.chkVehicleMatrixCM28.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM28.Location = new System.Drawing.Point(317, 31);
            this.chkVehicleMatrixCM28.Name = "chkVehicleMatrixCM28";
            this.chkVehicleMatrixCM28.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM28.TabIndex = 76;
            this.chkVehicleMatrixCM28.Tag = "28";
            this.chkVehicleMatrixCM28.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM28.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM28.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM29
            // 
            this.chkVehicleMatrixCM29.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM29.Location = new System.Drawing.Point(5, 55);
            this.chkVehicleMatrixCM29.Name = "chkVehicleMatrixCM29";
            this.chkVehicleMatrixCM29.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM29.TabIndex = 77;
            this.chkVehicleMatrixCM29.Tag = "29";
            this.chkVehicleMatrixCM29.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM29.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM29.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM30
            // 
            this.chkVehicleMatrixCM30.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM30.Location = new System.Drawing.Point(29, 55);
            this.chkVehicleMatrixCM30.Name = "chkVehicleMatrixCM30";
            this.chkVehicleMatrixCM30.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM30.TabIndex = 78;
            this.chkVehicleMatrixCM30.Tag = "30";
            this.chkVehicleMatrixCM30.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM30.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM30.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM31
            // 
            this.chkVehicleMatrixCM31.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM31.Location = new System.Drawing.Point(53, 55);
            this.chkVehicleMatrixCM31.Name = "chkVehicleMatrixCM31";
            this.chkVehicleMatrixCM31.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM31.TabIndex = 79;
            this.chkVehicleMatrixCM31.Tag = "31";
            this.chkVehicleMatrixCM31.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM31.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM31.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM32
            // 
            this.chkVehicleMatrixCM32.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM32.Location = new System.Drawing.Point(77, 55);
            this.chkVehicleMatrixCM32.Name = "chkVehicleMatrixCM32";
            this.chkVehicleMatrixCM32.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM32.TabIndex = 80;
            this.chkVehicleMatrixCM32.Tag = "32";
            this.chkVehicleMatrixCM32.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM32.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM32.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM33
            // 
            this.chkVehicleMatrixCM33.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM33.Location = new System.Drawing.Point(101, 55);
            this.chkVehicleMatrixCM33.Name = "chkVehicleMatrixCM33";
            this.chkVehicleMatrixCM33.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM33.TabIndex = 81;
            this.chkVehicleMatrixCM33.Tag = "33";
            this.chkVehicleMatrixCM33.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM33.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM33.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM34
            // 
            this.chkVehicleMatrixCM34.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM34.Location = new System.Drawing.Point(125, 55);
            this.chkVehicleMatrixCM34.Name = "chkVehicleMatrixCM34";
            this.chkVehicleMatrixCM34.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM34.TabIndex = 82;
            this.chkVehicleMatrixCM34.Tag = "34";
            this.chkVehicleMatrixCM34.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM34.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM34.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM35
            // 
            this.chkVehicleMatrixCM35.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM35.Location = new System.Drawing.Point(149, 55);
            this.chkVehicleMatrixCM35.Name = "chkVehicleMatrixCM35";
            this.chkVehicleMatrixCM35.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM35.TabIndex = 83;
            this.chkVehicleMatrixCM35.Tag = "35";
            this.chkVehicleMatrixCM35.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM35.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM35.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM36
            // 
            this.chkVehicleMatrixCM36.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM36.Location = new System.Drawing.Point(173, 55);
            this.chkVehicleMatrixCM36.Name = "chkVehicleMatrixCM36";
            this.chkVehicleMatrixCM36.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM36.TabIndex = 84;
            this.chkVehicleMatrixCM36.Tag = "36";
            this.chkVehicleMatrixCM36.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM36.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM36.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM37
            // 
            this.chkVehicleMatrixCM37.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM37.Location = new System.Drawing.Point(197, 55);
            this.chkVehicleMatrixCM37.Name = "chkVehicleMatrixCM37";
            this.chkVehicleMatrixCM37.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM37.TabIndex = 85;
            this.chkVehicleMatrixCM37.Tag = "37";
            this.chkVehicleMatrixCM37.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM37.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM37.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM38
            // 
            this.chkVehicleMatrixCM38.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM38.Location = new System.Drawing.Point(221, 55);
            this.chkVehicleMatrixCM38.Name = "chkVehicleMatrixCM38";
            this.chkVehicleMatrixCM38.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM38.TabIndex = 86;
            this.chkVehicleMatrixCM38.Tag = "38";
            this.chkVehicleMatrixCM38.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM38.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM38.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM39
            // 
            this.chkVehicleMatrixCM39.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM39.Location = new System.Drawing.Point(245, 55);
            this.chkVehicleMatrixCM39.Name = "chkVehicleMatrixCM39";
            this.chkVehicleMatrixCM39.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM39.TabIndex = 87;
            this.chkVehicleMatrixCM39.Tag = "39";
            this.chkVehicleMatrixCM39.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM39.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM39.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // chkVehicleMatrixCM40
            // 
            this.chkVehicleMatrixCM40.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVehicleMatrixCM40.Location = new System.Drawing.Point(269, 55);
            this.chkVehicleMatrixCM40.Name = "chkVehicleMatrixCM40";
            this.chkVehicleMatrixCM40.Size = new System.Drawing.Size(24, 24);
            this.chkVehicleMatrixCM40.TabIndex = 88;
            this.chkVehicleMatrixCM40.Tag = "40";
            this.chkVehicleMatrixCM40.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkVehicleMatrixCM40.UseVisualStyleBackColor = true;
            this.chkVehicleMatrixCM40.CheckedChanged += new System.EventHandler(this.chkVehicleCM_CheckedChanged);
            // 
            // lblVehicleFirewallLabel
            // 
            this.lblVehicleFirewallLabel.AutoSize = true;
            this.lblVehicleFirewallLabel.Location = new System.Drawing.Point(725, 128);
            this.lblVehicleFirewallLabel.Name = "lblVehicleFirewallLabel";
            this.lblVehicleFirewallLabel.Size = new System.Drawing.Size(45, 13);
            this.lblVehicleFirewallLabel.TabIndex = 200;
            this.lblVehicleFirewallLabel.Tag = "Label_Firewall";
            this.lblVehicleFirewallLabel.Text = "Firewall:";
            // 
            // lblVehicleDataProcessingLabel
            // 
            this.lblVehicleDataProcessingLabel.AutoSize = true;
            this.lblVehicleDataProcessingLabel.Location = new System.Drawing.Point(626, 128);
            this.lblVehicleDataProcessingLabel.Name = "lblVehicleDataProcessingLabel";
            this.lblVehicleDataProcessingLabel.Size = new System.Drawing.Size(58, 13);
            this.lblVehicleDataProcessingLabel.TabIndex = 198;
            this.lblVehicleDataProcessingLabel.Tag = "Label_DataProcessing";
            this.lblVehicleDataProcessingLabel.Text = "Data Proc:";
            // 
            // lblVehicleSleazeLabel
            // 
            this.lblVehicleSleazeLabel.AutoSize = true;
            this.lblVehicleSleazeLabel.Location = new System.Drawing.Point(533, 128);
            this.lblVehicleSleazeLabel.Name = "lblVehicleSleazeLabel";
            this.lblVehicleSleazeLabel.Size = new System.Drawing.Size(42, 13);
            this.lblVehicleSleazeLabel.TabIndex = 196;
            this.lblVehicleSleazeLabel.Tag = "Label_Sleaze";
            this.lblVehicleSleazeLabel.Text = "Sleaze:";
            // 
            // lblVehicleAttackLabel
            // 
            this.lblVehicleAttackLabel.AutoSize = true;
            this.lblVehicleAttackLabel.Location = new System.Drawing.Point(417, 128);
            this.lblVehicleAttackLabel.Name = "lblVehicleAttackLabel";
            this.lblVehicleAttackLabel.Size = new System.Drawing.Size(41, 13);
            this.lblVehicleAttackLabel.TabIndex = 194;
            this.lblVehicleAttackLabel.Tag = "Label_Attack";
            this.lblVehicleAttackLabel.Text = "Attack:";
            // 
            // cmdAddVehicleLocation
            // 
            this.cmdAddVehicleLocation.AutoSize = true;
            this.cmdAddVehicleLocation.Location = new System.Drawing.Point(220, 7);
            this.cmdAddVehicleLocation.Name = "cmdAddVehicleLocation";
            this.cmdAddVehicleLocation.Size = new System.Drawing.Size(80, 23);
            this.cmdAddVehicleLocation.TabIndex = 138;
            this.cmdAddVehicleLocation.Tag = "Button_AddLocation";
            this.cmdAddVehicleLocation.Text = "Add Location";
            this.cmdAddVehicleLocation.UseVisualStyleBackColor = true;
            this.cmdAddVehicleLocation.Click += new System.EventHandler(this.cmdAddVehicleLocation_Click);
            // 
            // chkVehicleHomeNode
            // 
            this.chkVehicleHomeNode.AutoSize = true;
            this.chkVehicleHomeNode.Location = new System.Drawing.Point(576, 248);
            this.chkVehicleHomeNode.Name = "chkVehicleHomeNode";
            this.chkVehicleHomeNode.Size = new System.Drawing.Size(83, 17);
            this.chkVehicleHomeNode.TabIndex = 135;
            this.chkVehicleHomeNode.Tag = "Checkbox_HomeNode";
            this.chkVehicleHomeNode.Text = "Home Node";
            this.chkVehicleHomeNode.UseVisualStyleBackColor = true;
            this.chkVehicleHomeNode.Visible = false;
            this.chkVehicleHomeNode.CheckedChanged += new System.EventHandler(this.chkVehicleHomeNode_CheckedChanged);
            // 
            // lblVehicleWeaponDicePool
            // 
            this.lblVehicleWeaponDicePool.AutoSize = true;
            this.lblVehicleWeaponDicePool.Location = new System.Drawing.Point(516, 465);
            this.lblVehicleWeaponDicePool.Name = "lblVehicleWeaponDicePool";
            this.lblVehicleWeaponDicePool.Size = new System.Drawing.Size(34, 13);
            this.lblVehicleWeaponDicePool.TabIndex = 134;
            this.lblVehicleWeaponDicePool.Text = "[Pool]";
            // 
            // lblVehicleWeaponDicePoolLabel
            // 
            this.lblVehicleWeaponDicePoolLabel.AutoSize = true;
            this.lblVehicleWeaponDicePoolLabel.Location = new System.Drawing.Point(418, 465);
            this.lblVehicleWeaponDicePoolLabel.Name = "lblVehicleWeaponDicePoolLabel";
            this.lblVehicleWeaponDicePoolLabel.Size = new System.Drawing.Size(56, 13);
            this.lblVehicleWeaponDicePoolLabel.TabIndex = 133;
            this.lblVehicleWeaponDicePoolLabel.Tag = "Label_DicePool";
            this.lblVehicleWeaponDicePoolLabel.Text = "Dice Pool:";
            // 
            // lblVehicleDevice
            // 
            this.lblVehicleDevice.AutoSize = true;
            this.lblVehicleDevice.Location = new System.Drawing.Point(774, 82);
            this.lblVehicleDevice.Name = "lblVehicleDevice";
            this.lblVehicleDevice.Size = new System.Drawing.Size(47, 13);
            this.lblVehicleDevice.TabIndex = 116;
            this.lblVehicleDevice.Text = "[Device]";
            // 
            // lblVehicleDeviceLabel
            // 
            this.lblVehicleDeviceLabel.AutoSize = true;
            this.lblVehicleDeviceLabel.Location = new System.Drawing.Point(725, 82);
            this.lblVehicleDeviceLabel.Name = "lblVehicleDeviceLabel";
            this.lblVehicleDeviceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblVehicleDeviceLabel.TabIndex = 115;
            this.lblVehicleDeviceLabel.Tag = "Label_Device";
            this.lblVehicleDeviceLabel.Text = "Device:";
            // 
            // cboVehicleWeaponAmmo
            // 
            this.cboVehicleWeaponAmmo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboVehicleWeaponAmmo.FormattingEnabled = true;
            this.cboVehicleWeaponAmmo.Location = new System.Drawing.Point(518, 439);
            this.cboVehicleWeaponAmmo.Name = "cboVehicleWeaponAmmo";
            this.cboVehicleWeaponAmmo.Size = new System.Drawing.Size(283, 21);
            this.cboVehicleWeaponAmmo.TabIndex = 114;
            this.cboVehicleWeaponAmmo.SelectedIndexChanged += new System.EventHandler(this.cboVehicleWeaponAmmo_SelectedIndexChanged);
            // 
            // lblVehicleGearQty
            // 
            this.lblVehicleGearQty.AutoSize = true;
            this.lblVehicleGearQty.Location = new System.Drawing.Point(488, 249);
            this.lblVehicleGearQty.Name = "lblVehicleGearQty";
            this.lblVehicleGearQty.Size = new System.Drawing.Size(44, 13);
            this.lblVehicleGearQty.TabIndex = 111;
            this.lblVehicleGearQty.Text = "[Rating]";
            // 
            // lblVehicleGearQtyLabel
            // 
            this.lblVehicleGearQtyLabel.AutoSize = true;
            this.lblVehicleGearQtyLabel.Location = new System.Drawing.Point(418, 249);
            this.lblVehicleGearQtyLabel.Name = "lblVehicleGearQtyLabel";
            this.lblVehicleGearQtyLabel.Size = new System.Drawing.Size(52, 13);
            this.lblVehicleGearQtyLabel.TabIndex = 110;
            this.lblVehicleGearQtyLabel.Tag = "Label_GearQty";
            this.lblVehicleGearQtyLabel.Text = "Gear Qty:";
            // 
            // lblVehicleWeaponRangeExtreme
            // 
            this.lblVehicleWeaponRangeExtreme.Location = new System.Drawing.Point(690, 387);
            this.lblVehicleWeaponRangeExtreme.Name = "lblVehicleWeaponRangeExtreme";
            this.lblVehicleWeaponRangeExtreme.Size = new System.Drawing.Size(64, 13);
            this.lblVehicleWeaponRangeExtreme.TabIndex = 107;
            this.lblVehicleWeaponRangeExtreme.Text = "[0]";
            this.lblVehicleWeaponRangeExtreme.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblVehicleWeaponRangeLong
            // 
            this.lblVehicleWeaponRangeLong.Location = new System.Drawing.Point(620, 387);
            this.lblVehicleWeaponRangeLong.Name = "lblVehicleWeaponRangeLong";
            this.lblVehicleWeaponRangeLong.Size = new System.Drawing.Size(64, 13);
            this.lblVehicleWeaponRangeLong.TabIndex = 106;
            this.lblVehicleWeaponRangeLong.Text = "[0]";
            this.lblVehicleWeaponRangeLong.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblVehicleWeaponRangeMedium
            // 
            this.lblVehicleWeaponRangeMedium.Location = new System.Drawing.Point(550, 387);
            this.lblVehicleWeaponRangeMedium.Name = "lblVehicleWeaponRangeMedium";
            this.lblVehicleWeaponRangeMedium.Size = new System.Drawing.Size(64, 13);
            this.lblVehicleWeaponRangeMedium.TabIndex = 105;
            this.lblVehicleWeaponRangeMedium.Text = "[0]";
            this.lblVehicleWeaponRangeMedium.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblVehicleWeaponRangeShort
            // 
            this.lblVehicleWeaponRangeShort.Location = new System.Drawing.Point(480, 387);
            this.lblVehicleWeaponRangeShort.Name = "lblVehicleWeaponRangeShort";
            this.lblVehicleWeaponRangeShort.Size = new System.Drawing.Size(64, 13);
            this.lblVehicleWeaponRangeShort.TabIndex = 104;
            this.lblVehicleWeaponRangeShort.Text = "[0]";
            this.lblVehicleWeaponRangeShort.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblVehicleWeaponRangeExtremeLabel
            // 
            this.lblVehicleWeaponRangeExtremeLabel.AutoSize = true;
            this.lblVehicleWeaponRangeExtremeLabel.Location = new System.Drawing.Point(692, 370);
            this.lblVehicleWeaponRangeExtremeLabel.Name = "lblVehicleWeaponRangeExtremeLabel";
            this.lblVehicleWeaponRangeExtremeLabel.Size = new System.Drawing.Size(63, 13);
            this.lblVehicleWeaponRangeExtremeLabel.TabIndex = 103;
            this.lblVehicleWeaponRangeExtremeLabel.Tag = "Label_RangeExtreme";
            this.lblVehicleWeaponRangeExtremeLabel.Text = "Extreme (-6)";
            this.lblVehicleWeaponRangeExtremeLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblVehicleWeaponRangeLongLabel
            // 
            this.lblVehicleWeaponRangeLongLabel.AutoSize = true;
            this.lblVehicleWeaponRangeLongLabel.Location = new System.Drawing.Point(629, 370);
            this.lblVehicleWeaponRangeLongLabel.Name = "lblVehicleWeaponRangeLongLabel";
            this.lblVehicleWeaponRangeLongLabel.Size = new System.Drawing.Size(49, 13);
            this.lblVehicleWeaponRangeLongLabel.TabIndex = 102;
            this.lblVehicleWeaponRangeLongLabel.Tag = "Label_RangeLong";
            this.lblVehicleWeaponRangeLongLabel.Text = "Long (-3)";
            this.lblVehicleWeaponRangeLongLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblVehicleWeaponRangeMediumLabel
            // 
            this.lblVehicleWeaponRangeMediumLabel.AutoSize = true;
            this.lblVehicleWeaponRangeMediumLabel.Location = new System.Drawing.Point(552, 370);
            this.lblVehicleWeaponRangeMediumLabel.Name = "lblVehicleWeaponRangeMediumLabel";
            this.lblVehicleWeaponRangeMediumLabel.Size = new System.Drawing.Size(62, 13);
            this.lblVehicleWeaponRangeMediumLabel.TabIndex = 101;
            this.lblVehicleWeaponRangeMediumLabel.Tag = "Label_RangeMedium";
            this.lblVehicleWeaponRangeMediumLabel.Text = "Medium (-1)";
            this.lblVehicleWeaponRangeMediumLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblVehicleWeaponRangeShortLabel
            // 
            this.lblVehicleWeaponRangeShortLabel.AutoSize = true;
            this.lblVehicleWeaponRangeShortLabel.Location = new System.Drawing.Point(488, 370);
            this.lblVehicleWeaponRangeShortLabel.Name = "lblVehicleWeaponRangeShortLabel";
            this.lblVehicleWeaponRangeShortLabel.Size = new System.Drawing.Size(50, 13);
            this.lblVehicleWeaponRangeShortLabel.TabIndex = 100;
            this.lblVehicleWeaponRangeShortLabel.Tag = "Label_RangeShort";
            this.lblVehicleWeaponRangeShortLabel.Text = "Short (-0)";
            this.lblVehicleWeaponRangeShortLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblVehicleWeaponRangeLabel
            // 
            this.lblVehicleWeaponRangeLabel.AutoSize = true;
            this.lblVehicleWeaponRangeLabel.Location = new System.Drawing.Point(417, 370);
            this.lblVehicleWeaponRangeLabel.Name = "lblVehicleWeaponRangeLabel";
            this.lblVehicleWeaponRangeLabel.Size = new System.Drawing.Size(39, 13);
            this.lblVehicleWeaponRangeLabel.TabIndex = 99;
            this.lblVehicleWeaponRangeLabel.Tag = "Label_RangeHeading";
            this.lblVehicleWeaponRangeLabel.Text = "Range";
            // 
            // chkVehicleIncludedInWeapon
            // 
            this.chkVehicleIncludedInWeapon.AutoSize = true;
            this.chkVehicleIncludedInWeapon.Enabled = false;
            this.chkVehicleIncludedInWeapon.Location = new System.Drawing.Point(576, 222);
            this.chkVehicleIncludedInWeapon.Name = "chkVehicleIncludedInWeapon";
            this.chkVehicleIncludedInWeapon.Size = new System.Drawing.Size(127, 17);
            this.chkVehicleIncludedInWeapon.TabIndex = 90;
            this.chkVehicleIncludedInWeapon.Tag = "Checkbox_BaseWeapon";
            this.chkVehicleIncludedInWeapon.Text = "Part of base Weapon";
            this.chkVehicleIncludedInWeapon.UseVisualStyleBackColor = true;
            // 
            // lblVehicleWeaponAmmo
            // 
            this.lblVehicleWeaponAmmo.AutoSize = true;
            this.lblVehicleWeaponAmmo.Location = new System.Drawing.Point(774, 342);
            this.lblVehicleWeaponAmmo.Name = "lblVehicleWeaponAmmo";
            this.lblVehicleWeaponAmmo.Size = new System.Drawing.Size(42, 13);
            this.lblVehicleWeaponAmmo.TabIndex = 88;
            this.lblVehicleWeaponAmmo.Text = "[Ammo]";
            // 
            // lblVehicleWeaponAmmoLabel
            // 
            this.lblVehicleWeaponAmmoLabel.AutoSize = true;
            this.lblVehicleWeaponAmmoLabel.Location = new System.Drawing.Point(725, 342);
            this.lblVehicleWeaponAmmoLabel.Name = "lblVehicleWeaponAmmoLabel";
            this.lblVehicleWeaponAmmoLabel.Size = new System.Drawing.Size(39, 13);
            this.lblVehicleWeaponAmmoLabel.TabIndex = 87;
            this.lblVehicleWeaponAmmoLabel.Tag = "Label_Ammo";
            this.lblVehicleWeaponAmmoLabel.Text = "Ammo:";
            // 
            // lblVehicleWeaponMode
            // 
            this.lblVehicleWeaponMode.AutoSize = true;
            this.lblVehicleWeaponMode.Location = new System.Drawing.Point(681, 342);
            this.lblVehicleWeaponMode.Name = "lblVehicleWeaponMode";
            this.lblVehicleWeaponMode.Size = new System.Drawing.Size(40, 13);
            this.lblVehicleWeaponMode.TabIndex = 86;
            this.lblVehicleWeaponMode.Text = "[Mode]";
            // 
            // lblVehicleWeaponModeLabel
            // 
            this.lblVehicleWeaponModeLabel.AutoSize = true;
            this.lblVehicleWeaponModeLabel.Location = new System.Drawing.Point(626, 342);
            this.lblVehicleWeaponModeLabel.Name = "lblVehicleWeaponModeLabel";
            this.lblVehicleWeaponModeLabel.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleWeaponModeLabel.TabIndex = 85;
            this.lblVehicleWeaponModeLabel.Tag = "Label_Mode";
            this.lblVehicleWeaponModeLabel.Text = "Mode:";
            // 
            // cmdReloadVehicleWeapon
            // 
            this.cmdReloadVehicleWeapon.Enabled = false;
            this.cmdReloadVehicleWeapon.Location = new System.Drawing.Point(723, 414);
            this.cmdReloadVehicleWeapon.Name = "cmdReloadVehicleWeapon";
            this.cmdReloadVehicleWeapon.Size = new System.Drawing.Size(79, 23);
            this.cmdReloadVehicleWeapon.TabIndex = 84;
            this.cmdReloadVehicleWeapon.Tag = "Button_Reload";
            this.cmdReloadVehicleWeapon.Text = "Reload";
            this.cmdReloadVehicleWeapon.UseVisualStyleBackColor = true;
            this.cmdReloadVehicleWeapon.Click += new System.EventHandler(this.cmdReloadVehicleWeapon_Click);
            // 
            // lblVehicleWeaponAmmoTypeLabel
            // 
            this.lblVehicleWeaponAmmoTypeLabel.AutoSize = true;
            this.lblVehicleWeaponAmmoTypeLabel.Location = new System.Drawing.Point(417, 442);
            this.lblVehicleWeaponAmmoTypeLabel.Name = "lblVehicleWeaponAmmoTypeLabel";
            this.lblVehicleWeaponAmmoTypeLabel.Size = new System.Drawing.Size(76, 13);
            this.lblVehicleWeaponAmmoTypeLabel.TabIndex = 82;
            this.lblVehicleWeaponAmmoTypeLabel.Tag = "Label_CurrentAmmo";
            this.lblVehicleWeaponAmmoTypeLabel.Text = "Current Ammo:";
            // 
            // lblVehicleWeaponAmmoRemaining
            // 
            this.lblVehicleWeaponAmmoRemaining.AutoSize = true;
            this.lblVehicleWeaponAmmoRemaining.Location = new System.Drawing.Point(515, 419);
            this.lblVehicleWeaponAmmoRemaining.Name = "lblVehicleWeaponAmmoRemaining";
            this.lblVehicleWeaponAmmoRemaining.Size = new System.Drawing.Size(95, 13);
            this.lblVehicleWeaponAmmoRemaining.TabIndex = 80;
            this.lblVehicleWeaponAmmoRemaining.Text = "[Ammo Remaining]";
            // 
            // lblVehicleWeaponAmmoRemainingLabel
            // 
            this.lblVehicleWeaponAmmoRemainingLabel.AutoSize = true;
            this.lblVehicleWeaponAmmoRemainingLabel.Location = new System.Drawing.Point(417, 419);
            this.lblVehicleWeaponAmmoRemainingLabel.Name = "lblVehicleWeaponAmmoRemainingLabel";
            this.lblVehicleWeaponAmmoRemainingLabel.Size = new System.Drawing.Size(92, 13);
            this.lblVehicleWeaponAmmoRemainingLabel.TabIndex = 79;
            this.lblVehicleWeaponAmmoRemainingLabel.Tag = "Label_AmmoRemaining";
            this.lblVehicleWeaponAmmoRemainingLabel.Text = "Ammo Remaining:";
            // 
            // lblVehicleWeaponNameLabel
            // 
            this.lblVehicleWeaponNameLabel.AutoSize = true;
            this.lblVehicleWeaponNameLabel.Location = new System.Drawing.Point(417, 296);
            this.lblVehicleWeaponNameLabel.Name = "lblVehicleWeaponNameLabel";
            this.lblVehicleWeaponNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblVehicleWeaponNameLabel.TabIndex = 63;
            this.lblVehicleWeaponNameLabel.Tag = "Label_Name";
            this.lblVehicleWeaponNameLabel.Text = "Name:";
            // 
            // lblVehicleWeaponName
            // 
            this.lblVehicleWeaponName.AutoSize = true;
            this.lblVehicleWeaponName.Location = new System.Drawing.Point(487, 296);
            this.lblVehicleWeaponName.Name = "lblVehicleWeaponName";
            this.lblVehicleWeaponName.Size = new System.Drawing.Size(41, 13);
            this.lblVehicleWeaponName.TabIndex = 64;
            this.lblVehicleWeaponName.Text = "[Name]";
            // 
            // lblVehicleWeaponCategoryLabel
            // 
            this.lblVehicleWeaponCategoryLabel.AutoSize = true;
            this.lblVehicleWeaponCategoryLabel.Location = new System.Drawing.Point(417, 319);
            this.lblVehicleWeaponCategoryLabel.Name = "lblVehicleWeaponCategoryLabel";
            this.lblVehicleWeaponCategoryLabel.Size = new System.Drawing.Size(52, 13);
            this.lblVehicleWeaponCategoryLabel.TabIndex = 65;
            this.lblVehicleWeaponCategoryLabel.Tag = "Label_Category";
            this.lblVehicleWeaponCategoryLabel.Text = "Category:";
            // 
            // lblVehicleWeaponAP
            // 
            this.lblVehicleWeaponAP.AutoSize = true;
            this.lblVehicleWeaponAP.Location = new System.Drawing.Point(574, 342);
            this.lblVehicleWeaponAP.Name = "lblVehicleWeaponAP";
            this.lblVehicleWeaponAP.Size = new System.Drawing.Size(27, 13);
            this.lblVehicleWeaponAP.TabIndex = 72;
            this.lblVehicleWeaponAP.Text = "[AP]";
            // 
            // lblVehicleWeaponCategory
            // 
            this.lblVehicleWeaponCategory.AutoSize = true;
            this.lblVehicleWeaponCategory.Location = new System.Drawing.Point(486, 319);
            this.lblVehicleWeaponCategory.Name = "lblVehicleWeaponCategory";
            this.lblVehicleWeaponCategory.Size = new System.Drawing.Size(55, 13);
            this.lblVehicleWeaponCategory.TabIndex = 66;
            this.lblVehicleWeaponCategory.Text = "[Category]";
            // 
            // lblVehicleWeaponAPLabel
            // 
            this.lblVehicleWeaponAPLabel.AutoSize = true;
            this.lblVehicleWeaponAPLabel.Location = new System.Drawing.Point(538, 342);
            this.lblVehicleWeaponAPLabel.Name = "lblVehicleWeaponAPLabel";
            this.lblVehicleWeaponAPLabel.Size = new System.Drawing.Size(24, 13);
            this.lblVehicleWeaponAPLabel.TabIndex = 71;
            this.lblVehicleWeaponAPLabel.Tag = "Label_AP";
            this.lblVehicleWeaponAPLabel.Text = "AP:";
            // 
            // lblVehicleWeaponDamageLabel
            // 
            this.lblVehicleWeaponDamageLabel.AutoSize = true;
            this.lblVehicleWeaponDamageLabel.Location = new System.Drawing.Point(417, 342);
            this.lblVehicleWeaponDamageLabel.Name = "lblVehicleWeaponDamageLabel";
            this.lblVehicleWeaponDamageLabel.Size = new System.Drawing.Size(50, 13);
            this.lblVehicleWeaponDamageLabel.TabIndex = 67;
            this.lblVehicleWeaponDamageLabel.Tag = "Label_Damage";
            this.lblVehicleWeaponDamageLabel.Text = "Damage:";
            // 
            // lblVehicleWeaponDamage
            // 
            this.lblVehicleWeaponDamage.AutoSize = true;
            this.lblVehicleWeaponDamage.Location = new System.Drawing.Point(486, 342);
            this.lblVehicleWeaponDamage.Name = "lblVehicleWeaponDamage";
            this.lblVehicleWeaponDamage.Size = new System.Drawing.Size(53, 13);
            this.lblVehicleWeaponDamage.TabIndex = 68;
            this.lblVehicleWeaponDamage.Text = "[Damage]";
            // 
            // lblVehicleRating
            // 
            this.lblVehicleRating.AutoSize = true;
            this.lblVehicleRating.Location = new System.Drawing.Point(488, 226);
            this.lblVehicleRating.Name = "lblVehicleRating";
            this.lblVehicleRating.Size = new System.Drawing.Size(44, 13);
            this.lblVehicleRating.TabIndex = 61;
            this.lblVehicleRating.Text = "[Rating]";
            // 
            // lblVehicleSource
            // 
            this.lblVehicleSource.AutoSize = true;
            this.lblVehicleSource.Location = new System.Drawing.Point(487, 272);
            this.lblVehicleSource.Name = "lblVehicleSource";
            this.lblVehicleSource.Size = new System.Drawing.Size(47, 13);
            this.lblVehicleSource.TabIndex = 60;
            this.lblVehicleSource.Text = "[Source]";
            this.lblVehicleSource.Click += new System.EventHandler(this.lblVehicleSource_Click);
            // 
            // lblVehicleSourceLabel
            // 
            this.lblVehicleSourceLabel.AutoSize = true;
            this.lblVehicleSourceLabel.Location = new System.Drawing.Point(418, 272);
            this.lblVehicleSourceLabel.Name = "lblVehicleSourceLabel";
            this.lblVehicleSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblVehicleSourceLabel.TabIndex = 59;
            this.lblVehicleSourceLabel.Tag = "Label_Source";
            this.lblVehicleSourceLabel.Text = "Source:";
            // 
            // lblVehicleSlots
            // 
            this.lblVehicleSlots.AutoSize = true;
            this.lblVehicleSlots.Location = new System.Drawing.Point(671, 152);
            this.lblVehicleSlots.Name = "lblVehicleSlots";
            this.lblVehicleSlots.Size = new System.Drawing.Size(36, 13);
            this.lblVehicleSlots.TabIndex = 58;
            this.lblVehicleSlots.Text = "[Slots]";
            // 
            // lblVehicleSlotsLabel
            // 
            this.lblVehicleSlotsLabel.AutoSize = true;
            this.lblVehicleSlotsLabel.Location = new System.Drawing.Point(632, 152);
            this.lblVehicleSlotsLabel.Name = "lblVehicleSlotsLabel";
            this.lblVehicleSlotsLabel.Size = new System.Drawing.Size(33, 13);
            this.lblVehicleSlotsLabel.TabIndex = 57;
            this.lblVehicleSlotsLabel.Tag = "Label_Slots";
            this.lblVehicleSlotsLabel.Text = "Slots:";
            // 
            // lblVehicleRatingLabel
            // 
            this.lblVehicleRatingLabel.AutoSize = true;
            this.lblVehicleRatingLabel.Location = new System.Drawing.Point(418, 226);
            this.lblVehicleRatingLabel.Name = "lblVehicleRatingLabel";
            this.lblVehicleRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblVehicleRatingLabel.TabIndex = 55;
            this.lblVehicleRatingLabel.Tag = "Label_Rating";
            this.lblVehicleRatingLabel.Text = "Rating:";
            // 
            // lblVehicleNameLabel
            // 
            this.lblVehicleNameLabel.AutoSize = true;
            this.lblVehicleNameLabel.Location = new System.Drawing.Point(417, 36);
            this.lblVehicleNameLabel.Name = "lblVehicleNameLabel";
            this.lblVehicleNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblVehicleNameLabel.TabIndex = 51;
            this.lblVehicleNameLabel.Tag = "Label_Name";
            this.lblVehicleNameLabel.Text = "Name:";
            // 
            // lblVehicleName
            // 
            this.lblVehicleName.AutoSize = true;
            this.lblVehicleName.Location = new System.Drawing.Point(487, 36);
            this.lblVehicleName.Name = "lblVehicleName";
            this.lblVehicleName.Size = new System.Drawing.Size(41, 13);
            this.lblVehicleName.TabIndex = 52;
            this.lblVehicleName.Text = "[Name]";
            // 
            // lblVehicleCategoryLabel
            // 
            this.lblVehicleCategoryLabel.AutoSize = true;
            this.lblVehicleCategoryLabel.Location = new System.Drawing.Point(417, 59);
            this.lblVehicleCategoryLabel.Name = "lblVehicleCategoryLabel";
            this.lblVehicleCategoryLabel.Size = new System.Drawing.Size(52, 13);
            this.lblVehicleCategoryLabel.TabIndex = 53;
            this.lblVehicleCategoryLabel.Tag = "Label_Category";
            this.lblVehicleCategoryLabel.Text = "Category:";
            // 
            // lblVehicleCategory
            // 
            this.lblVehicleCategory.AutoSize = true;
            this.lblVehicleCategory.Location = new System.Drawing.Point(487, 59);
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
            this.lblVehiclePilot.Location = new System.Drawing.Point(487, 104);
            this.lblVehiclePilot.Name = "lblVehiclePilot";
            this.lblVehiclePilot.Size = new System.Drawing.Size(33, 13);
            this.lblVehiclePilot.TabIndex = 40;
            this.lblVehiclePilot.Text = "[Pilot]";
            // 
            // lblVehiclePilotLabel
            // 
            this.lblVehiclePilotLabel.AutoSize = true;
            this.lblVehiclePilotLabel.Location = new System.Drawing.Point(417, 104);
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
            this.lblVehicleCost.Location = new System.Drawing.Point(574, 152);
            this.lblVehicleCost.Name = "lblVehicleCost";
            this.lblVehicleCost.Size = new System.Drawing.Size(34, 13);
            this.lblVehicleCost.TabIndex = 50;
            this.lblVehicleCost.Text = "[Cost]";
            // 
            // lblVehicleCostLabel
            // 
            this.lblVehicleCostLabel.AutoSize = true;
            this.lblVehicleCostLabel.Location = new System.Drawing.Point(534, 152);
            this.lblVehicleCostLabel.Name = "lblVehicleCostLabel";
            this.lblVehicleCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblVehicleCostLabel.TabIndex = 49;
            this.lblVehicleCostLabel.Tag = "Label_Cost";
            this.lblVehicleCostLabel.Text = "Cost:";
            // 
            // lblVehicleAvail
            // 
            this.lblVehicleAvail.AutoSize = true;
            this.lblVehicleAvail.Location = new System.Drawing.Point(488, 152);
            this.lblVehicleAvail.Name = "lblVehicleAvail";
            this.lblVehicleAvail.Size = new System.Drawing.Size(36, 13);
            this.lblVehicleAvail.TabIndex = 48;
            this.lblVehicleAvail.Text = "[Avail]";
            // 
            // lblVehicleAvailLabel
            // 
            this.lblVehicleAvailLabel.AutoSize = true;
            this.lblVehicleAvailLabel.Location = new System.Drawing.Point(418, 152);
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
            this.lblVehicleHandlingLabel.Location = new System.Drawing.Point(417, 82);
            this.lblVehicleHandlingLabel.Name = "lblVehicleHandlingLabel";
            this.lblVehicleHandlingLabel.Size = new System.Drawing.Size(52, 13);
            this.lblVehicleHandlingLabel.TabIndex = 33;
            this.lblVehicleHandlingLabel.Tag = "Label_Handling";
            this.lblVehicleHandlingLabel.Text = "Handling:";
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
            this.treVehicles.Size = new System.Drawing.Size(403, 339);
            this.treVehicles.TabIndex = 30;
            this.treVehicles.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treVehicles_AfterSelect);
            this.treVehicles.DragOver += new System.Windows.Forms.DragEventHandler(this.treVehicles_DragOver);
            this.treVehicles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treVehicles_KeyDown);
            this.treVehicles.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            // 
            // cmdRollVehicleWeapon
            // 
            this.cmdRollVehicleWeapon.FlatAppearance.BorderSize = 0;
            this.cmdRollVehicleWeapon.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdRollVehicleWeapon.Image = global::Chummer.Properties.Resources.die;
            this.cmdRollVehicleWeapon.Location = new System.Drawing.Point(556, 461);
            this.cmdRollVehicleWeapon.Name = "cmdRollVehicleWeapon";
            this.cmdRollVehicleWeapon.Size = new System.Drawing.Size(24, 24);
            this.cmdRollVehicleWeapon.TabIndex = 137;
            this.cmdRollVehicleWeapon.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.cmdRollVehicleWeapon.UseVisualStyleBackColor = true;
            this.cmdRollVehicleWeapon.Visible = false;
            this.cmdRollVehicleWeapon.Click += new System.EventHandler(this.cmdRollVehicleWeapon_Click);
            // 
            // cmdAddVehicle
            // 
            this.cmdAddVehicle.AutoSize = true;
            this.cmdAddVehicle.ContextMenuStrip = this.cmsVehicle;
            this.cmdAddVehicle.Location = new System.Drawing.Point(8, 7);
            this.cmdAddVehicle.Name = "cmdAddVehicle";
            this.cmdAddVehicle.Size = new System.Drawing.Size(92, 23);
            this.cmdAddVehicle.SplitMenuStrip = this.cmsVehicle;
            this.cmdAddVehicle.TabIndex = 113;
            this.cmdAddVehicle.Tag = "Button_AddVehicle";
            this.cmdAddVehicle.Text = "&Add Vehicle";
            this.cmdAddVehicle.UseVisualStyleBackColor = true;
            this.cmdAddVehicle.Click += new System.EventHandler(this.cmdAddVehicle_Click);
            // 
            // cmdFireVehicleWeapon
            // 
            this.cmdFireVehicleWeapon.AutoSize = true;
            this.cmdFireVehicleWeapon.ContextMenuStrip = this.cmdVehicleAmmoExpense;
            this.cmdFireVehicleWeapon.Enabled = false;
            this.cmdFireVehicleWeapon.Location = new System.Drawing.Point(638, 414);
            this.cmdFireVehicleWeapon.Name = "cmdFireVehicleWeapon";
            this.cmdFireVehicleWeapon.Size = new System.Drawing.Size(79, 23);
            this.cmdFireVehicleWeapon.SplitMenuStrip = this.cmdVehicleAmmoExpense;
            this.cmdFireVehicleWeapon.TabIndex = 81;
            this.cmdFireVehicleWeapon.Tag = "Button_Fire";
            this.cmdFireVehicleWeapon.Text = "FIRE!";
            this.cmdFireVehicleWeapon.UseVisualStyleBackColor = true;
            this.cmdFireVehicleWeapon.Click += new System.EventHandler(this.cmdFireVehicleWeapon_Click);
            // 
            // cmdDeleteVehicle
            // 
            this.cmdDeleteVehicle.AutoSize = true;
            this.cmdDeleteVehicle.ContextMenuStrip = this.cmsDeleteVehicle;
            this.cmdDeleteVehicle.Location = new System.Drawing.Point(104, 7);
            this.cmdDeleteVehicle.Name = "cmdDeleteVehicle";
            this.cmdDeleteVehicle.Size = new System.Drawing.Size(80, 23);
            this.cmdDeleteVehicle.SplitMenuStrip = this.cmsDeleteVehicle;
            this.cmdDeleteVehicle.TabIndex = 62;
            this.cmdDeleteVehicle.Tag = "String_Delete";
            this.cmdDeleteVehicle.Text = "Delete";
            this.cmdDeleteVehicle.UseVisualStyleBackColor = true;
            this.cmdDeleteVehicle.Click += new System.EventHandler(this.cmdDeleteVehicle_Click);
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
            this.tabCharacterInfo.Controls.Add(this.cmdBurnStreetCred);
            this.tabCharacterInfo.Controls.Add(this.lblPublicAwareTotal);
            this.tabCharacterInfo.Controls.Add(this.lblNotorietyTotal);
            this.tabCharacterInfo.Controls.Add(this.lblStreetCredTotal);
            this.tabCharacterInfo.Controls.Add(this.lblCharacterName);
            this.tabCharacterInfo.Controls.Add(this.txtCharacterName);
            this.tabCharacterInfo.Controls.Add(this.txtPlayerName);
            this.tabCharacterInfo.Controls.Add(this.txtNotes);
            this.tabCharacterInfo.Controls.Add(this.txtConcept);
            this.tabCharacterInfo.Controls.Add(this.txtBackground);
            this.tabCharacterInfo.Controls.Add(this.txtDescription);
            this.tabCharacterInfo.Controls.Add(this.txtSkin);
            this.tabCharacterInfo.Controls.Add(this.txtWeight);
            this.tabCharacterInfo.Controls.Add(this.txtHeight);
            this.tabCharacterInfo.Controls.Add(this.txtHair);
            this.tabCharacterInfo.Controls.Add(this.txtEyes);
            this.tabCharacterInfo.Controls.Add(this.txtAge);
            this.tabCharacterInfo.Controls.Add(this.txtSex);
            this.tabCharacterInfo.Controls.Add(this.nudPublicAware);
            this.tabCharacterInfo.Controls.Add(this.lblPublicAware);
            this.tabCharacterInfo.Controls.Add(this.nudNotoriety);
            this.tabCharacterInfo.Controls.Add(this.lblNotoriety);
            this.tabCharacterInfo.Controls.Add(this.nudStreetCred);
            this.tabCharacterInfo.Controls.Add(this.lblStreetCred);
            this.tabCharacterInfo.Controls.Add(this.lblPlayerName);
            this.tabCharacterInfo.Controls.Add(this.lblNotes);
            this.tabCharacterInfo.Controls.Add(this.cmdDeleteMugshot);
            this.tabCharacterInfo.Controls.Add(this.cmdAddMugshot);
            this.tabCharacterInfo.Controls.Add(this.lblMugshot);
            this.tabCharacterInfo.Controls.Add(this.lblConcept);
            this.tabCharacterInfo.Controls.Add(this.lblBackground);
            this.tabCharacterInfo.Controls.Add(this.lblDescription);
            this.tabCharacterInfo.Controls.Add(this.lblSkin);
            this.tabCharacterInfo.Controls.Add(this.lblWeight);
            this.tabCharacterInfo.Controls.Add(this.lblHeight);
            this.tabCharacterInfo.Controls.Add(this.lblHair);
            this.tabCharacterInfo.Controls.Add(this.lblEyes);
            this.tabCharacterInfo.Controls.Add(this.lblAge);
            this.tabCharacterInfo.Controls.Add(this.lblSex);
            this.tabCharacterInfo.Controls.Add(this.picMugshot);
            this.tabCharacterInfo.Location = new System.Drawing.Point(4, 22);
            this.tabCharacterInfo.Name = "tabCharacterInfo";
            this.tabCharacterInfo.Size = new System.Drawing.Size(861, 586);
            this.tabCharacterInfo.TabIndex = 9;
            this.tabCharacterInfo.Tag = "Tab_CharacterInfo";
            this.tabCharacterInfo.Text = "Character Info";
            // 
            // cboPrimaryArm
            // 
            this.cboPrimaryArm.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPrimaryArm.FormattingEnabled = true;
            this.cboPrimaryArm.Location = new System.Drawing.Point(731, 66);
            this.cboPrimaryArm.Name = "cboPrimaryArm";
            this.cboPrimaryArm.Size = new System.Drawing.Size(100, 21);
            this.cboPrimaryArm.TabIndex = 95;
            this.cboPrimaryArm.SelectedIndexChanged += new System.EventHandler(this.cboPrimaryArm_SelectedIndexChanged);
            // 
            // lblHandedness
            // 
            this.lblHandedness.AutoSize = true;
            this.lblHandedness.Location = new System.Drawing.Point(654, 69);
            this.lblHandedness.Name = "lblHandedness";
            this.lblHandedness.Size = new System.Drawing.Size(70, 13);
            this.lblHandedness.TabIndex = 94;
            this.lblHandedness.Tag = "Label_Handedness";
            this.lblHandedness.Text = "Handedness:";
            // 
            // chkIsMainMugshot
            // 
            this.chkIsMainMugshot.AutoSize = true;
            this.chkIsMainMugshot.Location = new System.Drawing.Point(644, 237);
            this.chkIsMainMugshot.Name = "chkIsMainMugshot";
            this.chkIsMainMugshot.Size = new System.Drawing.Size(104, 17);
            this.chkIsMainMugshot.TabIndex = 97;
            this.chkIsMainMugshot.Text = "Is Main Mugshot";
            this.chkIsMainMugshot.UseVisualStyleBackColor = true;
            this.chkIsMainMugshot.CheckedChanged += new System.EventHandler(this.chkIsMainMugshot_CheckedChanged);
            // 
            // lblNumMugshots
            // 
            this.lblNumMugshots.AutoSize = true;
            this.lblNumMugshots.Location = new System.Drawing.Point(762, 185);
            this.lblNumMugshots.Name = "lblNumMugshots";
            this.lblNumMugshots.Size = new System.Drawing.Size(21, 13);
            this.lblNumMugshots.TabIndex = 96;
            this.lblNumMugshots.Tag = "";
            this.lblNumMugshots.Text = "/ 0";
            // 
            // nudMugshotIndex
            // 
            this.nudMugshotIndex.Location = new System.Drawing.Point(719, 182);
            this.nudMugshotIndex.Name = "nudMugshotIndex";
            this.nudMugshotIndex.Size = new System.Drawing.Size(42, 20);
            this.nudMugshotIndex.TabIndex = 95;
            this.nudMugshotIndex.ValueChanged += new System.EventHandler(this.nudMugshotIndex_ValueChanged);
            // 
            // lblMugshotDimensions
            // 
            this.lblMugshotDimensions.AutoSize = true;
            this.lblMugshotDimensions.Location = new System.Drawing.Point(771, 213);
            this.lblMugshotDimensions.Name = "lblMugshotDimensions";
            this.lblMugshotDimensions.Size = new System.Drawing.Size(78, 13);
            this.lblMugshotDimensions.TabIndex = 93;
            this.lblMugshotDimensions.Tag = "Label_MugshotDimensions";
            this.lblMugshotDimensions.Text = "210px X 310px";
            // 
            // lblPublicAwareTotal
            // 
            this.lblPublicAwareTotal.AutoSize = true;
            this.lblPublicAwareTotal.Location = new System.Drawing.Point(779, 142);
            this.lblPublicAwareTotal.Name = "lblPublicAwareTotal";
            this.lblPublicAwareTotal.Size = new System.Drawing.Size(19, 13);
            this.lblPublicAwareTotal.TabIndex = 81;
            this.lblPublicAwareTotal.Tag = "Label_StreetCred";
            this.lblPublicAwareTotal.Text = "[0]";
            // 
            // lblNotorietyTotal
            // 
            this.lblNotorietyTotal.AutoSize = true;
            this.lblNotorietyTotal.Location = new System.Drawing.Point(779, 119);
            this.lblNotorietyTotal.Name = "lblNotorietyTotal";
            this.lblNotorietyTotal.Size = new System.Drawing.Size(19, 13);
            this.lblNotorietyTotal.TabIndex = 80;
            this.lblNotorietyTotal.Tag = "Label_StreetCred";
            this.lblNotorietyTotal.Text = "[0]";
            // 
            // lblStreetCredTotal
            // 
            this.lblStreetCredTotal.AutoSize = true;
            this.lblStreetCredTotal.Location = new System.Drawing.Point(779, 97);
            this.lblStreetCredTotal.Name = "lblStreetCredTotal";
            this.lblStreetCredTotal.Size = new System.Drawing.Size(19, 13);
            this.lblStreetCredTotal.TabIndex = 79;
            this.lblStreetCredTotal.Tag = "Label_StreetCred";
            this.lblStreetCredTotal.Text = "[0]";
            // 
            // lblCharacterName
            // 
            this.lblCharacterName.AutoSize = true;
            this.lblCharacterName.Location = new System.Drawing.Point(493, 38);
            this.lblCharacterName.Name = "lblCharacterName";
            this.lblCharacterName.Size = new System.Drawing.Size(38, 13);
            this.lblCharacterName.TabIndex = 77;
            this.lblCharacterName.Tag = "Label_CharacterName";
            this.lblCharacterName.Text = "Name:";
            // 
            // txtCharacterName
            // 
            this.txtCharacterName.Location = new System.Drawing.Point(543, 35);
            this.txtCharacterName.Name = "txtCharacterName";
            this.txtCharacterName.Size = new System.Drawing.Size(100, 20);
            this.txtCharacterName.TabIndex = 78;
            this.txtCharacterName.TextChanged += new System.EventHandler(this.txtCharacterName_TextChanged);
            // 
            // txtPlayerName
            // 
            this.txtPlayerName.Location = new System.Drawing.Point(705, 35);
            this.txtPlayerName.Name = "txtPlayerName";
            this.txtPlayerName.Size = new System.Drawing.Size(100, 20);
            this.txtPlayerName.TabIndex = 66;
            this.txtPlayerName.TextChanged += new System.EventHandler(this.txtPlayerName_TextChanged);
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
            // txtBackground
            // 
            this.txtBackground.Location = new System.Drawing.Point(10, 200);
            this.txtBackground.Multiline = true;
            this.txtBackground.Name = "txtBackground";
            this.txtBackground.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBackground.Size = new System.Drawing.Size(618, 96);
            this.txtBackground.TabIndex = 17;
            this.txtBackground.TextChanged += new System.EventHandler(this.txtBackground_TextChanged);
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
            // txtSkin
            // 
            this.txtSkin.Location = new System.Drawing.Point(375, 35);
            this.txtSkin.Name = "txtSkin";
            this.txtSkin.Size = new System.Drawing.Size(100, 20);
            this.txtSkin.TabIndex = 13;
            this.txtSkin.TextChanged += new System.EventHandler(this.txtSkin_TextChanged);
            // 
            // txtWeight
            // 
            this.txtWeight.Location = new System.Drawing.Point(222, 35);
            this.txtWeight.Name = "txtWeight";
            this.txtWeight.Size = new System.Drawing.Size(100, 20);
            this.txtWeight.TabIndex = 11;
            this.txtWeight.TextChanged += new System.EventHandler(this.txtWeight_TextChanged);
            // 
            // txtHeight
            // 
            this.txtHeight.Location = new System.Drawing.Point(54, 35);
            this.txtHeight.Name = "txtHeight";
            this.txtHeight.Size = new System.Drawing.Size(100, 20);
            this.txtHeight.TabIndex = 9;
            this.txtHeight.TextChanged += new System.EventHandler(this.txtHeight_TextChanged);
            // 
            // txtHair
            // 
            this.txtHair.Location = new System.Drawing.Point(543, 9);
            this.txtHair.Name = "txtHair";
            this.txtHair.Size = new System.Drawing.Size(100, 20);
            this.txtHair.TabIndex = 7;
            this.txtHair.TextChanged += new System.EventHandler(this.txtHair_TextChanged);
            // 
            // txtEyes
            // 
            this.txtEyes.Location = new System.Drawing.Point(375, 9);
            this.txtEyes.Name = "txtEyes";
            this.txtEyes.Size = new System.Drawing.Size(100, 20);
            this.txtEyes.TabIndex = 5;
            this.txtEyes.TextChanged += new System.EventHandler(this.txtEyes_TextChanged);
            // 
            // txtAge
            // 
            this.txtAge.Location = new System.Drawing.Point(222, 9);
            this.txtAge.Name = "txtAge";
            this.txtAge.Size = new System.Drawing.Size(100, 20);
            this.txtAge.TabIndex = 3;
            this.txtAge.TextChanged += new System.EventHandler(this.txtAge_TextChanged);
            // 
            // txtSex
            // 
            this.txtSex.Location = new System.Drawing.Point(54, 9);
            this.txtSex.Name = "txtSex";
            this.txtSex.Size = new System.Drawing.Size(100, 20);
            this.txtSex.TabIndex = 1;
            this.txtSex.TextChanged += new System.EventHandler(this.txtSex_TextChanged);
            // 
            // nudPublicAware
            // 
            this.nudPublicAware.Location = new System.Drawing.Point(731, 140);
            this.nudPublicAware.Name = "nudPublicAware";
            this.nudPublicAware.Size = new System.Drawing.Size(42, 20);
            this.nudPublicAware.TabIndex = 76;
            this.nudPublicAware.ValueChanged += new System.EventHandler(this.nudPublicAware_ValueChanged);
            // 
            // lblPublicAware
            // 
            this.lblPublicAware.AutoSize = true;
            this.lblPublicAware.Location = new System.Drawing.Point(654, 142);
            this.lblPublicAware.Name = "lblPublicAware";
            this.lblPublicAware.Size = new System.Drawing.Size(72, 13);
            this.lblPublicAware.TabIndex = 75;
            this.lblPublicAware.Tag = "Label_PublicAwareness";
            this.lblPublicAware.Text = "Public Aware:";
            // 
            // nudNotoriety
            // 
            this.nudNotoriety.Location = new System.Drawing.Point(731, 117);
            this.nudNotoriety.Name = "nudNotoriety";
            this.nudNotoriety.Size = new System.Drawing.Size(42, 20);
            this.nudNotoriety.TabIndex = 74;
            this.nudNotoriety.ValueChanged += new System.EventHandler(this.nudNotoriety_ValueChanged);
            // 
            // nudStreetCred
            // 
            this.nudStreetCred.Location = new System.Drawing.Point(731, 94);
            this.nudStreetCred.Name = "nudStreetCred";
            this.nudStreetCred.Size = new System.Drawing.Size(42, 20);
            this.nudStreetCred.TabIndex = 72;
            this.nudStreetCred.ValueChanged += new System.EventHandler(this.nudStreetCred_ValueChanged);
            // 
            // lblPlayerName
            // 
            this.lblPlayerName.AutoSize = true;
            this.lblPlayerName.Location = new System.Drawing.Point(654, 38);
            this.lblPlayerName.Name = "lblPlayerName";
            this.lblPlayerName.Size = new System.Drawing.Size(39, 13);
            this.lblPlayerName.TabIndex = 65;
            this.lblPlayerName.Tag = "Label_Player";
            this.lblPlayerName.Text = "Player:";
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
            this.cmdDeleteMugshot.Location = new System.Drawing.Point(708, 208);
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
            this.cmdAddMugshot.Location = new System.Drawing.Point(644, 208);
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
            this.lblMugshot.Location = new System.Drawing.Point(642, 184);
            this.lblMugshot.Name = "lblMugshot";
            this.lblMugshot.Size = new System.Drawing.Size(51, 13);
            this.lblMugshot.TabIndex = 21;
            this.lblMugshot.Tag = "Label_Mugshot";
            this.lblMugshot.Text = "Mugshot:";
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
            this.picMugshot.Location = new System.Drawing.Point(641, 260);
            this.picMugshot.Name = "picMugshot";
            this.picMugshot.Size = new System.Drawing.Size(210, 310);
            this.picMugshot.TabIndex = 20;
            this.picMugshot.TabStop = false;
            // 
            // tabKarma
            // 
            this.tabKarma.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabKarma.Controls.Add(this.splitKarmaNuyen);
            this.tabKarma.Location = new System.Drawing.Point(4, 22);
            this.tabKarma.Name = "tabKarma";
            this.tabKarma.Padding = new System.Windows.Forms.Padding(3);
            this.tabKarma.Size = new System.Drawing.Size(861, 586);
            this.tabKarma.TabIndex = 11;
            this.tabKarma.Tag = "Tab_Karma";
            this.tabKarma.Text = "Karma and Nuyen";
            // 
            // splitKarmaNuyen
            // 
            this.splitKarmaNuyen.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitKarmaNuyen.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.splitKarmaNuyen.Location = new System.Drawing.Point(3, 3);
            this.splitKarmaNuyen.Name = "splitKarmaNuyen";
            // 
            // splitKarmaNuyen.Panel1
            // 
            this.splitKarmaNuyen.Panel1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.splitKarmaNuyen.Panel1.Controls.Add(this.chkShowFreeKarma);
            this.splitKarmaNuyen.Panel1.Controls.Add(this.chtKarma);
            this.splitKarmaNuyen.Panel1.Controls.Add(this.cmdKarmaEdit);
            this.splitKarmaNuyen.Panel1.Controls.Add(this.cmdKarmaGained);
            this.splitKarmaNuyen.Panel1.Controls.Add(this.lstKarma);
            this.splitKarmaNuyen.Panel1.Controls.Add(this.cmdKarmaSpent);
            this.splitKarmaNuyen.Panel1.Resize += new System.EventHandler(this.splitKarmaNuyen_Panel1_Resize);
            // 
            // splitKarmaNuyen.Panel2
            // 
            this.splitKarmaNuyen.Panel2.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.splitKarmaNuyen.Panel2.Controls.Add(this.chkShowFreeNuyen);
            this.splitKarmaNuyen.Panel2.Controls.Add(this.chtNuyen);
            this.splitKarmaNuyen.Panel2.Controls.Add(this.cmdNuyenEdit);
            this.splitKarmaNuyen.Panel2.Controls.Add(this.lstNuyen);
            this.splitKarmaNuyen.Panel2.Controls.Add(this.cmdNuyenSpent);
            this.splitKarmaNuyen.Panel2.Controls.Add(this.cmdNuyenGained);
            this.splitKarmaNuyen.Panel2.Resize += new System.EventHandler(this.splitKarmaNuyen_Panel2_Resize);
            this.splitKarmaNuyen.Size = new System.Drawing.Size(856, 577);
            this.splitKarmaNuyen.SplitterDistance = 423;
            this.splitKarmaNuyen.TabIndex = 6;
            // 
            // chkShowFreeKarma
            // 
            this.chkShowFreeKarma.AutoSize = true;
            this.chkShowFreeKarma.Location = new System.Drawing.Point(291, 7);
            this.chkShowFreeKarma.Name = "chkShowFreeKarma";
            this.chkShowFreeKarma.Size = new System.Drawing.Size(112, 17);
            this.chkShowFreeKarma.TabIndex = 5;
            this.chkShowFreeKarma.Tag = "Checkbox_ShowFreeEntries";
            this.chkShowFreeKarma.Text = "Show Free Entries";
            this.chkShowFreeKarma.UseVisualStyleBackColor = true;
            this.chkShowFreeKarma.CheckedChanged += new System.EventHandler(this.chkShowFreeKarma_CheckedChanged);
            // 
            // chtKarma
            // 
            this.chtKarma.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            chartArea1.Name = "ChartArea1";
            this.chtKarma.ChartAreas.Add(chartArea1);
            this.chtKarma.Location = new System.Drawing.Point(0, 366);
            this.chtKarma.Name = "chtKarma";
            this.chtKarma.Size = new System.Drawing.Size(409, 208);
            this.chtKarma.TabIndex = 4;
            this.chtKarma.Text = "chart1";
            // 
            // cmdKarmaEdit
            // 
            this.cmdKarmaEdit.AutoSize = true;
            this.cmdKarmaEdit.Location = new System.Drawing.Point(194, 3);
            this.cmdKarmaEdit.Name = "cmdKarmaEdit";
            this.cmdKarmaEdit.Size = new System.Drawing.Size(91, 23);
            this.cmdKarmaEdit.TabIndex = 3;
            this.cmdKarmaEdit.Tag = "Button_EditExpense";
            this.cmdKarmaEdit.Text = "Edit Expense";
            this.cmdKarmaEdit.UseVisualStyleBackColor = true;
            this.cmdKarmaEdit.Click += new System.EventHandler(this.cmdKarmaEdit_Click);
            // 
            // cmdKarmaGained
            // 
            this.cmdKarmaGained.AutoSize = true;
            this.cmdKarmaGained.Location = new System.Drawing.Point(0, 3);
            this.cmdKarmaGained.Name = "cmdKarmaGained";
            this.cmdKarmaGained.Size = new System.Drawing.Size(91, 23);
            this.cmdKarmaGained.TabIndex = 1;
            this.cmdKarmaGained.Tag = "Button_KarmaGained";
            this.cmdKarmaGained.Text = "Karma Gained";
            this.cmdKarmaGained.UseVisualStyleBackColor = true;
            this.cmdKarmaGained.Click += new System.EventHandler(this.cmdKarmaGained_Click);
            // 
            // lstKarma
            // 
            this.lstKarma.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstKarma.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colKarmaDate,
            this.colKarmaAmount,
            this.colKarmaReason});
            this.lstKarma.FullRowSelect = true;
            this.lstKarma.GridLines = true;
            this.lstKarma.HideSelection = false;
            this.lstKarma.Location = new System.Drawing.Point(0, 32);
            this.lstKarma.MultiSelect = false;
            this.lstKarma.Name = "lstKarma";
            this.lstKarma.Size = new System.Drawing.Size(409, 328);
            this.lstKarma.Sorting = System.Windows.Forms.SortOrder.Descending;
            this.lstKarma.TabIndex = 0;
            this.lstKarma.UseCompatibleStateImageBehavior = false;
            this.lstKarma.View = System.Windows.Forms.View.Details;
            this.lstKarma.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lstKarma_ColumnClick);
            this.lstKarma.DoubleClick += new System.EventHandler(this.lstKarma_DoubleClick);
            // 
            // colKarmaDate
            // 
            this.colKarmaDate.Tag = "String_Date";
            this.colKarmaDate.Text = "Date";
            this.colKarmaDate.Width = 131;
            // 
            // colKarmaAmount
            // 
            this.colKarmaAmount.Tag = "String_Amount";
            this.colKarmaAmount.Text = "Amount";
            this.colKarmaAmount.Width = 65;
            // 
            // colKarmaReason
            // 
            this.colKarmaReason.Tag = "String_Reason";
            this.colKarmaReason.Text = "Reason";
            this.colKarmaReason.Width = 190;
            // 
            // cmdKarmaSpent
            // 
            this.cmdKarmaSpent.AutoSize = true;
            this.cmdKarmaSpent.Location = new System.Drawing.Point(97, 3);
            this.cmdKarmaSpent.Name = "cmdKarmaSpent";
            this.cmdKarmaSpent.Size = new System.Drawing.Size(91, 23);
            this.cmdKarmaSpent.TabIndex = 2;
            this.cmdKarmaSpent.Tag = "Button_KarmaSpent";
            this.cmdKarmaSpent.Text = "Karma Spent";
            this.cmdKarmaSpent.UseVisualStyleBackColor = true;
            this.cmdKarmaSpent.Click += new System.EventHandler(this.cmdKarmaSpent_Click);
            // 
            // chkShowFreeNuyen
            // 
            this.chkShowFreeNuyen.AutoSize = true;
            this.chkShowFreeNuyen.Location = new System.Drawing.Point(291, 7);
            this.chkShowFreeNuyen.Name = "chkShowFreeNuyen";
            this.chkShowFreeNuyen.Size = new System.Drawing.Size(112, 17);
            this.chkShowFreeNuyen.TabIndex = 6;
            this.chkShowFreeNuyen.Tag = "Checkbox_ShowFreeEntries";
            this.chkShowFreeNuyen.Text = "Show Free Entries";
            this.chkShowFreeNuyen.UseVisualStyleBackColor = true;
            this.chkShowFreeNuyen.CheckedChanged += new System.EventHandler(this.chkShowFreeNuyen_CheckedChanged);
            // 
            // chtNuyen
            // 
            this.chtNuyen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            chartArea2.Name = "ChartArea1";
            this.chtNuyen.ChartAreas.Add(chartArea2);
            this.chtNuyen.Location = new System.Drawing.Point(0, 366);
            this.chtNuyen.Name = "chtNuyen";
            this.chtNuyen.Size = new System.Drawing.Size(410, 208);
            this.chtNuyen.TabIndex = 7;
            this.chtNuyen.Text = "chart1";
            // 
            // cmdNuyenEdit
            // 
            this.cmdNuyenEdit.AutoSize = true;
            this.cmdNuyenEdit.Location = new System.Drawing.Point(194, 3);
            this.cmdNuyenEdit.Name = "cmdNuyenEdit";
            this.cmdNuyenEdit.Size = new System.Drawing.Size(91, 23);
            this.cmdNuyenEdit.TabIndex = 6;
            this.cmdNuyenEdit.Tag = "Button_EditExpense";
            this.cmdNuyenEdit.Text = "Edit Expense";
            this.cmdNuyenEdit.UseVisualStyleBackColor = true;
            this.cmdNuyenEdit.Click += new System.EventHandler(this.cmdNuyenEdit_Click);
            // 
            // lstNuyen
            // 
            this.lstNuyen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstNuyen.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colNuyenDate,
            this.colNuyenAmount,
            this.colNuyenReason});
            this.lstNuyen.FullRowSelect = true;
            this.lstNuyen.GridLines = true;
            this.lstNuyen.HideSelection = false;
            this.lstNuyen.Location = new System.Drawing.Point(0, 32);
            this.lstNuyen.MultiSelect = false;
            this.lstNuyen.Name = "lstNuyen";
            this.lstNuyen.Size = new System.Drawing.Size(410, 328);
            this.lstNuyen.Sorting = System.Windows.Forms.SortOrder.Descending;
            this.lstNuyen.TabIndex = 3;
            this.lstNuyen.UseCompatibleStateImageBehavior = false;
            this.lstNuyen.View = System.Windows.Forms.View.Details;
            this.lstNuyen.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lstNuyen_ColumnClick);
            this.lstNuyen.DoubleClick += new System.EventHandler(this.lstNuyen_DoubleClick);
            // 
            // colNuyenDate
            // 
            this.colNuyenDate.Tag = "String_Date";
            this.colNuyenDate.Text = "Date";
            this.colNuyenDate.Width = 131;
            // 
            // colNuyenAmount
            // 
            this.colNuyenAmount.Tag = "String_Amount";
            this.colNuyenAmount.Text = "Amount";
            this.colNuyenAmount.Width = 65;
            // 
            // colNuyenReason
            // 
            this.colNuyenReason.Tag = "String_Reason";
            this.colNuyenReason.Text = "Reason";
            this.colNuyenReason.Width = 190;
            // 
            // cmdNuyenSpent
            // 
            this.cmdNuyenSpent.AutoSize = true;
            this.cmdNuyenSpent.Location = new System.Drawing.Point(97, 3);
            this.cmdNuyenSpent.Name = "cmdNuyenSpent";
            this.cmdNuyenSpent.Size = new System.Drawing.Size(91, 23);
            this.cmdNuyenSpent.TabIndex = 5;
            this.cmdNuyenSpent.Tag = "Button_NuyenSpent";
            this.cmdNuyenSpent.Text = "Nuyen Spent";
            this.cmdNuyenSpent.UseVisualStyleBackColor = true;
            this.cmdNuyenSpent.Click += new System.EventHandler(this.cmdNuyenSpent_Click);
            // 
            // cmdNuyenGained
            // 
            this.cmdNuyenGained.AutoSize = true;
            this.cmdNuyenGained.Location = new System.Drawing.Point(0, 3);
            this.cmdNuyenGained.Name = "cmdNuyenGained";
            this.cmdNuyenGained.Size = new System.Drawing.Size(91, 23);
            this.cmdNuyenGained.TabIndex = 4;
            this.cmdNuyenGained.Tag = "Button_NuyenGained";
            this.cmdNuyenGained.Text = "Nuyen Gained";
            this.cmdNuyenGained.UseVisualStyleBackColor = true;
            this.cmdNuyenGained.Click += new System.EventHandler(this.cmdNuyenGained_Click);
            // 
            // tabCalendar
            // 
            this.tabCalendar.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabCalendar.Controls.Add(this.cmdDeleteWeek);
            this.tabCalendar.Controls.Add(this.cmdChangeStartWeek);
            this.tabCalendar.Controls.Add(this.cmdEditWeek);
            this.tabCalendar.Controls.Add(this.cmdAddWeek);
            this.tabCalendar.Controls.Add(this.lstCalendar);
            this.tabCalendar.Location = new System.Drawing.Point(4, 22);
            this.tabCalendar.Name = "tabCalendar";
            this.tabCalendar.Padding = new System.Windows.Forms.Padding(3);
            this.tabCalendar.Size = new System.Drawing.Size(861, 586);
            this.tabCalendar.TabIndex = 15;
            this.tabCalendar.Tag = "Tab_Calendar";
            this.tabCalendar.Text = "Calendar";
            // 
            // cmdDeleteWeek
            // 
            this.cmdDeleteWeek.AutoSize = true;
            this.cmdDeleteWeek.Location = new System.Drawing.Point(186, 6);
            this.cmdDeleteWeek.Name = "cmdDeleteWeek";
            this.cmdDeleteWeek.Size = new System.Drawing.Size(83, 23);
            this.cmdDeleteWeek.TabIndex = 5;
            this.cmdDeleteWeek.Tag = "Button_DeleteWeek";
            this.cmdDeleteWeek.Text = "Delete Week";
            this.cmdDeleteWeek.UseVisualStyleBackColor = true;
            this.cmdDeleteWeek.Click += new System.EventHandler(this.cmdDeleteWeek_Click);
            // 
            // cmdChangeStartWeek
            // 
            this.cmdChangeStartWeek.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdChangeStartWeek.AutoSize = true;
            this.cmdChangeStartWeek.Location = new System.Drawing.Point(737, 6);
            this.cmdChangeStartWeek.Name = "cmdChangeStartWeek";
            this.cmdChangeStartWeek.Size = new System.Drawing.Size(119, 23);
            this.cmdChangeStartWeek.TabIndex = 4;
            this.cmdChangeStartWeek.Tag = "Button_ChangeStartWeek";
            this.cmdChangeStartWeek.Text = "Change Starting Date";
            this.cmdChangeStartWeek.UseVisualStyleBackColor = true;
            this.cmdChangeStartWeek.Click += new System.EventHandler(this.cmdChangeStartWeek_Click);
            // 
            // cmdEditWeek
            // 
            this.cmdEditWeek.AutoSize = true;
            this.cmdEditWeek.Location = new System.Drawing.Point(97, 6);
            this.cmdEditWeek.Name = "cmdEditWeek";
            this.cmdEditWeek.Size = new System.Drawing.Size(83, 23);
            this.cmdEditWeek.TabIndex = 3;
            this.cmdEditWeek.Tag = "Button_EditWeek";
            this.cmdEditWeek.Text = "Edit Week";
            this.cmdEditWeek.UseVisualStyleBackColor = true;
            this.cmdEditWeek.Click += new System.EventHandler(this.cmdEditWeek_Click);
            // 
            // cmdAddWeek
            // 
            this.cmdAddWeek.AutoSize = true;
            this.cmdAddWeek.Location = new System.Drawing.Point(8, 6);
            this.cmdAddWeek.Name = "cmdAddWeek";
            this.cmdAddWeek.Size = new System.Drawing.Size(83, 23);
            this.cmdAddWeek.TabIndex = 2;
            this.cmdAddWeek.Tag = "Button_AddWeek";
            this.cmdAddWeek.Text = "Add Week";
            this.cmdAddWeek.UseVisualStyleBackColor = true;
            this.cmdAddWeek.Click += new System.EventHandler(this.cmdAddWeek_Click);
            // 
            // lstCalendar
            // 
            this.lstCalendar.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstCalendar.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colCalendarDate,
            this.colCalendarNotes});
            this.lstCalendar.FullRowSelect = true;
            this.lstCalendar.GridLines = true;
            this.lstCalendar.HideSelection = false;
            this.lstCalendar.Location = new System.Drawing.Point(8, 35);
            this.lstCalendar.MultiSelect = false;
            this.lstCalendar.Name = "lstCalendar";
            this.lstCalendar.Size = new System.Drawing.Size(848, 545);
            this.lstCalendar.TabIndex = 1;
            this.lstCalendar.UseCompatibleStateImageBehavior = false;
            this.lstCalendar.View = System.Windows.Forms.View.Details;
            this.lstCalendar.DoubleClick += new System.EventHandler(this.lstCalendar_DoubleClick);
            // 
            // colCalendarDate
            // 
            this.colCalendarDate.Tag = "String_Date";
            this.colCalendarDate.Text = "Date";
            this.colCalendarDate.Width = 131;
            // 
            // colCalendarNotes
            // 
            this.colCalendarNotes.Tag = "Title_Notes";
            this.colCalendarNotes.Text = "Notes";
            this.colCalendarNotes.Width = 650;
            // 
            // tabNotes
            // 
            this.tabNotes.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabNotes.Controls.Add(this.txtGameNotes);
            this.tabNotes.Location = new System.Drawing.Point(4, 22);
            this.tabNotes.Name = "tabNotes";
            this.tabNotes.Padding = new System.Windows.Forms.Padding(3);
            this.tabNotes.Size = new System.Drawing.Size(861, 586);
            this.tabNotes.TabIndex = 13;
            this.tabNotes.Tag = "Tab_Notes";
            this.tabNotes.Text = "Notes";
            // 
            // txtGameNotes
            // 
            this.txtGameNotes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGameNotes.Location = new System.Drawing.Point(8, 9);
            this.txtGameNotes.Multiline = true;
            this.txtGameNotes.Name = "txtGameNotes";
            this.txtGameNotes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtGameNotes.Size = new System.Drawing.Size(848, 571);
            this.txtGameNotes.TabIndex = 0;
            this.txtGameNotes.TextChanged += new System.EventHandler(this.txtGameNotes_TextChanged);
            this.txtGameNotes.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtNotes_KeyDown);
            // 
            // tabImprovements
            // 
            this.tabImprovements.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabImprovements.Controls.Add(this.cmdImprovementsDisableAll);
            this.tabImprovements.Controls.Add(this.cmdImprovementsEnableAll);
            this.tabImprovements.Controls.Add(this.cmdAddImprovementGroup);
            this.tabImprovements.Controls.Add(this.cmdDeleteImprovement);
            this.tabImprovements.Controls.Add(this.cmdEditImprovement);
            this.tabImprovements.Controls.Add(this.chkImprovementActive);
            this.tabImprovements.Controls.Add(this.lblImprovementValue);
            this.tabImprovements.Controls.Add(this.lblImprovementType);
            this.tabImprovements.Controls.Add(this.lblImprovementTypeLabel);
            this.tabImprovements.Controls.Add(this.treImprovements);
            this.tabImprovements.Controls.Add(this.cmdAddImprovement);
            this.tabImprovements.Location = new System.Drawing.Point(4, 22);
            this.tabImprovements.Name = "tabImprovements";
            this.tabImprovements.Size = new System.Drawing.Size(861, 586);
            this.tabImprovements.TabIndex = 14;
            this.tabImprovements.Tag = "Tab_Improvements";
            this.tabImprovements.Text = "Improvements";
            // 
            // cmdImprovementsDisableAll
            // 
            this.cmdImprovementsDisableAll.AutoSize = true;
            this.cmdImprovementsDisableAll.Location = new System.Drawing.Point(462, 79);
            this.cmdImprovementsDisableAll.Name = "cmdImprovementsDisableAll";
            this.cmdImprovementsDisableAll.Size = new System.Drawing.Size(82, 23);
            this.cmdImprovementsDisableAll.TabIndex = 109;
            this.cmdImprovementsDisableAll.Tag = "Button_DisableAll";
            this.cmdImprovementsDisableAll.Text = "Disable All";
            this.cmdImprovementsDisableAll.UseVisualStyleBackColor = true;
            this.cmdImprovementsDisableAll.Visible = false;
            this.cmdImprovementsDisableAll.Click += new System.EventHandler(this.cmdImprovementsDisableAll_Click);
            // 
            // cmdImprovementsEnableAll
            // 
            this.cmdImprovementsEnableAll.AutoSize = true;
            this.cmdImprovementsEnableAll.Location = new System.Drawing.Point(374, 79);
            this.cmdImprovementsEnableAll.Name = "cmdImprovementsEnableAll";
            this.cmdImprovementsEnableAll.Size = new System.Drawing.Size(82, 23);
            this.cmdImprovementsEnableAll.TabIndex = 108;
            this.cmdImprovementsEnableAll.Tag = "Button_EnableAll";
            this.cmdImprovementsEnableAll.Text = "Enable All";
            this.cmdImprovementsEnableAll.UseVisualStyleBackColor = true;
            this.cmdImprovementsEnableAll.Visible = false;
            this.cmdImprovementsEnableAll.Click += new System.EventHandler(this.cmdImprovementsEnableAll_Click);
            // 
            // cmdAddImprovementGroup
            // 
            this.cmdAddImprovementGroup.AutoSize = true;
            this.cmdAddImprovementGroup.Location = new System.Drawing.Point(338, 6);
            this.cmdAddImprovementGroup.Name = "cmdAddImprovementGroup";
            this.cmdAddImprovementGroup.Size = new System.Drawing.Size(80, 23);
            this.cmdAddImprovementGroup.TabIndex = 107;
            this.cmdAddImprovementGroup.Tag = "Button_AddGroup";
            this.cmdAddImprovementGroup.Text = "Add Group";
            this.cmdAddImprovementGroup.UseVisualStyleBackColor = true;
            this.cmdAddImprovementGroup.Click += new System.EventHandler(this.cmdAddImprovementGroup_Click);
            // 
            // cmdDeleteImprovement
            // 
            this.cmdDeleteImprovement.AutoSize = true;
            this.cmdDeleteImprovement.Location = new System.Drawing.Point(220, 6);
            this.cmdDeleteImprovement.Name = "cmdDeleteImprovement";
            this.cmdDeleteImprovement.Size = new System.Drawing.Size(80, 23);
            this.cmdDeleteImprovement.TabIndex = 100;
            this.cmdDeleteImprovement.Tag = "String_Delete";
            this.cmdDeleteImprovement.Text = "Delete";
            this.cmdDeleteImprovement.UseVisualStyleBackColor = true;
            this.cmdDeleteImprovement.Click += new System.EventHandler(this.cmdDeleteImprovement_Click);
            // 
            // cmdEditImprovement
            // 
            this.cmdEditImprovement.AutoSize = true;
            this.cmdEditImprovement.Location = new System.Drawing.Point(114, 6);
            this.cmdEditImprovement.Name = "cmdEditImprovement";
            this.cmdEditImprovement.Size = new System.Drawing.Size(100, 23);
            this.cmdEditImprovement.TabIndex = 99;
            this.cmdEditImprovement.Tag = "Button_EditImprovement";
            this.cmdEditImprovement.Text = "Edit Improvement";
            this.cmdEditImprovement.UseVisualStyleBackColor = true;
            this.cmdEditImprovement.Click += new System.EventHandler(this.cmdEditImprovement_Click);
            // 
            // chkImprovementActive
            // 
            this.chkImprovementActive.AutoSize = true;
            this.chkImprovementActive.Location = new System.Drawing.Point(312, 83);
            this.chkImprovementActive.Name = "chkImprovementActive";
            this.chkImprovementActive.Size = new System.Drawing.Size(56, 17);
            this.chkImprovementActive.TabIndex = 98;
            this.chkImprovementActive.Tag = "Checkbox_Active";
            this.chkImprovementActive.Text = "Active";
            this.chkImprovementActive.UseVisualStyleBackColor = true;
            this.chkImprovementActive.CheckedChanged += new System.EventHandler(this.chkImprovementActive_CheckedChanged);
            // 
            // lblImprovementValue
            // 
            this.lblImprovementValue.AutoSize = true;
            this.lblImprovementValue.Location = new System.Drawing.Point(309, 58);
            this.lblImprovementValue.Name = "lblImprovementValue";
            this.lblImprovementValue.Size = new System.Drawing.Size(19, 13);
            this.lblImprovementValue.TabIndex = 84;
            this.lblImprovementValue.Text = "[0]";
            // 
            // lblImprovementType
            // 
            this.lblImprovementType.AutoSize = true;
            this.lblImprovementType.Location = new System.Drawing.Point(349, 35);
            this.lblImprovementType.Name = "lblImprovementType";
            this.lblImprovementType.Size = new System.Drawing.Size(19, 13);
            this.lblImprovementType.TabIndex = 83;
            this.lblImprovementType.Text = "[0]";
            // 
            // lblImprovementTypeLabel
            // 
            this.lblImprovementTypeLabel.AutoSize = true;
            this.lblImprovementTypeLabel.Location = new System.Drawing.Point(309, 35);
            this.lblImprovementTypeLabel.Name = "lblImprovementTypeLabel";
            this.lblImprovementTypeLabel.Size = new System.Drawing.Size(34, 13);
            this.lblImprovementTypeLabel.TabIndex = 82;
            this.lblImprovementTypeLabel.Tag = "Label_Type";
            this.lblImprovementTypeLabel.Text = "Type:";
            // 
            // treImprovements
            // 
            this.treImprovements.AllowDrop = true;
            this.treImprovements.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treImprovements.HideSelection = false;
            this.treImprovements.Location = new System.Drawing.Point(8, 35);
            this.treImprovements.Name = "treImprovements";
            treeNode26.Name = "nodImprovementsRoot";
            treeNode26.Tag = "Node_SelectedImprovements";
            treeNode26.Text = "Selected Improvements";
            this.treImprovements.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode26});
            this.treImprovements.ShowNodeToolTips = true;
            this.treImprovements.Size = new System.Drawing.Size(295, 548);
            this.treImprovements.TabIndex = 81;
            this.treImprovements.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treImprovements_AfterSelect);
            this.treImprovements.DragOver += new System.Windows.Forms.DragEventHandler(this.treImprovements_DragOver);
            this.treImprovements.DoubleClick += new System.EventHandler(this.treImprovements_DoubleClick);
            this.treImprovements.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treImprovements_KeyDown);
            this.treImprovements.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            // 
            // cmdAddImprovement
            // 
            this.cmdAddImprovement.AutoSize = true;
            this.cmdAddImprovement.Location = new System.Drawing.Point(8, 6);
            this.cmdAddImprovement.Name = "cmdAddImprovement";
            this.cmdAddImprovement.Size = new System.Drawing.Size(100, 23);
            this.cmdAddImprovement.TabIndex = 4;
            this.cmdAddImprovement.Tag = "Button_AddImproevment";
            this.cmdAddImprovement.Text = "&Add Improvement";
            this.cmdAddImprovement.UseVisualStyleBackColor = true;
            this.cmdAddImprovement.Click += new System.EventHandler(this.cmdAddImprovement_Click);
            // 
            // panAttributes
            // 
            this.panAttributes.Location = new System.Drawing.Point(0, 0);
            this.panAttributes.Name = "panAttributes";
            this.panAttributes.Size = new System.Drawing.Size(200, 100);
            this.panAttributes.TabIndex = 0;
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
            // cmsImprovement
            // 
            this.cmsImprovement.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsImprovementNotes});
            this.cmsImprovement.Name = "cmsImprovement";
            this.cmsImprovement.Size = new System.Drawing.Size(106, 26);
            // 
            // tsImprovementNotes
            // 
            this.tsImprovementNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsImprovementNotes.Name = "tsImprovementNotes";
            this.tsImprovementNotes.Size = new System.Drawing.Size(105, 22);
            this.tsImprovementNotes.Tag = "Menu_Notes";
            this.tsImprovementNotes.Text = "&Notes";
            this.tsImprovementNotes.Click += new System.EventHandler(this.tsImprovementNotes_Click);
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
            // cmsImprovementLocation
            // 
            this.cmsImprovementLocation.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsImprovementRenameLocation});
            this.cmsImprovementLocation.Name = "cmsImprovementLocation";
            this.cmsImprovementLocation.Size = new System.Drawing.Size(167, 26);
            this.cmsImprovementLocation.Tag = "";
            this.cmsImprovementLocation.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
            // 
            // tsImprovementRenameLocation
            // 
            this.tsImprovementRenameLocation.Image = global::Chummer.Properties.Resources.building_edit;
            this.tsImprovementRenameLocation.Name = "tsImprovementRenameLocation";
            this.tsImprovementRenameLocation.Size = new System.Drawing.Size(166, 22);
            this.tsImprovementRenameLocation.Tag = "Menu_RenameLocation";
            this.tsImprovementRenameLocation.Text = "&Rename Location";
            this.tsImprovementRenameLocation.Click += new System.EventHandler(this.tsImprovementRenameLocation_Click);
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
            // cmsVehicleWeaponMod
            // 
            this.cmsVehicleWeaponMod.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsVehicleWeaponModNotes});
            this.cmsVehicleWeaponMod.Name = "cmsWeaponMod";
            this.cmsVehicleWeaponMod.Size = new System.Drawing.Size(68, 26);
            // 
            // tsVehicleWeaponModNotes
            // 
            this.tsVehicleWeaponModNotes.Name = "tsVehicleWeaponModNotes";
            this.tsVehicleWeaponModNotes.Size = new System.Drawing.Size(67, 22);
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
            // cmsLimitModifier
            // 
            this.cmsLimitModifier.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tssLimitModifierEdit,
            this.tssLimitModifierNotes});
            this.cmsLimitModifier.Name = "cmsLimitModifier";
            this.cmsLimitModifier.Size = new System.Drawing.Size(106, 48);
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
            // tssLimitModifierNotes
            // 
            this.tssLimitModifierNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tssLimitModifierNotes.Name = "tssLimitModifierNotes";
            this.tssLimitModifierNotes.Size = new System.Drawing.Size(105, 22);
            this.tssLimitModifierNotes.Tag = "Menu_Notes";
            this.tssLimitModifierNotes.Text = "&Notes";
            this.tssLimitModifierNotes.Click += new System.EventHandler(this.tssLimitModifierNotes_Click);
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
            // cmdIncreasePowerPoints
            // 
            this.cmdIncreasePowerPoints.Enabled = false;
            this.cmdIncreasePowerPoints.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdIncreasePowerPoints.Image = global::Chummer.Properties.Resources.add;
            this.cmdIncreasePowerPoints.Location = new System.Drawing.Point(805, 283);
            this.cmdIncreasePowerPoints.Name = "cmdIncreasePowerPoints";
            this.cmdIncreasePowerPoints.Size = new System.Drawing.Size(24, 24);
            this.cmdIncreasePowerPoints.TabIndex = 106;
            this.cmdIncreasePowerPoints.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.cmdIncreasePowerPoints.UseVisualStyleBackColor = true;
            this.cmdIncreasePowerPoints.Click += new System.EventHandler(this.cmdIncreasePowerPoints_Click);
            // 
            // frmCareer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1066, 639);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.StatusStrip);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.mnuCreateMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mnuCreateMenu;
            this.Name = "frmCareer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Chummer - Career Mode";
            this.Activated += new System.EventHandler(this.frmCareer_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmCareer_FormClosing);
            this.Load += new System.EventHandler(this.frmCareer_Load);
            this.Shown += new System.EventHandler(this.frmCareer_Shown);
            this.Resize += new System.EventHandler(this.frmCareer_Resize);
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.cmsMartialArts.ResumeLayout(false);
            this.cmsSpellButton.ResumeLayout(false);
            this.cmsComplexForm.ResumeLayout(false);
            this.cmsCyberware.ResumeLayout(false);
            this.cmsDeleteCyberware.ResumeLayout(false);
            this.cmsLifestyle.ResumeLayout(false);
            this.cmsArmor.ResumeLayout(false);
            this.cmsDeleteArmor.ResumeLayout(false);
            this.cmsWeapon.ResumeLayout(false);
            this.cmsDeleteWeapon.ResumeLayout(false);
            this.cmsAmmoExpense.ResumeLayout(false);
            this.cmsGearButton.ResumeLayout(false);
            this.cmsDeleteGear.ResumeLayout(false);
            this.cmsVehicle.ResumeLayout(false);
            this.cmdVehicleAmmoExpense.ResumeLayout(false);
            this.cmsDeleteVehicle.ResumeLayout(false);
            this.panStunCM.ResumeLayout(false);
            this.panStunCM.PerformLayout();
            this.panPhysicalCM.ResumeLayout(false);
            this.panPhysicalCM.PerformLayout();
            this.tabInfo.ResumeLayout(false);
            this.tabOtherInfo.ResumeLayout(false);
            this.tabOtherInfo.PerformLayout();
            this.tabConditionMonitor.ResumeLayout(false);
            this.tabConditionMonitor.PerformLayout();
            this.tabDefences.ResumeLayout(false);
            this.tabDefences.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCounterspellingDice)).EndInit();
            this.mnuCreateMenu.ResumeLayout(false);
            this.mnuCreateMenu.PerformLayout();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.cmsGear.ResumeLayout(false);
            this.cmsVehicleWeapon.ResumeLayout(false);
            this.cmsVehicleGear.ResumeLayout(false);
            this.cmsUndoKarmaExpense.ResumeLayout(false);
            this.cmsUndoNuyenExpense.ResumeLayout(false);
            this.cmsArmorGear.ResumeLayout(false);
            this.cmsArmorMod.ResumeLayout(false);
            this.cmsQuality.ResumeLayout(false);
            this.cmsMartialArtManeuver.ResumeLayout(false);
            this.cmsSpell.ResumeLayout(false);
            this.cmsCritterPowers.ResumeLayout(false);
            this.cmsMetamagic.ResumeLayout(false);
            this.cmsLifestyleNotes.ResumeLayout(false);
            this.cmsWeaponAccessory.ResumeLayout(false);
            this.cmsGearPlugin.ResumeLayout(false);
            this.cmsComplexFormPlugin.ResumeLayout(false);
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
            this.tabCritter.ResumeLayout(false);
            this.tabCritter.PerformLayout();
            this.tabAdvancedPrograms.ResumeLayout(false);
            this.tabAdvancedPrograms.PerformLayout();
            this.tabInitiation.ResumeLayout(false);
            this.tabInitiation.PerformLayout();
            this.tabCyberware.ResumeLayout(false);
            this.tabCyberware.PerformLayout();
            this.tabCyberwareCM.ResumeLayout(false);
            this.tabCyberwareMatrixCM.ResumeLayout(false);
            this.tabStreetGear.ResumeLayout(false);
            this.tabStreetGearTabs.ResumeLayout(false);
            this.tabLifestyle.ResumeLayout(false);
            this.tabLifestyle.PerformLayout();
            this.tabArmor.ResumeLayout(false);
            this.tabArmor.PerformLayout();
            this.tabWeapons.ResumeLayout(false);
            this.tabWeapons.PerformLayout();
            this.tabGear.ResumeLayout(false);
            this.tabGear.PerformLayout();
            this.tabGearMatrixCM.ResumeLayout(false);
            this.tabMatrixCM.ResumeLayout(false);
            this.tabPets.ResumeLayout(false);
            this.tabPets.PerformLayout();
            this.tabVehicles.ResumeLayout(false);
            this.tabVehicles.PerformLayout();
            this.panVehicleCM.ResumeLayout(false);
            this.tabVehiclePhysicalCM.ResumeLayout(false);
            this.tabVehicleMatrixCM.ResumeLayout(false);
            this.tabCharacterInfo.ResumeLayout(false);
            this.tabCharacterInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMugshotIndex)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPublicAware)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNotoriety)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudStreetCred)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMugshot)).EndInit();
            this.tabKarma.ResumeLayout(false);
            this.splitKarmaNuyen.Panel1.ResumeLayout(false);
            this.splitKarmaNuyen.Panel1.PerformLayout();
            this.splitKarmaNuyen.Panel2.ResumeLayout(false);
            this.splitKarmaNuyen.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitKarmaNuyen)).EndInit();
            this.splitKarmaNuyen.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chtKarma)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chtNuyen)).EndInit();
            this.tabCalendar.ResumeLayout(false);
            this.tabCalendar.PerformLayout();
            this.tabNotes.ResumeLayout(false);
            this.tabNotes.PerformLayout();
            this.tabImprovements.ResumeLayout(false);
            this.tabImprovements.PerformLayout();
            this.cmsBioware.ResumeLayout(false);
            this.cmsAdvancedLifestyle.ResumeLayout(false);
            this.cmsGearLocation.ResumeLayout(false);
            this.cmsImprovement.ResumeLayout(false);
            this.cmsArmorLocation.ResumeLayout(false);
            this.cmsImprovementLocation.ResumeLayout(false);
            this.cmsCyberwareGear.ResumeLayout(false);
            this.cmsWeaponAccessoryGear.ResumeLayout(false);
            this.cmsVehicleLocation.ResumeLayout(false);
            this.cmsVehicleWeaponAccessory.ResumeLayout(false);
            this.cmsVehicleWeaponAccessoryGear.ResumeLayout(false);
            this.cmsVehicleWeaponMod.ResumeLayout(false);
            this.cmsWeaponLocation.ResumeLayout(false);
            this.cmsLimitModifier.ResumeLayout(false);
            this.cmsInitiationNotes.ResumeLayout(false);
            this.cmsTechnique.ResumeLayout(false);
            this.cmsAdvancedProgram.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.StatusStrip StatusStrip;
        private System.Windows.Forms.SaveFileDialog dlgSaveFile;
        private TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip tipTooltip;
        private System.Windows.Forms.TabControl tabInfo;
        private System.Windows.Forms.TabPage tabOtherInfo;
        private System.Windows.Forms.Label lblESSMax;
        private System.Windows.Forms.Label lblESS;
        private System.Windows.Forms.Label lblArmor;
        private System.Windows.Forms.Label lblCMStun;
        private System.Windows.Forms.Label lblCMPhysical;
        private System.Windows.Forms.Label lblCMStunLabel;
        private System.Windows.Forms.Label lblCMPhysicalLabel;
        private System.Windows.Forms.Label lblRemainingNuyen;
        private System.Windows.Forms.Label lblRemainingNuyenLabel;
        private System.Windows.Forms.MenuStrip mnuCreateMenu;
        private System.Windows.Forms.ToolStripMenuItem mnuCreateFile;
        private System.Windows.Forms.ToolStripMenuItem mnuFileSaveAs;
        private System.Windows.Forms.ToolStripMenuItem mnuFileSave;
        private System.Windows.Forms.ContextMenuStrip cmsCyberware;
        private System.Windows.Forms.ToolStripMenuItem tsCyberwareAddAsPlugin;
        private System.Windows.Forms.ContextMenuStrip cmsWeapon;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponAddAccessory;
        private System.Windows.Forms.ContextMenuStrip cmsArmor;
        private System.Windows.Forms.ToolStripMenuItem tsAddArmorMod;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton tsbSave;
        private System.Windows.Forms.ContextMenuStrip cmsGear;
        private System.Windows.Forms.ToolStripMenuItem tsGearAddAsPlugin;
        private System.Windows.Forms.ToolStripButton tsbPrint;
        private System.Windows.Forms.ContextMenuStrip cmsVehicle;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddMod;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddWeapon;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddWeaponAccessory;
        private System.Windows.Forms.ContextMenuStrip cmsVehicleWeapon;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddWeaponAccessoryAlt;
        private System.Windows.Forms.ContextMenuStrip cmsMartialArts;
        private System.Windows.Forms.ToolStripMenuItem tsMartialArtsAddAdvantage;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.ToolStripStatusLabel tssEssence;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel4;
        private System.Windows.Forms.ToolStripStatusLabel tssNuyen;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponAddModification;
        private System.Windows.Forms.ToolStripMenuItem mnuFilePrint;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddSensor;
        private System.Windows.Forms.ContextMenuStrip cmsVehicleGear;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleGearAddAsPlugin;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleSensorAddAsPlugin;
        private System.Windows.Forms.ToolStripProgressBar pgbProgress;
        private System.Windows.Forms.ContextMenuStrip cmsAmmoExpense;
        private System.Windows.Forms.ToolStripMenuItem cmsAmmoSingleShot;
        private System.Windows.Forms.ToolStripMenuItem cmsAmmoShortBurst;
        private System.Windows.Forms.ToolStripMenuItem cmsAmmoLongBurst;
        private System.Windows.Forms.ToolStripMenuItem cmsAmmoFullBurst;
        private System.Windows.Forms.ToolStripMenuItem cmsAmmoSuppressiveFire;
        private System.Windows.Forms.ToolStripStatusLabel tssKarmaLabel;
        private System.Windows.Forms.ToolStripStatusLabel tssKarma;
        private System.Windows.Forms.ContextMenuStrip cmsDeleteCyberware;
        private System.Windows.Forms.ToolStripMenuItem tsCyberwareSell;
        private System.Windows.Forms.ContextMenuStrip cmsDeleteArmor;
        private System.Windows.Forms.ToolStripMenuItem tsArmorSell;
        private System.Windows.Forms.ContextMenuStrip cmsDeleteWeapon;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponSell;
        private System.Windows.Forms.ContextMenuStrip cmsDeleteGear;
        private System.Windows.Forms.ToolStripMenuItem sellItemToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip cmsDeleteVehicle;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleSell;
        private System.Windows.Forms.ContextMenuStrip cmdVehicleAmmoExpense;
        private System.Windows.Forms.ToolStripMenuItem cmsVehicleAmmoSingleShot;
        private System.Windows.Forms.ToolStripMenuItem cmsVehicleAmmoShortBurst;
        private System.Windows.Forms.ToolStripMenuItem cmsVehicleAmmoLongBurst;
        private System.Windows.Forms.ToolStripMenuItem cmsVehicleAmmoFullBurst;
        private System.Windows.Forms.ToolStripMenuItem cmsVehicleAmmoSuppressiveFire;
        private System.Windows.Forms.ContextMenuStrip cmsLifestyle;
        private System.Windows.Forms.ToolStripMenuItem tsAdvancedLifestyle;
        private System.Windows.Forms.Panel panStunCM;
        private System.Windows.Forms.CheckBox chkStunCM18;
        private System.Windows.Forms.CheckBox chkStunCM17;
        private System.Windows.Forms.CheckBox chkStunCM16;
        private System.Windows.Forms.CheckBox chkStunCM15;
        private System.Windows.Forms.CheckBox chkStunCM14;
        private System.Windows.Forms.CheckBox chkStunCM13;
        private System.Windows.Forms.CheckBox chkStunCM12;
        private System.Windows.Forms.CheckBox chkStunCM11;
        private System.Windows.Forms.CheckBox chkStunCM10;
        private System.Windows.Forms.CheckBox chkStunCM9;
        private System.Windows.Forms.CheckBox chkStunCM8;
        private System.Windows.Forms.CheckBox chkStunCM7;
        private System.Windows.Forms.CheckBox chkStunCM6;
        private System.Windows.Forms.CheckBox chkStunCM5;
        private System.Windows.Forms.CheckBox chkStunCM4;
        private System.Windows.Forms.CheckBox chkStunCM3;
        private System.Windows.Forms.CheckBox chkStunCM2;
        private System.Windows.Forms.CheckBox chkStunCM1;
        private System.Windows.Forms.Panel panPhysicalCM;
        private System.Windows.Forms.CheckBox chkPhysicalCM18;
        private System.Windows.Forms.CheckBox chkPhysicalCM17;
        private System.Windows.Forms.CheckBox chkPhysicalCM16;
        private System.Windows.Forms.CheckBox chkPhysicalCM15;
        private System.Windows.Forms.CheckBox chkPhysicalCM14;
        private System.Windows.Forms.CheckBox chkPhysicalCM13;
        private System.Windows.Forms.CheckBox chkPhysicalCM12;
        private System.Windows.Forms.CheckBox chkPhysicalCM11;
        private System.Windows.Forms.CheckBox chkPhysicalCM10;
        private System.Windows.Forms.CheckBox chkPhysicalCM9;
        private System.Windows.Forms.CheckBox chkPhysicalCM8;
        private System.Windows.Forms.CheckBox chkPhysicalCM7;
        private System.Windows.Forms.CheckBox chkPhysicalCM6;
        private System.Windows.Forms.CheckBox chkPhysicalCM5;
        private System.Windows.Forms.CheckBox chkPhysicalCM4;
        private System.Windows.Forms.CheckBox chkPhysicalCM3;
        private System.Windows.Forms.CheckBox chkPhysicalCM2;
        private System.Windows.Forms.CheckBox chkPhysicalCM1;
        private System.Windows.Forms.Label lblCMPenalty;
        private System.Windows.Forms.Label lblCMPenaltyLabel;
        private System.Windows.Forms.Label lblStunCMLabel;
        private System.Windows.Forms.Label lblPhysicalCMLabel;
        private System.Windows.Forms.TabPage tabConditionMonitor;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponName;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponAddUnderbarrel;
        private System.Windows.Forms.ContextMenuStrip cmsComplexForm;
        private System.Windows.Forms.ToolStripMenuItem tsAddComplexFormOption;
        private System.Windows.Forms.Button cmdEdgeGained;
        private System.Windows.Forms.Button cmdEdgeSpent;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ContextMenuStrip cmsGearButton;
        private System.Windows.Forms.ToolStripMenuItem tsGearButtonAddAccessory;
        private System.Windows.Forms.ToolStripMenuItem tsGearAddNexus;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddNexus;
        private System.Windows.Forms.Label lblCareerKarma;
        private System.Windows.Forms.Label lblCareerKarmaLabel;
        private System.Windows.Forms.Label lblMovement;
        private System.Windows.Forms.Label lblMovementLabel;
        private System.Windows.Forms.CheckBox chkPhysicalCM24;
        private System.Windows.Forms.CheckBox chkPhysicalCM23;
        private System.Windows.Forms.CheckBox chkPhysicalCM22;
        private System.Windows.Forms.CheckBox chkPhysicalCM21;
        private System.Windows.Forms.CheckBox chkPhysicalCM20;
        private System.Windows.Forms.CheckBox chkPhysicalCM19;
        private System.Windows.Forms.ContextMenuStrip cmsUndoKarmaExpense;
        private System.Windows.Forms.ToolStripMenuItem tsUndoKarmaExpense;
        private System.Windows.Forms.ContextMenuStrip cmsUndoNuyenExpense;
        private System.Windows.Forms.ToolStripMenuItem tsUndoNuyenExpense;
        private System.Windows.Forms.Label lblMemory;
        private System.Windows.Forms.Label lblMemoryLabel;
        private System.Windows.Forms.Label lblLiftCarry;
        private System.Windows.Forms.Label lblLiftCarryLabel;
        private System.Windows.Forms.Label lblJudgeIntentions;
        private System.Windows.Forms.Label lblJudgeIntentionsLabel;
        private System.Windows.Forms.Label lblComposure;
        private System.Windows.Forms.Label lblComposureLabel;
        private System.Windows.Forms.ToolStripMenuItem tsAddArmorGear;
        private System.Windows.Forms.ContextMenuStrip cmsArmorGear;
        private System.Windows.Forms.ToolStripMenuItem tsArmorGearAddAsPlugin;
        private System.Windows.Forms.Label lblCMDamageResistancePool;
        private System.Windows.Forms.Label lblCMDamageResistancePoolLabel;
        private System.Windows.Forms.Label lblCMArmor;
        private System.Windows.Forms.Label lblCMArmorLabel;
        private System.Windows.Forms.ToolStripMenuItem tsArmorNotes;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponNotes;
        private System.Windows.Forms.ToolStripMenuItem tsCyberwareNotes;
        private System.Windows.Forms.ContextMenuStrip cmsArmorMod;
        private System.Windows.Forms.ToolStripMenuItem tsArmorModNotes;
        private System.Windows.Forms.ContextMenuStrip cmsQuality;
        private System.Windows.Forms.ToolStripMenuItem tsQualityNotes;
        private System.Windows.Forms.ToolStripMenuItem tsMartialArtsNotes;
        private System.Windows.Forms.ContextMenuStrip cmsMartialArtManeuver;
        private System.Windows.Forms.ToolStripMenuItem tsMartialArtManeuverNotes;
        private System.Windows.Forms.ContextMenuStrip cmsSpell;
        private System.Windows.Forms.ToolStripMenuItem tsSpellNotes;
        private System.Windows.Forms.ToolStripMenuItem tsComplexFormNotes;
        private System.Windows.Forms.ContextMenuStrip cmsCritterPowers;
        private System.Windows.Forms.ToolStripMenuItem tsCritterPowersNotes;
        private System.Windows.Forms.ContextMenuStrip cmsMetamagic;
        private System.Windows.Forms.ToolStripMenuItem tsMetamagicNotes;
        private System.Windows.Forms.ToolStripMenuItem tsGearNotes;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleNotes;
        private System.Windows.Forms.ContextMenuStrip cmsLifestyleNotes;
        private System.Windows.Forms.ToolStripMenuItem tsLifestyleNotes;
        private System.Windows.Forms.ToolStripMenuItem tsArmorGearNotes;
        private System.Windows.Forms.ContextMenuStrip cmsWeaponMod;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponModNotes;
        private System.Windows.Forms.ContextMenuStrip cmsWeaponAccessory;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponAccessoryNotes;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleWeaponNotes;
        private System.Windows.Forms.ContextMenuStrip cmsGearPlugin;
        private System.Windows.Forms.ToolStripMenuItem tsGearPluginNotes;
        private System.Windows.Forms.ContextMenuStrip cmsComplexFormPlugin;
        private System.Windows.Forms.ToolStripMenuItem tsComplexFormPluginNotes;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddWeaponWeapon;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddGear;
        private System.Windows.Forms.ToolStripMenuItem mnuCreateSpecial;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialCyberzombie;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialReduceAttribute;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddUnderbarrelWeapon;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddUnderbarrelWeaponAlt;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleName;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddCyberware;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialCloningMachine;
        private System.Windows.Forms.Label lblFly;
        private System.Windows.Forms.Label lblFlyLabel;
        private System.Windows.Forms.Label lblSwim;
        private System.Windows.Forms.Label lblSwimLabel;
        private System.Windows.Forms.ToolStripMenuItem tsArmorName;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.ContextMenuStrip cmsBioware;
        private System.Windows.Forms.ToolStripMenuItem tsBiowareNotes;
        private System.Windows.Forms.ContextMenuStrip cmsAdvancedLifestyle;
        private System.Windows.Forms.ToolStripMenuItem tsEditAdvancedLifestyle;
        private System.Windows.Forms.ToolStripMenuItem tsAdvancedLifestyleNotes;
        private System.Windows.Forms.ToolStripMenuItem tsLifestyleName;
        private System.Windows.Forms.ToolStripMenuItem mnuFileClose;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem tsEditKarmaExpense;
        private System.Windows.Forms.ToolStripMenuItem tsEditNuyenExpense;
        private System.Windows.Forms.ToolStripMenuItem mnuFileExport;
        private System.Windows.Forms.ContextMenuStrip cmsGearLocation;
        private System.Windows.Forms.ToolStripMenuItem tsGearRenameLocation;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialReapplyImprovements;
        private System.Windows.Forms.Label lblCareerNuyen;
        private System.Windows.Forms.Label lblCareerNuyenLabel;
        private System.Windows.Forms.ContextMenuStrip cmsSpellButton;
        private System.Windows.Forms.ToolStripMenuItem tsCreateSpell;
        private System.Windows.Forms.ToolStripMenuItem tsBoltHole;
        private System.Windows.Forms.ToolStripMenuItem tsSafehouse;
        private System.Windows.Forms.ContextMenuStrip cmsImprovement;
        private System.Windows.Forms.ToolStripMenuItem tsImprovementNotes;
        private System.Windows.Forms.ContextMenuStrip cmsArmorLocation;
        private System.Windows.Forms.ToolStripMenuItem tsArmorRenameLocation;
        private System.Windows.Forms.Label lblEDGInfo;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialPossess;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialPossessInanimate;
        private System.Windows.Forms.ContextMenuStrip cmsImprovementLocation;
        private System.Windows.Forms.ToolStripMenuItem tsImprovementRenameLocation;
        private System.Windows.Forms.ToolStripMenuItem tsEditLifestyle;
        private System.Windows.Forms.ContextMenuStrip cmsCyberwareGear;
        private System.Windows.Forms.ToolStripMenuItem tsCyberwareGearMenuAddAsPlugin;
        private System.Windows.Forms.ContextMenuStrip cmsWeaponAccessoryGear;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponAccessoryGearMenuAddAsPlugin;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponAccessoryAddGear;
        private System.Windows.Forms.ToolStripMenuItem tsCyberwareAddGear;
        private System.Windows.Forms.ContextMenuStrip cmsVehicleLocation;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleRenameLocation;
        private System.Windows.Forms.ToolStripButton tsbCopy;
        private System.Windows.Forms.ToolStripSeparator tsbSeparator;
        private System.Windows.Forms.ToolStripMenuItem mnuCreateEdit;
        private System.Windows.Forms.ToolStripMenuItem mnuEditCopy;
        private System.Windows.Forms.ToolStripMenuItem tsCreateNaturalWeapon;
        private System.Windows.Forms.ContextMenuStrip cmsVehicleWeaponAccessory;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleWeaponAccessoryAddGear;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleWeaponAccessoryNotes;
        private System.Windows.Forms.ContextMenuStrip cmsVehicleWeaponAccessoryGear;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleWeaponAccessoryGearMenuAddAsPlugin;
        private System.Windows.Forms.ContextMenuStrip cmsVehicleWeaponMod;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleWeaponModNotes;
        private System.Windows.Forms.ContextMenuStrip cmsWeaponLocation;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponRenameLocation;
        private System.Windows.Forms.ToolStripMenuItem tsGearName;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleGearNotes;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialConvertToFreeSprite;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialAddBiowareSuite;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialAddCyberwareSuite;
        private System.Windows.Forms.ContextMenuStrip cmsLimitModifier;
        private System.Windows.Forms.ToolStripMenuItem tssLimitModifierNotes;
        private System.Windows.Forms.Label lblArmorLabel;
        private System.Windows.Forms.Label lblRiggingINI;
        private System.Windows.Forms.Label lblRiggingINILabel;
        private System.Windows.Forms.Label lblMatrixINIHot;
        private System.Windows.Forms.Label lblMatrixINIHotLabel;
        private System.Windows.Forms.Label lblMatrixINICold;
        private System.Windows.Forms.Label lblMatrixINIColdLabel;
        private System.Windows.Forms.Label lblAstralINI;
        private System.Windows.Forms.Label lblMatrixINI;
        private System.Windows.Forms.Label lblINI;
        private System.Windows.Forms.Label lblAstralINILabel;
        private System.Windows.Forms.Label lblMatrixINILabel;
        private System.Windows.Forms.Label lblINILabel;
        private System.Windows.Forms.ToolStripMenuItem tsMetamagicAddArt;
        private System.Windows.Forms.ToolStripMenuItem tsMetamagicAddEnchantment;
        private System.Windows.Forms.ToolStripMenuItem tsMetamagicAddMetamagic;
        private System.Windows.Forms.ToolStripMenuItem tsMetamagicAddRitual;
        private System.Windows.Forms.ContextMenuStrip cmsInitiationNotes;
        private System.Windows.Forms.ToolStripMenuItem tsInitiationNotes;
        private System.Windows.Forms.ToolStripMenuItem tsMetamagicAddEnhancement;
        private System.Windows.Forms.ContextMenuStrip cmsTechnique;
        private System.Windows.Forms.ToolStripMenuItem tsAddTechniqueNotes;
        private System.Windows.Forms.TabPage tabDefences;
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
        private System.Windows.Forms.Label lblSpellDefenceDirectSoakPhysicalLabel;
        private System.Windows.Forms.Label lblSpellDefenceDirectSoakMana;
        private System.Windows.Forms.Label lblSpellDefenceDirectSoakManaLabel;
        private System.Windows.Forms.Label lblSpellDefenceIndirectSoak;
        private System.Windows.Forms.Label lblSpellDefenceIndirectSoakLabel;
        private System.Windows.Forms.Label lblSpellDefenceIndirectDodge;
        private System.Windows.Forms.Label lblSpellDefenceIndirectDodgeLabel;
        private System.Windows.Forms.ToolStripMenuItem tssLimitModifierEdit;
        private System.Windows.Forms.TabControl tabCharacterTabs;
        private System.Windows.Forms.TabPage tabCommon;
        private System.Windows.Forms.TabControl tabPeople;
        private System.Windows.Forms.TabPage tabContacts;
        private System.Windows.Forms.FlowLayoutPanel panContacts;
        private System.Windows.Forms.Button cmdAddContact;
        private System.Windows.Forms.Label lblContactArchtypeLabel;
        private System.Windows.Forms.Label lblContactNameLabel;
        private System.Windows.Forms.Label lblContactLocationLabel;
        private System.Windows.Forms.TabPage tabEnemies;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.FlowLayoutPanel panEnemies;
        private System.Windows.Forms.Button cmdAddEnemy;
        private System.Windows.Forms.Label lblPossessed;
        private System.Windows.Forms.TextBox txtAlias;
        private System.Windows.Forms.Label lblAlias;
        private System.Windows.Forms.Label lblMetatypeSource;
        private System.Windows.Forms.Label lblMetatypeSourceLabel;
        private System.Windows.Forms.Button cmdSwapQuality;
        private System.Windows.Forms.Label lblQualityBP;
        private System.Windows.Forms.Label lblQualityBPLabel;
        private System.Windows.Forms.Label lblQualitySource;
        private System.Windows.Forms.Label lblQualitySourceLabel;
        private System.Windows.Forms.Button cmdDeleteQuality;
        private System.Windows.Forms.Button cmdAddQuality;
        private helpers.TreeView treQualities;
        private System.Windows.Forms.Label lblMysticAdeptAssignment;
        private System.Windows.Forms.Label lblMysticAdeptMAGAdept;
        private System.Windows.Forms.Label lblMetatype;
        private System.Windows.Forms.Label lblMetatypeLabel;
        private System.Windows.Forms.Panel panAttributes;
        private System.Windows.Forms.TabPage tabSkills;
        private UI.Skills.SkillsTabUserControl tabSkillsUc;
        private System.Windows.Forms.TabPage tabLimits;
        private System.Windows.Forms.Label lblAstralLabel;
        private System.Windows.Forms.Label lblAstral;
        private System.Windows.Forms.Label lblSocialLimitLabel;
        private System.Windows.Forms.Label lblSocial;
        private System.Windows.Forms.Label lblMentalLimitLabel;
        private System.Windows.Forms.Label lblMental;
        private System.Windows.Forms.Label lblPhysicalLimitLabel;
        private System.Windows.Forms.Label lblPhysical;
        private System.Windows.Forms.Button cmdAddLimitModifier;
        private helpers.TreeView treLimit;
        private System.Windows.Forms.Button cmdDeleteLimitModifier;
        private System.Windows.Forms.TabPage tabMartialArts;
        private SplitButton cmdAddMartialArt;
        private System.Windows.Forms.Label lblMartialArtSource;
        private System.Windows.Forms.Label lblMartialArtSourceLabel;
        private helpers.TreeView treMartialArts;
        private System.Windows.Forms.Button cmdDeleteMartialArt;
        private System.Windows.Forms.TabPage tabMagician;
        private System.Windows.Forms.ComboBox cboSpiritManipulation;
        private System.Windows.Forms.Label lblSpiritManipulation;
        private System.Windows.Forms.ComboBox cboSpiritIllusion;
        private System.Windows.Forms.Label lblSpiritIllusion;
        private System.Windows.Forms.ComboBox cboSpiritHealth;
        private System.Windows.Forms.Label lblSpiritHealth;
        private System.Windows.Forms.ComboBox cboSpiritDetection;
        private System.Windows.Forms.Label lblSpiritDetection;
        private System.Windows.Forms.ComboBox cboSpiritCombat;
        private System.Windows.Forms.Label lblSpiritCombat;
        private System.Windows.Forms.ComboBox cboDrain;
        private System.Windows.Forms.TextBox txtTraditionName;
        private System.Windows.Forms.Label lblTraditionName;
        private System.Windows.Forms.Button cmdQuickenSpell;
        private System.Windows.Forms.Label lblSpellDicePool;
        private System.Windows.Forms.Label lblSpellDicePoolLabel;
        private System.Windows.Forms.Label lblMentorSpirit;
        private System.Windows.Forms.Label lblMentorSpiritLabel;
        private System.Windows.Forms.Label lblMentorSpiritInformation;
        private System.Windows.Forms.ComboBox cboTradition;
        private System.Windows.Forms.Label lblDrainAttributesValue;
        private System.Windows.Forms.Label lblDrainAttributes;
        private System.Windows.Forms.Label lblDrainAttributesLabel;
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
        private helpers.TreeView treSpells;
        private System.Windows.Forms.Button cmdDeleteSpell;
        private System.Windows.Forms.Button cmdAddSpirit;
        private System.Windows.Forms.Label lblSpirits;
        private System.Windows.Forms.Panel panSpirits;
        private System.Windows.Forms.Label lblSelectedSpells;
        private System.Windows.Forms.Button cmdRollDrain;
        private System.Windows.Forms.Button cmdRollSpell;
        private SplitButton cmdAddSpell;
        private System.Windows.Forms.TabPage tabAdept;
        private System.Windows.Forms.TabPage tabTechnomancer;
        private System.Windows.Forms.Label lblFV;
        private System.Windows.Forms.Label lblFVLabel;
        private System.Windows.Forms.Label lblDuration;
        private System.Windows.Forms.Label lblDurationLabel;
        private System.Windows.Forms.Label lblTarget;
        private System.Windows.Forms.Label lblTargetLabel;
        private System.Windows.Forms.Label lblComplexFormSource;
        private System.Windows.Forms.Label lblComplexFormSourceLabel;
        private System.Windows.Forms.Label lblLivingPersonaFirewall;
        private System.Windows.Forms.Label lblLivingPersonaFirewallLabel;
        private System.Windows.Forms.Label lblLivingPersonaDataProcessing;
        private System.Windows.Forms.Label lblLivingPersonaDataProcessingLabel;
        private System.Windows.Forms.Label lblLivingPersonaSleaze;
        private System.Windows.Forms.Label lblLivingPersonaSleazeLabel;
        private System.Windows.Forms.Label lblLivingPersonaAttack;
        private System.Windows.Forms.Label lblLivingPersonaAttackLabel;
        private System.Windows.Forms.Label lblLivingPersonaLabel;
        private System.Windows.Forms.Label lblLivingPersonaDeviceRating;
        private System.Windows.Forms.Label lblLivingPersonaDeviceRatingLabel;
        private System.Windows.Forms.Button cmdRollFading;
        private System.Windows.Forms.ComboBox cboStream;
        private System.Windows.Forms.Label lblFadingAttributesValue;
        private System.Windows.Forms.Label lblFadingAttributes;
        private System.Windows.Forms.Label lblFadingAttributesLabel;
        private System.Windows.Forms.Label lblStreamLabel;
        private helpers.TreeView treComplexForms;
        private System.Windows.Forms.Button cmdDeleteComplexForm;
        private System.Windows.Forms.Label lblComplexForms;
        private System.Windows.Forms.Button cmdAddSprite;
        private System.Windows.Forms.Label lblSprites;
        private System.Windows.Forms.Panel panSprites;
        private SplitButton cmdAddComplexForm;
        private System.Windows.Forms.TabPage tabCritter;
        private System.Windows.Forms.CheckBox chkCritterPowerCount;
        private System.Windows.Forms.Label lblCritterPowerPointCost;
        private System.Windows.Forms.Label lblCritterPowerPointCostLabel;
        private System.Windows.Forms.Label lblCritterPowerPoints;
        private System.Windows.Forms.Label lblCritterPowerPointsLabel;
        private System.Windows.Forms.Button cmdDeleteCritterPower;
        private System.Windows.Forms.Button cmdAddCritterPower;
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
        private helpers.TreeView treCritterPowers;
        private System.Windows.Forms.TabPage tabAdvancedPrograms;
        private System.Windows.Forms.Button cmdAddAIProgram;
        private System.Windows.Forms.Label lblAIProgramsRequires;
        private System.Windows.Forms.Label lblAIProgramsRequiresLabel;
        private System.Windows.Forms.Label lblAIProgramsSource;
        private System.Windows.Forms.Label lblAIProgramsSourceLabel;
        private helpers.TreeView treAIPrograms;
        private System.Windows.Forms.Button cmdDeleteAIProgram;
        private System.Windows.Forms.Label lblAIProgramsAdvancedPrograms;
        private System.Windows.Forms.TabPage tabInitiation;
        private System.Windows.Forms.CheckBox chkInitiationSchooling;
        private System.Windows.Forms.CheckBox chkInitiationOrdeal;
        private System.Windows.Forms.CheckBox chkInitiationGroup;
        private System.Windows.Forms.CheckBox chkJoinGroup;
        private System.Windows.Forms.TextBox txtGroupNotes;
        private System.Windows.Forms.TextBox txtGroupName;
        private System.Windows.Forms.Label lblGroupNotes;
        private System.Windows.Forms.Label lblGroupName;
        private System.Windows.Forms.Label lblMetamagicSource;
        private System.Windows.Forms.Label lblMetamagicSourceLabel;
        private System.Windows.Forms.TreeView treMetamagic;
        private System.Windows.Forms.Button cmdAddMetamagic;
        private System.Windows.Forms.TabPage tabCyberware;
        private System.Windows.Forms.Label lblCyberlimbSTR;
        private System.Windows.Forms.Label lblCyberlimbAGI;
        private System.Windows.Forms.Label lblCyberlimbSTRLabel;
        private System.Windows.Forms.Label lblCyberlimbAGILabel;
        private System.Windows.Forms.ComboBox cboCyberwareGearOverclocker;
        private System.Windows.Forms.Label lblCyberwareGearOverclocker;
        private System.Windows.Forms.ComboBox cboCyberwareGearDataProcessing;
        private System.Windows.Forms.ComboBox cboCyberwareGearFirewall;
        private System.Windows.Forms.ComboBox cboCyberwareGearSleaze;
        private System.Windows.Forms.ComboBox cboCyberwareGearAttack;
        private System.Windows.Forms.TabControl tabCyberwareCM;
        private System.Windows.Forms.TabPage tabCyberwareMatrixCM;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM1;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM2;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM3;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM4;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM5;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM6;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM7;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM8;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM9;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM10;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM11;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM12;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM13;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM14;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM15;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM16;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM17;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM18;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM19;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM20;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM21;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM22;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM23;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM24;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM25;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM26;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM27;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM28;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM29;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM30;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM31;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM32;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM33;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM34;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM35;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM36;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM37;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM38;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM39;
        private System.Windows.Forms.CheckBox chkCyberwareMatrixCM40;
        private System.Windows.Forms.Label lblCyberFirewallLabel;
        private System.Windows.Forms.Label lblCyberDataProcessingLabel;
        private System.Windows.Forms.Label lblCyberSleazeLabel;
        private System.Windows.Forms.Label lblCyberAttackLabel;
        private System.Windows.Forms.Label lblCyberDeviceRating;
        private System.Windows.Forms.Label lblCyberDeviceRatingLabel;
        private System.Windows.Forms.Label lblEssenceHoleESS;
        private System.Windows.Forms.Label lblEssenceHoleESSLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblBiowareESS;
        private System.Windows.Forms.Label lblCyberwareESS;
        private System.Windows.Forms.Label lblBiowareESSLabel;
        private System.Windows.Forms.Label lblCyberwareESSLabel;
        private System.Windows.Forms.Label lblCyberwareRating;
        private System.Windows.Forms.Label lblCyberwareGrade;
        private System.Windows.Forms.Label lblCyberwareSource;
        private System.Windows.Forms.Label lblCyberwareSourceLabel;
        private System.Windows.Forms.Button cmdAddBioware;
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
        private helpers.TreeView treCyberware;
        private SplitButton cmdAddCyberware;
        private SplitButton cmdDeleteCyberware;
        private System.Windows.Forms.TabPage tabStreetGear;
        private System.Windows.Forms.TabControl tabStreetGearTabs;
        private System.Windows.Forms.TabPage tabLifestyle;
        private SplitButton cmdAddLifestyle;
        private System.Windows.Forms.Label lblBaseLifestyle;
        private System.Windows.Forms.Label lblLifestyleComfortsLabel;
        private System.Windows.Forms.Label lblLifestyleQualities;
        private System.Windows.Forms.Label lblLifestyleQualitiesLabel;
        private System.Windows.Forms.Button cmdIncreaseLifestyleMonths;
        private System.Windows.Forms.Button cmdDecreaseLifestyleMonths;
        private System.Windows.Forms.Label lblLifestyleMonths;
        private System.Windows.Forms.Label lblLifestyleSource;
        private System.Windows.Forms.Label lblLifestyleSourceLabel;
        private System.Windows.Forms.Label lblLifestyleCostLabel;
        private helpers.TreeView treLifestyles;
        private System.Windows.Forms.Label lblLifestyleCost;
        private System.Windows.Forms.Button cmdDeleteLifestyle;
        private System.Windows.Forms.Label lblLifestyleMonthsLabel;
        private System.Windows.Forms.TabPage tabArmor;
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
        private System.Windows.Forms.Label lblArmorValueLabel;
        private System.Windows.Forms.Label lblArmorValue;
        private System.Windows.Forms.CheckBox chkIncludedInArmor;
        private System.Windows.Forms.Label lblArmorEquipped;
        private System.Windows.Forms.Label lblArmorEquippedLabel;
        private System.Windows.Forms.Button cmdArmorUnEquipAll;
        private System.Windows.Forms.Button cmdArmorEquipAll;
        private System.Windows.Forms.Button cmdAddArmorBundle;
        private System.Windows.Forms.Label lblArmorCapacity;
        private System.Windows.Forms.Label lblArmorCapacityLabel;
        private System.Windows.Forms.Label lblArmorRating;
        private System.Windows.Forms.Label lblArmorRatingLabel;
        private System.Windows.Forms.Label lblArmorSource;
        private System.Windows.Forms.Label lblArmorSourceLabel;
        private System.Windows.Forms.CheckBox chkArmorEquipped;
        private System.Windows.Forms.Label lblArmorCost;
        private System.Windows.Forms.Label lblArmorCostLabel;
        private System.Windows.Forms.Label lblArmorAvail;
        private helpers.TreeView treArmor;
        private System.Windows.Forms.Label lblArmorAvailLabel;
        private System.Windows.Forms.Button cmdArmorIncrease;
        private System.Windows.Forms.Button cmdArmorDecrease;
        private SplitButton cmdAddArmor;
        private SplitButton cmdDeleteArmor;
        private System.Windows.Forms.TabPage tabWeapons;
        private System.Windows.Forms.ComboBox cboWeaponGearDataProcessing;
        private System.Windows.Forms.ComboBox cboWeaponGearFirewall;
        private System.Windows.Forms.ComboBox cboWeaponGearSleaze;
        private System.Windows.Forms.ComboBox cboWeaponGearAttack;
        private System.Windows.Forms.Label lblWeaponRating;
        private System.Windows.Forms.Label lblWeaponRatingLabel;
        private System.Windows.Forms.Label lblWeaponFirewallLabel;
        private System.Windows.Forms.Label lblWeaponDataProcessingLabel;
        private System.Windows.Forms.Label lblWeaponSleazeLabel;
        private System.Windows.Forms.Label lblWeaponAttackLabel;
        private System.Windows.Forms.Label lblWeaponDeviceRating;
        private System.Windows.Forms.Label lblWeaponDeviceRatingLabel;
        private System.Windows.Forms.Label lblWeaponAccuracyLabel;
        private System.Windows.Forms.Label lblWeaponAccuracy;
        private System.Windows.Forms.Button cmdAddWeaponLocation;
        private System.Windows.Forms.ComboBox cboWeaponAmmo;
        private System.Windows.Forms.Label lblWeaponDicePool;
        private System.Windows.Forms.Label lblWeaponDicePoolLabel;
        private System.Windows.Forms.Label lblWeaponConceal;
        private System.Windows.Forms.Label lblWeaponConcealLabel;
        private System.Windows.Forms.Label lblWeaponRangeExtreme;
        private System.Windows.Forms.Label lblWeaponRangeLong;
        private System.Windows.Forms.Label lblWeaponRangeMedium;
        private System.Windows.Forms.Label lblWeaponRangeShort;
        private System.Windows.Forms.Label lblWeaponRangeExtremeLabel;
        private System.Windows.Forms.Label lblWeaponRangeLongLabel;
        private System.Windows.Forms.Label lblWeaponRangeMediumLabel;
        private System.Windows.Forms.Label lblWeaponRangeShortLabel;
        private System.Windows.Forms.Label lblWeaponRangeLabel;
        private System.Windows.Forms.CheckBox chkIncludedInWeapon;
        private System.Windows.Forms.CheckBox chkWeaponAccessoryInstalled;
        private System.Windows.Forms.Button cmdReloadWeapon;
        private System.Windows.Forms.Label lblWeaponAmmoTypeLabel;
        private System.Windows.Forms.Label lblWeaponAmmoRemaining;
        private System.Windows.Forms.Label lblWeaponAmmoRemainingLabel;
        private System.Windows.Forms.Label lblWeaponSlots;
        private System.Windows.Forms.Label lblWeaponSlotsLabel;
        private System.Windows.Forms.Label lblWeaponSource;
        private System.Windows.Forms.Label lblWeaponSourceLabel;
        private System.Windows.Forms.Label lblWeaponAmmo;
        private System.Windows.Forms.Label lblWeaponAmmoLabel;
        private helpers.TreeView treWeapons;
        private System.Windows.Forms.Label lblWeaponMode;
        private System.Windows.Forms.Label lblWeaponModeLabel;
        private System.Windows.Forms.Label lblWeaponNameLabel;
        private System.Windows.Forms.Label lblWeaponReach;
        private System.Windows.Forms.Label lblWeaponName;
        private System.Windows.Forms.Label lblWeaponReachLabel;
        private System.Windows.Forms.Label lblWeaponCategoryLabel;
        private System.Windows.Forms.Label lblWeaponAP;
        private System.Windows.Forms.Label lblWeaponCategory;
        private System.Windows.Forms.Label lblWeaponAPLabel;
        private System.Windows.Forms.Label lblWeaponDamageLabel;
        private System.Windows.Forms.Label lblWeaponCost;
        private System.Windows.Forms.Label lblWeaponDamage;
        private System.Windows.Forms.Label lblWeaponCostLabel;
        private System.Windows.Forms.Label lblWeaponRCLabel;
        private System.Windows.Forms.Label lblWeaponAvail;
        private System.Windows.Forms.Label lblWeaponRC;
        private System.Windows.Forms.Label lblWeaponAvailLabel;
        private System.Windows.Forms.Button cmdRollWeapon;
        private System.Windows.Forms.Button cmdWeaponMoveToVehicle;
        private System.Windows.Forms.Button cmdWeaponBuyAmmo;
        private SplitButton cmdAddWeapon;
        private SplitButton cmdDeleteWeapon;
        private SplitButton cmdFireWeapon;
        private System.Windows.Forms.TabPage tabGear;
        private System.Windows.Forms.ComboBox cboGearOverclocker;
        private System.Windows.Forms.Label lblGearOverclocker;
        private System.Windows.Forms.TabControl tabGearMatrixCM;
        private System.Windows.Forms.TabPage tabMatrixCM;
        private System.Windows.Forms.CheckBox chkGearMatrixCM1;
        private System.Windows.Forms.CheckBox chkGearMatrixCM2;
        private System.Windows.Forms.CheckBox chkGearMatrixCM3;
        private System.Windows.Forms.CheckBox chkGearMatrixCM4;
        private System.Windows.Forms.CheckBox chkGearMatrixCM5;
        private System.Windows.Forms.CheckBox chkGearMatrixCM6;
        private System.Windows.Forms.CheckBox chkGearMatrixCM7;
        private System.Windows.Forms.CheckBox chkGearMatrixCM8;
        private System.Windows.Forms.CheckBox chkGearMatrixCM9;
        private System.Windows.Forms.CheckBox chkGearMatrixCM10;
        private System.Windows.Forms.CheckBox chkGearMatrixCM11;
        private System.Windows.Forms.CheckBox chkGearMatrixCM12;
        private System.Windows.Forms.CheckBox chkGearMatrixCM13;
        private System.Windows.Forms.CheckBox chkGearMatrixCM14;
        private System.Windows.Forms.CheckBox chkGearMatrixCM15;
        private System.Windows.Forms.CheckBox chkGearMatrixCM16;
        private System.Windows.Forms.CheckBox chkGearMatrixCM17;
        private System.Windows.Forms.CheckBox chkGearMatrixCM18;
        private System.Windows.Forms.CheckBox chkGearMatrixCM19;
        private System.Windows.Forms.CheckBox chkGearMatrixCM20;
        private System.Windows.Forms.CheckBox chkGearMatrixCM21;
        private System.Windows.Forms.CheckBox chkGearMatrixCM22;
        private System.Windows.Forms.CheckBox chkGearMatrixCM23;
        private System.Windows.Forms.CheckBox chkGearMatrixCM24;
        private System.Windows.Forms.ComboBox cboGearDataProcessing;
        private System.Windows.Forms.ComboBox cboGearFirewall;
        private System.Windows.Forms.ComboBox cboGearSleaze;
        private System.Windows.Forms.ComboBox cboGearAttack;
        private System.Windows.Forms.Label lblGearFirewallLabel;
        private System.Windows.Forms.Label lblGearDataProcessingLabel;
        private System.Windows.Forms.Label lblGearSleazeLabel;
        private System.Windows.Forms.Label lblGearAttackLabel;
        private System.Windows.Forms.Label lblGearDeviceRating;
        private System.Windows.Forms.Label lblGearDeviceRatingLabel;
        private System.Windows.Forms.CheckBox chkActiveCommlink;
        private System.Windows.Forms.CheckBox chkCommlinks;
        private System.Windows.Forms.Button cmdCreateStackedFocus;
        private System.Windows.Forms.CheckBox chkGearHomeNode;
        private System.Windows.Forms.Label lblGearAP;
        private System.Windows.Forms.Label lblGearAPLabel;
        private System.Windows.Forms.Label lblGearDamage;
        private System.Windows.Forms.Label lblGearDamageLabel;
        private System.Windows.Forms.Button cmdAddLocation;
        private System.Windows.Forms.CheckBox chkGearEquipped;
        private System.Windows.Forms.Label lblGearRating;
        private System.Windows.Forms.Label lblGearQty;
        private System.Windows.Forms.Label lblFoci;
        private System.Windows.Forms.TreeView treFoci;
        private System.Windows.Forms.Label lblGearSource;
        private System.Windows.Forms.Label lblGearSourceLabel;
        private System.Windows.Forms.Label lblGearQtyLabel;
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
        private System.Windows.Forms.Label lblGearRatingLabel;
        private helpers.TreeView treGear;
        private System.Windows.Forms.Button cmdGearMoveToVehicle;
        private System.Windows.Forms.Button cmdGearMergeQty;
        private System.Windows.Forms.Button cmdGearSplitQty;
        private System.Windows.Forms.Button cmdGearIncreaseQty;
        private System.Windows.Forms.Button cmdGearReduceQty;
        private SplitButton cmdAddGear;
        private SplitButton cmdDeleteGear;
        private System.Windows.Forms.TabPage tabPets;
        private System.Windows.Forms.FlowLayoutPanel panPets;
        private System.Windows.Forms.Button cmdAddPet;
        private System.Windows.Forms.TabPage tabVehicles;
        private System.Windows.Forms.Label lblVehicleSeats;
        private System.Windows.Forms.Label lblVehicleSeatsLabel;
        private System.Windows.Forms.Label lblVehicleDroneModSlots;
        private System.Windows.Forms.Label lblVehicleDroneModSlotsLabel;
        private System.Windows.Forms.Label lblVehicleCosmetic;
        private System.Windows.Forms.Label lblVehicleElectromagnetic;
        private System.Windows.Forms.Label lblVehicleBodymod;
        private System.Windows.Forms.Label lblVehicleWeaponsmod;
        private System.Windows.Forms.Label lblVehicleProtection;
        private System.Windows.Forms.Label lblVehiclePowertrain;
        private System.Windows.Forms.Label lblVehicleCosmeticLabel;
        private System.Windows.Forms.Label lblVehicleElectromagneticLabel;
        private System.Windows.Forms.Label lblVehicleBodymodLabel;
        private System.Windows.Forms.Label lblVehicleWeaponsmodLabel;
        private System.Windows.Forms.Label lblVehicleProtectionLabel;
        private System.Windows.Forms.Label lblVehiclePowertrainLabel;
        private System.Windows.Forms.ComboBox cboVehicleGearDataProcessing;
        private System.Windows.Forms.ComboBox cboVehicleGearFirewall;
        private System.Windows.Forms.ComboBox cboVehicleGearSleaze;
        private System.Windows.Forms.ComboBox cboVehicleGearAttack;
        private System.Windows.Forms.TabControl panVehicleCM;
        private System.Windows.Forms.TabPage tabVehiclePhysicalCM;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM40;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM1;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM39;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM2;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM38;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM3;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM37;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM4;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM36;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM5;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM35;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM6;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM34;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM7;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM33;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM8;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM32;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM9;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM31;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM10;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM30;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM11;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM29;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM12;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM28;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM13;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM27;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM14;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM26;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM15;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM25;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM16;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM24;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM17;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM23;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM18;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM22;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM19;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM21;
        private System.Windows.Forms.CheckBox chkVehiclePhysicalCM20;
        private System.Windows.Forms.TabPage tabVehicleMatrixCM;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM1;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM2;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM3;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM4;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM5;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM6;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM7;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM8;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM9;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM10;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM11;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM12;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM13;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM14;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM15;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM16;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM17;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM18;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM19;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM20;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM21;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM22;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM23;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM24;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM25;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM26;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM27;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM28;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM29;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM30;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM31;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM32;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM33;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM34;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM35;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM36;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM37;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM38;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM39;
        private System.Windows.Forms.CheckBox chkVehicleMatrixCM40;
        private System.Windows.Forms.Label lblVehicleFirewallLabel;
        private System.Windows.Forms.Label lblVehicleDataProcessingLabel;
        private System.Windows.Forms.Label lblVehicleSleazeLabel;
        private System.Windows.Forms.Label lblVehicleAttackLabel;
        private System.Windows.Forms.Button cmdAddVehicleLocation;
        private System.Windows.Forms.CheckBox chkVehicleHomeNode;
        private System.Windows.Forms.Label lblVehicleWeaponDicePool;
        private System.Windows.Forms.Label lblVehicleWeaponDicePoolLabel;
        private System.Windows.Forms.Label lblVehicleDevice;
        private System.Windows.Forms.Label lblVehicleDeviceLabel;
        private System.Windows.Forms.ComboBox cboVehicleWeaponAmmo;
        private System.Windows.Forms.Label lblVehicleGearQty;
        private System.Windows.Forms.Label lblVehicleGearQtyLabel;
        private System.Windows.Forms.Label lblVehicleWeaponRangeExtreme;
        private System.Windows.Forms.Label lblVehicleWeaponRangeLong;
        private System.Windows.Forms.Label lblVehicleWeaponRangeMedium;
        private System.Windows.Forms.Label lblVehicleWeaponRangeShort;
        private System.Windows.Forms.Label lblVehicleWeaponRangeExtremeLabel;
        private System.Windows.Forms.Label lblVehicleWeaponRangeLongLabel;
        private System.Windows.Forms.Label lblVehicleWeaponRangeMediumLabel;
        private System.Windows.Forms.Label lblVehicleWeaponRangeShortLabel;
        private System.Windows.Forms.Label lblVehicleWeaponRangeLabel;
        private System.Windows.Forms.CheckBox chkVehicleIncludedInWeapon;
        private System.Windows.Forms.CheckBox chkVehicleWeaponAccessoryInstalled;
        private System.Windows.Forms.Label lblVehicleWeaponAmmo;
        private System.Windows.Forms.Label lblVehicleWeaponAmmoLabel;
        private System.Windows.Forms.Label lblVehicleWeaponMode;
        private System.Windows.Forms.Label lblVehicleWeaponModeLabel;
        private System.Windows.Forms.Button cmdReloadVehicleWeapon;
        private System.Windows.Forms.Label lblVehicleWeaponAmmoTypeLabel;
        private System.Windows.Forms.Label lblVehicleWeaponAmmoRemaining;
        private System.Windows.Forms.Label lblVehicleWeaponAmmoRemainingLabel;
        private System.Windows.Forms.Label lblVehicleWeaponNameLabel;
        private System.Windows.Forms.Label lblVehicleWeaponName;
        private System.Windows.Forms.Label lblVehicleWeaponCategoryLabel;
        private System.Windows.Forms.Label lblVehicleWeaponAP;
        private System.Windows.Forms.Label lblVehicleWeaponCategory;
        private System.Windows.Forms.Label lblVehicleWeaponAPLabel;
        private System.Windows.Forms.Label lblVehicleWeaponDamageLabel;
        private System.Windows.Forms.Label lblVehicleWeaponDamage;
        private System.Windows.Forms.Label lblVehicleRating;
        private System.Windows.Forms.Label lblVehicleSource;
        private System.Windows.Forms.Label lblVehicleSourceLabel;
        private System.Windows.Forms.Label lblVehicleSlots;
        private System.Windows.Forms.Label lblVehicleSlotsLabel;
        private System.Windows.Forms.Label lblVehicleRatingLabel;
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
        private helpers.TreeView treVehicles;
        private System.Windows.Forms.Button cmdRollVehicleWeapon;
        private System.Windows.Forms.Button cmdVehicleMoveToInventory;
        private System.Windows.Forms.Button cmdVehicleGearReduceQty;
        private SplitButton cmdAddVehicle;
        private SplitButton cmdFireVehicleWeapon;
        private SplitButton cmdDeleteVehicle;
        private System.Windows.Forms.TabPage tabCharacterInfo;
        private System.Windows.Forms.Button cmdBurnStreetCred;
        private System.Windows.Forms.Label lblPublicAwareTotal;
        private System.Windows.Forms.Label lblNotorietyTotal;
        private System.Windows.Forms.Label lblStreetCredTotal;
        private System.Windows.Forms.Label lblCharacterName;
        private System.Windows.Forms.TextBox txtCharacterName;
        private System.Windows.Forms.TextBox txtPlayerName;
        private System.Windows.Forms.TextBox txtNotes;
        private System.Windows.Forms.TextBox txtConcept;
        private System.Windows.Forms.TextBox txtBackground;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.TextBox txtSkin;
        private System.Windows.Forms.TextBox txtWeight;
        private System.Windows.Forms.TextBox txtHeight;
        private System.Windows.Forms.TextBox txtHair;
        private System.Windows.Forms.TextBox txtEyes;
        private System.Windows.Forms.TextBox txtAge;
        private System.Windows.Forms.TextBox txtSex;
        private System.Windows.Forms.NumericUpDown nudPublicAware;
        private System.Windows.Forms.Label lblPublicAware;
        private System.Windows.Forms.NumericUpDown nudNotoriety;
        private System.Windows.Forms.Label lblNotoriety;
        private System.Windows.Forms.NumericUpDown nudStreetCred;
        private System.Windows.Forms.Label lblStreetCred;
        private System.Windows.Forms.Label lblPlayerName;
        private System.Windows.Forms.Label lblNotes;
        private System.Windows.Forms.Button cmdDeleteMugshot;
        private System.Windows.Forms.Button cmdAddMugshot;
        private System.Windows.Forms.Label lblMugshot;
        private System.Windows.Forms.Label lblConcept;
        private System.Windows.Forms.Label lblBackground;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblSkin;
        private System.Windows.Forms.Label lblWeight;
        private System.Windows.Forms.Label lblHeight;
        private System.Windows.Forms.Label lblHair;
        private System.Windows.Forms.Label lblEyes;
        private System.Windows.Forms.Label lblAge;
        private System.Windows.Forms.Label lblSex;
        private System.Windows.Forms.PictureBox picMugshot;
        private System.Windows.Forms.TabPage tabKarma;
        private System.Windows.Forms.SplitContainer splitKarmaNuyen;
        private System.Windows.Forms.CheckBox chkShowFreeKarma;
        private System.Windows.Forms.DataVisualization.Charting.Chart chtKarma;
        private System.Windows.Forms.Button cmdKarmaEdit;
        private System.Windows.Forms.Button cmdKarmaGained;
        private System.Windows.Forms.ListView lstKarma;
        private System.Windows.Forms.ColumnHeader colKarmaDate;
        private System.Windows.Forms.ColumnHeader colKarmaAmount;
        private System.Windows.Forms.ColumnHeader colKarmaReason;
        private System.Windows.Forms.Button cmdKarmaSpent;
        private System.Windows.Forms.CheckBox chkShowFreeNuyen;
        private System.Windows.Forms.DataVisualization.Charting.Chart chtNuyen;
        private System.Windows.Forms.Button cmdNuyenEdit;
        private System.Windows.Forms.ListView lstNuyen;
        private System.Windows.Forms.ColumnHeader colNuyenDate;
        private System.Windows.Forms.ColumnHeader colNuyenAmount;
        private System.Windows.Forms.ColumnHeader colNuyenReason;
        private System.Windows.Forms.Button cmdNuyenSpent;
        private System.Windows.Forms.Button cmdNuyenGained;
        private System.Windows.Forms.TabPage tabCalendar;
        private System.Windows.Forms.Button cmdDeleteWeek;
        private System.Windows.Forms.Button cmdChangeStartWeek;
        private System.Windows.Forms.Button cmdEditWeek;
        private System.Windows.Forms.Button cmdAddWeek;
        private System.Windows.Forms.ListView lstCalendar;
        private System.Windows.Forms.ColumnHeader colCalendarDate;
        private System.Windows.Forms.ColumnHeader colCalendarNotes;
        private System.Windows.Forms.TabPage tabNotes;
        private System.Windows.Forms.TextBox txtGameNotes;
        private System.Windows.Forms.TabPage tabImprovements;
        private System.Windows.Forms.Button cmdImprovementsDisableAll;
        private System.Windows.Forms.Button cmdImprovementsEnableAll;
        private System.Windows.Forms.Button cmdAddImprovementGroup;
        private System.Windows.Forms.Button cmdDeleteImprovement;
        private System.Windows.Forms.Button cmdEditImprovement;
        private System.Windows.Forms.CheckBox chkImprovementActive;
        private System.Windows.Forms.Label lblImprovementValue;
        private System.Windows.Forms.Label lblImprovementType;
        private System.Windows.Forms.Label lblImprovementTypeLabel;
        private helpers.TreeView treImprovements;
        private System.Windows.Forms.Button cmdAddImprovement;
        private System.Windows.Forms.ContextMenuStrip cmsAdvancedProgram;
        private System.Windows.Forms.ToolStripMenuItem tsAddAdvancedProgramOption;
        private System.Windows.Forms.ToolStripMenuItem tsAIProgramNotes;
        private ComboBox cboPrimaryArm;
        private System.Windows.Forms.Label lblHandedness;
        private System.Windows.Forms.Label lblMugshotDimensions;
        private System.Windows.Forms.Label lblNumMugshots;
        private System.Windows.Forms.NumericUpDown nudMugshotIndex;
        private System.Windows.Forms.CheckBox chkIsMainMugshot;
        private System.Windows.Forms.Label lblTraditionSource;
        private System.Windows.Forms.Label lblTraditionSourceLabel;
        private PowersTabUserControl tabPowerUc;
        private FlowLayoutPanel pnlAttributes;
        private Label lblAttributes;
        private Label lblAttributesAug;
        private Label lblAttributesMetatype;
        private Chummer.helpers.Button cmdIncreasePowerPoints;
    }
}
