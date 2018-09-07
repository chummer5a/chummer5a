namespace Chummer
{
    partial class frmSelectLifestyleAdvanced
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Positive Qualities");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Negative Qualities");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Entertainments");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Free Matrix Grids");
            this.Label_SelectAdvancedLifestyle_Upgrade_Comforts = new System.Windows.Forms.Label();
            this.cmdOKAdd = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lblTotalLPLabel = new System.Windows.Forms.Label();
            this.lblTotalLP = new System.Windows.Forms.Label();
            this.lblCostLabel = new System.Windows.Forms.Label();
            this.lblCost = new System.Windows.Forms.Label();
            this.lblLifestyleNameLabel = new System.Windows.Forms.Label();
            this.txtLifestyleName = new System.Windows.Forms.TextBox();
            this.nudPercentage = new System.Windows.Forms.NumericUpDown();
            this.lblPercentage = new System.Windows.Forms.Label();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.nudRoommates = new System.Windows.Forms.NumericUpDown();
            this.lblRoommates = new System.Windows.Forms.Label();
            this.cboBaseLifestyle = new System.Windows.Forms.ComboBox();
            this.Label_SelectAdvancedLifestyle_Base_Lifestyle = new System.Windows.Forms.Label();
            this.nudSecurity = new System.Windows.Forms.NumericUpDown();
            this.nudArea = new System.Windows.Forms.NumericUpDown();
            this.nudComforts = new System.Windows.Forms.NumericUpDown();
            this.Label_SelectAdvancedLifestyle_Base_Comforts = new System.Windows.Forms.Label();
            this.Label_SelectAdvancedLifestyle_Base_Area = new System.Windows.Forms.Label();
            this.Label_SelectAdvancedLifestyle_Base_Securities = new System.Windows.Forms.Label();
            this.cmdAddQuality = new System.Windows.Forms.Button();
            this.cmdDeleteQuality = new System.Windows.Forms.Button();
            this.treLifestyleQualities = new System.Windows.Forms.TreeView();
            this.chkTrustFund = new System.Windows.Forms.CheckBox();
            this.lblQualityLp = new System.Windows.Forms.Label();
            this.lblQualityLPLabel = new System.Windows.Forms.Label();
            this.lblQualitySource = new System.Windows.Forms.Label();
            this.lblQualitySourceLabel = new System.Windows.Forms.Label();
            this.lblQualityCost = new System.Windows.Forms.Label();
            this.lblQualityCostLabel = new System.Windows.Forms.Label();
            this.chkPrimaryTenant = new System.Windows.Forms.CheckBox();
            this.lblSecurityTotal = new System.Windows.Forms.Label();
            this.lblAreaTotal = new System.Windows.Forms.Label();
            this.lblComfortTotal = new System.Windows.Forms.Label();
            this.chkQualityContributesLP = new System.Windows.Forms.CheckBox();
            this.lblBonusLP = new System.Windows.Forms.Label();
            this.nudBonusLP = new System.Windows.Forms.NumericUpDown();
            this.chkBonusLPRandomize = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudPercentage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRoommates)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSecurity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudArea)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudComforts)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBonusLP)).BeginInit();
            this.SuspendLayout();
            // 
            // Label_SelectAdvancedLifestyle_Upgrade_Comforts
            // 
            this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.AutoSize = true;
            this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.Location = new System.Drawing.Point(374, 102);
            this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.Name = "Label_SelectAdvancedLifestyle_Upgrade_Comforts";
            this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.Size = new System.Drawing.Size(51, 13);
            this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.TabIndex = 2;
            this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.Tag = "Label_SelectAdvancedLifestyle_Rating";
            this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.Text = "Upgrade:";
            // 
            // cmdOKAdd
            // 
            this.cmdOKAdd.Location = new System.Drawing.Point(404, 451);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(75, 23);
            this.cmdOKAdd.TabIndex = 27;
            this.cmdOKAdd.Tag = "String_AddMore";
            this.cmdOKAdd.Text = "&Add && More";
            this.cmdOKAdd.UseVisualStyleBackColor = true;
            this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(323, 451);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 28;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.Location = new System.Drawing.Point(485, 451);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 26;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lblTotalLPLabel
            // 
            this.lblTotalLPLabel.AutoSize = true;
            this.lblTotalLPLabel.Location = new System.Drawing.Point(272, 199);
            this.lblTotalLPLabel.Name = "lblTotalLPLabel";
            this.lblTotalLPLabel.Size = new System.Drawing.Size(63, 13);
            this.lblTotalLPLabel.TabIndex = 12;
            this.lblTotalLPLabel.Tag = "Label_SelectAdvancedLifestyle_UnusedLP";
            this.lblTotalLPLabel.Text = "Unused LP:";
            // 
            // lblTotalLP
            // 
            this.lblTotalLP.AutoSize = true;
            this.lblTotalLP.Location = new System.Drawing.Point(344, 199);
            this.lblTotalLP.Name = "lblTotalLP";
            this.lblTotalLP.Size = new System.Drawing.Size(26, 13);
            this.lblTotalLP.TabIndex = 13;
            this.lblTotalLP.Text = "[LP]";
            // 
            // lblCostLabel
            // 
            this.lblCostLabel.AutoSize = true;
            this.lblCostLabel.Location = new System.Drawing.Point(269, 271);
            this.lblCostLabel.Name = "lblCostLabel";
            this.lblCostLabel.Size = new System.Drawing.Size(66, 13);
            this.lblCostLabel.TabIndex = 18;
            this.lblCostLabel.Tag = "Label_SelectLifestyle_CostPerMonth";
            this.lblCostLabel.Text = "Cost/Month:";
            // 
            // lblCost
            // 
            this.lblCost.AutoSize = true;
            this.lblCost.Location = new System.Drawing.Point(344, 271);
            this.lblCost.Name = "lblCost";
            this.lblCost.Size = new System.Drawing.Size(34, 13);
            this.lblCost.TabIndex = 19;
            this.lblCost.Text = "[Cost]";
            // 
            // lblLifestyleNameLabel
            // 
            this.lblLifestyleNameLabel.AutoSize = true;
            this.lblLifestyleNameLabel.Location = new System.Drawing.Point(287, 17);
            this.lblLifestyleNameLabel.Name = "lblLifestyleNameLabel";
            this.lblLifestyleNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblLifestyleNameLabel.TabIndex = 0;
            this.lblLifestyleNameLabel.Tag = "Label_Name";
            this.lblLifestyleNameLabel.Text = "Name:";
            // 
            // txtLifestyleName
            // 
            this.txtLifestyleName.Location = new System.Drawing.Point(331, 14);
            this.txtLifestyleName.Name = "txtLifestyleName";
            this.txtLifestyleName.Size = new System.Drawing.Size(229, 20);
            this.txtLifestyleName.TabIndex = 1;
            // 
            // nudPercentage
            // 
            this.nudPercentage.DecimalPlaces = 2;
            this.nudPercentage.Location = new System.Drawing.Point(347, 246);
            this.nudPercentage.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            131072});
            this.nudPercentage.Name = "nudPercentage";
            this.nudPercentage.Size = new System.Drawing.Size(67, 20);
            this.nudPercentage.TabIndex = 17;
            this.nudPercentage.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudPercentage.ValueChanged += new System.EventHandler(this.nudPercentage_ValueChanged);
            // 
            // lblPercentage
            // 
            this.lblPercentage.AutoSize = true;
            this.lblPercentage.Location = new System.Drawing.Point(284, 248);
            this.lblPercentage.Name = "lblPercentage";
            this.lblPercentage.Size = new System.Drawing.Size(51, 13);
            this.lblPercentage.TabIndex = 16;
            this.lblPercentage.Tag = "Label_SelectLifestyle_PercentToPay";
            this.lblPercentage.Text = "% to Pay:";
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.lblSource.Location = new System.Drawing.Point(344, 293);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 21;
            this.lblSource.Text = "[Source]";
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(291, 293);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 20;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            // 
            // nudRoommates
            // 
            this.nudRoommates.Location = new System.Drawing.Point(347, 220);
            this.nudRoommates.Name = "nudRoommates";
            this.nudRoommates.Size = new System.Drawing.Size(67, 20);
            this.nudRoommates.TabIndex = 15;
            this.nudRoommates.ValueChanged += new System.EventHandler(this.nudRoommates_ValueChanged);
            // 
            // lblRoommates
            // 
            this.lblRoommates.AutoSize = true;
            this.lblRoommates.Location = new System.Drawing.Point(269, 222);
            this.lblRoommates.Name = "lblRoommates";
            this.lblRoommates.Size = new System.Drawing.Size(66, 13);
            this.lblRoommates.TabIndex = 14;
            this.lblRoommates.Tag = "Label_SelectLifestyle_Roommates";
            this.lblRoommates.Text = "Roommates:";
            // 
            // cboBaseLifestyle
            // 
            this.cboBaseLifestyle.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cboBaseLifestyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBaseLifestyle.FormattingEnabled = true;
            this.cboBaseLifestyle.Location = new System.Drawing.Point(331, 40);
            this.cboBaseLifestyle.Name = "cboBaseLifestyle";
            this.cboBaseLifestyle.Size = new System.Drawing.Size(229, 21);
            this.cboBaseLifestyle.TabIndex = 32;
            this.cboBaseLifestyle.SelectedIndexChanged += new System.EventHandler(this.cboBaseLifestyle_SelectedIndexChanged);
            // 
            // Label_SelectAdvancedLifestyle_Base_Lifestyle
            // 
            this.Label_SelectAdvancedLifestyle_Base_Lifestyle.AutoSize = true;
            this.Label_SelectAdvancedLifestyle_Base_Lifestyle.Location = new System.Drawing.Point(277, 43);
            this.Label_SelectAdvancedLifestyle_Base_Lifestyle.Name = "Label_SelectAdvancedLifestyle_Base_Lifestyle";
            this.Label_SelectAdvancedLifestyle_Base_Lifestyle.Size = new System.Drawing.Size(48, 13);
            this.Label_SelectAdvancedLifestyle_Base_Lifestyle.TabIndex = 31;
            this.Label_SelectAdvancedLifestyle_Base_Lifestyle.Tag = "Label_SelectAdvancedLifestyle_Base_Lifestyle";
            this.Label_SelectAdvancedLifestyle_Base_Lifestyle.Text = "Lifestyle:";
            // 
            // nudSecurity
            // 
            this.nudSecurity.Location = new System.Drawing.Point(377, 170);
            this.nudSecurity.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudSecurity.Name = "nudSecurity";
            this.nudSecurity.Size = new System.Drawing.Size(48, 20);
            this.nudSecurity.TabIndex = 41;
            this.nudSecurity.ValueChanged += new System.EventHandler(this.nudSecurity_ValueChanged);
            // 
            // nudArea
            // 
            this.nudArea.Location = new System.Drawing.Point(377, 144);
            this.nudArea.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudArea.Name = "nudArea";
            this.nudArea.Size = new System.Drawing.Size(48, 20);
            this.nudArea.TabIndex = 40;
            this.nudArea.ValueChanged += new System.EventHandler(this.nudArea_ValueChanged);
            // 
            // nudComforts
            // 
            this.nudComforts.Location = new System.Drawing.Point(377, 118);
            this.nudComforts.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudComforts.Name = "nudComforts";
            this.nudComforts.Size = new System.Drawing.Size(48, 20);
            this.nudComforts.TabIndex = 39;
            this.nudComforts.ValueChanged += new System.EventHandler(this.nudComforts_ValueChanged);
            // 
            // Label_SelectAdvancedLifestyle_Base_Comforts
            // 
            this.Label_SelectAdvancedLifestyle_Base_Comforts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Label_SelectAdvancedLifestyle_Base_Comforts.Location = new System.Drawing.Point(278, 120);
            this.Label_SelectAdvancedLifestyle_Base_Comforts.Name = "Label_SelectAdvancedLifestyle_Base_Comforts";
            this.Label_SelectAdvancedLifestyle_Base_Comforts.Size = new System.Drawing.Size(93, 13);
            this.Label_SelectAdvancedLifestyle_Base_Comforts.TabIndex = 42;
            this.Label_SelectAdvancedLifestyle_Base_Comforts.Tag = "Label_SelectAdvancedLifestyle_Base_Comforts";
            this.Label_SelectAdvancedLifestyle_Base_Comforts.Text = "Comforts: [{0}/{1}]";
            this.Label_SelectAdvancedLifestyle_Base_Comforts.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Label_SelectAdvancedLifestyle_Base_Area
            // 
            this.Label_SelectAdvancedLifestyle_Base_Area.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Label_SelectAdvancedLifestyle_Base_Area.Location = new System.Drawing.Point(278, 146);
            this.Label_SelectAdvancedLifestyle_Base_Area.Name = "Label_SelectAdvancedLifestyle_Base_Area";
            this.Label_SelectAdvancedLifestyle_Base_Area.Size = new System.Drawing.Size(93, 13);
            this.Label_SelectAdvancedLifestyle_Base_Area.TabIndex = 46;
            this.Label_SelectAdvancedLifestyle_Base_Area.Tag = "Label_SelectAdvancedLifestyle_Base_Area";
            this.Label_SelectAdvancedLifestyle_Base_Area.Text = "Area: [{0}/{1}]";
            this.Label_SelectAdvancedLifestyle_Base_Area.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Label_SelectAdvancedLifestyle_Base_Securities
            // 
            this.Label_SelectAdvancedLifestyle_Base_Securities.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Label_SelectAdvancedLifestyle_Base_Securities.Location = new System.Drawing.Point(278, 172);
            this.Label_SelectAdvancedLifestyle_Base_Securities.Name = "Label_SelectAdvancedLifestyle_Base_Securities";
            this.Label_SelectAdvancedLifestyle_Base_Securities.Size = new System.Drawing.Size(93, 13);
            this.Label_SelectAdvancedLifestyle_Base_Securities.TabIndex = 48;
            this.Label_SelectAdvancedLifestyle_Base_Securities.Tag = "Label_SelectAdvancedLifestyle_Base_Security";
            this.Label_SelectAdvancedLifestyle_Base_Securities.Text = "Security: [{0}/{1}]";
            this.Label_SelectAdvancedLifestyle_Base_Securities.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cmdAddQuality
            // 
            this.cmdAddQuality.Location = new System.Drawing.Point(12, 12);
            this.cmdAddQuality.Name = "cmdAddQuality";
            this.cmdAddQuality.Size = new System.Drawing.Size(122, 23);
            this.cmdAddQuality.TabIndex = 51;
            this.cmdAddQuality.Tag = "Button_AddQuality";
            this.cmdAddQuality.Text = "Add Quality";
            this.cmdAddQuality.UseVisualStyleBackColor = true;
            this.cmdAddQuality.Click += new System.EventHandler(this.cmdAddQuality_Click);
            // 
            // cmdDeleteQuality
            // 
            this.cmdDeleteQuality.Enabled = false;
            this.cmdDeleteQuality.Location = new System.Drawing.Point(142, 12);
            this.cmdDeleteQuality.Name = "cmdDeleteQuality";
            this.cmdDeleteQuality.Size = new System.Drawing.Size(108, 23);
            this.cmdDeleteQuality.TabIndex = 52;
            this.cmdDeleteQuality.Tag = "Button_DeleteQuality";
            this.cmdDeleteQuality.Text = "Delete Quality";
            this.cmdDeleteQuality.UseVisualStyleBackColor = true;
            this.cmdDeleteQuality.Click += new System.EventHandler(this.cmdDeleteQuality_Click);
            // 
            // treLifestyleQualities
            // 
            this.treLifestyleQualities.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treLifestyleQualities.HideSelection = false;
            this.treLifestyleQualities.Location = new System.Drawing.Point(12, 41);
            this.treLifestyleQualities.Name = "treLifestyleQualities";
            treeNode1.Name = "nodPositiveLifestyleQualities";
            treeNode1.Tag = "Node_SelectAdvancedLifestyle_PositiveQualities";
            treeNode1.Text = "Positive Qualities";
            treeNode2.Name = "nodNegativeLifestyleQualities";
            treeNode2.Tag = "Node_SelectAdvancedLifestyle_NegativeQualities";
            treeNode2.Text = "Negative Qualities";
            treeNode3.Name = "nodLifestyleEntertainments";
            treeNode3.Tag = "Node_SelectAdvancedLifestyle_Entertainments";
            treeNode3.Text = "Entertainments";
            treeNode4.Name = "nodFreeMatrixGrids";
            treeNode4.Tag = "Node_SelectAdvancedLifestyle_FreeMatrixGrids";
            treeNode4.Text = "Free Matrix Grids";
            this.treLifestyleQualities.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3,
            treeNode4});
            this.treLifestyleQualities.Size = new System.Drawing.Size(238, 433);
            this.treLifestyleQualities.TabIndex = 53;
            this.treLifestyleQualities.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treLifestyleQualities_AfterSelect);
            // 
            // chkTrustFund
            // 
            this.chkTrustFund.AutoSize = true;
            this.chkTrustFund.Location = new System.Drawing.Point(420, 247);
            this.chkTrustFund.Name = "chkTrustFund";
            this.chkTrustFund.Size = new System.Drawing.Size(77, 17);
            this.chkTrustFund.TabIndex = 54;
            this.chkTrustFund.Text = "Trust Fund";
            this.chkTrustFund.UseVisualStyleBackColor = true;
            this.chkTrustFund.Visible = false;
            this.chkTrustFund.CheckedChanged += new System.EventHandler(this.chkTrustFund_Changed);
            // 
            // lblQualityLp
            // 
            this.lblQualityLp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblQualityLp.AutoSize = true;
            this.lblQualityLp.Location = new System.Drawing.Point(341, 341);
            this.lblQualityLp.Name = "lblQualityLp";
            this.lblQualityLp.Size = new System.Drawing.Size(26, 13);
            this.lblQualityLp.TabIndex = 71;
            this.lblQualityLp.Text = "[LP]";
            // 
            // lblQualityLPLabel
            // 
            this.lblQualityLPLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblQualityLPLabel.AutoSize = true;
            this.lblQualityLPLabel.Location = new System.Drawing.Point(277, 341);
            this.lblQualityLPLabel.Name = "lblQualityLPLabel";
            this.lblQualityLPLabel.Size = new System.Drawing.Size(58, 13);
            this.lblQualityLPLabel.TabIndex = 70;
            this.lblQualityLPLabel.Tag = "Label_QualityLP";
            this.lblQualityLPLabel.Text = "Quality LP:";
            this.lblQualityLPLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblQualityLPLabel.Visible = false;
            // 
            // lblQualitySource
            // 
            this.lblQualitySource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblQualitySource.AutoSize = true;
            this.lblQualitySource.Location = new System.Drawing.Point(341, 386);
            this.lblQualitySource.Name = "lblQualitySource";
            this.lblQualitySource.Size = new System.Drawing.Size(47, 13);
            this.lblQualitySource.TabIndex = 69;
            this.lblQualitySource.Text = "[Source]";
            // 
            // lblQualitySourceLabel
            // 
            this.lblQualitySourceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblQualitySourceLabel.AutoSize = true;
            this.lblQualitySourceLabel.Location = new System.Drawing.Point(256, 386);
            this.lblQualitySourceLabel.Name = "lblQualitySourceLabel";
            this.lblQualitySourceLabel.Size = new System.Drawing.Size(79, 13);
            this.lblQualitySourceLabel.TabIndex = 68;
            this.lblQualitySourceLabel.Tag = "Label_QualitySource";
            this.lblQualitySourceLabel.Text = "Quality Source:";
            this.lblQualitySourceLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblQualitySourceLabel.Visible = false;
            // 
            // lblQualityCost
            // 
            this.lblQualityCost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblQualityCost.AutoSize = true;
            this.lblQualityCost.Location = new System.Drawing.Point(341, 364);
            this.lblQualityCost.Name = "lblQualityCost";
            this.lblQualityCost.Size = new System.Drawing.Size(34, 13);
            this.lblQualityCost.TabIndex = 73;
            this.lblQualityCost.Text = "[Cost]";
            // 
            // lblQualityCostLabel
            // 
            this.lblQualityCostLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblQualityCostLabel.AutoSize = true;
            this.lblQualityCostLabel.Location = new System.Drawing.Point(269, 364);
            this.lblQualityCostLabel.Name = "lblQualityCostLabel";
            this.lblQualityCostLabel.Size = new System.Drawing.Size(66, 13);
            this.lblQualityCostLabel.TabIndex = 72;
            this.lblQualityCostLabel.Tag = "Label_QualityCost";
            this.lblQualityCostLabel.Text = "Quality Cost:";
            this.lblQualityCostLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblQualityCostLabel.Visible = false;
            // 
            // chkPrimaryTenant
            // 
            this.chkPrimaryTenant.AutoSize = true;
            this.chkPrimaryTenant.Location = new System.Drawing.Point(420, 221);
            this.chkPrimaryTenant.Name = "chkPrimaryTenant";
            this.chkPrimaryTenant.Size = new System.Drawing.Size(97, 17);
            this.chkPrimaryTenant.TabIndex = 74;
            this.chkPrimaryTenant.Tag = "Label_SelectAdvancedLifestyle_Tenant";
            this.chkPrimaryTenant.Text = "Primary Tenant";
            this.chkPrimaryTenant.UseVisualStyleBackColor = true;
            this.chkPrimaryTenant.CheckedChanged += new System.EventHandler(this.chkPrimaryTenant_CheckedChanged);
            // 
            // lblSecurityTotal
            // 
            this.lblSecurityTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSecurityTotal.Location = new System.Drawing.Point(431, 172);
            this.lblSecurityTotal.Name = "lblSecurityTotal";
            this.lblSecurityTotal.Size = new System.Drawing.Size(31, 13);
            this.lblSecurityTotal.TabIndex = 77;
            this.lblSecurityTotal.Tag = "";
            this.lblSecurityTotal.Text = "[0]";
            this.lblSecurityTotal.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblAreaTotal
            // 
            this.lblAreaTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAreaTotal.Location = new System.Drawing.Point(431, 146);
            this.lblAreaTotal.Name = "lblAreaTotal";
            this.lblAreaTotal.Size = new System.Drawing.Size(31, 13);
            this.lblAreaTotal.TabIndex = 76;
            this.lblAreaTotal.Tag = "";
            this.lblAreaTotal.Text = "[0]";
            this.lblAreaTotal.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblComfortTotal
            // 
            this.lblComfortTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblComfortTotal.Location = new System.Drawing.Point(431, 120);
            this.lblComfortTotal.Name = "lblComfortTotal";
            this.lblComfortTotal.Size = new System.Drawing.Size(31, 13);
            this.lblComfortTotal.TabIndex = 75;
            this.lblComfortTotal.Tag = "";
            this.lblComfortTotal.Text = "[0]";
            this.lblComfortTotal.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // chkQualityContributesLP
            // 
            this.chkQualityContributesLP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkQualityContributesLP.AutoSize = true;
            this.chkQualityContributesLP.Location = new System.Drawing.Point(420, 341);
            this.chkQualityContributesLP.Name = "chkQualityContributesLP";
            this.chkQualityContributesLP.Size = new System.Drawing.Size(142, 17);
            this.chkQualityContributesLP.TabIndex = 78;
            this.chkQualityContributesLP.Tag = "Label_SelectAdvancedLifestyle_LPContribution";
            this.chkQualityContributesLP.Text = "Quality Contributes to LP";
            this.chkQualityContributesLP.UseVisualStyleBackColor = true;
            this.chkQualityContributesLP.Visible = false;
            this.chkQualityContributesLP.CheckedChanged += new System.EventHandler(this.chkQualityContributesLP_CheckedChanged);
            // 
            // lblBonusLP
            // 
            this.lblBonusLP.AutoSize = true;
            this.lblBonusLP.Location = new System.Drawing.Point(268, 69);
            this.lblBonusLP.Name = "lblBonusLP";
            this.lblBonusLP.Size = new System.Drawing.Size(56, 13);
            this.lblBonusLP.TabIndex = 81;
            this.lblBonusLP.Tag = "Label_BonusLP";
            this.lblBonusLP.Text = "Bonus LP:";
            this.lblBonusLP.Visible = false;
            // 
            // nudBonusLP
            // 
            this.nudBonusLP.Location = new System.Drawing.Point(330, 67);
            this.nudBonusLP.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.nudBonusLP.Name = "nudBonusLP";
            this.nudBonusLP.Size = new System.Drawing.Size(48, 20);
            this.nudBonusLP.TabIndex = 82;
            this.nudBonusLP.Visible = false;
            this.nudBonusLP.ValueChanged += new System.EventHandler(this.nudBonusLP_ValueChanged);
            // 
            // chkBonusLPRandomize
            // 
            this.chkBonusLPRandomize.AutoSize = true;
            this.chkBonusLPRandomize.Checked = true;
            this.chkBonusLPRandomize.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBonusLPRandomize.Location = new System.Drawing.Point(384, 68);
            this.chkBonusLPRandomize.Name = "chkBonusLPRandomize";
            this.chkBonusLPRandomize.Size = new System.Drawing.Size(102, 17);
            this.chkBonusLPRandomize.TabIndex = 83;
            this.chkBonusLPRandomize.Tag = "Checkbox_Randomize1D6";
            this.chkBonusLPRandomize.Text = "Randomize 1D6";
            this.chkBonusLPRandomize.UseVisualStyleBackColor = true;
            this.chkBonusLPRandomize.Visible = false;
            this.chkBonusLPRandomize.CheckedChanged += new System.EventHandler(this.chkTravelerBonusLPRandomize_CheckedChanged);
            // 
            // frmSelectLifestyleAdvanced
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(572, 486);
            this.Controls.Add(this.chkBonusLPRandomize);
            this.Controls.Add(this.nudBonusLP);
            this.Controls.Add(this.lblBonusLP);
            this.Controls.Add(this.chkQualityContributesLP);
            this.Controls.Add(this.lblSecurityTotal);
            this.Controls.Add(this.lblAreaTotal);
            this.Controls.Add(this.lblComfortTotal);
            this.Controls.Add(this.chkPrimaryTenant);
            this.Controls.Add(this.lblQualityCost);
            this.Controls.Add(this.lblQualityCostLabel);
            this.Controls.Add(this.lblQualityLp);
            this.Controls.Add(this.lblQualityLPLabel);
            this.Controls.Add(this.lblQualitySource);
            this.Controls.Add(this.lblQualitySourceLabel);
            this.Controls.Add(this.chkTrustFund);
            this.Controls.Add(this.treLifestyleQualities);
            this.Controls.Add(this.cmdDeleteQuality);
            this.Controls.Add(this.cmdAddQuality);
            this.Controls.Add(this.Label_SelectAdvancedLifestyle_Base_Securities);
            this.Controls.Add(this.Label_SelectAdvancedLifestyle_Base_Area);
            this.Controls.Add(this.Label_SelectAdvancedLifestyle_Base_Comforts);
            this.Controls.Add(this.nudSecurity);
            this.Controls.Add(this.nudArea);
            this.Controls.Add(this.nudComforts);
            this.Controls.Add(this.cboBaseLifestyle);
            this.Controls.Add(this.Label_SelectAdvancedLifestyle_Base_Lifestyle);
            this.Controls.Add(this.nudRoommates);
            this.Controls.Add(this.lblRoommates);
            this.Controls.Add(this.lblSource);
            this.Controls.Add(this.lblSourceLabel);
            this.Controls.Add(this.nudPercentage);
            this.Controls.Add(this.lblPercentage);
            this.Controls.Add(this.txtLifestyleName);
            this.Controls.Add(this.lblLifestyleNameLabel);
            this.Controls.Add(this.lblCost);
            this.Controls.Add(this.lblCostLabel);
            this.Controls.Add(this.lblTotalLP);
            this.Controls.Add(this.lblTotalLPLabel);
            this.Controls.Add(this.cmdOKAdd);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.Label_SelectAdvancedLifestyle_Upgrade_Comforts);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectLifestyleAdvanced";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectAdvancedLifestyle";
            this.Text = "Build Advanced Lifestyle";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmSelectLifestyleAdvanced_FormClosing);
            this.Load += new System.EventHandler(this.frmSelectAdvancedLifestyle_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudPercentage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRoommates)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSecurity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudArea)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudComforts)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBonusLP)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void nudSecurity_ValueChanged(object sender, System.EventArgs e)
        {
            CalculateValues();
        }

        private void nudComforts_ValueChanged(object sender, System.EventArgs e)
        {
            CalculateValues();
        }

        private void nudArea_ValueChanged(object sender, System.EventArgs e)
        {
            CalculateValues();
        }

        private void nudSecurityEntertainment_ValueChanged(object sender, System.EventArgs e)
        {
            CalculateValues();
        }

        private void nudComfortsEntertainment_ValueChanged(object sender, System.EventArgs e)
        {
            CalculateValues();
        }

        private void nudAreaEntertainment_ValueChanged(object sender, System.EventArgs e)
        {
            CalculateValues();
        }

        #endregion

        private System.Windows.Forms.Label Label_SelectAdvancedLifestyle_Upgrade_Comforts;
        private System.Windows.Forms.Button cmdOKAdd;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Label lblTotalLPLabel;
        private System.Windows.Forms.Label lblTotalLP;
        private System.Windows.Forms.Label lblCostLabel;
        private System.Windows.Forms.Label lblCost;
        private System.Windows.Forms.Label lblLifestyleNameLabel;
        private System.Windows.Forms.TextBox txtLifestyleName;
        private System.Windows.Forms.NumericUpDown nudPercentage;
        private System.Windows.Forms.Label lblPercentage;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblSourceLabel;
        private System.Windows.Forms.NumericUpDown nudRoommates;
        private System.Windows.Forms.Label lblRoommates;
        private System.Windows.Forms.ComboBox cboBaseLifestyle;
        private System.Windows.Forms.Label Label_SelectAdvancedLifestyle_Base_Lifestyle;
        private System.Windows.Forms.NumericUpDown nudSecurity;
        private System.Windows.Forms.NumericUpDown nudArea;
        private System.Windows.Forms.NumericUpDown nudComforts;
        private System.Windows.Forms.Label Label_SelectAdvancedLifestyle_Base_Comforts;
        private System.Windows.Forms.Label Label_SelectAdvancedLifestyle_Base_Area;
        private System.Windows.Forms.Label Label_SelectAdvancedLifestyle_Base_Securities;
        private System.Windows.Forms.Button cmdAddQuality;
        private System.Windows.Forms.Button cmdDeleteQuality;
        private System.Windows.Forms.TreeView treLifestyleQualities;
        private System.Windows.Forms.CheckBox chkTrustFund;
        private System.Windows.Forms.Label lblQualityLp;
        private System.Windows.Forms.Label lblQualityLPLabel;
        private System.Windows.Forms.Label lblQualitySource;
        private System.Windows.Forms.Label lblQualitySourceLabel;
        private System.Windows.Forms.Label lblQualityCost;
        private System.Windows.Forms.Label lblQualityCostLabel;
        private System.Windows.Forms.CheckBox chkPrimaryTenant;
        private System.Windows.Forms.Label lblSecurityTotal;
        private System.Windows.Forms.Label lblAreaTotal;
        private System.Windows.Forms.Label lblComfortTotal;
        private System.Windows.Forms.CheckBox chkQualityContributesLP;
        private System.Windows.Forms.Label lblBonusLP;
        private System.Windows.Forms.NumericUpDown nudBonusLP;
        private System.Windows.Forms.CheckBox chkBonusLPRandomize;
    }
}
