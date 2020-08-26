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
            this.nudKarma = new System.Windows.Forms.NumericUpDown();
            this.cmdOK = new System.Windows.Forms.Button();
            this.chkIgnoreRules = new System.Windows.Forms.CheckBox();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cboBuildMethod = new Chummer.ElasticComboBox();
            this.lblMaxAvail = new System.Windows.Forms.Label();
            this.nudMaxAvail = new System.Windows.Forms.NumericUpDown();
            this.cboGamePlay = new Chummer.ElasticComboBox();
            this.lblStartingKarma = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.nudMaxNuyen = new System.Windows.Forms.NumericUpDown();
            this.lblMaxNuyen = new System.Windows.Forms.Label();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.nudSumtoTen = new System.Windows.Forms.NumericUpDown();
            this.lblSumToX = new System.Windows.Forms.Label();
            this.flpButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.tlpNumericUpDowns = new Chummer.BufferedTableLayoutPanel(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxAvail)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxNuyen)).BeginInit();
            this.tlpMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSumtoTen)).BeginInit();
            this.flpButtons.SuspendLayout();
            this.tlpNumericUpDowns.SuspendLayout();
            this.SuspendLayout();
            // 
            // nudKarma
            // 
            this.nudKarma.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudKarma.AutoSize = true;
            this.nudKarma.Location = new System.Drawing.Point(114, 3);
            this.nudKarma.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nudKarma.Name = "nudKarma";
            this.nudKarma.Size = new System.Drawing.Size(53, 20);
            this.nudKarma.TabIndex = 2;
            this.nudKarma.Tag = "";
            this.nudKarma.Value = new decimal(new int[] {
            400,
            0,
            0,
            0});
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
            // chkIgnoreRules
            // 
            this.chkIgnoreRules.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkIgnoreRules.AutoSize = true;
            this.chkIgnoreRules.CheckAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.chkIgnoreRules.Location = new System.Drawing.Point(3, 120);
            this.chkIgnoreRules.Name = "chkIgnoreRules";
            this.chkIgnoreRules.Size = new System.Drawing.Size(177, 17);
            this.chkIgnoreRules.TabIndex = 5;
            this.chkIgnoreRules.Tag = "Checkbox_SelectBP_IgnoreRules";
            this.chkIgnoreRules.Text = "Ignore Character Creation Rules";
            this.chkIgnoreRules.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.chkIgnoreRules.UseVisualStyleBackColor = true;
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
            // cboBuildMethod
            // 
            this.cboBuildMethod.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboBuildMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBuildMethod.FormattingEnabled = true;
            this.cboBuildMethod.Location = new System.Drawing.Point(3, 3);
            this.cboBuildMethod.Name = "cboBuildMethod";
            this.cboBuildMethod.Size = new System.Drawing.Size(217, 21);
            this.cboBuildMethod.TabIndex = 1;
            this.cboBuildMethod.TooltipText = "";
            this.cboBuildMethod.SelectedIndexChanged += new System.EventHandler(this.cboBuildMethod_SelectedIndexChanged);
            // 
            // lblMaxAvail
            // 
            this.lblMaxAvail.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMaxAvail.AutoSize = true;
            this.lblMaxAvail.Location = new System.Drawing.Point(227, 6);
            this.lblMaxAvail.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMaxAvail.Name = "lblMaxAvail";
            this.lblMaxAvail.Size = new System.Drawing.Size(103, 13);
            this.lblMaxAvail.TabIndex = 3;
            this.lblMaxAvail.Tag = "Label_SelectBP_MaxAvail";
            this.lblMaxAvail.Text = "Maximum Availability";
            this.lblMaxAvail.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // nudMaxAvail
            // 
            this.nudMaxAvail.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudMaxAvail.AutoSize = true;
            this.nudMaxAvail.Location = new System.Drawing.Point(336, 3);
            this.nudMaxAvail.Name = "nudMaxAvail";
            this.nudMaxAvail.Size = new System.Drawing.Size(41, 20);
            this.nudMaxAvail.TabIndex = 4;
            this.nudMaxAvail.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
            // 
            // cboGamePlay
            // 
            this.cboGamePlay.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboGamePlay.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGamePlay.FormattingEnabled = true;
            this.cboGamePlay.Location = new System.Drawing.Point(226, 3);
            this.cboGamePlay.Name = "cboGamePlay";
            this.cboGamePlay.Size = new System.Drawing.Size(217, 21);
            this.cboGamePlay.TabIndex = 8;
            this.cboGamePlay.TooltipText = "";
            this.cboGamePlay.SelectedIndexChanged += new System.EventHandler(this.cboGamePlay_SelectedIndexChanged);
            // 
            // lblStartingKarma
            // 
            this.lblStartingKarma.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblStartingKarma.AutoSize = true;
            this.lblStartingKarma.Location = new System.Drawing.Point(32, 6);
            this.lblStartingKarma.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblStartingKarma.Name = "lblStartingKarma";
            this.lblStartingKarma.Size = new System.Drawing.Size(76, 13);
            this.lblStartingKarma.TabIndex = 11;
            this.lblStartingKarma.Tag = "Label_SelectBP_StartingKarma";
            this.lblStartingKarma.Text = "Starting Karma";
            this.lblStartingKarma.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.lblDescription, 2);
            this.lblDescription.Location = new System.Drawing.Point(3, 33);
            this.lblDescription.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(440, 13);
            this.lblDescription.TabIndex = 0;
            this.lblDescription.Tag = "String_SelectBP_KarmaSummary";
            this.lblDescription.Text = "Enter the amount of Build Points you are allowed to create your character with (D" +
    "efault 400).";
            // 
            // nudMaxNuyen
            // 
            this.nudMaxNuyen.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudMaxNuyen.AutoSize = true;
            this.nudMaxNuyen.Location = new System.Drawing.Point(114, 29);
            this.nudMaxNuyen.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nudMaxNuyen.Name = "nudMaxNuyen";
            this.nudMaxNuyen.Size = new System.Drawing.Size(53, 20);
            this.nudMaxNuyen.TabIndex = 13;
            this.nudMaxNuyen.Value = new decimal(new int[] {
            235,
            0,
            0,
            0});
            // 
            // lblMaxNuyen
            // 
            this.lblMaxNuyen.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMaxNuyen.AutoSize = true;
            this.lblMaxNuyen.Location = new System.Drawing.Point(14, 32);
            this.lblMaxNuyen.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMaxNuyen.Name = "lblMaxNuyen";
            this.lblMaxNuyen.Size = new System.Drawing.Size(94, 13);
            this.lblMaxNuyen.TabIndex = 15;
            this.lblMaxNuyen.Tag = "Label_SelectBP_MaxNuyen";
            this.lblMaxNuyen.Text = "Nuyen Karma Max";
            this.lblMaxNuyen.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Controls.Add(this.chkIgnoreRules, 0, 3);
            this.tlpMain.Controls.Add(this.cboGamePlay, 1, 0);
            this.tlpMain.Controls.Add(this.cboBuildMethod, 0, 0);
            this.tlpMain.Controls.Add(this.lblDescription, 0, 1);
            this.tlpMain.Controls.Add(this.flpButtons, 1, 3);
            this.tlpMain.Controls.Add(this.tlpNumericUpDowns, 0, 2);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 4;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(446, 143);
            this.tlpMain.TabIndex = 16;
            // 
            // nudSumtoTen
            // 
            this.nudSumtoTen.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudSumtoTen.AutoSize = true;
            this.nudSumtoTen.Location = new System.Drawing.Point(336, 29);
            this.nudSumtoTen.Name = "nudSumtoTen";
            this.nudSumtoTen.Size = new System.Drawing.Size(41, 20);
            this.nudSumtoTen.TabIndex = 9;
            this.nudSumtoTen.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // lblSumToX
            // 
            this.lblSumToX.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSumToX.AutoSize = true;
            this.lblSumToX.Location = new System.Drawing.Point(268, 32);
            this.lblSumToX.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSumToX.Name = "lblSumToX";
            this.lblSumToX.Size = new System.Drawing.Size(62, 13);
            this.lblSumToX.TabIndex = 10;
            this.lblSumToX.Tag = "Label_SelectBP_SumToX";
            this.lblSumToX.Text = "Sum to Ten";
            this.lblSumToX.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // flpButtons
            // 
            this.flpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flpButtons.AutoSize = true;
            this.flpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpButtons.Controls.Add(this.cmdOK);
            this.flpButtons.Controls.Add(this.cmdCancel);
            this.flpButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpButtons.Location = new System.Drawing.Point(287, 117);
            this.flpButtons.Name = "flpButtons";
            this.flpButtons.Size = new System.Drawing.Size(156, 23);
            this.flpButtons.TabIndex = 16;
            // 
            // tlpNumericUpDowns
            // 
            this.tlpNumericUpDowns.AutoSize = true;
            this.tlpNumericUpDowns.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpNumericUpDowns.ColumnCount = 4;
            this.tlpMain.SetColumnSpan(this.tlpNumericUpDowns, 2);
            this.tlpNumericUpDowns.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpNumericUpDowns.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpNumericUpDowns.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpNumericUpDowns.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpNumericUpDowns.Controls.Add(this.lblStartingKarma, 0, 0);
            this.tlpNumericUpDowns.Controls.Add(this.nudKarma, 1, 0);
            this.tlpNumericUpDowns.Controls.Add(this.lblMaxNuyen, 0, 1);
            this.tlpNumericUpDowns.Controls.Add(this.nudMaxNuyen, 1, 1);
            this.tlpNumericUpDowns.Controls.Add(this.lblMaxAvail, 2, 0);
            this.tlpNumericUpDowns.Controls.Add(this.lblSumToX, 2, 1);
            this.tlpNumericUpDowns.Controls.Add(this.nudSumtoTen, 3, 1);
            this.tlpNumericUpDowns.Controls.Add(this.nudMaxAvail, 3, 0);
            this.tlpNumericUpDowns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpNumericUpDowns.Location = new System.Drawing.Point(0, 62);
            this.tlpNumericUpDowns.Margin = new System.Windows.Forms.Padding(0);
            this.tlpNumericUpDowns.Name = "tlpNumericUpDowns";
            this.tlpNumericUpDowns.RowCount = 2;
            this.tlpNumericUpDowns.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpNumericUpDowns.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpNumericUpDowns.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpNumericUpDowns.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpNumericUpDowns.Size = new System.Drawing.Size(446, 52);
            this.tlpNumericUpDowns.TabIndex = 17;
            // 
            // frmSelectBuildMethod
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(464, 161);
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
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxAvail)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxNuyen)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSumtoTen)).EndInit();
            this.flpButtons.ResumeLayout(false);
            this.flpButtons.PerformLayout();
            this.tlpNumericUpDowns.ResumeLayout(false);
            this.tlpNumericUpDowns.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown nudKarma;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.CheckBox chkIgnoreRules;
        private System.Windows.Forms.Button cmdCancel;
        private ElasticComboBox cboBuildMethod;
        private System.Windows.Forms.Label lblMaxAvail;
        private System.Windows.Forms.NumericUpDown nudMaxAvail;
        private ElasticComboBox cboGamePlay;
        private System.Windows.Forms.Label lblStartingKarma;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.NumericUpDown nudMaxNuyen;
        private System.Windows.Forms.Label lblMaxNuyen;
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.NumericUpDown nudSumtoTen;
        private System.Windows.Forms.Label lblSumToX;
        private System.Windows.Forms.FlowLayoutPanel flpButtons;
        private BufferedTableLayoutPanel tlpNumericUpDowns;
    }
}
