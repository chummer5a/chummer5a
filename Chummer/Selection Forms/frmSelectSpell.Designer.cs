namespace Chummer
{
    partial class frmSelectSpell
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
			this.chkLimited = new System.Windows.Forms.CheckBox();
			this.chkExtended = new System.Windows.Forms.CheckBox();
			this.chkAlchemical = new System.Windows.Forms.CheckBox();
			this.chkFreeBonus = new System.Windows.Forms.CheckBox();
			this.lblCategory = new System.Windows.Forms.Label();
			this.cboCategory = new Chummer.ElasticComboBox();
			this.tableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.tableLayoutPanel1.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// lstSpells
			// 
			this.lstSpells.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.SetColumnSpan(this.lstSpells, 2);
			this.lstSpells.FormattingEnabled = true;
			this.lstSpells.Location = new System.Drawing.Point(3, 30);
			this.lstSpells.Name = "lstSpells";
			this.tableLayoutPanel1.SetRowSpan(this.lstSpells, 12);
			this.lstSpells.Size = new System.Drawing.Size(294, 381);
			this.lstSpells.TabIndex = 17;
			this.lstSpells.SelectedIndexChanged += new System.EventHandler(this.lstSpells_SelectedIndexChanged);
			this.lstSpells.DoubleClick += new System.EventHandler(this.treSpells_DoubleClick);
			// 
			// lblDescriptorsLabel
			// 
			this.lblDescriptorsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblDescriptorsLabel.AutoSize = true;
			this.lblDescriptorsLabel.Location = new System.Drawing.Point(324, 33);
			this.lblDescriptorsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
			this.lblDescriptorsLabel.Name = "lblDescriptorsLabel";
			this.lblDescriptorsLabel.Size = new System.Drawing.Size(63, 13);
			this.lblDescriptorsLabel.TabIndex = 2;
			this.lblDescriptorsLabel.Tag = "Label_Descriptors";
			this.lblDescriptorsLabel.Text = "Descriptors:";
			// 
			// lblDescriptors
			// 
			this.lblDescriptors.AutoSize = true;
			this.lblDescriptors.Location = new System.Drawing.Point(393, 33);
			this.lblDescriptors.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
			this.lblDescriptors.Name = "lblDescriptors";
			this.lblDescriptors.Size = new System.Drawing.Size(66, 13);
			this.lblDescriptors.TabIndex = 3;
			this.lblDescriptors.Text = "[Descriptors]";
			// 
			// lblTypeLabel
			// 
			this.lblTypeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblTypeLabel.AutoSize = true;
			this.lblTypeLabel.Location = new System.Drawing.Point(353, 58);
			this.lblTypeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
			this.lblTypeLabel.Name = "lblTypeLabel";
			this.lblTypeLabel.Size = new System.Drawing.Size(34, 13);
			this.lblTypeLabel.TabIndex = 4;
			this.lblTypeLabel.Tag = "Label_Type";
			this.lblTypeLabel.Text = "Type:";
			// 
			// lblType
			// 
			this.lblType.AutoSize = true;
			this.lblType.Location = new System.Drawing.Point(393, 58);
			this.lblType.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
			this.lblType.Name = "lblType";
			this.lblType.Size = new System.Drawing.Size(37, 13);
			this.lblType.TabIndex = 5;
			this.lblType.Text = "[Type]";
			// 
			// lblRangeLabel
			// 
			this.lblRangeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblRangeLabel.AutoSize = true;
			this.lblRangeLabel.Location = new System.Drawing.Point(345, 83);
			this.lblRangeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
			this.lblRangeLabel.Name = "lblRangeLabel";
			this.lblRangeLabel.Size = new System.Drawing.Size(42, 13);
			this.lblRangeLabel.TabIndex = 6;
			this.lblRangeLabel.Tag = "Label_Range";
			this.lblRangeLabel.Text = "Range:";
			// 
			// lblRange
			// 
			this.lblRange.AutoSize = true;
			this.lblRange.Location = new System.Drawing.Point(393, 83);
			this.lblRange.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
			this.lblRange.Name = "lblRange";
			this.lblRange.Size = new System.Drawing.Size(45, 13);
			this.lblRange.TabIndex = 7;
			this.lblRange.Text = "[Range]";
			// 
			// lblDamageLabel
			// 
			this.lblDamageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblDamageLabel.AutoSize = true;
			this.lblDamageLabel.Location = new System.Drawing.Point(337, 108);
			this.lblDamageLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
			this.lblDamageLabel.Name = "lblDamageLabel";
			this.lblDamageLabel.Size = new System.Drawing.Size(50, 13);
			this.lblDamageLabel.TabIndex = 8;
			this.lblDamageLabel.Tag = "Label_Damage";
			this.lblDamageLabel.Text = "Damage:";
			// 
			// lblDamage
			// 
			this.lblDamage.AutoSize = true;
			this.lblDamage.Location = new System.Drawing.Point(393, 108);
			this.lblDamage.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
			this.lblDamage.Name = "lblDamage";
			this.lblDamage.Size = new System.Drawing.Size(53, 13);
			this.lblDamage.TabIndex = 9;
			this.lblDamage.Text = "[Damage]";
			// 
			// lblDurationLabel
			// 
			this.lblDurationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblDurationLabel.AutoSize = true;
			this.lblDurationLabel.Location = new System.Drawing.Point(337, 133);
			this.lblDurationLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
			this.lblDurationLabel.Name = "lblDurationLabel";
			this.lblDurationLabel.Size = new System.Drawing.Size(50, 13);
			this.lblDurationLabel.TabIndex = 10;
			this.lblDurationLabel.Tag = "Label_Duration";
			this.lblDurationLabel.Text = "Duration:";
			// 
			// lblDuration
			// 
			this.lblDuration.AutoSize = true;
			this.lblDuration.Location = new System.Drawing.Point(393, 133);
			this.lblDuration.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
			this.lblDuration.Name = "lblDuration";
			this.lblDuration.Size = new System.Drawing.Size(53, 13);
			this.lblDuration.TabIndex = 11;
			this.lblDuration.Text = "[Duration]";
			// 
			// lblDVLabel
			// 
			this.lblDVLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblDVLabel.AutoSize = true;
			this.lblDVLabel.Location = new System.Drawing.Point(362, 158);
			this.lblDVLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
			this.lblDVLabel.Name = "lblDVLabel";
			this.lblDVLabel.Size = new System.Drawing.Size(25, 13);
			this.lblDVLabel.TabIndex = 12;
			this.lblDVLabel.Tag = "Label_DV";
			this.lblDVLabel.Text = "DV:";
			// 
			// lblDV
			// 
			this.lblDV.AutoSize = true;
			this.lblDV.Location = new System.Drawing.Point(393, 158);
			this.lblDV.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
			this.lblDV.Name = "lblDV";
			this.lblDV.Size = new System.Drawing.Size(28, 13);
			this.lblDV.TabIndex = 13;
			this.lblDV.Text = "[DV]";
			// 
			// cmdOK
			// 
			this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdOK.AutoSize = true;
			this.cmdOK.Location = new System.Drawing.Point(162, 0);
			this.cmdOK.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.cmdOK.Name = "cmdOK";
			this.cmdOK.Size = new System.Drawing.Size(75, 23);
			this.cmdOK.TabIndex = 18;
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
			this.cmdCancel.TabIndex = 20;
			this.cmdCancel.Tag = "String_Cancel";
			this.cmdCancel.Text = "Cancel";
			this.cmdCancel.UseVisualStyleBackColor = true;
			this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
			// 
			// txtSearch
			// 
			this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtSearch.Location = new System.Drawing.Point(393, 3);
			this.txtSearch.Name = "txtSearch";
			this.txtSearch.Size = new System.Drawing.Size(204, 20);
			this.txtSearch.TabIndex = 1;
			this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
			this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
			this.txtSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyUp);
			// 
			// lblSearchLabel
			// 
			this.lblSearchLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblSearchLabel.AutoSize = true;
			this.lblSearchLabel.Location = new System.Drawing.Point(343, 6);
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
			this.cmdOKAdd.AutoSize = true;
			this.cmdOKAdd.Location = new System.Drawing.Point(81, 0);
			this.cmdOKAdd.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.cmdOKAdd.Name = "cmdOKAdd";
			this.cmdOKAdd.Size = new System.Drawing.Size(75, 23);
			this.cmdOKAdd.TabIndex = 19;
			this.cmdOKAdd.Tag = "String_AddMore";
			this.cmdOKAdd.Text = "&Add && More";
			this.cmdOKAdd.UseVisualStyleBackColor = true;
			this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
			// 
			// lblSource
			// 
			this.lblSource.AutoSize = true;
			this.lblSource.Location = new System.Drawing.Point(393, 233);
			this.lblSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
			this.lblSource.Name = "lblSource";
			this.lblSource.Size = new System.Drawing.Size(47, 13);
			this.lblSource.TabIndex = 16;
			this.lblSource.Text = "[Source]";
			this.lblSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
			// 
			// lblSourceLabel
			// 
			this.lblSourceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblSourceLabel.AutoSize = true;
			this.lblSourceLabel.Location = new System.Drawing.Point(343, 233);
			this.lblSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
			this.lblSourceLabel.Name = "lblSourceLabel";
			this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
			this.lblSourceLabel.TabIndex = 15;
			this.lblSourceLabel.Tag = "Label_Source";
			this.lblSourceLabel.Text = "Source:";
			// 
			// chkLimited
			// 
			this.chkLimited.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.chkLimited.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.chkLimited, 2);
			this.chkLimited.Location = new System.Drawing.Point(303, 181);
			this.chkLimited.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.chkLimited.Name = "chkLimited";
			this.chkLimited.Size = new System.Drawing.Size(294, 17);
			this.chkLimited.TabIndex = 14;
			this.chkLimited.Tag = "Checkbox_SelectSpell_LimitedSpell";
			this.chkLimited.Text = "Limited Spell";
			this.chkLimited.UseVisualStyleBackColor = true;
			this.chkLimited.CheckedChanged += new System.EventHandler(this.chkLimited_CheckedChanged);
			// 
			// chkExtended
			// 
			this.chkExtended.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.chkExtended.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.chkExtended, 2);
			this.chkExtended.Enabled = false;
			this.chkExtended.Location = new System.Drawing.Point(303, 281);
			this.chkExtended.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.chkExtended.Name = "chkExtended";
			this.chkExtended.Size = new System.Drawing.Size(294, 17);
			this.chkExtended.TabIndex = 21;
			this.chkExtended.Tag = "Checkbox_SelectSpell_ExtendedSpell";
			this.chkExtended.Text = "Extended Spell";
			this.chkExtended.UseVisualStyleBackColor = true;
			this.chkExtended.Visible = false;
			this.chkExtended.CheckedChanged += new System.EventHandler(this.chkExtended_CheckedChanged);
			// 
			// chkAlchemical
			// 
			this.chkAlchemical.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.chkAlchemical.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.chkAlchemical, 2);
			this.chkAlchemical.Location = new System.Drawing.Point(303, 206);
			this.chkAlchemical.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.chkAlchemical.Name = "chkAlchemical";
			this.chkAlchemical.Size = new System.Drawing.Size(294, 17);
			this.chkAlchemical.TabIndex = 15;
			this.chkAlchemical.Tag = "Checkbox_SelectSpell_Alchemical";
			this.chkAlchemical.Text = "Alchemical Preparation";
			this.chkAlchemical.UseVisualStyleBackColor = true;
			// 
			// chkFreeBonus
			// 
			this.chkFreeBonus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.chkFreeBonus.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.chkFreeBonus, 2);
			this.chkFreeBonus.Location = new System.Drawing.Point(303, 256);
			this.chkFreeBonus.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.chkFreeBonus.Name = "chkFreeBonus";
			this.chkFreeBonus.Size = new System.Drawing.Size(294, 17);
			this.chkFreeBonus.TabIndex = 22;
			this.chkFreeBonus.Tag = "Checkbox_Free";
			this.chkFreeBonus.Text = "Free!";
			this.chkFreeBonus.UseVisualStyleBackColor = true;
			// 
			// lblCategory
			// 
			this.lblCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblCategory.AutoSize = true;
			this.lblCategory.Location = new System.Drawing.Point(17, 6);
			this.lblCategory.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
			this.lblCategory.Name = "lblCategory";
			this.lblCategory.Size = new System.Drawing.Size(52, 13);
			this.lblCategory.TabIndex = 37;
			this.lblCategory.Tag = "Label_Category";
			this.lblCategory.Text = "Category:";
			// 
			// cboCategory
			// 
			this.cboCategory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboCategory.FormattingEnabled = true;
			this.cboCategory.Location = new System.Drawing.Point(75, 3);
			this.cboCategory.Name = "cboCategory";
			this.cboCategory.Size = new System.Drawing.Size(222, 21);
			this.cboCategory.TabIndex = 38;
			this.cboCategory.TooltipText = "";
			this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
			this.tableLayoutPanel1.Controls.Add(this.lstSpells, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.cboCategory, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.lblCategory, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.lblDescriptorsLabel, 2, 1);
			this.tableLayoutPanel1.Controls.Add(this.lblDescriptors, 3, 1);
			this.tableLayoutPanel1.Controls.Add(this.lblTypeLabel, 2, 2);
			this.tableLayoutPanel1.Controls.Add(this.txtSearch, 3, 0);
			this.tableLayoutPanel1.Controls.Add(this.chkFreeBonus, 2, 10);
			this.tableLayoutPanel1.Controls.Add(this.lblSearchLabel, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.lblType, 3, 2);
			this.tableLayoutPanel1.Controls.Add(this.chkAlchemical, 2, 8);
			this.tableLayoutPanel1.Controls.Add(this.lblSourceLabel, 2, 9);
			this.tableLayoutPanel1.Controls.Add(this.lblSource, 3, 9);
			this.tableLayoutPanel1.Controls.Add(this.lblRangeLabel, 2, 3);
			this.tableLayoutPanel1.Controls.Add(this.lblRange, 3, 3);
			this.tableLayoutPanel1.Controls.Add(this.chkLimited, 2, 7);
			this.tableLayoutPanel1.Controls.Add(this.lblDamageLabel, 2, 4);
			this.tableLayoutPanel1.Controls.Add(this.lblDamage, 3, 4);
			this.tableLayoutPanel1.Controls.Add(this.lblDurationLabel, 2, 5);
			this.tableLayoutPanel1.Controls.Add(this.lblDuration, 3, 5);
			this.tableLayoutPanel1.Controls.Add(this.lblDV, 3, 6);
			this.tableLayoutPanel1.Controls.Add(this.lblDVLabel, 2, 6);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 2, 12);
			this.tableLayoutPanel1.Controls.Add(this.chkExtended, 2, 11);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 13;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(600, 417);
			this.tableLayoutPanel1.TabIndex = 39;
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.flowLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 2);
			this.flowLayoutPanel1.Controls.Add(this.cmdOK);
			this.flowLayoutPanel1.Controls.Add(this.cmdOKAdd);
			this.flowLayoutPanel1.Controls.Add(this.cmdCancel);
			this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(363, 394);
			this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(237, 23);
			this.flowLayoutPanel1.TabIndex = 23;
			// 
			// frmSelectSpell
			// 
			this.AcceptButton = this.cmdOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cmdCancel;
			this.ClientSize = new System.Drawing.Size(624, 441);
			this.Controls.Add(this.tableLayoutPanel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmSelectSpell";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Tag = "Title_SelectSpell";
			this.Text = "Select a Spell";
			this.Load += new System.EventHandler(this.frmSelectSpell_Load);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

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
        private System.Windows.Forms.CheckBox chkLimited;
        private System.Windows.Forms.CheckBox chkExtended;
        private System.Windows.Forms.CheckBox chkAlchemical;
        private System.Windows.Forms.CheckBox chkFreeBonus;
        private System.Windows.Forms.Label lblCategory;
        private ElasticComboBox cboCategory;
        private Chummer.BufferedTableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
