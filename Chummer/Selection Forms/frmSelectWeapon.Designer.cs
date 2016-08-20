namespace Chummer
{
	partial class frmSelectWeapon
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
			this.cboCategory = new System.Windows.Forms.ComboBox();
			this.lblCategory = new System.Windows.Forms.Label();
			this.lstWeapon = new System.Windows.Forms.ListBox();
			this.cmdOK = new System.Windows.Forms.Button();
			this.cmdCancel = new System.Windows.Forms.Button();
			this.lblWeaponAmmo = new System.Windows.Forms.Label();
			this.lblWeaponAmmoLabel = new System.Windows.Forms.Label();
			this.lblWeaponMode = new System.Windows.Forms.Label();
			this.lblWeaponModeLabel = new System.Windows.Forms.Label();
			this.lblWeaponReach = new System.Windows.Forms.Label();
			this.lblWeaponReachLabel = new System.Windows.Forms.Label();
			this.lblWeaponAP = new System.Windows.Forms.Label();
			this.lblWeaponAPLabel = new System.Windows.Forms.Label();
			this.lblWeaponCost = new System.Windows.Forms.Label();
			this.lblWeaponCostLabel = new System.Windows.Forms.Label();
			this.lblWeaponAvail = new System.Windows.Forms.Label();
			this.lblWeaponAvailLabel = new System.Windows.Forms.Label();
			this.lblWeaponRC = new System.Windows.Forms.Label();
			this.lblWeaponRCLabel = new System.Windows.Forms.Label();
			this.lblWeaponDamage = new System.Windows.Forms.Label();
			this.lblWeaponDamageLabel = new System.Windows.Forms.Label();
			this.txtSearch = new System.Windows.Forms.TextBox();
			this.lblSearchLabel = new System.Windows.Forms.Label();
			this.cmdOKAdd = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.lblIncludedAccessories = new System.Windows.Forms.Label();
			this.lblSource = new System.Windows.Forms.Label();
			this.lblSourceLabel = new System.Windows.Forms.Label();
			this.chkFreeItem = new System.Windows.Forms.CheckBox();
			this.nudMarkup = new System.Windows.Forms.NumericUpDown();
			this.lblMarkupLabel = new System.Windows.Forms.Label();
			this.lblMarkupPercentLabel = new System.Windows.Forms.Label();
			this.lblTest = new System.Windows.Forms.Label();
			this.lblTestLabel = new System.Windows.Forms.Label();
			this.tipTooltip = new System.Windows.Forms.ToolTip(this.components);
			this.lblWeaponAccuracy = new System.Windows.Forms.Label();
			this.lblWeaponAccuracyLabel = new System.Windows.Forms.Label();
			this.dgvWeapons = new System.Windows.Forms.DataGridView();
			this.WeaponName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Dice = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Accuracy = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Damage = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.AP = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.RC = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Ammo = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Mode = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Reach = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Accessories = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Avail = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Source = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Cost = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.chkBrowse = new System.Windows.Forms.CheckBox();
			this.tmrSearch = new System.Windows.Forms.Timer(this.components);
			this.chkBlackMarketDiscount = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvWeapons)).BeginInit();
			this.SuspendLayout();
			// 
			// cboCategory
			// 
			this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboCategory.FormattingEnabled = true;
			this.cboCategory.Location = new System.Drawing.Point(70, 6);
			this.cboCategory.Name = "cboCategory";
			this.cboCategory.Size = new System.Drawing.Size(182, 21);
			this.cboCategory.TabIndex = 30;
			this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
			// 
			// lblCategory
			// 
			this.lblCategory.AutoSize = true;
			this.lblCategory.Location = new System.Drawing.Point(12, 9);
			this.lblCategory.Name = "lblCategory";
			this.lblCategory.Size = new System.Drawing.Size(52, 13);
			this.lblCategory.TabIndex = 29;
			this.lblCategory.Tag = "Label_Category";
			this.lblCategory.Text = "Category:";
			// 
			// lstWeapon
			// 
			this.lstWeapon.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.lstWeapon.FormattingEnabled = true;
			this.lstWeapon.Location = new System.Drawing.Point(12, 33);
			this.lstWeapon.Name = "lstWeapon";
			this.lstWeapon.Size = new System.Drawing.Size(240, 394);
			this.lstWeapon.TabIndex = 28;
			this.lstWeapon.SelectedIndexChanged += new System.EventHandler(this.lstWeapon_SelectedIndexChanged);
			this.lstWeapon.DoubleClick += new System.EventHandler(this.lstWeapon_DoubleClick);
			// 
			// cmdOK
			// 
			this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdOK.Location = new System.Drawing.Point(489, 437);
			this.cmdOK.Name = "cmdOK";
			this.cmdOK.Size = new System.Drawing.Size(75, 23);
			this.cmdOK.TabIndex = 31;
			this.cmdOK.Tag = "String_OK";
			this.cmdOK.Text = "OK";
			this.cmdOK.UseVisualStyleBackColor = true;
			this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
			// 
			// cmdCancel
			// 
			this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCancel.Location = new System.Drawing.Point(408, 437);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.Size = new System.Drawing.Size(75, 23);
			this.cmdCancel.TabIndex = 33;
			this.cmdCancel.Tag = "String_Cancel";
			this.cmdCancel.Text = "Cancel";
			this.cmdCancel.UseVisualStyleBackColor = true;
			this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
			// 
			// lblWeaponAmmo
			// 
			this.lblWeaponAmmo.AutoSize = true;
			this.lblWeaponAmmo.Location = new System.Drawing.Point(433, 68);
			this.lblWeaponAmmo.Name = "lblWeaponAmmo";
			this.lblWeaponAmmo.Size = new System.Drawing.Size(42, 13);
			this.lblWeaponAmmo.TabIndex = 9;
			this.lblWeaponAmmo.Text = "[Ammo]";
			// 
			// lblWeaponAmmoLabel
			// 
			this.lblWeaponAmmoLabel.AutoSize = true;
			this.lblWeaponAmmoLabel.Location = new System.Drawing.Point(372, 68);
			this.lblWeaponAmmoLabel.Name = "lblWeaponAmmoLabel";
			this.lblWeaponAmmoLabel.Size = new System.Drawing.Size(39, 13);
			this.lblWeaponAmmoLabel.TabIndex = 8;
			this.lblWeaponAmmoLabel.Tag = "Label_Ammo";
			this.lblWeaponAmmoLabel.Text = "Ammo:";
			// 
			// lblWeaponMode
			// 
			this.lblWeaponMode.AutoSize = true;
			this.lblWeaponMode.Location = new System.Drawing.Point(433, 91);
			this.lblWeaponMode.Name = "lblWeaponMode";
			this.lblWeaponMode.Size = new System.Drawing.Size(40, 13);
			this.lblWeaponMode.TabIndex = 13;
			this.lblWeaponMode.Text = "[Mode]";
			// 
			// lblWeaponModeLabel
			// 
			this.lblWeaponModeLabel.AutoSize = true;
			this.lblWeaponModeLabel.Location = new System.Drawing.Point(372, 91);
			this.lblWeaponModeLabel.Name = "lblWeaponModeLabel";
			this.lblWeaponModeLabel.Size = new System.Drawing.Size(37, 13);
			this.lblWeaponModeLabel.TabIndex = 12;
			this.lblWeaponModeLabel.Tag = "Label_Mode";
			this.lblWeaponModeLabel.Text = "Mode:";
			// 
			// lblWeaponReach
			// 
			this.lblWeaponReach.AutoSize = true;
			this.lblWeaponReach.Location = new System.Drawing.Point(315, 91);
			this.lblWeaponReach.Name = "lblWeaponReach";
			this.lblWeaponReach.Size = new System.Drawing.Size(45, 13);
			this.lblWeaponReach.TabIndex = 11;
			this.lblWeaponReach.Text = "[Reach]";
			// 
			// lblWeaponReachLabel
			// 
			this.lblWeaponReachLabel.AutoSize = true;
			this.lblWeaponReachLabel.Location = new System.Drawing.Point(258, 91);
			this.lblWeaponReachLabel.Name = "lblWeaponReachLabel";
			this.lblWeaponReachLabel.Size = new System.Drawing.Size(42, 13);
			this.lblWeaponReachLabel.TabIndex = 10;
			this.lblWeaponReachLabel.Tag = "Label_Reach";
			this.lblWeaponReachLabel.Text = "Reach:";
			// 
			// lblWeaponAP
			// 
			this.lblWeaponAP.AutoSize = true;
			this.lblWeaponAP.Location = new System.Drawing.Point(315, 68);
			this.lblWeaponAP.Name = "lblWeaponAP";
			this.lblWeaponAP.Size = new System.Drawing.Size(27, 13);
			this.lblWeaponAP.TabIndex = 7;
			this.lblWeaponAP.Text = "[AP]";
			// 
			// lblWeaponAPLabel
			// 
			this.lblWeaponAPLabel.AutoSize = true;
			this.lblWeaponAPLabel.Location = new System.Drawing.Point(258, 68);
			this.lblWeaponAPLabel.Name = "lblWeaponAPLabel";
			this.lblWeaponAPLabel.Size = new System.Drawing.Size(24, 13);
			this.lblWeaponAPLabel.TabIndex = 6;
			this.lblWeaponAPLabel.Tag = "Label_AP";
			this.lblWeaponAPLabel.Text = "AP:";
			// 
			// lblWeaponCost
			// 
			this.lblWeaponCost.AutoSize = true;
			this.lblWeaponCost.Location = new System.Drawing.Point(315, 137);
			this.lblWeaponCost.Name = "lblWeaponCost";
			this.lblWeaponCost.Size = new System.Drawing.Size(34, 13);
			this.lblWeaponCost.TabIndex = 19;
			this.lblWeaponCost.Text = "[Cost]";
			// 
			// lblWeaponCostLabel
			// 
			this.lblWeaponCostLabel.AutoSize = true;
			this.lblWeaponCostLabel.Location = new System.Drawing.Point(258, 137);
			this.lblWeaponCostLabel.Name = "lblWeaponCostLabel";
			this.lblWeaponCostLabel.Size = new System.Drawing.Size(31, 13);
			this.lblWeaponCostLabel.TabIndex = 18;
			this.lblWeaponCostLabel.Tag = "Label_Cost";
			this.lblWeaponCostLabel.Text = "Cost:";
			// 
			// lblWeaponAvail
			// 
			this.lblWeaponAvail.AutoSize = true;
			this.lblWeaponAvail.Location = new System.Drawing.Point(315, 114);
			this.lblWeaponAvail.Name = "lblWeaponAvail";
			this.lblWeaponAvail.Size = new System.Drawing.Size(36, 13);
			this.lblWeaponAvail.TabIndex = 15;
			this.lblWeaponAvail.Text = "[Avail]";
			// 
			// lblWeaponAvailLabel
			// 
			this.lblWeaponAvailLabel.AutoSize = true;
			this.lblWeaponAvailLabel.Location = new System.Drawing.Point(258, 114);
			this.lblWeaponAvailLabel.Name = "lblWeaponAvailLabel";
			this.lblWeaponAvailLabel.Size = new System.Drawing.Size(33, 13);
			this.lblWeaponAvailLabel.TabIndex = 14;
			this.lblWeaponAvailLabel.Tag = "Label_Avail";
			this.lblWeaponAvailLabel.Text = "Avail:";
			// 
			// lblWeaponRC
			// 
			this.lblWeaponRC.AutoSize = true;
			this.lblWeaponRC.Location = new System.Drawing.Point(466, 45);
			this.lblWeaponRC.Name = "lblWeaponRC";
			this.lblWeaponRC.Size = new System.Drawing.Size(28, 13);
			this.lblWeaponRC.TabIndex = 5;
			this.lblWeaponRC.Text = "[RC]";
			// 
			// lblWeaponRCLabel
			// 
			this.lblWeaponRCLabel.AutoSize = true;
			this.lblWeaponRCLabel.Location = new System.Drawing.Point(409, 45);
			this.lblWeaponRCLabel.Name = "lblWeaponRCLabel";
			this.lblWeaponRCLabel.Size = new System.Drawing.Size(25, 13);
			this.lblWeaponRCLabel.TabIndex = 4;
			this.lblWeaponRCLabel.Tag = "Label_RC";
			this.lblWeaponRCLabel.Text = "RC:";
			// 
			// lblWeaponDamage
			// 
			this.lblWeaponDamage.AutoSize = true;
			this.lblWeaponDamage.Location = new System.Drawing.Point(315, 45);
			this.lblWeaponDamage.Name = "lblWeaponDamage";
			this.lblWeaponDamage.Size = new System.Drawing.Size(53, 13);
			this.lblWeaponDamage.TabIndex = 3;
			this.lblWeaponDamage.Text = "[Damage]";
			// 
			// lblWeaponDamageLabel
			// 
			this.lblWeaponDamageLabel.AutoSize = true;
			this.lblWeaponDamageLabel.Location = new System.Drawing.Point(258, 45);
			this.lblWeaponDamageLabel.Name = "lblWeaponDamageLabel";
			this.lblWeaponDamageLabel.Size = new System.Drawing.Size(50, 13);
			this.lblWeaponDamageLabel.TabIndex = 2;
			this.lblWeaponDamageLabel.Tag = "Label_Damage";
			this.lblWeaponDamageLabel.Text = "Damage:";
			// 
			// txtSearch
			// 
			this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.txtSearch.Location = new System.Drawing.Point(332, 6);
			this.txtSearch.Name = "txtSearch";
			this.txtSearch.Size = new System.Drawing.Size(174, 20);
			this.txtSearch.TabIndex = 1;
			this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
			this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
			this.txtSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyUp);
			// 
			// lblSearchLabel
			// 
			this.lblSearchLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblSearchLabel.AutoSize = true;
			this.lblSearchLabel.Location = new System.Drawing.Point(282, 9);
			this.lblSearchLabel.Name = "lblSearchLabel";
			this.lblSearchLabel.Size = new System.Drawing.Size(44, 13);
			this.lblSearchLabel.TabIndex = 0;
			this.lblSearchLabel.Tag = "Label_Search";
			this.lblSearchLabel.Text = "&Search:";
			// 
			// cmdOKAdd
			// 
			this.cmdOKAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdOKAdd.Location = new System.Drawing.Point(489, 408);
			this.cmdOKAdd.Name = "cmdOKAdd";
			this.cmdOKAdd.Size = new System.Drawing.Size(75, 23);
			this.cmdOKAdd.TabIndex = 32;
			this.cmdOKAdd.Tag = "String_AddMore";
			this.cmdOKAdd.Text = "&Add && More";
			this.cmdOKAdd.UseVisualStyleBackColor = true;
			this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(257, 233);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(197, 13);
			this.label2.TabIndex = 24;
			this.label2.Tag = "Label_SelectWeapon_IncludedItems";
			this.label2.Text = "Included Accessories and Modifications:";
			// 
			// lblIncludedAccessories
			// 
			this.lblIncludedAccessories.AutoSize = true;
			this.lblIncludedAccessories.Location = new System.Drawing.Point(275, 256);
			this.lblIncludedAccessories.Name = "lblIncludedAccessories";
			this.lblIncludedAccessories.Size = new System.Drawing.Size(39, 13);
			this.lblIncludedAccessories.TabIndex = 25;
			this.lblIncludedAccessories.Text = "[None]";
			// 
			// lblSource
			// 
			this.lblSource.AutoSize = true;
			this.lblSource.Location = new System.Drawing.Point(306, 210);
			this.lblSource.Name = "lblSource";
			this.lblSource.Size = new System.Drawing.Size(47, 13);
			this.lblSource.TabIndex = 27;
			this.lblSource.Text = "[Source]";
			this.lblSource.Click += new System.EventHandler(this.lblSource_Click);
			// 
			// lblSourceLabel
			// 
			this.lblSourceLabel.AutoSize = true;
			this.lblSourceLabel.Location = new System.Drawing.Point(255, 210);
			this.lblSourceLabel.Name = "lblSourceLabel";
			this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
			this.lblSourceLabel.TabIndex = 26;
			this.lblSourceLabel.Tag = "Label_Source";
			this.lblSourceLabel.Text = "Source:";
			// 
			// chkFreeItem
			// 
			this.chkFreeItem.AutoSize = true;
			this.chkFreeItem.Location = new System.Drawing.Point(412, 165);
			this.chkFreeItem.Name = "chkFreeItem";
			this.chkFreeItem.Size = new System.Drawing.Size(50, 17);
			this.chkFreeItem.TabIndex = 20;
			this.chkFreeItem.Tag = "Checkbox_Free";
			this.chkFreeItem.Text = "Free!";
			this.chkFreeItem.UseVisualStyleBackColor = true;
			this.chkFreeItem.Visible = true;
			this.chkFreeItem.CheckedChanged += new System.EventHandler(this.chkFreeItem_CheckedChanged);
			// 
			// nudMarkup
			// 
			this.nudMarkup.Location = new System.Drawing.Point(318, 164);
			this.nudMarkup.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.nudMarkup.Minimum = new decimal(new int[] {
            99,
            0,
            0,
            -2147483648});
			this.nudMarkup.Name = "nudMarkup";
			this.nudMarkup.Size = new System.Drawing.Size(56, 20);
			this.nudMarkup.TabIndex = 22;
			this.nudMarkup.ValueChanged += new System.EventHandler(this.nudMarkup_ValueChanged);
			// 
			// lblMarkupLabel
			// 
			this.lblMarkupLabel.AutoSize = true;
			this.lblMarkupLabel.Location = new System.Drawing.Point(258, 166);
			this.lblMarkupLabel.Name = "lblMarkupLabel";
			this.lblMarkupLabel.Size = new System.Drawing.Size(46, 13);
			this.lblMarkupLabel.TabIndex = 21;
			this.lblMarkupLabel.Tag = "Label_SelectGear_Markup";
			this.lblMarkupLabel.Text = "Markup:";
			// 
			// lblMarkupPercentLabel
			// 
			this.lblMarkupPercentLabel.AutoSize = true;
			this.lblMarkupPercentLabel.Location = new System.Drawing.Point(373, 166);
			this.lblMarkupPercentLabel.Name = "lblMarkupPercentLabel";
			this.lblMarkupPercentLabel.Size = new System.Drawing.Size(15, 13);
			this.lblMarkupPercentLabel.TabIndex = 23;
			this.lblMarkupPercentLabel.Text = "%";
			// 
			// lblTest
			// 
			this.lblTest.AutoSize = true;
			this.lblTest.Location = new System.Drawing.Point(433, 114);
			this.lblTest.Name = "lblTest";
			this.lblTest.Size = new System.Drawing.Size(19, 13);
			this.lblTest.TabIndex = 17;
			this.lblTest.Text = "[0]";
			// 
			// lblTestLabel
			// 
			this.lblTestLabel.AutoSize = true;
			this.lblTestLabel.Location = new System.Drawing.Point(372, 114);
			this.lblTestLabel.Name = "lblTestLabel";
			this.lblTestLabel.Size = new System.Drawing.Size(31, 13);
			this.lblTestLabel.TabIndex = 16;
			this.lblTestLabel.Tag = "Label_Test";
			this.lblTestLabel.Text = "Test:";
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
			// lblWeaponAccuracy
			// 
			this.lblWeaponAccuracy.AutoSize = true;
			this.lblWeaponAccuracy.Location = new System.Drawing.Point(433, 137);
			this.lblWeaponAccuracy.Name = "lblWeaponAccuracy";
			this.lblWeaponAccuracy.Size = new System.Drawing.Size(58, 13);
			this.lblWeaponAccuracy.TabIndex = 35;
			this.lblWeaponAccuracy.Text = "[Accuracy]";
			// 
			// lblWeaponAccuracyLabel
			// 
			this.lblWeaponAccuracyLabel.AutoSize = true;
			this.lblWeaponAccuracyLabel.Location = new System.Drawing.Point(372, 137);
			this.lblWeaponAccuracyLabel.Name = "lblWeaponAccuracyLabel";
			this.lblWeaponAccuracyLabel.Size = new System.Drawing.Size(55, 13);
			this.lblWeaponAccuracyLabel.TabIndex = 34;
			this.lblWeaponAccuracyLabel.Tag = "Label_Accuracy";
			this.lblWeaponAccuracyLabel.Text = "Accuracy:";
			// 
			// dgvWeapons
			// 
			this.dgvWeapons.AllowUserToAddRows = false;
			this.dgvWeapons.AllowUserToDeleteRows = false;
			dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.dgvWeapons.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
			this.dgvWeapons.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dgvWeapons.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvWeapons.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.WeaponName,
            this.Dice,
            this.Accuracy,
            this.Damage,
            this.AP,
            this.RC,
            this.Ammo,
            this.Mode,
            this.Reach,
            this.Accessories,
            this.Avail,
            this.Source,
            this.Cost});
			this.dgvWeapons.Location = new System.Drawing.Point(12, 33);
			this.dgvWeapons.MultiSelect = false;
			this.dgvWeapons.Name = "dgvWeapons";
			this.dgvWeapons.ReadOnly = true;
			this.dgvWeapons.RowHeadersVisible = false;
			this.dgvWeapons.RowTemplate.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
			this.dgvWeapons.RowTemplate.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dgvWeapons.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvWeapons.Size = new System.Drawing.Size(552, 369);
			this.dgvWeapons.TabIndex = 36;
			this.dgvWeapons.Visible = false;
			this.dgvWeapons.DoubleClick += new System.EventHandler(this.dgvWeapons_DoubleClick);
			// 
			// WeaponName
			// 
			this.WeaponName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.WeaponName.DataPropertyName = "WeaponName";
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.WeaponName.DefaultCellStyle = dataGridViewCellStyle2;
			this.WeaponName.Frozen = true;
			this.WeaponName.HeaderText = "Name";
			this.WeaponName.Name = "WeaponName";
			this.WeaponName.ReadOnly = true;
			this.WeaponName.Width = 60;
			// 
			// Dice
			// 
			this.Dice.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Dice.DataPropertyName = "Dice";
			this.Dice.FillWeight = 30F;
			this.Dice.HeaderText = "Dice";
			this.Dice.Name = "Dice";
			this.Dice.ReadOnly = true;
			this.Dice.Width = 54;
			// 
			// Accuracy
			// 
			this.Accuracy.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Accuracy.DataPropertyName = "Accuracy";
			this.Accuracy.FillWeight = 50F;
			this.Accuracy.HeaderText = "Accuracy";
			this.Accuracy.Name = "Accuracy";
			this.Accuracy.ReadOnly = true;
			this.Accuracy.Width = 77;
			// 
			// Damage
			// 
			this.Damage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Damage.DataPropertyName = "Damage";
			this.Damage.FillWeight = 50F;
			this.Damage.HeaderText = "Damage";
			this.Damage.Name = "Damage";
			this.Damage.ReadOnly = true;
			this.Damage.Width = 72;
			// 
			// AP
			// 
			this.AP.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.AP.DataPropertyName = "AP";
			this.AP.FillWeight = 30F;
			this.AP.HeaderText = "AP";
			this.AP.Name = "AP";
			this.AP.ReadOnly = true;
			this.AP.Width = 46;
			// 
			// RC
			// 
			this.RC.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.RC.DataPropertyName = "RC";
			this.RC.FillWeight = 30F;
			this.RC.HeaderText = "RC";
			this.RC.Name = "RC";
			this.RC.ReadOnly = true;
			this.RC.Width = 47;
			// 
			// Ammo
			// 
			this.Ammo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Ammo.DataPropertyName = "Ammo";
			this.Ammo.FillWeight = 60F;
			this.Ammo.HeaderText = "Ammo";
			this.Ammo.Name = "Ammo";
			this.Ammo.ReadOnly = true;
			this.Ammo.Width = 61;
			// 
			// Mode
			// 
			this.Mode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Mode.DataPropertyName = "Mode";
			this.Mode.FillWeight = 60F;
			this.Mode.HeaderText = "Mode";
			this.Mode.Name = "Mode";
			this.Mode.ReadOnly = true;
			this.Mode.Width = 59;
			// 
			// Reach
			// 
			this.Reach.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Reach.DataPropertyName = "Reach";
			this.Reach.FillWeight = 40F;
			this.Reach.HeaderText = "Reach";
			this.Reach.Name = "Reach";
			this.Reach.ReadOnly = true;
			this.Reach.Width = 64;
			// 
			// Accessories
			// 
			this.Accessories.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Accessories.DataPropertyName = "Accessories";
			dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.Accessories.DefaultCellStyle = dataGridViewCellStyle3;
			this.Accessories.HeaderText = "Accessories";
			this.Accessories.Name = "Accessories";
			this.Accessories.ReadOnly = true;
			this.Accessories.Width = 89;
			// 
			// Avail
			// 
			this.Avail.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Avail.DataPropertyName = "Avail";
			this.Avail.FillWeight = 30F;
			this.Avail.HeaderText = "Avail";
			this.Avail.Name = "Avail";
			this.Avail.ReadOnly = true;
			this.Avail.Width = 55;
			// 
			// Source
			// 
			this.Source.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Source.DataPropertyName = "Source";
			this.Source.HeaderText = "Source";
			this.Source.Name = "Source";
			this.Source.ReadOnly = true;
			this.Source.Width = 66;
			// 
			// Cost
			// 
			this.Cost.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Cost.DataPropertyName = "Cost";
			dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopRight;
			dataGridViewCellStyle4.Format = "#,###,##0¥";
			dataGridViewCellStyle4.NullValue = null;
			this.Cost.DefaultCellStyle = dataGridViewCellStyle4;
			this.Cost.FillWeight = 60F;
			this.Cost.HeaderText = "Cost";
			this.Cost.Name = "Cost";
			this.Cost.ReadOnly = true;
			this.Cost.Width = 53;
			// 
			// chkBrowse
			// 
			this.chkBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.chkBrowse.Appearance = System.Windows.Forms.Appearance.Button;
			this.chkBrowse.AutoSize = true;
			this.chkBrowse.Location = new System.Drawing.Point(512, 4);
			this.chkBrowse.Name = "chkBrowse";
			this.chkBrowse.Size = new System.Drawing.Size(52, 23);
			this.chkBrowse.TabIndex = 37;
			this.chkBrowse.Text = "Browse";
			this.chkBrowse.UseVisualStyleBackColor = true;
			this.chkBrowse.CheckedChanged += new System.EventHandler(this.chkBrowse_CheckedChanged);
			// 
			// tmrSearch
			// 
			this.tmrSearch.Interval = 250;
			this.tmrSearch.Tick += new System.EventHandler(this.tmrSearch_Tick);
			// 
			// chkBlackMarketDiscount
			// 
			this.chkBlackMarketDiscount.AutoSize = true;
			this.chkBlackMarketDiscount.Location = new System.Drawing.Point(258, 188);
			this.chkBlackMarketDiscount.Name = "chkBlackMarketDiscount";
			this.chkBlackMarketDiscount.Size = new System.Drawing.Size(163, 17);
			this.chkBlackMarketDiscount.TabIndex = 39;
			this.chkBlackMarketDiscount.Tag = "Checkbox_BlackMarketDiscount";
			this.chkBlackMarketDiscount.Text = "Black Market Discount (10%)";
			this.chkBlackMarketDiscount.UseVisualStyleBackColor = true;
			this.chkBlackMarketDiscount.Visible = false;
			this.chkBlackMarketDiscount.CheckedChanged += new System.EventHandler(this.chkBlackMarketDiscount_CheckedChanged);
			// 
			// frmSelectWeapon
			// 
			this.AcceptButton = this.cmdOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cmdCancel;
			this.ClientSize = new System.Drawing.Size(576, 472);
			this.Controls.Add(this.chkBlackMarketDiscount);
			this.Controls.Add(this.chkBrowse);
			this.Controls.Add(this.lblWeaponAccuracy);
			this.Controls.Add(this.lblWeaponAccuracyLabel);
			this.Controls.Add(this.lblTest);
			this.Controls.Add(this.lblTestLabel);
			this.Controls.Add(this.nudMarkup);
			this.Controls.Add(this.lblMarkupLabel);
			this.Controls.Add(this.lblMarkupPercentLabel);
			this.Controls.Add(this.chkFreeItem);
			this.Controls.Add(this.lblSource);
			this.Controls.Add(this.lblSourceLabel);
			this.Controls.Add(this.lblIncludedAccessories);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.cmdOKAdd);
			this.Controls.Add(this.txtSearch);
			this.Controls.Add(this.lblSearchLabel);
			this.Controls.Add(this.lblWeaponAmmo);
			this.Controls.Add(this.lblWeaponAmmoLabel);
			this.Controls.Add(this.lblWeaponMode);
			this.Controls.Add(this.lblWeaponModeLabel);
			this.Controls.Add(this.lblWeaponReach);
			this.Controls.Add(this.lblWeaponReachLabel);
			this.Controls.Add(this.lblWeaponAP);
			this.Controls.Add(this.lblWeaponAPLabel);
			this.Controls.Add(this.lblWeaponCost);
			this.Controls.Add(this.lblWeaponCostLabel);
			this.Controls.Add(this.lblWeaponAvail);
			this.Controls.Add(this.lblWeaponAvailLabel);
			this.Controls.Add(this.lblWeaponRC);
			this.Controls.Add(this.lblWeaponRCLabel);
			this.Controls.Add(this.lblWeaponDamage);
			this.Controls.Add(this.lblWeaponDamageLabel);
			this.Controls.Add(this.cmdCancel);
			this.Controls.Add(this.cmdOK);
			this.Controls.Add(this.lstWeapon);
			this.Controls.Add(this.lblCategory);
			this.Controls.Add(this.cboCategory);
			this.Controls.Add(this.dgvWeapons);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmSelectWeapon";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Tag = "Title_SelectWeapon";
			this.Text = "Select Weapon";
			this.Load += new System.EventHandler(this.frmSelectWeapon_Load);
			((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvWeapons)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.ComboBox cboCategory;
		private System.Windows.Forms.Label lblCategory;
		private System.Windows.Forms.ListBox lstWeapon;
		private System.Windows.Forms.Button cmdOK;
		private System.Windows.Forms.Button cmdCancel;
		private System.Windows.Forms.Label lblWeaponAmmo;
		private System.Windows.Forms.Label lblWeaponAmmoLabel;
		private System.Windows.Forms.Label lblWeaponMode;
		private System.Windows.Forms.Label lblWeaponModeLabel;
		private System.Windows.Forms.Label lblWeaponReach;
		private System.Windows.Forms.Label lblWeaponReachLabel;
		private System.Windows.Forms.Label lblWeaponAP;
		private System.Windows.Forms.Label lblWeaponAPLabel;
		private System.Windows.Forms.Label lblWeaponCost;
		private System.Windows.Forms.Label lblWeaponCostLabel;
		private System.Windows.Forms.Label lblWeaponAvail;
		private System.Windows.Forms.Label lblWeaponAvailLabel;
		private System.Windows.Forms.Label lblWeaponRC;
		private System.Windows.Forms.Label lblWeaponRCLabel;
		private System.Windows.Forms.Label lblWeaponDamage;
		private System.Windows.Forms.Label lblWeaponDamageLabel;
		private System.Windows.Forms.TextBox txtSearch;
		private System.Windows.Forms.Label lblSearchLabel;
		private System.Windows.Forms.Button cmdOKAdd;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label lblIncludedAccessories;
		private System.Windows.Forms.Label lblSource;
		private System.Windows.Forms.Label lblSourceLabel;
		private System.Windows.Forms.CheckBox chkFreeItem;
		private System.Windows.Forms.NumericUpDown nudMarkup;
		private System.Windows.Forms.Label lblMarkupLabel;
		private System.Windows.Forms.Label lblMarkupPercentLabel;
		private System.Windows.Forms.Label lblTest;
		private System.Windows.Forms.Label lblTestLabel;
		private System.Windows.Forms.ToolTip tipTooltip;
        private System.Windows.Forms.Label lblWeaponAccuracy;
        private System.Windows.Forms.Label lblWeaponAccuracyLabel;
        private System.Windows.Forms.DataGridView dgvWeapons;
        private System.Windows.Forms.CheckBox chkBrowse;
        private System.Windows.Forms.Timer tmrSearch;
        private System.Windows.Forms.DataGridViewTextBoxColumn WeaponName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Dice;
        private System.Windows.Forms.DataGridViewTextBoxColumn Accuracy;
        private System.Windows.Forms.DataGridViewTextBoxColumn Damage;
        private System.Windows.Forms.DataGridViewTextBoxColumn AP;
        private System.Windows.Forms.DataGridViewTextBoxColumn RC;
        private System.Windows.Forms.DataGridViewTextBoxColumn Ammo;
        private System.Windows.Forms.DataGridViewTextBoxColumn Mode;
        private System.Windows.Forms.DataGridViewTextBoxColumn Reach;
        private System.Windows.Forms.DataGridViewTextBoxColumn Accessories;
        private System.Windows.Forms.DataGridViewTextBoxColumn Avail;
        private System.Windows.Forms.DataGridViewTextBoxColumn Source;
        private System.Windows.Forms.DataGridViewTextBoxColumn Cost;
		private System.Windows.Forms.CheckBox chkBlackMarketDiscount;
	}
}