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
            this.cmdRestart = new System.Windows.Forms.Button();
            this.lblUpdaterStatusLabel = new System.Windows.Forms.Label();
            this.lblUpdaterStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // webNotes
            // 
            this.webNotes.AllowNavigation = false;
            this.webNotes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webNotes.Location = new System.Drawing.Point(12, 9);
            this.webNotes.MinimumSize = new System.Drawing.Size(20, 20);
            this.webNotes.Name = "webNotes";
            this.webNotes.Size = new System.Drawing.Size(821, 466);
            this.webNotes.TabIndex = 0;
            this.webNotes.WebBrowserShortcutsEnabled = false;
            // 
            // cmdUpdate
            // 
            this.cmdUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdUpdate.Location = new System.Drawing.Point(12, 514);
            this.cmdUpdate.Name = "cmdUpdate";
            this.cmdUpdate.Size = new System.Drawing.Size(105, 31);
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
            this.pgbOverallProgress.Location = new System.Drawing.Point(123, 514);
            this.pgbOverallProgress.Name = "pgbOverallProgress";
            this.pgbOverallProgress.Size = new System.Drawing.Size(599, 31);
            this.pgbOverallProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pgbOverallProgress.TabIndex = 2;
            // 
            // cmdRestart
            // 
            this.cmdRestart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdRestart.Enabled = false;
            this.cmdRestart.Location = new System.Drawing.Point(728, 514);
            this.cmdRestart.Name = "cmdRestart";
            this.cmdRestart.Size = new System.Drawing.Size(105, 31);
            this.cmdRestart.TabIndex = 3;
            this.cmdRestart.Tag = "Button_Install_Restart";
            this.cmdRestart.Text = "Install and Restart";
            this.cmdRestart.UseVisualStyleBackColor = true;
            this.cmdRestart.Click += new System.EventHandler(this.cmdRestart_Click);
            // 
            // lblUpdaterStatusLabel
            // 
            this.lblUpdaterStatusLabel.AutoSize = true;
            this.lblUpdaterStatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUpdaterStatusLabel.Location = new System.Drawing.Point(9, 488);
            this.lblUpdaterStatusLabel.Name = "lblUpdaterStatusLabel";
            this.lblUpdaterStatusLabel.Size = new System.Drawing.Size(81, 13);
            this.lblUpdaterStatusLabel.TabIndex = 104;
            this.lblUpdaterStatusLabel.Tag = "Label_Updater_Status";
            this.lblUpdaterStatusLabel.Text = "Updater Status:";
            // 
            // lblUpdaterStatus
            // 
            this.lblUpdaterStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblUpdaterStatus.AutoSize = true;
            this.lblUpdaterStatus.Location = new System.Drawing.Point(96, 488);
            this.lblUpdaterStatus.Name = "lblUpdaterStatus";
            this.lblUpdaterStatus.Size = new System.Drawing.Size(61, 13);
            this.lblUpdaterStatus.TabIndex = 106;
            this.lblUpdaterStatus.Tag = "";
            this.lblUpdaterStatus.Text = "[0.000.000]";
            // 
            // frmUpdate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(845, 557);
            this.Controls.Add(this.lblUpdaterStatus);
            this.Controls.Add(this.lblUpdaterStatusLabel);
            this.Controls.Add(this.cmdRestart);
            this.Controls.Add(this.pgbOverallProgress);
            this.Controls.Add(this.cmdUpdate);
            this.Controls.Add(this.webNotes);
            this.Name = "frmUpdate";
            this.Text = "Chummer Updater";
            this.Load += new System.EventHandler(this.frmUpdate_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser webNotes;
        private System.Windows.Forms.Button cmdUpdate;
        private System.Windows.Forms.ProgressBar pgbOverallProgress;
        private System.Windows.Forms.Button cmdRestart;
        private System.Windows.Forms.Label lblUpdaterStatusLabel;
        private System.Windows.Forms.Label lblUpdaterStatus;
    }
}
