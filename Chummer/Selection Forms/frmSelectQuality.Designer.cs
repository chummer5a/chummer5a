namespace Chummer
{
    partial class frmSelectQuality
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
            this.lstQualities = new System.Windows.Forms.ListBox();
            this.lblCategory = new System.Windows.Forms.Label();
            this.cboCategory = new Chummer.ElasticComboBox();
            this.lblSource = new Chummer.LabelWithToolTip();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.cmdOKAdd = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lblBP = new Chummer.LabelWithToolTip();
            this.lblBPLabel = new System.Windows.Forms.Label();
            this.chkFree = new System.Windows.Forms.CheckBox();
            this.chkMetagenic = new System.Windows.Forms.CheckBox();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearchLabel = new System.Windows.Forms.Label();
            this.chkNotMetagenic = new System.Windows.Forms.CheckBox();
            this.nudMinimumBP = new System.Windows.Forms.NumericUpDown();
            this.nudValueBP = new System.Windows.Forms.NumericUpDown();
            this.nudMaximumBP = new System.Windows.Forms.NumericUpDown();
            this.lblMinimumBP = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.gpbKarmaFilter = new System.Windows.Forms.GroupBox();
            this.tlpKarmaFilter = new System.Windows.Forms.TableLayoutPanel();
            this.chkLimitList = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinimumBP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudValueBP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaximumBP)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.gpbKarmaFilter.SuspendLayout();
            this.tlpKarmaFilter.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstQualities
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.lstQualities, 2);
            this.lstQualities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstQualities.FormattingEnabled = true;
            this.lstQualities.Location = new System.Drawing.Point(3, 30);
            this.lstQualities.Name = "lstQualities";
            this.lstQualities.Size = new System.Drawing.Size(295, 390);
            this.lstQualities.TabIndex = 9;
            this.lstQualities.SelectedIndexChanged += new System.EventHandler(this.lstQualities_SelectedIndexChanged);
            this.lstQualities.DoubleClick += new System.EventHandler(this.lstQualities_DoubleClick);
            // 
            // lblCategory
            // 
            this.lblCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(3, 6);
            this.lblCategory.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(52, 13);
            this.lblCategory.TabIndex = 10;
            this.lblCategory.Tag = "Label_Category";
            this.lblCategory.Text = "Category:";
            // 
            // cboCategory
            // 
            this.cboCategory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategory.FormattingEnabled = true;
            this.cboCategory.Location = new System.Drawing.Point(61, 3);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(237, 21);
            this.cboCategory.TabIndex = 11;
            this.cboCategory.TooltipText = "";
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.lblSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSource.Location = new System.Drawing.Point(354, 57);
            this.lblSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 5;
            this.lblSource.Text = "[Source]";
            this.lblSource.ToolTipText = "";
            this.lblSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(304, 57);
            this.lblSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 4;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            // 
            // cmdOKAdd
            // 
            this.cmdOKAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOKAdd.AutoSize = true;
            this.cmdOKAdd.Location = new System.Drawing.Point(84, 3);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(75, 23);
            this.cmdOKAdd.TabIndex = 13;
            this.cmdOKAdd.Tag = "String_AddMore";
            this.cmdOKAdd.Text = "&Add && More";
            this.cmdOKAdd.UseVisualStyleBackColor = true;
            this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.AutoSize = true;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(3, 3);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 14;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.AutoSize = true;
            this.cmdOK.Location = new System.Drawing.Point(165, 3);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 12;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lblBP
            // 
            this.lblBP.AutoSize = true;
            this.lblBP.Location = new System.Drawing.Point(354, 32);
            this.lblBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBP.Name = "lblBP";
            this.lblBP.Size = new System.Drawing.Size(27, 13);
            this.lblBP.TabIndex = 3;
            this.lblBP.Text = "[BP]";
            this.lblBP.ToolTipText = "";
            // 
            // lblBPLabel
            // 
            this.lblBPLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBPLabel.AutoSize = true;
            this.lblBPLabel.Location = new System.Drawing.Point(308, 32);
            this.lblBPLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBPLabel.Name = "lblBPLabel";
            this.lblBPLabel.Size = new System.Drawing.Size(40, 13);
            this.lblBPLabel.TabIndex = 2;
            this.lblBPLabel.Tag = "Label_Karma";
            this.lblBPLabel.Text = "Karma:";
            // 
            // chkFree
            // 
            this.chkFree.AutoSize = true;
            this.chkFree.Location = new System.Drawing.Point(3, 132);
            this.chkFree.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkFree.Name = "chkFree";
            this.chkFree.Size = new System.Drawing.Size(50, 17);
            this.chkFree.TabIndex = 8;
            this.chkFree.Tag = "Checkbox_Free";
            this.chkFree.Text = "Free!";
            this.chkFree.UseVisualStyleBackColor = true;
            this.chkFree.CheckedChanged += new System.EventHandler(this.chkFree_CheckedChanged);
            // 
            // chkMetagenic
            // 
            this.chkMetagenic.AutoSize = true;
            this.chkMetagenic.Location = new System.Drawing.Point(3, 157);
            this.chkMetagenic.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkMetagenic.Name = "chkMetagenic";
            this.chkMetagenic.Size = new System.Drawing.Size(171, 17);
            this.chkMetagenic.TabIndex = 7;
            this.chkMetagenic.Tag = "Checkbox_SelectQuality_Metagenic";
            this.chkMetagenic.Text = "Show only Metagenic Qualities";
            this.chkMetagenic.UseVisualStyleBackColor = true;
            this.chkMetagenic.CheckedChanged += new System.EventHandler(this.chkMetagenic_CheckedChanged);
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(354, 3);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(249, 20);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
            this.txtSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyUp);
            // 
            // lblSearchLabel
            // 
            this.lblSearchLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSearchLabel.AutoSize = true;
            this.lblSearchLabel.Location = new System.Drawing.Point(304, 6);
            this.lblSearchLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSearchLabel.Name = "lblSearchLabel";
            this.lblSearchLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSearchLabel.TabIndex = 0;
            this.lblSearchLabel.Tag = "Label_Search";
            this.lblSearchLabel.Text = "&Search:";
            // 
            // chkNotMetagenic
            // 
            this.chkNotMetagenic.AutoSize = true;
            this.chkNotMetagenic.Location = new System.Drawing.Point(3, 182);
            this.chkNotMetagenic.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkNotMetagenic.Name = "chkNotMetagenic";
            this.chkNotMetagenic.Size = new System.Drawing.Size(175, 17);
            this.chkNotMetagenic.TabIndex = 15;
            this.chkNotMetagenic.Tag = "Checkbox_SelectQuality_Not_Metagenic";
            this.chkNotMetagenic.Text = "Don\'t show Metagenic Qualities";
            this.chkNotMetagenic.UseVisualStyleBackColor = true;
            this.chkNotMetagenic.CheckedChanged += new System.EventHandler(this.chkNotMetagenic_CheckedChanged);
            // 
            // nudMinimumBP
            // 
            this.nudMinimumBP.AutoSize = true;
            this.nudMinimumBP.Location = new System.Drawing.Point(63, 3);
            this.nudMinimumBP.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.nudMinimumBP.Name = "nudMinimumBP";
            this.nudMinimumBP.Size = new System.Drawing.Size(41, 20);
            this.nudMinimumBP.TabIndex = 16;
            this.nudMinimumBP.TextChanged += new System.EventHandler(this.KarmaFilter);
            this.nudMinimumBP.ValueChanged += new System.EventHandler(this.KarmaFilter);
            // 
            // nudValueBP
            // 
            this.nudValueBP.AutoSize = true;
            this.nudValueBP.Location = new System.Drawing.Point(63, 29);
            this.nudValueBP.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.nudValueBP.Name = "nudValueBP";
            this.nudValueBP.Size = new System.Drawing.Size(41, 20);
            this.nudValueBP.TabIndex = 17;
            this.nudValueBP.TextChanged += new System.EventHandler(this.KarmaFilter);
            this.nudValueBP.ValueChanged += new System.EventHandler(this.KarmaFilter);
            // 
            // nudMaximumBP
            // 
            this.nudMaximumBP.AutoSize = true;
            this.nudMaximumBP.Location = new System.Drawing.Point(63, 55);
            this.nudMaximumBP.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.nudMaximumBP.Name = "nudMaximumBP";
            this.nudMaximumBP.Size = new System.Drawing.Size(41, 20);
            this.nudMaximumBP.TabIndex = 18;
            this.nudMaximumBP.TextChanged += new System.EventHandler(this.KarmaFilter);
            this.nudMaximumBP.ValueChanged += new System.EventHandler(this.KarmaFilter);
            // 
            // lblMinimumBP
            // 
            this.lblMinimumBP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMinimumBP.AutoSize = true;
            this.lblMinimumBP.Location = new System.Drawing.Point(6, 6);
            this.lblMinimumBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMinimumBP.Name = "lblMinimumBP";
            this.lblMinimumBP.Size = new System.Drawing.Size(51, 13);
            this.lblMinimumBP.TabIndex = 19;
            this.lblMinimumBP.Tag = "Label_CreateImprovementMinimum";
            this.lblMinimumBP.Text = "Minimum:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 32);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 13);
            this.label2.TabIndex = 20;
            this.label2.Tag = "Label_CreateImprovementExactly";
            this.label2.Text = "Exactly:";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 58);
            this.label3.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 13);
            this.label3.TabIndex = 21;
            this.label3.Tag = "Label_CreateImprovementMaximum";
            this.label3.Text = "Maximum:";
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 3;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.txtSearch, 2, 0);
            this.tlpMain.Controls.Add(this.lblSearchLabel, 1, 0);
            this.tlpMain.Controls.Add(this.lblBPLabel, 1, 1);
            this.tlpMain.Controls.Add(this.lblSourceLabel, 1, 2);
            this.tlpMain.Controls.Add(this.lblBP, 2, 1);
            this.tlpMain.Controls.Add(this.lblSource, 2, 2);
            this.tlpMain.Controls.Add(this.flowLayoutPanel1, 1, 4);
            this.tlpMain.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tlpMain.Controls.Add(this.flowLayoutPanel2, 1, 3);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 5;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(606, 423);
            this.tlpMain.TabIndex = 23;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.flowLayoutPanel1, 2);
            this.flowLayoutPanel1.Controls.Add(this.cmdOK);
            this.flowLayoutPanel1.Controls.Add(this.cmdOKAdd);
            this.flowLayoutPanel1.Controls.Add(this.cmdCancel);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(363, 394);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(243, 29);
            this.flowLayoutPanel1.TabIndex = 23;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.lblCategory, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.cboCategory, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.lstQualities, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tlpMain.SetRowSpan(this.tableLayoutPanel2, 5);
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(301, 423);
            this.tableLayoutPanel2.TabIndex = 24;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoScroll = true;
            this.flowLayoutPanel2.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.flowLayoutPanel2, 2);
            this.flowLayoutPanel2.Controls.Add(this.gpbKarmaFilter);
            this.flowLayoutPanel2.Controls.Add(this.chkLimitList);
            this.flowLayoutPanel2.Controls.Add(this.chkFree);
            this.flowLayoutPanel2.Controls.Add(this.chkMetagenic);
            this.flowLayoutPanel2.Controls.Add(this.chkNotMetagenic);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(301, 76);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(305, 318);
            this.flowLayoutPanel2.TabIndex = 26;
            this.flowLayoutPanel2.WrapContents = false;
            // 
            // gpbKarmaFilter
            // 
            this.gpbKarmaFilter.AutoSize = true;
            this.gpbKarmaFilter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbKarmaFilter.Controls.Add(this.tlpKarmaFilter);
            this.gpbKarmaFilter.Location = new System.Drawing.Point(3, 3);
            this.gpbKarmaFilter.Name = "gpbKarmaFilter";
            this.gpbKarmaFilter.Size = new System.Drawing.Size(113, 97);
            this.gpbKarmaFilter.TabIndex = 25;
            this.gpbKarmaFilter.TabStop = false;
            this.gpbKarmaFilter.Tag = "Label_FilterByKarma";
            this.gpbKarmaFilter.Text = "Filter by Karma:";
            // 
            // tlpKarmaFilter
            // 
            this.tlpKarmaFilter.AutoSize = true;
            this.tlpKarmaFilter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpKarmaFilter.ColumnCount = 2;
            this.tlpKarmaFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpKarmaFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpKarmaFilter.Controls.Add(this.lblMinimumBP, 0, 0);
            this.tlpKarmaFilter.Controls.Add(this.label2, 0, 1);
            this.tlpKarmaFilter.Controls.Add(this.label3, 0, 2);
            this.tlpKarmaFilter.Controls.Add(this.nudMinimumBP, 1, 0);
            this.tlpKarmaFilter.Controls.Add(this.nudValueBP, 1, 1);
            this.tlpKarmaFilter.Controls.Add(this.nudMaximumBP, 1, 2);
            this.tlpKarmaFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpKarmaFilter.Location = new System.Drawing.Point(3, 16);
            this.tlpKarmaFilter.Name = "tlpKarmaFilter";
            this.tlpKarmaFilter.RowCount = 3;
            this.tlpKarmaFilter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaFilter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaFilter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaFilter.Size = new System.Drawing.Size(107, 78);
            this.tlpKarmaFilter.TabIndex = 0;
            // 
            // chkLimitList
            // 
            this.chkLimitList.AutoSize = true;
            this.chkLimitList.Checked = true;
            this.chkLimitList.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLimitList.Location = new System.Drawing.Point(3, 107);
            this.chkLimitList.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkLimitList.Name = "chkLimitList";
            this.chkLimitList.Size = new System.Drawing.Size(169, 17);
            this.chkLimitList.TabIndex = 6;
            this.chkLimitList.Tag = "Checkbox_SelectQuality_LimitList";
            this.chkLimitList.Text = "Show only Qualities I can take";
            this.chkLimitList.UseVisualStyleBackColor = true;
            this.chkLimitList.CheckedChanged += new System.EventHandler(this.chkLimitList_CheckedChanged);
            // 
            // frmSelectQuality
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.tlpMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectQuality";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectQuality";
            this.Text = "Select a Quality";
            this.Load += new System.EventHandler(this.frmSelectQuality_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudMinimumBP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudValueBP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaximumBP)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.gpbKarmaFilter.ResumeLayout(false);
            this.gpbKarmaFilter.PerformLayout();
            this.tlpKarmaFilter.ResumeLayout(false);
            this.tlpKarmaFilter.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstQualities;
        private System.Windows.Forms.Label lblCategory;
        private ElasticComboBox cboCategory;
        private Chummer.LabelWithToolTip lblSource;
        private System.Windows.Forms.Label lblSourceLabel;
        private System.Windows.Forms.Button cmdOKAdd;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private Chummer.LabelWithToolTip lblBP;
        private System.Windows.Forms.Label lblBPLabel;
        private System.Windows.Forms.CheckBox chkFree;
        private System.Windows.Forms.CheckBox chkMetagenic;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearchLabel;
        private System.Windows.Forms.CheckBox chkNotMetagenic;
        private System.Windows.Forms.NumericUpDown nudMinimumBP;
        private System.Windows.Forms.NumericUpDown nudValueBP;
        private System.Windows.Forms.NumericUpDown nudMaximumBP;
        private System.Windows.Forms.Label lblMinimumBP;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.CheckBox chkLimitList;
        private System.Windows.Forms.GroupBox gpbKarmaFilter;
        private System.Windows.Forms.TableLayoutPanel tlpKarmaFilter;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
    }
}
