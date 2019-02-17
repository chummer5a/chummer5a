namespace CrashHandler
{
	partial class frmNoMoreUserInput
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
			this.lblProgress = new System.Windows.Forms.Label();
			this.lblThanks = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lblProgress
			// 
			this.lblProgress.AutoSize = true;
			this.lblProgress.Location = new System.Drawing.Point(12, 35);
			this.lblProgress.Name = "lblProgress";
			this.lblProgress.Size = new System.Drawing.Size(53, 13);
			this.lblProgress.TabIndex = 0;
			this.lblProgress.Text = "[progress]";
			// 
			// lblThanks
			// 
			this.lblThanks.AutoSize = true;
			this.lblThanks.Location = new System.Drawing.Point(12, 9);
			this.lblThanks.Name = "lblThanks";
			this.lblThanks.Size = new System.Drawing.Size(301, 13);
			this.lblThanks.TabIndex = 1;
			this.lblThanks.Text = "Thank you. This window will close automatically once finished.";
			// 
			// frmNoMoreUserInput
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(330, 61);
			this.Controls.Add(this.lblThanks);
			this.Controls.Add(this.lblProgress);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "frmNoMoreUserInput";
			this.Text = "Submitting";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblProgress;
		private System.Windows.Forms.Label lblThanks;
	}
}