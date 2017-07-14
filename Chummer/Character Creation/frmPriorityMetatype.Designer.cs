namespace Chummer
{
    partial class frmPriorityMetatype
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
            this.components = new System.ComponentModel.Container();
            this.pnlPriorities = new System.Windows.Forms.Panel();
            this.lblSumtoTen = new System.Windows.Forms.Label();
            this.cboResources = new System.Windows.Forms.ComboBox();
            this.lblResourcesLabel = new System.Windows.Forms.Label();
            this.cboSkills = new System.Windows.Forms.ComboBox();
            this.lblSkillsLabel = new System.Windows.Forms.Label();
            this.cboAttributes = new System.Windows.Forms.ComboBox();
            this.lblAttributesLabel = new System.Windows.Forms.Label();
            this.cboTalent = new System.Windows.Forms.ComboBox();
            this.lblTalentLabel = new System.Windows.Forms.Label();
            this.cboHeritage = new System.Windows.Forms.ComboBox();
            this.lblHeritageLabel = new System.Windows.Forms.Label();
            this.chkBloodSpirit = new System.Windows.Forms.CheckBox();
            this.cboPossessionMethod = new System.Windows.Forms.ComboBox();
            this.chkPossessionBased = new System.Windows.Forms.CheckBox();
            this.nudForce = new System.Windows.Forms.NumericUpDown();
            this.lblForceLabel = new System.Windows.Forms.Label();
            this.cboCategory = new System.Windows.Forms.ComboBox();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.lblMetavariantQualities = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cboMetavariant = new System.Windows.Forms.ComboBox();
            this.cmdOK = new System.Windows.Forms.Button();
            this.Label19 = new System.Windows.Forms.Label();
            this.lblINI = new System.Windows.Forms.Label();
            this.Label17 = new System.Windows.Forms.Label();
            this.lblWIL = new System.Windows.Forms.Label();
            this.Label15 = new System.Windows.Forms.Label();
            this.lblLOG = new System.Windows.Forms.Label();
            this.Label13 = new System.Windows.Forms.Label();
            this.lblINT = new System.Windows.Forms.Label();
            this.Label11 = new System.Windows.Forms.Label();
            this.lblCHA = new System.Windows.Forms.Label();
            this.lblSTR = new System.Windows.Forms.Label();
            this.Label9 = new System.Windows.Forms.Label();
            this.lblREA = new System.Windows.Forms.Label();
            this.Label7 = new System.Windows.Forms.Label();
            this.lblAGI = new System.Windows.Forms.Label();
            this.Label5 = new System.Windows.Forms.Label();
            this.lblBOD = new System.Windows.Forms.Label();
            this.Label3 = new System.Windows.Forms.Label();
            this.lstMetatypes = new System.Windows.Forms.ListBox();
            this.pnlMetatypes = new System.Windows.Forms.Panel();
            this.lblMetavariantBP = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblMetatypeSkillSelection = new System.Windows.Forms.Label();
            this.cboSkill1 = new System.Windows.Forms.ComboBox();
            this.cboSkill2 = new System.Windows.Forms.ComboBox();
            this.cboTalents = new System.Windows.Forms.ComboBox();
            this.lblSpecial = new System.Windows.Forms.Label();
            this.lblSpecialAttributes = new System.Windows.Forms.Label();
            this.tipTooltip = new TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip();
            this.pnlPriorities.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudForce)).BeginInit();
            this.pnlMetatypes.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlPriorities
            // 
            this.pnlPriorities.Controls.Add(this.lblSumtoTen);
            this.pnlPriorities.Controls.Add(this.cboResources);
            this.pnlPriorities.Controls.Add(this.lblResourcesLabel);
            this.pnlPriorities.Controls.Add(this.cboSkills);
            this.pnlPriorities.Controls.Add(this.lblSkillsLabel);
            this.pnlPriorities.Controls.Add(this.cboAttributes);
            this.pnlPriorities.Controls.Add(this.lblAttributesLabel);
            this.pnlPriorities.Controls.Add(this.cboTalent);
            this.pnlPriorities.Controls.Add(this.lblTalentLabel);
            this.pnlPriorities.Controls.Add(this.cboHeritage);
            this.pnlPriorities.Controls.Add(this.lblHeritageLabel);
            this.pnlPriorities.Location = new System.Drawing.Point(8, 8);
            this.pnlPriorities.Name = "pnlPriorities";
            this.pnlPriorities.Size = new System.Drawing.Size(472, 139);
            this.pnlPriorities.TabIndex = 104;
            // 
            // lblSumtoTen
            // 
            this.lblSumtoTen.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSumtoTen.Location = new System.Drawing.Point(94, 85);
            this.lblSumtoTen.Name = "lblSumtoTen";
            this.lblSumtoTen.Size = new System.Drawing.Size(82, 45);
            this.lblSumtoTen.TabIndex = 111;
            this.lblSumtoTen.Text = "0/10";
            this.lblSumtoTen.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblSumtoTen.Visible = false;
            // 
            // cboResources
            // 
            this.cboResources.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboResources.FormattingEnabled = true;
            this.cboResources.Location = new System.Drawing.Point(182, 109);
            this.cboResources.Name = "cboResources";
            this.cboResources.Size = new System.Drawing.Size(286, 21);
            this.cboResources.TabIndex = 5;
            this.cboResources.SelectedIndexChanged += new System.EventHandler(this.cboResources_SelectedIndexChanged);
            // 
            // lblResourcesLabel
            // 
            this.lblResourcesLabel.AutoSize = true;
            this.lblResourcesLabel.Location = new System.Drawing.Point(1, 112);
            this.lblResourcesLabel.Name = "lblResourcesLabel";
            this.lblResourcesLabel.Size = new System.Drawing.Size(61, 13);
            this.lblResourcesLabel.TabIndex = 110;
            this.lblResourcesLabel.Tag = "Label_PriorityResources";
            this.lblResourcesLabel.Text = "Resources:";
            // 
            // cboSkills
            // 
            this.cboSkills.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSkills.FormattingEnabled = true;
            this.cboSkills.Location = new System.Drawing.Point(182, 82);
            this.cboSkills.Name = "cboSkills";
            this.cboSkills.Size = new System.Drawing.Size(286, 21);
            this.cboSkills.TabIndex = 4;
            this.cboSkills.SelectedIndexChanged += new System.EventHandler(this.cboSkills_SelectedIndexChanged);
            // 
            // lblSkillsLabel
            // 
            this.lblSkillsLabel.AutoSize = true;
            this.lblSkillsLabel.Location = new System.Drawing.Point(1, 85);
            this.lblSkillsLabel.Name = "lblSkillsLabel";
            this.lblSkillsLabel.Size = new System.Drawing.Size(31, 13);
            this.lblSkillsLabel.TabIndex = 108;
            this.lblSkillsLabel.Tag = "Label_PrioritySkills";
            this.lblSkillsLabel.Text = "Skills";
            // 
            // cboAttributes
            // 
            this.cboAttributes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAttributes.FormattingEnabled = true;
            this.cboAttributes.Location = new System.Drawing.Point(182, 28);
            this.cboAttributes.Name = "cboAttributes";
            this.cboAttributes.Size = new System.Drawing.Size(286, 21);
            this.cboAttributes.TabIndex = 2;
            this.cboAttributes.SelectedIndexChanged += new System.EventHandler(this.cboAttributes_SelectedIndexChanged);
            // 
            // lblAttributesLabel
            // 
            this.lblAttributesLabel.AutoSize = true;
            this.lblAttributesLabel.Location = new System.Drawing.Point(1, 31);
            this.lblAttributesLabel.Name = "lblAttributesLabel";
            this.lblAttributesLabel.Size = new System.Drawing.Size(54, 13);
            this.lblAttributesLabel.TabIndex = 106;
            this.lblAttributesLabel.Tag = "Label_PriorityAttributes";
            this.lblAttributesLabel.Text = "Attributes:";
            // 
            // cboTalent
            // 
            this.cboTalent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTalent.FormattingEnabled = true;
            this.cboTalent.Location = new System.Drawing.Point(182, 55);
            this.cboTalent.Name = "cboTalent";
            this.cboTalent.Size = new System.Drawing.Size(286, 21);
            this.cboTalent.TabIndex = 3;
            this.cboTalent.SelectedIndexChanged += new System.EventHandler(this.cboTalent_SelectedIndexChanged);
            // 
            // lblTalentLabel
            // 
            this.lblTalentLabel.AutoSize = true;
            this.lblTalentLabel.Location = new System.Drawing.Point(1, 58);
            this.lblTalentLabel.Name = "lblTalentLabel";
            this.lblTalentLabel.Size = new System.Drawing.Size(109, 13);
            this.lblTalentLabel.TabIndex = 104;
            this.lblTalentLabel.Tag = "Label_PriorityTalent";
            this.lblTalentLabel.Text = "Magic or Resonance:";
            // 
            // cboHeritage
            // 
            this.cboHeritage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboHeritage.FormattingEnabled = true;
            this.cboHeritage.Location = new System.Drawing.Point(182, 1);
            this.cboHeritage.Name = "cboHeritage";
            this.cboHeritage.Size = new System.Drawing.Size(286, 21);
            this.cboHeritage.TabIndex = 1;
            this.cboHeritage.SelectedIndexChanged += new System.EventHandler(this.cboHeritage_SelectedIndexChanged);
            // 
            // lblHeritageLabel
            // 
            this.lblHeritageLabel.AutoSize = true;
            this.lblHeritageLabel.Location = new System.Drawing.Point(1, 4);
            this.lblHeritageLabel.Name = "lblHeritageLabel";
            this.lblHeritageLabel.Size = new System.Drawing.Size(54, 13);
            this.lblHeritageLabel.TabIndex = 102;
            this.lblHeritageLabel.Tag = "Label_PriorityHeritage";
            this.lblHeritageLabel.Text = "Metatype:";
            // 
            // chkBloodSpirit
            // 
            this.chkBloodSpirit.AutoSize = true;
            this.chkBloodSpirit.Location = new System.Drawing.Point(173, 250);
            this.chkBloodSpirit.Name = "chkBloodSpirit";
            this.chkBloodSpirit.Size = new System.Drawing.Size(79, 17);
            this.chkBloodSpirit.TabIndex = 69;
            this.chkBloodSpirit.Tag = "Checkbox_Metatype_BloodSpirit";
            this.chkBloodSpirit.Text = "Blood Spirit";
            this.chkBloodSpirit.UseVisualStyleBackColor = true;
            this.chkBloodSpirit.Visible = false;
            // 
            // cboPossessionMethod
            // 
            this.cboPossessionMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPossessionMethod.Enabled = false;
            this.cboPossessionMethod.FormattingEnabled = true;
            this.cboPossessionMethod.Location = new System.Drawing.Point(173, 296);
            this.cboPossessionMethod.Name = "cboPossessionMethod";
            this.cboPossessionMethod.Size = new System.Drawing.Size(174, 21);
            this.cboPossessionMethod.TabIndex = 65;
            this.cboPossessionMethod.Visible = false;
            // 
            // chkPossessionBased
            // 
            this.chkPossessionBased.AutoSize = true;
            this.chkPossessionBased.Location = new System.Drawing.Point(173, 273);
            this.chkPossessionBased.Name = "chkPossessionBased";
            this.chkPossessionBased.Size = new System.Drawing.Size(211, 17);
            this.chkPossessionBased.TabIndex = 64;
            this.chkPossessionBased.Tag = "Checkbox_Metatype_PossessionTradition";
            this.chkPossessionBased.Text = "Summoned by Possess-based Tradition";
            this.chkPossessionBased.UseVisualStyleBackColor = true;
            this.chkPossessionBased.Visible = false;
            // 
            // nudForce
            // 
            this.nudForce.Location = new System.Drawing.Point(407, 86);
            this.nudForce.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nudForce.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudForce.Name = "nudForce";
            this.nudForce.Size = new System.Drawing.Size(42, 20);
            this.nudForce.TabIndex = 57;
            this.nudForce.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudForce.Visible = false;
            // 
            // lblForceLabel
            // 
            this.lblForceLabel.AutoSize = true;
            this.lblForceLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblForceLabel.Location = new System.Drawing.Point(404, 75);
            this.lblForceLabel.Name = "lblForceLabel";
            this.lblForceLabel.Size = new System.Drawing.Size(48, 13);
            this.lblForceLabel.TabIndex = 56;
            this.lblForceLabel.Tag = "String_Force";
            this.lblForceLabel.Text = "FORCE";
            this.lblForceLabel.Visible = false;
            // 
            // cboCategory
            // 
            this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategory.FormattingEnabled = true;
            this.cboCategory.Location = new System.Drawing.Point(3, 3);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(158, 21);
            this.cboCategory.TabIndex = 6;
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(313, 323);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 12;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // lblMetavariantQualities
            // 
            this.lblMetavariantQualities.AutoSize = true;
            this.lblMetavariantQualities.Location = new System.Drawing.Point(286, 230);
            this.lblMetavariantQualities.Name = "lblMetavariantQualities";
            this.lblMetavariantQualities.Size = new System.Drawing.Size(33, 13);
            this.lblMetavariantQualities.TabIndex = 63;
            this.lblMetavariantQualities.Tag = "String_None";
            this.lblMetavariantQualities.Text = "None";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(170, 230);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 62;
            this.label4.Tag = "String_Qualities";
            this.label4.Text = "Qualities";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(353, 114);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 13);
            this.label2.TabIndex = 58;
            this.label2.Tag = "Label_Metavariant";
            this.label2.Text = "Metavariant";
            // 
            // cboMetavariant
            // 
            this.cboMetavariant.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMetavariant.DropDownWidth = 69;
            this.cboMetavariant.FormattingEnabled = true;
            this.cboMetavariant.Location = new System.Drawing.Point(356, 127);
            this.cboMetavariant.Name = "cboMetavariant";
            this.cboMetavariant.Size = new System.Drawing.Size(69, 21);
            this.cboMetavariant.TabIndex = 59;
            this.cboMetavariant.SelectedIndexChanged += new System.EventHandler(this.cboMetavariant_SelectedIndexChanged);
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.Location = new System.Drawing.Point(394, 323);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 11;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // Label19
            // 
            this.Label19.AutoSize = true;
            this.Label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label19.Location = new System.Drawing.Point(353, 75);
            this.Label19.Name = "Label19";
            this.Label19.Size = new System.Drawing.Size(24, 13);
            this.Label19.TabIndex = 54;
            this.Label19.Tag = "String_AttributeINIShort";
            this.Label19.Text = "INI";
            // 
            // lblINI
            // 
            this.lblINI.AutoSize = true;
            this.lblINI.Location = new System.Drawing.Point(353, 88);
            this.lblINI.Name = "lblINI";
            this.lblINI.Size = new System.Drawing.Size(51, 13);
            this.lblINI.TabIndex = 55;
            this.lblINI.Text = "2/12 (18)";
            // 
            // Label17
            // 
            this.Label17.AutoSize = true;
            this.Label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label17.Location = new System.Drawing.Point(296, 75);
            this.Label17.Name = "Label17";
            this.Label17.Size = new System.Drawing.Size(30, 13);
            this.Label17.TabIndex = 52;
            this.Label17.Tag = "String_AttributeWILShort";
            this.Label17.Text = "WIL";
            // 
            // lblWIL
            // 
            this.lblWIL.AutoSize = true;
            this.lblWIL.Location = new System.Drawing.Point(296, 88);
            this.lblWIL.Name = "lblWIL";
            this.lblWIL.Size = new System.Drawing.Size(51, 13);
            this.lblWIL.TabIndex = 53;
            this.lblWIL.Text = "2/12 (18)";
            // 
            // Label15
            // 
            this.Label15.AutoSize = true;
            this.Label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label15.Location = new System.Drawing.Point(239, 75);
            this.Label15.Name = "Label15";
            this.Label15.Size = new System.Drawing.Size(32, 13);
            this.Label15.TabIndex = 50;
            this.Label15.Tag = "String_AttributeLOGShort";
            this.Label15.Text = "LOG";
            // 
            // lblLOG
            // 
            this.lblLOG.AutoSize = true;
            this.lblLOG.Location = new System.Drawing.Point(239, 88);
            this.lblLOG.Name = "lblLOG";
            this.lblLOG.Size = new System.Drawing.Size(51, 13);
            this.lblLOG.TabIndex = 51;
            this.lblLOG.Text = "2/12 (18)";
            // 
            // Label13
            // 
            this.Label13.AutoSize = true;
            this.Label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label13.Location = new System.Drawing.Point(182, 75);
            this.Label13.Name = "Label13";
            this.Label13.Size = new System.Drawing.Size(28, 13);
            this.Label13.TabIndex = 48;
            this.Label13.Tag = "String_AttributeINTShort";
            this.Label13.Text = "INT";
            // 
            // lblINT
            // 
            this.lblINT.AutoSize = true;
            this.lblINT.Location = new System.Drawing.Point(182, 88);
            this.lblINT.Name = "lblINT";
            this.lblINT.Size = new System.Drawing.Size(51, 13);
            this.lblINT.TabIndex = 49;
            this.lblINT.Text = "2/12 (18)";
            // 
            // Label11
            // 
            this.Label11.AutoSize = true;
            this.Label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label11.Location = new System.Drawing.Point(404, 39);
            this.Label11.Name = "Label11";
            this.Label11.Size = new System.Drawing.Size(32, 13);
            this.Label11.TabIndex = 46;
            this.Label11.Tag = "String_AttributeCHAShort";
            this.Label11.Text = "CHA";
            // 
            // lblCHA
            // 
            this.lblCHA.AutoSize = true;
            this.lblCHA.Location = new System.Drawing.Point(404, 52);
            this.lblCHA.Name = "lblCHA";
            this.lblCHA.Size = new System.Drawing.Size(51, 13);
            this.lblCHA.TabIndex = 47;
            this.lblCHA.Text = "2/12 (18)";
            // 
            // lblSTR
            // 
            this.lblSTR.AutoSize = true;
            this.lblSTR.Location = new System.Drawing.Point(353, 52);
            this.lblSTR.Name = "lblSTR";
            this.lblSTR.Size = new System.Drawing.Size(51, 13);
            this.lblSTR.TabIndex = 45;
            this.lblSTR.Text = "2/12 (18)";
            // 
            // Label9
            // 
            this.Label9.AutoSize = true;
            this.Label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label9.Location = new System.Drawing.Point(353, 39);
            this.Label9.Name = "Label9";
            this.Label9.Size = new System.Drawing.Size(32, 13);
            this.Label9.TabIndex = 44;
            this.Label9.Tag = "String_AttributeSTRShort";
            this.Label9.Text = "STR";
            // 
            // lblREA
            // 
            this.lblREA.AutoSize = true;
            this.lblREA.Location = new System.Drawing.Point(296, 52);
            this.lblREA.Name = "lblREA";
            this.lblREA.Size = new System.Drawing.Size(51, 13);
            this.lblREA.TabIndex = 43;
            this.lblREA.Text = "2/12 (18)";
            // 
            // Label7
            // 
            this.Label7.AutoSize = true;
            this.Label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label7.Location = new System.Drawing.Point(296, 39);
            this.Label7.Name = "Label7";
            this.Label7.Size = new System.Drawing.Size(32, 13);
            this.Label7.TabIndex = 42;
            this.Label7.Tag = "String_AttributeREAShort";
            this.Label7.Text = "REA";
            // 
            // lblAGI
            // 
            this.lblAGI.AutoSize = true;
            this.lblAGI.Location = new System.Drawing.Point(239, 52);
            this.lblAGI.Name = "lblAGI";
            this.lblAGI.Size = new System.Drawing.Size(51, 13);
            this.lblAGI.TabIndex = 41;
            this.lblAGI.Text = "2/12 (18)";
            // 
            // Label5
            // 
            this.Label5.AutoSize = true;
            this.Label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label5.Location = new System.Drawing.Point(239, 39);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(28, 13);
            this.Label5.TabIndex = 40;
            this.Label5.Tag = "String_AttributeAGIShort";
            this.Label5.Text = "AGI";
            // 
            // lblBOD
            // 
            this.lblBOD.AutoSize = true;
            this.lblBOD.Location = new System.Drawing.Point(182, 52);
            this.lblBOD.Name = "lblBOD";
            this.lblBOD.Size = new System.Drawing.Size(51, 13);
            this.lblBOD.TabIndex = 39;
            this.lblBOD.Text = "2/12 (18)";
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label3.Location = new System.Drawing.Point(182, 39);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(33, 13);
            this.Label3.TabIndex = 38;
            this.Label3.Tag = "String_AttributeBODShort";
            this.Label3.Text = "BOD";
            // 
            // lstMetatypes
            // 
            this.lstMetatypes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstMetatypes.FormattingEnabled = true;
            this.lstMetatypes.Location = new System.Drawing.Point(3, 30);
            this.lstMetatypes.Name = "lstMetatypes";
            this.lstMetatypes.Size = new System.Drawing.Size(158, 316);
            this.lstMetatypes.Sorted = true;
            this.lstMetatypes.TabIndex = 7;
            this.lstMetatypes.SelectedIndexChanged += new System.EventHandler(this.lstMetatypes_SelectedIndexChanged);
            this.lstMetatypes.DoubleClick += new System.EventHandler(this.lstMetatypes_DoubleClick);
            // 
            // pnlMetatypes
            // 
            this.pnlMetatypes.Controls.Add(this.lblMetavariantBP);
            this.pnlMetatypes.Controls.Add(this.label6);
            this.pnlMetatypes.Controls.Add(this.lblMetatypeSkillSelection);
            this.pnlMetatypes.Controls.Add(this.cboSkill1);
            this.pnlMetatypes.Controls.Add(this.cboSkill2);
            this.pnlMetatypes.Controls.Add(this.cboTalents);
            this.pnlMetatypes.Controls.Add(this.lblSpecial);
            this.pnlMetatypes.Controls.Add(this.lblSpecialAttributes);
            this.pnlMetatypes.Controls.Add(this.chkBloodSpirit);
            this.pnlMetatypes.Controls.Add(this.cboPossessionMethod);
            this.pnlMetatypes.Controls.Add(this.chkPossessionBased);
            this.pnlMetatypes.Controls.Add(this.nudForce);
            this.pnlMetatypes.Controls.Add(this.lblForceLabel);
            this.pnlMetatypes.Controls.Add(this.cboCategory);
            this.pnlMetatypes.Controls.Add(this.cmdCancel);
            this.pnlMetatypes.Controls.Add(this.lblMetavariantQualities);
            this.pnlMetatypes.Controls.Add(this.label4);
            this.pnlMetatypes.Controls.Add(this.label2);
            this.pnlMetatypes.Controls.Add(this.cboMetavariant);
            this.pnlMetatypes.Controls.Add(this.cmdOK);
            this.pnlMetatypes.Controls.Add(this.Label19);
            this.pnlMetatypes.Controls.Add(this.lblINI);
            this.pnlMetatypes.Controls.Add(this.Label17);
            this.pnlMetatypes.Controls.Add(this.lblWIL);
            this.pnlMetatypes.Controls.Add(this.Label15);
            this.pnlMetatypes.Controls.Add(this.lblLOG);
            this.pnlMetatypes.Controls.Add(this.Label13);
            this.pnlMetatypes.Controls.Add(this.lblINT);
            this.pnlMetatypes.Controls.Add(this.Label11);
            this.pnlMetatypes.Controls.Add(this.lblCHA);
            this.pnlMetatypes.Controls.Add(this.lblSTR);
            this.pnlMetatypes.Controls.Add(this.Label9);
            this.pnlMetatypes.Controls.Add(this.lblREA);
            this.pnlMetatypes.Controls.Add(this.Label7);
            this.pnlMetatypes.Controls.Add(this.lblAGI);
            this.pnlMetatypes.Controls.Add(this.Label5);
            this.pnlMetatypes.Controls.Add(this.lblBOD);
            this.pnlMetatypes.Controls.Add(this.Label3);
            this.pnlMetatypes.Controls.Add(this.lstMetatypes);
            this.pnlMetatypes.Location = new System.Drawing.Point(8, 153);
            this.pnlMetatypes.Name = "pnlMetatypes";
            this.pnlMetatypes.Size = new System.Drawing.Size(472, 354);
            this.pnlMetatypes.TabIndex = 36;
            // 
            // lblMetavariantBP
            // 
            this.lblMetavariantBP.AutoSize = true;
            this.lblMetavariantBP.Location = new System.Drawing.Point(182, 127);
            this.lblMetavariantBP.Name = "lblMetavariantBP";
            this.lblMetavariantBP.Size = new System.Drawing.Size(13, 13);
            this.lblMetavariantBP.TabIndex = 77;
            this.lblMetavariantBP.Text = "0";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(182, 114);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(42, 13);
            this.label6.TabIndex = 76;
            this.label6.Tag = "String_Karma";
            this.label6.Text = "Karma";
            // 
            // lblMetatypeSkillSelection
            // 
            this.lblMetatypeSkillSelection.Location = new System.Drawing.Point(182, 148);
            this.lblMetatypeSkillSelection.Name = "lblMetatypeSkillSelection";
            this.lblMetatypeSkillSelection.Size = new System.Drawing.Size(286, 28);
            this.lblMetatypeSkillSelection.TabIndex = 75;
            this.lblMetatypeSkillSelection.Tag = "String_MetamagicSkillBase";
            this.lblMetatypeSkillSelection.Text = "Based on your talent selection, you may choose 2 magical skills to start at ratin" +
    "g 5.";
            this.lblMetatypeSkillSelection.Visible = false;
            // 
            // cboSkill1
            // 
            this.cboSkill1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSkill1.FormattingEnabled = true;
            this.cboSkill1.Location = new System.Drawing.Point(182, 178);
            this.cboSkill1.Name = "cboSkill1";
            this.cboSkill1.Size = new System.Drawing.Size(286, 21);
            this.cboSkill1.TabIndex = 9;
            this.cboSkill1.Visible = false;
            // 
            // cboSkill2
            // 
            this.cboSkill2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSkill2.FormattingEnabled = true;
            this.cboSkill2.Location = new System.Drawing.Point(182, 205);
            this.cboSkill2.Name = "cboSkill2";
            this.cboSkill2.Size = new System.Drawing.Size(286, 21);
            this.cboSkill2.TabIndex = 10;
            this.cboSkill2.Visible = false;
            // 
            // cboTalents
            // 
            this.cboTalents.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTalents.FormattingEnabled = true;
            this.cboTalents.Location = new System.Drawing.Point(182, 3);
            this.cboTalents.Name = "cboTalents";
            this.cboTalents.Size = new System.Drawing.Size(286, 21);
            this.cboTalents.TabIndex = 8;
            this.cboTalents.SelectedIndexChanged += new System.EventHandler(this.cboTalents_SelectedIndexChanged);
            // 
            // lblSpecial
            // 
            this.lblSpecial.AutoSize = true;
            this.lblSpecial.Location = new System.Drawing.Point(239, 127);
            this.lblSpecial.Name = "lblSpecial";
            this.lblSpecial.Size = new System.Drawing.Size(13, 13);
            this.lblSpecial.TabIndex = 71;
            this.lblSpecial.Text = "0";
            // 
            // lblSpecialAttributes
            // 
            this.lblSpecialAttributes.AutoSize = true;
            this.lblSpecialAttributes.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpecialAttributes.Location = new System.Drawing.Point(239, 114);
            this.lblSpecialAttributes.Name = "lblSpecialAttributes";
            this.lblSpecialAttributes.Size = new System.Drawing.Size(107, 13);
            this.lblSpecialAttributes.TabIndex = 70;
            this.lblSpecialAttributes.Tag = "String_Special";
            this.lblSpecialAttributes.Text = "Special Attributes";
            // 
            // tipTooltip
            // 
            this.tipTooltip.AutoPopDelay = 10000;
            this.tipTooltip.InitialDelay = 250;
            this.tipTooltip.IsBalloon = true;
            this.tipTooltip.ReshowDelay = 100;
            this.tipTooltip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.tipTooltip.ToolTipTitle = "Chummer Help";
            // 
            // frmPriorityMetatype
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(488, 514);
            this.ControlBox = false;
            this.Controls.Add(this.pnlPriorities);
            this.Controls.Add(this.pnlMetatypes);
            this.Name = "frmPriorityMetatype";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Choose Character Priorities";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmPriorityMetatype_FormClosed);
            this.Load += new System.EventHandler(this.frmPriorityMetatype_Load);
            this.pnlPriorities.ResumeLayout(false);
            this.pnlPriorities.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudForce)).EndInit();
            this.pnlMetatypes.ResumeLayout(false);
            this.pnlMetatypes.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        
        internal System.Windows.Forms.Label lblMetavariantBP;
        private System.Windows.Forms.Panel pnlPriorities;
        private System.Windows.Forms.ComboBox cboResources;
        private System.Windows.Forms.Label lblResourcesLabel;
        private System.Windows.Forms.ComboBox cboSkills;
        private System.Windows.Forms.Label lblSkillsLabel;
        private System.Windows.Forms.ComboBox cboAttributes;
        private System.Windows.Forms.Label lblAttributesLabel;
        private System.Windows.Forms.ComboBox cboTalent;
        private System.Windows.Forms.Label lblTalentLabel;
        private System.Windows.Forms.ComboBox cboHeritage;
        private System.Windows.Forms.Label lblHeritageLabel;
        private System.Windows.Forms.CheckBox chkBloodSpirit;
        private System.Windows.Forms.ComboBox cboPossessionMethod;
        private System.Windows.Forms.CheckBox chkPossessionBased;
        private System.Windows.Forms.NumericUpDown nudForce;
        private System.Windows.Forms.Label lblForceLabel;
        private System.Windows.Forms.ComboBox cboCategory;
        internal System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Label lblMetavariantQualities;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboMetavariant;
        internal System.Windows.Forms.Button cmdOK;
        internal System.Windows.Forms.Label Label19;
        internal System.Windows.Forms.Label lblINI;
        internal System.Windows.Forms.Label Label17;
        internal System.Windows.Forms.Label lblWIL;
        internal System.Windows.Forms.Label Label15;
        internal System.Windows.Forms.Label lblLOG;
        internal System.Windows.Forms.Label Label13;
        internal System.Windows.Forms.Label lblINT;
        internal System.Windows.Forms.Label Label11;
        internal System.Windows.Forms.Label lblCHA;
        internal System.Windows.Forms.Label lblSTR;
        internal System.Windows.Forms.Label Label9;
        internal System.Windows.Forms.Label lblREA;
        internal System.Windows.Forms.Label Label7;
        internal System.Windows.Forms.Label lblAGI;
        internal System.Windows.Forms.Label Label5;
        internal System.Windows.Forms.Label lblBOD;
        internal System.Windows.Forms.Label Label3;
        internal System.Windows.Forms.ListBox lstMetatypes;

        private System.Windows.Forms.Panel pnlMetatypes;
        private TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip tipTooltip;
        internal System.Windows.Forms.Label lblSpecial;
        internal System.Windows.Forms.Label lblSpecialAttributes;
        private System.Windows.Forms.ComboBox cboTalents;
        private System.Windows.Forms.Label lblMetatypeSkillSelection;
        private System.Windows.Forms.ComboBox cboSkill1;
        private System.Windows.Forms.ComboBox cboSkill2;
        internal System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblSumtoTen;
    }
}