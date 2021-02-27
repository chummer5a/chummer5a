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
            this.tlpComboBoxes = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblContactArchtypeLabel = new System.Windows.Forms.Label();
            this.lblContactLocationLabel = new System.Windows.Forms.Label();
            this.lblContactNameLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudConnection)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLoyalty)).BeginInit();
            this.cmsContact.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgNotes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgLink)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.tlpComboBoxes.SuspendLayout();
            this.SuspendLayout();
            // 
            // nudConnection
            // 
            this.nudConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudConnection.AutoSize = true;
            this.nudConnection.Location = new System.Drawing.Point(73, 49);
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
            this.nudConnection.Size = new System.Drawing.Size(153, 20);
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
            this.nudLoyalty.Location = new System.Drawing.Point(281, 49);
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
            this.nudLoyalty.Size = new System.Drawing.Size(153, 20);
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
            this.cmdDelete.Image = global::Chummer.Properties.Resources.delete;
            this.cmdDelete.Location = new System.Drawing.Point(706, 3);
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
            this.cboContactRole.Location = new System.Drawing.Point(405, 22);
            this.cboContactRole.Name = "cboContactRole";
            this.cboContactRole.Size = new System.Drawing.Size(196, 21);
            this.cboContactRole.TabIndex = 2;
            this.cboContactRole.TooltipText = "";
            this.cboContactRole.TextChanged += new System.EventHandler(this.cboContactRole_TextChanged);
            // 
            // txtContactName
            // 
            this.txtContactName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtContactName.Location = new System.Drawing.Point(3, 22);
            this.txtContactName.Name = "txtContactName";
            this.txtContactName.Size = new System.Drawing.Size(195, 20);
            this.txtContactName.TabIndex = 0;
            this.txtContactName.TextChanged += new System.EventHandler(this.txtContactName_TextChanged);
            // 
            // txtContactLocation
            // 
            this.txtContactLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtContactLocation.Location = new System.Drawing.Point(204, 22);
            this.txtContactLocation.Name = "txtContactLocation";
            this.txtContactLocation.Size = new System.Drawing.Size(195, 20);
            this.txtContactLocation.TabIndex = 1;
            this.txtContactLocation.TextChanged += new System.EventHandler(this.txtContactLocation_TextChanged);
            // 
            // cmdExpand
            // 
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
            this.imgNotes.Location = new System.Drawing.Point(462, 46);
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
            this.imgLink.Location = new System.Drawing.Point(440, 46);
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
            this.chkGroup.Location = new System.Drawing.Point(537, 50);
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
            this.chkFree.Location = new System.Drawing.Point(484, 50);
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
            this.lblQuickStats.Location = new System.Drawing.Point(636, 6);
            this.lblQuickStats.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblQuickStats.MinimumSize = new System.Drawing.Size(36, 0);
            this.lblQuickStats.Name = "lblQuickStats";
            this.lblQuickStats.Size = new System.Drawing.Size(64, 34);
            this.lblQuickStats.TabIndex = 14;
            this.lblQuickStats.Text = "(00/0)";
            this.lblQuickStats.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblConnection
            // 
            this.lblConnection.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblConnection.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.lblConnection, 2);
            this.lblConnection.Location = new System.Drawing.Point(3, 52);
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
            this.lblLoyalty.Location = new System.Drawing.Point(232, 52);
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
            this.chkFamily.Location = new System.Drawing.Point(675, 50);
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
            this.chkBlackmail.Location = new System.Drawing.Point(598, 50);
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
            this.tlpMain.Size = new System.Drawing.Size(733, 72);
            this.tlpMain.TabIndex = 35;
            // 
            // tlpComboBoxes
            // 
            this.tlpComboBoxes.AutoSize = true;
            this.tlpComboBoxes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpComboBoxes.ColumnCount = 3;
            this.tlpMain.SetColumnSpan(this.tlpComboBoxes, 9);
            this.tlpComboBoxes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpComboBoxes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tlpComboBoxes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tlpComboBoxes.Controls.Add(this.lblContactArchtypeLabel, 2, 0);
            this.tlpComboBoxes.Controls.Add(this.lblContactLocationLabel, 1, 0);
            this.tlpComboBoxes.Controls.Add(this.lblContactNameLabel, 0, 0);
            this.tlpComboBoxes.Controls.Add(this.txtContactName, 0, 1);
            this.tlpComboBoxes.Controls.Add(this.txtContactLocation, 1, 1);
            this.tlpComboBoxes.Controls.Add(this.cboContactRole, 2, 1);
            this.tlpComboBoxes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpComboBoxes.Location = new System.Drawing.Point(29, 0);
            this.tlpComboBoxes.Margin = new System.Windows.Forms.Padding(0);
            this.tlpComboBoxes.Name = "tlpComboBoxes";
            this.tlpComboBoxes.RowCount = 2;
            this.tlpComboBoxes.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpComboBoxes.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpComboBoxes.Size = new System.Drawing.Size(604, 46);
            this.tlpComboBoxes.TabIndex = 35;
            // 
            // lblContactArchtypeLabel
            // 
            this.lblContactArchtypeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblContactArchtypeLabel.AutoSize = true;
            this.lblContactArchtypeLabel.Location = new System.Drawing.Point(405, 3);
            this.lblContactArchtypeLabel.Margin = new System.Windows.Forms.Padding(3);
            this.lblContactArchtypeLabel.Name = "lblContactArchtypeLabel";
            this.lblContactArchtypeLabel.Size = new System.Drawing.Size(52, 13);
            this.lblContactArchtypeLabel.TabIndex = 45;
            this.lblContactArchtypeLabel.Tag = "Label_Archetype";
            this.lblContactArchtypeLabel.Text = "Archtype:";
            // 
            // lblContactLocationLabel
            // 
            this.lblContactLocationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblContactLocationLabel.AutoSize = true;
            this.lblContactLocationLabel.Location = new System.Drawing.Point(204, 3);
            this.lblContactLocationLabel.Margin = new System.Windows.Forms.Padding(3);
            this.lblContactLocationLabel.Name = "lblContactLocationLabel";
            this.lblContactLocationLabel.Size = new System.Drawing.Size(51, 13);
            this.lblContactLocationLabel.TabIndex = 44;
            this.lblContactLocationLabel.Tag = "Label_Location";
            this.lblContactLocationLabel.Text = "Location:";
            // 
            // lblContactNameLabel
            // 
            this.lblContactNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblContactNameLabel.AutoSize = true;
            this.lblContactNameLabel.Location = new System.Drawing.Point(3, 3);
            this.lblContactNameLabel.Margin = new System.Windows.Forms.Padding(3);
            this.lblContactNameLabel.Name = "lblContactNameLabel";
            this.lblContactNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblContactNameLabel.TabIndex = 43;
            this.lblContactNameLabel.Tag = "Label_Name";
            this.lblContactNameLabel.Text = "Name:";
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
            this.Size = new System.Drawing.Size(733, 72);
            this.Load += new System.EventHandler(this.ContactControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudConnection)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLoyalty)).EndInit();
            this.cmsContact.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.imgNotes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgLink)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
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
        private Chummer.ColorableCheckBox chkBlackmail;
        private Chummer.ColorableCheckBox chkFamily;
        private System.Windows.Forms.Label lblContactNameLabel;
        private System.Windows.Forms.Label lblContactLocationLabel;
        private System.Windows.Forms.Label lblContactArchtypeLabel;
    }
}
