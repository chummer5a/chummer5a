namespace Chummer
{
	partial class frmSelectAdvancedLifestyle
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
            this.Label_SelectAdvancedLifestyle_Upgrade_Comforts = new System.Windows.Forms.Label();
            this.cmdOKAdd = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.trePositiveQualities = new System.Windows.Forms.TreeView();
            this.treNegativeQualities = new System.Windows.Forms.TreeView();
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
            this.treEntertainments = new System.Windows.Forms.TreeView();
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
            this.tabQualitiesEntertainments = new System.Windows.Forms.TabControl();
            this.tabPosQualities = new System.Windows.Forms.TabPage();
            this.tabNegQualities = new System.Windows.Forms.TabPage();
            this.tabEntertainments = new System.Windows.Forms.TabPage();
            ((System.ComponentModel.ISupportInitialize)(this.nudPercentage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRoommates)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudComfortsEntertainment)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAreaEntertainment)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSecurityEntertainment)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSecurity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudArea)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudComforts)).BeginInit();
            this.tabQualitiesEntertainments.SuspendLayout();
            this.tabPosQualities.SuspendLayout();
            this.tabNegQualities.SuspendLayout();
            this.tabEntertainments.SuspendLayout();
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
            // trePositiveQualities
            // 
            this.trePositiveQualities.CheckBoxes = true;
            this.trePositiveQualities.HideSelection = false;
            this.trePositiveQualities.Location = new System.Drawing.Point(0, 0);
            this.trePositiveQualities.Name = "trePositiveQualities";
            this.trePositiveQualities.ShowLines = false;
            this.trePositiveQualities.ShowPlusMinus = false;
            this.trePositiveQualities.ShowRootLines = false;
            this.trePositiveQualities.Size = new System.Drawing.Size(241, 290);
            this.trePositiveQualities.TabIndex = 23;
            this.trePositiveQualities.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.trePositiveQualities_AfterCheck);
            this.trePositiveQualities.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.trePositiveQualities_AfterSelect);
            // 
            // treNegativeQualities
            // 
            this.treNegativeQualities.CheckBoxes = true;
            this.treNegativeQualities.HideSelection = false;
            this.treNegativeQualities.Location = new System.Drawing.Point(0, 0);
            this.treNegativeQualities.Name = "treNegativeQualities";
            this.treNegativeQualities.ShowLines = false;
            this.treNegativeQualities.ShowPlusMinus = false;
            this.treNegativeQualities.ShowRootLines = false;
            this.treNegativeQualities.Size = new System.Drawing.Size(241, 290);
            this.treNegativeQualities.TabIndex = 25;
            this.treNegativeQualities.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treNegativeQualities_AfterCheck);
            this.treNegativeQualities.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treNegativeQualities_AfterSelect);
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
            // treEntertainments
            // 
            this.treEntertainments.CheckBoxes = true;
            this.treEntertainments.HideSelection = false;
            this.treEntertainments.Location = new System.Drawing.Point(0, 0);
            this.treEntertainments.Name = "treEntertainments";
            this.treEntertainments.ShowLines = false;
            this.treEntertainments.ShowPlusMinus = false;
            this.treEntertainments.ShowRootLines = false;
            this.treEntertainments.Size = new System.Drawing.Size(241, 290);
            this.treEntertainments.TabIndex = 30;
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
            // tabQualitiesEntertainments
            // 
            this.tabQualitiesEntertainments.Controls.Add(this.tabPosQualities);
            this.tabQualitiesEntertainments.Controls.Add(this.tabNegQualities);
            this.tabQualitiesEntertainments.Controls.Add(this.tabEntertainments);
            this.tabQualitiesEntertainments.Location = new System.Drawing.Point(2, 3);
            this.tabQualitiesEntertainments.Name = "tabQualitiesEntertainments";
            this.tabQualitiesEntertainments.SelectedIndex = 0;
            this.tabQualitiesEntertainments.Size = new System.Drawing.Size(249, 316);
            this.tabQualitiesEntertainments.TabIndex = 50;
            // 
            // tabPosQualities
            // 
            this.tabPosQualities.Controls.Add(this.trePositiveQualities);
            this.tabPosQualities.Location = new System.Drawing.Point(4, 22);
            this.tabPosQualities.Name = "tabPosQualities";
            this.tabPosQualities.Padding = new System.Windows.Forms.Padding(3);
            this.tabPosQualities.Size = new System.Drawing.Size(241, 310);
            this.tabPosQualities.TabIndex = 1;
            this.tabPosQualities.Text = "Pos. Qualities";
            this.tabPosQualities.UseVisualStyleBackColor = true;
            // 
            // tabNegQualities
            // 
            this.tabNegQualities.Controls.Add(this.treNegativeQualities);
            this.tabNegQualities.Location = new System.Drawing.Point(4, 22);
            this.tabNegQualities.Name = "tabNegQualities";
            this.tabNegQualities.Padding = new System.Windows.Forms.Padding(3);
            this.tabNegQualities.Size = new System.Drawing.Size(241, 310);
            this.tabNegQualities.TabIndex = 2;
            this.tabNegQualities.Text = "Neg. Qualities";
            this.tabNegQualities.UseVisualStyleBackColor = true;
            // 
            // tabEntertainments
            // 
            this.tabEntertainments.Controls.Add(this.treEntertainments);
            this.tabEntertainments.Location = new System.Drawing.Point(4, 22);
            this.tabEntertainments.Name = "tabEntertainments";
            this.tabEntertainments.Padding = new System.Windows.Forms.Padding(3);
            this.tabEntertainments.Size = new System.Drawing.Size(241, 290);
            this.tabEntertainments.TabIndex = 3;
            this.tabEntertainments.Text = "Entertainment";
            this.tabEntertainments.UseVisualStyleBackColor = true;
            // 
            // frmSelectAdvancedLifestyle
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(572, 342);
            this.Controls.Add(this.tabQualitiesEntertainments);
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
            this.Name = "frmSelectAdvancedLifestyle";
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
            this.tabQualitiesEntertainments.ResumeLayout(false);
            this.tabPosQualities.ResumeLayout(false);
            this.tabNegQualities.ResumeLayout(false);
            this.tabEntertainments.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

        private System.Windows.Forms.Label Label_SelectAdvancedLifestyle_Upgrade_Comforts;
		private System.Windows.Forms.Button cmdOKAdd;
		private System.Windows.Forms.Button cmdCancel;
		private System.Windows.Forms.Button cmdOK;
		private System.Windows.Forms.TreeView trePositiveQualities;
        private System.Windows.Forms.TreeView treNegativeQualities;
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
        private System.Windows.Forms.TreeView treEntertainments;
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
        private System.Windows.Forms.TabControl tabQualitiesEntertainments;
        private System.Windows.Forms.TabPage tabPosQualities;
        private System.Windows.Forms.TabPage tabNegQualities;
        private System.Windows.Forms.TabPage tabEntertainments;
	}
}