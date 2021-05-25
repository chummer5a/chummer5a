namespace Chummer
{
    partial class frmLoading
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLoading));
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblLoadingInfo = new System.Windows.Forms.Label();
            this.pgbLoadingProgress = new System.Windows.Forms.ProgressBar();
            this.tlpMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.lblLoadingInfo, 0, 0);
            this.tlpMain.Controls.Add(this.pgbLoadingProgress, 0, 1);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(426, 55);
            this.tlpMain.TabIndex = 0;
            this.tlpMain.UseWaitCursor = true;
            // 
            // lblLoadingInfo
            // 
            this.lblLoadingInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblLoadingInfo.AutoSize = true;
            this.lblLoadingInfo.Location = new System.Drawing.Point(3, 6);
            this.lblLoadingInfo.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLoadingInfo.Name = "lblLoadingInfo";
            this.lblLoadingInfo.Size = new System.Drawing.Size(83, 11);
            this.lblLoadingInfo.TabIndex = 0;
            this.lblLoadingInfo.Tag = "String_Initializing";
            this.lblLoadingInfo.Text = "Loading [Item]...";
            this.lblLoadingInfo.UseWaitCursor = true;
            // 
            // pgbLoadingProgress
            // 
            this.pgbLoadingProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgbLoadingProgress.Location = new System.Drawing.Point(3, 26);
            this.pgbLoadingProgress.Name = "pgbLoadingProgress";
            this.pgbLoadingProgress.Size = new System.Drawing.Size(420, 26);
            this.pgbLoadingProgress.Step = 1;
            this.pgbLoadingProgress.TabIndex = 1;
            this.pgbLoadingProgress.UseWaitCursor = true;
            // 
            // frmLoading
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(444, 73);
            this.ControlBox = false;
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmLoading";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Tag = "String_Loading";
            this.Text = "Loading [Character]...";
            this.UseWaitCursor = true;
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Chummer.BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.Label lblLoadingInfo;
        private System.Windows.Forms.ProgressBar pgbLoadingProgress;
    }
}
