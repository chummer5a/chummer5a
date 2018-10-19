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
            this.tableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.nudSumtoTen = new System.Windows.Forms.NumericUpDown();
            this.lblSumToX = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxAvail)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxNuyen)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSumtoTen)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // nudKarma
            // 
            this.nudKarma.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarma.Location = new System.Drawing.Point(225, 53);
            this.nudKarma.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nudKarma.Name = "nudKarma";
            this.nudKarma.Size = new System.Drawing.Size(105, 20);
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
            this.tableLayoutPanel1.SetColumnSpan(this.cboBuildMethod, 2);
            this.cboBuildMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBuildMethod.FormattingEnabled = true;
            this.cboBuildMethod.Location = new System.Drawing.Point(3, 3);
            this.cboBuildMethod.Name = "cboBuildMethod";
            this.cboBuildMethod.Size = new System.Drawing.Size(216, 21);
            this.cboBuildMethod.TabIndex = 1;
            this.cboBuildMethod.TooltipText = "";
            this.cboBuildMethod.SelectedIndexChanged += new System.EventHandler(this.cboBuildMethod_SelectedIndexChanged);
            // 
            // lblMaxAvail
            // 
            this.lblMaxAvail.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMaxAvail.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblMaxAvail, 2);
            this.lblMaxAvail.Location = new System.Drawing.Point(116, 82);
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
            this.nudMaxAvail.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudMaxAvail.Location = new System.Drawing.Point(225, 79);
            this.nudMaxAvail.Name = "nudMaxAvail";
            this.nudMaxAvail.Size = new System.Drawing.Size(105, 20);
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
            this.tableLayoutPanel1.SetColumnSpan(this.cboGamePlay, 2);
            this.cboGamePlay.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGamePlay.FormattingEnabled = true;
            this.cboGamePlay.Location = new System.Drawing.Point(225, 3);
            this.cboGamePlay.Name = "cboGamePlay";
            this.cboGamePlay.Size = new System.Drawing.Size(218, 21);
            this.cboGamePlay.TabIndex = 8;
            this.cboGamePlay.TooltipText = "";
            this.cboGamePlay.SelectedIndexChanged += new System.EventHandler(this.cboGamePlay_SelectedIndexChanged);
            // 
            // lblStartingKarma
            // 
            this.lblStartingKarma.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStartingKarma.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblStartingKarma, 2);
            this.lblStartingKarma.Location = new System.Drawing.Point(143, 56);
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
            this.tableLayoutPanel1.SetColumnSpan(this.lblDescription, 4);
            this.lblDescription.Location = new System.Drawing.Point(3, 33);
            this.lblDescription.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(440, 11);
            this.lblDescription.TabIndex = 0;
            this.lblDescription.Tag = "String_SelectBP_KarmaSummary";
            this.lblDescription.Text = "Enter the amount of Build Points you are allowed to create your character with (D" +
    "efault 400).";
            // 
            // nudMaxNuyen
            // 
            this.nudMaxNuyen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudMaxNuyen.Location = new System.Drawing.Point(225, 105);
            this.nudMaxNuyen.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nudMaxNuyen.Name = "nudMaxNuyen";
            this.nudMaxNuyen.Size = new System.Drawing.Size(105, 20);
            this.nudMaxNuyen.TabIndex = 13;
            this.nudMaxNuyen.Value = new decimal(new int[] {
            235,
            0,
            0,
            0});
            // 
            // lblMaxNuyen
            // 
            this.lblMaxNuyen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMaxNuyen.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblMaxNuyen, 2);
            this.lblMaxNuyen.Location = new System.Drawing.Point(125, 108);
            this.lblMaxNuyen.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMaxNuyen.Name = "lblMaxNuyen";
            this.lblMaxNuyen.Size = new System.Drawing.Size(94, 13);
            this.lblMaxNuyen.TabIndex = 15;
            this.lblMaxNuyen.Tag = "Label_SelectBP_MaxNuyen";
            this.lblMaxNuyen.Text = "Nuyen Karma Max";
            this.lblMaxNuyen.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.nudMaxNuyen, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.chkIgnoreRules, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.cboGamePlay, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblStartingKarma, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.cboBuildMethod, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.nudMaxAvail, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblDescription, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.nudKarma, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.nudSumtoTen, 2, 5);
            this.tableLayoutPanel1.Controls.Add(this.lblMaxAvail, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblMaxNuyen, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblSumToX, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 2, 6);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(9, 9);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 7;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(446, 183);
            this.tableLayoutPanel1.TabIndex = 16;
            // 
            // nudSumtoTen
            // 
            this.nudSumtoTen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudSumtoTen.Location = new System.Drawing.Point(225, 131);
            this.nudSumtoTen.Name = "nudSumtoTen";
            this.nudSumtoTen.Size = new System.Drawing.Size(105, 20);
            this.nudSumtoTen.TabIndex = 9;
            this.nudSumtoTen.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // lblSumToX
            // 
            this.lblSumToX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSumToX.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblSumToX, 2);
            this.lblSumToX.Location = new System.Drawing.Point(157, 134);
            this.lblSumToX.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSumToX.Name = "lblSumToX";
            this.lblSumToX.Size = new System.Drawing.Size(62, 13);
            this.lblSumToX.TabIndex = 10;
            this.lblSumToX.Tag = "Label_SelectBP_SumToX";
            this.lblSumToX.Text = "Sum to Ten";
            this.lblSumToX.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
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
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxAvail)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxNuyen)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSumtoTen)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
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
        private Chummer.BufferedTableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.NumericUpDown nudSumtoTen;
        private System.Windows.Forms.Label lblSumToX;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
