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
            this.cboCategory = new Chummer.ElasticComboBox();
            this.lblType = new System.Windows.Forms.Label();
            this.cboType = new Chummer.ElasticComboBox();
            this.lblRange = new System.Windows.Forms.Label();
            this.cboRange = new Chummer.ElasticComboBox();
            this.chkRestricted = new System.Windows.Forms.CheckBox();
            this.chkVeryRestricted = new System.Windows.Forms.CheckBox();
            this.txtRestriction = new System.Windows.Forms.TextBox();
            this.lblDuration = new System.Windows.Forms.Label();
            this.cboDuration = new Chummer.ElasticComboBox();
            this.lblDVLabel = new System.Windows.Forms.Label();
            this.lblDV = new System.Windows.Forms.Label();
            this.lblSpellOptions = new System.Windows.Forms.Label();
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
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.flpModifiers = new System.Windows.Forms.FlowLayoutPanel();
            this.flpModifier3 = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.nudNumberOfEffects)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.flpModifiers.SuspendLayout();
            this.flpModifier3.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdCancel.AutoSize = true;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(0, 0);
            this.cmdCancel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
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
            this.cmdOK.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdOK.AutoSize = true;
            this.cmdOK.Location = new System.Drawing.Point(81, 0);
            this.cmdOK.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
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
            this.lblCategory.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCategory.AutoSize = true;
            this.lblCategory.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCategory.Location = new System.Drawing.Point(23, 33);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(52, 13);
            this.lblCategory.TabIndex = 2;
            this.lblCategory.Tag = "Label_Category";
            this.lblCategory.Text = "Category:";
            this.lblCategory.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboCategory
            // 
            this.cboCategory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategory.FormattingEnabled = true;
            this.cboCategory.Location = new System.Drawing.Point(81, 29);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(212, 21);
            this.cboCategory.TabIndex = 3;
            this.cboCategory.TooltipText = "";
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // lblType
            // 
            this.lblType.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblType.AutoSize = true;
            this.lblType.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblType.Location = new System.Drawing.Point(41, 60);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(34, 13);
            this.lblType.TabIndex = 4;
            this.lblType.Tag = "Label_Type";
            this.lblType.Text = "Type:";
            this.lblType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboType
            // 
            this.cboType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboType.FormattingEnabled = true;
            this.cboType.Location = new System.Drawing.Point(81, 56);
            this.cboType.Name = "cboType";
            this.cboType.Size = new System.Drawing.Size(212, 21);
            this.cboType.TabIndex = 5;
            this.cboType.TooltipText = "";
            this.cboType.SelectedIndexChanged += new System.EventHandler(this.cboType_SelectedIndexChanged);
            // 
            // lblRange
            // 
            this.lblRange.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblRange.AutoSize = true;
            this.lblRange.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRange.Location = new System.Drawing.Point(33, 87);
            this.lblRange.Name = "lblRange";
            this.lblRange.Size = new System.Drawing.Size(42, 13);
            this.lblRange.TabIndex = 6;
            this.lblRange.Tag = "Label_Range";
            this.lblRange.Text = "Range:";
            this.lblRange.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboRange
            // 
            this.cboRange.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboRange.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboRange.FormattingEnabled = true;
            this.cboRange.Location = new System.Drawing.Point(81, 83);
            this.cboRange.Name = "cboRange";
            this.cboRange.Size = new System.Drawing.Size(212, 21);
            this.cboRange.TabIndex = 7;
            this.cboRange.TooltipText = "";
            this.cboRange.SelectedIndexChanged += new System.EventHandler(this.cboRange_SelectedIndexChanged);
            // 
            // chkRestricted
            // 
            this.chkRestricted.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkRestricted.AutoSize = true;
            this.chkRestricted.Location = new System.Drawing.Point(3, 4);
            this.chkRestricted.Name = "chkRestricted";
            this.chkRestricted.Size = new System.Drawing.Size(108, 17);
            this.chkRestricted.TabIndex = 8;
            this.chkRestricted.Text = "Restricted Target";
            this.chkRestricted.UseVisualStyleBackColor = true;
            this.chkRestricted.CheckedChanged += new System.EventHandler(this.chkRestricted_CheckedChanged);
            // 
            // chkVeryRestricted
            // 
            this.chkVeryRestricted.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkVeryRestricted.AutoSize = true;
            this.chkVeryRestricted.Location = new System.Drawing.Point(117, 4);
            this.chkVeryRestricted.Name = "chkVeryRestricted";
            this.chkVeryRestricted.Size = new System.Drawing.Size(132, 17);
            this.chkVeryRestricted.TabIndex = 9;
            this.chkVeryRestricted.Text = "Very Restricted Target";
            this.chkVeryRestricted.UseVisualStyleBackColor = true;
            this.chkVeryRestricted.CheckedChanged += new System.EventHandler(this.chkVeryRestricted_CheckedChanged);
            // 
            // txtRestriction
            // 
            this.txtRestriction.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txtRestriction.Enabled = false;
            this.txtRestriction.Location = new System.Drawing.Point(255, 3);
            this.txtRestriction.Name = "txtRestriction";
            this.txtRestriction.Size = new System.Drawing.Size(188, 20);
            this.txtRestriction.TabIndex = 10;
            // 
            // lblDuration
            // 
            this.lblDuration.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDuration.AutoSize = true;
            this.lblDuration.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDuration.Location = new System.Drawing.Point(25, 140);
            this.lblDuration.Name = "lblDuration";
            this.lblDuration.Size = new System.Drawing.Size(50, 13);
            this.lblDuration.TabIndex = 11;
            this.lblDuration.Tag = "Label_Duration";
            this.lblDuration.Text = "Duration:";
            this.lblDuration.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboDuration
            // 
            this.cboDuration.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboDuration.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDuration.FormattingEnabled = true;
            this.cboDuration.Location = new System.Drawing.Point(81, 136);
            this.cboDuration.Name = "cboDuration";
            this.cboDuration.Size = new System.Drawing.Size(212, 21);
            this.cboDuration.TabIndex = 12;
            this.cboDuration.TooltipText = "";
            this.cboDuration.SelectedIndexChanged += new System.EventHandler(this.cboDuration_SelectedIndexChanged);
            // 
            // lblDVLabel
            // 
            this.lblDVLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDVLabel.AutoSize = true;
            this.lblDVLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDVLabel.Location = new System.Drawing.Point(3, 0);
            this.lblDVLabel.Name = "lblDVLabel";
            this.lblDVLabel.Size = new System.Drawing.Size(25, 13);
            this.lblDVLabel.TabIndex = 15;
            this.lblDVLabel.Tag = "Label_DV";
            this.lblDVLabel.Text = "DV:";
            this.lblDVLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblDV
            // 
            this.lblDV.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDV.AutoSize = true;
            this.lblDV.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDV.Location = new System.Drawing.Point(34, 0);
            this.lblDV.Name = "lblDV";
            this.lblDV.Size = new System.Drawing.Size(19, 13);
            this.lblDV.TabIndex = 16;
            this.lblDV.Tag = "String_Empty";
            this.lblDV.Text = "[0]";
            this.lblDV.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSpellOptions
            // 
            this.lblSpellOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSpellOptions.AutoSize = true;
            this.lblSpellOptions.Location = new System.Drawing.Point(3, 166);
            this.lblSpellOptions.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellOptions.Name = "lblSpellOptions";
            this.lblSpellOptions.Size = new System.Drawing.Size(72, 13);
            this.lblSpellOptions.TabIndex = 13;
            this.lblSpellOptions.Tag = "Label_SpellOptions";
            this.lblSpellOptions.Text = "Spell Options:";
            // 
            // chkModifier14
            // 
            this.chkModifier14.AutoSize = true;
            this.chkModifier14.Location = new System.Drawing.Point(3, 305);
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
            this.chkModifier13.Location = new System.Drawing.Point(3, 282);
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
            this.chkModifier12.Location = new System.Drawing.Point(3, 259);
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
            this.chkModifier11.Location = new System.Drawing.Point(3, 236);
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
            this.chkModifier10.Location = new System.Drawing.Point(3, 213);
            this.chkModifier10.Name = "chkModifier10";
            this.chkModifier10.Size = new System.Drawing.Size(100, 17);
            this.chkModifier10.TabIndex = 9;
            this.chkModifier10.Text = "[Modifier Name]";
            this.chkModifier10.UseVisualStyleBackColor = true;
            this.chkModifier10.CheckedChanged += new System.EventHandler(this.chkModifier_CheckedChanged);
            // 
            // nudNumberOfEffects
            // 
            this.nudNumberOfEffects.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudNumberOfEffects.Location = new System.Drawing.Point(109, 3);
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
            this.chkModifier9.Location = new System.Drawing.Point(3, 190);
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
            this.chkModifier8.Location = new System.Drawing.Point(3, 167);
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
            this.chkModifier7.Location = new System.Drawing.Point(3, 144);
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
            this.chkModifier6.Location = new System.Drawing.Point(3, 121);
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
            this.chkModifier5.Location = new System.Drawing.Point(3, 98);
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
            this.chkModifier4.Location = new System.Drawing.Point(3, 75);
            this.chkModifier4.Name = "chkModifier4";
            this.chkModifier4.Size = new System.Drawing.Size(100, 17);
            this.chkModifier4.TabIndex = 3;
            this.chkModifier4.Text = "[Modifier Name]";
            this.chkModifier4.UseVisualStyleBackColor = true;
            this.chkModifier4.CheckedChanged += new System.EventHandler(this.chkModifier_CheckedChanged);
            // 
            // chkModifier3
            // 
            this.chkModifier3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkModifier3.AutoSize = true;
            this.chkModifier3.Location = new System.Drawing.Point(3, 4);
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
            this.chkModifier2.Location = new System.Drawing.Point(3, 26);
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
            this.chkModifier1.Location = new System.Drawing.Point(3, 3);
            this.chkModifier1.Name = "chkModifier1";
            this.chkModifier1.Size = new System.Drawing.Size(100, 17);
            this.chkModifier1.TabIndex = 0;
            this.chkModifier1.Text = "[Modifier Name]";
            this.chkModifier1.UseVisualStyleBackColor = true;
            this.chkModifier1.CheckedChanged += new System.EventHandler(this.chkModifier_CheckedChanged);
            // 
            // lblName
            // 
            this.lblName.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(37, 6);
            this.lblName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(38, 13);
            this.lblName.TabIndex = 0;
            this.lblName.Tag = "Label_Name";
            this.lblName.Text = "Name:";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtName
            // 
            this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMain.SetColumnSpan(this.txtName, 2);
            this.txtName.Location = new System.Drawing.Point(81, 3);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(430, 20);
            this.txtName.TabIndex = 1;
            // 
            // chkArea
            // 
            this.chkArea.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkArea.AutoSize = true;
            this.chkArea.Location = new System.Drawing.Point(299, 85);
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
            this.chkLimited.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkLimited.AutoSize = true;
            this.chkLimited.Location = new System.Drawing.Point(299, 31);
            this.chkLimited.Name = "chkLimited";
            this.chkLimited.Size = new System.Drawing.Size(85, 17);
            this.chkLimited.TabIndex = 20;
            this.chkLimited.Tag = "Checkbox_SelectSpell_LimitedSpell";
            this.chkLimited.Text = "Limited Spell";
            this.chkLimited.UseVisualStyleBackColor = true;
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.ColumnCount = 4;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpMain.Controls.Add(this.chkLimited, 2, 1);
            this.tlpMain.Controls.Add(this.lblSpellOptions, 0, 6);
            this.tlpMain.Controls.Add(this.txtName, 1, 0);
            this.tlpMain.Controls.Add(this.lblName, 0, 0);
            this.tlpMain.Controls.Add(this.lblCategory, 0, 1);
            this.tlpMain.Controls.Add(this.cboDuration, 1, 5);
            this.tlpMain.Controls.Add(this.lblDuration, 0, 5);
            this.tlpMain.Controls.Add(this.cboCategory, 1, 1);
            this.tlpMain.Controls.Add(this.lblType, 0, 2);
            this.tlpMain.Controls.Add(this.cboType, 1, 2);
            this.tlpMain.Controls.Add(this.lblRange, 0, 3);
            this.tlpMain.Controls.Add(this.cboRange, 1, 3);
            this.tlpMain.Controls.Add(this.chkArea, 2, 3);
            this.tlpMain.Controls.Add(this.flowLayoutPanel1, 1, 4);
            this.tlpMain.Controls.Add(this.flowLayoutPanel2, 0, 7);
            this.tlpMain.Controls.Add(this.flowLayoutPanel3, 3, 1);
            this.tlpMain.Controls.Add(this.flpModifiers, 1, 6);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 8;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(734, 519);
            this.tlpMain.TabIndex = 21;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.SetColumnSpan(this.flowLayoutPanel1, 3);
            this.flowLayoutPanel1.Controls.Add(this.chkRestricted);
            this.flowLayoutPanel1.Controls.Add(this.chkVeryRestricted);
            this.flowLayoutPanel1.Controls.Add(this.txtRestriction);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(78, 107);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(446, 26);
            this.flowLayoutPanel1.TabIndex = 21;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.SetColumnSpan(this.flowLayoutPanel2, 4);
            this.flowLayoutPanel2.Controls.Add(this.cmdOK);
            this.flowLayoutPanel2.Controls.Add(this.cmdCancel);
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(575, 493);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(156, 23);
            this.flowLayoutPanel2.TabIndex = 22;
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flowLayoutPanel3.AutoSize = true;
            this.flowLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel3.Controls.Add(this.lblDVLabel);
            this.flowLayoutPanel3.Controls.Add(this.lblDV);
            this.flowLayoutPanel3.Location = new System.Drawing.Point(514, 33);
            this.flowLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(56, 13);
            this.flowLayoutPanel3.TabIndex = 23;
            // 
            // flpModifiers
            // 
            this.flpModifiers.AutoScroll = true;
            this.flpModifiers.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.flpModifiers, 3);
            this.flpModifiers.Controls.Add(this.chkModifier1);
            this.flpModifiers.Controls.Add(this.chkModifier2);
            this.flpModifiers.Controls.Add(this.flpModifier3);
            this.flpModifiers.Controls.Add(this.chkModifier4);
            this.flpModifiers.Controls.Add(this.chkModifier5);
            this.flpModifiers.Controls.Add(this.chkModifier6);
            this.flpModifiers.Controls.Add(this.chkModifier7);
            this.flpModifiers.Controls.Add(this.chkModifier8);
            this.flpModifiers.Controls.Add(this.chkModifier9);
            this.flpModifiers.Controls.Add(this.chkModifier10);
            this.flpModifiers.Controls.Add(this.chkModifier11);
            this.flpModifiers.Controls.Add(this.chkModifier12);
            this.flpModifiers.Controls.Add(this.chkModifier13);
            this.flpModifiers.Controls.Add(this.chkModifier14);
            this.flpModifiers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpModifiers.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpModifiers.Location = new System.Drawing.Point(78, 160);
            this.flpModifiers.Margin = new System.Windows.Forms.Padding(0);
            this.flpModifiers.Name = "flpModifiers";
            this.flpModifiers.Size = new System.Drawing.Size(656, 330);
            this.flpModifiers.TabIndex = 24;
            this.flpModifiers.WrapContents = false;
            // 
            // flpModifier3
            // 
            this.flpModifier3.AutoSize = true;
            this.flpModifier3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpModifier3.Controls.Add(this.chkModifier3);
            this.flpModifier3.Controls.Add(this.nudNumberOfEffects);
            this.flpModifier3.Location = new System.Drawing.Point(0, 46);
            this.flpModifier3.Margin = new System.Windows.Forms.Padding(0);
            this.flpModifier3.Name = "flpModifier3";
            this.flpModifier3.Size = new System.Drawing.Size(154, 26);
            this.flpModifier3.TabIndex = 2;
            this.flpModifier3.WrapContents = false;
            // 
            // frmCreateSpell
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(752, 537);
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmCreateSpell";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_CreateSpell";
            this.Text = "Create Spell";
            this.Load += new System.EventHandler(this.frmCreateSpell_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudNumberOfEffects)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel3.PerformLayout();
            this.flpModifiers.ResumeLayout(false);
            this.flpModifiers.PerformLayout();
            this.flpModifier3.ResumeLayout(false);
            this.flpModifier3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Label lblCategory;
        private ElasticComboBox cboCategory;
        private System.Windows.Forms.Label lblType;
        private ElasticComboBox cboType;
        private System.Windows.Forms.Label lblRange;
        private ElasticComboBox cboRange;
        private System.Windows.Forms.CheckBox chkRestricted;
        private System.Windows.Forms.CheckBox chkVeryRestricted;
        private System.Windows.Forms.TextBox txtRestriction;
        private System.Windows.Forms.Label lblDuration;
        private ElasticComboBox cboDuration;
        private System.Windows.Forms.Label lblDVLabel;
        private System.Windows.Forms.Label lblDV;
        private System.Windows.Forms.Label lblSpellOptions;
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
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.FlowLayoutPanel flpModifiers;
        private System.Windows.Forms.FlowLayoutPanel flpModifier3;
    }
}
