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
            this.chkFree = new Chummer.ColorableCheckBox(this.components);
            this.chkMetagenic = new Chummer.ColorableCheckBox(this.components);
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearchLabel = new System.Windows.Forms.Label();
            this.chkNotMetagenic = new Chummer.ColorableCheckBox(this.components);
            this.nudMinimumBP = new Chummer.NumericUpDownEx();
            this.nudValueBP = new Chummer.NumericUpDownEx();
            this.nudMaximumBP = new Chummer.NumericUpDownEx();
            this.lblMinimumBP = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tableLayoutPanel2 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpLowerRight = new Chummer.BufferedTableLayoutPanel(this.components);
            this.chkLimitList = new Chummer.ColorableCheckBox(this.components);
            this.gpbKarmaFilter = new System.Windows.Forms.GroupBox();
            this.tlpKarmaFilter = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpRight = new Chummer.BufferedTableLayoutPanel(this.components);
            this.flpRating = new System.Windows.Forms.FlowLayoutPanel();
            this.nudRating = new Chummer.NumericUpDownEx();
            this.lblRatingNALabel = new System.Windows.Forms.Label();
            this.lblRatingLabel = new System.Windows.Forms.Label();
            this.bufferedTableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.nudMinimumBP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudValueBP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaximumBP)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            this.tlpLowerRight.SuspendLayout();
            this.gpbKarmaFilter.SuspendLayout();
            this.tlpKarmaFilter.SuspendLayout();
            this.tlpRight.SuspendLayout();
            this.flpRating.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRating)).BeginInit();
            this.bufferedTableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstQualities
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.lstQualities, 2);
            this.lstQualities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstQualities.FormattingEnabled = true;
            this.lstQualities.Location = new System.Drawing.Point(3, 30);
            this.lstQualities.Name = "lstQualities";
            this.lstQualities.Size = new System.Drawing.Size(297, 390);
            this.lstQualities.TabIndex = 9;
            this.lstQualities.SelectedIndexChanged += new System.EventHandler(this.lstQualities_SelectedIndexChanged);
            this.lstQualities.DoubleClick += new System.EventHandler(this.cmdOK_Click);
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
            this.cboCategory.Size = new System.Drawing.Size(239, 21);
            this.cboCategory.TabIndex = 11;
            this.cboCategory.TooltipText = "";
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // lblSource
            // 
            this.lblSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSource.AutoSize = true;
            this.lblSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSource.Location = new System.Drawing.Point(53, 82);
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
            this.lblSourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(3, 82);
            this.lblSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 4;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            // 
            // cmdOKAdd
            // 
            this.cmdOKAdd.AutoSize = true;
            this.cmdOKAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOKAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOKAdd.Location = new System.Drawing.Point(81, 3);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(72, 23);
            this.cmdOKAdd.TabIndex = 13;
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
            this.cmdCancel.TabIndex = 14;
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
            this.cmdOK.TabIndex = 12;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lblBP
            // 
            this.lblBP.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblBP.AutoSize = true;
            this.lblBP.Location = new System.Drawing.Point(53, 6);
            this.lblBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBP.Name = "lblBP";
            this.lblBP.Size = new System.Drawing.Size(27, 13);
            this.lblBP.TabIndex = 3;
            this.lblBP.Text = "[BP]";
            this.lblBP.ToolTipText = "";
            // 
            // lblBPLabel
            // 
            this.lblBPLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblBPLabel.AutoSize = true;
            this.lblBPLabel.Location = new System.Drawing.Point(7, 6);
            this.lblBPLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBPLabel.Name = "lblBPLabel";
            this.lblBPLabel.Size = new System.Drawing.Size(40, 13);
            this.lblBPLabel.TabIndex = 2;
            this.lblBPLabel.Tag = "Label_Karma";
            this.lblBPLabel.Text = "Karma:";
            // 
            // chkFree
            // 
            this.chkFree.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkFree.AutoSize = true;
            this.tlpRight.SetColumnSpan(this.chkFree, 2);
            this.chkFree.DefaultColorScheme = true;
            this.chkFree.Location = new System.Drawing.Point(3, 55);
            this.chkFree.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkFree.Name = "chkFree";
            this.chkFree.Size = new System.Drawing.Size(50, 17);
            this.chkFree.TabIndex = 8;
            this.chkFree.Tag = "Checkbox_Free";
            this.chkFree.Text = "Free!";
            this.chkFree.UseVisualStyleBackColor = true;
            this.chkFree.CheckedChanged += new System.EventHandler(this.CostControl_Changed);
            // 
            // chkMetagenic
            // 
            this.chkMetagenic.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkMetagenic.AutoSize = true;
            this.chkMetagenic.DefaultColorScheme = true;
            this.chkMetagenic.Location = new System.Drawing.Point(3, 132);
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
            // chkNotMetagenic
            // 
            this.chkNotMetagenic.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkNotMetagenic.AutoSize = true;
            this.chkNotMetagenic.DefaultColorScheme = true;
            this.chkNotMetagenic.Location = new System.Drawing.Point(3, 157);
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
            this.nudMinimumBP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudMinimumBP.AutoSize = true;
            this.nudMinimumBP.Location = new System.Drawing.Point(63, 3);
            this.nudMinimumBP.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
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
            this.nudValueBP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudValueBP.AutoSize = true;
            this.nudValueBP.Location = new System.Drawing.Point(63, 29);
            this.nudValueBP.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
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
            this.nudMaximumBP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudMaximumBP.AutoSize = true;
            this.nudMaximumBP.Location = new System.Drawing.Point(63, 55);
            this.nudMaximumBP.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
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
            this.lblMinimumBP.Anchor = System.Windows.Forms.AnchorStyles.Right;
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
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
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
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
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
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tlpMain.Controls.Add(this.tlpButtons, 1, 4);
            this.tlpMain.Controls.Add(this.tlpLowerRight, 1, 3);
            this.tlpMain.Controls.Add(this.tlpRight, 1, 1);
            this.tlpMain.Controls.Add(this.bufferedTableLayoutPanel1, 1, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 5;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(606, 423);
            this.tlpMain.TabIndex = 23;
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
            this.tableLayoutPanel2.Size = new System.Drawing.Size(303, 423);
            this.tableLayoutPanel2.TabIndex = 24;
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
            this.tlpButtons.TabIndex = 27;
            // 
            // tlpLowerRight
            // 
            this.tlpLowerRight.AutoSize = true;
            this.tlpLowerRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpLowerRight.ColumnCount = 1;
            this.tlpLowerRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLowerRight.Controls.Add(this.chkNotMetagenic, 0, 3);
            this.tlpLowerRight.Controls.Add(this.chkMetagenic, 0, 2);
            this.tlpLowerRight.Controls.Add(this.chkLimitList, 0, 1);
            this.tlpLowerRight.Controls.Add(this.gpbKarmaFilter, 0, 0);
            this.tlpLowerRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpLowerRight.Location = new System.Drawing.Point(303, 216);
            this.tlpLowerRight.Margin = new System.Windows.Forms.Padding(0);
            this.tlpLowerRight.Name = "tlpLowerRight";
            this.tlpLowerRight.RowCount = 4;
            this.tlpLowerRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLowerRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpLowerRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpLowerRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpLowerRight.Size = new System.Drawing.Size(303, 178);
            this.tlpLowerRight.TabIndex = 29;
            // 
            // chkLimitList
            // 
            this.chkLimitList.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkLimitList.AutoSize = true;
            this.chkLimitList.Checked = true;
            this.chkLimitList.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLimitList.DefaultColorScheme = true;
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
            // tlpRight
            // 
            this.tlpRight.AutoSize = true;
            this.tlpRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.ColumnCount = 2;
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRight.Controls.Add(this.flpRating, 1, 1);
            this.tlpRight.Controls.Add(this.lblRatingLabel, 0, 1);
            this.tlpRight.Controls.Add(this.lblSource, 1, 3);
            this.tlpRight.Controls.Add(this.lblSourceLabel, 0, 3);
            this.tlpRight.Controls.Add(this.lblBPLabel, 0, 0);
            this.tlpRight.Controls.Add(this.lblBP, 1, 0);
            this.tlpRight.Controls.Add(this.chkFree, 0, 2);
            this.tlpRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRight.Location = new System.Drawing.Point(303, 26);
            this.tlpRight.Margin = new System.Windows.Forms.Padding(0);
            this.tlpRight.Name = "tlpRight";
            this.tlpRight.RowCount = 4;
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.Size = new System.Drawing.Size(303, 101);
            this.tlpRight.TabIndex = 28;
            // 
            // flpRating
            // 
            this.flpRating.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpRating.AutoSize = true;
            this.flpRating.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpRating.Controls.Add(this.nudRating);
            this.flpRating.Controls.Add(this.lblRatingNALabel);
            this.flpRating.Location = new System.Drawing.Point(50, 25);
            this.flpRating.Margin = new System.Windows.Forms.Padding(0);
            this.flpRating.Name = "flpRating";
            this.flpRating.Size = new System.Drawing.Size(80, 26);
            this.flpRating.TabIndex = 68;
            this.flpRating.WrapContents = false;
            // 
            // nudRating
            // 
            this.nudRating.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudRating.AutoSize = true;
            this.nudRating.Location = new System.Drawing.Point(3, 3);
            this.nudRating.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudRating.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudRating.Name = "nudRating";
            this.nudRating.Size = new System.Drawing.Size(41, 20);
            this.nudRating.TabIndex = 11;
            this.nudRating.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudRating.ValueChanged += new System.EventHandler(this.CostControl_Changed);
            // 
            // lblRatingNALabel
            // 
            this.lblRatingNALabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblRatingNALabel.AutoSize = true;
            this.lblRatingNALabel.Location = new System.Drawing.Point(50, 6);
            this.lblRatingNALabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRatingNALabel.Name = "lblRatingNALabel";
            this.lblRatingNALabel.Size = new System.Drawing.Size(27, 13);
            this.lblRatingNALabel.TabIndex = 12;
            this.lblRatingNALabel.Tag = "String_NotApplicable";
            this.lblRatingNALabel.Text = "N/A";
            this.lblRatingNALabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblRatingNALabel.Visible = false;
            // 
            // lblRatingLabel
            // 
            this.lblRatingLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblRatingLabel.AutoSize = true;
            this.lblRatingLabel.Location = new System.Drawing.Point(6, 31);
            this.lblRatingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRatingLabel.Name = "lblRatingLabel";
            this.lblRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblRatingLabel.TabIndex = 11;
            this.lblRatingLabel.Tag = "Label_Rating";
            this.lblRatingLabel.Text = "Rating:";
            this.lblRatingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // bufferedTableLayoutPanel1
            // 
            this.bufferedTableLayoutPanel1.AutoSize = true;
            this.bufferedTableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bufferedTableLayoutPanel1.ColumnCount = 2;
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bufferedTableLayoutPanel1.Controls.Add(this.txtSearch, 1, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblSearchLabel, 0, 0);
            this.bufferedTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedTableLayoutPanel1.Location = new System.Drawing.Point(303, 0);
            this.bufferedTableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.bufferedTableLayoutPanel1.Name = "bufferedTableLayoutPanel1";
            this.bufferedTableLayoutPanel1.RowCount = 1;
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bufferedTableLayoutPanel1.Size = new System.Drawing.Size(303, 26);
            this.bufferedTableLayoutPanel1.TabIndex = 30;
            // 
            // frmSelectQuality
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
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.tlpLowerRight.ResumeLayout(false);
            this.tlpLowerRight.PerformLayout();
            this.gpbKarmaFilter.ResumeLayout(false);
            this.gpbKarmaFilter.PerformLayout();
            this.tlpKarmaFilter.ResumeLayout(false);
            this.tlpKarmaFilter.PerformLayout();
            this.tlpRight.ResumeLayout(false);
            this.tlpRight.PerformLayout();
            this.flpRating.ResumeLayout(false);
            this.flpRating.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRating)).EndInit();
            this.bufferedTableLayoutPanel1.ResumeLayout(false);
            this.bufferedTableLayoutPanel1.PerformLayout();
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
        private Chummer.ColorableCheckBox chkFree;
        private Chummer.ColorableCheckBox chkMetagenic;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearchLabel;
        private Chummer.ColorableCheckBox chkNotMetagenic;
        private Chummer.NumericUpDownEx nudMinimumBP;
        private Chummer.NumericUpDownEx nudValueBP;
        private Chummer.NumericUpDownEx nudMaximumBP;
        private System.Windows.Forms.Label lblMinimumBP;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private Chummer.ColorableCheckBox chkLimitList;
        private System.Windows.Forms.GroupBox gpbKarmaFilter;
        private BufferedTableLayoutPanel tlpButtons;
        private BufferedTableLayoutPanel tableLayoutPanel2;
        private BufferedTableLayoutPanel tlpKarmaFilter;
        private BufferedTableLayoutPanel tlpRight;
        private BufferedTableLayoutPanel tlpLowerRight;
        private BufferedTableLayoutPanel bufferedTableLayoutPanel1;
        private System.Windows.Forms.Label lblRatingLabel;
        private System.Windows.Forms.FlowLayoutPanel flpRating;
        private NumericUpDownEx nudRating;
        private System.Windows.Forms.Label lblRatingNALabel;
    }
}
