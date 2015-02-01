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
			this.cmdSelectAll = new System.Windows.Forms.Button();
			this.webNotes = new System.Windows.Forms.WebBrowser();
			this.pgbOverallProgress = new System.Windows.Forms.ProgressBar();
			this.cmdUpdate = new System.Windows.Forms.Button();
			this.treeUpdate = new System.Windows.Forms.TreeView();
			this.pgbFileProgress = new System.Windows.Forms.ProgressBar();
			this.lblOverallProgress = new System.Windows.Forms.Label();
			this.lblFileProgress = new System.Windows.Forms.Label();
			this.cmdRestart = new System.Windows.Forms.Button();
			this.lblDone = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// cmdSelectAll
			// 
			this.cmdSelectAll.AutoSize = true;
			this.cmdSelectAll.Location = new System.Drawing.Point(493, 13);
			this.cmdSelectAll.Name = "cmdSelectAll";
			this.cmdSelectAll.Size = new System.Drawing.Size(75, 23);
			this.cmdSelectAll.TabIndex = 2;
			this.cmdSelectAll.Tag = "Button_Update_SelectAll";
			this.cmdSelectAll.Text = "Select &All";
			this.cmdSelectAll.UseVisualStyleBackColor = true;
			this.cmdSelectAll.Click += new System.EventHandler(this.cmdSelectAll_Click);
			// 
			// webNotes
			// 
			this.webNotes.AllowWebBrowserDrop = false;
			this.webNotes.IsWebBrowserContextMenuEnabled = false;
			this.webNotes.Location = new System.Drawing.Point(297, 100);
			this.webNotes.MinimumSize = new System.Drawing.Size(20, 20);
			this.webNotes.Name = "webNotes";
			this.webNotes.Size = new System.Drawing.Size(630, 460);
			this.webNotes.TabIndex = 7;
			this.webNotes.WebBrowserShortcutsEnabled = false;
			// 
			// pgbOverallProgress
			// 
			this.pgbOverallProgress.Location = new System.Drawing.Point(389, 42);
			this.pgbOverallProgress.Name = "pgbOverallProgress";
			this.pgbOverallProgress.Size = new System.Drawing.Size(538, 23);
			this.pgbOverallProgress.TabIndex = 4;
			// 
			// cmdUpdate
			// 
			this.cmdUpdate.AutoSize = true;
			this.cmdUpdate.Location = new System.Drawing.Point(296, 12);
			this.cmdUpdate.Name = "cmdUpdate";
			this.cmdUpdate.Size = new System.Drawing.Size(160, 23);
			this.cmdUpdate.TabIndex = 1;
			this.cmdUpdate.Tag = "Button_Update_DownloadSelectedUpdates";
			this.cmdUpdate.Text = "&Download Selected Updates";
			this.cmdUpdate.UseVisualStyleBackColor = true;
			this.cmdUpdate.Click += new System.EventHandler(this.cmdUpdate_Click);
			// 
			// treeUpdate
			// 
			this.treeUpdate.CheckBoxes = true;
			this.treeUpdate.HotTracking = true;
			this.treeUpdate.Location = new System.Drawing.Point(12, 12);
			this.treeUpdate.Name = "treeUpdate";
			this.treeUpdate.Size = new System.Drawing.Size(278, 548);
			this.treeUpdate.TabIndex = 0;
			this.treeUpdate.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeUpdate_AfterCheck);
			this.treeUpdate.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeUpdate_AfterSelect);
			// 
			// pgbFileProgress
			// 
			this.pgbFileProgress.Location = new System.Drawing.Point(389, 71);
			this.pgbFileProgress.Name = "pgbFileProgress";
			this.pgbFileProgress.Size = new System.Drawing.Size(538, 23);
			this.pgbFileProgress.TabIndex = 6;
			// 
			// lblOverallProgress
			// 
			this.lblOverallProgress.AutoSize = true;
			this.lblOverallProgress.Location = new System.Drawing.Point(296, 46);
			this.lblOverallProgress.Name = "lblOverallProgress";
			this.lblOverallProgress.Size = new System.Drawing.Size(87, 13);
			this.lblOverallProgress.TabIndex = 3;
			this.lblOverallProgress.Tag = "Label_Update_OverallProgress";
			this.lblOverallProgress.Text = "Overall Progress:";
			// 
			// lblFileProgress
			// 
			this.lblFileProgress.AutoSize = true;
			this.lblFileProgress.Location = new System.Drawing.Point(296, 75);
			this.lblFileProgress.Name = "lblFileProgress";
			this.lblFileProgress.Size = new System.Drawing.Size(70, 13);
			this.lblFileProgress.TabIndex = 5;
			this.lblFileProgress.Tag = "Label_Update_FileProgress";
			this.lblFileProgress.Text = "File Progress:";
			// 
			// cmdRestart
			// 
			this.cmdRestart.AutoSize = true;
			this.cmdRestart.Location = new System.Drawing.Point(826, 12);
			this.cmdRestart.Name = "cmdRestart";
			this.cmdRestart.Size = new System.Drawing.Size(101, 23);
			this.cmdRestart.TabIndex = 8;
			this.cmdRestart.Tag = "Button_Update_RestartChummer";
			this.cmdRestart.Text = "Restart Chummer";
			this.cmdRestart.UseVisualStyleBackColor = true;
			this.cmdRestart.Visible = false;
			this.cmdRestart.Click += new System.EventHandler(this.cmdRestart_Click);
			// 
			// lblDone
			// 
			this.lblDone.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblDone.Location = new System.Drawing.Point(389, 42);
			this.lblDone.Name = "lblDone";
			this.lblDone.Size = new System.Drawing.Size(538, 52);
			this.lblDone.TabIndex = 9;
			this.lblDone.Tag = "Label_Update_DownloadComplete";
			this.lblDone.Text = "Download Complete";
			this.lblDone.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lblDone.Visible = false;
			// 
			// frmUpdate
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(939, 572);
			this.Controls.Add(this.cmdRestart);
			this.Controls.Add(this.lblFileProgress);
			this.Controls.Add(this.lblOverallProgress);
			this.Controls.Add(this.pgbFileProgress);
			this.Controls.Add(this.cmdSelectAll);
			this.Controls.Add(this.webNotes);
			this.Controls.Add(this.pgbOverallProgress);
			this.Controls.Add(this.cmdUpdate);
			this.Controls.Add(this.treeUpdate);
			this.Controls.Add(this.lblDone);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmUpdate";
			this.Opacity = 0D;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Tag = "Title_Update";
			this.Text = "Chummer Update";
			this.Load += new System.EventHandler(this.frmUpdate_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal System.Windows.Forms.Button cmdSelectAll;
		internal System.Windows.Forms.WebBrowser webNotes;
		internal System.Windows.Forms.ProgressBar pgbOverallProgress;
		internal System.Windows.Forms.Button cmdUpdate;
		internal System.Windows.Forms.TreeView treeUpdate;
		internal System.Windows.Forms.ProgressBar pgbFileProgress;
		private System.Windows.Forms.Label lblOverallProgress;
		private System.Windows.Forms.Label lblFileProgress;
		internal System.Windows.Forms.Button cmdRestart;
		private System.Windows.Forms.Label lblDone;
	}
}