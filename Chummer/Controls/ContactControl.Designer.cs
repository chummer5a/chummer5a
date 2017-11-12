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
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.tipTooltip = new TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip();
            this.cboContactRole = new System.Windows.Forms.ComboBox();
            this.txtContactName = new System.Windows.Forms.TextBox();
            this.txtContactLocation = new System.Windows.Forms.TextBox();
            this.cmdExpand = new System.Windows.Forms.Button();
            this.imgNotes = new System.Windows.Forms.PictureBox();
            this.imgLink = new System.Windows.Forms.PictureBox();
            this.chkGroup = new System.Windows.Forms.CheckBox();
            this.chkFree = new System.Windows.Forms.CheckBox();
            this.lblQuickStats = new System.Windows.Forms.Label();
            this.lblLine = new System.Windows.Forms.Label();
            this.chkBlackmail = new System.Windows.Forms.CheckBox();
            this.chkFamily = new System.Windows.Forms.CheckBox();
            this.lblConnection = new System.Windows.Forms.Label();
            this.lblLoyalty = new System.Windows.Forms.Label();
            this.cboMetatype = new System.Windows.Forms.ComboBox();
            this.cboSex = new System.Windows.Forms.ComboBox();
            this.cboType = new System.Windows.Forms.ComboBox();
            this.cboPreferredPayment = new System.Windows.Forms.ComboBox();
            this.cboAge = new System.Windows.Forms.ComboBox();
            this.cboHobbiesVice = new System.Windows.Forms.ComboBox();
            this.cboPersonalLife = new System.Windows.Forms.ComboBox();
            this.lblMetatype = new System.Windows.Forms.Label();
            this.lblSex = new System.Windows.Forms.Label();
            this.lblAge = new System.Windows.Forms.Label();
            this.lblPersonalLife = new System.Windows.Forms.Label();
            this.lblType = new System.Windows.Forms.Label();
            this.lblPreferredPayment = new System.Windows.Forms.Label();
            this.lblHobbiesVice = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudConnection)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLoyalty)).BeginInit();
            this.cmsContact.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgNotes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgLink)).BeginInit();
            this.SuspendLayout();
            // 
            // nudConnection
            // 
            this.nudConnection.Location = new System.Drawing.Point(70, 24);
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
            this.tipTooltip.SetToolTip(this.nudConnection, "Connection");
            this.nudConnection.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudConnection.ValueChanged += new System.EventHandler(this.nudConnection_ValueChanged);
            // 
            // nudLoyalty
            // 
            this.nudLoyalty.Location = new System.Drawing.Point(161, 24);
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
            this.tipTooltip.SetToolTip(this.nudLoyalty, "Loyalty");
            this.nudLoyalty.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudLoyalty.ValueChanged += new System.EventHandler(this.nudLoyalty_ValueChanged);
            // 
            // cmdDelete
            // 
            this.cmdDelete.Location = new System.Drawing.Point(416, 0);
            this.cmdDelete.Margin = new System.Windows.Forms.Padding(3, 3, 3, 1);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(49, 23);
            this.cmdDelete.TabIndex = 7;
            this.cmdDelete.Tag = "String_Delete";
            this.cmdDelete.Text = "Delete";
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
            this.cmsContact.Opening += new System.ComponentModel.CancelEventHandler(this.cmsContact_Opening);
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
            // tipTooltip
            // 
            this.tipTooltip.AllowLinksHandling = true;
            this.tipTooltip.AutoPopDelay = 10000;
            this.tipTooltip.BaseStylesheet = null;
            this.tipTooltip.InitialDelay = 250;
            this.tipTooltip.IsBalloon = true;
            this.tipTooltip.MaximumSize = new System.Drawing.Size(0, 0);
            this.tipTooltip.OwnerDraw = true;
            this.tipTooltip.ReshowDelay = 100;
            this.tipTooltip.TooltipCssClass = "htmltooltip";
            this.tipTooltip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.tipTooltip.ToolTipTitle = "Chummer Help";
            // 
            // cboContactRole
            // 
            this.cboContactRole.FormattingEnabled = true;
            this.cboContactRole.Location = new System.Drawing.Point(254, 0);
            this.cboContactRole.Margin = new System.Windows.Forms.Padding(3, 3, 3, 1);
            this.cboContactRole.Name = "cboContactRole";
            this.cboContactRole.Size = new System.Drawing.Size(120, 21);
            this.cboContactRole.TabIndex = 2;
            this.cboContactRole.TextChanged += new System.EventHandler(this.cboContactRole_TextChanged);
            // 
            // txtContactName
            // 
            this.txtContactName.Location = new System.Drawing.Point(3, 0);
            this.txtContactName.Margin = new System.Windows.Forms.Padding(3, 3, 3, 1);
            this.txtContactName.Name = "txtContactName";
            this.txtContactName.Size = new System.Drawing.Size(120, 20);
            this.txtContactName.TabIndex = 0;
            this.txtContactName.TextChanged += new System.EventHandler(this.txtContactName_TextChanged);
            // 
            // txtContactLocation
            // 
            this.txtContactLocation.Location = new System.Drawing.Point(128, 0);
            this.txtContactLocation.Margin = new System.Windows.Forms.Padding(3, 3, 3, 1);
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
            this.cmdExpand.Location = new System.Drawing.Point(468, 1);
            this.cmdExpand.Margin = new System.Windows.Forms.Padding(0);
            this.cmdExpand.Name = "cmdExpand";
            this.cmdExpand.Size = new System.Drawing.Size(21, 21);
            this.cmdExpand.TabIndex = 11;
            this.cmdExpand.UseVisualStyleBackColor = true;
            this.cmdExpand.Click += new System.EventHandler(this.cmdExpand_Click);
            // 
            // imgNotes
            // 
            this.imgNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.imgNotes.Location = new System.Drawing.Point(233, 25);
            this.imgNotes.Name = "imgNotes";
            this.imgNotes.Size = new System.Drawing.Size(16, 16);
            this.imgNotes.TabIndex = 10;
            this.imgNotes.TabStop = false;
            this.imgNotes.Click += new System.EventHandler(this.imgNotes_Click);
            // 
            // imgLink
            // 
            this.imgLink.Image = global::Chummer.Properties.Resources.link;
            this.imgLink.Location = new System.Drawing.Point(207, 25);
            this.imgLink.Name = "imgLink";
            this.imgLink.Size = new System.Drawing.Size(16, 16);
            this.imgLink.TabIndex = 6;
            this.imgLink.TabStop = false;
            this.imgLink.Click += new System.EventHandler(this.imgLink_Click);
            // 
            // chkGroup
            // 
            this.chkGroup.AutoSize = true;
            this.chkGroup.Location = new System.Drawing.Point(256, 25);
            this.chkGroup.Name = "chkGroup";
            this.chkGroup.Size = new System.Drawing.Size(55, 17);
            this.chkGroup.TabIndex = 12;
            this.chkGroup.Text = "Group";
            this.chkGroup.UseVisualStyleBackColor = true;
            this.chkGroup.CheckedChanged += new System.EventHandler(this.chkGroup_CheckedChanged);
            // 
            // chkFree
            // 
            this.chkFree.AutoSize = true;
            this.chkFree.Location = new System.Drawing.Point(311, 25);
            this.chkFree.Name = "chkFree";
            this.chkFree.Size = new System.Drawing.Size(47, 17);
            this.chkFree.TabIndex = 13;
            this.chkFree.Text = "Free";
            this.chkFree.UseVisualStyleBackColor = true;
            this.chkFree.CheckedChanged += new System.EventHandler(this.chkFree_CheckedChanged);
            // 
            // lblQuickStats
            // 
            this.lblQuickStats.AutoSize = true;
            this.lblQuickStats.Location = new System.Drawing.Point(380, 5);
            this.lblQuickStats.Name = "lblQuickStats";
            this.lblQuickStats.Size = new System.Drawing.Size(30, 13);
            this.lblQuickStats.TabIndex = 14;
            this.lblQuickStats.Text = "(1/1)";
            // 
            // lblLine
            // 
            this.lblLine.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblLine.Location = new System.Drawing.Point(0, 145);
            this.lblLine.Name = "lblLine";
            this.lblLine.Size = new System.Drawing.Size(490, 2);
            this.lblLine.TabIndex = 15;
            this.lblLine.Visible = false;
            // 
            // chkBlackmail
            // 
            this.chkBlackmail.AutoSize = true;
            this.chkBlackmail.Location = new System.Drawing.Point(360, 25);
            this.chkBlackmail.Name = "chkBlackmail";
            this.chkBlackmail.Size = new System.Drawing.Size(71, 17);
            this.chkBlackmail.TabIndex = 16;
            this.chkBlackmail.Text = "Blackmail";
            this.chkBlackmail.UseVisualStyleBackColor = true;
            this.chkBlackmail.CheckedChanged += new System.EventHandler(this.chkBlackmail_CheckedChanged);
            // 
            // chkFamily
            // 
            this.chkFamily.AutoSize = true;
            this.chkFamily.Location = new System.Drawing.Point(439, 25);
            this.chkFamily.Name = "chkFamily";
            this.chkFamily.Size = new System.Drawing.Size(55, 17);
            this.chkFamily.TabIndex = 17;
            this.chkFamily.Text = "Family";
            this.chkFamily.UseVisualStyleBackColor = true;
            this.chkFamily.CheckedChanged += new System.EventHandler(this.chkFamily_CheckedChanged);
            // 
            // lblConnection
            // 
            this.lblConnection.AutoSize = true;
            this.lblConnection.Location = new System.Drawing.Point(0, 26);
            this.lblConnection.Name = "lblConnection";
            this.lblConnection.Size = new System.Drawing.Size(64, 13);
            this.lblConnection.TabIndex = 18;
            this.lblConnection.Tag = "Label_Contact_Connection";
            this.lblConnection.Text = "Connection:";
            // 
            // lblLoyalty
            // 
            this.lblLoyalty.AutoSize = true;
            this.lblLoyalty.Location = new System.Drawing.Point(112, 26);
            this.lblLoyalty.Name = "lblLoyalty";
            this.lblLoyalty.Size = new System.Drawing.Size(43, 13);
            this.lblLoyalty.TabIndex = 19;
            this.lblLoyalty.Tag = "Label_Contact_Loyalty";
            this.lblLoyalty.Text = "Loyalty:";
            // 
            // cboMetatype
            // 
            this.cboMetatype.FormattingEnabled = true;
            this.cboMetatype.Location = new System.Drawing.Point(70, 47);
            this.cboMetatype.Margin = new System.Windows.Forms.Padding(3, 3, 3, 1);
            this.cboMetatype.Name = "cboMetatype";
            this.cboMetatype.Size = new System.Drawing.Size(153, 21);
            this.cboMetatype.TabIndex = 20;
            this.cboMetatype.TextChanged += new System.EventHandler(this.cboMetatype_TextChanged);
            // 
            // cboSex
            // 
            this.cboSex.FormattingEnabled = true;
            this.cboSex.Location = new System.Drawing.Point(70, 72);
            this.cboSex.Margin = new System.Windows.Forms.Padding(3, 3, 3, 1);
            this.cboSex.Name = "cboSex";
            this.cboSex.Size = new System.Drawing.Size(153, 21);
            this.cboSex.TabIndex = 21;
            this.cboSex.TextChanged += new System.EventHandler(this.cboSex_TextChanged);
            // 
            // cboType
            // 
            this.cboType.FormattingEnabled = true;
            this.cboType.Location = new System.Drawing.Point(70, 122);
            this.cboType.Margin = new System.Windows.Forms.Padding(3, 3, 3, 1);
            this.cboType.Name = "cboType";
            this.cboType.Size = new System.Drawing.Size(153, 21);
            this.cboType.TabIndex = 22;
            this.cboType.TextChanged += new System.EventHandler(this.cboType_TextChanged);
            // 
            // cboPreferredPayment
            // 
            this.cboPreferredPayment.FormattingEnabled = true;
            this.cboPreferredPayment.Location = new System.Drawing.Point(333, 72);
            this.cboPreferredPayment.Margin = new System.Windows.Forms.Padding(3, 3, 3, 1);
            this.cboPreferredPayment.Name = "cboPreferredPayment";
            this.cboPreferredPayment.Size = new System.Drawing.Size(144, 21);
            this.cboPreferredPayment.TabIndex = 23;
            this.cboPreferredPayment.TextChanged += new System.EventHandler(this.cboPreferredPayment_TextChanged);
            // 
            // cboAge
            // 
            this.cboAge.FormattingEnabled = true;
            this.cboAge.Location = new System.Drawing.Point(70, 97);
            this.cboAge.Margin = new System.Windows.Forms.Padding(3, 3, 3, 1);
            this.cboAge.Name = "cboAge";
            this.cboAge.Size = new System.Drawing.Size(153, 21);
            this.cboAge.TabIndex = 24;
            this.cboAge.TextChanged += new System.EventHandler(this.cboAge_TextChanged);
            // 
            // cboHobbiesVice
            // 
            this.cboHobbiesVice.FormattingEnabled = true;
            this.cboHobbiesVice.Location = new System.Drawing.Point(333, 97);
            this.cboHobbiesVice.Margin = new System.Windows.Forms.Padding(3, 3, 3, 1);
            this.cboHobbiesVice.Name = "cboHobbiesVice";
            this.cboHobbiesVice.Size = new System.Drawing.Size(144, 21);
            this.cboHobbiesVice.TabIndex = 25;
            this.cboHobbiesVice.TextChanged += new System.EventHandler(this.cboHobbiesVice_TextChanged);
            // 
            // cboPersonalLife
            // 
            this.cboPersonalLife.FormattingEnabled = true;
            this.cboPersonalLife.Location = new System.Drawing.Point(333, 47);
            this.cboPersonalLife.Margin = new System.Windows.Forms.Padding(3, 3, 3, 1);
            this.cboPersonalLife.Name = "cboPersonalLife";
            this.cboPersonalLife.Size = new System.Drawing.Size(144, 21);
            this.cboPersonalLife.TabIndex = 26;
            this.cboPersonalLife.TextChanged += new System.EventHandler(this.cboPersonalLife_TextChanged);
            // 
            // lblMetatype
            // 
            this.lblMetatype.AutoSize = true;
            this.lblMetatype.Location = new System.Drawing.Point(8, 50);
            this.lblMetatype.Name = "lblMetatype";
            this.lblMetatype.Size = new System.Drawing.Size(54, 13);
            this.lblMetatype.TabIndex = 27;
            this.lblMetatype.Tag = "Label_Metatype";
            this.lblMetatype.Text = "Metatype:";
            this.lblMetatype.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblSex
            // 
            this.lblSex.AutoSize = true;
            this.lblSex.Location = new System.Drawing.Point(34, 75);
            this.lblSex.Name = "lblSex";
            this.lblSex.Size = new System.Drawing.Size(28, 13);
            this.lblSex.TabIndex = 28;
            this.lblSex.Tag = "Label_Sex";
            this.lblSex.Text = "Sex:";
            this.lblSex.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblAge
            // 
            this.lblAge.AutoSize = true;
            this.lblAge.Location = new System.Drawing.Point(33, 100);
            this.lblAge.Name = "lblAge";
            this.lblAge.Size = new System.Drawing.Size(29, 13);
            this.lblAge.TabIndex = 29;
            this.lblAge.Tag = "Label_Age";
            this.lblAge.Text = "Age:";
            this.lblAge.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblPersonalLife
            // 
            this.lblPersonalLife.AutoSize = true;
            this.lblPersonalLife.Location = new System.Drawing.Point(256, 50);
            this.lblPersonalLife.Name = "lblPersonalLife";
            this.lblPersonalLife.Size = new System.Drawing.Size(71, 13);
            this.lblPersonalLife.TabIndex = 30;
            this.lblPersonalLife.Tag = "Label_Contact_PersonalLife";
            this.lblPersonalLife.Text = "Personal Life:";
            this.lblPersonalLife.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(30, 125);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(34, 13);
            this.lblType.TabIndex = 31;
            this.lblType.Tag = "Label_Type";
            this.lblType.Text = "Type:";
            this.lblType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblPreferredPayment
            // 
            this.lblPreferredPayment.AutoSize = true;
            this.lblPreferredPayment.Location = new System.Drawing.Point(230, 75);
            this.lblPreferredPayment.Name = "lblPreferredPayment";
            this.lblPreferredPayment.Size = new System.Drawing.Size(97, 13);
            this.lblPreferredPayment.TabIndex = 32;
            this.lblPreferredPayment.Tag = "Label_Contact_PreferredPayment";
            this.lblPreferredPayment.Text = "Preferred Payment:";
            this.lblPreferredPayment.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblHobbiesVice
            // 
            this.lblHobbiesVice.AutoSize = true;
            this.lblHobbiesVice.Location = new System.Drawing.Point(252, 100);
            this.lblHobbiesVice.Name = "lblHobbiesVice";
            this.lblHobbiesVice.Size = new System.Drawing.Size(75, 13);
            this.lblHobbiesVice.TabIndex = 33;
            this.lblHobbiesVice.Tag = "Label_Contact_HobbiesVice";
            this.lblHobbiesVice.Text = "Hobbies/Vice:";
            this.lblHobbiesVice.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ContactControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.lblHobbiesVice);
            this.Controls.Add(this.lblPreferredPayment);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.lblPersonalLife);
            this.Controls.Add(this.lblAge);
            this.Controls.Add(this.lblSex);
            this.Controls.Add(this.lblMetatype);
            this.Controls.Add(this.cboPersonalLife);
            this.Controls.Add(this.cboHobbiesVice);
            this.Controls.Add(this.cboAge);
            this.Controls.Add(this.cboPreferredPayment);
            this.Controls.Add(this.cboType);
            this.Controls.Add(this.cboSex);
            this.Controls.Add(this.cboMetatype);
            this.Controls.Add(this.lblLoyalty);
            this.Controls.Add(this.lblConnection);
            this.Controls.Add(this.chkFamily);
            this.Controls.Add(this.chkBlackmail);
            this.Controls.Add(this.lblLine);
            this.Controls.Add(this.lblQuickStats);
            this.Controls.Add(this.chkFree);
            this.Controls.Add(this.chkGroup);
            this.Controls.Add(this.cmdExpand);
            this.Controls.Add(this.txtContactLocation);
            this.Controls.Add(this.txtContactName);
            this.Controls.Add(this.imgNotes);
            this.Controls.Add(this.imgLink);
            this.Controls.Add(this.cmdDelete);
            this.Controls.Add(this.nudConnection);
            this.Controls.Add(this.nudLoyalty);
            this.Controls.Add(this.cboContactRole);
            this.MinimumSize = new System.Drawing.Size(490, 22);
            this.Name = "ContactControl";
            this.Size = new System.Drawing.Size(492, 147);
            this.Load += new System.EventHandler(this.ContactControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudConnection)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLoyalty)).EndInit();
            this.cmsContact.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.imgNotes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgLink)).EndInit();
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
        private TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip tipTooltip;
        private System.Windows.Forms.PictureBox imgNotes;
        private System.Windows.Forms.ComboBox cboContactRole;
        private System.Windows.Forms.TextBox txtContactName;
        private System.Windows.Forms.TextBox txtContactLocation;
        private System.Windows.Forms.Button cmdExpand;
        private System.Windows.Forms.CheckBox chkGroup;
        private System.Windows.Forms.CheckBox chkFree;
        private System.Windows.Forms.Label lblQuickStats;
        private System.Windows.Forms.Label lblLine;
        private System.Windows.Forms.CheckBox chkBlackmail;
        private System.Windows.Forms.CheckBox chkFamily;
        private System.Windows.Forms.Label lblConnection;
        private System.Windows.Forms.Label lblLoyalty;
        private System.Windows.Forms.ComboBox cboMetatype;
        private System.Windows.Forms.ComboBox cboSex;
        private System.Windows.Forms.ComboBox cboType;
        private System.Windows.Forms.ComboBox cboPreferredPayment;
        private System.Windows.Forms.ComboBox cboAge;
        private System.Windows.Forms.ComboBox cboHobbiesVice;
        private System.Windows.Forms.ComboBox cboPersonalLife;
        private System.Windows.Forms.Label lblMetatype;
        private System.Windows.Forms.Label lblSex;
        private System.Windows.Forms.Label lblAge;
        private System.Windows.Forms.Label lblPersonalLife;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.Label lblPreferredPayment;
        private System.Windows.Forms.Label lblHobbiesVice;
    }
}
