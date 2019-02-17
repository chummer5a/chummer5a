namespace ChummerDataViewer
{
	partial class CrashReportView
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
			this.lblGuid = new System.Windows.Forms.Label();
			this.lblVersion = new System.Windows.Forms.Label();
			this.lblBuildType = new System.Windows.Forms.Label();
			this.lblDate = new System.Windows.Forms.Label();
			this.btnAction = new System.Windows.Forms.Button();
			this.lblExceptionGuess = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lblGuid
			// 
			this.lblGuid.AutoSize = true;
			this.lblGuid.Location = new System.Drawing.Point(70, 5);
			this.lblGuid.Name = "lblGuid";
			this.lblGuid.Size = new System.Drawing.Size(211, 13);
			this.lblGuid.TabIndex = 0;
			this.lblGuid.Text = "00000000-0000-0000-0000-000000000000";
			// 
			// lblVersion
			// 
			this.lblVersion.AutoSize = true;
			this.lblVersion.Location = new System.Drawing.Point(364, 5);
			this.lblVersion.Name = "lblVersion";
			this.lblVersion.Size = new System.Drawing.Size(43, 13);
			this.lblVersion.TabIndex = 1;
			this.lblVersion.Text = "5.000.0";
			// 
			// lblBuildType
			// 
			this.lblBuildType.AutoSize = true;
			this.lblBuildType.Location = new System.Drawing.Point(296, 5);
			this.lblBuildType.Name = "lblBuildType";
			this.lblBuildType.Size = new System.Drawing.Size(62, 13);
			this.lblBuildType.TabIndex = 2;
			this.lblBuildType.Text = "[RELEASE]";
			// 
			// lblDate
			// 
			this.lblDate.AutoSize = true;
			this.lblDate.Location = new System.Drawing.Point(4, 5);
			this.lblDate.Name = "lblDate";
			this.lblDate.Size = new System.Drawing.Size(60, 13);
			this.lblDate.TabIndex = 3;
			this.lblDate.Text = "[01 Jan 00]";
			// 
			// lblExceptionGuess
			// 
			this.lblExceptionGuess.AutoSize = true;
			this.lblExceptionGuess.Location = new System.Drawing.Point(413, 5);
			this.lblExceptionGuess.Name = "lblExceptionGuess";
			this.lblExceptionGuess.Size = new System.Drawing.Size(43, 13);
			this.lblExceptionGuess.TabIndex = 5;
			this.lblExceptionGuess.Text = "[Guess]";
			this.lblExceptionGuess.Visible = false;
			// 
			// btnAction
			// 
			this.btnAction.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnAction.Location = new System.Drawing.Point(659, 0);
			this.btnAction.Name = "btnAction";
			this.btnAction.Size = new System.Drawing.Size(75, 23);
			this.btnAction.TabIndex = 4;
			this.btnAction.Text = "[Action]";
			this.btnAction.UseVisualStyleBackColor = true;
			this.btnAction.Click += new System.EventHandler(this.btnAction_Click);
			// 
			// CrashReportView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.btnAction);
			this.Controls.Add(this.lblExceptionGuess);
			this.Controls.Add(this.lblDate);
			this.Controls.Add(this.lblBuildType);
			this.Controls.Add(this.lblVersion);
			this.Controls.Add(this.lblGuid);
			this.Name = "CrashReportView";
			this.Size = new System.Drawing.Size(737, 23);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblGuid;
		private System.Windows.Forms.Label lblVersion;
		private System.Windows.Forms.Label lblBuildType;
		private System.Windows.Forms.Label lblDate;
		private System.Windows.Forms.Button btnAction;
		private System.Windows.Forms.Label lblExceptionGuess;
	}
}
