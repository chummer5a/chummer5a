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
            this.chkApplyToRating = new System.Windows.Forms.CheckBox();
            this.chkFree = new System.Windows.Forms.CheckBox();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.txtTranslateSelection = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.chkIgnoreLimits = new System.Windows.Forms.CheckBox();
            this.txtHelp = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudVal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAug)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblImprovementType
            // 
            this.lblImprovementType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblImprovementType.AutoSize = true;
            this.lblImprovementType.Location = new System.Drawing.Point(4, 7);
            this.lblImprovementType.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.lblImprovementType.Name = "lblImprovementType";
            this.lblImprovementType.Size = new System.Drawing.Size(129, 17);
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
            this.cboImprovemetType.Location = new System.Drawing.Point(141, 4);
            this.cboImprovemetType.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cboImprovemetType.Name = "cboImprovemetType";
            this.cboImprovemetType.Size = new System.Drawing.Size(507, 24);
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
            this.cmdCancel.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(100, 33);
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
            this.cmdOK.Location = new System.Drawing.Point(108, 0);
            this.cmdOK.Margin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(100, 33);
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
            this.lblVal.Location = new System.Drawing.Point(85, 103);
            this.lblVal.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.lblVal.Name = "lblVal";
            this.lblVal.Size = new System.Drawing.Size(48, 17);
            this.lblVal.TabIndex = 7;
            this.lblVal.Tag = "Label_CreateImprovementValue";
            this.lblVal.Text = "Value:";
            // 
            // nudVal
            // 
            this.nudVal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudVal.Location = new System.Drawing.Point(141, 100);
            this.nudVal.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.nudVal.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.nudVal.Name = "nudVal";
            this.nudVal.Size = new System.Drawing.Size(71, 22);
            this.nudVal.TabIndex = 8;
            // 
            // lblName
            // 
            this.lblName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(84, 39);
            this.lblName.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(49, 17);
            this.lblName.TabIndex = 2;
            this.lblName.Tag = "Label_Name";
            this.lblName.Text = "Name:";
            // 
            // txtName
            // 
            this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMain.SetColumnSpan(this.txtName, 3);
            this.txtName.Location = new System.Drawing.Point(141, 36);
            this.txtName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(507, 22);
            this.txtName.TabIndex = 3;
            // 
            // nudMin
            // 
            this.nudMin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudMin.Location = new System.Drawing.Point(141, 131);
            this.nudMin.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.nudMin.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.nudMin.Name = "nudMin";
            this.nudMin.Size = new System.Drawing.Size(71, 22);
            this.nudMin.TabIndex = 11;
            // 
            // lblMin
            // 
            this.lblMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMin.AutoSize = true;
            this.lblMin.Location = new System.Drawing.Point(66, 134);
            this.lblMin.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.lblMin.Name = "lblMin";
            this.lblMin.Size = new System.Drawing.Size(67, 17);
            this.lblMin.TabIndex = 10;
            this.lblMin.Tag = "Label_CreateImprovementMinimum";
            this.lblMin.Text = "Minimum:";
            // 
            // nudMax
            // 
            this.nudMax.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudMax.Location = new System.Drawing.Point(141, 162);
            this.nudMax.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.nudMax.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.nudMax.Name = "nudMax";
            this.nudMax.Size = new System.Drawing.Size(71, 22);
            this.nudMax.TabIndex = 13;
            // 
            // lblMax
            // 
            this.lblMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMax.AutoSize = true;
            this.lblMax.Location = new System.Drawing.Point(63, 165);
            this.lblMax.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.lblMax.Name = "lblMax";
            this.lblMax.Size = new System.Drawing.Size(70, 17);
            this.lblMax.TabIndex = 12;
            this.lblMax.Tag = "Label_CreateImprovementMaximum";
            this.lblMax.Text = "Maximum:";
            // 
            // nudAug
            // 
            this.nudAug.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudAug.Location = new System.Drawing.Point(141, 193);
            this.nudAug.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.nudAug.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.nudAug.Name = "nudAug";
            this.nudAug.Size = new System.Drawing.Size(71, 22);
            this.nudAug.TabIndex = 15;
            // 
            // lblAug
            // 
            this.lblAug.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAug.AutoSize = true;
            this.lblAug.Location = new System.Drawing.Point(49, 196);
            this.lblAug.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.lblAug.Name = "lblAug";
            this.lblAug.Size = new System.Drawing.Size(84, 17);
            this.lblAug.TabIndex = 14;
            this.lblAug.Tag = "Label_CreateImprovementAugmented";
            this.lblAug.Text = "Augmented:";
            // 
            // lblSelect
            // 
            this.lblSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSelect.AutoSize = true;
            this.lblSelect.Location = new System.Drawing.Point(26, 70);
            this.lblSelect.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.lblSelect.Name = "lblSelect";
            this.lblSelect.Size = new System.Drawing.Size(107, 17);
            this.lblSelect.TabIndex = 4;
            this.lblSelect.Tag = "Label_CreateImprovementSelectedValue";
            this.lblSelect.Text = "Selected Value:";
            // 
            // txtSelect
            // 
            this.txtSelect.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMain.SetColumnSpan(this.txtSelect, 2);
            this.txtSelect.Location = new System.Drawing.Point(141, 224);
            this.txtSelect.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtSelect.Name = "txtSelect";
            this.txtSelect.ReadOnly = true;
            this.txtSelect.Size = new System.Drawing.Size(245, 22);
            this.txtSelect.TabIndex = 5;
            this.txtSelect.Visible = false;
            // 
            // cmdChangeSelection
            // 
            this.cmdChangeSelection.AutoSize = true;
            this.cmdChangeSelection.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdChangeSelection.Location = new System.Drawing.Point(3, 3);
            this.cmdChangeSelection.Name = "cmdChangeSelection";
            this.cmdChangeSelection.Size = new System.Drawing.Size(97, 27);
            this.cmdChangeSelection.TabIndex = 6;
            this.cmdChangeSelection.Tag = "Button_ChangeSelection";
            this.cmdChangeSelection.Text = "Select Value";
            this.cmdChangeSelection.UseVisualStyleBackColor = true;
            this.cmdChangeSelection.Click += new System.EventHandler(this.cmdChangeSelection_Click);
            // 
            // chkApplyToRating
            // 
            this.chkApplyToRating.AutoSize = true;
            this.chkApplyToRating.Location = new System.Drawing.Point(220, 101);
            this.chkApplyToRating.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkApplyToRating.Name = "chkApplyToRating";
            this.chkApplyToRating.Size = new System.Drawing.Size(126, 21);
            this.chkApplyToRating.TabIndex = 9;
            this.chkApplyToRating.Tag = "Checkbox_CreateImprovementApplyToRating";
            this.chkApplyToRating.Text = "Apply to Rating";
            this.chkApplyToRating.UseVisualStyleBackColor = true;
            // 
            // chkFree
            // 
            this.chkFree.AutoSize = true;
            this.chkFree.Location = new System.Drawing.Point(220, 132);
            this.chkFree.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkFree.Name = "chkFree";
            this.chkFree.Size = new System.Drawing.Size(62, 21);
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
            this.tlpMain.Controls.Add(this.flowLayoutPanel2, 3, 2);
            this.tlpMain.Controls.Add(this.txtHelp, 4, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(12, 11);
            this.tlpMain.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
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
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpMain.Size = new System.Drawing.Size(915, 324);
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
            this.flowLayoutPanel1.Location = new System.Drawing.Point(703, 287);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(208, 33);
            this.flowLayoutPanel1.TabIndex = 20;
            // 
            // txtTranslateSelection
            // 
            this.txtTranslateSelection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMain.SetColumnSpan(this.txtTranslateSelection, 2);
            this.txtTranslateSelection.Location = new System.Drawing.Point(141, 67);
            this.txtTranslateSelection.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtTranslateSelection.Name = "txtTranslateSelection";
            this.txtTranslateSelection.ReadOnly = true;
            this.txtTranslateSelection.Size = new System.Drawing.Size(245, 22);
            this.txtTranslateSelection.TabIndex = 21;
            this.txtTranslateSelection.Visible = false;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel2.Controls.Add(this.cmdChangeSelection);
            this.flowLayoutPanel2.Controls.Add(this.chkIgnoreLimits);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(390, 63);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(219, 33);
            this.flowLayoutPanel2.TabIndex = 22;
            // 
            // chkIgnoreLimits
            // 
            this.chkIgnoreLimits.AutoSize = true;
            this.chkIgnoreLimits.Location = new System.Drawing.Point(106, 4);
            this.chkIgnoreLimits.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkIgnoreLimits.Name = "chkIgnoreLimits";
            this.chkIgnoreLimits.Size = new System.Drawing.Size(110, 21);
            this.chkIgnoreLimits.TabIndex = 7;
            this.chkIgnoreLimits.Tag = "Checkbox_CreateImprovementIgnoreLimits";
            this.chkIgnoreLimits.Text = "Ignore Limits";
            this.chkIgnoreLimits.UseVisualStyleBackColor = true;
            this.chkIgnoreLimits.Visible = false;
            // 
            // txtHelp
            // 
            this.txtHelp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtHelp.Location = new System.Drawing.Point(652, 3);
            this.txtHelp.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.txtHelp.Multiline = true;
            this.txtHelp.Name = "txtHelp";
            this.txtHelp.ReadOnly = true;
            this.tlpMain.SetRowSpan(this.txtHelp, 8);
            this.txtHelp.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtHelp.Size = new System.Drawing.Size(263, 277);
            this.txtHelp.TabIndex = 23;
            this.txtHelp.Tag = "String_Empty";
            this.txtHelp.Text = "[Help]";
            // 
            // frmCreateImprovement
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(939, 346);
            this.ControlBox = false;
            this.Controls.Add(this.tlpMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmCreateImprovement";
            this.Padding = new System.Windows.Forms.Padding(12, 11, 12, 11);
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
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
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
        private System.Windows.Forms.CheckBox chkApplyToRating;
        private System.Windows.Forms.CheckBox chkFree;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TextBox txtTranslateSelection;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.CheckBox chkIgnoreLimits;
        private System.Windows.Forms.TextBox txtHelp;
    }
}
