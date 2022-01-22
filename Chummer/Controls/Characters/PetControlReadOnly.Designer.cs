namespace Chummer
{
    partial class PetControlReadOnly
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
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.txtMetatype = new System.Windows.Forms.TextBox();
            this.cmdNotes = new Chummer.ButtonWithToolTip(this.components);
            this.lblName = new System.Windows.Forms.Label();
            this.lblMetatypeLabel = new System.Windows.Forms.Label();
            this.cmdLink = new Chummer.ButtonWithToolTip(this.components);
            this.txtName = new System.Windows.Forms.TextBox();
            this.tlpMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 6;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Controls.Add(this.txtMetatype, 3, 0);
            this.tlpMain.Controls.Add(this.cmdNotes, 5, 0);
            this.tlpMain.Controls.Add(this.lblName, 0, 0);
            this.tlpMain.Controls.Add(this.lblMetatypeLabel, 2, 0);
            this.tlpMain.Controls.Add(this.cmdLink, 4, 0);
            this.tlpMain.Controls.Add(this.txtName, 1, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 1;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tlpMain.Size = new System.Drawing.Size(768, 32);
            this.tlpMain.TabIndex = 26;
            // 
            // txtMetatype
            // 
            this.txtMetatype.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMetatype.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMetatype.Location = new System.Drawing.Point(411, 9);
            this.txtMetatype.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
            this.txtMetatype.Multiline = true;
            this.txtMetatype.Name = "txtMetatype";
            this.txtMetatype.ReadOnly = true;
            this.txtMetatype.Size = new System.Drawing.Size(298, 20);
            this.txtMetatype.TabIndex = 29;
            this.txtMetatype.Text = "[Metatype]";
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
            this.cmdNotes.Location = new System.Drawing.Point(743, 5);
            this.cmdNotes.Name = "cmdNotes";
            this.cmdNotes.Size = new System.Drawing.Size(22, 22);
            this.cmdNotes.TabIndex = 26;
            this.cmdNotes.ToolTipText = "";
            this.cmdNotes.UseVisualStyleBackColor = true;
            // 
            // lblName
            // 
            this.lblName.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(3, 9);
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
            this.lblMetatypeLabel.Location = new System.Drawing.Point(351, 9);
            this.lblMetatypeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetatypeLabel.Name = "lblMetatypeLabel";
            this.lblMetatypeLabel.Size = new System.Drawing.Size(54, 13);
            this.lblMetatypeLabel.TabIndex = 23;
            this.lblMetatypeLabel.Tag = "Label_Metatype";
            this.lblMetatypeLabel.Text = "Metatype:";
            this.lblMetatypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
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
            this.cmdLink.Location = new System.Drawing.Point(715, 5);
            this.cmdLink.Name = "cmdLink";
            this.cmdLink.Size = new System.Drawing.Size(22, 22);
            this.cmdLink.TabIndex = 25;
            this.cmdLink.ToolTipText = "";
            this.cmdLink.UseVisualStyleBackColor = true;
            this.cmdLink.Click += new System.EventHandler(this.cmdLink_Click);
            // 
            // txtName
            // 
            this.txtName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtName.Location = new System.Drawing.Point(47, 9);
            this.txtName.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
            this.txtName.Multiline = true;
            this.txtName.Name = "txtName";
            this.txtName.ReadOnly = true;
            this.txtName.Size = new System.Drawing.Size(298, 20);
            this.txtName.TabIndex = 28;
            this.txtName.Text = "[Name]";
            // 
            // PetControlReadOnly
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.Name = "PetControlReadOnly";
            this.Size = new System.Drawing.Size(768, 32);
            this.Load += new System.EventHandler(this.PetControlReadOnly_Load);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BufferedTableLayoutPanel tlpMain;
        private ButtonWithToolTip cmdNotes;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblMetatypeLabel;
        private ButtonWithToolTip cmdLink;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.TextBox txtMetatype;
    }
}
