namespace Chummer
{
    partial class frmCreateSpell
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
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lblCategory = new System.Windows.Forms.Label();
            this.cboCategory = new System.Windows.Forms.ComboBox();
            this.lblType = new System.Windows.Forms.Label();
            this.cboType = new System.Windows.Forms.ComboBox();
            this.lblRange = new System.Windows.Forms.Label();
            this.cboRange = new System.Windows.Forms.ComboBox();
            this.chkRestricted = new System.Windows.Forms.CheckBox();
            this.chkVeryRestricted = new System.Windows.Forms.CheckBox();
            this.txtRestriction = new System.Windows.Forms.TextBox();
            this.lblDuration = new System.Windows.Forms.Label();
            this.cboDuration = new System.Windows.Forms.ComboBox();
            this.lblDVLabel = new System.Windows.Forms.Label();
            this.lblDV = new System.Windows.Forms.Label();
            this.lblSpellOptions = new System.Windows.Forms.Label();
            this.panModifiers = new System.Windows.Forms.Panel();
            this.chkModifier14 = new System.Windows.Forms.CheckBox();
            this.chkModifier13 = new System.Windows.Forms.CheckBox();
            this.chkModifier12 = new System.Windows.Forms.CheckBox();
            this.chkModifier11 = new System.Windows.Forms.CheckBox();
            this.chkModifier10 = new System.Windows.Forms.CheckBox();
            this.nudNumberOfEffects = new System.Windows.Forms.NumericUpDown();
            this.chkModifier9 = new System.Windows.Forms.CheckBox();
            this.chkModifier8 = new System.Windows.Forms.CheckBox();
            this.chkModifier7 = new System.Windows.Forms.CheckBox();
            this.chkModifier6 = new System.Windows.Forms.CheckBox();
            this.chkModifier5 = new System.Windows.Forms.CheckBox();
            this.chkModifier4 = new System.Windows.Forms.CheckBox();
            this.chkModifier3 = new System.Windows.Forms.CheckBox();
            this.chkModifier2 = new System.Windows.Forms.CheckBox();
            this.chkModifier1 = new System.Windows.Forms.CheckBox();
            this.lblName = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.chkArea = new System.Windows.Forms.CheckBox();
            this.chkLimited = new System.Windows.Forms.CheckBox();
            this.panModifiers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudNumberOfEffects)).BeginInit();
            this.SuspendLayout();
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(403, 500);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 17;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.Location = new System.Drawing.Point(484, 500);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 18;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lblCategory
            // 
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(12, 41);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(52, 13);
            this.lblCategory.TabIndex = 2;
            this.lblCategory.Tag = "Label_Category";
            this.lblCategory.Text = "Category:";
            // 
            // cboCategory
            // 
            this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategory.FormattingEnabled = true;
            this.cboCategory.Location = new System.Drawing.Point(89, 38);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(182, 21);
            this.cboCategory.TabIndex = 3;
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(12, 68);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(34, 13);
            this.lblType.TabIndex = 4;
            this.lblType.Tag = "Label_Type";
            this.lblType.Text = "Type:";
            // 
            // cboType
            // 
            this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboType.FormattingEnabled = true;
            this.cboType.Location = new System.Drawing.Point(89, 65);
            this.cboType.Name = "cboType";
            this.cboType.Size = new System.Drawing.Size(182, 21);
            this.cboType.TabIndex = 5;
            this.cboType.SelectedIndexChanged += new System.EventHandler(this.cboType_SelectedIndexChanged);
            // 
            // lblRange
            // 
            this.lblRange.AutoSize = true;
            this.lblRange.Location = new System.Drawing.Point(12, 95);
            this.lblRange.Name = "lblRange";
            this.lblRange.Size = new System.Drawing.Size(42, 13);
            this.lblRange.TabIndex = 6;
            this.lblRange.Tag = "Label_Range";
            this.lblRange.Text = "Range:";
            // 
            // cboRange
            // 
            this.cboRange.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboRange.FormattingEnabled = true;
            this.cboRange.Location = new System.Drawing.Point(89, 92);
            this.cboRange.Name = "cboRange";
            this.cboRange.Size = new System.Drawing.Size(182, 21);
            this.cboRange.TabIndex = 7;
            this.cboRange.SelectedIndexChanged += new System.EventHandler(this.cboRange_SelectedIndexChanged);
            // 
            // chkRestricted
            // 
            this.chkRestricted.AutoSize = true;
            this.chkRestricted.Location = new System.Drawing.Point(89, 119);
            this.chkRestricted.Name = "chkRestricted";
            this.chkRestricted.Size = new System.Drawing.Size(108, 17);
            this.chkRestricted.TabIndex = 8;
            this.chkRestricted.Text = "Restricted Target";
            this.chkRestricted.UseVisualStyleBackColor = true;
            this.chkRestricted.CheckedChanged += new System.EventHandler(this.chkRestricted_CheckedChanged);
            // 
            // chkVeryRestricted
            // 
            this.chkVeryRestricted.AutoSize = true;
            this.chkVeryRestricted.Location = new System.Drawing.Point(203, 119);
            this.chkVeryRestricted.Name = "chkVeryRestricted";
            this.chkVeryRestricted.Size = new System.Drawing.Size(132, 17);
            this.chkVeryRestricted.TabIndex = 9;
            this.chkVeryRestricted.Text = "Very Restricted Target";
            this.chkVeryRestricted.UseVisualStyleBackColor = true;
            this.chkVeryRestricted.CheckedChanged += new System.EventHandler(this.chkVeryRestricted_CheckedChanged);
            // 
            // txtRestriction
            // 
            this.txtRestriction.Enabled = false;
            this.txtRestriction.Location = new System.Drawing.Point(341, 117);
            this.txtRestriction.Name = "txtRestriction";
            this.txtRestriction.Size = new System.Drawing.Size(188, 20);
            this.txtRestriction.TabIndex = 10;
            // 
            // lblDuration
            // 
            this.lblDuration.AutoSize = true;
            this.lblDuration.Location = new System.Drawing.Point(12, 145);
            this.lblDuration.Name = "lblDuration";
            this.lblDuration.Size = new System.Drawing.Size(50, 13);
            this.lblDuration.TabIndex = 11;
            this.lblDuration.Tag = "Label_Duration";
            this.lblDuration.Text = "Duration:";
            // 
            // cboDuration
            // 
            this.cboDuration.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDuration.FormattingEnabled = true;
            this.cboDuration.Location = new System.Drawing.Point(89, 142);
            this.cboDuration.Name = "cboDuration";
            this.cboDuration.Size = new System.Drawing.Size(182, 21);
            this.cboDuration.TabIndex = 12;
            this.cboDuration.SelectedIndexChanged += new System.EventHandler(this.cboDuration_SelectedIndexChanged);
            // 
            // lblDVLabel
            // 
            this.lblDVLabel.AutoSize = true;
            this.lblDVLabel.Location = new System.Drawing.Point(467, 41);
            this.lblDVLabel.Name = "lblDVLabel";
            this.lblDVLabel.Size = new System.Drawing.Size(25, 13);
            this.lblDVLabel.TabIndex = 15;
            this.lblDVLabel.Tag = "Label_DV";
            this.lblDVLabel.Text = "DV:";
            // 
            // lblDV
            // 
            this.lblDV.AutoSize = true;
            this.lblDV.Location = new System.Drawing.Point(498, 41);
            this.lblDV.Name = "lblDV";
            this.lblDV.Size = new System.Drawing.Size(19, 13);
            this.lblDV.TabIndex = 16;
            this.lblDV.Tag = "String_Empty";
            this.lblDV.Text = "[0]";
            // 
            // lblSpellOptions
            // 
            this.lblSpellOptions.AutoSize = true;
            this.lblSpellOptions.Location = new System.Drawing.Point(12, 174);
            this.lblSpellOptions.Name = "lblSpellOptions";
            this.lblSpellOptions.Size = new System.Drawing.Size(72, 13);
            this.lblSpellOptions.TabIndex = 13;
            this.lblSpellOptions.Tag = "Label_SpellOptions";
            this.lblSpellOptions.Text = "Spell Options:";
            // 
            // panModifiers
            // 
            this.panModifiers.Controls.Add(this.chkModifier14);
            this.panModifiers.Controls.Add(this.chkModifier13);
            this.panModifiers.Controls.Add(this.chkModifier12);
            this.panModifiers.Controls.Add(this.chkModifier11);
            this.panModifiers.Controls.Add(this.chkModifier10);
            this.panModifiers.Controls.Add(this.nudNumberOfEffects);
            this.panModifiers.Controls.Add(this.chkModifier9);
            this.panModifiers.Controls.Add(this.chkModifier8);
            this.panModifiers.Controls.Add(this.chkModifier7);
            this.panModifiers.Controls.Add(this.chkModifier6);
            this.panModifiers.Controls.Add(this.chkModifier5);
            this.panModifiers.Controls.Add(this.chkModifier4);
            this.panModifiers.Controls.Add(this.chkModifier3);
            this.panModifiers.Controls.Add(this.chkModifier2);
            this.panModifiers.Controls.Add(this.chkModifier1);
            this.panModifiers.Location = new System.Drawing.Point(89, 169);
            this.panModifiers.Name = "panModifiers";
            this.panModifiers.Size = new System.Drawing.Size(440, 325);
            this.panModifiers.TabIndex = 14;
            // 
            // chkModifier14
            // 
            this.chkModifier14.AutoSize = true;
            this.chkModifier14.Location = new System.Drawing.Point(0, 303);
            this.chkModifier14.Name = "chkModifier14";
            this.chkModifier14.Size = new System.Drawing.Size(100, 17);
            this.chkModifier14.TabIndex = 13;
            this.chkModifier14.Text = "[Modifier Name]";
            this.chkModifier14.UseVisualStyleBackColor = true;
            this.chkModifier14.CheckedChanged += new System.EventHandler(this.chkModifier_CheckedChanged);
            // 
            // chkModifier13
            // 
            this.chkModifier13.AutoSize = true;
            this.chkModifier13.Location = new System.Drawing.Point(0, 280);
            this.chkModifier13.Name = "chkModifier13";
            this.chkModifier13.Size = new System.Drawing.Size(100, 17);
            this.chkModifier13.TabIndex = 12;
            this.chkModifier13.Text = "[Modifier Name]";
            this.chkModifier13.UseVisualStyleBackColor = true;
            this.chkModifier13.CheckedChanged += new System.EventHandler(this.chkModifier_CheckedChanged);
            // 
            // chkModifier12
            // 
            this.chkModifier12.AutoSize = true;
            this.chkModifier12.Location = new System.Drawing.Point(0, 257);
            this.chkModifier12.Name = "chkModifier12";
            this.chkModifier12.Size = new System.Drawing.Size(100, 17);
            this.chkModifier12.TabIndex = 11;
            this.chkModifier12.Text = "[Modifier Name]";
            this.chkModifier12.UseVisualStyleBackColor = true;
            this.chkModifier12.CheckedChanged += new System.EventHandler(this.chkModifier_CheckedChanged);
            // 
            // chkModifier11
            // 
            this.chkModifier11.AutoSize = true;
            this.chkModifier11.Location = new System.Drawing.Point(0, 234);
            this.chkModifier11.Name = "chkModifier11";
            this.chkModifier11.Size = new System.Drawing.Size(100, 17);
            this.chkModifier11.TabIndex = 10;
            this.chkModifier11.Text = "[Modifier Name]";
            this.chkModifier11.UseVisualStyleBackColor = true;
            this.chkModifier11.CheckedChanged += new System.EventHandler(this.chkModifier_CheckedChanged);
            // 
            // chkModifier10
            // 
            this.chkModifier10.AutoSize = true;
            this.chkModifier10.Location = new System.Drawing.Point(0, 211);
            this.chkModifier10.Name = "chkModifier10";
            this.chkModifier10.Size = new System.Drawing.Size(100, 17);
            this.chkModifier10.TabIndex = 9;
            this.chkModifier10.Text = "[Modifier Name]";
            this.chkModifier10.UseVisualStyleBackColor = true;
            this.chkModifier10.CheckedChanged += new System.EventHandler(this.chkModifier_CheckedChanged);
            // 
            // nudNumberOfEffects
            // 
            this.nudNumberOfEffects.Location = new System.Drawing.Point(114, 49);
            this.nudNumberOfEffects.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudNumberOfEffects.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudNumberOfEffects.Name = "nudNumberOfEffects";
            this.nudNumberOfEffects.Size = new System.Drawing.Size(42, 20);
            this.nudNumberOfEffects.TabIndex = 48;
            this.nudNumberOfEffects.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudNumberOfEffects.Visible = false;
            this.nudNumberOfEffects.ValueChanged += new System.EventHandler(this.nudNumberOfEffects_ValueChanged);
            // 
            // chkModifier9
            // 
            this.chkModifier9.AutoSize = true;
            this.chkModifier9.Location = new System.Drawing.Point(0, 188);
            this.chkModifier9.Name = "chkModifier9";
            this.chkModifier9.Size = new System.Drawing.Size(100, 17);
            this.chkModifier9.TabIndex = 8;
            this.chkModifier9.Text = "[Modifier Name]";
            this.chkModifier9.UseVisualStyleBackColor = true;
            this.chkModifier9.CheckedChanged += new System.EventHandler(this.chkModifier_CheckedChanged);
            // 
            // chkModifier8
            // 
            this.chkModifier8.AutoSize = true;
            this.chkModifier8.Location = new System.Drawing.Point(0, 165);
            this.chkModifier8.Name = "chkModifier8";
            this.chkModifier8.Size = new System.Drawing.Size(100, 17);
            this.chkModifier8.TabIndex = 7;
            this.chkModifier8.Text = "[Modifier Name]";
            this.chkModifier8.UseVisualStyleBackColor = true;
            this.chkModifier8.CheckedChanged += new System.EventHandler(this.chkModifier_CheckedChanged);
            // 
            // chkModifier7
            // 
            this.chkModifier7.AutoSize = true;
            this.chkModifier7.Location = new System.Drawing.Point(0, 142);
            this.chkModifier7.Name = "chkModifier7";
            this.chkModifier7.Size = new System.Drawing.Size(100, 17);
            this.chkModifier7.TabIndex = 6;
            this.chkModifier7.Text = "[Modifier Name]";
            this.chkModifier7.UseVisualStyleBackColor = true;
            this.chkModifier7.CheckedChanged += new System.EventHandler(this.chkModifier_CheckedChanged);
            // 
            // chkModifier6
            // 
            this.chkModifier6.AutoSize = true;
            this.chkModifier6.Location = new System.Drawing.Point(0, 119);
            this.chkModifier6.Name = "chkModifier6";
            this.chkModifier6.Size = new System.Drawing.Size(100, 17);
            this.chkModifier6.TabIndex = 5;
            this.chkModifier6.Text = "[Modifier Name]";
            this.chkModifier6.UseVisualStyleBackColor = true;
            this.chkModifier6.CheckedChanged += new System.EventHandler(this.chkModifier_CheckedChanged);
            // 
            // chkModifier5
            // 
            this.chkModifier5.AutoSize = true;
            this.chkModifier5.Location = new System.Drawing.Point(0, 96);
            this.chkModifier5.Name = "chkModifier5";
            this.chkModifier5.Size = new System.Drawing.Size(100, 17);
            this.chkModifier5.TabIndex = 4;
            this.chkModifier5.Text = "[Modifier Name]";
            this.chkModifier5.UseVisualStyleBackColor = true;
            this.chkModifier5.CheckedChanged += new System.EventHandler(this.chkModifier_CheckedChanged);
            // 
            // chkModifier4
            // 
            this.chkModifier4.AutoSize = true;
            this.chkModifier4.Location = new System.Drawing.Point(0, 73);
            this.chkModifier4.Name = "chkModifier4";
            this.chkModifier4.Size = new System.Drawing.Size(100, 17);
            this.chkModifier4.TabIndex = 3;
            this.chkModifier4.Text = "[Modifier Name]";
            this.chkModifier4.UseVisualStyleBackColor = true;
            this.chkModifier4.CheckedChanged += new System.EventHandler(this.chkModifier_CheckedChanged);
            // 
            // chkModifier3
            // 
            this.chkModifier3.AutoSize = true;
            this.chkModifier3.Location = new System.Drawing.Point(0, 50);
            this.chkModifier3.Name = "chkModifier3";
            this.chkModifier3.Size = new System.Drawing.Size(100, 17);
            this.chkModifier3.TabIndex = 2;
            this.chkModifier3.Text = "[Modifier Name]";
            this.chkModifier3.UseVisualStyleBackColor = true;
            this.chkModifier3.CheckedChanged += new System.EventHandler(this.chkModifier_CheckedChanged);
            // 
            // chkModifier2
            // 
            this.chkModifier2.AutoSize = true;
            this.chkModifier2.Location = new System.Drawing.Point(0, 27);
            this.chkModifier2.Name = "chkModifier2";
            this.chkModifier2.Size = new System.Drawing.Size(100, 17);
            this.chkModifier2.TabIndex = 1;
            this.chkModifier2.Text = "[Modifier Name]";
            this.chkModifier2.UseVisualStyleBackColor = true;
            this.chkModifier2.CheckedChanged += new System.EventHandler(this.chkModifier_CheckedChanged);
            // 
            // chkModifier1
            // 
            this.chkModifier1.AutoSize = true;
            this.chkModifier1.Location = new System.Drawing.Point(0, 4);
            this.chkModifier1.Name = "chkModifier1";
            this.chkModifier1.Size = new System.Drawing.Size(100, 17);
            this.chkModifier1.TabIndex = 0;
            this.chkModifier1.Text = "[Modifier Name]";
            this.chkModifier1.UseVisualStyleBackColor = true;
            this.chkModifier1.CheckedChanged += new System.EventHandler(this.chkModifier_CheckedChanged);
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(11, 15);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(38, 13);
            this.lblName.TabIndex = 0;
            this.lblName.Tag = "Label_Name";
            this.lblName.Text = "Name:";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(89, 12);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(323, 20);
            this.txtName.TabIndex = 1;
            // 
            // chkArea
            // 
            this.chkArea.AutoSize = true;
            this.chkArea.Location = new System.Drawing.Point(277, 94);
            this.chkArea.Name = "chkArea";
            this.chkArea.Size = new System.Drawing.Size(48, 17);
            this.chkArea.TabIndex = 19;
            this.chkArea.Tag = "String_DescArea";
            this.chkArea.Text = "Area";
            this.chkArea.UseVisualStyleBackColor = true;
            this.chkArea.CheckedChanged += new System.EventHandler(this.chkArea_CheckedChanged);
            // 
            // chkLimited
            // 
            this.chkLimited.AutoSize = true;
            this.chkLimited.Location = new System.Drawing.Point(277, 40);
            this.chkLimited.Name = "chkLimited";
            this.chkLimited.Size = new System.Drawing.Size(85, 17);
            this.chkLimited.TabIndex = 20;
            this.chkLimited.Tag = "Checkbox_SelectSpell_LimitedSpell";
            this.chkLimited.Text = "Limited Spell";
            this.chkLimited.UseVisualStyleBackColor = true;
            // 
            // frmCreateSpell
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(571, 532);
            this.Controls.Add(this.chkLimited);
            this.Controls.Add(this.chkArea);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.panModifiers);
            this.Controls.Add(this.lblSpellOptions);
            this.Controls.Add(this.lblDV);
            this.Controls.Add(this.lblDVLabel);
            this.Controls.Add(this.lblDuration);
            this.Controls.Add(this.cboDuration);
            this.Controls.Add(this.txtRestriction);
            this.Controls.Add(this.chkVeryRestricted);
            this.Controls.Add(this.chkRestricted);
            this.Controls.Add(this.lblRange);
            this.Controls.Add(this.cboRange);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.cboType);
            this.Controls.Add(this.lblCategory);
            this.Controls.Add(this.cboCategory);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmCreateSpell";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_CreateSpell";
            this.Text = "Create Spell";
            this.Load += new System.EventHandler(this.frmCreateSpell_Load);
            this.panModifiers.ResumeLayout(false);
            this.panModifiers.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudNumberOfEffects)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.ComboBox cboCategory;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.ComboBox cboType;
        private System.Windows.Forms.Label lblRange;
        private System.Windows.Forms.ComboBox cboRange;
        private System.Windows.Forms.CheckBox chkRestricted;
        private System.Windows.Forms.CheckBox chkVeryRestricted;
        private System.Windows.Forms.TextBox txtRestriction;
        private System.Windows.Forms.Label lblDuration;
        private System.Windows.Forms.ComboBox cboDuration;
        private System.Windows.Forms.Label lblDVLabel;
        private System.Windows.Forms.Label lblDV;
        private System.Windows.Forms.Label lblSpellOptions;
        private System.Windows.Forms.Panel panModifiers;
        private System.Windows.Forms.CheckBox chkModifier9;
        private System.Windows.Forms.CheckBox chkModifier8;
        private System.Windows.Forms.CheckBox chkModifier7;
        private System.Windows.Forms.CheckBox chkModifier6;
        private System.Windows.Forms.CheckBox chkModifier5;
        private System.Windows.Forms.CheckBox chkModifier4;
        private System.Windows.Forms.CheckBox chkModifier3;
        private System.Windows.Forms.CheckBox chkModifier2;
        private System.Windows.Forms.CheckBox chkModifier1;
        private System.Windows.Forms.NumericUpDown nudNumberOfEffects;
        private System.Windows.Forms.CheckBox chkModifier14;
        private System.Windows.Forms.CheckBox chkModifier13;
        private System.Windows.Forms.CheckBox chkModifier12;
        private System.Windows.Forms.CheckBox chkModifier11;
        private System.Windows.Forms.CheckBox chkModifier10;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.CheckBox chkArea;
        private System.Windows.Forms.CheckBox chkLimited;
    }
}