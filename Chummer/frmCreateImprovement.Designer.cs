namespace Chummer
{
    partial class frmCreateImprovement
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
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.lblImprovementType = new System.Windows.Forms.Label();
            this.cboImprovemetType = new Chummer.ElasticComboBox();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lblVal = new System.Windows.Forms.Label();
            this.nudVal = new System.Windows.Forms.NumericUpDown();
            this.lblName = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.nudMin = new System.Windows.Forms.NumericUpDown();
            this.lblMin = new System.Windows.Forms.Label();
            this.nudMax = new System.Windows.Forms.NumericUpDown();
            this.lblMax = new System.Windows.Forms.Label();
            this.nudAug = new System.Windows.Forms.NumericUpDown();
            this.lblAug = new System.Windows.Forms.Label();
            this.lblSelect = new System.Windows.Forms.Label();
            this.txtSelect = new System.Windows.Forms.TextBox();
            this.cmdChangeSelection = new System.Windows.Forms.Button();
            this.lblHelp = new System.Windows.Forms.Label();
            this.chkApplyToRating = new System.Windows.Forms.CheckBox();
            this.chkFree = new System.Windows.Forms.CheckBox();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.txtTranslateSelection = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudVal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAug)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblImprovementType
            // 
            this.lblImprovementType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblImprovementType.AutoSize = true;
            this.lblImprovementType.Location = new System.Drawing.Point(3, 6);
            this.lblImprovementType.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblImprovementType.Name = "lblImprovementType";
            this.lblImprovementType.Size = new System.Drawing.Size(98, 13);
            this.lblImprovementType.TabIndex = 0;
            this.lblImprovementType.Tag = "Label_ImprovementType";
            this.lblImprovementType.Text = "Improvement Type:";
            // 
            // cboImprovemetType
            // 
            this.cboImprovemetType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMain.SetColumnSpan(this.cboImprovemetType, 3);
            this.cboImprovemetType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboImprovemetType.FormattingEnabled = true;
            this.cboImprovemetType.Location = new System.Drawing.Point(107, 3);
            this.cboImprovemetType.Name = "cboImprovemetType";
            this.cboImprovemetType.Size = new System.Drawing.Size(366, 21);
            this.cboImprovemetType.TabIndex = 1;
            this.cboImprovemetType.TooltipText = "";
            this.cboImprovemetType.SelectedIndexChanged += new System.EventHandler(this.cboImprovemetType_SelectedIndexChanged);
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
            this.cmdCancel.TabIndex = 18;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.AutoSize = true;
            this.cmdOK.Location = new System.Drawing.Point(81, 0);
            this.cmdOK.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 17;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lblVal
            // 
            this.lblVal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVal.AutoSize = true;
            this.lblVal.Location = new System.Drawing.Point(64, 88);
            this.lblVal.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVal.Name = "lblVal";
            this.lblVal.Size = new System.Drawing.Size(37, 13);
            this.lblVal.TabIndex = 7;
            this.lblVal.Tag = "Label_CreateImprovementValue";
            this.lblVal.Text = "Value:";
            // 
            // nudVal
            // 
            this.nudVal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudVal.Location = new System.Drawing.Point(107, 85);
            this.nudVal.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.nudVal.Name = "nudVal";
            this.nudVal.Size = new System.Drawing.Size(53, 20);
            this.nudVal.TabIndex = 8;
            // 
            // lblName
            // 
            this.lblName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(63, 33);
            this.lblName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(38, 13);
            this.lblName.TabIndex = 2;
            this.lblName.Tag = "Label_Name";
            this.lblName.Text = "Name:";
            // 
            // txtName
            // 
            this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMain.SetColumnSpan(this.txtName, 3);
            this.txtName.Location = new System.Drawing.Point(107, 30);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(366, 20);
            this.txtName.TabIndex = 3;
            // 
            // nudMin
            // 
            this.nudMin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudMin.Location = new System.Drawing.Point(107, 111);
            this.nudMin.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.nudMin.Name = "nudMin";
            this.nudMin.Size = new System.Drawing.Size(53, 20);
            this.nudMin.TabIndex = 11;
            // 
            // lblMin
            // 
            this.lblMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMin.AutoSize = true;
            this.lblMin.Location = new System.Drawing.Point(50, 114);
            this.lblMin.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMin.Name = "lblMin";
            this.lblMin.Size = new System.Drawing.Size(51, 13);
            this.lblMin.TabIndex = 10;
            this.lblMin.Tag = "Label_CreateImprovementMinimum";
            this.lblMin.Text = "Minimum:";
            // 
            // nudMax
            // 
            this.nudMax.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudMax.Location = new System.Drawing.Point(107, 137);
            this.nudMax.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.nudMax.Name = "nudMax";
            this.nudMax.Size = new System.Drawing.Size(53, 20);
            this.nudMax.TabIndex = 13;
            // 
            // lblMax
            // 
            this.lblMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMax.AutoSize = true;
            this.lblMax.Location = new System.Drawing.Point(47, 140);
            this.lblMax.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMax.Name = "lblMax";
            this.lblMax.Size = new System.Drawing.Size(54, 13);
            this.lblMax.TabIndex = 12;
            this.lblMax.Tag = "Label_CreateImprovementMaximum";
            this.lblMax.Text = "Maximum:";
            // 
            // nudAug
            // 
            this.nudAug.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudAug.Location = new System.Drawing.Point(107, 163);
            this.nudAug.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.nudAug.Name = "nudAug";
            this.nudAug.Size = new System.Drawing.Size(53, 20);
            this.nudAug.TabIndex = 15;
            // 
            // lblAug
            // 
            this.lblAug.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAug.AutoSize = true;
            this.lblAug.Location = new System.Drawing.Point(37, 166);
            this.lblAug.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAug.Name = "lblAug";
            this.lblAug.Size = new System.Drawing.Size(64, 13);
            this.lblAug.TabIndex = 14;
            this.lblAug.Tag = "Label_CreateImprovementAugmented";
            this.lblAug.Text = "Augmented:";
            // 
            // lblSelect
            // 
            this.lblSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSelect.AutoSize = true;
            this.lblSelect.Location = new System.Drawing.Point(19, 59);
            this.lblSelect.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSelect.Name = "lblSelect";
            this.lblSelect.Size = new System.Drawing.Size(82, 13);
            this.lblSelect.TabIndex = 4;
            this.lblSelect.Tag = "Label_CreateImprovementSelectedValue";
            this.lblSelect.Text = "Selected Value:";
            // 
            // txtSelect
            // 
            this.txtSelect.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMain.SetColumnSpan(this.txtSelect, 2);
            this.txtSelect.Location = new System.Drawing.Point(107, 189);
            this.txtSelect.Name = "txtSelect";
            this.txtSelect.ReadOnly = true;
            this.txtSelect.Size = new System.Drawing.Size(157, 20);
            this.txtSelect.TabIndex = 5;
            this.txtSelect.Visible = false;
            // 
            // cmdChangeSelection
            // 
            this.cmdChangeSelection.AutoSize = true;
            this.cmdChangeSelection.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdChangeSelection.Location = new System.Drawing.Point(270, 56);
            this.cmdChangeSelection.Name = "cmdChangeSelection";
            this.cmdChangeSelection.Size = new System.Drawing.Size(77, 23);
            this.cmdChangeSelection.TabIndex = 6;
            this.cmdChangeSelection.Tag = "Button_ChangeSelection";
            this.cmdChangeSelection.Text = "Select Value";
            this.cmdChangeSelection.UseVisualStyleBackColor = true;
            this.cmdChangeSelection.Click += new System.EventHandler(this.cmdChangeSelection_Click);
            // 
            // lblHelp
            // 
            this.lblHelp.AutoSize = true;
            this.lblHelp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHelp.Location = new System.Drawing.Point(479, 0);
            this.lblHelp.Name = "lblHelp";
            this.tlpMain.SetRowSpan(this.lblHelp, 8);
            this.lblHelp.Size = new System.Drawing.Size(204, 234);
            this.lblHelp.TabIndex = 16;
            this.lblHelp.Tag = "String_Empty";
            this.lblHelp.Text = "[Help]";
            // 
            // chkApplyToRating
            // 
            this.chkApplyToRating.AutoSize = true;
            this.chkApplyToRating.Location = new System.Drawing.Point(166, 86);
            this.chkApplyToRating.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkApplyToRating.Name = "chkApplyToRating";
            this.chkApplyToRating.Size = new System.Drawing.Size(98, 17);
            this.chkApplyToRating.TabIndex = 9;
            this.chkApplyToRating.Tag = "Checkbox_CreateImprovementApplyToRating";
            this.chkApplyToRating.Text = "Apply to Rating";
            this.chkApplyToRating.UseVisualStyleBackColor = true;
            // 
            // chkFree
            // 
            this.chkFree.AutoSize = true;
            this.chkFree.Location = new System.Drawing.Point(166, 112);
            this.chkFree.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkFree.Name = "chkFree";
            this.chkFree.Size = new System.Drawing.Size(50, 17);
            this.chkFree.TabIndex = 19;
            this.chkFree.Tag = "Checkbox_Free";
            this.chkFree.Text = "Free!";
            this.chkFree.UseVisualStyleBackColor = true;
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 5;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.Controls.Add(this.nudAug, 1, 6);
            this.tlpMain.Controls.Add(this.chkFree, 2, 4);
            this.tlpMain.Controls.Add(this.lblAug, 0, 6);
            this.tlpMain.Controls.Add(this.lblImprovementType, 0, 0);
            this.tlpMain.Controls.Add(this.nudMax, 1, 5);
            this.tlpMain.Controls.Add(this.chkApplyToRating, 2, 3);
            this.tlpMain.Controls.Add(this.lblMax, 0, 5);
            this.tlpMain.Controls.Add(this.lblName, 0, 1);
            this.tlpMain.Controls.Add(this.cmdChangeSelection, 3, 2);
            this.tlpMain.Controls.Add(this.lblHelp, 4, 0);
            this.tlpMain.Controls.Add(this.nudMin, 1, 4);
            this.tlpMain.Controls.Add(this.cboImprovemetType, 1, 0);
            this.tlpMain.Controls.Add(this.txtName, 1, 1);
            this.tlpMain.Controls.Add(this.nudVal, 1, 3);
            this.tlpMain.Controls.Add(this.lblMin, 0, 4);
            this.tlpMain.Controls.Add(this.lblSelect, 0, 2);
            this.tlpMain.Controls.Add(this.lblVal, 0, 3);
            this.tlpMain.Controls.Add(this.flowLayoutPanel1, 0, 8);
            this.tlpMain.Controls.Add(this.txtSelect, 1, 7);
            this.tlpMain.Controls.Add(this.txtTranslateSelection, 1, 2);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 9;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Size = new System.Drawing.Size(686, 263);
            this.tlpMain.TabIndex = 20;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.SetColumnSpan(this.flowLayoutPanel1, 5);
            this.flowLayoutPanel1.Controls.Add(this.cmdOK);
            this.flowLayoutPanel1.Controls.Add(this.cmdCancel);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(527, 237);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(156, 23);
            this.flowLayoutPanel1.TabIndex = 20;
            // 
            // txtTranslateSelection
            // 
            this.txtTranslateSelection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMain.SetColumnSpan(this.txtTranslateSelection, 2);
            this.txtTranslateSelection.Location = new System.Drawing.Point(107, 56);
            this.txtTranslateSelection.Name = "txtTranslateSelection";
            this.txtTranslateSelection.ReadOnly = true;
            this.txtTranslateSelection.Size = new System.Drawing.Size(157, 20);
            this.txtTranslateSelection.TabIndex = 21;
            // 
            // frmCreateImprovement
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(704, 281);
            this.ControlBox = false;
            this.Controls.Add(this.tlpMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmCreateImprovement";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_CreateImprovement";
            this.Text = "Create Improvement";
            this.Load += new System.EventHandler(this.frmCreateImprovement_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudVal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAug)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblImprovementType;
        private ElasticComboBox cboImprovemetType;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Label lblVal;
        private System.Windows.Forms.NumericUpDown nudVal;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.NumericUpDown nudMin;
        private System.Windows.Forms.Label lblMin;
        private System.Windows.Forms.NumericUpDown nudMax;
        private System.Windows.Forms.Label lblMax;
        private System.Windows.Forms.NumericUpDown nudAug;
        private System.Windows.Forms.Label lblAug;
        private System.Windows.Forms.Label lblSelect;
        private System.Windows.Forms.TextBox txtSelect;
        private System.Windows.Forms.Button cmdChangeSelection;
        private System.Windows.Forms.Label lblHelp;
        private System.Windows.Forms.CheckBox chkApplyToRating;
        private System.Windows.Forms.CheckBox chkFree;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TextBox txtTranslateSelection;
    }
}
