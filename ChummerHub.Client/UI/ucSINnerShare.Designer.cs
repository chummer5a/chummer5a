namespace ChummerHub.Client.UI
{
    partial class ucSINnerShare
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
            this.tlpShareMain = new System.Windows.Forms.TableLayoutPanel();
            this.lStatus = new System.Windows.Forms.Label();
            this.tbStatus = new System.Windows.Forms.TextBox();
            this.pgbStatus = new System.Windows.Forms.ProgressBar();
            this.lLink = new System.Windows.Forms.Label();
            this.tbLink = new System.Windows.Forms.TextBox();
            this.bOk = new System.Windows.Forms.Button();
            this.tlpShareMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpShareMain
            // 
            this.tlpShareMain.AutoSize = true;
            this.tlpShareMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpShareMain.ColumnCount = 2;
            this.tlpShareMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpShareMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpShareMain.Controls.Add(this.lStatus, 0, 0);
            this.tlpShareMain.Controls.Add(this.tbStatus, 1, 0);
            this.tlpShareMain.Controls.Add(this.pgbStatus, 0, 1);
            this.tlpShareMain.Controls.Add(this.lLink, 0, 2);
            this.tlpShareMain.Controls.Add(this.tbLink, 1, 2);
            this.tlpShareMain.Controls.Add(this.bOk, 0, 3);
            this.tlpShareMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpShareMain.Location = new System.Drawing.Point(0, 0);
            this.tlpShareMain.Name = "tlpShareMain";
            this.tlpShareMain.RowCount = 4;
            this.tlpShareMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpShareMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpShareMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpShareMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpShareMain.Size = new System.Drawing.Size(641, 270);
            this.tlpShareMain.TabIndex = 0;
            // 
            // lStatus
            // 
            this.lStatus.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lStatus.AutoSize = true;
            this.lStatus.Location = new System.Drawing.Point(3, 86);
            this.lStatus.Name = "lStatus";
            this.lStatus.Size = new System.Drawing.Size(40, 13);
            this.lStatus.TabIndex = 0;
            this.lStatus.Text = "Status:";
            // 
            // tbStatus
            // 
            this.tbStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbStatus.Location = new System.Drawing.Point(49, 3);
            this.tbStatus.Multiline = true;
            this.tbStatus.Name = "tbStatus";
            this.tbStatus.Size = new System.Drawing.Size(589, 180);
            this.tbStatus.TabIndex = 1;
            // 
            // pgbStatus
            // 
            this.pgbStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpShareMain.SetColumnSpan(this.pgbStatus, 2);
            this.pgbStatus.Location = new System.Drawing.Point(3, 189);
            this.pgbStatus.Name = "pgbStatus";
            this.pgbStatus.Size = new System.Drawing.Size(635, 23);
            this.pgbStatus.TabIndex = 2;
            // 
            // lLink
            // 
            this.lLink.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lLink.AutoSize = true;
            this.lLink.Location = new System.Drawing.Point(3, 221);
            this.lLink.Name = "lLink";
            this.lLink.Size = new System.Drawing.Size(30, 13);
            this.lLink.TabIndex = 3;
            this.lLink.Text = "Link:";
            // 
            // tbLink
            // 
            this.tbLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLink.Location = new System.Drawing.Point(49, 218);
            this.tbLink.Name = "tbLink";
            this.tbLink.ReadOnly = true;
            this.tbLink.Size = new System.Drawing.Size(589, 20);
            this.tbLink.TabIndex = 4;
            // 
            // bOk
            // 
            this.bOk.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tlpShareMain.SetColumnSpan(this.bOk, 2);
            this.bOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.bOk.Location = new System.Drawing.Point(283, 244);
            this.bOk.Name = "bOk";
            this.bOk.Size = new System.Drawing.Size(75, 23);
            this.bOk.TabIndex = 5;
            this.bOk.Text = "Ok";
            this.bOk.UseVisualStyleBackColor = true;
            // 
            // ucSINnerShare
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tlpShareMain);
            this.Name = "ucSINnerShare";
            this.Size = new System.Drawing.Size(641, 270);
            this.Load += new System.EventHandler(this.UcSINnerShare_Load);
            this.tlpShareMain.ResumeLayout(false);
            this.tlpShareMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpShareMain;
        private System.Windows.Forms.Label lStatus;
        private System.Windows.Forms.TextBox tbStatus;
        private System.Windows.Forms.ProgressBar pgbStatus;
        private System.Windows.Forms.Label lLink;
        private System.Windows.Forms.TextBox tbLink;
        private System.Windows.Forms.Button bOk;
    }
}
