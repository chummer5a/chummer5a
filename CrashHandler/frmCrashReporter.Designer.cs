namespace CrashHandler
{
	public sealed partial class frmCrashReporter
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
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.lblDesc = new System.Windows.Forms.Label();
            this.txtIdSelectable = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.llblContents = new System.Windows.Forms.LinkLabel();
            this.lblIntroText = new System.Windows.Forms.Label();
            this.yabUserStory = new System.Windows.Forms.TabPage();
            this.txtUserStory = new System.Windows.Forms.TextBox();
            this.cmdSubmitIssue = new System.Windows.Forms.Button();
            this.lblDescriptionWarning = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.statusStrip1.SuspendLayout();
            this.tabUserStory.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.yabUserStory.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusCollectionProgess});
            this.statusStrip1.Location = new System.Drawing.Point(12, 506);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusStrip1.Size = new System.Drawing.Size(808, 26);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip";
            // 
            // statusCollectionProgess
            // 
            this.statusCollectionProgess.Name = "statusCollectionProgess";
            this.statusCollectionProgess.Size = new System.Drawing.Size(70, 20);
            this.statusCollectionProgess.Text = "Starting...";
            // 
            // btnNo
            // 
            this.btnNo.AutoSize = true;
            this.btnNo.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnNo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnNo.Location = new System.Drawing.Point(4, 4);
            this.btnNo.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnNo.MaximumSize = new System.Drawing.Size(133, 43);
            this.btnNo.Name = "btnNo";
            this.btnNo.Size = new System.Drawing.Size(132, 43);
            this.btnNo.TabIndex = 4;
            this.btnNo.Text = "Close";
            this.btnNo.UseVisualStyleBackColor = true;
            this.btnNo.Click += new System.EventHandler(this.btnNo_Click);
            // 
            // btnSend
            // 
            this.btnSend.AutoSize = true;
            this.btnSend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSend.Enabled = false;
            this.btnSend.Location = new System.Drawing.Point(285, 4);
            this.btnSend.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSend.MaximumSize = new System.Drawing.Size(133, 43);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(133, 43);
            this.btnSend.TabIndex = 5;
            this.btnSend.Text = "Send Error Report and Close";
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
            this.tableLayoutPanel1.SetColumnSpan(this.tabUserStory, 2);
            this.tabUserStory.Controls.Add(this.tabGeneral);
            this.tabUserStory.Controls.Add(this.yabUserStory);
            this.tabUserStory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabUserStory.Location = new System.Drawing.Point(4, 4);
            this.tabUserStory.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabUserStory.Name = "tabUserStory";
            this.tabUserStory.SelectedIndex = 0;
            this.tabUserStory.Size = new System.Drawing.Size(800, 428);
            this.tabUserStory.TabIndex = 11;
            // 
            // tabGeneral
            // 
            this.tabGeneral.BackColor = System.Drawing.SystemColors.Control;
            this.tabGeneral.Controls.Add(this.tableLayoutPanel2);
            this.tabGeneral.Location = new System.Drawing.Point(4, 25);
            this.tabGeneral.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabGeneral.Size = new System.Drawing.Size(792, 399);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.lblDesc, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.txtIdSelectable, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.llblContents, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.lblIntroText, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(4, 4);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 5;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(784, 391);
            this.tableLayoutPanel2.TabIndex = 21;
            // 
            // lblDesc
            // 
            this.lblDesc.AutoSize = true;
            this.lblDesc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDesc.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDesc.Location = new System.Drawing.Point(4, 268);
            this.lblDesc.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.lblDesc.Name = "lblDesc";
            this.lblDesc.Size = new System.Drawing.Size(776, 116);
            this.lblDesc.TabIndex = 16;
            this.lblDesc.Text = "[DESC]";
            // 
            // txtIdSelectable
            // 
            this.txtIdSelectable.BackColor = System.Drawing.SystemColors.Control;
            this.txtIdSelectable.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtIdSelectable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtIdSelectable.Location = new System.Drawing.Point(4, 239);
            this.txtIdSelectable.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.txtIdSelectable.Name = "txtIdSelectable";
            this.txtIdSelectable.ReadOnly = true;
            this.txtIdSelectable.Size = new System.Drawing.Size(776, 15);
            this.txtIdSelectable.TabIndex = 20;
            this.txtIdSelectable.TabStop = false;
            this.txtIdSelectable.Text = "[ID]";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(4, 205);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 20);
            this.label3.TabIndex = 17;
            this.label3.Text = "Details:";
            // 
            // llblContents
            // 
            this.llblContents.AutoSize = true;
            this.llblContents.Location = new System.Drawing.Point(4, 174);
            this.llblContents.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.llblContents.Name = "llblContents";
            this.llblContents.Size = new System.Drawing.Size(200, 17);
            this.llblContents.TabIndex = 19;
            this.llblContents.TabStop = true;
            this.llblContents.Text = "What does the report contain?";
            this.llblContents.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llblContents_LinkClicked);
            // 
            // lblIntroText
            // 
            this.lblIntroText.AutoSize = true;
            this.lblIntroText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblIntroText.Location = new System.Drawing.Point(4, 7);
            this.lblIntroText.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.lblIntroText.Name = "lblIntroText";
            this.lblIntroText.Size = new System.Drawing.Size(776, 153);
            this.lblIntroText.TabIndex = 18;
            this.lblIntroText.Text = resources.GetString("lblIntroText.Text");
            // 
            // yabUserStory
            // 
            this.yabUserStory.Controls.Add(this.txtUserStory);
            this.yabUserStory.Location = new System.Drawing.Point(4, 25);
            this.yabUserStory.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.yabUserStory.Name = "yabUserStory";
            this.yabUserStory.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.yabUserStory.Size = new System.Drawing.Size(792, 389);
            this.yabUserStory.TabIndex = 2;
            this.yabUserStory.Text = "Error Description";
            this.yabUserStory.UseVisualStyleBackColor = true;
            // 
            // txtUserStory
            // 
            this.txtUserStory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtUserStory.Location = new System.Drawing.Point(4, 4);
            this.txtUserStory.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtUserStory.Multiline = true;
            this.txtUserStory.Name = "txtUserStory";
            this.txtUserStory.Size = new System.Drawing.Size(784, 381);
            this.txtUserStory.TabIndex = 0;
            this.txtUserStory.Text = "\r\n### Expected behaviour\r\n(Enter text here)\r\n\r\n### Actual behaviour\r\n(Enter text " +
    "here)\r\n\r\n### Steps to reproduce the behaviour\r\n(Enter text here)";
            this.txtUserStory.TextChanged += new System.EventHandler(this.txtDesc_TextChanged);
            // 
            // cmdSubmitIssue
            // 
            this.cmdSubmitIssue.AutoSize = true;
            this.cmdSubmitIssue.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdSubmitIssue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdSubmitIssue.Location = new System.Drawing.Point(144, 4);
            this.cmdSubmitIssue.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cmdSubmitIssue.MaximumSize = new System.Drawing.Size(133, 43);
            this.cmdSubmitIssue.Name = "cmdSubmitIssue";
            this.cmdSubmitIssue.Size = new System.Drawing.Size(133, 43);
            this.cmdSubmitIssue.TabIndex = 12;
            this.cmdSubmitIssue.Text = "Create New Issue";
            this.cmdSubmitIssue.UseVisualStyleBackColor = true;
            this.cmdSubmitIssue.Click += new System.EventHandler(this.cmdSubmitIssue_Click);
            // 
            // lblDescriptionWarning
            // 
            this.lblDescriptionWarning.AutoSize = true;
            this.lblDescriptionWarning.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblDescriptionWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDescriptionWarning.Location = new System.Drawing.Point(4, 443);
            this.lblDescriptionWarning.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.lblDescriptionWarning.Name = "lblDescriptionWarning";
            this.lblDescriptionWarning.Size = new System.Drawing.Size(377, 40);
            this.lblDescriptionWarning.TabIndex = 11;
            this.lblDescriptionWarning.Text = "Please describe the error on the \r\nDescription Tab before submitting";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.tabUserStory, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblDescriptionWarning, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 11);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 31);
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(808, 521);
            this.tableLayoutPanel1.TabIndex = 13;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.ColumnCount = 3;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel3.Controls.Add(this.btnSend, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.cmdSubmitIssue, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnNo, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(385, 439);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(423, 51);
            this.tableLayoutPanel3.TabIndex = 14;
            // 
            // frmCrashReporter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(832, 543);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmCrashReporter";
            this.Padding = new System.Windows.Forms.Padding(12, 11, 12, 11);
            this.Text = "Chummer5a Crash Reporter";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmCrashReporter_FormClosing);
            this.Load += new System.EventHandler(this.frmCrashReporter_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabUserStory.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.yabUserStory.ResumeLayout(false);
            this.yabUserStory.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
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
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
    }
}

