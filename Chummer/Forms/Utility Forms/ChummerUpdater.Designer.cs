namespace Chummer
{
    partial class ChummerUpdater
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChummerUpdater));
            this.webNotes = new System.Windows.Forms.WebBrowser();
            this.cmdUpdate = new System.Windows.Forms.Button();
            this.pgbOverallProgress = new System.Windows.Forms.ProgressBar();
            this.cmdCleanReinstall = new System.Windows.Forms.Button();
            this.lblUpdaterStatusLabel = new System.Windows.Forms.Label();
            this.lblUpdaterStatus = new System.Windows.Forms.Label();
            this.cmdRestart = new System.Windows.Forms.Button();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.tlpBottom = new System.Windows.Forms.TableLayoutPanel();
            this.tlpMain.SuspendLayout();
            this.tlpBottom.SuspendLayout();
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
            this.cmdUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdUpdate.AutoSize = true;
            this.cmdUpdate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdUpdate.Enabled = false;
            this.cmdUpdate.Location = new System.Drawing.Point(3, 7);
            this.cmdUpdate.MinimumSize = new System.Drawing.Size(80, 0);
            this.cmdUpdate.Name = "cmdUpdate";
            this.cmdUpdate.Size = new System.Drawing.Size(80, 23);
            this.cmdUpdate.TabIndex = 1;
            this.cmdUpdate.Tag = "Button_Download";
            this.cmdUpdate.Text = "Download";
            this.cmdUpdate.UseVisualStyleBackColor = true;
            this.cmdUpdate.Click += new System.EventHandler(this.cmdUpdate_Click);
            // 
            // pgbOverallProgress
            // 
            this.pgbOverallProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgbOverallProgress.Location = new System.Drawing.Point(89, 3);
            this.pgbOverallProgress.Name = "pgbOverallProgress";
            this.pgbOverallProgress.Size = new System.Drawing.Size(473, 31);
            this.pgbOverallProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pgbOverallProgress.TabIndex = 2;
            // 
            // cmdCleanReinstall
            // 
            this.cmdCleanReinstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCleanReinstall.AutoSize = true;
            this.cmdCleanReinstall.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCleanReinstall.Enabled = false;
            this.cmdCleanReinstall.Location = new System.Drawing.Point(676, 7);
            this.cmdCleanReinstall.MinimumSize = new System.Drawing.Size(80, 0);
            this.cmdCleanReinstall.Name = "cmdCleanReinstall";
            this.cmdCleanReinstall.Size = new System.Drawing.Size(87, 23);
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
            this.cmdRestart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdRestart.AutoSize = true;
            this.cmdRestart.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRestart.Enabled = false;
            this.cmdRestart.Location = new System.Drawing.Point(568, 7);
            this.cmdRestart.MinimumSize = new System.Drawing.Size(80, 0);
            this.cmdRestart.Name = "cmdRestart";
            this.cmdRestart.Size = new System.Drawing.Size(102, 23);
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
            this.tlpMain.Controls.Add(this.tlpBottom, 0, 2);
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
            // tlpBottom
            // 
            this.tlpBottom.AutoSize = true;
            this.tlpBottom.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpBottom.ColumnCount = 4;
            this.tlpMain.SetColumnSpan(this.tlpBottom, 2);
            this.tlpBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBottom.Controls.Add(this.cmdRestart, 2, 0);
            this.tlpBottom.Controls.Add(this.pgbOverallProgress, 1, 0);
            this.tlpBottom.Controls.Add(this.cmdUpdate, 0, 0);
            this.tlpBottom.Controls.Add(this.cmdCleanReinstall, 3, 0);
            this.tlpBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tlpBottom.Location = new System.Drawing.Point(0, 506);
            this.tlpBottom.Margin = new System.Windows.Forms.Padding(0);
            this.tlpBottom.Name = "tlpBottom";
            this.tlpBottom.RowCount = 1;
            this.tlpBottom.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBottom.Size = new System.Drawing.Size(766, 37);
            this.tlpBottom.TabIndex = 107;
            // 
            // ChummerUpdater
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ChummerUpdater";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.Text = "Chummer Updater";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ChummerUpdater_FormClosing);
            this.Load += new System.EventHandler(this.ChummerUpdater_Load);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpBottom.ResumeLayout(false);
            this.tlpBottom.PerformLayout();
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
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.TableLayoutPanel tlpBottom;
    }
}
