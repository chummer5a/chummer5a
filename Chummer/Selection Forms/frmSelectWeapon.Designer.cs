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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.cboCategory = new Chummer.ElasticComboBox();
            this.lblCategory = new System.Windows.Forms.Label();
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearchLabel = new System.Windows.Forms.Label();
            this.cmdOKAdd = new System.Windows.Forms.Button();
            this.dgvWeapons = new System.Windows.Forms.DataGridView();
            this.dgvc_Guid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvc_Name = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dgvc_Dice = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dgvc_Accuracy = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dgvc_Damage = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dgvc_AP = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dgvc_RC = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dgvc_Ammo = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dgvc_Mode = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dgvc_Reach = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dgvc_Accessories = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.Label_Avail = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.Label_Source = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dgvc_Cost = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabListView = new System.Windows.Forms.TabPage();
            this.tlpWeapon = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lstWeapon = new System.Windows.Forms.ListBox();
            this.lblWeaponRC = new System.Windows.Forms.Label();
            this.lblWeaponRCLabel = new System.Windows.Forms.Label();
            this.lblWeaponAccuracyLabel = new System.Windows.Forms.Label();
            this.lblWeaponAccuracy = new System.Windows.Forms.Label();
            this.lblWeaponAmmo = new System.Windows.Forms.Label();
            this.lblWeaponAmmoLabel = new System.Windows.Forms.Label();
            this.lblTest = new System.Windows.Forms.Label();
            this.lblWeaponMode = new System.Windows.Forms.Label();
            this.lblWeaponModeLabel = new System.Windows.Forms.Label();
            this.lblTestLabel = new System.Windows.Forms.Label();
            this.lblWeaponDamageLabel = new System.Windows.Forms.Label();
            this.lblWeaponAPLabel = new System.Windows.Forms.Label();
            this.lblWeaponReachLabel = new System.Windows.Forms.Label();
            this.lblWeaponReach = new System.Windows.Forms.Label();
            this.lblWeaponDamage = new System.Windows.Forms.Label();
            this.lblWeaponAvailLabel = new System.Windows.Forms.Label();
            this.lblWeaponCostLabel = new System.Windows.Forms.Label();
            this.lblWeaponCost = new System.Windows.Forms.Label();
            this.lblWeaponAP = new System.Windows.Forms.Label();
            this.lblWeaponAvail = new System.Windows.Forms.Label();
            this.lblMarkupLabel = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.chkHideOverAvailLimit = new System.Windows.Forms.CheckBox();
            this.chkShowOnlyAffordItems = new System.Windows.Forms.CheckBox();
            this.lblIncludedAccessoriesLabel = new System.Windows.Forms.Label();
            this.lblIncludedAccessories = new System.Windows.Forms.Label();
            this.lblSource = new System.Windows.Forms.Label();
            this.flpMarkup = new System.Windows.Forms.FlowLayoutPanel();
            this.nudMarkup = new System.Windows.Forms.NumericUpDown();
            this.lblMarkupPercentLabel = new System.Windows.Forms.Label();
            this.flpCheckBoxes = new System.Windows.Forms.FlowLayoutPanel();
            this.chkFreeItem = new System.Windows.Forms.CheckBox();
            this.chkBlackMarketDiscount = new System.Windows.Forms.CheckBox();
            this.tabBrowse = new System.Windows.Forms.TabPage();
            this.tmrSearch = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel2 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWeapons)).BeginInit();
            this.tabControl.SuspendLayout();
            this.tabListView.SuspendLayout();
            this.tlpWeapon.SuspendLayout();
            this.flpMarkup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).BeginInit();
            this.flpCheckBoxes.SuspendLayout();
            this.tabBrowse.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cboCategory
            // 
            this.cboCategory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategory.FormattingEnabled = true;
            this.cboCategory.Location = new System.Drawing.Point(61, 3);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(243, 21);
            this.cboCategory.TabIndex = 30;
            this.cboCategory.TooltipText = "";
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // lblCategory
            // 
            this.lblCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(3, 6);
            this.lblCategory.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(52, 13);
            this.lblCategory.TabIndex = 29;
            this.lblCategory.Tag = "Label_Category";
            this.lblCategory.Text = "Category:";
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.AutoSize = true;
            this.cmdOK.Location = new System.Drawing.Point(162, 0);
            this.cmdOK.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
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
            this.cmdCancel.AutoSize = true;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(0, 0);
            this.cmdCancel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 33;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(360, 3);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(243, 20);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
            this.txtSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyUp);
            // 
            // lblSearchLabel
            // 
            this.lblSearchLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSearchLabel.AutoSize = true;
            this.lblSearchLabel.Location = new System.Drawing.Point(310, 6);
            this.lblSearchLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSearchLabel.Name = "lblSearchLabel";
            this.lblSearchLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSearchLabel.TabIndex = 0;
            this.lblSearchLabel.Tag = "Label_Search";
            this.lblSearchLabel.Text = "&Search:";
            // 
            // cmdOKAdd
            // 
            this.cmdOKAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOKAdd.Location = new System.Drawing.Point(81, 0);
            this.cmdOKAdd.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(75, 23);
            this.cmdOKAdd.TabIndex = 32;
            this.cmdOKAdd.Tag = "String_AddMore";
            this.cmdOKAdd.Text = "&Add && More";
            this.cmdOKAdd.UseVisualStyleBackColor = true;
            this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
            // 
            // dgvWeapons
            // 
            this.dgvWeapons.AllowUserToAddRows = false;
            this.dgvWeapons.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvWeapons.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvWeapons.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvWeapons.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgvc_Guid,
            this.dgvc_Name,
            this.dgvc_Dice,
            this.dgvc_Accuracy,
            this.dgvc_Damage,
            this.dgvc_AP,
            this.dgvc_RC,
            this.dgvc_Ammo,
            this.dgvc_Mode,
            this.dgvc_Reach,
            this.dgvc_Accessories,
            this.Label_Avail,
            this.Label_Source,
            this.dgvc_Cost});
            this.dgvWeapons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvWeapons.Location = new System.Drawing.Point(3, 3);
            this.dgvWeapons.MultiSelect = false;
            this.dgvWeapons.Name = "dgvWeapons";
            this.dgvWeapons.ReadOnly = true;
            this.dgvWeapons.RowHeadersVisible = false;
            this.dgvWeapons.RowTemplate.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            this.dgvWeapons.RowTemplate.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvWeapons.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvWeapons.Size = new System.Drawing.Size(592, 334);
            this.dgvWeapons.TabIndex = 36;
            this.dgvWeapons.DoubleClick += new System.EventHandler(this.dgvWeapons_DoubleClick);
            // 
            // dgvc_Guid
            // 
            this.dgvc_Guid.DataPropertyName = "WeaponGuid";
            this.dgvc_Guid.HeaderText = "dgvc_Guid";
            this.dgvc_Guid.Name = "dgvc_Guid";
            this.dgvc_Guid.ReadOnly = true;
            this.dgvc_Guid.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Guid.Visible = false;
            // 
            // dgvc_Name
            // 
            this.dgvc_Name.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvc_Name.DataPropertyName = "WeaponName";
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Name.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvc_Name.HeaderText = "Name";
            this.dgvc_Name.Name = "dgvc_Name";
            this.dgvc_Name.ReadOnly = true;
            this.dgvc_Name.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Name.TranslationTag = null;
            this.dgvc_Name.Width = 60;
            // 
            // dgvc_Dice
            // 
            this.dgvc_Dice.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvc_Dice.DataPropertyName = "Dice";
            this.dgvc_Dice.FillWeight = 30F;
            this.dgvc_Dice.HeaderText = "Dice Pool";
            this.dgvc_Dice.Name = "dgvc_Dice";
            this.dgvc_Dice.ReadOnly = true;
            this.dgvc_Dice.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Dice.TranslationTag = null;
            this.dgvc_Dice.Width = 78;
            // 
            // dgvc_Accuracy
            // 
            this.dgvc_Accuracy.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvc_Accuracy.DataPropertyName = "Accuracy";
            this.dgvc_Accuracy.FillWeight = 50F;
            this.dgvc_Accuracy.HeaderText = "Accuracy";
            this.dgvc_Accuracy.Name = "dgvc_Accuracy";
            this.dgvc_Accuracy.ReadOnly = true;
            this.dgvc_Accuracy.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Accuracy.TranslationTag = null;
            this.dgvc_Accuracy.Width = 77;
            // 
            // dgvc_Damage
            // 
            this.dgvc_Damage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvc_Damage.DataPropertyName = "Damage";
            this.dgvc_Damage.FillWeight = 50F;
            this.dgvc_Damage.HeaderText = "Damage";
            this.dgvc_Damage.Name = "dgvc_Damage";
            this.dgvc_Damage.ReadOnly = true;
            this.dgvc_Damage.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Damage.TranslationTag = null;
            this.dgvc_Damage.Width = 72;
            // 
            // dgvc_AP
            // 
            this.dgvc_AP.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvc_AP.DataPropertyName = "AP";
            this.dgvc_AP.FillWeight = 30F;
            this.dgvc_AP.HeaderText = "AP";
            this.dgvc_AP.Name = "dgvc_AP";
            this.dgvc_AP.ReadOnly = true;
            this.dgvc_AP.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_AP.TranslationTag = null;
            this.dgvc_AP.Width = 46;
            // 
            // dgvc_RC
            // 
            this.dgvc_RC.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvc_RC.DataPropertyName = "RC";
            this.dgvc_RC.FillWeight = 30F;
            this.dgvc_RC.HeaderText = "RC";
            this.dgvc_RC.Name = "dgvc_RC";
            this.dgvc_RC.ReadOnly = true;
            this.dgvc_RC.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_RC.TranslationTag = null;
            this.dgvc_RC.Width = 47;
            // 
            // dgvc_Ammo
            // 
            this.dgvc_Ammo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvc_Ammo.DataPropertyName = "Ammo";
            this.dgvc_Ammo.FillWeight = 60F;
            this.dgvc_Ammo.HeaderText = "Ammo";
            this.dgvc_Ammo.Name = "dgvc_Ammo";
            this.dgvc_Ammo.ReadOnly = true;
            this.dgvc_Ammo.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Ammo.TranslationTag = null;
            this.dgvc_Ammo.Width = 61;
            // 
            // dgvc_Mode
            // 
            this.dgvc_Mode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvc_Mode.DataPropertyName = "Mode";
            this.dgvc_Mode.FillWeight = 60F;
            this.dgvc_Mode.HeaderText = "Mode";
            this.dgvc_Mode.Name = "dgvc_Mode";
            this.dgvc_Mode.ReadOnly = true;
            this.dgvc_Mode.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Mode.TranslationTag = null;
            this.dgvc_Mode.Width = 59;
            // 
            // dgvc_Reach
            // 
            this.dgvc_Reach.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvc_Reach.DataPropertyName = "Reach";
            this.dgvc_Reach.FillWeight = 40F;
            this.dgvc_Reach.HeaderText = "Reach";
            this.dgvc_Reach.Name = "dgvc_Reach";
            this.dgvc_Reach.ReadOnly = true;
            this.dgvc_Reach.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Reach.TranslationTag = null;
            this.dgvc_Reach.Width = 64;
            // 
            // dgvc_Accessories
            // 
            this.dgvc_Accessories.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvc_Accessories.DataPropertyName = "Accessories";
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Accessories.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvc_Accessories.HeaderText = "Accessories";
            this.dgvc_Accessories.Name = "dgvc_Accessories";
            this.dgvc_Accessories.ReadOnly = true;
            this.dgvc_Accessories.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Accessories.TranslationTag = null;
            this.dgvc_Accessories.Width = 89;
            // 
            // Label_Avail
            // 
            this.Label_Avail.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Label_Avail.DataPropertyName = "Avail";
            this.Label_Avail.FillWeight = 30F;
            this.Label_Avail.HeaderText = "Avail";
            this.Label_Avail.Name = "Label_Avail";
            this.Label_Avail.ReadOnly = true;
            this.Label_Avail.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Label_Avail.TranslationTag = null;
            this.Label_Avail.Width = 55;
            // 
            // Label_Source
            // 
            this.Label_Source.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Label_Source.DataPropertyName = "Source";
            this.Label_Source.HeaderText = "Source";
            this.Label_Source.Name = "Label_Source";
            this.Label_Source.ReadOnly = true;
            this.Label_Source.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Label_Source.TranslationTag = null;
            this.Label_Source.Width = 66;
            // 
            // dgvc_Cost
            // 
            this.dgvc_Cost.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvc_Cost.DataPropertyName = "Cost";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopRight;
            dataGridViewCellStyle4.Format = "#,0.##¥";
            dataGridViewCellStyle4.NullValue = null;
            this.dgvc_Cost.DefaultCellStyle = dataGridViewCellStyle4;
            this.dgvc_Cost.FillWeight = 60F;
            this.dgvc_Cost.HeaderText = "Cost";
            this.dgvc_Cost.Name = "dgvc_Cost";
            this.dgvc_Cost.ReadOnly = true;
            this.dgvc_Cost.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Cost.TranslationTag = null;
            this.dgvc_Cost.Width = 53;
            // 
            // tabControl
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.tabControl, 4);
            this.tabControl.Controls.Add(this.tabListView);
            this.tabControl.Controls.Add(this.tabBrowse);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 27);
            this.tabControl.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(606, 366);
            this.tabControl.TabIndex = 38;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tmrSearch_Tick);
            // 
            // tabListView
            // 
            this.tabListView.Controls.Add(this.tlpWeapon);
            this.tabListView.Location = new System.Drawing.Point(4, 22);
            this.tabListView.Name = "tabListView";
            this.tabListView.Padding = new System.Windows.Forms.Padding(3);
            this.tabListView.Size = new System.Drawing.Size(598, 340);
            this.tabListView.TabIndex = 1;
            this.tabListView.Tag = "Title_ListView";
            this.tabListView.Text = "List View";
            this.tabListView.UseVisualStyleBackColor = true;
            // 
            // tlpWeapon
            // 
            this.tlpWeapon.ColumnCount = 5;
            this.tlpWeapon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 301F));
            this.tlpWeapon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpWeapon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpWeapon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpWeapon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpWeapon.Controls.Add(this.lstWeapon, 0, 0);
            this.tlpWeapon.Controls.Add(this.lblWeaponRC, 4, 0);
            this.tlpWeapon.Controls.Add(this.lblWeaponRCLabel, 3, 0);
            this.tlpWeapon.Controls.Add(this.lblWeaponAccuracyLabel, 3, 4);
            this.tlpWeapon.Controls.Add(this.lblWeaponAccuracy, 4, 4);
            this.tlpWeapon.Controls.Add(this.lblWeaponAmmo, 4, 1);
            this.tlpWeapon.Controls.Add(this.lblWeaponAmmoLabel, 3, 1);
            this.tlpWeapon.Controls.Add(this.lblTest, 4, 3);
            this.tlpWeapon.Controls.Add(this.lblWeaponMode, 4, 2);
            this.tlpWeapon.Controls.Add(this.lblWeaponModeLabel, 3, 2);
            this.tlpWeapon.Controls.Add(this.lblTestLabel, 3, 3);
            this.tlpWeapon.Controls.Add(this.lblWeaponDamageLabel, 1, 0);
            this.tlpWeapon.Controls.Add(this.lblWeaponAPLabel, 1, 1);
            this.tlpWeapon.Controls.Add(this.lblWeaponReachLabel, 1, 2);
            this.tlpWeapon.Controls.Add(this.lblWeaponReach, 2, 2);
            this.tlpWeapon.Controls.Add(this.lblWeaponDamage, 2, 0);
            this.tlpWeapon.Controls.Add(this.lblWeaponAvailLabel, 1, 3);
            this.tlpWeapon.Controls.Add(this.lblWeaponCostLabel, 1, 4);
            this.tlpWeapon.Controls.Add(this.lblWeaponCost, 2, 4);
            this.tlpWeapon.Controls.Add(this.lblWeaponAP, 2, 1);
            this.tlpWeapon.Controls.Add(this.lblWeaponAvail, 2, 3);
            this.tlpWeapon.Controls.Add(this.lblMarkupLabel, 1, 6);
            this.tlpWeapon.Controls.Add(this.lblSourceLabel, 1, 7);
            this.tlpWeapon.Controls.Add(this.chkHideOverAvailLimit, 1, 8);
            this.tlpWeapon.Controls.Add(this.chkShowOnlyAffordItems, 1, 9);
            this.tlpWeapon.Controls.Add(this.lblIncludedAccessoriesLabel, 1, 10);
            this.tlpWeapon.Controls.Add(this.lblIncludedAccessories, 1, 11);
            this.tlpWeapon.Controls.Add(this.lblSource, 2, 7);
            this.tlpWeapon.Controls.Add(this.flpMarkup, 2, 6);
            this.tlpWeapon.Controls.Add(this.flpCheckBoxes, 1, 5);
            this.tlpWeapon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpWeapon.Location = new System.Drawing.Point(3, 3);
            this.tlpWeapon.Name = "tlpWeapon";
            this.tlpWeapon.RowCount = 12;
            this.tlpWeapon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpWeapon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpWeapon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpWeapon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpWeapon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpWeapon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpWeapon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpWeapon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpWeapon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpWeapon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpWeapon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpWeapon.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpWeapon.Size = new System.Drawing.Size(592, 334);
            this.tlpWeapon.TabIndex = 71;
            // 
            // lstWeapon
            // 
            this.lstWeapon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstWeapon.FormattingEnabled = true;
            this.lstWeapon.Location = new System.Drawing.Point(3, 3);
            this.lstWeapon.Name = "lstWeapon";
            this.tlpWeapon.SetRowSpan(this.lstWeapon, 12);
            this.lstWeapon.Size = new System.Drawing.Size(295, 328);
            this.lstWeapon.TabIndex = 66;
            this.lstWeapon.SelectedIndexChanged += new System.EventHandler(this.lstWeapon_SelectedIndexChanged);
            this.lstWeapon.DoubleClick += new System.EventHandler(this.cmdOK_Click);
            // 
            // lblWeaponRC
            // 
            this.lblWeaponRC.AutoSize = true;
            this.lblWeaponRC.Location = new System.Drawing.Point(508, 6);
            this.lblWeaponRC.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponRC.Name = "lblWeaponRC";
            this.lblWeaponRC.Size = new System.Drawing.Size(28, 13);
            this.lblWeaponRC.TabIndex = 43;
            this.lblWeaponRC.Text = "[RC]";
            // 
            // lblWeaponRCLabel
            // 
            this.lblWeaponRCLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponRCLabel.AutoSize = true;
            this.lblWeaponRCLabel.Location = new System.Drawing.Point(477, 6);
            this.lblWeaponRCLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponRCLabel.Name = "lblWeaponRCLabel";
            this.lblWeaponRCLabel.Size = new System.Drawing.Size(25, 13);
            this.lblWeaponRCLabel.TabIndex = 42;
            this.lblWeaponRCLabel.Tag = "Label_RC";
            this.lblWeaponRCLabel.Text = "RC:";
            // 
            // lblWeaponAccuracyLabel
            // 
            this.lblWeaponAccuracyLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponAccuracyLabel.AutoSize = true;
            this.lblWeaponAccuracyLabel.Location = new System.Drawing.Point(447, 106);
            this.lblWeaponAccuracyLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponAccuracyLabel.Name = "lblWeaponAccuracyLabel";
            this.lblWeaponAccuracyLabel.Size = new System.Drawing.Size(55, 13);
            this.lblWeaponAccuracyLabel.TabIndex = 67;
            this.lblWeaponAccuracyLabel.Tag = "Label_Accuracy";
            this.lblWeaponAccuracyLabel.Text = "Accuracy:";
            // 
            // lblWeaponAccuracy
            // 
            this.lblWeaponAccuracy.AutoSize = true;
            this.lblWeaponAccuracy.Location = new System.Drawing.Point(508, 106);
            this.lblWeaponAccuracy.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponAccuracy.Name = "lblWeaponAccuracy";
            this.lblWeaponAccuracy.Size = new System.Drawing.Size(58, 13);
            this.lblWeaponAccuracy.TabIndex = 68;
            this.lblWeaponAccuracy.Text = "[Accuracy]";
            // 
            // lblWeaponAmmo
            // 
            this.lblWeaponAmmo.AutoSize = true;
            this.lblWeaponAmmo.Location = new System.Drawing.Point(508, 31);
            this.lblWeaponAmmo.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponAmmo.Name = "lblWeaponAmmo";
            this.lblWeaponAmmo.Size = new System.Drawing.Size(42, 13);
            this.lblWeaponAmmo.TabIndex = 47;
            this.lblWeaponAmmo.Text = "[Ammo]";
            // 
            // lblWeaponAmmoLabel
            // 
            this.lblWeaponAmmoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponAmmoLabel.AutoSize = true;
            this.lblWeaponAmmoLabel.Location = new System.Drawing.Point(463, 31);
            this.lblWeaponAmmoLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponAmmoLabel.Name = "lblWeaponAmmoLabel";
            this.lblWeaponAmmoLabel.Size = new System.Drawing.Size(39, 13);
            this.lblWeaponAmmoLabel.TabIndex = 46;
            this.lblWeaponAmmoLabel.Tag = "Label_Ammo";
            this.lblWeaponAmmoLabel.Text = "Ammo:";
            // 
            // lblTest
            // 
            this.lblTest.AutoSize = true;
            this.lblTest.Location = new System.Drawing.Point(508, 81);
            this.lblTest.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTest.Name = "lblTest";
            this.lblTest.Size = new System.Drawing.Size(19, 13);
            this.lblTest.TabIndex = 55;
            this.lblTest.Text = "[0]";
            // 
            // lblWeaponMode
            // 
            this.lblWeaponMode.AutoSize = true;
            this.lblWeaponMode.Location = new System.Drawing.Point(508, 56);
            this.lblWeaponMode.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponMode.Name = "lblWeaponMode";
            this.lblWeaponMode.Size = new System.Drawing.Size(40, 13);
            this.lblWeaponMode.TabIndex = 51;
            this.lblWeaponMode.Text = "[Mode]";
            // 
            // lblWeaponModeLabel
            // 
            this.lblWeaponModeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponModeLabel.AutoSize = true;
            this.lblWeaponModeLabel.Location = new System.Drawing.Point(465, 56);
            this.lblWeaponModeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponModeLabel.Name = "lblWeaponModeLabel";
            this.lblWeaponModeLabel.Size = new System.Drawing.Size(37, 13);
            this.lblWeaponModeLabel.TabIndex = 50;
            this.lblWeaponModeLabel.Tag = "Label_Mode";
            this.lblWeaponModeLabel.Text = "Mode:";
            // 
            // lblTestLabel
            // 
            this.lblTestLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTestLabel.AutoSize = true;
            this.lblTestLabel.Location = new System.Drawing.Point(471, 81);
            this.lblTestLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTestLabel.Name = "lblTestLabel";
            this.lblTestLabel.Size = new System.Drawing.Size(31, 13);
            this.lblTestLabel.TabIndex = 54;
            this.lblTestLabel.Tag = "Label_Test";
            this.lblTestLabel.Text = "Test:";
            // 
            // lblWeaponDamageLabel
            // 
            this.lblWeaponDamageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponDamageLabel.AutoSize = true;
            this.lblWeaponDamageLabel.Location = new System.Drawing.Point(304, 6);
            this.lblWeaponDamageLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponDamageLabel.Name = "lblWeaponDamageLabel";
            this.lblWeaponDamageLabel.Size = new System.Drawing.Size(50, 13);
            this.lblWeaponDamageLabel.TabIndex = 40;
            this.lblWeaponDamageLabel.Tag = "Label_Damage";
            this.lblWeaponDamageLabel.Text = "Damage:";
            // 
            // lblWeaponAPLabel
            // 
            this.lblWeaponAPLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponAPLabel.AutoSize = true;
            this.lblWeaponAPLabel.Location = new System.Drawing.Point(330, 31);
            this.lblWeaponAPLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponAPLabel.Name = "lblWeaponAPLabel";
            this.lblWeaponAPLabel.Size = new System.Drawing.Size(24, 13);
            this.lblWeaponAPLabel.TabIndex = 44;
            this.lblWeaponAPLabel.Tag = "Label_AP";
            this.lblWeaponAPLabel.Text = "AP:";
            // 
            // lblWeaponReachLabel
            // 
            this.lblWeaponReachLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponReachLabel.AutoSize = true;
            this.lblWeaponReachLabel.Location = new System.Drawing.Point(312, 56);
            this.lblWeaponReachLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponReachLabel.Name = "lblWeaponReachLabel";
            this.lblWeaponReachLabel.Size = new System.Drawing.Size(42, 13);
            this.lblWeaponReachLabel.TabIndex = 48;
            this.lblWeaponReachLabel.Tag = "Label_Reach";
            this.lblWeaponReachLabel.Text = "Reach:";
            // 
            // lblWeaponReach
            // 
            this.lblWeaponReach.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblWeaponReach.AutoSize = true;
            this.lblWeaponReach.Location = new System.Drawing.Point(360, 56);
            this.lblWeaponReach.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponReach.Name = "lblWeaponReach";
            this.lblWeaponReach.Size = new System.Drawing.Size(45, 13);
            this.lblWeaponReach.TabIndex = 49;
            this.lblWeaponReach.Text = "[Reach]";
            // 
            // lblWeaponDamage
            // 
            this.lblWeaponDamage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblWeaponDamage.AutoSize = true;
            this.lblWeaponDamage.Location = new System.Drawing.Point(360, 6);
            this.lblWeaponDamage.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponDamage.Name = "lblWeaponDamage";
            this.lblWeaponDamage.Size = new System.Drawing.Size(53, 13);
            this.lblWeaponDamage.TabIndex = 41;
            this.lblWeaponDamage.Text = "[Damage]";
            // 
            // lblWeaponAvailLabel
            // 
            this.lblWeaponAvailLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponAvailLabel.AutoSize = true;
            this.lblWeaponAvailLabel.Location = new System.Drawing.Point(321, 81);
            this.lblWeaponAvailLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponAvailLabel.Name = "lblWeaponAvailLabel";
            this.lblWeaponAvailLabel.Size = new System.Drawing.Size(33, 13);
            this.lblWeaponAvailLabel.TabIndex = 52;
            this.lblWeaponAvailLabel.Tag = "Label_Avail";
            this.lblWeaponAvailLabel.Text = "Avail:";
            // 
            // lblWeaponCostLabel
            // 
            this.lblWeaponCostLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponCostLabel.AutoSize = true;
            this.lblWeaponCostLabel.Location = new System.Drawing.Point(323, 106);
            this.lblWeaponCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponCostLabel.Name = "lblWeaponCostLabel";
            this.lblWeaponCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblWeaponCostLabel.TabIndex = 56;
            this.lblWeaponCostLabel.Tag = "Label_Cost";
            this.lblWeaponCostLabel.Text = "Cost:";
            // 
            // lblWeaponCost
            // 
            this.lblWeaponCost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblWeaponCost.AutoSize = true;
            this.lblWeaponCost.Location = new System.Drawing.Point(360, 106);
            this.lblWeaponCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponCost.Name = "lblWeaponCost";
            this.lblWeaponCost.Size = new System.Drawing.Size(34, 13);
            this.lblWeaponCost.TabIndex = 57;
            this.lblWeaponCost.Text = "[Cost]";
            // 
            // lblWeaponAP
            // 
            this.lblWeaponAP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblWeaponAP.AutoSize = true;
            this.lblWeaponAP.Location = new System.Drawing.Point(360, 31);
            this.lblWeaponAP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponAP.Name = "lblWeaponAP";
            this.lblWeaponAP.Size = new System.Drawing.Size(27, 13);
            this.lblWeaponAP.TabIndex = 45;
            this.lblWeaponAP.Text = "[AP]";
            // 
            // lblWeaponAvail
            // 
            this.lblWeaponAvail.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblWeaponAvail.AutoSize = true;
            this.lblWeaponAvail.Location = new System.Drawing.Point(360, 81);
            this.lblWeaponAvail.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponAvail.Name = "lblWeaponAvail";
            this.lblWeaponAvail.Size = new System.Drawing.Size(36, 13);
            this.lblWeaponAvail.TabIndex = 53;
            this.lblWeaponAvail.Text = "[Avail]";
            // 
            // lblMarkupLabel
            // 
            this.lblMarkupLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMarkupLabel.AutoSize = true;
            this.lblMarkupLabel.Location = new System.Drawing.Point(308, 156);
            this.lblMarkupLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMarkupLabel.Name = "lblMarkupLabel";
            this.lblMarkupLabel.Size = new System.Drawing.Size(46, 13);
            this.lblMarkupLabel.TabIndex = 59;
            this.lblMarkupLabel.Tag = "Label_SelectGear_Markup";
            this.lblMarkupLabel.Text = "Markup:";
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(310, 182);
            this.lblSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 64;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            // 
            // chkHideOverAvailLimit
            // 
            this.chkHideOverAvailLimit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkHideOverAvailLimit.AutoSize = true;
            this.tlpWeapon.SetColumnSpan(this.chkHideOverAvailLimit, 4);
            this.chkHideOverAvailLimit.Location = new System.Drawing.Point(304, 205);
            this.chkHideOverAvailLimit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkHideOverAvailLimit.Name = "chkHideOverAvailLimit";
            this.chkHideOverAvailLimit.Size = new System.Drawing.Size(285, 17);
            this.chkHideOverAvailLimit.TabIndex = 70;
            this.chkHideOverAvailLimit.Tag = "Checkbox_HideOverAvailLimit";
            this.chkHideOverAvailLimit.Text = "Hide Items Over Avail Limit ({0})";
            this.chkHideOverAvailLimit.UseVisualStyleBackColor = true;
            this.chkHideOverAvailLimit.CheckedChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // chkShowOnlyAffordItems
            // 
            this.chkShowOnlyAffordItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkShowOnlyAffordItems.AutoSize = true;
            this.tlpWeapon.SetColumnSpan(this.chkShowOnlyAffordItems, 4);
            this.chkShowOnlyAffordItems.Location = new System.Drawing.Point(304, 230);
            this.chkShowOnlyAffordItems.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkShowOnlyAffordItems.Name = "chkShowOnlyAffordItems";
            this.chkShowOnlyAffordItems.Size = new System.Drawing.Size(285, 17);
            this.chkShowOnlyAffordItems.TabIndex = 71;
            this.chkShowOnlyAffordItems.Tag = "Checkbox_ShowOnlyAffordItems";
            this.chkShowOnlyAffordItems.Text = "Show Only Items I Can Afford";
            this.chkShowOnlyAffordItems.UseVisualStyleBackColor = true;
            this.chkShowOnlyAffordItems.CheckedChanged += new System.EventHandler(this.chkShowOnlyAffordItems_CheckedChanged);
            // 
            // lblIncludedAccessoriesLabel
            // 
            this.lblIncludedAccessoriesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblIncludedAccessoriesLabel.AutoSize = true;
            this.tlpWeapon.SetColumnSpan(this.lblIncludedAccessoriesLabel, 3);
            this.lblIncludedAccessoriesLabel.Location = new System.Drawing.Point(304, 257);
            this.lblIncludedAccessoriesLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblIncludedAccessoriesLabel.Name = "lblIncludedAccessoriesLabel";
            this.lblIncludedAccessoriesLabel.Size = new System.Drawing.Size(197, 13);
            this.lblIncludedAccessoriesLabel.TabIndex = 62;
            this.lblIncludedAccessoriesLabel.Tag = "Label_SelectWeapon_IncludedItems";
            this.lblIncludedAccessoriesLabel.Text = "Included Accessories and Modifications:";
            // 
            // lblIncludedAccessories
            // 
            this.lblIncludedAccessories.AutoSize = true;
            this.tlpWeapon.SetColumnSpan(this.lblIncludedAccessories, 4);
            this.lblIncludedAccessories.Location = new System.Drawing.Point(304, 282);
            this.lblIncludedAccessories.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblIncludedAccessories.Name = "lblIncludedAccessories";
            this.lblIncludedAccessories.Size = new System.Drawing.Size(39, 13);
            this.lblIncludedAccessories.TabIndex = 63;
            this.lblIncludedAccessories.Text = "[None]";
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.lblSource.Location = new System.Drawing.Point(360, 182);
            this.lblSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 65;
            this.lblSource.Text = "[Source]";
            this.lblSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // flpMarkup
            // 
            this.flpMarkup.AutoSize = true;
            this.flpMarkup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpWeapon.SetColumnSpan(this.flpMarkup, 3);
            this.flpMarkup.Controls.Add(this.nudMarkup);
            this.flpMarkup.Controls.Add(this.lblMarkupPercentLabel);
            this.flpMarkup.Location = new System.Drawing.Point(357, 150);
            this.flpMarkup.Margin = new System.Windows.Forms.Padding(0);
            this.flpMarkup.Name = "flpMarkup";
            this.flpMarkup.Size = new System.Drawing.Size(127, 26);
            this.flpMarkup.TabIndex = 72;
            // 
            // nudMarkup
            // 
            this.nudMarkup.DecimalPlaces = 2;
            this.nudMarkup.Location = new System.Drawing.Point(3, 3);
            this.nudMarkup.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudMarkup.Minimum = new decimal(new int[] {
            9999,
            0,
            0,
            -2147352576});
            this.nudMarkup.Name = "nudMarkup";
            this.nudMarkup.Size = new System.Drawing.Size(100, 20);
            this.nudMarkup.TabIndex = 60;
            this.nudMarkup.ValueChanged += new System.EventHandler(this.nudMarkup_ValueChanged);
            // 
            // lblMarkupPercentLabel
            // 
            this.lblMarkupPercentLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblMarkupPercentLabel.AutoSize = true;
            this.lblMarkupPercentLabel.Location = new System.Drawing.Point(109, 6);
            this.lblMarkupPercentLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMarkupPercentLabel.Name = "lblMarkupPercentLabel";
            this.lblMarkupPercentLabel.Size = new System.Drawing.Size(15, 14);
            this.lblMarkupPercentLabel.TabIndex = 61;
            this.lblMarkupPercentLabel.Text = "%";
            // 
            // flpCheckBoxes
            // 
            this.flpCheckBoxes.AutoSize = true;
            this.flpCheckBoxes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpWeapon.SetColumnSpan(this.flpCheckBoxes, 4);
            this.flpCheckBoxes.Controls.Add(this.chkFreeItem);
            this.flpCheckBoxes.Controls.Add(this.chkBlackMarketDiscount);
            this.flpCheckBoxes.Location = new System.Drawing.Point(301, 125);
            this.flpCheckBoxes.Margin = new System.Windows.Forms.Padding(0);
            this.flpCheckBoxes.Name = "flpCheckBoxes";
            this.flpCheckBoxes.Size = new System.Drawing.Size(225, 25);
            this.flpCheckBoxes.TabIndex = 73;
            // 
            // chkFreeItem
            // 
            this.chkFreeItem.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkFreeItem.AutoSize = true;
            this.chkFreeItem.Location = new System.Drawing.Point(3, 4);
            this.chkFreeItem.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkFreeItem.Name = "chkFreeItem";
            this.chkFreeItem.Size = new System.Drawing.Size(50, 17);
            this.chkFreeItem.TabIndex = 58;
            this.chkFreeItem.Tag = "Checkbox_Free";
            this.chkFreeItem.Text = "Free!";
            this.chkFreeItem.UseVisualStyleBackColor = true;
            this.chkFreeItem.CheckedChanged += new System.EventHandler(this.chkFreeItem_CheckedChanged);
            // 
            // chkBlackMarketDiscount
            // 
            this.chkBlackMarketDiscount.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkBlackMarketDiscount.AutoSize = true;
            this.chkBlackMarketDiscount.Location = new System.Drawing.Point(59, 4);
            this.chkBlackMarketDiscount.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkBlackMarketDiscount.Name = "chkBlackMarketDiscount";
            this.chkBlackMarketDiscount.Size = new System.Drawing.Size(163, 17);
            this.chkBlackMarketDiscount.TabIndex = 69;
            this.chkBlackMarketDiscount.Tag = "Checkbox_BlackMarketDiscount";
            this.chkBlackMarketDiscount.Text = "Black Market Discount (10%)";
            this.chkBlackMarketDiscount.UseVisualStyleBackColor = true;
            this.chkBlackMarketDiscount.Visible = false;
            this.chkBlackMarketDiscount.CheckedChanged += new System.EventHandler(this.chkBlackMarketDiscount_CheckedChanged);
            // 
            // tabBrowse
            // 
            this.tabBrowse.Controls.Add(this.dgvWeapons);
            this.tabBrowse.Location = new System.Drawing.Point(4, 22);
            this.tabBrowse.Name = "tabBrowse";
            this.tabBrowse.Padding = new System.Windows.Forms.Padding(3);
            this.tabBrowse.Size = new System.Drawing.Size(598, 340);
            this.tabBrowse.TabIndex = 0;
            this.tabBrowse.Tag = "Title_Browse";
            this.tabBrowse.Text = "Browse";
            this.tabBrowse.UseVisualStyleBackColor = true;
            // 
            // tmrSearch
            // 
            this.tmrSearch.Interval = 250;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.tabControl, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.lblCategory, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.cboCategory, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.txtSearch, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.lblSearchLabel, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.flowLayoutPanel1, 0, 2);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(9, 9);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(606, 423);
            this.tableLayoutPanel2.TabIndex = 39;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel2.SetColumnSpan(this.flowLayoutPanel1, 4);
            this.flowLayoutPanel1.Controls.Add(this.cmdOK);
            this.flowLayoutPanel1.Controls.Add(this.cmdOKAdd);
            this.flowLayoutPanel1.Controls.Add(this.cmdCancel);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(366, 397);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(237, 23);
            this.flowLayoutPanel1.TabIndex = 39;
            // 
            // frmSelectWeapon
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.tableLayoutPanel2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectWeapon";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectWeapon";
            this.Text = "Select Weapon";
            this.Load += new System.EventHandler(this.frmSelectWeapon_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvWeapons)).EndInit();
            this.tabControl.ResumeLayout(false);
            this.tabListView.ResumeLayout(false);
            this.tlpWeapon.ResumeLayout(false);
            this.tlpWeapon.PerformLayout();
            this.flpMarkup.ResumeLayout(false);
            this.flpMarkup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).EndInit();
            this.flpCheckBoxes.ResumeLayout(false);
            this.flpCheckBoxes.PerformLayout();
            this.tabBrowse.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private ElasticComboBox cboCategory;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearchLabel;
        private System.Windows.Forms.Button cmdOKAdd;
        private System.Windows.Forms.DataGridView dgvWeapons;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabBrowse;
        private System.Windows.Forms.TabPage tabListView;
        private System.Windows.Forms.CheckBox chkBlackMarketDiscount;
        private System.Windows.Forms.Label lblWeaponAccuracy;
        private System.Windows.Forms.Label lblWeaponAccuracyLabel;
        private System.Windows.Forms.Label lblTest;
        private System.Windows.Forms.NumericUpDown nudMarkup;
        private System.Windows.Forms.Label lblMarkupPercentLabel;
        private System.Windows.Forms.CheckBox chkFreeItem;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblSourceLabel;
        private System.Windows.Forms.Label lblIncludedAccessories;
        private System.Windows.Forms.Label lblIncludedAccessoriesLabel;
        private System.Windows.Forms.Label lblTestLabel;
        private System.Windows.Forms.Label lblMarkupLabel;
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
        private System.Windows.Forms.ListBox lstWeapon;
        private System.Windows.Forms.Timer tmrSearch;
        private System.Windows.Forms.CheckBox chkHideOverAvailLimit;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvc_Guid;
        private DataGridViewTextBoxColumnTranslated dgvc_Name;
        private DataGridViewTextBoxColumnTranslated dgvc_Dice;
        private DataGridViewTextBoxColumnTranslated dgvc_Accuracy;
        private DataGridViewTextBoxColumnTranslated dgvc_Damage;
        private DataGridViewTextBoxColumnTranslated dgvc_AP;
        private DataGridViewTextBoxColumnTranslated dgvc_RC;
        private DataGridViewTextBoxColumnTranslated dgvc_Ammo;
        private DataGridViewTextBoxColumnTranslated dgvc_Mode;
        private DataGridViewTextBoxColumnTranslated dgvc_Reach;
        private DataGridViewTextBoxColumnTranslated dgvc_Accessories;
        private DataGridViewTextBoxColumnTranslated Label_Avail;
        private DataGridViewTextBoxColumnTranslated Label_Source;
        private DataGridViewTextBoxColumnTranslated dgvc_Cost;
        private Chummer.BufferedTableLayoutPanel tlpWeapon;
        private System.Windows.Forms.CheckBox chkShowOnlyAffordItems;
        private Chummer.BufferedTableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flpMarkup;
        private System.Windows.Forms.FlowLayoutPanel flpCheckBoxes;
    }
}
