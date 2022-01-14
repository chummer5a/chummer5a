namespace Chummer
{
    public sealed partial class frmSelectBuildMethod
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
            this.chkIgnoreRules = new Chummer.ColorableCheckBox(this.components);
            this.lblMaxAvailLabel = new System.Windows.Forms.Label();
            this.cboCharacterSetting = new Chummer.ElasticComboBox();
            this.lblKarmaLabel = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblMaxNuyenLabel = new System.Windows.Forms.Label();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdEditCharacterSetting = new Chummer.ButtonWithToolTip(this.components);
            this.lblCharacterSetting = new System.Windows.Forms.Label();
            this.tlpSummary = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblBuildMethodLabel = new System.Windows.Forms.Label();
            this.lblBuildMethod = new System.Windows.Forms.Label();
            this.lblBuildMethodParamLabel = new System.Windows.Forms.Label();
            this.lblBuildMethodParam = new System.Windows.Forms.Label();
            this.lblMaxAvail = new System.Windows.Forms.Label();
            this.lblKarma = new System.Windows.Forms.Label();
            this.lblQualityKarmaLabel = new System.Windows.Forms.Label();
            this.lblQualityKarma = new System.Windows.Forms.Label();
            this.lblMaxNuyen = new System.Windows.Forms.Label();
            this.gpbBooks = new System.Windows.Forms.GroupBox();
            this.pnlBooks = new System.Windows.Forms.Panel();
            this.lblBooks = new System.Windows.Forms.Label();
            this.gpbCustomData = new System.Windows.Forms.GroupBox();
            this.pnlCustomData = new System.Windows.Forms.Panel();
            this.lblCustomData = new System.Windows.Forms.Label();
            this.tlpButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.cboBuildMethod = new Chummer.ElasticComboBox();
            this.nudMaxAvail = new Chummer.NumericUpDownEx();
            this.cboGamePlay = new Chummer.ElasticComboBox();
            this.lblStartingKarma = new System.Windows.Forms.Label();
            this.tlpMain.SuspendLayout();
            this.tlpSummary.SuspendLayout();
            this.gpbBooks.SuspendLayout();
            this.pnlBooks.SuspendLayout();
            this.gpbCustomData.SuspendLayout();
            this.pnlCustomData.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxAvail)).BeginInit();
            this.SuspendLayout();
            // 
            // chkIgnoreRules
            // 
            this.chkIgnoreRules.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkIgnoreRules.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.chkIgnoreRules, 2);
            this.chkIgnoreRules.DefaultColorScheme = true;
            this.chkIgnoreRules.Location = new System.Drawing.Point(3, 402);
            this.chkIgnoreRules.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkIgnoreRules.Name = "chkIgnoreRules";
            this.chkIgnoreRules.Size = new System.Drawing.Size(177, 17);
            this.chkIgnoreRules.TabIndex = 5;
            this.chkIgnoreRules.Tag = "Checkbox_SelectBP_IgnoreRules";
            this.chkIgnoreRules.Text = "Ignore Character Creation Rules";
            this.chkIgnoreRules.UseVisualStyleBackColor = true;
            // 
            // lblMaxAvailLabel
            // 
            this.lblMaxAvailLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMaxAvailLabel.AutoSize = true;
            this.lblMaxAvailLabel.Location = new System.Drawing.Point(45, 56);
            this.lblMaxAvailLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMaxAvailLabel.Name = "lblMaxAvailLabel";
            this.lblMaxAvailLabel.Size = new System.Drawing.Size(103, 13);
            this.lblMaxAvailLabel.TabIndex = 3;
            this.lblMaxAvailLabel.Tag = "Label_SelectBP_MaxAvail";
            this.lblMaxAvailLabel.Text = "Maximum Availability";
            this.lblMaxAvailLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboCharacterSetting
            // 
            this.cboCharacterSetting.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMain.SetColumnSpan(this.cboCharacterSetting, 2);
            this.cboCharacterSetting.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCharacterSetting.FormattingEnabled = true;
            this.cboCharacterSetting.Location = new System.Drawing.Point(74, 4);
            this.cboCharacterSetting.Name = "cboCharacterSetting";
            this.cboCharacterSetting.Size = new System.Drawing.Size(466, 21);
            this.cboCharacterSetting.TabIndex = 8;
            this.cboCharacterSetting.TooltipText = "";
            this.cboCharacterSetting.SelectedIndexChanged += new System.EventHandler(this.cboGamePlay_SelectedIndexChanged);
            // 
            // lblKarmaLabel
            // 
            this.lblKarmaLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaLabel.AutoSize = true;
            this.lblKarmaLabel.Location = new System.Drawing.Point(72, 31);
            this.lblKarmaLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaLabel.Name = "lblKarmaLabel";
            this.lblKarmaLabel.Size = new System.Drawing.Size(76, 13);
            this.lblKarmaLabel.TabIndex = 11;
            this.lblKarmaLabel.Tag = "Label_SelectBP_StartingKarma";
            this.lblKarmaLabel.Text = "Starting Karma";
            this.lblKarmaLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblDescription
            // 
            this.lblDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblDescription.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.lblDescription, 4);
            this.lblDescription.Location = new System.Drawing.Point(3, 35);
            this.lblDescription.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(218, 13);
            this.lblDescription.TabIndex = 0;
            this.lblDescription.Tag = "String_SelectBP_Summary";
            this.lblDescription.Text = "Brief Summary of Selected Gameplay Option:";
            // 
            // lblMaxNuyenLabel
            // 
            this.lblMaxNuyenLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMaxNuyenLabel.AutoSize = true;
            this.lblMaxNuyenLabel.Location = new System.Drawing.Point(356, 56);
            this.lblMaxNuyenLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMaxNuyenLabel.Name = "lblMaxNuyenLabel";
            this.lblMaxNuyenLabel.Size = new System.Drawing.Size(94, 13);
            this.lblMaxNuyenLabel.TabIndex = 15;
            this.lblMaxNuyenLabel.Tag = "Label_SelectBP_MaxNuyen";
            this.lblMaxNuyenLabel.Text = "Nuyen Karma Max";
            this.lblMaxNuyenLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 4;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.chkIgnoreRules, 0, 6);
            this.tlpMain.Controls.Add(this.lblDescription, 0, 1);
            this.tlpMain.Controls.Add(this.cboCharacterSetting, 1, 0);
            this.tlpMain.Controls.Add(this.cmdEditCharacterSetting, 3, 0);
            this.tlpMain.Controls.Add(this.lblCharacterSetting, 0, 0);
            this.tlpMain.Controls.Add(this.tlpSummary, 0, 2);
            this.tlpMain.Controls.Add(this.tlpButtons, 2, 6);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 4;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(606, 423);
            this.tlpMain.TabIndex = 16;
            // 
            // cmdEditCharacterSetting
            // 
            this.cmdEditCharacterSetting.AutoSize = true;
            this.cmdEditCharacterSetting.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdEditCharacterSetting.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdEditCharacterSetting.Image = null;
            this.cmdEditCharacterSetting.ImageDpi120 = null;
            this.cmdEditCharacterSetting.ImageDpi144 = null;
            this.cmdEditCharacterSetting.ImageDpi192 = null;
            this.cmdEditCharacterSetting.ImageDpi288 = null;
            this.cmdEditCharacterSetting.ImageDpi384 = null;
            this.cmdEditCharacterSetting.ImageDpi96 = null;
            this.cmdEditCharacterSetting.Location = new System.Drawing.Point(546, 3);
            this.cmdEditCharacterSetting.Name = "cmdEditCharacterSetting";
            this.cmdEditCharacterSetting.Size = new System.Drawing.Size(57, 23);
            this.cmdEditCharacterSetting.TabIndex = 17;
            this.cmdEditCharacterSetting.Text = "Modify...";
            this.cmdEditCharacterSetting.ToolTipText = "";
            this.cmdEditCharacterSetting.UseVisualStyleBackColor = true;
            this.cmdEditCharacterSetting.Click += new System.EventHandler(this.cmdEditCharacterOption_Click);
            // 
            // lblCharacterSetting
            // 
            this.lblCharacterSetting.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCharacterSetting.AutoSize = true;
            this.lblCharacterSetting.Location = new System.Drawing.Point(3, 8);
            this.lblCharacterSetting.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCharacterSetting.Name = "lblCharacterSetting";
            this.lblCharacterSetting.Size = new System.Drawing.Size(65, 13);
            this.lblCharacterSetting.TabIndex = 18;
            this.lblCharacterSetting.Tag = "Label_SelectBP_UseSetting";
            this.lblCharacterSetting.Text = "Use Setting:";
            // 
            // tlpSummary
            // 
            this.tlpSummary.AutoSize = true;
            this.tlpSummary.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpSummary.ColumnCount = 4;
            this.tlpMain.SetColumnSpan(this.tlpSummary, 4);
            this.tlpSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpSummary.Controls.Add(this.lblBuildMethodLabel, 0, 0);
            this.tlpSummary.Controls.Add(this.lblBuildMethod, 1, 0);
            this.tlpSummary.Controls.Add(this.lblBuildMethodParamLabel, 2, 0);
            this.tlpSummary.Controls.Add(this.lblBuildMethodParam, 3, 0);
            this.tlpSummary.Controls.Add(this.lblKarmaLabel, 0, 1);
            this.tlpSummary.Controls.Add(this.lblMaxAvailLabel, 0, 2);
            this.tlpSummary.Controls.Add(this.lblMaxAvail, 1, 2);
            this.tlpSummary.Controls.Add(this.lblKarma, 1, 1);
            this.tlpSummary.Controls.Add(this.lblQualityKarmaLabel, 2, 1);
            this.tlpSummary.Controls.Add(this.lblMaxNuyenLabel, 2, 2);
            this.tlpSummary.Controls.Add(this.lblQualityKarma, 3, 1);
            this.tlpSummary.Controls.Add(this.lblMaxNuyen, 3, 2);
            this.tlpSummary.Controls.Add(this.gpbBooks, 0, 3);
            this.tlpSummary.Controls.Add(this.gpbCustomData, 2, 3);
            this.tlpSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSummary.Location = new System.Drawing.Point(0, 54);
            this.tlpSummary.Margin = new System.Windows.Forms.Padding(0);
            this.tlpSummary.Name = "tlpSummary";
            this.tlpSummary.RowCount = 4;
            this.tlpSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSummary.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpSummary.Size = new System.Drawing.Size(606, 340);
            this.tlpSummary.TabIndex = 19;
            // 
            // lblBuildMethodLabel
            // 
            this.lblBuildMethodLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblBuildMethodLabel.AutoSize = true;
            this.lblBuildMethodLabel.Location = new System.Drawing.Point(79, 6);
            this.lblBuildMethodLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildMethodLabel.Name = "lblBuildMethodLabel";
            this.lblBuildMethodLabel.Size = new System.Drawing.Size(69, 13);
            this.lblBuildMethodLabel.TabIndex = 16;
            this.lblBuildMethodLabel.Tag = "Label_SelectBP_BuildMethod";
            this.lblBuildMethodLabel.Text = "Build Method";
            // 
            // lblBuildMethod
            // 
            this.lblBuildMethod.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblBuildMethod.AutoSize = true;
            this.lblBuildMethod.Location = new System.Drawing.Point(154, 6);
            this.lblBuildMethod.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildMethod.Name = "lblBuildMethod";
            this.lblBuildMethod.Size = new System.Drawing.Size(75, 13);
            this.lblBuildMethod.TabIndex = 17;
            this.lblBuildMethod.Text = "[Build Method]";
            // 
            // lblBuildMethodParamLabel
            // 
            this.lblBuildMethodParamLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblBuildMethodParamLabel.AutoSize = true;
            this.lblBuildMethodParamLabel.Location = new System.Drawing.Point(388, 6);
            this.lblBuildMethodParamLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildMethodParamLabel.Name = "lblBuildMethodParamLabel";
            this.lblBuildMethodParamLabel.Size = new System.Drawing.Size(62, 13);
            this.lblBuildMethodParamLabel.TabIndex = 10;
            this.lblBuildMethodParamLabel.Tag = "Label_SelectBP_SumToX";
            this.lblBuildMethodParamLabel.Text = "Sum to Ten";
            this.lblBuildMethodParamLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblBuildMethodParam
            // 
            this.lblBuildMethodParam.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblBuildMethodParam.AutoSize = true;
            this.lblBuildMethodParam.Location = new System.Drawing.Point(456, 6);
            this.lblBuildMethodParam.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildMethodParam.Name = "lblBuildMethodParam";
            this.lblBuildMethodParam.Size = new System.Drawing.Size(61, 13);
            this.lblBuildMethodParam.TabIndex = 18;
            this.lblBuildMethodParam.Text = "[Parameter]";
            // 
            // lblMaxAvail
            // 
            this.lblMaxAvail.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMaxAvail.AutoSize = true;
            this.lblMaxAvail.Location = new System.Drawing.Point(154, 56);
            this.lblMaxAvail.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMaxAvail.Name = "lblMaxAvail";
            this.lblMaxAvail.Size = new System.Drawing.Size(36, 13);
            this.lblMaxAvail.TabIndex = 19;
            this.lblMaxAvail.Text = "[Avail]";
            this.lblMaxAvail.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKarma
            // 
            this.lblKarma.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarma.AutoSize = true;
            this.lblKarma.Location = new System.Drawing.Point(154, 31);
            this.lblKarma.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarma.Name = "lblKarma";
            this.lblKarma.Size = new System.Drawing.Size(43, 13);
            this.lblKarma.TabIndex = 21;
            this.lblKarma.Text = "[Karma]";
            this.lblKarma.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblQualityKarmaLabel
            // 
            this.lblQualityKarmaLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblQualityKarmaLabel.AutoSize = true;
            this.lblQualityKarmaLabel.Location = new System.Drawing.Point(354, 31);
            this.lblQualityKarmaLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblQualityKarmaLabel.Name = "lblQualityKarmaLabel";
            this.lblQualityKarmaLabel.Size = new System.Drawing.Size(96, 13);
            this.lblQualityKarmaLabel.TabIndex = 22;
            this.lblQualityKarmaLabel.Tag = "Label_SelectBP_QualityKarmaLimit";
            this.lblQualityKarmaLabel.Text = "Quality Karma Limit";
            this.lblQualityKarmaLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblQualityKarma
            // 
            this.lblQualityKarma.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblQualityKarma.AutoSize = true;
            this.lblQualityKarma.Location = new System.Drawing.Point(456, 31);
            this.lblQualityKarma.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblQualityKarma.Name = "lblQualityKarma";
            this.lblQualityKarma.Size = new System.Drawing.Size(43, 13);
            this.lblQualityKarma.TabIndex = 23;
            this.lblQualityKarma.Text = "[Karma]";
            // 
            // lblMaxNuyen
            // 
            this.lblMaxNuyen.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMaxNuyen.AutoSize = true;
            this.lblMaxNuyen.Location = new System.Drawing.Point(456, 56);
            this.lblMaxNuyen.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMaxNuyen.Name = "lblMaxNuyen";
            this.lblMaxNuyen.Size = new System.Drawing.Size(43, 13);
            this.lblMaxNuyen.TabIndex = 20;
            this.lblMaxNuyen.Text = "[Karma]";
            this.lblMaxNuyen.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // gpbBooks
            // 
            this.gpbBooks.AutoSize = true;
            this.gpbBooks.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpSummary.SetColumnSpan(this.gpbBooks, 2);
            this.gpbBooks.Controls.Add(this.pnlBooks);
            this.gpbBooks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpbBooks.Location = new System.Drawing.Point(3, 78);
            this.gpbBooks.Name = "gpbBooks";
            this.gpbBooks.Size = new System.Drawing.Size(296, 259);
            this.gpbBooks.TabIndex = 32;
            this.gpbBooks.TabStop = false;
            this.gpbBooks.Tag = "Label_SelectBP_EnabledBooks";
            this.gpbBooks.Text = "Enabled Books";
            // 
            // pnlBooks
            // 
            this.pnlBooks.AutoScroll = true;
            this.pnlBooks.Controls.Add(this.lblBooks);
            this.pnlBooks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBooks.Location = new System.Drawing.Point(3, 16);
            this.pnlBooks.Margin = new System.Windows.Forms.Padding(0);
            this.pnlBooks.Name = "pnlBooks";
            this.pnlBooks.Padding = new System.Windows.Forms.Padding(3, 6, 13, 6);
            this.pnlBooks.Size = new System.Drawing.Size(290, 240);
            this.pnlBooks.TabIndex = 30;
            // 
            // lblBooks
            // 
            this.lblBooks.AutoSize = true;
            this.lblBooks.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblBooks.Location = new System.Drawing.Point(3, 6);
            this.lblBooks.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBooks.Name = "lblBooks";
            this.lblBooks.Size = new System.Drawing.Size(43, 13);
            this.lblBooks.TabIndex = 26;
            this.lblBooks.Text = "[Books]";
            // 
            // gpbCustomData
            // 
            this.gpbCustomData.AutoSize = true;
            this.gpbCustomData.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpSummary.SetColumnSpan(this.gpbCustomData, 2);
            this.gpbCustomData.Controls.Add(this.pnlCustomData);
            this.gpbCustomData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpbCustomData.Location = new System.Drawing.Point(305, 78);
            this.gpbCustomData.Name = "gpbCustomData";
            this.gpbCustomData.Size = new System.Drawing.Size(298, 259);
            this.gpbCustomData.TabIndex = 33;
            this.gpbCustomData.TabStop = false;
            this.gpbCustomData.Tag = "Label_SelectBP_CustomData";
            this.gpbCustomData.Text = "Custom Data";
            // 
            // pnlCustomData
            // 
            this.pnlCustomData.AutoScroll = true;
            this.pnlCustomData.Controls.Add(this.lblCustomData);
            this.pnlCustomData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCustomData.Location = new System.Drawing.Point(3, 16);
            this.pnlCustomData.Margin = new System.Windows.Forms.Padding(0);
            this.pnlCustomData.Name = "pnlCustomData";
            this.pnlCustomData.Padding = new System.Windows.Forms.Padding(3, 6, 13, 6);
            this.pnlCustomData.Size = new System.Drawing.Size(292, 240);
            this.pnlCustomData.TabIndex = 31;
            // 
            // lblCustomData
            // 
            this.lblCustomData.AutoSize = true;
            this.lblCustomData.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCustomData.Location = new System.Drawing.Point(3, 6);
            this.lblCustomData.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCustomData.Name = "lblCustomData";
            this.lblCustomData.Size = new System.Drawing.Size(110, 13);
            this.lblCustomData.TabIndex = 27;
            this.lblCustomData.Text = "[Custom Data Names]";
            // 
            // tlpButtons
            // 
            this.tlpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpButtons.AutoSize = true;
            this.tlpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpButtons.ColumnCount = 2;
            this.tlpMain.SetColumnSpan(this.tlpButtons, 2);
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.Controls.Add(this.cmdCancel, 0, 0);
            this.tlpButtons.Controls.Add(this.cmdOK, 1, 0);
            this.tlpButtons.Location = new System.Drawing.Point(494, 394);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.Size = new System.Drawing.Size(112, 29);
            this.tlpButtons.TabIndex = 20;
            // 
            // cmdCancel
            // 
            this.cmdCancel.AutoSize = true;
            this.cmdCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdCancel.Location = new System.Drawing.Point(3, 3);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(50, 23);
            this.cmdCancel.TabIndex = 7;
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
            this.cmdOK.Location = new System.Drawing.Point(59, 3);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(50, 23);
            this.cmdOK.TabIndex = 6;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cboBuildMethod
            // 
            this.cboBuildMethod.Location = new System.Drawing.Point(0, 0);
            this.cboBuildMethod.Name = "cboBuildMethod";
            this.cboBuildMethod.Size = new System.Drawing.Size(121, 21);
            this.cboBuildMethod.TabIndex = 0;
            this.cboBuildMethod.TooltipText = "";
            // 
            // nudMaxAvail
            // 
            this.nudMaxAvail.Location = new System.Drawing.Point(0, 0);
            this.nudMaxAvail.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudMaxAvail.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMaxAvail.Name = "nudMaxAvail";
            this.nudMaxAvail.Size = new System.Drawing.Size(120, 20);
            this.nudMaxAvail.TabIndex = 0;
            // 
            // cboGamePlay
            // 
            this.cboGamePlay.Location = new System.Drawing.Point(0, 0);
            this.cboGamePlay.Name = "cboGamePlay";
            this.cboGamePlay.Size = new System.Drawing.Size(121, 21);
            this.cboGamePlay.TabIndex = 0;
            this.cboGamePlay.TooltipText = "";
            // 
            // lblStartingKarma
            // 
            this.lblStartingKarma.Location = new System.Drawing.Point(0, 0);
            this.lblStartingKarma.Name = "lblStartingKarma";
            this.lblStartingKarma.Size = new System.Drawing.Size(100, 23);
            this.lblStartingKarma.TabIndex = 0;
            // 
            // frmSelectBuildMethod
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.ControlBox = false;
            this.Controls.Add(this.tlpMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectBuildMethod";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectBP";
            this.Text = "Select Build Method";
            this.Load += new System.EventHandler(this.frmSelectBuildMethod_Load);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpSummary.ResumeLayout(false);
            this.tlpSummary.PerformLayout();
            this.gpbBooks.ResumeLayout(false);
            this.pnlBooks.ResumeLayout(false);
            this.pnlBooks.PerformLayout();
            this.gpbCustomData.ResumeLayout(false);
            this.pnlCustomData.ResumeLayout(false);
            this.pnlCustomData.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxAvail)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private ColorableCheckBox chkIgnoreRules;
        private System.Windows.Forms.Label lblMaxAvailLabel;
        private ElasticComboBox cboCharacterSetting;
        private System.Windows.Forms.Label lblKarmaLabel;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblMaxNuyenLabel;
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.Label lblBuildMethodParamLabel;
        private ButtonWithToolTip cmdEditCharacterSetting;
        private System.Windows.Forms.Label lblCharacterSetting;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private ElasticComboBox cboBuildMethod;
        private BufferedTableLayoutPanel tlpSummary;
        private System.Windows.Forms.Label lblBuildMethodLabel;
        private System.Windows.Forms.Label lblBuildMethod;
        private System.Windows.Forms.Label lblBuildMethodParam;
        private System.Windows.Forms.Label lblMaxAvail;
        private NumericUpDownEx nudMaxAvail;
        private ElasticComboBox cboGamePlay;
        private System.Windows.Forms.Label lblStartingKarma;
        private System.Windows.Forms.Label lblMaxNuyen;
        private System.Windows.Forms.Label lblKarma;
        private System.Windows.Forms.Label lblQualityKarmaLabel;
        private System.Windows.Forms.Label lblQualityKarma;
        private System.Windows.Forms.Label lblBooks;
        private System.Windows.Forms.Label lblCustomData;
        private BufferedTableLayoutPanel tlpButtons;
        private System.Windows.Forms.Panel pnlBooks;
        private System.Windows.Forms.Panel pnlCustomData;
        private System.Windows.Forms.GroupBox gpbBooks;
        private System.Windows.Forms.GroupBox gpbCustomData;
    }
}
