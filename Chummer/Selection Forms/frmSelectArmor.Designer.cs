using System.Windows.Forms;

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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.lblCategory = new System.Windows.Forms.Label();
            this.cboCategory = new Chummer.ElasticComboBox();
            this.cmdOKAdd = new System.Windows.Forms.Button();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearchLabel = new System.Windows.Forms.Label();
            this.dgvArmor = new System.Windows.Forms.DataGridView();
            this.Guid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ArmorName = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.Armor = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.Capacity = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.Special = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.Avail = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.Source = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.Cost = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabListDetail = new System.Windows.Forms.TabPage();
            this.tlpListDetail = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lstArmor = new System.Windows.Forms.ListBox();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblCapacityLabel = new System.Windows.Forms.Label();
            this.lblCapacity = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.chkHideOverAvailLimit = new System.Windows.Forms.CheckBox();
            this.chkShowOnlyAffordItems = new System.Windows.Forms.CheckBox();
            this.lblArmorValueLabel = new System.Windows.Forms.Label();
            this.lblAvail = new System.Windows.Forms.Label();
            this.lblArmorValue = new System.Windows.Forms.Label();
            this.flpMarkup = new System.Windows.Forms.FlowLayoutPanel();
            this.nudMarkup = new System.Windows.Forms.NumericUpDown();
            this.lblMarkupPercentLabel = new System.Windows.Forms.Label();
            this.flpCheckBoxes = new System.Windows.Forms.FlowLayoutPanel();
            this.chkFreeItem = new System.Windows.Forms.CheckBox();
            this.chkBlackMarketDiscount = new System.Windows.Forms.CheckBox();
            this.lblMarkupLabel = new System.Windows.Forms.Label();
            this.lblCostLabel = new System.Windows.Forms.Label();
            this.lblRatingLabel = new System.Windows.Forms.Label();
            this.lblCost = new System.Windows.Forms.Label();
            this.flpRating = new System.Windows.Forms.FlowLayoutPanel();
            this.nudRating = new System.Windows.Forms.NumericUpDown();
            this.lblRatingNALabel = new System.Windows.Forms.Label();
            this.lblAvailLabel = new System.Windows.Forms.Label();
            this.lblTestLabel = new System.Windows.Forms.Label();
            this.lblTest = new System.Windows.Forms.Label();
            this.tabBrowse = new System.Windows.Forms.TabPage();
            this.tmrSearch = new System.Windows.Forms.Timer(this.components);
            this.dataGridViewTextBoxColumnTranslated1 = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dataGridViewTextBoxColumnTranslated2 = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dataGridViewTextBoxColumnTranslated3 = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dataGridViewTextBoxColumnTranslated4 = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dataGridViewTextBoxColumnTranslated5 = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dataGridViewTextBoxColumnTranslated6 = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dataGridViewTextBoxColumnTranslated7 = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dgvArmor)).BeginInit();
            this.tabControl.SuspendLayout();
            this.tabListDetail.SuspendLayout();
            this.tlpListDetail.SuspendLayout();
            this.flpMarkup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).BeginInit();
            this.flpCheckBoxes.SuspendLayout();
            this.flpRating.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRating)).BeginInit();
            this.tabBrowse.SuspendLayout();
            this.tlpMain.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdOK
            // 
            this.cmdOK.AutoSize = true;
            this.cmdOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOK.Location = new System.Drawing.Point(159, 3);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(72, 23);
            this.cmdOK.TabIndex = 23;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
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
            this.cmdCancel.TabIndex = 25;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // lblCategory
            // 
            this.lblCategory.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(3, 7);
            this.lblCategory.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(52, 13);
            this.lblCategory.TabIndex = 20;
            this.lblCategory.Tag = "Label_Category";
            this.lblCategory.Text = "Category:";
            this.lblCategory.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboCategory
            // 
            this.cboCategory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategory.FormattingEnabled = true;
            this.cboCategory.Location = new System.Drawing.Point(61, 3);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(323, 21);
            this.cboCategory.TabIndex = 21;
            this.cboCategory.TooltipText = "";
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // cmdOKAdd
            // 
            this.cmdOKAdd.AutoSize = true;
            this.cmdOKAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOKAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOKAdd.Location = new System.Drawing.Point(81, 3);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(72, 23);
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
            this.txtSearch.Location = new System.Drawing.Point(440, 3);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(323, 20);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
            this.txtSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyUp);
            // 
            // lblSearchLabel
            // 
            this.lblSearchLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSearchLabel.AutoSize = true;
            this.lblSearchLabel.Location = new System.Drawing.Point(390, 7);
            this.lblSearchLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSearchLabel.Name = "lblSearchLabel";
            this.lblSearchLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSearchLabel.TabIndex = 0;
            this.lblSearchLabel.Tag = "Label_Search";
            this.lblSearchLabel.Text = "&Search:";
            this.lblSearchLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // dgvArmor
            // 
            this.dgvArmor.AllowUserToAddRows = false;
            this.dgvArmor.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.dgvArmor.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvArmor.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvArmor.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvArmor.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvArmor.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvArmor.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Guid,
            this.ArmorName,
            this.Armor,
            this.Capacity,
            this.Special,
            this.Avail,
            this.Source,
            this.Cost});
            this.dgvArmor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvArmor.Location = new System.Drawing.Point(3, 3);
            this.dgvArmor.MultiSelect = false;
            this.dgvArmor.Name = "dgvArmor";
            this.dgvArmor.ReadOnly = true;
            this.dgvArmor.RowHeadersVisible = false;
            this.dgvArmor.RowTemplate.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            this.dgvArmor.RowTemplate.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvArmor.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvArmor.Size = new System.Drawing.Size(752, 455);
            this.dgvArmor.TabIndex = 37;
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
            // ArmorName
            // 
            this.ArmorName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ArmorName.DataPropertyName = "ArmorName";
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.ArmorName.DefaultCellStyle = dataGridViewCellStyle3;
            this.ArmorName.HeaderText = "Name";
            this.ArmorName.Name = "ArmorName";
            this.ArmorName.ReadOnly = true;
            this.ArmorName.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.ArmorName.TranslationTag = "String_Name";
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
            this.Armor.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Armor.TranslationTag = "String_Armor";
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
            this.Capacity.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Capacity.TranslationTag = "String_Capacity";
            this.Capacity.Width = 73;
            // 
            // Special
            // 
            this.Special.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Special.DataPropertyName = "Special";
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.Special.DefaultCellStyle = dataGridViewCellStyle4;
            this.Special.HeaderText = "Special";
            this.Special.Name = "Special";
            this.Special.ReadOnly = true;
            this.Special.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Special.TranslationTag = "String_Special";
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
            this.Avail.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Avail.TranslationTag = "String_Avail";
            this.Avail.Width = 55;
            // 
            // Source
            // 
            this.Source.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Source.DataPropertyName = "Source";
            this.Source.HeaderText = "Source";
            this.Source.Name = "Source";
            this.Source.ReadOnly = true;
            this.Source.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Source.TranslationTag = "String_Source";
            this.Source.Width = 66;
            // 
            // Cost
            // 
            this.Cost.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Cost.DataPropertyName = "Cost";
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopRight;
            dataGridViewCellStyle5.Format = "#,0.##¥";
            dataGridViewCellStyle5.NullValue = null;
            this.Cost.DefaultCellStyle = dataGridViewCellStyle5;
            this.Cost.FillWeight = 60F;
            this.Cost.HeaderText = "Cost";
            this.Cost.Name = "Cost";
            this.Cost.ReadOnly = true;
            this.Cost.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Cost.TranslationTag = "String_Cost";
            this.Cost.Width = 53;
            // 
            // tabControl
            // 
            this.tlpMain.SetColumnSpan(this.tabControl, 4);
            this.tabControl.Controls.Add(this.tabListDetail);
            this.tabControl.Controls.Add(this.tabBrowse);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 27);
            this.tabControl.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(766, 487);
            this.tabControl.TabIndex = 39;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tmrSearch_Tick);
            // 
            // tabListDetail
            // 
            this.tabListDetail.Controls.Add(this.tlpListDetail);
            this.tabListDetail.Location = new System.Drawing.Point(4, 22);
            this.tabListDetail.Name = "tabListDetail";
            this.tabListDetail.Padding = new System.Windows.Forms.Padding(3);
            this.tabListDetail.Size = new System.Drawing.Size(758, 461);
            this.tabListDetail.TabIndex = 1;
            this.tabListDetail.Tag = "Title_ListView";
            this.tabListDetail.Text = "List View";
            this.tabListDetail.UseVisualStyleBackColor = true;
            // 
            // tlpListDetail
            // 
            this.tlpListDetail.AutoSize = true;
            this.tlpListDetail.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpListDetail.ColumnCount = 5;
            this.tlpListDetail.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpListDetail.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpListDetail.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpListDetail.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpListDetail.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpListDetail.Controls.Add(this.lstArmor, 0, 0);
            this.tlpListDetail.Controls.Add(this.lblSource, 2, 6);
            this.tlpListDetail.Controls.Add(this.lblCapacityLabel, 1, 0);
            this.tlpListDetail.Controls.Add(this.lblCapacity, 2, 0);
            this.tlpListDetail.Controls.Add(this.lblSourceLabel, 1, 6);
            this.tlpListDetail.Controls.Add(this.chkHideOverAvailLimit, 1, 7);
            this.tlpListDetail.Controls.Add(this.chkShowOnlyAffordItems, 1, 8);
            this.tlpListDetail.Controls.Add(this.lblArmorValueLabel, 1, 1);
            this.tlpListDetail.Controls.Add(this.lblAvail, 2, 3);
            this.tlpListDetail.Controls.Add(this.lblArmorValue, 2, 1);
            this.tlpListDetail.Controls.Add(this.flpMarkup, 4, 4);
            this.tlpListDetail.Controls.Add(this.flpCheckBoxes, 1, 5);
            this.tlpListDetail.Controls.Add(this.lblMarkupLabel, 3, 4);
            this.tlpListDetail.Controls.Add(this.lblCostLabel, 1, 4);
            this.tlpListDetail.Controls.Add(this.lblRatingLabel, 1, 2);
            this.tlpListDetail.Controls.Add(this.lblCost, 2, 4);
            this.tlpListDetail.Controls.Add(this.flpRating, 2, 2);
            this.tlpListDetail.Controls.Add(this.lblAvailLabel, 1, 3);
            this.tlpListDetail.Controls.Add(this.lblTestLabel, 3, 3);
            this.tlpListDetail.Controls.Add(this.lblTest, 4, 3);
            this.tlpListDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpListDetail.Location = new System.Drawing.Point(3, 3);
            this.tlpListDetail.Name = "tlpListDetail";
            this.tlpListDetail.RowCount = 10;
            this.tlpListDetail.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpListDetail.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpListDetail.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpListDetail.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpListDetail.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpListDetail.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpListDetail.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpListDetail.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpListDetail.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpListDetail.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpListDetail.Size = new System.Drawing.Size(752, 455);
            this.tlpListDetail.TabIndex = 65;
            // 
            // lstArmor
            // 
            this.lstArmor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstArmor.FormattingEnabled = true;
            this.lstArmor.Location = new System.Drawing.Point(3, 3);
            this.lstArmor.Name = "lstArmor";
            this.tlpListDetail.SetRowSpan(this.lstArmor, 10);
            this.lstArmor.Size = new System.Drawing.Size(295, 449);
            this.lstArmor.TabIndex = 58;
            this.lstArmor.SelectedIndexChanged += new System.EventHandler(this.lstArmor_SelectedIndexChanged);
            this.lstArmor.DoubleClick += new System.EventHandler(this.lstArmor_DoubleClick);
            // 
            // lblSource
            // 
            this.lblSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSource.AutoSize = true;
            this.tlpListDetail.SetColumnSpan(this.lblSource, 3);
            this.lblSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSource.Location = new System.Drawing.Point(361, 156);
            this.lblSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 57;
            this.lblSource.Text = "[Source]";
            this.lblSource.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblCapacityLabel
            // 
            this.lblCapacityLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCapacityLabel.AutoSize = true;
            this.lblCapacityLabel.Location = new System.Drawing.Point(304, 6);
            this.lblCapacityLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCapacityLabel.Name = "lblCapacityLabel";
            this.lblCapacityLabel.Size = new System.Drawing.Size(51, 13);
            this.lblCapacityLabel.TabIndex = 44;
            this.lblCapacityLabel.Tag = "Label_Capacity";
            this.lblCapacityLabel.Text = "Capacity:";
            this.lblCapacityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblCapacity
            // 
            this.lblCapacity.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCapacity.AutoSize = true;
            this.lblCapacity.Location = new System.Drawing.Point(361, 6);
            this.lblCapacity.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCapacity.Name = "lblCapacity";
            this.lblCapacity.Size = new System.Drawing.Size(54, 13);
            this.lblCapacity.TabIndex = 45;
            this.lblCapacity.Text = "[Capacity]";
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(311, 156);
            this.lblSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 56;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            this.lblSourceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkHideOverAvailLimit
            // 
            this.chkHideOverAvailLimit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkHideOverAvailLimit.AutoSize = true;
            this.tlpListDetail.SetColumnSpan(this.chkHideOverAvailLimit, 4);
            this.chkHideOverAvailLimit.Location = new System.Drawing.Point(304, 178);
            this.chkHideOverAvailLimit.Name = "chkHideOverAvailLimit";
            this.chkHideOverAvailLimit.Size = new System.Drawing.Size(175, 17);
            this.chkHideOverAvailLimit.TabIndex = 64;
            this.chkHideOverAvailLimit.Tag = "Checkbox_HideOverAvailLimit";
            this.chkHideOverAvailLimit.Text = "Hide Items Over Avail Limit ({0})";
            this.chkHideOverAvailLimit.UseVisualStyleBackColor = true;
            this.chkHideOverAvailLimit.CheckedChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // chkShowOnlyAffordItems
            // 
            this.chkShowOnlyAffordItems.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkShowOnlyAffordItems.AutoSize = true;
            this.tlpListDetail.SetColumnSpan(this.chkShowOnlyAffordItems, 4);
            this.chkShowOnlyAffordItems.Location = new System.Drawing.Point(304, 201);
            this.chkShowOnlyAffordItems.Name = "chkShowOnlyAffordItems";
            this.chkShowOnlyAffordItems.Size = new System.Drawing.Size(164, 17);
            this.chkShowOnlyAffordItems.TabIndex = 67;
            this.chkShowOnlyAffordItems.Tag = "Checkbox_ShowOnlyAffordItems";
            this.chkShowOnlyAffordItems.Text = "Show Only Items I Can Afford";
            this.chkShowOnlyAffordItems.UseVisualStyleBackColor = true;
            this.chkShowOnlyAffordItems.CheckedChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // lblArmorValueLabel
            // 
            this.lblArmorValueLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblArmorValueLabel.AutoSize = true;
            this.lblArmorValueLabel.Location = new System.Drawing.Point(318, 31);
            this.lblArmorValueLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorValueLabel.Name = "lblArmorValueLabel";
            this.lblArmorValueLabel.Size = new System.Drawing.Size(37, 13);
            this.lblArmorValueLabel.TabIndex = 59;
            this.lblArmorValueLabel.Tag = "Label_ArmorValueShort";
            this.lblArmorValueLabel.Text = "Armor:";
            this.lblArmorValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblAvail
            // 
            this.lblAvail.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblAvail.AutoSize = true;
            this.lblAvail.Location = new System.Drawing.Point(361, 82);
            this.lblAvail.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAvail.Name = "lblAvail";
            this.lblAvail.Size = new System.Drawing.Size(36, 13);
            this.lblAvail.TabIndex = 47;
            this.lblAvail.Text = "[Avail]";
            // 
            // lblArmorValue
            // 
            this.lblArmorValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblArmorValue.AutoSize = true;
            this.lblArmorValue.Location = new System.Drawing.Point(361, 31);
            this.lblArmorValue.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorValue.Name = "lblArmorValue";
            this.lblArmorValue.Size = new System.Drawing.Size(20, 13);
            this.lblArmorValue.TabIndex = 60;
            this.lblArmorValue.Text = "[A]";
            this.lblArmorValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flpMarkup
            // 
            this.flpMarkup.AutoSize = true;
            this.flpMarkup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpMarkup.Controls.Add(this.nudMarkup);
            this.flpMarkup.Controls.Add(this.lblMarkupPercentLabel);
            this.flpMarkup.Location = new System.Drawing.Point(581, 101);
            this.flpMarkup.Margin = new System.Windows.Forms.Padding(0);
            this.flpMarkup.Name = "flpMarkup";
            this.flpMarkup.Size = new System.Drawing.Size(83, 26);
            this.flpMarkup.TabIndex = 69;
            // 
            // nudMarkup
            // 
            this.nudMarkup.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudMarkup.AutoSize = true;
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
            this.nudMarkup.Size = new System.Drawing.Size(56, 20);
            this.nudMarkup.TabIndex = 54;
            this.nudMarkup.ValueChanged += new System.EventHandler(this.nudMarkup_ValueChanged);
            // 
            // lblMarkupPercentLabel
            // 
            this.lblMarkupPercentLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMarkupPercentLabel.AutoSize = true;
            this.lblMarkupPercentLabel.Location = new System.Drawing.Point(65, 6);
            this.lblMarkupPercentLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMarkupPercentLabel.Name = "lblMarkupPercentLabel";
            this.lblMarkupPercentLabel.Size = new System.Drawing.Size(15, 13);
            this.lblMarkupPercentLabel.TabIndex = 55;
            this.lblMarkupPercentLabel.Text = "%";
            this.lblMarkupPercentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flpCheckBoxes
            // 
            this.flpCheckBoxes.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpCheckBoxes.AutoSize = true;
            this.flpCheckBoxes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpListDetail.SetColumnSpan(this.flpCheckBoxes, 4);
            this.flpCheckBoxes.Controls.Add(this.chkFreeItem);
            this.flpCheckBoxes.Controls.Add(this.chkBlackMarketDiscount);
            this.flpCheckBoxes.Location = new System.Drawing.Point(301, 127);
            this.flpCheckBoxes.Margin = new System.Windows.Forms.Padding(0);
            this.flpCheckBoxes.Name = "flpCheckBoxes";
            this.flpCheckBoxes.Size = new System.Drawing.Size(225, 23);
            this.flpCheckBoxes.TabIndex = 68;
            // 
            // chkFreeItem
            // 
            this.chkFreeItem.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkFreeItem.AutoSize = true;
            this.chkFreeItem.Location = new System.Drawing.Point(3, 3);
            this.chkFreeItem.Name = "chkFreeItem";
            this.chkFreeItem.Size = new System.Drawing.Size(50, 17);
            this.chkFreeItem.TabIndex = 52;
            this.chkFreeItem.Tag = "Checkbox_Free";
            this.chkFreeItem.Text = "Free!";
            this.chkFreeItem.UseVisualStyleBackColor = true;
            this.chkFreeItem.CheckedChanged += new System.EventHandler(this.chkFreeItem_CheckedChanged);
            // 
            // chkBlackMarketDiscount
            // 
            this.chkBlackMarketDiscount.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkBlackMarketDiscount.AutoSize = true;
            this.chkBlackMarketDiscount.Location = new System.Drawing.Point(59, 3);
            this.chkBlackMarketDiscount.Name = "chkBlackMarketDiscount";
            this.chkBlackMarketDiscount.Size = new System.Drawing.Size(163, 17);
            this.chkBlackMarketDiscount.TabIndex = 63;
            this.chkBlackMarketDiscount.Tag = "Checkbox_BlackMarketDiscount";
            this.chkBlackMarketDiscount.Text = "Black Market Discount (10%)";
            this.chkBlackMarketDiscount.UseVisualStyleBackColor = true;
            this.chkBlackMarketDiscount.Visible = false;
            this.chkBlackMarketDiscount.CheckedChanged += new System.EventHandler(this.chkBlackMarketDiscount_CheckedChanged);
            // 
            // lblMarkupLabel
            // 
            this.lblMarkupLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMarkupLabel.AutoSize = true;
            this.lblMarkupLabel.Location = new System.Drawing.Point(532, 107);
            this.lblMarkupLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMarkupLabel.Name = "lblMarkupLabel";
            this.lblMarkupLabel.Size = new System.Drawing.Size(46, 13);
            this.lblMarkupLabel.TabIndex = 53;
            this.lblMarkupLabel.Tag = "Label_SelectGear_Markup";
            this.lblMarkupLabel.Text = "Markup:";
            this.lblMarkupLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblCostLabel
            // 
            this.lblCostLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCostLabel.AutoSize = true;
            this.lblCostLabel.Location = new System.Drawing.Point(324, 107);
            this.lblCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCostLabel.Name = "lblCostLabel";
            this.lblCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblCostLabel.TabIndex = 50;
            this.lblCostLabel.Tag = "Label_Cost";
            this.lblCostLabel.Text = "Cost:";
            this.lblCostLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblRatingLabel
            // 
            this.lblRatingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRatingLabel.AutoSize = true;
            this.lblRatingLabel.Location = new System.Drawing.Point(314, 56);
            this.lblRatingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRatingLabel.Name = "lblRatingLabel";
            this.lblRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblRatingLabel.TabIndex = 61;
            this.lblRatingLabel.Tag = "Label_Rating";
            this.lblRatingLabel.Text = "Rating:";
            this.lblRatingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblCost
            // 
            this.lblCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCost.AutoSize = true;
            this.lblCost.Location = new System.Drawing.Point(361, 107);
            this.lblCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCost.Name = "lblCost";
            this.lblCost.Size = new System.Drawing.Size(34, 13);
            this.lblCost.TabIndex = 51;
            this.lblCost.Text = "[Cost]";
            // 
            // flpRating
            // 
            this.flpRating.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpRating.AutoSize = true;
            this.flpRating.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpListDetail.SetColumnSpan(this.flpRating, 3);
            this.flpRating.Controls.Add(this.nudRating);
            this.flpRating.Controls.Add(this.lblRatingNALabel);
            this.flpRating.Location = new System.Drawing.Point(358, 50);
            this.flpRating.Margin = new System.Windows.Forms.Padding(0);
            this.flpRating.Name = "flpRating";
            this.flpRating.Size = new System.Drawing.Size(80, 26);
            this.flpRating.TabIndex = 65;
            this.flpRating.WrapContents = false;
            // 
            // nudRating
            // 
            this.nudRating.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudRating.AutoSize = true;
            this.nudRating.Enabled = false;
            this.nudRating.Location = new System.Drawing.Point(3, 3);
            this.nudRating.Name = "nudRating";
            this.nudRating.Size = new System.Drawing.Size(41, 20);
            this.nudRating.TabIndex = 62;
            this.nudRating.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudRating.ValueChanged += new System.EventHandler(this.nudRating_ValueChanged);
            // 
            // lblRatingNALabel
            // 
            this.lblRatingNALabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblRatingNALabel.AutoSize = true;
            this.lblRatingNALabel.Location = new System.Drawing.Point(50, 6);
            this.lblRatingNALabel.Name = "lblRatingNALabel";
            this.lblRatingNALabel.Size = new System.Drawing.Size(27, 13);
            this.lblRatingNALabel.TabIndex = 13;
            this.lblRatingNALabel.Tag = "String_NotApplicable";
            this.lblRatingNALabel.Text = "N/A";
            this.lblRatingNALabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblRatingNALabel.Visible = false;
            // 
            // lblAvailLabel
            // 
            this.lblAvailLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblAvailLabel.AutoSize = true;
            this.lblAvailLabel.Location = new System.Drawing.Point(322, 82);
            this.lblAvailLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAvailLabel.Name = "lblAvailLabel";
            this.lblAvailLabel.Size = new System.Drawing.Size(33, 13);
            this.lblAvailLabel.TabIndex = 46;
            this.lblAvailLabel.Tag = "Label_Avail";
            this.lblAvailLabel.Text = "Avail:";
            this.lblAvailLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblTestLabel
            // 
            this.lblTestLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblTestLabel.AutoSize = true;
            this.lblTestLabel.Location = new System.Drawing.Point(547, 82);
            this.lblTestLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTestLabel.Name = "lblTestLabel";
            this.lblTestLabel.Size = new System.Drawing.Size(31, 13);
            this.lblTestLabel.TabIndex = 48;
            this.lblTestLabel.Tag = "Label_Test";
            this.lblTestLabel.Text = "Test:";
            this.lblTestLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblTest
            // 
            this.lblTest.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblTest.AutoSize = true;
            this.lblTest.Location = new System.Drawing.Point(584, 82);
            this.lblTest.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTest.Name = "lblTest";
            this.lblTest.Size = new System.Drawing.Size(19, 13);
            this.lblTest.TabIndex = 49;
            this.lblTest.Text = "[0]";
            // 
            // tabBrowse
            // 
            this.tabBrowse.Controls.Add(this.dgvArmor);
            this.tabBrowse.Location = new System.Drawing.Point(4, 22);
            this.tabBrowse.Name = "tabBrowse";
            this.tabBrowse.Padding = new System.Windows.Forms.Padding(3);
            this.tabBrowse.Size = new System.Drawing.Size(758, 461);
            this.tabBrowse.TabIndex = 0;
            this.tabBrowse.Tag = "Title_Browse";
            this.tabBrowse.Text = "Browse";
            this.tabBrowse.UseVisualStyleBackColor = true;
            // 
            // tmrSearch
            // 
            this.tmrSearch.Interval = 250;
            // 
            // dataGridViewTextBoxColumnTranslated1
            // 
            this.dataGridViewTextBoxColumnTranslated1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumnTranslated1.DataPropertyName = "ArmorName";
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewTextBoxColumnTranslated1.DefaultCellStyle = dataGridViewCellStyle6;
            this.dataGridViewTextBoxColumnTranslated1.HeaderText = "Name";
            this.dataGridViewTextBoxColumnTranslated1.Name = "dataGridViewTextBoxColumnTranslated1";
            this.dataGridViewTextBoxColumnTranslated1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewTextBoxColumnTranslated1.TranslationTag = "String_Name";
            // 
            // dataGridViewTextBoxColumnTranslated2
            // 
            this.dataGridViewTextBoxColumnTranslated2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumnTranslated2.DataPropertyName = "Armor";
            this.dataGridViewTextBoxColumnTranslated2.FillWeight = 50F;
            this.dataGridViewTextBoxColumnTranslated2.HeaderText = "Armor";
            this.dataGridViewTextBoxColumnTranslated2.Name = "dataGridViewTextBoxColumnTranslated2";
            this.dataGridViewTextBoxColumnTranslated2.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewTextBoxColumnTranslated2.TranslationTag = "String_Armor";
            // 
            // dataGridViewTextBoxColumnTranslated3
            // 
            this.dataGridViewTextBoxColumnTranslated3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumnTranslated3.DataPropertyName = "Capacity";
            this.dataGridViewTextBoxColumnTranslated3.FillWeight = 50F;
            this.dataGridViewTextBoxColumnTranslated3.HeaderText = "Capacity";
            this.dataGridViewTextBoxColumnTranslated3.Name = "dataGridViewTextBoxColumnTranslated3";
            this.dataGridViewTextBoxColumnTranslated3.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewTextBoxColumnTranslated3.TranslationTag = "String_Capacity";
            // 
            // dataGridViewTextBoxColumnTranslated4
            // 
            this.dataGridViewTextBoxColumnTranslated4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumnTranslated4.DataPropertyName = "Special";
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewTextBoxColumnTranslated4.DefaultCellStyle = dataGridViewCellStyle7;
            this.dataGridViewTextBoxColumnTranslated4.HeaderText = "Special";
            this.dataGridViewTextBoxColumnTranslated4.Name = "dataGridViewTextBoxColumnTranslated4";
            this.dataGridViewTextBoxColumnTranslated4.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewTextBoxColumnTranslated4.TranslationTag = "String_Special";
            // 
            // dataGridViewTextBoxColumnTranslated5
            // 
            this.dataGridViewTextBoxColumnTranslated5.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumnTranslated5.DataPropertyName = "Avail";
            this.dataGridViewTextBoxColumnTranslated5.FillWeight = 30F;
            this.dataGridViewTextBoxColumnTranslated5.HeaderText = "Avail";
            this.dataGridViewTextBoxColumnTranslated5.Name = "dataGridViewTextBoxColumnTranslated5";
            this.dataGridViewTextBoxColumnTranslated5.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewTextBoxColumnTranslated5.TranslationTag = "String_Avail";
            // 
            // dataGridViewTextBoxColumnTranslated6
            // 
            this.dataGridViewTextBoxColumnTranslated6.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumnTranslated6.DataPropertyName = "Source";
            this.dataGridViewTextBoxColumnTranslated6.HeaderText = "Source";
            this.dataGridViewTextBoxColumnTranslated6.Name = "dataGridViewTextBoxColumnTranslated6";
            this.dataGridViewTextBoxColumnTranslated6.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewTextBoxColumnTranslated6.TranslationTag = "String_Source";
            // 
            // dataGridViewTextBoxColumnTranslated7
            // 
            this.dataGridViewTextBoxColumnTranslated7.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumnTranslated7.DataPropertyName = "Cost";
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopRight;
            dataGridViewCellStyle8.Format = "#,0.##¥";
            dataGridViewCellStyle8.NullValue = null;
            this.dataGridViewTextBoxColumnTranslated7.DefaultCellStyle = dataGridViewCellStyle8;
            this.dataGridViewTextBoxColumnTranslated7.FillWeight = 60F;
            this.dataGridViewTextBoxColumnTranslated7.HeaderText = "Cost";
            this.dataGridViewTextBoxColumnTranslated7.Name = "dataGridViewTextBoxColumnTranslated7";
            this.dataGridViewTextBoxColumnTranslated7.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewTextBoxColumnTranslated7.TranslationTag = "String_Cost";
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 4;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.Controls.Add(this.tabControl, 0, 1);
            this.tlpMain.Controls.Add(this.lblCategory, 0, 0);
            this.tlpMain.Controls.Add(this.txtSearch, 3, 0);
            this.tlpMain.Controls.Add(this.cboCategory, 1, 0);
            this.tlpMain.Controls.Add(this.lblSearchLabel, 2, 0);
            this.tlpMain.Controls.Add(this.tlpButtons, 0, 2);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 3;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(766, 543);
            this.tlpMain.TabIndex = 40;
            // 
            // tlpButtons
            // 
            this.tlpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpButtons.AutoSize = true;
            this.tlpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpButtons.ColumnCount = 3;
            this.tlpMain.SetColumnSpan(this.tlpButtons, 4);
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
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tlpButtons.Size = new System.Drawing.Size(234, 29);
            this.tlpButtons.TabIndex = 41;
            // 
            // frmSelectArmor
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectArmor";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectArmor";
            this.Text = "Select Armor";
            this.Load += new System.EventHandler(this.frmSelectArmor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvArmor)).EndInit();
            this.tabControl.ResumeLayout(false);
            this.tabListDetail.ResumeLayout(false);
            this.tabListDetail.PerformLayout();
            this.tlpListDetail.ResumeLayout(false);
            this.tlpListDetail.PerformLayout();
            this.flpMarkup.ResumeLayout(false);
            this.flpMarkup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).EndInit();
            this.flpCheckBoxes.ResumeLayout(false);
            this.flpCheckBoxes.PerformLayout();
            this.flpRating.ResumeLayout(false);
            this.flpRating.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRating)).EndInit();
            this.tabBrowse.ResumeLayout(false);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Label lblCategory;
        private ElasticComboBox cboCategory;
        private System.Windows.Forms.Button cmdOKAdd;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearchLabel;
        private System.Windows.Forms.DataGridView dgvArmor;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabBrowse;
        private System.Windows.Forms.TabPage tabListDetail;
        private System.Windows.Forms.CheckBox chkBlackMarketDiscount;
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
        private System.Windows.Forms.ListBox lstArmor;
        private System.Windows.Forms.Timer tmrSearch;
        private System.Windows.Forms.DataGridViewTextBoxColumn Guid;
        private DataGridViewTextBoxColumnTranslated ArmorName;
        private DataGridViewTextBoxColumnTranslated Armor;
        private DataGridViewTextBoxColumnTranslated Capacity;
        private DataGridViewTextBoxColumnTranslated Special;
        private DataGridViewTextBoxColumnTranslated Avail;
        private DataGridViewTextBoxColumnTranslated Source;
        private DataGridViewTextBoxColumnTranslated Cost;
        private CheckBox chkHideOverAvailLimit;
        private FlowLayoutPanel flpRating;
        private NumericUpDown nudRating;
        private Label lblRatingNALabel;
        private CheckBox chkShowOnlyAffordItems;
        private DataGridViewTextBoxColumnTranslated dataGridViewTextBoxColumnTranslated1;
        private DataGridViewTextBoxColumnTranslated dataGridViewTextBoxColumnTranslated2;
        private DataGridViewTextBoxColumnTranslated dataGridViewTextBoxColumnTranslated3;
        private DataGridViewTextBoxColumnTranslated dataGridViewTextBoxColumnTranslated4;
        private DataGridViewTextBoxColumnTranslated dataGridViewTextBoxColumnTranslated5;
        private DataGridViewTextBoxColumnTranslated dataGridViewTextBoxColumnTranslated6;
        private DataGridViewTextBoxColumnTranslated dataGridViewTextBoxColumnTranslated7;
        private BufferedTableLayoutPanel tlpListDetail;
        private BufferedTableLayoutPanel tlpMain;
        private FlowLayoutPanel flpCheckBoxes;
        private FlowLayoutPanel flpMarkup;
        private BufferedTableLayoutPanel tlpButtons;
    }
}
