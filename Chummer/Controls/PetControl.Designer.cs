namespace Chummer
{
    partial class PetControl
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
                UnbindPetControl();
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
            this.txtContactName = new System.Windows.Forms.TextBox();
            this.cmdDelete = new System.Windows.Forms.Button();
            this.cmsContact = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.imgLink = new System.Windows.Forms.PictureBox();
            this.imgNotes = new System.Windows.Forms.PictureBox();
            this.lblName = new System.Windows.Forms.Label();
            this.lblMetatypeLabel = new System.Windows.Forms.Label();
            this.cboMetatype = new Chummer.ElasticComboBox();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tsContactOpen = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.tsRemoveCharacter = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.tsAttachCharacter = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.cmsContact.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgLink)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgNotes)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtContactName
            // 
            this.txtContactName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtContactName.BackColor = System.Drawing.SystemColors.Window;
            this.txtContactName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtContactName.Location = new System.Drawing.Point(47, 5);
            this.txtContactName.Name = "txtContactName";
            this.txtContactName.Size = new System.Drawing.Size(271, 20);
            this.txtContactName.TabIndex = 11;
            this.txtContactName.TextChanged += new System.EventHandler(this.txtContactName_TextChanged);
            // 
            // cmdDelete
            // 
            this.cmdDelete.AutoSize = true;
            this.cmdDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDelete.Location = new System.Drawing.Point(705, 3);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(48, 23);
            this.cmdDelete.TabIndex = 18;
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
            this.cmsContact.Size = new System.Drawing.Size(181, 92);
            // 
            // imgLink
            // 
            this.imgLink.Cursor = System.Windows.Forms.Cursors.Hand;
            this.imgLink.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imgLink.Image = global::Chummer.Properties.Resources.link;
            this.imgLink.Location = new System.Drawing.Point(661, 0);
            this.imgLink.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.imgLink.Name = "imgLink";
            this.imgLink.Size = new System.Drawing.Size(16, 30);
            this.imgLink.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imgLink.TabIndex = 17;
            this.imgLink.TabStop = false;
            this.imgLink.Click += new System.EventHandler(this.imgLink_Click);
            // 
            // imgNotes
            // 
            this.imgNotes.Cursor = System.Windows.Forms.Cursors.Hand;
            this.imgNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imgNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.imgNotes.Location = new System.Drawing.Point(683, 0);
            this.imgNotes.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.imgNotes.Name = "imgNotes";
            this.imgNotes.Size = new System.Drawing.Size(16, 30);
            this.imgNotes.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imgNotes.TabIndex = 21;
            this.imgNotes.TabStop = false;
            this.imgNotes.Click += new System.EventHandler(this.imgNotes_Click);
            // 
            // lblName
            // 
            this.lblName.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(3, 8);
            this.lblName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(38, 13);
            this.lblName.TabIndex = 22;
            this.lblName.Tag = "Label_CharacterName";
            this.lblName.Text = "Name:";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblMetatypeLabel
            // 
            this.lblMetatypeLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMetatypeLabel.AutoSize = true;
            this.lblMetatypeLabel.Location = new System.Drawing.Point(324, 8);
            this.lblMetatypeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetatypeLabel.Name = "lblMetatypeLabel";
            this.lblMetatypeLabel.Size = new System.Drawing.Size(54, 13);
            this.lblMetatypeLabel.TabIndex = 23;
            this.lblMetatypeLabel.Tag = "Label_Metatype";
            this.lblMetatypeLabel.Text = "Metatype:";
            this.lblMetatypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboMetatype
            // 
            this.cboMetatype.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboMetatype.FormattingEnabled = true;
            this.cboMetatype.Location = new System.Drawing.Point(384, 4);
            this.cboMetatype.Name = "cboMetatype";
            this.cboMetatype.Size = new System.Drawing.Size(271, 21);
            this.cboMetatype.TabIndex = 24;
            this.cboMetatype.TooltipText = "";
            this.cboMetatype.TextChanged += new System.EventHandler(this.cboMetatype_TextChanged);
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 7;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Controls.Add(this.cmdDelete, 6, 0);
            this.tlpMain.Controls.Add(this.imgNotes, 5, 0);
            this.tlpMain.Controls.Add(this.imgLink, 4, 0);
            this.tlpMain.Controls.Add(this.lblName, 0, 0);
            this.tlpMain.Controls.Add(this.cboMetatype, 3, 0);
            this.tlpMain.Controls.Add(this.txtContactName, 1, 0);
            this.tlpMain.Controls.Add(this.lblMetatypeLabel, 2, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 1;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(756, 30);
            this.tlpMain.TabIndex = 25;
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
            this.tsContactOpen.Size = new System.Drawing.Size(180, 22);
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
            this.tsRemoveCharacter.Size = new System.Drawing.Size(180, 22);
            this.tsRemoveCharacter.Tag = "MenuItem_RemoveCharacter";
            this.tsRemoveCharacter.Text = "Remove Character";
            this.tsRemoveCharacter.Click += new System.EventHandler(this.tsRemoveCharacter_Click);
            // 
            // tsAttachCharacter
            // 
            this.tsAttachCharacter.Image = global::Chummer.Properties.Resources.link_add;
            this.tsAttachCharacter.ImageDpi120 = null;
            this.tsAttachCharacter.ImageDpi144 = null;
            this.tsAttachCharacter.ImageDpi192 = global::Chummer.Properties.Resources.link_add1;
            this.tsAttachCharacter.ImageDpi288 = null;
            this.tsAttachCharacter.ImageDpi384 = null;
            this.tsAttachCharacter.ImageDpi96 = global::Chummer.Properties.Resources.link_add;
            this.tsAttachCharacter.Name = "tsAttachCharacter";
            this.tsAttachCharacter.Size = new System.Drawing.Size(180, 22);
            this.tsAttachCharacter.Tag = "MenuItem_AttachCharacter";
            this.tsAttachCharacter.Text = "Attach Character";
            this.tsAttachCharacter.Click += new System.EventHandler(this.tsAttachCharacter_Click);
            // 
            // PetControl
            // 
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.MinimumSize = new System.Drawing.Size(0, 30);
            this.Name = "PetControl";
            this.Size = new System.Drawing.Size(756, 30);
            this.Load += new System.EventHandler(this.PetControl_Load);
            this.cmsContact.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.imgLink)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgNotes)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.PictureBox imgLink;
        private System.Windows.Forms.TextBox txtContactName;
        private System.Windows.Forms.PictureBox imgNotes;
        private System.Windows.Forms.Button cmdDelete;
        private System.Windows.Forms.ContextMenuStrip cmsContact;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblMetatypeLabel;
        private ElasticComboBox cboMetatype;
        private BufferedTableLayoutPanel tlpMain;
        private DpiFriendlyToolStripMenuItem tsContactOpen;
        private DpiFriendlyToolStripMenuItem tsRemoveCharacter;
        private DpiFriendlyToolStripMenuItem tsAttachCharacter;
    }
}
