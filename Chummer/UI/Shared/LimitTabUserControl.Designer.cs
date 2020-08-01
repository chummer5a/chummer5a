namespace Chummer.UI.Shared
{
    partial class LimitTabUserControl
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
                if (!(ParentForm is CharacterShared frmParent) || frmParent.CharacterObject != _objCharacter)
                    _objCharacter?.Dispose();
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
            this.treLimit = new System.Windows.Forms.TreeView();
            this.flowButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.cmdAddLimitModifier = new System.Windows.Forms.Button();
            this.cmdDeleteLimitModifier = new System.Windows.Forms.Button();
            this.tlpParent = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpDetails = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblMentalLimitLabel = new System.Windows.Forms.Label();
            this.lblSocialLimitLabel = new System.Windows.Forms.Label();
            this.lblPhysicalLimitLabel = new System.Windows.Forms.Label();
            this.lblAstral = new Chummer.LabelWithToolTip();
            this.lblPhysical = new Chummer.LabelWithToolTip();
            this.lblMental = new Chummer.LabelWithToolTip();
            this.lblSocial = new Chummer.LabelWithToolTip();
            this.lblAstralLabel = new System.Windows.Forms.Label();
            this.cmsLimitModifier = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tssLimitModifierEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.tssLimitModifierNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsLimitModifierNotesOnly = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tssLimitModifierNotesOnlyNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.flowButtons.SuspendLayout();
            this.tlpParent.SuspendLayout();
            this.tlpDetails.SuspendLayout();
            this.cmsLimitModifier.SuspendLayout();
            this.cmsLimitModifierNotesOnly.SuspendLayout();
            this.SuspendLayout();
            // 
            // treLimit
            // 
            this.treLimit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treLimit.HideSelection = false;
            this.treLimit.Location = new System.Drawing.Point(3, 32);
            this.treLimit.Name = "treLimit";
            this.treLimit.ShowNodeToolTips = true;
            this.treLimit.ShowPlusMinus = false;
            this.treLimit.ShowRootLines = false;
            this.treLimit.Size = new System.Drawing.Size(295, 391);
            this.treLimit.TabIndex = 91;
            this.treLimit.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treLimit_AfterSelect);
            this.treLimit.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treLimit_KeyDown);
            // 
            // flowButtons
            // 
            this.flowButtons.AutoSize = true;
            this.flowButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpParent.SetColumnSpan(this.flowButtons, 2);
            this.flowButtons.Controls.Add(this.cmdAddLimitModifier);
            this.flowButtons.Controls.Add(this.cmdDeleteLimitModifier);
            this.flowButtons.Location = new System.Drawing.Point(0, 0);
            this.flowButtons.Margin = new System.Windows.Forms.Padding(0);
            this.flowButtons.Name = "flowButtons";
            this.flowButtons.Size = new System.Drawing.Size(197, 29);
            this.flowButtons.TabIndex = 92;
            this.flowButtons.WrapContents = false;
            // 
            // cmdAddLimitModifier
            // 
            this.cmdAddLimitModifier.AutoSize = true;
            this.cmdAddLimitModifier.Location = new System.Drawing.Point(3, 3);
            this.cmdAddLimitModifier.Name = "cmdAddLimitModifier";
            this.cmdAddLimitModifier.Size = new System.Drawing.Size(105, 23);
            this.cmdAddLimitModifier.TabIndex = 80;
            this.cmdAddLimitModifier.Tag = "String_AddLimitModifier";
            this.cmdAddLimitModifier.Text = "Add Limit Modifier";
            this.cmdAddLimitModifier.UseVisualStyleBackColor = true;
            this.cmdAddLimitModifier.Click += new System.EventHandler(this.cmdAddLimitModifier_Click);
            // 
            // cmdDeleteLimitModifier
            // 
            this.cmdDeleteLimitModifier.AutoSize = true;
            this.cmdDeleteLimitModifier.Enabled = false;
            this.cmdDeleteLimitModifier.Location = new System.Drawing.Point(114, 3);
            this.cmdDeleteLimitModifier.Name = "cmdDeleteLimitModifier";
            this.cmdDeleteLimitModifier.Size = new System.Drawing.Size(80, 23);
            this.cmdDeleteLimitModifier.TabIndex = 78;
            this.cmdDeleteLimitModifier.Tag = "String_Delete";
            this.cmdDeleteLimitModifier.Text = "Delete";
            this.cmdDeleteLimitModifier.UseVisualStyleBackColor = true;
            this.cmdDeleteLimitModifier.Click += new System.EventHandler(this.cmdDeleteLimitModifier_Click);
            // 
            // tlpParent
            // 
            this.tlpParent.AutoSize = true;
            this.tlpParent.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpParent.ColumnCount = 2;
            this.tlpParent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpParent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpParent.Controls.Add(this.treLimit, 0, 1);
            this.tlpParent.Controls.Add(this.flowButtons, 0, 0);
            this.tlpParent.Controls.Add(this.tlpDetails, 1, 1);
            this.tlpParent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpParent.Location = new System.Drawing.Point(0, 0);
            this.tlpParent.Name = "tlpParent";
            this.tlpParent.RowCount = 2;
            this.tlpParent.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpParent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpParent.Size = new System.Drawing.Size(480, 426);
            this.tlpParent.TabIndex = 93;
            // 
            // tlpDetails
            // 
            this.tlpDetails.AutoSize = true;
            this.tlpDetails.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpDetails.ColumnCount = 2;
            this.tlpDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpDetails.Controls.Add(this.lblMentalLimitLabel, 0, 1);
            this.tlpDetails.Controls.Add(this.lblSocialLimitLabel, 0, 2);
            this.tlpDetails.Controls.Add(this.lblPhysicalLimitLabel, 0, 0);
            this.tlpDetails.Controls.Add(this.lblAstral, 1, 3);
            this.tlpDetails.Controls.Add(this.lblPhysical, 1, 0);
            this.tlpDetails.Controls.Add(this.lblMental, 1, 1);
            this.tlpDetails.Controls.Add(this.lblSocial, 1, 2);
            this.tlpDetails.Controls.Add(this.lblAstralLabel, 0, 3);
            this.tlpDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpDetails.Location = new System.Drawing.Point(301, 29);
            this.tlpDetails.Margin = new System.Windows.Forms.Padding(0);
            this.tlpDetails.Name = "tlpDetails";
            this.tlpDetails.RowCount = 4;
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDetails.Size = new System.Drawing.Size(179, 397);
            this.tlpDetails.TabIndex = 93;
            // 
            // lblMentalLimitLabel
            // 
            this.lblMentalLimitLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMentalLimitLabel.AutoSize = true;
            this.lblMentalLimitLabel.Location = new System.Drawing.Point(10, 31);
            this.lblMentalLimitLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMentalLimitLabel.Name = "lblMentalLimitLabel";
            this.lblMentalLimitLabel.Size = new System.Drawing.Size(39, 13);
            this.lblMentalLimitLabel.TabIndex = 83;
            this.lblMentalLimitLabel.Tag = "Node_Mental";
            this.lblMentalLimitLabel.Text = "Mental";
            // 
            // lblSocialLimitLabel
            // 
            this.lblSocialLimitLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSocialLimitLabel.AutoSize = true;
            this.lblSocialLimitLabel.Location = new System.Drawing.Point(13, 56);
            this.lblSocialLimitLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSocialLimitLabel.Name = "lblSocialLimitLabel";
            this.lblSocialLimitLabel.Size = new System.Drawing.Size(36, 13);
            this.lblSocialLimitLabel.TabIndex = 85;
            this.lblSocialLimitLabel.Tag = "Node_Social";
            this.lblSocialLimitLabel.Text = "Social";
            // 
            // lblPhysicalLimitLabel
            // 
            this.lblPhysicalLimitLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPhysicalLimitLabel.AutoSize = true;
            this.lblPhysicalLimitLabel.Location = new System.Drawing.Point(3, 6);
            this.lblPhysicalLimitLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPhysicalLimitLabel.Name = "lblPhysicalLimitLabel";
            this.lblPhysicalLimitLabel.Size = new System.Drawing.Size(46, 13);
            this.lblPhysicalLimitLabel.TabIndex = 81;
            this.lblPhysicalLimitLabel.Tag = "Node_Physical";
            this.lblPhysicalLimitLabel.Text = "Physical";
            // 
            // lblAstral
            // 
            this.lblAstral.AutoSize = true;
            this.lblAstral.Location = new System.Drawing.Point(55, 81);
            this.lblAstral.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAstral.Name = "lblAstral";
            this.lblAstral.Size = new System.Drawing.Size(19, 13);
            this.lblAstral.TabIndex = 88;
            this.lblAstral.Text = "[0]";
            this.lblAstral.ToolTipText = "";
            // 
            // lblPhysical
            // 
            this.lblPhysical.AutoSize = true;
            this.lblPhysical.Location = new System.Drawing.Point(55, 6);
            this.lblPhysical.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPhysical.Name = "lblPhysical";
            this.lblPhysical.Size = new System.Drawing.Size(19, 13);
            this.lblPhysical.TabIndex = 82;
            this.lblPhysical.Text = "[0]";
            this.lblPhysical.ToolTipText = "";
            // 
            // lblMental
            // 
            this.lblMental.AutoSize = true;
            this.lblMental.Location = new System.Drawing.Point(55, 31);
            this.lblMental.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMental.Name = "lblMental";
            this.lblMental.Size = new System.Drawing.Size(19, 13);
            this.lblMental.TabIndex = 84;
            this.lblMental.Text = "[0]";
            this.lblMental.ToolTipText = "";
            // 
            // lblSocial
            // 
            this.lblSocial.AutoSize = true;
            this.lblSocial.Location = new System.Drawing.Point(55, 56);
            this.lblSocial.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSocial.Name = "lblSocial";
            this.lblSocial.Size = new System.Drawing.Size(19, 13);
            this.lblSocial.TabIndex = 86;
            this.lblSocial.Text = "[0]";
            this.lblSocial.ToolTipText = "";
            // 
            // lblAstralLabel
            // 
            this.lblAstralLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAstralLabel.AutoSize = true;
            this.lblAstralLabel.Location = new System.Drawing.Point(16, 81);
            this.lblAstralLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAstralLabel.Name = "lblAstralLabel";
            this.lblAstralLabel.Size = new System.Drawing.Size(33, 13);
            this.lblAstralLabel.TabIndex = 87;
            this.lblAstralLabel.Tag = "Node_Astral";
            this.lblAstralLabel.Text = "Astral";
            // 
            // cmsLimitModifier
            // 
            this.cmsLimitModifier.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tssLimitModifierEdit,
            this.tssLimitModifierNotes});
            this.cmsLimitModifier.Name = "cmsLimitModifier";
            this.cmsLimitModifier.Size = new System.Drawing.Size(106, 48);
            // 
            // tssLimitModifierEdit
            // 
            this.tssLimitModifierEdit.Image = global::Chummer.Properties.Resources.house_edit;
            this.tssLimitModifierEdit.Name = "tssLimitModifierEdit";
            this.tssLimitModifierEdit.Size = new System.Drawing.Size(105, 22);
            this.tssLimitModifierEdit.Tag = "Menu_Main_Edit";
            this.tssLimitModifierEdit.Text = "&Edit";
            this.tssLimitModifierEdit.Click += new System.EventHandler(this.tssLimitModifierEdit_Click);
            // 
            // tssLimitModifierNotes
            // 
            this.tssLimitModifierNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tssLimitModifierNotes.Name = "tssLimitModifierNotes";
            this.tssLimitModifierNotes.Size = new System.Drawing.Size(105, 22);
            this.tssLimitModifierNotes.Tag = "Menu_Notes";
            this.tssLimitModifierNotes.Text = "&Notes";
            this.tssLimitModifierNotes.Click += new System.EventHandler(this.tssLimitModifierNotes_Click);
            // 
            // cmsLimitModifierNotesOnly
            // 
            this.cmsLimitModifierNotesOnly.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tssLimitModifierNotesOnlyNotes});
            this.cmsLimitModifierNotesOnly.Name = "cmsLimitModifier";
            this.cmsLimitModifierNotesOnly.Size = new System.Drawing.Size(106, 26);
            // 
            // tssLimitModifierNotesOnlyNotes
            // 
            this.tssLimitModifierNotesOnlyNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tssLimitModifierNotesOnlyNotes.Name = "tssLimitModifierNotesOnlyNotes";
            this.tssLimitModifierNotesOnlyNotes.Size = new System.Drawing.Size(105, 22);
            this.tssLimitModifierNotesOnlyNotes.Tag = "Menu_Notes";
            this.tssLimitModifierNotesOnlyNotes.Text = "&Notes";
            this.tssLimitModifierNotesOnlyNotes.Click += new System.EventHandler(this.tssLimitModifierNotes_Click);
            // 
            // LimitTabUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tlpParent);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(480, 0);
            this.Name = "LimitTabUserControl";
            this.Size = new System.Drawing.Size(480, 426);
            this.Load += new System.EventHandler(this.LimitTabUserControl_Load);
            this.flowButtons.ResumeLayout(false);
            this.flowButtons.PerformLayout();
            this.tlpParent.ResumeLayout(false);
            this.tlpParent.PerformLayout();
            this.tlpDetails.ResumeLayout(false);
            this.tlpDetails.PerformLayout();
            this.cmsLimitModifier.ResumeLayout(false);
            this.cmsLimitModifierNotesOnly.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treLimit;
        private System.Windows.Forms.FlowLayoutPanel flowButtons;
        private System.Windows.Forms.Button cmdAddLimitModifier;
        private System.Windows.Forms.Button cmdDeleteLimitModifier;
        private Chummer.BufferedTableLayoutPanel tlpParent;
        private Chummer.BufferedTableLayoutPanel tlpDetails;
        private System.Windows.Forms.Label lblMentalLimitLabel;
        private System.Windows.Forms.Label lblSocialLimitLabel;
        private System.Windows.Forms.Label lblPhysicalLimitLabel;
        private LabelWithToolTip lblAstral;
        private LabelWithToolTip lblPhysical;
        private LabelWithToolTip lblMental;
        private LabelWithToolTip lblSocial;
        private System.Windows.Forms.Label lblAstralLabel;
        private System.Windows.Forms.ContextMenuStrip cmsLimitModifier;
        private System.Windows.Forms.ToolStripMenuItem tssLimitModifierEdit;
        private System.Windows.Forms.ToolStripMenuItem tssLimitModifierNotes;
        private System.Windows.Forms.ContextMenuStrip cmsLimitModifierNotesOnly;
        private System.Windows.Forms.ToolStripMenuItem tssLimitModifierNotesOnlyNotes;
    }
}
