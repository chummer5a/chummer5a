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
            this.nudServices = new System.Windows.Forms.NumericUpDown();
            this.lblForce = new System.Windows.Forms.Label();
            this.nudForce = new System.Windows.Forms.NumericUpDown();
            this.chkBound = new System.Windows.Forms.CheckBox();
            this.cboSpiritName = new Chummer.ElasticComboBox();
            this.imgLink = new System.Windows.Forms.PictureBox();
            this.cmsSpirit = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsContactOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.tsRemoveCharacter = new System.Windows.Forms.ToolStripMenuItem();
            this.tsAttachCharacter = new System.Windows.Forms.ToolStripMenuItem();
            this.tsCreateCharacter = new System.Windows.Forms.ToolStripMenuItem();
            this.imgNotes = new System.Windows.Forms.PictureBox();
            this.txtCritterName = new System.Windows.Forms.TextBox();
            this.chkFettered = new System.Windows.Forms.CheckBox();
            this.bufferedTableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.nudServices)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudForce)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgLink)).BeginInit();
            this.cmsSpirit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgNotes)).BeginInit();
            this.bufferedTableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdDelete
            // 
            this.cmdDelete.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdDelete.Location = new System.Drawing.Point(730, 3);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(75, 23);
            this.cmdDelete.TabIndex = 6;
            this.cmdDelete.Tag = "String_Delete";
            this.cmdDelete.Text = "Delete";
            this.cmdDelete.UseVisualStyleBackColor = true;
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // lblServices
            // 
            this.lblServices.AutoSize = true;
            this.lblServices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblServices.Location = new System.Drawing.Point(418, 0);
            this.lblServices.Name = "lblServices";
            this.lblServices.Size = new System.Drawing.Size(82, 29);
            this.lblServices.TabIndex = 3;
            this.lblServices.Tag = "Label_Spirit_ServicesOwed";
            this.lblServices.Text = "Services Owed:";
            this.lblServices.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // nudServices
            // 
            this.nudServices.Location = new System.Drawing.Point(506, 3);
            this.nudServices.Maximum = new decimal(new int[] {
            -1,
            -1,
            -1,
            0});
            this.nudServices.Name = "nudServices";
            this.nudServices.Size = new System.Drawing.Size(40, 20);
            this.nudServices.TabIndex = 4;
            this.nudServices.ValueChanged += new System.EventHandler(this.nudServices_ValueChanged);
            // 
            // lblForce
            // 
            this.lblForce.AutoSize = true;
            this.lblForce.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblForce.Location = new System.Drawing.Point(329, 0);
            this.lblForce.Name = "lblForce";
            this.lblForce.Size = new System.Drawing.Size(37, 29);
            this.lblForce.TabIndex = 1;
            this.lblForce.Tag = "Label_Spirit_Force";
            this.lblForce.Text = "Force:";
            this.lblForce.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // nudForce
            // 
            this.nudForce.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudForce.Enabled = false;
            this.nudForce.Location = new System.Drawing.Point(372, 3);
            this.nudForce.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.nudForce.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudForce.Name = "nudForce";
            this.nudForce.Size = new System.Drawing.Size(40, 20);
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
            this.chkBound.AutoSize = true;
            this.chkBound.Checked = true;
            this.chkBound.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBound.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkBound.Enabled = false;
            this.chkBound.Location = new System.Drawing.Point(552, 3);
            this.chkBound.Name = "chkBound";
            this.chkBound.Size = new System.Drawing.Size(57, 23);
            this.chkBound.TabIndex = 5;
            this.chkBound.Tag = "Checkbox_Spirit_Bound";
            this.chkBound.Text = "Bound";
            this.chkBound.UseVisualStyleBackColor = true;
            this.chkBound.CheckedChanged += new System.EventHandler(this.chkBound_CheckedChanged);
            // 
            // cboSpiritName
            // 
            this.cboSpiritName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboSpiritName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSpiritName.FormattingEnabled = true;
            this.cboSpiritName.Location = new System.Drawing.Point(3, 3);
            this.cboSpiritName.Name = "cboSpiritName";
            this.cboSpiritName.Size = new System.Drawing.Size(148, 21);
            this.cboSpiritName.TabIndex = 7;
            this.cboSpiritName.TooltipText = "";
            this.cboSpiritName.SelectedIndexChanged += new System.EventHandler(this.cboSpiritName_SelectedIndexChanged);
            // 
            // imgLink
            // 
            this.imgLink.Image = global::Chummer.Properties.Resources.link;
            this.imgLink.Location = new System.Drawing.Point(686, 3);
            this.imgLink.Name = "imgLink";
            this.imgLink.Size = new System.Drawing.Size(16, 16);
            this.imgLink.TabIndex = 8;
            this.imgLink.TabStop = false;
            this.imgLink.Click += new System.EventHandler(this.imgLink_Click);
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
            // tsCreateCharacter
            // 
            this.tsCreateCharacter.Image = global::Chummer.Properties.Resources.user_add;
            this.tsCreateCharacter.Name = "tsCreateCharacter";
            this.tsCreateCharacter.Size = new System.Drawing.Size(171, 22);
            this.tsCreateCharacter.Tag = "MenuItem_CreateCritter";
            this.tsCreateCharacter.Text = "Create Critter";
            this.tsCreateCharacter.Click += new System.EventHandler(this.tsCreateCharacter_Click);
            // 
            // imgNotes
            // 
            this.imgNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.imgNotes.Location = new System.Drawing.Point(708, 3);
            this.imgNotes.Name = "imgNotes";
            this.imgNotes.Size = new System.Drawing.Size(16, 16);
            this.imgNotes.TabIndex = 11;
            this.imgNotes.TabStop = false;
            this.imgNotes.Click += new System.EventHandler(this.imgNotes_Click);
            // 
            // txtCritterName
            // 
            this.txtCritterName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCritterName.Location = new System.Drawing.Point(157, 3);
            this.txtCritterName.Name = "txtCritterName";
            this.txtCritterName.Size = new System.Drawing.Size(166, 20);
            this.txtCritterName.TabIndex = 12;
            this.txtCritterName.TextChanged += new System.EventHandler(this.txtCritterName_TextChanged);
            // 
            // chkFettered
            // 
            this.chkFettered.AutoSize = true;
            this.chkFettered.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkFettered.Location = new System.Drawing.Point(615, 3);
            this.chkFettered.Name = "chkFettered";
            this.chkFettered.Size = new System.Drawing.Size(65, 23);
            this.chkFettered.TabIndex = 13;
            this.chkFettered.Tag = "Checkbox_Spirit_Fettered";
            this.chkFettered.Text = "Fettered";
            this.chkFettered.UseVisualStyleBackColor = true;
            this.chkFettered.CheckedChanged += new System.EventHandler(this.chkFettered_CheckedChanged);
            // 
            // bufferedTableLayoutPanel1
            // 
            this.bufferedTableLayoutPanel1.AutoSize = true;
            this.bufferedTableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bufferedTableLayoutPanel1.ColumnCount = 12;
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bufferedTableLayoutPanel1.Controls.Add(this.cmdDelete, 10, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.imgNotes, 9, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.cboSpiritName, 0, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.txtCritterName, 1, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.chkFettered, 7, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.imgLink, 8, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblForce, 2, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.chkBound, 6, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.nudForce, 3, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.nudServices, 5, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblServices, 4, 0);
            this.bufferedTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedTableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.bufferedTableLayoutPanel1.Name = "bufferedTableLayoutPanel1";
            this.bufferedTableLayoutPanel1.RowCount = 1;
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bufferedTableLayoutPanel1.Size = new System.Drawing.Size(808, 29);
            this.bufferedTableLayoutPanel1.TabIndex = 14;
            // 
            // SpiritControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.bufferedTableLayoutPanel1);
            this.Name = "SpiritControl";
            this.Size = new System.Drawing.Size(808, 29);
            this.Load += new System.EventHandler(this.SpiritControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudServices)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudForce)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgLink)).EndInit();
            this.cmsSpirit.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.imgNotes)).EndInit();
            this.bufferedTableLayoutPanel1.ResumeLayout(false);
            this.bufferedTableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdDelete;
        private System.Windows.Forms.Label lblServices;
        private System.Windows.Forms.NumericUpDown nudServices;
        private System.Windows.Forms.Label lblForce;
        private System.Windows.Forms.NumericUpDown nudForce;
        private System.Windows.Forms.CheckBox chkBound;
        private ElasticComboBox cboSpiritName;
        private System.Windows.Forms.PictureBox imgLink;
        private System.Windows.Forms.ContextMenuStrip cmsSpirit;
        private System.Windows.Forms.ToolStripMenuItem tsContactOpen;
        private System.Windows.Forms.ToolStripMenuItem tsRemoveCharacter;
        private System.Windows.Forms.ToolStripMenuItem tsAttachCharacter;
        private System.Windows.Forms.PictureBox imgNotes;
        private System.Windows.Forms.ToolStripMenuItem tsCreateCharacter;
        private System.Windows.Forms.TextBox txtCritterName;
        private System.Windows.Forms.CheckBox chkFettered;
        private BufferedTableLayoutPanel bufferedTableLayoutPanel1;
    }
}
