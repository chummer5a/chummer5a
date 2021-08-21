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
            this.cmsContact = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsContactOpen = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.tsRemoveCharacter = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.tsAttachCharacter = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.cboContactRole = new Chummer.ElasticComboBox();
            this.txtContactName = new System.Windows.Forms.TextBox();
            this.txtContactLocation = new System.Windows.Forms.TextBox();
            this.cmdExpand = new Chummer.DpiFriendlyImagedButton(this.components);
            this.lblQuickStats = new System.Windows.Forms.Label();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdDelete = new Chummer.DpiFriendlyImagedButton(this.components);
            this.tlpComboBoxes = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblContactArchtypeLabel = new System.Windows.Forms.Label();
            this.lblContactLocationLabel = new System.Windows.Forms.Label();
            this.lblContactNameLabel = new System.Windows.Forms.Label();
            this.cmdNotes = new Chummer.ButtonWithToolTip();
            this.cmsContact.SuspendLayout();
            this.tlpMain.SuspendLayout();
            this.tlpComboBoxes.SuspendLayout();
            this.SuspendLayout();
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
            this.tsContactOpen.ImageDpi120 = null;
            this.tsContactOpen.ImageDpi144 = null;
            this.tsContactOpen.ImageDpi192 = global::Chummer.Properties.Resources.link_go1;
            this.tsContactOpen.ImageDpi288 = null;
            this.tsContactOpen.ImageDpi384 = null;
            this.tsContactOpen.ImageDpi96 = global::Chummer.Properties.Resources.link_go;
            this.tsContactOpen.Name = "tsContactOpen";
            this.tsContactOpen.Size = new System.Drawing.Size(171, 22);
            this.tsContactOpen.Tag = "MenuItem_OpenCharacter";
            this.tsContactOpen.Text = "Open Character";
            this.tsContactOpen.Click += new System.EventHandler(this.tsContactOpen_Click);
            // 
            // tsRemoveCharacter
            // 
            this.tsRemoveCharacter.Image = global::Chummer.Properties.Resources.link_delete;
            this.tsRemoveCharacter.ImageDpi120 = null;
            this.tsRemoveCharacter.ImageDpi144 = null;
            this.tsRemoveCharacter.ImageDpi192 = global::Chummer.Properties.Resources.link_delete1;
            this.tsRemoveCharacter.ImageDpi288 = null;
            this.tsRemoveCharacter.ImageDpi384 = null;
            this.tsRemoveCharacter.ImageDpi96 = global::Chummer.Properties.Resources.link_delete;
            this.tsRemoveCharacter.Name = "tsRemoveCharacter";
            this.tsRemoveCharacter.Size = new System.Drawing.Size(171, 22);
            this.tsRemoveCharacter.Tag = "MenuItem_RemoveCharacter";
            this.tsRemoveCharacter.Text = "Remove Character";
            this.tsRemoveCharacter.Click += new System.EventHandler(this.tsRemoveCharacter_Click);
            // 
            // tsAttachCharacter
            // 
            this.tsAttachCharacter.Image = global::Chummer.Properties.Resources.link_add1;
            this.tsAttachCharacter.ImageDpi120 = null;
            this.tsAttachCharacter.ImageDpi144 = null;
            this.tsAttachCharacter.ImageDpi192 = null;
            this.tsAttachCharacter.ImageDpi288 = null;
            this.tsAttachCharacter.ImageDpi384 = null;
            this.tsAttachCharacter.ImageDpi96 = global::Chummer.Properties.Resources.link_add1;
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
            this.cboContactRole.Location = new System.Drawing.Point(360, 31);
            this.cboContactRole.Name = "cboContactRole";
            this.cboContactRole.Size = new System.Drawing.Size(174, 21);
            this.cboContactRole.TabIndex = 2;
            this.cboContactRole.TooltipText = "";
            this.cboContactRole.SelectedIndexChanged += new System.EventHandler(this.UpdateContactRole);
            this.cboContactRole.TextChanged += new System.EventHandler(this.cboContactRole_TextChanged);
            this.cboContactRole.Leave += new System.EventHandler(this.UpdateContactRole);
            // 
            // txtContactName
            // 
            this.txtContactName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtContactName.Location = new System.Drawing.Point(3, 31);
            this.txtContactName.Name = "txtContactName";
            this.txtContactName.Size = new System.Drawing.Size(172, 20);
            this.txtContactName.TabIndex = 0;
            this.txtContactName.TextChanged += new System.EventHandler(this.txtContactName_TextChanged);
            // 
            // txtContactLocation
            // 
            this.txtContactLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpComboBoxes.SetColumnSpan(this.txtContactLocation, 2);
            this.txtContactLocation.Location = new System.Drawing.Point(181, 31);
            this.txtContactLocation.Name = "txtContactLocation";
            this.txtContactLocation.Size = new System.Drawing.Size(173, 20);
            this.txtContactLocation.TabIndex = 1;
            this.txtContactLocation.TextChanged += new System.EventHandler(this.txtContactLocation_TextChanged);
            // 
            // cmdExpand
            // 
            this.cmdExpand.AutoSize = true;
            this.cmdExpand.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdExpand.Image = global::Chummer.Properties.Resources.toggle_expand;
            this.cmdExpand.ImageDpi120 = null;
            this.cmdExpand.ImageDpi144 = null;
            this.cmdExpand.ImageDpi192 = global::Chummer.Properties.Resources.toggle_expand1;
            this.cmdExpand.ImageDpi288 = null;
            this.cmdExpand.ImageDpi384 = null;
            this.cmdExpand.ImageDpi96 = global::Chummer.Properties.Resources.toggle_expand;
            this.cmdExpand.Location = new System.Drawing.Point(3, 3);
            this.cmdExpand.Name = "cmdExpand";
            this.cmdExpand.Padding = new System.Windows.Forms.Padding(1);
            this.tlpMain.SetRowSpan(this.cmdExpand, 2);
            this.cmdExpand.Size = new System.Drawing.Size(24, 24);
            this.cmdExpand.TabIndex = 11;
            this.cmdExpand.UseVisualStyleBackColor = true;
            this.cmdExpand.Click += new System.EventHandler(this.cmdExpand_Click);
            // 
            // lblQuickStats
            // 
            this.lblQuickStats.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblQuickStats.AutoSize = true;
            this.lblQuickStats.Location = new System.Drawing.Point(570, 22);
            this.lblQuickStats.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblQuickStats.MinimumSize = new System.Drawing.Size(40, 0);
            this.lblQuickStats.Name = "lblQuickStats";
            this.tlpMain.SetRowSpan(this.lblQuickStats, 2);
            this.lblQuickStats.Size = new System.Drawing.Size(40, 13);
            this.lblQuickStats.TabIndex = 14;
            this.lblQuickStats.Text = "(00/0)";
            this.lblQuickStats.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 13;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.cmdDelete, 11, 0);
            this.tlpMain.Controls.Add(this.lblQuickStats, 10, 0);
            this.tlpMain.Controls.Add(this.cmdExpand, 0, 0);
            this.tlpMain.Controls.Add(this.tlpComboBoxes, 1, 0);
            this.tlpMain.Controls.Add(this.cmdNotes, 12, 1);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 4;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(643, 58);
            this.tlpMain.TabIndex = 35;
            // 
            // cmdDelete
            // 
            this.cmdDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdDelete.AutoSize = true;
            this.cmdDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.SetColumnSpan(this.cmdDelete, 2);
            this.cmdDelete.Image = global::Chummer.Properties.Resources.delete;
            this.cmdDelete.ImageDpi120 = null;
            this.cmdDelete.ImageDpi144 = null;
            this.cmdDelete.ImageDpi192 = global::Chummer.Properties.Resources.delete1;
            this.cmdDelete.ImageDpi288 = null;
            this.cmdDelete.ImageDpi384 = null;
            this.cmdDelete.ImageDpi96 = global::Chummer.Properties.Resources.delete;
            this.cmdDelete.Location = new System.Drawing.Point(616, 3);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Padding = new System.Windows.Forms.Padding(1);
            this.cmdDelete.Size = new System.Drawing.Size(24, 24);
            this.cmdDelete.TabIndex = 7;
            this.cmdDelete.Tag = "";
            this.cmdDelete.UseVisualStyleBackColor = true;
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // tlpComboBoxes
            // 
            this.tlpComboBoxes.AutoSize = true;
            this.tlpComboBoxes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpComboBoxes.ColumnCount = 4;
            this.tlpMain.SetColumnSpan(this.tlpComboBoxes, 9);
            this.tlpComboBoxes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpComboBoxes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tlpComboBoxes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpComboBoxes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tlpComboBoxes.Controls.Add(this.lblContactArchtypeLabel, 3, 0);
            this.tlpComboBoxes.Controls.Add(this.lblContactLocationLabel, 1, 0);
            this.tlpComboBoxes.Controls.Add(this.lblContactNameLabel, 0, 0);
            this.tlpComboBoxes.Controls.Add(this.txtContactName, 0, 1);
            this.tlpComboBoxes.Controls.Add(this.txtContactLocation, 1, 1);
            this.tlpComboBoxes.Controls.Add(this.cboContactRole, 3, 1);
            this.tlpComboBoxes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpComboBoxes.Location = new System.Drawing.Point(30, 0);
            this.tlpComboBoxes.Margin = new System.Windows.Forms.Padding(0);
            this.tlpComboBoxes.Name = "tlpComboBoxes";
            this.tlpComboBoxes.RowCount = 2;
            this.tlpMain.SetRowSpan(this.tlpComboBoxes, 2);
            this.tlpComboBoxes.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpComboBoxes.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpComboBoxes.Size = new System.Drawing.Size(537, 58);
            this.tlpComboBoxes.TabIndex = 35;
            // 
            // lblContactArchtypeLabel
            // 
            this.lblContactArchtypeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblContactArchtypeLabel.AutoSize = true;
            this.lblContactArchtypeLabel.Location = new System.Drawing.Point(360, 6);
            this.lblContactArchtypeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
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
            this.tlpComboBoxes.SetColumnSpan(this.lblContactLocationLabel, 2);
            this.lblContactLocationLabel.Location = new System.Drawing.Point(181, 6);
            this.lblContactLocationLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
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
            this.lblContactNameLabel.Location = new System.Drawing.Point(3, 6);
            this.lblContactNameLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblContactNameLabel.Name = "lblContactNameLabel";
            this.lblContactNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblContactNameLabel.TabIndex = 43;
            this.lblContactNameLabel.Tag = "Label_Name";
            this.lblContactNameLabel.Text = "Name:";
            // 
            // cmdNotes
            // 
            this.cmdNotes.AutoSize = true;
            this.cmdNotes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdNotes.FlatAppearance.BorderSize = 0;
            this.cmdNotes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.cmdNotes.ImageDpi120 = null;
            this.cmdNotes.ImageDpi144 = null;
            this.cmdNotes.ImageDpi192 = global::Chummer.Properties.Resources.note_edit1;
            this.cmdNotes.ImageDpi288 = null;
            this.cmdNotes.ImageDpi384 = null;
            this.cmdNotes.ImageDpi96 = global::Chummer.Properties.Resources.note_edit;
            this.cmdNotes.Location = new System.Drawing.Point(618, 33);
            this.cmdNotes.Name = "cmdNotes";
            this.cmdNotes.Size = new System.Drawing.Size(22, 22);
            this.cmdNotes.TabIndex = 36;
            this.cmdNotes.ToolTipText = "";
            this.cmdNotes.UseVisualStyleBackColor = true;
            this.cmdNotes.Click += new System.EventHandler(this.cmdNotes_Click);
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
            this.Size = new System.Drawing.Size(643, 58);
            this.Load += new System.EventHandler(this.ContactControl_Load);
            this.cmsContact.ResumeLayout(false);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpComboBoxes.ResumeLayout(false);
            this.tlpComboBoxes.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ContextMenuStrip cmsContact;
        private ElasticComboBox cboContactRole;
        private System.Windows.Forms.TextBox txtContactName;
        private System.Windows.Forms.TextBox txtContactLocation;
        private DpiFriendlyImagedButton cmdExpand;
        private System.Windows.Forms.Label lblQuickStats;
        private BufferedTableLayoutPanel tlpMain;
        private BufferedTableLayoutPanel tlpComboBoxes;
        private System.Windows.Forms.Label lblContactNameLabel;
        private System.Windows.Forms.Label lblContactLocationLabel;
        private System.Windows.Forms.Label lblContactArchtypeLabel;
        private DpiFriendlyImagedButton cmdDelete;
        private DpiFriendlyToolStripMenuItem tsContactOpen;
        private DpiFriendlyToolStripMenuItem tsRemoveCharacter;
        private DpiFriendlyToolStripMenuItem tsAttachCharacter;
        private ButtonWithToolTip cmdNotes;
    }
}
