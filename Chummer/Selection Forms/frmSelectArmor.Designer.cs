namespace Chummer
{
	partial class frmSelectArmor
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
			this.lstArmor = new System.Windows.Forms.ListBox();
			this.cmdOK = new System.Windows.Forms.Button();
			this.cmdCancel = new System.Windows.Forms.Button();
			this.lblArmorLabel = new System.Windows.Forms.Label();
			this.lblArmor = new System.Windows.Forms.Label();
			this.lblCost = new System.Windows.Forms.Label();
			this.lblCostLabel = new System.Windows.Forms.Label();
			this.lblAvail = new System.Windows.Forms.Label();
			this.lblAvailLabel = new System.Windows.Forms.Label();
			this.lblCategory = new System.Windows.Forms.Label();
			this.cboCategory = new System.Windows.Forms.ComboBox();
			this.cmdOKAdd = new System.Windows.Forms.Button();
			this.lblSourceLabel = new System.Windows.Forms.Label();
			this.lblSource = new System.Windows.Forms.Label();
			this.txtSearch = new System.Windows.Forms.TextBox();
			this.lblSearchLabel = new System.Windows.Forms.Label();
			this.chkFreeItem = new System.Windows.Forms.CheckBox();
			this.lblCapacity = new System.Windows.Forms.Label();
			this.lblCapacityLabel = new System.Windows.Forms.Label();
			this.nudMarkup = new System.Windows.Forms.NumericUpDown();
			this.lblMarkupLabel = new System.Windows.Forms.Label();
			this.lblMarkupPercentLabel = new System.Windows.Forms.Label();
			this.lblTest = new System.Windows.Forms.Label();
			this.lblTestLabel = new System.Windows.Forms.Label();
			this.tipTooltip = new System.Windows.Forms.ToolTip(this.components);
			this.lblArmorValue = new System.Windows.Forms.Label();
			this.lblArmorValueLabel = new System.Windows.Forms.Label();
			this.dgvArmor = new System.Windows.Forms.DataGridView();
			this.ArmorName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Armor = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Capacity = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Special = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Avail = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Source = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Cost = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.chkBrowse = new System.Windows.Forms.CheckBox();
			this.tmrSearch = new System.Windows.Forms.Timer(this.components);
			this.nudRating = new System.Windows.Forms.NumericUpDown();
			this.lblRatingLabel = new System.Windows.Forms.Label();
			this.chkBlackMarketDiscount = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvArmor)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudRating)).BeginInit();
			this.SuspendLayout();
			// 
			// lstArmor
			// 
			this.lstArmor.FormattingEnabled = true;
			this.lstArmor.Location = new System.Drawing.Point(12, 38);
			this.lstArmor.Name = "lstArmor";
			this.lstArmor.Size = new System.Drawing.Size(306, 329);
			this.lstArmor.TabIndex = 22;
			this.lstArmor.SelectedIndexChanged += new System.EventHandler(this.lstArmor_SelectedIndexChanged);
			this.lstArmor.DoubleClick += new System.EventHandler(this.lstArmor_DoubleClick);
			// 
			// cmdOK
			// 
			this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdOK.Location = new System.Drawing.Point(497, 343);
			this.cmdOK.Name = "cmdOK";
			this.cmdOK.Size = new System.Drawing.Size(75, 23);
			this.cmdOK.TabIndex = 23;
			this.cmdOK.Tag = "String_OK";
			this.cmdOK.Text = "OK";
			this.cmdOK.UseVisualStyleBackColor = true;
			this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
			// 
			// cmdCancel
			// 
			this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCancel.Location = new System.Drawing.Point(416, 342);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.Size = new System.Drawing.Size(75, 23);
			this.cmdCancel.TabIndex = 25;
			this.cmdCancel.Tag = "String_Cancel";
			this.cmdCancel.Text = "Cancel";
			this.cmdCancel.UseVisualStyleBackColor = true;
			this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
			// 
			// lblArmorLabel
			// 
			this.lblArmorLabel.AutoSize = true;
			this.lblArmorLabel.Location = new System.Drawing.Point(324, 37);
			this.lblArmorLabel.Name = "lblArmorLabel";
			this.lblArmorLabel.Size = new System.Drawing.Size(38, 13);
			this.lblArmorLabel.TabIndex = 2;
			this.lblArmorLabel.Tag = "Label_ArmorShort";
			this.lblArmorLabel.Text = "Name:";
			// 
			// lblArmor
			// 
			this.lblArmor.AutoSize = true;
			this.lblArmor.Location = new System.Drawing.Point(381, 37);
			this.lblArmor.Name = "lblArmor";
			this.lblArmor.Size = new System.Drawing.Size(20, 13);
			this.lblArmor.TabIndex = 3;
			this.lblArmor.Text = "[A]";
			// 
			// lblCost
			// 
			this.lblCost.AutoSize = true;
			this.lblCost.Location = new System.Drawing.Point(381, 129);
			this.lblCost.Name = "lblCost";
			this.lblCost.Size = new System.Drawing.Size(34, 13);
			this.lblCost.TabIndex = 13;
			this.lblCost.Text = "[Cost]";
			// 
			// lblCostLabel
			// 
			this.lblCostLabel.AutoSize = true;
			this.lblCostLabel.Location = new System.Drawing.Point(324, 129);
			this.lblCostLabel.Name = "lblCostLabel";
			this.lblCostLabel.Size = new System.Drawing.Size(31, 13);
			this.lblCostLabel.TabIndex = 12;
			this.lblCostLabel.Tag = "Label_Cost";
			this.lblCostLabel.Text = "Cost:";
			// 
			// lblAvail
			// 
			this.lblAvail.AutoSize = true;
			this.lblAvail.Location = new System.Drawing.Point(381, 106);
			this.lblAvail.Name = "lblAvail";
			this.lblAvail.Size = new System.Drawing.Size(36, 13);
			this.lblAvail.TabIndex = 9;
			this.lblAvail.Text = "[Avail]";
			// 
			// lblAvailLabel
			// 
			this.lblAvailLabel.AutoSize = true;
			this.lblAvailLabel.Location = new System.Drawing.Point(324, 106);
			this.lblAvailLabel.Name = "lblAvailLabel";
			this.lblAvailLabel.Size = new System.Drawing.Size(33, 13);
			this.lblAvailLabel.TabIndex = 8;
			this.lblAvailLabel.Tag = "Label_Avail";
			this.lblAvailLabel.Text = "Avail:";
			// 
			// lblCategory
			// 
			this.lblCategory.AutoSize = true;
			this.lblCategory.Location = new System.Drawing.Point(9, 9);
			this.lblCategory.Name = "lblCategory";
			this.lblCategory.Size = new System.Drawing.Size(52, 13);
			this.lblCategory.TabIndex = 20;
			this.lblCategory.Tag = "Label_Category";
			this.lblCategory.Text = "Category:";
			// 
			// cboCategory
			// 
			this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboCategory.FormattingEnabled = true;
			this.cboCategory.Location = new System.Drawing.Point(67, 6);
			this.cboCategory.Name = "cboCategory";
			this.cboCategory.Size = new System.Drawing.Size(205, 21);
			this.cboCategory.TabIndex = 21;
			this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
			// 
			// cmdOKAdd
			// 
			this.cmdOKAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdOKAdd.Location = new System.Drawing.Point(497, 314);
			this.cmdOKAdd.Name = "cmdOKAdd";
			this.cmdOKAdd.Size = new System.Drawing.Size(75, 23);
			this.cmdOKAdd.TabIndex = 24;
			this.cmdOKAdd.Tag = "String_AddMore";
			this.cmdOKAdd.Text = "&Add && More";
			this.cmdOKAdd.UseVisualStyleBackColor = true;
			this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
			// 
			// lblSourceLabel
			// 
			this.lblSourceLabel.AutoSize = true;
			this.lblSourceLabel.Location = new System.Drawing.Point(324, 213);
			this.lblSourceLabel.Name = "lblSourceLabel";
			this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
			this.lblSourceLabel.TabIndex = 18;
			this.lblSourceLabel.Tag = "Label_Source";
			this.lblSourceLabel.Text = "Source:";
			// 
			// lblSource
			// 
			this.lblSource.AutoSize = true;
			this.lblSource.Location = new System.Drawing.Point(381, 213);
			this.lblSource.Name = "lblSource";
			this.lblSource.Size = new System.Drawing.Size(47, 13);
			this.lblSource.TabIndex = 19;
			this.lblSource.Text = "[Source]";
			this.lblSource.Click += new System.EventHandler(this.lblSource_Click);
			// 
			// txtSearch
			// 
			this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtSearch.Location = new System.Drawing.Point(338, 6);
			this.txtSearch.Name = "txtSearch";
			this.txtSearch.Size = new System.Drawing.Size(176, 20);
			this.txtSearch.TabIndex = 1;
			this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
			this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
			this.txtSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyUp);
			// 
			// lblSearchLabel
			// 
			this.lblSearchLabel.AutoSize = true;
			this.lblSearchLabel.Location = new System.Drawing.Point(288, 9);
			this.lblSearchLabel.Name = "lblSearchLabel";
			this.lblSearchLabel.Size = new System.Drawing.Size(44, 13);
			this.lblSearchLabel.TabIndex = 0;
			this.lblSearchLabel.Tag = "Label_Search";
			this.lblSearchLabel.Text = "&Search:";
			// 
			// chkFreeItem
			// 
			this.chkFreeItem.AutoSize = true;
			this.chkFreeItem.Location = new System.Drawing.Point(327, 155);
			this.chkFreeItem.Name = "chkFreeItem";
			this.chkFreeItem.Size = new System.Drawing.Size(50, 17);
			this.chkFreeItem.TabIndex = 14;
			this.chkFreeItem.Tag = "Checkbox_Free";
			this.chkFreeItem.Text = "Free!";
			this.chkFreeItem.UseVisualStyleBackColor = true;
			this.chkFreeItem.Visible = true;
			this.chkFreeItem.CheckedChanged += new System.EventHandler(this.chkFreeItem_CheckedChanged);
			// 
			// lblCapacity
			// 
			this.lblCapacity.AutoSize = true;
			this.lblCapacity.Location = new System.Drawing.Point(381, 83);
			this.lblCapacity.Name = "lblCapacity";
			this.lblCapacity.Size = new System.Drawing.Size(54, 13);
			this.lblCapacity.TabIndex = 7;
			this.lblCapacity.Text = "[Capacity]";
			// 
			// lblCapacityLabel
			// 
			this.lblCapacityLabel.AutoSize = true;
			this.lblCapacityLabel.Location = new System.Drawing.Point(324, 83);
			this.lblCapacityLabel.Name = "lblCapacityLabel";
			this.lblCapacityLabel.Size = new System.Drawing.Size(51, 13);
			this.lblCapacityLabel.TabIndex = 6;
			this.lblCapacityLabel.Tag = "Label_Capacity";
			this.lblCapacityLabel.Text = "Capacity:";
			// 
			// nudMarkup
			// 
			this.nudMarkup.Location = new System.Drawing.Point(384, 178);
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
			this.nudMarkup.TabIndex = 16;
			this.nudMarkup.ValueChanged += new System.EventHandler(this.nudMarkup_ValueChanged);
			// 
			// lblMarkupLabel
			// 
			this.lblMarkupLabel.AutoSize = true;
			this.lblMarkupLabel.Location = new System.Drawing.Point(324, 180);
			this.lblMarkupLabel.Name = "lblMarkupLabel";
			this.lblMarkupLabel.Size = new System.Drawing.Size(46, 13);
			this.lblMarkupLabel.TabIndex = 15;
			this.lblMarkupLabel.Tag = "Label_SelectGear_Markup";
			this.lblMarkupLabel.Text = "Markup:";
			// 
			// lblMarkupPercentLabel
			// 
			this.lblMarkupPercentLabel.AutoSize = true;
			this.lblMarkupPercentLabel.Location = new System.Drawing.Point(439, 180);
			this.lblMarkupPercentLabel.Name = "lblMarkupPercentLabel";
			this.lblMarkupPercentLabel.Size = new System.Drawing.Size(15, 13);
			this.lblMarkupPercentLabel.TabIndex = 17;
			this.lblMarkupPercentLabel.Text = "%";
			// 
			// lblTest
			// 
			this.lblTest.AutoSize = true;
			this.lblTest.Location = new System.Drawing.Point(490, 106);
			this.lblTest.Name = "lblTest";
			this.lblTest.Size = new System.Drawing.Size(19, 13);
			this.lblTest.TabIndex = 11;
			this.lblTest.Text = "[0]";
			// 
			// lblTestLabel
			// 
			this.lblTestLabel.AutoSize = true;
			this.lblTestLabel.Location = new System.Drawing.Point(439, 106);
			this.lblTestLabel.Name = "lblTestLabel";
			this.lblTestLabel.Size = new System.Drawing.Size(31, 13);
			this.lblTestLabel.TabIndex = 10;
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
			// lblArmorValue
			// 
			this.lblArmorValue.AutoSize = true;
			this.lblArmorValue.Location = new System.Drawing.Point(381, 60);
			this.lblArmorValue.Name = "lblArmorValue";
			this.lblArmorValue.Size = new System.Drawing.Size(20, 13);
			this.lblArmorValue.TabIndex = 27;
			this.lblArmorValue.Text = "[A]";
			// 
			// lblArmorValueLabel
			// 
			this.lblArmorValueLabel.AutoSize = true;
			this.lblArmorValueLabel.Location = new System.Drawing.Point(324, 60);
			this.lblArmorValueLabel.Name = "lblArmorValueLabel";
			this.lblArmorValueLabel.Size = new System.Drawing.Size(37, 13);
			this.lblArmorValueLabel.TabIndex = 26;
			this.lblArmorValueLabel.Tag = "Label_ArmorValueShort";
			this.lblArmorValueLabel.Text = "Armor:";
			// 
			// dgvArmor
			// 
			this.dgvArmor.AllowUserToAddRows = false;
			this.dgvArmor.AllowUserToDeleteRows = false;
			dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.dgvArmor.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
			this.dgvArmor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dgvArmor.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvArmor.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ArmorName,
            this.Armor,
            this.Capacity,
            this.Special,
            this.Avail,
            this.Source,
            this.Cost});
			this.dgvArmor.Location = new System.Drawing.Point(12, 33);
			this.dgvArmor.MultiSelect = false;
			this.dgvArmor.Name = "dgvArmor";
			this.dgvArmor.ReadOnly = true;
			this.dgvArmor.RowHeadersVisible = false;
			this.dgvArmor.RowTemplate.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
			this.dgvArmor.RowTemplate.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dgvArmor.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvArmor.Size = new System.Drawing.Size(560, 275);
			this.dgvArmor.TabIndex = 37;
			this.dgvArmor.Visible = false;
			this.dgvArmor.SortCompare += new System.Windows.Forms.DataGridViewSortCompareEventHandler(this.dgvArmor_SortCompare);
			this.dgvArmor.DoubleClick += new System.EventHandler(this.dgvArmor_DoubleClick);
			// 
			// ArmorName
			// 
			this.ArmorName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.ArmorName.DataPropertyName = "ArmorName";
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.ArmorName.DefaultCellStyle = dataGridViewCellStyle2;
			this.ArmorName.HeaderText = "Name";
			this.ArmorName.Name = "ArmorName";
			this.ArmorName.ReadOnly = true;
			this.ArmorName.Width = 60;
			// 
			// Armor
			// 
			this.Armor.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Armor.DataPropertyName = "Armor";
			this.Armor.FillWeight = 50F;
			this.Armor.HeaderText = "Armor";
			this.Armor.Name = "Armor";
			this.Armor.ReadOnly = true;
			this.Armor.Width = 59;
			// 
			// Capacity
			// 
			this.Capacity.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Capacity.DataPropertyName = "Capacity";
			this.Capacity.FillWeight = 50F;
			this.Capacity.HeaderText = "Capacity";
			this.Capacity.Name = "Capacity";
			this.Capacity.ReadOnly = true;
			this.Capacity.Width = 73;
			// 
			// Special
			// 
			this.Special.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Special.DataPropertyName = "Special";
			dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.Special.DefaultCellStyle = dataGridViewCellStyle3;
			this.Special.HeaderText = "Special";
			this.Special.Name = "Special";
			this.Special.ReadOnly = true;
			this.Special.Width = 67;
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
			this.chkBrowse.Location = new System.Drawing.Point(520, 4);
			this.chkBrowse.Name = "chkBrowse";
			this.chkBrowse.Size = new System.Drawing.Size(52, 23);
			this.chkBrowse.TabIndex = 38;
			this.chkBrowse.Text = "Browse";
			this.chkBrowse.UseVisualStyleBackColor = true;
			this.chkBrowse.CheckedChanged += new System.EventHandler(this.chkBrowse_CheckedChanged);
			// 
			// tmrSearch
			// 
			this.tmrSearch.Interval = 250;
			this.tmrSearch.Tick += new System.EventHandler(this.tmrSearch_Tick);
			// 
			// nudRating
			// 
			this.nudRating.Enabled = false;
			this.nudRating.Location = new System.Drawing.Point(493, 58);
			this.nudRating.Name = "nudRating";
			this.nudRating.Size = new System.Drawing.Size(33, 20);
			this.nudRating.TabIndex = 40;
			this.nudRating.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.nudRating.ValueChanged += new System.EventHandler(this.nudRating_ValueChanged);
			// 
			// lblRatingLabel
			// 
			this.lblRatingLabel.AutoSize = true;
			this.lblRatingLabel.Location = new System.Drawing.Point(439, 60);
			this.lblRatingLabel.Name = "lblRatingLabel";
			this.lblRatingLabel.Size = new System.Drawing.Size(41, 13);
			this.lblRatingLabel.TabIndex = 39;
			this.lblRatingLabel.Tag = "Label_Rating";
			this.lblRatingLabel.Text = "Rating:";
			// 
			// chkBlackMarketDiscount
			// 
			this.chkBlackMarketDiscount.AutoSize = true;
			this.chkBlackMarketDiscount.Location = new System.Drawing.Point(384, 155);
			this.chkBlackMarketDiscount.Name = "chkBlackMarketDiscount";
			this.chkBlackMarketDiscount.Size = new System.Drawing.Size(163, 17);
			this.chkBlackMarketDiscount.TabIndex = 41;
			this.chkBlackMarketDiscount.Tag = "Checkbox_BlackMarketDiscount";
			this.chkBlackMarketDiscount.Text = "Black Market Discount (10%)";
			this.chkBlackMarketDiscount.UseVisualStyleBackColor = true;
			this.chkBlackMarketDiscount.Visible = false;
			this.chkBlackMarketDiscount.CheckedChanged += new System.EventHandler(this.chkBlackMarketDiscount_CheckedChanged);
			// 
			// frmSelectArmor
			// 
			this.AcceptButton = this.cmdOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cmdCancel;
			this.ClientSize = new System.Drawing.Size(584, 378);
			this.Controls.Add(this.chkBlackMarketDiscount);
			this.Controls.Add(this.nudRating);
			this.Controls.Add(this.lblRatingLabel);
			this.Controls.Add(this.chkBrowse);
			this.Controls.Add(this.lblArmorValue);
			this.Controls.Add(this.lblArmorValueLabel);
			this.Controls.Add(this.lblTest);
			this.Controls.Add(this.lblTestLabel);
			this.Controls.Add(this.nudMarkup);
			this.Controls.Add(this.lblMarkupLabel);
			this.Controls.Add(this.lblMarkupPercentLabel);
			this.Controls.Add(this.lblCapacity);
			this.Controls.Add(this.lblCapacityLabel);
			this.Controls.Add(this.chkFreeItem);
			this.Controls.Add(this.txtSearch);
			this.Controls.Add(this.lblSearchLabel);
			this.Controls.Add(this.lblSource);
			this.Controls.Add(this.lblSourceLabel);
			this.Controls.Add(this.cmdOKAdd);
			this.Controls.Add(this.lblCategory);
			this.Controls.Add(this.cboCategory);
			this.Controls.Add(this.lblCost);
			this.Controls.Add(this.lblCostLabel);
			this.Controls.Add(this.lblAvail);
			this.Controls.Add(this.lblAvailLabel);
			this.Controls.Add(this.lblArmor);
			this.Controls.Add(this.lblArmorLabel);
			this.Controls.Add(this.cmdCancel);
			this.Controls.Add(this.cmdOK);
			this.Controls.Add(this.lstArmor);
			this.Controls.Add(this.dgvArmor);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmSelectArmor";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Tag = "Title_SelectArmor";
			this.Text = "Select Armor";
			this.Load += new System.EventHandler(this.frmSelectArmor_Load);
			((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvArmor)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudRating)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListBox lstArmor;
		private System.Windows.Forms.Button cmdOK;
		private System.Windows.Forms.Button cmdCancel;
		private System.Windows.Forms.Label lblArmorLabel;
		private System.Windows.Forms.Label lblArmor;
		private System.Windows.Forms.Label lblCost;
		private System.Windows.Forms.Label lblCostLabel;
		private System.Windows.Forms.Label lblAvail;
		private System.Windows.Forms.Label lblAvailLabel;
		private System.Windows.Forms.Label lblCategory;
		private System.Windows.Forms.ComboBox cboCategory;
		private System.Windows.Forms.Button cmdOKAdd;
		private System.Windows.Forms.Label lblSourceLabel;
		private System.Windows.Forms.Label lblSource;
		private System.Windows.Forms.TextBox txtSearch;
		private System.Windows.Forms.Label lblSearchLabel;
		private System.Windows.Forms.CheckBox chkFreeItem;
		private System.Windows.Forms.Label lblCapacity;
		private System.Windows.Forms.Label lblCapacityLabel;
		private System.Windows.Forms.NumericUpDown nudMarkup;
		private System.Windows.Forms.Label lblMarkupLabel;
		private System.Windows.Forms.Label lblMarkupPercentLabel;
		private System.Windows.Forms.Label lblTest;
		private System.Windows.Forms.Label lblTestLabel;
		private System.Windows.Forms.ToolTip tipTooltip;
        private System.Windows.Forms.Label lblArmorValue;
        private System.Windows.Forms.Label lblArmorValueLabel;
        private System.Windows.Forms.DataGridView dgvArmor;
        private System.Windows.Forms.CheckBox chkBrowse;
        private System.Windows.Forms.Timer tmrSearch;
        private System.Windows.Forms.DataGridViewTextBoxColumn ArmorName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Armor;
        private System.Windows.Forms.DataGridViewTextBoxColumn Capacity;
        private System.Windows.Forms.DataGridViewTextBoxColumn Special;
        private System.Windows.Forms.DataGridViewTextBoxColumn Avail;
        private System.Windows.Forms.DataGridViewTextBoxColumn Source;
        private System.Windows.Forms.DataGridViewTextBoxColumn Cost;
		private System.Windows.Forms.NumericUpDown nudRating;
		private System.Windows.Forms.Label lblRatingLabel;
		private System.Windows.Forms.CheckBox chkBlackMarketDiscount;
	}
}