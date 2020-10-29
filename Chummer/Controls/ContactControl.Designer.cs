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
            this.nudConnection = new Chummer.NumericUpDownEx();
            this.nudLoyalty = new Chummer.NumericUpDownEx();
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
            this.chkGroup = new Chummer.ColorableCheckBox(this.components);
            this.chkFree = new Chummer.ColorableCheckBox(this.components);
            this.lblQuickStats = new System.Windows.Forms.Label();
            this.lblConnection = new System.Windows.Forms.Label();
            this.lblLoyalty = new System.Windows.Forms.Label();
            this.chkFamily = new Chummer.ColorableCheckBox(this.components);
            this.chkBlackmail = new Chummer.ColorableCheckBox(this.components);
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpStatBlock = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cboMetatype = new Chummer.ElasticComboBox();
            this.cboGender = new Chummer.ElasticComboBox();
            this.cboType = new Chummer.ElasticComboBox();
            this.cboAge = new Chummer.ElasticComboBox();
            this.lblType = new System.Windows.Forms.Label();
            this.lblMetatype = new System.Windows.Forms.Label();
            this.lblGender = new System.Windows.Forms.Label();
            this.lblAge = new System.Windows.Forms.Label();
            this.lblPersonalLife = new System.Windows.Forms.Label();
            this.cboPersonalLife = new Chummer.ElasticComboBox();
            this.lblPreferredPayment = new System.Windows.Forms.Label();
            this.cboPreferredPayment = new Chummer.ElasticComboBox();
            this.lblHobbiesVice = new System.Windows.Forms.Label();
            this.cboHobbiesVice = new Chummer.ElasticComboBox();
            this.tlpComboBoxes = new Chummer.BufferedTableLayoutPanel(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.nudConnection)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLoyalty)).BeginInit();
            this.cmsContact.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgNotes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgLink)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.tlpStatBlock.SuspendLayout();
            this.tlpComboBoxes.SuspendLayout();
            this.SuspendLayout();
            // 
            // nudConnection
            // 
            this.nudConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudConnection.AutoSize = true;
            this.nudConnection.Location = new System.Drawing.Point(73, 33);
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
            this.nudConnection.Size = new System.Drawing.Size(63, 20);
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
            this.nudLoyalty.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudLoyalty.AutoSize = true;
            this.nudLoyalty.Location = new System.Drawing.Point(191, 33);
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
            this.nudLoyalty.Size = new System.Drawing.Size(63, 20);
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
            this.cmdDelete.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdDelete.AutoSize = true;
            this.cmdDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDelete.Image = global::Chummer.Properties.Resources.delete;
            this.cmdDelete.Location = new System.Drawing.Point(526, 3);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Padding = new System.Windows.Forms.Padding(1);
            this.cmdDelete.Size = new System.Drawing.Size(24, 24);
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
            this.cboContactRole.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboContactRole.FormattingEnabled = true;
            this.cboContactRole.Location = new System.Drawing.Point(285, 4);
            this.cboContactRole.Name = "cboContactRole";
            this.cboContactRole.Size = new System.Drawing.Size(136, 21);
            this.cboContactRole.TabIndex = 2;
            this.cboContactRole.TooltipText = "";
            this.cboContactRole.TextChanged += new System.EventHandler(this.cboContactRole_TextChanged);
            // 
            // txtContactName
            // 
            this.txtContactName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtContactName.Location = new System.Drawing.Point(3, 5);
            this.txtContactName.Name = "txtContactName";
            this.txtContactName.Size = new System.Drawing.Size(135, 20);
            this.txtContactName.TabIndex = 0;
            this.txtContactName.TextChanged += new System.EventHandler(this.txtContactName_TextChanged);
            // 
            // txtContactLocation
            // 
            this.txtContactLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtContactLocation.Location = new System.Drawing.Point(144, 5);
            this.txtContactLocation.Name = "txtContactLocation";
            this.txtContactLocation.Size = new System.Drawing.Size(135, 20);
            this.txtContactLocation.TabIndex = 1;
            this.txtContactLocation.TextChanged += new System.EventHandler(this.txtContactLocation_TextChanged);
            // 
            // cmdExpand
            // 
            this.cmdExpand.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmdExpand.AutoSize = true;
            this.cmdExpand.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdExpand.Image = global::Chummer.Properties.Resources.Expand;
            this.cmdExpand.Location = new System.Drawing.Point(3, 3);
            this.cmdExpand.Name = "cmdExpand";
            this.cmdExpand.Padding = new System.Windows.Forms.Padding(1);
            this.cmdExpand.Size = new System.Drawing.Size(23, 23);
            this.cmdExpand.TabIndex = 11;
            this.cmdExpand.UseVisualStyleBackColor = true;
            this.cmdExpand.Click += new System.EventHandler(this.cmdExpand_Click);
            // 
            // imgNotes
            // 
            this.imgNotes.Cursor = System.Windows.Forms.Cursors.Hand;
            this.imgNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imgNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.imgNotes.Location = new System.Drawing.Point(282, 30);
            this.imgNotes.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.imgNotes.Name = "imgNotes";
            this.imgNotes.Size = new System.Drawing.Size(16, 26);
            this.imgNotes.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imgNotes.TabIndex = 10;
            this.imgNotes.TabStop = false;
            this.imgNotes.Click += new System.EventHandler(this.imgNotes_Click);
            // 
            // imgLink
            // 
            this.imgLink.Cursor = System.Windows.Forms.Cursors.Hand;
            this.imgLink.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imgLink.Image = global::Chummer.Properties.Resources.link;
            this.imgLink.Location = new System.Drawing.Point(260, 30);
            this.imgLink.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.imgLink.Name = "imgLink";
            this.imgLink.Size = new System.Drawing.Size(16, 26);
            this.imgLink.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imgLink.TabIndex = 6;
            this.imgLink.TabStop = false;
            this.imgLink.Click += new System.EventHandler(this.imgLink_Click);
            // 
            // chkGroup
            // 
            this.chkGroup.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkGroup.AutoSize = true;
            this.chkGroup.DefaultColorScheme = true;
            this.chkGroup.Location = new System.Drawing.Point(357, 34);
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
            this.chkFree.DefaultColorScheme = true;
            this.chkFree.Location = new System.Drawing.Point(304, 34);
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
            this.tlpMain.SetColumnSpan(this.lblQuickStats, 2);
            this.lblQuickStats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblQuickStats.Location = new System.Drawing.Point(456, 6);
            this.lblQuickStats.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblQuickStats.MinimumSize = new System.Drawing.Size(36, 0);
            this.lblQuickStats.Name = "lblQuickStats";
            this.lblQuickStats.Size = new System.Drawing.Size(64, 18);
            this.lblQuickStats.TabIndex = 14;
            this.lblQuickStats.Text = "(00/0)";
            this.lblQuickStats.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblConnection
            // 
            this.lblConnection.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblConnection.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.lblConnection, 2);
            this.lblConnection.Location = new System.Drawing.Point(3, 36);
            this.lblConnection.Name = "lblConnection";
            this.lblConnection.Size = new System.Drawing.Size(64, 13);
            this.lblConnection.TabIndex = 18;
            this.lblConnection.Tag = "Label_Contact_Connection";
            this.lblConnection.Text = "Connection:";
            this.lblConnection.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblLoyalty
            // 
            this.lblLoyalty.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblLoyalty.AutoSize = true;
            this.lblLoyalty.Location = new System.Drawing.Point(142, 36);
            this.lblLoyalty.Name = "lblLoyalty";
            this.lblLoyalty.Size = new System.Drawing.Size(43, 13);
            this.lblLoyalty.TabIndex = 19;
            this.lblLoyalty.Tag = "Label_Contact_Loyalty";
            this.lblLoyalty.Text = "Loyalty:";
            this.lblLoyalty.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkFamily
            // 
            this.chkFamily.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkFamily.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.chkFamily, 2);
            this.chkFamily.DefaultColorScheme = true;
            this.chkFamily.Location = new System.Drawing.Point(495, 34);
            this.chkFamily.Name = "chkFamily";
            this.chkFamily.Size = new System.Drawing.Size(55, 17);
            this.chkFamily.TabIndex = 17;
            this.chkFamily.Tag = "Checkbox_Contact_Family";
            this.chkFamily.Text = "Family";
            this.chkFamily.UseVisualStyleBackColor = true;
            this.chkFamily.CheckedChanged += new System.EventHandler(this.chkFamily_CheckedChanged);
            // 
            // chkBlackmail
            // 
            this.chkBlackmail.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkBlackmail.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.chkBlackmail, 2);
            this.chkBlackmail.DefaultColorScheme = true;
            this.chkBlackmail.Location = new System.Drawing.Point(418, 34);
            this.chkBlackmail.Name = "chkBlackmail";
            this.chkBlackmail.Size = new System.Drawing.Size(71, 17);
            this.chkBlackmail.TabIndex = 16;
            this.chkBlackmail.Tag = "Checkbox_Contact_Blackmail";
            this.chkBlackmail.Text = "Blackmail";
            this.chkBlackmail.UseVisualStyleBackColor = true;
            this.chkBlackmail.CheckedChanged += new System.EventHandler(this.chkBlackmail_CheckedChanged);
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 13;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.lblLoyalty, 3, 1);
            this.tlpMain.Controls.Add(this.nudConnection, 2, 1);
            this.tlpMain.Controls.Add(this.nudLoyalty, 4, 1);
            this.tlpMain.Controls.Add(this.imgLink, 5, 1);
            this.tlpMain.Controls.Add(this.imgNotes, 6, 1);
            this.tlpMain.Controls.Add(this.chkGroup, 8, 1);
            this.tlpMain.Controls.Add(this.cmdDelete, 12, 0);
            this.tlpMain.Controls.Add(this.chkFamily, 11, 1);
            this.tlpMain.Controls.Add(this.chkFree, 7, 1);
            this.tlpMain.Controls.Add(this.lblQuickStats, 10, 0);
            this.tlpMain.Controls.Add(this.tlpStatBlock, 0, 2);
            this.tlpMain.Controls.Add(this.lblConnection, 0, 1);
            this.tlpMain.Controls.Add(this.cmdExpand, 0, 0);
            this.tlpMain.Controls.Add(this.chkBlackmail, 9, 1);
            this.tlpMain.Controls.Add(this.tlpComboBoxes, 1, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 3;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(553, 170);
            this.tlpMain.TabIndex = 35;
            // 
            // tlpStatBlock
            // 
            this.tlpStatBlock.AutoSize = true;
            this.tlpStatBlock.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpStatBlock.ColumnCount = 4;
            this.tlpMain.SetColumnSpan(this.tlpStatBlock, 13);
            this.tlpStatBlock.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpStatBlock.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpStatBlock.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpStatBlock.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpStatBlock.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpStatBlock.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpStatBlock.Controls.Add(this.cboMetatype, 1, 1);
            this.tlpStatBlock.Controls.Add(this.cboGender, 1, 2);
            this.tlpStatBlock.Controls.Add(this.cboType, 1, 4);
            this.tlpStatBlock.Controls.Add(this.cboAge, 1, 3);
            this.tlpStatBlock.Controls.Add(this.lblType, 0, 4);
            this.tlpStatBlock.Controls.Add(this.lblMetatype, 0, 1);
            this.tlpStatBlock.Controls.Add(this.lblGender, 0, 2);
            this.tlpStatBlock.Controls.Add(this.lblAge, 0, 3);
            this.tlpStatBlock.Controls.Add(this.lblPersonalLife, 2, 1);
            this.tlpStatBlock.Controls.Add(this.cboPersonalLife, 3, 1);
            this.tlpStatBlock.Controls.Add(this.lblPreferredPayment, 2, 2);
            this.tlpStatBlock.Controls.Add(this.cboPreferredPayment, 3, 2);
            this.tlpStatBlock.Controls.Add(this.lblHobbiesVice, 2, 3);
            this.tlpStatBlock.Controls.Add(this.cboHobbiesVice, 3, 3);
            this.tlpStatBlock.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpStatBlock.Location = new System.Drawing.Point(3, 59);
            this.tlpStatBlock.Name = "tlpStatBlock";
            this.tlpStatBlock.RowCount = 5;
            this.tlpStatBlock.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpStatBlock.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpStatBlock.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpStatBlock.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpStatBlock.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpStatBlock.Size = new System.Drawing.Size(547, 108);
            this.tlpStatBlock.TabIndex = 34;
            this.tlpStatBlock.Visible = false;
            // 
            // cboMetatype
            // 
            this.cboMetatype.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboMetatype.FormattingEnabled = true;
            this.cboMetatype.Location = new System.Drawing.Point(63, 3);
            this.cboMetatype.Name = "cboMetatype";
            this.cboMetatype.Size = new System.Drawing.Size(186, 21);
            this.cboMetatype.TabIndex = 20;
            this.cboMetatype.TooltipText = "";
            this.cboMetatype.TextChanged += new System.EventHandler(this.cboMetatype_TextChanged);
            // 
            // cboGender
            // 
            this.cboGender.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboGender.FormattingEnabled = true;
            this.cboGender.Location = new System.Drawing.Point(63, 30);
            this.cboGender.Name = "cboGender";
            this.cboGender.Size = new System.Drawing.Size(186, 21);
            this.cboGender.TabIndex = 21;
            this.cboGender.TooltipText = "";
            this.cboGender.TextChanged += new System.EventHandler(this.cboGender_TextChanged);
            // 
            // cboType
            // 
            this.cboType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboType.FormattingEnabled = true;
            this.cboType.Location = new System.Drawing.Point(63, 84);
            this.cboType.Name = "cboType";
            this.cboType.Size = new System.Drawing.Size(186, 21);
            this.cboType.TabIndex = 22;
            this.cboType.TooltipText = "";
            this.cboType.TextChanged += new System.EventHandler(this.cboType_TextChanged);
            // 
            // cboAge
            // 
            this.cboAge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboAge.FormattingEnabled = true;
            this.cboAge.Location = new System.Drawing.Point(63, 57);
            this.cboAge.Name = "cboAge";
            this.cboAge.Size = new System.Drawing.Size(186, 21);
            this.cboAge.TabIndex = 24;
            this.cboAge.TooltipText = "";
            this.cboAge.TextChanged += new System.EventHandler(this.cboAge_TextChanged);
            // 
            // lblType
            // 
            this.lblType.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(23, 88);
            this.lblType.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(34, 13);
            this.lblType.TabIndex = 31;
            this.lblType.Tag = "Label_Type";
            this.lblType.Text = "Type:";
            this.lblType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblMetatype
            // 
            this.lblMetatype.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMetatype.AutoSize = true;
            this.lblMetatype.Location = new System.Drawing.Point(3, 7);
            this.lblMetatype.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetatype.Name = "lblMetatype";
            this.lblMetatype.Size = new System.Drawing.Size(54, 13);
            this.lblMetatype.TabIndex = 27;
            this.lblMetatype.Tag = "Label_Metatype";
            this.lblMetatype.Text = "Metatype:";
            this.lblMetatype.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblGender
            // 
            this.lblGender.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblGender.AutoSize = true;
            this.lblGender.Location = new System.Drawing.Point(12, 34);
            this.lblGender.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGender.Name = "lblGender";
            this.lblGender.Size = new System.Drawing.Size(45, 13);
            this.lblGender.TabIndex = 28;
            this.lblGender.Tag = "Label_Gender";
            this.lblGender.Text = "Gender:";
            this.lblGender.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblAge
            // 
            this.lblAge.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblAge.AutoSize = true;
            this.lblAge.Location = new System.Drawing.Point(28, 61);
            this.lblAge.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
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
            this.lblPersonalLife.Location = new System.Drawing.Point(281, 7);
            this.lblPersonalLife.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPersonalLife.Name = "lblPersonalLife";
            this.lblPersonalLife.Size = new System.Drawing.Size(71, 13);
            this.lblPersonalLife.TabIndex = 30;
            this.lblPersonalLife.Tag = "Label_Contact_PersonalLife";
            this.lblPersonalLife.Text = "Personal Life:";
            this.lblPersonalLife.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboPersonalLife
            // 
            this.cboPersonalLife.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboPersonalLife.FormattingEnabled = true;
            this.cboPersonalLife.Location = new System.Drawing.Point(358, 3);
            this.cboPersonalLife.Name = "cboPersonalLife";
            this.cboPersonalLife.Size = new System.Drawing.Size(186, 21);
            this.cboPersonalLife.TabIndex = 26;
            this.cboPersonalLife.TooltipText = "";
            this.cboPersonalLife.TextChanged += new System.EventHandler(this.cboPersonalLife_TextChanged);
            // 
            // lblPreferredPayment
            // 
            this.lblPreferredPayment.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblPreferredPayment.AutoSize = true;
            this.lblPreferredPayment.Location = new System.Drawing.Point(255, 34);
            this.lblPreferredPayment.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPreferredPayment.Name = "lblPreferredPayment";
            this.lblPreferredPayment.Size = new System.Drawing.Size(97, 13);
            this.lblPreferredPayment.TabIndex = 32;
            this.lblPreferredPayment.Tag = "Label_Contact_PreferredPayment";
            this.lblPreferredPayment.Text = "Preferred Payment:";
            this.lblPreferredPayment.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboPreferredPayment
            // 
            this.cboPreferredPayment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboPreferredPayment.FormattingEnabled = true;
            this.cboPreferredPayment.Location = new System.Drawing.Point(358, 30);
            this.cboPreferredPayment.Name = "cboPreferredPayment";
            this.cboPreferredPayment.Size = new System.Drawing.Size(186, 21);
            this.cboPreferredPayment.TabIndex = 23;
            this.cboPreferredPayment.TooltipText = "";
            this.cboPreferredPayment.TextChanged += new System.EventHandler(this.cboPreferredPayment_TextChanged);
            // 
            // lblHobbiesVice
            // 
            this.lblHobbiesVice.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblHobbiesVice.AutoSize = true;
            this.lblHobbiesVice.Location = new System.Drawing.Point(277, 61);
            this.lblHobbiesVice.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblHobbiesVice.Name = "lblHobbiesVice";
            this.lblHobbiesVice.Size = new System.Drawing.Size(75, 13);
            this.lblHobbiesVice.TabIndex = 33;
            this.lblHobbiesVice.Tag = "Label_Contact_HobbiesVice";
            this.lblHobbiesVice.Text = "Hobbies/Vice:";
            this.lblHobbiesVice.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboHobbiesVice
            // 
            this.cboHobbiesVice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboHobbiesVice.FormattingEnabled = true;
            this.cboHobbiesVice.Location = new System.Drawing.Point(358, 57);
            this.cboHobbiesVice.Name = "cboHobbiesVice";
            this.cboHobbiesVice.Size = new System.Drawing.Size(186, 21);
            this.cboHobbiesVice.TabIndex = 25;
            this.cboHobbiesVice.TooltipText = "";
            this.cboHobbiesVice.TextChanged += new System.EventHandler(this.cboHobbiesVice_TextChanged);
            // 
            // tlpComboBoxes
            // 
            this.tlpComboBoxes.AutoSize = true;
            this.tlpComboBoxes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpComboBoxes.ColumnCount = 3;
            this.tlpMain.SetColumnSpan(this.tlpComboBoxes, 9);
            this.tlpComboBoxes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpComboBoxes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpComboBoxes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpComboBoxes.Controls.Add(this.txtContactName, 0, 0);
            this.tlpComboBoxes.Controls.Add(this.txtContactLocation, 1, 0);
            this.tlpComboBoxes.Controls.Add(this.cboContactRole, 2, 0);
            this.tlpComboBoxes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpComboBoxes.Location = new System.Drawing.Point(29, 0);
            this.tlpComboBoxes.Margin = new System.Windows.Forms.Padding(0);
            this.tlpComboBoxes.Name = "tlpComboBoxes";
            this.tlpComboBoxes.RowCount = 1;
            this.tlpComboBoxes.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpComboBoxes.Size = new System.Drawing.Size(424, 30);
            this.tlpComboBoxes.TabIndex = 35;
            // 
            // ContactControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.MinimumSize = new System.Drawing.Size(480, 0);
            this.Name = "ContactControl";
            this.Size = new System.Drawing.Size(553, 170);
            this.Load += new System.EventHandler(this.ContactControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudConnection)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLoyalty)).EndInit();
            this.cmsContact.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.imgNotes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgLink)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpStatBlock.ResumeLayout(false);
            this.tlpStatBlock.PerformLayout();
            this.tlpComboBoxes.ResumeLayout(false);
            this.tlpComboBoxes.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Chummer.NumericUpDownEx nudConnection;
        private Chummer.NumericUpDownEx nudLoyalty;
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
        private Chummer.ColorableCheckBox chkGroup;
        private Chummer.ColorableCheckBox chkFree;
        private System.Windows.Forms.Label lblQuickStats;
        private System.Windows.Forms.Label lblConnection;
        private System.Windows.Forms.Label lblLoyalty;
        private BufferedTableLayoutPanel tlpMain;
        private BufferedTableLayoutPanel tlpComboBoxes;
        private BufferedTableLayoutPanel tlpStatBlock;
        private ElasticComboBox cboMetatype;
        private ElasticComboBox cboGender;
        private System.Windows.Forms.Label lblHobbiesVice;
        private ElasticComboBox cboType;
        private System.Windows.Forms.Label lblPreferredPayment;
        private ElasticComboBox cboAge;
        private System.Windows.Forms.Label lblPersonalLife;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.Label lblMetatype;
        private System.Windows.Forms.Label lblGender;
        private System.Windows.Forms.Label lblAge;
        private ElasticComboBox cboPreferredPayment;
        private ElasticComboBox cboHobbiesVice;
        private ElasticComboBox cboPersonalLife;
        private Chummer.ColorableCheckBox chkBlackmail;
        private Chummer.ColorableCheckBox chkFamily;
    }
}
