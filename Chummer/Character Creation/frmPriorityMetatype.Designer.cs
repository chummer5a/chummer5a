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
            this.nudForce = new System.Windows.Forms.NumericUpDown();
            this.lblForceLabel = new System.Windows.Forms.Label();
            this.cboCategory = new Chummer.ElasticComboBox();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.lblMetavariantQualities = new System.Windows.Forms.Label();
            this.lblMetavariantQualitiesLabel = new System.Windows.Forms.Label();
            this.lblMetavariantLabel = new System.Windows.Forms.Label();
            this.cboMetavariant = new Chummer.ElasticComboBox();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lblINILabel = new System.Windows.Forms.Label();
            this.lblINI = new System.Windows.Forms.Label();
            this.lblWILLabel = new System.Windows.Forms.Label();
            this.lblWIL = new System.Windows.Forms.Label();
            this.lblLOGLabel = new System.Windows.Forms.Label();
            this.lblLOG = new System.Windows.Forms.Label();
            this.lblINTLabel = new System.Windows.Forms.Label();
            this.lblINT = new System.Windows.Forms.Label();
            this.lblCHALabel = new System.Windows.Forms.Label();
            this.lblCHA = new System.Windows.Forms.Label();
            this.lblSTR = new System.Windows.Forms.Label();
            this.lblSTRLabel = new System.Windows.Forms.Label();
            this.lblREA = new System.Windows.Forms.Label();
            this.lblREALabel = new System.Windows.Forms.Label();
            this.lblAGI = new System.Windows.Forms.Label();
            this.lblAGILabel = new System.Windows.Forms.Label();
            this.lblBOD = new System.Windows.Forms.Label();
            this.lblBODLabel = new System.Windows.Forms.Label();
            this.lstMetatypes = new System.Windows.Forms.ListBox();
            this.tlpBottomHalf = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblMetavariantKarma = new System.Windows.Forms.Label();
            this.lblSpecialAttributes = new System.Windows.Forms.Label();
            this.lblSpecialAttributesLabel = new System.Windows.Forms.Label();
            this.lblMetavariantKarmaLabel = new System.Windows.Forms.Label();
            this.lblMetatypeSkillSelection = new System.Windows.Forms.Label();
            this.cboSkill1 = new Chummer.ElasticComboBox();
            this.cboSkill2 = new Chummer.ElasticComboBox();
            this.cboSkill3 = new Chummer.ElasticComboBox();
            this.flpButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.cboTalents = new Chummer.ElasticComboBox();
            this.tlpTopHalf = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblHeritageLabel = new System.Windows.Forms.Label();
            this.lblAttributesLabel = new System.Windows.Forms.Label();
            this.cboResources = new Chummer.ElasticComboBox();
            this.lblTalentLabel = new System.Windows.Forms.Label();
            this.cboSkills = new Chummer.ElasticComboBox();
            this.lblResourcesLabel = new System.Windows.Forms.Label();
            this.cboTalent = new Chummer.ElasticComboBox();
            this.cboAttributes = new Chummer.ElasticComboBox();
            this.lblSkillsLabel = new System.Windows.Forms.Label();
            this.cboHeritage = new Chummer.ElasticComboBox();
            this.lblSumtoTen = new System.Windows.Forms.Label();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.nudForce)).BeginInit();
            this.tlpBottomHalf.SuspendLayout();
            this.flpButtons.SuspendLayout();
            this.tlpTopHalf.SuspendLayout();
            this.tlpMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // nudForce
            // 
            this.nudForce.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudForce.Location = new System.Drawing.Point(653, 80);
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
            this.nudForce.Size = new System.Drawing.Size(78, 20);
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
            this.lblForceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblForceLabel.AutoSize = true;
            this.lblForceLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblForceLabel.Location = new System.Drawing.Point(653, 58);
            this.lblForceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblForceLabel.Name = "lblForceLabel";
            this.lblForceLabel.Size = new System.Drawing.Size(48, 13);
            this.lblForceLabel.TabIndex = 56;
            this.lblForceLabel.Tag = "String_Force";
            this.lblForceLabel.Text = "FORCE";
            this.lblForceLabel.Visible = false;
            // 
            // cboCategory
            // 
            this.cboCategory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategory.FormattingEnabled = true;
            this.cboCategory.Location = new System.Drawing.Point(3, 3);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(295, 21);
            this.cboCategory.TabIndex = 6;
            this.cboCategory.TooltipText = "";
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
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
            this.cmdCancel.TabIndex = 12;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // lblMetavariantQualities
            // 
            this.lblMetavariantQualities.AutoSize = true;
            this.tlpBottomHalf.SetColumnSpan(this.lblMetavariantQualities, 4);
            this.lblMetavariantQualities.Location = new System.Drawing.Point(407, 199);
            this.lblMetavariantQualities.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetavariantQualities.MaximumSize = new System.Drawing.Size(206, 1000);
            this.lblMetavariantQualities.Name = "lblMetavariantQualities";
            this.lblMetavariantQualities.Size = new System.Drawing.Size(33, 13);
            this.lblMetavariantQualities.TabIndex = 63;
            this.lblMetavariantQualities.Tag = "String_None";
            this.lblMetavariantQualities.Text = "None";
            // 
            // lblMetavariantQualitiesLabel
            // 
            this.lblMetavariantQualitiesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMetavariantQualitiesLabel.AutoSize = true;
            this.tlpBottomHalf.SetColumnSpan(this.lblMetavariantQualitiesLabel, 2);
            this.lblMetavariantQualitiesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMetavariantQualitiesLabel.Location = new System.Drawing.Point(341, 199);
            this.lblMetavariantQualitiesLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetavariantQualitiesLabel.Name = "lblMetavariantQualitiesLabel";
            this.lblMetavariantQualitiesLabel.Size = new System.Drawing.Size(60, 13);
            this.lblMetavariantQualitiesLabel.TabIndex = 62;
            this.lblMetavariantQualitiesLabel.Tag = "Label_Qualities";
            this.lblMetavariantQualitiesLabel.Text = "Qualities:";
            // 
            // lblMetavariantLabel
            // 
            this.lblMetavariantLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMetavariantLabel.AutoSize = true;
            this.tlpBottomHalf.SetColumnSpan(this.lblMetavariantLabel, 2);
            this.lblMetavariantLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMetavariantLabel.Location = new System.Drawing.Point(323, 109);
            this.lblMetavariantLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetavariantLabel.Name = "lblMetavariantLabel";
            this.lblMetavariantLabel.Size = new System.Drawing.Size(78, 13);
            this.lblMetavariantLabel.TabIndex = 58;
            this.lblMetavariantLabel.Tag = "Label_Metavariant";
            this.lblMetavariantLabel.Text = "Metavariant:";
            // 
            // cboMetavariant
            // 
            this.cboMetavariant.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpBottomHalf.SetColumnSpan(this.cboMetavariant, 4);
            this.cboMetavariant.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMetavariant.DropDownWidth = 69;
            this.cboMetavariant.FormattingEnabled = true;
            this.cboMetavariant.Location = new System.Drawing.Point(407, 106);
            this.cboMetavariant.Name = "cboMetavariant";
            this.cboMetavariant.Size = new System.Drawing.Size(324, 21);
            this.cboMetavariant.TabIndex = 59;
            this.cboMetavariant.TooltipText = "";
            this.cboMetavariant.SelectedIndexChanged += new System.EventHandler(this.cboMetavariant_SelectedIndexChanged);
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.AutoSize = true;
            this.cmdOK.Location = new System.Drawing.Point(81, 0);
            this.cmdOK.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 11;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lblINILabel
            // 
            this.lblINILabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblINILabel.AutoSize = true;
            this.lblINILabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblINILabel.Location = new System.Drawing.Point(571, 58);
            this.lblINILabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblINILabel.Name = "lblINILabel";
            this.lblINILabel.Size = new System.Drawing.Size(24, 13);
            this.lblINILabel.TabIndex = 54;
            this.lblINILabel.Tag = "String_AttributeINIShort";
            this.lblINILabel.Text = "INI";
            // 
            // lblINI
            // 
            this.lblINI.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblINI.AutoSize = true;
            this.lblINI.Location = new System.Drawing.Point(571, 83);
            this.lblINI.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblINI.Name = "lblINI";
            this.lblINI.Size = new System.Drawing.Size(51, 14);
            this.lblINI.TabIndex = 55;
            this.lblINI.Text = "2/12 (18)";
            // 
            // lblWILLabel
            // 
            this.lblWILLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblWILLabel.AutoSize = true;
            this.lblWILLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWILLabel.Location = new System.Drawing.Point(489, 58);
            this.lblWILLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWILLabel.Name = "lblWILLabel";
            this.lblWILLabel.Size = new System.Drawing.Size(30, 13);
            this.lblWILLabel.TabIndex = 52;
            this.lblWILLabel.Tag = "String_AttributeWILShort";
            this.lblWILLabel.Text = "WIL";
            // 
            // lblWIL
            // 
            this.lblWIL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblWIL.AutoSize = true;
            this.lblWIL.Location = new System.Drawing.Point(489, 83);
            this.lblWIL.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWIL.Name = "lblWIL";
            this.lblWIL.Size = new System.Drawing.Size(51, 14);
            this.lblWIL.TabIndex = 53;
            this.lblWIL.Text = "2/12 (18)";
            // 
            // lblLOGLabel
            // 
            this.lblLOGLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblLOGLabel.AutoSize = true;
            this.lblLOGLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLOGLabel.Location = new System.Drawing.Point(407, 58);
            this.lblLOGLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLOGLabel.Name = "lblLOGLabel";
            this.lblLOGLabel.Size = new System.Drawing.Size(32, 13);
            this.lblLOGLabel.TabIndex = 50;
            this.lblLOGLabel.Tag = "String_AttributeLOGShort";
            this.lblLOGLabel.Text = "LOG";
            // 
            // lblLOG
            // 
            this.lblLOG.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblLOG.AutoSize = true;
            this.lblLOG.Location = new System.Drawing.Point(407, 83);
            this.lblLOG.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLOG.Name = "lblLOG";
            this.lblLOG.Size = new System.Drawing.Size(51, 14);
            this.lblLOG.TabIndex = 51;
            this.lblLOG.Text = "2/12 (18)";
            // 
            // lblINTLabel
            // 
            this.lblINTLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblINTLabel.AutoSize = true;
            this.lblINTLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblINTLabel.Location = new System.Drawing.Point(325, 58);
            this.lblINTLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblINTLabel.Name = "lblINTLabel";
            this.lblINTLabel.Size = new System.Drawing.Size(28, 13);
            this.lblINTLabel.TabIndex = 48;
            this.lblINTLabel.Tag = "String_AttributeINTShort";
            this.lblINTLabel.Text = "INT";
            // 
            // lblINT
            // 
            this.lblINT.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblINT.AutoSize = true;
            this.lblINT.Location = new System.Drawing.Point(325, 83);
            this.lblINT.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblINT.Name = "lblINT";
            this.lblINT.Size = new System.Drawing.Size(51, 14);
            this.lblINT.TabIndex = 49;
            this.lblINT.Text = "2/12 (18)";
            // 
            // lblCHALabel
            // 
            this.lblCHALabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblCHALabel.AutoSize = true;
            this.lblCHALabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCHALabel.Location = new System.Drawing.Point(653, 8);
            this.lblCHALabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCHALabel.Name = "lblCHALabel";
            this.lblCHALabel.Size = new System.Drawing.Size(32, 13);
            this.lblCHALabel.TabIndex = 46;
            this.lblCHALabel.Tag = "String_AttributeCHAShort";
            this.lblCHALabel.Text = "CHA";
            // 
            // lblCHA
            // 
            this.lblCHA.AutoSize = true;
            this.lblCHA.Location = new System.Drawing.Point(653, 33);
            this.lblCHA.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCHA.Name = "lblCHA";
            this.lblCHA.Size = new System.Drawing.Size(51, 13);
            this.lblCHA.TabIndex = 47;
            this.lblCHA.Text = "2/12 (18)";
            // 
            // lblSTR
            // 
            this.lblSTR.AutoSize = true;
            this.lblSTR.Location = new System.Drawing.Point(571, 33);
            this.lblSTR.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSTR.Name = "lblSTR";
            this.lblSTR.Size = new System.Drawing.Size(51, 13);
            this.lblSTR.TabIndex = 45;
            this.lblSTR.Text = "2/12 (18)";
            // 
            // lblSTRLabel
            // 
            this.lblSTRLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSTRLabel.AutoSize = true;
            this.lblSTRLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSTRLabel.Location = new System.Drawing.Point(571, 8);
            this.lblSTRLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSTRLabel.Name = "lblSTRLabel";
            this.lblSTRLabel.Size = new System.Drawing.Size(32, 13);
            this.lblSTRLabel.TabIndex = 44;
            this.lblSTRLabel.Tag = "String_AttributeSTRShort";
            this.lblSTRLabel.Text = "STR";
            // 
            // lblREA
            // 
            this.lblREA.AutoSize = true;
            this.lblREA.Location = new System.Drawing.Point(489, 33);
            this.lblREA.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblREA.Name = "lblREA";
            this.lblREA.Size = new System.Drawing.Size(51, 13);
            this.lblREA.TabIndex = 43;
            this.lblREA.Text = "2/12 (18)";
            // 
            // lblREALabel
            // 
            this.lblREALabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblREALabel.AutoSize = true;
            this.lblREALabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblREALabel.Location = new System.Drawing.Point(489, 8);
            this.lblREALabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblREALabel.Name = "lblREALabel";
            this.lblREALabel.Size = new System.Drawing.Size(32, 13);
            this.lblREALabel.TabIndex = 42;
            this.lblREALabel.Tag = "String_AttributeREAShort";
            this.lblREALabel.Text = "REA";
            // 
            // lblAGI
            // 
            this.lblAGI.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblAGI.AutoSize = true;
            this.lblAGI.Location = new System.Drawing.Point(407, 33);
            this.lblAGI.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAGI.Name = "lblAGI";
            this.lblAGI.Size = new System.Drawing.Size(51, 13);
            this.lblAGI.TabIndex = 41;
            this.lblAGI.Text = "2/12 (18)";
            // 
            // lblAGILabel
            // 
            this.lblAGILabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblAGILabel.AutoSize = true;
            this.lblAGILabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAGILabel.Location = new System.Drawing.Point(407, 8);
            this.lblAGILabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAGILabel.Name = "lblAGILabel";
            this.lblAGILabel.Size = new System.Drawing.Size(28, 13);
            this.lblAGILabel.TabIndex = 40;
            this.lblAGILabel.Tag = "String_AttributeAGIShort";
            this.lblAGILabel.Text = "AGI";
            // 
            // lblBOD
            // 
            this.lblBOD.AutoSize = true;
            this.lblBOD.Location = new System.Drawing.Point(325, 33);
            this.lblBOD.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBOD.Name = "lblBOD";
            this.lblBOD.Size = new System.Drawing.Size(51, 13);
            this.lblBOD.TabIndex = 39;
            this.lblBOD.Text = "2/12 (18)";
            // 
            // lblBODLabel
            // 
            this.lblBODLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblBODLabel.AutoSize = true;
            this.lblBODLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBODLabel.Location = new System.Drawing.Point(325, 8);
            this.lblBODLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBODLabel.Name = "lblBODLabel";
            this.lblBODLabel.Size = new System.Drawing.Size(33, 13);
            this.lblBODLabel.TabIndex = 38;
            this.lblBODLabel.Tag = "String_AttributeBODShort";
            this.lblBODLabel.Text = "BOD";
            // 
            // lstMetatypes
            // 
            this.lstMetatypes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstMetatypes.FormattingEnabled = true;
            this.lstMetatypes.Location = new System.Drawing.Point(3, 30);
            this.lstMetatypes.Name = "lstMetatypes";
            this.tlpBottomHalf.SetRowSpan(this.lstMetatypes, 12);
            this.lstMetatypes.Size = new System.Drawing.Size(295, 331);
            this.lstMetatypes.Sorted = true;
            this.lstMetatypes.TabIndex = 7;
            this.lstMetatypes.SelectedIndexChanged += new System.EventHandler(this.lstMetatypes_SelectedIndexChanged);
            this.lstMetatypes.DoubleClick += new System.EventHandler(this.lstMetatypes_DoubleClick);
            // 
            // tlpBottomHalf
            // 
            this.tlpBottomHalf.ColumnCount = 7;
            this.tlpBottomHalf.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 301F));
            this.tlpBottomHalf.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 4.999999F));
            this.tlpBottomHalf.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19F));
            this.tlpBottomHalf.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19F));
            this.tlpBottomHalf.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19F));
            this.tlpBottomHalf.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19F));
            this.tlpBottomHalf.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19F));
            this.tlpBottomHalf.Controls.Add(this.lstMetatypes, 0, 2);
            this.tlpBottomHalf.Controls.Add(this.lblBODLabel, 2, 1);
            this.tlpBottomHalf.Controls.Add(this.lblAGILabel, 3, 1);
            this.tlpBottomHalf.Controls.Add(this.lblREALabel, 4, 1);
            this.tlpBottomHalf.Controls.Add(this.lblSTRLabel, 5, 1);
            this.tlpBottomHalf.Controls.Add(this.lblCHALabel, 6, 1);
            this.tlpBottomHalf.Controls.Add(this.lblBOD, 2, 2);
            this.tlpBottomHalf.Controls.Add(this.lblAGI, 3, 2);
            this.tlpBottomHalf.Controls.Add(this.lblREA, 4, 2);
            this.tlpBottomHalf.Controls.Add(this.lblSTR, 5, 2);
            this.tlpBottomHalf.Controls.Add(this.lblCHA, 6, 2);
            this.tlpBottomHalf.Controls.Add(this.lblINTLabel, 2, 3);
            this.tlpBottomHalf.Controls.Add(this.nudForce, 6, 4);
            this.tlpBottomHalf.Controls.Add(this.lblLOGLabel, 3, 3);
            this.tlpBottomHalf.Controls.Add(this.lblForceLabel, 6, 3);
            this.tlpBottomHalf.Controls.Add(this.lblWILLabel, 4, 3);
            this.tlpBottomHalf.Controls.Add(this.lblINILabel, 5, 3);
            this.tlpBottomHalf.Controls.Add(this.lblINI, 5, 4);
            this.tlpBottomHalf.Controls.Add(this.lblWIL, 4, 4);
            this.tlpBottomHalf.Controls.Add(this.lblINT, 2, 4);
            this.tlpBottomHalf.Controls.Add(this.lblLOG, 3, 4);
            this.tlpBottomHalf.Controls.Add(this.cboMetavariant, 3, 5);
            this.tlpBottomHalf.Controls.Add(this.lblMetavariantKarma, 3, 6);
            this.tlpBottomHalf.Controls.Add(this.lblMetavariantQualities, 3, 8);
            this.tlpBottomHalf.Controls.Add(this.lblSpecialAttributes, 3, 7);
            this.tlpBottomHalf.Controls.Add(this.lblSpecialAttributesLabel, 1, 7);
            this.tlpBottomHalf.Controls.Add(this.lblMetavariantKarmaLabel, 1, 6);
            this.tlpBottomHalf.Controls.Add(this.lblMetavariantLabel, 1, 5);
            this.tlpBottomHalf.Controls.Add(this.lblMetavariantQualitiesLabel, 1, 8);
            this.tlpBottomHalf.Controls.Add(this.lblMetatypeSkillSelection, 1, 9);
            this.tlpBottomHalf.Controls.Add(this.cboSkill1, 1, 10);
            this.tlpBottomHalf.Controls.Add(this.cboSkill2, 1, 11);
            this.tlpBottomHalf.Controls.Add(this.cboSkill3, 1, 12);
            this.tlpBottomHalf.Controls.Add(this.flpButtons, 1, 13);
            this.tlpBottomHalf.Controls.Add(this.cboCategory, 0, 1);
            this.tlpBottomHalf.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBottomHalf.Location = new System.Drawing.Point(0, 155);
            this.tlpBottomHalf.Margin = new System.Windows.Forms.Padding(0, 20, 0, 0);
            this.tlpBottomHalf.Name = "tlpBottomHalf";
            this.tlpBottomHalf.RowCount = 14;
            this.tlpBottomHalf.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBottomHalf.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBottomHalf.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBottomHalf.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBottomHalf.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBottomHalf.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBottomHalf.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBottomHalf.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBottomHalf.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBottomHalf.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBottomHalf.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBottomHalf.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBottomHalf.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBottomHalf.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBottomHalf.Size = new System.Drawing.Size(734, 364);
            this.tlpBottomHalf.TabIndex = 79;
            // 
            // lblMetavariantKarma
            // 
            this.lblMetavariantKarma.AutoSize = true;
            this.tlpBottomHalf.SetColumnSpan(this.lblMetavariantKarma, 4);
            this.lblMetavariantKarma.Location = new System.Drawing.Point(407, 136);
            this.lblMetavariantKarma.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetavariantKarma.Name = "lblMetavariantKarma";
            this.lblMetavariantKarma.Size = new System.Drawing.Size(13, 13);
            this.lblMetavariantKarma.TabIndex = 77;
            this.lblMetavariantKarma.Text = "0";
            // 
            // lblSpecialAttributes
            // 
            this.lblSpecialAttributes.AutoSize = true;
            this.tlpBottomHalf.SetColumnSpan(this.lblSpecialAttributes, 4);
            this.lblSpecialAttributes.Location = new System.Drawing.Point(407, 161);
            this.lblSpecialAttributes.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpecialAttributes.Name = "lblSpecialAttributes";
            this.lblSpecialAttributes.Size = new System.Drawing.Size(13, 13);
            this.lblSpecialAttributes.TabIndex = 71;
            this.lblSpecialAttributes.Text = "0";
            // 
            // lblSpecialAttributesLabel
            // 
            this.lblSpecialAttributesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSpecialAttributesLabel.AutoSize = true;
            this.tlpBottomHalf.SetColumnSpan(this.lblSpecialAttributesLabel, 2);
            this.lblSpecialAttributesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpecialAttributesLabel.Location = new System.Drawing.Point(336, 161);
            this.lblSpecialAttributesLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpecialAttributesLabel.Name = "lblSpecialAttributesLabel";
            this.lblSpecialAttributesLabel.Size = new System.Drawing.Size(65, 26);
            this.lblSpecialAttributesLabel.TabIndex = 70;
            this.lblSpecialAttributesLabel.Tag = "Label_SpecialAttributes";
            this.lblSpecialAttributesLabel.Text = "Special Attributes:";
            // 
            // lblMetavariantKarmaLabel
            // 
            this.lblMetavariantKarmaLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMetavariantKarmaLabel.AutoSize = true;
            this.tlpBottomHalf.SetColumnSpan(this.lblMetavariantKarmaLabel, 2);
            this.lblMetavariantKarmaLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMetavariantKarmaLabel.Location = new System.Drawing.Point(355, 136);
            this.lblMetavariantKarmaLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetavariantKarmaLabel.Name = "lblMetavariantKarmaLabel";
            this.lblMetavariantKarmaLabel.Size = new System.Drawing.Size(46, 13);
            this.lblMetavariantKarmaLabel.TabIndex = 76;
            this.lblMetavariantKarmaLabel.Tag = "Label_Karma";
            this.lblMetavariantKarmaLabel.Text = "Karma:";
            // 
            // lblMetatypeSkillSelection
            // 
            this.lblMetatypeSkillSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblMetatypeSkillSelection.AutoSize = true;
            this.tlpBottomHalf.SetColumnSpan(this.lblMetatypeSkillSelection, 6);
            this.lblMetatypeSkillSelection.Location = new System.Drawing.Point(304, 235);
            this.lblMetatypeSkillSelection.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetatypeSkillSelection.Name = "lblMetatypeSkillSelection";
            this.lblMetatypeSkillSelection.Size = new System.Drawing.Size(393, 13);
            this.lblMetatypeSkillSelection.TabIndex = 75;
            this.lblMetatypeSkillSelection.Tag = "String_MetamagicSkillBase";
            this.lblMetatypeSkillSelection.Text = "Based on your talent selection, you may choose 2 magical skills to start at ratin" +
    "g 5.";
            this.lblMetatypeSkillSelection.Visible = false;
            // 
            // cboSkill1
            // 
            this.cboSkill1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpBottomHalf.SetColumnSpan(this.cboSkill1, 6);
            this.cboSkill1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSkill1.FormattingEnabled = true;
            this.cboSkill1.Location = new System.Drawing.Point(304, 257);
            this.cboSkill1.Name = "cboSkill1";
            this.cboSkill1.Size = new System.Drawing.Size(427, 21);
            this.cboSkill1.TabIndex = 9;
            this.cboSkill1.TooltipText = "";
            this.cboSkill1.Visible = false;
            // 
            // cboSkill2
            // 
            this.cboSkill2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpBottomHalf.SetColumnSpan(this.cboSkill2, 6);
            this.cboSkill2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSkill2.FormattingEnabled = true;
            this.cboSkill2.Location = new System.Drawing.Point(304, 284);
            this.cboSkill2.Name = "cboSkill2";
            this.cboSkill2.Size = new System.Drawing.Size(427, 21);
            this.cboSkill2.TabIndex = 10;
            this.cboSkill2.TooltipText = "";
            this.cboSkill2.Visible = false;
            // 
            // cboSkill3
            // 
            this.cboSkill3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpBottomHalf.SetColumnSpan(this.cboSkill3, 6);
            this.cboSkill3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSkill3.FormattingEnabled = true;
            this.cboSkill3.Location = new System.Drawing.Point(304, 311);
            this.cboSkill3.Name = "cboSkill3";
            this.cboSkill3.Size = new System.Drawing.Size(427, 21);
            this.cboSkill3.TabIndex = 78;
            this.cboSkill3.TooltipText = "";
            this.cboSkill3.Visible = false;
            // 
            // flpButtons
            // 
            this.flpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flpButtons.AutoSize = true;
            this.flpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpBottomHalf.SetColumnSpan(this.flpButtons, 6);
            this.flpButtons.Controls.Add(this.cmdOK);
            this.flpButtons.Controls.Add(this.cmdCancel);
            this.flpButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpButtons.Location = new System.Drawing.Point(575, 338);
            this.flpButtons.Name = "flpButtons";
            this.flpButtons.Size = new System.Drawing.Size(156, 23);
            this.flpButtons.TabIndex = 79;
            // 
            // cboTalents
            // 
            this.cboTalents.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboTalents.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTalents.FormattingEnabled = true;
            this.cboTalents.Location = new System.Drawing.Point(427, 57);
            this.cboTalents.Name = "cboTalents";
            this.cboTalents.Size = new System.Drawing.Size(304, 21);
            this.cboTalents.TabIndex = 8;
            this.cboTalents.TooltipText = "";
            this.cboTalents.SelectedIndexChanged += new System.EventHandler(this.cboTalents_SelectedIndexChanged);
            // 
            // tlpTopHalf
            // 
            this.tlpTopHalf.AutoSize = true;
            this.tlpTopHalf.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpTopHalf.ColumnCount = 3;
            this.tlpTopHalf.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpTopHalf.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpTopHalf.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpTopHalf.Controls.Add(this.cboTalents, 2, 2);
            this.tlpTopHalf.Controls.Add(this.lblHeritageLabel, 0, 0);
            this.tlpTopHalf.Controls.Add(this.lblAttributesLabel, 0, 1);
            this.tlpTopHalf.Controls.Add(this.cboResources, 1, 4);
            this.tlpTopHalf.Controls.Add(this.lblTalentLabel, 0, 2);
            this.tlpTopHalf.Controls.Add(this.cboSkills, 1, 3);
            this.tlpTopHalf.Controls.Add(this.lblResourcesLabel, 0, 4);
            this.tlpTopHalf.Controls.Add(this.cboTalent, 1, 2);
            this.tlpTopHalf.Controls.Add(this.cboAttributes, 1, 1);
            this.tlpTopHalf.Controls.Add(this.lblSkillsLabel, 0, 3);
            this.tlpTopHalf.Controls.Add(this.cboHeritage, 1, 0);
            this.tlpTopHalf.Controls.Add(this.lblSumtoTen, 2, 3);
            this.tlpTopHalf.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTopHalf.Location = new System.Drawing.Point(0, 0);
            this.tlpTopHalf.Margin = new System.Windows.Forms.Padding(0);
            this.tlpTopHalf.Name = "tlpTopHalf";
            this.tlpTopHalf.RowCount = 5;
            this.tlpTopHalf.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpTopHalf.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpTopHalf.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpTopHalf.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpTopHalf.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpTopHalf.Size = new System.Drawing.Size(734, 135);
            this.tlpTopHalf.TabIndex = 105;
            // 
            // lblHeritageLabel
            // 
            this.lblHeritageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblHeritageLabel.AutoSize = true;
            this.lblHeritageLabel.Location = new System.Drawing.Point(58, 6);
            this.lblHeritageLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblHeritageLabel.Name = "lblHeritageLabel";
            this.lblHeritageLabel.Size = new System.Drawing.Size(54, 13);
            this.lblHeritageLabel.TabIndex = 102;
            this.lblHeritageLabel.Tag = "Label_PriorityHeritage";
            this.lblHeritageLabel.Text = "Metatype:";
            // 
            // lblAttributesLabel
            // 
            this.lblAttributesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAttributesLabel.AutoSize = true;
            this.lblAttributesLabel.Location = new System.Drawing.Point(58, 33);
            this.lblAttributesLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAttributesLabel.Name = "lblAttributesLabel";
            this.lblAttributesLabel.Size = new System.Drawing.Size(54, 13);
            this.lblAttributesLabel.TabIndex = 106;
            this.lblAttributesLabel.Tag = "Label_PriorityAttributes";
            this.lblAttributesLabel.Text = "Attributes:";
            // 
            // cboResources
            // 
            this.cboResources.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboResources.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboResources.FormattingEnabled = true;
            this.cboResources.Location = new System.Drawing.Point(118, 111);
            this.cboResources.Name = "cboResources";
            this.cboResources.Size = new System.Drawing.Size(303, 21);
            this.cboResources.TabIndex = 5;
            this.cboResources.TooltipText = "";
            this.cboResources.SelectedIndexChanged += new System.EventHandler(this.cboResources_SelectedIndexChanged);
            // 
            // lblTalentLabel
            // 
            this.lblTalentLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTalentLabel.AutoSize = true;
            this.lblTalentLabel.Location = new System.Drawing.Point(3, 60);
            this.lblTalentLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTalentLabel.Name = "lblTalentLabel";
            this.lblTalentLabel.Size = new System.Drawing.Size(109, 13);
            this.lblTalentLabel.TabIndex = 104;
            this.lblTalentLabel.Tag = "Label_PriorityTalent";
            this.lblTalentLabel.Text = "Magic or Resonance:";
            // 
            // cboSkills
            // 
            this.cboSkills.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSkills.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSkills.FormattingEnabled = true;
            this.cboSkills.Location = new System.Drawing.Point(118, 84);
            this.cboSkills.Name = "cboSkills";
            this.cboSkills.Size = new System.Drawing.Size(303, 21);
            this.cboSkills.TabIndex = 4;
            this.cboSkills.TooltipText = "";
            this.cboSkills.SelectedIndexChanged += new System.EventHandler(this.cboSkills_SelectedIndexChanged);
            // 
            // lblResourcesLabel
            // 
            this.lblResourcesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblResourcesLabel.AutoSize = true;
            this.lblResourcesLabel.Location = new System.Drawing.Point(51, 114);
            this.lblResourcesLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblResourcesLabel.Name = "lblResourcesLabel";
            this.lblResourcesLabel.Size = new System.Drawing.Size(61, 13);
            this.lblResourcesLabel.TabIndex = 110;
            this.lblResourcesLabel.Tag = "Label_PriorityResources";
            this.lblResourcesLabel.Text = "Resources:";
            // 
            // cboTalent
            // 
            this.cboTalent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboTalent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTalent.FormattingEnabled = true;
            this.cboTalent.Location = new System.Drawing.Point(118, 57);
            this.cboTalent.Name = "cboTalent";
            this.cboTalent.Size = new System.Drawing.Size(303, 21);
            this.cboTalent.TabIndex = 3;
            this.cboTalent.TooltipText = "";
            this.cboTalent.SelectedIndexChanged += new System.EventHandler(this.cboTalent_SelectedIndexChanged);
            // 
            // cboAttributes
            // 
            this.cboAttributes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboAttributes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAttributes.FormattingEnabled = true;
            this.cboAttributes.Location = new System.Drawing.Point(118, 30);
            this.cboAttributes.Name = "cboAttributes";
            this.cboAttributes.Size = new System.Drawing.Size(303, 21);
            this.cboAttributes.TabIndex = 2;
            this.cboAttributes.TooltipText = "";
            this.cboAttributes.SelectedIndexChanged += new System.EventHandler(this.cboAttributes_SelectedIndexChanged);
            // 
            // lblSkillsLabel
            // 
            this.lblSkillsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSkillsLabel.AutoSize = true;
            this.lblSkillsLabel.Location = new System.Drawing.Point(78, 87);
            this.lblSkillsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSkillsLabel.Name = "lblSkillsLabel";
            this.lblSkillsLabel.Size = new System.Drawing.Size(34, 13);
            this.lblSkillsLabel.TabIndex = 108;
            this.lblSkillsLabel.Tag = "Label_PrioritySkills";
            this.lblSkillsLabel.Text = "Skills:";
            // 
            // cboHeritage
            // 
            this.cboHeritage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboHeritage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboHeritage.FormattingEnabled = true;
            this.cboHeritage.Location = new System.Drawing.Point(118, 3);
            this.cboHeritage.Name = "cboHeritage";
            this.cboHeritage.Size = new System.Drawing.Size(303, 21);
            this.cboHeritage.TabIndex = 1;
            this.cboHeritage.TooltipText = "";
            this.cboHeritage.SelectedIndexChanged += new System.EventHandler(this.cboHeritage_SelectedIndexChanged);
            // 
            // lblSumtoTen
            // 
            this.lblSumtoTen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSumtoTen.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSumtoTen.Location = new System.Drawing.Point(427, 87);
            this.lblSumtoTen.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSumtoTen.Name = "lblSumtoTen";
            this.tlpTopHalf.SetRowSpan(this.lblSumtoTen, 2);
            this.lblSumtoTen.Size = new System.Drawing.Size(304, 42);
            this.lblSumtoTen.TabIndex = 111;
            this.lblSumtoTen.Text = "0/10";
            this.lblSumtoTen.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblSumtoTen.Visible = false;
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.tlpBottomHalf, 0, 1);
            this.tlpMain.Controls.Add(this.tlpTopHalf, 0, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(734, 519);
            this.tlpMain.TabIndex = 106;
            // 
            // frmPriorityMetatype
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(752, 537);
            this.ControlBox = false;
            this.Controls.Add(this.tlpMain);
            this.Name = "frmPriorityMetatype";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_ChooseCharacterPriorities";
            this.Text = "Choose Character Priorities";
            this.Load += new System.EventHandler(this.frmPriorityMetatype_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudForce)).EndInit();
            this.tlpBottomHalf.ResumeLayout(false);
            this.tlpBottomHalf.PerformLayout();
            this.flpButtons.ResumeLayout(false);
            this.flpButtons.PerformLayout();
            this.tlpTopHalf.ResumeLayout(false);
            this.tlpTopHalf.PerformLayout();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        
        internal System.Windows.Forms.Label lblMetavariantKarma;
        private System.Windows.Forms.NumericUpDown nudForce;
        private System.Windows.Forms.Label lblForceLabel;
        private ElasticComboBox cboCategory;
        internal System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Label lblMetavariantQualities;
        private System.Windows.Forms.Label lblMetavariantQualitiesLabel;
        private System.Windows.Forms.Label lblMetavariantLabel;
        private ElasticComboBox cboMetavariant;
        internal System.Windows.Forms.Button cmdOK;
        internal System.Windows.Forms.Label lblINILabel;
        internal System.Windows.Forms.Label lblINI;
        internal System.Windows.Forms.Label lblWILLabel;
        internal System.Windows.Forms.Label lblWIL;
        internal System.Windows.Forms.Label lblLOGLabel;
        internal System.Windows.Forms.Label lblLOG;
        internal System.Windows.Forms.Label lblINTLabel;
        internal System.Windows.Forms.Label lblINT;
        internal System.Windows.Forms.Label lblCHALabel;
        internal System.Windows.Forms.Label lblCHA;
        internal System.Windows.Forms.Label lblSTR;
        internal System.Windows.Forms.Label lblSTRLabel;
        internal System.Windows.Forms.Label lblREA;
        internal System.Windows.Forms.Label lblREALabel;
        internal System.Windows.Forms.Label lblAGI;
        internal System.Windows.Forms.Label lblAGILabel;
        internal System.Windows.Forms.Label lblBOD;
        internal System.Windows.Forms.Label lblBODLabel;
        internal System.Windows.Forms.ListBox lstMetatypes;
        internal System.Windows.Forms.Label lblSpecialAttributes;
        internal System.Windows.Forms.Label lblSpecialAttributesLabel;
        private ElasticComboBox cboTalents;
        private System.Windows.Forms.Label lblMetatypeSkillSelection;
        private ElasticComboBox cboSkill1;
        private ElasticComboBox cboSkill2;
        internal System.Windows.Forms.Label lblMetavariantKarmaLabel;
        private ElasticComboBox cboSkill3;
        private Chummer.BufferedTableLayoutPanel tlpTopHalf;
        private System.Windows.Forms.Label lblHeritageLabel;
        private System.Windows.Forms.Label lblAttributesLabel;
        private ElasticComboBox cboResources;
        private System.Windows.Forms.Label lblTalentLabel;
        private ElasticComboBox cboSkills;
        private System.Windows.Forms.Label lblResourcesLabel;
        private ElasticComboBox cboTalent;
        private ElasticComboBox cboAttributes;
        private System.Windows.Forms.Label lblSkillsLabel;
        private ElasticComboBox cboHeritage;
        private System.Windows.Forms.Label lblSumtoTen;
        private Chummer.BufferedTableLayoutPanel tlpBottomHalf;
        private System.Windows.Forms.FlowLayoutPanel flpButtons;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
    }
}
