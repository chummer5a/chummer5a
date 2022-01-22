namespace Chummer.Controls.Skills
{
    partial class SkillControlReadOnly
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
                _fntNormal?.Dispose();
                _fntItalic?.Dispose();
                _fntNormalName?.Dispose();
                _fntItalicName?.Dispose();
                UnbindSkillControl();
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
            this.lblName = new Chummer.LabelWithToolTip();
            this.pnlAttributes = new System.Windows.Forms.Panel();
            this.cmdAttribute = new System.Windows.Forms.Button();
            this.cboSelectAttribute = new Chummer.ElasticComboBox();
            this.lblModifiedRating = new Chummer.LabelWithToolTip();
            this.lblSpec = new System.Windows.Forms.Label();
            this.lblRating = new System.Windows.Forms.Label();
            this.cmdNotes = new Chummer.ButtonWithToolTip(this.components);
            this.chkKarma = new Chummer.ColorableCheckBox(this.components);
            this.tlpMain.SuspendLayout();
            this.pnlAttributes.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 7;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.lblName, 0, 0);
            this.tlpMain.Controls.Add(this.pnlAttributes, 1, 0);
            this.tlpMain.Controls.Add(this.lblModifiedRating, 3, 0);
            this.tlpMain.Controls.Add(this.lblSpec, 4, 0);
            this.tlpMain.Controls.Add(this.lblRating, 2, 0);
            this.tlpMain.Controls.Add(this.cmdNotes, 6, 0);
            this.tlpMain.Controls.Add(this.chkKarma, 5, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 1;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(356, 28);
            this.tlpMain.TabIndex = 29;
            // 
            // lblName
            // 
            this.lblName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(3, 7);
            this.lblName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(41, 13);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "[Name]";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblName.ToolTipText = "";
            this.lblName.Click += new System.EventHandler(this.lblName_Click);
            // 
            // pnlAttributes
            // 
            this.pnlAttributes.AutoSize = true;
            this.pnlAttributes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlAttributes.Controls.Add(this.cmdAttribute);
            this.pnlAttributes.Controls.Add(this.cboSelectAttribute);
            this.pnlAttributes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlAttributes.Location = new System.Drawing.Point(47, 0);
            this.pnlAttributes.Margin = new System.Windows.Forms.Padding(0);
            this.pnlAttributes.MinimumSize = new System.Drawing.Size(40, 0);
            this.pnlAttributes.Name = "pnlAttributes";
            this.pnlAttributes.Size = new System.Drawing.Size(40, 28);
            this.pnlAttributes.TabIndex = 33;
            // 
            // cmdAttribute
            // 
            this.cmdAttribute.AutoSize = true;
            this.cmdAttribute.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAttribute.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdAttribute.FlatAppearance.BorderSize = 0;
            this.cmdAttribute.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdAttribute.Location = new System.Drawing.Point(0, 0);
            this.cmdAttribute.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdAttribute.Name = "cmdAttribute";
            this.cmdAttribute.Size = new System.Drawing.Size(40, 28);
            this.cmdAttribute.TabIndex = 24;
            this.cmdAttribute.Text = "ATR";
            this.cmdAttribute.UseVisualStyleBackColor = true;
            this.cmdAttribute.Click += new System.EventHandler(this.cmdAttribute_Click);
            // 
            // cboSelectAttribute
            // 
            this.cboSelectAttribute.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboSelectAttribute.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSelectAttribute.FormattingEnabled = true;
            this.cboSelectAttribute.Location = new System.Drawing.Point(0, 0);
            this.cboSelectAttribute.Name = "cboSelectAttribute";
            this.cboSelectAttribute.Size = new System.Drawing.Size(40, 21);
            this.cboSelectAttribute.TabIndex = 25;
            this.cboSelectAttribute.TooltipText = "";
            this.cboSelectAttribute.DropDownClosed += new System.EventHandler(this.cboSelectAttribute_Closed);
            // 
            // lblModifiedRating
            // 
            this.lblModifiedRating.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblModifiedRating.AutoSize = true;
            this.lblModifiedRating.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblModifiedRating.Location = new System.Drawing.Point(121, 7);
            this.lblModifiedRating.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblModifiedRating.MinimumSize = new System.Drawing.Size(50, 0);
            this.lblModifiedRating.Name = "lblModifiedRating";
            this.lblModifiedRating.Size = new System.Drawing.Size(50, 13);
            this.lblModifiedRating.TabIndex = 16;
            this.lblModifiedRating.Text = "00 (00)";
            this.lblModifiedRating.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblModifiedRating.ToolTipText = "";
            // 
            // lblSpec
            // 
            this.lblSpec.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSpec.AutoSize = true;
            this.lblSpec.Location = new System.Drawing.Point(177, 7);
            this.lblSpec.Margin = new System.Windows.Forms.Padding(3, 6, 6, 6);
            this.lblSpec.Name = "lblSpec";
            this.lblSpec.Size = new System.Drawing.Size(83, 13);
            this.lblSpec.TabIndex = 34;
            this.lblSpec.Text = "[Specializations]";
            this.lblSpec.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblRating
            // 
            this.lblRating.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblRating.AutoSize = true;
            this.lblRating.Location = new System.Drawing.Point(90, 7);
            this.lblRating.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRating.Name = "lblRating";
            this.lblRating.Size = new System.Drawing.Size(25, 13);
            this.lblRating.TabIndex = 35;
            this.lblRating.Text = "[00]";
            this.lblRating.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
            this.cmdNotes.Location = new System.Drawing.Point(331, 3);
            this.cmdNotes.Name = "cmdNotes";
            this.cmdNotes.Size = new System.Drawing.Size(22, 22);
            this.cmdNotes.TabIndex = 36;
            this.cmdNotes.ToolTipText = "";
            this.cmdNotes.UseVisualStyleBackColor = true;
            // 
            // chkKarma
            // 
            this.chkKarma.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkKarma.AutoSize = true;
            this.chkKarma.DefaultColorScheme = true;
            this.chkKarma.Enabled = false;
            this.chkKarma.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.chkKarma.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ControlDark;
            this.chkKarma.Location = new System.Drawing.Point(269, 7);
            this.chkKarma.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.chkKarma.Name = "chkKarma";
            this.chkKarma.Size = new System.Drawing.Size(56, 17);
            this.chkKarma.TabIndex = 37;
            this.chkKarma.Text = "Karma";
            this.chkKarma.UseVisualStyleBackColor = true;
            // 
            // SkillControlReadOnly
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tlpMain);
            this.Name = "SkillControlReadOnly";
            this.Size = new System.Drawing.Size(356, 28);
            this.Load += new System.EventHandler(this.SkillControlReadOnly_Load);
            this.MouseLeave += new System.EventHandler(this.OnMouseLeave);
            this.DpiChangedAfterParent += new System.EventHandler(this.SkillControlReadOnly_DpiChangedAfterParent);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.pnlAttributes.ResumeLayout(false);
            this.pnlAttributes.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BufferedTableLayoutPanel tlpMain;
        private LabelWithToolTip lblName;
        private System.Windows.Forms.Panel pnlAttributes;
        private System.Windows.Forms.Button cmdAttribute;
        private System.Windows.Forms.Label lblSpec;
        private LabelWithToolTip lblModifiedRating;
        private System.Windows.Forms.Label lblRating;
        private ButtonWithToolTip cmdNotes;
        private ColorableCheckBox chkKarma;
        private ElasticComboBox cboSelectAttribute;
    }
}
