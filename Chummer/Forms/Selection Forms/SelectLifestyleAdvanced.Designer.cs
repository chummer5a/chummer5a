using System.Windows.Forms;

namespace Chummer
{
    partial class SelectLifestyleAdvanced
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
            this.nudPercentage = new Chummer.NumericUpDownEx();
            this.lblPercentage = new System.Windows.Forms.Label();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.nudRoommates = new Chummer.NumericUpDownEx();
            this.lblRoommates = new System.Windows.Forms.Label();
            this.cboBaseLifestyle = new Chummer.ElasticComboBox();
            this.Label_SelectAdvancedLifestyle_Lifestyle = new System.Windows.Forms.Label();
            this.nudSecurity = new Chummer.NumericUpDownEx();
            this.nudArea = new Chummer.NumericUpDownEx();
            this.nudComforts = new Chummer.NumericUpDownEx();
            this.lblComfortsLabel = new System.Windows.Forms.Label();
            this.lblAreaLabel = new System.Windows.Forms.Label();
            this.lblSecurityLabel = new System.Windows.Forms.Label();
            this.cmdAddQuality = new System.Windows.Forms.Button();
            this.cmdDeleteQuality = new System.Windows.Forms.Button();
            this.chkTrustFund = new Chummer.ColorableCheckBox(this.components);
            this.lblQualityLp = new System.Windows.Forms.Label();
            this.lblQualityLPLabel = new System.Windows.Forms.Label();
            this.lblQualitySource = new System.Windows.Forms.Label();
            this.lblQualitySourceLabel = new System.Windows.Forms.Label();
            this.lblQualityCost = new System.Windows.Forms.Label();
            this.lblQualityCostLabel = new System.Windows.Forms.Label();
            this.chkPrimaryTenant = new Chummer.ColorableCheckBox(this.components);
            this.lblSecurityTotal = new System.Windows.Forms.Label();
            this.lblAreaTotal = new System.Windows.Forms.Label();
            this.lblComfortTotal = new System.Windows.Forms.Label();
            this.chkQualityContributesLP = new Chummer.ColorableCheckBox(this.components);
            this.lblBonusLP = new System.Windows.Forms.Label();
            this.nudBonusLP = new Chummer.NumericUpDownEx();
            this.chkBonusLPRandomize = new Chummer.ColorableCheckBox(this.components);
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.treLifestyleQualities = new System.Windows.Forms.TreeView();
            this.tlpButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpQualityButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpRight = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblComforts = new System.Windows.Forms.Label();
            this.lblArea = new System.Windows.Forms.Label();
            this.lblSecurity = new System.Windows.Forms.Label();
            this.lblBorough = new System.Windows.Forms.Label();
            this.cboBorough = new Chummer.ElasticComboBox();
            this.lblDistrict = new System.Windows.Forms.Label();
            this.cboDistrict = new Chummer.ElasticComboBox();
            this.lblCity = new System.Windows.Forms.Label();
            this.cboCity = new Chummer.ElasticComboBox();
            this.tlpLifestyleQuality = new Chummer.BufferedTableLayoutPanel(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.nudPercentage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRoommates)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSecurity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudArea)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudComforts)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBonusLP)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            this.tlpQualityButtons.SuspendLayout();
            this.tlpRight.SuspendLayout();
            this.tlpLifestyleQuality.SuspendLayout();
            this.SuspendLayout();
            // 
            // Label_SelectAdvancedLifestyle_Upgrade_Comforts
            // 
            this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.AutoSize = true;
            this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.Location = new System.Drawing.Point(152, 85);
            this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.Name = "Label_SelectAdvancedLifestyle_Upgrade_Comforts";
            this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.Size = new System.Drawing.Size(51, 13);
            this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.TabIndex = 2;
            this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.Tag = "Label_SelectAdvancedLifestyle_Rating";
            this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.Text = "Upgrade:";
            this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // cmdOKAdd
            // 
            this.cmdOKAdd.AutoSize = true;
            this.cmdOKAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOKAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOKAdd.Location = new System.Drawing.Point(81, 3);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(72, 23);
            this.cmdOKAdd.TabIndex = 27;
            this.cmdOKAdd.Tag = "String_AddMore";
            this.cmdOKAdd.Text = "&Add && More";
            this.cmdOKAdd.UseVisualStyleBackColor = true;
            this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.AutoSize = true;
            this.cmdCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdCancel.Location = new System.Drawing.Point(3, 3);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(72, 23);
            this.cmdCancel.TabIndex = 28;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.AutoSize = true;
            this.cmdOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOK.Location = new System.Drawing.Point(159, 3);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(72, 23);
            this.cmdOK.TabIndex = 26;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lblTotalLPLabel
            // 
            this.lblTotalLPLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblTotalLPLabel.AutoSize = true;
            this.lblTotalLPLabel.Location = new System.Drawing.Point(17, 188);
            this.lblTotalLPLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTotalLPLabel.Name = "lblTotalLPLabel";
            this.lblTotalLPLabel.Size = new System.Drawing.Size(63, 13);
            this.lblTotalLPLabel.TabIndex = 12;
            this.lblTotalLPLabel.Tag = "Label_SelectAdvancedLifestyle_UnusedLP";
            this.lblTotalLPLabel.Text = "Unused LP:";
            this.lblTotalLPLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblTotalLP
            // 
            this.lblTotalLP.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblTotalLP.AutoSize = true;
            this.tlpRight.SetColumnSpan(this.lblTotalLP, 3);
            this.lblTotalLP.Location = new System.Drawing.Point(86, 188);
            this.lblTotalLP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTotalLP.Name = "lblTotalLP";
            this.lblTotalLP.Size = new System.Drawing.Size(26, 13);
            this.lblTotalLP.TabIndex = 13;
            this.lblTotalLP.Text = "[LP]";
            this.lblTotalLP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCostLabel
            // 
            this.lblCostLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCostLabel.AutoSize = true;
            this.lblCostLabel.Location = new System.Drawing.Point(14, 265);
            this.lblCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCostLabel.Name = "lblCostLabel";
            this.lblCostLabel.Size = new System.Drawing.Size(66, 13);
            this.lblCostLabel.TabIndex = 18;
            this.lblCostLabel.Tag = "Label_SelectLifestyle_CostPerMonth";
            this.lblCostLabel.Text = "Cost/Month:";
            this.lblCostLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblCost
            // 
            this.lblCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCost.AutoSize = true;
            this.tlpRight.SetColumnSpan(this.lblCost, 3);
            this.lblCost.Location = new System.Drawing.Point(86, 265);
            this.lblCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCost.Name = "lblCost";
            this.lblCost.Size = new System.Drawing.Size(34, 13);
            this.lblCost.TabIndex = 19;
            this.lblCost.Text = "[Cost]";
            // 
            // lblLifestyleNameLabel
            // 
            this.lblLifestyleNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblLifestyleNameLabel.AutoSize = true;
            this.lblLifestyleNameLabel.Location = new System.Drawing.Point(42, 6);
            this.lblLifestyleNameLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLifestyleNameLabel.Name = "lblLifestyleNameLabel";
            this.lblLifestyleNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblLifestyleNameLabel.TabIndex = 0;
            this.lblLifestyleNameLabel.Tag = "Label_Name";
            this.lblLifestyleNameLabel.Text = "Name:";
            this.lblLifestyleNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtLifestyleName
            // 
            this.txtLifestyleName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpRight.SetColumnSpan(this.txtLifestyleName, 3);
            this.txtLifestyleName.Location = new System.Drawing.Point(86, 3);
            this.txtLifestyleName.Name = "txtLifestyleName";
            this.txtLifestyleName.Size = new System.Drawing.Size(371, 20);
            this.txtLifestyleName.TabIndex = 1;
            // 
            // nudPercentage
            // 
            this.nudPercentage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudPercentage.AutoSize = true;
            this.nudPercentage.DecimalPlaces = 2;
            this.nudPercentage.Location = new System.Drawing.Point(86, 236);
            this.nudPercentage.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            131072});
            this.nudPercentage.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudPercentage.MinimumSize = new System.Drawing.Size(60, 0);
            this.nudPercentage.Name = "nudPercentage";
            this.nudPercentage.Size = new System.Drawing.Size(60, 20);
            this.nudPercentage.TabIndex = 17;
            this.nudPercentage.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // lblPercentage
            // 
            this.lblPercentage.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblPercentage.AutoSize = true;
            this.lblPercentage.Location = new System.Drawing.Point(29, 239);
            this.lblPercentage.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPercentage.Name = "lblPercentage";
            this.lblPercentage.Size = new System.Drawing.Size(51, 13);
            this.lblPercentage.TabIndex = 16;
            this.lblPercentage.Tag = "Label_SelectLifestyle_PercentToPay";
            this.lblPercentage.Text = "% to Pay:";
            this.lblPercentage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblSource
            // 
            this.lblSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSource.AutoSize = true;
            this.tlpRight.SetColumnSpan(this.lblSource, 3);
            this.lblSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSource.Location = new System.Drawing.Point(86, 290);
            this.lblSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 21;
            this.lblSource.Text = "[Source]";
            this.lblSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(36, 290);
            this.lblSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 20;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            this.lblSourceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudRoommates
            // 
            this.nudRoommates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudRoommates.AutoSize = true;
            this.nudRoommates.Location = new System.Drawing.Point(86, 210);
            this.nudRoommates.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudRoommates.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudRoommates.MinimumSize = new System.Drawing.Size(60, 0);
            this.nudRoommates.Name = "nudRoommates";
            this.nudRoommates.Size = new System.Drawing.Size(60, 20);
            this.nudRoommates.TabIndex = 15;
            this.nudRoommates.ValueChanged += new System.EventHandler(this.nudRoommates_ValueChanged);
            // 
            // lblRoommates
            // 
            this.lblRoommates.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblRoommates.AutoSize = true;
            this.lblRoommates.Location = new System.Drawing.Point(14, 213);
            this.lblRoommates.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRoommates.Name = "lblRoommates";
            this.lblRoommates.Size = new System.Drawing.Size(66, 13);
            this.lblRoommates.TabIndex = 14;
            this.lblRoommates.Tag = "Label_SelectLifestyle_Roommates";
            this.lblRoommates.Text = "Roommates:";
            this.lblRoommates.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboBaseLifestyle
            // 
            this.cboBaseLifestyle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpRight.SetColumnSpan(this.cboBaseLifestyle, 3);
            this.cboBaseLifestyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBaseLifestyle.FormattingEnabled = true;
            this.cboBaseLifestyle.Location = new System.Drawing.Point(86, 29);
            this.cboBaseLifestyle.Name = "cboBaseLifestyle";
            this.cboBaseLifestyle.Size = new System.Drawing.Size(371, 21);
            this.cboBaseLifestyle.TabIndex = 32;
            this.cboBaseLifestyle.TooltipText = "";
            this.cboBaseLifestyle.SelectedIndexChanged += new System.EventHandler(this.cboBaseLifestyle_SelectedIndexChanged);
            // 
            // Label_SelectAdvancedLifestyle_Lifestyle
            // 
            this.Label_SelectAdvancedLifestyle_Lifestyle.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.Label_SelectAdvancedLifestyle_Lifestyle.AutoSize = true;
            this.Label_SelectAdvancedLifestyle_Lifestyle.Location = new System.Drawing.Point(32, 33);
            this.Label_SelectAdvancedLifestyle_Lifestyle.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.Label_SelectAdvancedLifestyle_Lifestyle.Name = "Label_SelectAdvancedLifestyle_Lifestyle";
            this.Label_SelectAdvancedLifestyle_Lifestyle.Size = new System.Drawing.Size(48, 13);
            this.Label_SelectAdvancedLifestyle_Lifestyle.TabIndex = 31;
            this.Label_SelectAdvancedLifestyle_Lifestyle.Tag = "Label_SelectAdvancedLifestyle_Lifestyle";
            this.Label_SelectAdvancedLifestyle_Lifestyle.Text = "Lifestyle:";
            this.Label_SelectAdvancedLifestyle_Lifestyle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudSecurity
            // 
            this.nudSecurity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudSecurity.AutoSize = true;
            this.nudSecurity.Location = new System.Drawing.Point(152, 159);
            this.nudSecurity.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudSecurity.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudSecurity.MinimumSize = new System.Drawing.Size(60, 0);
            this.nudSecurity.Name = "nudSecurity";
            this.nudSecurity.Size = new System.Drawing.Size(60, 20);
            this.nudSecurity.TabIndex = 41;
            // 
            // nudArea
            // 
            this.nudArea.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudArea.AutoSize = true;
            this.nudArea.Location = new System.Drawing.Point(152, 133);
            this.nudArea.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudArea.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudArea.MinimumSize = new System.Drawing.Size(60, 0);
            this.nudArea.Name = "nudArea";
            this.nudArea.Size = new System.Drawing.Size(60, 20);
            this.nudArea.TabIndex = 40;
            // 
            // nudComforts
            // 
            this.nudComforts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudComforts.AutoSize = true;
            this.nudComforts.Location = new System.Drawing.Point(152, 107);
            this.nudComforts.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudComforts.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudComforts.MinimumSize = new System.Drawing.Size(60, 0);
            this.nudComforts.Name = "nudComforts";
            this.nudComforts.Size = new System.Drawing.Size(60, 20);
            this.nudComforts.TabIndex = 39;
            // 
            // lblComfortsLabel
            // 
            this.lblComfortsLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblComfortsLabel.AutoSize = true;
            this.lblComfortsLabel.Location = new System.Drawing.Point(29, 110);
            this.lblComfortsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblComfortsLabel.Name = "lblComfortsLabel";
            this.lblComfortsLabel.Size = new System.Drawing.Size(51, 13);
            this.lblComfortsLabel.TabIndex = 42;
            this.lblComfortsLabel.Tag = "Label_SelectAdvancedLifestyle_Comforts";
            this.lblComfortsLabel.Text = "Comforts:";
            this.lblComfortsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblAreaLabel
            // 
            this.lblAreaLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblAreaLabel.AutoSize = true;
            this.lblAreaLabel.Location = new System.Drawing.Point(3, 136);
            this.lblAreaLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAreaLabel.Name = "lblAreaLabel";
            this.lblAreaLabel.Size = new System.Drawing.Size(77, 13);
            this.lblAreaLabel.TabIndex = 46;
            this.lblAreaLabel.Tag = "Label_SelectAdvancedLifestyle_Neighborhood";
            this.lblAreaLabel.Text = "Neighborhood:";
            this.lblAreaLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblSecurityLabel
            // 
            this.lblSecurityLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSecurityLabel.AutoSize = true;
            this.lblSecurityLabel.Location = new System.Drawing.Point(32, 162);
            this.lblSecurityLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSecurityLabel.Name = "lblSecurityLabel";
            this.lblSecurityLabel.Size = new System.Drawing.Size(48, 13);
            this.lblSecurityLabel.TabIndex = 48;
            this.lblSecurityLabel.Tag = "Label_SelectAdvancedLifestyle_Security";
            this.lblSecurityLabel.Text = "Security:";
            this.lblSecurityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmdAddQuality
            // 
            this.cmdAddQuality.AutoSize = true;
            this.cmdAddQuality.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddQuality.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdAddQuality.Location = new System.Drawing.Point(3, 3);
            this.cmdAddQuality.Name = "cmdAddQuality";
            this.cmdAddQuality.Size = new System.Drawing.Size(83, 23);
            this.cmdAddQuality.TabIndex = 51;
            this.cmdAddQuality.Tag = "Button_AddQuality";
            this.cmdAddQuality.Text = "Add Quality";
            this.cmdAddQuality.UseVisualStyleBackColor = true;
            this.cmdAddQuality.Click += new System.EventHandler(this.cmdAddQuality_Click);
            // 
            // cmdDeleteQuality
            // 
            this.cmdDeleteQuality.AutoSize = true;
            this.cmdDeleteQuality.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDeleteQuality.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdDeleteQuality.Enabled = false;
            this.cmdDeleteQuality.Location = new System.Drawing.Point(92, 3);
            this.cmdDeleteQuality.Name = "cmdDeleteQuality";
            this.cmdDeleteQuality.Size = new System.Drawing.Size(83, 23);
            this.cmdDeleteQuality.TabIndex = 52;
            this.cmdDeleteQuality.Tag = "Button_DeleteQuality";
            this.cmdDeleteQuality.Text = "Delete Quality";
            this.cmdDeleteQuality.UseVisualStyleBackColor = true;
            this.cmdDeleteQuality.Click += new System.EventHandler(this.cmdDeleteQuality_Click);
            // 
            // chkTrustFund
            // 
            this.chkTrustFund.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkTrustFund.AutoSize = true;
            this.tlpRight.SetColumnSpan(this.chkTrustFund, 2);
            this.chkTrustFund.DefaultColorScheme = true;
            this.chkTrustFund.Location = new System.Drawing.Point(152, 237);
            this.chkTrustFund.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.lblQualityLp.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblQualityLp.AutoSize = true;
            this.lblQualityLp.Location = new System.Drawing.Point(88, 6);
            this.lblQualityLp.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblQualityLp.Name = "lblQualityLp";
            this.lblQualityLp.Size = new System.Drawing.Size(26, 13);
            this.lblQualityLp.TabIndex = 71;
            this.lblQualityLp.Text = "[LP]";
            this.lblQualityLp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblQualityLPLabel
            // 
            this.lblQualityLPLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblQualityLPLabel.AutoSize = true;
            this.lblQualityLPLabel.Location = new System.Drawing.Point(24, 6);
            this.lblQualityLPLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblQualityLPLabel.Name = "lblQualityLPLabel";
            this.lblQualityLPLabel.Size = new System.Drawing.Size(58, 13);
            this.lblQualityLPLabel.TabIndex = 70;
            this.lblQualityLPLabel.Tag = "Label_QualityLP";
            this.lblQualityLPLabel.Text = "Quality LP:";
            this.lblQualityLPLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblQualitySource
            // 
            this.lblQualitySource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblQualitySource.AutoSize = true;
            this.tlpLifestyleQuality.SetColumnSpan(this.lblQualitySource, 2);
            this.lblQualitySource.Location = new System.Drawing.Point(88, 56);
            this.lblQualitySource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblQualitySource.Name = "lblQualitySource";
            this.lblQualitySource.Size = new System.Drawing.Size(47, 13);
            this.lblQualitySource.TabIndex = 69;
            this.lblQualitySource.Text = "[Source]";
            this.lblQualitySource.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblQualitySource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblQualitySourceLabel
            // 
            this.lblQualitySourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblQualitySourceLabel.AutoSize = true;
            this.lblQualitySourceLabel.Location = new System.Drawing.Point(3, 56);
            this.lblQualitySourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblQualitySourceLabel.Name = "lblQualitySourceLabel";
            this.lblQualitySourceLabel.Size = new System.Drawing.Size(79, 13);
            this.lblQualitySourceLabel.TabIndex = 68;
            this.lblQualitySourceLabel.Tag = "Label_QualitySource";
            this.lblQualitySourceLabel.Text = "Quality Source:";
            this.lblQualitySourceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblQualityCost
            // 
            this.lblQualityCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblQualityCost.AutoSize = true;
            this.tlpLifestyleQuality.SetColumnSpan(this.lblQualityCost, 2);
            this.lblQualityCost.Location = new System.Drawing.Point(88, 31);
            this.lblQualityCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblQualityCost.Name = "lblQualityCost";
            this.lblQualityCost.Size = new System.Drawing.Size(34, 13);
            this.lblQualityCost.TabIndex = 73;
            this.lblQualityCost.Text = "[Cost]";
            this.lblQualityCost.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblQualityCostLabel
            // 
            this.lblQualityCostLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblQualityCostLabel.AutoSize = true;
            this.lblQualityCostLabel.Location = new System.Drawing.Point(16, 31);
            this.lblQualityCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblQualityCostLabel.Name = "lblQualityCostLabel";
            this.lblQualityCostLabel.Size = new System.Drawing.Size(66, 13);
            this.lblQualityCostLabel.TabIndex = 72;
            this.lblQualityCostLabel.Tag = "Label_QualityCost";
            this.lblQualityCostLabel.Text = "Quality Cost:";
            this.lblQualityCostLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkPrimaryTenant
            // 
            this.chkPrimaryTenant.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkPrimaryTenant.AutoSize = true;
            this.tlpRight.SetColumnSpan(this.chkPrimaryTenant, 2);
            this.chkPrimaryTenant.DefaultColorScheme = true;
            this.chkPrimaryTenant.Location = new System.Drawing.Point(152, 211);
            this.chkPrimaryTenant.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkPrimaryTenant.Name = "chkPrimaryTenant";
            this.chkPrimaryTenant.Size = new System.Drawing.Size(97, 17);
            this.chkPrimaryTenant.TabIndex = 74;
            this.chkPrimaryTenant.Tag = "Label_SelectAdvancedLifestyle_Tenant";
            this.chkPrimaryTenant.Text = "Primary Tenant";
            this.chkPrimaryTenant.UseVisualStyleBackColor = true;
            // 
            // lblSecurityTotal
            // 
            this.lblSecurityTotal.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSecurityTotal.AutoSize = true;
            this.lblSecurityTotal.Location = new System.Drawing.Point(218, 162);
            this.lblSecurityTotal.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSecurityTotal.Name = "lblSecurityTotal";
            this.lblSecurityTotal.Size = new System.Drawing.Size(19, 13);
            this.lblSecurityTotal.TabIndex = 77;
            this.lblSecurityTotal.Tag = "";
            this.lblSecurityTotal.Text = "[0]";
            this.lblSecurityTotal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAreaTotal
            // 
            this.lblAreaTotal.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblAreaTotal.AutoSize = true;
            this.lblAreaTotal.Location = new System.Drawing.Point(218, 136);
            this.lblAreaTotal.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAreaTotal.Name = "lblAreaTotal";
            this.lblAreaTotal.Size = new System.Drawing.Size(19, 13);
            this.lblAreaTotal.TabIndex = 76;
            this.lblAreaTotal.Tag = "";
            this.lblAreaTotal.Text = "[0]";
            this.lblAreaTotal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblComfortTotal
            // 
            this.lblComfortTotal.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblComfortTotal.AutoSize = true;
            this.lblComfortTotal.Location = new System.Drawing.Point(218, 110);
            this.lblComfortTotal.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblComfortTotal.Name = "lblComfortTotal";
            this.lblComfortTotal.Size = new System.Drawing.Size(19, 13);
            this.lblComfortTotal.TabIndex = 75;
            this.lblComfortTotal.Tag = "";
            this.lblComfortTotal.Text = "[0]";
            this.lblComfortTotal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // chkQualityContributesLP
            // 
            this.chkQualityContributesLP.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkQualityContributesLP.AutoSize = true;
            this.chkQualityContributesLP.DefaultColorScheme = true;
            this.chkQualityContributesLP.Location = new System.Drawing.Point(120, 4);
            this.chkQualityContributesLP.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.lblBonusLP.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblBonusLP.AutoSize = true;
            this.lblBonusLP.Location = new System.Drawing.Point(24, 59);
            this.lblBonusLP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBonusLP.Name = "lblBonusLP";
            this.lblBonusLP.Size = new System.Drawing.Size(56, 13);
            this.lblBonusLP.TabIndex = 81;
            this.lblBonusLP.Tag = "Label_BonusLP";
            this.lblBonusLP.Text = "Bonus LP:";
            this.lblBonusLP.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblBonusLP.Visible = false;
            // 
            // nudBonusLP
            // 
            this.nudBonusLP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudBonusLP.AutoSize = true;
            this.nudBonusLP.Location = new System.Drawing.Point(86, 56);
            this.nudBonusLP.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudBonusLP.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.nudBonusLP.MinimumSize = new System.Drawing.Size(60, 0);
            this.nudBonusLP.Name = "nudBonusLP";
            this.nudBonusLP.Size = new System.Drawing.Size(60, 20);
            this.nudBonusLP.TabIndex = 82;
            this.nudBonusLP.Visible = false;
            // 
            // chkBonusLPRandomize
            // 
            this.chkBonusLPRandomize.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkBonusLPRandomize.AutoSize = true;
            this.chkBonusLPRandomize.Checked = true;
            this.chkBonusLPRandomize.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tlpRight.SetColumnSpan(this.chkBonusLPRandomize, 2);
            this.chkBonusLPRandomize.DefaultColorScheme = true;
            this.chkBonusLPRandomize.Location = new System.Drawing.Point(152, 57);
            this.chkBonusLPRandomize.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkBonusLPRandomize.Name = "chkBonusLPRandomize";
            this.chkBonusLPRandomize.Size = new System.Drawing.Size(102, 17);
            this.chkBonusLPRandomize.TabIndex = 83;
            this.chkBonusLPRandomize.Tag = "Checkbox_Randomize1D6";
            this.chkBonusLPRandomize.Text = "Randomize 1D6";
            this.chkBonusLPRandomize.UseVisualStyleBackColor = true;
            this.chkBonusLPRandomize.Visible = false;
            this.chkBonusLPRandomize.CheckedChanged += new System.EventHandler(this.chkTravelerBonusLPRandomize_CheckedChanged);
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tlpMain.Controls.Add(this.treLifestyleQualities, 0, 1);
            this.tlpMain.Controls.Add(this.tlpButtons, 1, 4);
            this.tlpMain.Controls.Add(this.tlpQualityButtons, 0, 0);
            this.tlpMain.Controls.Add(this.tlpRight, 1, 0);
            this.tlpMain.Controls.Add(this.tlpLifestyleQuality, 1, 3);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 5;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(766, 543);
            this.tlpMain.TabIndex = 84;
            // 
            // treLifestyleQualities
            // 
            this.treLifestyleQualities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treLifestyleQualities.HideSelection = false;
            this.treLifestyleQualities.Location = new System.Drawing.Point(3, 32);
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
            this.tlpMain.SetRowSpan(this.treLifestyleQualities, 4);
            this.treLifestyleQualities.Size = new System.Drawing.Size(300, 508);
            this.treLifestyleQualities.TabIndex = 53;
            this.treLifestyleQualities.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treLifestyleQualities_AfterSelect);
            // 
            // tlpButtons
            // 
            this.tlpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpButtons.AutoSize = true;
            this.tlpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpButtons.ColumnCount = 3;
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpButtons.Controls.Add(this.cmdCancel, 0, 0);
            this.tlpButtons.Controls.Add(this.cmdOKAdd, 1, 0);
            this.tlpButtons.Controls.Add(this.cmdOK, 2, 0);
            this.tlpButtons.Location = new System.Drawing.Point(532, 514);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButtons.Size = new System.Drawing.Size(234, 29);
            this.tlpButtons.TabIndex = 89;
            // 
            // tlpQualityButtons
            // 
            this.tlpQualityButtons.AutoSize = true;
            this.tlpQualityButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpQualityButtons.ColumnCount = 2;
            this.tlpQualityButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpQualityButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpQualityButtons.Controls.Add(this.cmdDeleteQuality, 1, 0);
            this.tlpQualityButtons.Controls.Add(this.cmdAddQuality, 0, 0);
            this.tlpQualityButtons.Location = new System.Drawing.Point(0, 0);
            this.tlpQualityButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpQualityButtons.Name = "tlpQualityButtons";
            this.tlpQualityButtons.RowCount = 1;
            this.tlpQualityButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpQualityButtons.Size = new System.Drawing.Size(178, 29);
            this.tlpQualityButtons.TabIndex = 90;
            // 
            // tlpRight
            // 
            this.tlpRight.AutoSize = true;
            this.tlpRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.ColumnCount = 4;
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRight.Controls.Add(this.lblLifestyleNameLabel, 0, 0);
            this.tlpRight.Controls.Add(this.txtLifestyleName, 1, 0);
            this.tlpRight.Controls.Add(this.Label_SelectAdvancedLifestyle_Lifestyle, 0, 1);
            this.tlpRight.Controls.Add(this.cboBaseLifestyle, 1, 1);
            this.tlpRight.Controls.Add(this.lblBonusLP, 0, 2);
            this.tlpRight.Controls.Add(this.nudBonusLP, 1, 2);
            this.tlpRight.Controls.Add(this.chkBonusLPRandomize, 2, 2);
            this.tlpRight.Controls.Add(this.lblComfortsLabel, 0, 4);
            this.tlpRight.Controls.Add(this.lblComforts, 1, 4);
            this.tlpRight.Controls.Add(this.nudComforts, 2, 4);
            this.tlpRight.Controls.Add(this.Label_SelectAdvancedLifestyle_Upgrade_Comforts, 2, 3);
            this.tlpRight.Controls.Add(this.chkPrimaryTenant, 2, 8);
            this.tlpRight.Controls.Add(this.lblComfortTotal, 3, 4);
            this.tlpRight.Controls.Add(this.lblSource, 1, 11);
            this.tlpRight.Controls.Add(this.chkTrustFund, 2, 9);
            this.tlpRight.Controls.Add(this.lblSourceLabel, 0, 11);
            this.tlpRight.Controls.Add(this.lblAreaLabel, 0, 5);
            this.tlpRight.Controls.Add(this.lblCost, 1, 10);
            this.tlpRight.Controls.Add(this.lblSecurityLabel, 0, 6);
            this.tlpRight.Controls.Add(this.lblCostLabel, 0, 10);
            this.tlpRight.Controls.Add(this.lblArea, 1, 5);
            this.tlpRight.Controls.Add(this.lblSecurity, 1, 6);
            this.tlpRight.Controls.Add(this.nudArea, 2, 5);
            this.tlpRight.Controls.Add(this.nudPercentage, 1, 9);
            this.tlpRight.Controls.Add(this.nudSecurity, 2, 6);
            this.tlpRight.Controls.Add(this.lblPercentage, 0, 9);
            this.tlpRight.Controls.Add(this.lblAreaTotal, 3, 5);
            this.tlpRight.Controls.Add(this.lblSecurityTotal, 3, 6);
            this.tlpRight.Controls.Add(this.nudRoommates, 1, 8);
            this.tlpRight.Controls.Add(this.lblTotalLPLabel, 0, 7);
            this.tlpRight.Controls.Add(this.lblTotalLP, 1, 7);
            this.tlpRight.Controls.Add(this.lblRoommates, 0, 8);
            this.tlpRight.Controls.Add(this.lblBorough, 0, 14);
            this.tlpRight.Controls.Add(this.cboBorough, 1, 14);
            this.tlpRight.Controls.Add(this.lblDistrict, 0, 13);
            this.tlpRight.Controls.Add(this.cboDistrict, 1, 13);
            this.tlpRight.Controls.Add(this.lblCity, 0, 12);
            this.tlpRight.Controls.Add(this.cboCity, 1, 12);
            this.tlpRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRight.Location = new System.Drawing.Point(306, 0);
            this.tlpRight.Margin = new System.Windows.Forms.Padding(0);
            this.tlpRight.Name = "tlpRight";
            this.tlpRight.RowCount = 15;
            this.tlpMain.SetRowSpan(this.tlpRight, 2);
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.Size = new System.Drawing.Size(460, 390);
            this.tlpRight.TabIndex = 97;
            // 
            // lblComforts
            // 
            this.lblComforts.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblComforts.AutoSize = true;
            this.lblComforts.Location = new System.Drawing.Point(100, 110);
            this.lblComforts.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblComforts.Name = "lblComforts";
            this.lblComforts.Size = new System.Drawing.Size(46, 13);
            this.lblComforts.TabIndex = 84;
            this.lblComforts.Tag = "Label_SelectAdvancedLifestyle_Base_Comforts";
            this.lblComforts.Text = "[{0}/{1}]";
            this.lblComforts.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblArea
            // 
            this.lblArea.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblArea.AutoSize = true;
            this.lblArea.Location = new System.Drawing.Point(100, 136);
            this.lblArea.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArea.Name = "lblArea";
            this.lblArea.Size = new System.Drawing.Size(46, 13);
            this.lblArea.TabIndex = 85;
            this.lblArea.Tag = "Label_SelectAdvancedLifestyle_Base_Neighborhood";
            this.lblArea.Text = "[{0}/{1}]";
            this.lblArea.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblSecurity
            // 
            this.lblSecurity.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSecurity.AutoSize = true;
            this.lblSecurity.Location = new System.Drawing.Point(100, 162);
            this.lblSecurity.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSecurity.Name = "lblSecurity";
            this.lblSecurity.Size = new System.Drawing.Size(46, 13);
            this.lblSecurity.TabIndex = 86;
            this.lblSecurity.Tag = "Label_SelectAdvancedLifestyle_Base_Security";
            this.lblSecurity.Text = "[{0}/{1}]";
            this.lblSecurity.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblBorough
            // 
            this.lblBorough.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblBorough.AutoSize = true;
            this.lblBorough.Location = new System.Drawing.Point(30, 370);
            this.lblBorough.Name = "lblBorough";
            this.lblBorough.Size = new System.Drawing.Size(50, 13);
            this.lblBorough.TabIndex = 93;
            this.lblBorough.Tag = "Label_SelectLifestyle_Borough";
            this.lblBorough.Text = "Borough:";
            this.lblBorough.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboBorough
            // 
            this.cboBorough.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpRight.SetColumnSpan(this.cboBorough, 3);
            this.cboBorough.FormattingEnabled = true;
            this.cboBorough.Location = new System.Drawing.Point(86, 366);
            this.cboBorough.Name = "cboBorough";
            this.cboBorough.Size = new System.Drawing.Size(371, 21);
            this.cboBorough.TabIndex = 96;
            this.cboBorough.TooltipText = "";
            this.cboBorough.SelectedIndexChanged += new System.EventHandler(this.cboBorough_SelectedIndexChanged);
            // 
            // lblDistrict
            // 
            this.lblDistrict.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDistrict.AutoSize = true;
            this.lblDistrict.Location = new System.Drawing.Point(38, 343);
            this.lblDistrict.Name = "lblDistrict";
            this.lblDistrict.Size = new System.Drawing.Size(42, 13);
            this.lblDistrict.TabIndex = 92;
            this.lblDistrict.Tag = "Label_SelectLifestyle_District";
            this.lblDistrict.Text = "District:";
            this.lblDistrict.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboDistrict
            // 
            this.cboDistrict.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpRight.SetColumnSpan(this.cboDistrict, 3);
            this.cboDistrict.FormattingEnabled = true;
            this.cboDistrict.Location = new System.Drawing.Point(86, 339);
            this.cboDistrict.Name = "cboDistrict";
            this.cboDistrict.Size = new System.Drawing.Size(371, 21);
            this.cboDistrict.TabIndex = 95;
            this.cboDistrict.TooltipText = "";
            this.cboDistrict.SelectedIndexChanged += new System.EventHandler(this.cboDistrict_SelectedIndexChanged);
            // 
            // lblCity
            // 
            this.lblCity.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCity.AutoSize = true;
            this.lblCity.Location = new System.Drawing.Point(53, 316);
            this.lblCity.Name = "lblCity";
            this.lblCity.Size = new System.Drawing.Size(27, 13);
            this.lblCity.TabIndex = 91;
            this.lblCity.Tag = "Label_SelectLifestyle_City";
            this.lblCity.Text = "City:";
            this.lblCity.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboCity
            // 
            this.cboCity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpRight.SetColumnSpan(this.cboCity, 3);
            this.cboCity.FormattingEnabled = true;
            this.cboCity.Location = new System.Drawing.Point(86, 312);
            this.cboCity.Name = "cboCity";
            this.cboCity.Size = new System.Drawing.Size(371, 21);
            this.cboCity.TabIndex = 94;
            this.cboCity.TooltipText = "";
            this.cboCity.SelectedIndexChanged += new System.EventHandler(this.cboCity_SelectedIndexChanged);
            // 
            // tlpLifestyleQuality
            // 
            this.tlpLifestyleQuality.AutoSize = true;
            this.tlpLifestyleQuality.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpLifestyleQuality.ColumnCount = 3;
            this.tlpLifestyleQuality.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpLifestyleQuality.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpLifestyleQuality.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLifestyleQuality.Controls.Add(this.lblQualityLPLabel, 0, 0);
            this.tlpLifestyleQuality.Controls.Add(this.lblQualityLp, 1, 0);
            this.tlpLifestyleQuality.Controls.Add(this.chkQualityContributesLP, 2, 0);
            this.tlpLifestyleQuality.Controls.Add(this.lblQualityCostLabel, 0, 1);
            this.tlpLifestyleQuality.Controls.Add(this.lblQualitySourceLabel, 0, 2);
            this.tlpLifestyleQuality.Controls.Add(this.lblQualityCost, 1, 1);
            this.tlpLifestyleQuality.Controls.Add(this.lblQualitySource, 1, 2);
            this.tlpLifestyleQuality.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpLifestyleQuality.Location = new System.Drawing.Point(306, 439);
            this.tlpLifestyleQuality.Margin = new System.Windows.Forms.Padding(0);
            this.tlpLifestyleQuality.Name = "tlpLifestyleQuality";
            this.tlpLifestyleQuality.RowCount = 3;
            this.tlpLifestyleQuality.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpLifestyleQuality.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpLifestyleQuality.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpLifestyleQuality.Size = new System.Drawing.Size(460, 75);
            this.tlpLifestyleQuality.TabIndex = 98;
            this.tlpLifestyleQuality.Visible = false;
            // 
            // frmSelectLifestyleAdvanced
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectLifestyleAdvanced";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectAdvancedLifestyle";
            this.Text = "Build Advanced Lifestyle";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SelectLifestyleAdvanced_FormClosing);
            this.Load += new System.EventHandler(this.SelectAdvancedLifestyle_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudPercentage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRoommates)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSecurity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudArea)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudComforts)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBonusLP)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.tlpQualityButtons.ResumeLayout(false);
            this.tlpQualityButtons.PerformLayout();
            this.tlpRight.ResumeLayout(false);
            this.tlpRight.PerformLayout();
            this.tlpLifestyleQuality.ResumeLayout(false);
            this.tlpLifestyleQuality.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private Chummer.NumericUpDownEx nudPercentage;
        private System.Windows.Forms.Label lblPercentage;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblSourceLabel;
        private Chummer.NumericUpDownEx nudRoommates;
        private System.Windows.Forms.Label lblRoommates;
        private ElasticComboBox cboBaseLifestyle;
        private System.Windows.Forms.Label Label_SelectAdvancedLifestyle_Lifestyle;
        private Chummer.NumericUpDownEx nudSecurity;
        private Chummer.NumericUpDownEx nudArea;
        private Chummer.NumericUpDownEx nudComforts;
        private System.Windows.Forms.Label lblComfortsLabel;
        private System.Windows.Forms.Label lblAreaLabel;
        private System.Windows.Forms.Label lblSecurityLabel;
        private System.Windows.Forms.Button cmdAddQuality;
        private System.Windows.Forms.Button cmdDeleteQuality;
        private Chummer.ColorableCheckBox chkTrustFund;
        private System.Windows.Forms.Label lblQualityLp;
        private System.Windows.Forms.Label lblQualityLPLabel;
        private System.Windows.Forms.Label lblQualitySource;
        private System.Windows.Forms.Label lblQualitySourceLabel;
        private System.Windows.Forms.Label lblQualityCost;
        private System.Windows.Forms.Label lblQualityCostLabel;
        private Chummer.ColorableCheckBox chkPrimaryTenant;
        private System.Windows.Forms.Label lblSecurityTotal;
        private System.Windows.Forms.Label lblAreaTotal;
        private System.Windows.Forms.Label lblComfortTotal;
        private Chummer.ColorableCheckBox chkQualityContributesLP;
        private System.Windows.Forms.Label lblBonusLP;
        private Chummer.NumericUpDownEx nudBonusLP;
        private Chummer.ColorableCheckBox chkBonusLPRandomize;
        private Label lblComforts;
        private Label lblArea;
        private Label lblSecurity;
        private BufferedTableLayoutPanel tlpMain;
        private BufferedTableLayoutPanel tlpButtons;
        private BufferedTableLayoutPanel tlpQualityButtons;
        private Label lblCity;
        private Label lblDistrict;
        private Label lblBorough;
        private ElasticComboBox cboCity;
        private ElasticComboBox cboDistrict;
        private ElasticComboBox cboBorough;
        private BufferedTableLayoutPanel tlpRight;
        private TreeView treLifestyleQualities;
        private BufferedTableLayoutPanel tlpLifestyleQuality;
    }
}
