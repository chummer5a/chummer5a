namespace Chummer.Controls
{
    partial class SustainedSpellControl
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
            this.bufferedTableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdDelete = new System.Windows.Forms.Button();
            this.chkSelfSustained = new Chummer.ColorableCheckBox(this.components);
            this.numericUpDownEx1 = new Chummer.NumericUpDownEx();
            this.lblForce = new System.Windows.Forms.Label();
            this.lblSustainedSpell = new System.Windows.Forms.Label();
            this.bufferedTableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEx1)).BeginInit();
            this.SuspendLayout();
            // 
            // bufferedTableLayoutPanel1
            // 
            this.bufferedTableLayoutPanel1.ColumnCount = 5;
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.Controls.Add(this.cmdDelete, 4, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.chkSelfSustained, 3, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.numericUpDownEx1, 2, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblForce, 1, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblSustainedSpell, 0, 0);
            this.bufferedTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedTableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.bufferedTableLayoutPanel1.Name = "bufferedTableLayoutPanel1";
            this.bufferedTableLayoutPanel1.RowCount = 1;
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel1.Size = new System.Drawing.Size(290, 30);
            this.bufferedTableLayoutPanel1.TabIndex = 1;
            // 
            // cmdDelete
            // 
            this.cmdDelete.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdDelete.AutoSize = true;
            this.cmdDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDelete.Location = new System.Drawing.Point(239, 3);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(48, 23);
            this.cmdDelete.TabIndex = 7;
            this.cmdDelete.Tag = "String_Delete";
            this.cmdDelete.Text = "Delete";
            this.cmdDelete.UseVisualStyleBackColor = true;
            // 
            // chkSelfSustained
            // 
            this.chkSelfSustained.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.chkSelfSustained.AutoSize = true;
            this.chkSelfSustained.DefaultColorScheme = true;
            this.chkSelfSustained.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.chkSelfSustained.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ControlDark;
            this.chkSelfSustained.Location = new System.Drawing.Point(189, 6);
            this.chkSelfSustained.Name = "chkSelfSustained";
            this.chkSelfSustained.Size = new System.Drawing.Size(44, 17);
            this.chkSelfSustained.TabIndex = 8;
            this.chkSelfSustained.Tag = "Label_Sustained_NotSelf";
            this.chkSelfSustained.Text = "Self";
            this.chkSelfSustained.UseVisualStyleBackColor = true;
            // 
            // numericUpDownEx1
            // 
            this.numericUpDownEx1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.numericUpDownEx1.Location = new System.Drawing.Point(151, 5);
            this.numericUpDownEx1.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownEx1.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownEx1.Name = "numericUpDownEx1";
            this.numericUpDownEx1.Size = new System.Drawing.Size(32, 20);
            this.numericUpDownEx1.TabIndex = 9;
            // 
            // lblForce
            // 
            this.lblForce.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblForce.AutoSize = true;
            this.lblForce.Location = new System.Drawing.Point(108, 8);
            this.lblForce.Name = "lblForce";
            this.lblForce.Size = new System.Drawing.Size(37, 13);
            this.lblForce.TabIndex = 10;
            this.lblForce.Tag = "Label_Sustained_Force";
            this.lblForce.Text = "Force:";
            // 
            // lblSustainedSpell
            // 
            this.lblSustainedSpell.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSustainedSpell.AutoSize = true;
            this.lblSustainedSpell.Location = new System.Drawing.Point(3, 2);
            this.lblSustainedSpell.Name = "lblSustainedSpell";
            this.lblSustainedSpell.Size = new System.Drawing.Size(76, 26);
            this.lblSustainedSpell.TabIndex = 11;
            this.lblSustainedSpell.Text = "Example Spell Name";
            // 
            // SustainedSpellControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.bufferedTableLayoutPanel1);
            this.Name = "SustainedSpellControl";
            this.Size = new System.Drawing.Size(290, 30);
            this.bufferedTableLayoutPanel1.ResumeLayout(false);
            this.bufferedTableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEx1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private BufferedTableLayoutPanel bufferedTableLayoutPanel1;
        private ColorableCheckBox chkSelfSustained;
        private NumericUpDownEx numericUpDownEx1;
        private System.Windows.Forms.Button cmdDelete;
        private System.Windows.Forms.Label lblForce;
        private System.Windows.Forms.Label lblSustainedSpell;
    }
}
