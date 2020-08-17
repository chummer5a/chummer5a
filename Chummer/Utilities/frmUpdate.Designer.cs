namespace Chummer
{
    partial class frmUpdate
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
                _workerConnectionLoader?.Dispose();
                _clientDownloader?.Dispose();
                _clientChangelogDownloader?.Dispose();
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
            this.webNotes = new System.Windows.Forms.WebBrowser();
            this.cmdUpdate = new System.Windows.Forms.Button();
            this.pgbOverallProgress = new System.Windows.Forms.ProgressBar();
            this.cmdCleanReinstall = new System.Windows.Forms.Button();
            this.lblUpdaterStatusLabel = new System.Windows.Forms.Label();
            this.lblUpdaterStatus = new System.Windows.Forms.Label();
            this.cmdRestart = new System.Windows.Forms.Button();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tableLayoutPanel2 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpMain.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // webNotes
            // 
            this.webNotes.AllowNavigation = false;
            this.tlpMain.SetColumnSpan(this.webNotes, 2);
            this.webNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webNotes.Location = new System.Drawing.Point(3, 3);
            this.webNotes.MinimumSize = new System.Drawing.Size(20, 20);
            this.webNotes.Name = "webNotes";
            this.webNotes.Size = new System.Drawing.Size(760, 475);
            this.webNotes.TabIndex = 0;
            this.webNotes.WebBrowserShortcutsEnabled = false;
            // 
            // cmdUpdate
            // 
            this.cmdUpdate.AutoSize = true;
            this.cmdUpdate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdUpdate.Enabled = false;
            this.cmdUpdate.Location = new System.Drawing.Point(3, 3);
            this.cmdUpdate.Name = "cmdUpdate";
            this.cmdUpdate.Size = new System.Drawing.Size(100, 31);
            this.cmdUpdate.TabIndex = 1;
            this.cmdUpdate.Tag = "Button_Download";
            this.cmdUpdate.Text = "Download";
            this.cmdUpdate.UseVisualStyleBackColor = true;
            this.cmdUpdate.Click += new System.EventHandler(this.cmdDownload_Click);
            // 
            // pgbOverallProgress
            // 
            this.pgbOverallProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgbOverallProgress.Location = new System.Drawing.Point(109, 3);
            this.pgbOverallProgress.Name = "pgbOverallProgress";
            this.pgbOverallProgress.Size = new System.Drawing.Size(440, 31);
            this.pgbOverallProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pgbOverallProgress.TabIndex = 2;
            // 
            // cmdCleanReinstall
            // 
            this.cmdCleanReinstall.AutoSize = true;
            this.cmdCleanReinstall.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdCleanReinstall.Enabled = false;
            this.cmdCleanReinstall.Location = new System.Drawing.Point(663, 3);
            this.cmdCleanReinstall.Name = "cmdCleanReinstall";
            this.cmdCleanReinstall.Size = new System.Drawing.Size(100, 31);
            this.cmdCleanReinstall.TabIndex = 4;
            this.cmdCleanReinstall.Tag = "Button_Clean_Reinstall";
            this.cmdCleanReinstall.Text = "Clean Reinstall";
            this.cmdCleanReinstall.UseVisualStyleBackColor = true;
            this.cmdCleanReinstall.Click += new System.EventHandler(this.cmdCleanReinstall_Click);
            // 
            // lblUpdaterStatusLabel
            // 
            this.lblUpdaterStatusLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblUpdaterStatusLabel.AutoSize = true;
            this.lblUpdaterStatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUpdaterStatusLabel.Location = new System.Drawing.Point(3, 487);
            this.lblUpdaterStatusLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblUpdaterStatusLabel.Name = "lblUpdaterStatusLabel";
            this.lblUpdaterStatusLabel.Size = new System.Drawing.Size(81, 13);
            this.lblUpdaterStatusLabel.TabIndex = 104;
            this.lblUpdaterStatusLabel.Tag = "Label_Updater_Status";
            this.lblUpdaterStatusLabel.Text = "Updater Status:";
            this.lblUpdaterStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblUpdaterStatus
            // 
            this.lblUpdaterStatus.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblUpdaterStatus.AutoSize = true;
            this.lblUpdaterStatus.Location = new System.Drawing.Point(90, 487);
            this.lblUpdaterStatus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblUpdaterStatus.Name = "lblUpdaterStatus";
            this.lblUpdaterStatus.Size = new System.Drawing.Size(119, 13);
            this.lblUpdaterStatus.TabIndex = 106;
            this.lblUpdaterStatus.Tag = "String_Checking_For_Update";
            this.lblUpdaterStatus.Text = "Checking for Updates...";
            this.lblUpdaterStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmdRestart
            // 
            this.cmdRestart.AutoSize = true;
            this.cmdRestart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdRestart.Enabled = false;
            this.cmdRestart.Location = new System.Drawing.Point(555, 3);
            this.cmdRestart.Name = "cmdRestart";
            this.cmdRestart.Size = new System.Drawing.Size(102, 31);
            this.cmdRestart.TabIndex = 3;
            this.cmdRestart.Tag = "Button_Install_Restart";
            this.cmdRestart.Text = "Install and Restart";
            this.cmdRestart.UseVisualStyleBackColor = true;
            this.cmdRestart.Click += new System.EventHandler(this.cmdRestart_Click);
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.webNotes, 0, 0);
            this.tlpMain.Controls.Add(this.lblUpdaterStatus, 1, 1);
            this.tlpMain.Controls.Add(this.lblUpdaterStatusLabel, 0, 1);
            this.tlpMain.Controls.Add(this.tableLayoutPanel2, 0, 2);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 3;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(766, 543);
            this.tlpMain.TabIndex = 107;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tlpMain.SetColumnSpan(this.tableLayoutPanel2, 2);
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.pgbOverallProgress, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.cmdCleanReinstall, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.cmdRestart, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.cmdUpdate, 0, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 506);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(766, 37);
            this.tableLayoutPanel2.TabIndex = 107;
            // 
            // frmUpdate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "frmUpdate";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.Text = "Chummer Updater";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmUpdate_FormClosing);
            this.Load += new System.EventHandler(this.frmUpdate_Load);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser webNotes;
        private System.Windows.Forms.Button cmdUpdate;
        private System.Windows.Forms.ProgressBar pgbOverallProgress;
        private System.Windows.Forms.Button cmdCleanReinstall;
        private System.Windows.Forms.Label lblUpdaterStatusLabel;
        private System.Windows.Forms.Label lblUpdaterStatus;
        private System.Windows.Forms.Button cmdRestart;
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private Chummer.BufferedTableLayoutPanel tableLayoutPanel2;
    }
}
