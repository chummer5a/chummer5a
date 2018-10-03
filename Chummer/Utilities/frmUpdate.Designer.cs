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
            this.webNotes = new System.Windows.Forms.WebBrowser();
            this.cmdUpdate = new System.Windows.Forms.Button();
            this.pgbOverallProgress = new System.Windows.Forms.ProgressBar();
            this.cmdCleanReinstall = new System.Windows.Forms.Button();
            this.lblUpdaterStatusLabel = new System.Windows.Forms.Label();
            this.lblUpdaterStatus = new System.Windows.Forms.Label();
            this.cmdRestart = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel();
            this.tableLayoutPanel2 = new Chummer.BufferedTableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // webNotes
            // 
            this.webNotes.AllowNavigation = false;
            this.webNotes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.webNotes, 2);
            this.webNotes.Location = new System.Drawing.Point(3, 3);
            this.webNotes.MinimumSize = new System.Drawing.Size(20, 20);
            this.webNotes.Name = "webNotes";
            this.webNotes.Size = new System.Drawing.Size(753, 468);
            this.webNotes.TabIndex = 0;
            this.webNotes.WebBrowserShortcutsEnabled = false;
            // 
            // cmdUpdate
            // 
            this.cmdUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
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
            this.pgbOverallProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pgbOverallProgress.Location = new System.Drawing.Point(109, 3);
            this.pgbOverallProgress.Name = "pgbOverallProgress";
            this.pgbOverallProgress.Size = new System.Drawing.Size(435, 31);
            this.pgbOverallProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pgbOverallProgress.TabIndex = 2;
            // 
            // cmdCleanReinstall
            // 
            this.cmdCleanReinstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCleanReinstall.Enabled = false;
            this.cmdCleanReinstall.Location = new System.Drawing.Point(656, 3);
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
            this.lblUpdaterStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblUpdaterStatusLabel.AutoSize = true;
            this.lblUpdaterStatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUpdaterStatusLabel.Location = new System.Drawing.Point(3, 480);
            this.lblUpdaterStatusLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblUpdaterStatusLabel.Name = "lblUpdaterStatusLabel";
            this.lblUpdaterStatusLabel.Size = new System.Drawing.Size(81, 13);
            this.lblUpdaterStatusLabel.TabIndex = 104;
            this.lblUpdaterStatusLabel.Tag = "Label_Updater_Status";
            this.lblUpdaterStatusLabel.Text = "Updater Status:";
            // 
            // lblUpdaterStatus
            // 
            this.lblUpdaterStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblUpdaterStatus.AutoSize = true;
            this.lblUpdaterStatus.Location = new System.Drawing.Point(90, 480);
            this.lblUpdaterStatus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblUpdaterStatus.Name = "lblUpdaterStatus";
            this.lblUpdaterStatus.Size = new System.Drawing.Size(119, 13);
            this.lblUpdaterStatus.TabIndex = 106;
            this.lblUpdaterStatus.Tag = "String_Checking_For_Update";
            this.lblUpdaterStatus.Text = "Checking for Updates...";
            // 
            // cmdRestart
            // 
            this.cmdRestart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdRestart.Enabled = false;
            this.cmdRestart.Location = new System.Drawing.Point(550, 3);
            this.cmdRestart.Name = "cmdRestart";
            this.cmdRestart.Size = new System.Drawing.Size(100, 31);
            this.cmdRestart.TabIndex = 3;
            this.cmdRestart.Tag = "Button_Install_Restart";
            this.cmdRestart.Text = "Install and Restart";
            this.cmdRestart.UseVisualStyleBackColor = true;
            this.cmdRestart.Click += new System.EventHandler(this.cmdRestart_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.webNotes, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblUpdaterStatus, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblUpdaterStatusLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(13, 13);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(759, 536);
            this.tableLayoutPanel1.TabIndex = 107;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel2, 2);
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.pgbOverallProgress, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.cmdCleanReinstall, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.cmdRestart, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.cmdUpdate, 0, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 499);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(759, 37);
            this.tableLayoutPanel2.TabIndex = 107;
            // 
            // frmUpdate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "frmUpdate";
            this.Text = "Chummer Updater";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmUpdate_FormClosing);
            this.Load += new System.EventHandler(this.frmUpdate_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
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
        private Chummer.BufferedTableLayoutPanel tableLayoutPanel1;
        private Chummer.BufferedTableLayoutPanel tableLayoutPanel2;
    }
}
