namespace Chummer
{
    partial class SpiritControl
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
                UnbindSpiritControl();
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
            this.cmdDelete = new System.Windows.Forms.Button();
            this.lblServices = new System.Windows.Forms.Label();
            this.nudServices = new Chummer.NumericUpDownEx();
            this.lblForce = new System.Windows.Forms.Label();
            this.nudForce = new Chummer.NumericUpDownEx();
            this.chkBound = new Chummer.ColorableCheckBox(this.components);
            this.cboSpiritName = new Chummer.ElasticComboBox();
            this.cmsSpirit = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsContactOpen = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.tsRemoveCharacter = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.tsAttachCharacter = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.tsCreateCharacter = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.txtCritterName = new System.Windows.Forms.TextBox();
            this.chkFettered = new Chummer.ColorableCheckBox(this.components);
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdNotes = new Chummer.ButtonWithToolTip(this.components);
            this.cmdLink = new Chummer.ButtonWithToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.nudServices)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudForce)).BeginInit();
            this.cmsSpirit.SuspendLayout();
            this.tlpMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdDelete
            // 
            this.cmdDelete.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmdDelete.AutoSize = true;
            this.cmdDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDelete.Location = new System.Drawing.Point(814, 3);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(48, 23);
            this.cmdDelete.TabIndex = 6;
            this.cmdDelete.Tag = "String_Delete";
            this.cmdDelete.Text = "Delete";
            this.cmdDelete.UseVisualStyleBackColor = true;
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // lblServices
            // 
            this.lblServices.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblServices.AutoSize = true;
            this.lblServices.Location = new System.Drawing.Point(333, 8);
            this.lblServices.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblServices.Name = "lblServices";
            this.lblServices.Size = new System.Drawing.Size(82, 13);
            this.lblServices.TabIndex = 3;
            this.lblServices.Tag = "Label_Spirit_ServicesOwed";
            this.lblServices.Text = "Services Owed:";
            this.lblServices.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // nudServices
            // 
            this.nudServices.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudServices.AutoSize = true;
            this.nudServices.Location = new System.Drawing.Point(421, 5);
            this.nudServices.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.nudServices.Maximum = new decimal(new int[] {
            -1,
            -1,
            -1,
            0});
            this.nudServices.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudServices.Name = "nudServices";
            this.nudServices.Size = new System.Drawing.Size(197, 20);
            this.nudServices.TabIndex = 4;
            this.nudServices.ValueChanged += new System.EventHandler(this.nudServices_ValueChanged);
            // 
            // lblForce
            // 
            this.lblForce.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblForce.AutoSize = true;
            this.lblForce.Location = new System.Drawing.Point(255, 8);
            this.lblForce.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblForce.Name = "lblForce";
            this.lblForce.Size = new System.Drawing.Size(37, 13);
            this.lblForce.TabIndex = 1;
            this.lblForce.Tag = "Label_Spirit_Force";
            this.lblForce.Text = "Force:";
            this.lblForce.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // nudForce
            // 
            this.nudForce.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudForce.AutoSize = true;
            this.nudForce.Enabled = false;
            this.nudForce.Location = new System.Drawing.Point(298, 5);
            this.nudForce.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.nudForce.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.nudForce.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudForce.Name = "nudForce";
            this.nudForce.Size = new System.Drawing.Size(29, 20);
            this.nudForce.TabIndex = 2;
            this.nudForce.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudForce.ValueChanged += new System.EventHandler(this.nudForce_ValueChanged);
            // 
            // chkBound
            // 
            this.chkBound.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkBound.AutoSize = true;
            this.chkBound.Checked = true;
            this.chkBound.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBound.DefaultColorScheme = true;
            this.chkBound.Enabled = false;
            this.chkBound.Location = new System.Drawing.Point(624, 6);
            this.chkBound.Name = "chkBound";
            this.chkBound.Size = new System.Drawing.Size(57, 17);
            this.chkBound.TabIndex = 5;
            this.chkBound.Tag = "Checkbox_Spirit_Bound";
            this.chkBound.Text = "Bound";
            this.chkBound.UseVisualStyleBackColor = true;
            this.chkBound.CheckedChanged += new System.EventHandler(this.chkBound_CheckedChanged);
            // 
            // cboSpiritName
            // 
            this.cboSpiritName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSpiritName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSpiritName.FormattingEnabled = true;
            this.cboSpiritName.Location = new System.Drawing.Point(3, 4);
            this.cboSpiritName.Name = "cboSpiritName";
            this.cboSpiritName.Size = new System.Drawing.Size(120, 21);
            this.cboSpiritName.TabIndex = 7;
            this.cboSpiritName.TooltipText = "";
            this.cboSpiritName.SelectedIndexChanged += new System.EventHandler(this.cboSpiritName_SelectedIndexChanged);
            // 
            // cmsSpirit
            // 
            this.cmsSpirit.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsContactOpen,
            this.tsRemoveCharacter,
            this.tsAttachCharacter,
            this.tsCreateCharacter});
            this.cmsSpirit.Name = "cmsContact";
            this.cmsSpirit.Size = new System.Drawing.Size(172, 92);
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
            this.tsAttachCharacter.Image = global::Chummer.Properties.Resources.link_add;
            this.tsAttachCharacter.ImageDpi120 = null;
            this.tsAttachCharacter.ImageDpi144 = null;
            this.tsAttachCharacter.ImageDpi192 = global::Chummer.Properties.Resources.link_add1;
            this.tsAttachCharacter.ImageDpi288 = null;
            this.tsAttachCharacter.ImageDpi384 = null;
            this.tsAttachCharacter.ImageDpi96 = global::Chummer.Properties.Resources.link_add;
            this.tsAttachCharacter.Name = "tsAttachCharacter";
            this.tsAttachCharacter.Size = new System.Drawing.Size(171, 22);
            this.tsAttachCharacter.Tag = "MenuItem_AttachCharacter";
            this.tsAttachCharacter.Text = "Attach Character";
            this.tsAttachCharacter.Click += new System.EventHandler(this.tsAttachCharacter_Click);
            // 
            // tsCreateCharacter
            // 
            this.tsCreateCharacter.Image = global::Chummer.Properties.Resources.ladybird_add;
            this.tsCreateCharacter.ImageDpi120 = null;
            this.tsCreateCharacter.ImageDpi144 = null;
            this.tsCreateCharacter.ImageDpi192 = global::Chummer.Properties.Resources.ladybird_add1;
            this.tsCreateCharacter.ImageDpi288 = null;
            this.tsCreateCharacter.ImageDpi384 = null;
            this.tsCreateCharacter.ImageDpi96 = global::Chummer.Properties.Resources.ladybird_add;
            this.tsCreateCharacter.Name = "tsCreateCharacter";
            this.tsCreateCharacter.Size = new System.Drawing.Size(171, 22);
            this.tsCreateCharacter.Tag = "MenuItem_CreateCritter";
            this.tsCreateCharacter.Text = "Create Critter";
            this.tsCreateCharacter.Click += new System.EventHandler(this.tsCreateCharacter_Click);
            // 
            // txtCritterName
            // 
            this.txtCritterName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCritterName.Location = new System.Drawing.Point(129, 5);
            this.txtCritterName.Name = "txtCritterName";
            this.txtCritterName.Size = new System.Drawing.Size(120, 20);
            this.txtCritterName.TabIndex = 12;
            this.txtCritterName.TextChanged += new System.EventHandler(this.txtCritterName_TextChanged);
            // 
            // chkFettered
            // 
            this.chkFettered.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkFettered.AutoSize = true;
            this.chkFettered.DefaultColorScheme = true;
            this.chkFettered.Location = new System.Drawing.Point(687, 6);
            this.chkFettered.Name = "chkFettered";
            this.chkFettered.Size = new System.Drawing.Size(65, 17);
            this.chkFettered.TabIndex = 13;
            this.chkFettered.Tag = "Checkbox_Spirit_Fettered";
            this.chkFettered.Text = "Fettered";
            this.chkFettered.UseVisualStyleBackColor = true;
            this.chkFettered.CheckedChanged += new System.EventHandler(this.chkFettered_CheckedChanged);
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 11;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Controls.Add(this.cmdDelete, 10, 0);
            this.tlpMain.Controls.Add(this.chkFettered, 7, 0);
            this.tlpMain.Controls.Add(this.chkBound, 6, 0);
            this.tlpMain.Controls.Add(this.nudServices, 5, 0);
            this.tlpMain.Controls.Add(this.lblServices, 4, 0);
            this.tlpMain.Controls.Add(this.nudForce, 3, 0);
            this.tlpMain.Controls.Add(this.lblForce, 2, 0);
            this.tlpMain.Controls.Add(this.cboSpiritName, 0, 0);
            this.tlpMain.Controls.Add(this.txtCritterName, 1, 0);
            this.tlpMain.Controls.Add(this.cmdNotes, 9, 0);
            this.tlpMain.Controls.Add(this.cmdLink, 8, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 1;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(865, 30);
            this.tlpMain.TabIndex = 14;
            // 
            // cmdNotes
            // 
            this.cmdNotes.AutoSize = true;
            this.cmdNotes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdNotes.FlatAppearance.BorderSize = 0;
            this.cmdNotes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.cmdNotes.ImageDpi120 = null;
            this.cmdNotes.ImageDpi144 = null;
            this.cmdNotes.ImageDpi192 = global::Chummer.Properties.Resources.note_edit1;
            this.cmdNotes.ImageDpi288 = null;
            this.cmdNotes.ImageDpi384 = null;
            this.cmdNotes.ImageDpi96 = global::Chummer.Properties.Resources.note_edit;
            this.cmdNotes.Location = new System.Drawing.Point(786, 3);
            this.cmdNotes.Name = "cmdNotes";
            this.cmdNotes.Size = new System.Drawing.Size(22, 24);
            this.cmdNotes.TabIndex = 14;
            this.cmdNotes.UseVisualStyleBackColor = true;
            this.cmdNotes.Click += new System.EventHandler(this.cmdNotes_Click);
            // 
            // cmdLink
            // 
            this.cmdLink.AutoSize = true;
            this.cmdLink.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdLink.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdLink.FlatAppearance.BorderSize = 0;
            this.cmdLink.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdLink.Image = global::Chummer.Properties.Resources.link;
            this.cmdLink.ImageDpi120 = null;
            this.cmdLink.ImageDpi144 = null;
            this.cmdLink.ImageDpi192 = global::Chummer.Properties.Resources.link1;
            this.cmdLink.ImageDpi288 = null;
            this.cmdLink.ImageDpi384 = null;
            this.cmdLink.ImageDpi96 = global::Chummer.Properties.Resources.link;
            this.cmdLink.Location = new System.Drawing.Point(758, 3);
            this.cmdLink.Name = "cmdLink";
            this.cmdLink.Size = new System.Drawing.Size(22, 24);
            this.cmdLink.TabIndex = 15;
            this.cmdLink.UseVisualStyleBackColor = true;
            this.cmdLink.Click += new System.EventHandler(this.cmdLink_Click);
            // 
            // SpiritControl
            // 
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.MinimumSize = new System.Drawing.Size(0, 30);
            this.Name = "SpiritControl";
            this.Size = new System.Drawing.Size(865, 30);
            this.Load += new System.EventHandler(this.SpiritControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudServices)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudForce)).EndInit();
            this.cmsSpirit.ResumeLayout(false);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdDelete;
        private System.Windows.Forms.Label lblServices;
        private Chummer.NumericUpDownEx nudServices;
        private System.Windows.Forms.Label lblForce;
        private Chummer.NumericUpDownEx nudForce;
        private Chummer.ColorableCheckBox chkBound;
        private ElasticComboBox cboSpiritName;
        private System.Windows.Forms.ContextMenuStrip cmsSpirit;
        private System.Windows.Forms.TextBox txtCritterName;
        private Chummer.ColorableCheckBox chkFettered;
        private BufferedTableLayoutPanel tlpMain;
        private DpiFriendlyToolStripMenuItem tsContactOpen;
        private DpiFriendlyToolStripMenuItem tsRemoveCharacter;
        private DpiFriendlyToolStripMenuItem tsAttachCharacter;
        private DpiFriendlyToolStripMenuItem tsCreateCharacter;
        private ButtonWithToolTip cmdNotes;
        private ButtonWithToolTip cmdLink;
    }
}
