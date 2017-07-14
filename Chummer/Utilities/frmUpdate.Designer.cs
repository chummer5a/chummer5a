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
            this.SuspendLayout();
            // 
            // webNotes
            // 
            this.webNotes.AllowNavigation = false;
            this.webNotes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webNotes.Location = new System.Drawing.Point(12, 12);
            this.webNotes.MinimumSize = new System.Drawing.Size(20, 20);
            this.webNotes.Name = "webNotes";
            this.webNotes.Size = new System.Drawing.Size(821, 496);
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
            this.cmdUpdate.Text = "Download";
            this.cmdUpdate.UseVisualStyleBackColor = true;
            this.cmdUpdate.Click += new System.EventHandler(this.cmdDownload_Click);
            // 
            // pgbOverallProgress
            // 
            this.pgbOverallProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
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
            this.cmdRestart.Text = "Install and Restart";
            this.cmdRestart.UseVisualStyleBackColor = true;
            this.cmdRestart.Click += new System.EventHandler(this.cmdRestart_Click);
            // 
            // frmUpdate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(845, 557);
            this.Controls.Add(this.cmdRestart);
            this.Controls.Add(this.pgbOverallProgress);
            this.Controls.Add(this.cmdUpdate);
            this.Controls.Add(this.webNotes);
            this.Name = "frmUpdate";
            this.Text = "Chummer Updater";
            this.Load += new System.EventHandler(this.frmUpdate_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser webNotes;
        private System.Windows.Forms.Button cmdUpdate;
        private System.Windows.Forms.ProgressBar pgbOverallProgress;
        private System.Windows.Forms.Button cmdRestart;
    }
}