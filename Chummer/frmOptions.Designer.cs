namespace Chummer
{
    partial class frmOptions
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
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.tlpOptions = new Chummer.BufferedTableLayoutPanel(this.components);
            this.txtSettingName = new System.Windows.Forms.TextBox();
            this.lblSetting = new System.Windows.Forms.Label();
            this.lblSettingName = new System.Windows.Forms.Label();
            this.cboSetting = new Chummer.ElasticComboBox();
            this.tabOptions = new System.Windows.Forms.TabControl();
            this.tabGlobal = new System.Windows.Forms.TabPage();
            this.tlpGlobal = new Chummer.BufferedTableLayoutPanel(this.components);
            this.chkAllowEasterEggs = new System.Windows.Forms.CheckBox();
            this.imgSheetLanguageFlag = new System.Windows.Forms.PictureBox();
            this.chkSearchInCategoryOnly = new System.Windows.Forms.CheckBox();
            this.cmdPDFAppPath = new System.Windows.Forms.Button();
            this.lblLanguage = new System.Windows.Forms.Label();
            this.lblPDFAppPath = new System.Windows.Forms.Label();
            this.chkHideCharacterRoster = new System.Windows.Forms.CheckBox();
            this.cmdVerify = new System.Windows.Forms.Button();
            this.cmdVerifyData = new System.Windows.Forms.Button();
            this.cboXSLT = new Chummer.ElasticComboBox();
            this.lblXSLT = new System.Windows.Forms.Label();
            this.chkLifeModule = new System.Windows.Forms.CheckBox();
            this.chkLiveCustomData = new System.Windows.Forms.CheckBox();
            this.chkUseLogging = new System.Windows.Forms.CheckBox();
            this.chkAutomaticUpdate = new System.Windows.Forms.CheckBox();
            this.chkOmaeEnabled = new System.Windows.Forms.CheckBox();
            this.chkStartupFullscreen = new System.Windows.Forms.CheckBox();
            this.chkSingleDiceRoller = new System.Windows.Forms.CheckBox();
            this.chkDatesIncludeTime = new System.Windows.Forms.CheckBox();
            this.lblPDFParametersLabel = new System.Windows.Forms.Label();
            this.cboPDFParameters = new Chummer.ElasticComboBox();
            this.txtPDFAppPath = new System.Windows.Forms.TextBox();
            this.grpSelectedSourcebook = new System.Windows.Forms.GroupBox();
            this.tlpSelectedSourcebook = new Chummer.BufferedTableLayoutPanel(this.components);
            this.txtPDFLocation = new System.Windows.Forms.TextBox();
            this.lblPDFLocation = new System.Windows.Forms.Label();
            this.cmdPDFLocation = new System.Windows.Forms.Button();
            this.lblPDFOffset = new System.Windows.Forms.Label();
            this.flpPDFOffset = new System.Windows.Forms.FlowLayoutPanel();
            this.nudPDFOffset = new System.Windows.Forms.NumericUpDown();
            this.cmdPDFTest = new System.Windows.Forms.Button();
            this.cboLanguage = new Chummer.ElasticComboBox();
            this.cboSheetLanguage = new Chummer.ElasticComboBox();
            this.chkConfirmDelete = new System.Windows.Forms.CheckBox();
            this.chkConfirmKarmaExpense = new System.Windows.Forms.CheckBox();
            this.chkHideItemsOverAvail = new System.Windows.Forms.CheckBox();
            this.chkAllowHoverIncrement = new System.Windows.Forms.CheckBox();
            this.chkLiveUpdateCleanCharacterFiles = new System.Windows.Forms.CheckBox();
            this.chkPreferNightlyBuilds = new System.Windows.Forms.CheckBox();
            this.lblCharacterRosterLabel = new System.Windows.Forms.Label();
            this.txtCharacterRosterPath = new System.Windows.Forms.TextBox();
            this.cmdCharacterRoster = new System.Windows.Forms.Button();
            this.chkCreateBackupOnCareer = new System.Windows.Forms.CheckBox();
            this.chkPrintToFileFirst = new System.Windows.Forms.CheckBox();
            this.grpCharacterDefaults = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel7 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cboBuildMethod = new Chummer.ElasticComboBox();
            this.cboDefaultGameplayOption = new System.Windows.Forms.ComboBox();
            this.lblEditSourcebookInfo = new System.Windows.Forms.Label();
            this.lstGlobalSourcebookInfos = new System.Windows.Forms.ListBox();
            this.imgLanguageFlag = new System.Windows.Forms.PictureBox();
            this.chkEnablePlugins = new System.Windows.Forms.CheckBox();
            this.nudBrowserVersion = new System.Windows.Forms.NumericUpDown();
            this.lblBrowserVersion = new System.Windows.Forms.Label();
            this.chkUseLoggingApplicationInsights = new System.Windows.Forms.CheckBox();
            this.tabCharacterOptions = new System.Windows.Forms.TabPage();
            this.tlpCharacterOptions = new Chummer.BufferedTableLayoutPanel(this.components);
            this.treSourcebook = new System.Windows.Forms.TreeView();
            this.chkPrintNotes = new System.Windows.Forms.CheckBox();
            this.lblSourcebooksToUse = new System.Windows.Forms.Label();
            this.chkPrintExpenses = new System.Windows.Forms.CheckBox();
            this.chkPrintSkillsWithZeroRating = new System.Windows.Forms.CheckBox();
            this.chkDontUseCyberlimbCalculation = new System.Windows.Forms.CheckBox();
            this.chkAllowSkillDiceRolling = new System.Windows.Forms.CheckBox();
            this.chkEnforceCapacity = new System.Windows.Forms.CheckBox();
            this.chkLicenseEachRestrictedItem = new System.Windows.Forms.CheckBox();
            this.lblEssenceDecimals = new System.Windows.Forms.Label();
            this.lblNuyenDecimalsMaximumLabel = new System.Windows.Forms.Label();
            this.lblNuyenDecimalsMinimumLabel = new System.Windows.Forms.Label();
            this.chkDontRoundEssenceInternally = new System.Windows.Forms.CheckBox();
            this.chkDronemods = new System.Windows.Forms.CheckBox();
            this.chkRestrictRecoil = new System.Windows.Forms.CheckBox();
            this.nudNuyenDecimalsMinimum = new System.Windows.Forms.NumericUpDown();
            this.nudNuyenDecimalsMaximum = new System.Windows.Forms.NumericUpDown();
            this.nudEssenceDecimals = new System.Windows.Forms.NumericUpDown();
            this.chkDronemodsMaximumPilot = new System.Windows.Forms.CheckBox();
            this.chkPrintFreeExpenses = new System.Windows.Forms.CheckBox();
            this.lblLimbCount = new System.Windows.Forms.Label();
            this.cboLimbCount = new Chummer.ElasticComboBox();
            this.cmdEnableSourcebooks = new System.Windows.Forms.Button();
            this.tabKarmaCosts = new System.Windows.Forms.TabPage();
            this.tlpKarmaCosts = new System.Windows.Forms.TableLayoutPanel();
            this.tlpKarmaCostsList = new Chummer.BufferedTableLayoutPanel(this.components);
            this.nudKarmaMysticAdeptPowerPoint = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaSpecialization = new System.Windows.Forms.Label();
            this.lblKarmaMysticAdeptPowerPoint = new System.Windows.Forms.Label();
            this.nudKarmaSpecialization = new System.Windows.Forms.NumericUpDown();
            this.nudKarmaNewAIAdvancedProgram = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaWeaponFocusExtra = new System.Windows.Forms.Label();
            this.nudKarmaInitiationFlat = new System.Windows.Forms.NumericUpDown();
            this.nudKarmaWeaponFocus = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaNewAIAdvancedProgram = new System.Windows.Forms.Label();
            this.lblKarmaWeaponFocus = new System.Windows.Forms.Label();
            this.nudKarmaNewAIProgram = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaKnowledgeSpecialization = new System.Windows.Forms.Label();
            this.lblKarmaRitualSpellcastingFocusExtra = new System.Windows.Forms.Label();
            this.lblKarmaNewAIProgram = new System.Windows.Forms.Label();
            this.nudKarmaKnowledgeSpecialization = new System.Windows.Forms.NumericUpDown();
            this.nudKarmaRitualSpellcastingFocus = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaNewKnowledgeSkill = new System.Windows.Forms.Label();
            this.lblKarmaRitualSpellcastingFocus = new System.Windows.Forms.Label();
            this.nudKarmaNewKnowledgeSkill = new System.Windows.Forms.NumericUpDown();
            this.lblFlexibleSignatureFocusExtra = new System.Windows.Forms.Label();
            this.lblKarmaMetamagic = new System.Windows.Forms.Label();
            this.lblKarmaInitiationExtra = new System.Windows.Forms.Label();
            this.lblKarmaInitiationBracket = new System.Windows.Forms.Label();
            this.nudKarmaInitiation = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaSpellShapingFocusExtra = new System.Windows.Forms.Label();
            this.nudKarmaFlexibleSignatureFocus = new System.Windows.Forms.NumericUpDown();
            this.nudKarmaSpellShapingFocus = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaInitiation = new System.Windows.Forms.Label();
            this.nudKarmaMetamagic = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaCarryoverExtra = new System.Windows.Forms.Label();
            this.lblKarmaSpellShapingFocus = new System.Windows.Forms.Label();
            this.nudKarmaCarryover = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaFlexibleSignatureFocus = new System.Windows.Forms.Label();
            this.lblKarmaCarryover = new System.Windows.Forms.Label();
            this.lblKarmaSustainingFocusExtra = new System.Windows.Forms.Label();
            this.lblKarmaEnemyExtra = new System.Windows.Forms.Label();
            this.nudKarmaEnemy = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaEnemy = new System.Windows.Forms.Label();
            this.lblKarmaContactExtra = new System.Windows.Forms.Label();
            this.nudKarmaContact = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaContact = new System.Windows.Forms.Label();
            this.lblKarmaJoinGroup = new System.Windows.Forms.Label();
            this.nudKarmaSustainingFocus = new System.Windows.Forms.NumericUpDown();
            this.nudKarmaJoinGroup = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaSustainingFocus = new System.Windows.Forms.Label();
            this.lblKarmaLeaveGroup = new System.Windows.Forms.Label();
            this.lblKarmaSummoningFocusExtra = new System.Windows.Forms.Label();
            this.nudKarmaLeaveGroup = new System.Windows.Forms.NumericUpDown();
            this.nudKarmaSummoningFocus = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaNewActiveSkill = new System.Windows.Forms.Label();
            this.nudKarmaManeuver = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaSummoningFocus = new System.Windows.Forms.Label();
            this.lblKarmaManeuver = new System.Windows.Forms.Label();
            this.nudKarmaNewActiveSkill = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaComplexFormSkillsoftExtra = new System.Windows.Forms.Label();
            this.lblKarmaSpellcastingFocusExtra = new System.Windows.Forms.Label();
            this.nudKarmaComplexFormSkillsoft = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaAlchemicalFocus = new System.Windows.Forms.Label();
            this.lblKarmaComplexFormSkillsoft = new System.Windows.Forms.Label();
            this.lblKarmaNuyenPerExtra = new System.Windows.Forms.Label();
            this.nudKarmaSpellcastingFocus = new System.Windows.Forms.NumericUpDown();
            this.nudKarmaNuyenPer = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaSpiritExtra = new System.Windows.Forms.Label();
            this.lblKarmaNuyenPer = new System.Windows.Forms.Label();
            this.lblKarmaComplexFormOptionExtra = new System.Windows.Forms.Label();
            this.nudKarmaSpirit = new System.Windows.Forms.NumericUpDown();
            this.nudKarmaAlchemicalFocus = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaSpirit = new System.Windows.Forms.Label();
            this.nudKarmaComplexFormOption = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaSpellcastingFocus = new System.Windows.Forms.Label();
            this.lblKarmaComplexFormOption = new System.Windows.Forms.Label();
            this.lblKarmaAlchemicalFocusExtra = new System.Windows.Forms.Label();
            this.lblKarmaNewSkillGroup = new System.Windows.Forms.Label();
            this.nudKarmaNewSkillGroup = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaBanishingFocus = new System.Windows.Forms.Label();
            this.nudKarmaBanishingFocus = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaBanishingFocusExtra = new System.Windows.Forms.Label();
            this.lblKarmaImproveKnowledgeSkill = new System.Windows.Forms.Label();
            this.lblKarmaQiFocusExtra = new System.Windows.Forms.Label();
            this.nudKarmaImproveKnowledgeSkill = new System.Windows.Forms.NumericUpDown();
            this.nudKarmaQiFocus = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaImproveKnowledgeSkillExtra = new System.Windows.Forms.Label();
            this.lblKarmaQiFocus = new System.Windows.Forms.Label();
            this.lblKarmaBindingFocus = new System.Windows.Forms.Label();
            this.lblKarmaPowerFocusExtra = new System.Windows.Forms.Label();
            this.nudKarmaBindingFocus = new System.Windows.Forms.NumericUpDown();
            this.nudKarmaPowerFocus = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaBindingFocusExtra = new System.Windows.Forms.Label();
            this.lblKarmaPowerFocus = new System.Windows.Forms.Label();
            this.lblKarmaImproveActiveSkill = new System.Windows.Forms.Label();
            this.lblKarmaMaskingFocusExtra = new System.Windows.Forms.Label();
            this.nudKarmaImproveActiveSkill = new System.Windows.Forms.NumericUpDown();
            this.nudKarmaMaskingFocus = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaImproveActiveSkillExtra = new System.Windows.Forms.Label();
            this.lblKarmaMaskingFocus = new System.Windows.Forms.Label();
            this.lblKarmaCenteringFocus = new System.Windows.Forms.Label();
            this.nudKarmaCenteringFocus = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaCenteringFocusExtra = new System.Windows.Forms.Label();
            this.lblKarmaCounterspellingFocus = new System.Windows.Forms.Label();
            this.nudKarmaCounterspellingFocus = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaCounterspellingFocusExtra = new System.Windows.Forms.Label();
            this.lblKarmaImproveSkillGroup = new System.Windows.Forms.Label();
            this.lblKarmaDisenchantingFocusExtra = new System.Windows.Forms.Label();
            this.nudKarmaImproveSkillGroup = new System.Windows.Forms.NumericUpDown();
            this.nudKarmaDisenchantingFocus = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaImproveSkillGroupExtra = new System.Windows.Forms.Label();
            this.lblKarmaDisenchantingFocus = new System.Windows.Forms.Label();
            this.lblKarmaAttribute = new System.Windows.Forms.Label();
            this.nudKarmaAttribute = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaAttributeExtra = new System.Windows.Forms.Label();
            this.lblKarmaQuality = new System.Windows.Forms.Label();
            this.lblKarmaImproveComplexFormExtra = new System.Windows.Forms.Label();
            this.nudKarmaQuality = new System.Windows.Forms.NumericUpDown();
            this.nudKarmaImproveComplexForm = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaQualityExtra = new System.Windows.Forms.Label();
            this.lblKarmaImproveComplexForm = new System.Windows.Forms.Label();
            this.lblKarmaSpell = new System.Windows.Forms.Label();
            this.nudKarmaSpell = new System.Windows.Forms.NumericUpDown();
            this.lblKarmaNewComplexForm = new System.Windows.Forms.Label();
            this.nudKarmaNewComplexForm = new System.Windows.Forms.NumericUpDown();
            this.nudMetatypeCostsKarmaMultiplier = new System.Windows.Forms.NumericUpDown();
            this.lblMetatypeCostsKarmaMultiplierLabel = new System.Windows.Forms.Label();
            this.lblNuyenPerBP = new System.Windows.Forms.Label();
            this.nudNuyenPerBP = new System.Windows.Forms.NumericUpDown();
            this.cmdRestoreDefaultsKarma = new System.Windows.Forms.Button();
            this.tabOptionalRules = new System.Windows.Forms.TabPage();
            this.tlpOptionalRules = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdDecreaseCustomDirectoryLoadOrder = new System.Windows.Forms.Button();
            this.cmdIncreaseCustomDirectoryLoadOrder = new System.Windows.Forms.Button();
            this.lblCustomDataDirectoriesLabel = new System.Windows.Forms.Label();
            this.cmdAddCustomDirectory = new System.Windows.Forms.Button();
            this.treCustomDataDirectories = new System.Windows.Forms.TreeView();
            this.cmdRenameCustomDataDirectory = new System.Windows.Forms.Button();
            this.cmdRemoveCustomDirectory = new System.Windows.Forms.Button();
            this.tabHouseRules = new System.Windows.Forms.TabPage();
            this.tlpHouseRules = new Chummer.BufferedTableLayoutPanel(this.components);
            this.chkNoArmorEncumbrance = new System.Windows.Forms.CheckBox();
            this.chkIgnoreArt = new System.Windows.Forms.CheckBox();
            this.chkExceedNegativeQualitiesLimit = new System.Windows.Forms.CheckBox();
            this.chkUseTotalValueForFreeKnowledge = new System.Windows.Forms.CheckBox();
            this.chkExceedNegativeQualities = new System.Windows.Forms.CheckBox();
            this.chkEnemyKarmaQualityLimit = new System.Windows.Forms.CheckBox();
            this.chkExceedPositiveQualitiesCostDoubled = new System.Windows.Forms.CheckBox();
            this.chkExceedPositiveQualities = new System.Windows.Forms.CheckBox();
            this.chkUnarmedSkillImprovements = new System.Windows.Forms.CheckBox();
            this.chkCompensateSkillGroupKarmaDifference = new System.Windows.Forms.CheckBox();
            this.chkCyberlegMovement = new System.Windows.Forms.CheckBox();
            this.chkMysAdeptSecondMAGAttribute = new System.Windows.Forms.CheckBox();
            this.chkDontDoubleQualityPurchases = new System.Windows.Forms.CheckBox();
            this.chkAllowPointBuySpecializationsOnKarmaSkills = new System.Windows.Forms.CheckBox();
            this.chkDontDoubleQualityRefunds = new System.Windows.Forms.CheckBox();
            this.chkReverseAttributePriorityOrder = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkStrictSkillGroups = new System.Windows.Forms.CheckBox();
            this.nudContactMultiplier = new System.Windows.Forms.NumericUpDown();
            this.chkPrioritySpellsAsAdeptPowers = new System.Windows.Forms.CheckBox();
            this.chkAllowInitiation = new System.Windows.Forms.CheckBox();
            this.chkFreeMartialArtSpecialization = new System.Windows.Forms.CheckBox();
            this.chkAllowCyberwareESSDiscounts = new System.Windows.Forms.CheckBox();
            this.chkMysAdPp = new System.Windows.Forms.CheckBox();
            this.chkESSLossReducesMaximumOnly = new System.Windows.Forms.CheckBox();
            this.chkAlternateMetatypeAttributeKarma = new System.Windows.Forms.CheckBox();
            this.chkUseCalculatedPublicAwareness = new System.Windows.Forms.CheckBox();
            this.nudDroneArmorMultiplier = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.chkDroneArmorMultiplier = new System.Windows.Forms.CheckBox();
            this.chkContactMultiplier = new System.Windows.Forms.CheckBox();
            this.chkKnowledgeMultiplier = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.nudKnowledgeMultiplier = new System.Windows.Forms.NumericUpDown();
            this.chkUseTotalValueForFreeContacts = new System.Windows.Forms.CheckBox();
            this.chkAllowSkillRegrouping = new System.Windows.Forms.CheckBox();
            this.chkExtendAnyDetectionSpell = new System.Windows.Forms.CheckBox();
            this.chkMoreLethalGameplay = new System.Windows.Forms.CheckBox();
            this.chkSpecialKarmaCost = new System.Windows.Forms.CheckBox();
            this.chkIgnoreComplexFormLimit = new System.Windows.Forms.CheckBox();
            this.tabGitHubIssues = new System.Windows.Forms.TabPage();
            this.cmdUploadPastebin = new System.Windows.Forms.Button();
            this.tabPlugins = new System.Windows.Forms.TabPage();
            this.bufferedTableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.grpAvailablePlugins = new System.Windows.Forms.GroupBox();
            this.clbPlugins = new System.Windows.Forms.CheckedListBox();
            this.panelPluginOption = new System.Windows.Forms.Panel();
            this.flpOKCancel = new System.Windows.Forms.FlowLayoutPanel();
            this.tlpOptions.SuspendLayout();
            this.tabOptions.SuspendLayout();
            this.tabGlobal.SuspendLayout();
            this.tlpGlobal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgSheetLanguageFlag)).BeginInit();
            this.grpSelectedSourcebook.SuspendLayout();
            this.tlpSelectedSourcebook.SuspendLayout();
            this.flpPDFOffset.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPDFOffset)).BeginInit();
            this.grpCharacterDefaults.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgLanguageFlag)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBrowserVersion)).BeginInit();
            this.tabCharacterOptions.SuspendLayout();
            this.tlpCharacterOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudNuyenDecimalsMinimum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNuyenDecimalsMaximum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEssenceDecimals)).BeginInit();
            this.tabKarmaCosts.SuspendLayout();
            this.tlpKarmaCosts.SuspendLayout();
            this.tlpKarmaCostsList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaMysticAdeptPowerPoint)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpecialization)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewAIAdvancedProgram)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaInitiationFlat)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaWeaponFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewAIProgram)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaKnowledgeSpecialization)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaRitualSpellcastingFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewKnowledgeSkill)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaInitiation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaFlexibleSignatureFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpellShapingFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaMetamagic)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaCarryover)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaEnemy)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaContact)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSustainingFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaJoinGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaLeaveGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSummoningFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaManeuver)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewActiveSkill)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaComplexFormSkillsoft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpellcastingFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNuyenPer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpirit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaAlchemicalFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaComplexFormOption)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewSkillGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaBanishingFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaImproveKnowledgeSkill)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaQiFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaBindingFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaPowerFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaImproveActiveSkill)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaMaskingFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaCenteringFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaCounterspellingFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaImproveSkillGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaDisenchantingFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaAttribute)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaQuality)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaImproveComplexForm)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpell)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewComplexForm)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMetatypeCostsKarmaMultiplier)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNuyenPerBP)).BeginInit();
            this.tabOptionalRules.SuspendLayout();
            this.tlpOptionalRules.SuspendLayout();
            this.tabHouseRules.SuspendLayout();
            this.tlpHouseRules.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudContactMultiplier)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDroneArmorMultiplier)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKnowledgeMultiplier)).BeginInit();
            this.tabGitHubIssues.SuspendLayout();
            this.tabPlugins.SuspendLayout();
            this.bufferedTableLayoutPanel1.SuspendLayout();
            this.grpAvailablePlugins.SuspendLayout();
            this.flpOKCancel.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdOK
            // 
            this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdOK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOK.Location = new System.Drawing.Point(86, 0);
            this.cmdOK.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(80, 30);
            this.cmdOK.TabIndex = 5;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.AutoSize = true;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(0, 0);
            this.cmdCancel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(80, 30);
            this.cmdCancel.TabIndex = 6;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // tlpOptions
            // 
            this.tlpOptions.AutoSize = true;
            this.tlpOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpOptions.ColumnCount = 4;
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpOptions.Controls.Add(this.txtSettingName, 3, 0);
            this.tlpOptions.Controls.Add(this.lblSetting, 0, 0);
            this.tlpOptions.Controls.Add(this.lblSettingName, 2, 0);
            this.tlpOptions.Controls.Add(this.cboSetting, 1, 0);
            this.tlpOptions.Controls.Add(this.tabOptions, 0, 1);
            this.tlpOptions.Controls.Add(this.flpOKCancel, 0, 2);
            this.tlpOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpOptions.Location = new System.Drawing.Point(9, 9);
            this.tlpOptions.Name = "tlpOptions";
            this.tlpOptions.RowCount = 3;
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.Size = new System.Drawing.Size(930, 749);
            this.tlpOptions.TabIndex = 6;
            // 
            // txtSettingName
            // 
            this.txtSettingName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSettingName.Location = new System.Drawing.Point(544, 3);
            this.txtSettingName.Name = "txtSettingName";
            this.txtSettingName.Size = new System.Drawing.Size(383, 20);
            this.txtSettingName.TabIndex = 3;
            // 
            // lblSetting
            // 
            this.lblSetting.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSetting.AutoSize = true;
            this.lblSetting.Location = new System.Drawing.Point(3, 6);
            this.lblSetting.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSetting.Name = "lblSetting";
            this.lblSetting.Size = new System.Drawing.Size(67, 13);
            this.lblSetting.TabIndex = 0;
            this.lblSetting.Tag = "Label_Options_SettingsFile";
            this.lblSetting.Text = "Settings File:";
            // 
            // lblSettingName
            // 
            this.lblSettingName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSettingName.AutoSize = true;
            this.lblSettingName.Location = new System.Drawing.Point(464, 6);
            this.lblSettingName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSettingName.Name = "lblSettingName";
            this.lblSettingName.Size = new System.Drawing.Size(74, 13);
            this.lblSettingName.TabIndex = 2;
            this.lblSettingName.Tag = "Label_Options_SettingName";
            this.lblSettingName.Text = "Setting Name:";
            // 
            // cboSetting
            // 
            this.cboSetting.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSetting.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSetting.FormattingEnabled = true;
            this.cboSetting.Location = new System.Drawing.Point(76, 3);
            this.cboSetting.Name = "cboSetting";
            this.cboSetting.Size = new System.Drawing.Size(382, 21);
            this.cboSetting.TabIndex = 1;
            this.cboSetting.TooltipText = "";
            this.cboSetting.SelectedIndexChanged += new System.EventHandler(this.cboSetting_SelectedIndexChanged);
            // 
            // tabOptions
            // 
            this.tlpOptions.SetColumnSpan(this.tabOptions, 4);
            this.tabOptions.Controls.Add(this.tabGlobal);
            this.tabOptions.Controls.Add(this.tabCharacterOptions);
            this.tabOptions.Controls.Add(this.tabKarmaCosts);
            this.tabOptions.Controls.Add(this.tabOptionalRules);
            this.tabOptions.Controls.Add(this.tabHouseRules);
            this.tabOptions.Controls.Add(this.tabGitHubIssues);
            this.tabOptions.Controls.Add(this.tabPlugins);
            this.tabOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabOptions.Location = new System.Drawing.Point(3, 30);
            this.tabOptions.Name = "tabOptions";
            this.tabOptions.SelectedIndex = 0;
            this.tabOptions.Size = new System.Drawing.Size(924, 680);
            this.tabOptions.TabIndex = 4;
            // 
            // tabGlobal
            // 
            this.tabGlobal.BackColor = System.Drawing.SystemColors.Control;
            this.tabGlobal.Controls.Add(this.tlpGlobal);
            this.tabGlobal.Location = new System.Drawing.Point(4, 22);
            this.tabGlobal.Name = "tabGlobal";
            this.tabGlobal.Padding = new System.Windows.Forms.Padding(9);
            this.tabGlobal.Size = new System.Drawing.Size(916, 654);
            this.tabGlobal.TabIndex = 5;
            this.tabGlobal.Tag = "Tab_Options_Global";
            this.tabGlobal.Text = "Global Options";
            // 
            // tlpGlobal
            // 
            this.tlpGlobal.AutoSize = true;
            this.tlpGlobal.ColumnCount = 8;
            this.tlpGlobal.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 301F));
            this.tlpGlobal.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpGlobal.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tlpGlobal.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28.27586F));
            this.tlpGlobal.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 17.24138F));
            this.tlpGlobal.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.58621F));
            this.tlpGlobal.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 9.655171F));
            this.tlpGlobal.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 17.24138F));
            this.tlpGlobal.Controls.Add(this.chkAllowEasterEggs, 0, 5);
            this.tlpGlobal.Controls.Add(this.imgSheetLanguageFlag, 2, 1);
            this.tlpGlobal.Controls.Add(this.chkSearchInCategoryOnly, 4, 9);
            this.tlpGlobal.Controls.Add(this.cmdPDFAppPath, 5, 19);
            this.tlpGlobal.Controls.Add(this.lblLanguage, 1, 0);
            this.tlpGlobal.Controls.Add(this.lblPDFAppPath, 1, 19);
            this.tlpGlobal.Controls.Add(this.chkHideCharacterRoster, 1, 13);
            this.tlpGlobal.Controls.Add(this.cmdVerify, 5, 0);
            this.tlpGlobal.Controls.Add(this.cmdVerifyData, 6, 0);
            this.tlpGlobal.Controls.Add(this.cboXSLT, 5, 1);
            this.tlpGlobal.Controls.Add(this.lblXSLT, 1, 1);
            this.tlpGlobal.Controls.Add(this.chkLifeModule, 1, 2);
            this.tlpGlobal.Controls.Add(this.chkLiveCustomData, 1, 6);
            this.tlpGlobal.Controls.Add(this.chkUseLogging, 1, 3);
            this.tlpGlobal.Controls.Add(this.chkAutomaticUpdate, 1, 4);
            this.tlpGlobal.Controls.Add(this.chkOmaeEnabled, 1, 10);
            this.tlpGlobal.Controls.Add(this.chkStartupFullscreen, 1, 7);
            this.tlpGlobal.Controls.Add(this.chkSingleDiceRoller, 1, 8);
            this.tlpGlobal.Controls.Add(this.chkDatesIncludeTime, 1, 9);
            this.tlpGlobal.Controls.Add(this.lblPDFParametersLabel, 1, 18);
            this.tlpGlobal.Controls.Add(this.cboPDFParameters, 2, 18);
            this.tlpGlobal.Controls.Add(this.txtPDFAppPath, 2, 19);
            this.tlpGlobal.Controls.Add(this.grpSelectedSourcebook, 1, 20);
            this.tlpGlobal.Controls.Add(this.cboLanguage, 3, 0);
            this.tlpGlobal.Controls.Add(this.cboSheetLanguage, 3, 1);
            this.tlpGlobal.Controls.Add(this.chkConfirmDelete, 4, 2);
            this.tlpGlobal.Controls.Add(this.chkConfirmKarmaExpense, 4, 3);
            this.tlpGlobal.Controls.Add(this.chkHideItemsOverAvail, 4, 4);
            this.tlpGlobal.Controls.Add(this.chkAllowHoverIncrement, 4, 7);
            this.tlpGlobal.Controls.Add(this.chkLiveUpdateCleanCharacterFiles, 1, 11);
            this.tlpGlobal.Controls.Add(this.chkPreferNightlyBuilds, 1, 12);
            this.tlpGlobal.Controls.Add(this.lblCharacterRosterLabel, 1, 17);
            this.tlpGlobal.Controls.Add(this.txtCharacterRosterPath, 2, 17);
            this.tlpGlobal.Controls.Add(this.cmdCharacterRoster, 5, 17);
            this.tlpGlobal.Controls.Add(this.chkCreateBackupOnCareer, 1, 16);
            this.tlpGlobal.Controls.Add(this.chkPrintToFileFirst, 1, 14);
            this.tlpGlobal.Controls.Add(this.grpCharacterDefaults, 4, 12);
            this.tlpGlobal.Controls.Add(this.lblEditSourcebookInfo, 0, 0);
            this.tlpGlobal.Controls.Add(this.lstGlobalSourcebookInfos, 0, 1);
            this.tlpGlobal.Controls.Add(this.imgLanguageFlag, 2, 0);
            this.tlpGlobal.Controls.Add(this.chkEnablePlugins, 4, 6);
            this.tlpGlobal.Controls.Add(this.nudBrowserVersion, 3, 15);
            this.tlpGlobal.Controls.Add(this.lblBrowserVersion, 1, 15);
            this.tlpGlobal.Controls.Add(this.chkUseLoggingApplicationInsights, 2, 3);
            this.tlpGlobal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpGlobal.Location = new System.Drawing.Point(9, 9);
            this.tlpGlobal.Name = "tlpGlobal";
            this.tlpGlobal.RowCount = 21;
            this.tlpGlobal.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobal.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobal.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobal.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobal.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobal.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobal.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobal.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobal.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobal.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobal.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobal.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobal.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobal.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobal.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobal.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobal.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobal.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobal.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobal.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobal.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGlobal.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpGlobal.Size = new System.Drawing.Size(898, 636);
            this.tlpGlobal.TabIndex = 39;
            // 
            // chkAllowEasterEggs
            // 
            this.chkAllowEasterEggs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAllowEasterEggs.AutoSize = true;
            this.tlpGlobal.SetColumnSpan(this.chkAllowEasterEggs, 3);
            this.chkAllowEasterEggs.Location = new System.Drawing.Point(304, 147);
            this.chkAllowEasterEggs.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkAllowEasterEggs.Name = "chkAllowEasterEggs";
            this.chkAllowEasterEggs.Size = new System.Drawing.Size(295, 17);
            this.chkAllowEasterEggs.TabIndex = 52;
            this.chkAllowEasterEggs.Tag = "Checkbox_Options_AllowEasterEggs";
            this.chkAllowEasterEggs.Text = "Allow Easter Eggs";
            this.chkAllowEasterEggs.UseVisualStyleBackColor = true;
            this.chkAllowEasterEggs.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // imgSheetLanguageFlag
            // 
            this.imgSheetLanguageFlag.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.imgSheetLanguageFlag.Location = new System.Drawing.Point(467, 37);
            this.imgSheetLanguageFlag.Name = "imgSheetLanguageFlag";
            this.imgSheetLanguageFlag.Size = new System.Drawing.Size(16, 28);
            this.imgSheetLanguageFlag.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.imgSheetLanguageFlag.TabIndex = 50;
            this.imgSheetLanguageFlag.TabStop = false;
            // 
            // chkSearchInCategoryOnly
            // 
            this.chkSearchInCategoryOnly.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkSearchInCategoryOnly.AutoSize = true;
            this.chkSearchInCategoryOnly.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkSearchInCategoryOnly.Checked = true;
            this.chkSearchInCategoryOnly.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tlpGlobal.SetColumnSpan(this.chkSearchInCategoryOnly, 4);
            this.chkSearchInCategoryOnly.Location = new System.Drawing.Point(605, 246);
            this.chkSearchInCategoryOnly.Name = "chkSearchInCategoryOnly";
            this.tlpGlobal.SetRowSpan(this.chkSearchInCategoryOnly, 2);
            this.chkSearchInCategoryOnly.Size = new System.Drawing.Size(290, 44);
            this.chkSearchInCategoryOnly.TabIndex = 21;
            this.chkSearchInCategoryOnly.Tag = "Checkbox_Options_SearchInCategoryOnly";
            this.chkSearchInCategoryOnly.Text = "Searching in selection forms is restricted to the current Category";
            this.chkSearchInCategoryOnly.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkSearchInCategoryOnly.UseVisualStyleBackColor = true;
            this.chkSearchInCategoryOnly.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // cmdPDFAppPath
            // 
            this.cmdPDFAppPath.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmdPDFAppPath.AutoSize = true;
            this.cmdPDFAppPath.Location = new System.Drawing.Point(676, 506);
            this.cmdPDFAppPath.Name = "cmdPDFAppPath";
            this.cmdPDFAppPath.Size = new System.Drawing.Size(26, 23);
            this.cmdPDFAppPath.TabIndex = 11;
            this.cmdPDFAppPath.Text = "...";
            this.cmdPDFAppPath.UseVisualStyleBackColor = true;
            this.cmdPDFAppPath.Click += new System.EventHandler(this.cmdPDFAppPath_Click);
            // 
            // lblLanguage
            // 
            this.lblLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLanguage.AutoSize = true;
            this.lblLanguage.Location = new System.Drawing.Point(403, 6);
            this.lblLanguage.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Size = new System.Drawing.Size(58, 13);
            this.lblLanguage.TabIndex = 0;
            this.lblLanguage.Tag = "Label_Options_Language";
            this.lblLanguage.Text = "Language:";
            // 
            // lblPDFAppPath
            // 
            this.lblPDFAppPath.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblPDFAppPath.AutoSize = true;
            this.lblPDFAppPath.Location = new System.Drawing.Point(320, 511);
            this.lblPDFAppPath.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPDFAppPath.Name = "lblPDFAppPath";
            this.lblPDFAppPath.Size = new System.Drawing.Size(141, 13);
            this.lblPDFAppPath.TabIndex = 9;
            this.lblPDFAppPath.Tag = "Label_Options_PDFApplicationPath";
            this.lblPDFAppPath.Text = "Location of PDF application:";
            // 
            // chkHideCharacterRoster
            // 
            this.chkHideCharacterRoster.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkHideCharacterRoster.AutoSize = true;
            this.tlpGlobal.SetColumnSpan(this.chkHideCharacterRoster, 3);
            this.chkHideCharacterRoster.Location = new System.Drawing.Point(304, 347);
            this.chkHideCharacterRoster.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkHideCharacterRoster.Name = "chkHideCharacterRoster";
            this.chkHideCharacterRoster.Size = new System.Drawing.Size(295, 17);
            this.chkHideCharacterRoster.TabIndex = 35;
            this.chkHideCharacterRoster.Tag = "Checkbox_Options_HideCharacterRoster";
            this.chkHideCharacterRoster.Text = "Hide the Character Roster";
            this.chkHideCharacterRoster.UseVisualStyleBackColor = true;
            this.chkHideCharacterRoster.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // cmdVerify
            // 
            this.cmdVerify.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdVerify.Enabled = false;
            this.cmdVerify.Location = new System.Drawing.Point(676, 3);
            this.cmdVerify.Name = "cmdVerify";
            this.cmdVerify.Size = new System.Drawing.Size(107, 28);
            this.cmdVerify.TabIndex = 2;
            this.cmdVerify.Text = "Verify";
            this.cmdVerify.UseVisualStyleBackColor = true;
            this.cmdVerify.Click += new System.EventHandler(this.cmdVerify_Click);
            // 
            // cmdVerifyData
            // 
            this.tlpGlobal.SetColumnSpan(this.cmdVerifyData, 2);
            this.cmdVerifyData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdVerifyData.Enabled = false;
            this.cmdVerifyData.Location = new System.Drawing.Point(789, 3);
            this.cmdVerifyData.Name = "cmdVerifyData";
            this.cmdVerifyData.Size = new System.Drawing.Size(106, 28);
            this.cmdVerifyData.TabIndex = 3;
            this.cmdVerifyData.Text = "Verify Data File";
            this.cmdVerifyData.UseVisualStyleBackColor = true;
            this.cmdVerifyData.Click += new System.EventHandler(this.cmdVerifyData_Click);
            // 
            // cboXSLT
            // 
            this.cboXSLT.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpGlobal.SetColumnSpan(this.cboXSLT, 3);
            this.cboXSLT.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboXSLT.FormattingEnabled = true;
            this.cboXSLT.Location = new System.Drawing.Point(676, 37);
            this.cboXSLT.Name = "cboXSLT";
            this.cboXSLT.Size = new System.Drawing.Size(219, 21);
            this.cboXSLT.TabIndex = 8;
            this.cboXSLT.TooltipText = "";
            // 
            // lblXSLT
            // 
            this.lblXSLT.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblXSLT.AutoSize = true;
            this.lblXSLT.Location = new System.Drawing.Point(337, 40);
            this.lblXSLT.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblXSLT.Name = "lblXSLT";
            this.lblXSLT.Size = new System.Drawing.Size(124, 13);
            this.lblXSLT.TabIndex = 7;
            this.lblXSLT.Tag = "Label_Options_DefaultCharacterSheet";
            this.lblXSLT.Text = "Default Character Sheet:";
            // 
            // chkLifeModule
            // 
            this.chkLifeModule.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkLifeModule.AutoSize = true;
            this.tlpGlobal.SetColumnSpan(this.chkLifeModule, 3);
            this.chkLifeModule.Location = new System.Drawing.Point(304, 72);
            this.chkLifeModule.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkLifeModule.Name = "chkLifeModule";
            this.chkLifeModule.Size = new System.Drawing.Size(295, 17);
            this.chkLifeModule.TabIndex = 22;
            this.chkLifeModule.Tag = "Checkbox_Options_UseLifeModule";
            this.chkLifeModule.Text = "Life modules visible";
            this.chkLifeModule.UseVisualStyleBackColor = true;
            this.chkLifeModule.CheckedChanged += new System.EventHandler(this.chkLifeModules_CheckedChanged);
            // 
            // chkLiveCustomData
            // 
            this.chkLiveCustomData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkLiveCustomData.AutoSize = true;
            this.tlpGlobal.SetColumnSpan(this.chkLiveCustomData, 3);
            this.chkLiveCustomData.Location = new System.Drawing.Point(304, 172);
            this.chkLiveCustomData.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkLiveCustomData.Name = "chkLiveCustomData";
            this.chkLiveCustomData.Size = new System.Drawing.Size(295, 17);
            this.chkLiveCustomData.TabIndex = 28;
            this.chkLiveCustomData.Tag = "Checkbox_Options_Live_CustomData";
            this.chkLiveCustomData.Text = "Allow Live Custom Data Updates from customdata Directory";
            this.chkLiveCustomData.UseVisualStyleBackColor = true;
            this.chkLiveCustomData.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkUseLogging
            // 
            this.chkUseLogging.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkUseLogging.AutoSize = true;
            this.chkUseLogging.Location = new System.Drawing.Point(304, 97);
            this.chkUseLogging.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkUseLogging.Name = "chkUseLogging";
            this.chkUseLogging.Size = new System.Drawing.Size(157, 17);
            this.chkUseLogging.TabIndex = 4;
            this.chkUseLogging.Tag = "Checkbox_Options_UseLogging";
            this.chkUseLogging.Text = "Use Debug Logging";
            this.chkUseLogging.UseVisualStyleBackColor = true;
            this.chkUseLogging.CheckedChanged += new System.EventHandler(this.ChkUseLogging_CheckedChanged);
            // 
            // chkAutomaticUpdate
            // 
            this.chkAutomaticUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAutomaticUpdate.AutoSize = true;
            this.tlpGlobal.SetColumnSpan(this.chkAutomaticUpdate, 3);
            this.chkAutomaticUpdate.Location = new System.Drawing.Point(304, 122);
            this.chkAutomaticUpdate.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkAutomaticUpdate.Name = "chkAutomaticUpdate";
            this.chkAutomaticUpdate.Size = new System.Drawing.Size(295, 17);
            this.chkAutomaticUpdate.TabIndex = 5;
            this.chkAutomaticUpdate.Tag = "Checkbox_Options_AutomaticUpdates";
            this.chkAutomaticUpdate.Text = "Automatic Updates";
            this.chkAutomaticUpdate.UseVisualStyleBackColor = true;
            this.chkAutomaticUpdate.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkOmaeEnabled
            // 
            this.chkOmaeEnabled.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkOmaeEnabled.AutoSize = true;
            this.tlpGlobal.SetColumnSpan(this.chkOmaeEnabled, 3);
            this.chkOmaeEnabled.Location = new System.Drawing.Point(304, 272);
            this.chkOmaeEnabled.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkOmaeEnabled.Name = "chkOmaeEnabled";
            this.chkOmaeEnabled.Size = new System.Drawing.Size(295, 17);
            this.chkOmaeEnabled.TabIndex = 24;
            this.chkOmaeEnabled.Tag = "Checkbox_Options_OmaeEnabled";
            this.chkOmaeEnabled.Text = "[Omae enabled]";
            this.chkOmaeEnabled.UseVisualStyleBackColor = true;
            this.chkOmaeEnabled.CheckedChanged += new System.EventHandler(this.chkOmaeEnabled_CheckedChanged);
            // 
            // chkStartupFullscreen
            // 
            this.chkStartupFullscreen.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkStartupFullscreen.AutoSize = true;
            this.tlpGlobal.SetColumnSpan(this.chkStartupFullscreen, 3);
            this.chkStartupFullscreen.Location = new System.Drawing.Point(304, 197);
            this.chkStartupFullscreen.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkStartupFullscreen.Name = "chkStartupFullscreen";
            this.chkStartupFullscreen.Size = new System.Drawing.Size(295, 17);
            this.chkStartupFullscreen.TabIndex = 7;
            this.chkStartupFullscreen.Tag = "Checkbox_Options_StartupFullscreen";
            this.chkStartupFullscreen.Text = "Start Chummer in fullscreen";
            this.chkStartupFullscreen.UseVisualStyleBackColor = true;
            this.chkStartupFullscreen.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkSingleDiceRoller
            // 
            this.chkSingleDiceRoller.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkSingleDiceRoller.AutoSize = true;
            this.tlpGlobal.SetColumnSpan(this.chkSingleDiceRoller, 3);
            this.chkSingleDiceRoller.Location = new System.Drawing.Point(304, 222);
            this.chkSingleDiceRoller.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkSingleDiceRoller.Name = "chkSingleDiceRoller";
            this.chkSingleDiceRoller.Size = new System.Drawing.Size(295, 17);
            this.chkSingleDiceRoller.TabIndex = 8;
            this.chkSingleDiceRoller.Tag = "Checkbox_Options_SingleDiceRoller";
            this.chkSingleDiceRoller.Text = "Use a single instance of the Dice Roller window";
            this.chkSingleDiceRoller.UseVisualStyleBackColor = true;
            this.chkSingleDiceRoller.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkDatesIncludeTime
            // 
            this.chkDatesIncludeTime.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkDatesIncludeTime.AutoSize = true;
            this.tlpGlobal.SetColumnSpan(this.chkDatesIncludeTime, 3);
            this.chkDatesIncludeTime.Location = new System.Drawing.Point(304, 247);
            this.chkDatesIncludeTime.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkDatesIncludeTime.Name = "chkDatesIncludeTime";
            this.chkDatesIncludeTime.Size = new System.Drawing.Size(295, 17);
            this.chkDatesIncludeTime.TabIndex = 9;
            this.chkDatesIncludeTime.Tag = "Checkbox_Options_DatesIncludeTime";
            this.chkDatesIncludeTime.Text = "Expense dates should include time";
            this.chkDatesIncludeTime.UseVisualStyleBackColor = true;
            this.chkDatesIncludeTime.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblPDFParametersLabel
            // 
            this.lblPDFParametersLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblPDFParametersLabel.AutoSize = true;
            this.lblPDFParametersLabel.Location = new System.Drawing.Point(374, 483);
            this.lblPDFParametersLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPDFParametersLabel.Name = "lblPDFParametersLabel";
            this.lblPDFParametersLabel.Size = new System.Drawing.Size(87, 13);
            this.lblPDFParametersLabel.TabIndex = 19;
            this.lblPDFParametersLabel.Tag = "Label_Options_PDFParameters";
            this.lblPDFParametersLabel.Text = "PDF Parameters:";
            // 
            // cboPDFParameters
            // 
            this.cboPDFParameters.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tlpGlobal.SetColumnSpan(this.cboPDFParameters, 3);
            this.cboPDFParameters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPDFParameters.FormattingEnabled = true;
            this.cboPDFParameters.Location = new System.Drawing.Point(467, 479);
            this.cboPDFParameters.Name = "cboPDFParameters";
            this.cboPDFParameters.Size = new System.Drawing.Size(203, 21);
            this.cboPDFParameters.TabIndex = 26;
            this.cboPDFParameters.TooltipText = "";
            this.cboPDFParameters.SelectedIndexChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // txtPDFAppPath
            // 
            this.txtPDFAppPath.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tlpGlobal.SetColumnSpan(this.txtPDFAppPath, 3);
            this.txtPDFAppPath.Location = new System.Drawing.Point(467, 507);
            this.txtPDFAppPath.Name = "txtPDFAppPath";
            this.txtPDFAppPath.ReadOnly = true;
            this.txtPDFAppPath.Size = new System.Drawing.Size(203, 20);
            this.txtPDFAppPath.TabIndex = 10;
            this.txtPDFAppPath.TextChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // grpSelectedSourcebook
            // 
            this.grpSelectedSourcebook.AutoSize = true;
            this.grpSelectedSourcebook.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpGlobal.SetColumnSpan(this.grpSelectedSourcebook, 7);
            this.grpSelectedSourcebook.Controls.Add(this.tlpSelectedSourcebook);
            this.grpSelectedSourcebook.Location = new System.Drawing.Point(304, 535);
            this.grpSelectedSourcebook.MaximumSize = new System.Drawing.Size(500, 10000);
            this.grpSelectedSourcebook.Name = "grpSelectedSourcebook";
            this.grpSelectedSourcebook.Size = new System.Drawing.Size(434, 84);
            this.grpSelectedSourcebook.TabIndex = 27;
            this.grpSelectedSourcebook.TabStop = false;
            this.grpSelectedSourcebook.Tag = "Label_Options_SelectedSourcebook";
            this.grpSelectedSourcebook.Text = "Selected Sourcebook:";
            this.grpSelectedSourcebook.Visible = false;
            // 
            // tlpSelectedSourcebook
            // 
            this.tlpSelectedSourcebook.AutoSize = true;
            this.tlpSelectedSourcebook.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpSelectedSourcebook.ColumnCount = 3;
            this.tlpSelectedSourcebook.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpSelectedSourcebook.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSelectedSourcebook.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpSelectedSourcebook.Controls.Add(this.txtPDFLocation, 1, 0);
            this.tlpSelectedSourcebook.Controls.Add(this.lblPDFLocation, 0, 0);
            this.tlpSelectedSourcebook.Controls.Add(this.cmdPDFLocation, 2, 0);
            this.tlpSelectedSourcebook.Controls.Add(this.lblPDFOffset, 0, 1);
            this.tlpSelectedSourcebook.Controls.Add(this.flpPDFOffset, 1, 1);
            this.tlpSelectedSourcebook.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSelectedSourcebook.Location = new System.Drawing.Point(3, 16);
            this.tlpSelectedSourcebook.Name = "tlpSelectedSourcebook";
            this.tlpSelectedSourcebook.RowCount = 2;
            this.tlpSelectedSourcebook.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSelectedSourcebook.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSelectedSourcebook.Size = new System.Drawing.Size(428, 65);
            this.tlpSelectedSourcebook.TabIndex = 18;
            // 
            // txtPDFLocation
            // 
            this.txtPDFLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPDFLocation.Location = new System.Drawing.Point(84, 4);
            this.txtPDFLocation.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtPDFLocation.Name = "txtPDFLocation";
            this.txtPDFLocation.ReadOnly = true;
            this.txtPDFLocation.Size = new System.Drawing.Size(309, 20);
            this.txtPDFLocation.TabIndex = 13;
            this.txtPDFLocation.TextChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblPDFLocation
            // 
            this.lblPDFLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPDFLocation.AutoSize = true;
            this.lblPDFLocation.Location = new System.Drawing.Point(3, 6);
            this.lblPDFLocation.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPDFLocation.Name = "lblPDFLocation";
            this.lblPDFLocation.Size = new System.Drawing.Size(75, 17);
            this.lblPDFLocation.TabIndex = 12;
            this.lblPDFLocation.Tag = "Label_Options_PDFLocation";
            this.lblPDFLocation.Text = "PDF Location:";
            this.lblPDFLocation.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmdPDFLocation
            // 
            this.cmdPDFLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdPDFLocation.AutoSize = true;
            this.cmdPDFLocation.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdPDFLocation.Location = new System.Drawing.Point(399, 3);
            this.cmdPDFLocation.Name = "cmdPDFLocation";
            this.cmdPDFLocation.Size = new System.Drawing.Size(26, 23);
            this.cmdPDFLocation.TabIndex = 14;
            this.cmdPDFLocation.Text = "...";
            this.cmdPDFLocation.UseVisualStyleBackColor = true;
            this.cmdPDFLocation.Click += new System.EventHandler(this.cmdPDFLocation_Click);
            // 
            // lblPDFOffset
            // 
            this.lblPDFOffset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPDFOffset.AutoSize = true;
            this.lblPDFOffset.Location = new System.Drawing.Point(12, 35);
            this.lblPDFOffset.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPDFOffset.Name = "lblPDFOffset";
            this.lblPDFOffset.Size = new System.Drawing.Size(66, 13);
            this.lblPDFOffset.TabIndex = 15;
            this.lblPDFOffset.Tag = "Label_Options_PDFOffset";
            this.lblPDFOffset.Text = "Page Offset:";
            this.lblPDFOffset.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // flpPDFOffset
            // 
            this.flpPDFOffset.AutoSize = true;
            this.tlpSelectedSourcebook.SetColumnSpan(this.flpPDFOffset, 2);
            this.flpPDFOffset.Controls.Add(this.nudPDFOffset);
            this.flpPDFOffset.Controls.Add(this.cmdPDFTest);
            this.flpPDFOffset.Location = new System.Drawing.Point(81, 29);
            this.flpPDFOffset.Margin = new System.Windows.Forms.Padding(0);
            this.flpPDFOffset.Name = "flpPDFOffset";
            this.flpPDFOffset.Size = new System.Drawing.Size(230, 36);
            this.flpPDFOffset.TabIndex = 16;
            // 
            // nudPDFOffset
            // 
            this.nudPDFOffset.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.nudPDFOffset.Location = new System.Drawing.Point(3, 4);
            this.nudPDFOffset.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.nudPDFOffset.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.nudPDFOffset.Name = "nudPDFOffset";
            this.nudPDFOffset.Size = new System.Drawing.Size(44, 20);
            this.nudPDFOffset.TabIndex = 16;
            this.nudPDFOffset.ValueChanged += new System.EventHandler(this.nudPDFOffset_ValueChanged);
            // 
            // cmdPDFTest
            // 
            this.cmdPDFTest.AutoSize = true;
            this.cmdPDFTest.Location = new System.Drawing.Point(53, 3);
            this.cmdPDFTest.Name = "cmdPDFTest";
            this.cmdPDFTest.Size = new System.Drawing.Size(174, 30);
            this.cmdPDFTest.TabIndex = 17;
            this.cmdPDFTest.Tag = "Button_Options_PDFTest";
            this.cmdPDFTest.Text = "Test - Open to Page 5";
            this.cmdPDFTest.UseVisualStyleBackColor = true;
            this.cmdPDFTest.Click += new System.EventHandler(this.cmdPDFTest_Click);
            // 
            // cboLanguage
            // 
            this.cboLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpGlobal.SetColumnSpan(this.cboLanguage, 2);
            this.cboLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLanguage.FormattingEnabled = true;
            this.cboLanguage.Location = new System.Drawing.Point(489, 3);
            this.cboLanguage.Name = "cboLanguage";
            this.cboLanguage.Size = new System.Drawing.Size(181, 21);
            this.cboLanguage.TabIndex = 1;
            this.cboLanguage.TooltipText = "";
            this.cboLanguage.SelectedIndexChanged += new System.EventHandler(this.cboLanguage_SelectedIndexChanged);
            // 
            // cboSheetLanguage
            // 
            this.cboSheetLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpGlobal.SetColumnSpan(this.cboSheetLanguage, 2);
            this.cboSheetLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSheetLanguage.FormattingEnabled = true;
            this.cboSheetLanguage.Location = new System.Drawing.Point(489, 37);
            this.cboSheetLanguage.Name = "cboSheetLanguage";
            this.cboSheetLanguage.Size = new System.Drawing.Size(181, 21);
            this.cboSheetLanguage.TabIndex = 34;
            this.cboSheetLanguage.TooltipText = "";
            this.cboSheetLanguage.SelectedIndexChanged += new System.EventHandler(this.cboSheetLanguage_SelectedIndexChanged);
            // 
            // chkConfirmDelete
            // 
            this.chkConfirmDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkConfirmDelete.AutoSize = true;
            this.tlpGlobal.SetColumnSpan(this.chkConfirmDelete, 4);
            this.chkConfirmDelete.Location = new System.Drawing.Point(605, 72);
            this.chkConfirmDelete.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkConfirmDelete.Name = "chkConfirmDelete";
            this.chkConfirmDelete.Size = new System.Drawing.Size(290, 17);
            this.chkConfirmDelete.TabIndex = 38;
            this.chkConfirmDelete.Tag = "Checkbox_Options_ConfirmDelete";
            this.chkConfirmDelete.Text = "Ask for confirmation when deleting items";
            this.chkConfirmDelete.UseVisualStyleBackColor = true;
            this.chkConfirmDelete.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkConfirmKarmaExpense
            // 
            this.chkConfirmKarmaExpense.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkConfirmKarmaExpense.AutoSize = true;
            this.tlpGlobal.SetColumnSpan(this.chkConfirmKarmaExpense, 4);
            this.chkConfirmKarmaExpense.Location = new System.Drawing.Point(605, 97);
            this.chkConfirmKarmaExpense.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkConfirmKarmaExpense.Name = "chkConfirmKarmaExpense";
            this.chkConfirmKarmaExpense.Size = new System.Drawing.Size(290, 17);
            this.chkConfirmKarmaExpense.TabIndex = 39;
            this.chkConfirmKarmaExpense.Tag = "Checkbox_Options_ConfirmKarmaExpense";
            this.chkConfirmKarmaExpense.Text = "Ask for confirmation for Karma expenses";
            this.chkConfirmKarmaExpense.UseVisualStyleBackColor = true;
            this.chkConfirmKarmaExpense.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkHideItemsOverAvail
            // 
            this.chkHideItemsOverAvail.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkHideItemsOverAvail.AutoSize = true;
            this.chkHideItemsOverAvail.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkHideItemsOverAvail.Checked = true;
            this.chkHideItemsOverAvail.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tlpGlobal.SetColumnSpan(this.chkHideItemsOverAvail, 4);
            this.chkHideItemsOverAvail.Location = new System.Drawing.Point(605, 121);
            this.chkHideItemsOverAvail.Name = "chkHideItemsOverAvail";
            this.chkHideItemsOverAvail.Size = new System.Drawing.Size(290, 19);
            this.chkHideItemsOverAvail.TabIndex = 40;
            this.chkHideItemsOverAvail.Tag = "Checkbox_Option_HideItemsOverAvailLimit";
            this.chkHideItemsOverAvail.Text = "Hide items that are over the Availability Limit during character creation";
            this.chkHideItemsOverAvail.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkHideItemsOverAvail.UseVisualStyleBackColor = true;
            this.chkHideItemsOverAvail.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkAllowHoverIncrement
            // 
            this.chkAllowHoverIncrement.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAllowHoverIncrement.AutoSize = true;
            this.chkAllowHoverIncrement.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkAllowHoverIncrement.Checked = true;
            this.chkAllowHoverIncrement.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tlpGlobal.SetColumnSpan(this.chkAllowHoverIncrement, 4);
            this.chkAllowHoverIncrement.Location = new System.Drawing.Point(605, 196);
            this.chkAllowHoverIncrement.Name = "chkAllowHoverIncrement";
            this.tlpGlobal.SetRowSpan(this.chkAllowHoverIncrement, 2);
            this.chkAllowHoverIncrement.Size = new System.Drawing.Size(290, 44);
            this.chkAllowHoverIncrement.TabIndex = 41;
            this.chkAllowHoverIncrement.Tag = "Checkbox_Options_AllowHoverIncrement";
            this.chkAllowHoverIncrement.Text = "Allow incrementingvalues of numericupdown controls by hovering over the control";
            this.chkAllowHoverIncrement.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkAllowHoverIncrement.UseVisualStyleBackColor = true;
            this.chkAllowHoverIncrement.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkLiveUpdateCleanCharacterFiles
            // 
            this.chkLiveUpdateCleanCharacterFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkLiveUpdateCleanCharacterFiles.AutoSize = true;
            this.tlpGlobal.SetColumnSpan(this.chkLiveUpdateCleanCharacterFiles, 7);
            this.chkLiveUpdateCleanCharacterFiles.Location = new System.Drawing.Point(304, 297);
            this.chkLiveUpdateCleanCharacterFiles.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkLiveUpdateCleanCharacterFiles.Name = "chkLiveUpdateCleanCharacterFiles";
            this.chkLiveUpdateCleanCharacterFiles.Size = new System.Drawing.Size(591, 17);
            this.chkLiveUpdateCleanCharacterFiles.TabIndex = 33;
            this.chkLiveUpdateCleanCharacterFiles.Tag = "Checkbox_Options_LiveUpdateCleanCharacterFiles";
            this.chkLiveUpdateCleanCharacterFiles.Text = "Automatically load changes from open characters\' save files if there are no pendi" +
    "ng changes to be saved";
            this.chkLiveUpdateCleanCharacterFiles.UseVisualStyleBackColor = true;
            this.chkLiveUpdateCleanCharacterFiles.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkPreferNightlyBuilds
            // 
            this.chkPreferNightlyBuilds.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkPreferNightlyBuilds.AutoSize = true;
            this.tlpGlobal.SetColumnSpan(this.chkPreferNightlyBuilds, 3);
            this.chkPreferNightlyBuilds.Location = new System.Drawing.Point(304, 322);
            this.chkPreferNightlyBuilds.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkPreferNightlyBuilds.Name = "chkPreferNightlyBuilds";
            this.chkPreferNightlyBuilds.Size = new System.Drawing.Size(295, 17);
            this.chkPreferNightlyBuilds.TabIndex = 25;
            this.chkPreferNightlyBuilds.Tag = "Checkbox_Options_PreferNightlyBuilds";
            this.chkPreferNightlyBuilds.Text = "Prefer Nightly Builds";
            this.chkPreferNightlyBuilds.UseVisualStyleBackColor = true;
            this.chkPreferNightlyBuilds.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblCharacterRosterLabel
            // 
            this.lblCharacterRosterLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCharacterRosterLabel.AutoSize = true;
            this.lblCharacterRosterLabel.Location = new System.Drawing.Point(304, 455);
            this.lblCharacterRosterLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCharacterRosterLabel.Name = "lblCharacterRosterLabel";
            this.lblCharacterRosterLabel.Size = new System.Drawing.Size(157, 13);
            this.lblCharacterRosterLabel.TabIndex = 44;
            this.lblCharacterRosterLabel.Tag = "Label_Options_CharacterRoster";
            this.lblCharacterRosterLabel.Text = "Character Roster Watch Folder:";
            // 
            // txtCharacterRosterPath
            // 
            this.txtCharacterRosterPath.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tlpGlobal.SetColumnSpan(this.txtCharacterRosterPath, 3);
            this.txtCharacterRosterPath.Location = new System.Drawing.Point(467, 451);
            this.txtCharacterRosterPath.Name = "txtCharacterRosterPath";
            this.txtCharacterRosterPath.ReadOnly = true;
            this.txtCharacterRosterPath.Size = new System.Drawing.Size(203, 20);
            this.txtCharacterRosterPath.TabIndex = 45;
            this.txtCharacterRosterPath.TextChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // cmdCharacterRoster
            // 
            this.cmdCharacterRoster.Dock = System.Windows.Forms.DockStyle.Left;
            this.cmdCharacterRoster.Location = new System.Drawing.Point(676, 450);
            this.cmdCharacterRoster.Name = "cmdCharacterRoster";
            this.cmdCharacterRoster.Size = new System.Drawing.Size(27, 23);
            this.cmdCharacterRoster.TabIndex = 46;
            this.cmdCharacterRoster.Text = "...";
            this.cmdCharacterRoster.UseVisualStyleBackColor = true;
            this.cmdCharacterRoster.Click += new System.EventHandler(this.cmdCharacterRoster_Click);
            // 
            // chkCreateBackupOnCareer
            // 
            this.chkCreateBackupOnCareer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkCreateBackupOnCareer.AutoSize = true;
            this.chkCreateBackupOnCareer.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.tlpGlobal.SetColumnSpan(this.chkCreateBackupOnCareer, 5);
            this.chkCreateBackupOnCareer.Location = new System.Drawing.Point(304, 426);
            this.chkCreateBackupOnCareer.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkCreateBackupOnCareer.Name = "chkCreateBackupOnCareer";
            this.chkCreateBackupOnCareer.Size = new System.Drawing.Size(479, 17);
            this.chkCreateBackupOnCareer.TabIndex = 24;
            this.chkCreateBackupOnCareer.Tag = "Checkbox_Option_CreateBackupOnCareer";
            this.chkCreateBackupOnCareer.Text = "Create backup of characters before moving them to Career Mode";
            this.chkCreateBackupOnCareer.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkCreateBackupOnCareer.UseVisualStyleBackColor = true;
            this.chkCreateBackupOnCareer.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkPrintToFileFirst
            // 
            this.chkPrintToFileFirst.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkPrintToFileFirst.AutoSize = true;
            this.tlpGlobal.SetColumnSpan(this.chkPrintToFileFirst, 3);
            this.chkPrintToFileFirst.Location = new System.Drawing.Point(304, 372);
            this.chkPrintToFileFirst.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkPrintToFileFirst.Name = "chkPrintToFileFirst";
            this.chkPrintToFileFirst.Size = new System.Drawing.Size(295, 17);
            this.chkPrintToFileFirst.TabIndex = 43;
            this.chkPrintToFileFirst.Tag = "Checkbox_Option_PrintToFileFirst";
            this.chkPrintToFileFirst.Text = "Apply Linux printing fix";
            this.chkPrintToFileFirst.UseVisualStyleBackColor = true;
            this.chkPrintToFileFirst.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // grpCharacterDefaults
            // 
            this.grpCharacterDefaults.AutoSize = true;
            this.grpCharacterDefaults.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpGlobal.SetColumnSpan(this.grpCharacterDefaults, 4);
            this.grpCharacterDefaults.Controls.Add(this.tableLayoutPanel7);
            this.grpCharacterDefaults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpCharacterDefaults.Location = new System.Drawing.Point(605, 333);
            this.grpCharacterDefaults.Margin = new System.Windows.Forms.Padding(3, 15, 3, 16);
            this.grpCharacterDefaults.MaximumSize = new System.Drawing.Size(500, 10000);
            this.grpCharacterDefaults.Name = "grpCharacterDefaults";
            this.tlpGlobal.SetRowSpan(this.grpCharacterDefaults, 4);
            this.grpCharacterDefaults.Size = new System.Drawing.Size(290, 73);
            this.grpCharacterDefaults.TabIndex = 42;
            this.grpCharacterDefaults.TabStop = false;
            this.grpCharacterDefaults.Tag = "Label_Options_Defaults";
            this.grpCharacterDefaults.Text = "Defaults for New Characters";
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.AutoSize = true;
            this.tableLayoutPanel7.ColumnCount = 1;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel7.Controls.Add(this.cboBuildMethod, 0, 0);
            this.tableLayoutPanel7.Controls.Add(this.cboDefaultGameplayOption, 0, 1);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 2;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel7.Size = new System.Drawing.Size(284, 54);
            this.tableLayoutPanel7.TabIndex = 0;
            // 
            // cboBuildMethod
            // 
            this.cboBuildMethod.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboBuildMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBuildMethod.FormattingEnabled = true;
            this.cboBuildMethod.Location = new System.Drawing.Point(3, 3);
            this.cboBuildMethod.Name = "cboBuildMethod";
            this.cboBuildMethod.Size = new System.Drawing.Size(278, 21);
            this.cboBuildMethod.TabIndex = 6;
            this.cboBuildMethod.TooltipText = "";
            this.cboBuildMethod.SelectedIndexChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // cboDefaultGameplayOption
            // 
            this.cboDefaultGameplayOption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboDefaultGameplayOption.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDefaultGameplayOption.FormattingEnabled = true;
            this.cboDefaultGameplayOption.Location = new System.Drawing.Point(3, 30);
            this.cboDefaultGameplayOption.Name = "cboDefaultGameplayOption";
            this.cboDefaultGameplayOption.Size = new System.Drawing.Size(278, 21);
            this.cboDefaultGameplayOption.TabIndex = 7;
            this.cboDefaultGameplayOption.SelectedIndexChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblEditSourcebookInfo
            // 
            this.lblEditSourcebookInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblEditSourcebookInfo.AutoSize = true;
            this.lblEditSourcebookInfo.Location = new System.Drawing.Point(3, 15);
            this.lblEditSourcebookInfo.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblEditSourcebookInfo.Name = "lblEditSourcebookInfo";
            this.lblEditSourcebookInfo.Size = new System.Drawing.Size(107, 13);
            this.lblEditSourcebookInfo.TabIndex = 47;
            this.lblEditSourcebookInfo.Tag = "Label_Options_EditSourcebookInfo";
            this.lblEditSourcebookInfo.Text = "Edit Sourcebook Info";
            // 
            // lstGlobalSourcebookInfos
            // 
            this.lstGlobalSourcebookInfos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstGlobalSourcebookInfos.FormattingEnabled = true;
            this.lstGlobalSourcebookInfos.Location = new System.Drawing.Point(3, 37);
            this.lstGlobalSourcebookInfos.Name = "lstGlobalSourcebookInfos";
            this.tlpGlobal.SetRowSpan(this.lstGlobalSourcebookInfos, 20);
            this.lstGlobalSourcebookInfos.Size = new System.Drawing.Size(295, 596);
            this.lstGlobalSourcebookInfos.TabIndex = 48;
            this.lstGlobalSourcebookInfos.SelectedIndexChanged += new System.EventHandler(this.lstGlobalSourcebookInfos_SelectedIndexChanged);
            // 
            // imgLanguageFlag
            // 
            this.imgLanguageFlag.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.imgLanguageFlag.Location = new System.Drawing.Point(467, 3);
            this.imgLanguageFlag.Name = "imgLanguageFlag";
            this.imgLanguageFlag.Size = new System.Drawing.Size(16, 28);
            this.imgLanguageFlag.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.imgLanguageFlag.TabIndex = 49;
            this.imgLanguageFlag.TabStop = false;
            // 
            // chkEnablePlugins
            // 
            this.chkEnablePlugins.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkEnablePlugins.AutoSize = true;
            this.tlpGlobal.SetColumnSpan(this.chkEnablePlugins, 4);
            this.chkEnablePlugins.Location = new System.Drawing.Point(605, 171);
            this.chkEnablePlugins.Name = "chkEnablePlugins";
            this.chkEnablePlugins.Size = new System.Drawing.Size(290, 19);
            this.chkEnablePlugins.TabIndex = 51;
            this.chkEnablePlugins.Tag = "Checkbox_Options_EnablePlugins";
            this.chkEnablePlugins.Text = "Enable Plugins (experimental)";
            this.chkEnablePlugins.UseVisualStyleBackColor = true;
            this.chkEnablePlugins.CheckedChanged += new System.EventHandler(this.chkEnablePlugins_CheckedChanged);
            // 
            // nudBrowserVersion
            // 
            this.nudBrowserVersion.Location = new System.Drawing.Point(489, 396);
            this.nudBrowserVersion.Maximum = new decimal(new int[] {
            11,
            0,
            0,
            0});
            this.nudBrowserVersion.Minimum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.nudBrowserVersion.Name = "nudBrowserVersion";
            this.nudBrowserVersion.Size = new System.Drawing.Size(40, 20);
            this.nudBrowserVersion.TabIndex = 54;
            this.nudBrowserVersion.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            // 
            // lblBrowserVersion
            // 
            this.lblBrowserVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBrowserVersion.AutoSize = true;
            this.tlpGlobal.SetColumnSpan(this.lblBrowserVersion, 2);
            this.lblBrowserVersion.Location = new System.Drawing.Point(304, 399);
            this.lblBrowserVersion.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBrowserVersion.Name = "lblBrowserVersion";
            this.lblBrowserVersion.Size = new System.Drawing.Size(179, 17);
            this.lblBrowserVersion.TabIndex = 53;
            this.lblBrowserVersion.Tag = "Label_Options_BrowserVersion";
            this.lblBrowserVersion.Text = "Browser Engine Version";
            // 
            // chkUseLoggingApplicationInsights
            // 
            this.chkUseLoggingApplicationInsights.AutoSize = true;
            this.tlpGlobal.SetColumnSpan(this.chkUseLoggingApplicationInsights, 2);
            this.chkUseLoggingApplicationInsights.Location = new System.Drawing.Point(467, 96);
            this.chkUseLoggingApplicationInsights.Name = "chkUseLoggingApplicationInsights";
            this.chkUseLoggingApplicationInsights.Size = new System.Drawing.Size(101, 17);
            this.chkUseLoggingApplicationInsights.TabIndex = 55;
            this.chkUseLoggingApplicationInsights.Text = "Upload Logging";
            this.chkUseLoggingApplicationInsights.UseVisualStyleBackColor = true;
            this.chkUseLoggingApplicationInsights.CheckedChanged += new System.EventHandler(this.chkUseLoggingApplicationInsights_CheckedChanged);
            // 
            // tabCharacterOptions
            // 
            this.tabCharacterOptions.BackColor = System.Drawing.SystemColors.Control;
            this.tabCharacterOptions.Controls.Add(this.tlpCharacterOptions);
            this.tabCharacterOptions.Location = new System.Drawing.Point(4, 22);
            this.tabCharacterOptions.Name = "tabCharacterOptions";
            this.tabCharacterOptions.Padding = new System.Windows.Forms.Padding(9);
            this.tabCharacterOptions.Size = new System.Drawing.Size(916, 654);
            this.tabCharacterOptions.TabIndex = 0;
            this.tabCharacterOptions.Tag = "Tab_Options_Character";
            this.tabCharacterOptions.Text = "Character Options";
            // 
            // tlpCharacterOptions
            // 
            this.tlpCharacterOptions.AutoSize = true;
            this.tlpCharacterOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpCharacterOptions.ColumnCount = 6;
            this.tlpCharacterOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 301F));
            this.tlpCharacterOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 2.499993F));
            this.tlpCharacterOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19.99994F));
            this.tlpCharacterOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19.99994F));
            this.tlpCharacterOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 7.500166F));
            this.tlpCharacterOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 49.99995F));
            this.tlpCharacterOptions.Controls.Add(this.treSourcebook, 0, 1);
            this.tlpCharacterOptions.Controls.Add(this.chkPrintNotes, 1, 15);
            this.tlpCharacterOptions.Controls.Add(this.lblSourcebooksToUse, 0, 0);
            this.tlpCharacterOptions.Controls.Add(this.chkPrintExpenses, 1, 13);
            this.tlpCharacterOptions.Controls.Add(this.chkPrintSkillsWithZeroRating, 1, 12);
            this.tlpCharacterOptions.Controls.Add(this.chkDontUseCyberlimbCalculation, 1, 1);
            this.tlpCharacterOptions.Controls.Add(this.chkAllowSkillDiceRolling, 1, 2);
            this.tlpCharacterOptions.Controls.Add(this.chkEnforceCapacity, 1, 3);
            this.tlpCharacterOptions.Controls.Add(this.chkLicenseEachRestrictedItem, 1, 4);
            this.tlpCharacterOptions.Controls.Add(this.lblEssenceDecimals, 1, 10);
            this.tlpCharacterOptions.Controls.Add(this.lblNuyenDecimalsMaximumLabel, 1, 9);
            this.tlpCharacterOptions.Controls.Add(this.lblNuyenDecimalsMinimumLabel, 1, 8);
            this.tlpCharacterOptions.Controls.Add(this.chkDontRoundEssenceInternally, 1, 11);
            this.tlpCharacterOptions.Controls.Add(this.chkDronemods, 1, 5);
            this.tlpCharacterOptions.Controls.Add(this.chkRestrictRecoil, 1, 7);
            this.tlpCharacterOptions.Controls.Add(this.nudNuyenDecimalsMinimum, 4, 8);
            this.tlpCharacterOptions.Controls.Add(this.nudNuyenDecimalsMaximum, 4, 9);
            this.tlpCharacterOptions.Controls.Add(this.nudEssenceDecimals, 4, 10);
            this.tlpCharacterOptions.Controls.Add(this.chkDronemodsMaximumPilot, 2, 6);
            this.tlpCharacterOptions.Controls.Add(this.chkPrintFreeExpenses, 2, 14);
            this.tlpCharacterOptions.Controls.Add(this.lblLimbCount, 1, 0);
            this.tlpCharacterOptions.Controls.Add(this.cboLimbCount, 3, 0);
            this.tlpCharacterOptions.Controls.Add(this.cmdEnableSourcebooks, 0, 16);
            this.tlpCharacterOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCharacterOptions.Location = new System.Drawing.Point(9, 9);
            this.tlpCharacterOptions.Name = "tlpCharacterOptions";
            this.tlpCharacterOptions.RowCount = 17;
            this.tlpCharacterOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCharacterOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterOptions.Size = new System.Drawing.Size(898, 636);
            this.tlpCharacterOptions.TabIndex = 40;
            // 
            // treSourcebook
            // 
            this.treSourcebook.CheckBoxes = true;
            this.treSourcebook.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treSourcebook.Location = new System.Drawing.Point(3, 41);
            this.treSourcebook.Name = "treSourcebook";
            this.tlpCharacterOptions.SetRowSpan(this.treSourcebook, 15);
            this.treSourcebook.ShowLines = false;
            this.treSourcebook.ShowPlusMinus = false;
            this.treSourcebook.ShowRootLines = false;
            this.treSourcebook.Size = new System.Drawing.Size(295, 563);
            this.treSourcebook.TabIndex = 1;
            // 
            // chkPrintNotes
            // 
            this.chkPrintNotes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkPrintNotes.AutoSize = true;
            this.chkPrintNotes.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.tlpCharacterOptions.SetColumnSpan(this.chkPrintNotes, 4);
            this.chkPrintNotes.Location = new System.Drawing.Point(304, 416);
            this.chkPrintNotes.Name = "chkPrintNotes";
            this.chkPrintNotes.Size = new System.Drawing.Size(290, 188);
            this.chkPrintNotes.TabIndex = 14;
            this.chkPrintNotes.Tag = "Checkbox_Option_PrintNotes";
            this.chkPrintNotes.Text = "Print Notes";
            this.chkPrintNotes.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkPrintNotes.UseVisualStyleBackColor = true;
            this.chkPrintNotes.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblSourcebooksToUse
            // 
            this.lblSourcebooksToUse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSourcebooksToUse.AutoSize = true;
            this.lblSourcebooksToUse.Location = new System.Drawing.Point(3, 19);
            this.lblSourcebooksToUse.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSourcebooksToUse.Name = "lblSourcebooksToUse";
            this.lblSourcebooksToUse.Size = new System.Drawing.Size(104, 13);
            this.lblSourcebooksToUse.TabIndex = 0;
            this.lblSourcebooksToUse.Tag = "Label_Options_SourcebooksToUse";
            this.lblSourcebooksToUse.Text = "Sourcebooks to Use";
            // 
            // chkPrintExpenses
            // 
            this.chkPrintExpenses.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkPrintExpenses.AutoSize = true;
            this.tlpCharacterOptions.SetColumnSpan(this.chkPrintExpenses, 4);
            this.chkPrintExpenses.Location = new System.Drawing.Point(304, 367);
            this.chkPrintExpenses.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkPrintExpenses.Name = "chkPrintExpenses";
            this.chkPrintExpenses.Size = new System.Drawing.Size(290, 17);
            this.chkPrintExpenses.TabIndex = 12;
            this.chkPrintExpenses.Tag = "Checkbox_Options_PrintExpenses";
            this.chkPrintExpenses.Text = "Print Karma and Nuyen Expenses";
            this.chkPrintExpenses.UseVisualStyleBackColor = true;
            this.chkPrintExpenses.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkPrintSkillsWithZeroRating
            // 
            this.chkPrintSkillsWithZeroRating.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkPrintSkillsWithZeroRating.AutoSize = true;
            this.tlpCharacterOptions.SetColumnSpan(this.chkPrintSkillsWithZeroRating, 4);
            this.chkPrintSkillsWithZeroRating.Location = new System.Drawing.Point(304, 342);
            this.chkPrintSkillsWithZeroRating.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkPrintSkillsWithZeroRating.Name = "chkPrintSkillsWithZeroRating";
            this.chkPrintSkillsWithZeroRating.Size = new System.Drawing.Size(290, 17);
            this.chkPrintSkillsWithZeroRating.TabIndex = 11;
            this.chkPrintSkillsWithZeroRating.Tag = "Checkbox_Options_PrintAllSkills";
            this.chkPrintSkillsWithZeroRating.Text = "Print all Active Skills with Rating 0 or higher";
            this.chkPrintSkillsWithZeroRating.UseVisualStyleBackColor = true;
            this.chkPrintSkillsWithZeroRating.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkDontUseCyberlimbCalculation
            // 
            this.chkDontUseCyberlimbCalculation.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkDontUseCyberlimbCalculation.AutoSize = true;
            this.tlpCharacterOptions.SetColumnSpan(this.chkDontUseCyberlimbCalculation, 4);
            this.chkDontUseCyberlimbCalculation.Location = new System.Drawing.Point(304, 42);
            this.chkDontUseCyberlimbCalculation.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkDontUseCyberlimbCalculation.Name = "chkDontUseCyberlimbCalculation";
            this.chkDontUseCyberlimbCalculation.Size = new System.Drawing.Size(290, 17);
            this.chkDontUseCyberlimbCalculation.TabIndex = 19;
            this.chkDontUseCyberlimbCalculation.Tag = "Checkbox_Options_UseCyberlimbCalculation";
            this.chkDontUseCyberlimbCalculation.Text = "Do not use Cyberlimbs when calculating augmented Attributes";
            this.chkDontUseCyberlimbCalculation.UseVisualStyleBackColor = true;
            this.chkDontUseCyberlimbCalculation.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkAllowSkillDiceRolling
            // 
            this.chkAllowSkillDiceRolling.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAllowSkillDiceRolling.AutoSize = true;
            this.tlpCharacterOptions.SetColumnSpan(this.chkAllowSkillDiceRolling, 4);
            this.chkAllowSkillDiceRolling.Location = new System.Drawing.Point(304, 67);
            this.chkAllowSkillDiceRolling.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkAllowSkillDiceRolling.Name = "chkAllowSkillDiceRolling";
            this.chkAllowSkillDiceRolling.Size = new System.Drawing.Size(290, 17);
            this.chkAllowSkillDiceRolling.TabIndex = 10;
            this.chkAllowSkillDiceRolling.Tag = "Checkbox_Option_AllowSkillDiceRolling";
            this.chkAllowSkillDiceRolling.Text = "Allow dice rolling for dice pools";
            this.chkAllowSkillDiceRolling.UseVisualStyleBackColor = true;
            this.chkAllowSkillDiceRolling.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkEnforceCapacity
            // 
            this.chkEnforceCapacity.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkEnforceCapacity.AutoSize = true;
            this.tlpCharacterOptions.SetColumnSpan(this.chkEnforceCapacity, 4);
            this.chkEnforceCapacity.Location = new System.Drawing.Point(304, 92);
            this.chkEnforceCapacity.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkEnforceCapacity.Name = "chkEnforceCapacity";
            this.chkEnforceCapacity.Size = new System.Drawing.Size(290, 17);
            this.chkEnforceCapacity.TabIndex = 25;
            this.chkEnforceCapacity.Tag = "Checkbox_Option_EnforceCapacity";
            this.chkEnforceCapacity.Text = "Enforce Capacity limits";
            this.chkEnforceCapacity.UseVisualStyleBackColor = true;
            this.chkEnforceCapacity.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkLicenseEachRestrictedItem
            // 
            this.chkLicenseEachRestrictedItem.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkLicenseEachRestrictedItem.AutoSize = true;
            this.tlpCharacterOptions.SetColumnSpan(this.chkLicenseEachRestrictedItem, 4);
            this.chkLicenseEachRestrictedItem.Location = new System.Drawing.Point(304, 117);
            this.chkLicenseEachRestrictedItem.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkLicenseEachRestrictedItem.Name = "chkLicenseEachRestrictedItem";
            this.chkLicenseEachRestrictedItem.Size = new System.Drawing.Size(290, 17);
            this.chkLicenseEachRestrictedItem.TabIndex = 27;
            this.chkLicenseEachRestrictedItem.Tag = "Checkbox_Options_LicenseRestricted";
            this.chkLicenseEachRestrictedItem.Text = "License each Restricted item";
            this.chkLicenseEachRestrictedItem.UseVisualStyleBackColor = true;
            this.chkLicenseEachRestrictedItem.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblEssenceDecimals
            // 
            this.lblEssenceDecimals.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblEssenceDecimals.AutoSize = true;
            this.tlpCharacterOptions.SetColumnSpan(this.lblEssenceDecimals, 3);
            this.lblEssenceDecimals.Location = new System.Drawing.Point(320, 295);
            this.lblEssenceDecimals.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblEssenceDecimals.Name = "lblEssenceDecimals";
            this.lblEssenceDecimals.Size = new System.Drawing.Size(230, 13);
            this.lblEssenceDecimals.TabIndex = 16;
            this.lblEssenceDecimals.Tag = "Label_Options_EssenceDecimals";
            this.lblEssenceDecimals.Text = "Number of decimal places to round Essence to:";
            // 
            // lblNuyenDecimalsMaximumLabel
            // 
            this.lblNuyenDecimalsMaximumLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNuyenDecimalsMaximumLabel.AutoSize = true;
            this.tlpCharacterOptions.SetColumnSpan(this.lblNuyenDecimalsMaximumLabel, 3);
            this.lblNuyenDecimalsMaximumLabel.Location = new System.Drawing.Point(327, 257);
            this.lblNuyenDecimalsMaximumLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblNuyenDecimalsMaximumLabel.Name = "lblNuyenDecimalsMaximumLabel";
            this.lblNuyenDecimalsMaximumLabel.Size = new System.Drawing.Size(223, 26);
            this.lblNuyenDecimalsMaximumLabel.TabIndex = 32;
            this.lblNuyenDecimalsMaximumLabel.Tag = "Label_Options_NuyenDecimalsMaximum";
            this.lblNuyenDecimalsMaximumLabel.Text = "Maximum number of Nuyen decimal places to display:";
            // 
            // lblNuyenDecimalsMinimumLabel
            // 
            this.lblNuyenDecimalsMinimumLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNuyenDecimalsMinimumLabel.AutoSize = true;
            this.tlpCharacterOptions.SetColumnSpan(this.lblNuyenDecimalsMinimumLabel, 3);
            this.lblNuyenDecimalsMinimumLabel.Location = new System.Drawing.Point(330, 219);
            this.lblNuyenDecimalsMinimumLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblNuyenDecimalsMinimumLabel.Name = "lblNuyenDecimalsMinimumLabel";
            this.lblNuyenDecimalsMinimumLabel.Size = new System.Drawing.Size(220, 26);
            this.lblNuyenDecimalsMinimumLabel.TabIndex = 30;
            this.lblNuyenDecimalsMinimumLabel.Tag = "Label_Options_NuyenDecimalsMinimum";
            this.lblNuyenDecimalsMinimumLabel.Text = "Minimum number of Nuyen decimal places to display:";
            // 
            // chkDontRoundEssenceInternally
            // 
            this.chkDontRoundEssenceInternally.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkDontRoundEssenceInternally.AutoSize = true;
            this.tlpCharacterOptions.SetColumnSpan(this.chkDontRoundEssenceInternally, 4);
            this.chkDontRoundEssenceInternally.Location = new System.Drawing.Point(304, 318);
            this.chkDontRoundEssenceInternally.Name = "chkDontRoundEssenceInternally";
            this.chkDontRoundEssenceInternally.Size = new System.Drawing.Size(290, 17);
            this.chkDontRoundEssenceInternally.TabIndex = 18;
            this.chkDontRoundEssenceInternally.Tag = "Checkbox_Option_DontRoundEssenceInternally";
            this.chkDontRoundEssenceInternally.Text = "Only round Essence for display purposes, not for internal calculations";
            this.chkDontRoundEssenceInternally.UseVisualStyleBackColor = true;
            this.chkDontRoundEssenceInternally.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkDronemods
            // 
            this.chkDronemods.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkDronemods.AutoSize = true;
            this.tlpCharacterOptions.SetColumnSpan(this.chkDronemods, 4);
            this.chkDronemods.Location = new System.Drawing.Point(304, 142);
            this.chkDronemods.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkDronemods.Name = "chkDronemods";
            this.chkDronemods.Size = new System.Drawing.Size(290, 17);
            this.chkDronemods.TabIndex = 36;
            this.chkDronemods.Tag = "Checkbox_Options_Dronemods";
            this.chkDronemods.Text = "Use Drone Modification rules (R5 122)";
            this.chkDronemods.UseVisualStyleBackColor = true;
            this.chkDronemods.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkRestrictRecoil
            // 
            this.chkRestrictRecoil.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkRestrictRecoil.AutoSize = true;
            this.tlpCharacterOptions.SetColumnSpan(this.chkRestrictRecoil, 4);
            this.chkRestrictRecoil.Location = new System.Drawing.Point(304, 192);
            this.chkRestrictRecoil.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkRestrictRecoil.Name = "chkRestrictRecoil";
            this.chkRestrictRecoil.Size = new System.Drawing.Size(290, 17);
            this.chkRestrictRecoil.TabIndex = 26;
            this.chkRestrictRecoil.Tag = "Checkbox_Options_UseRestrictionsToRecoilCompensation";
            this.chkRestrictRecoil.Text = "Use Restrictions to Recoil Compensation (RG 53)";
            this.chkRestrictRecoil.UseVisualStyleBackColor = true;
            this.chkRestrictRecoil.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // nudNuyenDecimalsMinimum
            // 
            this.nudNuyenDecimalsMinimum.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudNuyenDecimalsMinimum.Location = new System.Drawing.Point(556, 216);
            this.nudNuyenDecimalsMinimum.Maximum = new decimal(new int[] {
            28,
            0,
            0,
            0});
            this.nudNuyenDecimalsMinimum.Name = "nudNuyenDecimalsMinimum";
            this.nudNuyenDecimalsMinimum.Size = new System.Drawing.Size(38, 20);
            this.nudNuyenDecimalsMinimum.TabIndex = 33;
            this.nudNuyenDecimalsMinimum.ValueChanged += new System.EventHandler(this.nudNuyenDecimalsMinimum_ValueChanged);
            // 
            // nudNuyenDecimalsMaximum
            // 
            this.nudNuyenDecimalsMaximum.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudNuyenDecimalsMaximum.Location = new System.Drawing.Point(556, 254);
            this.nudNuyenDecimalsMaximum.Maximum = new decimal(new int[] {
            28,
            0,
            0,
            0});
            this.nudNuyenDecimalsMaximum.Name = "nudNuyenDecimalsMaximum";
            this.nudNuyenDecimalsMaximum.Size = new System.Drawing.Size(38, 20);
            this.nudNuyenDecimalsMaximum.TabIndex = 34;
            this.nudNuyenDecimalsMaximum.ValueChanged += new System.EventHandler(this.nudNuyenDecimalsMaximum_ValueChanged);
            // 
            // nudEssenceDecimals
            // 
            this.nudEssenceDecimals.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudEssenceDecimals.Location = new System.Drawing.Point(556, 292);
            this.nudEssenceDecimals.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudEssenceDecimals.Name = "nudEssenceDecimals";
            this.nudEssenceDecimals.Size = new System.Drawing.Size(38, 20);
            this.nudEssenceDecimals.TabIndex = 17;
            this.nudEssenceDecimals.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // chkDronemodsMaximumPilot
            // 
            this.chkDronemodsMaximumPilot.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkDronemodsMaximumPilot.AutoSize = true;
            this.tlpCharacterOptions.SetColumnSpan(this.chkDronemodsMaximumPilot, 3);
            this.chkDronemodsMaximumPilot.Location = new System.Drawing.Point(318, 167);
            this.chkDronemodsMaximumPilot.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkDronemodsMaximumPilot.Name = "chkDronemodsMaximumPilot";
            this.chkDronemodsMaximumPilot.Size = new System.Drawing.Size(276, 17);
            this.chkDronemodsMaximumPilot.TabIndex = 37;
            this.chkDronemodsMaximumPilot.Tag = "Checkbox_Options_Dronemods_Pilot";
            this.chkDronemodsMaximumPilot.Text = "Use Maximum Attribute for Pilot Attribute";
            this.chkDronemodsMaximumPilot.UseVisualStyleBackColor = true;
            this.chkDronemodsMaximumPilot.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkPrintFreeExpenses
            // 
            this.chkPrintFreeExpenses.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkPrintFreeExpenses.AutoSize = true;
            this.tlpCharacterOptions.SetColumnSpan(this.chkPrintFreeExpenses, 3);
            this.chkPrintFreeExpenses.Location = new System.Drawing.Point(318, 392);
            this.chkPrintFreeExpenses.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkPrintFreeExpenses.Name = "chkPrintFreeExpenses";
            this.chkPrintFreeExpenses.Size = new System.Drawing.Size(276, 17);
            this.chkPrintFreeExpenses.TabIndex = 13;
            this.chkPrintFreeExpenses.Tag = "Checkbox_Options_PrintFreeExpenses";
            this.chkPrintFreeExpenses.Text = "Print Free Karma and Nuyen Expenses";
            this.chkPrintFreeExpenses.UseVisualStyleBackColor = true;
            this.chkPrintFreeExpenses.CheckedChanged += new System.EventHandler(this.chkPrintFreeExpenses_CheckedChanged);
            // 
            // lblLimbCount
            // 
            this.lblLimbCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLimbCount.AutoSize = true;
            this.tlpCharacterOptions.SetColumnSpan(this.lblLimbCount, 2);
            this.lblLimbCount.Location = new System.Drawing.Point(353, 6);
            this.lblLimbCount.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLimbCount.Name = "lblLimbCount";
            this.lblLimbCount.Size = new System.Drawing.Size(78, 26);
            this.lblLimbCount.TabIndex = 0;
            this.lblLimbCount.Tag = "Label_Options_CyberlimbCount";
            this.lblLimbCount.Text = "Limb Count for Cyberlimbs:";
            // 
            // cboLimbCount
            // 
            this.cboLimbCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpCharacterOptions.SetColumnSpan(this.cboLimbCount, 2);
            this.cboLimbCount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLimbCount.FormattingEnabled = true;
            this.cboLimbCount.Location = new System.Drawing.Point(437, 3);
            this.cboLimbCount.Name = "cboLimbCount";
            this.cboLimbCount.Size = new System.Drawing.Size(157, 21);
            this.cboLimbCount.TabIndex = 1;
            this.cboLimbCount.TooltipText = "";
            this.cboLimbCount.SelectedIndexChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // cmdEnableSourcebooks
            // 
            this.cmdEnableSourcebooks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdEnableSourcebooks.Location = new System.Drawing.Point(3, 610);
            this.cmdEnableSourcebooks.Name = "cmdEnableSourcebooks";
            this.cmdEnableSourcebooks.Size = new System.Drawing.Size(295, 23);
            this.cmdEnableSourcebooks.TabIndex = 6;
            this.cmdEnableSourcebooks.Tag = "Button_ToggleSourcebooks";
            this.cmdEnableSourcebooks.Text = "Toggle all Sourcebooks On/Off";
            this.cmdEnableSourcebooks.UseVisualStyleBackColor = true;
            this.cmdEnableSourcebooks.Click += new System.EventHandler(this.cmdEnableSourcebooks_Click);
            // 
            // tabKarmaCosts
            // 
            this.tabKarmaCosts.BackColor = System.Drawing.SystemColors.Control;
            this.tabKarmaCosts.Controls.Add(this.tlpKarmaCosts);
            this.tabKarmaCosts.Location = new System.Drawing.Point(4, 22);
            this.tabKarmaCosts.Name = "tabKarmaCosts";
            this.tabKarmaCosts.Padding = new System.Windows.Forms.Padding(9);
            this.tabKarmaCosts.Size = new System.Drawing.Size(916, 654);
            this.tabKarmaCosts.TabIndex = 1;
            this.tabKarmaCosts.Tag = "Tab_Options_KarmaCosts";
            this.tabKarmaCosts.Text = "Karma Costs";
            // 
            // tlpKarmaCosts
            // 
            this.tlpKarmaCosts.ColumnCount = 1;
            this.tlpKarmaCosts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpKarmaCosts.Controls.Add(this.tlpKarmaCostsList, 0, 0);
            this.tlpKarmaCosts.Controls.Add(this.cmdRestoreDefaultsKarma, 0, 1);
            this.tlpKarmaCosts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpKarmaCosts.Location = new System.Drawing.Point(9, 9);
            this.tlpKarmaCosts.Name = "tlpKarmaCosts";
            this.tlpKarmaCosts.RowCount = 2;
            this.tlpKarmaCosts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpKarmaCosts.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCosts.Size = new System.Drawing.Size(898, 636);
            this.tlpKarmaCosts.TabIndex = 125;
            // 
            // tlpKarmaCostsList
            // 
            this.tlpKarmaCostsList.AutoScroll = true;
            this.tlpKarmaCostsList.AutoSize = true;
            this.tlpKarmaCostsList.ColumnCount = 8;
            this.tlpKarmaCostsList.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21.5F));
            this.tlpKarmaCostsList.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1.5F));
            this.tlpKarmaCostsList.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 7.5F));
            this.tlpKarmaCostsList.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12F));
            this.tlpKarmaCostsList.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 7.5F));
            this.tlpKarmaCostsList.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 23F));
            this.tlpKarmaCostsList.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 7.5F));
            this.tlpKarmaCostsList.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19.5F));
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaMysticAdeptPowerPoint, 2, 22);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaSpecialization, 0, 0);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaMysticAdeptPowerPoint, 0, 22);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaSpecialization, 2, 0);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaNewAIAdvancedProgram, 6, 20);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaWeaponFocusExtra, 7, 18);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaInitiationFlat, 4, 21);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaWeaponFocus, 6, 18);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaNewAIAdvancedProgram, 5, 20);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaWeaponFocus, 5, 18);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaNewAIProgram, 6, 19);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaKnowledgeSpecialization, 0, 1);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaRitualSpellcastingFocusExtra, 7, 13);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaNewAIProgram, 5, 19);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaKnowledgeSpecialization, 2, 1);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaRitualSpellcastingFocus, 6, 13);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaNewKnowledgeSkill, 0, 2);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaRitualSpellcastingFocus, 5, 13);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaNewKnowledgeSkill, 2, 2);
            this.tlpKarmaCostsList.Controls.Add(this.lblFlexibleSignatureFocusExtra, 7, 9);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaMetamagic, 5, 0);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaInitiationExtra, 3, 21);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaInitiationBracket, 1, 21);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaInitiation, 2, 21);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaSpellShapingFocusExtra, 7, 17);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaFlexibleSignatureFocus, 6, 9);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaSpellShapingFocus, 6, 17);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaInitiation, 0, 21);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaMetamagic, 6, 0);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaCarryoverExtra, 3, 20);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaSpellShapingFocus, 5, 17);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaCarryover, 2, 20);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaFlexibleSignatureFocus, 5, 9);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaCarryover, 0, 20);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaSustainingFocusExtra, 7, 16);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaEnemyExtra, 3, 19);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaEnemy, 2, 19);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaEnemy, 0, 19);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaContactExtra, 3, 18);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaContact, 2, 18);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaContact, 0, 18);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaJoinGroup, 5, 1);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaSustainingFocus, 6, 16);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaJoinGroup, 6, 1);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaSustainingFocus, 5, 16);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaLeaveGroup, 5, 2);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaSummoningFocusExtra, 7, 15);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaLeaveGroup, 6, 2);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaSummoningFocus, 6, 15);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaNewActiveSkill, 0, 3);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaManeuver, 2, 16);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaSummoningFocus, 5, 15);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaManeuver, 0, 16);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaNewActiveSkill, 2, 3);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaComplexFormSkillsoftExtra, 3, 14);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaSpellcastingFocusExtra, 7, 14);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaComplexFormSkillsoft, 2, 14);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaAlchemicalFocus, 5, 3);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaComplexFormSkillsoft, 0, 14);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaNuyenPerExtra, 3, 17);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaSpellcastingFocus, 6, 14);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaNuyenPer, 2, 17);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaSpiritExtra, 3, 15);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaNuyenPer, 0, 17);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaComplexFormOptionExtra, 3, 13);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaSpirit, 2, 15);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaAlchemicalFocus, 6, 3);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaSpirit, 0, 15);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaComplexFormOption, 2, 13);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaSpellcastingFocus, 5, 14);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaComplexFormOption, 0, 13);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaAlchemicalFocusExtra, 7, 3);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaNewSkillGroup, 0, 4);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaNewSkillGroup, 2, 4);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaBanishingFocus, 5, 4);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaBanishingFocus, 6, 4);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaBanishingFocusExtra, 7, 4);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaImproveKnowledgeSkill, 0, 5);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaQiFocusExtra, 7, 12);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaImproveKnowledgeSkill, 2, 5);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaQiFocus, 6, 12);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaImproveKnowledgeSkillExtra, 3, 5);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaQiFocus, 5, 12);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaBindingFocus, 5, 5);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaPowerFocusExtra, 7, 11);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaBindingFocus, 6, 5);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaPowerFocus, 6, 11);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaBindingFocusExtra, 7, 5);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaPowerFocus, 5, 11);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaImproveActiveSkill, 0, 6);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaMaskingFocusExtra, 7, 10);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaImproveActiveSkill, 2, 6);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaMaskingFocus, 6, 10);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaImproveActiveSkillExtra, 3, 6);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaMaskingFocus, 5, 10);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaCenteringFocus, 5, 6);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaCenteringFocus, 6, 6);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaCenteringFocusExtra, 7, 6);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaCounterspellingFocus, 5, 7);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaCounterspellingFocus, 6, 7);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaCounterspellingFocusExtra, 7, 7);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaImproveSkillGroup, 0, 7);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaDisenchantingFocusExtra, 7, 8);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaImproveSkillGroup, 2, 7);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaDisenchantingFocus, 6, 8);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaImproveSkillGroupExtra, 3, 7);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaDisenchantingFocus, 5, 8);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaAttribute, 0, 8);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaAttribute, 2, 8);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaAttributeExtra, 3, 8);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaQuality, 0, 9);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaImproveComplexFormExtra, 3, 12);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaQuality, 2, 9);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaImproveComplexForm, 2, 12);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaQualityExtra, 3, 9);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaImproveComplexForm, 0, 12);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaSpell, 0, 10);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaSpell, 2, 10);
            this.tlpKarmaCostsList.Controls.Add(this.lblKarmaNewComplexForm, 0, 11);
            this.tlpKarmaCostsList.Controls.Add(this.nudKarmaNewComplexForm, 2, 11);
            this.tlpKarmaCostsList.Controls.Add(this.nudMetatypeCostsKarmaMultiplier, 6, 21);
            this.tlpKarmaCostsList.Controls.Add(this.lblMetatypeCostsKarmaMultiplierLabel, 5, 21);
            this.tlpKarmaCostsList.Controls.Add(this.lblNuyenPerBP, 5, 22);
            this.tlpKarmaCostsList.Controls.Add(this.nudNuyenPerBP, 6, 22);
            this.tlpKarmaCostsList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpKarmaCostsList.Location = new System.Drawing.Point(3, 3);
            this.tlpKarmaCostsList.Name = "tlpKarmaCostsList";
            this.tlpKarmaCostsList.RowCount = 23;
            this.tlpKarmaCostsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCostsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCostsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCostsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCostsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCostsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCostsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCostsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCostsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCostsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCostsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCostsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCostsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCostsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCostsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCostsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCostsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCostsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCostsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCostsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCostsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCostsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCostsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaCostsList.Size = new System.Drawing.Size(892, 601);
            this.tlpKarmaCostsList.TabIndex = 124;
            // 
            // nudKarmaMysticAdeptPowerPoint
            // 
            this.nudKarmaMysticAdeptPowerPoint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaMysticAdeptPowerPoint.Location = new System.Drawing.Point(207, 575);
            this.nudKarmaMysticAdeptPowerPoint.Name = "nudKarmaMysticAdeptPowerPoint";
            this.nudKarmaMysticAdeptPowerPoint.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaMysticAdeptPowerPoint.TabIndex = 123;
            this.nudKarmaMysticAdeptPowerPoint.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaSpecialization
            // 
            this.lblKarmaSpecialization.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaSpecialization.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaSpecialization, 2);
            this.lblKarmaSpecialization.Location = new System.Drawing.Point(49, 6);
            this.lblKarmaSpecialization.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSpecialization.Name = "lblKarmaSpecialization";
            this.lblKarmaSpecialization.Size = new System.Drawing.Size(152, 13);
            this.lblKarmaSpecialization.TabIndex = 0;
            this.lblKarmaSpecialization.Tag = "Label_Options_NewSpecialization";
            this.lblKarmaSpecialization.Text = "New Active Skill Specialization";
            // 
            // lblKarmaMysticAdeptPowerPoint
            // 
            this.lblKarmaMysticAdeptPowerPoint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaMysticAdeptPowerPoint.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaMysticAdeptPowerPoint, 2);
            this.lblKarmaMysticAdeptPowerPoint.Location = new System.Drawing.Point(73, 578);
            this.lblKarmaMysticAdeptPowerPoint.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaMysticAdeptPowerPoint.Name = "lblKarmaMysticAdeptPowerPoint";
            this.lblKarmaMysticAdeptPowerPoint.Size = new System.Drawing.Size(128, 13);
            this.lblKarmaMysticAdeptPowerPoint.TabIndex = 122;
            this.lblKarmaMysticAdeptPowerPoint.Tag = "Label_Options_KarmaMysticAdeptPowerPoint";
            this.lblKarmaMysticAdeptPowerPoint.Text = "Mystic Adept Power Point";
            // 
            // nudKarmaSpecialization
            // 
            this.nudKarmaSpecialization.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaSpecialization.Location = new System.Drawing.Point(207, 3);
            this.nudKarmaSpecialization.Name = "nudKarmaSpecialization";
            this.nudKarmaSpecialization.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaSpecialization.TabIndex = 1;
            this.nudKarmaSpecialization.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // nudKarmaNewAIAdvancedProgram
            // 
            this.nudKarmaNewAIAdvancedProgram.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaNewAIAdvancedProgram.Location = new System.Drawing.Point(651, 523);
            this.nudKarmaNewAIAdvancedProgram.Name = "nudKarmaNewAIAdvancedProgram";
            this.nudKarmaNewAIAdvancedProgram.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaNewAIAdvancedProgram.TabIndex = 112;
            this.nudKarmaNewAIAdvancedProgram.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaWeaponFocusExtra
            // 
            this.lblKarmaWeaponFocusExtra.AutoSize = true;
            this.lblKarmaWeaponFocusExtra.Location = new System.Drawing.Point(717, 474);
            this.lblKarmaWeaponFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaWeaponFocusExtra.Name = "lblKarmaWeaponFocusExtra";
            this.lblKarmaWeaponFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaWeaponFocusExtra.TabIndex = 107;
            this.lblKarmaWeaponFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaWeaponFocusExtra.Text = "x Force";
            // 
            // nudKarmaInitiationFlat
            // 
            this.nudKarmaInitiationFlat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaInitiationFlat.Location = new System.Drawing.Point(380, 549);
            this.nudKarmaInitiationFlat.Name = "nudKarmaInitiationFlat";
            this.nudKarmaInitiationFlat.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaInitiationFlat.TabIndex = 121;
            this.nudKarmaInitiationFlat.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // nudKarmaWeaponFocus
            // 
            this.nudKarmaWeaponFocus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaWeaponFocus.Location = new System.Drawing.Point(651, 471);
            this.nudKarmaWeaponFocus.Name = "nudKarmaWeaponFocus";
            this.nudKarmaWeaponFocus.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaWeaponFocus.TabIndex = 106;
            this.nudKarmaWeaponFocus.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaNewAIAdvancedProgram
            // 
            this.lblKarmaNewAIAdvancedProgram.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaNewAIAdvancedProgram.AutoSize = true;
            this.lblKarmaNewAIAdvancedProgram.Location = new System.Drawing.Point(503, 526);
            this.lblKarmaNewAIAdvancedProgram.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaNewAIAdvancedProgram.Name = "lblKarmaNewAIAdvancedProgram";
            this.lblKarmaNewAIAdvancedProgram.Size = new System.Drawing.Size(142, 13);
            this.lblKarmaNewAIAdvancedProgram.TabIndex = 110;
            this.lblKarmaNewAIAdvancedProgram.Tag = "Label_Options_NewAIAdvancedProgram";
            this.lblKarmaNewAIAdvancedProgram.Text = "New Advanced Program (AI)";
            // 
            // lblKarmaWeaponFocus
            // 
            this.lblKarmaWeaponFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaWeaponFocus.AutoSize = true;
            this.lblKarmaWeaponFocus.Location = new System.Drawing.Point(565, 474);
            this.lblKarmaWeaponFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaWeaponFocus.Name = "lblKarmaWeaponFocus";
            this.lblKarmaWeaponFocus.Size = new System.Drawing.Size(80, 13);
            this.lblKarmaWeaponFocus.TabIndex = 105;
            this.lblKarmaWeaponFocus.Tag = "Label_Options_WeaponFocus";
            this.lblKarmaWeaponFocus.Text = "Weapon Focus";
            // 
            // nudKarmaNewAIProgram
            // 
            this.nudKarmaNewAIProgram.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaNewAIProgram.Location = new System.Drawing.Point(651, 497);
            this.nudKarmaNewAIProgram.Name = "nudKarmaNewAIProgram";
            this.nudKarmaNewAIProgram.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaNewAIProgram.TabIndex = 111;
            this.nudKarmaNewAIProgram.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaKnowledgeSpecialization
            // 
            this.lblKarmaKnowledgeSpecialization.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaKnowledgeSpecialization.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaKnowledgeSpecialization, 2);
            this.lblKarmaKnowledgeSpecialization.Location = new System.Drawing.Point(26, 32);
            this.lblKarmaKnowledgeSpecialization.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaKnowledgeSpecialization.Name = "lblKarmaKnowledgeSpecialization";
            this.lblKarmaKnowledgeSpecialization.Size = new System.Drawing.Size(175, 13);
            this.lblKarmaKnowledgeSpecialization.TabIndex = 119;
            this.lblKarmaKnowledgeSpecialization.Tag = "Label_Options_NewKnoSpecialization";
            this.lblKarmaKnowledgeSpecialization.Text = "New Knowledge Skill Specialization";
            // 
            // lblKarmaRitualSpellcastingFocusExtra
            // 
            this.lblKarmaRitualSpellcastingFocusExtra.AutoSize = true;
            this.lblKarmaRitualSpellcastingFocusExtra.Location = new System.Drawing.Point(717, 344);
            this.lblKarmaRitualSpellcastingFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaRitualSpellcastingFocusExtra.Name = "lblKarmaRitualSpellcastingFocusExtra";
            this.lblKarmaRitualSpellcastingFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaRitualSpellcastingFocusExtra.TabIndex = 118;
            this.lblKarmaRitualSpellcastingFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaRitualSpellcastingFocusExtra.Text = "x Force";
            // 
            // lblKarmaNewAIProgram
            // 
            this.lblKarmaNewAIProgram.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaNewAIProgram.AutoSize = true;
            this.lblKarmaNewAIProgram.Location = new System.Drawing.Point(555, 500);
            this.lblKarmaNewAIProgram.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaNewAIProgram.Name = "lblKarmaNewAIProgram";
            this.lblKarmaNewAIProgram.Size = new System.Drawing.Size(90, 13);
            this.lblKarmaNewAIProgram.TabIndex = 109;
            this.lblKarmaNewAIProgram.Tag = "Label_Options_NewAIProgram";
            this.lblKarmaNewAIProgram.Text = "New Program (AI)";
            // 
            // nudKarmaKnowledgeSpecialization
            // 
            this.nudKarmaKnowledgeSpecialization.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaKnowledgeSpecialization.Location = new System.Drawing.Point(207, 29);
            this.nudKarmaKnowledgeSpecialization.Name = "nudKarmaKnowledgeSpecialization";
            this.nudKarmaKnowledgeSpecialization.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaKnowledgeSpecialization.TabIndex = 120;
            this.nudKarmaKnowledgeSpecialization.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // nudKarmaRitualSpellcastingFocus
            // 
            this.nudKarmaRitualSpellcastingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaRitualSpellcastingFocus.Location = new System.Drawing.Point(651, 341);
            this.nudKarmaRitualSpellcastingFocus.Name = "nudKarmaRitualSpellcastingFocus";
            this.nudKarmaRitualSpellcastingFocus.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaRitualSpellcastingFocus.TabIndex = 117;
            // 
            // lblKarmaNewKnowledgeSkill
            // 
            this.lblKarmaNewKnowledgeSkill.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaNewKnowledgeSkill.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaNewKnowledgeSkill, 2);
            this.lblKarmaNewKnowledgeSkill.Location = new System.Drawing.Point(94, 58);
            this.lblKarmaNewKnowledgeSkill.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaNewKnowledgeSkill.Name = "lblKarmaNewKnowledgeSkill";
            this.lblKarmaNewKnowledgeSkill.Size = new System.Drawing.Size(107, 13);
            this.lblKarmaNewKnowledgeSkill.TabIndex = 2;
            this.lblKarmaNewKnowledgeSkill.Tag = "Label_Options_NewKnowledgeSkill";
            this.lblKarmaNewKnowledgeSkill.Text = "New Knowledge Skill";
            // 
            // lblKarmaRitualSpellcastingFocus
            // 
            this.lblKarmaRitualSpellcastingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaRitualSpellcastingFocus.AutoSize = true;
            this.lblKarmaRitualSpellcastingFocus.Location = new System.Drawing.Point(519, 344);
            this.lblKarmaRitualSpellcastingFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaRitualSpellcastingFocus.Name = "lblKarmaRitualSpellcastingFocus";
            this.lblKarmaRitualSpellcastingFocus.Size = new System.Drawing.Size(126, 13);
            this.lblKarmaRitualSpellcastingFocus.TabIndex = 116;
            this.lblKarmaRitualSpellcastingFocus.Tag = "Label_Options_RitualSpellcastingFocus";
            this.lblKarmaRitualSpellcastingFocus.Text = "Ritual Spellcasting Focus";
            // 
            // nudKarmaNewKnowledgeSkill
            // 
            this.nudKarmaNewKnowledgeSkill.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaNewKnowledgeSkill.Location = new System.Drawing.Point(207, 55);
            this.nudKarmaNewKnowledgeSkill.Name = "nudKarmaNewKnowledgeSkill";
            this.nudKarmaNewKnowledgeSkill.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaNewKnowledgeSkill.TabIndex = 3;
            this.nudKarmaNewKnowledgeSkill.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblFlexibleSignatureFocusExtra
            // 
            this.lblFlexibleSignatureFocusExtra.AutoSize = true;
            this.lblFlexibleSignatureFocusExtra.Location = new System.Drawing.Point(717, 240);
            this.lblFlexibleSignatureFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblFlexibleSignatureFocusExtra.Name = "lblFlexibleSignatureFocusExtra";
            this.lblFlexibleSignatureFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblFlexibleSignatureFocusExtra.TabIndex = 115;
            this.lblFlexibleSignatureFocusExtra.Tag = "Label_Options_Force";
            this.lblFlexibleSignatureFocusExtra.Text = "x Force";
            // 
            // lblKarmaMetamagic
            // 
            this.lblKarmaMetamagic.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaMetamagic.AutoSize = true;
            this.lblKarmaMetamagic.Location = new System.Drawing.Point(491, 6);
            this.lblKarmaMetamagic.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaMetamagic.Name = "lblKarmaMetamagic";
            this.lblKarmaMetamagic.Size = new System.Drawing.Size(154, 13);
            this.lblKarmaMetamagic.TabIndex = 57;
            this.lblKarmaMetamagic.Tag = "Label_Options_Metamagics";
            this.lblKarmaMetamagic.Text = "Additional Metamagics/Echoes";
            // 
            // lblKarmaInitiationExtra
            // 
            this.lblKarmaInitiationExtra.AutoSize = true;
            this.lblKarmaInitiationExtra.Location = new System.Drawing.Point(273, 552);
            this.lblKarmaInitiationExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaInitiationExtra.Name = "lblKarmaInitiationExtra";
            this.lblKarmaInitiationExtra.Size = new System.Drawing.Size(83, 13);
            this.lblKarmaInitiationExtra.TabIndex = 56;
            this.lblKarmaInitiationExtra.Tag = "Label_Options_NewRatingPlus";
            this.lblKarmaInitiationExtra.Text = "x New Rating) +";
            // 
            // lblKarmaInitiationBracket
            // 
            this.lblKarmaInitiationBracket.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaInitiationBracket.AutoSize = true;
            this.lblKarmaInitiationBracket.Location = new System.Drawing.Point(194, 552);
            this.lblKarmaInitiationBracket.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaInitiationBracket.Name = "lblKarmaInitiationBracket";
            this.lblKarmaInitiationBracket.Size = new System.Drawing.Size(7, 13);
            this.lblKarmaInitiationBracket.TabIndex = 54;
            this.lblKarmaInitiationBracket.Text = "(";
            // 
            // nudKarmaInitiation
            // 
            this.nudKarmaInitiation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaInitiation.Location = new System.Drawing.Point(207, 549);
            this.nudKarmaInitiation.Name = "nudKarmaInitiation";
            this.nudKarmaInitiation.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaInitiation.TabIndex = 55;
            this.nudKarmaInitiation.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaSpellShapingFocusExtra
            // 
            this.lblKarmaSpellShapingFocusExtra.AutoSize = true;
            this.lblKarmaSpellShapingFocusExtra.Location = new System.Drawing.Point(717, 448);
            this.lblKarmaSpellShapingFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSpellShapingFocusExtra.Name = "lblKarmaSpellShapingFocusExtra";
            this.lblKarmaSpellShapingFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaSpellShapingFocusExtra.TabIndex = 104;
            this.lblKarmaSpellShapingFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaSpellShapingFocusExtra.Text = "x Force";
            // 
            // nudKarmaFlexibleSignatureFocus
            // 
            this.nudKarmaFlexibleSignatureFocus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaFlexibleSignatureFocus.Location = new System.Drawing.Point(651, 237);
            this.nudKarmaFlexibleSignatureFocus.Name = "nudKarmaFlexibleSignatureFocus";
            this.nudKarmaFlexibleSignatureFocus.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaFlexibleSignatureFocus.TabIndex = 114;
            // 
            // nudKarmaSpellShapingFocus
            // 
            this.nudKarmaSpellShapingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaSpellShapingFocus.Location = new System.Drawing.Point(651, 445);
            this.nudKarmaSpellShapingFocus.Name = "nudKarmaSpellShapingFocus";
            this.nudKarmaSpellShapingFocus.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaSpellShapingFocus.TabIndex = 103;
            this.nudKarmaSpellShapingFocus.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaInitiation
            // 
            this.lblKarmaInitiation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaInitiation.AutoSize = true;
            this.lblKarmaInitiation.Location = new System.Drawing.Point(76, 552);
            this.lblKarmaInitiation.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaInitiation.Name = "lblKarmaInitiation";
            this.lblKarmaInitiation.Size = new System.Drawing.Size(112, 13);
            this.lblKarmaInitiation.TabIndex = 53;
            this.lblKarmaInitiation.Tag = "Label_Options_Initiation";
            this.lblKarmaInitiation.Text = "Initiation / Submersion";
            // 
            // nudKarmaMetamagic
            // 
            this.nudKarmaMetamagic.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaMetamagic.Location = new System.Drawing.Point(651, 3);
            this.nudKarmaMetamagic.Name = "nudKarmaMetamagic";
            this.nudKarmaMetamagic.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaMetamagic.TabIndex = 58;
            this.nudKarmaMetamagic.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaCarryoverExtra
            // 
            this.lblKarmaCarryoverExtra.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaCarryoverExtra, 2);
            this.lblKarmaCarryoverExtra.Location = new System.Drawing.Point(273, 526);
            this.lblKarmaCarryoverExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaCarryoverExtra.Name = "lblKarmaCarryoverExtra";
            this.lblKarmaCarryoverExtra.Size = new System.Drawing.Size(51, 13);
            this.lblKarmaCarryoverExtra.TabIndex = 52;
            this.lblKarmaCarryoverExtra.Tag = "Label_Options_Maximum";
            this.lblKarmaCarryoverExtra.Text = "Maximum";
            // 
            // lblKarmaSpellShapingFocus
            // 
            this.lblKarmaSpellShapingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaSpellShapingFocus.AutoSize = true;
            this.lblKarmaSpellShapingFocus.Location = new System.Drawing.Point(541, 448);
            this.lblKarmaSpellShapingFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSpellShapingFocus.Name = "lblKarmaSpellShapingFocus";
            this.lblKarmaSpellShapingFocus.Size = new System.Drawing.Size(104, 13);
            this.lblKarmaSpellShapingFocus.TabIndex = 102;
            this.lblKarmaSpellShapingFocus.Tag = "Label_Options_SpellShapingFocus";
            this.lblKarmaSpellShapingFocus.Text = "Spell Shaping Focus";
            // 
            // nudKarmaCarryover
            // 
            this.nudKarmaCarryover.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaCarryover.Location = new System.Drawing.Point(207, 523);
            this.nudKarmaCarryover.Name = "nudKarmaCarryover";
            this.nudKarmaCarryover.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaCarryover.TabIndex = 51;
            this.nudKarmaCarryover.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaFlexibleSignatureFocus
            // 
            this.lblKarmaFlexibleSignatureFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaFlexibleSignatureFocus.AutoSize = true;
            this.lblKarmaFlexibleSignatureFocus.Location = new System.Drawing.Point(523, 240);
            this.lblKarmaFlexibleSignatureFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaFlexibleSignatureFocus.Name = "lblKarmaFlexibleSignatureFocus";
            this.lblKarmaFlexibleSignatureFocus.Size = new System.Drawing.Size(122, 13);
            this.lblKarmaFlexibleSignatureFocus.TabIndex = 113;
            this.lblKarmaFlexibleSignatureFocus.Tag = "Label_Options_FlexibleSignatureFocus";
            this.lblKarmaFlexibleSignatureFocus.Text = "Flexible Signature Focus";
            // 
            // lblKarmaCarryover
            // 
            this.lblKarmaCarryover.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaCarryover.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaCarryover, 2);
            this.lblKarmaCarryover.Location = new System.Drawing.Point(60, 526);
            this.lblKarmaCarryover.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaCarryover.Name = "lblKarmaCarryover";
            this.lblKarmaCarryover.Size = new System.Drawing.Size(141, 13);
            this.lblKarmaCarryover.TabIndex = 50;
            this.lblKarmaCarryover.Tag = "Label_Options_Carryover";
            this.lblKarmaCarryover.Text = "Carryover for New Character";
            // 
            // lblKarmaSustainingFocusExtra
            // 
            this.lblKarmaSustainingFocusExtra.AutoSize = true;
            this.lblKarmaSustainingFocusExtra.Location = new System.Drawing.Point(717, 422);
            this.lblKarmaSustainingFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSustainingFocusExtra.Name = "lblKarmaSustainingFocusExtra";
            this.lblKarmaSustainingFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaSustainingFocusExtra.TabIndex = 101;
            this.lblKarmaSustainingFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaSustainingFocusExtra.Text = "x Force";
            // 
            // lblKarmaEnemyExtra
            // 
            this.lblKarmaEnemyExtra.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaEnemyExtra, 2);
            this.lblKarmaEnemyExtra.Location = new System.Drawing.Point(273, 500);
            this.lblKarmaEnemyExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaEnemyExtra.Name = "lblKarmaEnemyExtra";
            this.lblKarmaEnemyExtra.Size = new System.Drawing.Size(120, 13);
            this.lblKarmaEnemyExtra.TabIndex = 49;
            this.lblKarmaEnemyExtra.Tag = "Label_Options_ConnectionLoyalty";
            this.lblKarmaEnemyExtra.Text = "x (Connection + Loyalty)";
            // 
            // nudKarmaEnemy
            // 
            this.nudKarmaEnemy.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaEnemy.Location = new System.Drawing.Point(207, 497);
            this.nudKarmaEnemy.Name = "nudKarmaEnemy";
            this.nudKarmaEnemy.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaEnemy.TabIndex = 48;
            this.nudKarmaEnemy.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaEnemy
            // 
            this.lblKarmaEnemy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaEnemy.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaEnemy, 2);
            this.lblKarmaEnemy.Location = new System.Drawing.Point(154, 500);
            this.lblKarmaEnemy.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaEnemy.Name = "lblKarmaEnemy";
            this.lblKarmaEnemy.Size = new System.Drawing.Size(47, 13);
            this.lblKarmaEnemy.TabIndex = 47;
            this.lblKarmaEnemy.Tag = "Label_Options_Enemies";
            this.lblKarmaEnemy.Text = "Enemies";
            // 
            // lblKarmaContactExtra
            // 
            this.lblKarmaContactExtra.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaContactExtra, 2);
            this.lblKarmaContactExtra.Location = new System.Drawing.Point(273, 474);
            this.lblKarmaContactExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaContactExtra.Name = "lblKarmaContactExtra";
            this.lblKarmaContactExtra.Size = new System.Drawing.Size(120, 13);
            this.lblKarmaContactExtra.TabIndex = 46;
            this.lblKarmaContactExtra.Tag = "Label_Options_ConnectionLoyalty";
            this.lblKarmaContactExtra.Text = "x (Connection + Loyalty)";
            // 
            // nudKarmaContact
            // 
            this.nudKarmaContact.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaContact.Location = new System.Drawing.Point(207, 471);
            this.nudKarmaContact.Name = "nudKarmaContact";
            this.nudKarmaContact.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaContact.TabIndex = 45;
            this.nudKarmaContact.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaContact
            // 
            this.lblKarmaContact.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaContact.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaContact, 2);
            this.lblKarmaContact.Location = new System.Drawing.Point(152, 474);
            this.lblKarmaContact.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaContact.Name = "lblKarmaContact";
            this.lblKarmaContact.Size = new System.Drawing.Size(49, 13);
            this.lblKarmaContact.TabIndex = 44;
            this.lblKarmaContact.Tag = "Label_Options_Contacts";
            this.lblKarmaContact.Text = "Contacts";
            // 
            // lblKarmaJoinGroup
            // 
            this.lblKarmaJoinGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaJoinGroup.AutoSize = true;
            this.lblKarmaJoinGroup.Location = new System.Drawing.Point(542, 32);
            this.lblKarmaJoinGroup.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaJoinGroup.Name = "lblKarmaJoinGroup";
            this.lblKarmaJoinGroup.Size = new System.Drawing.Size(103, 13);
            this.lblKarmaJoinGroup.TabIndex = 56;
            this.lblKarmaJoinGroup.Tag = "Label_Options_JoinGroup";
            this.lblKarmaJoinGroup.Text = "Join Group/Network";
            // 
            // nudKarmaSustainingFocus
            // 
            this.nudKarmaSustainingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaSustainingFocus.Location = new System.Drawing.Point(651, 419);
            this.nudKarmaSustainingFocus.Name = "nudKarmaSustainingFocus";
            this.nudKarmaSustainingFocus.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaSustainingFocus.TabIndex = 100;
            this.nudKarmaSustainingFocus.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // nudKarmaJoinGroup
            // 
            this.nudKarmaJoinGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaJoinGroup.Location = new System.Drawing.Point(651, 29);
            this.nudKarmaJoinGroup.Name = "nudKarmaJoinGroup";
            this.nudKarmaJoinGroup.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaJoinGroup.TabIndex = 57;
            this.nudKarmaJoinGroup.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaSustainingFocus
            // 
            this.lblKarmaSustainingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaSustainingFocus.AutoSize = true;
            this.lblKarmaSustainingFocus.Location = new System.Drawing.Point(557, 422);
            this.lblKarmaSustainingFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSustainingFocus.Name = "lblKarmaSustainingFocus";
            this.lblKarmaSustainingFocus.Size = new System.Drawing.Size(88, 13);
            this.lblKarmaSustainingFocus.TabIndex = 99;
            this.lblKarmaSustainingFocus.Tag = "Label_Options_SustainingFocus";
            this.lblKarmaSustainingFocus.Text = "Sustaining Focus";
            // 
            // lblKarmaLeaveGroup
            // 
            this.lblKarmaLeaveGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaLeaveGroup.AutoSize = true;
            this.lblKarmaLeaveGroup.Location = new System.Drawing.Point(531, 58);
            this.lblKarmaLeaveGroup.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaLeaveGroup.Name = "lblKarmaLeaveGroup";
            this.lblKarmaLeaveGroup.Size = new System.Drawing.Size(114, 13);
            this.lblKarmaLeaveGroup.TabIndex = 58;
            this.lblKarmaLeaveGroup.Tag = "Label_Options_LeaveGroup";
            this.lblKarmaLeaveGroup.Text = "Leave Group/Network";
            // 
            // lblKarmaSummoningFocusExtra
            // 
            this.lblKarmaSummoningFocusExtra.AutoSize = true;
            this.lblKarmaSummoningFocusExtra.Location = new System.Drawing.Point(717, 396);
            this.lblKarmaSummoningFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSummoningFocusExtra.Name = "lblKarmaSummoningFocusExtra";
            this.lblKarmaSummoningFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaSummoningFocusExtra.TabIndex = 98;
            this.lblKarmaSummoningFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaSummoningFocusExtra.Text = "x Force";
            // 
            // nudKarmaLeaveGroup
            // 
            this.nudKarmaLeaveGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaLeaveGroup.Location = new System.Drawing.Point(651, 55);
            this.nudKarmaLeaveGroup.Name = "nudKarmaLeaveGroup";
            this.nudKarmaLeaveGroup.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaLeaveGroup.TabIndex = 59;
            this.nudKarmaLeaveGroup.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // nudKarmaSummoningFocus
            // 
            this.nudKarmaSummoningFocus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaSummoningFocus.Location = new System.Drawing.Point(651, 393);
            this.nudKarmaSummoningFocus.Name = "nudKarmaSummoningFocus";
            this.nudKarmaSummoningFocus.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaSummoningFocus.TabIndex = 97;
            this.nudKarmaSummoningFocus.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaNewActiveSkill
            // 
            this.lblKarmaNewActiveSkill.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaNewActiveSkill.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaNewActiveSkill, 2);
            this.lblKarmaNewActiveSkill.Location = new System.Drawing.Point(117, 84);
            this.lblKarmaNewActiveSkill.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaNewActiveSkill.Name = "lblKarmaNewActiveSkill";
            this.lblKarmaNewActiveSkill.Size = new System.Drawing.Size(84, 13);
            this.lblKarmaNewActiveSkill.TabIndex = 4;
            this.lblKarmaNewActiveSkill.Tag = "Label_Options_NewActiveSkill";
            this.lblKarmaNewActiveSkill.Text = "New Active Skill";
            // 
            // nudKarmaManeuver
            // 
            this.nudKarmaManeuver.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaManeuver.Location = new System.Drawing.Point(207, 419);
            this.nudKarmaManeuver.Name = "nudKarmaManeuver";
            this.nudKarmaManeuver.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaManeuver.TabIndex = 40;
            this.nudKarmaManeuver.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaSummoningFocus
            // 
            this.lblKarmaSummoningFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaSummoningFocus.AutoSize = true;
            this.lblKarmaSummoningFocus.Location = new System.Drawing.Point(551, 396);
            this.lblKarmaSummoningFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSummoningFocus.Name = "lblKarmaSummoningFocus";
            this.lblKarmaSummoningFocus.Size = new System.Drawing.Size(94, 13);
            this.lblKarmaSummoningFocus.TabIndex = 96;
            this.lblKarmaSummoningFocus.Tag = "Label_Options_SummoningFocus";
            this.lblKarmaSummoningFocus.Text = "Summoning Focus";
            // 
            // lblKarmaManeuver
            // 
            this.lblKarmaManeuver.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaManeuver.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaManeuver, 2);
            this.lblKarmaManeuver.Location = new System.Drawing.Point(107, 422);
            this.lblKarmaManeuver.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaManeuver.Name = "lblKarmaManeuver";
            this.lblKarmaManeuver.Size = new System.Drawing.Size(94, 13);
            this.lblKarmaManeuver.TabIndex = 39;
            this.lblKarmaManeuver.Tag = "Label_Options_CombatManeuver";
            this.lblKarmaManeuver.Text = "Combat Maneuver";
            // 
            // nudKarmaNewActiveSkill
            // 
            this.nudKarmaNewActiveSkill.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaNewActiveSkill.Location = new System.Drawing.Point(207, 81);
            this.nudKarmaNewActiveSkill.Name = "nudKarmaNewActiveSkill";
            this.nudKarmaNewActiveSkill.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaNewActiveSkill.TabIndex = 5;
            this.nudKarmaNewActiveSkill.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaComplexFormSkillsoftExtra
            // 
            this.lblKarmaComplexFormSkillsoftExtra.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaComplexFormSkillsoftExtra, 2);
            this.lblKarmaComplexFormSkillsoftExtra.Location = new System.Drawing.Point(273, 370);
            this.lblKarmaComplexFormSkillsoftExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaComplexFormSkillsoftExtra.Name = "lblKarmaComplexFormSkillsoftExtra";
            this.lblKarmaComplexFormSkillsoftExtra.Size = new System.Drawing.Size(46, 13);
            this.lblKarmaComplexFormSkillsoftExtra.TabIndex = 35;
            this.lblKarmaComplexFormSkillsoftExtra.Tag = "Label_Options_Rating";
            this.lblKarmaComplexFormSkillsoftExtra.Text = "x Rating";
            // 
            // lblKarmaSpellcastingFocusExtra
            // 
            this.lblKarmaSpellcastingFocusExtra.AutoSize = true;
            this.lblKarmaSpellcastingFocusExtra.Location = new System.Drawing.Point(717, 370);
            this.lblKarmaSpellcastingFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSpellcastingFocusExtra.Name = "lblKarmaSpellcastingFocusExtra";
            this.lblKarmaSpellcastingFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaSpellcastingFocusExtra.TabIndex = 95;
            this.lblKarmaSpellcastingFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaSpellcastingFocusExtra.Text = "x Force";
            // 
            // nudKarmaComplexFormSkillsoft
            // 
            this.nudKarmaComplexFormSkillsoft.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaComplexFormSkillsoft.Location = new System.Drawing.Point(207, 367);
            this.nudKarmaComplexFormSkillsoft.Name = "nudKarmaComplexFormSkillsoft";
            this.nudKarmaComplexFormSkillsoft.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaComplexFormSkillsoft.TabIndex = 34;
            this.nudKarmaComplexFormSkillsoft.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaAlchemicalFocus
            // 
            this.lblKarmaAlchemicalFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaAlchemicalFocus.AutoSize = true;
            this.lblKarmaAlchemicalFocus.Location = new System.Drawing.Point(555, 84);
            this.lblKarmaAlchemicalFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaAlchemicalFocus.Name = "lblKarmaAlchemicalFocus";
            this.lblKarmaAlchemicalFocus.Size = new System.Drawing.Size(90, 13);
            this.lblKarmaAlchemicalFocus.TabIndex = 60;
            this.lblKarmaAlchemicalFocus.Tag = "Label_Options_AlchemicalFocus";
            this.lblKarmaAlchemicalFocus.Text = "Alchemical Focus";
            // 
            // lblKarmaComplexFormSkillsoft
            // 
            this.lblKarmaComplexFormSkillsoft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaComplexFormSkillsoft.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaComplexFormSkillsoft, 2);
            this.lblKarmaComplexFormSkillsoft.Location = new System.Drawing.Point(84, 370);
            this.lblKarmaComplexFormSkillsoft.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaComplexFormSkillsoft.Name = "lblKarmaComplexFormSkillsoft";
            this.lblKarmaComplexFormSkillsoft.Size = new System.Drawing.Size(117, 13);
            this.lblKarmaComplexFormSkillsoft.TabIndex = 33;
            this.lblKarmaComplexFormSkillsoft.Tag = "Label_Options_ComplexFormSkillsoft";
            this.lblKarmaComplexFormSkillsoft.Text = "Complex Form Skillsofts";
            // 
            // lblKarmaNuyenPerExtra
            // 
            this.lblKarmaNuyenPerExtra.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaNuyenPerExtra, 2);
            this.lblKarmaNuyenPerExtra.Location = new System.Drawing.Point(273, 448);
            this.lblKarmaNuyenPerExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaNuyenPerExtra.Name = "lblKarmaNuyenPerExtra";
            this.lblKarmaNuyenPerExtra.Size = new System.Drawing.Size(55, 13);
            this.lblKarmaNuyenPerExtra.TabIndex = 43;
            this.lblKarmaNuyenPerExtra.Tag = "Label_Options_PerKarma";
            this.lblKarmaNuyenPerExtra.Text = "per Karma";
            // 
            // nudKarmaSpellcastingFocus
            // 
            this.nudKarmaSpellcastingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaSpellcastingFocus.Location = new System.Drawing.Point(651, 367);
            this.nudKarmaSpellcastingFocus.Name = "nudKarmaSpellcastingFocus";
            this.nudKarmaSpellcastingFocus.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaSpellcastingFocus.TabIndex = 94;
            this.nudKarmaSpellcastingFocus.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // nudKarmaNuyenPer
            // 
            this.nudKarmaNuyenPer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaNuyenPer.Location = new System.Drawing.Point(207, 445);
            this.nudKarmaNuyenPer.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.nudKarmaNuyenPer.Name = "nudKarmaNuyenPer";
            this.nudKarmaNuyenPer.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaNuyenPer.TabIndex = 42;
            this.nudKarmaNuyenPer.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaSpiritExtra
            // 
            this.lblKarmaSpiritExtra.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaSpiritExtra, 2);
            this.lblKarmaSpiritExtra.Location = new System.Drawing.Point(273, 396);
            this.lblKarmaSpiritExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSpiritExtra.Name = "lblKarmaSpiritExtra";
            this.lblKarmaSpiritExtra.Size = new System.Drawing.Size(87, 13);
            this.lblKarmaSpiritExtra.TabIndex = 38;
            this.lblKarmaSpiritExtra.Tag = "Label_Options_ServicesOwed";
            this.lblKarmaSpiritExtra.Text = "x Services Owed";
            // 
            // lblKarmaNuyenPer
            // 
            this.lblKarmaNuyenPer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaNuyenPer.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaNuyenPer, 2);
            this.lblKarmaNuyenPer.Location = new System.Drawing.Point(163, 448);
            this.lblKarmaNuyenPer.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaNuyenPer.Name = "lblKarmaNuyenPer";
            this.lblKarmaNuyenPer.Size = new System.Drawing.Size(38, 13);
            this.lblKarmaNuyenPer.TabIndex = 41;
            this.lblKarmaNuyenPer.Tag = "Label_Options_Nuyen";
            this.lblKarmaNuyenPer.Text = "Nuyen";
            // 
            // lblKarmaComplexFormOptionExtra
            // 
            this.lblKarmaComplexFormOptionExtra.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaComplexFormOptionExtra, 2);
            this.lblKarmaComplexFormOptionExtra.Location = new System.Drawing.Point(273, 344);
            this.lblKarmaComplexFormOptionExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaComplexFormOptionExtra.Name = "lblKarmaComplexFormOptionExtra";
            this.lblKarmaComplexFormOptionExtra.Size = new System.Drawing.Size(46, 13);
            this.lblKarmaComplexFormOptionExtra.TabIndex = 32;
            this.lblKarmaComplexFormOptionExtra.Tag = "Label_Options_Rating";
            this.lblKarmaComplexFormOptionExtra.Text = "x Rating";
            // 
            // nudKarmaSpirit
            // 
            this.nudKarmaSpirit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaSpirit.Location = new System.Drawing.Point(207, 393);
            this.nudKarmaSpirit.Name = "nudKarmaSpirit";
            this.nudKarmaSpirit.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaSpirit.TabIndex = 37;
            this.nudKarmaSpirit.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // nudKarmaAlchemicalFocus
            // 
            this.nudKarmaAlchemicalFocus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaAlchemicalFocus.Location = new System.Drawing.Point(651, 81);
            this.nudKarmaAlchemicalFocus.Name = "nudKarmaAlchemicalFocus";
            this.nudKarmaAlchemicalFocus.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaAlchemicalFocus.TabIndex = 61;
            this.nudKarmaAlchemicalFocus.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaSpirit
            // 
            this.lblKarmaSpirit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaSpirit.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaSpirit, 2);
            this.lblKarmaSpirit.Location = new System.Drawing.Point(171, 396);
            this.lblKarmaSpirit.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSpirit.Name = "lblKarmaSpirit";
            this.lblKarmaSpirit.Size = new System.Drawing.Size(30, 13);
            this.lblKarmaSpirit.TabIndex = 36;
            this.lblKarmaSpirit.Tag = "Label_Options_Spirit";
            this.lblKarmaSpirit.Text = "Spirit";
            // 
            // nudKarmaComplexFormOption
            // 
            this.nudKarmaComplexFormOption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaComplexFormOption.Location = new System.Drawing.Point(207, 341);
            this.nudKarmaComplexFormOption.Name = "nudKarmaComplexFormOption";
            this.nudKarmaComplexFormOption.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaComplexFormOption.TabIndex = 31;
            this.nudKarmaComplexFormOption.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaSpellcastingFocus
            // 
            this.lblKarmaSpellcastingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaSpellcastingFocus.AutoSize = true;
            this.lblKarmaSpellcastingFocus.Location = new System.Drawing.Point(549, 370);
            this.lblKarmaSpellcastingFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSpellcastingFocus.Name = "lblKarmaSpellcastingFocus";
            this.lblKarmaSpellcastingFocus.Size = new System.Drawing.Size(96, 13);
            this.lblKarmaSpellcastingFocus.TabIndex = 93;
            this.lblKarmaSpellcastingFocus.Tag = "Label_Options_SpellcastingFocus";
            this.lblKarmaSpellcastingFocus.Text = "Spellcasting Focus";
            // 
            // lblKarmaComplexFormOption
            // 
            this.lblKarmaComplexFormOption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaComplexFormOption.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaComplexFormOption, 2);
            this.lblKarmaComplexFormOption.Location = new System.Drawing.Point(89, 344);
            this.lblKarmaComplexFormOption.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaComplexFormOption.Name = "lblKarmaComplexFormOption";
            this.lblKarmaComplexFormOption.Size = new System.Drawing.Size(112, 13);
            this.lblKarmaComplexFormOption.TabIndex = 30;
            this.lblKarmaComplexFormOption.Tag = "Label_Options_ComplexFormOptions";
            this.lblKarmaComplexFormOption.Text = "Complex Form Options";
            // 
            // lblKarmaAlchemicalFocusExtra
            // 
            this.lblKarmaAlchemicalFocusExtra.AutoSize = true;
            this.lblKarmaAlchemicalFocusExtra.Location = new System.Drawing.Point(717, 84);
            this.lblKarmaAlchemicalFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaAlchemicalFocusExtra.Name = "lblKarmaAlchemicalFocusExtra";
            this.lblKarmaAlchemicalFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaAlchemicalFocusExtra.TabIndex = 62;
            this.lblKarmaAlchemicalFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaAlchemicalFocusExtra.Text = "x Force";
            // 
            // lblKarmaNewSkillGroup
            // 
            this.lblKarmaNewSkillGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaNewSkillGroup.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaNewSkillGroup, 2);
            this.lblKarmaNewSkillGroup.Location = new System.Drawing.Point(118, 110);
            this.lblKarmaNewSkillGroup.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaNewSkillGroup.Name = "lblKarmaNewSkillGroup";
            this.lblKarmaNewSkillGroup.Size = new System.Drawing.Size(83, 13);
            this.lblKarmaNewSkillGroup.TabIndex = 6;
            this.lblKarmaNewSkillGroup.Tag = "Label_Options_NewSkillGroup";
            this.lblKarmaNewSkillGroup.Text = "New Skill Group";
            // 
            // nudKarmaNewSkillGroup
            // 
            this.nudKarmaNewSkillGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaNewSkillGroup.Location = new System.Drawing.Point(207, 107);
            this.nudKarmaNewSkillGroup.Name = "nudKarmaNewSkillGroup";
            this.nudKarmaNewSkillGroup.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaNewSkillGroup.TabIndex = 7;
            this.nudKarmaNewSkillGroup.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaBanishingFocus
            // 
            this.lblKarmaBanishingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaBanishingFocus.AutoSize = true;
            this.lblKarmaBanishingFocus.Location = new System.Drawing.Point(560, 110);
            this.lblKarmaBanishingFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaBanishingFocus.Name = "lblKarmaBanishingFocus";
            this.lblKarmaBanishingFocus.Size = new System.Drawing.Size(85, 13);
            this.lblKarmaBanishingFocus.TabIndex = 63;
            this.lblKarmaBanishingFocus.Tag = "Label_Options_BanishingFocus";
            this.lblKarmaBanishingFocus.Text = "Banishing Focus";
            // 
            // nudKarmaBanishingFocus
            // 
            this.nudKarmaBanishingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaBanishingFocus.Location = new System.Drawing.Point(651, 107);
            this.nudKarmaBanishingFocus.Name = "nudKarmaBanishingFocus";
            this.nudKarmaBanishingFocus.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaBanishingFocus.TabIndex = 64;
            this.nudKarmaBanishingFocus.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaBanishingFocusExtra
            // 
            this.lblKarmaBanishingFocusExtra.AutoSize = true;
            this.lblKarmaBanishingFocusExtra.Location = new System.Drawing.Point(717, 110);
            this.lblKarmaBanishingFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaBanishingFocusExtra.Name = "lblKarmaBanishingFocusExtra";
            this.lblKarmaBanishingFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaBanishingFocusExtra.TabIndex = 65;
            this.lblKarmaBanishingFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaBanishingFocusExtra.Text = "x Force";
            // 
            // lblKarmaImproveKnowledgeSkill
            // 
            this.lblKarmaImproveKnowledgeSkill.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaImproveKnowledgeSkill.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaImproveKnowledgeSkill, 2);
            this.lblKarmaImproveKnowledgeSkill.Location = new System.Drawing.Point(55, 136);
            this.lblKarmaImproveKnowledgeSkill.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaImproveKnowledgeSkill.Name = "lblKarmaImproveKnowledgeSkill";
            this.lblKarmaImproveKnowledgeSkill.Size = new System.Drawing.Size(146, 13);
            this.lblKarmaImproveKnowledgeSkill.TabIndex = 8;
            this.lblKarmaImproveKnowledgeSkill.Tag = "Label_Options_ImproveKnowledgeSkill";
            this.lblKarmaImproveKnowledgeSkill.Text = "Improve Knowledge Skill by 1";
            // 
            // lblKarmaQiFocusExtra
            // 
            this.lblKarmaQiFocusExtra.AutoSize = true;
            this.lblKarmaQiFocusExtra.Location = new System.Drawing.Point(717, 318);
            this.lblKarmaQiFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaQiFocusExtra.Name = "lblKarmaQiFocusExtra";
            this.lblKarmaQiFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaQiFocusExtra.TabIndex = 92;
            this.lblKarmaQiFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaQiFocusExtra.Text = "x Force";
            // 
            // nudKarmaImproveKnowledgeSkill
            // 
            this.nudKarmaImproveKnowledgeSkill.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaImproveKnowledgeSkill.Location = new System.Drawing.Point(207, 133);
            this.nudKarmaImproveKnowledgeSkill.Name = "nudKarmaImproveKnowledgeSkill";
            this.nudKarmaImproveKnowledgeSkill.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaImproveKnowledgeSkill.TabIndex = 9;
            this.nudKarmaImproveKnowledgeSkill.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // nudKarmaQiFocus
            // 
            this.nudKarmaQiFocus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaQiFocus.Location = new System.Drawing.Point(651, 315);
            this.nudKarmaQiFocus.Name = "nudKarmaQiFocus";
            this.nudKarmaQiFocus.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaQiFocus.TabIndex = 91;
            this.nudKarmaQiFocus.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaImproveKnowledgeSkillExtra
            // 
            this.lblKarmaImproveKnowledgeSkillExtra.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaImproveKnowledgeSkillExtra, 2);
            this.lblKarmaImproveKnowledgeSkillExtra.Location = new System.Drawing.Point(273, 136);
            this.lblKarmaImproveKnowledgeSkillExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaImproveKnowledgeSkillExtra.Name = "lblKarmaImproveKnowledgeSkillExtra";
            this.lblKarmaImproveKnowledgeSkillExtra.Size = new System.Drawing.Size(71, 13);
            this.lblKarmaImproveKnowledgeSkillExtra.TabIndex = 10;
            this.lblKarmaImproveKnowledgeSkillExtra.Tag = "Label_Options_NewRating";
            this.lblKarmaImproveKnowledgeSkillExtra.Text = "x New Rating";
            // 
            // lblKarmaQiFocus
            // 
            this.lblKarmaQiFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaQiFocus.AutoSize = true;
            this.lblKarmaQiFocus.Location = new System.Drawing.Point(596, 318);
            this.lblKarmaQiFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaQiFocus.Name = "lblKarmaQiFocus";
            this.lblKarmaQiFocus.Size = new System.Drawing.Size(49, 13);
            this.lblKarmaQiFocus.TabIndex = 90;
            this.lblKarmaQiFocus.Tag = "Label_Options_QiFocus";
            this.lblKarmaQiFocus.Text = "Qi Focus";
            // 
            // lblKarmaBindingFocus
            // 
            this.lblKarmaBindingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaBindingFocus.AutoSize = true;
            this.lblKarmaBindingFocus.Location = new System.Drawing.Point(571, 136);
            this.lblKarmaBindingFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaBindingFocus.Name = "lblKarmaBindingFocus";
            this.lblKarmaBindingFocus.Size = new System.Drawing.Size(74, 13);
            this.lblKarmaBindingFocus.TabIndex = 66;
            this.lblKarmaBindingFocus.Tag = "Label_Options_BindingFocus";
            this.lblKarmaBindingFocus.Text = "Binding Focus";
            // 
            // lblKarmaPowerFocusExtra
            // 
            this.lblKarmaPowerFocusExtra.AutoSize = true;
            this.lblKarmaPowerFocusExtra.Location = new System.Drawing.Point(717, 292);
            this.lblKarmaPowerFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaPowerFocusExtra.Name = "lblKarmaPowerFocusExtra";
            this.lblKarmaPowerFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaPowerFocusExtra.TabIndex = 89;
            this.lblKarmaPowerFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaPowerFocusExtra.Text = "x Force";
            // 
            // nudKarmaBindingFocus
            // 
            this.nudKarmaBindingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaBindingFocus.Location = new System.Drawing.Point(651, 133);
            this.nudKarmaBindingFocus.Name = "nudKarmaBindingFocus";
            this.nudKarmaBindingFocus.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaBindingFocus.TabIndex = 67;
            this.nudKarmaBindingFocus.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // nudKarmaPowerFocus
            // 
            this.nudKarmaPowerFocus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaPowerFocus.Location = new System.Drawing.Point(651, 289);
            this.nudKarmaPowerFocus.Name = "nudKarmaPowerFocus";
            this.nudKarmaPowerFocus.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaPowerFocus.TabIndex = 88;
            this.nudKarmaPowerFocus.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaBindingFocusExtra
            // 
            this.lblKarmaBindingFocusExtra.AutoSize = true;
            this.lblKarmaBindingFocusExtra.Location = new System.Drawing.Point(717, 136);
            this.lblKarmaBindingFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaBindingFocusExtra.Name = "lblKarmaBindingFocusExtra";
            this.lblKarmaBindingFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaBindingFocusExtra.TabIndex = 68;
            this.lblKarmaBindingFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaBindingFocusExtra.Text = "x Force";
            // 
            // lblKarmaPowerFocus
            // 
            this.lblKarmaPowerFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaPowerFocus.AutoSize = true;
            this.lblKarmaPowerFocus.Location = new System.Drawing.Point(576, 292);
            this.lblKarmaPowerFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaPowerFocus.Name = "lblKarmaPowerFocus";
            this.lblKarmaPowerFocus.Size = new System.Drawing.Size(69, 13);
            this.lblKarmaPowerFocus.TabIndex = 87;
            this.lblKarmaPowerFocus.Tag = "Label_Options_PowerFocus";
            this.lblKarmaPowerFocus.Text = "Power Focus";
            // 
            // lblKarmaImproveActiveSkill
            // 
            this.lblKarmaImproveActiveSkill.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaImproveActiveSkill.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaImproveActiveSkill, 2);
            this.lblKarmaImproveActiveSkill.Location = new System.Drawing.Point(78, 162);
            this.lblKarmaImproveActiveSkill.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaImproveActiveSkill.Name = "lblKarmaImproveActiveSkill";
            this.lblKarmaImproveActiveSkill.Size = new System.Drawing.Size(123, 13);
            this.lblKarmaImproveActiveSkill.TabIndex = 11;
            this.lblKarmaImproveActiveSkill.Tag = "Label_Options_ImproveActiveSkill";
            this.lblKarmaImproveActiveSkill.Text = "Improve Active Skill by 1";
            // 
            // lblKarmaMaskingFocusExtra
            // 
            this.lblKarmaMaskingFocusExtra.AutoSize = true;
            this.lblKarmaMaskingFocusExtra.Location = new System.Drawing.Point(717, 266);
            this.lblKarmaMaskingFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaMaskingFocusExtra.Name = "lblKarmaMaskingFocusExtra";
            this.lblKarmaMaskingFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaMaskingFocusExtra.TabIndex = 86;
            this.lblKarmaMaskingFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaMaskingFocusExtra.Text = "x Force";
            // 
            // nudKarmaImproveActiveSkill
            // 
            this.nudKarmaImproveActiveSkill.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaImproveActiveSkill.Location = new System.Drawing.Point(207, 159);
            this.nudKarmaImproveActiveSkill.Name = "nudKarmaImproveActiveSkill";
            this.nudKarmaImproveActiveSkill.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaImproveActiveSkill.TabIndex = 12;
            this.nudKarmaImproveActiveSkill.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // nudKarmaMaskingFocus
            // 
            this.nudKarmaMaskingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaMaskingFocus.Location = new System.Drawing.Point(651, 263);
            this.nudKarmaMaskingFocus.Name = "nudKarmaMaskingFocus";
            this.nudKarmaMaskingFocus.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaMaskingFocus.TabIndex = 85;
            this.nudKarmaMaskingFocus.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaImproveActiveSkillExtra
            // 
            this.lblKarmaImproveActiveSkillExtra.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaImproveActiveSkillExtra, 2);
            this.lblKarmaImproveActiveSkillExtra.Location = new System.Drawing.Point(273, 162);
            this.lblKarmaImproveActiveSkillExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaImproveActiveSkillExtra.Name = "lblKarmaImproveActiveSkillExtra";
            this.lblKarmaImproveActiveSkillExtra.Size = new System.Drawing.Size(71, 13);
            this.lblKarmaImproveActiveSkillExtra.TabIndex = 13;
            this.lblKarmaImproveActiveSkillExtra.Tag = "Label_Options_NewRating";
            this.lblKarmaImproveActiveSkillExtra.Text = "x New Rating";
            // 
            // lblKarmaMaskingFocus
            // 
            this.lblKarmaMaskingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaMaskingFocus.AutoSize = true;
            this.lblKarmaMaskingFocus.Location = new System.Drawing.Point(566, 266);
            this.lblKarmaMaskingFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaMaskingFocus.Name = "lblKarmaMaskingFocus";
            this.lblKarmaMaskingFocus.Size = new System.Drawing.Size(79, 13);
            this.lblKarmaMaskingFocus.TabIndex = 84;
            this.lblKarmaMaskingFocus.Tag = "Label_Options_MaskingFocus";
            this.lblKarmaMaskingFocus.Text = "Masking Focus";
            // 
            // lblKarmaCenteringFocus
            // 
            this.lblKarmaCenteringFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaCenteringFocus.AutoSize = true;
            this.lblKarmaCenteringFocus.Location = new System.Drawing.Point(561, 162);
            this.lblKarmaCenteringFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaCenteringFocus.Name = "lblKarmaCenteringFocus";
            this.lblKarmaCenteringFocus.Size = new System.Drawing.Size(84, 13);
            this.lblKarmaCenteringFocus.TabIndex = 69;
            this.lblKarmaCenteringFocus.Tag = "Label_Options_CenteringFocus";
            this.lblKarmaCenteringFocus.Text = "Centering Focus";
            // 
            // nudKarmaCenteringFocus
            // 
            this.nudKarmaCenteringFocus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaCenteringFocus.Location = new System.Drawing.Point(651, 159);
            this.nudKarmaCenteringFocus.Name = "nudKarmaCenteringFocus";
            this.nudKarmaCenteringFocus.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaCenteringFocus.TabIndex = 70;
            this.nudKarmaCenteringFocus.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaCenteringFocusExtra
            // 
            this.lblKarmaCenteringFocusExtra.AutoSize = true;
            this.lblKarmaCenteringFocusExtra.Location = new System.Drawing.Point(717, 162);
            this.lblKarmaCenteringFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaCenteringFocusExtra.Name = "lblKarmaCenteringFocusExtra";
            this.lblKarmaCenteringFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaCenteringFocusExtra.TabIndex = 71;
            this.lblKarmaCenteringFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaCenteringFocusExtra.Text = "x Force";
            // 
            // lblKarmaCounterspellingFocus
            // 
            this.lblKarmaCounterspellingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaCounterspellingFocus.AutoSize = true;
            this.lblKarmaCounterspellingFocus.Location = new System.Drawing.Point(534, 188);
            this.lblKarmaCounterspellingFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaCounterspellingFocus.Name = "lblKarmaCounterspellingFocus";
            this.lblKarmaCounterspellingFocus.Size = new System.Drawing.Size(111, 13);
            this.lblKarmaCounterspellingFocus.TabIndex = 72;
            this.lblKarmaCounterspellingFocus.Tag = "Label_Options_CounterspellingFocus";
            this.lblKarmaCounterspellingFocus.Text = "Counterspelling Focus";
            // 
            // nudKarmaCounterspellingFocus
            // 
            this.nudKarmaCounterspellingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaCounterspellingFocus.Location = new System.Drawing.Point(651, 185);
            this.nudKarmaCounterspellingFocus.Name = "nudKarmaCounterspellingFocus";
            this.nudKarmaCounterspellingFocus.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaCounterspellingFocus.TabIndex = 73;
            this.nudKarmaCounterspellingFocus.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaCounterspellingFocusExtra
            // 
            this.lblKarmaCounterspellingFocusExtra.AutoSize = true;
            this.lblKarmaCounterspellingFocusExtra.Location = new System.Drawing.Point(717, 188);
            this.lblKarmaCounterspellingFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaCounterspellingFocusExtra.Name = "lblKarmaCounterspellingFocusExtra";
            this.lblKarmaCounterspellingFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaCounterspellingFocusExtra.TabIndex = 74;
            this.lblKarmaCounterspellingFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaCounterspellingFocusExtra.Text = "x Force";
            // 
            // lblKarmaImproveSkillGroup
            // 
            this.lblKarmaImproveSkillGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaImproveSkillGroup.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaImproveSkillGroup, 2);
            this.lblKarmaImproveSkillGroup.Location = new System.Drawing.Point(79, 188);
            this.lblKarmaImproveSkillGroup.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaImproveSkillGroup.Name = "lblKarmaImproveSkillGroup";
            this.lblKarmaImproveSkillGroup.Size = new System.Drawing.Size(122, 13);
            this.lblKarmaImproveSkillGroup.TabIndex = 14;
            this.lblKarmaImproveSkillGroup.Tag = "Label_Options_ImproveSkillGroup";
            this.lblKarmaImproveSkillGroup.Text = "Improve Skill Group by 1";
            // 
            // lblKarmaDisenchantingFocusExtra
            // 
            this.lblKarmaDisenchantingFocusExtra.AutoSize = true;
            this.lblKarmaDisenchantingFocusExtra.Location = new System.Drawing.Point(717, 214);
            this.lblKarmaDisenchantingFocusExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaDisenchantingFocusExtra.Name = "lblKarmaDisenchantingFocusExtra";
            this.lblKarmaDisenchantingFocusExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaDisenchantingFocusExtra.TabIndex = 83;
            this.lblKarmaDisenchantingFocusExtra.Tag = "Label_Options_Force";
            this.lblKarmaDisenchantingFocusExtra.Text = "x Force";
            // 
            // nudKarmaImproveSkillGroup
            // 
            this.nudKarmaImproveSkillGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaImproveSkillGroup.Location = new System.Drawing.Point(207, 185);
            this.nudKarmaImproveSkillGroup.Name = "nudKarmaImproveSkillGroup";
            this.nudKarmaImproveSkillGroup.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaImproveSkillGroup.TabIndex = 15;
            this.nudKarmaImproveSkillGroup.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // nudKarmaDisenchantingFocus
            // 
            this.nudKarmaDisenchantingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaDisenchantingFocus.Location = new System.Drawing.Point(651, 211);
            this.nudKarmaDisenchantingFocus.Name = "nudKarmaDisenchantingFocus";
            this.nudKarmaDisenchantingFocus.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaDisenchantingFocus.TabIndex = 82;
            this.nudKarmaDisenchantingFocus.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaImproveSkillGroupExtra
            // 
            this.lblKarmaImproveSkillGroupExtra.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaImproveSkillGroupExtra, 2);
            this.lblKarmaImproveSkillGroupExtra.Location = new System.Drawing.Point(273, 188);
            this.lblKarmaImproveSkillGroupExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaImproveSkillGroupExtra.Name = "lblKarmaImproveSkillGroupExtra";
            this.lblKarmaImproveSkillGroupExtra.Size = new System.Drawing.Size(71, 13);
            this.lblKarmaImproveSkillGroupExtra.TabIndex = 16;
            this.lblKarmaImproveSkillGroupExtra.Tag = "Label_Options_NewRating";
            this.lblKarmaImproveSkillGroupExtra.Text = "x New Rating";
            // 
            // lblKarmaDisenchantingFocus
            // 
            this.lblKarmaDisenchantingFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaDisenchantingFocus.AutoSize = true;
            this.lblKarmaDisenchantingFocus.Location = new System.Drawing.Point(538, 214);
            this.lblKarmaDisenchantingFocus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaDisenchantingFocus.Name = "lblKarmaDisenchantingFocus";
            this.lblKarmaDisenchantingFocus.Size = new System.Drawing.Size(107, 13);
            this.lblKarmaDisenchantingFocus.TabIndex = 81;
            this.lblKarmaDisenchantingFocus.Tag = "Label_Options_DisenchantingFocus";
            this.lblKarmaDisenchantingFocus.Text = "Disenchanting Focus";
            // 
            // lblKarmaAttribute
            // 
            this.lblKarmaAttribute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaAttribute.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaAttribute, 2);
            this.lblKarmaAttribute.Location = new System.Drawing.Point(91, 214);
            this.lblKarmaAttribute.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaAttribute.Name = "lblKarmaAttribute";
            this.lblKarmaAttribute.Size = new System.Drawing.Size(110, 13);
            this.lblKarmaAttribute.TabIndex = 17;
            this.lblKarmaAttribute.Tag = "Label_Options_ImproveAttribute";
            this.lblKarmaAttribute.Text = "Improve Attribute by 1";
            // 
            // nudKarmaAttribute
            // 
            this.nudKarmaAttribute.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaAttribute.Location = new System.Drawing.Point(207, 211);
            this.nudKarmaAttribute.Name = "nudKarmaAttribute";
            this.nudKarmaAttribute.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaAttribute.TabIndex = 18;
            this.nudKarmaAttribute.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaAttributeExtra
            // 
            this.lblKarmaAttributeExtra.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaAttributeExtra, 2);
            this.lblKarmaAttributeExtra.Location = new System.Drawing.Point(273, 214);
            this.lblKarmaAttributeExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaAttributeExtra.Name = "lblKarmaAttributeExtra";
            this.lblKarmaAttributeExtra.Size = new System.Drawing.Size(71, 13);
            this.lblKarmaAttributeExtra.TabIndex = 19;
            this.lblKarmaAttributeExtra.Tag = "Label_Options_NewRating";
            this.lblKarmaAttributeExtra.Text = "x New Rating";
            // 
            // lblKarmaQuality
            // 
            this.lblKarmaQuality.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaQuality.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaQuality, 2);
            this.lblKarmaQuality.Location = new System.Drawing.Point(68, 240);
            this.lblKarmaQuality.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaQuality.Name = "lblKarmaQuality";
            this.lblKarmaQuality.Size = new System.Drawing.Size(133, 13);
            this.lblKarmaQuality.TabIndex = 20;
            this.lblKarmaQuality.Tag = "Label_Options_Qualities";
            this.lblKarmaQuality.Text = "Positive / Negative Quality";
            // 
            // lblKarmaImproveComplexFormExtra
            // 
            this.lblKarmaImproveComplexFormExtra.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaImproveComplexFormExtra, 2);
            this.lblKarmaImproveComplexFormExtra.Location = new System.Drawing.Point(273, 318);
            this.lblKarmaImproveComplexFormExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaImproveComplexFormExtra.Name = "lblKarmaImproveComplexFormExtra";
            this.lblKarmaImproveComplexFormExtra.Size = new System.Drawing.Size(71, 13);
            this.lblKarmaImproveComplexFormExtra.TabIndex = 29;
            this.lblKarmaImproveComplexFormExtra.Tag = "Label_Options_NewRating";
            this.lblKarmaImproveComplexFormExtra.Text = "x New Rating";
            // 
            // nudKarmaQuality
            // 
            this.nudKarmaQuality.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaQuality.Location = new System.Drawing.Point(207, 237);
            this.nudKarmaQuality.Name = "nudKarmaQuality";
            this.nudKarmaQuality.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaQuality.TabIndex = 21;
            this.nudKarmaQuality.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // nudKarmaImproveComplexForm
            // 
            this.nudKarmaImproveComplexForm.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaImproveComplexForm.Location = new System.Drawing.Point(207, 315);
            this.nudKarmaImproveComplexForm.Name = "nudKarmaImproveComplexForm";
            this.nudKarmaImproveComplexForm.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaImproveComplexForm.TabIndex = 28;
            this.nudKarmaImproveComplexForm.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaQualityExtra
            // 
            this.lblKarmaQualityExtra.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaQualityExtra, 2);
            this.lblKarmaQualityExtra.Location = new System.Drawing.Point(273, 240);
            this.lblKarmaQualityExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaQualityExtra.Name = "lblKarmaQualityExtra";
            this.lblKarmaQualityExtra.Size = new System.Drawing.Size(53, 13);
            this.lblKarmaQualityExtra.TabIndex = 22;
            this.lblKarmaQualityExtra.Tag = "Label_Options_BPCost";
            this.lblKarmaQualityExtra.Text = "x BP Cost";
            // 
            // lblKarmaImproveComplexForm
            // 
            this.lblKarmaImproveComplexForm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaImproveComplexForm.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaImproveComplexForm, 2);
            this.lblKarmaImproveComplexForm.Location = new System.Drawing.Point(64, 318);
            this.lblKarmaImproveComplexForm.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaImproveComplexForm.Name = "lblKarmaImproveComplexForm";
            this.lblKarmaImproveComplexForm.Size = new System.Drawing.Size(137, 13);
            this.lblKarmaImproveComplexForm.TabIndex = 27;
            this.lblKarmaImproveComplexForm.Tag = "Label_Options_ImproveComplexForm";
            this.lblKarmaImproveComplexForm.Text = "Improve Complex Form by 1";
            // 
            // lblKarmaSpell
            // 
            this.lblKarmaSpell.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaSpell.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaSpell, 2);
            this.lblKarmaSpell.Location = new System.Drawing.Point(146, 266);
            this.lblKarmaSpell.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSpell.Name = "lblKarmaSpell";
            this.lblKarmaSpell.Size = new System.Drawing.Size(55, 13);
            this.lblKarmaSpell.TabIndex = 23;
            this.lblKarmaSpell.Tag = "Label_Options_NewSpell";
            this.lblKarmaSpell.Text = "New Spell";
            // 
            // nudKarmaSpell
            // 
            this.nudKarmaSpell.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaSpell.Location = new System.Drawing.Point(207, 263);
            this.nudKarmaSpell.Name = "nudKarmaSpell";
            this.nudKarmaSpell.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaSpell.TabIndex = 24;
            this.nudKarmaSpell.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblKarmaNewComplexForm
            // 
            this.lblKarmaNewComplexForm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaNewComplexForm.AutoSize = true;
            this.tlpKarmaCostsList.SetColumnSpan(this.lblKarmaNewComplexForm, 2);
            this.lblKarmaNewComplexForm.Location = new System.Drawing.Point(103, 292);
            this.lblKarmaNewComplexForm.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaNewComplexForm.Name = "lblKarmaNewComplexForm";
            this.lblKarmaNewComplexForm.Size = new System.Drawing.Size(98, 13);
            this.lblKarmaNewComplexForm.TabIndex = 25;
            this.lblKarmaNewComplexForm.Tag = "Label_Options_NewComplexForm";
            this.lblKarmaNewComplexForm.Text = "New Complex Form";
            // 
            // nudKarmaNewComplexForm
            // 
            this.nudKarmaNewComplexForm.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaNewComplexForm.Location = new System.Drawing.Point(207, 289);
            this.nudKarmaNewComplexForm.Name = "nudKarmaNewComplexForm";
            this.nudKarmaNewComplexForm.Size = new System.Drawing.Size(60, 20);
            this.nudKarmaNewComplexForm.TabIndex = 26;
            this.nudKarmaNewComplexForm.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // nudMetatypeCostsKarmaMultiplier
            // 
            this.nudMetatypeCostsKarmaMultiplier.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudMetatypeCostsKarmaMultiplier.Location = new System.Drawing.Point(651, 549);
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
            this.nudMetatypeCostsKarmaMultiplier.Size = new System.Drawing.Size(60, 20);
            this.nudMetatypeCostsKarmaMultiplier.TabIndex = 124;
            this.nudMetatypeCostsKarmaMultiplier.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMetatypeCostsKarmaMultiplier.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblMetatypeCostsKarmaMultiplierLabel
            // 
            this.lblMetatypeCostsKarmaMultiplierLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMetatypeCostsKarmaMultiplierLabel.AutoSize = true;
            this.lblMetatypeCostsKarmaMultiplierLabel.Location = new System.Drawing.Point(493, 552);
            this.lblMetatypeCostsKarmaMultiplierLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetatypeCostsKarmaMultiplierLabel.Name = "lblMetatypeCostsKarmaMultiplierLabel";
            this.lblMetatypeCostsKarmaMultiplierLabel.Size = new System.Drawing.Size(152, 13);
            this.lblMetatypeCostsKarmaMultiplierLabel.TabIndex = 125;
            this.lblMetatypeCostsKarmaMultiplierLabel.Tag = "Label_Options_MetatypesCostKarma";
            this.lblMetatypeCostsKarmaMultiplierLabel.Text = "Metatype Karma Cost Multiplier";
            // 
            // lblNuyenPerBP
            // 
            this.lblNuyenPerBP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNuyenPerBP.AutoSize = true;
            this.lblNuyenPerBP.Location = new System.Drawing.Point(556, 578);
            this.lblNuyenPerBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblNuyenPerBP.Name = "lblNuyenPerBP";
            this.lblNuyenPerBP.Size = new System.Drawing.Size(89, 13);
            this.lblNuyenPerBP.TabIndex = 126;
            this.lblNuyenPerBP.Tag = "Label_Options_NuyenPerBP";
            this.lblNuyenPerBP.Text = "Nuyen per Karma";
            // 
            // nudNuyenPerBP
            // 
            this.nudNuyenPerBP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudNuyenPerBP.Location = new System.Drawing.Point(651, 575);
            this.nudNuyenPerBP.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudNuyenPerBP.Name = "nudNuyenPerBP";
            this.nudNuyenPerBP.Size = new System.Drawing.Size(60, 20);
            this.nudNuyenPerBP.TabIndex = 127;
            this.nudNuyenPerBP.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nudNuyenPerBP.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // cmdRestoreDefaultsKarma
            // 
            this.cmdRestoreDefaultsKarma.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdRestoreDefaultsKarma.AutoSize = true;
            this.cmdRestoreDefaultsKarma.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRestoreDefaultsKarma.Location = new System.Drawing.Point(799, 610);
            this.cmdRestoreDefaultsKarma.Name = "cmdRestoreDefaultsKarma";
            this.cmdRestoreDefaultsKarma.Size = new System.Drawing.Size(96, 23);
            this.cmdRestoreDefaultsKarma.TabIndex = 108;
            this.cmdRestoreDefaultsKarma.Tag = "Button_Options_RestoreDefaults";
            this.cmdRestoreDefaultsKarma.Text = "Restore Defaults";
            this.cmdRestoreDefaultsKarma.UseVisualStyleBackColor = true;
            this.cmdRestoreDefaultsKarma.Click += new System.EventHandler(this.cmdRestoreDefaultsKarma_Click);
            // 
            // tabOptionalRules
            // 
            this.tabOptionalRules.BackColor = System.Drawing.SystemColors.Control;
            this.tabOptionalRules.Controls.Add(this.tlpOptionalRules);
            this.tabOptionalRules.Location = new System.Drawing.Point(4, 22);
            this.tabOptionalRules.Name = "tabOptionalRules";
            this.tabOptionalRules.Padding = new System.Windows.Forms.Padding(9);
            this.tabOptionalRules.Size = new System.Drawing.Size(916, 654);
            this.tabOptionalRules.TabIndex = 2;
            this.tabOptionalRules.Tag = "Tab_Options_OptionalRules";
            this.tabOptionalRules.Text = "Optional Rules";
            // 
            // tlpOptionalRules
            // 
            this.tlpOptionalRules.AutoSize = true;
            this.tlpOptionalRules.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpOptionalRules.ColumnCount = 5;
            this.tlpOptionalRules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpOptionalRules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpOptionalRules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpOptionalRules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpOptionalRules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpOptionalRules.Controls.Add(this.cmdDecreaseCustomDirectoryLoadOrder, 3, 2);
            this.tlpOptionalRules.Controls.Add(this.cmdIncreaseCustomDirectoryLoadOrder, 1, 2);
            this.tlpOptionalRules.Controls.Add(this.lblCustomDataDirectoriesLabel, 0, 0);
            this.tlpOptionalRules.Controls.Add(this.cmdAddCustomDirectory, 1, 0);
            this.tlpOptionalRules.Controls.Add(this.treCustomDataDirectories, 0, 1);
            this.tlpOptionalRules.Controls.Add(this.cmdRenameCustomDataDirectory, 2, 0);
            this.tlpOptionalRules.Controls.Add(this.cmdRemoveCustomDirectory, 4, 0);
            this.tlpOptionalRules.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpOptionalRules.Location = new System.Drawing.Point(9, 9);
            this.tlpOptionalRules.Name = "tlpOptionalRules";
            this.tlpOptionalRules.RowCount = 3;
            this.tlpOptionalRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptionalRules.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpOptionalRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptionalRules.Size = new System.Drawing.Size(898, 636);
            this.tlpOptionalRules.TabIndex = 44;
            // 
            // cmdDecreaseCustomDirectoryLoadOrder
            // 
            this.cmdDecreaseCustomDirectoryLoadOrder.AutoSize = true;
            this.tlpOptionalRules.SetColumnSpan(this.cmdDecreaseCustomDirectoryLoadOrder, 2);
            this.cmdDecreaseCustomDirectoryLoadOrder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdDecreaseCustomDirectoryLoadOrder.Location = new System.Drawing.Point(685, 603);
            this.cmdDecreaseCustomDirectoryLoadOrder.Name = "cmdDecreaseCustomDirectoryLoadOrder";
            this.cmdDecreaseCustomDirectoryLoadOrder.Size = new System.Drawing.Size(210, 30);
            this.cmdDecreaseCustomDirectoryLoadOrder.TabIndex = 42;
            this.cmdDecreaseCustomDirectoryLoadOrder.Tag = "Button_DecreaseCustomDirectoryLoadOrder";
            this.cmdDecreaseCustomDirectoryLoadOrder.Text = "Decrease Load Order";
            this.cmdDecreaseCustomDirectoryLoadOrder.UseVisualStyleBackColor = true;
            this.cmdDecreaseCustomDirectoryLoadOrder.Click += new System.EventHandler(this.cmdDecreaseCustomDirectoryLoadOrder_Click);
            // 
            // cmdIncreaseCustomDirectoryLoadOrder
            // 
            this.cmdIncreaseCustomDirectoryLoadOrder.AutoSize = true;
            this.tlpOptionalRules.SetColumnSpan(this.cmdIncreaseCustomDirectoryLoadOrder, 2);
            this.cmdIncreaseCustomDirectoryLoadOrder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdIncreaseCustomDirectoryLoadOrder.Location = new System.Drawing.Point(500, 603);
            this.cmdIncreaseCustomDirectoryLoadOrder.Name = "cmdIncreaseCustomDirectoryLoadOrder";
            this.cmdIncreaseCustomDirectoryLoadOrder.Size = new System.Drawing.Size(179, 30);
            this.cmdIncreaseCustomDirectoryLoadOrder.TabIndex = 43;
            this.cmdIncreaseCustomDirectoryLoadOrder.Tag = "Button_IncreaseCustomDirectoryLoadOrder";
            this.cmdIncreaseCustomDirectoryLoadOrder.Text = "Increase Load Order";
            this.cmdIncreaseCustomDirectoryLoadOrder.UseVisualStyleBackColor = true;
            this.cmdIncreaseCustomDirectoryLoadOrder.Click += new System.EventHandler(this.cmdIncreaseCustomDirectoryLoadOrder_Click);
            // 
            // lblCustomDataDirectoriesLabel
            // 
            this.lblCustomDataDirectoriesLabel.AutoSize = true;
            this.lblCustomDataDirectoriesLabel.Location = new System.Drawing.Point(3, 6);
            this.lblCustomDataDirectoriesLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCustomDataDirectoriesLabel.Name = "lblCustomDataDirectoriesLabel";
            this.lblCustomDataDirectoriesLabel.Size = new System.Drawing.Size(358, 13);
            this.lblCustomDataDirectoriesLabel.TabIndex = 36;
            this.lblCustomDataDirectoriesLabel.Tag = "Label_Options_CustomDataDirectories";
            this.lblCustomDataDirectoriesLabel.Text = "Custom Data Directories to Use (Changes Are Only Applied After a Restart)";
            // 
            // cmdAddCustomDirectory
            // 
            this.cmdAddCustomDirectory.AutoSize = true;
            this.cmdAddCustomDirectory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdAddCustomDirectory.Location = new System.Drawing.Point(500, 3);
            this.cmdAddCustomDirectory.Name = "cmdAddCustomDirectory";
            this.cmdAddCustomDirectory.Size = new System.Drawing.Size(115, 46);
            this.cmdAddCustomDirectory.TabIndex = 38;
            this.cmdAddCustomDirectory.Tag = "Button_AddCustomDirectory";
            this.cmdAddCustomDirectory.Text = "Add Directory";
            this.cmdAddCustomDirectory.UseVisualStyleBackColor = true;
            this.cmdAddCustomDirectory.Click += new System.EventHandler(this.cmdAddCustomDirectory_Click);
            // 
            // treCustomDataDirectories
            // 
            this.treCustomDataDirectories.CheckBoxes = true;
            this.tlpOptionalRules.SetColumnSpan(this.treCustomDataDirectories, 5);
            this.treCustomDataDirectories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treCustomDataDirectories.Location = new System.Drawing.Point(3, 55);
            this.treCustomDataDirectories.Name = "treCustomDataDirectories";
            this.treCustomDataDirectories.ShowLines = false;
            this.treCustomDataDirectories.ShowPlusMinus = false;
            this.treCustomDataDirectories.ShowRootLines = false;
            this.treCustomDataDirectories.Size = new System.Drawing.Size(892, 542);
            this.treCustomDataDirectories.TabIndex = 40;
            this.treCustomDataDirectories.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treCustomDataDirectories_AfterCheck);
            // 
            // cmdRenameCustomDataDirectory
            // 
            this.cmdRenameCustomDataDirectory.AutoSize = true;
            this.tlpOptionalRules.SetColumnSpan(this.cmdRenameCustomDataDirectory, 2);
            this.cmdRenameCustomDataDirectory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdRenameCustomDataDirectory.Location = new System.Drawing.Point(621, 3);
            this.cmdRenameCustomDataDirectory.Name = "cmdRenameCustomDataDirectory";
            this.cmdRenameCustomDataDirectory.Size = new System.Drawing.Size(122, 46);
            this.cmdRenameCustomDataDirectory.TabIndex = 41;
            this.cmdRenameCustomDataDirectory.Tag = "Button_RenameCustomDataDirectory";
            this.cmdRenameCustomDataDirectory.Text = "Rename Entry";
            this.cmdRenameCustomDataDirectory.UseVisualStyleBackColor = true;
            this.cmdRenameCustomDataDirectory.Click += new System.EventHandler(this.cmdRenameCustomDataDirectory_Click);
            // 
            // cmdRemoveCustomDirectory
            // 
            this.cmdRemoveCustomDirectory.AutoSize = true;
            this.cmdRemoveCustomDirectory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdRemoveCustomDirectory.Location = new System.Drawing.Point(749, 3);
            this.cmdRemoveCustomDirectory.Name = "cmdRemoveCustomDirectory";
            this.cmdRemoveCustomDirectory.Size = new System.Drawing.Size(146, 46);
            this.cmdRemoveCustomDirectory.TabIndex = 39;
            this.cmdRemoveCustomDirectory.Tag = "Button_RemoveCustomDirectory";
            this.cmdRemoveCustomDirectory.Text = "Remove Directory";
            this.cmdRemoveCustomDirectory.UseVisualStyleBackColor = true;
            this.cmdRemoveCustomDirectory.Click += new System.EventHandler(this.cmdRemoveCustomDirectory_Click);
            // 
            // tabHouseRules
            // 
            this.tabHouseRules.AutoScroll = true;
            this.tabHouseRules.BackColor = System.Drawing.SystemColors.Control;
            this.tabHouseRules.Controls.Add(this.tlpHouseRules);
            this.tabHouseRules.Location = new System.Drawing.Point(4, 22);
            this.tabHouseRules.Name = "tabHouseRules";
            this.tabHouseRules.Padding = new System.Windows.Forms.Padding(9);
            this.tabHouseRules.Size = new System.Drawing.Size(916, 654);
            this.tabHouseRules.TabIndex = 3;
            this.tabHouseRules.Tag = "Tab_Options_HouseRules";
            this.tabHouseRules.Text = "House Rules";
            // 
            // tlpHouseRules
            // 
            this.tlpHouseRules.AutoScroll = true;
            this.tlpHouseRules.AutoSize = true;
            this.tlpHouseRules.ColumnCount = 9;
            this.tlpHouseRules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.5F));
            this.tlpHouseRules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tlpHouseRules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 7.5F));
            this.tlpHouseRules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tlpHouseRules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 2.5F));
            this.tlpHouseRules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.5F));
            this.tlpHouseRules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 2.5F));
            this.tlpHouseRules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 7.5F));
            this.tlpHouseRules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tlpHouseRules.Controls.Add(this.chkNoArmorEncumbrance, 4, 8);
            this.tlpHouseRules.Controls.Add(this.chkIgnoreArt, 0, 0);
            this.tlpHouseRules.Controls.Add(this.chkExceedNegativeQualitiesLimit, 5, 7);
            this.tlpHouseRules.Controls.Add(this.chkUseTotalValueForFreeKnowledge, 4, 3);
            this.tlpHouseRules.Controls.Add(this.chkExceedNegativeQualities, 4, 6);
            this.tlpHouseRules.Controls.Add(this.chkEnemyKarmaQualityLimit, 0, 19);
            this.tlpHouseRules.Controls.Add(this.chkExceedPositiveQualitiesCostDoubled, 5, 5);
            this.tlpHouseRules.Controls.Add(this.chkExceedPositiveQualities, 4, 4);
            this.tlpHouseRules.Controls.Add(this.chkUnarmedSkillImprovements, 0, 1);
            this.tlpHouseRules.Controls.Add(this.chkCompensateSkillGroupKarmaDifference, 0, 18);
            this.tlpHouseRules.Controls.Add(this.chkCyberlegMovement, 0, 2);
            this.tlpHouseRules.Controls.Add(this.chkMysAdeptSecondMAGAttribute, 0, 17);
            this.tlpHouseRules.Controls.Add(this.chkDontDoubleQualityPurchases, 0, 3);
            this.tlpHouseRules.Controls.Add(this.chkAllowPointBuySpecializationsOnKarmaSkills, 0, 16);
            this.tlpHouseRules.Controls.Add(this.chkDontDoubleQualityRefunds, 0, 4);
            this.tlpHouseRules.Controls.Add(this.chkReverseAttributePriorityOrder, 0, 15);
            this.tlpHouseRules.Controls.Add(this.label2, 6, 0);
            this.tlpHouseRules.Controls.Add(this.chkStrictSkillGroups, 0, 5);
            this.tlpHouseRules.Controls.Add(this.nudContactMultiplier, 7, 0);
            this.tlpHouseRules.Controls.Add(this.chkPrioritySpellsAsAdeptPowers, 0, 14);
            this.tlpHouseRules.Controls.Add(this.chkAllowInitiation, 0, 6);
            this.tlpHouseRules.Controls.Add(this.chkFreeMartialArtSpecialization, 0, 13);
            this.tlpHouseRules.Controls.Add(this.chkAllowCyberwareESSDiscounts, 0, 7);
            this.tlpHouseRules.Controls.Add(this.chkMysAdPp, 0, 12);
            this.tlpHouseRules.Controls.Add(this.chkESSLossReducesMaximumOnly, 0, 8);
            this.tlpHouseRules.Controls.Add(this.chkAlternateMetatypeAttributeKarma, 0, 11);
            this.tlpHouseRules.Controls.Add(this.chkUseCalculatedPublicAwareness, 0, 9);
            this.tlpHouseRules.Controls.Add(this.nudDroneArmorMultiplier, 2, 10);
            this.tlpHouseRules.Controls.Add(this.label4, 1, 10);
            this.tlpHouseRules.Controls.Add(this.chkDroneArmorMultiplier, 0, 10);
            this.tlpHouseRules.Controls.Add(this.chkContactMultiplier, 4, 0);
            this.tlpHouseRules.Controls.Add(this.chkKnowledgeMultiplier, 4, 2);
            this.tlpHouseRules.Controls.Add(this.label3, 6, 2);
            this.tlpHouseRules.Controls.Add(this.nudKnowledgeMultiplier, 7, 2);
            this.tlpHouseRules.Controls.Add(this.chkUseTotalValueForFreeContacts, 4, 1);
            this.tlpHouseRules.Controls.Add(this.chkAllowSkillRegrouping, 4, 9);
            this.tlpHouseRules.Controls.Add(this.chkExtendAnyDetectionSpell, 4, 10);
            this.tlpHouseRules.Controls.Add(this.chkMoreLethalGameplay, 4, 11);
            this.tlpHouseRules.Controls.Add(this.chkSpecialKarmaCost, 4, 12);
            this.tlpHouseRules.Controls.Add(this.chkIgnoreComplexFormLimit, 4, 13);
            this.tlpHouseRules.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpHouseRules.Location = new System.Drawing.Point(9, 9);
            this.tlpHouseRules.Name = "tlpHouseRules";
            this.tlpHouseRules.RowCount = 20;
            this.tlpHouseRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRules.Size = new System.Drawing.Size(898, 636);
            this.tlpHouseRules.TabIndex = 39;
            // 
            // chkNoArmorEncumbrance
            // 
            this.chkNoArmorEncumbrance.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkNoArmorEncumbrance.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkNoArmorEncumbrance, 4);
            this.chkNoArmorEncumbrance.Location = new System.Drawing.Point(449, 206);
            this.chkNoArmorEncumbrance.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkNoArmorEncumbrance.Name = "chkNoArmorEncumbrance";
            this.chkNoArmorEncumbrance.Size = new System.Drawing.Size(396, 17);
            this.chkNoArmorEncumbrance.TabIndex = 38;
            this.chkNoArmorEncumbrance.Tag = "Checkbox_Options_NoArmorEncumbrance";
            this.chkNoArmorEncumbrance.Text = "No Armor Encumbrance";
            this.chkNoArmorEncumbrance.UseVisualStyleBackColor = true;
            this.chkNoArmorEncumbrance.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkIgnoreArt
            // 
            this.chkIgnoreArt.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkIgnoreArt.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkIgnoreArt, 3);
            this.chkIgnoreArt.Location = new System.Drawing.Point(3, 4);
            this.chkIgnoreArt.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkIgnoreArt.Name = "chkIgnoreArt";
            this.chkIgnoreArt.Size = new System.Drawing.Size(396, 18);
            this.chkIgnoreArt.TabIndex = 1;
            this.chkIgnoreArt.Tag = "Checkbox_Options_IgnoreArt";
            this.chkIgnoreArt.Text = "Ignore Art Requirements from Street Grimoire";
            this.chkIgnoreArt.UseVisualStyleBackColor = true;
            this.chkIgnoreArt.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkExceedNegativeQualitiesLimit
            // 
            this.chkExceedNegativeQualitiesLimit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkExceedNegativeQualitiesLimit.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkExceedNegativeQualitiesLimit, 3);
            this.chkExceedNegativeQualitiesLimit.Enabled = false;
            this.chkExceedNegativeQualitiesLimit.Location = new System.Drawing.Point(471, 181);
            this.chkExceedNegativeQualitiesLimit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkExceedNegativeQualitiesLimit.Name = "chkExceedNegativeQualitiesLimit";
            this.chkExceedNegativeQualitiesLimit.Size = new System.Drawing.Size(374, 17);
            this.chkExceedNegativeQualitiesLimit.TabIndex = 18;
            this.chkExceedNegativeQualitiesLimit.Tag = "Checkbox_Options_ExceedNegativeQualitiesLimit";
            this.chkExceedNegativeQualitiesLimit.Text = "Characters only receive up to 35 BP from Negative Qualities";
            this.chkExceedNegativeQualitiesLimit.UseVisualStyleBackColor = true;
            this.chkExceedNegativeQualitiesLimit.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkUseTotalValueForFreeKnowledge
            // 
            this.chkUseTotalValueForFreeKnowledge.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkUseTotalValueForFreeKnowledge.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkUseTotalValueForFreeKnowledge, 4);
            this.chkUseTotalValueForFreeKnowledge.Location = new System.Drawing.Point(449, 81);
            this.chkUseTotalValueForFreeKnowledge.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkUseTotalValueForFreeKnowledge.Name = "chkUseTotalValueForFreeKnowledge";
            this.chkUseTotalValueForFreeKnowledge.Size = new System.Drawing.Size(396, 17);
            this.chkUseTotalValueForFreeKnowledge.TabIndex = 24;
            this.chkUseTotalValueForFreeKnowledge.Tag = "Checkbox_Options_UseTotalValueForFreeKnowledge";
            this.chkUseTotalValueForFreeKnowledge.Text = "Free Knowledge Points use the augmented LOG+INT values";
            this.chkUseTotalValueForFreeKnowledge.UseVisualStyleBackColor = true;
            this.chkUseTotalValueForFreeKnowledge.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkExceedNegativeQualities
            // 
            this.chkExceedNegativeQualities.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkExceedNegativeQualities.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkExceedNegativeQualities, 4);
            this.chkExceedNegativeQualities.Location = new System.Drawing.Point(449, 156);
            this.chkExceedNegativeQualities.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkExceedNegativeQualities.Name = "chkExceedNegativeQualities";
            this.chkExceedNegativeQualities.Size = new System.Drawing.Size(396, 17);
            this.chkExceedNegativeQualities.TabIndex = 17;
            this.chkExceedNegativeQualities.Tag = "Checkbox_Options_ExceedNegativeQualities";
            this.chkExceedNegativeQualities.Text = "Allow characters to exceed their Negative Quality limit";
            this.chkExceedNegativeQualities.UseVisualStyleBackColor = true;
            this.chkExceedNegativeQualities.CheckedChanged += new System.EventHandler(this.chkExceedNegativeQualities_CheckedChanged);
            // 
            // chkEnemyKarmaQualityLimit
            // 
            this.chkEnemyKarmaQualityLimit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkEnemyKarmaQualityLimit.AutoSize = true;
            this.chkEnemyKarmaQualityLimit.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.tlpHouseRules.SetColumnSpan(this.chkEnemyKarmaQualityLimit, 3);
            this.chkEnemyKarmaQualityLimit.Location = new System.Drawing.Point(3, 482);
            this.chkEnemyKarmaQualityLimit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkEnemyKarmaQualityLimit.Name = "chkEnemyKarmaQualityLimit";
            this.chkEnemyKarmaQualityLimit.Size = new System.Drawing.Size(396, 150);
            this.chkEnemyKarmaQualityLimit.TabIndex = 37;
            this.chkEnemyKarmaQualityLimit.Tag = "Checkbox_Options_EnemyKarmaQualityLimit";
            this.chkEnemyKarmaQualityLimit.Text = "Karma spent on enemies counts towards negative Quality limit in create mode";
            this.chkEnemyKarmaQualityLimit.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkEnemyKarmaQualityLimit.UseVisualStyleBackColor = true;
            this.chkEnemyKarmaQualityLimit.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkExceedPositiveQualitiesCostDoubled
            // 
            this.chkExceedPositiveQualitiesCostDoubled.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkExceedPositiveQualitiesCostDoubled.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkExceedPositiveQualitiesCostDoubled, 3);
            this.chkExceedPositiveQualitiesCostDoubled.Enabled = false;
            this.chkExceedPositiveQualitiesCostDoubled.Location = new System.Drawing.Point(471, 131);
            this.chkExceedPositiveQualitiesCostDoubled.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkExceedPositiveQualitiesCostDoubled.Name = "chkExceedPositiveQualitiesCostDoubled";
            this.chkExceedPositiveQualitiesCostDoubled.Size = new System.Drawing.Size(374, 17);
            this.chkExceedPositiveQualitiesCostDoubled.TabIndex = 16;
            this.chkExceedPositiveQualitiesCostDoubled.Tag = "Checkbox_Options_ExceedPositiveQualitiesCostDoubled";
            this.chkExceedPositiveQualitiesCostDoubled.Text = "Use Career costs for all Positive Quality karma costs in excess of the limit";
            this.chkExceedPositiveQualitiesCostDoubled.UseVisualStyleBackColor = true;
            this.chkExceedPositiveQualitiesCostDoubled.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkExceedPositiveQualities
            // 
            this.chkExceedPositiveQualities.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkExceedPositiveQualities.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkExceedPositiveQualities, 4);
            this.chkExceedPositiveQualities.Location = new System.Drawing.Point(449, 106);
            this.chkExceedPositiveQualities.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkExceedPositiveQualities.Name = "chkExceedPositiveQualities";
            this.chkExceedPositiveQualities.Size = new System.Drawing.Size(396, 17);
            this.chkExceedPositiveQualities.TabIndex = 15;
            this.chkExceedPositiveQualities.Tag = "Checkbox_Options_ExceedPositiveQualities";
            this.chkExceedPositiveQualities.Text = "Allow characters to exceed their Positive Quality limit";
            this.chkExceedPositiveQualities.UseVisualStyleBackColor = true;
            this.chkExceedPositiveQualities.CheckedChanged += new System.EventHandler(this.chkExceedPositiveQualities_CheckedChanged);
            // 
            // chkUnarmedSkillImprovements
            // 
            this.chkUnarmedSkillImprovements.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkUnarmedSkillImprovements.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkUnarmedSkillImprovements, 3);
            this.chkUnarmedSkillImprovements.Location = new System.Drawing.Point(3, 30);
            this.chkUnarmedSkillImprovements.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkUnarmedSkillImprovements.Name = "chkUnarmedSkillImprovements";
            this.chkUnarmedSkillImprovements.Size = new System.Drawing.Size(396, 17);
            this.chkUnarmedSkillImprovements.TabIndex = 0;
            this.chkUnarmedSkillImprovements.Tag = "Checkbox_Options_UnarmedSkillImprovements";
            this.chkUnarmedSkillImprovements.Text = "Unarmed Combat-based Weapons Benefit from Unarmed Attack Bonuses";
            this.chkUnarmedSkillImprovements.UseVisualStyleBackColor = true;
            this.chkUnarmedSkillImprovements.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkCompensateSkillGroupKarmaDifference
            // 
            this.chkCompensateSkillGroupKarmaDifference.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkCompensateSkillGroupKarmaDifference.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkCompensateSkillGroupKarmaDifference, 3);
            this.chkCompensateSkillGroupKarmaDifference.Location = new System.Drawing.Point(3, 457);
            this.chkCompensateSkillGroupKarmaDifference.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkCompensateSkillGroupKarmaDifference.Name = "chkCompensateSkillGroupKarmaDifference";
            this.chkCompensateSkillGroupKarmaDifference.Size = new System.Drawing.Size(396, 17);
            this.chkCompensateSkillGroupKarmaDifference.TabIndex = 36;
            this.chkCompensateSkillGroupKarmaDifference.Tag = "Checkbox_Options_CompensateSkillGroupKarmaDifference";
            this.chkCompensateSkillGroupKarmaDifference.Text = "Compensate for higher karma costs when raising the rating of the last skill in a " +
    "skill group";
            this.chkCompensateSkillGroupKarmaDifference.UseVisualStyleBackColor = true;
            this.chkCompensateSkillGroupKarmaDifference.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkCyberlegMovement
            // 
            this.chkCyberlegMovement.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkCyberlegMovement.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkCyberlegMovement, 3);
            this.chkCyberlegMovement.Location = new System.Drawing.Point(3, 55);
            this.chkCyberlegMovement.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkCyberlegMovement.Name = "chkCyberlegMovement";
            this.chkCyberlegMovement.Size = new System.Drawing.Size(396, 18);
            this.chkCyberlegMovement.TabIndex = 2;
            this.chkCyberlegMovement.Tag = "Checkbox_Options_CyberlegMovement";
            this.chkCyberlegMovement.Text = "Use Cyberleg Stats for Movement";
            this.chkCyberlegMovement.UseVisualStyleBackColor = true;
            this.chkCyberlegMovement.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkMysAdeptSecondMAGAttribute
            // 
            this.chkMysAdeptSecondMAGAttribute.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkMysAdeptSecondMAGAttribute.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkMysAdeptSecondMAGAttribute, 3);
            this.chkMysAdeptSecondMAGAttribute.Location = new System.Drawing.Point(3, 432);
            this.chkMysAdeptSecondMAGAttribute.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkMysAdeptSecondMAGAttribute.Name = "chkMysAdeptSecondMAGAttribute";
            this.chkMysAdeptSecondMAGAttribute.Size = new System.Drawing.Size(396, 17);
            this.chkMysAdeptSecondMAGAttribute.TabIndex = 35;
            this.chkMysAdeptSecondMAGAttribute.Tag = "Checkbox_Options_MysAdeptSecondMAGAttribute";
            this.chkMysAdeptSecondMAGAttribute.Text = "Mystic Adepts use second MAG attribute for Adept abilities instead of special PP " +
    "rules";
            this.chkMysAdeptSecondMAGAttribute.UseVisualStyleBackColor = true;
            this.chkMysAdeptSecondMAGAttribute.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkDontDoubleQualityPurchases
            // 
            this.chkDontDoubleQualityPurchases.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkDontDoubleQualityPurchases.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkDontDoubleQualityPurchases, 3);
            this.chkDontDoubleQualityPurchases.Location = new System.Drawing.Point(3, 81);
            this.chkDontDoubleQualityPurchases.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkDontDoubleQualityPurchases.Name = "chkDontDoubleQualityPurchases";
            this.chkDontDoubleQualityPurchases.Size = new System.Drawing.Size(396, 17);
            this.chkDontDoubleQualityPurchases.TabIndex = 5;
            this.chkDontDoubleQualityPurchases.Tag = "Checkbox_Options_DontDoubleQualityPurchases";
            this.chkDontDoubleQualityPurchases.Text = "Don\'t double the cost of purchasing Positive Qualities in Career Mode";
            this.chkDontDoubleQualityPurchases.UseVisualStyleBackColor = true;
            this.chkDontDoubleQualityPurchases.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkAllowPointBuySpecializationsOnKarmaSkills
            // 
            this.chkAllowPointBuySpecializationsOnKarmaSkills.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAllowPointBuySpecializationsOnKarmaSkills.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkAllowPointBuySpecializationsOnKarmaSkills, 3);
            this.chkAllowPointBuySpecializationsOnKarmaSkills.Location = new System.Drawing.Point(3, 407);
            this.chkAllowPointBuySpecializationsOnKarmaSkills.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkAllowPointBuySpecializationsOnKarmaSkills.Name = "chkAllowPointBuySpecializationsOnKarmaSkills";
            this.chkAllowPointBuySpecializationsOnKarmaSkills.Size = new System.Drawing.Size(396, 17);
            this.chkAllowPointBuySpecializationsOnKarmaSkills.TabIndex = 34;
            this.chkAllowPointBuySpecializationsOnKarmaSkills.Tag = "Checkbox_Options_AllowPointBuySpecializationsOnKarmaSkills";
            this.chkAllowPointBuySpecializationsOnKarmaSkills.Text = "Allow skill points to be used to buy specializations for karma-bought skills";
            this.chkAllowPointBuySpecializationsOnKarmaSkills.UseVisualStyleBackColor = true;
            this.chkAllowPointBuySpecializationsOnKarmaSkills.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkDontDoubleQualityRefunds
            // 
            this.chkDontDoubleQualityRefunds.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkDontDoubleQualityRefunds.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkDontDoubleQualityRefunds, 3);
            this.chkDontDoubleQualityRefunds.Location = new System.Drawing.Point(3, 106);
            this.chkDontDoubleQualityRefunds.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkDontDoubleQualityRefunds.Name = "chkDontDoubleQualityRefunds";
            this.chkDontDoubleQualityRefunds.Size = new System.Drawing.Size(396, 17);
            this.chkDontDoubleQualityRefunds.TabIndex = 21;
            this.chkDontDoubleQualityRefunds.Tag = "Checkbox_Options_DontDoubleNegativeQualityRefunds";
            this.chkDontDoubleQualityRefunds.Text = "Don\'t double the cost of refunding Negative Qualities in Career Mode";
            this.chkDontDoubleQualityRefunds.UseVisualStyleBackColor = true;
            this.chkDontDoubleQualityRefunds.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkReverseAttributePriorityOrder
            // 
            this.chkReverseAttributePriorityOrder.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkReverseAttributePriorityOrder.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkReverseAttributePriorityOrder, 3);
            this.chkReverseAttributePriorityOrder.Location = new System.Drawing.Point(3, 382);
            this.chkReverseAttributePriorityOrder.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkReverseAttributePriorityOrder.Name = "chkReverseAttributePriorityOrder";
            this.chkReverseAttributePriorityOrder.Size = new System.Drawing.Size(396, 17);
            this.chkReverseAttributePriorityOrder.TabIndex = 33;
            this.chkReverseAttributePriorityOrder.Tag = "Checkbox_Options_ReverseAttributePriorityOrder";
            this.chkReverseAttributePriorityOrder.Text = "Spend Karma on Attributes before Priority Points";
            this.chkReverseAttributePriorityOrder.UseVisualStyleBackColor = true;
            this.chkReverseAttributePriorityOrder.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(766, 6);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(12, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "x";
            // 
            // chkStrictSkillGroups
            // 
            this.chkStrictSkillGroups.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkStrictSkillGroups.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkStrictSkillGroups, 3);
            this.chkStrictSkillGroups.Location = new System.Drawing.Point(3, 131);
            this.chkStrictSkillGroups.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkStrictSkillGroups.Name = "chkStrictSkillGroups";
            this.chkStrictSkillGroups.Size = new System.Drawing.Size(396, 17);
            this.chkStrictSkillGroups.TabIndex = 6;
            this.chkStrictSkillGroups.Tag = "Checkbox_Options_StrictSkillGroups";
            this.chkStrictSkillGroups.Text = "Strict interprentation of breaking skill groups in create mode";
            this.chkStrictSkillGroups.UseVisualStyleBackColor = true;
            this.chkStrictSkillGroups.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // nudContactMultiplier
            // 
            this.nudContactMultiplier.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudContactMultiplier.Enabled = false;
            this.nudContactMultiplier.Location = new System.Drawing.Point(784, 3);
            this.nudContactMultiplier.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudContactMultiplier.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudContactMultiplier.Name = "nudContactMultiplier";
            this.nudContactMultiplier.Size = new System.Drawing.Size(61, 20);
            this.nudContactMultiplier.TabIndex = 11;
            this.nudContactMultiplier.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.nudContactMultiplier.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkPrioritySpellsAsAdeptPowers
            // 
            this.chkPrioritySpellsAsAdeptPowers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkPrioritySpellsAsAdeptPowers.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkPrioritySpellsAsAdeptPowers, 3);
            this.chkPrioritySpellsAsAdeptPowers.Location = new System.Drawing.Point(3, 357);
            this.chkPrioritySpellsAsAdeptPowers.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkPrioritySpellsAsAdeptPowers.Name = "chkPrioritySpellsAsAdeptPowers";
            this.chkPrioritySpellsAsAdeptPowers.Size = new System.Drawing.Size(396, 17);
            this.chkPrioritySpellsAsAdeptPowers.TabIndex = 31;
            this.chkPrioritySpellsAsAdeptPowers.Tag = "Checkbox_Option_PrioritySpellsAsAdeptPowers";
            this.chkPrioritySpellsAsAdeptPowers.Text = "Allow spending of free spells from Magic Priority as power points";
            this.chkPrioritySpellsAsAdeptPowers.UseVisualStyleBackColor = true;
            this.chkPrioritySpellsAsAdeptPowers.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkAllowInitiation
            // 
            this.chkAllowInitiation.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAllowInitiation.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkAllowInitiation, 3);
            this.chkAllowInitiation.Location = new System.Drawing.Point(3, 156);
            this.chkAllowInitiation.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkAllowInitiation.Name = "chkAllowInitiation";
            this.chkAllowInitiation.Size = new System.Drawing.Size(396, 17);
            this.chkAllowInitiation.TabIndex = 7;
            this.chkAllowInitiation.Tag = "Checkbox_Options_AllowInitiation";
            this.chkAllowInitiation.Text = "Allow Initiation/Submersion in Create mode";
            this.chkAllowInitiation.UseVisualStyleBackColor = true;
            this.chkAllowInitiation.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkFreeMartialArtSpecialization
            // 
            this.chkFreeMartialArtSpecialization.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkFreeMartialArtSpecialization.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkFreeMartialArtSpecialization, 3);
            this.chkFreeMartialArtSpecialization.Location = new System.Drawing.Point(3, 332);
            this.chkFreeMartialArtSpecialization.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkFreeMartialArtSpecialization.Name = "chkFreeMartialArtSpecialization";
            this.chkFreeMartialArtSpecialization.Size = new System.Drawing.Size(396, 17);
            this.chkFreeMartialArtSpecialization.TabIndex = 30;
            this.chkFreeMartialArtSpecialization.Tag = "Checkbox_Option_FreeMartialArtSpecialization";
            this.chkFreeMartialArtSpecialization.Text = "Allow Martial Arts to grant a free specialisation in a skill";
            this.chkFreeMartialArtSpecialization.UseVisualStyleBackColor = true;
            this.chkFreeMartialArtSpecialization.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkAllowCyberwareESSDiscounts
            // 
            this.chkAllowCyberwareESSDiscounts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAllowCyberwareESSDiscounts.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkAllowCyberwareESSDiscounts, 3);
            this.chkAllowCyberwareESSDiscounts.Location = new System.Drawing.Point(3, 181);
            this.chkAllowCyberwareESSDiscounts.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkAllowCyberwareESSDiscounts.Name = "chkAllowCyberwareESSDiscounts";
            this.chkAllowCyberwareESSDiscounts.Size = new System.Drawing.Size(396, 17);
            this.chkAllowCyberwareESSDiscounts.TabIndex = 19;
            this.chkAllowCyberwareESSDiscounts.Tag = "Checkbox_Options_AllowCyberwareESSDiscounts";
            this.chkAllowCyberwareESSDiscounts.Text = "Allow Cyber/Bioware Essence costs to be customized";
            this.chkAllowCyberwareESSDiscounts.UseVisualStyleBackColor = true;
            this.chkAllowCyberwareESSDiscounts.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkMysAdPp
            // 
            this.chkMysAdPp.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkMysAdPp.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkMysAdPp, 3);
            this.chkMysAdPp.Location = new System.Drawing.Point(3, 307);
            this.chkMysAdPp.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkMysAdPp.Name = "chkMysAdPp";
            this.chkMysAdPp.Size = new System.Drawing.Size(396, 17);
            this.chkMysAdPp.TabIndex = 29;
            this.chkMysAdPp.Tag = "Checkbox_Option_AllowMysadPowerPointCareer";
            this.chkMysAdPp.Text = "Allow Mystic Adepts to buy power points during career";
            this.chkMysAdPp.UseVisualStyleBackColor = true;
            this.chkMysAdPp.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkESSLossReducesMaximumOnly
            // 
            this.chkESSLossReducesMaximumOnly.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkESSLossReducesMaximumOnly.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkESSLossReducesMaximumOnly, 3);
            this.chkESSLossReducesMaximumOnly.Location = new System.Drawing.Point(3, 206);
            this.chkESSLossReducesMaximumOnly.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkESSLossReducesMaximumOnly.Name = "chkESSLossReducesMaximumOnly";
            this.chkESSLossReducesMaximumOnly.Size = new System.Drawing.Size(396, 17);
            this.chkESSLossReducesMaximumOnly.TabIndex = 20;
            this.chkESSLossReducesMaximumOnly.Tag = "Checkbox_Options_EssenceLossReducesMaximum";
            this.chkESSLossReducesMaximumOnly.Text = "Essence Loss only Reduces Maximum Essence";
            this.chkESSLossReducesMaximumOnly.UseVisualStyleBackColor = true;
            this.chkESSLossReducesMaximumOnly.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkAlternateMetatypeAttributeKarma
            // 
            this.chkAlternateMetatypeAttributeKarma.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAlternateMetatypeAttributeKarma.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkAlternateMetatypeAttributeKarma, 3);
            this.chkAlternateMetatypeAttributeKarma.Location = new System.Drawing.Point(3, 282);
            this.chkAlternateMetatypeAttributeKarma.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkAlternateMetatypeAttributeKarma.Name = "chkAlternateMetatypeAttributeKarma";
            this.chkAlternateMetatypeAttributeKarma.Size = new System.Drawing.Size(396, 17);
            this.chkAlternateMetatypeAttributeKarma.TabIndex = 28;
            this.chkAlternateMetatypeAttributeKarma.Tag = "Checkbox_Option_AlternateMetatypeAttributeKarma";
            this.chkAlternateMetatypeAttributeKarma.Text = "Treat Metatype Attribute Minimum as 1 for the purpose of determining Karma costs";
            this.chkAlternateMetatypeAttributeKarma.UseVisualStyleBackColor = true;
            this.chkAlternateMetatypeAttributeKarma.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkUseCalculatedPublicAwareness
            // 
            this.chkUseCalculatedPublicAwareness.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkUseCalculatedPublicAwareness.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkUseCalculatedPublicAwareness, 3);
            this.chkUseCalculatedPublicAwareness.Location = new System.Drawing.Point(3, 231);
            this.chkUseCalculatedPublicAwareness.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkUseCalculatedPublicAwareness.Name = "chkUseCalculatedPublicAwareness";
            this.chkUseCalculatedPublicAwareness.Size = new System.Drawing.Size(396, 17);
            this.chkUseCalculatedPublicAwareness.TabIndex = 22;
            this.chkUseCalculatedPublicAwareness.Tag = "Checkbox_Options_UseCalculatedPublicAwareness";
            this.chkUseCalculatedPublicAwareness.Text = "Public Awareness should be (Street Cred + Notoriety /3)";
            this.chkUseCalculatedPublicAwareness.UseVisualStyleBackColor = true;
            this.chkUseCalculatedPublicAwareness.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // nudDroneArmorMultiplier
            // 
            this.nudDroneArmorMultiplier.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudDroneArmorMultiplier.Enabled = false;
            this.nudDroneArmorMultiplier.Location = new System.Drawing.Point(338, 255);
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
            this.nudDroneArmorMultiplier.Size = new System.Drawing.Size(61, 20);
            this.nudDroneArmorMultiplier.TabIndex = 26;
            this.nudDroneArmorMultiplier.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nudDroneArmorMultiplier.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(320, 258);
            this.label4.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(12, 13);
            this.label4.TabIndex = 27;
            this.label4.Text = "x";
            // 
            // chkDroneArmorMultiplier
            // 
            this.chkDroneArmorMultiplier.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkDroneArmorMultiplier.AutoSize = true;
            this.chkDroneArmorMultiplier.Location = new System.Drawing.Point(3, 256);
            this.chkDroneArmorMultiplier.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkDroneArmorMultiplier.Name = "chkDroneArmorMultiplier";
            this.chkDroneArmorMultiplier.Size = new System.Drawing.Size(285, 18);
            this.chkDroneArmorMultiplier.TabIndex = 25;
            this.chkDroneArmorMultiplier.Tag = "Checkbox_Options_DroneArmorMultiplier";
            this.chkDroneArmorMultiplier.Text = "Limit Drone Armor Enhance ment to Drone Body";
            this.chkDroneArmorMultiplier.UseVisualStyleBackColor = true;
            this.chkDroneArmorMultiplier.CheckedChanged += new System.EventHandler(this.chkDroneArmorMultiplier_CheckedChanged);
            // 
            // chkContactMultiplier
            // 
            this.chkContactMultiplier.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkContactMultiplier.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkContactMultiplier, 2);
            this.chkContactMultiplier.Location = new System.Drawing.Point(449, 4);
            this.chkContactMultiplier.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkContactMultiplier.Name = "chkContactMultiplier";
            this.chkContactMultiplier.Size = new System.Drawing.Size(307, 18);
            this.chkContactMultiplier.TabIndex = 9;
            this.chkContactMultiplier.Tag = "Checkbox_Options_ContactMultiplier";
            this.chkContactMultiplier.Text = "Override Contact Points Charisma Multiplier";
            this.chkContactMultiplier.UseVisualStyleBackColor = true;
            this.chkContactMultiplier.CheckedChanged += new System.EventHandler(this.chkContactMultiplier_CheckedChanged);
            // 
            // chkKnowledgeMultiplier
            // 
            this.chkKnowledgeMultiplier.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkKnowledgeMultiplier.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkKnowledgeMultiplier, 2);
            this.chkKnowledgeMultiplier.Location = new System.Drawing.Point(449, 55);
            this.chkKnowledgeMultiplier.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkKnowledgeMultiplier.Name = "chkKnowledgeMultiplier";
            this.chkKnowledgeMultiplier.Size = new System.Drawing.Size(307, 18);
            this.chkKnowledgeMultiplier.TabIndex = 12;
            this.chkKnowledgeMultiplier.Tag = "Checkbox_Options_KnowledgeMultiplier";
            this.chkKnowledgeMultiplier.Text = "Override Knowledge Points (INT + LOG) Multiplier";
            this.chkKnowledgeMultiplier.UseVisualStyleBackColor = true;
            this.chkKnowledgeMultiplier.CheckedChanged += new System.EventHandler(this.chkKnowledgeMultiplier_CheckedChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(766, 57);
            this.label3.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(12, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "x";
            // 
            // nudKnowledgeMultiplier
            // 
            this.nudKnowledgeMultiplier.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKnowledgeMultiplier.Enabled = false;
            this.nudKnowledgeMultiplier.Location = new System.Drawing.Point(784, 54);
            this.nudKnowledgeMultiplier.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudKnowledgeMultiplier.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudKnowledgeMultiplier.Name = "nudKnowledgeMultiplier";
            this.nudKnowledgeMultiplier.Size = new System.Drawing.Size(61, 20);
            this.nudKnowledgeMultiplier.TabIndex = 14;
            this.nudKnowledgeMultiplier.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nudKnowledgeMultiplier.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkUseTotalValueForFreeContacts
            // 
            this.chkUseTotalValueForFreeContacts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkUseTotalValueForFreeContacts.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkUseTotalValueForFreeContacts, 4);
            this.chkUseTotalValueForFreeContacts.Location = new System.Drawing.Point(449, 30);
            this.chkUseTotalValueForFreeContacts.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkUseTotalValueForFreeContacts.Name = "chkUseTotalValueForFreeContacts";
            this.chkUseTotalValueForFreeContacts.Size = new System.Drawing.Size(396, 17);
            this.chkUseTotalValueForFreeContacts.TabIndex = 23;
            this.chkUseTotalValueForFreeContacts.Tag = "Checkbox_Options_UseTotalValueForFreeContacts";
            this.chkUseTotalValueForFreeContacts.Text = "Free Contacts use the augmented Charisma value";
            this.chkUseTotalValueForFreeContacts.UseVisualStyleBackColor = true;
            this.chkUseTotalValueForFreeContacts.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkAllowSkillRegrouping
            // 
            this.chkAllowSkillRegrouping.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAllowSkillRegrouping.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkAllowSkillRegrouping, 4);
            this.chkAllowSkillRegrouping.Location = new System.Drawing.Point(449, 231);
            this.chkAllowSkillRegrouping.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkAllowSkillRegrouping.Name = "chkAllowSkillRegrouping";
            this.chkAllowSkillRegrouping.Size = new System.Drawing.Size(396, 17);
            this.chkAllowSkillRegrouping.TabIndex = 39;
            this.chkAllowSkillRegrouping.Tag = "Checkbox_Options_SkillRegroup";
            this.chkAllowSkillRegrouping.Text = "Allow Skills to be re-Grouped if all Ratings are the same";
            this.chkAllowSkillRegrouping.UseVisualStyleBackColor = true;
            this.chkAllowSkillRegrouping.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkExtendAnyDetectionSpell
            // 
            this.chkExtendAnyDetectionSpell.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkExtendAnyDetectionSpell.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkExtendAnyDetectionSpell, 4);
            this.chkExtendAnyDetectionSpell.Location = new System.Drawing.Point(449, 256);
            this.chkExtendAnyDetectionSpell.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkExtendAnyDetectionSpell.Name = "chkExtendAnyDetectionSpell";
            this.chkExtendAnyDetectionSpell.Size = new System.Drawing.Size(396, 18);
            this.chkExtendAnyDetectionSpell.TabIndex = 40;
            this.chkExtendAnyDetectionSpell.Tag = "Checkbox_Options_ExtendAnyDetectionSpell";
            this.chkExtendAnyDetectionSpell.Text = "Allow any Detection Spell to be taken as Extended range version";
            this.chkExtendAnyDetectionSpell.UseVisualStyleBackColor = true;
            this.chkExtendAnyDetectionSpell.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkMoreLethalGameplay
            // 
            this.chkMoreLethalGameplay.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkMoreLethalGameplay.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkMoreLethalGameplay, 4);
            this.chkMoreLethalGameplay.Location = new System.Drawing.Point(449, 282);
            this.chkMoreLethalGameplay.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkMoreLethalGameplay.Name = "chkMoreLethalGameplay";
            this.chkMoreLethalGameplay.Size = new System.Drawing.Size(396, 17);
            this.chkMoreLethalGameplay.TabIndex = 41;
            this.chkMoreLethalGameplay.Tag = "Checkbox_Options_MoreLethalGameplace";
            this.chkMoreLethalGameplay.Text = "Use 4th Edition Rules for More Lethal Gameplay (SR4 75)";
            this.chkMoreLethalGameplay.UseVisualStyleBackColor = true;
            this.chkMoreLethalGameplay.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkSpecialKarmaCost
            // 
            this.chkSpecialKarmaCost.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkSpecialKarmaCost.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkSpecialKarmaCost, 4);
            this.chkSpecialKarmaCost.Location = new System.Drawing.Point(449, 307);
            this.chkSpecialKarmaCost.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkSpecialKarmaCost.Name = "chkSpecialKarmaCost";
            this.chkSpecialKarmaCost.Size = new System.Drawing.Size(396, 17);
            this.chkSpecialKarmaCost.TabIndex = 42;
            this.chkSpecialKarmaCost.Tag = "Checkbox_Options_SpecialKarmaCost";
            this.chkSpecialKarmaCost.Text = "Karma cost for increasing Special Attributes is reduced with Essence Loss";
            this.chkSpecialKarmaCost.UseVisualStyleBackColor = true;
            this.chkSpecialKarmaCost.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkIgnoreComplexFormLimit
            // 
            this.chkIgnoreComplexFormLimit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkIgnoreComplexFormLimit.AutoSize = true;
            this.tlpHouseRules.SetColumnSpan(this.chkIgnoreComplexFormLimit, 4);
            this.chkIgnoreComplexFormLimit.Location = new System.Drawing.Point(449, 332);
            this.chkIgnoreComplexFormLimit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkIgnoreComplexFormLimit.Name = "chkIgnoreComplexFormLimit";
            this.chkIgnoreComplexFormLimit.Size = new System.Drawing.Size(396, 17);
            this.chkIgnoreComplexFormLimit.TabIndex = 43;
            this.chkIgnoreComplexFormLimit.Tag = "Checkbox_Options_IgnoreComplexFormLimit";
            this.chkIgnoreComplexFormLimit.Text = "Ignore complex form limit in Career mode";
            this.chkIgnoreComplexFormLimit.UseVisualStyleBackColor = true;
            // 
            // tabGitHubIssues
            // 
            this.tabGitHubIssues.BackColor = System.Drawing.SystemColors.Control;
            this.tabGitHubIssues.Controls.Add(this.cmdUploadPastebin);
            this.tabGitHubIssues.Location = new System.Drawing.Point(4, 22);
            this.tabGitHubIssues.Name = "tabGitHubIssues";
            this.tabGitHubIssues.Padding = new System.Windows.Forms.Padding(3);
            this.tabGitHubIssues.Size = new System.Drawing.Size(916, 654);
            this.tabGitHubIssues.TabIndex = 4;
            this.tabGitHubIssues.Tag = "Tab_Options_GitHubIssues";
            this.tabGitHubIssues.Text = "GitHub Issues";
            // 
            // cmdUploadPastebin
            // 
            this.cmdUploadPastebin.AutoSize = true;
            this.cmdUploadPastebin.Enabled = false;
            this.cmdUploadPastebin.Location = new System.Drawing.Point(6, 6);
            this.cmdUploadPastebin.Name = "cmdUploadPastebin";
            this.cmdUploadPastebin.Size = new System.Drawing.Size(178, 30);
            this.cmdUploadPastebin.TabIndex = 1;
            this.cmdUploadPastebin.Tag = "Button_Options_UploadPastebin";
            this.cmdUploadPastebin.Text = "Upload file to Pastebin";
            this.cmdUploadPastebin.UseVisualStyleBackColor = true;
            this.cmdUploadPastebin.Click += new System.EventHandler(this.cmdUploadPastebin_Click);
            // 
            // tabPlugins
            // 
            this.tabPlugins.BackColor = System.Drawing.SystemColors.Control;
            this.tabPlugins.Controls.Add(this.bufferedTableLayoutPanel1);
            this.tabPlugins.Location = new System.Drawing.Point(4, 22);
            this.tabPlugins.Name = "tabPlugins";
            this.tabPlugins.Padding = new System.Windows.Forms.Padding(3);
            this.tabPlugins.Size = new System.Drawing.Size(916, 654);
            this.tabPlugins.TabIndex = 6;
            this.tabPlugins.Tag = "Tab_Options_Plugins";
            this.tabPlugins.Text = "Plugins";
            // 
            // bufferedTableLayoutPanel1
            // 
            this.bufferedTableLayoutPanel1.AutoSize = true;
            this.bufferedTableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bufferedTableLayoutPanel1.ColumnCount = 2;
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 85F));
            this.bufferedTableLayoutPanel1.Controls.Add(this.grpAvailablePlugins, 0, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.panelPluginOption, 1, 0);
            this.bufferedTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedTableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.bufferedTableLayoutPanel1.MinimumSize = new System.Drawing.Size(823, 516);
            this.bufferedTableLayoutPanel1.Name = "bufferedTableLayoutPanel1";
            this.bufferedTableLayoutPanel1.RowCount = 2;
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.bufferedTableLayoutPanel1.Size = new System.Drawing.Size(910, 648);
            this.bufferedTableLayoutPanel1.TabIndex = 0;
            // 
            // grpAvailablePlugins
            // 
            this.grpAvailablePlugins.Controls.Add(this.clbPlugins);
            this.grpAvailablePlugins.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpAvailablePlugins.Location = new System.Drawing.Point(3, 3);
            this.grpAvailablePlugins.Name = "grpAvailablePlugins";
            this.grpAvailablePlugins.Size = new System.Drawing.Size(130, 622);
            this.grpAvailablePlugins.TabIndex = 0;
            this.grpAvailablePlugins.TabStop = false;
            this.grpAvailablePlugins.Tag = "String_AvailablePlugins";
            this.grpAvailablePlugins.Text = "Available Plugins";
            // 
            // clbPlugins
            // 
            this.clbPlugins.Dock = System.Windows.Forms.DockStyle.Fill;
            this.clbPlugins.FormattingEnabled = true;
            this.clbPlugins.Location = new System.Drawing.Point(3, 16);
            this.clbPlugins.Name = "clbPlugins";
            this.clbPlugins.Size = new System.Drawing.Size(124, 603);
            this.clbPlugins.TabIndex = 0;
            this.clbPlugins.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clbPlugins_ItemCheck);
            this.clbPlugins.SelectedValueChanged += new System.EventHandler(this.clbPlugins_SelectedValueChanged);
            this.clbPlugins.VisibleChanged += new System.EventHandler(this.clbPlugins_VisibleChanged);
            // 
            // panelPluginOption
            // 
            this.panelPluginOption.AutoSize = true;
            this.panelPluginOption.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelPluginOption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPluginOption.Location = new System.Drawing.Point(139, 3);
            this.panelPluginOption.Name = "panelPluginOption";
            this.panelPluginOption.Size = new System.Drawing.Size(768, 622);
            this.panelPluginOption.TabIndex = 1;
            // 
            // flpOKCancel
            // 
            this.flpOKCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flpOKCancel.AutoSize = true;
            this.tlpOptions.SetColumnSpan(this.flpOKCancel, 4);
            this.flpOKCancel.Controls.Add(this.cmdOK);
            this.flpOKCancel.Controls.Add(this.cmdCancel);
            this.flpOKCancel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpOKCancel.Location = new System.Drawing.Point(761, 716);
            this.flpOKCancel.Name = "flpOKCancel";
            this.flpOKCancel.Size = new System.Drawing.Size(166, 30);
            this.flpOKCancel.TabIndex = 5;
            // 
            // frmOptions
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(948, 767);
            this.Controls.Add(this.tlpOptions);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(20, 661);
            this.Name = "frmOptions";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_Options";
            this.Text = "Options";
            this.Load += new System.EventHandler(this.frmOptions_Load);
            this.tlpOptions.ResumeLayout(false);
            this.tlpOptions.PerformLayout();
            this.tabOptions.ResumeLayout(false);
            this.tabGlobal.ResumeLayout(false);
            this.tabGlobal.PerformLayout();
            this.tlpGlobal.ResumeLayout(false);
            this.tlpGlobal.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgSheetLanguageFlag)).EndInit();
            this.grpSelectedSourcebook.ResumeLayout(false);
            this.grpSelectedSourcebook.PerformLayout();
            this.tlpSelectedSourcebook.ResumeLayout(false);
            this.tlpSelectedSourcebook.PerformLayout();
            this.flpPDFOffset.ResumeLayout(false);
            this.flpPDFOffset.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPDFOffset)).EndInit();
            this.grpCharacterDefaults.ResumeLayout(false);
            this.grpCharacterDefaults.PerformLayout();
            this.tableLayoutPanel7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.imgLanguageFlag)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBrowserVersion)).EndInit();
            this.tabCharacterOptions.ResumeLayout(false);
            this.tabCharacterOptions.PerformLayout();
            this.tlpCharacterOptions.ResumeLayout(false);
            this.tlpCharacterOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudNuyenDecimalsMinimum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNuyenDecimalsMaximum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEssenceDecimals)).EndInit();
            this.tabKarmaCosts.ResumeLayout(false);
            this.tlpKarmaCosts.ResumeLayout(false);
            this.tlpKarmaCosts.PerformLayout();
            this.tlpKarmaCostsList.ResumeLayout(false);
            this.tlpKarmaCostsList.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaMysticAdeptPowerPoint)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpecialization)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewAIAdvancedProgram)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaInitiationFlat)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaWeaponFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewAIProgram)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaKnowledgeSpecialization)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaRitualSpellcastingFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewKnowledgeSkill)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaInitiation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaFlexibleSignatureFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpellShapingFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaMetamagic)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaCarryover)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaEnemy)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaContact)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSustainingFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaJoinGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaLeaveGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSummoningFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaManeuver)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewActiveSkill)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaComplexFormSkillsoft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpellcastingFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNuyenPer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpirit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaAlchemicalFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaComplexFormOption)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewSkillGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaBanishingFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaImproveKnowledgeSkill)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaQiFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaBindingFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaPowerFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaImproveActiveSkill)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaMaskingFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaCenteringFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaCounterspellingFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaImproveSkillGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaDisenchantingFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaAttribute)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaQuality)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaImproveComplexForm)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpell)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewComplexForm)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMetatypeCostsKarmaMultiplier)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNuyenPerBP)).EndInit();
            this.tabOptionalRules.ResumeLayout(false);
            this.tabOptionalRules.PerformLayout();
            this.tlpOptionalRules.ResumeLayout(false);
            this.tlpOptionalRules.PerformLayout();
            this.tabHouseRules.ResumeLayout(false);
            this.tabHouseRules.PerformLayout();
            this.tlpHouseRules.ResumeLayout(false);
            this.tlpHouseRules.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudContactMultiplier)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDroneArmorMultiplier)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKnowledgeMultiplier)).EndInit();
            this.tabGitHubIssues.ResumeLayout(false);
            this.tabGitHubIssues.PerformLayout();
            this.tabPlugins.ResumeLayout(false);
            this.tabPlugins.PerformLayout();
            this.bufferedTableLayoutPanel1.ResumeLayout(false);
            this.bufferedTableLayoutPanel1.PerformLayout();
            this.grpAvailablePlugins.ResumeLayout(false);
            this.flpOKCancel.ResumeLayout(false);
            this.flpOKCancel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Label lblKarmaCarryoverExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaCarryover;
        private System.Windows.Forms.Label lblKarmaCarryover;
        private System.Windows.Forms.Label lblKarmaContactExtra;
        private System.Windows.Forms.Label lblKarmaEnemyExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaContact;
        private System.Windows.Forms.NumericUpDown nudKarmaEnemy;
        private System.Windows.Forms.Label lblKarmaContact;
        private System.Windows.Forms.Label lblKarmaEnemy;
        private System.Windows.Forms.Label lblKarmaNuyenPerExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaNuyenPer;
        private System.Windows.Forms.Label lblKarmaNuyenPer;
        private System.Windows.Forms.Label lblKarmaImproveComplexFormExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaImproveComplexForm;
        private System.Windows.Forms.Label lblKarmaImproveComplexForm;
        private System.Windows.Forms.NumericUpDown nudKarmaNewComplexForm;
        private System.Windows.Forms.Label lblKarmaNewComplexForm;
        private System.Windows.Forms.NumericUpDown nudKarmaSpell;
        private System.Windows.Forms.Label lblKarmaSpell;
        private System.Windows.Forms.Label lblKarmaQualityExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaQuality;
        private System.Windows.Forms.Label lblKarmaQuality;
        private System.Windows.Forms.Label lblKarmaAttributeExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaAttribute;
        private System.Windows.Forms.Label lblKarmaAttribute;
        private System.Windows.Forms.Label lblKarmaImproveSkillGroupExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaImproveSkillGroup;
        private System.Windows.Forms.Label lblKarmaImproveSkillGroup;
        private System.Windows.Forms.Label lblKarmaImproveActiveSkillExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaImproveActiveSkill;
        private System.Windows.Forms.Label lblKarmaImproveActiveSkill;
        private System.Windows.Forms.Label lblKarmaImproveKnowledgeSkillExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaImproveKnowledgeSkill;
        private System.Windows.Forms.Label lblKarmaImproveKnowledgeSkill;
        private System.Windows.Forms.NumericUpDown nudKarmaNewSkillGroup;
        private System.Windows.Forms.Label lblKarmaNewSkillGroup;
        private System.Windows.Forms.NumericUpDown nudKarmaNewActiveSkill;
        private System.Windows.Forms.Label lblKarmaNewActiveSkill;
        private System.Windows.Forms.NumericUpDown nudKarmaNewKnowledgeSkill;
        private System.Windows.Forms.Label lblKarmaNewKnowledgeSkill;
        private System.Windows.Forms.NumericUpDown nudKarmaSpecialization;
        private System.Windows.Forms.Label lblKarmaSpecialization;
        private System.Windows.Forms.Label lblKarmaSpiritExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaSpirit;
        private System.Windows.Forms.Label lblKarmaSpirit;
        private System.Windows.Forms.NumericUpDown nudKarmaManeuver;
        private System.Windows.Forms.Label lblKarmaManeuver;
        private System.Windows.Forms.Label lblKarmaInitiationBracket;
        private System.Windows.Forms.Label lblKarmaInitiationExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaInitiation;
        private System.Windows.Forms.Label lblKarmaInitiation;
        private System.Windows.Forms.NumericUpDown nudKarmaMetamagic;
        private System.Windows.Forms.Label lblKarmaMetamagic;
        private System.Windows.Forms.CheckBox chkUnarmedSkillImprovements;
        private System.Windows.Forms.Label lblKarmaComplexFormOptionExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaComplexFormOption;
        private System.Windows.Forms.Label lblKarmaComplexFormOption;
        private System.Windows.Forms.TabControl tabOptions;
        private System.Windows.Forms.TabPage tabCharacterOptions;
        private System.Windows.Forms.TabPage tabKarmaCosts;
        private System.Windows.Forms.TabPage tabOptionalRules;
        private System.Windows.Forms.TabPage tabHouseRules;
        private ElasticComboBox cboSetting;
        private System.Windows.Forms.Label lblSetting;
        private System.Windows.Forms.Label lblSettingName;
        private System.Windows.Forms.TextBox txtSettingName;
        private System.Windows.Forms.NumericUpDown nudKarmaLeaveGroup;
        private System.Windows.Forms.Label lblKarmaLeaveGroup;
        private System.Windows.Forms.NumericUpDown nudKarmaJoinGroup;
        private System.Windows.Forms.Label lblKarmaJoinGroup;
        private System.Windows.Forms.Label lblKarmaComplexFormSkillsoftExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaComplexFormSkillsoft;
        private System.Windows.Forms.Label lblKarmaComplexFormSkillsoft;
        private System.Windows.Forms.Label lblKarmaAlchemicalFocusExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaAlchemicalFocus;
        private System.Windows.Forms.Label lblKarmaAlchemicalFocus;
        private System.Windows.Forms.Label lblKarmaWeaponFocusExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaWeaponFocus;
        private System.Windows.Forms.Label lblKarmaWeaponFocus;
        private System.Windows.Forms.Label lblKarmaSpellShapingFocusExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaSpellShapingFocus;
        private System.Windows.Forms.Label lblKarmaSpellShapingFocus;
        private System.Windows.Forms.Label lblKarmaSustainingFocusExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaSustainingFocus;
        private System.Windows.Forms.Label lblKarmaSustainingFocus;
        private System.Windows.Forms.Label lblKarmaSummoningFocusExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaSummoningFocus;
        private System.Windows.Forms.Label lblKarmaSummoningFocus;
        private System.Windows.Forms.Label lblKarmaSpellcastingFocusExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaSpellcastingFocus;
        private System.Windows.Forms.Label lblKarmaSpellcastingFocus;
        private System.Windows.Forms.Label lblKarmaQiFocusExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaQiFocus;
        private System.Windows.Forms.Label lblKarmaQiFocus;
        private System.Windows.Forms.Label lblKarmaPowerFocusExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaPowerFocus;
        private System.Windows.Forms.Label lblKarmaPowerFocus;
        private System.Windows.Forms.Label lblKarmaMaskingFocusExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaMaskingFocus;
        private System.Windows.Forms.Label lblKarmaMaskingFocus;
        private System.Windows.Forms.Label lblKarmaDisenchantingFocusExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaDisenchantingFocus;
        private System.Windows.Forms.Label lblKarmaDisenchantingFocus;
        private System.Windows.Forms.Label lblKarmaCounterspellingFocusExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaCounterspellingFocus;
        private System.Windows.Forms.Label lblKarmaCounterspellingFocus;
        private System.Windows.Forms.Label lblKarmaCenteringFocusExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaCenteringFocus;
        private System.Windows.Forms.Label lblKarmaCenteringFocus;
        private System.Windows.Forms.Label lblKarmaBindingFocusExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaBindingFocus;
        private System.Windows.Forms.Label lblKarmaBindingFocus;
        private System.Windows.Forms.Label lblKarmaBanishingFocusExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaBanishingFocus;
        private System.Windows.Forms.Label lblKarmaBanishingFocus;
        private System.Windows.Forms.Button cmdRestoreDefaultsKarma;
        private System.Windows.Forms.CheckBox chkIgnoreArt;
        private System.Windows.Forms.CheckBox chkCyberlegMovement;
        private System.Windows.Forms.CheckBox chkDontDoubleQualityPurchases;
        private System.Windows.Forms.CheckBox chkStrictSkillGroups;
        private System.Windows.Forms.CheckBox chkAllowInitiation;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nudKnowledgeMultiplier;
        private System.Windows.Forms.CheckBox chkKnowledgeMultiplier;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nudContactMultiplier;
        private System.Windows.Forms.CheckBox chkContactMultiplier;
        private System.Windows.Forms.CheckBox chkExceedNegativeQualitiesLimit;
        private System.Windows.Forms.CheckBox chkExceedNegativeQualities;
        private System.Windows.Forms.CheckBox chkExceedPositiveQualitiesCostDoubled;
        private System.Windows.Forms.CheckBox chkExceedPositiveQualities;
        private System.Windows.Forms.CheckBox chkAllowCyberwareESSDiscounts;
        private System.Windows.Forms.CheckBox chkESSLossReducesMaximumOnly;
        private System.Windows.Forms.CheckBox chkDontDoubleQualityRefunds;
        private System.Windows.Forms.CheckBox chkUseCalculatedPublicAwareness;
        private System.Windows.Forms.CheckBox chkUseTotalValueForFreeContacts;
        private System.Windows.Forms.CheckBox chkUseTotalValueForFreeKnowledge;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown nudDroneArmorMultiplier;
        private System.Windows.Forms.CheckBox chkDroneArmorMultiplier;
        private System.Windows.Forms.CheckBox chkAlternateMetatypeAttributeKarma;
        private System.Windows.Forms.CheckBox chkMysAdPp;
        private System.Windows.Forms.NumericUpDown nudKarmaNewAIAdvancedProgram;
        private System.Windows.Forms.NumericUpDown nudKarmaNewAIProgram;
        private System.Windows.Forms.Label lblKarmaNewAIAdvancedProgram;
        private System.Windows.Forms.Label lblKarmaNewAIProgram;
        private System.Windows.Forms.CheckBox chkFreeMartialArtSpecialization;
        private System.Windows.Forms.CheckBox chkPrioritySpellsAsAdeptPowers;
        private System.Windows.Forms.CheckBox chkReverseAttributePriorityOrder;
        private System.Windows.Forms.Label lblFlexibleSignatureFocusExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaFlexibleSignatureFocus;
        private System.Windows.Forms.Label lblKarmaFlexibleSignatureFocus;
        private System.Windows.Forms.Label lblKarmaRitualSpellcastingFocusExtra;
        private System.Windows.Forms.NumericUpDown nudKarmaRitualSpellcastingFocus;
        private System.Windows.Forms.Label lblKarmaRitualSpellcastingFocus;
        private System.Windows.Forms.TreeView treCustomDataDirectories;
        private System.Windows.Forms.Button cmdRemoveCustomDirectory;
        private System.Windows.Forms.Button cmdAddCustomDirectory;
        private System.Windows.Forms.Button cmdRenameCustomDataDirectory;
        private System.Windows.Forms.Label lblCustomDataDirectoriesLabel;
        private System.Windows.Forms.Button cmdIncreaseCustomDirectoryLoadOrder;
        private System.Windows.Forms.Button cmdDecreaseCustomDirectoryLoadOrder;
        private System.Windows.Forms.CheckBox chkAllowPointBuySpecializationsOnKarmaSkills;
        private System.Windows.Forms.CheckBox chkMysAdeptSecondMAGAttribute;
        private System.Windows.Forms.Label lblKarmaKnowledgeSpecialization;
        private System.Windows.Forms.NumericUpDown nudKarmaKnowledgeSpecialization;
        private System.Windows.Forms.NumericUpDown nudKarmaInitiationFlat;
        private System.Windows.Forms.CheckBox chkCompensateSkillGroupKarmaDifference;
        private System.Windows.Forms.CheckBox chkEnemyKarmaQualityLimit;
        private System.Windows.Forms.CheckBox chkNoArmorEncumbrance;
        private System.Windows.Forms.NumericUpDown nudKarmaMysticAdeptPowerPoint;
        private System.Windows.Forms.Label lblKarmaMysticAdeptPowerPoint;
        private Chummer.BufferedTableLayoutPanel tlpOptions;
        private System.Windows.Forms.FlowLayoutPanel flpOKCancel;
        private System.Windows.Forms.Button cmdCancel;
        private Chummer.BufferedTableLayoutPanel tlpKarmaCostsList;
        private Chummer.BufferedTableLayoutPanel tlpHouseRules;
        private System.Windows.Forms.CheckBox chkAllowSkillRegrouping;
        private System.Windows.Forms.CheckBox chkExtendAnyDetectionSpell;
        private System.Windows.Forms.CheckBox chkMoreLethalGameplay;
        private System.Windows.Forms.NumericUpDown nudMetatypeCostsKarmaMultiplier;
        private System.Windows.Forms.Label lblMetatypeCostsKarmaMultiplierLabel;
        private Chummer.BufferedTableLayoutPanel tlpOptionalRules;
        private System.Windows.Forms.CheckBox chkSpecialKarmaCost;
        private System.Windows.Forms.TabPage tabGitHubIssues;
        private System.Windows.Forms.Button cmdUploadPastebin;
        private System.Windows.Forms.Label lblNuyenPerBP;
        private System.Windows.Forms.NumericUpDown nudNuyenPerBP;
        private System.Windows.Forms.TabPage tabGlobal;
        private Chummer.BufferedTableLayoutPanel tlpGlobal;
        private System.Windows.Forms.CheckBox chkSearchInCategoryOnly;
        private System.Windows.Forms.Button cmdPDFAppPath;
        private System.Windows.Forms.Label lblLanguage;
        private System.Windows.Forms.Label lblPDFAppPath;
        private System.Windows.Forms.CheckBox chkHideCharacterRoster;
        private System.Windows.Forms.Button cmdVerify;
        private System.Windows.Forms.Button cmdVerifyData;
        private ElasticComboBox cboXSLT;
        private System.Windows.Forms.Label lblXSLT;
        private System.Windows.Forms.CheckBox chkLifeModule;
        private System.Windows.Forms.CheckBox chkLiveCustomData;
        private System.Windows.Forms.CheckBox chkUseLogging;
        private System.Windows.Forms.CheckBox chkAutomaticUpdate;
        private System.Windows.Forms.CheckBox chkOmaeEnabled;
        private System.Windows.Forms.CheckBox chkStartupFullscreen;
        private System.Windows.Forms.CheckBox chkSingleDiceRoller;
        private System.Windows.Forms.CheckBox chkDatesIncludeTime;
        private System.Windows.Forms.Label lblPDFParametersLabel;
        private ElasticComboBox cboPDFParameters;
        private System.Windows.Forms.TextBox txtPDFAppPath;
        private System.Windows.Forms.GroupBox grpSelectedSourcebook;
        private System.Windows.Forms.Label lblPDFLocation;
        private System.Windows.Forms.TextBox txtPDFLocation;
        private System.Windows.Forms.Button cmdPDFLocation;
        private System.Windows.Forms.Label lblPDFOffset;
        private System.Windows.Forms.NumericUpDown nudPDFOffset;
        private System.Windows.Forms.Button cmdPDFTest;
        private ElasticComboBox cboLanguage;
        private ElasticComboBox cboSheetLanguage;
        private System.Windows.Forms.CheckBox chkConfirmDelete;
        private System.Windows.Forms.CheckBox chkConfirmKarmaExpense;
        private System.Windows.Forms.CheckBox chkHideItemsOverAvail;
        private System.Windows.Forms.CheckBox chkAllowHoverIncrement;
        private System.Windows.Forms.CheckBox chkLiveUpdateCleanCharacterFiles;
        private System.Windows.Forms.CheckBox chkPreferNightlyBuilds;
        private System.Windows.Forms.Label lblCharacterRosterLabel;
        private System.Windows.Forms.GroupBox grpCharacterDefaults;
        private Chummer.BufferedTableLayoutPanel tableLayoutPanel7;
        private ElasticComboBox cboBuildMethod;
        private System.Windows.Forms.TextBox txtCharacterRosterPath;
        private System.Windows.Forms.Button cmdCharacterRoster;
        private System.Windows.Forms.CheckBox chkCreateBackupOnCareer;
        private System.Windows.Forms.CheckBox chkPrintToFileFirst;
        private Chummer.BufferedTableLayoutPanel tlpCharacterOptions;
        private System.Windows.Forms.TreeView treSourcebook;
        private System.Windows.Forms.CheckBox chkPrintNotes;
        private System.Windows.Forms.Label lblSourcebooksToUse;
        private System.Windows.Forms.CheckBox chkPrintExpenses;
        private System.Windows.Forms.CheckBox chkPrintSkillsWithZeroRating;
        private System.Windows.Forms.CheckBox chkDontUseCyberlimbCalculation;
        private System.Windows.Forms.CheckBox chkAllowSkillDiceRolling;
        private System.Windows.Forms.CheckBox chkEnforceCapacity;
        private System.Windows.Forms.CheckBox chkLicenseEachRestrictedItem;
        private System.Windows.Forms.Label lblEssenceDecimals;
        private System.Windows.Forms.Label lblNuyenDecimalsMaximumLabel;
        private System.Windows.Forms.Label lblNuyenDecimalsMinimumLabel;
        private System.Windows.Forms.CheckBox chkDontRoundEssenceInternally;
        private System.Windows.Forms.CheckBox chkDronemods;
        private System.Windows.Forms.CheckBox chkRestrictRecoil;
        private System.Windows.Forms.NumericUpDown nudNuyenDecimalsMinimum;
        private System.Windows.Forms.NumericUpDown nudNuyenDecimalsMaximum;
        private System.Windows.Forms.NumericUpDown nudEssenceDecimals;
        private System.Windows.Forms.CheckBox chkDronemodsMaximumPilot;
        private System.Windows.Forms.CheckBox chkPrintFreeExpenses;
        private System.Windows.Forms.Label lblLimbCount;
        private ElasticComboBox cboLimbCount;
        private System.Windows.Forms.Button cmdEnableSourcebooks;
        private System.Windows.Forms.Label lblEditSourcebookInfo;
        private System.Windows.Forms.ListBox lstGlobalSourcebookInfos;
        private System.Windows.Forms.ComboBox cboDefaultGameplayOption;
        private System.Windows.Forms.PictureBox imgLanguageFlag;
        private System.Windows.Forms.PictureBox imgSheetLanguageFlag;
        private Chummer.BufferedTableLayoutPanel tlpSelectedSourcebook;
        private System.Windows.Forms.FlowLayoutPanel flpPDFOffset;
        private System.Windows.Forms.TableLayoutPanel tlpKarmaCosts;
        private System.Windows.Forms.CheckBox chkEnablePlugins;
        private System.Windows.Forms.TabPage tabPlugins;
        private BufferedTableLayoutPanel bufferedTableLayoutPanel1;
        private System.Windows.Forms.GroupBox grpAvailablePlugins;
        private System.Windows.Forms.CheckedListBox clbPlugins;
        private System.Windows.Forms.Panel panelPluginOption;
        private System.Windows.Forms.CheckBox chkIgnoreComplexFormLimit;
        private System.Windows.Forms.CheckBox chkAllowEasterEggs;
        private System.Windows.Forms.NumericUpDown nudBrowserVersion;
        private System.Windows.Forms.Label lblBrowserVersion;
        private System.Windows.Forms.CheckBox chkUseLoggingApplicationInsights;
    }
}
