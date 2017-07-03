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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmCrashReporter));
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.statusCollectionProgess = new System.Windows.Forms.ToolStripStatusLabel();
			this.btnNo = new System.Windows.Forms.Button();
			this.btnSend = new System.Windows.Forms.Button();
			this.timerRefreshTextFile = new System.Windows.Forms.Timer(this.components);
			this.tabUserStory = new System.Windows.Forms.TabControl();
			this.tabGeneral = new System.Windows.Forms.TabPage();
			this.lblIntroText = new System.Windows.Forms.Label();
			this.llblContents = new System.Windows.Forms.LinkLabel();
			this.label3 = new System.Windows.Forms.Label();
			this.txtIdSelectable = new System.Windows.Forms.TextBox();
			this.lblDesc = new System.Windows.Forms.Label();
			this.yabUserStory = new System.Windows.Forms.TabPage();
			this.txtUserStory = new System.Windows.Forms.TextBox();
			this.cmdSubmitIssue = new System.Windows.Forms.Button();
			this.lblDescriptionWarning = new System.Windows.Forms.Label();
			this.statusStrip1.SuspendLayout();
			this.tabUserStory.SuspendLayout();
			this.tabGeneral.SuspendLayout();
			this.yabUserStory.SuspendLayout();
			this.SuspendLayout();
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusCollectionProgess});
			this.statusStrip1.Location = new System.Drawing.Point(0, 326);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(546, 22);
			this.statusStrip1.TabIndex = 0;
			this.statusStrip1.Text = "statusStrip";
			// 
			// statusCollectionProgess
			// 
			this.statusCollectionProgess.Name = "statusCollectionProgess";
			this.statusCollectionProgess.Size = new System.Drawing.Size(57, 17);
			this.statusCollectionProgess.Text = "Starting...";
			// 
			// btnNo
			// 
			this.btnNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnNo.Location = new System.Drawing.Point(444, 288);
			this.btnNo.Name = "btnNo";
			this.btnNo.Size = new System.Drawing.Size(90, 34);
			this.btnNo.TabIndex = 4;
			this.btnNo.Text = "Close";
			this.btnNo.UseVisualStyleBackColor = true;
			this.btnNo.Click += new System.EventHandler(this.btnNo_Click);
			// 
			// btnSend
			// 
			this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSend.Location = new System.Drawing.Point(252, 287);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(90, 35);
			this.btnSend.TabIndex = 5;
			this.btnSend.Text = "Send Error Report";
			this.btnSend.UseVisualStyleBackColor = true;
			this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
			// 
			// timerRefreshTextFile
			// 
			this.timerRefreshTextFile.Interval = 5000;
			this.timerRefreshTextFile.Tick += new System.EventHandler(this.timerRefreshTextFile_Tick);
			// 
			// tabUserStory
			// 
			this.tabUserStory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabUserStory.Controls.Add(this.tabGeneral);
			this.tabUserStory.Controls.Add(this.yabUserStory);
			this.tabUserStory.Location = new System.Drawing.Point(7, 12);
			this.tabUserStory.Name = "tabUserStory";
			this.tabUserStory.SelectedIndex = 0;
			this.tabUserStory.Size = new System.Drawing.Size(527, 274);
			this.tabUserStory.TabIndex = 11;
			// 
			// tabGeneral
			// 
			this.tabGeneral.BackColor = System.Drawing.SystemColors.Control;
			this.tabGeneral.Controls.Add(this.lblIntroText);
			this.tabGeneral.Controls.Add(this.llblContents);
			this.tabGeneral.Controls.Add(this.label3);
			this.tabGeneral.Controls.Add(this.txtIdSelectable);
			this.tabGeneral.Controls.Add(this.lblDesc);
			this.tabGeneral.Location = new System.Drawing.Point(4, 22);
			this.tabGeneral.Name = "tabGeneral";
			this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
			this.tabGeneral.Size = new System.Drawing.Size(519, 248);
			this.tabGeneral.TabIndex = 0;
			this.tabGeneral.Text = "General";
			// 
			// lblIntroText
			// 
			this.lblIntroText.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblIntroText.Location = new System.Drawing.Point(3, 3);
			this.lblIntroText.Margin = new System.Windows.Forms.Padding(5);
			this.lblIntroText.Name = "lblIntroText";
			this.lblIntroText.Size = new System.Drawing.Size(513, 122);
			this.lblIntroText.TabIndex = 18;
			this.lblIntroText.Text = resources.GetString("lblIntroText.Text");
			// 
			// llblContents
			// 
			this.llblContents.AutoSize = true;
			this.llblContents.Location = new System.Drawing.Point(8, 134);
			this.llblContents.Margin = new System.Windows.Forms.Padding(3);
			this.llblContents.Name = "llblContents";
			this.llblContents.Size = new System.Drawing.Size(151, 13);
			this.llblContents.TabIndex = 19;
			this.llblContents.TabStop = true;
			this.llblContents.Text = "What does the report contain?";
			this.llblContents.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llblContents_LinkClicked);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(8, 153);
			this.label3.Margin = new System.Windows.Forms.Padding(3);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(63, 17);
			this.label3.TabIndex = 17;
			this.label3.Text = "Details:";
			// 
			// txtIdSelectable
			// 
			this.txtIdSelectable.BackColor = System.Drawing.SystemColors.Control;
			this.txtIdSelectable.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txtIdSelectable.Location = new System.Drawing.Point(11, 176);
			this.txtIdSelectable.Name = "txtIdSelectable";
			this.txtIdSelectable.ReadOnly = true;
			this.txtIdSelectable.Size = new System.Drawing.Size(423, 13);
			this.txtIdSelectable.TabIndex = 20;
			this.txtIdSelectable.TabStop = false;
			this.txtIdSelectable.Text = "[ID]";
			// 
			// lblDesc
			// 
			this.lblDesc.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblDesc.Location = new System.Drawing.Point(6, 195);
			this.lblDesc.Margin = new System.Windows.Forms.Padding(3);
			this.lblDesc.Name = "lblDesc";
			this.lblDesc.Size = new System.Drawing.Size(507, 47);
			this.lblDesc.TabIndex = 16;
			this.lblDesc.Text = "[DESC]";
			// 
			// yabUserStory
			// 
			this.yabUserStory.Controls.Add(this.txtUserStory);
			this.yabUserStory.Location = new System.Drawing.Point(4, 22);
			this.yabUserStory.Name = "yabUserStory";
			this.yabUserStory.Padding = new System.Windows.Forms.Padding(3);
			this.yabUserStory.Size = new System.Drawing.Size(519, 248);
			this.yabUserStory.TabIndex = 2;
			this.yabUserStory.Text = "Error Description";
			this.yabUserStory.UseVisualStyleBackColor = true;
			// 
			// txtUserStory
			// 
			this.txtUserStory.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtUserStory.Location = new System.Drawing.Point(3, 3);
			this.txtUserStory.Multiline = true;
			this.txtUserStory.Name = "txtUserStory";
			this.txtUserStory.Size = new System.Drawing.Size(513, 242);
			this.txtUserStory.TabIndex = 0;
			this.txtUserStory.Text = "\r\n### Expected behaviour\r\n(Enter text here)\r\n\r\n### Actual behaviour\r\n(Enter text " +
    "here)\r\n\r\n### Steps to reproduce the behaviour\r\n(Enter text here)";
			this.txtUserStory.TextChanged += new System.EventHandler(this.txtDesc_TextChanged);
			// 
			// cmdSubmitIssue
			// 
			this.cmdSubmitIssue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdSubmitIssue.Location = new System.Drawing.Point(348, 287);
			this.cmdSubmitIssue.Name = "cmdSubmitIssue";
			this.cmdSubmitIssue.Size = new System.Drawing.Size(90, 35);
			this.cmdSubmitIssue.TabIndex = 12;
			this.cmdSubmitIssue.Text = "Create New Issue";
			this.cmdSubmitIssue.UseVisualStyleBackColor = true;
			this.cmdSubmitIssue.Click += new System.EventHandler(this.cmdSubmitIssue_Click);
			// 
			// lblDescriptionWarning
			// 
			this.lblDescriptionWarning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lblDescriptionWarning.AutoSize = true;
			this.lblDescriptionWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblDescriptionWarning.Location = new System.Drawing.Point(8, 289);
			this.lblDescriptionWarning.Name = "lblDescriptionWarning";
			this.lblDescriptionWarning.Size = new System.Drawing.Size(221, 34);
			this.lblDescriptionWarning.TabIndex = 11;
			this.lblDescriptionWarning.Text = "Please describe the error on the \r\nDescription Tab before submitting";
			// 
			// frmCrashReporter
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ClientSize = new System.Drawing.Size(546, 348);
			this.Controls.Add(this.lblDescriptionWarning);
			this.Controls.Add(this.cmdSubmitIssue);
			this.Controls.Add(this.tabUserStory);
			this.Controls.Add(this.btnSend);
			this.Controls.Add(this.btnNo);
			this.Controls.Add(this.statusStrip1);
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(910, 577);
			this.MinimizeBox = false;
			this.Name = "frmCrashReporter";
			this.Text = "Chummer5a Crash Reporter";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmCrashReporter_FormClosing);
			this.Load += new System.EventHandler(this.frmCrashReporter_Load);
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.tabUserStory.ResumeLayout(false);
			this.tabGeneral.ResumeLayout(false);
			this.tabGeneral.PerformLayout();
			this.yabUserStory.ResumeLayout(false);
			this.yabUserStory.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel statusCollectionProgess;
		private System.Windows.Forms.Button btnNo;
		private System.Windows.Forms.Button btnSend;
		private System.Windows.Forms.Timer timerRefreshTextFile;
		private System.Windows.Forms.TabControl tabUserStory;
		private System.Windows.Forms.TabPage tabGeneral;
		private System.Windows.Forms.Button cmdSubmitIssue;
		private System.Windows.Forms.Label lblDescriptionWarning;
		private System.Windows.Forms.TabPage yabUserStory;
		private System.Windows.Forms.TextBox txtUserStory;
		private System.Windows.Forms.Label lblIntroText;
		private System.Windows.Forms.LinkLabel llblContents;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txtIdSelectable;
		private System.Windows.Forms.Label lblDesc;
	}
}

