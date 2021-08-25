namespace Chummer
{
    partial class frmSelectLifestyle
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
            this.cboLifestyle = new Chummer.ElasticComboBox();
            this.lblLifestyles = new System.Windows.Forms.Label();
            this.cmdOKAdd = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.treQualities = new System.Windows.Forms.TreeView();
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
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpRight = new Chummer.BufferedTableLayoutPanel(this.components);
            this.chkTrustFund = new Chummer.ColorableCheckBox(this.components);
            this.lblBorough = new System.Windows.Forms.Label();
            this.lblDistrict = new System.Windows.Forms.Label();
            this.lblCity = new System.Windows.Forms.Label();
            this.chkPrimaryTenant = new Chummer.ColorableCheckBox(this.components);
            this.cboCity = new Chummer.ElasticComboBox();
            this.cboDistrict = new Chummer.ElasticComboBox();
            this.cboBorough = new Chummer.ElasticComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudPercentage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRoommates)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            this.tlpRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // cboLifestyle
            // 
            this.cboLifestyle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpRight.SetColumnSpan(this.cboLifestyle, 2);
            this.cboLifestyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLifestyle.FormattingEnabled = true;
            this.cboLifestyle.Location = new System.Drawing.Point(75, 29);
            this.cboLifestyle.Name = "cboLifestyle";
            this.cboLifestyle.Size = new System.Drawing.Size(225, 21);
            this.cboLifestyle.TabIndex = 3;
            this.cboLifestyle.TooltipText = "";
            this.cboLifestyle.SelectedIndexChanged += new System.EventHandler(this.RefreshValues);
            // 
            // lblLifestyles
            // 
            this.lblLifestyles.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblLifestyles.AutoSize = true;
            this.lblLifestyles.Location = new System.Drawing.Point(21, 33);
            this.lblLifestyles.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLifestyles.Name = "lblLifestyles";
            this.lblLifestyles.Size = new System.Drawing.Size(48, 13);
            this.lblLifestyles.TabIndex = 2;
            this.lblLifestyles.Tag = "Label_SelectAdvancedLifestyle_Comforts";
            this.lblLifestyles.Text = "Lifestyle:";
            this.lblLifestyles.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
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
            // treQualities
            // 
            this.treQualities.CheckBoxes = true;
            this.treQualities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treQualities.HideSelection = false;
            this.treQualities.Location = new System.Drawing.Point(3, 3);
            this.treQualities.Name = "treQualities";
            this.tlpMain.SetRowSpan(this.treQualities, 2);
            this.treQualities.ShowLines = false;
            this.treQualities.ShowPlusMinus = false;
            this.treQualities.ShowRootLines = false;
            this.treQualities.Size = new System.Drawing.Size(297, 417);
            this.treQualities.TabIndex = 23;
            this.treQualities.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treQualities_AfterCheck);
            this.treQualities.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treQualities_AfterSelect);
            // 
            // lblCostLabel
            // 
            this.lblCostLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCostLabel.AutoSize = true;
            this.lblCostLabel.Location = new System.Drawing.Point(3, 111);
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
            this.tlpRight.SetColumnSpan(this.lblCost, 2);
            this.lblCost.Location = new System.Drawing.Point(75, 111);
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
            this.lblLifestyleNameLabel.Location = new System.Drawing.Point(31, 6);
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
            this.tlpRight.SetColumnSpan(this.txtLifestyleName, 2);
            this.txtLifestyleName.Location = new System.Drawing.Point(75, 3);
            this.txtLifestyleName.Name = "txtLifestyleName";
            this.txtLifestyleName.Size = new System.Drawing.Size(225, 20);
            this.txtLifestyleName.TabIndex = 1;
            // 
            // nudPercentage
            // 
            this.nudPercentage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudPercentage.AutoSize = true;
            this.nudPercentage.DecimalPlaces = 2;
            this.nudPercentage.Location = new System.Drawing.Point(75, 82);
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
            this.nudPercentage.Name = "nudPercentage";
            this.nudPercentage.Size = new System.Drawing.Size(56, 20);
            this.nudPercentage.TabIndex = 17;
            this.nudPercentage.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudPercentage.ValueChanged += new System.EventHandler(this.RefreshValues);
            // 
            // lblPercentage
            // 
            this.lblPercentage.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblPercentage.AutoSize = true;
            this.lblPercentage.Location = new System.Drawing.Point(18, 85);
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
            this.lblSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSource.Location = new System.Drawing.Point(75, 136);
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
            this.lblSourceLabel.Location = new System.Drawing.Point(25, 136);
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
            this.nudRoommates.Location = new System.Drawing.Point(75, 56);
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
            this.nudRoommates.Name = "nudRoommates";
            this.nudRoommates.Size = new System.Drawing.Size(56, 20);
            this.nudRoommates.TabIndex = 15;
            this.nudRoommates.ValueChanged += new System.EventHandler(this.nudRoommates_ValueChanged);
            // 
            // lblRoommates
            // 
            this.lblRoommates.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblRoommates.AutoSize = true;
            this.lblRoommates.Location = new System.Drawing.Point(3, 59);
            this.lblRoommates.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRoommates.Name = "lblRoommates";
            this.lblRoommates.Size = new System.Drawing.Size(66, 13);
            this.lblRoommates.TabIndex = 14;
            this.lblRoommates.Tag = "Label_SelectLifestyle_Roommates";
            this.lblRoommates.Text = "Roommates:";
            this.lblRoommates.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.Controls.Add(this.treQualities, 0, 0);
            this.tlpMain.Controls.Add(this.tlpButtons, 1, 1);
            this.tlpMain.Controls.Add(this.tlpRight, 1, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Size = new System.Drawing.Size(606, 423);
            this.tlpMain.TabIndex = 29;
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
            this.tlpButtons.Location = new System.Drawing.Point(372, 394);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButtons.Size = new System.Drawing.Size(234, 29);
            this.tlpButtons.TabIndex = 26;
            // 
            // tlpRight
            // 
            this.tlpRight.AutoSize = true;
            this.tlpRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.ColumnCount = 3;
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpRight.Controls.Add(this.chkTrustFund, 2, 3);
            this.tlpRight.Controls.Add(this.lblSource, 1, 5);
            this.tlpRight.Controls.Add(this.lblBorough, 0, 8);
            this.tlpRight.Controls.Add(this.lblDistrict, 0, 7);
            this.tlpRight.Controls.Add(this.lblCity, 0, 6);
            this.tlpRight.Controls.Add(this.lblSourceLabel, 0, 5);
            this.tlpRight.Controls.Add(this.lblLifestyleNameLabel, 0, 0);
            this.tlpRight.Controls.Add(this.lblLifestyles, 0, 1);
            this.tlpRight.Controls.Add(this.lblRoommates, 0, 2);
            this.tlpRight.Controls.Add(this.lblCost, 1, 4);
            this.tlpRight.Controls.Add(this.lblCostLabel, 0, 4);
            this.tlpRight.Controls.Add(this.chkPrimaryTenant, 2, 2);
            this.tlpRight.Controls.Add(this.lblPercentage, 0, 3);
            this.tlpRight.Controls.Add(this.txtLifestyleName, 1, 0);
            this.tlpRight.Controls.Add(this.cboLifestyle, 1, 1);
            this.tlpRight.Controls.Add(this.nudRoommates, 1, 2);
            this.tlpRight.Controls.Add(this.nudPercentage, 1, 3);
            this.tlpRight.Controls.Add(this.cboCity, 1, 6);
            this.tlpRight.Controls.Add(this.cboDistrict, 1, 7);
            this.tlpRight.Controls.Add(this.cboBorough, 1, 8);
            this.tlpRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRight.Location = new System.Drawing.Point(303, 0);
            this.tlpRight.Margin = new System.Windows.Forms.Padding(0);
            this.tlpRight.Name = "tlpRight";
            this.tlpRight.RowCount = 9;
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.Size = new System.Drawing.Size(303, 236);
            this.tlpRight.TabIndex = 33;
            // 
            // chkTrustFund
            // 
            this.chkTrustFund.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkTrustFund.AutoSize = true;
            this.chkTrustFund.DefaultColorScheme = true;
            this.chkTrustFund.Location = new System.Drawing.Point(137, 83);
            this.chkTrustFund.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkTrustFund.Name = "chkTrustFund";
            this.chkTrustFund.Size = new System.Drawing.Size(77, 17);
            this.chkTrustFund.TabIndex = 55;
            this.chkTrustFund.Text = "Trust Fund";
            this.chkTrustFund.UseVisualStyleBackColor = true;
            this.chkTrustFund.Visible = false;
            this.chkTrustFund.CheckedChanged += new System.EventHandler(this.chkTrustFund_CheckedChanged);
            // 
            // lblBorough
            // 
            this.lblBorough.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblBorough.AutoSize = true;
            this.lblBorough.Location = new System.Drawing.Point(19, 216);
            this.lblBorough.Name = "lblBorough";
            this.lblBorough.Size = new System.Drawing.Size(50, 13);
            this.lblBorough.TabIndex = 29;
            this.lblBorough.Tag = "Label_SelectLifestyle_Borough";
            this.lblBorough.Text = "Borough:";
            this.lblBorough.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblDistrict
            // 
            this.lblDistrict.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDistrict.AutoSize = true;
            this.lblDistrict.Location = new System.Drawing.Point(27, 189);
            this.lblDistrict.Name = "lblDistrict";
            this.lblDistrict.Size = new System.Drawing.Size(42, 13);
            this.lblDistrict.TabIndex = 28;
            this.lblDistrict.Tag = "Label_SelectLifestyle_District";
            this.lblDistrict.Text = "District:";
            this.lblDistrict.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblCity
            // 
            this.lblCity.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCity.AutoSize = true;
            this.lblCity.Location = new System.Drawing.Point(42, 162);
            this.lblCity.Name = "lblCity";
            this.lblCity.Size = new System.Drawing.Size(27, 13);
            this.lblCity.TabIndex = 27;
            this.lblCity.Tag = "Label_SelectLifestyle_City";
            this.lblCity.Text = "City:";
            this.lblCity.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkPrimaryTenant
            // 
            this.chkPrimaryTenant.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkPrimaryTenant.AutoSize = true;
            this.chkPrimaryTenant.Checked = true;
            this.chkPrimaryTenant.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkPrimaryTenant.DefaultColorScheme = true;
            this.chkPrimaryTenant.Location = new System.Drawing.Point(137, 57);
            this.chkPrimaryTenant.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkPrimaryTenant.Name = "chkPrimaryTenant";
            this.chkPrimaryTenant.Size = new System.Drawing.Size(97, 17);
            this.chkPrimaryTenant.TabIndex = 25;
            this.chkPrimaryTenant.Text = "Primary Tenant";
            this.chkPrimaryTenant.UseVisualStyleBackColor = true;
            this.chkPrimaryTenant.CheckedChanged += new System.EventHandler(this.RefreshValues);
            // 
            // cboCity
            // 
            this.cboCity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpRight.SetColumnSpan(this.cboCity, 2);
            this.cboCity.FormattingEnabled = true;
            this.cboCity.Location = new System.Drawing.Point(75, 158);
            this.cboCity.Name = "cboCity";
            this.cboCity.Size = new System.Drawing.Size(225, 21);
            this.cboCity.TabIndex = 30;
            this.cboCity.TooltipText = "";
            this.cboCity.SelectedIndexChanged += new System.EventHandler(this.cboCity_SelectedIndexChanged);
            // 
            // cboDistrict
            // 
            this.cboDistrict.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpRight.SetColumnSpan(this.cboDistrict, 2);
            this.cboDistrict.FormattingEnabled = true;
            this.cboDistrict.Location = new System.Drawing.Point(75, 185);
            this.cboDistrict.Name = "cboDistrict";
            this.cboDistrict.Size = new System.Drawing.Size(225, 21);
            this.cboDistrict.TabIndex = 31;
            this.cboDistrict.TooltipText = "";
            this.cboDistrict.SelectedIndexChanged += new System.EventHandler(this.cboDistrict_SelectedIndexChanged);
            // 
            // cboBorough
            // 
            this.cboBorough.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpRight.SetColumnSpan(this.cboBorough, 2);
            this.cboBorough.FormattingEnabled = true;
            this.cboBorough.Location = new System.Drawing.Point(75, 212);
            this.cboBorough.Name = "cboBorough";
            this.cboBorough.Size = new System.Drawing.Size(225, 21);
            this.cboBorough.TabIndex = 32;
            this.cboBorough.TooltipText = "";
            // 
            // frmSelectLifestyle
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectLifestyle";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectAdvancedLifestyle";
            this.Text = "Build Lifestyle";
            this.Load += new System.EventHandler(this.frmSelectLifestyle_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudPercentage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRoommates)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.tlpRight.ResumeLayout(false);
            this.tlpRight.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ElasticComboBox cboLifestyle;
        private System.Windows.Forms.Label lblLifestyles;
        private System.Windows.Forms.Button cmdOKAdd;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.TreeView treQualities;
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
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private Chummer.ColorableCheckBox chkPrimaryTenant;
        private BufferedTableLayoutPanel tlpButtons;
        private System.Windows.Forms.Label lblCity;
        private System.Windows.Forms.Label lblDistrict;
        private System.Windows.Forms.Label lblBorough;
        private ElasticComboBox cboCity;
        private ElasticComboBox cboDistrict;
        private ElasticComboBox cboBorough;
        private BufferedTableLayoutPanel tlpRight;
        private ColorableCheckBox chkTrustFund;
    }
}
