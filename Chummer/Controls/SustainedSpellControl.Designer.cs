namespace Chummer
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
            this.lblSustainedSpell = new System.Windows.Forms.Label();
            this.lblNetHits = new System.Windows.Forms.Label();
            this.nudNetHits = new Chummer.NumericUpDownEx();
            this.lblForce = new System.Windows.Forms.Label();
            this.nudForce = new Chummer.NumericUpDownEx();
            this.chkSelfSustained = new Chummer.ColorableCheckBox(this.components);
            this.cmdDelete = new System.Windows.Forms.Button();
            this.lblSelfSustained = new System.Windows.Forms.Label();
            this.bufferedTableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudNetHits)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudForce)).BeginInit();
            this.SuspendLayout();
            // 
            // bufferedTableLayoutPanel1
            // 
            this.bufferedTableLayoutPanel1.AutoSize = true;
            this.bufferedTableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bufferedTableLayoutPanel1.ColumnCount = 8;
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblSustainedSpell, 0, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblNetHits, 0, 1);
            this.bufferedTableLayoutPanel1.Controls.Add(this.nudNetHits, 2, 1);
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblForce, 3, 1);
            this.bufferedTableLayoutPanel1.Controls.Add(this.nudForce, 4, 1);
            this.bufferedTableLayoutPanel1.Controls.Add(this.chkSelfSustained, 6, 1);
            this.bufferedTableLayoutPanel1.Controls.Add(this.cmdDelete, 7, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblSelfSustained, 5, 1);
            this.bufferedTableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.bufferedTableLayoutPanel1.Name = "bufferedTableLayoutPanel1";
            this.bufferedTableLayoutPanel1.RowCount = 2;
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel1.Size = new System.Drawing.Size(265, 39);
            this.bufferedTableLayoutPanel1.TabIndex = 1;
            // 
            // lblSustainedSpell
            // 
            this.lblSustainedSpell.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSustainedSpell.AutoSize = true;
            this.bufferedTableLayoutPanel1.SetColumnSpan(this.lblSustainedSpell, 6);
            this.lblSustainedSpell.Location = new System.Drawing.Point(3, 0);
            this.lblSustainedSpell.Name = "lblSustainedSpell";
            this.lblSustainedSpell.Size = new System.Drawing.Size(61, 13);
            this.lblSustainedSpell.TabIndex = 11;
            this.lblSustainedSpell.Text = "Spell Name";
            this.lblSustainedSpell.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblNetHits
            // 
            this.lblNetHits.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblNetHits.AutoSize = true;
            this.lblNetHits.Location = new System.Drawing.Point(1, 19);
            this.lblNetHits.Margin = new System.Windows.Forms.Padding(1, 0, 0, 0);
            this.lblNetHits.Name = "lblNetHits";
            this.lblNetHits.Size = new System.Drawing.Size(45, 13);
            this.lblNetHits.TabIndex = 13;
            this.lblNetHits.Tag = "Label_NetHits";
            this.lblNetHits.Text = "NetHits:";
            this.lblNetHits.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudNetHits
            // 
            this.nudNetHits.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudNetHits.Location = new System.Drawing.Point(49, 16);
            this.nudNetHits.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.nudNetHits.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudNetHits.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudNetHits.Name = "nudNetHits";
            this.nudNetHits.Size = new System.Drawing.Size(32, 20);
            this.nudNetHits.TabIndex = 12;
            this.nudNetHits.ValueChanged += new System.EventHandler(this.nudNetHits_ValueChanged);
            // 
            // lblForce
            // 
            this.lblForce.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblForce.AutoSize = true;
            this.lblForce.Location = new System.Drawing.Point(85, 19);
            this.lblForce.Margin = new System.Windows.Forms.Padding(1, 0, 0, 0);
            this.lblForce.Name = "lblForce";
            this.lblForce.Size = new System.Drawing.Size(37, 13);
            this.lblForce.TabIndex = 10;
            this.lblForce.Tag = "Label_SustainedForce";
            this.lblForce.Text = "Force:";
            this.lblForce.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudForce
            // 
            this.nudForce.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudForce.Location = new System.Drawing.Point(125, 16);
            this.nudForce.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.nudForce.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudForce.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudForce.Name = "nudForce";
            this.nudForce.Size = new System.Drawing.Size(32, 20);
            this.nudForce.TabIndex = 9;
            this.nudForce.ValueChanged += new System.EventHandler(this.nudForce_ValueChanged);
            // 
            // chkSelfSustained
            // 
            this.chkSelfSustained.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkSelfSustained.AutoSize = true;
            this.chkSelfSustained.DefaultColorScheme = true;
            this.chkSelfSustained.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.chkSelfSustained.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ControlDark;
            this.chkSelfSustained.Location = new System.Drawing.Point(221, 19);
            this.chkSelfSustained.Name = "chkSelfSustained";
            this.chkSelfSustained.Size = new System.Drawing.Size(15, 14);
            this.chkSelfSustained.TabIndex = 8;
            this.chkSelfSustained.Tag = "";
            this.chkSelfSustained.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkSelfSustained.UseVisualStyleBackColor = true;
            this.chkSelfSustained.Visible = false;
            this.chkSelfSustained.CheckedChanged += new System.EventHandler(this.chkSelf_CheckedChanged);
            // 
            // cmdDelete
            // 
            this.cmdDelete.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmdDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDelete.BackgroundImage = global::Chummer.Properties.Resources.delete;
            this.cmdDelete.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.cmdDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdDelete.Location = new System.Drawing.Point(242, 9);
            this.cmdDelete.Name = "cmdDelete";
            this.bufferedTableLayoutPanel1.SetRowSpan(this.cmdDelete, 2);
            this.cmdDelete.Size = new System.Drawing.Size(20, 20);
            this.cmdDelete.TabIndex = 7;
            this.cmdDelete.Tag = "";
            this.cmdDelete.UseVisualStyleBackColor = true;
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // lblSelfSustained
            // 
            this.lblSelfSustained.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSelfSustained.AutoSize = true;
            this.lblSelfSustained.Location = new System.Drawing.Point(163, 13);
            this.lblSelfSustained.Name = "lblSelfSustained";
            this.lblSelfSustained.Size = new System.Drawing.Size(52, 26);
            this.lblSelfSustained.TabIndex = 14;
            this.lblSelfSustained.Tag = "Label_SustainedSelf";
            this.lblSelfSustained.Text = "Self \r\nsustained";
            this.lblSelfSustained.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblSelfSustained.Visible = false;
            // 
            // SustainedSpellControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.bufferedTableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximumSize = new System.Drawing.Size(1000, 45);
            this.MinimumSize = new System.Drawing.Size(241, 2);
            this.Name = "SustainedSpellControl";
            this.Size = new System.Drawing.Size(268, 42);
            this.Load += new System.EventHandler(this.SustainedSpellControl_Load);
            this.bufferedTableLayoutPanel1.ResumeLayout(false);
            this.bufferedTableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudNetHits)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudForce)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BufferedTableLayoutPanel bufferedTableLayoutPanel1;
        private ColorableCheckBox chkSelfSustained;
        private NumericUpDownEx nudForce;
        private System.Windows.Forms.Button cmdDelete;
        private System.Windows.Forms.Label lblForce;
        private System.Windows.Forms.Label lblSustainedSpell;
        private NumericUpDownEx nudNetHits;
        private System.Windows.Forms.Label lblNetHits;
        private System.Windows.Forms.Label lblSelfSustained;
    }
}
