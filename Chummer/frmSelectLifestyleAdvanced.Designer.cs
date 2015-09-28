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
			System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Positive Qualities");
			System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Negative Qualities");
			System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Entertainments");
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
			this.tipTooltip = new System.Windows.Forms.ToolTip(this.components);
			this.cboBaseLifestyle = new System.Windows.Forms.ComboBox();
			this.Label_SelectAdvancedLifestyle_Base_Lifestyle = new System.Windows.Forms.Label();
			this.nudComfortsEntertainment = new System.Windows.Forms.NumericUpDown();
			this.label5 = new System.Windows.Forms.Label();
			this.nudAreaEntertainment = new System.Windows.Forms.NumericUpDown();
			this.nudSecurityEntertainment = new System.Windows.Forms.NumericUpDown();
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
			((System.ComponentModel.ISupportInitialize)(this.nudPercentage)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudRoommates)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudComfortsEntertainment)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudAreaEntertainment)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudSecurityEntertainment)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudSecurity)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudArea)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudComforts)).BeginInit();
			this.SuspendLayout();
			// 
			// Label_SelectAdvancedLifestyle_Upgrade_Comforts
			// 
			this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.AutoSize = true;
			this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.Location = new System.Drawing.Point(353, 105);
			this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.Name = "Label_SelectAdvancedLifestyle_Upgrade_Comforts";
			this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.Size = new System.Drawing.Size(51, 13);
			this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.TabIndex = 2;
			this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.Tag = "Label_SelectAdvancedLifestyle_Rating";
			this.Label_SelectAdvancedLifestyle_Upgrade_Comforts.Text = "Upgrade:";
			// 
			// cmdOKAdd
			// 
			this.cmdOKAdd.Location = new System.Drawing.Point(468, 284);
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
			this.cmdCancel.Location = new System.Drawing.Point(387, 313);
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
			this.cmdOK.Location = new System.Drawing.Point(468, 313);
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
			this.lblTotalLPLabel.Location = new System.Drawing.Point(256, 226);
			this.lblTotalLPLabel.Name = "lblTotalLPLabel";
			this.lblTotalLPLabel.Size = new System.Drawing.Size(50, 13);
			this.lblTotalLPLabel.TabIndex = 12;
			this.lblTotalLPLabel.Tag = "Label_SelectAdvancedLifestyle_TotalLP";
			this.lblTotalLPLabel.Text = "Total LP:";
			// 
			// lblTotalLP
			// 
			this.lblTotalLP.AutoSize = true;
			this.lblTotalLP.Location = new System.Drawing.Point(321, 226);
			this.lblTotalLP.Name = "lblTotalLP";
			this.lblTotalLP.Size = new System.Drawing.Size(26, 13);
			this.lblTotalLP.TabIndex = 13;
			this.lblTotalLP.Text = "[LP]";
			// 
			// lblCostLabel
			// 
			this.lblCostLabel.AutoSize = true;
			this.lblCostLabel.Location = new System.Drawing.Point(256, 295);
			this.lblCostLabel.Name = "lblCostLabel";
			this.lblCostLabel.Size = new System.Drawing.Size(66, 13);
			this.lblCostLabel.TabIndex = 18;
			this.lblCostLabel.Tag = "Label_SelectLifestyle_CostPerMonth";
			this.lblCostLabel.Text = "Cost/Month:";
			// 
			// lblCost
			// 
			this.lblCost.AutoSize = true;
			this.lblCost.Location = new System.Drawing.Point(334, 295);
			this.lblCost.Name = "lblCost";
			this.lblCost.Size = new System.Drawing.Size(34, 13);
			this.lblCost.TabIndex = 19;
			this.lblCost.Text = "[Cost]";
			// 
			// lblLifestyleNameLabel
			// 
			this.lblLifestyleNameLabel.AutoSize = true;
			this.lblLifestyleNameLabel.Location = new System.Drawing.Point(256, 25);
			this.lblLifestyleNameLabel.Name = "lblLifestyleNameLabel";
			this.lblLifestyleNameLabel.Size = new System.Drawing.Size(38, 13);
			this.lblLifestyleNameLabel.TabIndex = 0;
			this.lblLifestyleNameLabel.Tag = "Label_Name";
			this.lblLifestyleNameLabel.Text = "Name:";
			// 
			// txtLifestyleName
			// 
			this.txtLifestyleName.Location = new System.Drawing.Point(334, 22);
			this.txtLifestyleName.Name = "txtLifestyleName";
			this.txtLifestyleName.Size = new System.Drawing.Size(176, 20);
			this.txtLifestyleName.TabIndex = 1;
			// 
			// nudPercentage
			// 
			this.nudPercentage.Location = new System.Drawing.Point(337, 270);
			this.nudPercentage.Maximum = new decimal(new int[] {
            900,
            0,
            0,
            0});
			this.nudPercentage.Name = "nudPercentage";
			this.nudPercentage.Size = new System.Drawing.Size(45, 20);
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
			this.lblPercentage.Location = new System.Drawing.Point(256, 272);
			this.lblPercentage.Name = "lblPercentage";
			this.lblPercentage.Size = new System.Drawing.Size(51, 13);
			this.lblPercentage.TabIndex = 16;
			this.lblPercentage.Tag = "Label_SelectLifestyle_PercentToPay";
			this.lblPercentage.Text = "% to Pay:";
			// 
			// lblSource
			// 
			this.lblSource.AutoSize = true;
			this.lblSource.Location = new System.Drawing.Point(334, 318);
			this.lblSource.Name = "lblSource";
			this.lblSource.Size = new System.Drawing.Size(47, 13);
			this.lblSource.TabIndex = 21;
			this.lblSource.Text = "[Source]";
			// 
			// lblSourceLabel
			// 
			this.lblSourceLabel.AutoSize = true;
			this.lblSourceLabel.Location = new System.Drawing.Point(256, 318);
			this.lblSourceLabel.Name = "lblSourceLabel";
			this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
			this.lblSourceLabel.TabIndex = 20;
			this.lblSourceLabel.Tag = "Label_Source";
			this.lblSourceLabel.Text = "Source:";
			// 
			// nudRoommates
			// 
			this.nudRoommates.Location = new System.Drawing.Point(337, 247);
			this.nudRoommates.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.nudRoommates.Name = "nudRoommates";
			this.nudRoommates.Size = new System.Drawing.Size(45, 20);
			this.nudRoommates.TabIndex = 15;
			this.nudRoommates.ValueChanged += new System.EventHandler(this.nudRoommates_ValueChanged);
			// 
			// lblRoommates
			// 
			this.lblRoommates.AutoSize = true;
			this.lblRoommates.Location = new System.Drawing.Point(256, 249);
			this.lblRoommates.Name = "lblRoommates";
			this.lblRoommates.Size = new System.Drawing.Size(66, 13);
			this.lblRoommates.TabIndex = 14;
			this.lblRoommates.Tag = "Label_SelectLifestyle_Roommates";
			this.lblRoommates.Text = "Roommates:";
			// 
			// tipTooltip
			// 
			this.tipTooltip.AutoPopDelay = 10000;
			this.tipTooltip.InitialDelay = 250;
			this.tipTooltip.IsBalloon = true;
			this.tipTooltip.ReshowDelay = 100;
			this.tipTooltip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
			this.tipTooltip.ToolTipTitle = "Chummer Help";
			// 
			// cboBaseLifestyle
			// 
			this.cboBaseLifestyle.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.cboBaseLifestyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboBaseLifestyle.FormattingEnabled = true;
			this.cboBaseLifestyle.Location = new System.Drawing.Point(334, 48);
			this.cboBaseLifestyle.Name = "cboBaseLifestyle";
			this.cboBaseLifestyle.Size = new System.Drawing.Size(176, 21);
			this.cboBaseLifestyle.TabIndex = 32;
			this.cboBaseLifestyle.SelectedIndexChanged += new System.EventHandler(this.cboBaseLifestyle_SelectedIndexChanged);
			// 
			// Label_SelectAdvancedLifestyle_Base_Lifestyle
			// 
			this.Label_SelectAdvancedLifestyle_Base_Lifestyle.AutoSize = true;
			this.Label_SelectAdvancedLifestyle_Base_Lifestyle.Location = new System.Drawing.Point(256, 51);
			this.Label_SelectAdvancedLifestyle_Base_Lifestyle.Name = "Label_SelectAdvancedLifestyle_Base_Lifestyle";
			this.Label_SelectAdvancedLifestyle_Base_Lifestyle.Size = new System.Drawing.Size(72, 13);
			this.Label_SelectAdvancedLifestyle_Base_Lifestyle.TabIndex = 31;
			this.Label_SelectAdvancedLifestyle_Base_Lifestyle.Tag = "Label_SelectAdvancedLifestyle_Base_Lifestyle";
			this.Label_SelectAdvancedLifestyle_Base_Lifestyle.Text = "Base Lifestyle";
			// 
			// nudComfortsEntertainment
			// 
			this.nudComfortsEntertainment.Location = new System.Drawing.Point(441, 122);
			this.nudComfortsEntertainment.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.nudComfortsEntertainment.Name = "nudComfortsEntertainment";
			this.nudComfortsEntertainment.Size = new System.Drawing.Size(69, 20);
			this.nudComfortsEntertainment.TabIndex = 33;
			this.nudComfortsEntertainment.ValueChanged += new System.EventHandler(this.nudComfortsEntertainment_ValueChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(438, 105);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(72, 13);
			this.label5.TabIndex = 34;
			this.label5.Tag = "Label_SelectAdvancedLifestyle_Entertainment";
			this.label5.Text = "Entertainment";
			// 
			// nudAreaEntertainment
			// 
			this.nudAreaEntertainment.Location = new System.Drawing.Point(441, 147);
			this.nudAreaEntertainment.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.nudAreaEntertainment.Name = "nudAreaEntertainment";
			this.nudAreaEntertainment.Size = new System.Drawing.Size(69, 20);
			this.nudAreaEntertainment.TabIndex = 35;
			this.nudAreaEntertainment.ValueChanged += new System.EventHandler(this.nudAreaEntertainment_ValueChanged);
			// 
			// nudSecurityEntertainment
			// 
			this.nudSecurityEntertainment.Location = new System.Drawing.Point(441, 173);
			this.nudSecurityEntertainment.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.nudSecurityEntertainment.Name = "nudSecurityEntertainment";
			this.nudSecurityEntertainment.Size = new System.Drawing.Size(69, 20);
			this.nudSecurityEntertainment.TabIndex = 37;
			// 
			// nudSecurity
			// 
			this.nudSecurity.Location = new System.Drawing.Point(356, 173);
			this.nudSecurity.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.nudSecurity.Name = "nudSecurity";
			this.nudSecurity.Size = new System.Drawing.Size(69, 20);
			this.nudSecurity.TabIndex = 41;
			this.nudSecurity.ValueChanged += new System.EventHandler(this.nudSecurity_ValueChanged);
			// 
			// nudArea
			// 
			this.nudArea.Location = new System.Drawing.Point(356, 147);
			this.nudArea.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.nudArea.Name = "nudArea";
			this.nudArea.Size = new System.Drawing.Size(69, 20);
			this.nudArea.TabIndex = 40;
			this.nudArea.ValueChanged += new System.EventHandler(this.nudArea_ValueChanged);
			// 
			// nudComforts
			// 
			this.nudComforts.Location = new System.Drawing.Point(356, 122);
			this.nudComforts.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.nudComforts.Name = "nudComforts";
			this.nudComforts.Size = new System.Drawing.Size(69, 20);
			this.nudComforts.TabIndex = 39;
			this.nudComforts.ValueChanged += new System.EventHandler(this.nudComforts_ValueChanged);
			// 
			// Label_SelectAdvancedLifestyle_Base_Comforts
			// 
			this.Label_SelectAdvancedLifestyle_Base_Comforts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.Label_SelectAdvancedLifestyle_Base_Comforts.Location = new System.Drawing.Point(256, 124);
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
			this.Label_SelectAdvancedLifestyle_Base_Area.Location = new System.Drawing.Point(256, 149);
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
			this.Label_SelectAdvancedLifestyle_Base_Securities.Location = new System.Drawing.Point(256, 175);
			this.Label_SelectAdvancedLifestyle_Base_Securities.Name = "Label_SelectAdvancedLifestyle_Base_Securities";
			this.Label_SelectAdvancedLifestyle_Base_Securities.Size = new System.Drawing.Size(93, 13);
			this.Label_SelectAdvancedLifestyle_Base_Securities.TabIndex = 48;
			this.Label_SelectAdvancedLifestyle_Base_Securities.Tag = "Label_SelectAdvancedLifestyle_Base_Security";
			this.Label_SelectAdvancedLifestyle_Base_Securities.Text = "Security:[{0}/{1}]";
			this.Label_SelectAdvancedLifestyle_Base_Securities.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// cmdAddQuality
			// 
			this.cmdAddQuality.Location = new System.Drawing.Point(1, 313);
			this.cmdAddQuality.Name = "cmdAddQuality";
			this.cmdAddQuality.Size = new System.Drawing.Size(122, 23);
			this.cmdAddQuality.TabIndex = 51;
			this.cmdAddQuality.Text = "Add Quality";
			this.cmdAddQuality.UseVisualStyleBackColor = true;
			this.cmdAddQuality.Click += new System.EventHandler(this.cmdAddQuality_Click);
			// 
			// cmdDeleteQuality
			// 
			this.cmdDeleteQuality.Location = new System.Drawing.Point(129, 313);
			this.cmdDeleteQuality.Name = "cmdDeleteQuality";
			this.cmdDeleteQuality.Size = new System.Drawing.Size(122, 23);
			this.cmdDeleteQuality.TabIndex = 52;
			this.cmdDeleteQuality.Text = "Delete Quality";
			this.cmdDeleteQuality.UseVisualStyleBackColor = true;
			this.cmdDeleteQuality.Click += new System.EventHandler(this.cmdDeleteQuality_Click);
			// 
			// treLifestyleQualities
			// 
			this.treLifestyleQualities.Location = new System.Drawing.Point(1, 8);
			this.treLifestyleQualities.Name = "treLifestyleQualities";
			treeNode4.Name = "nodPositiveLifestyleQualities";
			treeNode4.Text = "Positive Qualities";
			treeNode5.Name = "nodNegativeLifestyleQualities";
			treeNode5.Text = "Negative Qualities";
			treeNode6.Name = "nodLifestyleEntertainments";
			treeNode6.Text = "Entertainments";
			this.treLifestyleQualities.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode4,
            treeNode5,
            treeNode6});
			this.treLifestyleQualities.Size = new System.Drawing.Size(249, 299);
			this.treLifestyleQualities.TabIndex = 53;
			// 
			// chkTrustFund
			// 
			this.chkTrustFund.AutoSize = true;
			this.chkTrustFund.Location = new System.Drawing.Point(259, 206);
			this.chkTrustFund.Name = "chkTrustFund";
			this.chkTrustFund.Size = new System.Drawing.Size(77, 17);
			this.chkTrustFund.TabIndex = 54;
			this.chkTrustFund.Text = "Trust Fund";
			this.chkTrustFund.UseVisualStyleBackColor = true;
			this.chkTrustFund.Visible = false;
			this.chkTrustFund.CheckedChanged += new System.EventHandler(this.chkTrustFund_Changed);
			// 
			// frmSelectLifestyleAdvanced
			// 
			this.AcceptButton = this.cmdOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cmdCancel;
			this.ClientSize = new System.Drawing.Size(572, 342);
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
			this.Controls.Add(this.nudSecurityEntertainment);
			this.Controls.Add(this.nudAreaEntertainment);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.nudComfortsEntertainment);
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
			this.Load += new System.EventHandler(this.frmSelectAdvancedLifestyle_Load);
			((System.ComponentModel.ISupportInitialize)(this.nudPercentage)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudRoommates)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudComfortsEntertainment)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudAreaEntertainment)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudSecurityEntertainment)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudSecurity)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudArea)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudComforts)).EndInit();
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
        private System.Windows.Forms.ToolTip tipTooltip;
        private System.Windows.Forms.ComboBox cboBaseLifestyle;
        private System.Windows.Forms.Label Label_SelectAdvancedLifestyle_Base_Lifestyle;
        private System.Windows.Forms.NumericUpDown nudComfortsEntertainment;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown nudAreaEntertainment;
        private System.Windows.Forms.NumericUpDown nudSecurityEntertainment;
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
	}
}