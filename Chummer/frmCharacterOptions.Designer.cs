namespace Chummer
{
    partial class frmCharacterOptions
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
            if(disposing)
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmCharacterOptions));
            this.tlpOptions = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cboSetting = new Chummer.ElasticComboBox();
            this.tabOptions = new System.Windows.Forms.TabControl();
            this.tabBasicOptions = new System.Windows.Forms.TabPage();
            this.tlpBasicOptions = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdEnableSourcebooks = new System.Windows.Forms.Button();
            this.gpbSourcebook = new System.Windows.Forms.GroupBox();
            this.treSourcebook = new System.Windows.Forms.TreeView();
            this.flpBasicOptions = new System.Windows.Forms.FlowLayoutPanel();
            this.gpbBasicOptionsCreateSettings = new System.Windows.Forms.GroupBox();
            this.tlpBasicOptionsCreateSettings = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblAllowedCyberwareGrades = new System.Windows.Forms.Label();
            this.nudMaxAvail = new Chummer.NumericUpDownEx();
            this.cboPriorityTable = new Chummer.ElasticComboBox();
            this.lblPriorityTable = new System.Windows.Forms.Label();
            this.lblBuildMethod = new System.Windows.Forms.Label();
            this.cboBuildMethod = new Chummer.ElasticComboBox();
            this.lblMaxNuyenKarma = new System.Windows.Forms.Label();
            this.lblMaxAvail = new System.Windows.Forms.Label();
            this.nudMaxNuyenKarma = new Chummer.NumericUpDownEx();
            this.flpAllowedCyberwareGrades = new System.Windows.Forms.FlowLayoutPanel();
            this.lblKnowledgePoints = new Chummer.LabelWithToolTip();
            this.lblContactPoints = new Chummer.LabelWithToolTip();
            this.txtKnowledgePoints = new System.Windows.Forms.TextBox();
            this.lblQualityKarmaLimit = new System.Windows.Forms.Label();
            this.txtContactPoints = new System.Windows.Forms.TextBox();
            this.nudQualityKarmaLimit = new Chummer.NumericUpDownEx();
            this.lblStartingKarma = new System.Windows.Forms.Label();
            this.nudStartingKarma = new Chummer.NumericUpDownEx();
            this.lblPriorities = new Chummer.LabelWithToolTip();
            this.txtPriorities = new System.Windows.Forms.TextBox();
            this.nudSumToTen = new Chummer.NumericUpDownEx();
            this.lblSumToTen = new System.Windows.Forms.Label();
            this.gpbBasicOptionsOfficialRules = new System.Windows.Forms.GroupBox();
            this.tlpBasicOptionsOfficialRules = new Chummer.BufferedTableLayoutPanel(this.components);
            this.chkEnemyKarmaQualityLimit = new Chummer.ColorableCheckBox(this.components);
            this.chkAllowFreeGrids = new Chummer.ColorableCheckBox(this.components);
            this.chkAllowPointBuySpecializationsOnKarmaSkills = new Chummer.ColorableCheckBox(this.components);
            this.chkStrictSkillGroups = new Chummer.ColorableCheckBox(this.components);
            this.chkEnforceCapacity = new Chummer.ColorableCheckBox(this.components);
            this.chkLicenseEachRestrictedItem = new Chummer.ColorableCheckBox(this.components);
            this.chkRestrictRecoil = new Chummer.ColorableCheckBox(this.components);
            this.chkDronemodsMaximumPilot = new Chummer.ColorableCheckBox(this.components);
            this.chkDronemods = new Chummer.ColorableCheckBox(this.components);
            this.gpbBasicOptionsCyberlimbs = new System.Windows.Forms.GroupBox();
            this.tlpBasicOptionsCyberlimbs = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblRedlinerLimbs = new System.Windows.Forms.Label();
            this.chkCyberlegMovement = new Chummer.ColorableCheckBox(this.components);
            this.chkDontUseCyberlimbCalculation = new Chummer.ColorableCheckBox(this.components);
            this.lblLimbCount = new System.Windows.Forms.Label();
            this.cboLimbCount = new Chummer.ElasticComboBox();
            this.tlpCyberlimbAttributeBonusCap = new Chummer.BufferedTableLayoutPanel(this.components);
            this.nudCyberlimbAttributeBonusCap = new Chummer.NumericUpDownEx();
            this.flpCyberlimbAttributeBonusCap = new System.Windows.Forms.FlowLayoutPanel();
            this.chkCyberlimbAttributeBonusCap = new Chummer.ColorableCheckBox(this.components);
            this.lblCyberlimbAttributeBonusCapPlus = new System.Windows.Forms.Label();
            this.flpRedlinerLimbs = new System.Windows.Forms.FlowLayoutPanel();
            this.chkRedlinerLimbsSkull = new Chummer.ColorableCheckBox(this.components);
            this.chkRedlinerLimbsTorso = new Chummer.ColorableCheckBox(this.components);
            this.chkRedlinerLimbsArms = new Chummer.ColorableCheckBox(this.components);
            this.chkRedlinerLimbsLegs = new Chummer.ColorableCheckBox(this.components);
            this.gpbBasicOptionsRounding = new System.Windows.Forms.GroupBox();
            this.tlpBasicOptionsRounding = new Chummer.BufferedTableLayoutPanel(this.components);
            this.chkDontRoundEssenceInternally = new Chummer.ColorableCheckBox(this.components);
            this.lblNuyenDecimalsMinimumLabel = new System.Windows.Forms.Label();
            this.lblNuyenDecimalsMaximumLabel = new System.Windows.Forms.Label();
            this.lblEssenceDecimals = new System.Windows.Forms.Label();
            this.nudNuyenDecimalsMinimum = new Chummer.NumericUpDownEx();
            this.nudNuyenDecimalsMaximum = new Chummer.NumericUpDownEx();
            this.nudEssenceDecimals = new Chummer.NumericUpDownEx();
            this.tabKarmaCosts = new System.Windows.Forms.TabPage();
            this.tlpKarmaCosts = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblKarmaSpecialization = new System.Windows.Forms.Label();
            this.nudKarmaSpecialization = new Chummer.NumericUpDownEx();
            this.lblKarmaKnowledgeSpecialization = new System.Windows.Forms.Label();
            this.nudKarmaKnowledgeSpecialization = new Chummer.NumericUpDownEx();
            this.lblKarmaNewKnowledgeSkill = new System.Windows.Forms.Label();
            this.nudKarmaNewKnowledgeSkill = new Chummer.NumericUpDownEx();
            this.lblKarmaLeaveGroup = new System.Windows.Forms.Label();
            this.nudKarmaLeaveGroup = new Chummer.NumericUpDownEx();
            this.lblKarmaNewActiveSkill = new System.Windows.Forms.Label();
            this.nudKarmaNewActiveSkill = new Chummer.NumericUpDownEx();
            this.lblKarmaNewSkillGroup = new System.Windows.Forms.Label();
            this.nudKarmaNewSkillGroup = new Chummer.NumericUpDownEx();
            this.lblKarmaImproveKnowledgeSkill = new System.Windows.Forms.Label();
            this.nudKarmaImproveKnowledgeSkill = new Chummer.NumericUpDownEx();
            this.lblKarmaImproveKnowledgeSkillExtra = new System.Windows.Forms.Label();
            this.lblKarmaImproveActiveSkill = new System.Windows.Forms.Label();
            this.nudKarmaImproveActiveSkill = new Chummer.NumericUpDownEx();
            this.lblKarmaImproveActiveSkillExtra = new System.Windows.Forms.Label();
            this.lblKarmaImproveSkillGroup = new System.Windows.Forms.Label();
            this.nudKarmaImproveSkillGroup = new Chummer.NumericUpDownEx();
            this.lblKarmaImproveSkillGroupExtra = new System.Windows.Forms.Label();
            this.lblKarmaAttribute = new System.Windows.Forms.Label();
            this.nudKarmaAttribute = new Chummer.NumericUpDownEx();
            this.lblKarmaAttributeExtra = new System.Windows.Forms.Label();
            this.lblKarmaAlchemicalFocus = new System.Windows.Forms.Label();
            this.nudKarmaAlchemicalFocus = new Chummer.NumericUpDownEx();
            this.lblKarmaAlchemicalFocusExtra = new System.Windows.Forms.Label();
            this.lblKarmaBanishingFocus = new System.Windows.Forms.Label();
            this.nudKarmaBanishingFocus = new Chummer.NumericUpDownEx();
            this.lblKarmaBindingFocus = new System.Windows.Forms.Label();
            this.nudKarmaBindingFocus = new Chummer.NumericUpDownEx();
            this.lblKarmaBanishingFocusExtra = new System.Windows.Forms.Label();
            this.lblKarmaCenteringFocus = new System.Windows.Forms.Label();
            this.nudKarmaCenteringFocus = new Chummer.NumericUpDownEx();
            this.lblKarmaBindingFocusExtra = new System.Windows.Forms.Label();
            this.lblKarmaCenteringFocusExtra = new System.Windows.Forms.Label();
            this.lblKarmaCounterspellingFocus = new System.Windows.Forms.Label();
            this.nudKarmaCounterspellingFocus = new Chummer.NumericUpDownEx();
            this.lblKarmaCounterspellingFocusExtra = new System.Windows.Forms.Label();
            this.lblKarmaDisenchantingFocus = new System.Windows.Forms.Label();
            this.nudKarmaDisenchantingFocus = new Chummer.NumericUpDownEx();
            this.lblKarmaDisenchantingFocusExtra = new System.Windows.Forms.Label();
            this.lblKarmaFlexibleSignatureFocus = new System.Windows.Forms.Label();
            this.nudKarmaFlexibleSignatureFocus = new Chummer.NumericUpDownEx();
            this.lblFlexibleSignatureFocusExtra = new System.Windows.Forms.Label();
            this.lblKarmaMaskingFocus = new System.Windows.Forms.Label();
            this.nudKarmaMaskingFocus = new Chummer.NumericUpDownEx();
            this.lblKarmaMaskingFocusExtra = new System.Windows.Forms.Label();
            this.lblKarmaPowerFocus = new System.Windows.Forms.Label();
            this.nudKarmaPowerFocus = new Chummer.NumericUpDownEx();
            this.lblKarmaPowerFocusExtra = new System.Windows.Forms.Label();
            this.lblKarmaQiFocus = new System.Windows.Forms.Label();
            this.nudKarmaQiFocus = new Chummer.NumericUpDownEx();
            this.lblKarmaQiFocusExtra = new System.Windows.Forms.Label();
            this.lblKarmaRitualSpellcastingFocus = new System.Windows.Forms.Label();
            this.nudKarmaRitualSpellcastingFocus = new Chummer.NumericUpDownEx();
            this.lblKarmaRitualSpellcastingFocusExtra = new System.Windows.Forms.Label();
            this.lblKarmaSpellcastingFocus = new System.Windows.Forms.Label();
            this.nudKarmaSpellcastingFocus = new Chummer.NumericUpDownEx();
            this.lblKarmaSpellcastingFocusExtra = new System.Windows.Forms.Label();
            this.lblKarmaSpell = new System.Windows.Forms.Label();
            this.nudKarmaSpell = new Chummer.NumericUpDownEx();
            this.lblKarmaContact = new System.Windows.Forms.Label();
            this.nudKarmaContact = new Chummer.NumericUpDownEx();
            this.lblKarmaContactExtra = new System.Windows.Forms.Label();
            this.lblKarmaEnemyExtra = new System.Windows.Forms.Label();
            this.nudKarmaEnemy = new Chummer.NumericUpDownEx();
            this.lblKarmaEnemy = new System.Windows.Forms.Label();
            this.lblKarmaSpirit = new System.Windows.Forms.Label();
            this.nudKarmaSpirit = new Chummer.NumericUpDownEx();
            this.lblKarmaSpiritExtra = new System.Windows.Forms.Label();
            this.lblKarmaNewComplexForm = new System.Windows.Forms.Label();
            this.nudKarmaNewComplexForm = new Chummer.NumericUpDownEx();
            this.lblKarmaMysticAdeptPowerPoint = new System.Windows.Forms.Label();
            this.nudKarmaMysticAdeptPowerPoint = new Chummer.NumericUpDownEx();
            this.lblKarmaNuyenPer = new System.Windows.Forms.Label();
            this.nudKarmaNuyenPer = new Chummer.NumericUpDownEx();
            this.lblKarmaNuyenPerExtra = new System.Windows.Forms.Label();
            this.lblKarmaJoinGroup = new System.Windows.Forms.Label();
            this.nudKarmaJoinGroup = new Chummer.NumericUpDownEx();
            this.lblKarmaNewAIProgram = new System.Windows.Forms.Label();
            this.nudKarmaNewAIProgram = new Chummer.NumericUpDownEx();
            this.lblKarmaNewAIAdvancedProgram = new System.Windows.Forms.Label();
            this.nudKarmaNewAIAdvancedProgram = new Chummer.NumericUpDownEx();
            this.lblKarmaMetamagic = new System.Windows.Forms.Label();
            this.nudKarmaMetamagic = new Chummer.NumericUpDownEx();
            this.lblKarmaTechnique = new System.Windows.Forms.Label();
            this.nudKarmaTechnique = new Chummer.NumericUpDownEx();
            this.flpKarmaInitiation = new System.Windows.Forms.FlowLayoutPanel();
            this.lblKarmaInitiation = new System.Windows.Forms.Label();
            this.lblKarmaInitiationBracket = new System.Windows.Forms.Label();
            this.nudKarmaInitiation = new Chummer.NumericUpDownEx();
            this.lblKarmaInitiationExtra = new System.Windows.Forms.Label();
            this.nudKarmaInitiationFlat = new Chummer.NumericUpDownEx();
            this.lblKarmaCarryover = new System.Windows.Forms.Label();
            this.nudKarmaCarryover = new Chummer.NumericUpDownEx();
            this.lblKarmaCarryoverExtra = new System.Windows.Forms.Label();
            this.lblKarmaQuality = new System.Windows.Forms.Label();
            this.nudKarmaQuality = new Chummer.NumericUpDownEx();
            this.lblKarmaQualityExtra = new System.Windows.Forms.Label();
            this.lblKarmaSummoningFocus = new System.Windows.Forms.Label();
            this.nudKarmaSummoningFocus = new Chummer.NumericUpDownEx();
            this.lblKarmaSpellShapingFocus = new System.Windows.Forms.Label();
            this.lblKarmaSpellShapingFocusExtra = new System.Windows.Forms.Label();
            this.nudKarmaSpellShapingFocus = new Chummer.NumericUpDownEx();
            this.lblKarmaSummoningFocusExtra = new System.Windows.Forms.Label();
            this.lblKarmaSustainingFocus = new System.Windows.Forms.Label();
            this.lblKarmaWeaponFocus = new System.Windows.Forms.Label();
            this.lblKarmaSustainingFocusExtra = new System.Windows.Forms.Label();
            this.lblKarmaWeaponFocusExtra = new System.Windows.Forms.Label();
            this.nudKarmaSustainingFocus = new Chummer.NumericUpDownEx();
            this.nudKarmaWeaponFocus = new Chummer.NumericUpDownEx();
            this.lblMetatypeCostsKarmaMultiplierLabel = new System.Windows.Forms.Label();
            this.nudMetatypeCostsKarmaMultiplier = new Chummer.NumericUpDownEx();
            this.tabCustomData = new System.Windows.Forms.TabPage();
            this.tlpOptionalRules = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblCustomDataDirectoriesLabel = new System.Windows.Forms.Label();
            this.treCustomDataDirectories = new System.Windows.Forms.TreeView();
            this.cmdIncreaseCustomDirectoryLoadOrder = new System.Windows.Forms.Button();
            this.cmdDecreaseCustomDirectoryLoadOrder = new System.Windows.Forms.Button();
            this.cmdGlobalOptionsCustomData = new System.Windows.Forms.Button();
            this.gbpDirectoryInfo = new System.Windows.Forms.GroupBox();
            this.tlpDirectoryInfo = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblDirectoryAuthors = new System.Windows.Forms.Label();
            this.lblDirectoryVersion = new System.Windows.Forms.Label();
            this.lblDirectoryName = new System.Windows.Forms.Label();
            this.lblDirectoryNameLabel = new System.Windows.Forms.Label();
            this.lblDirectoryVersionLabel = new System.Windows.Forms.Label();
            this.lblDirectoryAuthorsLabel = new System.Windows.Forms.Label();
            this.lblDirectoryDescriptionLabel = new System.Windows.Forms.Label();
            this.cmdManageDependencies = new System.Windows.Forms.Button();
            this.gbpDirectoryInfoExclusivities = new System.Windows.Forms.GroupBox();
            this.flpExclusivities = new System.Windows.Forms.FlowLayoutPanel();
            this.gbpDirectoryInfoDependencies = new System.Windows.Forms.GroupBox();
            this.flpDirectoryDependencies = new System.Windows.Forms.FlowLayoutPanel();
            this.tabHouseRules = new System.Windows.Forms.TabPage();
            this.flpHouseRules = new System.Windows.Forms.FlowLayoutPanel();
            this.gpbHouseRulesQualities = new System.Windows.Forms.GroupBox();
            this.tlpHouseRulesQualities = new Chummer.BufferedTableLayoutPanel(this.components);
            this.chkExceedNegativeQualities = new Chummer.ColorableCheckBox(this.components);
            this.chkDontDoubleQualityPurchases = new Chummer.ColorableCheckBox(this.components);
            this.chkExceedPositiveQualities = new Chummer.ColorableCheckBox(this.components);
            this.chkDontDoubleQualityRefunds = new Chummer.ColorableCheckBox(this.components);
            this.chkExceedPositiveQualitiesCostDoubled = new Chummer.ColorableCheckBox(this.components);
            this.chkExceedNegativeQualitiesLimit = new Chummer.ColorableCheckBox(this.components);
            this.gpbHouseRulesAttributes = new System.Windows.Forms.GroupBox();
            this.bufferedTableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.chkESSLossReducesMaximumOnly = new Chummer.ColorableCheckBox(this.components);
            this.chkAllowCyberwareESSDiscounts = new Chummer.ColorableCheckBox(this.components);
            this.chkAlternateMetatypeAttributeKarma = new Chummer.ColorableCheckBox(this.components);
            this.chkReverseAttributePriorityOrder = new Chummer.ColorableCheckBox(this.components);
            this.flpDroneArmorMultiplier = new System.Windows.Forms.FlowLayoutPanel();
            this.chkDroneArmorMultiplier = new Chummer.ColorableCheckBox(this.components);
            this.lblDroneArmorMultiplierTimes = new System.Windows.Forms.Label();
            this.nudDroneArmorMultiplier = new Chummer.NumericUpDownEx();
            this.chkUseCalculatedPublicAwareness = new Chummer.ColorableCheckBox(this.components);
            this.chkUnclampAttributeMinimum = new Chummer.ColorableCheckBox(this.components);
            this.gpbHouseRulesSkills = new System.Windows.Forms.GroupBox();
            this.tlpHouseRulesSkills = new Chummer.BufferedTableLayoutPanel(this.components);
            this.chkCompensateSkillGroupKarmaDifference = new Chummer.ColorableCheckBox(this.components);
            this.chkAllowSkillRegrouping = new Chummer.ColorableCheckBox(this.components);
            this.chkFreeMartialArtSpecialization = new Chummer.ColorableCheckBox(this.components);
            this.chkUsePointsOnBrokenGroups = new Chummer.ColorableCheckBox(this.components);
            this.gpbHouseRulesCombat = new System.Windows.Forms.GroupBox();
            this.bufferedTableLayoutPanel2 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.chkNoArmorEncumbrance = new Chummer.ColorableCheckBox(this.components);
            this.chkMoreLethalGameplay = new Chummer.ColorableCheckBox(this.components);
            this.chkUnarmedSkillImprovements = new Chummer.ColorableCheckBox(this.components);
            this.gpbHouseRulesMagicResonance = new System.Windows.Forms.GroupBox();
            this.tlpHouseRulesMagicResonance = new Chummer.BufferedTableLayoutPanel(this.components);
            this.chkMysAdPp = new Chummer.ColorableCheckBox(this.components);
            this.chkPrioritySpellsAsAdeptPowers = new Chummer.ColorableCheckBox(this.components);
            this.chkExtendAnyDetectionSpell = new Chummer.ColorableCheckBox(this.components);
            this.chkIncreasedImprovedAbilityModifier = new Chummer.ColorableCheckBox(this.components);
            this.chkIgnoreComplexFormLimit = new Chummer.ColorableCheckBox(this.components);
            this.chkSpecialKarmaCost = new Chummer.ColorableCheckBox(this.components);
            this.chkAllowTechnomancerSchooling = new Chummer.ColorableCheckBox(this.components);
            this.chkAllowInitiation = new Chummer.ColorableCheckBox(this.components);
            this.chkMysAdeptSecondMAGAttribute = new Chummer.ColorableCheckBox(this.components);
            this.chkIgnoreArt = new Chummer.ColorableCheckBox(this.components);
            this.lblSettingName = new System.Windows.Forms.Label();
            this.tlpButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdRename = new System.Windows.Forms.Button();
            this.cmdDelete = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdRestoreDefaults = new System.Windows.Forms.Button();
            this.cmdSave = new System.Windows.Forms.Button();
            this.cmdSaveAs = new System.Windows.Forms.Button();
            this.tboxDirectoryDescription = new System.Windows.Forms.TextBox();
            this.tlpOptions.SuspendLayout();
            this.tabOptions.SuspendLayout();
            this.tabBasicOptions.SuspendLayout();
            this.tlpBasicOptions.SuspendLayout();
            this.gpbSourcebook.SuspendLayout();
            this.flpBasicOptions.SuspendLayout();
            this.gpbBasicOptionsCreateSettings.SuspendLayout();
            this.tlpBasicOptionsCreateSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxAvail)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxNuyenKarma)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudQualityKarmaLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudStartingKarma)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSumToTen)).BeginInit();
            this.gpbBasicOptionsOfficialRules.SuspendLayout();
            this.tlpBasicOptionsOfficialRules.SuspendLayout();
            this.gpbBasicOptionsCyberlimbs.SuspendLayout();
            this.tlpBasicOptionsCyberlimbs.SuspendLayout();
            this.tlpCyberlimbAttributeBonusCap.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCyberlimbAttributeBonusCap)).BeginInit();
            this.flpCyberlimbAttributeBonusCap.SuspendLayout();
            this.flpRedlinerLimbs.SuspendLayout();
            this.gpbBasicOptionsRounding.SuspendLayout();
            this.tlpBasicOptionsRounding.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudNuyenDecimalsMinimum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNuyenDecimalsMaximum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEssenceDecimals)).BeginInit();
            this.tabKarmaCosts.SuspendLayout();
            this.tlpKarmaCosts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpecialization)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaKnowledgeSpecialization)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewKnowledgeSkill)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaLeaveGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewActiveSkill)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewSkillGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaImproveKnowledgeSkill)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaImproveActiveSkill)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaImproveSkillGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaAttribute)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaAlchemicalFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaBanishingFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaBindingFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaCenteringFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaCounterspellingFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaDisenchantingFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaFlexibleSignatureFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaMaskingFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaPowerFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaQiFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaRitualSpellcastingFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpellcastingFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpell)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaContact)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaEnemy)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpirit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewComplexForm)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaMysticAdeptPowerPoint)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNuyenPer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaJoinGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewAIProgram)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewAIAdvancedProgram)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaMetamagic)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaTechnique)).BeginInit();
            this.flpKarmaInitiation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaInitiation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaInitiationFlat)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaCarryover)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaQuality)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSummoningFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpellShapingFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSustainingFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaWeaponFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMetatypeCostsKarmaMultiplier)).BeginInit();
            this.tabCustomData.SuspendLayout();
            this.tlpOptionalRules.SuspendLayout();
            this.gbpDirectoryInfo.SuspendLayout();
            this.tlpDirectoryInfo.SuspendLayout();
            this.gbpDirectoryInfoExclusivities.SuspendLayout();
            this.gbpDirectoryInfoDependencies.SuspendLayout();
            this.tabHouseRules.SuspendLayout();
            this.flpHouseRules.SuspendLayout();
            this.gpbHouseRulesQualities.SuspendLayout();
            this.tlpHouseRulesQualities.SuspendLayout();
            this.gpbHouseRulesAttributes.SuspendLayout();
            this.bufferedTableLayoutPanel1.SuspendLayout();
            this.flpDroneArmorMultiplier.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDroneArmorMultiplier)).BeginInit();
            this.gpbHouseRulesSkills.SuspendLayout();
            this.tlpHouseRulesSkills.SuspendLayout();
            this.gpbHouseRulesCombat.SuspendLayout();
            this.bufferedTableLayoutPanel2.SuspendLayout();
            this.gpbHouseRulesMagicResonance.SuspendLayout();
            this.tlpHouseRulesMagicResonance.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpOptions
            // 
            this.tlpOptions.AutoSize = true;
            this.tlpOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpOptions.ColumnCount = 3;
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpOptions.Controls.Add(this.cboSetting, 1, 1);
            this.tlpOptions.Controls.Add(this.tabOptions, 0, 0);
            this.tlpOptions.Controls.Add(this.lblSettingName, 0, 1);
            this.tlpOptions.Controls.Add(this.tlpButtons, 2, 1);
            this.tlpOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpOptions.Location = new System.Drawing.Point(9, 9);
            this.tlpOptions.Name = "tlpOptions";
            this.tlpOptions.RowCount = 2;
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.Size = new System.Drawing.Size(1246, 663);
            this.tlpOptions.TabIndex = 7;
            // 
            // cboSetting
            // 
            this.cboSetting.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSetting.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSetting.FormattingEnabled = true;
            this.cboSetting.Location = new System.Drawing.Point(83, 638);
            this.cboSetting.Name = "cboSetting";
            this.cboSetting.Size = new System.Drawing.Size(560, 21);
            this.cboSetting.TabIndex = 1;
            this.cboSetting.TooltipText = "";
            this.cboSetting.SelectedIndexChanged += new System.EventHandler(this.cboSetting_SelectedIndexChanged);
            // 
            // tabOptions
            // 
            this.tlpOptions.SetColumnSpan(this.tabOptions, 3);
            this.tabOptions.Controls.Add(this.tabBasicOptions);
            this.tabOptions.Controls.Add(this.tabKarmaCosts);
            this.tabOptions.Controls.Add(this.tabCustomData);
            this.tabOptions.Controls.Add(this.tabHouseRules);
            this.tabOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabOptions.Location = new System.Drawing.Point(3, 3);
            this.tabOptions.Name = "tabOptions";
            this.tabOptions.SelectedIndex = 0;
            this.tabOptions.Size = new System.Drawing.Size(1240, 628);
            this.tabOptions.TabIndex = 4;
            // 
            // tabBasicOptions
            // 
            this.tabBasicOptions.BackColor = System.Drawing.SystemColors.Control;
            this.tabBasicOptions.Controls.Add(this.tlpBasicOptions);
            this.tabBasicOptions.Location = new System.Drawing.Point(4, 22);
            this.tabBasicOptions.Name = "tabBasicOptions";
            this.tabBasicOptions.Padding = new System.Windows.Forms.Padding(9);
            this.tabBasicOptions.Size = new System.Drawing.Size(1232, 602);
            this.tabBasicOptions.TabIndex = 0;
            this.tabBasicOptions.Tag = "Tab_Options_Basic";
            this.tabBasicOptions.Text = "Basic Options";
            // 
            // tlpBasicOptions
            // 
            this.tlpBasicOptions.AutoSize = true;
            this.tlpBasicOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpBasicOptions.ColumnCount = 2;
            this.tlpBasicOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpBasicOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.tlpBasicOptions.Controls.Add(this.cmdEnableSourcebooks, 0, 1);
            this.tlpBasicOptions.Controls.Add(this.gpbSourcebook, 0, 0);
            this.tlpBasicOptions.Controls.Add(this.flpBasicOptions, 1, 0);
            this.tlpBasicOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBasicOptions.Location = new System.Drawing.Point(9, 9);
            this.tlpBasicOptions.Name = "tlpBasicOptions";
            this.tlpBasicOptions.RowCount = 2;
            this.tlpBasicOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBasicOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptions.Size = new System.Drawing.Size(1214, 584);
            this.tlpBasicOptions.TabIndex = 40;
            // 
            // cmdEnableSourcebooks
            // 
            this.cmdEnableSourcebooks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdEnableSourcebooks.Location = new System.Drawing.Point(3, 558);
            this.cmdEnableSourcebooks.Name = "cmdEnableSourcebooks";
            this.cmdEnableSourcebooks.Size = new System.Drawing.Size(297, 23);
            this.cmdEnableSourcebooks.TabIndex = 6;
            this.cmdEnableSourcebooks.Tag = "Button_ToggleSourcebooks";
            this.cmdEnableSourcebooks.Text = "Toggle all Sourcebooks On/Off";
            this.cmdEnableSourcebooks.UseVisualStyleBackColor = true;
            this.cmdEnableSourcebooks.Click += new System.EventHandler(this.cmdEnableSourcebooks_Click);
            // 
            // gpbSourcebook
            // 
            this.gpbSourcebook.AutoSize = true;
            this.gpbSourcebook.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbSourcebook.Controls.Add(this.treSourcebook);
            this.gpbSourcebook.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpbSourcebook.Location = new System.Drawing.Point(3, 3);
            this.gpbSourcebook.Name = "gpbSourcebook";
            this.gpbSourcebook.Size = new System.Drawing.Size(297, 549);
            this.gpbSourcebook.TabIndex = 38;
            this.gpbSourcebook.TabStop = false;
            this.gpbSourcebook.Tag = "Label_Options_SourcebooksToUse";
            this.gpbSourcebook.Text = "Sourcebooks to Use";
            // 
            // treSourcebook
            // 
            this.treSourcebook.CheckBoxes = true;
            this.treSourcebook.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treSourcebook.Location = new System.Drawing.Point(3, 16);
            this.treSourcebook.Name = "treSourcebook";
            this.treSourcebook.ShowLines = false;
            this.treSourcebook.ShowPlusMinus = false;
            this.treSourcebook.ShowRootLines = false;
            this.treSourcebook.Size = new System.Drawing.Size(291, 530);
            this.treSourcebook.TabIndex = 1;
            this.treSourcebook.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treSourcebook_AfterCheck);
            // 
            // flpBasicOptions
            // 
            this.flpBasicOptions.AutoScroll = true;
            this.flpBasicOptions.Controls.Add(this.gpbBasicOptionsCreateSettings);
            this.flpBasicOptions.Controls.Add(this.gpbBasicOptionsOfficialRules);
            this.flpBasicOptions.Controls.Add(this.gpbBasicOptionsCyberlimbs);
            this.flpBasicOptions.Controls.Add(this.gpbBasicOptionsRounding);
            this.flpBasicOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpBasicOptions.Location = new System.Drawing.Point(303, 0);
            this.flpBasicOptions.Margin = new System.Windows.Forms.Padding(0);
            this.flpBasicOptions.Name = "flpBasicOptions";
            this.tlpBasicOptions.SetRowSpan(this.flpBasicOptions, 2);
            this.flpBasicOptions.Size = new System.Drawing.Size(911, 584);
            this.flpBasicOptions.TabIndex = 39;
            // 
            // gpbBasicOptionsCreateSettings
            // 
            this.gpbBasicOptionsCreateSettings.AutoSize = true;
            this.gpbBasicOptionsCreateSettings.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbBasicOptionsCreateSettings.Controls.Add(this.tlpBasicOptionsCreateSettings);
            this.gpbBasicOptionsCreateSettings.Location = new System.Drawing.Point(3, 3);
            this.gpbBasicOptionsCreateSettings.Name = "gpbBasicOptionsCreateSettings";
            this.gpbBasicOptionsCreateSettings.Size = new System.Drawing.Size(468, 202);
            this.gpbBasicOptionsCreateSettings.TabIndex = 5;
            this.gpbBasicOptionsCreateSettings.TabStop = false;
            this.gpbBasicOptionsCreateSettings.Tag = "Label_CharacterOptions_CharacterCreationSettings";
            this.gpbBasicOptionsCreateSettings.Text = "Character Creation Settings";
            // 
            // tlpBasicOptionsCreateSettings
            // 
            this.tlpBasicOptionsCreateSettings.AutoSize = true;
            this.tlpBasicOptionsCreateSettings.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpBasicOptionsCreateSettings.ColumnCount = 4;
            this.tlpBasicOptionsCreateSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBasicOptionsCreateSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBasicOptionsCreateSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBasicOptionsCreateSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblAllowedCyberwareGrades, 2, 4);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.nudMaxAvail, 3, 0);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.cboPriorityTable, 1, 1);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblPriorityTable, 0, 1);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblBuildMethod, 0, 0);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.cboBuildMethod, 1, 0);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblMaxNuyenKarma, 2, 3);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblMaxAvail, 2, 0);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.nudMaxNuyenKarma, 3, 3);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.flpAllowedCyberwareGrades, 2, 5);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblKnowledgePoints, 0, 6);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblContactPoints, 0, 5);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.txtKnowledgePoints, 1, 6);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblQualityKarmaLimit, 0, 4);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.txtContactPoints, 1, 5);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.nudQualityKarmaLimit, 1, 4);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblStartingKarma, 0, 3);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.nudStartingKarma, 1, 3);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblPriorities, 2, 1);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.txtPriorities, 3, 1);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.nudSumToTen, 3, 2);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblSumToTen, 2, 2);
            this.tlpBasicOptionsCreateSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBasicOptionsCreateSettings.Location = new System.Drawing.Point(3, 16);
            this.tlpBasicOptionsCreateSettings.Name = "tlpBasicOptionsCreateSettings";
            this.tlpBasicOptionsCreateSettings.RowCount = 8;
            this.tlpBasicOptionsCreateSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsCreateSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsCreateSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsCreateSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsCreateSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsCreateSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsCreateSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsCreateSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBasicOptionsCreateSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBasicOptionsCreateSettings.Size = new System.Drawing.Size(462, 183);
            this.tlpBasicOptionsCreateSettings.TabIndex = 0;
            // 
            // lblAllowedCyberwareGrades
            // 
            this.lblAllowedCyberwareGrades.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblAllowedCyberwareGrades.AutoSize = true;
            this.tlpBasicOptionsCreateSettings.SetColumnSpan(this.lblAllowedCyberwareGrades, 2);
            this.lblAllowedCyberwareGrades.Location = new System.Drawing.Point(291, 112);
            this.lblAllowedCyberwareGrades.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAllowedCyberwareGrades.Name = "lblAllowedCyberwareGrades";
            this.lblAllowedCyberwareGrades.Size = new System.Drawing.Size(134, 13);
            this.lblAllowedCyberwareGrades.TabIndex = 28;
            this.lblAllowedCyberwareGrades.Tag = "Label_CharacterOptions_AllowedCyberwareGrades";
            this.lblAllowedCyberwareGrades.Text = "Allowed Cyberware Grades";
            this.lblAllowedCyberwareGrades.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudMaxAvail
            // 
            this.nudMaxAvail.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudMaxAvail.AutoSize = true;
            this.nudMaxAvail.Location = new System.Drawing.Point(400, 3);
            this.nudMaxAvail.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudMaxAvail.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMaxAvail.Name = "nudMaxAvail";
            this.nudMaxAvail.Size = new System.Drawing.Size(41, 20);
            this.nudMaxAvail.TabIndex = 26;
            // 
            // cboPriorityTable
            // 
            this.cboPriorityTable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboPriorityTable.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPriorityTable.FormattingEnabled = true;
            this.cboPriorityTable.Location = new System.Drawing.Point(105, 42);
            this.cboPriorityTable.Name = "cboPriorityTable";
            this.tlpBasicOptionsCreateSettings.SetRowSpan(this.cboPriorityTable, 2);
            this.cboPriorityTable.Size = new System.Drawing.Size(180, 21);
            this.cboPriorityTable.TabIndex = 18;
            this.cboPriorityTable.TooltipText = "";
            this.cboPriorityTable.SelectedIndexChanged += new System.EventHandler(this.cboPriorityTable_SelectedIndexChanged);
            // 
            // lblPriorityTable
            // 
            this.lblPriorityTable.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblPriorityTable.AutoSize = true;
            this.lblPriorityTable.Location = new System.Drawing.Point(31, 46);
            this.lblPriorityTable.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPriorityTable.Name = "lblPriorityTable";
            this.tlpBasicOptionsCreateSettings.SetRowSpan(this.lblPriorityTable, 2);
            this.lblPriorityTable.Size = new System.Drawing.Size(68, 13);
            this.lblPriorityTable.TabIndex = 17;
            this.lblPriorityTable.Tag = "Label_CharacterOptions_PriorityTable";
            this.lblPriorityTable.Text = "Priority Table";
            this.lblPriorityTable.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblBuildMethod
            // 
            this.lblBuildMethod.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblBuildMethod.AutoSize = true;
            this.lblBuildMethod.Location = new System.Drawing.Point(30, 7);
            this.lblBuildMethod.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildMethod.Name = "lblBuildMethod";
            this.lblBuildMethod.Size = new System.Drawing.Size(69, 13);
            this.lblBuildMethod.TabIndex = 0;
            this.lblBuildMethod.Tag = "Label_SelectBP_BuildMethod";
            this.lblBuildMethod.Text = "Build Method";
            this.lblBuildMethod.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboBuildMethod
            // 
            this.cboBuildMethod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboBuildMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBuildMethod.FormattingEnabled = true;
            this.cboBuildMethod.Location = new System.Drawing.Point(105, 3);
            this.cboBuildMethod.Name = "cboBuildMethod";
            this.cboBuildMethod.Size = new System.Drawing.Size(180, 21);
            this.cboBuildMethod.TabIndex = 1;
            this.cboBuildMethod.TooltipText = "";
            // 
            // lblMaxNuyenKarma
            // 
            this.lblMaxNuyenKarma.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMaxNuyenKarma.AutoSize = true;
            this.lblMaxNuyenKarma.Location = new System.Drawing.Point(300, 85);
            this.lblMaxNuyenKarma.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMaxNuyenKarma.Name = "lblMaxNuyenKarma";
            this.lblMaxNuyenKarma.Size = new System.Drawing.Size(94, 13);
            this.lblMaxNuyenKarma.TabIndex = 16;
            this.lblMaxNuyenKarma.Tag = "Label_SelectBP_MaxNuyen";
            this.lblMaxNuyenKarma.Text = "Nuyen Karma Max";
            this.lblMaxNuyenKarma.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblMaxAvail
            // 
            this.lblMaxAvail.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMaxAvail.AutoSize = true;
            this.lblMaxAvail.Location = new System.Drawing.Point(291, 7);
            this.lblMaxAvail.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMaxAvail.Name = "lblMaxAvail";
            this.lblMaxAvail.Size = new System.Drawing.Size(103, 13);
            this.lblMaxAvail.TabIndex = 19;
            this.lblMaxAvail.Tag = "Label_SelectBP_MaxAvail";
            this.lblMaxAvail.Text = "Maximum Availability";
            this.lblMaxAvail.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudMaxNuyenKarma
            // 
            this.nudMaxNuyenKarma.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudMaxNuyenKarma.AutoSize = true;
            this.nudMaxNuyenKarma.Location = new System.Drawing.Point(400, 82);
            this.nudMaxNuyenKarma.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.nudMaxNuyenKarma.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMaxNuyenKarma.Name = "nudMaxNuyenKarma";
            this.nudMaxNuyenKarma.Size = new System.Drawing.Size(59, 20);
            this.nudMaxNuyenKarma.TabIndex = 25;
            // 
            // flpAllowedCyberwareGrades
            // 
            this.flpAllowedCyberwareGrades.AutoSize = true;
            this.flpAllowedCyberwareGrades.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpBasicOptionsCreateSettings.SetColumnSpan(this.flpAllowedCyberwareGrades, 2);
            this.flpAllowedCyberwareGrades.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpAllowedCyberwareGrades.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpAllowedCyberwareGrades.Location = new System.Drawing.Point(288, 131);
            this.flpAllowedCyberwareGrades.Margin = new System.Windows.Forms.Padding(0);
            this.flpAllowedCyberwareGrades.Name = "flpAllowedCyberwareGrades";
            this.tlpBasicOptionsCreateSettings.SetRowSpan(this.flpAllowedCyberwareGrades, 3);
            this.flpAllowedCyberwareGrades.Size = new System.Drawing.Size(174, 52);
            this.flpAllowedCyberwareGrades.TabIndex = 29;
            this.flpAllowedCyberwareGrades.WrapContents = false;
            // 
            // lblKnowledgePoints
            // 
            this.lblKnowledgePoints.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKnowledgePoints.AutoSize = true;
            this.lblKnowledgePoints.Location = new System.Drawing.Point(7, 163);
            this.lblKnowledgePoints.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKnowledgePoints.Name = "lblKnowledgePoints";
            this.lblKnowledgePoints.Size = new System.Drawing.Size(92, 13);
            this.lblKnowledgePoints.TabIndex = 1;
            this.lblKnowledgePoints.Tag = "String_KnowledgePoints";
            this.lblKnowledgePoints.Text = "Knowledge Points";
            this.lblKnowledgePoints.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblKnowledgePoints.ToolTipText = "";
            // 
            // lblContactPoints
            // 
            this.lblContactPoints.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblContactPoints.AutoSize = true;
            this.lblContactPoints.Location = new System.Drawing.Point(23, 137);
            this.lblContactPoints.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblContactPoints.Name = "lblContactPoints";
            this.lblContactPoints.Size = new System.Drawing.Size(76, 13);
            this.lblContactPoints.TabIndex = 0;
            this.lblContactPoints.Tag = "String_ContactPoints";
            this.lblContactPoints.Text = "Contact Points";
            this.lblContactPoints.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblContactPoints.ToolTipText = "";
            // 
            // txtKnowledgePoints
            // 
            this.txtKnowledgePoints.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtKnowledgePoints.Location = new System.Drawing.Point(105, 160);
            this.txtKnowledgePoints.Name = "txtKnowledgePoints";
            this.txtKnowledgePoints.Size = new System.Drawing.Size(180, 20);
            this.txtKnowledgePoints.TabIndex = 3;
            this.txtKnowledgePoints.Text = "({INTUnaug} + {LOGUnaug}) * 2";
            this.txtKnowledgePoints.TextChanged += new System.EventHandler(this.txtKnowledgePoints_TextChanged);
            // 
            // lblQualityKarmaLimit
            // 
            this.lblQualityKarmaLimit.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblQualityKarmaLimit.AutoSize = true;
            this.lblQualityKarmaLimit.Location = new System.Drawing.Point(3, 111);
            this.lblQualityKarmaLimit.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblQualityKarmaLimit.Name = "lblQualityKarmaLimit";
            this.lblQualityKarmaLimit.Size = new System.Drawing.Size(96, 13);
            this.lblQualityKarmaLimit.TabIndex = 30;
            this.lblQualityKarmaLimit.Tag = "Label_SelectBP_QualityKarmaLimit";
            this.lblQualityKarmaLimit.Text = "Quality Karma Limit";
            this.lblQualityKarmaLimit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtContactPoints
            // 
            this.txtContactPoints.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtContactPoints.Location = new System.Drawing.Point(105, 134);
            this.txtContactPoints.Name = "txtContactPoints";
            this.txtContactPoints.Size = new System.Drawing.Size(180, 20);
            this.txtContactPoints.TabIndex = 2;
            this.txtContactPoints.Text = "{CHAUnaug} * 3";
            this.txtContactPoints.TextChanged += new System.EventHandler(this.txtContactPoints_TextChanged);
            // 
            // nudQualityKarmaLimit
            // 
            this.nudQualityKarmaLimit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudQualityKarmaLimit.AutoSize = true;
            this.nudQualityKarmaLimit.Location = new System.Drawing.Point(105, 108);
            this.nudQualityKarmaLimit.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.nudQualityKarmaLimit.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudQualityKarmaLimit.Name = "nudQualityKarmaLimit";
            this.nudQualityKarmaLimit.Size = new System.Drawing.Size(59, 20);
            this.nudQualityKarmaLimit.TabIndex = 31;
            // 
            // lblStartingKarma
            // 
            this.lblStartingKarma.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblStartingKarma.AutoSize = true;
            this.lblStartingKarma.Location = new System.Drawing.Point(23, 85);
            this.lblStartingKarma.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblStartingKarma.Name = "lblStartingKarma";
            this.lblStartingKarma.Size = new System.Drawing.Size(76, 13);
            this.lblStartingKarma.TabIndex = 12;
            this.lblStartingKarma.Tag = "Label_SelectBP_StartingKarma";
            this.lblStartingKarma.Text = "Starting Karma";
            this.lblStartingKarma.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudStartingKarma
            // 
            this.nudStartingKarma.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudStartingKarma.AutoSize = true;
            this.nudStartingKarma.Location = new System.Drawing.Point(105, 82);
            this.nudStartingKarma.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.nudStartingKarma.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudStartingKarma.Name = "nudStartingKarma";
            this.nudStartingKarma.Size = new System.Drawing.Size(59, 20);
            this.nudStartingKarma.TabIndex = 24;
            // 
            // lblPriorities
            // 
            this.lblPriorities.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblPriorities.AutoSize = true;
            this.lblPriorities.Location = new System.Drawing.Point(348, 33);
            this.lblPriorities.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPriorities.Name = "lblPriorities";
            this.lblPriorities.Size = new System.Drawing.Size(46, 13);
            this.lblPriorities.TabIndex = 2;
            this.lblPriorities.Tag = "Label_SelectBP_Priorities";
            this.lblPriorities.Text = "Priorities";
            this.lblPriorities.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblPriorities.ToolTipText = "";
            // 
            // txtPriorities
            // 
            this.txtPriorities.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPriorities.Location = new System.Drawing.Point(400, 30);
            this.txtPriorities.MaxLength = 5;
            this.txtPriorities.Name = "txtPriorities";
            this.txtPriorities.Size = new System.Drawing.Size(59, 20);
            this.txtPriorities.TabIndex = 3;
            this.txtPriorities.TextChanged += new System.EventHandler(this.txtPriorities_TextChanged);
            this.txtPriorities.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPriorities_KeyPress);
            // 
            // nudSumToTen
            // 
            this.nudSumToTen.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudSumToTen.AutoSize = true;
            this.nudSumToTen.Location = new System.Drawing.Point(400, 56);
            this.nudSumToTen.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudSumToTen.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudSumToTen.Name = "nudSumToTen";
            this.nudSumToTen.Size = new System.Drawing.Size(41, 20);
            this.nudSumToTen.TabIndex = 5;
            // 
            // lblSumToTen
            // 
            this.lblSumToTen.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSumToTen.AutoSize = true;
            this.lblSumToTen.Location = new System.Drawing.Point(332, 59);
            this.lblSumToTen.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSumToTen.Name = "lblSumToTen";
            this.lblSumToTen.Size = new System.Drawing.Size(62, 13);
            this.lblSumToTen.TabIndex = 4;
            this.lblSumToTen.Tag = "Label_SelectBP_SumToX";
            this.lblSumToTen.Text = "Sum to Ten";
            this.lblSumToTen.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // gpbBasicOptionsOfficialRules
            // 
            this.gpbBasicOptionsOfficialRules.AutoSize = true;
            this.gpbBasicOptionsOfficialRules.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbBasicOptionsOfficialRules.Controls.Add(this.tlpBasicOptionsOfficialRules);
            this.gpbBasicOptionsOfficialRules.Location = new System.Drawing.Point(3, 211);
            this.gpbBasicOptionsOfficialRules.Name = "gpbBasicOptionsOfficialRules";
            this.gpbBasicOptionsOfficialRules.Size = new System.Drawing.Size(472, 228);
            this.gpbBasicOptionsOfficialRules.TabIndex = 3;
            this.gpbBasicOptionsOfficialRules.TabStop = false;
            this.gpbBasicOptionsOfficialRules.Tag = "Label_CharacterOptions_OptionsOfficialRules";
            this.gpbBasicOptionsOfficialRules.Text = "Options for Official Rules";
            // 
            // tlpBasicOptionsOfficialRules
            // 
            this.tlpBasicOptionsOfficialRules.AutoSize = true;
            this.tlpBasicOptionsOfficialRules.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpBasicOptionsOfficialRules.ColumnCount = 1;
            this.tlpBasicOptionsOfficialRules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBasicOptionsOfficialRules.Controls.Add(this.chkEnemyKarmaQualityLimit, 0, 8);
            this.tlpBasicOptionsOfficialRules.Controls.Add(this.chkAllowFreeGrids, 0, 7);
            this.tlpBasicOptionsOfficialRules.Controls.Add(this.chkAllowPointBuySpecializationsOnKarmaSkills, 0, 6);
            this.tlpBasicOptionsOfficialRules.Controls.Add(this.chkStrictSkillGroups, 0, 5);
            this.tlpBasicOptionsOfficialRules.Controls.Add(this.chkEnforceCapacity, 0, 0);
            this.tlpBasicOptionsOfficialRules.Controls.Add(this.chkLicenseEachRestrictedItem, 0, 1);
            this.tlpBasicOptionsOfficialRules.Controls.Add(this.chkRestrictRecoil, 0, 4);
            this.tlpBasicOptionsOfficialRules.Controls.Add(this.chkDronemodsMaximumPilot, 0, 3);
            this.tlpBasicOptionsOfficialRules.Controls.Add(this.chkDronemods, 0, 2);
            this.tlpBasicOptionsOfficialRules.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBasicOptionsOfficialRules.Location = new System.Drawing.Point(3, 16);
            this.tlpBasicOptionsOfficialRules.Name = "tlpBasicOptionsOfficialRules";
            this.tlpBasicOptionsOfficialRules.RowCount = 9;
            this.tlpBasicOptionsOfficialRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsOfficialRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsOfficialRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsOfficialRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsOfficialRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsOfficialRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsOfficialRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsOfficialRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsOfficialRules.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBasicOptionsOfficialRules.Size = new System.Drawing.Size(466, 209);
            this.tlpBasicOptionsOfficialRules.TabIndex = 0;
            // 
            // chkEnemyKarmaQualityLimit
            // 
            this.chkEnemyKarmaQualityLimit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkEnemyKarmaQualityLimit.AutoSize = true;
            this.chkEnemyKarmaQualityLimit.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.tlpBasicOptionsOfficialRules.SetColumnSpan(this.chkEnemyKarmaQualityLimit, 4);
            this.chkEnemyKarmaQualityLimit.DefaultColorScheme = true;
            this.chkEnemyKarmaQualityLimit.Location = new System.Drawing.Point(3, 189);
            this.chkEnemyKarmaQualityLimit.Name = "chkEnemyKarmaQualityLimit";
            this.chkEnemyKarmaQualityLimit.Size = new System.Drawing.Size(389, 17);
            this.chkEnemyKarmaQualityLimit.TabIndex = 47;
            this.chkEnemyKarmaQualityLimit.Tag = "Checkbox_Options_EnemyKarmaQualityLimit";
            this.chkEnemyKarmaQualityLimit.Text = "Karma spent on enemies counts towards negative Quality limit in create mode";
            this.chkEnemyKarmaQualityLimit.UseVisualStyleBackColor = true;
            // 
            // chkAllowFreeGrids
            // 
            this.chkAllowFreeGrids.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkAllowFreeGrids.AutoSize = true;
            this.tlpBasicOptionsOfficialRules.SetColumnSpan(this.chkAllowFreeGrids, 5);
            this.chkAllowFreeGrids.DefaultColorScheme = true;
            this.chkAllowFreeGrids.Location = new System.Drawing.Point(3, 166);
            this.chkAllowFreeGrids.Name = "chkAllowFreeGrids";
            this.chkAllowFreeGrids.Size = new System.Drawing.Size(460, 17);
            this.chkAllowFreeGrids.TabIndex = 46;
            this.chkAllowFreeGrids.Tag = "Checkbox_Options_AllowFreeGrids";
            this.chkAllowFreeGrids.Text = "Allow Free Grid Subscription Qualities for lifestyles even if Hard Targets is not" +
    " an active book";
            this.chkAllowFreeGrids.UseVisualStyleBackColor = true;
            // 
            // chkAllowPointBuySpecializationsOnKarmaSkills
            // 
            this.chkAllowPointBuySpecializationsOnKarmaSkills.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkAllowPointBuySpecializationsOnKarmaSkills.AutoSize = true;
            this.tlpBasicOptionsOfficialRules.SetColumnSpan(this.chkAllowPointBuySpecializationsOnKarmaSkills, 4);
            this.chkAllowPointBuySpecializationsOnKarmaSkills.DefaultColorScheme = true;
            this.chkAllowPointBuySpecializationsOnKarmaSkills.Location = new System.Drawing.Point(3, 143);
            this.chkAllowPointBuySpecializationsOnKarmaSkills.Name = "chkAllowPointBuySpecializationsOnKarmaSkills";
            this.chkAllowPointBuySpecializationsOnKarmaSkills.Size = new System.Drawing.Size(366, 17);
            this.chkAllowPointBuySpecializationsOnKarmaSkills.TabIndex = 39;
            this.chkAllowPointBuySpecializationsOnKarmaSkills.Tag = "Checkbox_Options_AllowPointBuySpecializationsOnKarmaSkills";
            this.chkAllowPointBuySpecializationsOnKarmaSkills.Text = "Allow skill points to be used to buy specializations for karma-bought skills";
            this.chkAllowPointBuySpecializationsOnKarmaSkills.UseVisualStyleBackColor = true;
            // 
            // chkStrictSkillGroups
            // 
            this.chkStrictSkillGroups.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkStrictSkillGroups.AutoSize = true;
            this.tlpBasicOptionsOfficialRules.SetColumnSpan(this.chkStrictSkillGroups, 4);
            this.chkStrictSkillGroups.DefaultColorScheme = true;
            this.chkStrictSkillGroups.Location = new System.Drawing.Point(3, 120);
            this.chkStrictSkillGroups.Name = "chkStrictSkillGroups";
            this.chkStrictSkillGroups.Size = new System.Drawing.Size(304, 17);
            this.chkStrictSkillGroups.TabIndex = 38;
            this.chkStrictSkillGroups.Tag = "Checkbox_Options_StrictSkillGroups";
            this.chkStrictSkillGroups.Text = "Strict interprentation of breaking skill groups in create mode";
            this.chkStrictSkillGroups.UseVisualStyleBackColor = true;
            // 
            // chkEnforceCapacity
            // 
            this.chkEnforceCapacity.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkEnforceCapacity.AutoSize = true;
            this.chkEnforceCapacity.DefaultColorScheme = true;
            this.chkEnforceCapacity.Location = new System.Drawing.Point(3, 3);
            this.chkEnforceCapacity.Name = "chkEnforceCapacity";
            this.chkEnforceCapacity.Size = new System.Drawing.Size(132, 17);
            this.chkEnforceCapacity.TabIndex = 25;
            this.chkEnforceCapacity.Tag = "Checkbox_Option_EnforceCapacity";
            this.chkEnforceCapacity.Text = "Enforce Capacity limits";
            this.chkEnforceCapacity.UseVisualStyleBackColor = true;
            // 
            // chkLicenseEachRestrictedItem
            // 
            this.chkLicenseEachRestrictedItem.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkLicenseEachRestrictedItem.AutoSize = true;
            this.chkLicenseEachRestrictedItem.DefaultColorScheme = true;
            this.chkLicenseEachRestrictedItem.Location = new System.Drawing.Point(3, 26);
            this.chkLicenseEachRestrictedItem.Name = "chkLicenseEachRestrictedItem";
            this.chkLicenseEachRestrictedItem.Size = new System.Drawing.Size(163, 17);
            this.chkLicenseEachRestrictedItem.TabIndex = 27;
            this.chkLicenseEachRestrictedItem.Tag = "Checkbox_Options_LicenseRestricted";
            this.chkLicenseEachRestrictedItem.Text = "License each Restricted item";
            this.chkLicenseEachRestrictedItem.UseVisualStyleBackColor = true;
            // 
            // chkRestrictRecoil
            // 
            this.chkRestrictRecoil.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkRestrictRecoil.AutoSize = true;
            this.chkRestrictRecoil.DefaultColorScheme = true;
            this.chkRestrictRecoil.Location = new System.Drawing.Point(3, 96);
            this.chkRestrictRecoil.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkRestrictRecoil.Name = "chkRestrictRecoil";
            this.chkRestrictRecoil.Size = new System.Drawing.Size(258, 17);
            this.chkRestrictRecoil.TabIndex = 26;
            this.chkRestrictRecoil.Tag = "Checkbox_Options_UseRestrictionsToRecoilCompensation";
            this.chkRestrictRecoil.Text = "Use Restrictions to Recoil Compensation (RG 53)";
            this.chkRestrictRecoil.UseVisualStyleBackColor = true;
            // 
            // chkDronemodsMaximumPilot
            // 
            this.chkDronemodsMaximumPilot.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkDronemodsMaximumPilot.AutoSize = true;
            this.chkDronemodsMaximumPilot.DefaultColorScheme = true;
            this.chkDronemodsMaximumPilot.Location = new System.Drawing.Point(23, 72);
            this.chkDronemodsMaximumPilot.Margin = new System.Windows.Forms.Padding(23, 3, 3, 3);
            this.chkDronemodsMaximumPilot.Name = "chkDronemodsMaximumPilot";
            this.chkDronemodsMaximumPilot.Size = new System.Drawing.Size(214, 17);
            this.chkDronemodsMaximumPilot.TabIndex = 37;
            this.chkDronemodsMaximumPilot.Tag = "Checkbox_Options_Dronemods_Pilot";
            this.chkDronemodsMaximumPilot.Text = "Use Maximum Attribute for Pilot Attribute";
            this.chkDronemodsMaximumPilot.UseVisualStyleBackColor = true;
            // 
            // chkDronemods
            // 
            this.chkDronemods.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkDronemods.AutoSize = true;
            this.chkDronemods.DefaultColorScheme = true;
            this.chkDronemods.Location = new System.Drawing.Point(3, 49);
            this.chkDronemods.Name = "chkDronemods";
            this.chkDronemods.Size = new System.Drawing.Size(206, 17);
            this.chkDronemods.TabIndex = 36;
            this.chkDronemods.Tag = "Checkbox_Options_Dronemods";
            this.chkDronemods.Text = "Use Drone Modification rules (R5 122)";
            this.chkDronemods.UseVisualStyleBackColor = true;
            // 
            // gpbBasicOptionsCyberlimbs
            // 
            this.gpbBasicOptionsCyberlimbs.AutoSize = true;
            this.gpbBasicOptionsCyberlimbs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbBasicOptionsCyberlimbs.Controls.Add(this.tlpBasicOptionsCyberlimbs);
            this.gpbBasicOptionsCyberlimbs.Location = new System.Drawing.Point(481, 211);
            this.gpbBasicOptionsCyberlimbs.Name = "gpbBasicOptionsCyberlimbs";
            this.gpbBasicOptionsCyberlimbs.Size = new System.Drawing.Size(385, 143);
            this.gpbBasicOptionsCyberlimbs.TabIndex = 2;
            this.gpbBasicOptionsCyberlimbs.TabStop = false;
            this.gpbBasicOptionsCyberlimbs.Tag = "Label_CharacterOptions_Cyberlimbs";
            this.gpbBasicOptionsCyberlimbs.Text = "Cyberlimbs";
            // 
            // tlpBasicOptionsCyberlimbs
            // 
            this.tlpBasicOptionsCyberlimbs.AutoSize = true;
            this.tlpBasicOptionsCyberlimbs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpBasicOptionsCyberlimbs.ColumnCount = 2;
            this.tlpBasicOptionsCyberlimbs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBasicOptionsCyberlimbs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBasicOptionsCyberlimbs.Controls.Add(this.lblRedlinerLimbs, 0, 4);
            this.tlpBasicOptionsCyberlimbs.Controls.Add(this.chkCyberlegMovement, 0, 2);
            this.tlpBasicOptionsCyberlimbs.Controls.Add(this.chkDontUseCyberlimbCalculation, 0, 1);
            this.tlpBasicOptionsCyberlimbs.Controls.Add(this.lblLimbCount, 0, 0);
            this.tlpBasicOptionsCyberlimbs.Controls.Add(this.cboLimbCount, 1, 0);
            this.tlpBasicOptionsCyberlimbs.Controls.Add(this.tlpCyberlimbAttributeBonusCap, 0, 3);
            this.tlpBasicOptionsCyberlimbs.Controls.Add(this.flpRedlinerLimbs, 1, 4);
            this.tlpBasicOptionsCyberlimbs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBasicOptionsCyberlimbs.Location = new System.Drawing.Point(3, 16);
            this.tlpBasicOptionsCyberlimbs.Name = "tlpBasicOptionsCyberlimbs";
            this.tlpBasicOptionsCyberlimbs.RowCount = 5;
            this.tlpBasicOptionsCyberlimbs.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsCyberlimbs.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsCyberlimbs.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsCyberlimbs.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsCyberlimbs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBasicOptionsCyberlimbs.Size = new System.Drawing.Size(379, 124);
            this.tlpBasicOptionsCyberlimbs.TabIndex = 0;
            // 
            // lblRedlinerLimbs
            // 
            this.lblRedlinerLimbs.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblRedlinerLimbs.AutoSize = true;
            this.lblRedlinerLimbs.Location = new System.Drawing.Point(3, 105);
            this.lblRedlinerLimbs.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRedlinerLimbs.Name = "lblRedlinerLimbs";
            this.lblRedlinerLimbs.Size = new System.Drawing.Size(149, 13);
            this.lblRedlinerLimbs.TabIndex = 53;
            this.lblRedlinerLimbs.Tag = "Label_CharacterOptions_RedlinerLimbs";
            this.lblRedlinerLimbs.Text = "Limbs Considered by Redliner:";
            // 
            // chkCyberlegMovement
            // 
            this.chkCyberlegMovement.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkCyberlegMovement.AutoSize = true;
            this.tlpBasicOptionsCyberlimbs.SetColumnSpan(this.chkCyberlegMovement, 2);
            this.chkCyberlegMovement.DefaultColorScheme = true;
            this.chkCyberlegMovement.Location = new System.Drawing.Point(3, 53);
            this.chkCyberlegMovement.Name = "chkCyberlegMovement";
            this.chkCyberlegMovement.Size = new System.Drawing.Size(184, 17);
            this.chkCyberlegMovement.TabIndex = 20;
            this.chkCyberlegMovement.Tag = "Checkbox_Options_CyberlegMovement";
            this.chkCyberlegMovement.Text = "Use Cyberleg Stats for Movement";
            this.chkCyberlegMovement.UseVisualStyleBackColor = true;
            // 
            // chkDontUseCyberlimbCalculation
            // 
            this.chkDontUseCyberlimbCalculation.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkDontUseCyberlimbCalculation.AutoSize = true;
            this.tlpBasicOptionsCyberlimbs.SetColumnSpan(this.chkDontUseCyberlimbCalculation, 2);
            this.chkDontUseCyberlimbCalculation.DefaultColorScheme = true;
            this.chkDontUseCyberlimbCalculation.Location = new System.Drawing.Point(3, 30);
            this.chkDontUseCyberlimbCalculation.Name = "chkDontUseCyberlimbCalculation";
            this.chkDontUseCyberlimbCalculation.Size = new System.Drawing.Size(317, 17);
            this.chkDontUseCyberlimbCalculation.TabIndex = 19;
            this.chkDontUseCyberlimbCalculation.Tag = "Checkbox_Options_UseCyberlimbCalculation";
            this.chkDontUseCyberlimbCalculation.Text = "Do not use Cyberlimbs when calculating augmented Attributes";
            this.chkDontUseCyberlimbCalculation.UseVisualStyleBackColor = true;
            // 
            // lblLimbCount
            // 
            this.lblLimbCount.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblLimbCount.AutoSize = true;
            this.lblLimbCount.Location = new System.Drawing.Point(21, 7);
            this.lblLimbCount.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLimbCount.Name = "lblLimbCount";
            this.lblLimbCount.Size = new System.Drawing.Size(131, 13);
            this.lblLimbCount.TabIndex = 0;
            this.lblLimbCount.Tag = "Label_Options_CyberlimbCount";
            this.lblLimbCount.Text = "Limb Count for Cyberlimbs:";
            // 
            // cboLimbCount
            // 
            this.cboLimbCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboLimbCount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLimbCount.FormattingEnabled = true;
            this.cboLimbCount.Location = new System.Drawing.Point(158, 3);
            this.cboLimbCount.Name = "cboLimbCount";
            this.cboLimbCount.Size = new System.Drawing.Size(218, 21);
            this.cboLimbCount.TabIndex = 1;
            this.cboLimbCount.TooltipText = "";
            this.cboLimbCount.SelectedIndexChanged += new System.EventHandler(this.cboLimbCount_SelectedIndexChanged);
            // 
            // tlpCyberlimbAttributeBonusCap
            // 
            this.tlpCyberlimbAttributeBonusCap.AutoSize = true;
            this.tlpCyberlimbAttributeBonusCap.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpCyberlimbAttributeBonusCap.ColumnCount = 2;
            this.tlpBasicOptionsCyberlimbs.SetColumnSpan(this.tlpCyberlimbAttributeBonusCap, 2);
            this.tlpCyberlimbAttributeBonusCap.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCyberlimbAttributeBonusCap.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCyberlimbAttributeBonusCap.Controls.Add(this.nudCyberlimbAttributeBonusCap, 1, 0);
            this.tlpCyberlimbAttributeBonusCap.Controls.Add(this.flpCyberlimbAttributeBonusCap, 0, 0);
            this.tlpCyberlimbAttributeBonusCap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCyberlimbAttributeBonusCap.Location = new System.Drawing.Point(0, 73);
            this.tlpCyberlimbAttributeBonusCap.Margin = new System.Windows.Forms.Padding(0);
            this.tlpCyberlimbAttributeBonusCap.Name = "tlpCyberlimbAttributeBonusCap";
            this.tlpCyberlimbAttributeBonusCap.RowCount = 1;
            this.tlpCyberlimbAttributeBonusCap.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCyberlimbAttributeBonusCap.Size = new System.Drawing.Size(379, 26);
            this.tlpCyberlimbAttributeBonusCap.TabIndex = 52;
            // 
            // nudCyberlimbAttributeBonusCap
            // 
            this.nudCyberlimbAttributeBonusCap.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudCyberlimbAttributeBonusCap.AutoSize = true;
            this.nudCyberlimbAttributeBonusCap.Enabled = false;
            this.nudCyberlimbAttributeBonusCap.Location = new System.Drawing.Point(262, 3);
            this.nudCyberlimbAttributeBonusCap.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudCyberlimbAttributeBonusCap.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudCyberlimbAttributeBonusCap.Name = "nudCyberlimbAttributeBonusCap";
            this.nudCyberlimbAttributeBonusCap.Size = new System.Drawing.Size(41, 20);
            this.nudCyberlimbAttributeBonusCap.TabIndex = 49;
            this.nudCyberlimbAttributeBonusCap.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // flpCyberlimbAttributeBonusCap
            // 
            this.flpCyberlimbAttributeBonusCap.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpCyberlimbAttributeBonusCap.AutoSize = true;
            this.flpCyberlimbAttributeBonusCap.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpCyberlimbAttributeBonusCap.Controls.Add(this.chkCyberlimbAttributeBonusCap);
            this.flpCyberlimbAttributeBonusCap.Controls.Add(this.lblCyberlimbAttributeBonusCapPlus);
            this.flpCyberlimbAttributeBonusCap.Location = new System.Drawing.Point(0, 1);
            this.flpCyberlimbAttributeBonusCap.Margin = new System.Windows.Forms.Padding(0);
            this.flpCyberlimbAttributeBonusCap.Name = "flpCyberlimbAttributeBonusCap";
            this.flpCyberlimbAttributeBonusCap.Size = new System.Drawing.Size(259, 23);
            this.flpCyberlimbAttributeBonusCap.TabIndex = 51;
            this.flpCyberlimbAttributeBonusCap.WrapContents = false;
            // 
            // chkCyberlimbAttributeBonusCap
            // 
            this.chkCyberlimbAttributeBonusCap.AutoSize = true;
            this.chkCyberlimbAttributeBonusCap.DefaultColorScheme = true;
            this.chkCyberlimbAttributeBonusCap.Dock = System.Windows.Forms.DockStyle.Left;
            this.chkCyberlimbAttributeBonusCap.Location = new System.Drawing.Point(3, 3);
            this.chkCyberlimbAttributeBonusCap.Name = "chkCyberlimbAttributeBonusCap";
            this.chkCyberlimbAttributeBonusCap.Size = new System.Drawing.Size(234, 17);
            this.chkCyberlimbAttributeBonusCap.TabIndex = 47;
            this.chkCyberlimbAttributeBonusCap.Tag = "Checkbox_Options_CyberlimbAttributeBonusCap";
            this.chkCyberlimbAttributeBonusCap.Text = "Override maximum Bonus cap for Cyberlimbs";
            this.chkCyberlimbAttributeBonusCap.UseVisualStyleBackColor = true;
            // 
            // lblCyberlimbAttributeBonusCapPlus
            // 
            this.lblCyberlimbAttributeBonusCapPlus.AutoSize = true;
            this.lblCyberlimbAttributeBonusCapPlus.Location = new System.Drawing.Point(243, 3);
            this.lblCyberlimbAttributeBonusCapPlus.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.lblCyberlimbAttributeBonusCapPlus.Name = "lblCyberlimbAttributeBonusCapPlus";
            this.lblCyberlimbAttributeBonusCapPlus.Size = new System.Drawing.Size(13, 13);
            this.lblCyberlimbAttributeBonusCapPlus.TabIndex = 50;
            this.lblCyberlimbAttributeBonusCapPlus.Text = "+";
            this.lblCyberlimbAttributeBonusCapPlus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // flpRedlinerLimbs
            // 
            this.flpRedlinerLimbs.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpRedlinerLimbs.AutoSize = true;
            this.flpRedlinerLimbs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpRedlinerLimbs.Controls.Add(this.chkRedlinerLimbsSkull);
            this.flpRedlinerLimbs.Controls.Add(this.chkRedlinerLimbsTorso);
            this.flpRedlinerLimbs.Controls.Add(this.chkRedlinerLimbsArms);
            this.flpRedlinerLimbs.Controls.Add(this.chkRedlinerLimbsLegs);
            this.flpRedlinerLimbs.Location = new System.Drawing.Point(155, 100);
            this.flpRedlinerLimbs.Margin = new System.Windows.Forms.Padding(0);
            this.flpRedlinerLimbs.Name = "flpRedlinerLimbs";
            this.flpRedlinerLimbs.Size = new System.Drawing.Size(224, 23);
            this.flpRedlinerLimbs.TabIndex = 54;
            // 
            // chkRedlinerLimbsSkull
            // 
            this.chkRedlinerLimbsSkull.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkRedlinerLimbsSkull.AutoSize = true;
            this.chkRedlinerLimbsSkull.DefaultColorScheme = true;
            this.chkRedlinerLimbsSkull.Location = new System.Drawing.Point(3, 3);
            this.chkRedlinerLimbsSkull.Name = "chkRedlinerLimbsSkull";
            this.chkRedlinerLimbsSkull.Size = new System.Drawing.Size(49, 17);
            this.chkRedlinerLimbsSkull.TabIndex = 21;
            this.chkRedlinerLimbsSkull.Tag = "String_Skull";
            this.chkRedlinerLimbsSkull.Text = "Skull";
            this.chkRedlinerLimbsSkull.UseVisualStyleBackColor = true;
            // 
            // chkRedlinerLimbsTorso
            // 
            this.chkRedlinerLimbsTorso.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkRedlinerLimbsTorso.AutoSize = true;
            this.chkRedlinerLimbsTorso.DefaultColorScheme = true;
            this.chkRedlinerLimbsTorso.Location = new System.Drawing.Point(58, 3);
            this.chkRedlinerLimbsTorso.Name = "chkRedlinerLimbsTorso";
            this.chkRedlinerLimbsTorso.Size = new System.Drawing.Size(53, 17);
            this.chkRedlinerLimbsTorso.TabIndex = 22;
            this.chkRedlinerLimbsTorso.Tag = "String_Torso";
            this.chkRedlinerLimbsTorso.Text = "Torso";
            this.chkRedlinerLimbsTorso.UseVisualStyleBackColor = true;
            // 
            // chkRedlinerLimbsArms
            // 
            this.chkRedlinerLimbsArms.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkRedlinerLimbsArms.AutoSize = true;
            this.chkRedlinerLimbsArms.DefaultColorScheme = true;
            this.chkRedlinerLimbsArms.Location = new System.Drawing.Point(117, 3);
            this.chkRedlinerLimbsArms.Name = "chkRedlinerLimbsArms";
            this.chkRedlinerLimbsArms.Size = new System.Drawing.Size(49, 17);
            this.chkRedlinerLimbsArms.TabIndex = 23;
            this.chkRedlinerLimbsArms.Tag = "String_Arms";
            this.chkRedlinerLimbsArms.Text = "Arms";
            this.chkRedlinerLimbsArms.UseVisualStyleBackColor = true;
            // 
            // chkRedlinerLimbsLegs
            // 
            this.chkRedlinerLimbsLegs.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkRedlinerLimbsLegs.AutoSize = true;
            this.chkRedlinerLimbsLegs.DefaultColorScheme = true;
            this.chkRedlinerLimbsLegs.Location = new System.Drawing.Point(172, 3);
            this.chkRedlinerLimbsLegs.Name = "chkRedlinerLimbsLegs";
            this.chkRedlinerLimbsLegs.Size = new System.Drawing.Size(49, 17);
            this.chkRedlinerLimbsLegs.TabIndex = 24;
            this.chkRedlinerLimbsLegs.Tag = "String_Legs";
            this.chkRedlinerLimbsLegs.Text = "Legs";
            this.chkRedlinerLimbsLegs.UseVisualStyleBackColor = true;
            // 
            // gpbBasicOptionsRounding
            // 
            this.gpbBasicOptionsRounding.AutoSize = true;
            this.gpbBasicOptionsRounding.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbBasicOptionsRounding.Controls.Add(this.tlpBasicOptionsRounding);
            this.gpbBasicOptionsRounding.Location = new System.Drawing.Point(3, 445);
            this.gpbBasicOptionsRounding.Name = "gpbBasicOptionsRounding";
            this.gpbBasicOptionsRounding.Size = new System.Drawing.Size(361, 120);
            this.gpbBasicOptionsRounding.TabIndex = 1;
            this.gpbBasicOptionsRounding.TabStop = false;
            this.gpbBasicOptionsRounding.Tag = "Label_CharacterOptions_DecimalsAndRounding";
            this.gpbBasicOptionsRounding.Text = "Decimals and Rounding";
            // 
            // tlpBasicOptionsRounding
            // 
            this.tlpBasicOptionsRounding.AutoSize = true;
            this.tlpBasicOptionsRounding.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpBasicOptionsRounding.ColumnCount = 2;
            this.tlpBasicOptionsRounding.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBasicOptionsRounding.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBasicOptionsRounding.Controls.Add(this.chkDontRoundEssenceInternally, 0, 3);
            this.tlpBasicOptionsRounding.Controls.Add(this.lblNuyenDecimalsMinimumLabel, 0, 0);
            this.tlpBasicOptionsRounding.Controls.Add(this.lblNuyenDecimalsMaximumLabel, 0, 1);
            this.tlpBasicOptionsRounding.Controls.Add(this.lblEssenceDecimals, 0, 2);
            this.tlpBasicOptionsRounding.Controls.Add(this.nudNuyenDecimalsMinimum, 1, 0);
            this.tlpBasicOptionsRounding.Controls.Add(this.nudNuyenDecimalsMaximum, 1, 1);
            this.tlpBasicOptionsRounding.Controls.Add(this.nudEssenceDecimals, 1, 2);
            this.tlpBasicOptionsRounding.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBasicOptionsRounding.Location = new System.Drawing.Point(3, 16);
            this.tlpBasicOptionsRounding.Name = "tlpBasicOptionsRounding";
            this.tlpBasicOptionsRounding.RowCount = 4;
            this.tlpBasicOptionsRounding.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsRounding.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsRounding.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsRounding.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBasicOptionsRounding.Size = new System.Drawing.Size(355, 101);
            this.tlpBasicOptionsRounding.TabIndex = 0;
            // 
            // chkDontRoundEssenceInternally
            // 
            this.chkDontRoundEssenceInternally.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkDontRoundEssenceInternally.AutoSize = true;
            this.tlpBasicOptionsRounding.SetColumnSpan(this.chkDontRoundEssenceInternally, 2);
            this.chkDontRoundEssenceInternally.DefaultColorScheme = true;
            this.chkDontRoundEssenceInternally.Location = new System.Drawing.Point(3, 81);
            this.chkDontRoundEssenceInternally.Name = "chkDontRoundEssenceInternally";
            this.chkDontRoundEssenceInternally.Size = new System.Drawing.Size(349, 17);
            this.chkDontRoundEssenceInternally.TabIndex = 18;
            this.chkDontRoundEssenceInternally.Tag = "Checkbox_Option_DontRoundEssenceInternally";
            this.chkDontRoundEssenceInternally.Text = "Only round Essence for display purposes, not for internal calculations";
            this.chkDontRoundEssenceInternally.UseVisualStyleBackColor = true;
            // 
            // lblNuyenDecimalsMinimumLabel
            // 
            this.lblNuyenDecimalsMinimumLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblNuyenDecimalsMinimumLabel.AutoSize = true;
            this.lblNuyenDecimalsMinimumLabel.Location = new System.Drawing.Point(6, 6);
            this.lblNuyenDecimalsMinimumLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblNuyenDecimalsMinimumLabel.Name = "lblNuyenDecimalsMinimumLabel";
            this.lblNuyenDecimalsMinimumLabel.Size = new System.Drawing.Size(255, 13);
            this.lblNuyenDecimalsMinimumLabel.TabIndex = 30;
            this.lblNuyenDecimalsMinimumLabel.Tag = "Label_Options_NuyenDecimalsMinimum";
            this.lblNuyenDecimalsMinimumLabel.Text = "Minimum number of Nuyen decimal places to display:";
            this.lblNuyenDecimalsMinimumLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblNuyenDecimalsMaximumLabel
            // 
            this.lblNuyenDecimalsMaximumLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblNuyenDecimalsMaximumLabel.AutoSize = true;
            this.lblNuyenDecimalsMaximumLabel.Location = new System.Drawing.Point(3, 32);
            this.lblNuyenDecimalsMaximumLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblNuyenDecimalsMaximumLabel.Name = "lblNuyenDecimalsMaximumLabel";
            this.lblNuyenDecimalsMaximumLabel.Size = new System.Drawing.Size(258, 13);
            this.lblNuyenDecimalsMaximumLabel.TabIndex = 32;
            this.lblNuyenDecimalsMaximumLabel.Tag = "Label_Options_NuyenDecimalsMaximum";
            this.lblNuyenDecimalsMaximumLabel.Text = "Maximum number of Nuyen decimal places to display:";
            this.lblNuyenDecimalsMaximumLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblEssenceDecimals
            // 
            this.lblEssenceDecimals.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblEssenceDecimals.AutoSize = true;
            this.lblEssenceDecimals.Location = new System.Drawing.Point(31, 58);
            this.lblEssenceDecimals.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblEssenceDecimals.Name = "lblEssenceDecimals";
            this.lblEssenceDecimals.Size = new System.Drawing.Size(230, 13);
            this.lblEssenceDecimals.TabIndex = 16;
            this.lblEssenceDecimals.Tag = "Label_Options_EssenceDecimals";
            this.lblEssenceDecimals.Text = "Number of decimal places to round Essence to:";
            this.lblEssenceDecimals.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudNuyenDecimalsMinimum
            // 
            this.nudNuyenDecimalsMinimum.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudNuyenDecimalsMinimum.AutoSize = true;
            this.nudNuyenDecimalsMinimum.Location = new System.Drawing.Point(267, 3);
            this.nudNuyenDecimalsMinimum.Maximum = new decimal(new int[] {
            28,
            0,
            0,
            0});
            this.nudNuyenDecimalsMinimum.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudNuyenDecimalsMinimum.Name = "nudNuyenDecimalsMinimum";
            this.nudNuyenDecimalsMinimum.Size = new System.Drawing.Size(35, 20);
            this.nudNuyenDecimalsMinimum.TabIndex = 33;
            // 
            // nudNuyenDecimalsMaximum
            // 
            this.nudNuyenDecimalsMaximum.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudNuyenDecimalsMaximum.AutoSize = true;
            this.nudNuyenDecimalsMaximum.Location = new System.Drawing.Point(267, 29);
            this.nudNuyenDecimalsMaximum.Maximum = new decimal(new int[] {
            28,
            0,
            0,
            0});
            this.nudNuyenDecimalsMaximum.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudNuyenDecimalsMaximum.Name = "nudNuyenDecimalsMaximum";
            this.nudNuyenDecimalsMaximum.Size = new System.Drawing.Size(35, 20);
            this.nudNuyenDecimalsMaximum.TabIndex = 34;
            // 
            // nudEssenceDecimals
            // 
            this.nudEssenceDecimals.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudEssenceDecimals.AutoSize = true;
            this.nudEssenceDecimals.Location = new System.Drawing.Point(267, 55);
            this.nudEssenceDecimals.Maximum = new decimal(new int[] {
            28,
            0,
            0,
            0});
            this.nudEssenceDecimals.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nudEssenceDecimals.Name = "nudEssenceDecimals";
            this.nudEssenceDecimals.Size = new System.Drawing.Size(35, 20);
            this.nudEssenceDecimals.TabIndex = 17;
            this.nudEssenceDecimals.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // tabKarmaCosts
            // 
            this.tabKarmaCosts.BackColor = System.Drawing.SystemColors.Control;
            this.tabKarmaCosts.Controls.Add(this.tlpKarmaCosts);
            this.tabKarmaCosts.Location = new System.Drawing.Point(4, 22);
            this.tabKarmaCosts.Name = "tabKarmaCosts";
            this.tabKarmaCosts.Padding = new System.Windows.Forms.Padding(9);
            this.tabKarmaCosts.Size = new System.Drawing.Size(1232, 602);
            this.tabKarmaCosts.TabIndex = 1;
            this.tabKarmaCosts.Tag = "Tab_Options_KarmaCosts";
            this.tabKarmaCosts.Text = "Karma Costs";
            // 
            // tlpKarmaCosts
            // 
            this.tlpKarmaCosts.AutoScroll = true;
            this.tlpKarmaCosts.AutoSize = true;
            this.tlpKarmaCosts.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpKarmaCosts.ColumnCount = 10;
            this.tlpKarmaCosts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpKarmaCosts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpKarmaCosts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpKarmaCosts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpKarmaCosts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpKarmaCosts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpKarmaCosts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpKarmaCosts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpKarmaCosts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpKarmaCosts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaSpecialization, 0, 0);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaSpecialization, 1, 0);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaKnowledgeSpecialization, 0, 1);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaKnowledgeSpecialization, 1, 1);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaNewKnowledgeSkill, 0, 2);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaNewKnowledgeSkill, 1, 2);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaLeaveGroup, 3, 1);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaLeaveGroup, 4, 1);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaNewActiveSkill, 0, 3);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaNewActiveSkill, 1, 3);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaNewSkillGroup, 0, 4);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaNewSkillGroup, 1, 4);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaImproveKnowledgeSkill, 0, 5);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaImproveKnowledgeSkill, 1, 5);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaImproveKnowledgeSkillExtra, 2, 5);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaImproveActiveSkill, 0, 6);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaImproveActiveSkill, 1, 6);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaImproveActiveSkillExtra, 2, 6);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaImproveSkillGroup, 0, 7);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaImproveSkillGroup, 1, 7);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaImproveSkillGroupExtra, 2, 7);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaAttribute, 0, 8);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaAttribute, 1, 8);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaAttributeExtra, 2, 8);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaAlchemicalFocus, 7, 0);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaAlchemicalFocus, 8, 0);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaAlchemicalFocusExtra, 9, 0);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaBanishingFocus, 7, 1);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaBanishingFocus, 8, 1);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaBindingFocus, 7, 2);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaBindingFocus, 8, 2);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaBanishingFocusExtra, 9, 1);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaCenteringFocus, 7, 3);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaCenteringFocus, 8, 3);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaBindingFocusExtra, 9, 2);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaCenteringFocusExtra, 9, 3);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaCounterspellingFocus, 7, 4);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaCounterspellingFocus, 8, 4);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaCounterspellingFocusExtra, 9, 4);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaDisenchantingFocus, 7, 5);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaDisenchantingFocus, 8, 5);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaDisenchantingFocusExtra, 9, 5);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaFlexibleSignatureFocus, 7, 6);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaFlexibleSignatureFocus, 8, 6);
            this.tlpKarmaCosts.Controls.Add(this.lblFlexibleSignatureFocusExtra, 9, 6);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaMaskingFocus, 7, 7);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaMaskingFocus, 8, 7);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaMaskingFocusExtra, 9, 7);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaPowerFocus, 7, 8);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaPowerFocus, 8, 8);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaPowerFocusExtra, 9, 8);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaQiFocus, 7, 9);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaQiFocus, 8, 9);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaQiFocusExtra, 9, 9);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaRitualSpellcastingFocus, 7, 10);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaRitualSpellcastingFocus, 8, 10);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaRitualSpellcastingFocusExtra, 9, 10);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaSpellcastingFocus, 7, 11);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaSpellcastingFocus, 8, 11);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaSpellcastingFocusExtra, 9, 11);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaSpell, 3, 2);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaSpell, 4, 2);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaContact, 0, 10);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaContact, 1, 10);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaContactExtra, 2, 10);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaEnemyExtra, 2, 11);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaEnemy, 1, 11);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaEnemy, 0, 11);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaSpirit, 3, 3);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaSpirit, 4, 3);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaSpiritExtra, 5, 3);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaNewComplexForm, 3, 5);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaNewComplexForm, 4, 5);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaMysticAdeptPowerPoint, 3, 4);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaMysticAdeptPowerPoint, 4, 4);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaNuyenPer, 0, 12);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaNuyenPer, 1, 12);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaNuyenPerExtra, 2, 12);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaJoinGroup, 3, 0);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaJoinGroup, 4, 0);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaNewAIProgram, 3, 6);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaNewAIProgram, 4, 6);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaNewAIAdvancedProgram, 3, 7);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaNewAIAdvancedProgram, 4, 7);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaMetamagic, 3, 9);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaMetamagic, 4, 9);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaTechnique, 3, 10);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaTechnique, 4, 10);
            this.tlpKarmaCosts.Controls.Add(this.flpKarmaInitiation, 3, 8);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaInitiation, 4, 8);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaInitiationExtra, 5, 8);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaInitiationFlat, 6, 8);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaCarryover, 0, 9);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaCarryover, 1, 9);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaCarryoverExtra, 2, 9);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaQuality, 0, 13);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaQuality, 1, 13);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaQualityExtra, 2, 13);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaSummoningFocus, 7, 13);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaSummoningFocus, 8, 13);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaSpellShapingFocus, 7, 12);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaSpellShapingFocusExtra, 9, 12);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaSpellShapingFocus, 8, 12);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaSummoningFocusExtra, 9, 13);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaSustainingFocus, 7, 14);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaWeaponFocus, 7, 15);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaSustainingFocusExtra, 9, 14);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaWeaponFocusExtra, 9, 15);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaSustainingFocus, 8, 14);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaWeaponFocus, 8, 15);
            this.tlpKarmaCosts.Controls.Add(this.lblMetatypeCostsKarmaMultiplierLabel, 0, 14);
            this.tlpKarmaCosts.Controls.Add(this.nudMetatypeCostsKarmaMultiplier, 1, 14);
            this.tlpKarmaCosts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpKarmaCosts.Location = new System.Drawing.Point(9, 9);
            this.tlpKarmaCosts.Name = "tlpKarmaCosts";
            this.tlpKarmaCosts.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.tlpKarmaCosts.RowCount = 17;
            this.tlpKarmaCosts.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCosts.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCosts.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCosts.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCosts.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCosts.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCosts.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCosts.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCosts.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCosts.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCosts.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCosts.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCosts.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCosts.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCosts.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCosts.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCosts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpKarmaCosts.Size = new System.Drawing.Size(1214, 584);
            this.tlpKarmaCosts.TabIndex = 124;
            // 
            // lblKarmaSpecialization
            // 
            this.lblKarmaSpecialization.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaSpecialization.AutoSize = true;
            this.lblKarmaSpecialization.Location = new System.Drawing.Point(26, 6);
            this.lblKarmaSpecialization.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSpecialization.Name = "lblKarmaSpecialization";
            this.lblKarmaSpecialization.Size = new System.Drawing.Size(152, 13);
            this.lblKarmaSpecialization.TabIndex = 0;
            this.lblKarmaSpecialization.Tag = "Label_Options_NewSpecialization";
            this.lblKarmaSpecialization.Text = "New Active Skill Specialization";
            this.lblKarmaSpecialization.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaSpecialization
            // 
            this.nudKarmaSpecialization.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaSpecialization.AutoSize = true;
            this.nudKarmaSpecialization.Location = new System.Drawing.Point(184, 3);
            this.nudKarmaSpecialization.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaSpecialization.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaSpecialization.Name = "nudKarmaSpecialization";
            this.nudKarmaSpecialization.Size = new System.Drawing.Size(47, 20);
            this.nudKarmaSpecialization.TabIndex = 1;
            // 
            // lblKarmaKnowledgeSpecialization
            // 
            this.lblKarmaKnowledgeSpecialization.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaKnowledgeSpecialization.AutoSize = true;
            this.lblKarmaKnowledgeSpecialization.Location = new System.Drawing.Point(3, 32);
            this.lblKarmaKnowledgeSpecialization.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaKnowledgeSpecialization.Name = "lblKarmaKnowledgeSpecialization";
            this.lblKarmaKnowledgeSpecialization.Size = new System.Drawing.Size(175, 13);
            this.lblKarmaKnowledgeSpecialization.TabIndex = 119;
            this.lblKarmaKnowledgeSpecialization.Tag = "Label_Options_NewKnoSpecialization";
            this.lblKarmaKnowledgeSpecialization.Text = "New Knowledge Skill Specialization";
            this.lblKarmaKnowledgeSpecialization.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaKnowledgeSpecialization
            // 
            this.nudKarmaKnowledgeSpecialization.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaKnowledgeSpecialization.AutoSize = true;
            this.nudKarmaKnowledgeSpecialization.Location = new System.Drawing.Point(184, 29);
            this.nudKarmaKnowledgeSpecialization.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaKnowledgeSpecialization.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaKnowledgeSpecialization.Name = "nudKarmaKnowledgeSpecialization";
            this.nudKarmaKnowledgeSpecialization.Size = new System.Drawing.Size(47, 20);
            this.nudKarmaKnowledgeSpecialization.TabIndex = 120;
            // 
            // lblKarmaNewKnowledgeSkill
            // 
            this.lblKarmaNewKnowledgeSkill.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaNewKnowledgeSkill.AutoSize = true;
            this.lblKarmaNewKnowledgeSkill.Location = new System.Drawing.Point(71, 58);
            this.lblKarmaNewKnowledgeSkill.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaNewKnowledgeSkill.Name = "lblKarmaNewKnowledgeSkill";
            this.lblKarmaNewKnowledgeSkill.Size = new System.Drawing.Size(107, 13);
            this.lblKarmaNewKnowledgeSkill.TabIndex = 2;
            this.lblKarmaNewKnowledgeSkill.Tag = "Label_Options_NewKnowledgeSkill";
            this.lblKarmaNewKnowledgeSkill.Text = "New Knowledge Skill";
            this.lblKarmaNewKnowledgeSkill.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaNewKnowledgeSkill
            // 
            this.nudKarmaNewKnowledgeSkill.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaNewKnowledgeSkill.AutoSize = true;
            this.nudKarmaNewKnowledgeSkill.Location = new System.Drawing.Point(184, 55);
            this.nudKarmaNewKnowledgeSkill.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaNewKnowledgeSkill.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaNewKnowledgeSkill.Name = "nudKarmaNewKnowledgeSkill";
            this.nudKarmaNewKnowledgeSkill.Size = new System.Drawing.Size(47, 20);
            this.nudKarmaNewKnowledgeSkill.TabIndex = 3;
            // 
            // lblKarmaLeaveGroup
            // 
            this.lblKarmaLeaveGroup.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaLeaveGroup.AutoSize = true;
            this.lblKarmaLeaveGroup.Location = new System.Drawing.Point(403, 32);
            this.lblKarmaLeaveGroup.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaLeaveGroup.Name = "lblKarmaLeaveGroup";
            this.lblKarmaLeaveGroup.Size = new System.Drawing.Size(114, 13);
            this.lblKarmaLeaveGroup.TabIndex = 58;
            this.lblKarmaLeaveGroup.Tag = "Label_Options_LeaveGroup";
            this.lblKarmaLeaveGroup.Text = "Leave Group/Network";
            this.lblKarmaLeaveGroup.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaLeaveGroup
            // 
            this.nudKarmaLeaveGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaLeaveGroup.AutoSize = true;
            this.nudKarmaLeaveGroup.Location = new System.Drawing.Point(523, 29);
            this.nudKarmaLeaveGroup.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaLeaveGroup.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaLeaveGroup.Name = "nudKarmaLeaveGroup";
            this.nudKarmaLeaveGroup.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaLeaveGroup.TabIndex = 59;
            // 
            // lblKarmaNewActiveSkill
            // 
            this.lblKarmaNewActiveSkill.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaNewActiveSkill.AutoSize = true;
            this.lblKarmaNewActiveSkill.Location = new System.Drawing.Point(94, 84);
            this.lblKarmaNewActiveSkill.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaNewActiveSkill.Name = "lblKarmaNewActiveSkill";
            this.lblKarmaNewActiveSkill.Size = new System.Drawing.Size(84, 13);
            this.lblKarmaNewActiveSkill.TabIndex = 4;
            this.lblKarmaNewActiveSkill.Tag = "Label_Options_NewActiveSkill";
            this.lblKarmaNewActiveSkill.Text = "New Active Skill";
            this.lblKarmaNewActiveSkill.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaNewActiveSkill
            // 
            this.nudKarmaNewActiveSkill.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaNewActiveSkill.AutoSize = true;
            this.nudKarmaNewActiveSkill.Location = new System.Drawing.Point(184, 81);
            this.nudKarmaNewActiveSkill.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaNewActiveSkill.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaNewActiveSkill.Name = "nudKarmaNewActiveSkill";
            this.nudKarmaNewActiveSkill.Size = new System.Drawing.Size(47, 20);
            this.nudKarmaNewActiveSkill.TabIndex = 5;
            // 
            // lblKarmaNewSkillGroup
            // 
            this.lblKarmaNewSkillGroup.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaNewSkillGroup.AutoSize = true;
            this.lblKarmaNewSkillGroup.Location = new System.Drawing.Point(95, 110);
            this.lblKarmaNewSkillGroup.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaNewSkillGroup.Name = "lblKarmaNewSkillGroup";
            this.lblKarmaNewSkillGroup.Size = new System.Drawing.Size(83, 13);
            this.lblKarmaNewSkillGroup.TabIndex = 6;
            this.lblKarmaNewSkillGroup.Tag = "Label_Options_NewSkillGroup";
            this.lblKarmaNewSkillGroup.Text = "New Skill Group";
            this.lblKarmaNewSkillGroup.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaNewSkillGroup
            // 
            this.nudKarmaNewSkillGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaNewSkillGroup.AutoSize = true;
            this.nudKarmaNewSkillGroup.Location = new System.Drawing.Point(184, 107);
            this.nudKarmaNewSkillGroup.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaNewSkillGroup.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaNewSkillGroup.Name = "nudKarmaNewSkillGroup";
            this.nudKarmaNewSkillGroup.Size = new System.Drawing.Size(47, 20);
            this.nudKarmaNewSkillGroup.TabIndex = 7;
            // 
            // lblKarmaImproveKnowledgeSkill
            // 
            this.lblKarmaImproveKnowledgeSkill.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaImproveKnowledgeSkill.AutoSize = true;
            this.lblKarmaImproveKnowledgeSkill.Location = new System.Drawing.Point(32, 136);
            this.lblKarmaImproveKnowledgeSkill.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaImproveKnowledgeSkill.Name = "lblKarmaImproveKnowledgeSkill";
            this.lblKarmaImproveKnowledgeSkill.Size = new System.Drawing.Size(146, 13);
            this.lblKarmaImproveKnowledgeSkill.TabIndex = 8;
            this.lblKarmaImproveKnowledgeSkill.Tag = "Label_Options_ImproveKnowledgeSkill";
            this.lblKarmaImproveKnowledgeSkill.Text = "Improve Knowledge Skill by 1";
            this.lblKarmaImproveKnowledgeSkill.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaImproveKnowledgeSkill
            // 
            this.nudKarmaImproveKnowledgeSkill.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaImproveKnowledgeSkill.AutoSize = true;
            this.nudKarmaImproveKnowledgeSkill.Location = new System.Drawing.Point(184, 133);
            this.nudKarmaImproveKnowledgeSkill.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaImproveKnowledgeSkill.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaImproveKnowledgeSkill.Name = "nudKarmaImproveKnowledgeSkill";
            this.nudKarmaImproveKnowledgeSkill.Size = new System.Drawing.Size(47, 20);
            this.nudKarmaImproveKnowledgeSkill.TabIndex = 9;
            // 
            // lblKarmaImproveKnowledgeSkillExtra
            // 
            this.lblKarmaImproveKnowledgeSkillExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaImproveKnowledgeSkillExtra.AutoSize = true;
            this.lblKarmaImproveKnowledgeSkillExtra.Location = new System.Drawing.Point(237, 136);
            this.lblKarmaImproveKnowledgeSkillExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaImproveKnowledgeSkillExtra.Name = "lblKarmaImproveKnowledgeSkillExtra";
            this.lblKarmaImproveKnowledgeSkillExtra.Size = new System.Drawing.Size(71, 13);
            this.lblKarmaImproveKnowledgeSkillExtra.TabIndex = 10;
            this.lblKarmaImproveKnowledgeSkillExtra.Tag = "Label_Options_NewRating";
            this.lblKarmaImproveKnowledgeSkillExtra.Text = "x New Rating";
            this.lblKarmaImproveKnowledgeSkillExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaImproveActiveSkill
            // 
            this.lblKarmaImproveActiveSkill.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaImproveActiveSkill.AutoSize = true;
            this.lblKarmaImproveActiveSkill.Location = new System.Drawing.Point(55, 162);
            this.lblKarmaImproveActiveSkill.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaImproveActiveSkill.Name = "lblKarmaImproveActiveSkill";
            this.lblKarmaImproveActiveSkill.Size = new System.Drawing.Size(123, 13);
            this.lblKarmaImproveActiveSkill.TabIndex = 11;
            this.lblKarmaImproveActiveSkill.Tag = "Label_Options_ImproveActiveSkill";
            this.lblKarmaImproveActiveSkill.Text = "Improve Active Skill by 1";
            this.lblKarmaImproveActiveSkill.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaImproveActiveSkill
            // 
            this.nudKarmaImproveActiveSkill.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaImproveActiveSkill.AutoSize = true;
            this.nudKarmaImproveActiveSkill.Location = new System.Drawing.Point(184, 159);
            this.nudKarmaImproveActiveSkill.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaImproveActiveSkill.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaImproveActiveSkill.Name = "nudKarmaImproveActiveSkill";
            this.nudKarmaImproveActiveSkill.Size = new System.Drawing.Size(47, 20);
            this.nudKarmaImproveActiveSkill.TabIndex = 12;
            // 
            // lblKarmaImproveActiveSkillExtra
            // 
            this.lblKarmaImproveActiveSkillExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaImproveActiveSkillExtra.AutoSize = true;
            this.lblKarmaImproveActiveSkillExtra.Location = new System.Drawing.Point(237, 162);
            this.lblKarmaImproveActiveSkillExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaImproveActiveSkillExtra.Name = "lblKarmaImproveActiveSkillExtra";
            this.lblKarmaImproveActiveSkillExtra.Size = new System.Drawing.Size(71, 13);
            this.lblKarmaImproveActiveSkillExtra.TabIndex = 13;
            this.lblKarmaImproveActiveSkillExtra.Tag = "Label_Options_NewRating";
            this.lblKarmaImproveActiveSkillExtra.Text = "x New Rating";
            this.lblKarmaImproveActiveSkillExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaImproveSkillGroup
            // 
            this.lblKarmaImproveSkillGroup.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaImproveSkillGroup.AutoSize = true;
            this.lblKarmaImproveSkillGroup.Location = new System.Drawing.Point(56, 188);
            this.lblKarmaImproveSkillGroup.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaImproveSkillGroup.Name = "lblKarmaImproveSkillGroup";
            this.lblKarmaImproveSkillGroup.Size = new System.Drawing.Size(122, 13);
            this.lblKarmaImproveSkillGroup.TabIndex = 14;
            this.lblKarmaImproveSkillGroup.Tag = "Label_Options_ImproveSkillGroup";
            this.lblKarmaImproveSkillGroup.Text = "Improve Skill Group by 1";
            this.lblKarmaImproveSkillGroup.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaImproveSkillGroup
            // 
            this.nudKarmaImproveSkillGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaImproveSkillGroup.AutoSize = true;
            this.nudKarmaImproveSkillGroup.Location = new System.Drawing.Point(184, 185);
            this.nudKarmaImproveSkillGroup.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaImproveSkillGroup.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaImproveSkillGroup.Name = "nudKarmaImproveSkillGroup";
            this.nudKarmaImproveSkillGroup.Size = new System.Drawing.Size(47, 20);
            this.nudKarmaImproveSkillGroup.TabIndex = 15;
            // 
            // lblKarmaImproveSkillGroupExtra
            // 
            this.lblKarmaImproveSkillGroupExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaImproveSkillGroupExtra.AutoSize = true;
            this.lblKarmaImproveSkillGroupExtra.Location = new System.Drawing.Point(237, 188);
            this.lblKarmaImproveSkillGroupExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaImproveSkillGroupExtra.Name = "lblKarmaImproveSkillGroupExtra";
            this.lblKarmaImproveSkillGroupExtra.Size = new System.Drawing.Size(71, 13);
            this.lblKarmaImproveSkillGroupExtra.TabIndex = 16;
            this.lblKarmaImproveSkillGroupExtra.Tag = "Label_Options_NewRating";
            this.lblKarmaImproveSkillGroupExtra.Text = "x New Rating";
            this.lblKarmaImproveSkillGroupExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaAttribute
            // 
            this.lblKarmaAttribute.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaAttribute.AutoSize = true;
            this.lblKarmaAttribute.Location = new System.Drawing.Point(68, 214);
            this.lblKarmaAttribute.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaAttribute.Name = "lblKarmaAttribute";
            this.lblKarmaAttribute.Size = new System.Drawing.Size(110, 13);
            this.lblKarmaAttribute.TabIndex = 17;
            this.lblKarmaAttribute.Tag = "Label_Options_ImproveAttribute";
            this.lblKarmaAttribute.Text = "Improve Attribute by 1";
            this.lblKarmaAttribute.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaAttribute
            // 
            this.nudKarmaAttribute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaAttribute.AutoSize = true;
            this.nudKarmaAttribute.Location = new System.Drawing.Point(184, 211);
            this.nudKarmaAttribute.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaAttribute.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaAttribute.Name = "nudKarmaAttribute";
            this.nudKarmaAttribute.Size = new System.Drawing.Size(47, 20);
            this.nudKarmaAttribute.TabIndex = 18;
            // 
            // lblKarmaAttributeExtra
            // 
            this.lblKarmaAttributeExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaAttributeExtra.AutoSize = true;
            this.lblKarmaAttributeExtra.Location = new System.Drawing.Point(237, 214);
            this.lblKarmaAttributeExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaAttributeExtra.Name = "lblKarmaAttributeExtra";
            this.lblKarmaAttributeExtra.Size = new System.Drawing.Size(71, 13);
            this.lblKarmaAttributeExtra.TabIndex = 19;
            this.lblKarmaAttributeExtra.Tag = "Label_Options_NewRating";
            this.lblKarmaAttributeExtra.Text = "x New Rating";
            this.lblKarmaAttributeExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaAlchemicalFocus
            // 
            this.lblKarmaAlchemicalFocus.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaAlchemicalFocus.AutoSize = true;
            this.lblKarmaAlchemicalFocus.Location = new System.Drawing.Point(742, 6);
            this.lblKarmaAlchemicalFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaAlchemicalFocus.Name = "lblKarmaAlchemicalFocus";
            this.lblKarmaAlchemicalFocus.Size = new System.Drawing.Size(90, 13);
            this.lblKarmaAlchemicalFocus.TabIndex = 60;
            this.lblKarmaAlchemicalFocus.Tag = "Label_Options_AlchemicalFocus";
            this.lblKarmaAlchemicalFocus.Text = "Alchemical Focus";
            this.lblKarmaAlchemicalFocus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaAlchemicalFocus
            // 
            this.nudKarmaAlchemicalFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaAlchemicalFocus.AutoSize = true;
            this.nudKarmaAlchemicalFocus.Location = new System.Drawing.Point(838, 3);
            this.nudKarmaAlchemicalFocus.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaAlchemicalFocus.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaAlchemicalFocus.Name = "nudKarmaAlchemicalFocus";
            this.nudKarmaAlchemicalFocus.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaAlchemicalFocus.TabIndex = 61;
            // 
            // lblKarmaAlchemicalFocusExtra
            // 
            this.lblKarmaAlchemicalFocusExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaAlchemicalFocusExtra.AutoSize = true;
            this.lblKarmaAlchemicalFocusExtra.Location = new System.Drawing.Point(885, 6);
            this.lblKarmaAlchemicalFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaAlchemicalFocusExtra.Name = "lblKarmaAlchemicalFocusExtra";
            this.lblKarmaAlchemicalFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaAlchemicalFocusExtra.TabIndex = 62;
            this.lblKarmaAlchemicalFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaAlchemicalFocusExtra.Text = "x Force";
            this.lblKarmaAlchemicalFocusExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaBanishingFocus
            // 
            this.lblKarmaBanishingFocus.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaBanishingFocus.AutoSize = true;
            this.lblKarmaBanishingFocus.Location = new System.Drawing.Point(747, 32);
            this.lblKarmaBanishingFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaBanishingFocus.Name = "lblKarmaBanishingFocus";
            this.lblKarmaBanishingFocus.Size = new System.Drawing.Size(85, 13);
            this.lblKarmaBanishingFocus.TabIndex = 63;
            this.lblKarmaBanishingFocus.Tag = "Label_Options_BanishingFocus";
            this.lblKarmaBanishingFocus.Text = "Banishing Focus";
            this.lblKarmaBanishingFocus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaBanishingFocus
            // 
            this.nudKarmaBanishingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaBanishingFocus.AutoSize = true;
            this.nudKarmaBanishingFocus.Location = new System.Drawing.Point(838, 29);
            this.nudKarmaBanishingFocus.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaBanishingFocus.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaBanishingFocus.Name = "nudKarmaBanishingFocus";
            this.nudKarmaBanishingFocus.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaBanishingFocus.TabIndex = 64;
            // 
            // lblKarmaBindingFocus
            // 
            this.lblKarmaBindingFocus.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaBindingFocus.AutoSize = true;
            this.lblKarmaBindingFocus.Location = new System.Drawing.Point(758, 58);
            this.lblKarmaBindingFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaBindingFocus.Name = "lblKarmaBindingFocus";
            this.lblKarmaBindingFocus.Size = new System.Drawing.Size(74, 13);
            this.lblKarmaBindingFocus.TabIndex = 66;
            this.lblKarmaBindingFocus.Tag = "Label_Options_BindingFocus";
            this.lblKarmaBindingFocus.Text = "Binding Focus";
            this.lblKarmaBindingFocus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaBindingFocus
            // 
            this.nudKarmaBindingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaBindingFocus.AutoSize = true;
            this.nudKarmaBindingFocus.Location = new System.Drawing.Point(838, 55);
            this.nudKarmaBindingFocus.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaBindingFocus.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaBindingFocus.Name = "nudKarmaBindingFocus";
            this.nudKarmaBindingFocus.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaBindingFocus.TabIndex = 67;
            // 
            // lblKarmaBanishingFocusExtra
            // 
            this.lblKarmaBanishingFocusExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaBanishingFocusExtra.AutoSize = true;
            this.lblKarmaBanishingFocusExtra.Location = new System.Drawing.Point(885, 32);
            this.lblKarmaBanishingFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaBanishingFocusExtra.Name = "lblKarmaBanishingFocusExtra";
            this.lblKarmaBanishingFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaBanishingFocusExtra.TabIndex = 65;
            this.lblKarmaBanishingFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaBanishingFocusExtra.Text = "x Force";
            this.lblKarmaBanishingFocusExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaCenteringFocus
            // 
            this.lblKarmaCenteringFocus.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaCenteringFocus.AutoSize = true;
            this.lblKarmaCenteringFocus.Location = new System.Drawing.Point(748, 84);
            this.lblKarmaCenteringFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaCenteringFocus.Name = "lblKarmaCenteringFocus";
            this.lblKarmaCenteringFocus.Size = new System.Drawing.Size(84, 13);
            this.lblKarmaCenteringFocus.TabIndex = 69;
            this.lblKarmaCenteringFocus.Tag = "Label_Options_CenteringFocus";
            this.lblKarmaCenteringFocus.Text = "Centering Focus";
            this.lblKarmaCenteringFocus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaCenteringFocus
            // 
            this.nudKarmaCenteringFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaCenteringFocus.AutoSize = true;
            this.nudKarmaCenteringFocus.Location = new System.Drawing.Point(838, 81);
            this.nudKarmaCenteringFocus.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaCenteringFocus.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaCenteringFocus.Name = "nudKarmaCenteringFocus";
            this.nudKarmaCenteringFocus.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaCenteringFocus.TabIndex = 70;
            // 
            // lblKarmaBindingFocusExtra
            // 
            this.lblKarmaBindingFocusExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaBindingFocusExtra.AutoSize = true;
            this.lblKarmaBindingFocusExtra.Location = new System.Drawing.Point(885, 58);
            this.lblKarmaBindingFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaBindingFocusExtra.Name = "lblKarmaBindingFocusExtra";
            this.lblKarmaBindingFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaBindingFocusExtra.TabIndex = 68;
            this.lblKarmaBindingFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaBindingFocusExtra.Text = "x Force";
            this.lblKarmaBindingFocusExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaCenteringFocusExtra
            // 
            this.lblKarmaCenteringFocusExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaCenteringFocusExtra.AutoSize = true;
            this.lblKarmaCenteringFocusExtra.Location = new System.Drawing.Point(885, 84);
            this.lblKarmaCenteringFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaCenteringFocusExtra.Name = "lblKarmaCenteringFocusExtra";
            this.lblKarmaCenteringFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaCenteringFocusExtra.TabIndex = 71;
            this.lblKarmaCenteringFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaCenteringFocusExtra.Text = "x Force";
            this.lblKarmaCenteringFocusExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaCounterspellingFocus
            // 
            this.lblKarmaCounterspellingFocus.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaCounterspellingFocus.AutoSize = true;
            this.lblKarmaCounterspellingFocus.Location = new System.Drawing.Point(721, 110);
            this.lblKarmaCounterspellingFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaCounterspellingFocus.Name = "lblKarmaCounterspellingFocus";
            this.lblKarmaCounterspellingFocus.Size = new System.Drawing.Size(111, 13);
            this.lblKarmaCounterspellingFocus.TabIndex = 72;
            this.lblKarmaCounterspellingFocus.Tag = "Label_Options_CounterspellingFocus";
            this.lblKarmaCounterspellingFocus.Text = "Counterspelling Focus";
            this.lblKarmaCounterspellingFocus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaCounterspellingFocus
            // 
            this.nudKarmaCounterspellingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaCounterspellingFocus.AutoSize = true;
            this.nudKarmaCounterspellingFocus.Location = new System.Drawing.Point(838, 107);
            this.nudKarmaCounterspellingFocus.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaCounterspellingFocus.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaCounterspellingFocus.Name = "nudKarmaCounterspellingFocus";
            this.nudKarmaCounterspellingFocus.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaCounterspellingFocus.TabIndex = 73;
            // 
            // lblKarmaCounterspellingFocusExtra
            // 
            this.lblKarmaCounterspellingFocusExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaCounterspellingFocusExtra.AutoSize = true;
            this.lblKarmaCounterspellingFocusExtra.Location = new System.Drawing.Point(885, 110);
            this.lblKarmaCounterspellingFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaCounterspellingFocusExtra.Name = "lblKarmaCounterspellingFocusExtra";
            this.lblKarmaCounterspellingFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaCounterspellingFocusExtra.TabIndex = 74;
            this.lblKarmaCounterspellingFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaCounterspellingFocusExtra.Text = "x Force";
            this.lblKarmaCounterspellingFocusExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaDisenchantingFocus
            // 
            this.lblKarmaDisenchantingFocus.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaDisenchantingFocus.AutoSize = true;
            this.lblKarmaDisenchantingFocus.Location = new System.Drawing.Point(725, 136);
            this.lblKarmaDisenchantingFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaDisenchantingFocus.Name = "lblKarmaDisenchantingFocus";
            this.lblKarmaDisenchantingFocus.Size = new System.Drawing.Size(107, 13);
            this.lblKarmaDisenchantingFocus.TabIndex = 81;
            this.lblKarmaDisenchantingFocus.Tag = "Label_Options_DisenchantingFocus";
            this.lblKarmaDisenchantingFocus.Text = "Disenchanting Focus";
            this.lblKarmaDisenchantingFocus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaDisenchantingFocus
            // 
            this.nudKarmaDisenchantingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaDisenchantingFocus.AutoSize = true;
            this.nudKarmaDisenchantingFocus.Location = new System.Drawing.Point(838, 133);
            this.nudKarmaDisenchantingFocus.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaDisenchantingFocus.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaDisenchantingFocus.Name = "nudKarmaDisenchantingFocus";
            this.nudKarmaDisenchantingFocus.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaDisenchantingFocus.TabIndex = 82;
            // 
            // lblKarmaDisenchantingFocusExtra
            // 
            this.lblKarmaDisenchantingFocusExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaDisenchantingFocusExtra.AutoSize = true;
            this.lblKarmaDisenchantingFocusExtra.Location = new System.Drawing.Point(885, 136);
            this.lblKarmaDisenchantingFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaDisenchantingFocusExtra.Name = "lblKarmaDisenchantingFocusExtra";
            this.lblKarmaDisenchantingFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaDisenchantingFocusExtra.TabIndex = 83;
            this.lblKarmaDisenchantingFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaDisenchantingFocusExtra.Text = "x Force";
            this.lblKarmaDisenchantingFocusExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaFlexibleSignatureFocus
            // 
            this.lblKarmaFlexibleSignatureFocus.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaFlexibleSignatureFocus.AutoSize = true;
            this.lblKarmaFlexibleSignatureFocus.Location = new System.Drawing.Point(710, 162);
            this.lblKarmaFlexibleSignatureFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaFlexibleSignatureFocus.Name = "lblKarmaFlexibleSignatureFocus";
            this.lblKarmaFlexibleSignatureFocus.Size = new System.Drawing.Size(122, 13);
            this.lblKarmaFlexibleSignatureFocus.TabIndex = 113;
            this.lblKarmaFlexibleSignatureFocus.Tag = "Label_Options_FlexibleSignatureFocus";
            this.lblKarmaFlexibleSignatureFocus.Text = "Flexible Signature Focus";
            this.lblKarmaFlexibleSignatureFocus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaFlexibleSignatureFocus
            // 
            this.nudKarmaFlexibleSignatureFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaFlexibleSignatureFocus.AutoSize = true;
            this.nudKarmaFlexibleSignatureFocus.Location = new System.Drawing.Point(838, 159);
            this.nudKarmaFlexibleSignatureFocus.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaFlexibleSignatureFocus.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaFlexibleSignatureFocus.Name = "nudKarmaFlexibleSignatureFocus";
            this.nudKarmaFlexibleSignatureFocus.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaFlexibleSignatureFocus.TabIndex = 114;
            // 
            // lblFlexibleSignatureFocusExtra
            // 
            this.lblFlexibleSignatureFocusExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblFlexibleSignatureFocusExtra.AutoSize = true;
            this.lblFlexibleSignatureFocusExtra.Location = new System.Drawing.Point(885, 162);
            this.lblFlexibleSignatureFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblFlexibleSignatureFocusExtra.Name = "lblFlexibleSignatureFocusExtra";
            this.lblFlexibleSignatureFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblFlexibleSignatureFocusExtra.TabIndex = 115;
            this.lblFlexibleSignatureFocusExtra.Tag = "Label_Options_Force";
            this.lblFlexibleSignatureFocusExtra.Text = "x Force";
            this.lblFlexibleSignatureFocusExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaMaskingFocus
            // 
            this.lblKarmaMaskingFocus.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaMaskingFocus.AutoSize = true;
            this.lblKarmaMaskingFocus.Location = new System.Drawing.Point(753, 188);
            this.lblKarmaMaskingFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaMaskingFocus.Name = "lblKarmaMaskingFocus";
            this.lblKarmaMaskingFocus.Size = new System.Drawing.Size(79, 13);
            this.lblKarmaMaskingFocus.TabIndex = 84;
            this.lblKarmaMaskingFocus.Tag = "Label_Options_MaskingFocus";
            this.lblKarmaMaskingFocus.Text = "Masking Focus";
            this.lblKarmaMaskingFocus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaMaskingFocus
            // 
            this.nudKarmaMaskingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaMaskingFocus.AutoSize = true;
            this.nudKarmaMaskingFocus.Location = new System.Drawing.Point(838, 185);
            this.nudKarmaMaskingFocus.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaMaskingFocus.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaMaskingFocus.Name = "nudKarmaMaskingFocus";
            this.nudKarmaMaskingFocus.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaMaskingFocus.TabIndex = 85;
            // 
            // lblKarmaMaskingFocusExtra
            // 
            this.lblKarmaMaskingFocusExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaMaskingFocusExtra.AutoSize = true;
            this.lblKarmaMaskingFocusExtra.Location = new System.Drawing.Point(885, 188);
            this.lblKarmaMaskingFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaMaskingFocusExtra.Name = "lblKarmaMaskingFocusExtra";
            this.lblKarmaMaskingFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaMaskingFocusExtra.TabIndex = 86;
            this.lblKarmaMaskingFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaMaskingFocusExtra.Text = "x Force";
            this.lblKarmaMaskingFocusExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaPowerFocus
            // 
            this.lblKarmaPowerFocus.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaPowerFocus.AutoSize = true;
            this.lblKarmaPowerFocus.Location = new System.Drawing.Point(763, 214);
            this.lblKarmaPowerFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaPowerFocus.Name = "lblKarmaPowerFocus";
            this.lblKarmaPowerFocus.Size = new System.Drawing.Size(69, 13);
            this.lblKarmaPowerFocus.TabIndex = 87;
            this.lblKarmaPowerFocus.Tag = "Label_Options_PowerFocus";
            this.lblKarmaPowerFocus.Text = "Power Focus";
            this.lblKarmaPowerFocus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaPowerFocus
            // 
            this.nudKarmaPowerFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaPowerFocus.AutoSize = true;
            this.nudKarmaPowerFocus.Location = new System.Drawing.Point(838, 211);
            this.nudKarmaPowerFocus.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaPowerFocus.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaPowerFocus.Name = "nudKarmaPowerFocus";
            this.nudKarmaPowerFocus.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaPowerFocus.TabIndex = 88;
            // 
            // lblKarmaPowerFocusExtra
            // 
            this.lblKarmaPowerFocusExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaPowerFocusExtra.AutoSize = true;
            this.lblKarmaPowerFocusExtra.Location = new System.Drawing.Point(885, 214);
            this.lblKarmaPowerFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaPowerFocusExtra.Name = "lblKarmaPowerFocusExtra";
            this.lblKarmaPowerFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaPowerFocusExtra.TabIndex = 89;
            this.lblKarmaPowerFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaPowerFocusExtra.Text = "x Force";
            this.lblKarmaPowerFocusExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaQiFocus
            // 
            this.lblKarmaQiFocus.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaQiFocus.AutoSize = true;
            this.lblKarmaQiFocus.Location = new System.Drawing.Point(783, 240);
            this.lblKarmaQiFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaQiFocus.Name = "lblKarmaQiFocus";
            this.lblKarmaQiFocus.Size = new System.Drawing.Size(49, 13);
            this.lblKarmaQiFocus.TabIndex = 90;
            this.lblKarmaQiFocus.Tag = "Label_Options_QiFocus";
            this.lblKarmaQiFocus.Text = "Qi Focus";
            this.lblKarmaQiFocus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaQiFocus
            // 
            this.nudKarmaQiFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaQiFocus.AutoSize = true;
            this.nudKarmaQiFocus.Location = new System.Drawing.Point(838, 237);
            this.nudKarmaQiFocus.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaQiFocus.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaQiFocus.Name = "nudKarmaQiFocus";
            this.nudKarmaQiFocus.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaQiFocus.TabIndex = 91;
            // 
            // lblKarmaQiFocusExtra
            // 
            this.lblKarmaQiFocusExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaQiFocusExtra.AutoSize = true;
            this.lblKarmaQiFocusExtra.Location = new System.Drawing.Point(885, 240);
            this.lblKarmaQiFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaQiFocusExtra.Name = "lblKarmaQiFocusExtra";
            this.lblKarmaQiFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaQiFocusExtra.TabIndex = 92;
            this.lblKarmaQiFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaQiFocusExtra.Text = "x Force";
            this.lblKarmaQiFocusExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaRitualSpellcastingFocus
            // 
            this.lblKarmaRitualSpellcastingFocus.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaRitualSpellcastingFocus.AutoSize = true;
            this.lblKarmaRitualSpellcastingFocus.Location = new System.Drawing.Point(706, 266);
            this.lblKarmaRitualSpellcastingFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaRitualSpellcastingFocus.Name = "lblKarmaRitualSpellcastingFocus";
            this.lblKarmaRitualSpellcastingFocus.Size = new System.Drawing.Size(126, 13);
            this.lblKarmaRitualSpellcastingFocus.TabIndex = 116;
            this.lblKarmaRitualSpellcastingFocus.Tag = "Label_Options_RitualSpellcastingFocus";
            this.lblKarmaRitualSpellcastingFocus.Text = "Ritual Spellcasting Focus";
            this.lblKarmaRitualSpellcastingFocus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaRitualSpellcastingFocus
            // 
            this.nudKarmaRitualSpellcastingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaRitualSpellcastingFocus.AutoSize = true;
            this.nudKarmaRitualSpellcastingFocus.Location = new System.Drawing.Point(838, 263);
            this.nudKarmaRitualSpellcastingFocus.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaRitualSpellcastingFocus.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaRitualSpellcastingFocus.Name = "nudKarmaRitualSpellcastingFocus";
            this.nudKarmaRitualSpellcastingFocus.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaRitualSpellcastingFocus.TabIndex = 117;
            // 
            // lblKarmaRitualSpellcastingFocusExtra
            // 
            this.lblKarmaRitualSpellcastingFocusExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaRitualSpellcastingFocusExtra.AutoSize = true;
            this.lblKarmaRitualSpellcastingFocusExtra.Location = new System.Drawing.Point(885, 266);
            this.lblKarmaRitualSpellcastingFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaRitualSpellcastingFocusExtra.Name = "lblKarmaRitualSpellcastingFocusExtra";
            this.lblKarmaRitualSpellcastingFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaRitualSpellcastingFocusExtra.TabIndex = 118;
            this.lblKarmaRitualSpellcastingFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaRitualSpellcastingFocusExtra.Text = "x Force";
            this.lblKarmaRitualSpellcastingFocusExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaSpellcastingFocus
            // 
            this.lblKarmaSpellcastingFocus.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaSpellcastingFocus.AutoSize = true;
            this.lblKarmaSpellcastingFocus.Location = new System.Drawing.Point(736, 292);
            this.lblKarmaSpellcastingFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSpellcastingFocus.Name = "lblKarmaSpellcastingFocus";
            this.lblKarmaSpellcastingFocus.Size = new System.Drawing.Size(96, 13);
            this.lblKarmaSpellcastingFocus.TabIndex = 93;
            this.lblKarmaSpellcastingFocus.Tag = "Label_Options_SpellcastingFocus";
            this.lblKarmaSpellcastingFocus.Text = "Spellcasting Focus";
            this.lblKarmaSpellcastingFocus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaSpellcastingFocus
            // 
            this.nudKarmaSpellcastingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaSpellcastingFocus.AutoSize = true;
            this.nudKarmaSpellcastingFocus.Location = new System.Drawing.Point(838, 289);
            this.nudKarmaSpellcastingFocus.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaSpellcastingFocus.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaSpellcastingFocus.Name = "nudKarmaSpellcastingFocus";
            this.nudKarmaSpellcastingFocus.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaSpellcastingFocus.TabIndex = 94;
            // 
            // lblKarmaSpellcastingFocusExtra
            // 
            this.lblKarmaSpellcastingFocusExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaSpellcastingFocusExtra.AutoSize = true;
            this.lblKarmaSpellcastingFocusExtra.Location = new System.Drawing.Point(885, 292);
            this.lblKarmaSpellcastingFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSpellcastingFocusExtra.Name = "lblKarmaSpellcastingFocusExtra";
            this.lblKarmaSpellcastingFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaSpellcastingFocusExtra.TabIndex = 95;
            this.lblKarmaSpellcastingFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaSpellcastingFocusExtra.Text = "x Force";
            this.lblKarmaSpellcastingFocusExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaSpell
            // 
            this.lblKarmaSpell.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaSpell.AutoSize = true;
            this.lblKarmaSpell.Location = new System.Drawing.Point(462, 58);
            this.lblKarmaSpell.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSpell.Name = "lblKarmaSpell";
            this.lblKarmaSpell.Size = new System.Drawing.Size(55, 13);
            this.lblKarmaSpell.TabIndex = 23;
            this.lblKarmaSpell.Tag = "Label_Options_NewSpell";
            this.lblKarmaSpell.Text = "New Spell";
            this.lblKarmaSpell.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaSpell
            // 
            this.nudKarmaSpell.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaSpell.AutoSize = true;
            this.nudKarmaSpell.Location = new System.Drawing.Point(523, 55);
            this.nudKarmaSpell.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaSpell.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaSpell.Name = "nudKarmaSpell";
            this.nudKarmaSpell.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaSpell.TabIndex = 24;
            // 
            // lblKarmaContact
            // 
            this.lblKarmaContact.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaContact.AutoSize = true;
            this.lblKarmaContact.Location = new System.Drawing.Point(129, 266);
            this.lblKarmaContact.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaContact.Name = "lblKarmaContact";
            this.lblKarmaContact.Size = new System.Drawing.Size(49, 13);
            this.lblKarmaContact.TabIndex = 44;
            this.lblKarmaContact.Tag = "Label_Options_Contacts";
            this.lblKarmaContact.Text = "Contacts";
            this.lblKarmaContact.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaContact
            // 
            this.nudKarmaContact.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaContact.AutoSize = true;
            this.nudKarmaContact.Location = new System.Drawing.Point(184, 263);
            this.nudKarmaContact.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaContact.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaContact.Name = "nudKarmaContact";
            this.nudKarmaContact.Size = new System.Drawing.Size(47, 20);
            this.nudKarmaContact.TabIndex = 45;
            // 
            // lblKarmaContactExtra
            // 
            this.lblKarmaContactExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaContactExtra.AutoSize = true;
            this.lblKarmaContactExtra.Location = new System.Drawing.Point(237, 266);
            this.lblKarmaContactExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaContactExtra.Name = "lblKarmaContactExtra";
            this.lblKarmaContactExtra.Size = new System.Drawing.Size(120, 13);
            this.lblKarmaContactExtra.TabIndex = 46;
            this.lblKarmaContactExtra.Tag = "Label_Options_ConnectionLoyalty";
            this.lblKarmaContactExtra.Text = "x (Connection + Loyalty)";
            this.lblKarmaContactExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaEnemyExtra
            // 
            this.lblKarmaEnemyExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaEnemyExtra.AutoSize = true;
            this.lblKarmaEnemyExtra.Location = new System.Drawing.Point(237, 292);
            this.lblKarmaEnemyExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaEnemyExtra.Name = "lblKarmaEnemyExtra";
            this.lblKarmaEnemyExtra.Size = new System.Drawing.Size(120, 13);
            this.lblKarmaEnemyExtra.TabIndex = 49;
            this.lblKarmaEnemyExtra.Tag = "Label_Options_ConnectionLoyalty";
            this.lblKarmaEnemyExtra.Text = "x (Connection + Loyalty)";
            this.lblKarmaEnemyExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // nudKarmaEnemy
            // 
            this.nudKarmaEnemy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaEnemy.AutoSize = true;
            this.nudKarmaEnemy.Location = new System.Drawing.Point(184, 289);
            this.nudKarmaEnemy.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaEnemy.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaEnemy.Name = "nudKarmaEnemy";
            this.nudKarmaEnemy.Size = new System.Drawing.Size(47, 20);
            this.nudKarmaEnemy.TabIndex = 48;
            // 
            // lblKarmaEnemy
            // 
            this.lblKarmaEnemy.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaEnemy.AutoSize = true;
            this.lblKarmaEnemy.Location = new System.Drawing.Point(131, 292);
            this.lblKarmaEnemy.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaEnemy.Name = "lblKarmaEnemy";
            this.lblKarmaEnemy.Size = new System.Drawing.Size(47, 13);
            this.lblKarmaEnemy.TabIndex = 47;
            this.lblKarmaEnemy.Tag = "Label_Options_Enemies";
            this.lblKarmaEnemy.Text = "Enemies";
            this.lblKarmaEnemy.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblKarmaSpirit
            // 
            this.lblKarmaSpirit.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaSpirit.AutoSize = true;
            this.lblKarmaSpirit.Location = new System.Drawing.Point(487, 84);
            this.lblKarmaSpirit.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSpirit.Name = "lblKarmaSpirit";
            this.lblKarmaSpirit.Size = new System.Drawing.Size(30, 13);
            this.lblKarmaSpirit.TabIndex = 36;
            this.lblKarmaSpirit.Tag = "Label_Options_Spirit";
            this.lblKarmaSpirit.Text = "Spirit";
            this.lblKarmaSpirit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaSpirit
            // 
            this.nudKarmaSpirit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaSpirit.AutoSize = true;
            this.nudKarmaSpirit.Location = new System.Drawing.Point(523, 81);
            this.nudKarmaSpirit.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaSpirit.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaSpirit.Name = "nudKarmaSpirit";
            this.nudKarmaSpirit.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaSpirit.TabIndex = 37;
            // 
            // lblKarmaSpiritExtra
            // 
            this.lblKarmaSpiritExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaSpiritExtra.AutoSize = true;
            this.tlpKarmaCosts.SetColumnSpan(this.lblKarmaSpiritExtra, 2);
            this.lblKarmaSpiritExtra.Location = new System.Drawing.Point(570, 84);
            this.lblKarmaSpiritExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSpiritExtra.Name = "lblKarmaSpiritExtra";
            this.lblKarmaSpiritExtra.Size = new System.Drawing.Size(87, 13);
            this.lblKarmaSpiritExtra.TabIndex = 38;
            this.lblKarmaSpiritExtra.Tag = "Label_Options_ServicesOwed";
            this.lblKarmaSpiritExtra.Text = "x Services Owed";
            this.lblKarmaSpiritExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaNewComplexForm
            // 
            this.lblKarmaNewComplexForm.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaNewComplexForm.AutoSize = true;
            this.lblKarmaNewComplexForm.Location = new System.Drawing.Point(419, 136);
            this.lblKarmaNewComplexForm.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaNewComplexForm.Name = "lblKarmaNewComplexForm";
            this.lblKarmaNewComplexForm.Size = new System.Drawing.Size(98, 13);
            this.lblKarmaNewComplexForm.TabIndex = 25;
            this.lblKarmaNewComplexForm.Tag = "Label_Options_NewComplexForm";
            this.lblKarmaNewComplexForm.Text = "New Complex Form";
            this.lblKarmaNewComplexForm.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaNewComplexForm
            // 
            this.nudKarmaNewComplexForm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaNewComplexForm.AutoSize = true;
            this.nudKarmaNewComplexForm.Location = new System.Drawing.Point(523, 133);
            this.nudKarmaNewComplexForm.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaNewComplexForm.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaNewComplexForm.Name = "nudKarmaNewComplexForm";
            this.nudKarmaNewComplexForm.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaNewComplexForm.TabIndex = 26;
            // 
            // lblKarmaMysticAdeptPowerPoint
            // 
            this.lblKarmaMysticAdeptPowerPoint.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaMysticAdeptPowerPoint.AutoSize = true;
            this.lblKarmaMysticAdeptPowerPoint.Location = new System.Drawing.Point(389, 110);
            this.lblKarmaMysticAdeptPowerPoint.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaMysticAdeptPowerPoint.Name = "lblKarmaMysticAdeptPowerPoint";
            this.lblKarmaMysticAdeptPowerPoint.Size = new System.Drawing.Size(128, 13);
            this.lblKarmaMysticAdeptPowerPoint.TabIndex = 122;
            this.lblKarmaMysticAdeptPowerPoint.Tag = "Label_Options_KarmaMysticAdeptPowerPoint";
            this.lblKarmaMysticAdeptPowerPoint.Text = "Mystic Adept Power Point";
            this.lblKarmaMysticAdeptPowerPoint.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaMysticAdeptPowerPoint
            // 
            this.nudKarmaMysticAdeptPowerPoint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaMysticAdeptPowerPoint.AutoSize = true;
            this.nudKarmaMysticAdeptPowerPoint.Location = new System.Drawing.Point(523, 107);
            this.nudKarmaMysticAdeptPowerPoint.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaMysticAdeptPowerPoint.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaMysticAdeptPowerPoint.Name = "nudKarmaMysticAdeptPowerPoint";
            this.nudKarmaMysticAdeptPowerPoint.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaMysticAdeptPowerPoint.TabIndex = 123;
            // 
            // lblKarmaNuyenPer
            // 
            this.lblKarmaNuyenPer.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaNuyenPer.AutoSize = true;
            this.lblKarmaNuyenPer.Location = new System.Drawing.Point(140, 318);
            this.lblKarmaNuyenPer.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaNuyenPer.Name = "lblKarmaNuyenPer";
            this.lblKarmaNuyenPer.Size = new System.Drawing.Size(38, 13);
            this.lblKarmaNuyenPer.TabIndex = 41;
            this.lblKarmaNuyenPer.Tag = "Label_Options_Nuyen";
            this.lblKarmaNuyenPer.Text = "Nuyen";
            this.lblKarmaNuyenPer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaNuyenPer
            // 
            this.nudKarmaNuyenPer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaNuyenPer.AutoSize = true;
            this.nudKarmaNuyenPer.Location = new System.Drawing.Point(184, 315);
            this.nudKarmaNuyenPer.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.nudKarmaNuyenPer.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaNuyenPer.Name = "nudKarmaNuyenPer";
            this.nudKarmaNuyenPer.Size = new System.Drawing.Size(47, 20);
            this.nudKarmaNuyenPer.TabIndex = 42;
            // 
            // lblKarmaNuyenPerExtra
            // 
            this.lblKarmaNuyenPerExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaNuyenPerExtra.AutoSize = true;
            this.lblKarmaNuyenPerExtra.Location = new System.Drawing.Point(237, 318);
            this.lblKarmaNuyenPerExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaNuyenPerExtra.Name = "lblKarmaNuyenPerExtra";
            this.lblKarmaNuyenPerExtra.Size = new System.Drawing.Size(55, 13);
            this.lblKarmaNuyenPerExtra.TabIndex = 43;
            this.lblKarmaNuyenPerExtra.Tag = "Label_Options_PerKarma";
            this.lblKarmaNuyenPerExtra.Text = "per Karma";
            this.lblKarmaNuyenPerExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaJoinGroup
            // 
            this.lblKarmaJoinGroup.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaJoinGroup.AutoSize = true;
            this.lblKarmaJoinGroup.Location = new System.Drawing.Point(414, 6);
            this.lblKarmaJoinGroup.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaJoinGroup.Name = "lblKarmaJoinGroup";
            this.lblKarmaJoinGroup.Size = new System.Drawing.Size(103, 13);
            this.lblKarmaJoinGroup.TabIndex = 56;
            this.lblKarmaJoinGroup.Tag = "Label_Options_JoinGroup";
            this.lblKarmaJoinGroup.Text = "Join Group/Network";
            this.lblKarmaJoinGroup.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaJoinGroup
            // 
            this.nudKarmaJoinGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaJoinGroup.AutoSize = true;
            this.nudKarmaJoinGroup.Location = new System.Drawing.Point(523, 3);
            this.nudKarmaJoinGroup.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaJoinGroup.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaJoinGroup.Name = "nudKarmaJoinGroup";
            this.nudKarmaJoinGroup.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaJoinGroup.TabIndex = 57;
            // 
            // lblKarmaNewAIProgram
            // 
            this.lblKarmaNewAIProgram.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaNewAIProgram.AutoSize = true;
            this.lblKarmaNewAIProgram.Location = new System.Drawing.Point(427, 162);
            this.lblKarmaNewAIProgram.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaNewAIProgram.Name = "lblKarmaNewAIProgram";
            this.lblKarmaNewAIProgram.Size = new System.Drawing.Size(90, 13);
            this.lblKarmaNewAIProgram.TabIndex = 109;
            this.lblKarmaNewAIProgram.Tag = "Label_Options_NewAIProgram";
            this.lblKarmaNewAIProgram.Text = "New Program (AI)";
            this.lblKarmaNewAIProgram.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaNewAIProgram
            // 
            this.nudKarmaNewAIProgram.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaNewAIProgram.AutoSize = true;
            this.nudKarmaNewAIProgram.Location = new System.Drawing.Point(523, 159);
            this.nudKarmaNewAIProgram.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaNewAIProgram.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaNewAIProgram.Name = "nudKarmaNewAIProgram";
            this.nudKarmaNewAIProgram.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaNewAIProgram.TabIndex = 111;
            // 
            // lblKarmaNewAIAdvancedProgram
            // 
            this.lblKarmaNewAIAdvancedProgram.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaNewAIAdvancedProgram.AutoSize = true;
            this.lblKarmaNewAIAdvancedProgram.Location = new System.Drawing.Point(375, 188);
            this.lblKarmaNewAIAdvancedProgram.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaNewAIAdvancedProgram.Name = "lblKarmaNewAIAdvancedProgram";
            this.lblKarmaNewAIAdvancedProgram.Size = new System.Drawing.Size(142, 13);
            this.lblKarmaNewAIAdvancedProgram.TabIndex = 110;
            this.lblKarmaNewAIAdvancedProgram.Tag = "Label_Options_NewAIAdvancedProgram";
            this.lblKarmaNewAIAdvancedProgram.Text = "New Advanced Program (AI)";
            this.lblKarmaNewAIAdvancedProgram.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaNewAIAdvancedProgram
            // 
            this.nudKarmaNewAIAdvancedProgram.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaNewAIAdvancedProgram.AutoSize = true;
            this.nudKarmaNewAIAdvancedProgram.Location = new System.Drawing.Point(523, 185);
            this.nudKarmaNewAIAdvancedProgram.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaNewAIAdvancedProgram.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaNewAIAdvancedProgram.Name = "nudKarmaNewAIAdvancedProgram";
            this.nudKarmaNewAIAdvancedProgram.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaNewAIAdvancedProgram.TabIndex = 112;
            // 
            // lblKarmaMetamagic
            // 
            this.lblKarmaMetamagic.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaMetamagic.AutoSize = true;
            this.lblKarmaMetamagic.Location = new System.Drawing.Point(363, 240);
            this.lblKarmaMetamagic.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaMetamagic.Name = "lblKarmaMetamagic";
            this.lblKarmaMetamagic.Size = new System.Drawing.Size(154, 13);
            this.lblKarmaMetamagic.TabIndex = 57;
            this.lblKarmaMetamagic.Tag = "Label_Options_Metamagics";
            this.lblKarmaMetamagic.Text = "Additional Metamagics/Echoes";
            this.lblKarmaMetamagic.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaMetamagic
            // 
            this.nudKarmaMetamagic.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaMetamagic.AutoSize = true;
            this.nudKarmaMetamagic.Location = new System.Drawing.Point(523, 237);
            this.nudKarmaMetamagic.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaMetamagic.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaMetamagic.Name = "nudKarmaMetamagic";
            this.nudKarmaMetamagic.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaMetamagic.TabIndex = 58;
            // 
            // lblKarmaTechnique
            // 
            this.lblKarmaTechnique.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaTechnique.AutoSize = true;
            this.lblKarmaTechnique.Location = new System.Drawing.Point(409, 266);
            this.lblKarmaTechnique.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaTechnique.Name = "lblKarmaTechnique";
            this.lblKarmaTechnique.Size = new System.Drawing.Size(108, 13);
            this.lblKarmaTechnique.TabIndex = 39;
            this.lblKarmaTechnique.Tag = "Label_Options_MartialArtTechnique";
            this.lblKarmaTechnique.Text = "Martial Art Technique";
            this.lblKarmaTechnique.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaTechnique
            // 
            this.nudKarmaTechnique.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaTechnique.AutoSize = true;
            this.nudKarmaTechnique.Location = new System.Drawing.Point(523, 263);
            this.nudKarmaTechnique.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaTechnique.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaTechnique.Name = "nudKarmaTechnique";
            this.nudKarmaTechnique.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaTechnique.TabIndex = 40;
            // 
            // flpKarmaInitiation
            // 
            this.flpKarmaInitiation.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.flpKarmaInitiation.AutoSize = true;
            this.flpKarmaInitiation.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpKarmaInitiation.Controls.Add(this.lblKarmaInitiation);
            this.flpKarmaInitiation.Controls.Add(this.lblKarmaInitiationBracket);
            this.flpKarmaInitiation.Location = new System.Drawing.Point(386, 208);
            this.flpKarmaInitiation.Margin = new System.Windows.Forms.Padding(0);
            this.flpKarmaInitiation.Name = "flpKarmaInitiation";
            this.flpKarmaInitiation.Size = new System.Drawing.Size(134, 25);
            this.flpKarmaInitiation.TabIndex = 128;
            this.flpKarmaInitiation.WrapContents = false;
            // 
            // lblKarmaInitiation
            // 
            this.lblKarmaInitiation.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaInitiation.AutoSize = true;
            this.lblKarmaInitiation.Location = new System.Drawing.Point(3, 6);
            this.lblKarmaInitiation.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaInitiation.Name = "lblKarmaInitiation";
            this.lblKarmaInitiation.Size = new System.Drawing.Size(112, 13);
            this.lblKarmaInitiation.TabIndex = 53;
            this.lblKarmaInitiation.Tag = "Label_Options_Initiation";
            this.lblKarmaInitiation.Text = "Initiation / Submersion";
            this.lblKarmaInitiation.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblKarmaInitiationBracket
            // 
            this.lblKarmaInitiationBracket.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaInitiationBracket.AutoSize = true;
            this.lblKarmaInitiationBracket.Location = new System.Drawing.Point(121, 6);
            this.lblKarmaInitiationBracket.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaInitiationBracket.Name = "lblKarmaInitiationBracket";
            this.lblKarmaInitiationBracket.Size = new System.Drawing.Size(10, 13);
            this.lblKarmaInitiationBracket.TabIndex = 54;
            this.lblKarmaInitiationBracket.Text = "(";
            this.lblKarmaInitiationBracket.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaInitiation
            // 
            this.nudKarmaInitiation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaInitiation.AutoSize = true;
            this.nudKarmaInitiation.Location = new System.Drawing.Point(523, 211);
            this.nudKarmaInitiation.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaInitiation.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaInitiation.Name = "nudKarmaInitiation";
            this.nudKarmaInitiation.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaInitiation.TabIndex = 55;
            // 
            // lblKarmaInitiationExtra
            // 
            this.lblKarmaInitiationExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaInitiationExtra.AutoSize = true;
            this.lblKarmaInitiationExtra.Location = new System.Drawing.Point(570, 214);
            this.lblKarmaInitiationExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaInitiationExtra.Name = "lblKarmaInitiationExtra";
            this.lblKarmaInitiationExtra.Size = new System.Drawing.Size(83, 13);
            this.lblKarmaInitiationExtra.TabIndex = 56;
            this.lblKarmaInitiationExtra.Tag = "Label_Options_NewRatingPlus";
            this.lblKarmaInitiationExtra.Text = "x New Rating) +";
            this.lblKarmaInitiationExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // nudKarmaInitiationFlat
            // 
            this.nudKarmaInitiationFlat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaInitiationFlat.AutoSize = true;
            this.nudKarmaInitiationFlat.Location = new System.Drawing.Point(659, 211);
            this.nudKarmaInitiationFlat.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaInitiationFlat.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaInitiationFlat.Name = "nudKarmaInitiationFlat";
            this.nudKarmaInitiationFlat.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaInitiationFlat.TabIndex = 121;
            // 
            // lblKarmaCarryover
            // 
            this.lblKarmaCarryover.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaCarryover.AutoSize = true;
            this.lblKarmaCarryover.Location = new System.Drawing.Point(37, 240);
            this.lblKarmaCarryover.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaCarryover.Name = "lblKarmaCarryover";
            this.lblKarmaCarryover.Size = new System.Drawing.Size(141, 13);
            this.lblKarmaCarryover.TabIndex = 50;
            this.lblKarmaCarryover.Tag = "Label_Options_Carryover";
            this.lblKarmaCarryover.Text = "Carryover for New Character";
            this.lblKarmaCarryover.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaCarryover
            // 
            this.nudKarmaCarryover.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaCarryover.AutoSize = true;
            this.nudKarmaCarryover.Location = new System.Drawing.Point(184, 237);
            this.nudKarmaCarryover.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaCarryover.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaCarryover.Name = "nudKarmaCarryover";
            this.nudKarmaCarryover.Size = new System.Drawing.Size(47, 20);
            this.nudKarmaCarryover.TabIndex = 51;
            // 
            // lblKarmaCarryoverExtra
            // 
            this.lblKarmaCarryoverExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaCarryoverExtra.AutoSize = true;
            this.lblKarmaCarryoverExtra.Location = new System.Drawing.Point(237, 240);
            this.lblKarmaCarryoverExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaCarryoverExtra.Name = "lblKarmaCarryoverExtra";
            this.lblKarmaCarryoverExtra.Size = new System.Drawing.Size(51, 13);
            this.lblKarmaCarryoverExtra.TabIndex = 52;
            this.lblKarmaCarryoverExtra.Tag = "Label_Options_Maximum";
            this.lblKarmaCarryoverExtra.Text = "Maximum";
            this.lblKarmaCarryoverExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaQuality
            // 
            this.lblKarmaQuality.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaQuality.AutoSize = true;
            this.lblKarmaQuality.Location = new System.Drawing.Point(45, 344);
            this.lblKarmaQuality.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaQuality.Name = "lblKarmaQuality";
            this.lblKarmaQuality.Size = new System.Drawing.Size(133, 13);
            this.lblKarmaQuality.TabIndex = 20;
            this.lblKarmaQuality.Tag = "Label_Options_Qualities";
            this.lblKarmaQuality.Text = "Positive / Negative Quality";
            this.lblKarmaQuality.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaQuality
            // 
            this.nudKarmaQuality.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaQuality.AutoSize = true;
            this.nudKarmaQuality.Location = new System.Drawing.Point(184, 341);
            this.nudKarmaQuality.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaQuality.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaQuality.Name = "nudKarmaQuality";
            this.nudKarmaQuality.Size = new System.Drawing.Size(47, 20);
            this.nudKarmaQuality.TabIndex = 21;
            // 
            // lblKarmaQualityExtra
            // 
            this.lblKarmaQualityExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaQualityExtra.AutoSize = true;
            this.lblKarmaQualityExtra.Location = new System.Drawing.Point(237, 344);
            this.lblKarmaQualityExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaQualityExtra.Name = "lblKarmaQualityExtra";
            this.lblKarmaQualityExtra.Size = new System.Drawing.Size(53, 13);
            this.lblKarmaQualityExtra.TabIndex = 22;
            this.lblKarmaQualityExtra.Tag = "Label_Options_BPCost";
            this.lblKarmaQualityExtra.Text = "x BP Cost";
            this.lblKarmaQualityExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaSummoningFocus
            // 
            this.lblKarmaSummoningFocus.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaSummoningFocus.AutoSize = true;
            this.lblKarmaSummoningFocus.Location = new System.Drawing.Point(738, 344);
            this.lblKarmaSummoningFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSummoningFocus.Name = "lblKarmaSummoningFocus";
            this.lblKarmaSummoningFocus.Size = new System.Drawing.Size(94, 13);
            this.lblKarmaSummoningFocus.TabIndex = 96;
            this.lblKarmaSummoningFocus.Tag = "Label_Options_SummoningFocus";
            this.lblKarmaSummoningFocus.Text = "Summoning Focus";
            this.lblKarmaSummoningFocus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaSummoningFocus
            // 
            this.nudKarmaSummoningFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaSummoningFocus.AutoSize = true;
            this.nudKarmaSummoningFocus.Location = new System.Drawing.Point(838, 341);
            this.nudKarmaSummoningFocus.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaSummoningFocus.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaSummoningFocus.Name = "nudKarmaSummoningFocus";
            this.nudKarmaSummoningFocus.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaSummoningFocus.TabIndex = 97;
            // 
            // lblKarmaSpellShapingFocus
            // 
            this.lblKarmaSpellShapingFocus.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaSpellShapingFocus.AutoSize = true;
            this.lblKarmaSpellShapingFocus.Location = new System.Drawing.Point(728, 318);
            this.lblKarmaSpellShapingFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSpellShapingFocus.Name = "lblKarmaSpellShapingFocus";
            this.lblKarmaSpellShapingFocus.Size = new System.Drawing.Size(104, 13);
            this.lblKarmaSpellShapingFocus.TabIndex = 102;
            this.lblKarmaSpellShapingFocus.Tag = "Label_Options_SpellShapingFocus";
            this.lblKarmaSpellShapingFocus.Text = "Spell Shaping Focus";
            this.lblKarmaSpellShapingFocus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblKarmaSpellShapingFocusExtra
            // 
            this.lblKarmaSpellShapingFocusExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaSpellShapingFocusExtra.AutoSize = true;
            this.lblKarmaSpellShapingFocusExtra.Location = new System.Drawing.Point(885, 318);
            this.lblKarmaSpellShapingFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSpellShapingFocusExtra.Name = "lblKarmaSpellShapingFocusExtra";
            this.lblKarmaSpellShapingFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaSpellShapingFocusExtra.TabIndex = 104;
            this.lblKarmaSpellShapingFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaSpellShapingFocusExtra.Text = "x Force";
            this.lblKarmaSpellShapingFocusExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // nudKarmaSpellShapingFocus
            // 
            this.nudKarmaSpellShapingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaSpellShapingFocus.AutoSize = true;
            this.nudKarmaSpellShapingFocus.Location = new System.Drawing.Point(838, 315);
            this.nudKarmaSpellShapingFocus.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaSpellShapingFocus.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaSpellShapingFocus.Name = "nudKarmaSpellShapingFocus";
            this.nudKarmaSpellShapingFocus.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaSpellShapingFocus.TabIndex = 103;
            // 
            // lblKarmaSummoningFocusExtra
            // 
            this.lblKarmaSummoningFocusExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaSummoningFocusExtra.AutoSize = true;
            this.lblKarmaSummoningFocusExtra.Location = new System.Drawing.Point(885, 344);
            this.lblKarmaSummoningFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSummoningFocusExtra.Name = "lblKarmaSummoningFocusExtra";
            this.lblKarmaSummoningFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaSummoningFocusExtra.TabIndex = 98;
            this.lblKarmaSummoningFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaSummoningFocusExtra.Text = "x Force";
            this.lblKarmaSummoningFocusExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaSustainingFocus
            // 
            this.lblKarmaSustainingFocus.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaSustainingFocus.AutoSize = true;
            this.lblKarmaSustainingFocus.Location = new System.Drawing.Point(744, 370);
            this.lblKarmaSustainingFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSustainingFocus.Name = "lblKarmaSustainingFocus";
            this.lblKarmaSustainingFocus.Size = new System.Drawing.Size(88, 13);
            this.lblKarmaSustainingFocus.TabIndex = 99;
            this.lblKarmaSustainingFocus.Tag = "Label_Options_SustainingFocus";
            this.lblKarmaSustainingFocus.Text = "Sustaining Focus";
            this.lblKarmaSustainingFocus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblKarmaWeaponFocus
            // 
            this.lblKarmaWeaponFocus.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaWeaponFocus.AutoSize = true;
            this.lblKarmaWeaponFocus.Location = new System.Drawing.Point(752, 396);
            this.lblKarmaWeaponFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaWeaponFocus.Name = "lblKarmaWeaponFocus";
            this.lblKarmaWeaponFocus.Size = new System.Drawing.Size(80, 13);
            this.lblKarmaWeaponFocus.TabIndex = 105;
            this.lblKarmaWeaponFocus.Tag = "Label_Options_WeaponFocus";
            this.lblKarmaWeaponFocus.Text = "Weapon Focus";
            this.lblKarmaWeaponFocus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblKarmaSustainingFocusExtra
            // 
            this.lblKarmaSustainingFocusExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaSustainingFocusExtra.AutoSize = true;
            this.lblKarmaSustainingFocusExtra.Location = new System.Drawing.Point(885, 370);
            this.lblKarmaSustainingFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSustainingFocusExtra.Name = "lblKarmaSustainingFocusExtra";
            this.lblKarmaSustainingFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaSustainingFocusExtra.TabIndex = 101;
            this.lblKarmaSustainingFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaSustainingFocusExtra.Text = "x Force";
            this.lblKarmaSustainingFocusExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaWeaponFocusExtra
            // 
            this.lblKarmaWeaponFocusExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaWeaponFocusExtra.AutoSize = true;
            this.lblKarmaWeaponFocusExtra.Location = new System.Drawing.Point(885, 396);
            this.lblKarmaWeaponFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaWeaponFocusExtra.Name = "lblKarmaWeaponFocusExtra";
            this.lblKarmaWeaponFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaWeaponFocusExtra.TabIndex = 107;
            this.lblKarmaWeaponFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaWeaponFocusExtra.Text = "x Force";
            this.lblKarmaWeaponFocusExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // nudKarmaSustainingFocus
            // 
            this.nudKarmaSustainingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaSustainingFocus.AutoSize = true;
            this.nudKarmaSustainingFocus.Location = new System.Drawing.Point(838, 367);
            this.nudKarmaSustainingFocus.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaSustainingFocus.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaSustainingFocus.Name = "nudKarmaSustainingFocus";
            this.nudKarmaSustainingFocus.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaSustainingFocus.TabIndex = 100;
            // 
            // nudKarmaWeaponFocus
            // 
            this.nudKarmaWeaponFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaWeaponFocus.AutoSize = true;
            this.nudKarmaWeaponFocus.Location = new System.Drawing.Point(838, 393);
            this.nudKarmaWeaponFocus.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaWeaponFocus.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaWeaponFocus.Name = "nudKarmaWeaponFocus";
            this.nudKarmaWeaponFocus.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaWeaponFocus.TabIndex = 106;
            // 
            // lblMetatypeCostsKarmaMultiplierLabel
            // 
            this.lblMetatypeCostsKarmaMultiplierLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMetatypeCostsKarmaMultiplierLabel.AutoSize = true;
            this.lblMetatypeCostsKarmaMultiplierLabel.Location = new System.Drawing.Point(26, 370);
            this.lblMetatypeCostsKarmaMultiplierLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetatypeCostsKarmaMultiplierLabel.Name = "lblMetatypeCostsKarmaMultiplierLabel";
            this.lblMetatypeCostsKarmaMultiplierLabel.Size = new System.Drawing.Size(152, 13);
            this.lblMetatypeCostsKarmaMultiplierLabel.TabIndex = 125;
            this.lblMetatypeCostsKarmaMultiplierLabel.Tag = "Label_Options_MetatypesCostKarma";
            this.lblMetatypeCostsKarmaMultiplierLabel.Text = "Metatype Karma Cost Multiplier";
            this.lblMetatypeCostsKarmaMultiplierLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudMetatypeCostsKarmaMultiplier
            // 
            this.nudMetatypeCostsKarmaMultiplier.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudMetatypeCostsKarmaMultiplier.AutoSize = true;
            this.nudMetatypeCostsKarmaMultiplier.Location = new System.Drawing.Point(184, 367);
            this.nudMetatypeCostsKarmaMultiplier.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudMetatypeCostsKarmaMultiplier.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMetatypeCostsKarmaMultiplier.Name = "nudMetatypeCostsKarmaMultiplier";
            this.nudMetatypeCostsKarmaMultiplier.Size = new System.Drawing.Size(47, 20);
            this.nudMetatypeCostsKarmaMultiplier.TabIndex = 124;
            this.nudMetatypeCostsKarmaMultiplier.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // tabCustomData
            // 
            this.tabCustomData.BackColor = System.Drawing.SystemColors.Control;
            this.tabCustomData.Controls.Add(this.tlpOptionalRules);
            this.tabCustomData.Location = new System.Drawing.Point(4, 22);
            this.tabCustomData.Name = "tabCustomData";
            this.tabCustomData.Padding = new System.Windows.Forms.Padding(9);
            this.tabCustomData.Size = new System.Drawing.Size(1232, 602);
            this.tabCustomData.TabIndex = 2;
            this.tabCustomData.Tag = "Tab_Options_CustomData";
            this.tabCustomData.Text = "Custom Data";
            // 
            // tlpOptionalRules
            // 
            this.tlpOptionalRules.AutoSize = true;
            this.tlpOptionalRules.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpOptionalRules.ColumnCount = 4;
            this.tlpOptionalRules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tlpOptionalRules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpOptionalRules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tlpOptionalRules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tlpOptionalRules.Controls.Add(this.lblCustomDataDirectoriesLabel, 0, 0);
            this.tlpOptionalRules.Controls.Add(this.treCustomDataDirectories, 0, 1);
            this.tlpOptionalRules.Controls.Add(this.cmdIncreaseCustomDirectoryLoadOrder, 2, 0);
            this.tlpOptionalRules.Controls.Add(this.cmdDecreaseCustomDirectoryLoadOrder, 3, 0);
            this.tlpOptionalRules.Controls.Add(this.cmdGlobalOptionsCustomData, 1, 0);
            this.tlpOptionalRules.Controls.Add(this.gbpDirectoryInfo, 1, 1);
            this.tlpOptionalRules.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpOptionalRules.Location = new System.Drawing.Point(9, 9);
            this.tlpOptionalRules.Name = "tlpOptionalRules";
            this.tlpOptionalRules.RowCount = 2;
            this.tlpOptionalRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptionalRules.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpOptionalRules.Size = new System.Drawing.Size(1214, 584);
            this.tlpOptionalRules.TabIndex = 44;
            // 
            // lblCustomDataDirectoriesLabel
            // 
            this.lblCustomDataDirectoriesLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCustomDataDirectoriesLabel.AutoSize = true;
            this.lblCustomDataDirectoriesLabel.Location = new System.Drawing.Point(3, 8);
            this.lblCustomDataDirectoriesLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCustomDataDirectoriesLabel.Name = "lblCustomDataDirectoriesLabel";
            this.lblCustomDataDirectoriesLabel.Size = new System.Drawing.Size(155, 13);
            this.lblCustomDataDirectoriesLabel.TabIndex = 36;
            this.lblCustomDataDirectoriesLabel.Tag = "Label_CharacterOptions_CustomData";
            this.lblCustomDataDirectoriesLabel.Text = "Custom Data Directories to Use";
            this.lblCustomDataDirectoriesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // treCustomDataDirectories
            // 
            this.treCustomDataDirectories.CheckBoxes = true;
            this.treCustomDataDirectories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treCustomDataDirectories.Location = new System.Drawing.Point(3, 32);
            this.treCustomDataDirectories.Name = "treCustomDataDirectories";
            this.treCustomDataDirectories.ShowLines = false;
            this.treCustomDataDirectories.ShowPlusMinus = false;
            this.treCustomDataDirectories.ShowRootLines = false;
            this.treCustomDataDirectories.Size = new System.Drawing.Size(722, 549);
            this.treCustomDataDirectories.TabIndex = 40;
            this.treCustomDataDirectories.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treCustomDataDirectories_AfterCheck);
            this.treCustomDataDirectories.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treCustomDataDirectories_AfterSelect);
            // 
            // cmdIncreaseCustomDirectoryLoadOrder
            // 
            this.cmdIncreaseCustomDirectoryLoadOrder.AutoSize = true;
            this.cmdIncreaseCustomDirectoryLoadOrder.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdIncreaseCustomDirectoryLoadOrder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdIncreaseCustomDirectoryLoadOrder.Location = new System.Drawing.Point(973, 3);
            this.cmdIncreaseCustomDirectoryLoadOrder.Name = "cmdIncreaseCustomDirectoryLoadOrder";
            this.cmdIncreaseCustomDirectoryLoadOrder.Size = new System.Drawing.Size(115, 23);
            this.cmdIncreaseCustomDirectoryLoadOrder.TabIndex = 43;
            this.cmdIncreaseCustomDirectoryLoadOrder.Tag = "Button_IncreaseCustomDirectoryLoadOrder";
            this.cmdIncreaseCustomDirectoryLoadOrder.Text = "Increase Load Order";
            this.cmdIncreaseCustomDirectoryLoadOrder.UseVisualStyleBackColor = true;
            this.cmdIncreaseCustomDirectoryLoadOrder.Click += new System.EventHandler(this.cmdIncreaseCustomDirectoryLoadOrder_Click);
            // 
            // cmdDecreaseCustomDirectoryLoadOrder
            // 
            this.cmdDecreaseCustomDirectoryLoadOrder.AutoSize = true;
            this.cmdDecreaseCustomDirectoryLoadOrder.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDecreaseCustomDirectoryLoadOrder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdDecreaseCustomDirectoryLoadOrder.Location = new System.Drawing.Point(1094, 3);
            this.cmdDecreaseCustomDirectoryLoadOrder.Name = "cmdDecreaseCustomDirectoryLoadOrder";
            this.cmdDecreaseCustomDirectoryLoadOrder.Size = new System.Drawing.Size(117, 23);
            this.cmdDecreaseCustomDirectoryLoadOrder.TabIndex = 42;
            this.cmdDecreaseCustomDirectoryLoadOrder.Tag = "Button_DecreaseCustomDirectoryLoadOrder";
            this.cmdDecreaseCustomDirectoryLoadOrder.Text = "Decrease Load Order";
            this.cmdDecreaseCustomDirectoryLoadOrder.UseVisualStyleBackColor = true;
            this.cmdDecreaseCustomDirectoryLoadOrder.Click += new System.EventHandler(this.cmdDecreaseCustomDirectoryLoadOrder_Click);
            // 
            // cmdGlobalOptionsCustomData
            // 
            this.cmdGlobalOptionsCustomData.AutoSize = true;
            this.cmdGlobalOptionsCustomData.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdGlobalOptionsCustomData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdGlobalOptionsCustomData.Location = new System.Drawing.Point(731, 3);
            this.cmdGlobalOptionsCustomData.Name = "cmdGlobalOptionsCustomData";
            this.cmdGlobalOptionsCustomData.Size = new System.Drawing.Size(236, 23);
            this.cmdGlobalOptionsCustomData.TabIndex = 44;
            this.cmdGlobalOptionsCustomData.Text = "Change Custom Data Entries";
            this.cmdGlobalOptionsCustomData.UseVisualStyleBackColor = true;
            this.cmdGlobalOptionsCustomData.Click += new System.EventHandler(this.cmdGlobalOptionsCustomData_Click);
            // 
            // gbpDirectoryInfo
            // 
            this.gbpDirectoryInfo.AutoSize = true;
            this.tlpOptionalRules.SetColumnSpan(this.gbpDirectoryInfo, 3);
            this.gbpDirectoryInfo.Controls.Add(this.tlpDirectoryInfo);
            this.gbpDirectoryInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbpDirectoryInfo.Location = new System.Drawing.Point(731, 32);
            this.gbpDirectoryInfo.Name = "gbpDirectoryInfo";
            this.gbpDirectoryInfo.Size = new System.Drawing.Size(480, 549);
            this.gbpDirectoryInfo.TabIndex = 45;
            this.gbpDirectoryInfo.TabStop = false;
            this.gbpDirectoryInfo.Tag = "Title_CustomDataDirectoryInfo";
            this.gbpDirectoryInfo.Text = "CustomDataDirectoryInfo";
            // 
            // tlpDirectoryInfo
            // 
            this.tlpDirectoryInfo.ColumnCount = 2;
            this.tlpDirectoryInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpDirectoryInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpDirectoryInfo.Controls.Add(this.lblDirectoryAuthors, 1, 2);
            this.tlpDirectoryInfo.Controls.Add(this.lblDirectoryVersion, 1, 1);
            this.tlpDirectoryInfo.Controls.Add(this.lblDirectoryName, 1, 0);
            this.tlpDirectoryInfo.Controls.Add(this.lblDirectoryNameLabel, 0, 0);
            this.tlpDirectoryInfo.Controls.Add(this.lblDirectoryVersionLabel, 0, 1);
            this.tlpDirectoryInfo.Controls.Add(this.lblDirectoryAuthorsLabel, 0, 2);
            this.tlpDirectoryInfo.Controls.Add(this.gbpDirectoryInfoDependencies, 0, 7);
            this.tlpDirectoryInfo.Controls.Add(this.gbpDirectoryInfoExclusivities, 1, 7);
            this.tlpDirectoryInfo.Controls.Add(this.lblDirectoryDescriptionLabel, 0, 5);
            this.tlpDirectoryInfo.Controls.Add(this.tboxDirectoryDescription, 0, 6);
            this.tlpDirectoryInfo.Controls.Add(this.cmdManageDependencies, 1, 8);
            this.tlpDirectoryInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpDirectoryInfo.Location = new System.Drawing.Point(3, 16);
            this.tlpDirectoryInfo.Name = "tlpDirectoryInfo";
            this.tlpDirectoryInfo.RowCount = 9;
            this.tlpDirectoryInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDirectoryInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDirectoryInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDirectoryInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDirectoryInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDirectoryInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDirectoryInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpDirectoryInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpDirectoryInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDirectoryInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDirectoryInfo.Size = new System.Drawing.Size(474, 530);
            this.tlpDirectoryInfo.TabIndex = 0;
            // 
            // lblDirectoryAuthors
            // 
            this.lblDirectoryAuthors.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDirectoryAuthors.AutoSize = true;
            this.lblDirectoryAuthors.Location = new System.Drawing.Point(238, 38);
            this.lblDirectoryAuthors.Name = "lblDirectoryAuthors";
            this.lblDirectoryAuthors.Padding = new System.Windows.Forms.Padding(3);
            this.lblDirectoryAuthors.Size = new System.Drawing.Size(100, 19);
            this.lblDirectoryAuthors.TabIndex = 6;
            this.lblDirectoryAuthors.Tag = "";
            this.lblDirectoryAuthors.Text = "[Directory Authors]";
            this.lblDirectoryAuthors.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblDirectoryVersion
            // 
            this.lblDirectoryVersion.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDirectoryVersion.AutoSize = true;
            this.lblDirectoryVersion.Location = new System.Drawing.Point(238, 19);
            this.lblDirectoryVersion.Name = "lblDirectoryVersion";
            this.lblDirectoryVersion.Padding = new System.Windows.Forms.Padding(3);
            this.lblDirectoryVersion.Size = new System.Drawing.Size(99, 19);
            this.lblDirectoryVersion.TabIndex = 5;
            this.lblDirectoryVersion.Tag = "";
            this.lblDirectoryVersion.Text = "[Directory Version]";
            this.lblDirectoryVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblDirectoryName
            // 
            this.lblDirectoryName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDirectoryName.AutoSize = true;
            this.lblDirectoryName.Location = new System.Drawing.Point(238, 0);
            this.lblDirectoryName.Name = "lblDirectoryName";
            this.lblDirectoryName.Padding = new System.Windows.Forms.Padding(3);
            this.lblDirectoryName.Size = new System.Drawing.Size(89, 19);
            this.lblDirectoryName.TabIndex = 4;
            this.lblDirectoryName.Tag = "";
            this.lblDirectoryName.Text = "[DirectoryName]";
            this.lblDirectoryName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblDirectoryNameLabel
            // 
            this.lblDirectoryNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDirectoryNameLabel.AutoSize = true;
            this.lblDirectoryNameLabel.Location = new System.Drawing.Point(188, 0);
            this.lblDirectoryNameLabel.Name = "lblDirectoryNameLabel";
            this.lblDirectoryNameLabel.Padding = new System.Windows.Forms.Padding(3);
            this.lblDirectoryNameLabel.Size = new System.Drawing.Size(44, 19);
            this.lblDirectoryNameLabel.TabIndex = 0;
            this.lblDirectoryNameLabel.Tag = "Label_DirectoryName";
            this.lblDirectoryNameLabel.Text = "Name:";
            this.lblDirectoryNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblDirectoryVersionLabel
            // 
            this.lblDirectoryVersionLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDirectoryVersionLabel.AutoSize = true;
            this.lblDirectoryVersionLabel.Location = new System.Drawing.Point(142, 19);
            this.lblDirectoryVersionLabel.Name = "lblDirectoryVersionLabel";
            this.lblDirectoryVersionLabel.Padding = new System.Windows.Forms.Padding(3);
            this.lblDirectoryVersionLabel.Size = new System.Drawing.Size(90, 19);
            this.lblDirectoryVersionLabel.TabIndex = 2;
            this.lblDirectoryVersionLabel.Tag = "Label_DirectoryVersion";
            this.lblDirectoryVersionLabel.Text = "DirectoryVersion";
            this.lblDirectoryVersionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblDirectoryAuthorsLabel
            // 
            this.lblDirectoryAuthorsLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDirectoryAuthorsLabel.AutoSize = true;
            this.lblDirectoryAuthorsLabel.Location = new System.Drawing.Point(138, 38);
            this.lblDirectoryAuthorsLabel.Name = "lblDirectoryAuthorsLabel";
            this.lblDirectoryAuthorsLabel.Padding = new System.Windows.Forms.Padding(3);
            this.lblDirectoryAuthorsLabel.Size = new System.Drawing.Size(94, 19);
            this.lblDirectoryAuthorsLabel.TabIndex = 12;
            this.lblDirectoryAuthorsLabel.Tag = "Label_DirectoryAuthors";
            this.lblDirectoryAuthorsLabel.Text = "Directory Authors";
            // 
            // lblDirectoryDescriptionLabel
            // 
            this.lblDirectoryDescriptionLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblDirectoryDescriptionLabel.AutoSize = true;
            this.tlpDirectoryInfo.SetColumnSpan(this.lblDirectoryDescriptionLabel, 2);
            this.lblDirectoryDescriptionLabel.Location = new System.Drawing.Point(183, 57);
            this.lblDirectoryDescriptionLabel.Name = "lblDirectoryDescriptionLabel";
            this.lblDirectoryDescriptionLabel.Padding = new System.Windows.Forms.Padding(3);
            this.lblDirectoryDescriptionLabel.Size = new System.Drawing.Size(108, 19);
            this.lblDirectoryDescriptionLabel.TabIndex = 3;
            this.lblDirectoryDescriptionLabel.Tag = "Label_DirectoryDescription";
            this.lblDirectoryDescriptionLabel.Text = "DirectoryDescription";
            this.lblDirectoryDescriptionLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // cmdManageDependencies
            // 
            this.cmdManageDependencies.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdManageDependencies.AutoSize = true;
            this.cmdManageDependencies.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpDirectoryInfo.SetColumnSpan(this.cmdManageDependencies, 2);
            this.cmdManageDependencies.Location = new System.Drawing.Point(3, 503);
            this.cmdManageDependencies.Name = "cmdManageDependencies";
            this.cmdManageDependencies.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdManageDependencies.Size = new System.Drawing.Size(468, 23);
            this.cmdManageDependencies.TabIndex = 10;
            this.cmdManageDependencies.Text = "Edit Manifest";
            this.cmdManageDependencies.UseVisualStyleBackColor = true;
            this.cmdManageDependencies.Click += new System.EventHandler(this.cmdManageDependencies_Click);
            // 
            // gbpDirectoryInfoExclusivities
            // 
            this.gbpDirectoryInfoExclusivities.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gbpDirectoryInfoExclusivities.Controls.Add(this.flpExclusivities);
            this.gbpDirectoryInfoExclusivities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbpDirectoryInfoExclusivities.Location = new System.Drawing.Point(238, 291);
            this.gbpDirectoryInfoExclusivities.Name = "gbpDirectoryInfoExclusivities";
            this.gbpDirectoryInfoExclusivities.Size = new System.Drawing.Size(233, 206);
            this.gbpDirectoryInfoExclusivities.TabIndex = 9;
            this.gbpDirectoryInfoExclusivities.TabStop = false;
            this.gbpDirectoryInfoExclusivities.Tag = "Title_DirectoryExclusivities";
            this.gbpDirectoryInfoExclusivities.Text = "Exclusivities";
            // 
            // flpExclusivities
            // 
            this.flpExclusivities.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpExclusivities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpExclusivities.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpExclusivities.Location = new System.Drawing.Point(3, 16);
            this.flpExclusivities.Name = "flpExclusivities";
            this.flpExclusivities.Size = new System.Drawing.Size(227, 187);
            this.flpExclusivities.TabIndex = 1;
            this.flpExclusivities.Tag = "";
            // 
            // gbpDirectoryInfoDependencies
            // 
            this.gbpDirectoryInfoDependencies.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gbpDirectoryInfoDependencies.Controls.Add(this.flpDirectoryDependencies);
            this.gbpDirectoryInfoDependencies.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbpDirectoryInfoDependencies.Location = new System.Drawing.Point(3, 291);
            this.gbpDirectoryInfoDependencies.Name = "gbpDirectoryInfoDependencies";
            this.gbpDirectoryInfoDependencies.Size = new System.Drawing.Size(229, 206);
            this.gbpDirectoryInfoDependencies.TabIndex = 8;
            this.gbpDirectoryInfoDependencies.TabStop = false;
            this.gbpDirectoryInfoDependencies.Tag = "Title_DirectoryDependencies";
            this.gbpDirectoryInfoDependencies.Text = "Dependencies";
            // 
            // flpDirectoryDependencies
            // 
            this.flpDirectoryDependencies.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpDirectoryDependencies.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpDirectoryDependencies.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpDirectoryDependencies.Location = new System.Drawing.Point(3, 16);
            this.flpDirectoryDependencies.Name = "flpDirectoryDependencies";
            this.flpDirectoryDependencies.Size = new System.Drawing.Size(223, 187);
            this.flpDirectoryDependencies.TabIndex = 0;
            this.flpDirectoryDependencies.Tag = "";
            // 
            // tabHouseRules
            // 
            this.tabHouseRules.AutoScroll = true;
            this.tabHouseRules.BackColor = System.Drawing.SystemColors.Control;
            this.tabHouseRules.Controls.Add(this.flpHouseRules);
            this.tabHouseRules.Location = new System.Drawing.Point(4, 22);
            this.tabHouseRules.Name = "tabHouseRules";
            this.tabHouseRules.Padding = new System.Windows.Forms.Padding(9);
            this.tabHouseRules.Size = new System.Drawing.Size(1232, 602);
            this.tabHouseRules.TabIndex = 3;
            this.tabHouseRules.Tag = "Tab_Options_HouseRules";
            this.tabHouseRules.Text = "House Rules";
            // 
            // flpHouseRules
            // 
            this.flpHouseRules.AutoScroll = true;
            this.flpHouseRules.Controls.Add(this.gpbHouseRulesQualities);
            this.flpHouseRules.Controls.Add(this.gpbHouseRulesAttributes);
            this.flpHouseRules.Controls.Add(this.gpbHouseRulesSkills);
            this.flpHouseRules.Controls.Add(this.gpbHouseRulesCombat);
            this.flpHouseRules.Controls.Add(this.gpbHouseRulesMagicResonance);
            this.flpHouseRules.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpHouseRules.Location = new System.Drawing.Point(9, 9);
            this.flpHouseRules.Margin = new System.Windows.Forms.Padding(0);
            this.flpHouseRules.Name = "flpHouseRules";
            this.flpHouseRules.Size = new System.Drawing.Size(1214, 584);
            this.flpHouseRules.TabIndex = 40;
            // 
            // gpbHouseRulesQualities
            // 
            this.gpbHouseRulesQualities.AutoSize = true;
            this.gpbHouseRulesQualities.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbHouseRulesQualities.Controls.Add(this.tlpHouseRulesQualities);
            this.gpbHouseRulesQualities.Location = new System.Drawing.Point(3, 3);
            this.gpbHouseRulesQualities.Name = "gpbHouseRulesQualities";
            this.gpbHouseRulesQualities.Size = new System.Drawing.Size(535, 157);
            this.gpbHouseRulesQualities.TabIndex = 1;
            this.gpbHouseRulesQualities.TabStop = false;
            this.gpbHouseRulesQualities.Tag = "String_Qualities";
            this.gpbHouseRulesQualities.Text = "Qualities";
            // 
            // tlpHouseRulesQualities
            // 
            this.tlpHouseRulesQualities.AutoSize = true;
            this.tlpHouseRulesQualities.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpHouseRulesQualities.ColumnCount = 1;
            this.tlpHouseRulesQualities.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHouseRulesQualities.Controls.Add(this.chkExceedNegativeQualities, 0, 4);
            this.tlpHouseRulesQualities.Controls.Add(this.chkDontDoubleQualityPurchases, 0, 0);
            this.tlpHouseRulesQualities.Controls.Add(this.chkExceedPositiveQualities, 0, 2);
            this.tlpHouseRulesQualities.Controls.Add(this.chkDontDoubleQualityRefunds, 0, 1);
            this.tlpHouseRulesQualities.Controls.Add(this.chkExceedPositiveQualitiesCostDoubled, 0, 3);
            this.tlpHouseRulesQualities.Controls.Add(this.chkExceedNegativeQualitiesLimit, 0, 5);
            this.tlpHouseRulesQualities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpHouseRulesQualities.Location = new System.Drawing.Point(3, 16);
            this.tlpHouseRulesQualities.Name = "tlpHouseRulesQualities";
            this.tlpHouseRulesQualities.RowCount = 6;
            this.tlpHouseRulesQualities.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesQualities.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesQualities.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesQualities.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesQualities.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesQualities.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHouseRulesQualities.Size = new System.Drawing.Size(529, 138);
            this.tlpHouseRulesQualities.TabIndex = 0;
            // 
            // chkExceedNegativeQualities
            // 
            this.chkExceedNegativeQualities.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkExceedNegativeQualities.AutoSize = true;
            this.chkExceedNegativeQualities.DefaultColorScheme = true;
            this.chkExceedNegativeQualities.Location = new System.Drawing.Point(3, 95);
            this.chkExceedNegativeQualities.Name = "chkExceedNegativeQualities";
            this.chkExceedNegativeQualities.Size = new System.Drawing.Size(278, 17);
            this.chkExceedNegativeQualities.TabIndex = 17;
            this.chkExceedNegativeQualities.Tag = "Checkbox_Options_ExceedNegativeQualities";
            this.chkExceedNegativeQualities.Text = "Allow characters to exceed their Negative Quality limit";
            this.chkExceedNegativeQualities.UseVisualStyleBackColor = true;
            // 
            // chkDontDoubleQualityPurchases
            // 
            this.chkDontDoubleQualityPurchases.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkDontDoubleQualityPurchases.AutoSize = true;
            this.chkDontDoubleQualityPurchases.DefaultColorScheme = true;
            this.chkDontDoubleQualityPurchases.Location = new System.Drawing.Point(3, 3);
            this.chkDontDoubleQualityPurchases.Name = "chkDontDoubleQualityPurchases";
            this.chkDontDoubleQualityPurchases.Size = new System.Drawing.Size(352, 17);
            this.chkDontDoubleQualityPurchases.TabIndex = 5;
            this.chkDontDoubleQualityPurchases.Tag = "Checkbox_Options_DontDoubleQualityPurchases";
            this.chkDontDoubleQualityPurchases.Text = "Don\'t double the cost of purchasing Positive Qualities in Career Mode";
            this.chkDontDoubleQualityPurchases.UseVisualStyleBackColor = true;
            // 
            // chkExceedPositiveQualities
            // 
            this.chkExceedPositiveQualities.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkExceedPositiveQualities.AutoSize = true;
            this.chkExceedPositiveQualities.DefaultColorScheme = true;
            this.chkExceedPositiveQualities.Location = new System.Drawing.Point(3, 49);
            this.chkExceedPositiveQualities.Name = "chkExceedPositiveQualities";
            this.chkExceedPositiveQualities.Size = new System.Drawing.Size(272, 17);
            this.chkExceedPositiveQualities.TabIndex = 15;
            this.chkExceedPositiveQualities.Tag = "Checkbox_Options_ExceedPositiveQualities";
            this.chkExceedPositiveQualities.Text = "Allow characters to exceed their Positive Quality limit";
            this.chkExceedPositiveQualities.UseVisualStyleBackColor = true;
            // 
            // chkDontDoubleQualityRefunds
            // 
            this.chkDontDoubleQualityRefunds.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkDontDoubleQualityRefunds.AutoSize = true;
            this.chkDontDoubleQualityRefunds.DefaultColorScheme = true;
            this.chkDontDoubleQualityRefunds.Location = new System.Drawing.Point(3, 26);
            this.chkDontDoubleQualityRefunds.Name = "chkDontDoubleQualityRefunds";
            this.chkDontDoubleQualityRefunds.Size = new System.Drawing.Size(350, 17);
            this.chkDontDoubleQualityRefunds.TabIndex = 21;
            this.chkDontDoubleQualityRefunds.Tag = "Checkbox_Options_DontDoubleNegativeQualityRefunds";
            this.chkDontDoubleQualityRefunds.Text = "Don\'t double the cost of refunding Negative Qualities in Career Mode";
            this.chkDontDoubleQualityRefunds.UseVisualStyleBackColor = true;
            // 
            // chkExceedPositiveQualitiesCostDoubled
            // 
            this.chkExceedPositiveQualitiesCostDoubled.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkExceedPositiveQualitiesCostDoubled.AutoSize = true;
            this.chkExceedPositiveQualitiesCostDoubled.DefaultColorScheme = true;
            this.chkExceedPositiveQualitiesCostDoubled.Enabled = false;
            this.chkExceedPositiveQualitiesCostDoubled.Location = new System.Drawing.Point(23, 72);
            this.chkExceedPositiveQualitiesCostDoubled.Margin = new System.Windows.Forms.Padding(23, 3, 3, 3);
            this.chkExceedPositiveQualitiesCostDoubled.Name = "chkExceedPositiveQualitiesCostDoubled";
            this.chkExceedPositiveQualitiesCostDoubled.Size = new System.Drawing.Size(367, 17);
            this.chkExceedPositiveQualitiesCostDoubled.TabIndex = 16;
            this.chkExceedPositiveQualitiesCostDoubled.Tag = "Checkbox_Options_ExceedPositiveQualitiesCostDoubled";
            this.chkExceedPositiveQualitiesCostDoubled.Text = "Use Career costs for all Positive Quality karma costs in excess of the limit";
            this.chkExceedPositiveQualitiesCostDoubled.UseVisualStyleBackColor = true;
            // 
            // chkExceedNegativeQualitiesLimit
            // 
            this.chkExceedNegativeQualitiesLimit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkExceedNegativeQualitiesLimit.AutoSize = true;
            this.chkExceedNegativeQualitiesLimit.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkExceedNegativeQualitiesLimit.DefaultColorScheme = true;
            this.chkExceedNegativeQualitiesLimit.Enabled = false;
            this.chkExceedNegativeQualitiesLimit.Location = new System.Drawing.Point(23, 118);
            this.chkExceedNegativeQualitiesLimit.Margin = new System.Windows.Forms.Padding(23, 3, 3, 3);
            this.chkExceedNegativeQualitiesLimit.Name = "chkExceedNegativeQualitiesLimit";
            this.chkExceedNegativeQualitiesLimit.Size = new System.Drawing.Size(503, 17);
            this.chkExceedNegativeQualitiesLimit.TabIndex = 18;
            this.chkExceedNegativeQualitiesLimit.Tag = "Checkbox_Options_ExceedNegativeQualitiesLimit";
            this.chkExceedNegativeQualitiesLimit.Text = "Characters do not gain Karma from taking Negative Qualities in excess of their Ga" +
    "meplay Option\'s limit";
            this.chkExceedNegativeQualitiesLimit.UseVisualStyleBackColor = true;
            // 
            // gpbHouseRulesAttributes
            // 
            this.gpbHouseRulesAttributes.AutoSize = true;
            this.gpbHouseRulesAttributes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbHouseRulesAttributes.Controls.Add(this.bufferedTableLayoutPanel1);
            this.gpbHouseRulesAttributes.Location = new System.Drawing.Point(544, 3);
            this.gpbHouseRulesAttributes.Name = "gpbHouseRulesAttributes";
            this.gpbHouseRulesAttributes.Size = new System.Drawing.Size(423, 183);
            this.gpbHouseRulesAttributes.TabIndex = 3;
            this.gpbHouseRulesAttributes.TabStop = false;
            this.gpbHouseRulesAttributes.Tag = "Label_Attributes";
            this.gpbHouseRulesAttributes.Text = "Attributes";
            // 
            // bufferedTableLayoutPanel1
            // 
            this.bufferedTableLayoutPanel1.AutoSize = true;
            this.bufferedTableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bufferedTableLayoutPanel1.ColumnCount = 1;
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bufferedTableLayoutPanel1.Controls.Add(this.chkESSLossReducesMaximumOnly, 0, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.chkAllowCyberwareESSDiscounts, 0, 2);
            this.bufferedTableLayoutPanel1.Controls.Add(this.chkAlternateMetatypeAttributeKarma, 0, 4);
            this.bufferedTableLayoutPanel1.Controls.Add(this.chkReverseAttributePriorityOrder, 0, 3);
            this.bufferedTableLayoutPanel1.Controls.Add(this.flpDroneArmorMultiplier, 0, 6);
            this.bufferedTableLayoutPanel1.Controls.Add(this.chkUseCalculatedPublicAwareness, 0, 5);
            this.bufferedTableLayoutPanel1.Controls.Add(this.chkUnclampAttributeMinimum, 0, 1);
            this.bufferedTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedTableLayoutPanel1.Location = new System.Drawing.Point(3, 16);
            this.bufferedTableLayoutPanel1.Name = "bufferedTableLayoutPanel1";
            this.bufferedTableLayoutPanel1.RowCount = 7;
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bufferedTableLayoutPanel1.Size = new System.Drawing.Size(417, 164);
            this.bufferedTableLayoutPanel1.TabIndex = 0;
            // 
            // chkESSLossReducesMaximumOnly
            // 
            this.chkESSLossReducesMaximumOnly.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkESSLossReducesMaximumOnly.AutoSize = true;
            this.chkESSLossReducesMaximumOnly.DefaultColorScheme = true;
            this.chkESSLossReducesMaximumOnly.Location = new System.Drawing.Point(3, 3);
            this.chkESSLossReducesMaximumOnly.Name = "chkESSLossReducesMaximumOnly";
            this.chkESSLossReducesMaximumOnly.Size = new System.Drawing.Size(251, 17);
            this.chkESSLossReducesMaximumOnly.TabIndex = 20;
            this.chkESSLossReducesMaximumOnly.Tag = "Checkbox_Options_EssenceLossReducesMaximum";
            this.chkESSLossReducesMaximumOnly.Text = "Essence Loss only Reduces Maximum Essence";
            this.chkESSLossReducesMaximumOnly.UseVisualStyleBackColor = true;
            // 
            // chkAllowCyberwareESSDiscounts
            // 
            this.chkAllowCyberwareESSDiscounts.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkAllowCyberwareESSDiscounts.AutoSize = true;
            this.chkAllowCyberwareESSDiscounts.DefaultColorScheme = true;
            this.chkAllowCyberwareESSDiscounts.Location = new System.Drawing.Point(3, 49);
            this.chkAllowCyberwareESSDiscounts.Name = "chkAllowCyberwareESSDiscounts";
            this.chkAllowCyberwareESSDiscounts.Size = new System.Drawing.Size(279, 17);
            this.chkAllowCyberwareESSDiscounts.TabIndex = 19;
            this.chkAllowCyberwareESSDiscounts.Tag = "Checkbox_Options_AllowCyberwareESSDiscounts";
            this.chkAllowCyberwareESSDiscounts.Text = "Allow Cyber/Bioware Essence costs to be customized";
            this.chkAllowCyberwareESSDiscounts.UseVisualStyleBackColor = true;
            // 
            // chkAlternateMetatypeAttributeKarma
            // 
            this.chkAlternateMetatypeAttributeKarma.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkAlternateMetatypeAttributeKarma.AutoSize = true;
            this.chkAlternateMetatypeAttributeKarma.DefaultColorScheme = true;
            this.chkAlternateMetatypeAttributeKarma.Location = new System.Drawing.Point(3, 95);
            this.chkAlternateMetatypeAttributeKarma.Name = "chkAlternateMetatypeAttributeKarma";
            this.chkAlternateMetatypeAttributeKarma.Size = new System.Drawing.Size(411, 17);
            this.chkAlternateMetatypeAttributeKarma.TabIndex = 28;
            this.chkAlternateMetatypeAttributeKarma.Tag = "Checkbox_Option_AlternateMetatypeAttributeKarma";
            this.chkAlternateMetatypeAttributeKarma.Text = "Treat Metatype Attribute Minimum as 1 for the purpose of determining Karma costs";
            this.chkAlternateMetatypeAttributeKarma.UseVisualStyleBackColor = true;
            // 
            // chkReverseAttributePriorityOrder
            // 
            this.chkReverseAttributePriorityOrder.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkReverseAttributePriorityOrder.AutoSize = true;
            this.chkReverseAttributePriorityOrder.DefaultColorScheme = true;
            this.chkReverseAttributePriorityOrder.Location = new System.Drawing.Point(3, 72);
            this.chkReverseAttributePriorityOrder.Name = "chkReverseAttributePriorityOrder";
            this.chkReverseAttributePriorityOrder.Size = new System.Drawing.Size(251, 17);
            this.chkReverseAttributePriorityOrder.TabIndex = 33;
            this.chkReverseAttributePriorityOrder.Tag = "Checkbox_Options_ReverseAttributePriorityOrder";
            this.chkReverseAttributePriorityOrder.Text = "Spend Karma on Attributes before Priority Points";
            this.chkReverseAttributePriorityOrder.UseVisualStyleBackColor = true;
            // 
            // flpDroneArmorMultiplier
            // 
            this.flpDroneArmorMultiplier.AutoSize = true;
            this.flpDroneArmorMultiplier.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpDroneArmorMultiplier.Controls.Add(this.chkDroneArmorMultiplier);
            this.flpDroneArmorMultiplier.Controls.Add(this.lblDroneArmorMultiplierTimes);
            this.flpDroneArmorMultiplier.Controls.Add(this.nudDroneArmorMultiplier);
            this.flpDroneArmorMultiplier.Location = new System.Drawing.Point(0, 138);
            this.flpDroneArmorMultiplier.Margin = new System.Windows.Forms.Padding(0);
            this.flpDroneArmorMultiplier.Name = "flpDroneArmorMultiplier";
            this.flpDroneArmorMultiplier.Size = new System.Drawing.Size(317, 26);
            this.flpDroneArmorMultiplier.TabIndex = 52;
            // 
            // chkDroneArmorMultiplier
            // 
            this.chkDroneArmorMultiplier.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkDroneArmorMultiplier.AutoSize = true;
            this.chkDroneArmorMultiplier.DefaultColorScheme = true;
            this.chkDroneArmorMultiplier.Location = new System.Drawing.Point(3, 4);
            this.chkDroneArmorMultiplier.Name = "chkDroneArmorMultiplier";
            this.chkDroneArmorMultiplier.Size = new System.Drawing.Size(252, 17);
            this.chkDroneArmorMultiplier.TabIndex = 25;
            this.chkDroneArmorMultiplier.Tag = "Checkbox_Options_DroneArmorMultiplier";
            this.chkDroneArmorMultiplier.Text = "Limit Drone Armor Enhance ment to Drone Body";
            this.chkDroneArmorMultiplier.UseVisualStyleBackColor = true;
            // 
            // lblDroneArmorMultiplierTimes
            // 
            this.lblDroneArmorMultiplierTimes.AutoSize = true;
            this.lblDroneArmorMultiplierTimes.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblDroneArmorMultiplierTimes.Location = new System.Drawing.Point(261, 3);
            this.lblDroneArmorMultiplierTimes.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.lblDroneArmorMultiplierTimes.Name = "lblDroneArmorMultiplierTimes";
            this.lblDroneArmorMultiplierTimes.Size = new System.Drawing.Size(12, 17);
            this.lblDroneArmorMultiplierTimes.TabIndex = 27;
            this.lblDroneArmorMultiplierTimes.Text = "x";
            this.lblDroneArmorMultiplierTimes.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudDroneArmorMultiplier
            // 
            this.nudDroneArmorMultiplier.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudDroneArmorMultiplier.AutoSize = true;
            this.nudDroneArmorMultiplier.Enabled = false;
            this.nudDroneArmorMultiplier.Location = new System.Drawing.Point(279, 3);
            this.nudDroneArmorMultiplier.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudDroneArmorMultiplier.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nudDroneArmorMultiplier.Name = "nudDroneArmorMultiplier";
            this.nudDroneArmorMultiplier.Size = new System.Drawing.Size(35, 20);
            this.nudDroneArmorMultiplier.TabIndex = 26;
            this.nudDroneArmorMultiplier.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // chkUseCalculatedPublicAwareness
            // 
            this.chkUseCalculatedPublicAwareness.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkUseCalculatedPublicAwareness.AutoSize = true;
            this.chkUseCalculatedPublicAwareness.DefaultColorScheme = true;
            this.chkUseCalculatedPublicAwareness.Location = new System.Drawing.Point(3, 118);
            this.chkUseCalculatedPublicAwareness.Name = "chkUseCalculatedPublicAwareness";
            this.chkUseCalculatedPublicAwareness.Size = new System.Drawing.Size(289, 17);
            this.chkUseCalculatedPublicAwareness.TabIndex = 22;
            this.chkUseCalculatedPublicAwareness.Tag = "Checkbox_Options_UseCalculatedPublicAwareness";
            this.chkUseCalculatedPublicAwareness.Text = "Public Awareness should be (Street Cred + Notoriety /3)";
            this.chkUseCalculatedPublicAwareness.UseVisualStyleBackColor = true;
            // 
            // chkUnclampAttributeMinimum
            // 
            this.chkUnclampAttributeMinimum.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkUnclampAttributeMinimum.AutoSize = true;
            this.chkUnclampAttributeMinimum.DefaultColorScheme = true;
            this.chkUnclampAttributeMinimum.Location = new System.Drawing.Point(3, 26);
            this.chkUnclampAttributeMinimum.Name = "chkUnclampAttributeMinimum";
            this.chkUnclampAttributeMinimum.Size = new System.Drawing.Size(328, 17);
            this.chkUnclampAttributeMinimum.TabIndex = 50;
            this.chkUnclampAttributeMinimum.Tag = "Checkbox_Options_UnclampAttributeMinimum";
            this.chkUnclampAttributeMinimum.Text = "Attribute values are allowed to go below 0 due to Essence Loss.";
            this.chkUnclampAttributeMinimum.UseVisualStyleBackColor = true;
            // 
            // gpbHouseRulesSkills
            // 
            this.gpbHouseRulesSkills.AutoSize = true;
            this.gpbHouseRulesSkills.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbHouseRulesSkills.Controls.Add(this.tlpHouseRulesSkills);
            this.gpbHouseRulesSkills.Location = new System.Drawing.Point(3, 192);
            this.gpbHouseRulesSkills.Name = "gpbHouseRulesSkills";
            this.gpbHouseRulesSkills.Size = new System.Drawing.Size(452, 111);
            this.gpbHouseRulesSkills.TabIndex = 2;
            this.gpbHouseRulesSkills.TabStop = false;
            this.gpbHouseRulesSkills.Tag = "Tab_Skills";
            this.gpbHouseRulesSkills.Text = "Skills";
            // 
            // tlpHouseRulesSkills
            // 
            this.tlpHouseRulesSkills.AutoSize = true;
            this.tlpHouseRulesSkills.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpHouseRulesSkills.ColumnCount = 1;
            this.tlpHouseRulesSkills.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHouseRulesSkills.Controls.Add(this.chkCompensateSkillGroupKarmaDifference, 0, 0);
            this.tlpHouseRulesSkills.Controls.Add(this.chkAllowSkillRegrouping, 0, 1);
            this.tlpHouseRulesSkills.Controls.Add(this.chkFreeMartialArtSpecialization, 0, 3);
            this.tlpHouseRulesSkills.Controls.Add(this.chkUsePointsOnBrokenGroups, 0, 2);
            this.tlpHouseRulesSkills.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpHouseRulesSkills.Location = new System.Drawing.Point(3, 16);
            this.tlpHouseRulesSkills.Name = "tlpHouseRulesSkills";
            this.tlpHouseRulesSkills.RowCount = 4;
            this.tlpHouseRulesSkills.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesSkills.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesSkills.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesSkills.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHouseRulesSkills.Size = new System.Drawing.Size(446, 92);
            this.tlpHouseRulesSkills.TabIndex = 0;
            // 
            // chkCompensateSkillGroupKarmaDifference
            // 
            this.chkCompensateSkillGroupKarmaDifference.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkCompensateSkillGroupKarmaDifference.AutoSize = true;
            this.chkCompensateSkillGroupKarmaDifference.DefaultColorScheme = true;
            this.chkCompensateSkillGroupKarmaDifference.Location = new System.Drawing.Point(3, 3);
            this.chkCompensateSkillGroupKarmaDifference.Name = "chkCompensateSkillGroupKarmaDifference";
            this.chkCompensateSkillGroupKarmaDifference.Size = new System.Drawing.Size(440, 17);
            this.chkCompensateSkillGroupKarmaDifference.TabIndex = 36;
            this.chkCompensateSkillGroupKarmaDifference.Tag = "Checkbox_Options_CompensateSkillGroupKarmaDifference";
            this.chkCompensateSkillGroupKarmaDifference.Text = "Compensate for higher karma costs when raising the rating of the last skill in a " +
    "skill group";
            this.chkCompensateSkillGroupKarmaDifference.UseVisualStyleBackColor = true;
            // 
            // chkAllowSkillRegrouping
            // 
            this.chkAllowSkillRegrouping.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkAllowSkillRegrouping.AutoSize = true;
            this.chkAllowSkillRegrouping.DefaultColorScheme = true;
            this.chkAllowSkillRegrouping.Location = new System.Drawing.Point(3, 26);
            this.chkAllowSkillRegrouping.Name = "chkAllowSkillRegrouping";
            this.chkAllowSkillRegrouping.Size = new System.Drawing.Size(285, 17);
            this.chkAllowSkillRegrouping.TabIndex = 39;
            this.chkAllowSkillRegrouping.Tag = "Checkbox_Options_SkillRegroup";
            this.chkAllowSkillRegrouping.Text = "Allow Skills to be re-Grouped if all Ratings are the same";
            this.chkAllowSkillRegrouping.UseVisualStyleBackColor = true;
            // 
            // chkFreeMartialArtSpecialization
            // 
            this.chkFreeMartialArtSpecialization.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkFreeMartialArtSpecialization.AutoSize = true;
            this.chkFreeMartialArtSpecialization.DefaultColorScheme = true;
            this.chkFreeMartialArtSpecialization.Location = new System.Drawing.Point(3, 72);
            this.chkFreeMartialArtSpecialization.Name = "chkFreeMartialArtSpecialization";
            this.chkFreeMartialArtSpecialization.Size = new System.Drawing.Size(281, 17);
            this.chkFreeMartialArtSpecialization.TabIndex = 30;
            this.chkFreeMartialArtSpecialization.Tag = "Checkbox_Option_FreeMartialArtSpecialization";
            this.chkFreeMartialArtSpecialization.Text = "Allow Martial Arts to grant a free specialisation in a skill";
            this.chkFreeMartialArtSpecialization.UseVisualStyleBackColor = true;
            // 
            // chkUsePointsOnBrokenGroups
            // 
            this.chkUsePointsOnBrokenGroups.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkUsePointsOnBrokenGroups.AutoSize = true;
            this.chkUsePointsOnBrokenGroups.DefaultColorScheme = true;
            this.chkUsePointsOnBrokenGroups.Location = new System.Drawing.Point(3, 49);
            this.chkUsePointsOnBrokenGroups.Name = "chkUsePointsOnBrokenGroups";
            this.chkUsePointsOnBrokenGroups.Size = new System.Drawing.Size(185, 17);
            this.chkUsePointsOnBrokenGroups.TabIndex = 49;
            this.chkUsePointsOnBrokenGroups.Tag = "Checkbox_Options_PointsOnBrokenGroups";
            this.chkUsePointsOnBrokenGroups.Text = "Use Skill Points on broken groups";
            this.chkUsePointsOnBrokenGroups.UseVisualStyleBackColor = true;
            // 
            // gpbHouseRulesCombat
            // 
            this.gpbHouseRulesCombat.AutoSize = true;
            this.gpbHouseRulesCombat.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbHouseRulesCombat.Controls.Add(this.bufferedTableLayoutPanel2);
            this.gpbHouseRulesCombat.Location = new System.Drawing.Point(461, 192);
            this.gpbHouseRulesCombat.Name = "gpbHouseRulesCombat";
            this.gpbHouseRulesCombat.Size = new System.Drawing.Size(384, 88);
            this.gpbHouseRulesCombat.TabIndex = 4;
            this.gpbHouseRulesCombat.TabStop = false;
            this.gpbHouseRulesCombat.Tag = "Label_CharacterOptions_Combat";
            this.gpbHouseRulesCombat.Text = "Combat";
            // 
            // bufferedTableLayoutPanel2
            // 
            this.bufferedTableLayoutPanel2.AutoSize = true;
            this.bufferedTableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bufferedTableLayoutPanel2.ColumnCount = 1;
            this.bufferedTableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bufferedTableLayoutPanel2.Controls.Add(this.chkNoArmorEncumbrance, 0, 0);
            this.bufferedTableLayoutPanel2.Controls.Add(this.chkMoreLethalGameplay, 0, 2);
            this.bufferedTableLayoutPanel2.Controls.Add(this.chkUnarmedSkillImprovements, 0, 1);
            this.bufferedTableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedTableLayoutPanel2.Location = new System.Drawing.Point(3, 16);
            this.bufferedTableLayoutPanel2.Name = "bufferedTableLayoutPanel2";
            this.bufferedTableLayoutPanel2.RowCount = 3;
            this.bufferedTableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bufferedTableLayoutPanel2.Size = new System.Drawing.Size(378, 69);
            this.bufferedTableLayoutPanel2.TabIndex = 0;
            // 
            // chkNoArmorEncumbrance
            // 
            this.chkNoArmorEncumbrance.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkNoArmorEncumbrance.AutoSize = true;
            this.chkNoArmorEncumbrance.DefaultColorScheme = true;
            this.chkNoArmorEncumbrance.Location = new System.Drawing.Point(3, 3);
            this.chkNoArmorEncumbrance.Name = "chkNoArmorEncumbrance";
            this.chkNoArmorEncumbrance.Size = new System.Drawing.Size(139, 17);
            this.chkNoArmorEncumbrance.TabIndex = 38;
            this.chkNoArmorEncumbrance.Tag = "Checkbox_Options_NoArmorEncumbrance";
            this.chkNoArmorEncumbrance.Text = "No Armor Encumbrance";
            this.chkNoArmorEncumbrance.UseVisualStyleBackColor = true;
            // 
            // chkMoreLethalGameplay
            // 
            this.chkMoreLethalGameplay.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkMoreLethalGameplay.AutoSize = true;
            this.chkMoreLethalGameplay.DefaultColorScheme = true;
            this.chkMoreLethalGameplay.Location = new System.Drawing.Point(3, 49);
            this.chkMoreLethalGameplay.Name = "chkMoreLethalGameplay";
            this.chkMoreLethalGameplay.Size = new System.Drawing.Size(297, 17);
            this.chkMoreLethalGameplay.TabIndex = 41;
            this.chkMoreLethalGameplay.Tag = "Checkbox_Options_MoreLethalGameplace";
            this.chkMoreLethalGameplay.Text = "Use 4th Edition Rules for More Lethal Gameplay (SR4 75)";
            this.chkMoreLethalGameplay.UseVisualStyleBackColor = true;
            // 
            // chkUnarmedSkillImprovements
            // 
            this.chkUnarmedSkillImprovements.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkUnarmedSkillImprovements.AutoSize = true;
            this.chkUnarmedSkillImprovements.DefaultColorScheme = true;
            this.chkUnarmedSkillImprovements.Location = new System.Drawing.Point(3, 26);
            this.chkUnarmedSkillImprovements.Name = "chkUnarmedSkillImprovements";
            this.chkUnarmedSkillImprovements.Size = new System.Drawing.Size(372, 17);
            this.chkUnarmedSkillImprovements.TabIndex = 0;
            this.chkUnarmedSkillImprovements.Tag = "Checkbox_Options_UnarmedSkillImprovements";
            this.chkUnarmedSkillImprovements.Text = "Unarmed Combat-based Weapons Benefit from Unarmed Attack Bonuses";
            this.chkUnarmedSkillImprovements.UseVisualStyleBackColor = true;
            // 
            // gpbHouseRulesMagicResonance
            // 
            this.gpbHouseRulesMagicResonance.AutoSize = true;
            this.gpbHouseRulesMagicResonance.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbHouseRulesMagicResonance.Controls.Add(this.tlpHouseRulesMagicResonance);
            this.gpbHouseRulesMagicResonance.Location = new System.Drawing.Point(3, 309);
            this.gpbHouseRulesMagicResonance.Name = "gpbHouseRulesMagicResonance";
            this.gpbHouseRulesMagicResonance.Size = new System.Drawing.Size(440, 249);
            this.gpbHouseRulesMagicResonance.TabIndex = 0;
            this.gpbHouseRulesMagicResonance.TabStop = false;
            this.gpbHouseRulesMagicResonance.Tag = "Label_CharacterOptions_MagicAndResonance";
            this.gpbHouseRulesMagicResonance.Text = "Magic and Resonance";
            // 
            // tlpHouseRulesMagicResonance
            // 
            this.tlpHouseRulesMagicResonance.AutoSize = true;
            this.tlpHouseRulesMagicResonance.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpHouseRulesMagicResonance.ColumnCount = 1;
            this.tlpHouseRulesMagicResonance.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHouseRulesMagicResonance.Controls.Add(this.chkMysAdPp, 0, 2);
            this.tlpHouseRulesMagicResonance.Controls.Add(this.chkPrioritySpellsAsAdeptPowers, 0, 3);
            this.tlpHouseRulesMagicResonance.Controls.Add(this.chkExtendAnyDetectionSpell, 0, 4);
            this.tlpHouseRulesMagicResonance.Controls.Add(this.chkIncreasedImprovedAbilityModifier, 0, 5);
            this.tlpHouseRulesMagicResonance.Controls.Add(this.chkIgnoreComplexFormLimit, 0, 9);
            this.tlpHouseRulesMagicResonance.Controls.Add(this.chkSpecialKarmaCost, 0, 6);
            this.tlpHouseRulesMagicResonance.Controls.Add(this.chkAllowTechnomancerSchooling, 0, 8);
            this.tlpHouseRulesMagicResonance.Controls.Add(this.chkAllowInitiation, 0, 7);
            this.tlpHouseRulesMagicResonance.Controls.Add(this.chkMysAdeptSecondMAGAttribute, 0, 1);
            this.tlpHouseRulesMagicResonance.Controls.Add(this.chkIgnoreArt, 0, 0);
            this.tlpHouseRulesMagicResonance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpHouseRulesMagicResonance.Location = new System.Drawing.Point(3, 16);
            this.tlpHouseRulesMagicResonance.Name = "tlpHouseRulesMagicResonance";
            this.tlpHouseRulesMagicResonance.RowCount = 10;
            this.tlpHouseRulesMagicResonance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesMagicResonance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesMagicResonance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesMagicResonance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesMagicResonance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesMagicResonance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesMagicResonance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesMagicResonance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesMagicResonance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesMagicResonance.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHouseRulesMagicResonance.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpHouseRulesMagicResonance.Size = new System.Drawing.Size(434, 230);
            this.tlpHouseRulesMagicResonance.TabIndex = 0;
            // 
            // chkMysAdPp
            // 
            this.chkMysAdPp.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkMysAdPp.AutoSize = true;
            this.chkMysAdPp.DefaultColorScheme = true;
            this.chkMysAdPp.Location = new System.Drawing.Point(3, 49);
            this.chkMysAdPp.Name = "chkMysAdPp";
            this.chkMysAdPp.Size = new System.Drawing.Size(280, 17);
            this.chkMysAdPp.TabIndex = 29;
            this.chkMysAdPp.Tag = "Checkbox_Option_AllowMysadPowerPointCareer";
            this.chkMysAdPp.Text = "Allow Mystic Adepts to buy power points during career";
            this.chkMysAdPp.UseVisualStyleBackColor = true;
            // 
            // chkPrioritySpellsAsAdeptPowers
            // 
            this.chkPrioritySpellsAsAdeptPowers.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkPrioritySpellsAsAdeptPowers.AutoSize = true;
            this.chkPrioritySpellsAsAdeptPowers.DefaultColorScheme = true;
            this.chkPrioritySpellsAsAdeptPowers.Location = new System.Drawing.Point(3, 72);
            this.chkPrioritySpellsAsAdeptPowers.Name = "chkPrioritySpellsAsAdeptPowers";
            this.chkPrioritySpellsAsAdeptPowers.Size = new System.Drawing.Size(325, 17);
            this.chkPrioritySpellsAsAdeptPowers.TabIndex = 31;
            this.chkPrioritySpellsAsAdeptPowers.Tag = "Checkbox_Option_PrioritySpellsAsAdeptPowers";
            this.chkPrioritySpellsAsAdeptPowers.Text = "Allow spending of free spells from Magic Priority as power points";
            this.chkPrioritySpellsAsAdeptPowers.UseVisualStyleBackColor = true;
            // 
            // chkExtendAnyDetectionSpell
            // 
            this.chkExtendAnyDetectionSpell.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkExtendAnyDetectionSpell.AutoSize = true;
            this.chkExtendAnyDetectionSpell.DefaultColorScheme = true;
            this.chkExtendAnyDetectionSpell.Location = new System.Drawing.Point(3, 95);
            this.chkExtendAnyDetectionSpell.Name = "chkExtendAnyDetectionSpell";
            this.chkExtendAnyDetectionSpell.Size = new System.Drawing.Size(332, 17);
            this.chkExtendAnyDetectionSpell.TabIndex = 40;
            this.chkExtendAnyDetectionSpell.Tag = "Checkbox_Options_ExtendAnyDetectionSpell";
            this.chkExtendAnyDetectionSpell.Text = "Allow any Detection Spell to be taken as Extended range version";
            this.chkExtendAnyDetectionSpell.UseVisualStyleBackColor = true;
            // 
            // chkIncreasedImprovedAbilityModifier
            // 
            this.chkIncreasedImprovedAbilityModifier.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkIncreasedImprovedAbilityModifier.AutoSize = true;
            this.chkIncreasedImprovedAbilityModifier.DefaultColorScheme = true;
            this.chkIncreasedImprovedAbilityModifier.Location = new System.Drawing.Point(3, 118);
            this.chkIncreasedImprovedAbilityModifier.Name = "chkIncreasedImprovedAbilityModifier";
            this.chkIncreasedImprovedAbilityModifier.Size = new System.Drawing.Size(332, 17);
            this.chkIncreasedImprovedAbilityModifier.TabIndex = 44;
            this.chkIncreasedImprovedAbilityModifier.Tag = "Checkbox_Options_IncreasedImprovedAbilityModifier";
            this.chkIncreasedImprovedAbilityModifier.Text = "Improved Ability is capped by Learned Rating x 1.5 instead of 0.5";
            this.chkIncreasedImprovedAbilityModifier.UseVisualStyleBackColor = true;
            // 
            // chkIgnoreComplexFormLimit
            // 
            this.chkIgnoreComplexFormLimit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkIgnoreComplexFormLimit.AutoSize = true;
            this.chkIgnoreComplexFormLimit.DefaultColorScheme = true;
            this.chkIgnoreComplexFormLimit.Location = new System.Drawing.Point(3, 210);
            this.chkIgnoreComplexFormLimit.Name = "chkIgnoreComplexFormLimit";
            this.chkIgnoreComplexFormLimit.Size = new System.Drawing.Size(215, 17);
            this.chkIgnoreComplexFormLimit.TabIndex = 43;
            this.chkIgnoreComplexFormLimit.Tag = "Checkbox_Options_IgnoreComplexFormLimit";
            this.chkIgnoreComplexFormLimit.Text = "Ignore complex form limit in Career mode";
            this.chkIgnoreComplexFormLimit.UseVisualStyleBackColor = true;
            // 
            // chkSpecialKarmaCost
            // 
            this.chkSpecialKarmaCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkSpecialKarmaCost.AutoSize = true;
            this.chkSpecialKarmaCost.DefaultColorScheme = true;
            this.chkSpecialKarmaCost.Location = new System.Drawing.Point(3, 141);
            this.chkSpecialKarmaCost.Name = "chkSpecialKarmaCost";
            this.chkSpecialKarmaCost.Size = new System.Drawing.Size(373, 17);
            this.chkSpecialKarmaCost.TabIndex = 42;
            this.chkSpecialKarmaCost.Tag = "Checkbox_Options_SpecialKarmaCost";
            this.chkSpecialKarmaCost.Text = "Karma cost for increasing Special Attributes is reduced with Essence Loss";
            this.chkSpecialKarmaCost.UseVisualStyleBackColor = true;
            // 
            // chkAllowTechnomancerSchooling
            // 
            this.chkAllowTechnomancerSchooling.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkAllowTechnomancerSchooling.AutoSize = true;
            this.chkAllowTechnomancerSchooling.DefaultColorScheme = true;
            this.chkAllowTechnomancerSchooling.Location = new System.Drawing.Point(3, 187);
            this.chkAllowTechnomancerSchooling.Name = "chkAllowTechnomancerSchooling";
            this.chkAllowTechnomancerSchooling.Size = new System.Drawing.Size(273, 17);
            this.chkAllowTechnomancerSchooling.TabIndex = 45;
            this.chkAllowTechnomancerSchooling.Tag = "Checkbox_Options_AllowTechnomancerSchooling";
            this.chkAllowTechnomancerSchooling.Text = "Technomancer: Allow \'Schooling\' Initiation discounts";
            this.chkAllowTechnomancerSchooling.UseVisualStyleBackColor = true;
            // 
            // chkAllowInitiation
            // 
            this.chkAllowInitiation.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkAllowInitiation.AutoSize = true;
            this.chkAllowInitiation.DefaultColorScheme = true;
            this.chkAllowInitiation.Location = new System.Drawing.Point(3, 164);
            this.chkAllowInitiation.Name = "chkAllowInitiation";
            this.chkAllowInitiation.Size = new System.Drawing.Size(227, 17);
            this.chkAllowInitiation.TabIndex = 7;
            this.chkAllowInitiation.Tag = "Checkbox_Options_AllowInitiation";
            this.chkAllowInitiation.Text = "Allow Initiation/Submersion in Create mode";
            this.chkAllowInitiation.UseVisualStyleBackColor = true;
            // 
            // chkMysAdeptSecondMAGAttribute
            // 
            this.chkMysAdeptSecondMAGAttribute.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkMysAdeptSecondMAGAttribute.AutoSize = true;
            this.chkMysAdeptSecondMAGAttribute.DefaultColorScheme = true;
            this.chkMysAdeptSecondMAGAttribute.Location = new System.Drawing.Point(3, 26);
            this.chkMysAdeptSecondMAGAttribute.Name = "chkMysAdeptSecondMAGAttribute";
            this.chkMysAdeptSecondMAGAttribute.Size = new System.Drawing.Size(428, 17);
            this.chkMysAdeptSecondMAGAttribute.TabIndex = 35;
            this.chkMysAdeptSecondMAGAttribute.Tag = "Checkbox_Options_MysAdeptSecondMAGAttribute";
            this.chkMysAdeptSecondMAGAttribute.Text = "Mystic Adepts use second MAG attribute for Adept abilities instead of special PP " +
    "rules";
            this.chkMysAdeptSecondMAGAttribute.UseVisualStyleBackColor = true;
            // 
            // chkIgnoreArt
            // 
            this.chkIgnoreArt.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkIgnoreArt.AutoSize = true;
            this.chkIgnoreArt.DefaultColorScheme = true;
            this.chkIgnoreArt.Location = new System.Drawing.Point(3, 3);
            this.chkIgnoreArt.Name = "chkIgnoreArt";
            this.chkIgnoreArt.Size = new System.Drawing.Size(235, 17);
            this.chkIgnoreArt.TabIndex = 1;
            this.chkIgnoreArt.Tag = "Checkbox_Options_IgnoreArt";
            this.chkIgnoreArt.Text = "Ignore Art Requirements from Street Grimoire";
            this.chkIgnoreArt.UseVisualStyleBackColor = true;
            // 
            // lblSettingName
            // 
            this.lblSettingName.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSettingName.AutoSize = true;
            this.lblSettingName.Location = new System.Drawing.Point(3, 642);
            this.lblSettingName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSettingName.Name = "lblSettingName";
            this.lblSettingName.Size = new System.Drawing.Size(74, 13);
            this.lblSettingName.TabIndex = 2;
            this.lblSettingName.Tag = "Label_Options_SettingName";
            this.lblSettingName.Text = "Setting Name:";
            this.lblSettingName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tlpButtons
            // 
            this.tlpButtons.AutoSize = true;
            this.tlpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpButtons.ColumnCount = 6;
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tlpButtons.Controls.Add(this.cmdRename, 0, 0);
            this.tlpButtons.Controls.Add(this.cmdDelete, 2, 0);
            this.tlpButtons.Controls.Add(this.cmdOK, 5, 0);
            this.tlpButtons.Controls.Add(this.cmdRestoreDefaults, 0, 0);
            this.tlpButtons.Controls.Add(this.cmdSave, 4, 0);
            this.tlpButtons.Controls.Add(this.cmdSaveAs, 3, 0);
            this.tlpButtons.Location = new System.Drawing.Point(649, 637);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButtons.Size = new System.Drawing.Size(594, 23);
            this.tlpButtons.TabIndex = 6;
            // 
            // cmdRename
            // 
            this.cmdRename.AutoSize = true;
            this.cmdRename.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRename.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdRename.Location = new System.Drawing.Point(101, 0);
            this.cmdRename.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdRename.Name = "cmdRename";
            this.cmdRename.Size = new System.Drawing.Size(92, 23);
            this.cmdRename.TabIndex = 10;
            this.cmdRename.Tag = "String_Rename";
            this.cmdRename.Text = "Rename";
            this.cmdRename.UseVisualStyleBackColor = true;
            this.cmdRename.Click += new System.EventHandler(this.cmdRename_Click);
            // 
            // cmdDelete
            // 
            this.cmdDelete.AutoSize = true;
            this.cmdDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDelete.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdDelete.Location = new System.Drawing.Point(199, 0);
            this.cmdDelete.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(92, 23);
            this.cmdDelete.TabIndex = 9;
            this.cmdDelete.Tag = "String_Delete";
            this.cmdDelete.Text = "Delete";
            this.cmdDelete.UseVisualStyleBackColor = true;
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.AutoSize = true;
            this.cmdOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdOK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOK.Location = new System.Drawing.Point(493, 0);
            this.cmdOK.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(101, 23);
            this.cmdOK.TabIndex = 6;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "Cancel";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdRestoreDefaults
            // 
            this.cmdRestoreDefaults.AutoSize = true;
            this.cmdRestoreDefaults.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRestoreDefaults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdRestoreDefaults.Location = new System.Drawing.Point(0, 0);
            this.cmdRestoreDefaults.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.cmdRestoreDefaults.Name = "cmdRestoreDefaults";
            this.cmdRestoreDefaults.Size = new System.Drawing.Size(95, 23);
            this.cmdRestoreDefaults.TabIndex = 8;
            this.cmdRestoreDefaults.Tag = "Button_Options_RestoreDefaults";
            this.cmdRestoreDefaults.Text = "Restore Defaults";
            this.cmdRestoreDefaults.UseVisualStyleBackColor = true;
            this.cmdRestoreDefaults.Click += new System.EventHandler(this.cmdRestoreDefaults_Click);
            // 
            // cmdSave
            // 
            this.cmdSave.AutoSize = true;
            this.cmdSave.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdSave.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdSave.Location = new System.Drawing.Point(395, 0);
            this.cmdSave.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdSave.Name = "cmdSave";
            this.cmdSave.Size = new System.Drawing.Size(92, 23);
            this.cmdSave.TabIndex = 5;
            this.cmdSave.Tag = "String_Save";
            this.cmdSave.Text = "Save";
            this.cmdSave.UseVisualStyleBackColor = true;
            this.cmdSave.Click += new System.EventHandler(this.cmdSave_Click);
            // 
            // cmdSaveAs
            // 
            this.cmdSaveAs.AutoSize = true;
            this.cmdSaveAs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdSaveAs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdSaveAs.Location = new System.Drawing.Point(297, 0);
            this.cmdSaveAs.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdSaveAs.Name = "cmdSaveAs";
            this.cmdSaveAs.Size = new System.Drawing.Size(92, 23);
            this.cmdSaveAs.TabIndex = 7;
            this.cmdSaveAs.Tag = "String_SaveAs";
            this.cmdSaveAs.Text = "Save As...";
            this.cmdSaveAs.UseVisualStyleBackColor = true;
            this.cmdSaveAs.Click += new System.EventHandler(this.cmdSaveAs_Click);
            // 
            // tboxDirectoryDescription
            // 
            this.tboxDirectoryDescription.BackColor = System.Drawing.SystemColors.Control;
            this.tlpDirectoryInfo.SetColumnSpan(this.tboxDirectoryDescription, 2);
            this.tboxDirectoryDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tboxDirectoryDescription.Location = new System.Drawing.Point(3, 79);
            this.tboxDirectoryDescription.Multiline = true;
            this.tboxDirectoryDescription.Name = "tboxDirectoryDescription";
            this.tboxDirectoryDescription.ReadOnly = true;
            this.tboxDirectoryDescription.Size = new System.Drawing.Size(468, 206);
            this.tboxDirectoryDescription.TabIndex = 13;
            // 
            // frmCharacterOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1264, 681);
            this.Controls.Add(this.tlpOptions);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.Name = "frmCharacterOptions";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_CharacterOptions";
            this.Text = "Character Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmCharacterOptions_FormClosing);
            this.Load += new System.EventHandler(this.frmCharacterOptions_Load);
            this.tlpOptions.ResumeLayout(false);
            this.tlpOptions.PerformLayout();
            this.tabOptions.ResumeLayout(false);
            this.tabBasicOptions.ResumeLayout(false);
            this.tabBasicOptions.PerformLayout();
            this.tlpBasicOptions.ResumeLayout(false);
            this.tlpBasicOptions.PerformLayout();
            this.gpbSourcebook.ResumeLayout(false);
            this.flpBasicOptions.ResumeLayout(false);
            this.flpBasicOptions.PerformLayout();
            this.gpbBasicOptionsCreateSettings.ResumeLayout(false);
            this.gpbBasicOptionsCreateSettings.PerformLayout();
            this.tlpBasicOptionsCreateSettings.ResumeLayout(false);
            this.tlpBasicOptionsCreateSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxAvail)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxNuyenKarma)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudQualityKarmaLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudStartingKarma)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSumToTen)).EndInit();
            this.gpbBasicOptionsOfficialRules.ResumeLayout(false);
            this.gpbBasicOptionsOfficialRules.PerformLayout();
            this.tlpBasicOptionsOfficialRules.ResumeLayout(false);
            this.tlpBasicOptionsOfficialRules.PerformLayout();
            this.gpbBasicOptionsCyberlimbs.ResumeLayout(false);
            this.gpbBasicOptionsCyberlimbs.PerformLayout();
            this.tlpBasicOptionsCyberlimbs.ResumeLayout(false);
            this.tlpBasicOptionsCyberlimbs.PerformLayout();
            this.tlpCyberlimbAttributeBonusCap.ResumeLayout(false);
            this.tlpCyberlimbAttributeBonusCap.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCyberlimbAttributeBonusCap)).EndInit();
            this.flpCyberlimbAttributeBonusCap.ResumeLayout(false);
            this.flpCyberlimbAttributeBonusCap.PerformLayout();
            this.flpRedlinerLimbs.ResumeLayout(false);
            this.flpRedlinerLimbs.PerformLayout();
            this.gpbBasicOptionsRounding.ResumeLayout(false);
            this.gpbBasicOptionsRounding.PerformLayout();
            this.tlpBasicOptionsRounding.ResumeLayout(false);
            this.tlpBasicOptionsRounding.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudNuyenDecimalsMinimum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNuyenDecimalsMaximum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEssenceDecimals)).EndInit();
            this.tabKarmaCosts.ResumeLayout(false);
            this.tabKarmaCosts.PerformLayout();
            this.tlpKarmaCosts.ResumeLayout(false);
            this.tlpKarmaCosts.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpecialization)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaKnowledgeSpecialization)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewKnowledgeSkill)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaLeaveGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewActiveSkill)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewSkillGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaImproveKnowledgeSkill)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaImproveActiveSkill)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaImproveSkillGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaAttribute)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaAlchemicalFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaBanishingFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaBindingFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaCenteringFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaCounterspellingFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaDisenchantingFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaFlexibleSignatureFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaMaskingFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaPowerFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaQiFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaRitualSpellcastingFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpellcastingFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpell)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaContact)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaEnemy)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpirit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewComplexForm)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaMysticAdeptPowerPoint)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNuyenPer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaJoinGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewAIProgram)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewAIAdvancedProgram)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaMetamagic)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaTechnique)).EndInit();
            this.flpKarmaInitiation.ResumeLayout(false);
            this.flpKarmaInitiation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaInitiation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaInitiationFlat)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaCarryover)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaQuality)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSummoningFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpellShapingFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSustainingFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaWeaponFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMetatypeCostsKarmaMultiplier)).EndInit();
            this.tabCustomData.ResumeLayout(false);
            this.tabCustomData.PerformLayout();
            this.tlpOptionalRules.ResumeLayout(false);
            this.tlpOptionalRules.PerformLayout();
            this.gbpDirectoryInfo.ResumeLayout(false);
            this.tlpDirectoryInfo.ResumeLayout(false);
            this.tlpDirectoryInfo.PerformLayout();
            this.gbpDirectoryInfoExclusivities.ResumeLayout(false);
            this.gbpDirectoryInfoDependencies.ResumeLayout(false);
            this.tabHouseRules.ResumeLayout(false);
            this.flpHouseRules.ResumeLayout(false);
            this.flpHouseRules.PerformLayout();
            this.gpbHouseRulesQualities.ResumeLayout(false);
            this.gpbHouseRulesQualities.PerformLayout();
            this.tlpHouseRulesQualities.ResumeLayout(false);
            this.tlpHouseRulesQualities.PerformLayout();
            this.gpbHouseRulesAttributes.ResumeLayout(false);
            this.gpbHouseRulesAttributes.PerformLayout();
            this.bufferedTableLayoutPanel1.ResumeLayout(false);
            this.bufferedTableLayoutPanel1.PerformLayout();
            this.flpDroneArmorMultiplier.ResumeLayout(false);
            this.flpDroneArmorMultiplier.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDroneArmorMultiplier)).EndInit();
            this.gpbHouseRulesSkills.ResumeLayout(false);
            this.gpbHouseRulesSkills.PerformLayout();
            this.tlpHouseRulesSkills.ResumeLayout(false);
            this.tlpHouseRulesSkills.PerformLayout();
            this.gpbHouseRulesCombat.ResumeLayout(false);
            this.gpbHouseRulesCombat.PerformLayout();
            this.bufferedTableLayoutPanel2.ResumeLayout(false);
            this.bufferedTableLayoutPanel2.PerformLayout();
            this.gpbHouseRulesMagicResonance.ResumeLayout(false);
            this.gpbHouseRulesMagicResonance.PerformLayout();
            this.tlpHouseRulesMagicResonance.ResumeLayout(false);
            this.tlpHouseRulesMagicResonance.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BufferedTableLayoutPanel tlpOptions;
        private System.Windows.Forms.Label lblSettingName;
        private ElasticComboBox cboSetting;
        private System.Windows.Forms.TabControl tabOptions;
        private System.Windows.Forms.TabPage tabBasicOptions;
        private BufferedTableLayoutPanel tlpBasicOptions;
        private System.Windows.Forms.TreeView treSourcebook;
        private ColorableCheckBox chkDontUseCyberlimbCalculation;
        private ColorableCheckBox chkEnforceCapacity;
        private ColorableCheckBox chkLicenseEachRestrictedItem;
        private System.Windows.Forms.Label lblEssenceDecimals;
        private System.Windows.Forms.Label lblNuyenDecimalsMaximumLabel;
        private System.Windows.Forms.Label lblNuyenDecimalsMinimumLabel;
        private ColorableCheckBox chkDontRoundEssenceInternally;
        private ColorableCheckBox chkDronemods;
        private ColorableCheckBox chkRestrictRecoil;
        private NumericUpDownEx nudNuyenDecimalsMinimum;
        private NumericUpDownEx nudNuyenDecimalsMaximum;
        private NumericUpDownEx nudEssenceDecimals;
        private ColorableCheckBox chkDronemodsMaximumPilot;
        private System.Windows.Forms.Label lblLimbCount;
        private ElasticComboBox cboLimbCount;
        private System.Windows.Forms.Button cmdEnableSourcebooks;
        private System.Windows.Forms.TabPage tabKarmaCosts;
        private BufferedTableLayoutPanel tlpKarmaCosts;
        private NumericUpDownEx nudKarmaMysticAdeptPowerPoint;
        private System.Windows.Forms.Label lblKarmaSpecialization;
        private System.Windows.Forms.Label lblKarmaMysticAdeptPowerPoint;
        private NumericUpDownEx nudKarmaSpecialization;
        private NumericUpDownEx nudKarmaNewAIAdvancedProgram;
        private System.Windows.Forms.Label lblKarmaWeaponFocusExtra;
        private NumericUpDownEx nudKarmaInitiationFlat;
        private NumericUpDownEx nudKarmaWeaponFocus;
        private System.Windows.Forms.Label lblKarmaNewAIAdvancedProgram;
        private System.Windows.Forms.Label lblKarmaWeaponFocus;
        private NumericUpDownEx nudKarmaNewAIProgram;
        private System.Windows.Forms.Label lblKarmaKnowledgeSpecialization;
        private System.Windows.Forms.Label lblKarmaRitualSpellcastingFocusExtra;
        private System.Windows.Forms.Label lblKarmaNewAIProgram;
        private NumericUpDownEx nudKarmaKnowledgeSpecialization;
        private NumericUpDownEx nudKarmaRitualSpellcastingFocus;
        private System.Windows.Forms.Label lblKarmaNewKnowledgeSkill;
        private System.Windows.Forms.Label lblKarmaRitualSpellcastingFocus;
        private NumericUpDownEx nudKarmaNewKnowledgeSkill;
        private System.Windows.Forms.Label lblFlexibleSignatureFocusExtra;
        private System.Windows.Forms.Label lblKarmaMetamagic;
        private System.Windows.Forms.Label lblKarmaInitiationExtra;
        private System.Windows.Forms.Label lblKarmaInitiationBracket;
        private NumericUpDownEx nudKarmaInitiation;
        private System.Windows.Forms.Label lblKarmaSpellShapingFocusExtra;
        private NumericUpDownEx nudKarmaFlexibleSignatureFocus;
        private NumericUpDownEx nudKarmaSpellShapingFocus;
        private System.Windows.Forms.Label lblKarmaInitiation;
        private NumericUpDownEx nudKarmaMetamagic;
        private System.Windows.Forms.Label lblKarmaCarryoverExtra;
        private System.Windows.Forms.Label lblKarmaSpellShapingFocus;
        private NumericUpDownEx nudKarmaCarryover;
        private System.Windows.Forms.Label lblKarmaFlexibleSignatureFocus;
        private System.Windows.Forms.Label lblKarmaCarryover;
        private System.Windows.Forms.Label lblKarmaSustainingFocusExtra;
        private System.Windows.Forms.Label lblKarmaEnemyExtra;
        private NumericUpDownEx nudKarmaEnemy;
        private System.Windows.Forms.Label lblKarmaEnemy;
        private System.Windows.Forms.Label lblKarmaContactExtra;
        private NumericUpDownEx nudKarmaContact;
        private System.Windows.Forms.Label lblKarmaContact;
        private System.Windows.Forms.Label lblKarmaJoinGroup;
        private NumericUpDownEx nudKarmaSustainingFocus;
        private NumericUpDownEx nudKarmaJoinGroup;
        private System.Windows.Forms.Label lblKarmaSustainingFocus;
        private System.Windows.Forms.Label lblKarmaLeaveGroup;
        private System.Windows.Forms.Label lblKarmaSummoningFocusExtra;
        private NumericUpDownEx nudKarmaLeaveGroup;
        private NumericUpDownEx nudKarmaSummoningFocus;
        private System.Windows.Forms.Label lblKarmaNewActiveSkill;
        private NumericUpDownEx nudKarmaTechnique;
        private System.Windows.Forms.Label lblKarmaSummoningFocus;
        private System.Windows.Forms.Label lblKarmaTechnique;
        private NumericUpDownEx nudKarmaNewActiveSkill;
        private System.Windows.Forms.Label lblKarmaSpellcastingFocusExtra;
        private System.Windows.Forms.Label lblKarmaAlchemicalFocus;
        private System.Windows.Forms.Label lblKarmaNuyenPerExtra;
        private NumericUpDownEx nudKarmaSpellcastingFocus;
        private NumericUpDownEx nudKarmaNuyenPer;
        private System.Windows.Forms.Label lblKarmaSpiritExtra;
        private System.Windows.Forms.Label lblKarmaNuyenPer;
        private NumericUpDownEx nudKarmaSpirit;
        private NumericUpDownEx nudKarmaAlchemicalFocus;
        private System.Windows.Forms.Label lblKarmaSpirit;
        private System.Windows.Forms.Label lblKarmaSpellcastingFocus;
        private System.Windows.Forms.Label lblKarmaAlchemicalFocusExtra;
        private System.Windows.Forms.Label lblKarmaNewSkillGroup;
        private NumericUpDownEx nudKarmaNewSkillGroup;
        private System.Windows.Forms.Label lblKarmaBanishingFocus;
        private NumericUpDownEx nudKarmaBanishingFocus;
        private System.Windows.Forms.Label lblKarmaBanishingFocusExtra;
        private System.Windows.Forms.Label lblKarmaImproveKnowledgeSkill;
        private System.Windows.Forms.Label lblKarmaQiFocusExtra;
        private NumericUpDownEx nudKarmaImproveKnowledgeSkill;
        private NumericUpDownEx nudKarmaQiFocus;
        private System.Windows.Forms.Label lblKarmaImproveKnowledgeSkillExtra;
        private System.Windows.Forms.Label lblKarmaQiFocus;
        private System.Windows.Forms.Label lblKarmaBindingFocus;
        private System.Windows.Forms.Label lblKarmaPowerFocusExtra;
        private NumericUpDownEx nudKarmaBindingFocus;
        private NumericUpDownEx nudKarmaPowerFocus;
        private System.Windows.Forms.Label lblKarmaBindingFocusExtra;
        private System.Windows.Forms.Label lblKarmaPowerFocus;
        private System.Windows.Forms.Label lblKarmaImproveActiveSkill;
        private System.Windows.Forms.Label lblKarmaMaskingFocusExtra;
        private NumericUpDownEx nudKarmaImproveActiveSkill;
        private NumericUpDownEx nudKarmaMaskingFocus;
        private System.Windows.Forms.Label lblKarmaImproveActiveSkillExtra;
        private System.Windows.Forms.Label lblKarmaMaskingFocus;
        private System.Windows.Forms.Label lblKarmaCenteringFocus;
        private NumericUpDownEx nudKarmaCenteringFocus;
        private System.Windows.Forms.Label lblKarmaCenteringFocusExtra;
        private System.Windows.Forms.Label lblKarmaCounterspellingFocus;
        private NumericUpDownEx nudKarmaCounterspellingFocus;
        private System.Windows.Forms.Label lblKarmaCounterspellingFocusExtra;
        private System.Windows.Forms.Label lblKarmaImproveSkillGroup;
        private System.Windows.Forms.Label lblKarmaDisenchantingFocusExtra;
        private NumericUpDownEx nudKarmaImproveSkillGroup;
        private NumericUpDownEx nudKarmaDisenchantingFocus;
        private System.Windows.Forms.Label lblKarmaImproveSkillGroupExtra;
        private System.Windows.Forms.Label lblKarmaDisenchantingFocus;
        private System.Windows.Forms.Label lblKarmaAttribute;
        private NumericUpDownEx nudKarmaAttribute;
        private System.Windows.Forms.Label lblKarmaAttributeExtra;
        private System.Windows.Forms.Label lblKarmaQuality;
        private NumericUpDownEx nudKarmaQuality;
        private System.Windows.Forms.Label lblKarmaQualityExtra;
        private System.Windows.Forms.Label lblKarmaSpell;
        private NumericUpDownEx nudKarmaSpell;
        private System.Windows.Forms.Label lblKarmaNewComplexForm;
        private NumericUpDownEx nudKarmaNewComplexForm;
        private NumericUpDownEx nudMetatypeCostsKarmaMultiplier;
        private System.Windows.Forms.Label lblMetatypeCostsKarmaMultiplierLabel;
        private System.Windows.Forms.TabPage tabCustomData;
        private BufferedTableLayoutPanel tlpOptionalRules;
        private System.Windows.Forms.Button cmdDecreaseCustomDirectoryLoadOrder;
        private System.Windows.Forms.Button cmdIncreaseCustomDirectoryLoadOrder;
        private System.Windows.Forms.Label lblCustomDataDirectoriesLabel;
        private System.Windows.Forms.TreeView treCustomDataDirectories;
        private System.Windows.Forms.TabPage tabHouseRules;
        private ColorableCheckBox chkIgnoreArt;
        private ColorableCheckBox chkExceedNegativeQualitiesLimit;
        private ColorableCheckBox chkExceedNegativeQualities;
        private ColorableCheckBox chkExceedPositiveQualitiesCostDoubled;
        private ColorableCheckBox chkExceedPositiveQualities;
        private ColorableCheckBox chkUnarmedSkillImprovements;
        private ColorableCheckBox chkCompensateSkillGroupKarmaDifference;
        private ColorableCheckBox chkMysAdeptSecondMAGAttribute;
        private ColorableCheckBox chkDontDoubleQualityPurchases;
        private ColorableCheckBox chkDontDoubleQualityRefunds;
        private ColorableCheckBox chkReverseAttributePriorityOrder;
        private ColorableCheckBox chkPrioritySpellsAsAdeptPowers;
        private ColorableCheckBox chkAllowInitiation;
        private ColorableCheckBox chkFreeMartialArtSpecialization;
        private ColorableCheckBox chkAllowCyberwareESSDiscounts;
        private ColorableCheckBox chkMysAdPp;
        private ColorableCheckBox chkESSLossReducesMaximumOnly;
        private ColorableCheckBox chkAlternateMetatypeAttributeKarma;
        private ColorableCheckBox chkUseCalculatedPublicAwareness;
        private NumericUpDownEx nudDroneArmorMultiplier;
        private System.Windows.Forms.Label lblDroneArmorMultiplierTimes;
        private ColorableCheckBox chkDroneArmorMultiplier;
        private ColorableCheckBox chkIgnoreComplexFormLimit;
        private ColorableCheckBox chkSpecialKarmaCost;
        private ColorableCheckBox chkMoreLethalGameplay;
        private ColorableCheckBox chkExtendAnyDetectionSpell;
        private ColorableCheckBox chkAllowSkillRegrouping;
        private ColorableCheckBox chkNoArmorEncumbrance;
        private ColorableCheckBox chkIncreasedImprovedAbilityModifier;
        private ColorableCheckBox chkAllowTechnomancerSchooling;
        private ColorableCheckBox chkUsePointsOnBrokenGroups;
        private ColorableCheckBox chkUnclampAttributeMinimum;
        private System.Windows.Forms.Button cmdSave;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.FlowLayoutPanel flpKarmaInitiation;
        private System.Windows.Forms.GroupBox gpbSourcebook;
        private System.Windows.Forms.FlowLayoutPanel flpBasicOptions;
        private System.Windows.Forms.GroupBox gpbBasicOptionsCyberlimbs;
        private System.Windows.Forms.GroupBox gpbBasicOptionsRounding;
        private BufferedTableLayoutPanel tlpBasicOptionsRounding;
        private BufferedTableLayoutPanel tlpBasicOptionsCyberlimbs;
        private ColorableCheckBox chkCyberlegMovement;
        private System.Windows.Forms.GroupBox gpbBasicOptionsOfficialRules;
        private BufferedTableLayoutPanel tlpBasicOptionsOfficialRules;
        private ColorableCheckBox chkStrictSkillGroups;
        private ColorableCheckBox chkAllowFreeGrids;
        private ColorableCheckBox chkAllowPointBuySpecializationsOnKarmaSkills;
        private BufferedTableLayoutPanel tlpCyberlimbAttributeBonusCap;
        private NumericUpDownEx nudCyberlimbAttributeBonusCap;
        private System.Windows.Forms.FlowLayoutPanel flpCyberlimbAttributeBonusCap;
        private ColorableCheckBox chkCyberlimbAttributeBonusCap;
        private System.Windows.Forms.Label lblCyberlimbAttributeBonusCapPlus;
        private ColorableCheckBox chkEnemyKarmaQualityLimit;
        private LabelWithToolTip lblContactPoints;
        private LabelWithToolTip lblKnowledgePoints;
        private System.Windows.Forms.TextBox txtContactPoints;
        private System.Windows.Forms.TextBox txtKnowledgePoints;
        private System.Windows.Forms.FlowLayoutPanel flpDroneArmorMultiplier;
        private System.Windows.Forms.GroupBox gpbBasicOptionsCreateSettings;
        private BufferedTableLayoutPanel tlpBasicOptionsCreateSettings;
        private System.Windows.Forms.Label lblBuildMethod;
        private ElasticComboBox cboBuildMethod;
        private LabelWithToolTip lblPriorities;
        private System.Windows.Forms.TextBox txtPriorities;
        private System.Windows.Forms.Label lblSumToTen;
        private NumericUpDownEx nudSumToTen;
        private System.Windows.Forms.Label lblStartingKarma;
        private System.Windows.Forms.Label lblMaxNuyenKarma;
        private ElasticComboBox cboPriorityTable;
        private System.Windows.Forms.Label lblPriorityTable;
        private System.Windows.Forms.Label lblMaxAvail;
        private System.Windows.Forms.Label lblRedlinerLimbs;
        private System.Windows.Forms.FlowLayoutPanel flpRedlinerLimbs;
        private ColorableCheckBox chkRedlinerLimbsSkull;
        private ColorableCheckBox chkRedlinerLimbsTorso;
        private ColorableCheckBox chkRedlinerLimbsArms;
        private ColorableCheckBox chkRedlinerLimbsLegs;
        private NumericUpDownEx nudStartingKarma;
        private NumericUpDownEx nudMaxAvail;
        private NumericUpDownEx nudMaxNuyenKarma;
        private System.Windows.Forms.Button cmdSaveAs;
        private System.Windows.Forms.Button cmdRestoreDefaults;
        private BufferedTableLayoutPanel tlpButtons;
        private System.Windows.Forms.Label lblAllowedCyberwareGrades;
        private System.Windows.Forms.FlowLayoutPanel flpAllowedCyberwareGrades;
        private System.Windows.Forms.Label lblQualityKarmaLimit;
        private NumericUpDownEx nudQualityKarmaLimit;
        private System.Windows.Forms.Button cmdDelete;
        private System.Windows.Forms.FlowLayoutPanel flpHouseRules;
        private System.Windows.Forms.GroupBox gpbHouseRulesQualities;
        private BufferedTableLayoutPanel tlpHouseRulesQualities;
        private System.Windows.Forms.GroupBox gpbHouseRulesAttributes;
        private BufferedTableLayoutPanel bufferedTableLayoutPanel1;
        private System.Windows.Forms.GroupBox gpbHouseRulesSkills;
        private BufferedTableLayoutPanel tlpHouseRulesSkills;
        private System.Windows.Forms.GroupBox gpbHouseRulesCombat;
        private BufferedTableLayoutPanel bufferedTableLayoutPanel2;
        private System.Windows.Forms.GroupBox gpbHouseRulesMagicResonance;
        private BufferedTableLayoutPanel tlpHouseRulesMagicResonance;
        private System.Windows.Forms.Button cmdGlobalOptionsCustomData;
        private System.Windows.Forms.Button cmdRename;
        private System.Windows.Forms.GroupBox gbpDirectoryInfo;
        private BufferedTableLayoutPanel tlpDirectoryInfo;
        private System.Windows.Forms.Label lblDirectoryNameLabel;
        private System.Windows.Forms.Label lblDirectoryVersion;
        private System.Windows.Forms.Label lblDirectoryName;
        private System.Windows.Forms.Label lblDirectoryVersionLabel;
        private System.Windows.Forms.Label lblDirectoryDescriptionLabel;
        private System.Windows.Forms.GroupBox gbpDirectoryInfoExclusivities;
        private System.Windows.Forms.FlowLayoutPanel flpExclusivities;
        private System.Windows.Forms.Label lblDirectoryAuthors;
        private System.Windows.Forms.GroupBox gbpDirectoryInfoDependencies;
        private System.Windows.Forms.FlowLayoutPanel flpDirectoryDependencies;
        private System.Windows.Forms.Button cmdManageDependencies;
        private System.Windows.Forms.Label lblDirectoryAuthorsLabel;
        private System.Windows.Forms.TextBox tboxDirectoryDescription;
    }
}
