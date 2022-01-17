namespace Chummer
{
    partial class SelectSpell
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
                Utils.ListItemListPool.Return(_lstCategory);
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
            this.lstSpells = new System.Windows.Forms.ListBox();
            this.lblDescriptorsLabel = new System.Windows.Forms.Label();
            this.lblDescriptors = new System.Windows.Forms.Label();
            this.lblTypeLabel = new System.Windows.Forms.Label();
            this.lblType = new System.Windows.Forms.Label();
            this.lblRangeLabel = new System.Windows.Forms.Label();
            this.lblRange = new System.Windows.Forms.Label();
            this.lblDamageLabel = new System.Windows.Forms.Label();
            this.lblDamage = new System.Windows.Forms.Label();
            this.lblDurationLabel = new System.Windows.Forms.Label();
            this.lblDuration = new System.Windows.Forms.Label();
            this.lblDVLabel = new System.Windows.Forms.Label();
            this.lblDV = new System.Windows.Forms.Label();
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearchLabel = new System.Windows.Forms.Label();
            this.cmdOKAdd = new System.Windows.Forms.Button();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.chkLimited = new Chummer.ColorableCheckBox(this.components);
            this.chkExtended = new Chummer.ColorableCheckBox(this.components);
            this.chkAlchemical = new Chummer.ColorableCheckBox(this.components);
            this.chkFreeBonus = new Chummer.ColorableCheckBox(this.components);
            this.lblCategory = new System.Windows.Forms.Label();
            this.cboCategory = new Chummer.ElasticComboBox();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tableLayoutPanel2 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpRight = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpTopRight = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpMain.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            this.tlpRight.SuspendLayout();
            this.tlpTopRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstSpells
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.lstSpells, 2);
            this.lstSpells.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstSpells.FormattingEnabled = true;
            this.lstSpells.Location = new System.Drawing.Point(3, 30);
            this.lstSpells.Name = "lstSpells";
            this.lstSpells.Size = new System.Drawing.Size(297, 390);
            this.lstSpells.TabIndex = 17;
            this.lstSpells.SelectedIndexChanged += new System.EventHandler(this.lstSpells_SelectedIndexChanged);
            this.lstSpells.DoubleClick += new System.EventHandler(this.cmdOK_Click);
            // 
            // lblDescriptorsLabel
            // 
            this.lblDescriptorsLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDescriptorsLabel.AutoSize = true;
            this.lblDescriptorsLabel.Location = new System.Drawing.Point(3, 6);
            this.lblDescriptorsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDescriptorsLabel.Name = "lblDescriptorsLabel";
            this.lblDescriptorsLabel.Size = new System.Drawing.Size(63, 13);
            this.lblDescriptorsLabel.TabIndex = 2;
            this.lblDescriptorsLabel.Tag = "Label_Descriptors";
            this.lblDescriptorsLabel.Text = "Descriptors:";
            // 
            // lblDescriptors
            // 
            this.lblDescriptors.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDescriptors.AutoSize = true;
            this.lblDescriptors.Location = new System.Drawing.Point(72, 6);
            this.lblDescriptors.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDescriptors.Name = "lblDescriptors";
            this.lblDescriptors.Size = new System.Drawing.Size(66, 13);
            this.lblDescriptors.TabIndex = 3;
            this.lblDescriptors.Text = "[Descriptors]";
            // 
            // lblTypeLabel
            // 
            this.lblTypeLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblTypeLabel.AutoSize = true;
            this.lblTypeLabel.Location = new System.Drawing.Point(32, 31);
            this.lblTypeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTypeLabel.Name = "lblTypeLabel";
            this.lblTypeLabel.Size = new System.Drawing.Size(34, 13);
            this.lblTypeLabel.TabIndex = 4;
            this.lblTypeLabel.Tag = "Label_Type";
            this.lblTypeLabel.Text = "Type:";
            // 
            // lblType
            // 
            this.lblType.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(72, 31);
            this.lblType.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(37, 13);
            this.lblType.TabIndex = 5;
            this.lblType.Text = "[Type]";
            // 
            // lblRangeLabel
            // 
            this.lblRangeLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblRangeLabel.AutoSize = true;
            this.lblRangeLabel.Location = new System.Drawing.Point(24, 56);
            this.lblRangeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRangeLabel.Name = "lblRangeLabel";
            this.lblRangeLabel.Size = new System.Drawing.Size(42, 13);
            this.lblRangeLabel.TabIndex = 6;
            this.lblRangeLabel.Tag = "Label_Range";
            this.lblRangeLabel.Text = "Range:";
            // 
            // lblRange
            // 
            this.lblRange.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblRange.AutoSize = true;
            this.lblRange.Location = new System.Drawing.Point(72, 56);
            this.lblRange.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRange.Name = "lblRange";
            this.lblRange.Size = new System.Drawing.Size(45, 13);
            this.lblRange.TabIndex = 7;
            this.lblRange.Text = "[Range]";
            // 
            // lblDamageLabel
            // 
            this.lblDamageLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDamageLabel.AutoSize = true;
            this.lblDamageLabel.Location = new System.Drawing.Point(16, 81);
            this.lblDamageLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDamageLabel.Name = "lblDamageLabel";
            this.lblDamageLabel.Size = new System.Drawing.Size(50, 13);
            this.lblDamageLabel.TabIndex = 8;
            this.lblDamageLabel.Tag = "Label_Damage";
            this.lblDamageLabel.Text = "Damage:";
            // 
            // lblDamage
            // 
            this.lblDamage.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDamage.AutoSize = true;
            this.lblDamage.Location = new System.Drawing.Point(72, 81);
            this.lblDamage.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDamage.Name = "lblDamage";
            this.lblDamage.Size = new System.Drawing.Size(53, 13);
            this.lblDamage.TabIndex = 9;
            this.lblDamage.Text = "[Damage]";
            // 
            // lblDurationLabel
            // 
            this.lblDurationLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDurationLabel.AutoSize = true;
            this.lblDurationLabel.Location = new System.Drawing.Point(16, 106);
            this.lblDurationLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDurationLabel.Name = "lblDurationLabel";
            this.lblDurationLabel.Size = new System.Drawing.Size(50, 13);
            this.lblDurationLabel.TabIndex = 10;
            this.lblDurationLabel.Tag = "Label_Duration";
            this.lblDurationLabel.Text = "Duration:";
            // 
            // lblDuration
            // 
            this.lblDuration.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDuration.AutoSize = true;
            this.lblDuration.Location = new System.Drawing.Point(72, 106);
            this.lblDuration.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDuration.Name = "lblDuration";
            this.lblDuration.Size = new System.Drawing.Size(53, 13);
            this.lblDuration.TabIndex = 11;
            this.lblDuration.Text = "[Duration]";
            // 
            // lblDVLabel
            // 
            this.lblDVLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDVLabel.AutoSize = true;
            this.lblDVLabel.Location = new System.Drawing.Point(41, 131);
            this.lblDVLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDVLabel.Name = "lblDVLabel";
            this.lblDVLabel.Size = new System.Drawing.Size(25, 13);
            this.lblDVLabel.TabIndex = 12;
            this.lblDVLabel.Tag = "Label_DV";
            this.lblDVLabel.Text = "DV:";
            // 
            // lblDV
            // 
            this.lblDV.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDV.AutoSize = true;
            this.lblDV.Location = new System.Drawing.Point(72, 131);
            this.lblDV.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDV.Name = "lblDV";
            this.lblDV.Size = new System.Drawing.Size(28, 13);
            this.lblDV.TabIndex = 13;
            this.lblDV.Text = "[DV]";
            // 
            // cmdOK
            // 
            this.cmdOK.AutoSize = true;
            this.cmdOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOK.Location = new System.Drawing.Point(159, 3);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(72, 23);
            this.cmdOK.TabIndex = 18;
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
            this.cmdCancel.TabIndex = 20;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(53, 3);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(247, 20);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
            this.txtSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyUp);
            // 
            // lblSearchLabel
            // 
            this.lblSearchLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSearchLabel.AutoSize = true;
            this.lblSearchLabel.Location = new System.Drawing.Point(3, 6);
            this.lblSearchLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSearchLabel.Name = "lblSearchLabel";
            this.lblSearchLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSearchLabel.TabIndex = 0;
            this.lblSearchLabel.Tag = "Label_Search";
            this.lblSearchLabel.Text = "&Search:";
            // 
            // cmdOKAdd
            // 
            this.cmdOKAdd.AutoSize = true;
            this.cmdOKAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOKAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOKAdd.Location = new System.Drawing.Point(81, 3);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(72, 23);
            this.cmdOKAdd.TabIndex = 19;
            this.cmdOKAdd.Tag = "String_AddMore";
            this.cmdOKAdd.Text = "&Add && More";
            this.cmdOKAdd.UseVisualStyleBackColor = true;
            this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
            // 
            // lblSource
            // 
            this.lblSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSource.AutoSize = true;
            this.lblSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSource.Location = new System.Drawing.Point(72, 250);
            this.lblSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 16;
            this.lblSource.Text = "[Source]";
            this.lblSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(22, 250);
            this.lblSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 15;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            // 
            // chkLimited
            // 
            this.chkLimited.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkLimited.AutoSize = true;
            this.tlpRight.SetColumnSpan(this.chkLimited, 2);
            this.chkLimited.DefaultColorScheme = true;
            this.chkLimited.Location = new System.Drawing.Point(3, 153);
            this.chkLimited.Name = "chkLimited";
            this.chkLimited.Size = new System.Drawing.Size(85, 17);
            this.chkLimited.TabIndex = 14;
            this.chkLimited.Tag = "Checkbox_SelectSpell_LimitedSpell";
            this.chkLimited.Text = "Limited Spell";
            this.chkLimited.UseVisualStyleBackColor = true;
            this.chkLimited.CheckedChanged += new System.EventHandler(this.chkLimited_CheckedChanged);
            // 
            // chkExtended
            // 
            this.chkExtended.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkExtended.AutoSize = true;
            this.tlpRight.SetColumnSpan(this.chkExtended, 2);
            this.chkExtended.DefaultColorScheme = true;
            this.chkExtended.Enabled = false;
            this.chkExtended.Location = new System.Drawing.Point(3, 176);
            this.chkExtended.Name = "chkExtended";
            this.chkExtended.Size = new System.Drawing.Size(97, 17);
            this.chkExtended.TabIndex = 21;
            this.chkExtended.Tag = "Checkbox_SelectSpell_ExtendedSpell";
            this.chkExtended.Text = "Extended Spell";
            this.chkExtended.UseVisualStyleBackColor = true;
            this.chkExtended.Visible = false;
            this.chkExtended.CheckedChanged += new System.EventHandler(this.chkExtended_CheckedChanged);
            // 
            // chkAlchemical
            // 
            this.chkAlchemical.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkAlchemical.AutoSize = true;
            this.tlpRight.SetColumnSpan(this.chkAlchemical, 2);
            this.chkAlchemical.DefaultColorScheme = true;
            this.chkAlchemical.Location = new System.Drawing.Point(3, 199);
            this.chkAlchemical.Name = "chkAlchemical";
            this.chkAlchemical.Size = new System.Drawing.Size(134, 17);
            this.chkAlchemical.TabIndex = 15;
            this.chkAlchemical.Tag = "Checkbox_SelectSpell_Alchemical";
            this.chkAlchemical.Text = "Alchemical Preparation";
            this.chkAlchemical.UseVisualStyleBackColor = true;
            // 
            // chkFreeBonus
            // 
            this.chkFreeBonus.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkFreeBonus.AutoSize = true;
            this.tlpRight.SetColumnSpan(this.chkFreeBonus, 2);
            this.chkFreeBonus.DefaultColorScheme = true;
            this.chkFreeBonus.Location = new System.Drawing.Point(3, 223);
            this.chkFreeBonus.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkFreeBonus.Name = "chkFreeBonus";
            this.chkFreeBonus.Size = new System.Drawing.Size(50, 17);
            this.chkFreeBonus.TabIndex = 22;
            this.chkFreeBonus.Tag = "Checkbox_Free";
            this.chkFreeBonus.Text = "Free!";
            this.chkFreeBonus.UseVisualStyleBackColor = true;
            // 
            // lblCategory
            // 
            this.lblCategory.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(3, 7);
            this.lblCategory.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(52, 13);
            this.lblCategory.TabIndex = 37;
            this.lblCategory.Tag = "Label_Category";
            this.lblCategory.Text = "Category:";
            // 
            // cboCategory
            // 
            this.cboCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategory.FormattingEnabled = true;
            this.cboCategory.Location = new System.Drawing.Point(61, 3);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(239, 21);
            this.cboCategory.TabIndex = 38;
            this.cboCategory.TooltipText = "";
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tlpMain.Controls.Add(this.tlpButtons, 1, 2);
            this.tlpMain.Controls.Add(this.tlpRight, 1, 1);
            this.tlpMain.Controls.Add(this.tlpTopRight, 1, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 3;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(606, 423);
            this.tlpMain.TabIndex = 39;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.lblCategory, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.cboCategory, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.lstSpells, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tlpMain.SetRowSpan(this.tableLayoutPanel2, 3);
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(303, 423);
            this.tableLayoutPanel2.TabIndex = 39;
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
            this.tlpButtons.TabIndex = 40;
            // 
            // tlpRight
            // 
            this.tlpRight.AutoSize = true;
            this.tlpRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.ColumnCount = 2;
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRight.Controls.Add(this.lblSource, 1, 10);
            this.tlpRight.Controls.Add(this.lblSourceLabel, 0, 10);
            this.tlpRight.Controls.Add(this.chkFreeBonus, 0, 9);
            this.tlpRight.Controls.Add(this.lblTypeLabel, 0, 1);
            this.tlpRight.Controls.Add(this.lblType, 1, 1);
            this.tlpRight.Controls.Add(this.lblDescriptors, 1, 0);
            this.tlpRight.Controls.Add(this.lblDescriptorsLabel, 0, 0);
            this.tlpRight.Controls.Add(this.chkExtended, 0, 7);
            this.tlpRight.Controls.Add(this.chkLimited, 0, 6);
            this.tlpRight.Controls.Add(this.lblRange, 1, 2);
            this.tlpRight.Controls.Add(this.lblDV, 1, 5);
            this.tlpRight.Controls.Add(this.lblDuration, 1, 4);
            this.tlpRight.Controls.Add(this.lblDVLabel, 0, 5);
            this.tlpRight.Controls.Add(this.lblDurationLabel, 0, 4);
            this.tlpRight.Controls.Add(this.lblDamage, 1, 3);
            this.tlpRight.Controls.Add(this.lblDamageLabel, 0, 3);
            this.tlpRight.Controls.Add(this.lblRangeLabel, 0, 2);
            this.tlpRight.Controls.Add(this.chkAlchemical, 0, 8);
            this.tlpRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRight.Location = new System.Drawing.Point(303, 26);
            this.tlpRight.Margin = new System.Windows.Forms.Padding(0);
            this.tlpRight.Name = "tlpRight";
            this.tlpRight.RowCount = 11;
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpRight.Size = new System.Drawing.Size(303, 269);
            this.tlpRight.TabIndex = 41;
            // 
            // tlpTopRight
            // 
            this.tlpTopRight.AutoSize = true;
            this.tlpTopRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpTopRight.ColumnCount = 2;
            this.tlpTopRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpTopRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTopRight.Controls.Add(this.lblSearchLabel, 0, 0);
            this.tlpTopRight.Controls.Add(this.txtSearch, 1, 0);
            this.tlpTopRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTopRight.Location = new System.Drawing.Point(303, 0);
            this.tlpTopRight.Margin = new System.Windows.Forms.Padding(0);
            this.tlpTopRight.Name = "tlpTopRight";
            this.tlpTopRight.RowCount = 1;
            this.tlpTopRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTopRight.Size = new System.Drawing.Size(303, 26);
            this.tlpTopRight.TabIndex = 42;
            // 
            // frmSelectSpell
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.tlpMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectSpell";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectSpell";
            this.Text = "Select a Spell";
            this.Load += new System.EventHandler(this.SelectSpell_Load);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.tlpRight.ResumeLayout(false);
            this.tlpRight.PerformLayout();
            this.tlpTopRight.ResumeLayout(false);
            this.tlpTopRight.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstSpells;
        private System.Windows.Forms.Label lblDescriptorsLabel;
        private System.Windows.Forms.Label lblDescriptors;
        private System.Windows.Forms.Label lblTypeLabel;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.Label lblRangeLabel;
        private System.Windows.Forms.Label lblRange;
        private System.Windows.Forms.Label lblDamageLabel;
        private System.Windows.Forms.Label lblDamage;
        private System.Windows.Forms.Label lblDurationLabel;
        private System.Windows.Forms.Label lblDuration;
        private System.Windows.Forms.Label lblDVLabel;
        private System.Windows.Forms.Label lblDV;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearchLabel;
        private System.Windows.Forms.Button cmdOKAdd;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblSourceLabel;
        private Chummer.ColorableCheckBox chkLimited;
        private Chummer.ColorableCheckBox chkExtended;
        private Chummer.ColorableCheckBox chkAlchemical;
        private Chummer.ColorableCheckBox chkFreeBonus;
        private System.Windows.Forms.Label lblCategory;
        private ElasticComboBox cboCategory;
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private BufferedTableLayoutPanel tlpButtons;
        private BufferedTableLayoutPanel tlpRight;
        private BufferedTableLayoutPanel tableLayoutPanel2;
        private BufferedTableLayoutPanel tlpTopRight;
    }
}
