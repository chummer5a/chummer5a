namespace Chummer
{
    partial class SpiritControlReadOnly
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
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblSpiritName = new System.Windows.Forms.Label();
            this.lblForce = new System.Windows.Forms.Label();
            this.chkFettered = new Chummer.ColorableCheckBox(this.components);
            this.chkBound = new Chummer.ColorableCheckBox(this.components);
            this.lblServicesLabel = new System.Windows.Forms.Label();
            this.lblForceLabel = new System.Windows.Forms.Label();
            this.txtCritterName = new System.Windows.Forms.TextBox();
            this.cmdNotes = new Chummer.ButtonWithToolTip(this.components);
            this.cmdLink = new Chummer.ButtonWithToolTip(this.components);
            this.lblServices = new System.Windows.Forms.Label();
            this.tlpMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 10;
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
            this.tlpMain.Controls.Add(this.lblSpiritName, 0, 0);
            this.tlpMain.Controls.Add(this.lblForce, 3, 0);
            this.tlpMain.Controls.Add(this.chkFettered, 7, 0);
            this.tlpMain.Controls.Add(this.chkBound, 6, 0);
            this.tlpMain.Controls.Add(this.lblServicesLabel, 4, 0);
            this.tlpMain.Controls.Add(this.lblForceLabel, 2, 0);
            this.tlpMain.Controls.Add(this.txtCritterName, 1, 0);
            this.tlpMain.Controls.Add(this.cmdNotes, 9, 0);
            this.tlpMain.Controls.Add(this.cmdLink, 8, 0);
            this.tlpMain.Controls.Add(this.lblServices, 5, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 1;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(865, 29);
            this.tlpMain.TabIndex = 15;
            // 
            // lblSpiritName
            // 
            this.lblSpiritName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSpiritName.AutoSize = true;
            this.lblSpiritName.Location = new System.Drawing.Point(3, 8);
            this.lblSpiritName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpiritName.Name = "lblSpiritName";
            this.lblSpiritName.Size = new System.Drawing.Size(63, 13);
            this.lblSpiritName.TabIndex = 18;
            this.lblSpiritName.Text = "[Spirit Type]";
            // 
            // lblForce
            // 
            this.lblForce.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblForce.AutoSize = true;
            this.lblForce.Location = new System.Drawing.Point(484, 8);
            this.lblForce.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblForce.Name = "lblForce";
            this.lblForce.Size = new System.Drawing.Size(40, 13);
            this.lblForce.TabIndex = 17;
            this.lblForce.Text = "[Force]";
            // 
            // chkFettered
            // 
            this.chkFettered.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkFettered.AutoSize = true;
            this.chkFettered.DefaultColorScheme = true;
            this.chkFettered.Enabled = false;
            this.chkFettered.Location = new System.Drawing.Point(741, 6);
            this.chkFettered.Name = "chkFettered";
            this.chkFettered.Size = new System.Drawing.Size(65, 17);
            this.chkFettered.TabIndex = 13;
            this.chkFettered.Tag = "Checkbox_Spirit_Fettered";
            this.chkFettered.Text = "Fettered";
            this.chkFettered.UseVisualStyleBackColor = true;
            // 
            // chkBound
            // 
            this.chkBound.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkBound.AutoSize = true;
            this.chkBound.Checked = true;
            this.chkBound.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBound.DefaultColorScheme = true;
            this.chkBound.Enabled = false;
            this.chkBound.Location = new System.Drawing.Point(678, 6);
            this.chkBound.Name = "chkBound";
            this.chkBound.Size = new System.Drawing.Size(57, 17);
            this.chkBound.TabIndex = 5;
            this.chkBound.Tag = "Checkbox_Spirit_Bound";
            this.chkBound.Text = "Bound";
            this.chkBound.UseVisualStyleBackColor = true;
            // 
            // lblServicesLabel
            // 
            this.lblServicesLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblServicesLabel.AutoSize = true;
            this.lblServicesLabel.Location = new System.Drawing.Point(530, 8);
            this.lblServicesLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblServicesLabel.Name = "lblServicesLabel";
            this.lblServicesLabel.Size = new System.Drawing.Size(82, 13);
            this.lblServicesLabel.TabIndex = 3;
            this.lblServicesLabel.Tag = "Label_Spirit_ServicesOwed";
            this.lblServicesLabel.Text = "Services Owed:";
            this.lblServicesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblForceLabel
            // 
            this.lblForceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblForceLabel.AutoSize = true;
            this.lblForceLabel.Location = new System.Drawing.Point(441, 8);
            this.lblForceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblForceLabel.Name = "lblForceLabel";
            this.lblForceLabel.Size = new System.Drawing.Size(37, 13);
            this.lblForceLabel.TabIndex = 1;
            this.lblForceLabel.Tag = "Label_Spirit_Force";
            this.lblForceLabel.Text = "Force:";
            this.lblForceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtCritterName
            // 
            this.txtCritterName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCritterName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCritterName.Location = new System.Drawing.Point(222, 6);
            this.txtCritterName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.txtCritterName.Multiline = true;
            this.txtCritterName.Name = "txtCritterName";
            this.txtCritterName.ReadOnly = true;
            this.txtCritterName.Size = new System.Drawing.Size(213, 20);
            this.txtCritterName.TabIndex = 12;
            this.txtCritterName.Text = "[Name]";
            // 
            // cmdNotes
            // 
            this.cmdNotes.Anchor = System.Windows.Forms.AnchorStyles.Left;
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
            this.cmdNotes.Location = new System.Drawing.Point(840, 3);
            this.cmdNotes.Name = "cmdNotes";
            this.cmdNotes.Size = new System.Drawing.Size(22, 22);
            this.cmdNotes.TabIndex = 14;
            this.cmdNotes.ToolTipText = "";
            this.cmdNotes.UseVisualStyleBackColor = true;
            // 
            // cmdLink
            // 
            this.cmdLink.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmdLink.AutoSize = true;
            this.cmdLink.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdLink.FlatAppearance.BorderSize = 0;
            this.cmdLink.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdLink.Image = global::Chummer.Properties.Resources.link;
            this.cmdLink.ImageDpi120 = null;
            this.cmdLink.ImageDpi144 = null;
            this.cmdLink.ImageDpi192 = global::Chummer.Properties.Resources.link1;
            this.cmdLink.ImageDpi288 = null;
            this.cmdLink.ImageDpi384 = null;
            this.cmdLink.ImageDpi96 = global::Chummer.Properties.Resources.link;
            this.cmdLink.Location = new System.Drawing.Point(812, 3);
            this.cmdLink.Name = "cmdLink";
            this.cmdLink.Size = new System.Drawing.Size(22, 22);
            this.cmdLink.TabIndex = 15;
            this.cmdLink.ToolTipText = "";
            this.cmdLink.UseVisualStyleBackColor = true;
            this.cmdLink.Click += new System.EventHandler(this.cmdLink_Click);
            // 
            // lblServices
            // 
            this.lblServices.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblServices.AutoSize = true;
            this.lblServices.Location = new System.Drawing.Point(618, 8);
            this.lblServices.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblServices.Name = "lblServices";
            this.lblServices.Size = new System.Drawing.Size(54, 13);
            this.lblServices.TabIndex = 16;
            this.lblServices.Text = "[Services]";
            // 
            // SpiritControlReadOnly
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tlpMain);
            this.Name = "SpiritControlReadOnly";
            this.Size = new System.Drawing.Size(865, 29);
            this.Load += new System.EventHandler(this.SpiritControlReadOnly_Load);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BufferedTableLayoutPanel tlpMain;
        private ColorableCheckBox chkFettered;
        private ColorableCheckBox chkBound;
        private System.Windows.Forms.Label lblServicesLabel;
        private System.Windows.Forms.Label lblForceLabel;
        private System.Windows.Forms.TextBox txtCritterName;
        private ButtonWithToolTip cmdNotes;
        private ButtonWithToolTip cmdLink;
        private System.Windows.Forms.Label lblServices;
        private System.Windows.Forms.Label lblForce;
        private System.Windows.Forms.Label lblSpiritName;
    }
}
