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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle25 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle26 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle27 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle28 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle29 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle30 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle31 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle32 = new System.Windows.Forms.DataGridViewCellStyle();
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
            this.chkHideOverAvailLimit = new Chummer.ColorableCheckBox(this.components);
            this.chkShowOnlyAffordItems = new Chummer.ColorableCheckBox(this.components);
            this.bufferedTableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblCapacity = new System.Windows.Forms.Label();
            this.lblAvail = new System.Windows.Forms.Label();
            this.lblArmorValue = new System.Windows.Forms.Label();
            this.flpCheckBoxes = new System.Windows.Forms.FlowLayoutPanel();
            this.chkFreeItem = new Chummer.ColorableCheckBox(this.components);
            this.chkBlackMarketDiscount = new Chummer.ColorableCheckBox(this.components);
            this.lblCost = new System.Windows.Forms.Label();
            this.flpRating = new System.Windows.Forms.FlowLayoutPanel();
            this.nudRating = new Chummer.NumericUpDownEx();
            this.lblRatingNALabel = new System.Windows.Forms.Label();
            this.lblCapacityLabel = new System.Windows.Forms.Label();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.lblArmorValueLabel = new System.Windows.Forms.Label();
            this.lblRatingLabel = new System.Windows.Forms.Label();
            this.flpMarkup = new System.Windows.Forms.FlowLayoutPanel();
            this.nudMarkup = new Chummer.NumericUpDownEx();
            this.lblMarkupPercentLabel = new System.Windows.Forms.Label();
            this.lblMarkupLabel = new System.Windows.Forms.Label();
            this.lblAvailLabel = new System.Windows.Forms.Label();
            this.lblCostLabel = new System.Windows.Forms.Label();
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
            this.bufferedTableLayoutPanel1.SuspendLayout();
            this.flpCheckBoxes.SuspendLayout();
            this.flpRating.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRating)).BeginInit();
            this.flpMarkup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).BeginInit();
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
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.RefreshCurrentList);
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
            dataGridViewCellStyle25.BackColor = System.Drawing.SystemColors.ControlLight;
            this.dgvArmor.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle25;
            this.dgvArmor.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvArmor.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
            dataGridViewCellStyle26.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle26.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle26.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle26.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle26.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle26.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvArmor.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle26;
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
            dataGridViewCellStyle27.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.ArmorName.DefaultCellStyle = dataGridViewCellStyle27;
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
            dataGridViewCellStyle28.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.Special.DefaultCellStyle = dataGridViewCellStyle28;
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
            dataGridViewCellStyle29.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopRight;
            dataGridViewCellStyle29.Format = "#,0.##¥";
            dataGridViewCellStyle29.NullValue = null;
            this.Cost.DefaultCellStyle = dataGridViewCellStyle29;
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
            this.tlpListDetail.ColumnCount = 2;
            this.tlpListDetail.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlpListDetail.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tlpListDetail.Controls.Add(this.lstArmor, 0, 0);
            this.tlpListDetail.Controls.Add(this.chkHideOverAvailLimit, 1, 2);
            this.tlpListDetail.Controls.Add(this.chkShowOnlyAffordItems, 1, 3);
            this.tlpListDetail.Controls.Add(this.bufferedTableLayoutPanel1, 1, 0);
            this.tlpListDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpListDetail.Location = new System.Drawing.Point(3, 3);
            this.tlpListDetail.Name = "tlpListDetail";
            this.tlpListDetail.RowCount = 4;
            this.tlpListDetail.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpListDetail.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpListDetail.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpListDetail.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpListDetail.Size = new System.Drawing.Size(752, 455);
            this.tlpListDetail.TabIndex = 65;
            // 
            // lstArmor
            // 
            this.lstArmor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstArmor.FormattingEnabled = true;
            this.lstArmor.Location = new System.Drawing.Point(3, 3);
            this.lstArmor.Name = "lstArmor";
            this.tlpListDetail.SetRowSpan(this.lstArmor, 4);
            this.lstArmor.Size = new System.Drawing.Size(294, 449);
            this.lstArmor.TabIndex = 58;
            this.lstArmor.SelectedIndexChanged += new System.EventHandler(this.lstArmor_SelectedIndexChanged);
            this.lstArmor.DoubleClick += new System.EventHandler(this.lstArmor_DoubleClick);
            // 
            // chkHideOverAvailLimit
            // 
            this.chkHideOverAvailLimit.AutoSize = true;
            this.chkHideOverAvailLimit.DefaultColorScheme = true;
            this.chkHideOverAvailLimit.Location = new System.Drawing.Point(303, 412);
            this.chkHideOverAvailLimit.Name = "chkHideOverAvailLimit";
            this.chkHideOverAvailLimit.Size = new System.Drawing.Size(175, 17);
            this.chkHideOverAvailLimit.TabIndex = 64;
            this.chkHideOverAvailLimit.Tag = "Checkbox_HideOverAvailLimit";
            this.chkHideOverAvailLimit.Text = "Hide Items Over Avail Limit ({0})";
            this.chkHideOverAvailLimit.UseVisualStyleBackColor = true;
            this.chkHideOverAvailLimit.CheckedChanged += new System.EventHandler(this.RefreshCurrentList);
            // 
            // chkShowOnlyAffordItems
            // 
            this.chkShowOnlyAffordItems.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkShowOnlyAffordItems.AutoSize = true;
            this.chkShowOnlyAffordItems.DefaultColorScheme = true;
            this.chkShowOnlyAffordItems.Location = new System.Drawing.Point(303, 435);
            this.chkShowOnlyAffordItems.Name = "chkShowOnlyAffordItems";
            this.chkShowOnlyAffordItems.Size = new System.Drawing.Size(164, 17);
            this.chkShowOnlyAffordItems.TabIndex = 67;
            this.chkShowOnlyAffordItems.Tag = "Checkbox_ShowOnlyAffordItems";
            this.chkShowOnlyAffordItems.Text = "Show Only Items I Can Afford";
            this.chkShowOnlyAffordItems.UseVisualStyleBackColor = true;
            this.chkShowOnlyAffordItems.CheckedChanged += new System.EventHandler(this.RefreshCurrentList);
            // 
            // bufferedTableLayoutPanel1
            // 
            this.bufferedTableLayoutPanel1.AutoSize = true;
            this.bufferedTableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bufferedTableLayoutPanel1.ColumnCount = 4;
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblCapacityLabel, 0, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblSource, 1, 6);
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblCapacity, 1, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblSourceLabel, 0, 6);
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblArmorValueLabel, 0, 1);
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblArmorValue, 1, 1);
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblRatingLabel, 0, 2);
            this.bufferedTableLayoutPanel1.Controls.Add(this.flpCheckBoxes, 0, 5);
            this.bufferedTableLayoutPanel1.Controls.Add(this.flpMarkup, 3, 4);
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblAvail, 1, 3);
            this.bufferedTableLayoutPanel1.Controls.Add(this.flpRating, 1, 2);
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblMarkupLabel, 2, 4);
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblAvailLabel, 0, 3);
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblCost, 1, 4);
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblCostLabel, 0, 4);
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblTestLabel, 2, 3);
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblTest, 3, 3);
            this.bufferedTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedTableLayoutPanel1.Location = new System.Drawing.Point(300, 0);
            this.bufferedTableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.bufferedTableLayoutPanel1.Name = "bufferedTableLayoutPanel1";
            this.bufferedTableLayoutPanel1.RowCount = 8;
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel1.Size = new System.Drawing.Size(452, 223);
            this.bufferedTableLayoutPanel1.TabIndex = 70;
            // 
            // lblCapacity
            // 
            this.lblCapacity.AutoSize = true;
            this.bufferedTableLayoutPanel1.SetColumnSpan(this.lblCapacity, 3);
            this.lblCapacity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCapacity.Location = new System.Drawing.Point(60, 6);
            this.lblCapacity.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCapacity.Name = "lblCapacity";
            this.lblCapacity.Size = new System.Drawing.Size(389, 13);
            this.lblCapacity.TabIndex = 45;
            this.lblCapacity.Text = "[Capacity]";
            // 
            // lblAvail
            // 
            this.lblAvail.AutoSize = true;
            this.lblAvail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAvail.Location = new System.Drawing.Point(60, 82);
            this.lblAvail.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAvail.Name = "lblAvail";
            this.lblAvail.Size = new System.Drawing.Size(165, 13);
            this.lblAvail.TabIndex = 47;
            this.lblAvail.Text = "[Avail]";
            // 
            // lblArmorValue
            // 
            this.lblArmorValue.AutoSize = true;
            this.bufferedTableLayoutPanel1.SetColumnSpan(this.lblArmorValue, 3);
            this.lblArmorValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblArmorValue.Location = new System.Drawing.Point(60, 31);
            this.lblArmorValue.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorValue.Name = "lblArmorValue";
            this.lblArmorValue.Size = new System.Drawing.Size(389, 13);
            this.lblArmorValue.TabIndex = 60;
            this.lblArmorValue.Text = "[A]";
            this.lblArmorValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flpCheckBoxes
            // 
            this.flpCheckBoxes.AutoSize = true;
            this.flpCheckBoxes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bufferedTableLayoutPanel1.SetColumnSpan(this.flpCheckBoxes, 4);
            this.flpCheckBoxes.Controls.Add(this.chkFreeItem);
            this.flpCheckBoxes.Controls.Add(this.chkBlackMarketDiscount);
            this.flpCheckBoxes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpCheckBoxes.Location = new System.Drawing.Point(0, 127);
            this.flpCheckBoxes.Margin = new System.Windows.Forms.Padding(0);
            this.flpCheckBoxes.Name = "flpCheckBoxes";
            this.flpCheckBoxes.Size = new System.Drawing.Size(452, 23);
            this.flpCheckBoxes.TabIndex = 68;
            // 
            // chkFreeItem
            // 
            this.chkFreeItem.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkFreeItem.AutoSize = true;
            this.chkFreeItem.DefaultColorScheme = true;
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
            this.chkBlackMarketDiscount.DefaultColorScheme = true;
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
            // lblCost
            // 
            this.lblCost.AutoSize = true;
            this.lblCost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCost.Location = new System.Drawing.Point(60, 107);
            this.lblCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCost.Name = "lblCost";
            this.lblCost.Size = new System.Drawing.Size(165, 14);
            this.lblCost.TabIndex = 51;
            this.lblCost.Text = "[Cost]";
            // 
            // flpRating
            // 
            this.flpRating.AutoSize = true;
            this.flpRating.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bufferedTableLayoutPanel1.SetColumnSpan(this.flpRating, 3);
            this.flpRating.Controls.Add(this.nudRating);
            this.flpRating.Controls.Add(this.lblRatingNALabel);
            this.flpRating.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpRating.Location = new System.Drawing.Point(57, 50);
            this.flpRating.Margin = new System.Windows.Forms.Padding(0);
            this.flpRating.Name = "flpRating";
            this.flpRating.Size = new System.Drawing.Size(395, 26);
            this.flpRating.TabIndex = 65;
            this.flpRating.WrapContents = false;
            // 
            // nudRating
            // 
            this.nudRating.AutoSize = true;
            this.nudRating.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudRating.Enabled = false;
            this.nudRating.Location = new System.Drawing.Point(3, 3);
            this.nudRating.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudRating.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
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
            this.lblRatingNALabel.AutoSize = true;
            this.lblRatingNALabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRatingNALabel.Location = new System.Drawing.Point(50, 6);
            this.lblRatingNALabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRatingNALabel.Name = "lblRatingNALabel";
            this.lblRatingNALabel.Size = new System.Drawing.Size(27, 14);
            this.lblRatingNALabel.TabIndex = 13;
            this.lblRatingNALabel.Tag = "String_NotApplicable";
            this.lblRatingNALabel.Text = "N/A";
            this.lblRatingNALabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblRatingNALabel.Visible = false;
            // 
            // lblCapacityLabel
            // 
            this.lblCapacityLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCapacityLabel.AutoSize = true;
            this.lblCapacityLabel.Location = new System.Drawing.Point(3, 6);
            this.lblCapacityLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCapacityLabel.Name = "lblCapacityLabel";
            this.lblCapacityLabel.Size = new System.Drawing.Size(51, 13);
            this.lblCapacityLabel.TabIndex = 44;
            this.lblCapacityLabel.Tag = "Label_Capacity";
            this.lblCapacityLabel.Text = "Capacity:";
            this.lblCapacityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.lblSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSource.Location = new System.Drawing.Point(60, 156);
            this.lblSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 57;
            this.lblSource.Text = "[Source]";
            this.lblSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblSourceLabel.Location = new System.Drawing.Point(10, 156);
            this.lblSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 56;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            // 
            // lblArmorValueLabel
            // 
            this.lblArmorValueLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblArmorValueLabel.AutoSize = true;
            this.lblArmorValueLabel.Location = new System.Drawing.Point(17, 31);
            this.lblArmorValueLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorValueLabel.Name = "lblArmorValueLabel";
            this.lblArmorValueLabel.Size = new System.Drawing.Size(37, 13);
            this.lblArmorValueLabel.TabIndex = 59;
            this.lblArmorValueLabel.Tag = "Label_ArmorValueShort";
            this.lblArmorValueLabel.Text = "Armor:";
            this.lblArmorValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblRatingLabel
            // 
            this.lblRatingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRatingLabel.AutoSize = true;
            this.lblRatingLabel.Location = new System.Drawing.Point(13, 56);
            this.lblRatingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRatingLabel.Name = "lblRatingLabel";
            this.lblRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblRatingLabel.TabIndex = 61;
            this.lblRatingLabel.Tag = "Label_Rating";
            this.lblRatingLabel.Text = "Rating:";
            this.lblRatingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // flpMarkup
            // 
            this.flpMarkup.AutoSize = true;
            this.flpMarkup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpMarkup.Controls.Add(this.nudMarkup);
            this.flpMarkup.Controls.Add(this.lblMarkupPercentLabel);
            this.flpMarkup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpMarkup.Location = new System.Drawing.Point(280, 101);
            this.flpMarkup.Margin = new System.Windows.Forms.Padding(0);
            this.flpMarkup.Name = "flpMarkup";
            this.flpMarkup.Size = new System.Drawing.Size(172, 26);
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
            this.lblMarkupPercentLabel.AutoSize = true;
            this.lblMarkupPercentLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMarkupPercentLabel.Location = new System.Drawing.Point(65, 6);
            this.lblMarkupPercentLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMarkupPercentLabel.Name = "lblMarkupPercentLabel";
            this.lblMarkupPercentLabel.Size = new System.Drawing.Size(15, 14);
            this.lblMarkupPercentLabel.TabIndex = 55;
            this.lblMarkupPercentLabel.Text = "%";
            this.lblMarkupPercentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMarkupLabel
            // 
            this.lblMarkupLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMarkupLabel.AutoSize = true;
            this.lblMarkupLabel.Location = new System.Drawing.Point(231, 107);
            this.lblMarkupLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMarkupLabel.Name = "lblMarkupLabel";
            this.lblMarkupLabel.Size = new System.Drawing.Size(46, 13);
            this.lblMarkupLabel.TabIndex = 53;
            this.lblMarkupLabel.Tag = "Label_SelectGear_Markup";
            this.lblMarkupLabel.Text = "Markup:";
            this.lblMarkupLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblAvailLabel
            // 
            this.lblAvailLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblAvailLabel.AutoSize = true;
            this.lblAvailLabel.Location = new System.Drawing.Point(21, 82);
            this.lblAvailLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAvailLabel.Name = "lblAvailLabel";
            this.lblAvailLabel.Size = new System.Drawing.Size(33, 13);
            this.lblAvailLabel.TabIndex = 46;
            this.lblAvailLabel.Tag = "Label_Avail";
            this.lblAvailLabel.Text = "Avail:";
            this.lblAvailLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblCostLabel
            // 
            this.lblCostLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCostLabel.AutoSize = true;
            this.lblCostLabel.Location = new System.Drawing.Point(23, 107);
            this.lblCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCostLabel.Name = "lblCostLabel";
            this.lblCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblCostLabel.TabIndex = 50;
            this.lblCostLabel.Tag = "Label_Cost";
            this.lblCostLabel.Text = "Cost:";
            this.lblCostLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblTestLabel
            // 
            this.lblTestLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblTestLabel.AutoSize = true;
            this.lblTestLabel.Location = new System.Drawing.Point(246, 82);
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
            this.lblTest.AutoSize = true;
            this.lblTest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTest.Location = new System.Drawing.Point(283, 82);
            this.lblTest.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTest.Name = "lblTest";
            this.lblTest.Size = new System.Drawing.Size(166, 13);
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
            dataGridViewCellStyle30.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewTextBoxColumnTranslated1.DefaultCellStyle = dataGridViewCellStyle30;
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
            dataGridViewCellStyle31.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewTextBoxColumnTranslated4.DefaultCellStyle = dataGridViewCellStyle31;
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
            dataGridViewCellStyle32.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopRight;
            dataGridViewCellStyle32.Format = "#,0.##¥";
            dataGridViewCellStyle32.NullValue = null;
            this.dataGridViewTextBoxColumnTranslated7.DefaultCellStyle = dataGridViewCellStyle32;
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
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
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
            this.bufferedTableLayoutPanel1.ResumeLayout(false);
            this.bufferedTableLayoutPanel1.PerformLayout();
            this.flpCheckBoxes.ResumeLayout(false);
            this.flpCheckBoxes.PerformLayout();
            this.flpRating.ResumeLayout(false);
            this.flpRating.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRating)).EndInit();
            this.flpMarkup.ResumeLayout(false);
            this.flpMarkup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).EndInit();
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
        private Chummer.ColorableCheckBox chkBlackMarketDiscount;
        private System.Windows.Forms.Label lblRatingLabel;
        private System.Windows.Forms.Label lblArmorValue;
        private System.Windows.Forms.Label lblArmorValueLabel;
        private System.Windows.Forms.Label lblTest;
        private System.Windows.Forms.Label lblTestLabel;
        private Chummer.NumericUpDownEx nudMarkup;
        private System.Windows.Forms.Label lblMarkupLabel;
        private System.Windows.Forms.Label lblMarkupPercentLabel;
        private System.Windows.Forms.Label lblCapacity;
        private System.Windows.Forms.Label lblCapacityLabel;
        private Chummer.ColorableCheckBox chkFreeItem;
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
        private FlowLayoutPanel flpRating;
        private NumericUpDownEx nudRating;
        private Label lblRatingNALabel;
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
        private ColorableCheckBox chkHideOverAvailLimit;
        private ColorableCheckBox chkShowOnlyAffordItems;
        private BufferedTableLayoutPanel bufferedTableLayoutPanel1;
    }
}
