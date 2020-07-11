using System.Windows.Forms;

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
            this.nudPercentage = new System.Windows.Forms.NumericUpDown();
            this.lblPercentage = new System.Windows.Forms.Label();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.nudRoommates = new System.Windows.Forms.NumericUpDown();
            this.lblRoommates = new System.Windows.Forms.Label();
            this.cboBaseLifestyle = new Chummer.ElasticComboBox();
            this.Label_SelectAdvancedLifestyle_Lifestyle = new System.Windows.Forms.Label();
            this.nudSecurity = new System.Windows.Forms.NumericUpDown();
            this.nudArea = new System.Windows.Forms.NumericUpDown();
            this.nudComforts = new System.Windows.Forms.NumericUpDown();
            this.lblComfortsLabel = new System.Windows.Forms.Label();
            this.lblAreaLabel = new System.Windows.Forms.Label();
            this.lblSecurityLabel = new System.Windows.Forms.Label();
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
            this.tableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblComforts = new System.Windows.Forms.Label();
            this.lblArea = new System.Windows.Forms.Label();
            this.lblSecurity = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.nudPercentage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRoommates)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSecurity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudArea)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudComforts)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBonusLP)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // Label_SelectAdvancedLifestyle_Upgrade_Comforts
            // 
            this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.AutoSize = true;
            this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.Location = new System.Drawing.Point(455, 88);
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
            this.cmdOKAdd.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdOKAdd.AutoSize = true;
            this.cmdOKAdd.Location = new System.Drawing.Point(84, 3);
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
            this.cmdCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdCancel.AutoSize = true;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(3, 3);
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
            this.cmdOK.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdOK.AutoSize = true;
            this.cmdOK.Location = new System.Drawing.Point(165, 3);
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
            this.lblTotalLPLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblTotalLPLabel.AutoSize = true;
            this.lblTotalLPLabel.Location = new System.Drawing.Point(320, 191);
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
            this.lblTotalLP.Location = new System.Drawing.Point(389, 191);
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
            this.lblCostLabel.Location = new System.Drawing.Point(317, 268);
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
            this.lblCost.Location = new System.Drawing.Point(389, 268);
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
            this.lblLifestyleNameLabel.Location = new System.Drawing.Point(345, 8);
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
            this.tableLayoutPanel1.SetColumnSpan(this.txtLifestyleName, 3);
            this.txtLifestyleName.Location = new System.Drawing.Point(389, 4);
            this.txtLifestyleName.Name = "txtLifestyleName";
            this.txtLifestyleName.Size = new System.Drawing.Size(214, 20);
            this.txtLifestyleName.TabIndex = 1;
            // 
            // nudPercentage
            // 
            this.nudPercentage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudPercentage.DecimalPlaces = 2;
            this.nudPercentage.Location = new System.Drawing.Point(389, 239);
            this.nudPercentage.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            131072});
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
            this.lblPercentage.Location = new System.Drawing.Point(332, 242);
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
            this.lblSource.Location = new System.Drawing.Point(389, 293);
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
            this.lblSourceLabel.Location = new System.Drawing.Point(339, 293);
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
            this.nudRoommates.Location = new System.Drawing.Point(389, 213);
            this.nudRoommates.Name = "nudRoommates";
            this.nudRoommates.Size = new System.Drawing.Size(60, 20);
            this.nudRoommates.TabIndex = 15;
            this.nudRoommates.ValueChanged += new System.EventHandler(this.nudRoommates_ValueChanged);
            // 
            // lblRoommates
            // 
            this.lblRoommates.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblRoommates.AutoSize = true;
            this.lblRoommates.Location = new System.Drawing.Point(317, 216);
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
            this.tableLayoutPanel1.SetColumnSpan(this.cboBaseLifestyle, 3);
            this.cboBaseLifestyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBaseLifestyle.FormattingEnabled = true;
            this.cboBaseLifestyle.Location = new System.Drawing.Point(389, 32);
            this.cboBaseLifestyle.Name = "cboBaseLifestyle";
            this.cboBaseLifestyle.Size = new System.Drawing.Size(214, 21);
            this.cboBaseLifestyle.TabIndex = 32;
            this.cboBaseLifestyle.TooltipText = "";
            this.cboBaseLifestyle.SelectedIndexChanged += new System.EventHandler(this.cboBaseLifestyle_SelectedIndexChanged);
            // 
            // Label_SelectAdvancedLifestyle_Lifestyle
            // 
            this.Label_SelectAdvancedLifestyle_Lifestyle.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.Label_SelectAdvancedLifestyle_Lifestyle.AutoSize = true;
            this.Label_SelectAdvancedLifestyle_Lifestyle.Location = new System.Drawing.Point(335, 36);
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
            this.nudSecurity.Location = new System.Drawing.Point(455, 162);
            this.nudSecurity.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudSecurity.Name = "nudSecurity";
            this.nudSecurity.Size = new System.Drawing.Size(71, 20);
            this.nudSecurity.TabIndex = 41;
            // 
            // nudArea
            // 
            this.nudArea.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudArea.Location = new System.Drawing.Point(455, 136);
            this.nudArea.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudArea.Name = "nudArea";
            this.nudArea.Size = new System.Drawing.Size(71, 20);
            this.nudArea.TabIndex = 40;
            // 
            // nudComforts
            // 
            this.nudComforts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudComforts.Location = new System.Drawing.Point(455, 110);
            this.nudComforts.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudComforts.Name = "nudComforts";
            this.nudComforts.Size = new System.Drawing.Size(71, 20);
            this.nudComforts.TabIndex = 39;
            // 
            // lblComfortsLabel
            // 
            this.lblComfortsLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblComfortsLabel.AutoSize = true;
            this.lblComfortsLabel.Location = new System.Drawing.Point(332, 113);
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
            this.lblAreaLabel.Location = new System.Drawing.Point(306, 139);
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
            this.lblSecurityLabel.Location = new System.Drawing.Point(335, 165);
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
            this.cmdAddQuality.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmdAddQuality.AutoSize = true;
            this.cmdAddQuality.Location = new System.Drawing.Point(3, 3);
            this.cmdAddQuality.Name = "cmdAddQuality";
            this.cmdAddQuality.Size = new System.Drawing.Size(90, 23);
            this.cmdAddQuality.TabIndex = 51;
            this.cmdAddQuality.Tag = "Button_AddQuality";
            this.cmdAddQuality.Text = "Add Quality";
            this.cmdAddQuality.UseVisualStyleBackColor = true;
            this.cmdAddQuality.Click += new System.EventHandler(this.cmdAddQuality_Click);
            // 
            // cmdDeleteQuality
            // 
            this.cmdDeleteQuality.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmdDeleteQuality.AutoSize = true;
            this.cmdDeleteQuality.Enabled = false;
            this.cmdDeleteQuality.Location = new System.Drawing.Point(99, 3);
            this.cmdDeleteQuality.Name = "cmdDeleteQuality";
            this.cmdDeleteQuality.Size = new System.Drawing.Size(90, 23);
            this.cmdDeleteQuality.TabIndex = 52;
            this.cmdDeleteQuality.Tag = "Button_DeleteQuality";
            this.cmdDeleteQuality.Text = "Delete Quality";
            this.cmdDeleteQuality.UseVisualStyleBackColor = true;
            this.cmdDeleteQuality.Click += new System.EventHandler(this.cmdDeleteQuality_Click);
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
            this.tableLayoutPanel1.SetRowSpan(this.treLifestyleQualities, 15);
            this.treLifestyleQualities.Size = new System.Drawing.Size(295, 388);
            this.treLifestyleQualities.TabIndex = 53;
            this.treLifestyleQualities.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treLifestyleQualities_AfterSelect);
            // 
            // chkTrustFund
            // 
            this.chkTrustFund.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkTrustFund.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkTrustFund, 2);
            this.chkTrustFund.Location = new System.Drawing.Point(455, 240);
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
            this.lblQualityLp.Location = new System.Drawing.Point(389, 318);
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
            this.lblQualityLPLabel.Location = new System.Drawing.Point(325, 318);
            this.lblQualityLPLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblQualityLPLabel.Name = "lblQualityLPLabel";
            this.lblQualityLPLabel.Size = new System.Drawing.Size(58, 13);
            this.lblQualityLPLabel.TabIndex = 70;
            this.lblQualityLPLabel.Tag = "Label_QualityLP";
            this.lblQualityLPLabel.Text = "Quality LP:";
            this.lblQualityLPLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblQualityLPLabel.Visible = false;
            // 
            // lblQualitySource
            // 
            this.lblQualitySource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblQualitySource.AutoSize = true;
            this.lblQualitySource.Location = new System.Drawing.Point(389, 368);
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
            this.lblQualitySourceLabel.Location = new System.Drawing.Point(304, 368);
            this.lblQualitySourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblQualitySourceLabel.Name = "lblQualitySourceLabel";
            this.lblQualitySourceLabel.Size = new System.Drawing.Size(79, 13);
            this.lblQualitySourceLabel.TabIndex = 68;
            this.lblQualitySourceLabel.Tag = "Label_QualitySource";
            this.lblQualitySourceLabel.Text = "Quality Source:";
            this.lblQualitySourceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblQualitySourceLabel.Visible = false;
            // 
            // lblQualityCost
            // 
            this.lblQualityCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblQualityCost.AutoSize = true;
            this.lblQualityCost.Location = new System.Drawing.Point(389, 343);
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
            this.lblQualityCostLabel.Location = new System.Drawing.Point(317, 343);
            this.lblQualityCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblQualityCostLabel.Name = "lblQualityCostLabel";
            this.lblQualityCostLabel.Size = new System.Drawing.Size(66, 13);
            this.lblQualityCostLabel.TabIndex = 72;
            this.lblQualityCostLabel.Tag = "Label_QualityCost";
            this.lblQualityCostLabel.Text = "Quality Cost:";
            this.lblQualityCostLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblQualityCostLabel.Visible = false;
            // 
            // chkPrimaryTenant
            // 
            this.chkPrimaryTenant.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkPrimaryTenant.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkPrimaryTenant, 2);
            this.chkPrimaryTenant.Location = new System.Drawing.Point(455, 214);
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
            this.lblSecurityTotal.Location = new System.Drawing.Point(532, 165);
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
            this.lblAreaTotal.Location = new System.Drawing.Point(532, 139);
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
            this.lblComfortTotal.Location = new System.Drawing.Point(532, 113);
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
            this.tableLayoutPanel1.SetColumnSpan(this.chkQualityContributesLP, 2);
            this.chkQualityContributesLP.Location = new System.Drawing.Point(455, 316);
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
            this.lblBonusLP.Location = new System.Drawing.Point(327, 62);
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
            this.nudBonusLP.Location = new System.Drawing.Point(389, 59);
            this.nudBonusLP.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
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
            this.tableLayoutPanel1.SetColumnSpan(this.chkBonusLPRandomize, 2);
            this.chkBonusLPRandomize.Location = new System.Drawing.Point(455, 60);
            this.chkBonusLPRandomize.Name = "chkBonusLPRandomize";
            this.chkBonusLPRandomize.Size = new System.Drawing.Size(102, 17);
            this.chkBonusLPRandomize.TabIndex = 83;
            this.chkBonusLPRandomize.Tag = "Checkbox_Randomize1D6";
            this.chkBonusLPRandomize.Text = "Randomize 1D6";
            this.chkBonusLPRandomize.UseVisualStyleBackColor = true;
            this.chkBonusLPRandomize.Visible = false;
            this.chkBonusLPRandomize.CheckedChanged += new System.EventHandler(this.chkTravelerBonusLPRandomize_CheckedChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 301F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.treLifestyleQualities, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblQualitySource, 2, 14);
            this.tableLayoutPanel1.Controls.Add(this.lblQualityCost, 2, 13);
            this.tableLayoutPanel1.Controls.Add(this.lblQualitySourceLabel, 1, 14);
            this.tableLayoutPanel1.Controls.Add(this.nudBonusLP, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblQualityCostLabel, 1, 13);
            this.tableLayoutPanel1.Controls.Add(this.chkPrimaryTenant, 3, 8);
            this.tableLayoutPanel1.Controls.Add(this.chkBonusLPRandomize, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblQualityLp, 2, 12);
            this.tableLayoutPanel1.Controls.Add(this.lblLifestyleNameLabel, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblQualityLPLabel, 1, 12);
            this.tableLayoutPanel1.Controls.Add(this.txtLifestyleName, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblBonusLP, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.Label_SelectAdvancedLifestyle_Lifestyle, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.chkTrustFund, 3, 9);
            this.tableLayoutPanel1.Controls.Add(this.cboBaseLifestyle, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblComfortsLabel, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblSource, 2, 11);
            this.tableLayoutPanel1.Controls.Add(this.lblAreaLabel, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.lblSourceLabel, 1, 11);
            this.tableLayoutPanel1.Controls.Add(this.lblSecurityLabel, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.lblCost, 2, 10);
            this.tableLayoutPanel1.Controls.Add(this.lblComfortTotal, 4, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblCostLabel, 1, 10);
            this.tableLayoutPanel1.Controls.Add(this.lblAreaTotal, 4, 5);
            this.tableLayoutPanel1.Controls.Add(this.nudPercentage, 2, 9);
            this.tableLayoutPanel1.Controls.Add(this.nudRoommates, 2, 8);
            this.tableLayoutPanel1.Controls.Add(this.lblPercentage, 1, 9);
            this.tableLayoutPanel1.Controls.Add(this.lblSecurityTotal, 4, 6);
            this.tableLayoutPanel1.Controls.Add(this.lblRoommates, 1, 8);
            this.tableLayoutPanel1.Controls.Add(this.nudComforts, 3, 4);
            this.tableLayoutPanel1.Controls.Add(this.nudArea, 3, 5);
            this.tableLayoutPanel1.Controls.Add(this.nudSecurity, 3, 6);
            this.tableLayoutPanel1.Controls.Add(this.Label_SelectAdvancedLifestyle_Upgrade_Comforts, 3, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblTotalLPLabel, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.lblTotalLP, 2, 7);
            this.tableLayoutPanel1.Controls.Add(this.chkQualityContributesLP, 3, 12);
            this.tableLayoutPanel1.Controls.Add(this.lblComforts, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblArea, 2, 5);
            this.tableLayoutPanel1.Controls.Add(this.lblSecurity, 2, 6);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 15);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(9, 9);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 16;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(606, 423);
            this.tableLayoutPanel1.TabIndex = 84;
            // 
            // lblComforts
            // 
            this.lblComforts.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblComforts.AutoSize = true;
            this.lblComforts.Location = new System.Drawing.Point(403, 113);
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
            this.lblArea.Location = new System.Drawing.Point(403, 139);
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
            this.lblSecurity.Location = new System.Drawing.Point(403, 165);
            this.lblSecurity.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSecurity.Name = "lblSecurity";
            this.lblSecurity.Size = new System.Drawing.Size(46, 13);
            this.lblSecurity.TabIndex = 86;
            this.lblSecurity.Tag = "Label_SelectAdvancedLifestyle_Base_Security";
            this.lblSecurity.Text = "[{0}/{1}]";
            this.lblSecurity.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 4);
            this.flowLayoutPanel1.Controls.Add(this.cmdOK);
            this.flowLayoutPanel1.Controls.Add(this.cmdOKAdd);
            this.flowLayoutPanel1.Controls.Add(this.cmdCancel);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(363, 394);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(243, 29);
            this.flowLayoutPanel1.TabIndex = 87;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel2.Controls.Add(this.cmdAddQuality);
            this.flowLayoutPanel2.Controls.Add(this.cmdDeleteQuality);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(192, 29);
            this.flowLayoutPanel2.TabIndex = 88;
            // 
            // frmSelectLifestyleAdvanced
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectLifestyleAdvanced";
            this.Padding = new System.Windows.Forms.Padding(9);
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
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
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
        private System.Windows.Forms.NumericUpDown nudPercentage;
        private System.Windows.Forms.Label lblPercentage;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblSourceLabel;
        private System.Windows.Forms.NumericUpDown nudRoommates;
        private System.Windows.Forms.Label lblRoommates;
        private ElasticComboBox cboBaseLifestyle;
        private System.Windows.Forms.Label Label_SelectAdvancedLifestyle_Lifestyle;
        private System.Windows.Forms.NumericUpDown nudSecurity;
        private System.Windows.Forms.NumericUpDown nudArea;
        private System.Windows.Forms.NumericUpDown nudComforts;
        private System.Windows.Forms.Label lblComfortsLabel;
        private System.Windows.Forms.Label lblAreaLabel;
        private System.Windows.Forms.Label lblSecurityLabel;
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
        private Label lblComforts;
        private Label lblArea;
        private Label lblSecurity;
        private FlowLayoutPanel flowLayoutPanel1;
        private BufferedTableLayoutPanel tableLayoutPanel1;
        private FlowLayoutPanel flowLayoutPanel2;
    }
}
