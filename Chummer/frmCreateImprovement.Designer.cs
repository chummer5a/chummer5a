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
            this.cboImprovemetType = new System.Windows.Forms.ComboBox();
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
            ((System.ComponentModel.ISupportInitialize)(this.nudVal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAug)).BeginInit();
            this.SuspendLayout();
            // 
            // lblImprovementType
            // 
            this.lblImprovementType.AutoSize = true;
            this.lblImprovementType.Location = new System.Drawing.Point(12, 9);
            this.lblImprovementType.Name = "lblImprovementType";
            this.lblImprovementType.Size = new System.Drawing.Size(98, 13);
            this.lblImprovementType.TabIndex = 0;
            this.lblImprovementType.Tag = "Label_ImprovementType";
            this.lblImprovementType.Text = "Improvement Type:";
            // 
            // cboImprovemetType
            // 
            this.cboImprovemetType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboImprovemetType.FormattingEnabled = true;
            this.cboImprovemetType.Location = new System.Drawing.Point(116, 6);
            this.cboImprovemetType.Name = "cboImprovemetType";
            this.cboImprovemetType.Size = new System.Drawing.Size(228, 21);
            this.cboImprovemetType.TabIndex = 1;
            this.cboImprovemetType.SelectedIndexChanged += new System.EventHandler(this.cboImprovemetType_SelectedIndexChanged);
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(548, 198);
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
            this.cmdOK.Location = new System.Drawing.Point(629, 198);
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
            this.lblVal.AutoSize = true;
            this.lblVal.Location = new System.Drawing.Point(12, 97);
            this.lblVal.Name = "lblVal";
            this.lblVal.Size = new System.Drawing.Size(37, 13);
            this.lblVal.TabIndex = 7;
            this.lblVal.Tag = "Label_CreateImprovementValue";
            this.lblVal.Text = "Value:";
            // 
            // nudVal
            // 
            this.nudVal.Location = new System.Drawing.Point(116, 97);
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
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(12, 48);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(38, 13);
            this.lblName.TabIndex = 2;
            this.lblName.Tag = "Label_Name";
            this.lblName.Text = "Name:";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(116, 45);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(228, 20);
            this.txtName.TabIndex = 3;
            // 
            // nudMin
            // 
            this.nudMin.Location = new System.Drawing.Point(116, 123);
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
            this.lblMin.AutoSize = true;
            this.lblMin.Location = new System.Drawing.Point(12, 123);
            this.lblMin.Name = "lblMin";
            this.lblMin.Size = new System.Drawing.Size(51, 13);
            this.lblMin.TabIndex = 10;
            this.lblMin.Tag = "Label_CreateImprovementMinimum";
            this.lblMin.Text = "Minimum:";
            // 
            // nudMax
            // 
            this.nudMax.Location = new System.Drawing.Point(116, 149);
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
            this.lblMax.AutoSize = true;
            this.lblMax.Location = new System.Drawing.Point(12, 149);
            this.lblMax.Name = "lblMax";
            this.lblMax.Size = new System.Drawing.Size(54, 13);
            this.lblMax.TabIndex = 12;
            this.lblMax.Tag = "Label_CreateImprovementMaximum";
            this.lblMax.Text = "Maximum:";
            // 
            // nudAug
            // 
            this.nudAug.Location = new System.Drawing.Point(116, 175);
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
            this.lblAug.AutoSize = true;
            this.lblAug.Location = new System.Drawing.Point(12, 175);
            this.lblAug.Name = "lblAug";
            this.lblAug.Size = new System.Drawing.Size(64, 13);
            this.lblAug.TabIndex = 14;
            this.lblAug.Tag = "Label_CreateImprovementAugmented";
            this.lblAug.Text = "Augmented:";
            // 
            // lblSelect
            // 
            this.lblSelect.AutoSize = true;
            this.lblSelect.Location = new System.Drawing.Point(12, 74);
            this.lblSelect.Name = "lblSelect";
            this.lblSelect.Size = new System.Drawing.Size(82, 13);
            this.lblSelect.TabIndex = 4;
            this.lblSelect.Tag = "Label_CreateImprovementSelectedValue";
            this.lblSelect.Text = "Selected Value:";
            // 
            // txtSelect
            // 
            this.txtSelect.Location = new System.Drawing.Point(116, 71);
            this.txtSelect.Name = "txtSelect";
            this.txtSelect.ReadOnly = true;
            this.txtSelect.Size = new System.Drawing.Size(145, 20);
            this.txtSelect.TabIndex = 5;
            // 
            // cmdChangeSelection
            // 
            this.cmdChangeSelection.AutoSize = true;
            this.cmdChangeSelection.Location = new System.Drawing.Point(267, 69);
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
            this.lblHelp.Location = new System.Drawing.Point(400, 9);
            this.lblHelp.Name = "lblHelp";
            this.lblHelp.Size = new System.Drawing.Size(304, 186);
            this.lblHelp.TabIndex = 16;
            this.lblHelp.Tag = "String_Empty";
            this.lblHelp.Text = "[Help]";
            // 
            // chkApplyToRating
            // 
            this.chkApplyToRating.AutoSize = true;
            this.chkApplyToRating.Location = new System.Drawing.Point(175, 98);
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
            this.chkFree.Location = new System.Drawing.Point(175, 124);
            this.chkFree.Name = "chkFree";
            this.chkFree.Size = new System.Drawing.Size(50, 17);
            this.chkFree.TabIndex = 19;
            this.chkFree.Tag = "Checkbox_Free";
            this.chkFree.Text = "Free!";
            this.chkFree.UseVisualStyleBackColor = true;
            // 
            // frmCreateImprovement
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(710, 226);
            this.ControlBox = false;
            this.Controls.Add(this.chkFree);
            this.Controls.Add(this.chkApplyToRating);
            this.Controls.Add(this.lblHelp);
            this.Controls.Add(this.cmdChangeSelection);
            this.Controls.Add(this.txtSelect);
            this.Controls.Add(this.lblSelect);
            this.Controls.Add(this.nudAug);
            this.Controls.Add(this.lblAug);
            this.Controls.Add(this.nudMax);
            this.Controls.Add(this.lblMax);
            this.Controls.Add(this.nudMin);
            this.Controls.Add(this.lblMin);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.nudVal);
            this.Controls.Add(this.lblVal);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.cboImprovemetType);
            this.Controls.Add(this.lblImprovementType);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmCreateImprovement";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_CreateImprovement";
            this.Text = "Create Improvement";
            this.Load += new System.EventHandler(this.frmCreateImprovement_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudVal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAug)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblImprovementType;
        private System.Windows.Forms.ComboBox cboImprovemetType;
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
    }
}