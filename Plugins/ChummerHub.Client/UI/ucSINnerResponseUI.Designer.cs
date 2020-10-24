namespace ChummerHub.Client.UI
{
    partial class ucSINnerResponseUI
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
            this.tlpSINnerResponseUI = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.tbSINnerResponseErrorText = new System.Windows.Forms.TextBox();
            this.gbSINnerResponseMyException = new System.Windows.Forms.GroupBox();
            this.tbSINnerResponseMyExpection = new System.Windows.Forms.TextBox();
            this.bOk = new System.Windows.Forms.Button();
            this.lInstallationId = new System.Windows.Forms.Label();
            this.tbInstallationId = new System.Windows.Forms.TextBox();
            this.tlpSINnerResponseUI.SuspendLayout();
            this.gbSINnerResponseMyException.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpSINnerResponseUI
            // 
            this.tlpSINnerResponseUI.AutoSize = true;
            this.tlpSINnerResponseUI.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpSINnerResponseUI.ColumnCount = 2;
            this.tlpSINnerResponseUI.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpSINnerResponseUI.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSINnerResponseUI.Controls.Add(this.label1, 0, 0);
            this.tlpSINnerResponseUI.Controls.Add(this.tbSINnerResponseErrorText, 1, 0);
            this.tlpSINnerResponseUI.Controls.Add(this.gbSINnerResponseMyException, 0, 1);
            this.tlpSINnerResponseUI.Controls.Add(this.bOk, 1, 2);
            this.tlpSINnerResponseUI.Controls.Add(this.lInstallationId, 0, 2);
            this.tlpSINnerResponseUI.Controls.Add(this.tbInstallationId, 1, 2);
            this.tlpSINnerResponseUI.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSINnerResponseUI.Location = new System.Drawing.Point(0, 0);
            this.tlpSINnerResponseUI.Name = "tlpSINnerResponseUI";
            this.tlpSINnerResponseUI.RowCount = 4;
            this.tlpSINnerResponseUI.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSINnerResponseUI.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSINnerResponseUI.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSINnerResponseUI.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSINnerResponseUI.Size = new System.Drawing.Size(430, 200);
            this.tlpSINnerResponseUI.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 36);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Response:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbSINnerResponseErrorText
            // 
            this.tbSINnerResponseErrorText.BackColor = System.Drawing.SystemColors.Control;
            this.tbSINnerResponseErrorText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbSINnerResponseErrorText.Location = new System.Drawing.Point(78, 3);
            this.tbSINnerResponseErrorText.Multiline = true;
            this.tbSINnerResponseErrorText.Name = "tbSINnerResponseErrorText";
            this.tbSINnerResponseErrorText.Size = new System.Drawing.Size(349, 80);
            this.tbSINnerResponseErrorText.TabIndex = 2;
            this.tbSINnerResponseErrorText.VisibleChanged += new System.EventHandler(this.TbSINnerResponseErrorText_VisibleChanged);
            // 
            // gbSINnerResponseMyException
            // 
            this.gbSINnerResponseMyException.AutoSize = true;
            this.gbSINnerResponseMyException.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpSINnerResponseUI.SetColumnSpan(this.gbSINnerResponseMyException, 2);
            this.gbSINnerResponseMyException.Controls.Add(this.tbSINnerResponseMyExpection);
            this.gbSINnerResponseMyException.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbSINnerResponseMyException.Location = new System.Drawing.Point(3, 89);
            this.gbSINnerResponseMyException.Name = "gbSINnerResponseMyException";
            this.gbSINnerResponseMyException.Size = new System.Drawing.Size(424, 53);
            this.gbSINnerResponseMyException.TabIndex = 3;
            this.gbSINnerResponseMyException.TabStop = false;
            this.gbSINnerResponseMyException.Text = "Details";
            // 
            // tbSINnerResponseMyExpection
            // 
            this.tbSINnerResponseMyExpection.BackColor = System.Drawing.SystemColors.Control;
            this.tbSINnerResponseMyExpection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbSINnerResponseMyExpection.Location = new System.Drawing.Point(3, 16);
            this.tbSINnerResponseMyExpection.Multiline = true;
            this.tbSINnerResponseMyExpection.Name = "tbSINnerResponseMyExpection";
            this.tbSINnerResponseMyExpection.Size = new System.Drawing.Size(418, 34);
            this.tbSINnerResponseMyExpection.TabIndex = 0;
            // 
            // bOk
            // 
            this.bOk.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.bOk.AutoSize = true;
            this.bOk.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpSINnerResponseUI.SetColumnSpan(this.bOk, 2);
            this.bOk.Location = new System.Drawing.Point(184, 174);
            this.bOk.Name = "bOk";
            this.bOk.Padding = new System.Windows.Forms.Padding(15, 0, 15, 0);
            this.bOk.Size = new System.Drawing.Size(62, 23);
            this.bOk.TabIndex = 0;
            this.bOk.Text = "OK";
            this.bOk.UseVisualStyleBackColor = true;
            this.bOk.Click += new System.EventHandler(this.BOk_Click);
            // 
            // lInstallationId
            // 
            this.lInstallationId.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lInstallationId.AutoSize = true;
            this.lInstallationId.Location = new System.Drawing.Point(3, 151);
            this.lInstallationId.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lInstallationId.Name = "lInstallationId";
            this.lInstallationId.Size = new System.Drawing.Size(69, 13);
            this.lInstallationId.TabIndex = 4;
            this.lInstallationId.Text = "Installation Id";
            // 
            // tbInstallationId
            // 
            this.tbInstallationId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbInstallationId.Location = new System.Drawing.Point(78, 148);
            this.tbInstallationId.Name = "tbInstallationId";
            this.tbInstallationId.ReadOnly = true;
            this.tbInstallationId.Size = new System.Drawing.Size(349, 20);
            this.tbInstallationId.TabIndex = 5;
            // 
            // ucSINnerResponseUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tlpSINnerResponseUI);
            this.MinimumSize = new System.Drawing.Size(200, 200);
            this.Name = "ucSINnerResponseUI";
            this.Size = new System.Drawing.Size(430, 200);
            this.tlpSINnerResponseUI.ResumeLayout(false);
            this.tlpSINnerResponseUI.PerformLayout();
            this.gbSINnerResponseMyException.ResumeLayout(false);
            this.gbSINnerResponseMyException.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpSINnerResponseUI;
        private System.Windows.Forms.Button bOk;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbSINnerResponseErrorText;
        private System.Windows.Forms.GroupBox gbSINnerResponseMyException;
        private System.Windows.Forms.TextBox tbSINnerResponseMyExpection;
        private System.Windows.Forms.Label lInstallationId;
        private System.Windows.Forms.TextBox tbInstallationId;
    }
}
