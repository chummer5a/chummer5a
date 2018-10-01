namespace Chummer
{
    partial class ContactControl
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
                UnbindContactControl();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.nudConnection = new System.Windows.Forms.NumericUpDown();
            this.nudLoyalty = new System.Windows.Forms.NumericUpDown();
            this.cmdDelete = new System.Windows.Forms.Button();
            this.cmsContact = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsContactOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.tsRemoveCharacter = new System.Windows.Forms.ToolStripMenuItem();
            this.tsAttachCharacter = new System.Windows.Forms.ToolStripMenuItem();
            this.cboContactRole = new Chummer.ElasticComboBox();
            this.txtContactName = new System.Windows.Forms.TextBox();
            this.txtContactLocation = new System.Windows.Forms.TextBox();
            this.cmdExpand = new System.Windows.Forms.Button();
            this.imgNotes = new System.Windows.Forms.PictureBox();
            this.imgLink = new System.Windows.Forms.PictureBox();
            this.chkGroup = new System.Windows.Forms.CheckBox();
            this.chkFree = new System.Windows.Forms.CheckBox();
            this.lblQuickStats = new System.Windows.Forms.Label();
            this.chkBlackmail = new System.Windows.Forms.CheckBox();
            this.chkFamily = new System.Windows.Forms.CheckBox();
            this.lblConnection = new System.Windows.Forms.Label();
            this.lblLoyalty = new System.Windows.Forms.Label();
            this.cboMetatype = new Chummer.ElasticComboBox();
            this.cboSex = new Chummer.ElasticComboBox();
            this.cboType = new Chummer.ElasticComboBox();
            this.cboPreferredPayment = new Chummer.ElasticComboBox();
            this.cboAge = new Chummer.ElasticComboBox();
            this.cboHobbiesVice = new Chummer.ElasticComboBox();
            this.cboPersonalLife = new Chummer.ElasticComboBox();
            this.lblMetatype = new System.Windows.Forms.Label();
            this.lblSex = new System.Windows.Forms.Label();
            this.lblAge = new System.Windows.Forms.Label();
            this.lblPersonalLife = new System.Windows.Forms.Label();
            this.lblType = new System.Windows.Forms.Label();
            this.lblPreferredPayment = new System.Windows.Forms.Label();
            this.lblHobbiesVice = new System.Windows.Forms.Label();
            this.tlpStatBlock = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpStatHeader = new Chummer.BufferedTableLayoutPanel(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.nudConnection)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLoyalty)).BeginInit();
            this.cmsContact.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgNotes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgLink)).BeginInit();
            this.tlpStatBlock.SuspendLayout();
            this.tlpStatHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // nudConnection
            // 
            this.nudConnection.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudConnection.Location = new System.Drawing.Point(73, 3);
            this.nudConnection.Maximum = new decimal(new int[] {
            12,
            0,
            0,
            0});
            this.nudConnection.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudConnection.Name = "nudConnection";
            this.nudConnection.Size = new System.Drawing.Size(40, 20);
            this.nudConnection.TabIndex = 3;
            this.nudConnection.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudConnection.ValueChanged += new System.EventHandler(this.nudConnection_ValueChanged);
            // 
            // nudLoyalty
            // 
            this.nudLoyalty.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudLoyalty.Location = new System.Drawing.Point(168, 3);
            this.nudLoyalty.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.nudLoyalty.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudLoyalty.Name = "nudLoyalty";
            this.nudLoyalty.Size = new System.Drawing.Size(40, 20);
            this.nudLoyalty.TabIndex = 4;
            this.nudLoyalty.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudLoyalty.ValueChanged += new System.EventHandler(this.nudLoyalty_ValueChanged);
            // 
            // cmdDelete
            // 
            this.cmdDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdDelete.AutoSize = true;
            this.cmdDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDelete.FlatAppearance.BorderSize = 0;
            this.cmdDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdDelete.Image = global::Chummer.Properties.Resources.delete;
            this.cmdDelete.Location = new System.Drawing.Point(488, 2);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(22, 22);
            this.cmdDelete.TabIndex = 7;
            this.cmdDelete.Tag = "";
            this.cmdDelete.UseVisualStyleBackColor = true;
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // cmsContact
            // 
            this.cmsContact.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsContactOpen,
            this.tsRemoveCharacter,
            this.tsAttachCharacter});
            this.cmsContact.Name = "cmsContact";
            this.cmsContact.Size = new System.Drawing.Size(172, 70);
            // 
            // tsContactOpen
            // 
            this.tsContactOpen.Image = global::Chummer.Properties.Resources.link_go;
            this.tsContactOpen.Name = "tsContactOpen";
            this.tsContactOpen.Size = new System.Drawing.Size(171, 22);
            this.tsContactOpen.Tag = "MenuItem_OpenCharacter";
            this.tsContactOpen.Text = "Open Character";
            this.tsContactOpen.Click += new System.EventHandler(this.tsContactOpen_Click);
            // 
            // tsRemoveCharacter
            // 
            this.tsRemoveCharacter.Image = global::Chummer.Properties.Resources.link_delete;
            this.tsRemoveCharacter.Name = "tsRemoveCharacter";
            this.tsRemoveCharacter.Size = new System.Drawing.Size(171, 22);
            this.tsRemoveCharacter.Tag = "MenuItem_RemoveCharacter";
            this.tsRemoveCharacter.Text = "Remove Character";
            this.tsRemoveCharacter.Click += new System.EventHandler(this.tsRemoveCharacter_Click);
            // 
            // tsAttachCharacter
            // 
            this.tsAttachCharacter.Image = global::Chummer.Properties.Resources.link_add;
            this.tsAttachCharacter.Name = "tsAttachCharacter";
            this.tsAttachCharacter.Size = new System.Drawing.Size(171, 22);
            this.tsAttachCharacter.Tag = "MenuItem_AttachCharacter";
            this.tsAttachCharacter.Text = "Attach Character";
            this.tsAttachCharacter.Click += new System.EventHandler(this.tsAttachCharacter_Click);
            // 
            // cboContactRole
            // 
            this.cboContactRole.FormattingEnabled = true;
            this.cboContactRole.Location = new System.Drawing.Point(279, 4);
            this.cboContactRole.Name = "cboContactRole";
            this.cboContactRole.Size = new System.Drawing.Size(120, 21);
            this.cboContactRole.TabIndex = 2;
            this.cboContactRole.TooltipText = "";
            this.cboContactRole.TextChanged += new System.EventHandler(this.cboContactRole_TextChanged);
            // 
            // txtContactName
            // 
            this.txtContactName.Location = new System.Drawing.Point(27, 4);
            this.txtContactName.Name = "txtContactName";
            this.txtContactName.Size = new System.Drawing.Size(120, 20);
            this.txtContactName.TabIndex = 0;
            this.txtContactName.TextChanged += new System.EventHandler(this.txtContactName_TextChanged);
            // 
            // txtContactLocation
            // 
            this.txtContactLocation.Location = new System.Drawing.Point(153, 4);
            this.txtContactLocation.Name = "txtContactLocation";
            this.txtContactLocation.Size = new System.Drawing.Size(120, 20);
            this.txtContactLocation.TabIndex = 1;
            this.txtContactLocation.TextChanged += new System.EventHandler(this.txtContactLocation_TextChanged);
            // 
            // cmdExpand
            // 
            this.cmdExpand.FlatAppearance.BorderSize = 0;
            this.cmdExpand.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdExpand.Image = global::Chummer.Properties.Resources.Collapse;
            this.cmdExpand.Location = new System.Drawing.Point(3, 3);
            this.cmdExpand.Name = "cmdExpand";
            this.cmdExpand.Size = new System.Drawing.Size(21, 21);
            this.cmdExpand.TabIndex = 11;
            this.cmdExpand.UseVisualStyleBackColor = true;
            this.cmdExpand.Click += new System.EventHandler(this.cmdExpand_Click);
            // 
            // imgNotes
            // 
            this.imgNotes.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.imgNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.imgNotes.Location = new System.Drawing.Point(236, 5);
            this.imgNotes.Name = "imgNotes";
            this.imgNotes.Size = new System.Drawing.Size(16, 16);
            this.imgNotes.TabIndex = 10;
            this.imgNotes.TabStop = false;
            this.imgNotes.Click += new System.EventHandler(this.imgNotes_Click);
            // 
            // imgLink
            // 
            this.imgLink.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.imgLink.Image = global::Chummer.Properties.Resources.link;
            this.imgLink.Location = new System.Drawing.Point(214, 5);
            this.imgLink.Name = "imgLink";
            this.imgLink.Size = new System.Drawing.Size(16, 16);
            this.imgLink.TabIndex = 6;
            this.imgLink.TabStop = false;
            this.imgLink.Click += new System.EventHandler(this.imgLink_Click);
            // 
            // chkGroup
            // 
            this.chkGroup.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkGroup.AutoSize = true;
            this.chkGroup.Location = new System.Drawing.Point(258, 4);
            this.chkGroup.Name = "chkGroup";
            this.chkGroup.Size = new System.Drawing.Size(55, 17);
            this.chkGroup.TabIndex = 12;
            this.chkGroup.Tag = "Checkbox_Contact_Group";
            this.chkGroup.Text = "Group";
            this.chkGroup.UseVisualStyleBackColor = true;
            this.chkGroup.CheckedChanged += new System.EventHandler(this.chkGroup_CheckedChanged);
            // 
            // chkFree
            // 
            this.chkFree.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkFree.AutoSize = true;
            this.chkFree.Location = new System.Drawing.Point(319, 4);
            this.chkFree.Name = "chkFree";
            this.chkFree.Size = new System.Drawing.Size(47, 17);
            this.chkFree.TabIndex = 13;
            this.chkFree.Tag = "Checkbox_Contact_Free";
            this.chkFree.Text = "Free";
            this.chkFree.UseVisualStyleBackColor = true;
            this.chkFree.CheckedChanged += new System.EventHandler(this.chkFree_CheckedChanged);
            // 
            // lblQuickStats
            // 
            this.lblQuickStats.AutoSize = true;
            this.lblQuickStats.Location = new System.Drawing.Point(405, 7);
            this.lblQuickStats.Name = "lblQuickStats";
            this.lblQuickStats.Size = new System.Drawing.Size(30, 13);
            this.lblQuickStats.TabIndex = 14;
            this.lblQuickStats.Text = "(1/1)";
            // 
            // chkBlackmail
            // 
            this.chkBlackmail.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkBlackmail.AutoSize = true;
            this.chkBlackmail.Location = new System.Drawing.Point(372, 4);
            this.chkBlackmail.Name = "chkBlackmail";
            this.chkBlackmail.Size = new System.Drawing.Size(71, 17);
            this.chkBlackmail.TabIndex = 16;
            this.chkBlackmail.Tag = "Checkbox_Contact_Blackmail";
            this.chkBlackmail.Text = "Blackmail";
            this.chkBlackmail.UseVisualStyleBackColor = true;
            this.chkBlackmail.CheckedChanged += new System.EventHandler(this.chkBlackmail_CheckedChanged);
            // 
            // chkFamily
            // 
            this.chkFamily.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkFamily.AutoSize = true;
            this.chkFamily.Location = new System.Drawing.Point(449, 4);
            this.chkFamily.Name = "chkFamily";
            this.chkFamily.Size = new System.Drawing.Size(55, 17);
            this.chkFamily.TabIndex = 17;
            this.chkFamily.Tag = "Checkbox_Contact_Family";
            this.chkFamily.Text = "Family";
            this.chkFamily.UseVisualStyleBackColor = true;
            this.chkFamily.CheckedChanged += new System.EventHandler(this.chkFamily_CheckedChanged);
            // 
            // lblConnection
            // 
            this.lblConnection.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblConnection.AutoSize = true;
            this.lblConnection.Location = new System.Drawing.Point(3, 6);
            this.lblConnection.Name = "lblConnection";
            this.lblConnection.Size = new System.Drawing.Size(64, 13);
            this.lblConnection.TabIndex = 18;
            this.lblConnection.Tag = "Label_Contact_Connection";
            this.lblConnection.Text = "Connection:";
            // 
            // lblLoyalty
            // 
            this.lblLoyalty.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblLoyalty.AutoSize = true;
            this.lblLoyalty.Location = new System.Drawing.Point(119, 6);
            this.lblLoyalty.Name = "lblLoyalty";
            this.lblLoyalty.Size = new System.Drawing.Size(43, 13);
            this.lblLoyalty.TabIndex = 19;
            this.lblLoyalty.Tag = "Label_Contact_Loyalty";
            this.lblLoyalty.Text = "Loyalty:";
            // 
            // cboMetatype
            // 
            this.cboMetatype.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboMetatype.FormattingEnabled = true;
            this.cboMetatype.Location = new System.Drawing.Point(63, 3);
            this.cboMetatype.Name = "cboMetatype";
            this.cboMetatype.Size = new System.Drawing.Size(164, 21);
            this.cboMetatype.TabIndex = 20;
            this.cboMetatype.TooltipText = "";
            this.cboMetatype.TextChanged += new System.EventHandler(this.cboMetatype_TextChanged);
            // 
            // cboSex
            // 
            this.cboSex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboSex.FormattingEnabled = true;
            this.cboSex.Location = new System.Drawing.Point(63, 30);
            this.cboSex.Name = "cboSex";
            this.cboSex.Size = new System.Drawing.Size(164, 21);
            this.cboSex.TabIndex = 21;
            this.cboSex.TooltipText = "";
            this.cboSex.TextChanged += new System.EventHandler(this.cboSex_TextChanged);
            // 
            // cboType
            // 
            this.cboType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboType.FormattingEnabled = true;
            this.cboType.Location = new System.Drawing.Point(63, 84);
            this.cboType.Name = "cboType";
            this.cboType.Size = new System.Drawing.Size(164, 21);
            this.cboType.TabIndex = 22;
            this.cboType.TooltipText = "";
            this.cboType.TextChanged += new System.EventHandler(this.cboType_TextChanged);
            // 
            // cboPreferredPayment
            // 
            this.cboPreferredPayment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboPreferredPayment.FormattingEnabled = true;
            this.cboPreferredPayment.Location = new System.Drawing.Point(336, 30);
            this.cboPreferredPayment.Name = "cboPreferredPayment";
            this.cboPreferredPayment.Size = new System.Drawing.Size(165, 21);
            this.cboPreferredPayment.TabIndex = 23;
            this.cboPreferredPayment.TooltipText = "";
            this.cboPreferredPayment.TextChanged += new System.EventHandler(this.cboPreferredPayment_TextChanged);
            // 
            // cboAge
            // 
            this.cboAge.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboAge.FormattingEnabled = true;
            this.cboAge.Location = new System.Drawing.Point(63, 57);
            this.cboAge.Name = "cboAge";
            this.cboAge.Size = new System.Drawing.Size(164, 21);
            this.cboAge.TabIndex = 24;
            this.cboAge.TooltipText = "";
            this.cboAge.TextChanged += new System.EventHandler(this.cboAge_TextChanged);
            // 
            // cboHobbiesVice
            // 
            this.cboHobbiesVice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboHobbiesVice.FormattingEnabled = true;
            this.cboHobbiesVice.Location = new System.Drawing.Point(336, 57);
            this.cboHobbiesVice.Name = "cboHobbiesVice";
            this.cboHobbiesVice.Size = new System.Drawing.Size(165, 21);
            this.cboHobbiesVice.TabIndex = 25;
            this.cboHobbiesVice.TooltipText = "";
            this.cboHobbiesVice.TextChanged += new System.EventHandler(this.cboHobbiesVice_TextChanged);
            // 
            // cboPersonalLife
            // 
            this.cboPersonalLife.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboPersonalLife.FormattingEnabled = true;
            this.cboPersonalLife.Location = new System.Drawing.Point(336, 3);
            this.cboPersonalLife.Name = "cboPersonalLife";
            this.cboPersonalLife.Size = new System.Drawing.Size(165, 21);
            this.cboPersonalLife.TabIndex = 26;
            this.cboPersonalLife.TooltipText = "";
            this.cboPersonalLife.TextChanged += new System.EventHandler(this.cboPersonalLife_TextChanged);
            // 
            // lblMetatype
            // 
            this.lblMetatype.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMetatype.AutoSize = true;
            this.lblMetatype.Location = new System.Drawing.Point(3, 7);
            this.lblMetatype.Name = "lblMetatype";
            this.lblMetatype.Size = new System.Drawing.Size(54, 13);
            this.lblMetatype.TabIndex = 27;
            this.lblMetatype.Tag = "Label_Metatype";
            this.lblMetatype.Text = "Metatype:";
            this.lblMetatype.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblSex
            // 
            this.lblSex.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSex.AutoSize = true;
            this.lblSex.Location = new System.Drawing.Point(29, 34);
            this.lblSex.Name = "lblSex";
            this.lblSex.Size = new System.Drawing.Size(28, 13);
            this.lblSex.TabIndex = 28;
            this.lblSex.Tag = "Label_Sex";
            this.lblSex.Text = "Sex:";
            this.lblSex.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblAge
            // 
            this.lblAge.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblAge.AutoSize = true;
            this.lblAge.Location = new System.Drawing.Point(28, 61);
            this.lblAge.Name = "lblAge";
            this.lblAge.Size = new System.Drawing.Size(29, 13);
            this.lblAge.TabIndex = 29;
            this.lblAge.Tag = "Label_Age";
            this.lblAge.Text = "Age:";
            this.lblAge.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblPersonalLife
            // 
            this.lblPersonalLife.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblPersonalLife.AutoSize = true;
            this.lblPersonalLife.Location = new System.Drawing.Point(259, 7);
            this.lblPersonalLife.Name = "lblPersonalLife";
            this.lblPersonalLife.Size = new System.Drawing.Size(71, 13);
            this.lblPersonalLife.TabIndex = 30;
            this.lblPersonalLife.Tag = "Label_Contact_PersonalLife";
            this.lblPersonalLife.Text = "Personal Life:";
            this.lblPersonalLife.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblType
            // 
            this.lblType.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(23, 88);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(34, 13);
            this.lblType.TabIndex = 31;
            this.lblType.Tag = "Label_Type";
            this.lblType.Text = "Type:";
            this.lblType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblPreferredPayment
            // 
            this.lblPreferredPayment.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblPreferredPayment.AutoSize = true;
            this.lblPreferredPayment.Location = new System.Drawing.Point(233, 34);
            this.lblPreferredPayment.Name = "lblPreferredPayment";
            this.lblPreferredPayment.Size = new System.Drawing.Size(97, 13);
            this.lblPreferredPayment.TabIndex = 32;
            this.lblPreferredPayment.Tag = "Label_Contact_PreferredPayment";
            this.lblPreferredPayment.Text = "Preferred Payment:";
            this.lblPreferredPayment.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblHobbiesVice
            // 
            this.lblHobbiesVice.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblHobbiesVice.AutoSize = true;
            this.lblHobbiesVice.Location = new System.Drawing.Point(255, 61);
            this.lblHobbiesVice.Name = "lblHobbiesVice";
            this.lblHobbiesVice.Size = new System.Drawing.Size(75, 13);
            this.lblHobbiesVice.TabIndex = 33;
            this.lblHobbiesVice.Tag = "Label_Contact_HobbiesVice";
            this.lblHobbiesVice.Text = "Hobbies/Vice:";
            this.lblHobbiesVice.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tlpStatBlock
            // 
            this.tlpStatBlock.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpStatBlock.BackColor = System.Drawing.Color.Transparent;
            this.tlpStatBlock.ColumnCount = 4;
            this.tlpStatBlock.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpStatBlock.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpStatBlock.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpStatBlock.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpStatBlock.Controls.Add(this.cboMetatype, 1, 1);
            this.tlpStatBlock.Controls.Add(this.cboSex, 1, 2);
            this.tlpStatBlock.Controls.Add(this.cboHobbiesVice, 3, 3);
            this.tlpStatBlock.Controls.Add(this.cboPersonalLife, 3, 1);
            this.tlpStatBlock.Controls.Add(this.cboPreferredPayment, 3, 2);
            this.tlpStatBlock.Controls.Add(this.lblHobbiesVice, 2, 3);
            this.tlpStatBlock.Controls.Add(this.cboType, 1, 4);
            this.tlpStatBlock.Controls.Add(this.lblPreferredPayment, 2, 2);
            this.tlpStatBlock.Controls.Add(this.cboAge, 1, 3);
            this.tlpStatBlock.Controls.Add(this.lblPersonalLife, 2, 1);
            this.tlpStatBlock.Controls.Add(this.lblType, 0, 4);
            this.tlpStatBlock.Controls.Add(this.lblMetatype, 0, 1);
            this.tlpStatBlock.Controls.Add(this.lblSex, 0, 2);
            this.tlpStatBlock.Controls.Add(this.lblAge, 0, 3);
            this.tlpStatBlock.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tlpStatBlock.Location = new System.Drawing.Point(6, 53);
            this.tlpStatBlock.Margin = new System.Windows.Forms.Padding(2);
            this.tlpStatBlock.Name = "tlpStatBlock";
            this.tlpStatBlock.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.tlpStatBlock.RowCount = 5;
            this.tlpStatBlock.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpStatBlock.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpStatBlock.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpStatBlock.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpStatBlock.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpStatBlock.Size = new System.Drawing.Size(504, 113);
            this.tlpStatBlock.TabIndex = 34;
            // 
            // tlpStatHeader
            // 
            this.tlpStatHeader.AutoSize = true;
            this.tlpStatHeader.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpStatHeader.ColumnCount = 10;
            this.tlpStatHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpStatHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpStatHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpStatHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpStatHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpStatHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpStatHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpStatHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpStatHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpStatHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpStatHeader.Controls.Add(this.lblConnection, 0, 0);
            this.tlpStatHeader.Controls.Add(this.nudConnection, 1, 0);
            this.tlpStatHeader.Controls.Add(this.chkFamily, 9, 0);
            this.tlpStatHeader.Controls.Add(this.lblLoyalty, 2, 0);
            this.tlpStatHeader.Controls.Add(this.chkBlackmail, 8, 0);
            this.tlpStatHeader.Controls.Add(this.nudLoyalty, 3, 0);
            this.tlpStatHeader.Controls.Add(this.imgLink, 4, 0);
            this.tlpStatHeader.Controls.Add(this.imgNotes, 5, 0);
            this.tlpStatHeader.Controls.Add(this.chkFree, 7, 0);
            this.tlpStatHeader.Controls.Add(this.chkGroup, 6, 0);
            this.tlpStatHeader.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.AddColumns;
            this.tlpStatHeader.Location = new System.Drawing.Point(6, 27);
            this.tlpStatHeader.Name = "tlpStatHeader";
            this.tlpStatHeader.RowCount = 1;
            this.tlpStatHeader.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpStatHeader.Size = new System.Drawing.Size(507, 26);
            this.tlpStatHeader.TabIndex = 34;
            // 
            // ContactControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.tlpStatBlock);
            this.Controls.Add(this.lblQuickStats);
            this.Controls.Add(this.cmdExpand);
            this.Controls.Add(this.txtContactLocation);
            this.Controls.Add(this.txtContactName);
            this.Controls.Add(this.cmdDelete);
            this.Controls.Add(this.cboContactRole);
            this.Controls.Add(this.tlpStatHeader);
            this.MinimumSize = new System.Drawing.Size(480, 22);
            this.Name = "ContactControl";
            this.Size = new System.Drawing.Size(516, 168);
            this.Load += new System.EventHandler(this.ContactControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudConnection)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLoyalty)).EndInit();
            this.cmsContact.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.imgNotes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgLink)).EndInit();
            this.tlpStatBlock.ResumeLayout(false);
            this.tlpStatBlock.PerformLayout();
            this.tlpStatHeader.ResumeLayout(false);
            this.tlpStatHeader.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown nudConnection;
        private System.Windows.Forms.NumericUpDown nudLoyalty;
        private System.Windows.Forms.Button cmdDelete;
        private System.Windows.Forms.PictureBox imgLink;
        private System.Windows.Forms.ContextMenuStrip cmsContact;
        private System.Windows.Forms.ToolStripMenuItem tsContactOpen;
        private System.Windows.Forms.ToolStripMenuItem tsRemoveCharacter;
        private System.Windows.Forms.ToolStripMenuItem tsAttachCharacter;
        private System.Windows.Forms.PictureBox imgNotes;
        private ElasticComboBox cboContactRole;
        private System.Windows.Forms.TextBox txtContactName;
        private System.Windows.Forms.TextBox txtContactLocation;
        private System.Windows.Forms.Button cmdExpand;
        private System.Windows.Forms.CheckBox chkGroup;
        private System.Windows.Forms.CheckBox chkFree;
        private System.Windows.Forms.Label lblQuickStats;
        private System.Windows.Forms.CheckBox chkBlackmail;
        private System.Windows.Forms.CheckBox chkFamily;
        private System.Windows.Forms.Label lblConnection;
        private System.Windows.Forms.Label lblLoyalty;
        private ElasticComboBox cboMetatype;
        private ElasticComboBox cboSex;
        private ElasticComboBox cboType;
        private ElasticComboBox cboPreferredPayment;
        private ElasticComboBox cboAge;
        private ElasticComboBox cboHobbiesVice;
        private ElasticComboBox cboPersonalLife;
        private System.Windows.Forms.Label lblMetatype;
        private System.Windows.Forms.Label lblSex;
        private System.Windows.Forms.Label lblAge;
        private System.Windows.Forms.Label lblPersonalLife;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.Label lblPreferredPayment;
        private System.Windows.Forms.Label lblHobbiesVice;
        private Chummer.BufferedTableLayoutPanel tlpStatBlock;
        private Chummer.BufferedTableLayoutPanel tlpStatHeader;
    }
}
