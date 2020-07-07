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
            this.chkIgnoreRules = new System.Windows.Forms.CheckBox();
            this.lblMaxAvailLabel = new System.Windows.Forms.Label();
            this.cboCharacterOption = new Chummer.ElasticComboBox();
            this.lblKarmaLabel = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblMaxNuyenLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdEditCharacterOption = new Chummer.ButtonWithToolTip();
            this.lblCharacterOption = new System.Windows.Forms.Label();
            this.tlpSummary = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblBuildMethodLabel = new System.Windows.Forms.Label();
            this.lblBuildMethod = new System.Windows.Forms.Label();
            this.lblBuildMethodParamLabel = new System.Windows.Forms.Label();
            this.lblBuildMethodParam = new System.Windows.Forms.Label();
            this.lblQualityKarmaLabel = new System.Windows.Forms.Label();
            this.lblQualityKarma = new System.Windows.Forms.Label();
            this.lblBooksLabel = new System.Windows.Forms.Label();
            this.lblCustomDataLabel = new System.Windows.Forms.Label();
            this.lblBooks = new System.Windows.Forms.Label();
            this.lblCustomData = new System.Windows.Forms.Label();
            this.lblMaxAvail = new System.Windows.Forms.Label();
            this.lblKarma = new System.Windows.Forms.Label();
            this.lblMaxNuyen = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tlpSummary.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkIgnoreRules
            // 
            this.chkIgnoreRules.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkIgnoreRules.AutoSize = true;
            this.chkIgnoreRules.CheckAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.tableLayoutPanel1.SetColumnSpan(this.chkIgnoreRules, 2);
            this.chkIgnoreRules.Location = new System.Drawing.Point(3, 162);
            this.chkIgnoreRules.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkIgnoreRules.Name = "chkIgnoreRules";
            this.chkIgnoreRules.Size = new System.Drawing.Size(177, 17);
            this.chkIgnoreRules.TabIndex = 5;
            this.chkIgnoreRules.Tag = "Checkbox_SelectBP_IgnoreRules";
            this.chkIgnoreRules.Text = "Ignore Character Creation Rules";
            this.chkIgnoreRules.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.chkIgnoreRules.UseVisualStyleBackColor = true;
            // 
            // lblMaxAvailLabel
            // 
            this.lblMaxAvailLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMaxAvailLabel.AutoSize = true;
            this.lblMaxAvailLabel.Location = new System.Drawing.Point(45, 56);
            this.lblMaxAvailLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMaxAvailLabel.Name = "lblMaxAvailLabel";
            this.lblMaxAvailLabel.Size = new System.Drawing.Size(56, 26);
            this.lblMaxAvailLabel.TabIndex = 3;
            this.lblMaxAvailLabel.Tag = "Label_SelectBP_MaxAvail";
            this.lblMaxAvailLabel.Text = "Maximum Availability";
            this.lblMaxAvailLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboCharacterOption
            // 
            this.cboCharacterOption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.cboCharacterOption, 2);
            this.cboCharacterOption.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCharacterOption.FormattingEnabled = true;
            this.cboCharacterOption.Location = new System.Drawing.Point(122, 4);
            this.cboCharacterOption.Name = "cboCharacterOption";
            this.cboCharacterOption.Size = new System.Drawing.Size(258, 21);
            this.cboCharacterOption.TabIndex = 8;
            this.cboCharacterOption.TooltipText = "";
            this.cboCharacterOption.SelectedIndexChanged += new System.EventHandler(this.cboGamePlay_SelectedIndexChanged);
            // 
            // lblKarmaLabel
            // 
            this.lblKarmaLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaLabel.AutoSize = true;
            this.lblKarmaLabel.Location = new System.Drawing.Point(25, 31);
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
            this.lblDescription.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblDescription, 4);
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
            this.lblMaxNuyenLabel.Location = new System.Drawing.Point(215, 62);
            this.lblMaxNuyenLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMaxNuyenLabel.Name = "lblMaxNuyenLabel";
            this.lblMaxNuyenLabel.Size = new System.Drawing.Size(94, 13);
            this.lblMaxNuyenLabel.TabIndex = 15;
            this.lblMaxNuyenLabel.Tag = "Label_SelectBP_MaxNuyen";
            this.lblMaxNuyenLabel.Text = "Nuyen Karma Max";
            this.lblMaxNuyenLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.chkIgnoreRules, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.lblDescription, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 2, 6);
            this.tableLayoutPanel1.Controls.Add(this.cboCharacterOption, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.cmdEditCharacterOption, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblCharacterOption, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tlpSummary, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(9, 9);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 7;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(446, 183);
            this.tableLayoutPanel1.TabIndex = 16;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 2);
            this.flowLayoutPanel1.Controls.Add(this.cmdOK);
            this.flowLayoutPanel1.Controls.Add(this.cmdCancel);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(287, 157);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(156, 23);
            this.flowLayoutPanel1.TabIndex = 16;
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.AutoSize = true;
            this.cmdOK.Location = new System.Drawing.Point(81, 0);
            this.cmdOK.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 6;
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
            this.cmdCancel.TabIndex = 7;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdEditCharacterOption
            // 
            this.cmdEditCharacterOption.AutoSize = true;
            this.cmdEditCharacterOption.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdEditCharacterOption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdEditCharacterOption.Location = new System.Drawing.Point(386, 3);
            this.cmdEditCharacterOption.Name = "cmdEditCharacterOption";
            this.cmdEditCharacterOption.Size = new System.Drawing.Size(57, 23);
            this.cmdEditCharacterOption.TabIndex = 17;
            this.cmdEditCharacterOption.Text = "Modify...";
            this.cmdEditCharacterOption.ToolTipText = "";
            this.cmdEditCharacterOption.UseVisualStyleBackColor = true;
            this.cmdEditCharacterOption.Click += new System.EventHandler(this.cmdEditCharacterOption_Click);
            // 
            // lblCharacterOption
            // 
            this.lblCharacterOption.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCharacterOption.AutoSize = true;
            this.lblCharacterOption.Location = new System.Drawing.Point(3, 8);
            this.lblCharacterOption.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCharacterOption.Name = "lblCharacterOption";
            this.lblCharacterOption.Size = new System.Drawing.Size(113, 13);
            this.lblCharacterOption.TabIndex = 18;
            this.lblCharacterOption.Text = "Use Gameplay Option:";
            // 
            // tlpSummary
            // 
            this.tlpSummary.AutoScroll = true;
            this.tlpSummary.AutoSize = true;
            this.tlpSummary.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpSummary.ColumnCount = 4;
            this.tableLayoutPanel1.SetColumnSpan(this.tlpSummary, 4);
            this.tlpSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpSummary.Controls.Add(this.lblBuildMethodLabel, 0, 0);
            this.tlpSummary.Controls.Add(this.lblBuildMethod, 1, 0);
            this.tlpSummary.Controls.Add(this.lblBuildMethodParamLabel, 2, 0);
            this.tlpSummary.Controls.Add(this.lblBuildMethodParam, 3, 0);
            this.tlpSummary.Controls.Add(this.lblBooksLabel, 0, 3);
            this.tlpSummary.Controls.Add(this.lblCustomDataLabel, 2, 3);
            this.tlpSummary.Controls.Add(this.lblBooks, 0, 4);
            this.tlpSummary.Controls.Add(this.lblCustomData, 2, 4);
            this.tlpSummary.Controls.Add(this.lblKarmaLabel, 0, 1);
            this.tlpSummary.Controls.Add(this.lblMaxAvailLabel, 0, 2);
            this.tlpSummary.Controls.Add(this.lblMaxAvail, 1, 2);
            this.tlpSummary.Controls.Add(this.lblKarma, 1, 1);
            this.tlpSummary.Controls.Add(this.lblQualityKarmaLabel, 2, 1);
            this.tlpSummary.Controls.Add(this.lblMaxNuyenLabel, 2, 2);
            this.tlpSummary.Controls.Add(this.lblQualityKarma, 3, 1);
            this.tlpSummary.Controls.Add(this.lblMaxNuyen, 3, 2);
            this.tlpSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSummary.Location = new System.Drawing.Point(0, 54);
            this.tlpSummary.Margin = new System.Windows.Forms.Padding(0);
            this.tlpSummary.Name = "tlpSummary";
            this.tlpSummary.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.tlpSummary.RowCount = 5;
            this.tlpSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSummary.Size = new System.Drawing.Size(446, 100);
            this.tlpSummary.TabIndex = 19;
            // 
            // lblBuildMethodLabel
            // 
            this.lblBuildMethodLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblBuildMethodLabel.AutoSize = true;
            this.lblBuildMethodLabel.Location = new System.Drawing.Point(32, 6);
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
            this.lblBuildMethod.Location = new System.Drawing.Point(107, 6);
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
            this.lblBuildMethodParamLabel.Location = new System.Drawing.Point(247, 6);
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
            this.lblBuildMethodParam.AutoSize = true;
            this.lblBuildMethodParam.Location = new System.Drawing.Point(315, 6);
            this.lblBuildMethodParam.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildMethodParam.Name = "lblBuildMethodParam";
            this.lblBuildMethodParam.Size = new System.Drawing.Size(61, 13);
            this.lblBuildMethodParam.TabIndex = 18;
            this.lblBuildMethodParam.Text = "[Parameter]";
            // 
            // lblQualityKarmaLabel
            // 
            this.lblQualityKarmaLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblQualityKarmaLabel.AutoSize = true;
            this.lblQualityKarmaLabel.Location = new System.Drawing.Point(213, 31);
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
            this.lblQualityKarma.Location = new System.Drawing.Point(315, 31);
            this.lblQualityKarma.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblQualityKarma.Name = "lblQualityKarma";
            this.lblQualityKarma.Size = new System.Drawing.Size(43, 13);
            this.lblQualityKarma.TabIndex = 23;
            this.lblQualityKarma.Text = "[Karma]";
            // 
            // lblBooksLabel
            // 
            this.lblBooksLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.lblBooksLabel.AutoSize = true;
            this.tlpSummary.SetColumnSpan(this.lblBooksLabel, 2);
            this.lblBooksLabel.Location = new System.Drawing.Point(64, 94);
            this.lblBooksLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBooksLabel.Name = "lblBooksLabel";
            this.lblBooksLabel.Size = new System.Drawing.Size(79, 13);
            this.lblBooksLabel.TabIndex = 24;
            this.lblBooksLabel.Text = "Enabled Books";
            // 
            // lblCustomDataLabel
            // 
            this.lblCustomDataLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.lblCustomDataLabel.AutoSize = true;
            this.tlpSummary.SetColumnSpan(this.lblCustomDataLabel, 2);
            this.lblCustomDataLabel.Location = new System.Drawing.Point(279, 94);
            this.lblCustomDataLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCustomDataLabel.Name = "lblCustomDataLabel";
            this.lblCustomDataLabel.Size = new System.Drawing.Size(68, 13);
            this.lblCustomDataLabel.TabIndex = 25;
            this.lblCustomDataLabel.Text = "Custom Data";
            // 
            // lblBooks
            // 
            this.lblBooks.AutoSize = true;
            this.tlpSummary.SetColumnSpan(this.lblBooks, 2);
            this.lblBooks.Location = new System.Drawing.Point(3, 119);
            this.lblBooks.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBooks.Name = "lblBooks";
            this.lblBooks.Size = new System.Drawing.Size(43, 13);
            this.lblBooks.TabIndex = 26;
            this.lblBooks.Text = "[Books]";
            // 
            // lblCustomData
            // 
            this.lblCustomData.AutoSize = true;
            this.tlpSummary.SetColumnSpan(this.lblCustomData, 2);
            this.lblCustomData.Location = new System.Drawing.Point(211, 119);
            this.lblCustomData.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCustomData.Name = "lblCustomData";
            this.lblCustomData.Size = new System.Drawing.Size(110, 13);
            this.lblCustomData.TabIndex = 27;
            this.lblCustomData.Text = "[Custom Data Names]";
            // 
            // lblMaxAvail
            // 
            this.lblMaxAvail.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMaxAvail.AutoSize = true;
            this.lblMaxAvail.Location = new System.Drawing.Point(107, 62);
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
            this.lblKarma.Location = new System.Drawing.Point(107, 31);
            this.lblKarma.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarma.Name = "lblKarma";
            this.lblKarma.Size = new System.Drawing.Size(43, 13);
            this.lblKarma.TabIndex = 21;
            this.lblKarma.Text = "[Karma]";
            this.lblKarma.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMaxNuyen
            // 
            this.lblMaxNuyen.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMaxNuyen.AutoSize = true;
            this.lblMaxNuyen.Location = new System.Drawing.Point(315, 62);
            this.lblMaxNuyen.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMaxNuyen.Name = "lblMaxNuyen";
            this.lblMaxNuyen.Size = new System.Drawing.Size(43, 13);
            this.lblMaxNuyen.TabIndex = 20;
            this.lblMaxNuyen.Text = "[Karma]";
            this.lblMaxNuyen.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // frmSelectBuildMethod
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(464, 201);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(480, 10000);
            this.MinimizeBox = false;
            this.Name = "frmSelectBuildMethod";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectBP";
            this.Text = "Select Build Method";
            this.Load += new System.EventHandler(this.frmSelectBuildMethod_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tlpSummary.ResumeLayout(false);
            this.tlpSummary.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.CheckBox chkIgnoreRules;
        private System.Windows.Forms.Label lblMaxAvailLabel;
        private ElasticComboBox cboCharacterOption;
        private System.Windows.Forms.Label lblKarmaLabel;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblMaxNuyenLabel;
        private Chummer.BufferedTableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblBuildMethodParamLabel;
        private ButtonWithToolTip cmdEditCharacterOption;
        private System.Windows.Forms.Label lblCharacterOption;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private BufferedTableLayoutPanel tlpSummary;
        private System.Windows.Forms.Label lblBuildMethodLabel;
        private System.Windows.Forms.Label lblBuildMethod;
        private System.Windows.Forms.Label lblBuildMethodParam;
        private System.Windows.Forms.Label lblMaxAvail;
        private System.Windows.Forms.Label lblMaxNuyen;
        private System.Windows.Forms.Label lblKarma;
        private System.Windows.Forms.Label lblQualityKarmaLabel;
        private System.Windows.Forms.Label lblQualityKarma;
        private System.Windows.Forms.Label lblBooksLabel;
        private System.Windows.Forms.Label lblCustomDataLabel;
        private System.Windows.Forms.Label lblBooks;
        private System.Windows.Forms.Label lblCustomData;
    }
}
