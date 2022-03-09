namespace Chummer
{
    partial class EditCharacterSettings
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
                if (_objCharacterSettings != null)
                {
                    _objCharacterSettings.PropertyChanged -= SettingsChanged;
                    _objCharacterSettings.Dispose();
                }
                Utils.ListItemListPool.Return(_lstSettings);
                Utils.StringHashSetPool.Return(_setPermanentSourcebooks);
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditCharacterSettings));
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
            this.lblMaxAvail = new System.Windows.Forms.Label();
            this.flpAllowedCyberwareGrades = new System.Windows.Forms.FlowLayoutPanel();
            this.lblKnowledgePoints = new Chummer.LabelWithToolTip();
            this.txtKnowledgePoints = new System.Windows.Forms.TextBox();
            this.lblStartingKarma = new System.Windows.Forms.Label();
            this.nudStartingKarma = new Chummer.NumericUpDownEx();
            this.lblPriorities = new Chummer.LabelWithToolTip();
            this.txtPriorities = new System.Windows.Forms.TextBox();
            this.nudSumToTen = new Chummer.NumericUpDownEx();
            this.lblSumToTen = new System.Windows.Forms.Label();
            this.lblMaxNuyenKarma = new System.Windows.Forms.Label();
            this.lblQualityKarmaLimit = new System.Windows.Forms.Label();
            this.nudMaxNuyenKarma = new Chummer.NumericUpDownEx();
            this.nudQualityKarmaLimit = new Chummer.NumericUpDownEx();
            this.lblNuyenExpression = new Chummer.LabelWithToolTip();
            this.txtNuyenExpression = new System.Windows.Forms.TextBox();
            this.lblContactPoints = new Chummer.LabelWithToolTip();
            this.txtContactPoints = new System.Windows.Forms.TextBox();
            this.lblMaxNumberMaxAttributes = new Chummer.LabelWithToolTip();
            this.nudMaxNumberMaxAttributes = new Chummer.NumericUpDownEx();
            this.lblMaxSkillRatingCreate = new Chummer.LabelWithToolTip();
            this.nudMaxSkillRatingCreate = new Chummer.NumericUpDownEx();
            this.lblMaxKnowledgeSkillRatingCreate = new Chummer.LabelWithToolTip();
            this.nudMaxKnowledgeSkillRatingCreate = new Chummer.NumericUpDownEx();
            this.gpbBasicOptionsOfficialRules = new System.Windows.Forms.GroupBox();
            this.tlpBasicOptionsOfficialRules = new Chummer.BufferedTableLayoutPanel(this.components);
            this.chkAllowFreeGrids = new Chummer.ColorableCheckBox(this.components);
            this.chkAllowPointBuySpecializationsOnKarmaSkills = new Chummer.ColorableCheckBox(this.components);
            this.chkStrictSkillGroups = new Chummer.ColorableCheckBox(this.components);
            this.chkEnforceCapacity = new Chummer.ColorableCheckBox(this.components);
            this.chkLicenseEachRestrictedItem = new Chummer.ColorableCheckBox(this.components);
            this.chkRestrictRecoil = new Chummer.ColorableCheckBox(this.components);
            this.chkDronemodsMaximumPilot = new Chummer.ColorableCheckBox(this.components);
            this.chkDronemods = new Chummer.ColorableCheckBox(this.components);
            this.gpbBasicOptionsEncumbrance = new System.Windows.Forms.GroupBox();
            this.tlpBasicOptionsEncumbrance = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblWeightDecimalPlaces = new Chummer.LabelWithToolTip();
            this.lblCarryLimit = new Chummer.LabelWithToolTip();
            this.lblLiftLimit = new Chummer.LabelWithToolTip();
            this.txtLiftLimit = new System.Windows.Forms.TextBox();
            this.txtCarryLimit = new System.Windows.Forms.TextBox();
            this.lblEncumbrancePenaltiesHeader = new System.Windows.Forms.Label();
            this.tlpEncumbranceInterval = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblEncumbranceIntervalLeft = new Chummer.LabelWithToolTip();
            this.txtEncumbranceInterval = new System.Windows.Forms.TextBox();
            this.lblEncumbranceIntervalRight = new Chummer.LabelWithToolTip();
            this.chkEncumbrancePenaltyPhysicalLimit = new Chummer.ColorableCheckBox(this.components);
            this.chkEncumbrancePenaltyMovementSpeed = new Chummer.ColorableCheckBox(this.components);
            this.chkEncumbrancePenaltyAgility = new Chummer.ColorableCheckBox(this.components);
            this.chkEncumbrancePenaltyReaction = new Chummer.ColorableCheckBox(this.components);
            this.chkEncumbrancePenaltyWoundModifier = new Chummer.ColorableCheckBox(this.components);
            this.nudEncumbrancePenaltyPhysicalLimit = new Chummer.NumericUpDownEx();
            this.nudEncumbrancePenaltyAgility = new Chummer.NumericUpDownEx();
            this.nudEncumbrancePenaltyReaction = new Chummer.NumericUpDownEx();
            this.nudEncumbrancePenaltyWoundModifier = new Chummer.NumericUpDownEx();
            this.nudEncumbrancePenaltyMovementSpeed = new Chummer.NumericUpDownEx();
            this.lblEncumbrancePenaltyMovementSpeedPercent = new Chummer.LabelWithToolTip();
            this.nudWeightDecimals = new Chummer.NumericUpDownEx();
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
            this.gpbBasicOptionsInitiativeDice = new System.Windows.Forms.GroupBox();
            this.tlpBasicOptionsInitiativeDice = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblMinInitiativeDiceLabel = new System.Windows.Forms.Label();
            this.lblHotSimInitiativeDiceLabel = new System.Windows.Forms.Label();
            this.lblAstralInitiativeDiceLabel = new System.Windows.Forms.Label();
            this.lblColdSimInitiativeDiceLabel = new System.Windows.Forms.Label();
            this.nudMinInitiativeDice = new Chummer.NumericUpDownEx();
            this.nudMaxInitiativeDice = new Chummer.NumericUpDownEx();
            this.lblInitiativeDiceLabel = new System.Windows.Forms.Label();
            this.lblMaxInitiativeDiceLabel = new System.Windows.Forms.Label();
            this.nudMaxAstralInitiativeDice = new Chummer.NumericUpDownEx();
            this.nudMaxColdSimInitiativeDice = new Chummer.NumericUpDownEx();
            this.nudMaxHotSimInitiativeDice = new Chummer.NumericUpDownEx();
            this.nudMinAstralInitiativeDice = new Chummer.NumericUpDownEx();
            this.nudMinColdSimInitiativeDice = new Chummer.NumericUpDownEx();
            this.nudMinHotSimInitiativeDice = new Chummer.NumericUpDownEx();
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
            this.lblKarmaSpirit = new System.Windows.Forms.Label();
            this.nudKarmaSpirit = new Chummer.NumericUpDownEx();
            this.lblKarmaSpiritExtra = new System.Windows.Forms.Label();
            this.lblKarmaJoinGroup = new System.Windows.Forms.Label();
            this.nudKarmaJoinGroup = new Chummer.NumericUpDownEx();
            this.lblKarmaCarryover = new System.Windows.Forms.Label();
            this.nudKarmaCarryover = new Chummer.NumericUpDownEx();
            this.lblKarmaCarryoverExtra = new System.Windows.Forms.Label();
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
            this.lblKarmaTechnique = new System.Windows.Forms.Label();
            this.lblKarmaMetamagic = new System.Windows.Forms.Label();
            this.flpKarmaInitiation = new System.Windows.Forms.FlowLayoutPanel();
            this.lblKarmaInitiation = new System.Windows.Forms.Label();
            this.lblKarmaInitiationBracket = new System.Windows.Forms.Label();
            this.lblKarmaNewAIAdvancedProgram = new System.Windows.Forms.Label();
            this.lblKarmaNewAIProgram = new System.Windows.Forms.Label();
            this.lblKarmaNewComplexForm = new System.Windows.Forms.Label();
            this.lblKarmaMysticAdeptPowerPoint = new System.Windows.Forms.Label();
            this.nudKarmaTechnique = new Chummer.NumericUpDownEx();
            this.nudKarmaMetamagic = new Chummer.NumericUpDownEx();
            this.nudKarmaInitiation = new Chummer.NumericUpDownEx();
            this.lblKarmaInitiationExtra = new System.Windows.Forms.Label();
            this.nudKarmaInitiationFlat = new Chummer.NumericUpDownEx();
            this.nudKarmaNewAIAdvancedProgram = new Chummer.NumericUpDownEx();
            this.nudKarmaNewAIProgram = new Chummer.NumericUpDownEx();
            this.nudKarmaNewComplexForm = new Chummer.NumericUpDownEx();
            this.nudKarmaMysticAdeptPowerPoint = new Chummer.NumericUpDownEx();
            this.lblKarmaSpiritFettering = new System.Windows.Forms.Label();
            this.nudKarmaSpiritFettering = new Chummer.NumericUpDownEx();
            this.lblKarmaSpiritFetteringExtra = new System.Windows.Forms.Label();
            this.lblKarmaQuality = new System.Windows.Forms.Label();
            this.nudKarmaQuality = new Chummer.NumericUpDownEx();
            this.lblKarmaQualityExtra = new System.Windows.Forms.Label();
            this.lblMetatypeCostsKarmaMultiplierLabel = new System.Windows.Forms.Label();
            this.nudMetatypeCostsKarmaMultiplier = new Chummer.NumericUpDownEx();
            this.lblKarmaNuyenPerWftM = new System.Windows.Forms.Label();
            this.nudKarmaNuyenPerWftM = new Chummer.NumericUpDownEx();
            this.lblKarmaNuyenPerExtraWftM = new System.Windows.Forms.Label();
            this.lblKarmaNuyenPerWftP = new System.Windows.Forms.Label();
            this.nudKarmaNuyenPerWftP = new Chummer.NumericUpDownEx();
            this.lblKarmaNuyenPerExtraWftP = new System.Windows.Forms.Label();
            this.tabCustomData = new System.Windows.Forms.TabPage();
            this.tlpOptionalRules = new Chummer.BufferedTableLayoutPanel(this.components);
            this.treCustomDataDirectories = new System.Windows.Forms.TreeView();
            this.gpbDirectoryInfo = new System.Windows.Forms.GroupBox();
            this.tlpDirectoryInfo = new Chummer.BufferedTableLayoutPanel(this.components);
            this.gbpDirectoryInfoDependencies = new System.Windows.Forms.GroupBox();
            this.pnlDirectoryDependencies = new System.Windows.Forms.Panel();
            this.lblDependencies = new System.Windows.Forms.Label();
            this.gbpDirectoryInfoIncompatibilities = new System.Windows.Forms.GroupBox();
            this.pnlDirectoryIncompatibilities = new System.Windows.Forms.Panel();
            this.lblIncompatibilities = new System.Windows.Forms.Label();
            this.tlpDirectoryInfoLeft = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblDirectoryNameLabel = new System.Windows.Forms.Label();
            this.lblDirectoryVersion = new System.Windows.Forms.Label();
            this.lblDirectoryName = new System.Windows.Forms.Label();
            this.lblDirectoryVersionLabel = new System.Windows.Forms.Label();
            this.gpbDirectoryAuthors = new System.Windows.Forms.GroupBox();
            this.pnlDirectoryAuthors = new System.Windows.Forms.Panel();
            this.lblDirectoryAuthors = new System.Windows.Forms.Label();
            this.rtbDirectoryDescription = new System.Windows.Forms.RichTextBox();
            this.lblCustomDataDirectoriesLabel = new System.Windows.Forms.Label();
            this.tlpOptionalRulesButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdToTopCustomDirectoryLoadOrder = new System.Windows.Forms.Button();
            this.cmdToBottomCustomDirectoryLoadOrder = new System.Windows.Forms.Button();
            this.cmdDecreaseCustomDirectoryLoadOrder = new System.Windows.Forms.Button();
            this.cmdIncreaseCustomDirectoryLoadOrder = new System.Windows.Forms.Button();
            this.cmdGlobalOptionsCustomData = new System.Windows.Forms.Button();
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
            this.tlpHouseRulesAttributes = new Chummer.BufferedTableLayoutPanel(this.components);
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
            this.flpMaxSkillRating = new System.Windows.Forms.FlowLayoutPanel();
            this.lblMaxSkillRating = new System.Windows.Forms.Label();
            this.nudMaxSkillRating = new Chummer.NumericUpDownEx();
            this.lblMaxKnowledgeSkillRating = new System.Windows.Forms.Label();
            this.nudMaxKnowledgeSkillRating = new Chummer.NumericUpDownEx();
            this.chkFreeMartialArtSpecialization = new Chummer.ColorableCheckBox(this.components);
            this.chkUsePointsOnBrokenGroups = new Chummer.ColorableCheckBox(this.components);
            this.chkAllowSkillRegrouping = new Chummer.ColorableCheckBox(this.components);
            this.chkCompensateSkillGroupKarmaDifference = new Chummer.ColorableCheckBox(this.components);
            this.chkSpecializationsBreakSkillGroups = new Chummer.ColorableCheckBox(this.components);
            this.gpbHouseRulesCombat = new System.Windows.Forms.GroupBox();
            this.tlpHouseRulesCombat = new Chummer.BufferedTableLayoutPanel(this.components);
            this.chkNoArmorEncumbrance = new Chummer.ColorableCheckBox(this.components);
            this.chkUnarmedSkillImprovements = new Chummer.ColorableCheckBox(this.components);
            this.gpbHouseRulesMagicResonance = new System.Windows.Forms.GroupBox();
            this.tlpHouseRulesMagicResonance = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblRegisteredSprites = new Chummer.LabelWithToolTip();
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
            this.lblBoundSpiritLimit = new Chummer.LabelWithToolTip();
            this.txtBoundSpiritLimit = new System.Windows.Forms.TextBox();
            this.txtRegisteredSpriteLimit = new System.Windows.Forms.TextBox();
            this.gpbHouseRules4eAdaptations = new System.Windows.Forms.GroupBox();
            this.tlpHouseRules4eAdaptations = new Chummer.BufferedTableLayoutPanel(this.components);
            this.chkEnemyKarmaQualityLimit = new Chummer.ColorableCheckBox(this.components);
            this.chkEnable4eStyleEnemyTracking = new Chummer.ColorableCheckBox(this.components);
            this.chkMoreLethalGameplay = new Chummer.ColorableCheckBox(this.components);
            this.flpKarmaGainedFromEnemies = new System.Windows.Forms.FlowLayoutPanel();
            this.lblKarmaGainedFromEnemies = new System.Windows.Forms.Label();
            this.nudKarmaGainedFromEnemies = new Chummer.NumericUpDownEx();
            this.lblKarmaGainedFromEnemiesExtra = new System.Windows.Forms.Label();
            this.lblSettingName = new System.Windows.Forms.Label();
            this.tlpButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdRename = new System.Windows.Forms.Button();
            this.cmdDelete = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdRestoreDefaults = new System.Windows.Forms.Button();
            this.cmdSave = new System.Windows.Forms.Button();
            this.cmdSaveAs = new System.Windows.Forms.Button();
            this.tlpOptions.SuspendLayout();
            this.tabOptions.SuspendLayout();
            this.tabBasicOptions.SuspendLayout();
            this.tlpBasicOptions.SuspendLayout();
            this.gpbSourcebook.SuspendLayout();
            this.flpBasicOptions.SuspendLayout();
            this.gpbBasicOptionsCreateSettings.SuspendLayout();
            this.tlpBasicOptionsCreateSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxAvail)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudStartingKarma)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSumToTen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxNuyenKarma)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudQualityKarmaLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxNumberMaxAttributes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxSkillRatingCreate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxKnowledgeSkillRatingCreate)).BeginInit();
            this.gpbBasicOptionsOfficialRules.SuspendLayout();
            this.tlpBasicOptionsOfficialRules.SuspendLayout();
            this.gpbBasicOptionsEncumbrance.SuspendLayout();
            this.tlpBasicOptionsEncumbrance.SuspendLayout();
            this.tlpEncumbranceInterval.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudEncumbrancePenaltyPhysicalLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEncumbrancePenaltyAgility)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEncumbrancePenaltyReaction)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEncumbrancePenaltyWoundModifier)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEncumbrancePenaltyMovementSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWeightDecimals)).BeginInit();
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
            this.gpbBasicOptionsInitiativeDice.SuspendLayout();
            this.tlpBasicOptionsInitiativeDice.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinInitiativeDice)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxInitiativeDice)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxAstralInitiativeDice)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxColdSimInitiativeDice)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxHotSimInitiativeDice)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinAstralInitiativeDice)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinColdSimInitiativeDice)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinHotSimInitiativeDice)).BeginInit();
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
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpirit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaJoinGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaCarryover)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSummoningFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpellShapingFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSustainingFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaWeaponFocus)).BeginInit();
            this.flpKarmaInitiation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaTechnique)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaMetamagic)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaInitiation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaInitiationFlat)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewAIAdvancedProgram)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewAIProgram)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewComplexForm)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaMysticAdeptPowerPoint)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpiritFettering)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaQuality)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMetatypeCostsKarmaMultiplier)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNuyenPerWftM)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNuyenPerWftP)).BeginInit();
            this.tabCustomData.SuspendLayout();
            this.tlpOptionalRules.SuspendLayout();
            this.gpbDirectoryInfo.SuspendLayout();
            this.tlpDirectoryInfo.SuspendLayout();
            this.gbpDirectoryInfoDependencies.SuspendLayout();
            this.pnlDirectoryDependencies.SuspendLayout();
            this.gbpDirectoryInfoIncompatibilities.SuspendLayout();
            this.pnlDirectoryIncompatibilities.SuspendLayout();
            this.tlpDirectoryInfoLeft.SuspendLayout();
            this.gpbDirectoryAuthors.SuspendLayout();
            this.pnlDirectoryAuthors.SuspendLayout();
            this.tlpOptionalRulesButtons.SuspendLayout();
            this.tabHouseRules.SuspendLayout();
            this.flpHouseRules.SuspendLayout();
            this.gpbHouseRulesQualities.SuspendLayout();
            this.tlpHouseRulesQualities.SuspendLayout();
            this.gpbHouseRulesAttributes.SuspendLayout();
            this.tlpHouseRulesAttributes.SuspendLayout();
            this.flpDroneArmorMultiplier.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDroneArmorMultiplier)).BeginInit();
            this.gpbHouseRulesSkills.SuspendLayout();
            this.tlpHouseRulesSkills.SuspendLayout();
            this.flpMaxSkillRating.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxSkillRating)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxKnowledgeSkillRating)).BeginInit();
            this.gpbHouseRulesCombat.SuspendLayout();
            this.tlpHouseRulesCombat.SuspendLayout();
            this.gpbHouseRulesMagicResonance.SuspendLayout();
            this.tlpHouseRulesMagicResonance.SuspendLayout();
            this.gpbHouseRules4eAdaptations.SuspendLayout();
            this.tlpHouseRules4eAdaptations.SuspendLayout();
            this.flpKarmaGainedFromEnemies.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaGainedFromEnemies)).BeginInit();
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
            this.flpBasicOptions.Controls.Add(this.gpbBasicOptionsEncumbrance);
            this.flpBasicOptions.Controls.Add(this.gpbBasicOptionsCyberlimbs);
            this.flpBasicOptions.Controls.Add(this.gpbBasicOptionsRounding);
            this.flpBasicOptions.Controls.Add(this.gpbBasicOptionsInitiativeDice);
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
            this.gpbBasicOptionsCreateSettings.Size = new System.Drawing.Size(569, 228);
            this.gpbBasicOptionsCreateSettings.TabIndex = 5;
            this.gpbBasicOptionsCreateSettings.TabStop = false;
            this.gpbBasicOptionsCreateSettings.Tag = "Label_CharacterOptions_CharacterCreationSettings";
            this.gpbBasicOptionsCreateSettings.Text = "Character Creation Settings";
            // 
            // tlpBasicOptionsCreateSettings
            // 
            this.tlpBasicOptionsCreateSettings.AutoSize = true;
            this.tlpBasicOptionsCreateSettings.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpBasicOptionsCreateSettings.ColumnCount = 6;
            this.tlpBasicOptionsCreateSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBasicOptionsCreateSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBasicOptionsCreateSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBasicOptionsCreateSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBasicOptionsCreateSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBasicOptionsCreateSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblAllowedCyberwareGrades, 4, 6);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.nudMaxAvail, 5, 0);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.cboPriorityTable, 1, 1);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblPriorityTable, 0, 1);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblBuildMethod, 0, 0);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.cboBuildMethod, 1, 0);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblMaxAvail, 4, 0);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.flpAllowedCyberwareGrades, 4, 7);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblKnowledgePoints, 0, 7);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.txtKnowledgePoints, 1, 7);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblStartingKarma, 0, 3);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.nudStartingKarma, 1, 3);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblPriorities, 4, 1);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.txtPriorities, 5, 1);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.nudSumToTen, 5, 2);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblSumToTen, 4, 2);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblMaxNuyenKarma, 4, 4);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblQualityKarmaLimit, 4, 3);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.nudMaxNuyenKarma, 5, 4);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.nudQualityKarmaLimit, 5, 3);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblNuyenExpression, 0, 4);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.txtNuyenExpression, 1, 4);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblContactPoints, 0, 6);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.txtContactPoints, 1, 6);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblMaxNumberMaxAttributes, 0, 5);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.nudMaxNumberMaxAttributes, 1, 5);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblMaxSkillRatingCreate, 2, 5);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.nudMaxSkillRatingCreate, 3, 5);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.lblMaxKnowledgeSkillRatingCreate, 4, 5);
            this.tlpBasicOptionsCreateSettings.Controls.Add(this.nudMaxKnowledgeSkillRatingCreate, 5, 5);
            this.tlpBasicOptionsCreateSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBasicOptionsCreateSettings.Location = new System.Drawing.Point(3, 16);
            this.tlpBasicOptionsCreateSettings.Name = "tlpBasicOptionsCreateSettings";
            this.tlpBasicOptionsCreateSettings.RowCount = 9;
            this.tlpBasicOptionsCreateSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsCreateSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsCreateSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsCreateSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsCreateSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsCreateSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsCreateSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsCreateSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsCreateSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBasicOptionsCreateSettings.Size = new System.Drawing.Size(563, 209);
            this.tlpBasicOptionsCreateSettings.TabIndex = 0;
            // 
            // lblAllowedCyberwareGrades
            // 
            this.lblAllowedCyberwareGrades.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblAllowedCyberwareGrades.AutoSize = true;
            this.tlpBasicOptionsCreateSettings.SetColumnSpan(this.lblAllowedCyberwareGrades, 2);
            this.lblAllowedCyberwareGrades.Location = new System.Drawing.Point(356, 164);
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
            this.nudMaxAvail.Location = new System.Drawing.Point(501, 3);
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
            this.tlpBasicOptionsCreateSettings.SetColumnSpan(this.cboPriorityTable, 3);
            this.cboPriorityTable.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPriorityTable.FormattingEnabled = true;
            this.cboPriorityTable.Location = new System.Drawing.Point(170, 42);
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
            this.lblPriorityTable.Location = new System.Drawing.Point(96, 46);
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
            this.lblBuildMethod.Location = new System.Drawing.Point(95, 7);
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
            this.tlpBasicOptionsCreateSettings.SetColumnSpan(this.cboBuildMethod, 3);
            this.cboBuildMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBuildMethod.FormattingEnabled = true;
            this.cboBuildMethod.Location = new System.Drawing.Point(170, 3);
            this.cboBuildMethod.Name = "cboBuildMethod";
            this.cboBuildMethod.Size = new System.Drawing.Size(180, 21);
            this.cboBuildMethod.TabIndex = 1;
            this.cboBuildMethod.TooltipText = "";
            // 
            // lblMaxAvail
            // 
            this.lblMaxAvail.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMaxAvail.AutoSize = true;
            this.lblMaxAvail.Location = new System.Drawing.Point(392, 7);
            this.lblMaxAvail.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMaxAvail.Name = "lblMaxAvail";
            this.lblMaxAvail.Size = new System.Drawing.Size(103, 13);
            this.lblMaxAvail.TabIndex = 19;
            this.lblMaxAvail.Tag = "Label_SelectBP_MaxAvail";
            this.lblMaxAvail.Text = "Maximum Availability";
            this.lblMaxAvail.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // flpAllowedCyberwareGrades
            // 
            this.flpAllowedCyberwareGrades.AutoSize = true;
            this.flpAllowedCyberwareGrades.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpBasicOptionsCreateSettings.SetColumnSpan(this.flpAllowedCyberwareGrades, 2);
            this.flpAllowedCyberwareGrades.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpAllowedCyberwareGrades.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpAllowedCyberwareGrades.Location = new System.Drawing.Point(353, 183);
            this.flpAllowedCyberwareGrades.Margin = new System.Windows.Forms.Padding(0);
            this.flpAllowedCyberwareGrades.Name = "flpAllowedCyberwareGrades";
            this.tlpBasicOptionsCreateSettings.SetRowSpan(this.flpAllowedCyberwareGrades, 2);
            this.flpAllowedCyberwareGrades.Size = new System.Drawing.Size(210, 26);
            this.flpAllowedCyberwareGrades.TabIndex = 29;
            this.flpAllowedCyberwareGrades.WrapContents = false;
            // 
            // lblKnowledgePoints
            // 
            this.lblKnowledgePoints.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKnowledgePoints.AutoSize = true;
            this.lblKnowledgePoints.Location = new System.Drawing.Point(72, 189);
            this.lblKnowledgePoints.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKnowledgePoints.Name = "lblKnowledgePoints";
            this.lblKnowledgePoints.Size = new System.Drawing.Size(92, 13);
            this.lblKnowledgePoints.TabIndex = 1;
            this.lblKnowledgePoints.Tag = "String_KnowledgePoints";
            this.lblKnowledgePoints.Text = "Knowledge Points";
            this.lblKnowledgePoints.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblKnowledgePoints.ToolTipText = "";
            // 
            // txtKnowledgePoints
            // 
            this.txtKnowledgePoints.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpBasicOptionsCreateSettings.SetColumnSpan(this.txtKnowledgePoints, 3);
            this.txtKnowledgePoints.Location = new System.Drawing.Point(170, 186);
            this.txtKnowledgePoints.Name = "txtKnowledgePoints";
            this.txtKnowledgePoints.Size = new System.Drawing.Size(180, 20);
            this.txtKnowledgePoints.TabIndex = 3;
            this.txtKnowledgePoints.Text = "({INTUnaug} + {LOGUnaug}) * 2";
            this.txtKnowledgePoints.TextChanged += new System.EventHandler(this.txtKnowledgePoints_TextChanged);
            // 
            // lblStartingKarma
            // 
            this.lblStartingKarma.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblStartingKarma.AutoSize = true;
            this.lblStartingKarma.Location = new System.Drawing.Point(88, 85);
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
            this.tlpBasicOptionsCreateSettings.SetColumnSpan(this.nudStartingKarma, 3);
            this.nudStartingKarma.Location = new System.Drawing.Point(170, 82);
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
            this.lblPriorities.Location = new System.Drawing.Point(449, 33);
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
            this.txtPriorities.Location = new System.Drawing.Point(501, 30);
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
            this.nudSumToTen.Location = new System.Drawing.Point(501, 56);
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
            this.lblSumToTen.Location = new System.Drawing.Point(433, 59);
            this.lblSumToTen.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSumToTen.Name = "lblSumToTen";
            this.lblSumToTen.Size = new System.Drawing.Size(62, 13);
            this.lblSumToTen.TabIndex = 4;
            this.lblSumToTen.Tag = "Label_SelectBP_SumToX";
            this.lblSumToTen.Text = "Sum to Ten";
            this.lblSumToTen.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblMaxNuyenKarma
            // 
            this.lblMaxNuyenKarma.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMaxNuyenKarma.AutoSize = true;
            this.lblMaxNuyenKarma.Location = new System.Drawing.Point(401, 111);
            this.lblMaxNuyenKarma.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMaxNuyenKarma.Name = "lblMaxNuyenKarma";
            this.lblMaxNuyenKarma.Size = new System.Drawing.Size(94, 13);
            this.lblMaxNuyenKarma.TabIndex = 16;
            this.lblMaxNuyenKarma.Tag = "Label_SelectBP_MaxNuyen";
            this.lblMaxNuyenKarma.Text = "Max Nuyen Karma";
            this.lblMaxNuyenKarma.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblQualityKarmaLimit
            // 
            this.lblQualityKarmaLimit.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblQualityKarmaLimit.AutoSize = true;
            this.lblQualityKarmaLimit.Location = new System.Drawing.Point(399, 85);
            this.lblQualityKarmaLimit.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblQualityKarmaLimit.Name = "lblQualityKarmaLimit";
            this.lblQualityKarmaLimit.Size = new System.Drawing.Size(96, 13);
            this.lblQualityKarmaLimit.TabIndex = 30;
            this.lblQualityKarmaLimit.Tag = "Label_SelectBP_QualityKarmaLimit";
            this.lblQualityKarmaLimit.Text = "Quality Karma Limit";
            this.lblQualityKarmaLimit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudMaxNuyenKarma
            // 
            this.nudMaxNuyenKarma.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudMaxNuyenKarma.AutoSize = true;
            this.nudMaxNuyenKarma.Location = new System.Drawing.Point(501, 108);
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
            // nudQualityKarmaLimit
            // 
            this.nudQualityKarmaLimit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudQualityKarmaLimit.AutoSize = true;
            this.nudQualityKarmaLimit.Location = new System.Drawing.Point(501, 82);
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
            // lblNuyenExpression
            // 
            this.lblNuyenExpression.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblNuyenExpression.AutoSize = true;
            this.lblNuyenExpression.Location = new System.Drawing.Point(72, 111);
            this.lblNuyenExpression.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblNuyenExpression.Name = "lblNuyenExpression";
            this.lblNuyenExpression.Size = new System.Drawing.Size(92, 13);
            this.lblNuyenExpression.TabIndex = 32;
            this.lblNuyenExpression.Tag = "Label_CharacterOptions_NuyenExpression";
            this.lblNuyenExpression.Text = "Nuyen Expression";
            this.lblNuyenExpression.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblNuyenExpression.ToolTipText = "";
            // 
            // txtNuyenExpression
            // 
            this.txtNuyenExpression.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpBasicOptionsCreateSettings.SetColumnSpan(this.txtNuyenExpression, 3);
            this.txtNuyenExpression.Location = new System.Drawing.Point(170, 108);
            this.txtNuyenExpression.Name = "txtNuyenExpression";
            this.txtNuyenExpression.Size = new System.Drawing.Size(180, 20);
            this.txtNuyenExpression.TabIndex = 33;
            this.txtNuyenExpression.Text = "{Karma} * 2000 + {PriorityNuyen}";
            this.txtNuyenExpression.TextChanged += new System.EventHandler(this.txtNuyenExpression_TextChanged);
            // 
            // lblContactPoints
            // 
            this.lblContactPoints.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblContactPoints.AutoSize = true;
            this.lblContactPoints.Location = new System.Drawing.Point(88, 163);
            this.lblContactPoints.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblContactPoints.Name = "lblContactPoints";
            this.lblContactPoints.Size = new System.Drawing.Size(76, 13);
            this.lblContactPoints.TabIndex = 0;
            this.lblContactPoints.Tag = "String_ContactPoints";
            this.lblContactPoints.Text = "Contact Points";
            this.lblContactPoints.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblContactPoints.ToolTipText = "";
            // 
            // txtContactPoints
            // 
            this.txtContactPoints.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpBasicOptionsCreateSettings.SetColumnSpan(this.txtContactPoints, 3);
            this.txtContactPoints.Location = new System.Drawing.Point(170, 160);
            this.txtContactPoints.Name = "txtContactPoints";
            this.txtContactPoints.Size = new System.Drawing.Size(180, 20);
            this.txtContactPoints.TabIndex = 2;
            this.txtContactPoints.Text = "{CHAUnaug} * 3";
            this.txtContactPoints.TextChanged += new System.EventHandler(this.txtContactPoints_TextChanged);
            // 
            // lblMaxNumberMaxAttributes
            // 
            this.lblMaxNumberMaxAttributes.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMaxNumberMaxAttributes.AutoSize = true;
            this.lblMaxNumberMaxAttributes.Location = new System.Drawing.Point(3, 137);
            this.lblMaxNumberMaxAttributes.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMaxNumberMaxAttributes.Name = "lblMaxNumberMaxAttributes";
            this.lblMaxNumberMaxAttributes.Size = new System.Drawing.Size(161, 13);
            this.lblMaxNumberMaxAttributes.TabIndex = 36;
            this.lblMaxNumberMaxAttributes.Tag = "Label_CharacterOptions_MaxNumberMaxAttributes";
            this.lblMaxNumberMaxAttributes.Text = "Max Number of Attributes at Max";
            this.lblMaxNumberMaxAttributes.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblMaxNumberMaxAttributes.ToolTipText = "";
            // 
            // nudMaxNumberMaxAttributes
            // 
            this.nudMaxNumberMaxAttributes.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudMaxNumberMaxAttributes.AutoSize = true;
            this.nudMaxNumberMaxAttributes.Location = new System.Drawing.Point(170, 134);
            this.nudMaxNumberMaxAttributes.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.nudMaxNumberMaxAttributes.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMaxNumberMaxAttributes.Name = "nudMaxNumberMaxAttributes";
            this.nudMaxNumberMaxAttributes.Size = new System.Drawing.Size(29, 20);
            this.nudMaxNumberMaxAttributes.TabIndex = 37;
            // 
            // lblMaxSkillRatingCreate
            // 
            this.lblMaxSkillRatingCreate.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMaxSkillRatingCreate.AutoSize = true;
            this.lblMaxSkillRatingCreate.Location = new System.Drawing.Point(226, 137);
            this.lblMaxSkillRatingCreate.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMaxSkillRatingCreate.Name = "lblMaxSkillRatingCreate";
            this.lblMaxSkillRatingCreate.Size = new System.Drawing.Size(83, 13);
            this.lblMaxSkillRatingCreate.TabIndex = 34;
            this.lblMaxSkillRatingCreate.Tag = "Label_CharacterOptions_MaxSkillRating";
            this.lblMaxSkillRatingCreate.Text = "Max Skill Rating";
            this.lblMaxSkillRatingCreate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblMaxSkillRatingCreate.ToolTipText = "";
            // 
            // nudMaxSkillRatingCreate
            // 
            this.nudMaxSkillRatingCreate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudMaxSkillRatingCreate.AutoSize = true;
            this.nudMaxSkillRatingCreate.Location = new System.Drawing.Point(315, 134);
            this.nudMaxSkillRatingCreate.Maximum = new decimal(new int[] {
            12,
            0,
            0,
            0});
            this.nudMaxSkillRatingCreate.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMaxSkillRatingCreate.Name = "nudMaxSkillRatingCreate";
            this.nudMaxSkillRatingCreate.Size = new System.Drawing.Size(35, 20);
            this.nudMaxSkillRatingCreate.TabIndex = 35;
            // 
            // lblMaxKnowledgeSkillRatingCreate
            // 
            this.lblMaxKnowledgeSkillRatingCreate.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMaxKnowledgeSkillRatingCreate.AutoSize = true;
            this.lblMaxKnowledgeSkillRatingCreate.Location = new System.Drawing.Point(356, 137);
            this.lblMaxKnowledgeSkillRatingCreate.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMaxKnowledgeSkillRatingCreate.Name = "lblMaxKnowledgeSkillRatingCreate";
            this.lblMaxKnowledgeSkillRatingCreate.Size = new System.Drawing.Size(139, 13);
            this.lblMaxKnowledgeSkillRatingCreate.TabIndex = 38;
            this.lblMaxKnowledgeSkillRatingCreate.Tag = "Label_CharacterOptions_MaxKnowledgeSkillRating";
            this.lblMaxKnowledgeSkillRatingCreate.Text = "Max Knowledge Skill Rating";
            this.lblMaxKnowledgeSkillRatingCreate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblMaxKnowledgeSkillRatingCreate.ToolTipText = "";
            // 
            // nudMaxKnowledgeSkillRatingCreate
            // 
            this.nudMaxKnowledgeSkillRatingCreate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudMaxKnowledgeSkillRatingCreate.AutoSize = true;
            this.nudMaxKnowledgeSkillRatingCreate.Location = new System.Drawing.Point(501, 134);
            this.nudMaxKnowledgeSkillRatingCreate.Maximum = new decimal(new int[] {
            12,
            0,
            0,
            0});
            this.nudMaxKnowledgeSkillRatingCreate.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMaxKnowledgeSkillRatingCreate.Name = "nudMaxKnowledgeSkillRatingCreate";
            this.nudMaxKnowledgeSkillRatingCreate.Size = new System.Drawing.Size(35, 20);
            this.nudMaxKnowledgeSkillRatingCreate.TabIndex = 39;
            // 
            // gpbBasicOptionsOfficialRules
            // 
            this.gpbBasicOptionsOfficialRules.AutoSize = true;
            this.gpbBasicOptionsOfficialRules.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbBasicOptionsOfficialRules.Controls.Add(this.tlpBasicOptionsOfficialRules);
            this.gpbBasicOptionsOfficialRules.Location = new System.Drawing.Point(3, 237);
            this.gpbBasicOptionsOfficialRules.Name = "gpbBasicOptionsOfficialRules";
            this.gpbBasicOptionsOfficialRules.Size = new System.Drawing.Size(472, 219);
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
            this.tlpBasicOptionsOfficialRules.RowCount = 8;
            this.tlpBasicOptionsOfficialRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsOfficialRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsOfficialRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsOfficialRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsOfficialRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsOfficialRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsOfficialRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsOfficialRules.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBasicOptionsOfficialRules.Size = new System.Drawing.Size(466, 200);
            this.tlpBasicOptionsOfficialRules.TabIndex = 0;
            // 
            // chkAllowFreeGrids
            // 
            this.chkAllowFreeGrids.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkAllowFreeGrids.AutoSize = true;
            this.tlpBasicOptionsOfficialRules.SetColumnSpan(this.chkAllowFreeGrids, 5);
            this.chkAllowFreeGrids.DefaultColorScheme = true;
            this.chkAllowFreeGrids.Location = new System.Drawing.Point(3, 179);
            this.chkAllowFreeGrids.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.chkAllowPointBuySpecializationsOnKarmaSkills.Location = new System.Drawing.Point(3, 154);
            this.chkAllowPointBuySpecializationsOnKarmaSkills.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.chkStrictSkillGroups.Location = new System.Drawing.Point(3, 129);
            this.chkStrictSkillGroups.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.chkEnforceCapacity.Location = new System.Drawing.Point(3, 4);
            this.chkEnforceCapacity.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.chkLicenseEachRestrictedItem.Location = new System.Drawing.Point(3, 29);
            this.chkLicenseEachRestrictedItem.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.chkRestrictRecoil.Location = new System.Drawing.Point(3, 104);
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
            this.chkDronemodsMaximumPilot.Location = new System.Drawing.Point(23, 79);
            this.chkDronemodsMaximumPilot.Margin = new System.Windows.Forms.Padding(23, 4, 3, 4);
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
            this.chkDronemods.Location = new System.Drawing.Point(3, 54);
            this.chkDronemods.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkDronemods.Name = "chkDronemods";
            this.chkDronemods.Size = new System.Drawing.Size(206, 17);
            this.chkDronemods.TabIndex = 36;
            this.chkDronemods.Tag = "Checkbox_Options_Dronemods";
            this.chkDronemods.Text = "Use Drone Modification rules (R5 122)";
            this.chkDronemods.UseVisualStyleBackColor = true;
            // 
            // gpbBasicOptionsEncumbrance
            // 
            this.gpbBasicOptionsEncumbrance.AutoSize = true;
            this.gpbBasicOptionsEncumbrance.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbBasicOptionsEncumbrance.Controls.Add(this.tlpBasicOptionsEncumbrance);
            this.gpbBasicOptionsEncumbrance.Location = new System.Drawing.Point(481, 237);
            this.gpbBasicOptionsEncumbrance.Name = "gpbBasicOptionsEncumbrance";
            this.gpbBasicOptionsEncumbrance.Size = new System.Drawing.Size(369, 278);
            this.gpbBasicOptionsEncumbrance.TabIndex = 7;
            this.gpbBasicOptionsEncumbrance.TabStop = false;
            this.gpbBasicOptionsEncumbrance.Tag = "String_Encumbrance";
            this.gpbBasicOptionsEncumbrance.Text = "Encumbrance";
            // 
            // tlpBasicOptionsEncumbrance
            // 
            this.tlpBasicOptionsEncumbrance.AutoSize = true;
            this.tlpBasicOptionsEncumbrance.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpBasicOptionsEncumbrance.ColumnCount = 4;
            this.tlpBasicOptionsEncumbrance.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBasicOptionsEncumbrance.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBasicOptionsEncumbrance.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBasicOptionsEncumbrance.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBasicOptionsEncumbrance.Controls.Add(this.lblWeightDecimalPlaces, 0, 0);
            this.tlpBasicOptionsEncumbrance.Controls.Add(this.lblCarryLimit, 0, 2);
            this.tlpBasicOptionsEncumbrance.Controls.Add(this.lblLiftLimit, 0, 1);
            this.tlpBasicOptionsEncumbrance.Controls.Add(this.txtLiftLimit, 1, 1);
            this.tlpBasicOptionsEncumbrance.Controls.Add(this.txtCarryLimit, 1, 2);
            this.tlpBasicOptionsEncumbrance.Controls.Add(this.lblEncumbrancePenaltiesHeader, 0, 4);
            this.tlpBasicOptionsEncumbrance.Controls.Add(this.tlpEncumbranceInterval, 0, 4);
            this.tlpBasicOptionsEncumbrance.Controls.Add(this.chkEncumbrancePenaltyPhysicalLimit, 0, 6);
            this.tlpBasicOptionsEncumbrance.Controls.Add(this.chkEncumbrancePenaltyMovementSpeed, 0, 7);
            this.tlpBasicOptionsEncumbrance.Controls.Add(this.chkEncumbrancePenaltyAgility, 0, 8);
            this.tlpBasicOptionsEncumbrance.Controls.Add(this.chkEncumbrancePenaltyReaction, 0, 9);
            this.tlpBasicOptionsEncumbrance.Controls.Add(this.chkEncumbrancePenaltyWoundModifier, 0, 10);
            this.tlpBasicOptionsEncumbrance.Controls.Add(this.nudEncumbrancePenaltyPhysicalLimit, 2, 6);
            this.tlpBasicOptionsEncumbrance.Controls.Add(this.nudEncumbrancePenaltyAgility, 2, 8);
            this.tlpBasicOptionsEncumbrance.Controls.Add(this.nudEncumbrancePenaltyReaction, 2, 9);
            this.tlpBasicOptionsEncumbrance.Controls.Add(this.nudEncumbrancePenaltyWoundModifier, 2, 10);
            this.tlpBasicOptionsEncumbrance.Controls.Add(this.nudEncumbrancePenaltyMovementSpeed, 2, 7);
            this.tlpBasicOptionsEncumbrance.Controls.Add(this.lblEncumbrancePenaltyMovementSpeedPercent, 3, 7);
            this.tlpBasicOptionsEncumbrance.Controls.Add(this.nudWeightDecimals, 2, 0);
            this.tlpBasicOptionsEncumbrance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBasicOptionsEncumbrance.Location = new System.Drawing.Point(3, 16);
            this.tlpBasicOptionsEncumbrance.Name = "tlpBasicOptionsEncumbrance";
            this.tlpBasicOptionsEncumbrance.RowCount = 11;
            this.tlpBasicOptionsEncumbrance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsEncumbrance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsEncumbrance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsEncumbrance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsEncumbrance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsEncumbrance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsEncumbrance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsEncumbrance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsEncumbrance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsEncumbrance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsEncumbrance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsEncumbrance.Size = new System.Drawing.Size(363, 259);
            this.tlpBasicOptionsEncumbrance.TabIndex = 0;
            // 
            // lblWeightDecimalPlaces
            // 
            this.lblWeightDecimalPlaces.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblWeightDecimalPlaces.AutoSize = true;
            this.tlpBasicOptionsEncumbrance.SetColumnSpan(this.lblWeightDecimalPlaces, 2);
            this.lblWeightDecimalPlaces.Location = new System.Drawing.Point(7, 6);
            this.lblWeightDecimalPlaces.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeightDecimalPlaces.Name = "lblWeightDecimalPlaces";
            this.lblWeightDecimalPlaces.Size = new System.Drawing.Size(221, 13);
            this.lblWeightDecimalPlaces.TabIndex = 67;
            this.lblWeightDecimalPlaces.Tag = "Label_CharacterOptions_WeightDecimalPlaces";
            this.lblWeightDecimalPlaces.Text = "Number of decimal places to show for weight:";
            this.lblWeightDecimalPlaces.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblWeightDecimalPlaces.ToolTipText = "";
            // 
            // lblCarryLimit
            // 
            this.lblCarryLimit.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCarryLimit.AutoSize = true;
            this.lblCarryLimit.Location = new System.Drawing.Point(3, 58);
            this.lblCarryLimit.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCarryLimit.Name = "lblCarryLimit";
            this.lblCarryLimit.Size = new System.Drawing.Size(58, 13);
            this.lblCarryLimit.TabIndex = 2;
            this.lblCarryLimit.Tag = "Label_CharacterOptions_CarryLimit";
            this.lblCarryLimit.Text = "Carry Limit:";
            this.lblCarryLimit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblCarryLimit.ToolTipText = "";
            // 
            // lblLiftLimit
            // 
            this.lblLiftLimit.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblLiftLimit.AutoSize = true;
            this.lblLiftLimit.Location = new System.Drawing.Point(13, 32);
            this.lblLiftLimit.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLiftLimit.Name = "lblLiftLimit";
            this.lblLiftLimit.Size = new System.Drawing.Size(48, 13);
            this.lblLiftLimit.TabIndex = 1;
            this.lblLiftLimit.Tag = "Label_CharacterOptions_LiftLimit";
            this.lblLiftLimit.Text = "Lift Limit:";
            this.lblLiftLimit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblLiftLimit.ToolTipText = "";
            // 
            // txtLiftLimit
            // 
            this.txtLiftLimit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpBasicOptionsEncumbrance.SetColumnSpan(this.txtLiftLimit, 3);
            this.txtLiftLimit.Location = new System.Drawing.Point(67, 29);
            this.txtLiftLimit.Name = "txtLiftLimit";
            this.txtLiftLimit.Size = new System.Drawing.Size(293, 20);
            this.txtLiftLimit.TabIndex = 3;
            this.txtLiftLimit.Text = "{STR} * 15";
            this.txtLiftLimit.TextChanged += new System.EventHandler(this.txtLiftLimit_TextChanged);
            // 
            // txtCarryLimit
            // 
            this.txtCarryLimit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpBasicOptionsEncumbrance.SetColumnSpan(this.txtCarryLimit, 3);
            this.txtCarryLimit.Location = new System.Drawing.Point(67, 55);
            this.txtCarryLimit.Name = "txtCarryLimit";
            this.txtCarryLimit.Size = new System.Drawing.Size(293, 20);
            this.txtCarryLimit.TabIndex = 4;
            this.txtCarryLimit.Text = "{STR} * 10";
            this.txtCarryLimit.TextChanged += new System.EventHandler(this.txtCarryLimit_TextChanged);
            // 
            // lblEncumbrancePenaltiesHeader
            // 
            this.lblEncumbrancePenaltiesHeader.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblEncumbrancePenaltiesHeader.AutoSize = true;
            this.tlpBasicOptionsEncumbrance.SetColumnSpan(this.lblEncumbrancePenaltiesHeader, 4);
            this.lblEncumbrancePenaltiesHeader.Location = new System.Drawing.Point(3, 84);
            this.lblEncumbrancePenaltiesHeader.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblEncumbrancePenaltiesHeader.Name = "lblEncumbrancePenaltiesHeader";
            this.lblEncumbrancePenaltiesHeader.Size = new System.Drawing.Size(248, 13);
            this.lblEncumbrancePenaltiesHeader.TabIndex = 53;
            this.lblEncumbrancePenaltiesHeader.Tag = "Label_CharacterOptions_EncumbrancePenaltiesHeader";
            this.lblEncumbrancePenaltiesHeader.Text = "Each encumbrance penalty tick does the following:";
            this.lblEncumbrancePenaltiesHeader.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tlpEncumbranceInterval
            // 
            this.tlpEncumbranceInterval.AutoSize = true;
            this.tlpEncumbranceInterval.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpEncumbranceInterval.ColumnCount = 3;
            this.tlpBasicOptionsEncumbrance.SetColumnSpan(this.tlpEncumbranceInterval, 4);
            this.tlpEncumbranceInterval.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpEncumbranceInterval.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpEncumbranceInterval.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpEncumbranceInterval.Controls.Add(this.lblEncumbranceIntervalLeft, 0, 0);
            this.tlpEncumbranceInterval.Controls.Add(this.txtEncumbranceInterval, 1, 0);
            this.tlpEncumbranceInterval.Controls.Add(this.lblEncumbranceIntervalRight, 2, 0);
            this.tlpEncumbranceInterval.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpEncumbranceInterval.Location = new System.Drawing.Point(0, 103);
            this.tlpEncumbranceInterval.Margin = new System.Windows.Forms.Padding(0);
            this.tlpEncumbranceInterval.Name = "tlpEncumbranceInterval";
            this.tlpEncumbranceInterval.RowCount = 1;
            this.tlpEncumbranceInterval.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpEncumbranceInterval.Size = new System.Drawing.Size(363, 26);
            this.tlpEncumbranceInterval.TabIndex = 59;
            // 
            // lblEncumbranceIntervalLeft
            // 
            this.lblEncumbranceIntervalLeft.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblEncumbranceIntervalLeft.AutoSize = true;
            this.lblEncumbranceIntervalLeft.Location = new System.Drawing.Point(3, 6);
            this.lblEncumbranceIntervalLeft.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblEncumbranceIntervalLeft.Name = "lblEncumbranceIntervalLeft";
            this.lblEncumbranceIntervalLeft.Size = new System.Drawing.Size(182, 13);
            this.lblEncumbranceIntervalLeft.TabIndex = 5;
            this.lblEncumbranceIntervalLeft.Tag = "Label_CharacterOptions_EncumbranceIntervalLeft";
            this.lblEncumbranceIntervalLeft.Text = "Increase encumbrance penalty every";
            this.lblEncumbranceIntervalLeft.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblEncumbranceIntervalLeft.ToolTipText = "";
            // 
            // txtEncumbranceInterval
            // 
            this.txtEncumbranceInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEncumbranceInterval.Location = new System.Drawing.Point(191, 3);
            this.txtEncumbranceInterval.Name = "txtEncumbranceInterval";
            this.txtEncumbranceInterval.Size = new System.Drawing.Size(69, 20);
            this.txtEncumbranceInterval.TabIndex = 52;
            this.txtEncumbranceInterval.Text = "15";
            this.txtEncumbranceInterval.TextChanged += new System.EventHandler(this.txtEncumbranceInterval_TextChanged);
            // 
            // lblEncumbranceIntervalRight
            // 
            this.lblEncumbranceIntervalRight.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblEncumbranceIntervalRight.AutoSize = true;
            this.lblEncumbranceIntervalRight.Location = new System.Drawing.Point(266, 6);
            this.lblEncumbranceIntervalRight.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblEncumbranceIntervalRight.Name = "lblEncumbranceIntervalRight";
            this.lblEncumbranceIntervalRight.Size = new System.Drawing.Size(94, 13);
            this.lblEncumbranceIntervalRight.TabIndex = 51;
            this.lblEncumbranceIntervalRight.Tag = "Label_CharacterOptions_EncumbranceIntervalRight";
            this.lblEncumbranceIntervalRight.Text = "kg over Carry Limit";
            this.lblEncumbranceIntervalRight.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblEncumbranceIntervalRight.ToolTipText = "";
            // 
            // chkEncumbrancePenaltyPhysicalLimit
            // 
            this.chkEncumbrancePenaltyPhysicalLimit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkEncumbrancePenaltyPhysicalLimit.AutoSize = true;
            this.tlpBasicOptionsEncumbrance.SetColumnSpan(this.chkEncumbrancePenaltyPhysicalLimit, 2);
            this.chkEncumbrancePenaltyPhysicalLimit.DefaultColorScheme = true;
            this.chkEncumbrancePenaltyPhysicalLimit.Location = new System.Drawing.Point(23, 133);
            this.chkEncumbrancePenaltyPhysicalLimit.Margin = new System.Windows.Forms.Padding(23, 4, 3, 4);
            this.chkEncumbrancePenaltyPhysicalLimit.Name = "chkEncumbrancePenaltyPhysicalLimit";
            this.chkEncumbrancePenaltyPhysicalLimit.Size = new System.Drawing.Size(135, 17);
            this.chkEncumbrancePenaltyPhysicalLimit.TabIndex = 54;
            this.chkEncumbrancePenaltyPhysicalLimit.Tag = "Checkbox_CharacterOptions_EncumbrancePenaltyPhysicalLimit";
            this.chkEncumbrancePenaltyPhysicalLimit.Text = "Lower Physical Limit by";
            this.chkEncumbrancePenaltyPhysicalLimit.UseVisualStyleBackColor = true;
            // 
            // chkEncumbrancePenaltyMovementSpeed
            // 
            this.chkEncumbrancePenaltyMovementSpeed.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkEncumbrancePenaltyMovementSpeed.AutoSize = true;
            this.tlpBasicOptionsEncumbrance.SetColumnSpan(this.chkEncumbrancePenaltyMovementSpeed, 2);
            this.chkEncumbrancePenaltyMovementSpeed.DefaultColorScheme = true;
            this.chkEncumbrancePenaltyMovementSpeed.Location = new System.Drawing.Point(23, 159);
            this.chkEncumbrancePenaltyMovementSpeed.Margin = new System.Windows.Forms.Padding(23, 4, 3, 4);
            this.chkEncumbrancePenaltyMovementSpeed.Name = "chkEncumbrancePenaltyMovementSpeed";
            this.chkEncumbrancePenaltyMovementSpeed.Size = new System.Drawing.Size(161, 17);
            this.chkEncumbrancePenaltyMovementSpeed.TabIndex = 55;
            this.chkEncumbrancePenaltyMovementSpeed.Tag = "Checkbox_CharacterOptions_EncumbrancePenaltyMovementSpeed";
            this.chkEncumbrancePenaltyMovementSpeed.Text = "Lower Movement Speeds by";
            this.chkEncumbrancePenaltyMovementSpeed.UseVisualStyleBackColor = true;
            // 
            // chkEncumbrancePenaltyAgility
            // 
            this.chkEncumbrancePenaltyAgility.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkEncumbrancePenaltyAgility.AutoSize = true;
            this.tlpBasicOptionsEncumbrance.SetColumnSpan(this.chkEncumbrancePenaltyAgility, 2);
            this.chkEncumbrancePenaltyAgility.DefaultColorScheme = true;
            this.chkEncumbrancePenaltyAgility.Location = new System.Drawing.Point(23, 185);
            this.chkEncumbrancePenaltyAgility.Margin = new System.Windows.Forms.Padding(23, 4, 3, 4);
            this.chkEncumbrancePenaltyAgility.Name = "chkEncumbrancePenaltyAgility";
            this.chkEncumbrancePenaltyAgility.Size = new System.Drawing.Size(99, 17);
            this.chkEncumbrancePenaltyAgility.TabIndex = 56;
            this.chkEncumbrancePenaltyAgility.Tag = "Checkbox_CharacterOptions_EncumbrancePenaltyAgility";
            this.chkEncumbrancePenaltyAgility.Text = "Lower Agility by";
            this.chkEncumbrancePenaltyAgility.UseVisualStyleBackColor = true;
            // 
            // chkEncumbrancePenaltyReaction
            // 
            this.chkEncumbrancePenaltyReaction.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkEncumbrancePenaltyReaction.AutoSize = true;
            this.tlpBasicOptionsEncumbrance.SetColumnSpan(this.chkEncumbrancePenaltyReaction, 2);
            this.chkEncumbrancePenaltyReaction.DefaultColorScheme = true;
            this.chkEncumbrancePenaltyReaction.Location = new System.Drawing.Point(23, 211);
            this.chkEncumbrancePenaltyReaction.Margin = new System.Windows.Forms.Padding(23, 4, 3, 4);
            this.chkEncumbrancePenaltyReaction.Name = "chkEncumbrancePenaltyReaction";
            this.chkEncumbrancePenaltyReaction.Size = new System.Drawing.Size(115, 17);
            this.chkEncumbrancePenaltyReaction.TabIndex = 57;
            this.chkEncumbrancePenaltyReaction.Tag = "Checkbox_CharacterOptions_EncumbrancePenaltyReaction";
            this.chkEncumbrancePenaltyReaction.Text = "Lower Reaction by";
            this.chkEncumbrancePenaltyReaction.UseVisualStyleBackColor = true;
            // 
            // chkEncumbrancePenaltyWoundModifier
            // 
            this.chkEncumbrancePenaltyWoundModifier.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkEncumbrancePenaltyWoundModifier.AutoSize = true;
            this.tlpBasicOptionsEncumbrance.SetColumnSpan(this.chkEncumbrancePenaltyWoundModifier, 2);
            this.chkEncumbrancePenaltyWoundModifier.DefaultColorScheme = true;
            this.chkEncumbrancePenaltyWoundModifier.Location = new System.Drawing.Point(23, 237);
            this.chkEncumbrancePenaltyWoundModifier.Margin = new System.Windows.Forms.Padding(23, 4, 3, 4);
            this.chkEncumbrancePenaltyWoundModifier.Name = "chkEncumbrancePenaltyWoundModifier";
            this.chkEncumbrancePenaltyWoundModifier.Size = new System.Drawing.Size(205, 17);
            this.chkEncumbrancePenaltyWoundModifier.TabIndex = 58;
            this.chkEncumbrancePenaltyWoundModifier.Tag = "Checkbox_CharacterOptions_EncumbrancePenaltySkillWoundModifier";
            this.chkEncumbrancePenaltyWoundModifier.Text = "Apply an additional Wound Modifier of";
            this.chkEncumbrancePenaltyWoundModifier.UseVisualStyleBackColor = true;
            // 
            // nudEncumbrancePenaltyPhysicalLimit
            // 
            this.nudEncumbrancePenaltyPhysicalLimit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudEncumbrancePenaltyPhysicalLimit.AutoSize = true;
            this.nudEncumbrancePenaltyPhysicalLimit.Location = new System.Drawing.Point(234, 132);
            this.nudEncumbrancePenaltyPhysicalLimit.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudEncumbrancePenaltyPhysicalLimit.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudEncumbrancePenaltyPhysicalLimit.Name = "nudEncumbrancePenaltyPhysicalLimit";
            this.nudEncumbrancePenaltyPhysicalLimit.Size = new System.Drawing.Size(41, 20);
            this.nudEncumbrancePenaltyPhysicalLimit.TabIndex = 60;
            this.nudEncumbrancePenaltyPhysicalLimit.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // nudEncumbrancePenaltyAgility
            // 
            this.nudEncumbrancePenaltyAgility.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudEncumbrancePenaltyAgility.AutoSize = true;
            this.nudEncumbrancePenaltyAgility.Location = new System.Drawing.Point(234, 184);
            this.nudEncumbrancePenaltyAgility.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudEncumbrancePenaltyAgility.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudEncumbrancePenaltyAgility.Name = "nudEncumbrancePenaltyAgility";
            this.nudEncumbrancePenaltyAgility.Size = new System.Drawing.Size(41, 20);
            this.nudEncumbrancePenaltyAgility.TabIndex = 61;
            this.nudEncumbrancePenaltyAgility.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // nudEncumbrancePenaltyReaction
            // 
            this.nudEncumbrancePenaltyReaction.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudEncumbrancePenaltyReaction.AutoSize = true;
            this.nudEncumbrancePenaltyReaction.Location = new System.Drawing.Point(234, 210);
            this.nudEncumbrancePenaltyReaction.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudEncumbrancePenaltyReaction.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudEncumbrancePenaltyReaction.Name = "nudEncumbrancePenaltyReaction";
            this.nudEncumbrancePenaltyReaction.Size = new System.Drawing.Size(41, 20);
            this.nudEncumbrancePenaltyReaction.TabIndex = 62;
            this.nudEncumbrancePenaltyReaction.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // nudEncumbrancePenaltyWoundModifier
            // 
            this.nudEncumbrancePenaltyWoundModifier.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudEncumbrancePenaltyWoundModifier.AutoSize = true;
            this.nudEncumbrancePenaltyWoundModifier.Location = new System.Drawing.Point(234, 236);
            this.nudEncumbrancePenaltyWoundModifier.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudEncumbrancePenaltyWoundModifier.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudEncumbrancePenaltyWoundModifier.Name = "nudEncumbrancePenaltyWoundModifier";
            this.nudEncumbrancePenaltyWoundModifier.Size = new System.Drawing.Size(41, 20);
            this.nudEncumbrancePenaltyWoundModifier.TabIndex = 63;
            this.nudEncumbrancePenaltyWoundModifier.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // nudEncumbrancePenaltyMovementSpeed
            // 
            this.nudEncumbrancePenaltyMovementSpeed.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudEncumbrancePenaltyMovementSpeed.AutoSize = true;
            this.nudEncumbrancePenaltyMovementSpeed.Location = new System.Drawing.Point(234, 158);
            this.nudEncumbrancePenaltyMovementSpeed.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudEncumbrancePenaltyMovementSpeed.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudEncumbrancePenaltyMovementSpeed.Name = "nudEncumbrancePenaltyMovementSpeed";
            this.nudEncumbrancePenaltyMovementSpeed.Size = new System.Drawing.Size(41, 20);
            this.nudEncumbrancePenaltyMovementSpeed.TabIndex = 64;
            this.nudEncumbrancePenaltyMovementSpeed.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblEncumbrancePenaltyMovementSpeedPercent
            // 
            this.lblEncumbrancePenaltyMovementSpeedPercent.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblEncumbrancePenaltyMovementSpeedPercent.AutoSize = true;
            this.lblEncumbrancePenaltyMovementSpeedPercent.Location = new System.Drawing.Point(281, 161);
            this.lblEncumbrancePenaltyMovementSpeedPercent.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblEncumbrancePenaltyMovementSpeedPercent.Name = "lblEncumbrancePenaltyMovementSpeedPercent";
            this.lblEncumbrancePenaltyMovementSpeedPercent.Size = new System.Drawing.Size(15, 13);
            this.lblEncumbrancePenaltyMovementSpeedPercent.TabIndex = 65;
            this.lblEncumbrancePenaltyMovementSpeedPercent.Tag = "";
            this.lblEncumbrancePenaltyMovementSpeedPercent.Text = "%";
            this.lblEncumbrancePenaltyMovementSpeedPercent.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblEncumbrancePenaltyMovementSpeedPercent.ToolTipText = "";
            // 
            // nudWeightDecimals
            // 
            this.nudWeightDecimals.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudWeightDecimals.AutoSize = true;
            this.nudWeightDecimals.Location = new System.Drawing.Point(234, 3);
            this.nudWeightDecimals.Maximum = new decimal(new int[] {
            28,
            0,
            0,
            0});
            this.nudWeightDecimals.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudWeightDecimals.Name = "nudWeightDecimals";
            this.nudWeightDecimals.Size = new System.Drawing.Size(35, 20);
            this.nudWeightDecimals.TabIndex = 66;
            this.nudWeightDecimals.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // gpbBasicOptionsCyberlimbs
            // 
            this.gpbBasicOptionsCyberlimbs.AutoSize = true;
            this.gpbBasicOptionsCyberlimbs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbBasicOptionsCyberlimbs.Controls.Add(this.tlpBasicOptionsCyberlimbs);
            this.gpbBasicOptionsCyberlimbs.Location = new System.Drawing.Point(3, 521);
            this.gpbBasicOptionsCyberlimbs.Name = "gpbBasicOptionsCyberlimbs";
            this.gpbBasicOptionsCyberlimbs.Size = new System.Drawing.Size(385, 147);
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
            this.tlpBasicOptionsCyberlimbs.Size = new System.Drawing.Size(379, 128);
            this.tlpBasicOptionsCyberlimbs.TabIndex = 0;
            // 
            // lblRedlinerLimbs
            // 
            this.lblRedlinerLimbs.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblRedlinerLimbs.AutoSize = true;
            this.lblRedlinerLimbs.Location = new System.Drawing.Point(3, 109);
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
            this.chkCyberlegMovement.Location = new System.Drawing.Point(3, 56);
            this.chkCyberlegMovement.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.chkDontUseCyberlimbCalculation.Location = new System.Drawing.Point(3, 31);
            this.chkDontUseCyberlimbCalculation.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.tlpCyberlimbAttributeBonusCap.Location = new System.Drawing.Point(0, 77);
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
            this.flpCyberlimbAttributeBonusCap.Location = new System.Drawing.Point(0, 0);
            this.flpCyberlimbAttributeBonusCap.Margin = new System.Windows.Forms.Padding(0);
            this.flpCyberlimbAttributeBonusCap.Name = "flpCyberlimbAttributeBonusCap";
            this.flpCyberlimbAttributeBonusCap.Size = new System.Drawing.Size(259, 25);
            this.flpCyberlimbAttributeBonusCap.TabIndex = 51;
            this.flpCyberlimbAttributeBonusCap.WrapContents = false;
            // 
            // chkCyberlimbAttributeBonusCap
            // 
            this.chkCyberlimbAttributeBonusCap.AutoSize = true;
            this.chkCyberlimbAttributeBonusCap.DefaultColorScheme = true;
            this.chkCyberlimbAttributeBonusCap.Dock = System.Windows.Forms.DockStyle.Left;
            this.chkCyberlimbAttributeBonusCap.Location = new System.Drawing.Point(3, 4);
            this.chkCyberlimbAttributeBonusCap.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.lblCyberlimbAttributeBonusCapPlus.Location = new System.Drawing.Point(243, 4);
            this.lblCyberlimbAttributeBonusCapPlus.Margin = new System.Windows.Forms.Padding(3, 4, 3, 8);
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
            this.flpRedlinerLimbs.Location = new System.Drawing.Point(155, 103);
            this.flpRedlinerLimbs.Margin = new System.Windows.Forms.Padding(0);
            this.flpRedlinerLimbs.Name = "flpRedlinerLimbs";
            this.flpRedlinerLimbs.Size = new System.Drawing.Size(224, 25);
            this.flpRedlinerLimbs.TabIndex = 54;
            // 
            // chkRedlinerLimbsSkull
            // 
            this.chkRedlinerLimbsSkull.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkRedlinerLimbsSkull.AutoSize = true;
            this.chkRedlinerLimbsSkull.DefaultColorScheme = true;
            this.chkRedlinerLimbsSkull.Location = new System.Drawing.Point(3, 4);
            this.chkRedlinerLimbsSkull.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.chkRedlinerLimbsTorso.Location = new System.Drawing.Point(58, 4);
            this.chkRedlinerLimbsTorso.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.chkRedlinerLimbsArms.Location = new System.Drawing.Point(117, 4);
            this.chkRedlinerLimbsArms.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.chkRedlinerLimbsLegs.Location = new System.Drawing.Point(172, 4);
            this.chkRedlinerLimbsLegs.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.gpbBasicOptionsRounding.Location = new System.Drawing.Point(394, 521);
            this.gpbBasicOptionsRounding.Name = "gpbBasicOptionsRounding";
            this.gpbBasicOptionsRounding.Size = new System.Drawing.Size(361, 122);
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
            this.tlpBasicOptionsRounding.Size = new System.Drawing.Size(355, 103);
            this.tlpBasicOptionsRounding.TabIndex = 0;
            // 
            // chkDontRoundEssenceInternally
            // 
            this.chkDontRoundEssenceInternally.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkDontRoundEssenceInternally.AutoSize = true;
            this.tlpBasicOptionsRounding.SetColumnSpan(this.chkDontRoundEssenceInternally, 2);
            this.chkDontRoundEssenceInternally.DefaultColorScheme = true;
            this.chkDontRoundEssenceInternally.Location = new System.Drawing.Point(3, 82);
            this.chkDontRoundEssenceInternally.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            // gpbBasicOptionsInitiativeDice
            // 
            this.gpbBasicOptionsInitiativeDice.AutoSize = true;
            this.gpbBasicOptionsInitiativeDice.Controls.Add(this.tlpBasicOptionsInitiativeDice);
            this.gpbBasicOptionsInitiativeDice.Location = new System.Drawing.Point(3, 674);
            this.gpbBasicOptionsInitiativeDice.Name = "gpbBasicOptionsInitiativeDice";
            this.gpbBasicOptionsInitiativeDice.Size = new System.Drawing.Size(239, 148);
            this.gpbBasicOptionsInitiativeDice.TabIndex = 6;
            this.gpbBasicOptionsInitiativeDice.TabStop = false;
            this.gpbBasicOptionsInitiativeDice.Text = "Initiative Dice";
            // 
            // tlpBasicOptionsInitiativeDice
            // 
            this.tlpBasicOptionsInitiativeDice.AutoSize = true;
            this.tlpBasicOptionsInitiativeDice.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpBasicOptionsInitiativeDice.ColumnCount = 3;
            this.tlpBasicOptionsInitiativeDice.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBasicOptionsInitiativeDice.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBasicOptionsInitiativeDice.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBasicOptionsInitiativeDice.Controls.Add(this.lblMinInitiativeDiceLabel, 1, 0);
            this.tlpBasicOptionsInitiativeDice.Controls.Add(this.lblHotSimInitiativeDiceLabel, 0, 4);
            this.tlpBasicOptionsInitiativeDice.Controls.Add(this.lblAstralInitiativeDiceLabel, 0, 2);
            this.tlpBasicOptionsInitiativeDice.Controls.Add(this.lblColdSimInitiativeDiceLabel, 0, 3);
            this.tlpBasicOptionsInitiativeDice.Controls.Add(this.nudMinInitiativeDice, 1, 1);
            this.tlpBasicOptionsInitiativeDice.Controls.Add(this.nudMaxInitiativeDice, 2, 1);
            this.tlpBasicOptionsInitiativeDice.Controls.Add(this.lblInitiativeDiceLabel, 0, 1);
            this.tlpBasicOptionsInitiativeDice.Controls.Add(this.lblMaxInitiativeDiceLabel, 2, 0);
            this.tlpBasicOptionsInitiativeDice.Controls.Add(this.nudMaxAstralInitiativeDice, 2, 2);
            this.tlpBasicOptionsInitiativeDice.Controls.Add(this.nudMaxColdSimInitiativeDice, 2, 3);
            this.tlpBasicOptionsInitiativeDice.Controls.Add(this.nudMaxHotSimInitiativeDice, 2, 4);
            this.tlpBasicOptionsInitiativeDice.Controls.Add(this.nudMinAstralInitiativeDice, 1, 2);
            this.tlpBasicOptionsInitiativeDice.Controls.Add(this.nudMinColdSimInitiativeDice, 1, 3);
            this.tlpBasicOptionsInitiativeDice.Controls.Add(this.nudMinHotSimInitiativeDice, 1, 4);
            this.tlpBasicOptionsInitiativeDice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBasicOptionsInitiativeDice.Location = new System.Drawing.Point(3, 16);
            this.tlpBasicOptionsInitiativeDice.Name = "tlpBasicOptionsInitiativeDice";
            this.tlpBasicOptionsInitiativeDice.RowCount = 5;
            this.tlpBasicOptionsInitiativeDice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBasicOptionsInitiativeDice.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsInitiativeDice.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsInitiativeDice.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsInitiativeDice.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBasicOptionsInitiativeDice.Size = new System.Drawing.Size(233, 129);
            this.tlpBasicOptionsInitiativeDice.TabIndex = 0;
            // 
            // lblMinInitiativeDiceLabel
            // 
            this.lblMinInitiativeDiceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblMinInitiativeDiceLabel.AutoSize = true;
            this.lblMinInitiativeDiceLabel.Location = new System.Drawing.Point(119, 6);
            this.lblMinInitiativeDiceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMinInitiativeDiceLabel.Name = "lblMinInitiativeDiceLabel";
            this.lblMinInitiativeDiceLabel.Size = new System.Drawing.Size(51, 13);
            this.lblMinInitiativeDiceLabel.TabIndex = 19;
            this.lblMinInitiativeDiceLabel.Tag = "Label_CreateImprovementMinimum";
            this.lblMinInitiativeDiceLabel.Text = "Minimum:";
            this.lblMinInitiativeDiceLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // lblHotSimInitiativeDiceLabel
            // 
            this.lblHotSimInitiativeDiceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblHotSimInitiativeDiceLabel.AutoSize = true;
            this.lblHotSimInitiativeDiceLabel.Location = new System.Drawing.Point(7, 109);
            this.lblHotSimInitiativeDiceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblHotSimInitiativeDiceLabel.Name = "lblHotSimInitiativeDiceLabel";
            this.lblHotSimInitiativeDiceLabel.Size = new System.Drawing.Size(106, 13);
            this.lblHotSimInitiativeDiceLabel.TabIndex = 16;
            this.lblHotSimInitiativeDiceLabel.Tag = "Label_OtherMatrixInitVRHot";
            this.lblHotSimInitiativeDiceLabel.Text = "Matrix Initiative (Hot):";
            this.lblHotSimInitiativeDiceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblAstralInitiativeDiceLabel
            // 
            this.lblAstralInitiativeDiceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblAstralInitiativeDiceLabel.AutoSize = true;
            this.lblAstralInitiativeDiceLabel.Location = new System.Drawing.Point(35, 57);
            this.lblAstralInitiativeDiceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAstralInitiativeDiceLabel.Name = "lblAstralInitiativeDiceLabel";
            this.lblAstralInitiativeDiceLabel.Size = new System.Drawing.Size(78, 13);
            this.lblAstralInitiativeDiceLabel.TabIndex = 14;
            this.lblAstralInitiativeDiceLabel.Tag = "Label_OtherAstralInit";
            this.lblAstralInitiativeDiceLabel.Text = "Astral Initiative:";
            this.lblAstralInitiativeDiceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblColdSimInitiativeDiceLabel
            // 
            this.lblColdSimInitiativeDiceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblColdSimInitiativeDiceLabel.AutoSize = true;
            this.lblColdSimInitiativeDiceLabel.Location = new System.Drawing.Point(3, 83);
            this.lblColdSimInitiativeDiceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblColdSimInitiativeDiceLabel.Name = "lblColdSimInitiativeDiceLabel";
            this.lblColdSimInitiativeDiceLabel.Size = new System.Drawing.Size(110, 13);
            this.lblColdSimInitiativeDiceLabel.TabIndex = 15;
            this.lblColdSimInitiativeDiceLabel.Tag = "Label_OtherMatrixInitVRCold";
            this.lblColdSimInitiativeDiceLabel.Text = "Matrix Initiative (Cold):";
            this.lblColdSimInitiativeDiceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudMinInitiativeDice
            // 
            this.nudMinInitiativeDice.AutoSize = true;
            this.nudMinInitiativeDice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudMinInitiativeDice.Location = new System.Drawing.Point(119, 28);
            this.nudMinInitiativeDice.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudMinInitiativeDice.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMinInitiativeDice.Name = "nudMinInitiativeDice";
            this.nudMinInitiativeDice.Size = new System.Drawing.Size(51, 20);
            this.nudMinInitiativeDice.TabIndex = 17;
            this.nudMinInitiativeDice.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // nudMaxInitiativeDice
            // 
            this.nudMaxInitiativeDice.AutoSize = true;
            this.nudMaxInitiativeDice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudMaxInitiativeDice.Location = new System.Drawing.Point(176, 28);
            this.nudMaxInitiativeDice.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudMaxInitiativeDice.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMaxInitiativeDice.Name = "nudMaxInitiativeDice";
            this.nudMaxInitiativeDice.Size = new System.Drawing.Size(54, 20);
            this.nudMaxInitiativeDice.TabIndex = 18;
            this.nudMaxInitiativeDice.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // lblInitiativeDiceLabel
            // 
            this.lblInitiativeDiceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblInitiativeDiceLabel.AutoSize = true;
            this.lblInitiativeDiceLabel.Location = new System.Drawing.Point(64, 31);
            this.lblInitiativeDiceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblInitiativeDiceLabel.Name = "lblInitiativeDiceLabel";
            this.lblInitiativeDiceLabel.Size = new System.Drawing.Size(49, 13);
            this.lblInitiativeDiceLabel.TabIndex = 13;
            this.lblInitiativeDiceLabel.Tag = "Label_OtherInit";
            this.lblInitiativeDiceLabel.Text = "Initiative:";
            this.lblInitiativeDiceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblMaxInitiativeDiceLabel
            // 
            this.lblMaxInitiativeDiceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblMaxInitiativeDiceLabel.AutoSize = true;
            this.lblMaxInitiativeDiceLabel.Location = new System.Drawing.Point(176, 6);
            this.lblMaxInitiativeDiceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMaxInitiativeDiceLabel.Name = "lblMaxInitiativeDiceLabel";
            this.lblMaxInitiativeDiceLabel.Size = new System.Drawing.Size(54, 13);
            this.lblMaxInitiativeDiceLabel.TabIndex = 20;
            this.lblMaxInitiativeDiceLabel.Tag = "Label_CreateImprovementMaximum";
            this.lblMaxInitiativeDiceLabel.Text = "Maximum:";
            this.lblMaxInitiativeDiceLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // nudMaxAstralInitiativeDice
            // 
            this.nudMaxAstralInitiativeDice.AutoSize = true;
            this.nudMaxAstralInitiativeDice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudMaxAstralInitiativeDice.Location = new System.Drawing.Point(176, 54);
            this.nudMaxAstralInitiativeDice.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudMaxAstralInitiativeDice.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMaxAstralInitiativeDice.Name = "nudMaxAstralInitiativeDice";
            this.nudMaxAstralInitiativeDice.Size = new System.Drawing.Size(54, 20);
            this.nudMaxAstralInitiativeDice.TabIndex = 21;
            this.nudMaxAstralInitiativeDice.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // nudMaxColdSimInitiativeDice
            // 
            this.nudMaxColdSimInitiativeDice.AutoSize = true;
            this.nudMaxColdSimInitiativeDice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudMaxColdSimInitiativeDice.Location = new System.Drawing.Point(176, 80);
            this.nudMaxColdSimInitiativeDice.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudMaxColdSimInitiativeDice.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMaxColdSimInitiativeDice.Name = "nudMaxColdSimInitiativeDice";
            this.nudMaxColdSimInitiativeDice.Size = new System.Drawing.Size(54, 20);
            this.nudMaxColdSimInitiativeDice.TabIndex = 22;
            this.nudMaxColdSimInitiativeDice.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // nudMaxHotSimInitiativeDice
            // 
            this.nudMaxHotSimInitiativeDice.AutoSize = true;
            this.nudMaxHotSimInitiativeDice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudMaxHotSimInitiativeDice.Location = new System.Drawing.Point(176, 106);
            this.nudMaxHotSimInitiativeDice.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudMaxHotSimInitiativeDice.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMaxHotSimInitiativeDice.Name = "nudMaxHotSimInitiativeDice";
            this.nudMaxHotSimInitiativeDice.Size = new System.Drawing.Size(54, 20);
            this.nudMaxHotSimInitiativeDice.TabIndex = 23;
            this.nudMaxHotSimInitiativeDice.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // nudMinAstralInitiativeDice
            // 
            this.nudMinAstralInitiativeDice.AutoSize = true;
            this.nudMinAstralInitiativeDice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudMinAstralInitiativeDice.Location = new System.Drawing.Point(119, 54);
            this.nudMinAstralInitiativeDice.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudMinAstralInitiativeDice.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMinAstralInitiativeDice.Name = "nudMinAstralInitiativeDice";
            this.nudMinAstralInitiativeDice.Size = new System.Drawing.Size(51, 20);
            this.nudMinAstralInitiativeDice.TabIndex = 24;
            this.nudMinAstralInitiativeDice.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // nudMinColdSimInitiativeDice
            // 
            this.nudMinColdSimInitiativeDice.AutoSize = true;
            this.nudMinColdSimInitiativeDice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudMinColdSimInitiativeDice.Location = new System.Drawing.Point(119, 80);
            this.nudMinColdSimInitiativeDice.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudMinColdSimInitiativeDice.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMinColdSimInitiativeDice.Name = "nudMinColdSimInitiativeDice";
            this.nudMinColdSimInitiativeDice.Size = new System.Drawing.Size(51, 20);
            this.nudMinColdSimInitiativeDice.TabIndex = 25;
            this.nudMinColdSimInitiativeDice.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // nudMinHotSimInitiativeDice
            // 
            this.nudMinHotSimInitiativeDice.AutoSize = true;
            this.nudMinHotSimInitiativeDice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudMinHotSimInitiativeDice.Location = new System.Drawing.Point(119, 106);
            this.nudMinHotSimInitiativeDice.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudMinHotSimInitiativeDice.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMinHotSimInitiativeDice.Name = "nudMinHotSimInitiativeDice";
            this.nudMinHotSimInitiativeDice.Size = new System.Drawing.Size(51, 20);
            this.nudMinHotSimInitiativeDice.TabIndex = 26;
            this.nudMinHotSimInitiativeDice.Value = new decimal(new int[] {
            4,
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
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaSpirit, 3, 3);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaSpirit, 4, 3);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaSpiritExtra, 5, 3);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaJoinGroup, 3, 0);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaJoinGroup, 4, 0);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaCarryover, 0, 9);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaCarryover, 1, 9);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaCarryoverExtra, 2, 9);
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
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaTechnique, 3, 11);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaMetamagic, 3, 10);
            this.tlpKarmaCosts.Controls.Add(this.flpKarmaInitiation, 3, 9);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaNewAIAdvancedProgram, 3, 8);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaNewAIProgram, 3, 7);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaNewComplexForm, 3, 6);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaMysticAdeptPowerPoint, 3, 5);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaTechnique, 4, 11);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaMetamagic, 4, 10);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaInitiation, 4, 9);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaInitiationExtra, 5, 9);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaInitiationFlat, 6, 9);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaNewAIAdvancedProgram, 4, 8);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaNewAIProgram, 4, 7);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaNewComplexForm, 4, 6);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaMysticAdeptPowerPoint, 4, 5);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaSpiritFettering, 3, 4);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaSpiritFettering, 4, 4);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaSpiritFetteringExtra, 5, 4);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaQuality, 0, 11);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaQuality, 1, 11);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaQualityExtra, 2, 11);
            this.tlpKarmaCosts.Controls.Add(this.lblMetatypeCostsKarmaMultiplierLabel, 0, 12);
            this.tlpKarmaCosts.Controls.Add(this.nudMetatypeCostsKarmaMultiplier, 1, 12);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaNuyenPerWftM, 0, 13);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaNuyenPerWftM, 1, 13);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaNuyenPerExtraWftM, 2, 13);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaNuyenPerWftP, 0, 14);
            this.tlpKarmaCosts.Controls.Add(this.nudKarmaNuyenPerWftP, 1, 14);
            this.tlpKarmaCosts.Controls.Add(this.lblKarmaNuyenPerExtraWftP, 2, 14);
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
            this.lblKarmaSpecialization.Location = new System.Drawing.Point(32, 6);
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
            this.nudKarmaSpecialization.Location = new System.Drawing.Point(190, 3);
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
            this.nudKarmaSpecialization.Size = new System.Drawing.Size(53, 20);
            this.nudKarmaSpecialization.TabIndex = 1;
            // 
            // lblKarmaKnowledgeSpecialization
            // 
            this.lblKarmaKnowledgeSpecialization.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaKnowledgeSpecialization.AutoSize = true;
            this.lblKarmaKnowledgeSpecialization.Location = new System.Drawing.Point(9, 32);
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
            this.nudKarmaKnowledgeSpecialization.Location = new System.Drawing.Point(190, 29);
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
            this.nudKarmaKnowledgeSpecialization.Size = new System.Drawing.Size(53, 20);
            this.nudKarmaKnowledgeSpecialization.TabIndex = 120;
            // 
            // lblKarmaNewKnowledgeSkill
            // 
            this.lblKarmaNewKnowledgeSkill.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaNewKnowledgeSkill.AutoSize = true;
            this.lblKarmaNewKnowledgeSkill.Location = new System.Drawing.Point(77, 58);
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
            this.nudKarmaNewKnowledgeSkill.Location = new System.Drawing.Point(190, 55);
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
            this.nudKarmaNewKnowledgeSkill.Size = new System.Drawing.Size(53, 20);
            this.nudKarmaNewKnowledgeSkill.TabIndex = 3;
            // 
            // lblKarmaLeaveGroup
            // 
            this.lblKarmaLeaveGroup.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaLeaveGroup.AutoSize = true;
            this.lblKarmaLeaveGroup.Location = new System.Drawing.Point(415, 32);
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
            this.nudKarmaLeaveGroup.Location = new System.Drawing.Point(535, 29);
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
            this.lblKarmaNewActiveSkill.Location = new System.Drawing.Point(100, 84);
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
            this.nudKarmaNewActiveSkill.Location = new System.Drawing.Point(190, 81);
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
            this.nudKarmaNewActiveSkill.Size = new System.Drawing.Size(53, 20);
            this.nudKarmaNewActiveSkill.TabIndex = 5;
            // 
            // lblKarmaNewSkillGroup
            // 
            this.lblKarmaNewSkillGroup.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaNewSkillGroup.AutoSize = true;
            this.lblKarmaNewSkillGroup.Location = new System.Drawing.Point(101, 110);
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
            this.nudKarmaNewSkillGroup.Location = new System.Drawing.Point(190, 107);
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
            this.nudKarmaNewSkillGroup.Size = new System.Drawing.Size(53, 20);
            this.nudKarmaNewSkillGroup.TabIndex = 7;
            // 
            // lblKarmaImproveKnowledgeSkill
            // 
            this.lblKarmaImproveKnowledgeSkill.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaImproveKnowledgeSkill.AutoSize = true;
            this.lblKarmaImproveKnowledgeSkill.Location = new System.Drawing.Point(38, 136);
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
            this.nudKarmaImproveKnowledgeSkill.Location = new System.Drawing.Point(190, 133);
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
            this.nudKarmaImproveKnowledgeSkill.Size = new System.Drawing.Size(53, 20);
            this.nudKarmaImproveKnowledgeSkill.TabIndex = 9;
            // 
            // lblKarmaImproveKnowledgeSkillExtra
            // 
            this.lblKarmaImproveKnowledgeSkillExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaImproveKnowledgeSkillExtra.AutoSize = true;
            this.lblKarmaImproveKnowledgeSkillExtra.Location = new System.Drawing.Point(249, 136);
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
            this.lblKarmaImproveActiveSkill.Location = new System.Drawing.Point(61, 162);
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
            this.nudKarmaImproveActiveSkill.Location = new System.Drawing.Point(190, 159);
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
            this.nudKarmaImproveActiveSkill.Size = new System.Drawing.Size(53, 20);
            this.nudKarmaImproveActiveSkill.TabIndex = 12;
            // 
            // lblKarmaImproveActiveSkillExtra
            // 
            this.lblKarmaImproveActiveSkillExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaImproveActiveSkillExtra.AutoSize = true;
            this.lblKarmaImproveActiveSkillExtra.Location = new System.Drawing.Point(249, 162);
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
            this.lblKarmaImproveSkillGroup.Location = new System.Drawing.Point(62, 188);
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
            this.nudKarmaImproveSkillGroup.Location = new System.Drawing.Point(190, 185);
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
            this.nudKarmaImproveSkillGroup.Size = new System.Drawing.Size(53, 20);
            this.nudKarmaImproveSkillGroup.TabIndex = 15;
            // 
            // lblKarmaImproveSkillGroupExtra
            // 
            this.lblKarmaImproveSkillGroupExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaImproveSkillGroupExtra.AutoSize = true;
            this.lblKarmaImproveSkillGroupExtra.Location = new System.Drawing.Point(249, 188);
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
            this.lblKarmaAttribute.Location = new System.Drawing.Point(74, 214);
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
            this.nudKarmaAttribute.Location = new System.Drawing.Point(190, 211);
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
            this.nudKarmaAttribute.Size = new System.Drawing.Size(53, 20);
            this.nudKarmaAttribute.TabIndex = 18;
            // 
            // lblKarmaAttributeExtra
            // 
            this.lblKarmaAttributeExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaAttributeExtra.AutoSize = true;
            this.lblKarmaAttributeExtra.Location = new System.Drawing.Point(249, 214);
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
            this.lblKarmaAlchemicalFocus.Location = new System.Drawing.Point(754, 6);
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
            this.nudKarmaAlchemicalFocus.Location = new System.Drawing.Point(850, 3);
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
            this.lblKarmaAlchemicalFocusExtra.Location = new System.Drawing.Point(897, 6);
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
            this.lblKarmaBanishingFocus.Location = new System.Drawing.Point(759, 32);
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
            this.nudKarmaBanishingFocus.Location = new System.Drawing.Point(850, 29);
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
            this.lblKarmaBindingFocus.Location = new System.Drawing.Point(770, 58);
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
            this.nudKarmaBindingFocus.Location = new System.Drawing.Point(850, 55);
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
            this.lblKarmaBanishingFocusExtra.Location = new System.Drawing.Point(897, 32);
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
            this.lblKarmaCenteringFocus.Location = new System.Drawing.Point(760, 84);
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
            this.nudKarmaCenteringFocus.Location = new System.Drawing.Point(850, 81);
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
            this.lblKarmaBindingFocusExtra.Location = new System.Drawing.Point(897, 58);
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
            this.lblKarmaCenteringFocusExtra.Location = new System.Drawing.Point(897, 84);
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
            this.lblKarmaCounterspellingFocus.Location = new System.Drawing.Point(733, 110);
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
            this.nudKarmaCounterspellingFocus.Location = new System.Drawing.Point(850, 107);
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
            this.lblKarmaCounterspellingFocusExtra.Location = new System.Drawing.Point(897, 110);
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
            this.lblKarmaDisenchantingFocus.Location = new System.Drawing.Point(737, 136);
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
            this.nudKarmaDisenchantingFocus.Location = new System.Drawing.Point(850, 133);
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
            this.lblKarmaDisenchantingFocusExtra.Location = new System.Drawing.Point(897, 136);
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
            this.lblKarmaFlexibleSignatureFocus.Location = new System.Drawing.Point(722, 162);
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
            this.nudKarmaFlexibleSignatureFocus.Location = new System.Drawing.Point(850, 159);
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
            this.lblFlexibleSignatureFocusExtra.Location = new System.Drawing.Point(897, 162);
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
            this.lblKarmaMaskingFocus.Location = new System.Drawing.Point(765, 188);
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
            this.nudKarmaMaskingFocus.Location = new System.Drawing.Point(850, 185);
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
            this.lblKarmaMaskingFocusExtra.Location = new System.Drawing.Point(897, 188);
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
            this.lblKarmaPowerFocus.Location = new System.Drawing.Point(775, 214);
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
            this.nudKarmaPowerFocus.Location = new System.Drawing.Point(850, 211);
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
            this.lblKarmaPowerFocusExtra.Location = new System.Drawing.Point(897, 214);
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
            this.lblKarmaQiFocus.Location = new System.Drawing.Point(795, 240);
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
            this.nudKarmaQiFocus.Location = new System.Drawing.Point(850, 237);
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
            this.lblKarmaQiFocusExtra.Location = new System.Drawing.Point(897, 240);
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
            this.lblKarmaRitualSpellcastingFocus.Location = new System.Drawing.Point(718, 266);
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
            this.nudKarmaRitualSpellcastingFocus.Location = new System.Drawing.Point(850, 263);
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
            this.lblKarmaRitualSpellcastingFocusExtra.Location = new System.Drawing.Point(897, 266);
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
            this.lblKarmaSpellcastingFocus.Location = new System.Drawing.Point(748, 292);
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
            this.nudKarmaSpellcastingFocus.Location = new System.Drawing.Point(850, 289);
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
            this.lblKarmaSpellcastingFocusExtra.Location = new System.Drawing.Point(897, 292);
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
            this.lblKarmaSpell.Location = new System.Drawing.Point(474, 58);
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
            this.nudKarmaSpell.Location = new System.Drawing.Point(535, 55);
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
            this.lblKarmaContact.Location = new System.Drawing.Point(135, 266);
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
            this.nudKarmaContact.Location = new System.Drawing.Point(190, 263);
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
            this.nudKarmaContact.Size = new System.Drawing.Size(53, 20);
            this.nudKarmaContact.TabIndex = 45;
            // 
            // lblKarmaContactExtra
            // 
            this.lblKarmaContactExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaContactExtra.AutoSize = true;
            this.lblKarmaContactExtra.Location = new System.Drawing.Point(249, 266);
            this.lblKarmaContactExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaContactExtra.Name = "lblKarmaContactExtra";
            this.lblKarmaContactExtra.Size = new System.Drawing.Size(120, 13);
            this.lblKarmaContactExtra.TabIndex = 46;
            this.lblKarmaContactExtra.Tag = "Label_Options_ConnectionLoyalty";
            this.lblKarmaContactExtra.Text = "x (Connection + Loyalty)";
            this.lblKarmaContactExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaSpirit
            // 
            this.lblKarmaSpirit.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaSpirit.AutoSize = true;
            this.lblKarmaSpirit.Location = new System.Drawing.Point(499, 84);
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
            this.nudKarmaSpirit.Location = new System.Drawing.Point(535, 81);
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
            this.lblKarmaSpiritExtra.Location = new System.Drawing.Point(582, 84);
            this.lblKarmaSpiritExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSpiritExtra.Name = "lblKarmaSpiritExtra";
            this.lblKarmaSpiritExtra.Size = new System.Drawing.Size(87, 13);
            this.lblKarmaSpiritExtra.TabIndex = 38;
            this.lblKarmaSpiritExtra.Tag = "Label_Options_ServicesOwed";
            this.lblKarmaSpiritExtra.Text = "x Services Owed";
            this.lblKarmaSpiritExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaJoinGroup
            // 
            this.lblKarmaJoinGroup.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaJoinGroup.AutoSize = true;
            this.lblKarmaJoinGroup.Location = new System.Drawing.Point(426, 6);
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
            this.nudKarmaJoinGroup.Location = new System.Drawing.Point(535, 3);
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
            // lblKarmaCarryover
            // 
            this.lblKarmaCarryover.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaCarryover.AutoSize = true;
            this.lblKarmaCarryover.Location = new System.Drawing.Point(43, 240);
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
            this.nudKarmaCarryover.Location = new System.Drawing.Point(190, 237);
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
            this.nudKarmaCarryover.Size = new System.Drawing.Size(53, 20);
            this.nudKarmaCarryover.TabIndex = 51;
            // 
            // lblKarmaCarryoverExtra
            // 
            this.lblKarmaCarryoverExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaCarryoverExtra.AutoSize = true;
            this.lblKarmaCarryoverExtra.Location = new System.Drawing.Point(249, 240);
            this.lblKarmaCarryoverExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaCarryoverExtra.Name = "lblKarmaCarryoverExtra";
            this.lblKarmaCarryoverExtra.Size = new System.Drawing.Size(51, 13);
            this.lblKarmaCarryoverExtra.TabIndex = 52;
            this.lblKarmaCarryoverExtra.Tag = "Label_Options_Maximum";
            this.lblKarmaCarryoverExtra.Text = "Maximum";
            this.lblKarmaCarryoverExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaSummoningFocus
            // 
            this.lblKarmaSummoningFocus.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaSummoningFocus.AutoSize = true;
            this.lblKarmaSummoningFocus.Location = new System.Drawing.Point(750, 344);
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
            this.nudKarmaSummoningFocus.Location = new System.Drawing.Point(850, 341);
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
            this.lblKarmaSpellShapingFocus.Location = new System.Drawing.Point(740, 318);
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
            this.lblKarmaSpellShapingFocusExtra.Location = new System.Drawing.Point(897, 318);
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
            this.nudKarmaSpellShapingFocus.Location = new System.Drawing.Point(850, 315);
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
            this.lblKarmaSummoningFocusExtra.Location = new System.Drawing.Point(897, 344);
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
            this.lblKarmaSustainingFocus.Location = new System.Drawing.Point(756, 370);
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
            this.lblKarmaWeaponFocus.Location = new System.Drawing.Point(764, 396);
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
            this.lblKarmaSustainingFocusExtra.Location = new System.Drawing.Point(897, 370);
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
            this.lblKarmaWeaponFocusExtra.Location = new System.Drawing.Point(897, 396);
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
            this.nudKarmaSustainingFocus.Location = new System.Drawing.Point(850, 367);
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
            this.nudKarmaWeaponFocus.Location = new System.Drawing.Point(850, 393);
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
            // lblKarmaTechnique
            // 
            this.lblKarmaTechnique.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaTechnique.AutoSize = true;
            this.lblKarmaTechnique.Location = new System.Drawing.Point(421, 292);
            this.lblKarmaTechnique.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaTechnique.Name = "lblKarmaTechnique";
            this.lblKarmaTechnique.Size = new System.Drawing.Size(108, 13);
            this.lblKarmaTechnique.TabIndex = 39;
            this.lblKarmaTechnique.Tag = "Label_Options_MartialArtTechnique";
            this.lblKarmaTechnique.Text = "Martial Art Technique";
            this.lblKarmaTechnique.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblKarmaMetamagic
            // 
            this.lblKarmaMetamagic.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaMetamagic.AutoSize = true;
            this.lblKarmaMetamagic.Location = new System.Drawing.Point(375, 266);
            this.lblKarmaMetamagic.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaMetamagic.Name = "lblKarmaMetamagic";
            this.lblKarmaMetamagic.Size = new System.Drawing.Size(154, 13);
            this.lblKarmaMetamagic.TabIndex = 57;
            this.lblKarmaMetamagic.Tag = "Label_Options_Metamagics";
            this.lblKarmaMetamagic.Text = "Additional Metamagics/Echoes";
            this.lblKarmaMetamagic.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // flpKarmaInitiation
            // 
            this.flpKarmaInitiation.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.flpKarmaInitiation.AutoSize = true;
            this.flpKarmaInitiation.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpKarmaInitiation.Controls.Add(this.lblKarmaInitiation);
            this.flpKarmaInitiation.Controls.Add(this.lblKarmaInitiationBracket);
            this.flpKarmaInitiation.Location = new System.Drawing.Point(398, 234);
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
            // lblKarmaNewAIAdvancedProgram
            // 
            this.lblKarmaNewAIAdvancedProgram.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaNewAIAdvancedProgram.AutoSize = true;
            this.lblKarmaNewAIAdvancedProgram.Location = new System.Drawing.Point(387, 214);
            this.lblKarmaNewAIAdvancedProgram.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaNewAIAdvancedProgram.Name = "lblKarmaNewAIAdvancedProgram";
            this.lblKarmaNewAIAdvancedProgram.Size = new System.Drawing.Size(142, 13);
            this.lblKarmaNewAIAdvancedProgram.TabIndex = 110;
            this.lblKarmaNewAIAdvancedProgram.Tag = "Label_Options_NewAIAdvancedProgram";
            this.lblKarmaNewAIAdvancedProgram.Text = "New Advanced Program (AI)";
            this.lblKarmaNewAIAdvancedProgram.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblKarmaNewAIProgram
            // 
            this.lblKarmaNewAIProgram.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaNewAIProgram.AutoSize = true;
            this.lblKarmaNewAIProgram.Location = new System.Drawing.Point(439, 188);
            this.lblKarmaNewAIProgram.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaNewAIProgram.Name = "lblKarmaNewAIProgram";
            this.lblKarmaNewAIProgram.Size = new System.Drawing.Size(90, 13);
            this.lblKarmaNewAIProgram.TabIndex = 109;
            this.lblKarmaNewAIProgram.Tag = "Label_Options_NewAIProgram";
            this.lblKarmaNewAIProgram.Text = "New Program (AI)";
            this.lblKarmaNewAIProgram.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblKarmaNewComplexForm
            // 
            this.lblKarmaNewComplexForm.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaNewComplexForm.AutoSize = true;
            this.lblKarmaNewComplexForm.Location = new System.Drawing.Point(431, 162);
            this.lblKarmaNewComplexForm.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaNewComplexForm.Name = "lblKarmaNewComplexForm";
            this.lblKarmaNewComplexForm.Size = new System.Drawing.Size(98, 13);
            this.lblKarmaNewComplexForm.TabIndex = 25;
            this.lblKarmaNewComplexForm.Tag = "Label_Options_NewComplexForm";
            this.lblKarmaNewComplexForm.Text = "New Complex Form";
            this.lblKarmaNewComplexForm.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblKarmaMysticAdeptPowerPoint
            // 
            this.lblKarmaMysticAdeptPowerPoint.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaMysticAdeptPowerPoint.AutoSize = true;
            this.lblKarmaMysticAdeptPowerPoint.Location = new System.Drawing.Point(401, 136);
            this.lblKarmaMysticAdeptPowerPoint.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaMysticAdeptPowerPoint.Name = "lblKarmaMysticAdeptPowerPoint";
            this.lblKarmaMysticAdeptPowerPoint.Size = new System.Drawing.Size(128, 13);
            this.lblKarmaMysticAdeptPowerPoint.TabIndex = 122;
            this.lblKarmaMysticAdeptPowerPoint.Tag = "Label_Options_KarmaMysticAdeptPowerPoint";
            this.lblKarmaMysticAdeptPowerPoint.Text = "Mystic Adept Power Point";
            this.lblKarmaMysticAdeptPowerPoint.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaTechnique
            // 
            this.nudKarmaTechnique.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaTechnique.AutoSize = true;
            this.nudKarmaTechnique.Location = new System.Drawing.Point(535, 289);
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
            // nudKarmaMetamagic
            // 
            this.nudKarmaMetamagic.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaMetamagic.AutoSize = true;
            this.nudKarmaMetamagic.Location = new System.Drawing.Point(535, 263);
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
            // nudKarmaInitiation
            // 
            this.nudKarmaInitiation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaInitiation.AutoSize = true;
            this.nudKarmaInitiation.Location = new System.Drawing.Point(535, 237);
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
            this.lblKarmaInitiationExtra.Location = new System.Drawing.Point(582, 240);
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
            this.nudKarmaInitiationFlat.Location = new System.Drawing.Point(671, 237);
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
            // nudKarmaNewAIAdvancedProgram
            // 
            this.nudKarmaNewAIAdvancedProgram.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaNewAIAdvancedProgram.AutoSize = true;
            this.nudKarmaNewAIAdvancedProgram.Location = new System.Drawing.Point(535, 211);
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
            // nudKarmaNewAIProgram
            // 
            this.nudKarmaNewAIProgram.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaNewAIProgram.AutoSize = true;
            this.nudKarmaNewAIProgram.Location = new System.Drawing.Point(535, 185);
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
            // nudKarmaNewComplexForm
            // 
            this.nudKarmaNewComplexForm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaNewComplexForm.AutoSize = true;
            this.nudKarmaNewComplexForm.Location = new System.Drawing.Point(535, 159);
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
            // nudKarmaMysticAdeptPowerPoint
            // 
            this.nudKarmaMysticAdeptPowerPoint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaMysticAdeptPowerPoint.AutoSize = true;
            this.nudKarmaMysticAdeptPowerPoint.Location = new System.Drawing.Point(535, 133);
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
            // lblKarmaSpiritFettering
            // 
            this.lblKarmaSpiritFettering.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaSpiritFettering.AutoSize = true;
            this.lblKarmaSpiritFettering.Location = new System.Drawing.Point(455, 110);
            this.lblKarmaSpiritFettering.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSpiritFettering.Name = "lblKarmaSpiritFettering";
            this.lblKarmaSpiritFettering.Size = new System.Drawing.Size(74, 13);
            this.lblKarmaSpiritFettering.TabIndex = 129;
            this.lblKarmaSpiritFettering.Tag = "Label_Options_SpiritFettering";
            this.lblKarmaSpiritFettering.Text = "Spirit Fettering";
            this.lblKarmaSpiritFettering.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaSpiritFettering
            // 
            this.nudKarmaSpiritFettering.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaSpiritFettering.AutoSize = true;
            this.nudKarmaSpiritFettering.Location = new System.Drawing.Point(535, 107);
            this.nudKarmaSpiritFettering.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaSpiritFettering.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaSpiritFettering.Name = "nudKarmaSpiritFettering";
            this.nudKarmaSpiritFettering.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaSpiritFettering.TabIndex = 130;
            // 
            // lblKarmaSpiritFetteringExtra
            // 
            this.lblKarmaSpiritFetteringExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaSpiritFetteringExtra.AutoSize = true;
            this.tlpKarmaCosts.SetColumnSpan(this.lblKarmaSpiritFetteringExtra, 2);
            this.lblKarmaSpiritFetteringExtra.Location = new System.Drawing.Point(582, 110);
            this.lblKarmaSpiritFetteringExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaSpiritFetteringExtra.Name = "lblKarmaSpiritFetteringExtra";
            this.lblKarmaSpiritFetteringExtra.Size = new System.Drawing.Size(42, 13);
            this.lblKarmaSpiritFetteringExtra.TabIndex = 131;
            this.lblKarmaSpiritFetteringExtra.Tag = "Label_Options_Force";
            this.lblKarmaSpiritFetteringExtra.Text = "x Force";
            this.lblKarmaSpiritFetteringExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaQuality
            // 
            this.lblKarmaQuality.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaQuality.AutoSize = true;
            this.lblKarmaQuality.Location = new System.Drawing.Point(51, 292);
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
            this.nudKarmaQuality.Location = new System.Drawing.Point(190, 289);
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
            this.nudKarmaQuality.Size = new System.Drawing.Size(53, 20);
            this.nudKarmaQuality.TabIndex = 21;
            // 
            // lblKarmaQualityExtra
            // 
            this.lblKarmaQualityExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaQualityExtra.AutoSize = true;
            this.lblKarmaQualityExtra.Location = new System.Drawing.Point(249, 292);
            this.lblKarmaQualityExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaQualityExtra.Name = "lblKarmaQualityExtra";
            this.lblKarmaQualityExtra.Size = new System.Drawing.Size(53, 13);
            this.lblKarmaQualityExtra.TabIndex = 22;
            this.lblKarmaQualityExtra.Tag = "Label_Options_BPCost";
            this.lblKarmaQualityExtra.Text = "x BP Cost";
            this.lblKarmaQualityExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMetatypeCostsKarmaMultiplierLabel
            // 
            this.lblMetatypeCostsKarmaMultiplierLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMetatypeCostsKarmaMultiplierLabel.AutoSize = true;
            this.lblMetatypeCostsKarmaMultiplierLabel.Location = new System.Drawing.Point(32, 318);
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
            this.nudMetatypeCostsKarmaMultiplier.Location = new System.Drawing.Point(190, 315);
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
            this.nudMetatypeCostsKarmaMultiplier.Size = new System.Drawing.Size(53, 20);
            this.nudMetatypeCostsKarmaMultiplier.TabIndex = 124;
            this.nudMetatypeCostsKarmaMultiplier.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblKarmaNuyenPerWftM
            // 
            this.lblKarmaNuyenPerWftM.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaNuyenPerWftM.AutoSize = true;
            this.lblKarmaNuyenPerWftM.Location = new System.Drawing.Point(9, 344);
            this.lblKarmaNuyenPerWftM.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaNuyenPerWftM.Name = "lblKarmaNuyenPerWftM";
            this.lblKarmaNuyenPerWftM.Size = new System.Drawing.Size(175, 13);
            this.lblKarmaNuyenPerWftM.TabIndex = 41;
            this.lblKarmaNuyenPerWftM.Tag = "Label_Options_WftM_Nuyen";
            this.lblKarmaNuyenPerWftM.Text = "Working for the Man Nuyen Gained";
            this.lblKarmaNuyenPerWftM.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaNuyenPerWftM
            // 
            this.nudKarmaNuyenPerWftM.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaNuyenPerWftM.AutoSize = true;
            this.nudKarmaNuyenPerWftM.Location = new System.Drawing.Point(190, 341);
            this.nudKarmaNuyenPerWftM.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.nudKarmaNuyenPerWftM.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaNuyenPerWftM.Name = "nudKarmaNuyenPerWftM";
            this.nudKarmaNuyenPerWftM.Size = new System.Drawing.Size(53, 20);
            this.nudKarmaNuyenPerWftM.TabIndex = 42;
            // 
            // lblKarmaNuyenPerExtraWftM
            // 
            this.lblKarmaNuyenPerExtraWftM.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaNuyenPerExtraWftM.AutoSize = true;
            this.lblKarmaNuyenPerExtraWftM.Location = new System.Drawing.Point(249, 344);
            this.lblKarmaNuyenPerExtraWftM.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaNuyenPerExtraWftM.Name = "lblKarmaNuyenPerExtraWftM";
            this.lblKarmaNuyenPerExtraWftM.Size = new System.Drawing.Size(86, 13);
            this.lblKarmaNuyenPerExtraWftM.TabIndex = 43;
            this.lblKarmaNuyenPerExtraWftM.Tag = "Label_Options_PerKarmaSpent";
            this.lblKarmaNuyenPerExtraWftM.Text = "per Karma Spent";
            this.lblKarmaNuyenPerExtraWftM.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarmaNuyenPerWftP
            // 
            this.lblKarmaNuyenPerWftP.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaNuyenPerWftP.AutoSize = true;
            this.lblKarmaNuyenPerWftP.Location = new System.Drawing.Point(3, 370);
            this.lblKarmaNuyenPerWftP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaNuyenPerWftP.Name = "lblKarmaNuyenPerWftP";
            this.lblKarmaNuyenPerWftP.Size = new System.Drawing.Size(181, 13);
            this.lblKarmaNuyenPerWftP.TabIndex = 133;
            this.lblKarmaNuyenPerWftP.Tag = "Label_Options_WftP_Nuyen";
            this.lblKarmaNuyenPerWftP.Text = "Working for the People Nuyen Spent";
            this.lblKarmaNuyenPerWftP.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaNuyenPerWftP
            // 
            this.nudKarmaNuyenPerWftP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaNuyenPerWftP.AutoSize = true;
            this.nudKarmaNuyenPerWftP.Location = new System.Drawing.Point(190, 367);
            this.nudKarmaNuyenPerWftP.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.nudKarmaNuyenPerWftP.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaNuyenPerWftP.Name = "nudKarmaNuyenPerWftP";
            this.nudKarmaNuyenPerWftP.Size = new System.Drawing.Size(53, 20);
            this.nudKarmaNuyenPerWftP.TabIndex = 134;
            // 
            // lblKarmaNuyenPerExtraWftP
            // 
            this.lblKarmaNuyenPerExtraWftP.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaNuyenPerExtraWftP.AutoSize = true;
            this.lblKarmaNuyenPerExtraWftP.Location = new System.Drawing.Point(249, 370);
            this.lblKarmaNuyenPerExtraWftP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaNuyenPerExtraWftP.Name = "lblKarmaNuyenPerExtraWftP";
            this.lblKarmaNuyenPerExtraWftP.Size = new System.Drawing.Size(92, 13);
            this.lblKarmaNuyenPerExtraWftP.TabIndex = 132;
            this.lblKarmaNuyenPerExtraWftP.Tag = "Label_Options_PerKarmaGained";
            this.lblKarmaNuyenPerExtraWftP.Text = "per Karma Gained";
            this.lblKarmaNuyenPerExtraWftP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.tlpOptionalRules.ColumnCount = 2;
            this.tlpOptionalRules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tlpOptionalRules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlpOptionalRules.Controls.Add(this.treCustomDataDirectories, 0, 1);
            this.tlpOptionalRules.Controls.Add(this.gpbDirectoryInfo, 1, 0);
            this.tlpOptionalRules.Controls.Add(this.lblCustomDataDirectoriesLabel, 0, 0);
            this.tlpOptionalRules.Controls.Add(this.tlpOptionalRulesButtons, 0, 2);
            this.tlpOptionalRules.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpOptionalRules.Location = new System.Drawing.Point(9, 9);
            this.tlpOptionalRules.Name = "tlpOptionalRules";
            this.tlpOptionalRules.RowCount = 3;
            this.tlpOptionalRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptionalRules.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpOptionalRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptionalRules.Size = new System.Drawing.Size(1214, 584);
            this.tlpOptionalRules.TabIndex = 44;
            // 
            // treCustomDataDirectories
            // 
            this.treCustomDataDirectories.CheckBoxes = true;
            this.treCustomDataDirectories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treCustomDataDirectories.Location = new System.Drawing.Point(3, 28);
            this.treCustomDataDirectories.Name = "treCustomDataDirectories";
            this.treCustomDataDirectories.ShowLines = false;
            this.treCustomDataDirectories.ShowPlusMinus = false;
            this.treCustomDataDirectories.ShowRootLines = false;
            this.treCustomDataDirectories.Size = new System.Drawing.Size(722, 524);
            this.treCustomDataDirectories.TabIndex = 40;
            this.treCustomDataDirectories.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treCustomDataDirectories_AfterCheck);
            this.treCustomDataDirectories.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treCustomDataDirectories_AfterSelect);
            // 
            // gpbDirectoryInfo
            // 
            this.gpbDirectoryInfo.AutoSize = true;
            this.gpbDirectoryInfo.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbDirectoryInfo.Controls.Add(this.tlpDirectoryInfo);
            this.gpbDirectoryInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpbDirectoryInfo.Location = new System.Drawing.Point(731, 3);
            this.gpbDirectoryInfo.Name = "gpbDirectoryInfo";
            this.tlpOptionalRules.SetRowSpan(this.gpbDirectoryInfo, 3);
            this.gpbDirectoryInfo.Size = new System.Drawing.Size(480, 578);
            this.gpbDirectoryInfo.TabIndex = 45;
            this.gpbDirectoryInfo.TabStop = false;
            this.gpbDirectoryInfo.Tag = "Title_CustomDataDirectoryInfo";
            this.gpbDirectoryInfo.Text = "Custom Data Directory Info";
            this.gpbDirectoryInfo.Visible = false;
            // 
            // tlpDirectoryInfo
            // 
            this.tlpDirectoryInfo.AutoSize = true;
            this.tlpDirectoryInfo.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpDirectoryInfo.ColumnCount = 2;
            this.tlpDirectoryInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpDirectoryInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpDirectoryInfo.Controls.Add(this.gbpDirectoryInfoDependencies, 0, 2);
            this.tlpDirectoryInfo.Controls.Add(this.gbpDirectoryInfoIncompatibilities, 1, 2);
            this.tlpDirectoryInfo.Controls.Add(this.tlpDirectoryInfoLeft, 0, 0);
            this.tlpDirectoryInfo.Controls.Add(this.gpbDirectoryAuthors, 1, 0);
            this.tlpDirectoryInfo.Controls.Add(this.rtbDirectoryDescription, 0, 1);
            this.tlpDirectoryInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpDirectoryInfo.Location = new System.Drawing.Point(3, 16);
            this.tlpDirectoryInfo.Name = "tlpDirectoryInfo";
            this.tlpDirectoryInfo.RowCount = 3;
            this.tlpDirectoryInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpDirectoryInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpDirectoryInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpDirectoryInfo.Size = new System.Drawing.Size(474, 559);
            this.tlpDirectoryInfo.TabIndex = 0;
            // 
            // gbpDirectoryInfoDependencies
            // 
            this.gbpDirectoryInfoDependencies.AutoSize = true;
            this.gbpDirectoryInfoDependencies.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gbpDirectoryInfoDependencies.Controls.Add(this.pnlDirectoryDependencies);
            this.gbpDirectoryInfoDependencies.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbpDirectoryInfoDependencies.Location = new System.Drawing.Point(3, 421);
            this.gbpDirectoryInfoDependencies.Name = "gbpDirectoryInfoDependencies";
            this.gbpDirectoryInfoDependencies.Size = new System.Drawing.Size(231, 135);
            this.gbpDirectoryInfoDependencies.TabIndex = 8;
            this.gbpDirectoryInfoDependencies.TabStop = false;
            this.gbpDirectoryInfoDependencies.Tag = "Title_DirectoryDependencies";
            this.gbpDirectoryInfoDependencies.Text = "Dependencies";
            // 
            // pnlDirectoryDependencies
            // 
            this.pnlDirectoryDependencies.AutoScroll = true;
            this.pnlDirectoryDependencies.Controls.Add(this.lblDependencies);
            this.pnlDirectoryDependencies.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDirectoryDependencies.Location = new System.Drawing.Point(3, 16);
            this.pnlDirectoryDependencies.Name = "pnlDirectoryDependencies";
            this.pnlDirectoryDependencies.Padding = new System.Windows.Forms.Padding(3, 6, 13, 6);
            this.pnlDirectoryDependencies.Size = new System.Drawing.Size(225, 116);
            this.pnlDirectoryDependencies.TabIndex = 1;
            // 
            // lblDependencies
            // 
            this.lblDependencies.AutoSize = true;
            this.lblDependencies.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDependencies.Location = new System.Drawing.Point(3, 6);
            this.lblDependencies.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDependencies.Name = "lblDependencies";
            this.lblDependencies.Size = new System.Drawing.Size(82, 13);
            this.lblDependencies.TabIndex = 0;
            this.lblDependencies.Text = "[Dependencies]";
            // 
            // gbpDirectoryInfoIncompatibilities
            // 
            this.gbpDirectoryInfoIncompatibilities.AutoSize = true;
            this.gbpDirectoryInfoIncompatibilities.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gbpDirectoryInfoIncompatibilities.Controls.Add(this.pnlDirectoryIncompatibilities);
            this.gbpDirectoryInfoIncompatibilities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbpDirectoryInfoIncompatibilities.Location = new System.Drawing.Point(240, 421);
            this.gbpDirectoryInfoIncompatibilities.Name = "gbpDirectoryInfoIncompatibilities";
            this.gbpDirectoryInfoIncompatibilities.Size = new System.Drawing.Size(231, 135);
            this.gbpDirectoryInfoIncompatibilities.TabIndex = 9;
            this.gbpDirectoryInfoIncompatibilities.TabStop = false;
            this.gbpDirectoryInfoIncompatibilities.Tag = "Title_DirectoryIncompatibilities";
            this.gbpDirectoryInfoIncompatibilities.Text = "Incompatibilities";
            // 
            // pnlDirectoryIncompatibilities
            // 
            this.pnlDirectoryIncompatibilities.AutoScroll = true;
            this.pnlDirectoryIncompatibilities.Controls.Add(this.lblIncompatibilities);
            this.pnlDirectoryIncompatibilities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDirectoryIncompatibilities.Location = new System.Drawing.Point(3, 16);
            this.pnlDirectoryIncompatibilities.Name = "pnlDirectoryIncompatibilities";
            this.pnlDirectoryIncompatibilities.Padding = new System.Windows.Forms.Padding(3, 6, 13, 6);
            this.pnlDirectoryIncompatibilities.Size = new System.Drawing.Size(225, 116);
            this.pnlDirectoryIncompatibilities.TabIndex = 2;
            // 
            // lblIncompatibilities
            // 
            this.lblIncompatibilities.AutoSize = true;
            this.lblIncompatibilities.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblIncompatibilities.Location = new System.Drawing.Point(3, 6);
            this.lblIncompatibilities.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblIncompatibilities.Name = "lblIncompatibilities";
            this.lblIncompatibilities.Size = new System.Drawing.Size(87, 13);
            this.lblIncompatibilities.TabIndex = 0;
            this.lblIncompatibilities.Text = "[Incompatibilities]";
            // 
            // tlpDirectoryInfoLeft
            // 
            this.tlpDirectoryInfoLeft.AutoSize = true;
            this.tlpDirectoryInfoLeft.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpDirectoryInfoLeft.ColumnCount = 2;
            this.tlpDirectoryInfoLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpDirectoryInfoLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDirectoryInfoLeft.Controls.Add(this.lblDirectoryNameLabel, 0, 0);
            this.tlpDirectoryInfoLeft.Controls.Add(this.lblDirectoryVersion, 1, 1);
            this.tlpDirectoryInfoLeft.Controls.Add(this.lblDirectoryName, 1, 0);
            this.tlpDirectoryInfoLeft.Controls.Add(this.lblDirectoryVersionLabel, 0, 1);
            this.tlpDirectoryInfoLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpDirectoryInfoLeft.Location = new System.Drawing.Point(0, 0);
            this.tlpDirectoryInfoLeft.Margin = new System.Windows.Forms.Padding(0);
            this.tlpDirectoryInfoLeft.Name = "tlpDirectoryInfoLeft";
            this.tlpDirectoryInfoLeft.RowCount = 2;
            this.tlpDirectoryInfoLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDirectoryInfoLeft.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDirectoryInfoLeft.Size = new System.Drawing.Size(237, 139);
            this.tlpDirectoryInfoLeft.TabIndex = 14;
            // 
            // lblDirectoryNameLabel
            // 
            this.lblDirectoryNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDirectoryNameLabel.AutoSize = true;
            this.lblDirectoryNameLabel.Location = new System.Drawing.Point(10, 6);
            this.lblDirectoryNameLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDirectoryNameLabel.Name = "lblDirectoryNameLabel";
            this.lblDirectoryNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblDirectoryNameLabel.TabIndex = 0;
            this.lblDirectoryNameLabel.Tag = "Label_DirectoryName";
            this.lblDirectoryNameLabel.Text = "Name:";
            this.lblDirectoryNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblDirectoryVersion
            // 
            this.lblDirectoryVersion.AutoSize = true;
            this.lblDirectoryVersion.Location = new System.Drawing.Point(54, 120);
            this.lblDirectoryVersion.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDirectoryVersion.Name = "lblDirectoryVersion";
            this.lblDirectoryVersion.Size = new System.Drawing.Size(93, 13);
            this.lblDirectoryVersion.TabIndex = 5;
            this.lblDirectoryVersion.Tag = "";
            this.lblDirectoryVersion.Text = "[Directory Version]";
            this.lblDirectoryVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblDirectoryName
            // 
            this.lblDirectoryName.AutoSize = true;
            this.lblDirectoryName.Location = new System.Drawing.Point(54, 6);
            this.lblDirectoryName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDirectoryName.Name = "lblDirectoryName";
            this.lblDirectoryName.Size = new System.Drawing.Size(86, 13);
            this.lblDirectoryName.TabIndex = 4;
            this.lblDirectoryName.Tag = "";
            this.lblDirectoryName.Text = "[Directory Name]";
            this.lblDirectoryName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblDirectoryVersionLabel
            // 
            this.lblDirectoryVersionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDirectoryVersionLabel.AutoSize = true;
            this.lblDirectoryVersionLabel.Location = new System.Drawing.Point(3, 120);
            this.lblDirectoryVersionLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDirectoryVersionLabel.Name = "lblDirectoryVersionLabel";
            this.lblDirectoryVersionLabel.Size = new System.Drawing.Size(45, 13);
            this.lblDirectoryVersionLabel.TabIndex = 2;
            this.lblDirectoryVersionLabel.Tag = "Label_DirectoryVersion";
            this.lblDirectoryVersionLabel.Text = "Version:";
            this.lblDirectoryVersionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // gpbDirectoryAuthors
            // 
            this.gpbDirectoryAuthors.AutoSize = true;
            this.gpbDirectoryAuthors.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbDirectoryAuthors.Controls.Add(this.pnlDirectoryAuthors);
            this.gpbDirectoryAuthors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpbDirectoryAuthors.Location = new System.Drawing.Point(240, 3);
            this.gpbDirectoryAuthors.Name = "gpbDirectoryAuthors";
            this.gpbDirectoryAuthors.Size = new System.Drawing.Size(231, 133);
            this.gpbDirectoryAuthors.TabIndex = 16;
            this.gpbDirectoryAuthors.TabStop = false;
            this.gpbDirectoryAuthors.Tag = "Label_DirectoryAuthors";
            this.gpbDirectoryAuthors.Text = "Authors";
            // 
            // pnlDirectoryAuthors
            // 
            this.pnlDirectoryAuthors.AutoScroll = true;
            this.pnlDirectoryAuthors.Controls.Add(this.lblDirectoryAuthors);
            this.pnlDirectoryAuthors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDirectoryAuthors.Location = new System.Drawing.Point(3, 16);
            this.pnlDirectoryAuthors.Name = "pnlDirectoryAuthors";
            this.pnlDirectoryAuthors.Padding = new System.Windows.Forms.Padding(3, 6, 13, 6);
            this.pnlDirectoryAuthors.Size = new System.Drawing.Size(225, 114);
            this.pnlDirectoryAuthors.TabIndex = 0;
            // 
            // lblDirectoryAuthors
            // 
            this.lblDirectoryAuthors.AutoSize = true;
            this.lblDirectoryAuthors.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDirectoryAuthors.Location = new System.Drawing.Point(3, 6);
            this.lblDirectoryAuthors.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDirectoryAuthors.Name = "lblDirectoryAuthors";
            this.lblDirectoryAuthors.Size = new System.Drawing.Size(94, 13);
            this.lblDirectoryAuthors.TabIndex = 6;
            this.lblDirectoryAuthors.Tag = "";
            this.lblDirectoryAuthors.Text = "[Directory Authors]";
            this.lblDirectoryAuthors.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // rtbDirectoryDescription
            // 
            this.rtbDirectoryDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tlpDirectoryInfo.SetColumnSpan(this.rtbDirectoryDescription, 2);
            this.rtbDirectoryDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbDirectoryDescription.Location = new System.Drawing.Point(3, 142);
            this.rtbDirectoryDescription.Name = "rtbDirectoryDescription";
            this.rtbDirectoryDescription.ReadOnly = true;
            this.rtbDirectoryDescription.Size = new System.Drawing.Size(468, 273);
            this.rtbDirectoryDescription.TabIndex = 17;
            this.rtbDirectoryDescription.Text = "";
            // 
            // lblCustomDataDirectoriesLabel
            // 
            this.lblCustomDataDirectoriesLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCustomDataDirectoriesLabel.AutoSize = true;
            this.lblCustomDataDirectoriesLabel.Location = new System.Drawing.Point(3, 6);
            this.lblCustomDataDirectoriesLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCustomDataDirectoriesLabel.Name = "lblCustomDataDirectoriesLabel";
            this.lblCustomDataDirectoriesLabel.Size = new System.Drawing.Size(155, 13);
            this.lblCustomDataDirectoriesLabel.TabIndex = 36;
            this.lblCustomDataDirectoriesLabel.Tag = "Label_CharacterOptions_CustomData";
            this.lblCustomDataDirectoriesLabel.Text = "Custom Data Directories to Use";
            this.lblCustomDataDirectoriesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tlpOptionalRulesButtons
            // 
            this.tlpOptionalRulesButtons.AutoSize = true;
            this.tlpOptionalRulesButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpOptionalRulesButtons.ColumnCount = 5;
            this.tlpOptionalRulesButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpOptionalRulesButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpOptionalRulesButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpOptionalRulesButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpOptionalRulesButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpOptionalRulesButtons.Controls.Add(this.cmdToTopCustomDirectoryLoadOrder, 2, 0);
            this.tlpOptionalRulesButtons.Controls.Add(this.cmdToBottomCustomDirectoryLoadOrder, 4, 0);
            this.tlpOptionalRulesButtons.Controls.Add(this.cmdDecreaseCustomDirectoryLoadOrder, 3, 0);
            this.tlpOptionalRulesButtons.Controls.Add(this.cmdIncreaseCustomDirectoryLoadOrder, 1, 0);
            this.tlpOptionalRulesButtons.Controls.Add(this.cmdGlobalOptionsCustomData, 0, 0);
            this.tlpOptionalRulesButtons.Location = new System.Drawing.Point(0, 555);
            this.tlpOptionalRulesButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpOptionalRulesButtons.Name = "tlpOptionalRulesButtons";
            this.tlpOptionalRulesButtons.RowCount = 1;
            this.tlpOptionalRulesButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpOptionalRulesButtons.Size = new System.Drawing.Size(625, 29);
            this.tlpOptionalRulesButtons.TabIndex = 46;
            // 
            // cmdToTopCustomDirectoryLoadOrder
            // 
            this.cmdToTopCustomDirectoryLoadOrder.AutoSize = true;
            this.cmdToTopCustomDirectoryLoadOrder.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdToTopCustomDirectoryLoadOrder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdToTopCustomDirectoryLoadOrder.Location = new System.Drawing.Point(253, 3);
            this.cmdToTopCustomDirectoryLoadOrder.Name = "cmdToTopCustomDirectoryLoadOrder";
            this.cmdToTopCustomDirectoryLoadOrder.Size = new System.Drawing.Size(119, 23);
            this.cmdToTopCustomDirectoryLoadOrder.TabIndex = 46;
            this.cmdToTopCustomDirectoryLoadOrder.Tag = "String_ToTop";
            this.cmdToTopCustomDirectoryLoadOrder.Text = "To Top";
            this.cmdToTopCustomDirectoryLoadOrder.UseVisualStyleBackColor = true;
            this.cmdToTopCustomDirectoryLoadOrder.Click += new System.EventHandler(this.cmdToTopCustomDirectoryLoadOrder_Click);
            // 
            // cmdToBottomCustomDirectoryLoadOrder
            // 
            this.cmdToBottomCustomDirectoryLoadOrder.AutoSize = true;
            this.cmdToBottomCustomDirectoryLoadOrder.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdToBottomCustomDirectoryLoadOrder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdToBottomCustomDirectoryLoadOrder.Location = new System.Drawing.Point(503, 3);
            this.cmdToBottomCustomDirectoryLoadOrder.Name = "cmdToBottomCustomDirectoryLoadOrder";
            this.cmdToBottomCustomDirectoryLoadOrder.Size = new System.Drawing.Size(119, 23);
            this.cmdToBottomCustomDirectoryLoadOrder.TabIndex = 45;
            this.cmdToBottomCustomDirectoryLoadOrder.Tag = "String_ToBottom";
            this.cmdToBottomCustomDirectoryLoadOrder.Text = "To Bottom";
            this.cmdToBottomCustomDirectoryLoadOrder.UseVisualStyleBackColor = true;
            this.cmdToBottomCustomDirectoryLoadOrder.Click += new System.EventHandler(this.cmdToBottomCustomDirectoryLoadOrder_Click);
            // 
            // cmdDecreaseCustomDirectoryLoadOrder
            // 
            this.cmdDecreaseCustomDirectoryLoadOrder.AutoSize = true;
            this.cmdDecreaseCustomDirectoryLoadOrder.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDecreaseCustomDirectoryLoadOrder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdDecreaseCustomDirectoryLoadOrder.Location = new System.Drawing.Point(378, 3);
            this.cmdDecreaseCustomDirectoryLoadOrder.Name = "cmdDecreaseCustomDirectoryLoadOrder";
            this.cmdDecreaseCustomDirectoryLoadOrder.Size = new System.Drawing.Size(119, 23);
            this.cmdDecreaseCustomDirectoryLoadOrder.TabIndex = 42;
            this.cmdDecreaseCustomDirectoryLoadOrder.Tag = "Button_DecreaseCustomDirectoryLoadOrder";
            this.cmdDecreaseCustomDirectoryLoadOrder.Text = "Decrease Load Order";
            this.cmdDecreaseCustomDirectoryLoadOrder.UseVisualStyleBackColor = true;
            this.cmdDecreaseCustomDirectoryLoadOrder.Click += new System.EventHandler(this.cmdDecreaseCustomDirectoryLoadOrder_Click);
            // 
            // cmdIncreaseCustomDirectoryLoadOrder
            // 
            this.cmdIncreaseCustomDirectoryLoadOrder.AutoSize = true;
            this.cmdIncreaseCustomDirectoryLoadOrder.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdIncreaseCustomDirectoryLoadOrder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdIncreaseCustomDirectoryLoadOrder.Location = new System.Drawing.Point(128, 3);
            this.cmdIncreaseCustomDirectoryLoadOrder.Name = "cmdIncreaseCustomDirectoryLoadOrder";
            this.cmdIncreaseCustomDirectoryLoadOrder.Size = new System.Drawing.Size(119, 23);
            this.cmdIncreaseCustomDirectoryLoadOrder.TabIndex = 43;
            this.cmdIncreaseCustomDirectoryLoadOrder.Tag = "Button_IncreaseCustomDirectoryLoadOrder";
            this.cmdIncreaseCustomDirectoryLoadOrder.Text = "Increase Load Order";
            this.cmdIncreaseCustomDirectoryLoadOrder.UseVisualStyleBackColor = true;
            this.cmdIncreaseCustomDirectoryLoadOrder.Click += new System.EventHandler(this.cmdIncreaseCustomDirectoryLoadOrder_Click);
            // 
            // cmdGlobalOptionsCustomData
            // 
            this.cmdGlobalOptionsCustomData.AutoSize = true;
            this.cmdGlobalOptionsCustomData.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdGlobalOptionsCustomData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdGlobalOptionsCustomData.Location = new System.Drawing.Point(3, 3);
            this.cmdGlobalOptionsCustomData.Name = "cmdGlobalOptionsCustomData";
            this.cmdGlobalOptionsCustomData.Size = new System.Drawing.Size(119, 23);
            this.cmdGlobalOptionsCustomData.TabIndex = 44;
            this.cmdGlobalOptionsCustomData.Tag = "Button_ChangeCustomDataEntries";
            this.cmdGlobalOptionsCustomData.Text = "Change Entries";
            this.cmdGlobalOptionsCustomData.UseVisualStyleBackColor = true;
            this.cmdGlobalOptionsCustomData.Click += new System.EventHandler(this.cmdGlobalOptionsCustomData_Click);
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
            this.flpHouseRules.Controls.Add(this.gpbHouseRules4eAdaptations);
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
            this.gpbHouseRulesQualities.Size = new System.Drawing.Size(535, 169);
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
            this.tlpHouseRulesQualities.Size = new System.Drawing.Size(529, 150);
            this.tlpHouseRulesQualities.TabIndex = 0;
            // 
            // chkExceedNegativeQualities
            // 
            this.chkExceedNegativeQualities.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkExceedNegativeQualities.AutoSize = true;
            this.chkExceedNegativeQualities.DefaultColorScheme = true;
            this.chkExceedNegativeQualities.Location = new System.Drawing.Point(3, 104);
            this.chkExceedNegativeQualities.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.chkDontDoubleQualityPurchases.Location = new System.Drawing.Point(3, 4);
            this.chkDontDoubleQualityPurchases.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.chkExceedPositiveQualities.Location = new System.Drawing.Point(3, 54);
            this.chkExceedPositiveQualities.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.chkDontDoubleQualityRefunds.Location = new System.Drawing.Point(3, 29);
            this.chkDontDoubleQualityRefunds.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.chkExceedPositiveQualitiesCostDoubled.Location = new System.Drawing.Point(23, 79);
            this.chkExceedPositiveQualitiesCostDoubled.Margin = new System.Windows.Forms.Padding(23, 4, 3, 4);
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
            this.chkExceedNegativeQualitiesLimit.Location = new System.Drawing.Point(23, 129);
            this.chkExceedNegativeQualitiesLimit.Margin = new System.Windows.Forms.Padding(23, 4, 3, 4);
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
            this.gpbHouseRulesAttributes.Controls.Add(this.tlpHouseRulesAttributes);
            this.gpbHouseRulesAttributes.Location = new System.Drawing.Point(544, 3);
            this.gpbHouseRulesAttributes.Name = "gpbHouseRulesAttributes";
            this.gpbHouseRulesAttributes.Size = new System.Drawing.Size(423, 195);
            this.gpbHouseRulesAttributes.TabIndex = 3;
            this.gpbHouseRulesAttributes.TabStop = false;
            this.gpbHouseRulesAttributes.Tag = "Label_Attributes";
            this.gpbHouseRulesAttributes.Text = "Attributes";
            // 
            // tlpHouseRulesAttributes
            // 
            this.tlpHouseRulesAttributes.AutoSize = true;
            this.tlpHouseRulesAttributes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpHouseRulesAttributes.ColumnCount = 1;
            this.tlpHouseRulesAttributes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHouseRulesAttributes.Controls.Add(this.chkESSLossReducesMaximumOnly, 0, 0);
            this.tlpHouseRulesAttributes.Controls.Add(this.chkAllowCyberwareESSDiscounts, 0, 2);
            this.tlpHouseRulesAttributes.Controls.Add(this.chkAlternateMetatypeAttributeKarma, 0, 4);
            this.tlpHouseRulesAttributes.Controls.Add(this.chkReverseAttributePriorityOrder, 0, 3);
            this.tlpHouseRulesAttributes.Controls.Add(this.flpDroneArmorMultiplier, 0, 6);
            this.tlpHouseRulesAttributes.Controls.Add(this.chkUseCalculatedPublicAwareness, 0, 5);
            this.tlpHouseRulesAttributes.Controls.Add(this.chkUnclampAttributeMinimum, 0, 1);
            this.tlpHouseRulesAttributes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpHouseRulesAttributes.Location = new System.Drawing.Point(3, 16);
            this.tlpHouseRulesAttributes.Name = "tlpHouseRulesAttributes";
            this.tlpHouseRulesAttributes.RowCount = 7;
            this.tlpHouseRulesAttributes.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesAttributes.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesAttributes.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesAttributes.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesAttributes.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesAttributes.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesAttributes.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHouseRulesAttributes.Size = new System.Drawing.Size(417, 176);
            this.tlpHouseRulesAttributes.TabIndex = 0;
            // 
            // chkESSLossReducesMaximumOnly
            // 
            this.chkESSLossReducesMaximumOnly.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkESSLossReducesMaximumOnly.AutoSize = true;
            this.chkESSLossReducesMaximumOnly.DefaultColorScheme = true;
            this.chkESSLossReducesMaximumOnly.Location = new System.Drawing.Point(3, 4);
            this.chkESSLossReducesMaximumOnly.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.chkAllowCyberwareESSDiscounts.Location = new System.Drawing.Point(3, 54);
            this.chkAllowCyberwareESSDiscounts.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.chkAlternateMetatypeAttributeKarma.Location = new System.Drawing.Point(3, 104);
            this.chkAlternateMetatypeAttributeKarma.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.chkReverseAttributePriorityOrder.Location = new System.Drawing.Point(3, 79);
            this.chkReverseAttributePriorityOrder.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.flpDroneArmorMultiplier.Location = new System.Drawing.Point(0, 150);
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
            this.chkDroneArmorMultiplier.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.lblDroneArmorMultiplierTimes.Location = new System.Drawing.Point(261, 4);
            this.lblDroneArmorMultiplierTimes.Margin = new System.Windows.Forms.Padding(3, 4, 3, 8);
            this.lblDroneArmorMultiplierTimes.Name = "lblDroneArmorMultiplierTimes";
            this.lblDroneArmorMultiplierTimes.Size = new System.Drawing.Size(12, 14);
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
            this.chkUseCalculatedPublicAwareness.Location = new System.Drawing.Point(3, 129);
            this.chkUseCalculatedPublicAwareness.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.chkUnclampAttributeMinimum.Location = new System.Drawing.Point(3, 29);
            this.chkUnclampAttributeMinimum.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.gpbHouseRulesSkills.Location = new System.Drawing.Point(3, 204);
            this.gpbHouseRulesSkills.Name = "gpbHouseRulesSkills";
            this.gpbHouseRulesSkills.Size = new System.Drawing.Size(452, 170);
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
            this.tlpHouseRulesSkills.Controls.Add(this.flpMaxSkillRating, 0, 0);
            this.tlpHouseRulesSkills.Controls.Add(this.chkFreeMartialArtSpecialization, 0, 5);
            this.tlpHouseRulesSkills.Controls.Add(this.chkUsePointsOnBrokenGroups, 0, 4);
            this.tlpHouseRulesSkills.Controls.Add(this.chkAllowSkillRegrouping, 0, 3);
            this.tlpHouseRulesSkills.Controls.Add(this.chkCompensateSkillGroupKarmaDifference, 0, 2);
            this.tlpHouseRulesSkills.Controls.Add(this.chkSpecializationsBreakSkillGroups, 0, 1);
            this.tlpHouseRulesSkills.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpHouseRulesSkills.Location = new System.Drawing.Point(3, 16);
            this.tlpHouseRulesSkills.Name = "tlpHouseRulesSkills";
            this.tlpHouseRulesSkills.RowCount = 6;
            this.tlpHouseRulesSkills.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesSkills.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesSkills.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesSkills.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesSkills.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesSkills.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesSkills.Size = new System.Drawing.Size(446, 151);
            this.tlpHouseRulesSkills.TabIndex = 0;
            // 
            // flpMaxSkillRating
            // 
            this.flpMaxSkillRating.AutoSize = true;
            this.flpMaxSkillRating.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpMaxSkillRating.Controls.Add(this.lblMaxSkillRating);
            this.flpMaxSkillRating.Controls.Add(this.nudMaxSkillRating);
            this.flpMaxSkillRating.Controls.Add(this.lblMaxKnowledgeSkillRating);
            this.flpMaxSkillRating.Controls.Add(this.nudMaxKnowledgeSkillRating);
            this.flpMaxSkillRating.Location = new System.Drawing.Point(0, 0);
            this.flpMaxSkillRating.Margin = new System.Windows.Forms.Padding(0);
            this.flpMaxSkillRating.Name = "flpMaxSkillRating";
            this.flpMaxSkillRating.Size = new System.Drawing.Size(328, 26);
            this.flpMaxSkillRating.TabIndex = 53;
            // 
            // lblMaxSkillRating
            // 
            this.lblMaxSkillRating.AutoSize = true;
            this.lblMaxSkillRating.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblMaxSkillRating.Location = new System.Drawing.Point(3, 4);
            this.lblMaxSkillRating.Margin = new System.Windows.Forms.Padding(3, 4, 3, 8);
            this.lblMaxSkillRating.Name = "lblMaxSkillRating";
            this.lblMaxSkillRating.Size = new System.Drawing.Size(83, 14);
            this.lblMaxSkillRating.TabIndex = 27;
            this.lblMaxSkillRating.Tag = "Label_CharacterOptions_MaxSkillRating";
            this.lblMaxSkillRating.Text = "Max Skill Rating";
            this.lblMaxSkillRating.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudMaxSkillRating
            // 
            this.nudMaxSkillRating.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudMaxSkillRating.AutoSize = true;
            this.nudMaxSkillRating.Location = new System.Drawing.Point(92, 3);
            this.nudMaxSkillRating.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudMaxSkillRating.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMaxSkillRating.Name = "nudMaxSkillRating";
            this.nudMaxSkillRating.Size = new System.Drawing.Size(41, 20);
            this.nudMaxSkillRating.TabIndex = 26;
            this.nudMaxSkillRating.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
            // 
            // lblMaxKnowledgeSkillRating
            // 
            this.lblMaxKnowledgeSkillRating.AutoSize = true;
            this.lblMaxKnowledgeSkillRating.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblMaxKnowledgeSkillRating.Location = new System.Drawing.Point(139, 4);
            this.lblMaxKnowledgeSkillRating.Margin = new System.Windows.Forms.Padding(3, 4, 3, 8);
            this.lblMaxKnowledgeSkillRating.Name = "lblMaxKnowledgeSkillRating";
            this.lblMaxKnowledgeSkillRating.Size = new System.Drawing.Size(139, 14);
            this.lblMaxKnowledgeSkillRating.TabIndex = 28;
            this.lblMaxKnowledgeSkillRating.Tag = "Label_CharacterOptions_MaxKnowledgeSkillRating";
            this.lblMaxKnowledgeSkillRating.Text = "Max Knowledge Skill Rating";
            this.lblMaxKnowledgeSkillRating.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudMaxKnowledgeSkillRating
            // 
            this.nudMaxKnowledgeSkillRating.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudMaxKnowledgeSkillRating.AutoSize = true;
            this.nudMaxKnowledgeSkillRating.Location = new System.Drawing.Point(284, 3);
            this.nudMaxKnowledgeSkillRating.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudMaxKnowledgeSkillRating.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMaxKnowledgeSkillRating.Name = "nudMaxKnowledgeSkillRating";
            this.nudMaxKnowledgeSkillRating.Size = new System.Drawing.Size(41, 20);
            this.nudMaxKnowledgeSkillRating.TabIndex = 29;
            this.nudMaxKnowledgeSkillRating.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
            // 
            // chkFreeMartialArtSpecialization
            // 
            this.chkFreeMartialArtSpecialization.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkFreeMartialArtSpecialization.AutoSize = true;
            this.chkFreeMartialArtSpecialization.DefaultColorScheme = true;
            this.chkFreeMartialArtSpecialization.Location = new System.Drawing.Point(3, 130);
            this.chkFreeMartialArtSpecialization.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.chkUsePointsOnBrokenGroups.Location = new System.Drawing.Point(3, 105);
            this.chkUsePointsOnBrokenGroups.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkUsePointsOnBrokenGroups.Name = "chkUsePointsOnBrokenGroups";
            this.chkUsePointsOnBrokenGroups.Size = new System.Drawing.Size(185, 17);
            this.chkUsePointsOnBrokenGroups.TabIndex = 49;
            this.chkUsePointsOnBrokenGroups.Tag = "Checkbox_Options_PointsOnBrokenGroups";
            this.chkUsePointsOnBrokenGroups.Text = "Use Skill Points on broken groups";
            this.chkUsePointsOnBrokenGroups.UseVisualStyleBackColor = true;
            // 
            // chkAllowSkillRegrouping
            // 
            this.chkAllowSkillRegrouping.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkAllowSkillRegrouping.AutoSize = true;
            this.chkAllowSkillRegrouping.DefaultColorScheme = true;
            this.chkAllowSkillRegrouping.Location = new System.Drawing.Point(3, 80);
            this.chkAllowSkillRegrouping.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkAllowSkillRegrouping.Name = "chkAllowSkillRegrouping";
            this.chkAllowSkillRegrouping.Size = new System.Drawing.Size(285, 17);
            this.chkAllowSkillRegrouping.TabIndex = 39;
            this.chkAllowSkillRegrouping.Tag = "Checkbox_Options_SkillRegroup";
            this.chkAllowSkillRegrouping.Text = "Allow Skills to be re-Grouped if all Ratings are the same";
            this.chkAllowSkillRegrouping.UseVisualStyleBackColor = true;
            // 
            // chkCompensateSkillGroupKarmaDifference
            // 
            this.chkCompensateSkillGroupKarmaDifference.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkCompensateSkillGroupKarmaDifference.AutoSize = true;
            this.chkCompensateSkillGroupKarmaDifference.DefaultColorScheme = true;
            this.chkCompensateSkillGroupKarmaDifference.Location = new System.Drawing.Point(3, 55);
            this.chkCompensateSkillGroupKarmaDifference.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkCompensateSkillGroupKarmaDifference.Name = "chkCompensateSkillGroupKarmaDifference";
            this.chkCompensateSkillGroupKarmaDifference.Size = new System.Drawing.Size(440, 17);
            this.chkCompensateSkillGroupKarmaDifference.TabIndex = 36;
            this.chkCompensateSkillGroupKarmaDifference.Tag = "Checkbox_Options_CompensateSkillGroupKarmaDifference";
            this.chkCompensateSkillGroupKarmaDifference.Text = "Compensate for higher karma costs when raising the rating of the last skill in a " +
    "skill group";
            this.chkCompensateSkillGroupKarmaDifference.UseVisualStyleBackColor = true;
            // 
            // chkSpecializationsBreakSkillGroups
            // 
            this.chkSpecializationsBreakSkillGroups.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkSpecializationsBreakSkillGroups.AutoSize = true;
            this.chkSpecializationsBreakSkillGroups.DefaultColorScheme = true;
            this.chkSpecializationsBreakSkillGroups.Location = new System.Drawing.Point(3, 30);
            this.chkSpecializationsBreakSkillGroups.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkSpecializationsBreakSkillGroups.Name = "chkSpecializationsBreakSkillGroups";
            this.chkSpecializationsBreakSkillGroups.Size = new System.Drawing.Size(181, 17);
            this.chkSpecializationsBreakSkillGroups.TabIndex = 54;
            this.chkSpecializationsBreakSkillGroups.Tag = "Checkbox_Options_SpecializationsBreakSkillGroups";
            this.chkSpecializationsBreakSkillGroups.Text = "Specializations break skill groups";
            this.chkSpecializationsBreakSkillGroups.UseVisualStyleBackColor = true;
            // 
            // gpbHouseRulesCombat
            // 
            this.gpbHouseRulesCombat.AutoSize = true;
            this.gpbHouseRulesCombat.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbHouseRulesCombat.Controls.Add(this.tlpHouseRulesCombat);
            this.gpbHouseRulesCombat.Location = new System.Drawing.Point(461, 204);
            this.gpbHouseRulesCombat.Name = "gpbHouseRulesCombat";
            this.gpbHouseRulesCombat.Size = new System.Drawing.Size(384, 69);
            this.gpbHouseRulesCombat.TabIndex = 4;
            this.gpbHouseRulesCombat.TabStop = false;
            this.gpbHouseRulesCombat.Tag = "Label_CharacterOptions_Combat";
            this.gpbHouseRulesCombat.Text = "Combat";
            // 
            // tlpHouseRulesCombat
            // 
            this.tlpHouseRulesCombat.AutoSize = true;
            this.tlpHouseRulesCombat.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpHouseRulesCombat.ColumnCount = 1;
            this.tlpHouseRulesCombat.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHouseRulesCombat.Controls.Add(this.chkNoArmorEncumbrance, 0, 0);
            this.tlpHouseRulesCombat.Controls.Add(this.chkUnarmedSkillImprovements, 0, 1);
            this.tlpHouseRulesCombat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpHouseRulesCombat.Location = new System.Drawing.Point(3, 16);
            this.tlpHouseRulesCombat.Name = "tlpHouseRulesCombat";
            this.tlpHouseRulesCombat.RowCount = 2;
            this.tlpHouseRulesCombat.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesCombat.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesCombat.Size = new System.Drawing.Size(378, 50);
            this.tlpHouseRulesCombat.TabIndex = 0;
            // 
            // chkNoArmorEncumbrance
            // 
            this.chkNoArmorEncumbrance.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkNoArmorEncumbrance.AutoSize = true;
            this.chkNoArmorEncumbrance.DefaultColorScheme = true;
            this.chkNoArmorEncumbrance.Location = new System.Drawing.Point(3, 4);
            this.chkNoArmorEncumbrance.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkNoArmorEncumbrance.Name = "chkNoArmorEncumbrance";
            this.chkNoArmorEncumbrance.Size = new System.Drawing.Size(139, 17);
            this.chkNoArmorEncumbrance.TabIndex = 38;
            this.chkNoArmorEncumbrance.Tag = "Checkbox_Options_NoArmorEncumbrance";
            this.chkNoArmorEncumbrance.Text = "No Armor Encumbrance";
            this.chkNoArmorEncumbrance.UseVisualStyleBackColor = true;
            // 
            // chkUnarmedSkillImprovements
            // 
            this.chkUnarmedSkillImprovements.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkUnarmedSkillImprovements.AutoSize = true;
            this.chkUnarmedSkillImprovements.DefaultColorScheme = true;
            this.chkUnarmedSkillImprovements.Location = new System.Drawing.Point(3, 29);
            this.chkUnarmedSkillImprovements.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.gpbHouseRulesMagicResonance.Location = new System.Drawing.Point(3, 380);
            this.gpbHouseRulesMagicResonance.Name = "gpbHouseRulesMagicResonance";
            this.gpbHouseRulesMagicResonance.Size = new System.Drawing.Size(440, 321);
            this.gpbHouseRulesMagicResonance.TabIndex = 0;
            this.gpbHouseRulesMagicResonance.TabStop = false;
            this.gpbHouseRulesMagicResonance.Tag = "Label_CharacterOptions_MagicAndResonance";
            this.gpbHouseRulesMagicResonance.Text = "Magic and Resonance";
            // 
            // tlpHouseRulesMagicResonance
            // 
            this.tlpHouseRulesMagicResonance.AutoSize = true;
            this.tlpHouseRulesMagicResonance.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpHouseRulesMagicResonance.ColumnCount = 2;
            this.tlpHouseRulesMagicResonance.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpHouseRulesMagicResonance.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpHouseRulesMagicResonance.Controls.Add(this.lblRegisteredSprites, 0, 11);
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
            this.tlpHouseRulesMagicResonance.Controls.Add(this.lblBoundSpiritLimit, 0, 10);
            this.tlpHouseRulesMagicResonance.Controls.Add(this.txtBoundSpiritLimit, 1, 10);
            this.tlpHouseRulesMagicResonance.Controls.Add(this.txtRegisteredSpriteLimit, 1, 11);
            this.tlpHouseRulesMagicResonance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpHouseRulesMagicResonance.Location = new System.Drawing.Point(3, 16);
            this.tlpHouseRulesMagicResonance.Name = "tlpHouseRulesMagicResonance";
            this.tlpHouseRulesMagicResonance.RowCount = 12;
            this.tlpHouseRulesMagicResonance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRulesMagicResonance.RowStyles.Add(new System.Windows.Forms.RowStyle());
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
            this.tlpHouseRulesMagicResonance.Size = new System.Drawing.Size(434, 302);
            this.tlpHouseRulesMagicResonance.TabIndex = 0;
            // 
            // lblRegisteredSprites
            // 
            this.lblRegisteredSprites.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblRegisteredSprites.AutoSize = true;
            this.lblRegisteredSprites.Location = new System.Drawing.Point(3, 282);
            this.lblRegisteredSprites.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRegisteredSprites.Name = "lblRegisteredSprites";
            this.lblRegisteredSprites.Size = new System.Drawing.Size(132, 13);
            this.lblRegisteredSprites.TabIndex = 48;
            this.lblRegisteredSprites.Tag = "Label_CharacterOptions_RegisteredSpriteLimit";
            this.lblRegisteredSprites.Text = "Maximum Compiled Sprites";
            this.lblRegisteredSprites.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblRegisteredSprites.ToolTipText = "";
            // 
            // chkMysAdPp
            // 
            this.chkMysAdPp.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkMysAdPp.AutoSize = true;
            this.tlpHouseRulesMagicResonance.SetColumnSpan(this.chkMysAdPp, 2);
            this.chkMysAdPp.DefaultColorScheme = true;
            this.chkMysAdPp.Location = new System.Drawing.Point(3, 54);
            this.chkMysAdPp.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.tlpHouseRulesMagicResonance.SetColumnSpan(this.chkPrioritySpellsAsAdeptPowers, 2);
            this.chkPrioritySpellsAsAdeptPowers.DefaultColorScheme = true;
            this.chkPrioritySpellsAsAdeptPowers.Location = new System.Drawing.Point(3, 79);
            this.chkPrioritySpellsAsAdeptPowers.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.tlpHouseRulesMagicResonance.SetColumnSpan(this.chkExtendAnyDetectionSpell, 2);
            this.chkExtendAnyDetectionSpell.DefaultColorScheme = true;
            this.chkExtendAnyDetectionSpell.Location = new System.Drawing.Point(3, 104);
            this.chkExtendAnyDetectionSpell.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.tlpHouseRulesMagicResonance.SetColumnSpan(this.chkIncreasedImprovedAbilityModifier, 2);
            this.chkIncreasedImprovedAbilityModifier.DefaultColorScheme = true;
            this.chkIncreasedImprovedAbilityModifier.Location = new System.Drawing.Point(3, 129);
            this.chkIncreasedImprovedAbilityModifier.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.tlpHouseRulesMagicResonance.SetColumnSpan(this.chkIgnoreComplexFormLimit, 2);
            this.chkIgnoreComplexFormLimit.DefaultColorScheme = true;
            this.chkIgnoreComplexFormLimit.Location = new System.Drawing.Point(3, 229);
            this.chkIgnoreComplexFormLimit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.tlpHouseRulesMagicResonance.SetColumnSpan(this.chkSpecialKarmaCost, 2);
            this.chkSpecialKarmaCost.DefaultColorScheme = true;
            this.chkSpecialKarmaCost.Location = new System.Drawing.Point(3, 154);
            this.chkSpecialKarmaCost.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.tlpHouseRulesMagicResonance.SetColumnSpan(this.chkAllowTechnomancerSchooling, 2);
            this.chkAllowTechnomancerSchooling.DefaultColorScheme = true;
            this.chkAllowTechnomancerSchooling.Location = new System.Drawing.Point(3, 204);
            this.chkAllowTechnomancerSchooling.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.tlpHouseRulesMagicResonance.SetColumnSpan(this.chkAllowInitiation, 2);
            this.chkAllowInitiation.DefaultColorScheme = true;
            this.chkAllowInitiation.Location = new System.Drawing.Point(3, 179);
            this.chkAllowInitiation.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.tlpHouseRulesMagicResonance.SetColumnSpan(this.chkMysAdeptSecondMAGAttribute, 2);
            this.chkMysAdeptSecondMAGAttribute.DefaultColorScheme = true;
            this.chkMysAdeptSecondMAGAttribute.Location = new System.Drawing.Point(3, 29);
            this.chkMysAdeptSecondMAGAttribute.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.tlpHouseRulesMagicResonance.SetColumnSpan(this.chkIgnoreArt, 2);
            this.chkIgnoreArt.DefaultColorScheme = true;
            this.chkIgnoreArt.Location = new System.Drawing.Point(3, 4);
            this.chkIgnoreArt.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkIgnoreArt.Name = "chkIgnoreArt";
            this.chkIgnoreArt.Size = new System.Drawing.Size(235, 17);
            this.chkIgnoreArt.TabIndex = 1;
            this.chkIgnoreArt.Tag = "Checkbox_Options_IgnoreArt";
            this.chkIgnoreArt.Text = "Ignore Art Requirements from Street Grimoire";
            this.chkIgnoreArt.UseVisualStyleBackColor = true;
            // 
            // lblBoundSpiritLimit
            // 
            this.lblBoundSpiritLimit.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblBoundSpiritLimit.AutoSize = true;
            this.lblBoundSpiritLimit.Location = new System.Drawing.Point(19, 256);
            this.lblBoundSpiritLimit.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBoundSpiritLimit.Name = "lblBoundSpiritLimit";
            this.lblBoundSpiritLimit.Size = new System.Drawing.Size(116, 13);
            this.lblBoundSpiritLimit.TabIndex = 46;
            this.lblBoundSpiritLimit.Tag = "Label_CharacterOptions_BoundSpiritLimit";
            this.lblBoundSpiritLimit.Text = "Maximum Bound Spirits";
            this.lblBoundSpiritLimit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblBoundSpiritLimit.ToolTipText = "";
            // 
            // txtBoundSpiritLimit
            // 
            this.txtBoundSpiritLimit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBoundSpiritLimit.Location = new System.Drawing.Point(141, 253);
            this.txtBoundSpiritLimit.Name = "txtBoundSpiritLimit";
            this.txtBoundSpiritLimit.Size = new System.Drawing.Size(290, 20);
            this.txtBoundSpiritLimit.TabIndex = 47;
            this.txtBoundSpiritLimit.Text = "{CHA}";
            this.txtBoundSpiritLimit.TextChanged += new System.EventHandler(this.txtBoundSpiritLimit_TextChanged);
            // 
            // txtRegisteredSpriteLimit
            // 
            this.txtRegisteredSpriteLimit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRegisteredSpriteLimit.Location = new System.Drawing.Point(141, 279);
            this.txtRegisteredSpriteLimit.Name = "txtRegisteredSpriteLimit";
            this.txtRegisteredSpriteLimit.Size = new System.Drawing.Size(290, 20);
            this.txtRegisteredSpriteLimit.TabIndex = 49;
            this.txtRegisteredSpriteLimit.Text = "{LOG}";
            this.txtRegisteredSpriteLimit.TextChanged += new System.EventHandler(this.txtRegisteredSpriteLimit_TextChanged);
            // 
            // gpbHouseRules4eAdaptations
            // 
            this.gpbHouseRules4eAdaptations.AutoSize = true;
            this.gpbHouseRules4eAdaptations.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbHouseRules4eAdaptations.Controls.Add(this.tlpHouseRules4eAdaptations);
            this.gpbHouseRules4eAdaptations.Location = new System.Drawing.Point(449, 380);
            this.gpbHouseRules4eAdaptations.Name = "gpbHouseRules4eAdaptations";
            this.gpbHouseRules4eAdaptations.Size = new System.Drawing.Size(441, 120);
            this.gpbHouseRules4eAdaptations.TabIndex = 5;
            this.gpbHouseRules4eAdaptations.TabStop = false;
            this.gpbHouseRules4eAdaptations.Tag = "Label_CharacterOptions_4eAdaptations";
            this.gpbHouseRules4eAdaptations.Text = "4th Edition Adaptations";
            // 
            // tlpHouseRules4eAdaptations
            // 
            this.tlpHouseRules4eAdaptations.AutoSize = true;
            this.tlpHouseRules4eAdaptations.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpHouseRules4eAdaptations.ColumnCount = 1;
            this.tlpHouseRules4eAdaptations.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHouseRules4eAdaptations.Controls.Add(this.chkEnemyKarmaQualityLimit, 0, 2);
            this.tlpHouseRules4eAdaptations.Controls.Add(this.chkEnable4eStyleEnemyTracking, 0, 0);
            this.tlpHouseRules4eAdaptations.Controls.Add(this.chkMoreLethalGameplay, 0, 3);
            this.tlpHouseRules4eAdaptations.Controls.Add(this.flpKarmaGainedFromEnemies, 0, 1);
            this.tlpHouseRules4eAdaptations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpHouseRules4eAdaptations.Location = new System.Drawing.Point(3, 16);
            this.tlpHouseRules4eAdaptations.Name = "tlpHouseRules4eAdaptations";
            this.tlpHouseRules4eAdaptations.RowCount = 4;
            this.tlpHouseRules4eAdaptations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRules4eAdaptations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRules4eAdaptations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpHouseRules4eAdaptations.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHouseRules4eAdaptations.Size = new System.Drawing.Size(435, 101);
            this.tlpHouseRules4eAdaptations.TabIndex = 0;
            // 
            // chkEnemyKarmaQualityLimit
            // 
            this.chkEnemyKarmaQualityLimit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkEnemyKarmaQualityLimit.AutoSize = true;
            this.chkEnemyKarmaQualityLimit.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.tlpHouseRules4eAdaptations.SetColumnSpan(this.chkEnemyKarmaQualityLimit, 4);
            this.chkEnemyKarmaQualityLimit.DefaultColorScheme = true;
            this.chkEnemyKarmaQualityLimit.Location = new System.Drawing.Point(23, 55);
            this.chkEnemyKarmaQualityLimit.Margin = new System.Windows.Forms.Padding(23, 4, 3, 4);
            this.chkEnemyKarmaQualityLimit.Name = "chkEnemyKarmaQualityLimit";
            this.chkEnemyKarmaQualityLimit.Size = new System.Drawing.Size(409, 17);
            this.chkEnemyKarmaQualityLimit.TabIndex = 48;
            this.chkEnemyKarmaQualityLimit.Tag = "Checkbox_Options_EnemyKarmaQualityLimit";
            this.chkEnemyKarmaQualityLimit.Text = "Karma Gained from Enemies counts towards Negative Quality limit in Create mode";
            this.chkEnemyKarmaQualityLimit.UseVisualStyleBackColor = true;
            // 
            // chkEnable4eStyleEnemyTracking
            // 
            this.chkEnable4eStyleEnemyTracking.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkEnable4eStyleEnemyTracking.AutoSize = true;
            this.chkEnable4eStyleEnemyTracking.DefaultColorScheme = true;
            this.chkEnable4eStyleEnemyTracking.Location = new System.Drawing.Point(3, 4);
            this.chkEnable4eStyleEnemyTracking.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkEnable4eStyleEnemyTracking.Name = "chkEnable4eStyleEnemyTracking";
            this.chkEnable4eStyleEnemyTracking.Size = new System.Drawing.Size(272, 17);
            this.chkEnable4eStyleEnemyTracking.TabIndex = 38;
            this.chkEnable4eStyleEnemyTracking.Tag = "Checkbox_Options_Enable4eStyleEnemyTracking";
            this.chkEnable4eStyleEnemyTracking.Text = "Use 4th Edition Rules for Enemies as a Contact type";
            this.chkEnable4eStyleEnemyTracking.UseVisualStyleBackColor = true;
            // 
            // chkMoreLethalGameplay
            // 
            this.chkMoreLethalGameplay.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkMoreLethalGameplay.AutoSize = true;
            this.chkMoreLethalGameplay.DefaultColorScheme = true;
            this.chkMoreLethalGameplay.Location = new System.Drawing.Point(3, 80);
            this.chkMoreLethalGameplay.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkMoreLethalGameplay.Name = "chkMoreLethalGameplay";
            this.chkMoreLethalGameplay.Size = new System.Drawing.Size(297, 17);
            this.chkMoreLethalGameplay.TabIndex = 41;
            this.chkMoreLethalGameplay.Tag = "Checkbox_Options_MoreLethalGameplace";
            this.chkMoreLethalGameplay.Text = "Use 4th Edition Rules for More Lethal Gameplay (SR4 75)";
            this.chkMoreLethalGameplay.UseVisualStyleBackColor = true;
            // 
            // flpKarmaGainedFromEnemies
            // 
            this.flpKarmaGainedFromEnemies.AutoSize = true;
            this.flpKarmaGainedFromEnemies.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpKarmaGainedFromEnemies.Controls.Add(this.lblKarmaGainedFromEnemies);
            this.flpKarmaGainedFromEnemies.Controls.Add(this.nudKarmaGainedFromEnemies);
            this.flpKarmaGainedFromEnemies.Controls.Add(this.lblKarmaGainedFromEnemiesExtra);
            this.flpKarmaGainedFromEnemies.Location = new System.Drawing.Point(20, 25);
            this.flpKarmaGainedFromEnemies.Margin = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.flpKarmaGainedFromEnemies.Name = "flpKarmaGainedFromEnemies";
            this.flpKarmaGainedFromEnemies.Size = new System.Drawing.Size(319, 26);
            this.flpKarmaGainedFromEnemies.TabIndex = 49;
            // 
            // lblKarmaGainedFromEnemies
            // 
            this.lblKarmaGainedFromEnemies.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaGainedFromEnemies.AutoSize = true;
            this.lblKarmaGainedFromEnemies.Location = new System.Drawing.Point(3, 6);
            this.lblKarmaGainedFromEnemies.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaGainedFromEnemies.Name = "lblKarmaGainedFromEnemies";
            this.lblKarmaGainedFromEnemies.Size = new System.Drawing.Size(140, 13);
            this.lblKarmaGainedFromEnemies.TabIndex = 48;
            this.lblKarmaGainedFromEnemies.Tag = "Label_Options_KarmaGainedFromEnemies";
            this.lblKarmaGainedFromEnemies.Text = "Karma Gained from Enemies";
            this.lblKarmaGainedFromEnemies.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarmaGainedFromEnemies
            // 
            this.nudKarmaGainedFromEnemies.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarmaGainedFromEnemies.AutoSize = true;
            this.nudKarmaGainedFromEnemies.Location = new System.Drawing.Point(149, 3);
            this.nudKarmaGainedFromEnemies.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudKarmaGainedFromEnemies.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudKarmaGainedFromEnemies.Name = "nudKarmaGainedFromEnemies";
            this.nudKarmaGainedFromEnemies.Size = new System.Drawing.Size(41, 20);
            this.nudKarmaGainedFromEnemies.TabIndex = 49;
            // 
            // lblKarmaGainedFromEnemiesExtra
            // 
            this.lblKarmaGainedFromEnemiesExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarmaGainedFromEnemiesExtra.AutoSize = true;
            this.lblKarmaGainedFromEnemiesExtra.Location = new System.Drawing.Point(196, 6);
            this.lblKarmaGainedFromEnemiesExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaGainedFromEnemiesExtra.Name = "lblKarmaGainedFromEnemiesExtra";
            this.lblKarmaGainedFromEnemiesExtra.Size = new System.Drawing.Size(120, 13);
            this.lblKarmaGainedFromEnemiesExtra.TabIndex = 50;
            this.lblKarmaGainedFromEnemiesExtra.Tag = "Label_Options_ConnectionLoyalty";
            this.lblKarmaGainedFromEnemiesExtra.Text = "x (Connection + Loyalty)";
            this.lblKarmaGainedFromEnemiesExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.cmdRename.Location = new System.Drawing.Point(102, 0);
            this.cmdRename.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdRename.Name = "cmdRename";
            this.cmdRename.Size = new System.Drawing.Size(93, 23);
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
            this.cmdDelete.Location = new System.Drawing.Point(201, 0);
            this.cmdDelete.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(93, 23);
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
            this.cmdOK.Location = new System.Drawing.Point(498, 0);
            this.cmdOK.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(96, 23);
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
            this.cmdRestoreDefaults.Size = new System.Drawing.Size(96, 23);
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
            this.cmdSave.Enabled = false;
            this.cmdSave.Location = new System.Drawing.Point(399, 0);
            this.cmdSave.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdSave.Name = "cmdSave";
            this.cmdSave.Size = new System.Drawing.Size(93, 23);
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
            this.cmdSaveAs.Enabled = false;
            this.cmdSaveAs.Location = new System.Drawing.Point(300, 0);
            this.cmdSaveAs.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdSaveAs.Name = "cmdSaveAs";
            this.cmdSaveAs.Size = new System.Drawing.Size(93, 23);
            this.cmdSaveAs.TabIndex = 7;
            this.cmdSaveAs.Tag = "String_SaveAs";
            this.cmdSaveAs.Text = "Save As...";
            this.cmdSaveAs.UseVisualStyleBackColor = true;
            this.cmdSaveAs.Click += new System.EventHandler(this.cmdSaveAs_Click);
            // 
            // EditCharacterSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1264, 681);
            this.Controls.Add(this.tlpOptions);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.Name = "EditCharacterSettings";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_CharacterOptions";
            this.Text = "Character Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditCharacterSettings_FormClosing);
            this.Load += new System.EventHandler(this.EditCharacterSettings_Load);
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
            ((System.ComponentModel.ISupportInitialize)(this.nudStartingKarma)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSumToTen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxNuyenKarma)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudQualityKarmaLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxNumberMaxAttributes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxSkillRatingCreate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxKnowledgeSkillRatingCreate)).EndInit();
            this.gpbBasicOptionsOfficialRules.ResumeLayout(false);
            this.gpbBasicOptionsOfficialRules.PerformLayout();
            this.tlpBasicOptionsOfficialRules.ResumeLayout(false);
            this.tlpBasicOptionsOfficialRules.PerformLayout();
            this.gpbBasicOptionsEncumbrance.ResumeLayout(false);
            this.gpbBasicOptionsEncumbrance.PerformLayout();
            this.tlpBasicOptionsEncumbrance.ResumeLayout(false);
            this.tlpBasicOptionsEncumbrance.PerformLayout();
            this.tlpEncumbranceInterval.ResumeLayout(false);
            this.tlpEncumbranceInterval.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudEncumbrancePenaltyPhysicalLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEncumbrancePenaltyAgility)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEncumbrancePenaltyReaction)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEncumbrancePenaltyWoundModifier)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEncumbrancePenaltyMovementSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWeightDecimals)).EndInit();
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
            this.gpbBasicOptionsInitiativeDice.ResumeLayout(false);
            this.gpbBasicOptionsInitiativeDice.PerformLayout();
            this.tlpBasicOptionsInitiativeDice.ResumeLayout(false);
            this.tlpBasicOptionsInitiativeDice.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinInitiativeDice)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxInitiativeDice)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxAstralInitiativeDice)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxColdSimInitiativeDice)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxHotSimInitiativeDice)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinAstralInitiativeDice)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinColdSimInitiativeDice)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinHotSimInitiativeDice)).EndInit();
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
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpirit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaJoinGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaCarryover)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSummoningFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpellShapingFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSustainingFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaWeaponFocus)).EndInit();
            this.flpKarmaInitiation.ResumeLayout(false);
            this.flpKarmaInitiation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaTechnique)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaMetamagic)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaInitiation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaInitiationFlat)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewAIAdvancedProgram)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewAIProgram)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNewComplexForm)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaMysticAdeptPowerPoint)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaSpiritFettering)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaQuality)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMetatypeCostsKarmaMultiplier)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNuyenPerWftM)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaNuyenPerWftP)).EndInit();
            this.tabCustomData.ResumeLayout(false);
            this.tabCustomData.PerformLayout();
            this.tlpOptionalRules.ResumeLayout(false);
            this.tlpOptionalRules.PerformLayout();
            this.gpbDirectoryInfo.ResumeLayout(false);
            this.gpbDirectoryInfo.PerformLayout();
            this.tlpDirectoryInfo.ResumeLayout(false);
            this.tlpDirectoryInfo.PerformLayout();
            this.gbpDirectoryInfoDependencies.ResumeLayout(false);
            this.pnlDirectoryDependencies.ResumeLayout(false);
            this.pnlDirectoryDependencies.PerformLayout();
            this.gbpDirectoryInfoIncompatibilities.ResumeLayout(false);
            this.pnlDirectoryIncompatibilities.ResumeLayout(false);
            this.pnlDirectoryIncompatibilities.PerformLayout();
            this.tlpDirectoryInfoLeft.ResumeLayout(false);
            this.tlpDirectoryInfoLeft.PerformLayout();
            this.gpbDirectoryAuthors.ResumeLayout(false);
            this.pnlDirectoryAuthors.ResumeLayout(false);
            this.pnlDirectoryAuthors.PerformLayout();
            this.tlpOptionalRulesButtons.ResumeLayout(false);
            this.tlpOptionalRulesButtons.PerformLayout();
            this.tabHouseRules.ResumeLayout(false);
            this.flpHouseRules.ResumeLayout(false);
            this.flpHouseRules.PerformLayout();
            this.gpbHouseRulesQualities.ResumeLayout(false);
            this.gpbHouseRulesQualities.PerformLayout();
            this.tlpHouseRulesQualities.ResumeLayout(false);
            this.tlpHouseRulesQualities.PerformLayout();
            this.gpbHouseRulesAttributes.ResumeLayout(false);
            this.gpbHouseRulesAttributes.PerformLayout();
            this.tlpHouseRulesAttributes.ResumeLayout(false);
            this.tlpHouseRulesAttributes.PerformLayout();
            this.flpDroneArmorMultiplier.ResumeLayout(false);
            this.flpDroneArmorMultiplier.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDroneArmorMultiplier)).EndInit();
            this.gpbHouseRulesSkills.ResumeLayout(false);
            this.gpbHouseRulesSkills.PerformLayout();
            this.tlpHouseRulesSkills.ResumeLayout(false);
            this.tlpHouseRulesSkills.PerformLayout();
            this.flpMaxSkillRating.ResumeLayout(false);
            this.flpMaxSkillRating.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxSkillRating)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxKnowledgeSkillRating)).EndInit();
            this.gpbHouseRulesCombat.ResumeLayout(false);
            this.gpbHouseRulesCombat.PerformLayout();
            this.tlpHouseRulesCombat.ResumeLayout(false);
            this.tlpHouseRulesCombat.PerformLayout();
            this.gpbHouseRulesMagicResonance.ResumeLayout(false);
            this.gpbHouseRulesMagicResonance.PerformLayout();
            this.tlpHouseRulesMagicResonance.ResumeLayout(false);
            this.tlpHouseRulesMagicResonance.PerformLayout();
            this.gpbHouseRules4eAdaptations.ResumeLayout(false);
            this.gpbHouseRules4eAdaptations.PerformLayout();
            this.tlpHouseRules4eAdaptations.ResumeLayout(false);
            this.tlpHouseRules4eAdaptations.PerformLayout();
            this.flpKarmaGainedFromEnemies.ResumeLayout(false);
            this.flpKarmaGainedFromEnemies.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarmaGainedFromEnemies)).EndInit();
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
        private System.Windows.Forms.Label lblKarmaNuyenPerExtraWftM;
        private NumericUpDownEx nudKarmaSpellcastingFocus;
        private NumericUpDownEx nudKarmaNuyenPerWftM;
        private System.Windows.Forms.Label lblKarmaSpiritExtra;
        private System.Windows.Forms.Label lblKarmaNuyenPerWftM;
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
        private BufferedTableLayoutPanel tlpHouseRulesAttributes;
        private System.Windows.Forms.GroupBox gpbHouseRulesSkills;
        private BufferedTableLayoutPanel tlpHouseRulesSkills;
        private System.Windows.Forms.GroupBox gpbHouseRulesCombat;
        private BufferedTableLayoutPanel tlpHouseRulesCombat;
        private System.Windows.Forms.GroupBox gpbHouseRulesMagicResonance;
        private BufferedTableLayoutPanel tlpHouseRulesMagicResonance;
        private System.Windows.Forms.Button cmdGlobalOptionsCustomData;
        private System.Windows.Forms.Button cmdRename;
        private System.Windows.Forms.Label lblKarmaSpiritFettering;
        private NumericUpDownEx nudKarmaSpiritFettering;
        private System.Windows.Forms.Label lblKarmaSpiritFetteringExtra;
        private System.Windows.Forms.GroupBox gpbDirectoryInfo;
        private BufferedTableLayoutPanel tlpDirectoryInfo;
        private System.Windows.Forms.Label lblDirectoryNameLabel;
        private System.Windows.Forms.Label lblDirectoryVersion;
        private System.Windows.Forms.Label lblDirectoryName;
        private System.Windows.Forms.Label lblDirectoryVersionLabel;
        private System.Windows.Forms.GroupBox gbpDirectoryInfoIncompatibilities;
        private System.Windows.Forms.Label lblDirectoryAuthors;
        private System.Windows.Forms.GroupBox gbpDirectoryInfoDependencies;
        private System.Windows.Forms.Label lblDependencies;
        private System.Windows.Forms.Label lblIncompatibilities;
        private System.Windows.Forms.Panel pnlDirectoryDependencies;
        private System.Windows.Forms.Panel pnlDirectoryIncompatibilities;
        private BufferedTableLayoutPanel tlpOptionalRulesButtons;
        private BufferedTableLayoutPanel tlpDirectoryInfoLeft;
        private System.Windows.Forms.GroupBox gpbDirectoryAuthors;
        private System.Windows.Forms.Panel pnlDirectoryAuthors;
        private LabelWithToolTip lblNuyenExpression;
        private System.Windows.Forms.TextBox txtNuyenExpression;
        private System.Windows.Forms.Label lblKarmaNuyenPerExtraWftP;
        private System.Windows.Forms.Label lblKarmaNuyenPerWftP;
        private NumericUpDownEx nudKarmaNuyenPerWftP;
        private System.Windows.Forms.Button cmdToBottomCustomDirectoryLoadOrder;
        private System.Windows.Forms.Button cmdToTopCustomDirectoryLoadOrder;
        private LabelWithToolTip lblRegisteredSprites;
        private LabelWithToolTip lblBoundSpiritLimit;
        private System.Windows.Forms.TextBox txtBoundSpiritLimit;
        private System.Windows.Forms.TextBox txtRegisteredSpriteLimit;
        private System.Windows.Forms.GroupBox gpbHouseRules4eAdaptations;
        private BufferedTableLayoutPanel tlpHouseRules4eAdaptations;
        private ColorableCheckBox chkEnemyKarmaQualityLimit;
        private ColorableCheckBox chkEnable4eStyleEnemyTracking;
        private System.Windows.Forms.FlowLayoutPanel flpKarmaGainedFromEnemies;
        private System.Windows.Forms.Label lblKarmaGainedFromEnemies;
        private NumericUpDownEx nudKarmaGainedFromEnemies;
        private System.Windows.Forms.Label lblKarmaGainedFromEnemiesExtra;
        private System.Windows.Forms.GroupBox gpbBasicOptionsInitiativeDice;
        private BufferedTableLayoutPanel tlpBasicOptionsInitiativeDice;
        private System.Windows.Forms.Label lblInitiativeDiceLabel;
        private System.Windows.Forms.Label lblAstralInitiativeDiceLabel;
        private System.Windows.Forms.Label lblMinInitiativeDiceLabel;
        private System.Windows.Forms.Label lblHotSimInitiativeDiceLabel;
        private System.Windows.Forms.Label lblColdSimInitiativeDiceLabel;
        private NumericUpDownEx nudMinInitiativeDice;
        private NumericUpDownEx nudMaxInitiativeDice;
        private System.Windows.Forms.Label lblMaxInitiativeDiceLabel;
        private NumericUpDownEx nudMaxAstralInitiativeDice;
        private NumericUpDownEx nudMaxColdSimInitiativeDice;
        private NumericUpDownEx nudMaxHotSimInitiativeDice;
        private NumericUpDownEx nudMinAstralInitiativeDice;
        private NumericUpDownEx nudMinColdSimInitiativeDice;
        private NumericUpDownEx nudMinHotSimInitiativeDice;
        private System.Windows.Forms.RichTextBox rtbDirectoryDescription;
        private System.Windows.Forms.GroupBox gpbBasicOptionsEncumbrance;
        private BufferedTableLayoutPanel tlpBasicOptionsEncumbrance;
        private LabelWithToolTip lblCarryLimit;
        private LabelWithToolTip lblLiftLimit;
        private System.Windows.Forms.TextBox txtLiftLimit;
        private System.Windows.Forms.TextBox txtCarryLimit;
        private LabelWithToolTip lblEncumbranceIntervalLeft;
        private LabelWithToolTip lblEncumbranceIntervalRight;
        private System.Windows.Forms.TextBox txtEncumbranceInterval;
        private System.Windows.Forms.Label lblEncumbrancePenaltiesHeader;
        private BufferedTableLayoutPanel tlpEncumbranceInterval;
        private ColorableCheckBox chkEncumbrancePenaltyPhysicalLimit;
        private ColorableCheckBox chkEncumbrancePenaltyMovementSpeed;
        private ColorableCheckBox chkEncumbrancePenaltyAgility;
        private ColorableCheckBox chkEncumbrancePenaltyReaction;
        private ColorableCheckBox chkEncumbrancePenaltyWoundModifier;
        private NumericUpDownEx nudEncumbrancePenaltyPhysicalLimit;
        private NumericUpDownEx nudEncumbrancePenaltyAgility;
        private NumericUpDownEx nudEncumbrancePenaltyReaction;
        private NumericUpDownEx nudEncumbrancePenaltyWoundModifier;
        private NumericUpDownEx nudEncumbrancePenaltyMovementSpeed;
        private LabelWithToolTip lblEncumbrancePenaltyMovementSpeedPercent;
        private LabelWithToolTip lblWeightDecimalPlaces;
        private NumericUpDownEx nudWeightDecimals;
        private LabelWithToolTip lblMaxSkillRatingCreate;
        private NumericUpDownEx nudMaxSkillRatingCreate;
        private LabelWithToolTip lblMaxNumberMaxAttributes;
        private NumericUpDownEx nudMaxNumberMaxAttributes;
        private LabelWithToolTip lblMaxKnowledgeSkillRatingCreate;
        private NumericUpDownEx nudMaxKnowledgeSkillRatingCreate;
        private System.Windows.Forms.FlowLayoutPanel flpMaxSkillRating;
        private System.Windows.Forms.Label lblMaxSkillRating;
        private NumericUpDownEx nudMaxSkillRating;
        private System.Windows.Forms.Label lblMaxKnowledgeSkillRating;
        private NumericUpDownEx nudMaxKnowledgeSkillRating;
        private ColorableCheckBox chkSpecializationsBreakSkillGroups;
    }
}
