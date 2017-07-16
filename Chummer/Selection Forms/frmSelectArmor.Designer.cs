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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.lblCategory = new System.Windows.Forms.Label();
            this.cboCategory = new System.Windows.Forms.ComboBox();
            this.cmdOKAdd = new System.Windows.Forms.Button();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearchLabel = new System.Windows.Forms.Label();
            this.dgvArmor = new System.Windows.Forms.DataGridView();
            this.Guid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabListDetail = new System.Windows.Forms.TabPage();
            this.chkBlackMarketDiscount = new System.Windows.Forms.CheckBox();
            this.nudRating = new System.Windows.Forms.NumericUpDown();
            this.lblRatingLabel = new System.Windows.Forms.Label();
            this.lblArmorValue = new System.Windows.Forms.Label();
            this.lblArmorValueLabel = new System.Windows.Forms.Label();
            this.lblTest = new System.Windows.Forms.Label();
            this.lblTestLabel = new System.Windows.Forms.Label();
            this.nudMarkup = new System.Windows.Forms.NumericUpDown();
            this.lblMarkupLabel = new System.Windows.Forms.Label();
            this.lblMarkupPercentLabel = new System.Windows.Forms.Label();
            this.lblCapacity = new System.Windows.Forms.Label();
            this.lblCapacityLabel = new System.Windows.Forms.Label();
            this.chkFreeItem = new System.Windows.Forms.CheckBox();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.lblCost = new System.Windows.Forms.Label();
            this.lblCostLabel = new System.Windows.Forms.Label();
            this.lblAvail = new System.Windows.Forms.Label();
            this.lblAvailLabel = new System.Windows.Forms.Label();
            this.lblArmor = new System.Windows.Forms.Label();
            this.lblArmorLabel = new System.Windows.Forms.Label();
            this.lstArmor = new System.Windows.Forms.ListBox();
            this.tabBrowse = new System.Windows.Forms.TabPage();
            this.Source = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tmrSearch = new System.Windows.Forms.Timer(this.components);
            this.Cost = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Special = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Capacity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Armor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ArmorName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tipTooltip = new TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip();
            this.Avail = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.chkHideOverAvailLimit = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvArmor)).BeginInit();
            this.tabControl.SuspendLayout();
            this.tabListDetail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRating)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).BeginInit();
            this.tabBrowse.SuspendLayout();
            this.SuspendLayout();
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
            this.cmdCancel.Location = new System.Drawing.Point(335, 343);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 25;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
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
            this.cmdOKAdd.Location = new System.Drawing.Point(416, 343);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(75, 23);
            this.cmdOKAdd.TabIndex = 24;
            this.cmdOKAdd.Tag = "String_AddMore";
            this.cmdOKAdd.Text = "&Add && More";
            this.cmdOKAdd.UseVisualStyleBackColor = true;
            this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(338, 6);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(230, 20);
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
            // dgvArmor
            // 
            this.dgvArmor.AllowUserToAddRows = false;
            this.dgvArmor.AllowUserToDeleteRows = false;
            dataGridViewCellStyle8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvArmor.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle8;
            this.dgvArmor.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvArmor.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
            this.dgvArmor.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvArmor.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Guid,
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4,
            this.dataGridViewTextBoxColumn5,
            this.dataGridViewTextBoxColumn6,
            this.dataGridViewTextBoxColumn7});
            this.dgvArmor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvArmor.Location = new System.Drawing.Point(3, 3);
            this.dgvArmor.MultiSelect = false;
            this.dgvArmor.Name = "dgvArmor";
            this.dgvArmor.ReadOnly = true;
            this.dgvArmor.RowHeadersVisible = false;
            this.dgvArmor.RowTemplate.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            this.dgvArmor.RowTemplate.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvArmor.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvArmor.Size = new System.Drawing.Size(545, 272);
            this.dgvArmor.TabIndex = 37;
            this.dgvArmor.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvArmor_CellContentClick);
            this.dgvArmor.SortCompare += new System.Windows.Forms.DataGridViewSortCompareEventHandler(this.dgvArmor_SortCompare);
            this.dgvArmor.DoubleClick += new System.EventHandler(this.dgvArmor_DoubleClick);
            // 
            // Guid
            // 
            this.Guid.DataPropertyName = "ArmorGuid";
            this.Guid.HeaderText = "Id";
            this.Guid.Name = "Guid";
            this.Guid.ReadOnly = true;
            this.Guid.Visible = false;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumn1.DataPropertyName = "ArmorName";
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewTextBoxColumn1.DefaultCellStyle = dataGridViewCellStyle9;
            this.dataGridViewTextBoxColumn1.HeaderText = "Name";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.Width = 60;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumn2.DataPropertyName = "Armor";
            this.dataGridViewTextBoxColumn2.FillWeight = 50F;
            this.dataGridViewTextBoxColumn2.HeaderText = "Armor";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            this.dataGridViewTextBoxColumn2.Width = 59;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumn3.DataPropertyName = "Capacity";
            this.dataGridViewTextBoxColumn3.FillWeight = 50F;
            this.dataGridViewTextBoxColumn3.HeaderText = "Capacity";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            this.dataGridViewTextBoxColumn3.Width = 73;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumn4.DataPropertyName = "Special";
            dataGridViewCellStyle10.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewTextBoxColumn4.DefaultCellStyle = dataGridViewCellStyle10;
            this.dataGridViewTextBoxColumn4.HeaderText = "Special";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            this.dataGridViewTextBoxColumn4.Width = 67;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumn5.DataPropertyName = "Avail";
            this.dataGridViewTextBoxColumn5.FillWeight = 30F;
            this.dataGridViewTextBoxColumn5.HeaderText = "Avail";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.ReadOnly = true;
            this.dataGridViewTextBoxColumn5.Width = 55;
            // 
            // dataGridViewTextBoxColumn6
            // 
            this.dataGridViewTextBoxColumn6.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumn6.DataPropertyName = "Source";
            this.dataGridViewTextBoxColumn6.HeaderText = "Source";
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            this.dataGridViewTextBoxColumn6.ReadOnly = true;
            this.dataGridViewTextBoxColumn6.Width = 66;
            // 
            // dataGridViewTextBoxColumn7
            // 
            this.dataGridViewTextBoxColumn7.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumn7.DataPropertyName = "Cost";
            dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopRight;
            dataGridViewCellStyle11.Format = "#,###,##0¥";
            dataGridViewCellStyle11.NullValue = null;
            this.dataGridViewTextBoxColumn7.DefaultCellStyle = dataGridViewCellStyle11;
            this.dataGridViewTextBoxColumn7.FillWeight = 60F;
            this.dataGridViewTextBoxColumn7.HeaderText = "Cost";
            this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
            this.dataGridViewTextBoxColumn7.ReadOnly = true;
            this.dataGridViewTextBoxColumn7.Width = 53;
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabListDetail);
            this.tabControl.Controls.Add(this.tabBrowse);
            this.tabControl.Location = new System.Drawing.Point(13, 33);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(559, 304);
            this.tabControl.TabIndex = 39;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tmrSearch_Tick);
            // 
            // tabListDetail
            // 
            this.tabListDetail.Controls.Add(this.chkHideOverAvailLimit);
            this.tabListDetail.Controls.Add(this.chkBlackMarketDiscount);
            this.tabListDetail.Controls.Add(this.nudRating);
            this.tabListDetail.Controls.Add(this.lblRatingLabel);
            this.tabListDetail.Controls.Add(this.lblArmorValue);
            this.tabListDetail.Controls.Add(this.lblArmorValueLabel);
            this.tabListDetail.Controls.Add(this.lblTest);
            this.tabListDetail.Controls.Add(this.lblTestLabel);
            this.tabListDetail.Controls.Add(this.nudMarkup);
            this.tabListDetail.Controls.Add(this.lblMarkupLabel);
            this.tabListDetail.Controls.Add(this.lblMarkupPercentLabel);
            this.tabListDetail.Controls.Add(this.lblCapacity);
            this.tabListDetail.Controls.Add(this.lblCapacityLabel);
            this.tabListDetail.Controls.Add(this.chkFreeItem);
            this.tabListDetail.Controls.Add(this.lblSource);
            this.tabListDetail.Controls.Add(this.lblSourceLabel);
            this.tabListDetail.Controls.Add(this.lblCost);
            this.tabListDetail.Controls.Add(this.lblCostLabel);
            this.tabListDetail.Controls.Add(this.lblAvail);
            this.tabListDetail.Controls.Add(this.lblAvailLabel);
            this.tabListDetail.Controls.Add(this.lblArmor);
            this.tabListDetail.Controls.Add(this.lblArmorLabel);
            this.tabListDetail.Controls.Add(this.lstArmor);
            this.tabListDetail.Location = new System.Drawing.Point(4, 22);
            this.tabListDetail.Name = "tabListDetail";
            this.tabListDetail.Padding = new System.Windows.Forms.Padding(3);
            this.tabListDetail.Size = new System.Drawing.Size(551, 278);
            this.tabListDetail.TabIndex = 1;
            this.tabListDetail.Tag = "Title_ListView";
            this.tabListDetail.Text = "List View";
            this.tabListDetail.UseVisualStyleBackColor = true;
            // 
            // chkBlackMarketDiscount
            // 
            this.chkBlackMarketDiscount.AutoSize = true;
            this.chkBlackMarketDiscount.Location = new System.Drawing.Point(381, 123);
            this.chkBlackMarketDiscount.Name = "chkBlackMarketDiscount";
            this.chkBlackMarketDiscount.Size = new System.Drawing.Size(163, 17);
            this.chkBlackMarketDiscount.TabIndex = 63;
            this.chkBlackMarketDiscount.Tag = "Checkbox_BlackMarketDiscount";
            this.chkBlackMarketDiscount.Text = "Black Market Discount (10%)";
            this.chkBlackMarketDiscount.UseVisualStyleBackColor = true;
            this.chkBlackMarketDiscount.Visible = false;
            this.chkBlackMarketDiscount.CheckedChanged += new System.EventHandler(this.chkBlackMarketDiscount_CheckedChanged);
            // 
            // nudRating
            // 
            this.nudRating.Enabled = false;
            this.nudRating.Location = new System.Drawing.Point(490, 26);
            this.nudRating.Name = "nudRating";
            this.nudRating.Size = new System.Drawing.Size(33, 20);
            this.nudRating.TabIndex = 62;
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
            this.lblRatingLabel.Location = new System.Drawing.Point(436, 28);
            this.lblRatingLabel.Name = "lblRatingLabel";
            this.lblRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblRatingLabel.TabIndex = 61;
            this.lblRatingLabel.Tag = "Label_Rating";
            this.lblRatingLabel.Text = "Rating:";
            // 
            // lblArmorValue
            // 
            this.lblArmorValue.AutoSize = true;
            this.lblArmorValue.Location = new System.Drawing.Point(378, 28);
            this.lblArmorValue.Name = "lblArmorValue";
            this.lblArmorValue.Size = new System.Drawing.Size(20, 13);
            this.lblArmorValue.TabIndex = 60;
            this.lblArmorValue.Text = "[A]";
            // 
            // lblArmorValueLabel
            // 
            this.lblArmorValueLabel.AutoSize = true;
            this.lblArmorValueLabel.Location = new System.Drawing.Point(321, 28);
            this.lblArmorValueLabel.Name = "lblArmorValueLabel";
            this.lblArmorValueLabel.Size = new System.Drawing.Size(37, 13);
            this.lblArmorValueLabel.TabIndex = 59;
            this.lblArmorValueLabel.Tag = "Label_ArmorValueShort";
            this.lblArmorValueLabel.Text = "Armor:";
            // 
            // lblTest
            // 
            this.lblTest.AutoSize = true;
            this.lblTest.Location = new System.Drawing.Point(487, 74);
            this.lblTest.Name = "lblTest";
            this.lblTest.Size = new System.Drawing.Size(19, 13);
            this.lblTest.TabIndex = 49;
            this.lblTest.Text = "[0]";
            // 
            // lblTestLabel
            // 
            this.lblTestLabel.AutoSize = true;
            this.lblTestLabel.Location = new System.Drawing.Point(436, 74);
            this.lblTestLabel.Name = "lblTestLabel";
            this.lblTestLabel.Size = new System.Drawing.Size(31, 13);
            this.lblTestLabel.TabIndex = 48;
            this.lblTestLabel.Tag = "Label_Test";
            this.lblTestLabel.Text = "Test:";
            // 
            // nudMarkup
            // 
            this.nudMarkup.Location = new System.Drawing.Point(381, 146);
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
            this.nudMarkup.TabIndex = 54;
            this.nudMarkup.ValueChanged += new System.EventHandler(this.nudMarkup_ValueChanged);
            // 
            // lblMarkupLabel
            // 
            this.lblMarkupLabel.AutoSize = true;
            this.lblMarkupLabel.Location = new System.Drawing.Point(321, 148);
            this.lblMarkupLabel.Name = "lblMarkupLabel";
            this.lblMarkupLabel.Size = new System.Drawing.Size(46, 13);
            this.lblMarkupLabel.TabIndex = 53;
            this.lblMarkupLabel.Tag = "Label_SelectGear_Markup";
            this.lblMarkupLabel.Text = "Markup:";
            // 
            // lblMarkupPercentLabel
            // 
            this.lblMarkupPercentLabel.AutoSize = true;
            this.lblMarkupPercentLabel.Location = new System.Drawing.Point(436, 148);
            this.lblMarkupPercentLabel.Name = "lblMarkupPercentLabel";
            this.lblMarkupPercentLabel.Size = new System.Drawing.Size(15, 13);
            this.lblMarkupPercentLabel.TabIndex = 55;
            this.lblMarkupPercentLabel.Text = "%";
            // 
            // lblCapacity
            // 
            this.lblCapacity.AutoSize = true;
            this.lblCapacity.Location = new System.Drawing.Point(378, 51);
            this.lblCapacity.Name = "lblCapacity";
            this.lblCapacity.Size = new System.Drawing.Size(54, 13);
            this.lblCapacity.TabIndex = 45;
            this.lblCapacity.Text = "[Capacity]";
            // 
            // lblCapacityLabel
            // 
            this.lblCapacityLabel.AutoSize = true;
            this.lblCapacityLabel.Location = new System.Drawing.Point(321, 51);
            this.lblCapacityLabel.Name = "lblCapacityLabel";
            this.lblCapacityLabel.Size = new System.Drawing.Size(51, 13);
            this.lblCapacityLabel.TabIndex = 44;
            this.lblCapacityLabel.Tag = "Label_Capacity";
            this.lblCapacityLabel.Text = "Capacity:";
            // 
            // chkFreeItem
            // 
            this.chkFreeItem.AutoSize = true;
            this.chkFreeItem.Location = new System.Drawing.Point(324, 123);
            this.chkFreeItem.Name = "chkFreeItem";
            this.chkFreeItem.Size = new System.Drawing.Size(50, 17);
            this.chkFreeItem.TabIndex = 52;
            this.chkFreeItem.Tag = "Checkbox_Free";
            this.chkFreeItem.Text = "Free!";
            this.chkFreeItem.UseVisualStyleBackColor = true;
            this.chkFreeItem.CheckedChanged += new System.EventHandler(this.chkFreeItem_CheckedChanged);
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.lblSource.Location = new System.Drawing.Point(378, 181);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 57;
            this.lblSource.Text = "[Source]";
            this.lblSource.Click += new System.EventHandler(this.lblSource_Click);
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(321, 181);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 56;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            // 
            // lblCost
            // 
            this.lblCost.AutoSize = true;
            this.lblCost.Location = new System.Drawing.Point(378, 97);
            this.lblCost.Name = "lblCost";
            this.lblCost.Size = new System.Drawing.Size(34, 13);
            this.lblCost.TabIndex = 51;
            this.lblCost.Text = "[Cost]";
            // 
            // lblCostLabel
            // 
            this.lblCostLabel.AutoSize = true;
            this.lblCostLabel.Location = new System.Drawing.Point(321, 97);
            this.lblCostLabel.Name = "lblCostLabel";
            this.lblCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblCostLabel.TabIndex = 50;
            this.lblCostLabel.Tag = "Label_Cost";
            this.lblCostLabel.Text = "Cost:";
            // 
            // lblAvail
            // 
            this.lblAvail.AutoSize = true;
            this.lblAvail.Location = new System.Drawing.Point(378, 74);
            this.lblAvail.Name = "lblAvail";
            this.lblAvail.Size = new System.Drawing.Size(36, 13);
            this.lblAvail.TabIndex = 47;
            this.lblAvail.Text = "[Avail]";
            // 
            // lblAvailLabel
            // 
            this.lblAvailLabel.AutoSize = true;
            this.lblAvailLabel.Location = new System.Drawing.Point(321, 74);
            this.lblAvailLabel.Name = "lblAvailLabel";
            this.lblAvailLabel.Size = new System.Drawing.Size(33, 13);
            this.lblAvailLabel.TabIndex = 46;
            this.lblAvailLabel.Tag = "Label_Avail";
            this.lblAvailLabel.Text = "Avail:";
            // 
            // lblArmor
            // 
            this.lblArmor.AutoSize = true;
            this.lblArmor.Location = new System.Drawing.Point(378, 5);
            this.lblArmor.Name = "lblArmor";
            this.lblArmor.Size = new System.Drawing.Size(20, 13);
            this.lblArmor.TabIndex = 43;
            this.lblArmor.Text = "[A]";
            // 
            // lblArmorLabel
            // 
            this.lblArmorLabel.AutoSize = true;
            this.lblArmorLabel.Location = new System.Drawing.Point(321, 5);
            this.lblArmorLabel.Name = "lblArmorLabel";
            this.lblArmorLabel.Size = new System.Drawing.Size(38, 13);
            this.lblArmorLabel.TabIndex = 42;
            this.lblArmorLabel.Tag = "Label_ArmorShort";
            this.lblArmorLabel.Text = "Name:";
            // 
            // lstArmor
            // 
            this.lstArmor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstArmor.FormattingEnabled = true;
            this.lstArmor.Location = new System.Drawing.Point(9, 6);
            this.lstArmor.Name = "lstArmor";
            this.lstArmor.Size = new System.Drawing.Size(306, 264);
            this.lstArmor.TabIndex = 58;
            this.lstArmor.SelectedIndexChanged += new System.EventHandler(this.lstArmor_SelectedIndexChanged);
            this.lstArmor.DoubleClick += new System.EventHandler(this.lstArmor_DoubleClick);
            // 
            // tabBrowse
            // 
            this.tabBrowse.Controls.Add(this.dgvArmor);
            this.tabBrowse.Location = new System.Drawing.Point(4, 22);
            this.tabBrowse.Name = "tabBrowse";
            this.tabBrowse.Padding = new System.Windows.Forms.Padding(3);
            this.tabBrowse.Size = new System.Drawing.Size(551, 278);
            this.tabBrowse.TabIndex = 0;
            this.tabBrowse.Tag = "Title_Browse";
            this.tabBrowse.Text = "Browse";
            this.tabBrowse.UseVisualStyleBackColor = true;
            // 
            // Source
            // 
            this.Source.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Source.DataPropertyName = "Source";
            this.Source.HeaderText = "Source";
            this.Source.Name = "Source";
            this.Source.ReadOnly = true;
            // 
            // tmrSearch
            // 
            this.tmrSearch.Interval = 250;
            // 
            // Cost
            // 
            this.Cost.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Cost.DataPropertyName = "Cost";
            dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopRight;
            dataGridViewCellStyle12.Format = "#,###,##0¥";
            dataGridViewCellStyle12.NullValue = null;
            this.Cost.DefaultCellStyle = dataGridViewCellStyle12;
            this.Cost.FillWeight = 60F;
            this.Cost.HeaderText = "Cost";
            this.Cost.Name = "Cost";
            this.Cost.ReadOnly = true;
            // 
            // Special
            // 
            this.Special.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Special.DataPropertyName = "Special";
            dataGridViewCellStyle13.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.Special.DefaultCellStyle = dataGridViewCellStyle13;
            this.Special.HeaderText = "Special";
            this.Special.Name = "Special";
            this.Special.ReadOnly = true;
            // 
            // Capacity
            // 
            this.Capacity.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Capacity.DataPropertyName = "Capacity";
            this.Capacity.FillWeight = 50F;
            this.Capacity.HeaderText = "Capacity";
            this.Capacity.Name = "Capacity";
            this.Capacity.ReadOnly = true;
            // 
            // Armor
            // 
            this.Armor.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Armor.DataPropertyName = "Armor";
            this.Armor.FillWeight = 50F;
            this.Armor.HeaderText = "Armor";
            this.Armor.Name = "Armor";
            this.Armor.ReadOnly = true;
            // 
            // ArmorName
            // 
            this.ArmorName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ArmorName.DataPropertyName = "ArmorName";
            dataGridViewCellStyle14.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.ArmorName.DefaultCellStyle = dataGridViewCellStyle14;
            this.ArmorName.HeaderText = "Name";
            this.ArmorName.Name = "ArmorName";
            this.ArmorName.ReadOnly = true;
            // 
            // tipTooltip
            // 
            this.tipTooltip.AllowLinksHandling = true;
            this.tipTooltip.AutoPopDelay = 10000;
            this.tipTooltip.BaseStylesheet = null;
            this.tipTooltip.InitialDelay = 250;
            this.tipTooltip.IsBalloon = true;
            this.tipTooltip.MaximumSize = new System.Drawing.Size(0, 0);
            this.tipTooltip.OwnerDraw = true;
            this.tipTooltip.ReshowDelay = 100;
            this.tipTooltip.TooltipCssClass = "htmltooltip";
            this.tipTooltip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.tipTooltip.ToolTipTitle = "Chummer Help";
            // 
            // Avail
            // 
            this.Avail.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Avail.DataPropertyName = "Avail";
            this.Avail.FillWeight = 30F;
            this.Avail.HeaderText = "Avail";
            this.Avail.Name = "Avail";
            this.Avail.ReadOnly = true;
            // 
            // chkHideOverAvailLimit
            // 
            this.chkHideOverAvailLimit.AutoSize = true;
            this.chkHideOverAvailLimit.Location = new System.Drawing.Point(324, 209);
            this.chkHideOverAvailLimit.Name = "chkHideOverAvailLimit";
            this.chkHideOverAvailLimit.Size = new System.Drawing.Size(175, 17);
            this.chkHideOverAvailLimit.TabIndex = 64;
            this.chkHideOverAvailLimit.Tag = "Checkbox_HideOverAvailLimit";
            this.chkHideOverAvailLimit.Text = "Hide Items Over Avail Limit ({0})";
            this.chkHideOverAvailLimit.UseVisualStyleBackColor = true;
            this.chkHideOverAvailLimit.CheckedChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // frmSelectArmor
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(584, 378);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.lblSearchLabel);
            this.Controls.Add(this.cmdOKAdd);
            this.Controls.Add(this.lblCategory);
            this.Controls.Add(this.cboCategory);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectArmor";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectArmor";
            this.Text = "Select Armor";
            this.Load += new System.EventHandler(this.frmSelectArmor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvArmor)).EndInit();
            this.tabControl.ResumeLayout(false);
            this.tabListDetail.ResumeLayout(false);
            this.tabListDetail.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRating)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).EndInit();
            this.tabBrowse.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.ComboBox cboCategory;
        private System.Windows.Forms.Button cmdOKAdd;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearchLabel;
        private System.Windows.Forms.DataGridView dgvArmor;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabBrowse;
        private System.Windows.Forms.TabPage tabListDetail;
        private System.Windows.Forms.CheckBox chkBlackMarketDiscount;
        private System.Windows.Forms.NumericUpDown nudRating;
        private System.Windows.Forms.Label lblRatingLabel;
        private System.Windows.Forms.Label lblArmorValue;
        private System.Windows.Forms.Label lblArmorValueLabel;
        private System.Windows.Forms.Label lblTest;
        private System.Windows.Forms.Label lblTestLabel;
        private System.Windows.Forms.NumericUpDown nudMarkup;
        private System.Windows.Forms.Label lblMarkupLabel;
        private System.Windows.Forms.Label lblMarkupPercentLabel;
        private System.Windows.Forms.Label lblCapacity;
        private System.Windows.Forms.Label lblCapacityLabel;
        private System.Windows.Forms.CheckBox chkFreeItem;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblSourceLabel;
        private System.Windows.Forms.Label lblCost;
        private System.Windows.Forms.Label lblCostLabel;
        private System.Windows.Forms.Label lblAvail;
        private System.Windows.Forms.Label lblAvailLabel;
        private System.Windows.Forms.Label lblArmor;
        private System.Windows.Forms.Label lblArmorLabel;
        private System.Windows.Forms.ListBox lstArmor;
        private System.Windows.Forms.DataGridViewTextBoxColumn Source;
        private System.Windows.Forms.Timer tmrSearch;
        private System.Windows.Forms.DataGridViewTextBoxColumn Cost;
        private System.Windows.Forms.DataGridViewTextBoxColumn Special;
        private System.Windows.Forms.DataGridViewTextBoxColumn Capacity;
        private System.Windows.Forms.DataGridViewTextBoxColumn Armor;
        private System.Windows.Forms.DataGridViewTextBoxColumn ArmorName;
        private TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip tipTooltip;
        private System.Windows.Forms.DataGridViewTextBoxColumn Avail;
        private System.Windows.Forms.DataGridViewTextBoxColumn Guid;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
        private System.Windows.Forms.CheckBox chkHideOverAvailLimit;
    }
}