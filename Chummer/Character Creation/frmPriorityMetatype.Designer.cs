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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPriorityMetatype));
            this.nudForce = new Chummer.NumericUpDownEx();
            this.lblForceLabel = new System.Windows.Forms.Label();
            this.cboCategory = new Chummer.ElasticComboBox();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.lblMetavariantQualities = new System.Windows.Forms.Label();
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
            this.lblSpecialAttributes = new System.Windows.Forms.Label();
            this.lblMetavariantKarma = new System.Windows.Forms.Label();
            this.tlpButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblMetatypeSkillSelection = new System.Windows.Forms.Label();
            this.cboSkill1 = new Chummer.ElasticComboBox();
            this.cboSkill2 = new Chummer.ElasticComboBox();
            this.cboSkill3 = new Chummer.ElasticComboBox();
            this.lblSpecialAttributesLabel = new System.Windows.Forms.Label();
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
            this.lblMetavariantQualitiesLabel = new System.Windows.Forms.Label();
            this.lblMetavariantKarmaLabel = new System.Windows.Forms.Label();
            this.lblMetavariantLabel = new System.Windows.Forms.Label();
            this.pnlQualities = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.nudForce)).BeginInit();
            this.tlpButtons.SuspendLayout();
            this.tlpTopHalf.SuspendLayout();
            this.tlpMain.SuspendLayout();
            this.pnlQualities.SuspendLayout();
            this.SuspendLayout();
            // 
            // nudForce
            // 
            this.nudForce.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudForce.Location = new System.Drawing.Point(655, 238);
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
            this.nudForce.Size = new System.Drawing.Size(76, 20);
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
            this.lblForceLabel.Location = new System.Drawing.Point(655, 216);
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
            this.cboCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategory.FormattingEnabled = true;
            this.cboCategory.Location = new System.Drawing.Point(3, 161);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(295, 21);
            this.cboCategory.TabIndex = 6;
            this.cboCategory.TooltipText = "";
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // cmdCancel
            // 
            this.cmdCancel.AutoSize = true;
            this.cmdCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdCancel.Location = new System.Drawing.Point(3, 3);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(50, 23);
            this.cmdCancel.TabIndex = 12;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // lblMetavariantQualities
            // 
            this.lblMetavariantQualities.AutoSize = true;
            this.lblMetavariantQualities.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblMetavariantQualities.Location = new System.Drawing.Point(3, 6);
            this.lblMetavariantQualities.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetavariantQualities.Name = "lblMetavariantQualities";
            this.lblMetavariantQualities.Size = new System.Drawing.Size(33, 13);
            this.lblMetavariantQualities.TabIndex = 63;
            this.lblMetavariantQualities.Tag = "String_None";
            this.lblMetavariantQualities.Text = "None";
            // 
            // cboMetavariant
            // 
            this.cboMetavariant.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMain.SetColumnSpan(this.cboMetavariant, 4);
            this.cboMetavariant.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMetavariant.DropDownWidth = 69;
            this.cboMetavariant.FormattingEnabled = true;
            this.cboMetavariant.Location = new System.Drawing.Point(412, 264);
            this.cboMetavariant.Name = "cboMetavariant";
            this.cboMetavariant.Size = new System.Drawing.Size(319, 21);
            this.cboMetavariant.TabIndex = 59;
            this.cboMetavariant.TooltipText = "";
            this.cboMetavariant.SelectedIndexChanged += new System.EventHandler(this.cboMetavariant_SelectedIndexChanged);
            // 
            // cmdOK
            // 
            this.cmdOK.AutoSize = true;
            this.cmdOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOK.Location = new System.Drawing.Point(59, 3);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(50, 23);
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
            this.lblINILabel.Location = new System.Drawing.Point(574, 216);
            this.lblINILabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblINILabel.Name = "lblINILabel";
            this.lblINILabel.Size = new System.Drawing.Size(24, 13);
            this.lblINILabel.TabIndex = 54;
            this.lblINILabel.Tag = "String_AttributeINIShort";
            this.lblINILabel.Text = "INI";
            // 
            // lblINI
            // 
            this.lblINI.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblINI.AutoSize = true;
            this.lblINI.Location = new System.Drawing.Point(574, 241);
            this.lblINI.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblINI.Name = "lblINI";
            this.lblINI.Size = new System.Drawing.Size(51, 13);
            this.lblINI.TabIndex = 55;
            this.lblINI.Text = "2/12 (18)";
            // 
            // lblWILLabel
            // 
            this.lblWILLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblWILLabel.AutoSize = true;
            this.lblWILLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWILLabel.Location = new System.Drawing.Point(493, 216);
            this.lblWILLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWILLabel.Name = "lblWILLabel";
            this.lblWILLabel.Size = new System.Drawing.Size(30, 13);
            this.lblWILLabel.TabIndex = 52;
            this.lblWILLabel.Tag = "String_AttributeWILShort";
            this.lblWILLabel.Text = "WIL";
            // 
            // lblWIL
            // 
            this.lblWIL.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblWIL.AutoSize = true;
            this.lblWIL.Location = new System.Drawing.Point(493, 241);
            this.lblWIL.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWIL.Name = "lblWIL";
            this.lblWIL.Size = new System.Drawing.Size(51, 13);
            this.lblWIL.TabIndex = 53;
            this.lblWIL.Text = "2/12 (18)";
            // 
            // lblLOGLabel
            // 
            this.lblLOGLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblLOGLabel.AutoSize = true;
            this.lblLOGLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLOGLabel.Location = new System.Drawing.Point(412, 216);
            this.lblLOGLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLOGLabel.Name = "lblLOGLabel";
            this.lblLOGLabel.Size = new System.Drawing.Size(32, 13);
            this.lblLOGLabel.TabIndex = 50;
            this.lblLOGLabel.Tag = "String_AttributeLOGShort";
            this.lblLOGLabel.Text = "LOG";
            // 
            // lblLOG
            // 
            this.lblLOG.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblLOG.AutoSize = true;
            this.lblLOG.Location = new System.Drawing.Point(412, 241);
            this.lblLOG.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLOG.Name = "lblLOG";
            this.lblLOG.Size = new System.Drawing.Size(51, 13);
            this.lblLOG.TabIndex = 51;
            this.lblLOG.Text = "2/12 (18)";
            // 
            // lblINTLabel
            // 
            this.lblINTLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblINTLabel.AutoSize = true;
            this.lblINTLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblINTLabel.Location = new System.Drawing.Point(324, 216);
            this.lblINTLabel.Margin = new System.Windows.Forms.Padding(23, 6, 3, 6);
            this.lblINTLabel.Name = "lblINTLabel";
            this.lblINTLabel.Size = new System.Drawing.Size(28, 13);
            this.lblINTLabel.TabIndex = 48;
            this.lblINTLabel.Tag = "String_AttributeINTShort";
            this.lblINTLabel.Text = "INT";
            // 
            // lblINT
            // 
            this.lblINT.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblINT.AutoSize = true;
            this.lblINT.Location = new System.Drawing.Point(324, 241);
            this.lblINT.Margin = new System.Windows.Forms.Padding(23, 6, 3, 6);
            this.lblINT.Name = "lblINT";
            this.lblINT.Size = new System.Drawing.Size(51, 13);
            this.lblINT.TabIndex = 49;
            this.lblINT.Text = "2/12 (18)";
            // 
            // lblCHALabel
            // 
            this.lblCHALabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblCHALabel.AutoSize = true;
            this.lblCHALabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCHALabel.Location = new System.Drawing.Point(655, 166);
            this.lblCHALabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCHALabel.Name = "lblCHALabel";
            this.lblCHALabel.Size = new System.Drawing.Size(32, 13);
            this.lblCHALabel.TabIndex = 46;
            this.lblCHALabel.Tag = "String_AttributeCHAShort";
            this.lblCHALabel.Text = "CHA";
            // 
            // lblCHA
            // 
            this.lblCHA.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCHA.AutoSize = true;
            this.lblCHA.Location = new System.Drawing.Point(655, 191);
            this.lblCHA.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCHA.Name = "lblCHA";
            this.lblCHA.Size = new System.Drawing.Size(51, 13);
            this.lblCHA.TabIndex = 47;
            this.lblCHA.Text = "2/12 (18)";
            // 
            // lblSTR
            // 
            this.lblSTR.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSTR.AutoSize = true;
            this.lblSTR.Location = new System.Drawing.Point(574, 191);
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
            this.lblSTRLabel.Location = new System.Drawing.Point(574, 166);
            this.lblSTRLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSTRLabel.Name = "lblSTRLabel";
            this.lblSTRLabel.Size = new System.Drawing.Size(32, 13);
            this.lblSTRLabel.TabIndex = 44;
            this.lblSTRLabel.Tag = "String_AttributeSTRShort";
            this.lblSTRLabel.Text = "STR";
            // 
            // lblREA
            // 
            this.lblREA.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblREA.AutoSize = true;
            this.lblREA.Location = new System.Drawing.Point(493, 191);
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
            this.lblREALabel.Location = new System.Drawing.Point(493, 166);
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
            this.lblAGI.Location = new System.Drawing.Point(412, 191);
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
            this.lblAGILabel.Location = new System.Drawing.Point(412, 166);
            this.lblAGILabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAGILabel.Name = "lblAGILabel";
            this.lblAGILabel.Size = new System.Drawing.Size(28, 13);
            this.lblAGILabel.TabIndex = 40;
            this.lblAGILabel.Tag = "String_AttributeAGIShort";
            this.lblAGILabel.Text = "AGI";
            // 
            // lblBOD
            // 
            this.lblBOD.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblBOD.AutoSize = true;
            this.lblBOD.Location = new System.Drawing.Point(324, 191);
            this.lblBOD.Margin = new System.Windows.Forms.Padding(23, 6, 3, 6);
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
            this.lblBODLabel.Location = new System.Drawing.Point(324, 166);
            this.lblBODLabel.Margin = new System.Windows.Forms.Padding(23, 6, 3, 6);
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
            this.lstMetatypes.Location = new System.Drawing.Point(3, 188);
            this.lstMetatypes.Name = "lstMetatypes";
            this.tlpMain.SetRowSpan(this.lstMetatypes, 11);
            this.lstMetatypes.Size = new System.Drawing.Size(295, 328);
            this.lstMetatypes.Sorted = true;
            this.lstMetatypes.TabIndex = 7;
            this.lstMetatypes.SelectedIndexChanged += new System.EventHandler(this.lstMetatypes_SelectedIndexChanged);
            this.lstMetatypes.DoubleClick += new System.EventHandler(this.cmdOK_Click);
            // 
            // lblSpecialAttributes
            // 
            this.lblSpecialAttributes.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSpecialAttributes.AutoSize = true;
            this.lblSpecialAttributes.Location = new System.Drawing.Point(655, 294);
            this.lblSpecialAttributes.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpecialAttributes.Name = "lblSpecialAttributes";
            this.lblSpecialAttributes.Size = new System.Drawing.Size(13, 13);
            this.lblSpecialAttributes.TabIndex = 71;
            this.lblSpecialAttributes.Text = "0";
            this.lblSpecialAttributes.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMetavariantKarma
            // 
            this.lblMetavariantKarma.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMetavariantKarma.AutoSize = true;
            this.lblMetavariantKarma.Location = new System.Drawing.Point(412, 294);
            this.lblMetavariantKarma.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetavariantKarma.Name = "lblMetavariantKarma";
            this.lblMetavariantKarma.Size = new System.Drawing.Size(13, 13);
            this.lblMetavariantKarma.TabIndex = 77;
            this.lblMetavariantKarma.Text = "0";
            this.lblMetavariantKarma.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tlpButtons
            // 
            this.tlpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpButtons.AutoSize = true;
            this.tlpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpButtons.ColumnCount = 2;
            this.tlpMain.SetColumnSpan(this.tlpButtons, 5);
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.Controls.Add(this.cmdCancel, 0, 0);
            this.tlpButtons.Controls.Add(this.cmdOK, 1, 0);
            this.tlpButtons.Location = new System.Drawing.Point(622, 490);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.Size = new System.Drawing.Size(112, 29);
            this.tlpButtons.TabIndex = 80;
            // 
            // lblMetatypeSkillSelection
            // 
            this.lblMetatypeSkillSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblMetatypeSkillSelection.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.lblMetatypeSkillSelection, 5);
            this.lblMetatypeSkillSelection.Location = new System.Drawing.Point(304, 390);
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
            this.cboSkill1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMain.SetColumnSpan(this.cboSkill1, 5);
            this.cboSkill1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSkill1.FormattingEnabled = true;
            this.cboSkill1.Location = new System.Drawing.Point(304, 412);
            this.cboSkill1.Name = "cboSkill1";
            this.cboSkill1.Size = new System.Drawing.Size(427, 21);
            this.cboSkill1.TabIndex = 9;
            this.cboSkill1.TooltipText = "";
            this.cboSkill1.Visible = false;
            // 
            // cboSkill2
            // 
            this.cboSkill2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMain.SetColumnSpan(this.cboSkill2, 5);
            this.cboSkill2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSkill2.FormattingEnabled = true;
            this.cboSkill2.Location = new System.Drawing.Point(304, 439);
            this.cboSkill2.Name = "cboSkill2";
            this.cboSkill2.Size = new System.Drawing.Size(427, 21);
            this.cboSkill2.TabIndex = 10;
            this.cboSkill2.TooltipText = "";
            this.cboSkill2.Visible = false;
            // 
            // cboSkill3
            // 
            this.cboSkill3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMain.SetColumnSpan(this.cboSkill3, 5);
            this.cboSkill3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSkill3.FormattingEnabled = true;
            this.cboSkill3.Location = new System.Drawing.Point(304, 466);
            this.cboSkill3.Name = "cboSkill3";
            this.cboSkill3.Size = new System.Drawing.Size(427, 21);
            this.cboSkill3.TabIndex = 78;
            this.cboSkill3.TooltipText = "";
            this.cboSkill3.Visible = false;
            // 
            // lblSpecialAttributesLabel
            // 
            this.lblSpecialAttributesLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSpecialAttributesLabel.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.lblSpecialAttributesLabel, 2);
            this.lblSpecialAttributesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpecialAttributesLabel.Location = new System.Drawing.Point(538, 294);
            this.lblSpecialAttributesLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpecialAttributesLabel.Name = "lblSpecialAttributesLabel";
            this.lblSpecialAttributesLabel.Size = new System.Drawing.Size(111, 13);
            this.lblSpecialAttributesLabel.TabIndex = 70;
            this.lblSpecialAttributesLabel.Tag = "Label_SpecialAttributes";
            this.lblSpecialAttributesLabel.Text = "Special Attributes:";
            this.lblSpecialAttributesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboTalents
            // 
            this.cboTalents.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
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
            this.tlpMain.SetColumnSpan(this.tlpTopHalf, 6);
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
            this.tlpTopHalf.Margin = new System.Windows.Forms.Padding(0, 0, 0, 23);
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
            this.lblHeritageLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblHeritageLabel.AutoSize = true;
            this.lblHeritageLabel.Location = new System.Drawing.Point(58, 7);
            this.lblHeritageLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblHeritageLabel.Name = "lblHeritageLabel";
            this.lblHeritageLabel.Size = new System.Drawing.Size(54, 13);
            this.lblHeritageLabel.TabIndex = 102;
            this.lblHeritageLabel.Tag = "Label_PriorityHeritage";
            this.lblHeritageLabel.Text = "Metatype:";
            // 
            // lblAttributesLabel
            // 
            this.lblAttributesLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblAttributesLabel.AutoSize = true;
            this.lblAttributesLabel.Location = new System.Drawing.Point(58, 34);
            this.lblAttributesLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAttributesLabel.Name = "lblAttributesLabel";
            this.lblAttributesLabel.Size = new System.Drawing.Size(54, 13);
            this.lblAttributesLabel.TabIndex = 106;
            this.lblAttributesLabel.Tag = "Label_PriorityAttributes";
            this.lblAttributesLabel.Text = "Attributes:";
            // 
            // cboResources
            // 
            this.cboResources.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
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
            this.lblTalentLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblTalentLabel.AutoSize = true;
            this.lblTalentLabel.Location = new System.Drawing.Point(3, 61);
            this.lblTalentLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTalentLabel.Name = "lblTalentLabel";
            this.lblTalentLabel.Size = new System.Drawing.Size(109, 13);
            this.lblTalentLabel.TabIndex = 104;
            this.lblTalentLabel.Tag = "Label_PriorityTalent";
            this.lblTalentLabel.Text = "Magic or Resonance:";
            // 
            // cboSkills
            // 
            this.cboSkills.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
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
            this.lblResourcesLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblResourcesLabel.AutoSize = true;
            this.lblResourcesLabel.Location = new System.Drawing.Point(51, 115);
            this.lblResourcesLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblResourcesLabel.Name = "lblResourcesLabel";
            this.lblResourcesLabel.Size = new System.Drawing.Size(61, 13);
            this.lblResourcesLabel.TabIndex = 110;
            this.lblResourcesLabel.Tag = "Label_PriorityResources";
            this.lblResourcesLabel.Text = "Resources:";
            // 
            // cboTalent
            // 
            this.cboTalent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
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
            this.cboAttributes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
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
            this.lblSkillsLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSkillsLabel.AutoSize = true;
            this.lblSkillsLabel.Location = new System.Drawing.Point(78, 88);
            this.lblSkillsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSkillsLabel.Name = "lblSkillsLabel";
            this.lblSkillsLabel.Size = new System.Drawing.Size(34, 13);
            this.lblSkillsLabel.TabIndex = 108;
            this.lblSkillsLabel.Tag = "Label_PrioritySkills";
            this.lblSkillsLabel.Text = "Skills:";
            // 
            // cboHeritage
            // 
            this.cboHeritage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
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
            this.lblSumtoTen.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblSumtoTen.AutoSize = true;
            this.lblSumtoTen.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSumtoTen.Location = new System.Drawing.Point(552, 95);
            this.lblSumtoTen.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSumtoTen.Name = "lblSumtoTen";
            this.tlpTopHalf.SetRowSpan(this.lblSumtoTen, 2);
            this.lblSumtoTen.Size = new System.Drawing.Size(54, 26);
            this.lblSumtoTen.TabIndex = 111;
            this.lblSumtoTen.Text = "0/10";
            this.lblSumtoTen.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblSumtoTen.Visible = false;
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 6;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18.75F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18.75F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18.75F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18.75F));
            this.tlpMain.Controls.Add(this.cboSkill3, 1, 12);
            this.tlpMain.Controls.Add(this.cboSkill2, 1, 11);
            this.tlpMain.Controls.Add(this.cboSkill1, 1, 10);
            this.tlpMain.Controls.Add(this.lblMetatypeSkillSelection, 1, 9);
            this.tlpMain.Controls.Add(this.lblMetavariantQualitiesLabel, 1, 8);
            this.tlpMain.Controls.Add(this.lstMetatypes, 0, 3);
            this.tlpMain.Controls.Add(this.lblSpecialAttributes, 5, 7);
            this.tlpMain.Controls.Add(this.lblMetavariantKarmaLabel, 1, 7);
            this.tlpMain.Controls.Add(this.lblMetavariantKarma, 2, 7);
            this.tlpMain.Controls.Add(this.cboMetavariant, 2, 6);
            this.tlpMain.Controls.Add(this.lblLOG, 2, 5);
            this.tlpMain.Controls.Add(this.lblSpecialAttributesLabel, 3, 7);
            this.tlpMain.Controls.Add(this.lblLOGLabel, 2, 4);
            this.tlpMain.Controls.Add(this.lblMetavariantLabel, 1, 6);
            this.tlpMain.Controls.Add(this.nudForce, 5, 5);
            this.tlpMain.Controls.Add(this.lblWIL, 3, 5);
            this.tlpMain.Controls.Add(this.lblWILLabel, 3, 4);
            this.tlpMain.Controls.Add(this.lblCHA, 5, 3);
            this.tlpMain.Controls.Add(this.lblINILabel, 4, 4);
            this.tlpMain.Controls.Add(this.lblSTR, 4, 3);
            this.tlpMain.Controls.Add(this.lblINI, 4, 5);
            this.tlpMain.Controls.Add(this.lblForceLabel, 5, 4);
            this.tlpMain.Controls.Add(this.lblREA, 3, 3);
            this.tlpMain.Controls.Add(this.lblAGI, 2, 3);
            this.tlpMain.Controls.Add(this.lblBOD, 1, 3);
            this.tlpMain.Controls.Add(this.lblCHALabel, 5, 2);
            this.tlpMain.Controls.Add(this.lblSTRLabel, 4, 2);
            this.tlpMain.Controls.Add(this.lblREALabel, 3, 2);
            this.tlpMain.Controls.Add(this.lblINTLabel, 1, 4);
            this.tlpMain.Controls.Add(this.lblAGILabel, 2, 2);
            this.tlpMain.Controls.Add(this.lblBODLabel, 1, 2);
            this.tlpMain.Controls.Add(this.tlpTopHalf, 0, 0);
            this.tlpMain.Controls.Add(this.cboCategory, 0, 2);
            this.tlpMain.Controls.Add(this.lblINT, 1, 5);
            this.tlpMain.Controls.Add(this.tlpButtons, 1, 13);
            this.tlpMain.Controls.Add(this.pnlQualities, 2, 8);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 14;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(734, 519);
            this.tlpMain.TabIndex = 106;
            // 
            // lblMetavariantQualitiesLabel
            // 
            this.lblMetavariantQualitiesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMetavariantQualitiesLabel.AutoSize = true;
            this.lblMetavariantQualitiesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMetavariantQualitiesLabel.Location = new System.Drawing.Point(346, 319);
            this.lblMetavariantQualitiesLabel.Margin = new System.Windows.Forms.Padding(23, 6, 3, 6);
            this.lblMetavariantQualitiesLabel.Name = "lblMetavariantQualitiesLabel";
            this.lblMetavariantQualitiesLabel.Size = new System.Drawing.Size(60, 13);
            this.lblMetavariantQualitiesLabel.TabIndex = 62;
            this.lblMetavariantQualitiesLabel.Tag = "Label_Qualities";
            this.lblMetavariantQualitiesLabel.Text = "Qualities:";
            // 
            // lblMetavariantKarmaLabel
            // 
            this.lblMetavariantKarmaLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMetavariantKarmaLabel.AutoSize = true;
            this.lblMetavariantKarmaLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMetavariantKarmaLabel.Location = new System.Drawing.Point(360, 294);
            this.lblMetavariantKarmaLabel.Margin = new System.Windows.Forms.Padding(23, 6, 3, 6);
            this.lblMetavariantKarmaLabel.Name = "lblMetavariantKarmaLabel";
            this.lblMetavariantKarmaLabel.Size = new System.Drawing.Size(46, 13);
            this.lblMetavariantKarmaLabel.TabIndex = 76;
            this.lblMetavariantKarmaLabel.Tag = "Label_Karma";
            this.lblMetavariantKarmaLabel.Text = "Karma:";
            this.lblMetavariantKarmaLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblMetavariantLabel
            // 
            this.lblMetavariantLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMetavariantLabel.AutoSize = true;
            this.lblMetavariantLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMetavariantLabel.Location = new System.Drawing.Point(328, 268);
            this.lblMetavariantLabel.Margin = new System.Windows.Forms.Padding(23, 6, 3, 6);
            this.lblMetavariantLabel.Name = "lblMetavariantLabel";
            this.lblMetavariantLabel.Size = new System.Drawing.Size(78, 13);
            this.lblMetavariantLabel.TabIndex = 58;
            this.lblMetavariantLabel.Tag = "Label_Metavariant";
            this.lblMetavariantLabel.Text = "Metavariant:";
            this.lblMetavariantLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlQualities
            // 
            this.pnlQualities.AutoScroll = true;
            this.tlpMain.SetColumnSpan(this.pnlQualities, 4);
            this.pnlQualities.Controls.Add(this.lblMetavariantQualities);
            this.pnlQualities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlQualities.Location = new System.Drawing.Point(409, 313);
            this.pnlQualities.Margin = new System.Windows.Forms.Padding(0);
            this.pnlQualities.Name = "pnlQualities";
            this.pnlQualities.Padding = new System.Windows.Forms.Padding(3, 6, 13, 6);
            this.pnlQualities.Size = new System.Drawing.Size(325, 71);
            this.pnlQualities.TabIndex = 106;
            // 
            // frmPriorityMetatype
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(752, 537);
            this.ControlBox = false;
            this.Controls.Add(this.tlpMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmPriorityMetatype";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_ChooseCharacterPriorities";
            this.Text = "Choose Character Priorities";
            this.Load += new System.EventHandler(this.frmPriorityMetatype_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudForce)).EndInit();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.tlpTopHalf.ResumeLayout(false);
            this.tlpTopHalf.PerformLayout();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.pnlQualities.ResumeLayout(false);
            this.pnlQualities.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        
        internal System.Windows.Forms.Label lblMetavariantKarma;
        private Chummer.NumericUpDownEx nudForce;
        private System.Windows.Forms.Label lblForceLabel;
        private ElasticComboBox cboCategory;
        internal System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Label lblMetavariantQualities;
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
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private BufferedTableLayoutPanel tlpButtons;
        internal System.Windows.Forms.Label lblMetavariantKarmaLabel;
        private System.Windows.Forms.Label lblMetavariantLabel;
        private System.Windows.Forms.Label lblMetavariantQualitiesLabel;
        private System.Windows.Forms.Panel pnlQualities;
    }
}
