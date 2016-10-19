namespace CrashHandler
{
	partial class frmCrashReporter
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
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.statusCollectionProgess = new System.Windows.Forms.ToolStripStatusLabel();
			this.lblTitle = new System.Windows.Forms.Label();
			this.lblDesc = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.btnNo = new System.Windows.Forms.Button();
			this.btnSend = new System.Windows.Forms.Button();
			this.llblContents = new System.Windows.Forms.LinkLabel();
			this.label2 = new System.Windows.Forms.Label();
			this.txtDesc = new System.Windows.Forms.TextBox();
			this.timerRefreshTextFile = new System.Windows.Forms.Timer(this.components);
			this.txtIdSelectable = new System.Windows.Forms.TextBox();
			this.statusStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusCollectionProgess});
			this.statusStrip1.Location = new System.Drawing.Point(0, 326);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(438, 22);
			this.statusStrip1.TabIndex = 0;
			this.statusStrip1.Text = "statusStrip";
			// 
			// statusCollectionProgess
			// 
			this.statusCollectionProgess.Name = "statusCollectionProgess";
			this.statusCollectionProgess.Size = new System.Drawing.Size(57, 17);
			this.statusCollectionProgess.Text = "Starting...";
			// 
			// lblTitle
			// 
			this.lblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblTitle.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblTitle.Location = new System.Drawing.Point(0, 0);
			this.lblTitle.Margin = new System.Windows.Forms.Padding(10);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(438, 58);
			this.lblTitle.TabIndex = 1;
			this.lblTitle.Text = "Chummer5a has encountered a problem and needs to close. We are sorry for the inco" +
    "nvenience.";
			this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblDesc
			// 
			this.lblDesc.AutoSize = true;
			this.lblDesc.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblDesc.Location = new System.Drawing.Point(22, 68);
			this.lblDesc.Name = "lblDesc";
			this.lblDesc.Size = new System.Drawing.Size(53, 17);
			this.lblDesc.TabIndex = 2;
			this.lblDesc.Text = "[DESC]";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(1, 85);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(417, 17);
			this.label1.TabIndex = 3;
			this.label1.Text = "Please send an error report to help resolve this problem";
			// 
			// btnNo
			// 
			this.btnNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnNo.Location = new System.Drawing.Point(351, 300);
			this.btnNo.Name = "btnNo";
			this.btnNo.Size = new System.Drawing.Size(75, 23);
			this.btnNo.TabIndex = 4;
			this.btnNo.Text = "Don\'t send";
			this.btnNo.UseVisualStyleBackColor = true;
			this.btnNo.Click += new System.EventHandler(this.btnNo_Click);
			// 
			// btnSend
			// 
			this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSend.Location = new System.Drawing.Point(270, 300);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(75, 23);
			this.btnSend.TabIndex = 5;
			this.btnSend.Text = "Send Error Report";
			this.btnSend.UseVisualStyleBackColor = true;
			this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
			// 
			// llblContents
			// 
			this.llblContents.AutoSize = true;
			this.llblContents.Location = new System.Drawing.Point(10, 115);
			this.llblContents.Name = "llblContents";
			this.llblContents.Size = new System.Drawing.Size(151, 13);
			this.llblContents.TabIndex = 6;
			this.llblContents.TabStop = true;
			this.llblContents.Text = "What does the report contain?";
			this.llblContents.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llblContents_LinkClicked);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(10, 128);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(357, 13);
			this.label2.TabIndex = 7;
			this.label2.Text = "Please describe what you where doing when chummer crashed (optional): ";
			// 
			// txtDesc
			// 
			this.txtDesc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtDesc.Location = new System.Drawing.Point(13, 144);
			this.txtDesc.Multiline = true;
			this.txtDesc.Name = "txtDesc";
			this.txtDesc.Size = new System.Drawing.Size(413, 150);
			this.txtDesc.TabIndex = 8;
			this.txtDesc.TextChanged += new System.EventHandler(this.txtDesc_TextChanged);
			// 
			// timerRefreshTextFile
			// 
			this.timerRefreshTextFile.Interval = 5000;
			this.timerRefreshTextFile.Tick += new System.EventHandler(this.timerRefreshTextFile_Tick);
			// 
			// txtIdSelectable
			// 
			this.txtIdSelectable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtIdSelectable.BackColor = System.Drawing.SystemColors.Control;
			this.txtIdSelectable.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txtIdSelectable.Location = new System.Drawing.Point(10, 102);
			this.txtIdSelectable.Name = "txtIdSelectable";
			this.txtIdSelectable.ReadOnly = true;
			this.txtIdSelectable.Size = new System.Drawing.Size(416, 13);
			this.txtIdSelectable.TabIndex = 10;
			this.txtIdSelectable.TabStop = false;
			this.txtIdSelectable.Text = "[ID]";
			// 
			// frmCrashReporter
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(438, 348);
			this.Controls.Add(this.txtIdSelectable);
			this.Controls.Add(this.txtDesc);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.llblContents);
			this.Controls.Add(this.btnSend);
			this.Controls.Add(this.btnNo);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.lblDesc);
			this.Controls.Add(this.lblTitle);
			this.Controls.Add(this.statusStrip1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmCrashReporter";
			this.Text = "Chummer5a Crash Reporter";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmCrashReporter_FormClosing);
			this.Load += new System.EventHandler(this.frmCrashReporter_Load);
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel statusCollectionProgess;
		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.Label lblDesc;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnNo;
		private System.Windows.Forms.Button btnSend;
		private System.Windows.Forms.LinkLabel llblContents;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtDesc;
		private System.Windows.Forms.Timer timerRefreshTextFile;
		private System.Windows.Forms.TextBox txtIdSelectable;
	}
}

