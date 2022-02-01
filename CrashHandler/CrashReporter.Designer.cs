namespace CrashHandler
{
	public sealed partial class CrashReporter
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CrashReporter));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusCollectionProgess = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnNo = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.tabUserStory = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.tlpUserStory = new System.Windows.Forms.TableLayoutPanel();
            this.txtIdSelectable = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lblContents = new System.Windows.Forms.LinkLabel();
            this.lblIntroText = new System.Windows.Forms.Label();
            this.pnlDesc = new System.Windows.Forms.Panel();
            this.lblDesc = new System.Windows.Forms.Label();
            this.yabUserStory = new System.Windows.Forms.TabPage();
            this.txtUserStory = new System.Windows.Forms.TextBox();
            this.cmdSubmitIssue = new System.Windows.Forms.Button();
            this.lblDescriptionWarning = new System.Windows.Forms.Label();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.tlpButtons = new System.Windows.Forms.TableLayoutPanel();
            this.txtIdSelectable2 = new System.Windows.Forms.TextBox();
            this.statusStrip1.SuspendLayout();
            this.tabUserStory.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.tlpUserStory.SuspendLayout();
            this.pnlDesc.SuspendLayout();
            this.yabUserStory.SuspendLayout();
            this.tlpMain.SuspendLayout();
            this.tlpButtons.SuspendLayout();
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
            this.btnNo.Margin = new System.Windows.Forms.Padding(4);
            this.btnNo.MaximumSize = new System.Drawing.Size(133, 43);
            this.btnNo.Name = "btnNo";
            this.btnNo.Size = new System.Drawing.Size(132, 38);
            this.btnNo.TabIndex = 4;
            this.btnNo.Text = "Close without Reporting";
            this.btnNo.UseVisualStyleBackColor = true;
            this.btnNo.Click += new System.EventHandler(this.btnNo_Click);
            // 
            // btnSend
            // 
            this.btnSend.AutoSize = true;
            this.btnSend.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSend.Enabled = false;
            this.btnSend.Location = new System.Drawing.Point(285, 4);
            this.btnSend.Margin = new System.Windows.Forms.Padding(4);
            this.btnSend.MaximumSize = new System.Drawing.Size(133, 43);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(133, 38);
            this.btnSend.TabIndex = 5;
            this.btnSend.Text = "Send Error Report and Close";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // tabUserStory
            // 
            this.tlpMain.SetColumnSpan(this.tabUserStory, 2);
            this.tabUserStory.Controls.Add(this.tabGeneral);
            this.tabUserStory.Controls.Add(this.yabUserStory);
            this.tabUserStory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabUserStory.Location = new System.Drawing.Point(4, 4);
            this.tabUserStory.Margin = new System.Windows.Forms.Padding(4);
            this.tabUserStory.Name = "tabUserStory";
            this.tabUserStory.SelectedIndex = 0;
            this.tabUserStory.Size = new System.Drawing.Size(800, 428);
            this.tabUserStory.TabIndex = 11;
            // 
            // tabGeneral
            // 
            this.tabGeneral.BackColor = System.Drawing.SystemColors.Control;
            this.tabGeneral.Controls.Add(this.tlpUserStory);
            this.tabGeneral.Location = new System.Drawing.Point(4, 25);
            this.tabGeneral.Margin = new System.Windows.Forms.Padding(4);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(4);
            this.tabGeneral.Size = new System.Drawing.Size(792, 399);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            // 
            // tlpUserStory
            // 
            this.tlpUserStory.ColumnCount = 1;
            this.tlpUserStory.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpUserStory.Controls.Add(this.txtIdSelectable2, 0, 4);
            this.tlpUserStory.Controls.Add(this.txtIdSelectable, 0, 3);
            this.tlpUserStory.Controls.Add(this.label3, 0, 2);
            this.tlpUserStory.Controls.Add(this.lblContents, 0, 1);
            this.tlpUserStory.Controls.Add(this.lblIntroText, 0, 0);
            this.tlpUserStory.Controls.Add(this.pnlDesc, 0, 5);
            this.tlpUserStory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpUserStory.Location = new System.Drawing.Point(4, 4);
            this.tlpUserStory.Margin = new System.Windows.Forms.Padding(4);
            this.tlpUserStory.Name = "tlpUserStory";
            this.tlpUserStory.RowCount = 6;
            this.tlpUserStory.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpUserStory.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpUserStory.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpUserStory.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpUserStory.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpUserStory.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpUserStory.Size = new System.Drawing.Size(784, 391);
            this.tlpUserStory.TabIndex = 21;
            // 
            // txtIdSelectable
            // 
            this.txtIdSelectable.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txtIdSelectable.BackColor = System.Drawing.SystemColors.Control;
            this.txtIdSelectable.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtIdSelectable.Location = new System.Drawing.Point(8, 213);
            this.txtIdSelectable.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.txtIdSelectable.Multiline = true;
            this.txtIdSelectable.Name = "txtIdSelectable";
            this.txtIdSelectable.ReadOnly = true;
            this.txtIdSelectable.Size = new System.Drawing.Size(765, 13);
            this.txtIdSelectable.TabIndex = 20;
            this.txtIdSelectable.TabStop = false;
            this.txtIdSelectable.Text = "[ID]";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(4, 179);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 20);
            this.label3.TabIndex = 17;
            this.label3.Text = "Details:";
            // 
            // lblContents
            // 
            this.lblContents.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblContents.AutoSize = true;
            this.lblContents.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblContents.Location = new System.Drawing.Point(4, 149);
            this.lblContents.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.lblContents.Name = "lblContents";
            this.lblContents.Size = new System.Drawing.Size(184, 16);
            this.lblContents.TabIndex = 19;
            this.lblContents.TabStop = true;
            this.lblContents.Text = "What does the report contain?";
            this.lblContents.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblContents_LinkClicked);
            // 
            // lblIntroText
            // 
            this.lblIntroText.AutoSize = true;
            this.lblIntroText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblIntroText.Location = new System.Drawing.Point(4, 7);
            this.lblIntroText.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.lblIntroText.Name = "lblIntroText";
            this.lblIntroText.Size = new System.Drawing.Size(776, 128);
            this.lblIntroText.TabIndex = 18;
            this.lblIntroText.Text = resources.GetString("lblIntroText.Text");
            // 
            // pnlDesc
            // 
            this.pnlDesc.AutoScroll = true;
            this.pnlDesc.Controls.Add(this.lblDesc);
            this.pnlDesc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDesc.Location = new System.Drawing.Point(0, 260);
            this.pnlDesc.Margin = new System.Windows.Forms.Padding(0);
            this.pnlDesc.Name = "pnlDesc";
            this.pnlDesc.Padding = new System.Windows.Forms.Padding(4, 7, 17, 4);
            this.pnlDesc.Size = new System.Drawing.Size(784, 131);
            this.pnlDesc.TabIndex = 21;
            // 
            // lblDesc
            // 
            this.lblDesc.AutoSize = true;
            this.lblDesc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDesc.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDesc.Location = new System.Drawing.Point(4, 7);
            this.lblDesc.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.lblDesc.Name = "lblDesc";
            this.lblDesc.Size = new System.Drawing.Size(53, 17);
            this.lblDesc.TabIndex = 16;
            this.lblDesc.Text = "[DESC]";
            // 
            // yabUserStory
            // 
            this.yabUserStory.Controls.Add(this.txtUserStory);
            this.yabUserStory.Location = new System.Drawing.Point(4, 25);
            this.yabUserStory.Margin = new System.Windows.Forms.Padding(4);
            this.yabUserStory.Name = "yabUserStory";
            this.yabUserStory.Padding = new System.Windows.Forms.Padding(4);
            this.yabUserStory.Size = new System.Drawing.Size(792, 399);
            this.yabUserStory.TabIndex = 2;
            this.yabUserStory.Text = "Error Description";
            this.yabUserStory.UseVisualStyleBackColor = true;
            // 
            // txtUserStory
            // 
            this.txtUserStory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtUserStory.Location = new System.Drawing.Point(4, 4);
            this.txtUserStory.Margin = new System.Windows.Forms.Padding(4);
            this.txtUserStory.Multiline = true;
            this.txtUserStory.Name = "txtUserStory";
            this.txtUserStory.Size = new System.Drawing.Size(784, 391);
            this.txtUserStory.TabIndex = 0;
            this.txtUserStory.Text = "### Expected behaviour\r\n(Enter text here)\r\n\r\n### Actual behaviour\r\n(Enter text he" +
    "re)\r\n\r\n### Steps to reproduce the behaviour\r\n(Enter text here)";
            this.txtUserStory.TextChanged += new System.EventHandler(this.txtDesc_TextChanged);
            // 
            // cmdSubmitIssue
            // 
            this.cmdSubmitIssue.AutoSize = true;
            this.cmdSubmitIssue.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdSubmitIssue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdSubmitIssue.Enabled = false;
            this.cmdSubmitIssue.Location = new System.Drawing.Point(144, 4);
            this.cmdSubmitIssue.Margin = new System.Windows.Forms.Padding(4);
            this.cmdSubmitIssue.MaximumSize = new System.Drawing.Size(133, 43);
            this.cmdSubmitIssue.Name = "cmdSubmitIssue";
            this.cmdSubmitIssue.Size = new System.Drawing.Size(133, 38);
            this.cmdSubmitIssue.TabIndex = 12;
            this.cmdSubmitIssue.Text = "Create New Issue on GitHub";
            this.cmdSubmitIssue.UseVisualStyleBackColor = true;
            this.cmdSubmitIssue.Click += new System.EventHandler(this.cmdSubmitIssue_Click);
            // 
            // lblDescriptionWarning
            // 
            this.lblDescriptionWarning.AutoSize = true;
            this.lblDescriptionWarning.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDescriptionWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDescriptionWarning.Location = new System.Drawing.Point(4, 443);
            this.lblDescriptionWarning.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.lblDescriptionWarning.Name = "lblDescriptionWarning";
            this.lblDescriptionWarning.Size = new System.Drawing.Size(369, 40);
            this.lblDescriptionWarning.TabIndex = 11;
            this.lblDescriptionWarning.Text = "Please describe the error on the \r\nDescription Tab before submitting";
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.tabUserStory, 0, 0);
            this.tlpMain.Controls.Add(this.lblDescriptionWarning, 0, 1);
            this.tlpMain.Controls.Add(this.tlpButtons, 1, 1);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(12, 11);
            this.tlpMain.Margin = new System.Windows.Forms.Padding(4);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.Padding = new System.Windows.Forms.Padding(0, 0, 0, 31);
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(808, 521);
            this.tlpMain.TabIndex = 13;
            // 
            // tlpButtons
            // 
            this.tlpButtons.AutoSize = true;
            this.tlpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpButtons.ColumnCount = 3;
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tlpButtons.Controls.Add(this.btnSend, 2, 0);
            this.tlpButtons.Controls.Add(this.cmdSubmitIssue, 1, 0);
            this.tlpButtons.Controls.Add(this.btnNo, 0, 0);
            this.tlpButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpButtons.Location = new System.Drawing.Point(381, 440);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(4);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButtons.Size = new System.Drawing.Size(423, 46);
            this.tlpButtons.TabIndex = 14;
            // 
            // txtIdSelectable2
            // 
            this.txtIdSelectable2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txtIdSelectable2.BackColor = System.Drawing.SystemColors.Control;
            this.txtIdSelectable2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtIdSelectable2.Location = new System.Drawing.Point(8, 240);
            this.txtIdSelectable2.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.txtIdSelectable2.Multiline = true;
            this.txtIdSelectable2.Name = "txtIdSelectable2";
            this.txtIdSelectable2.ReadOnly = true;
            this.txtIdSelectable2.Size = new System.Drawing.Size(765, 13);
            this.txtIdSelectable2.TabIndex = 22;
            this.txtIdSelectable2.TabStop = false;
            this.txtIdSelectable2.Text = "[ID]";
            // 
            // CrashReporter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(832, 543);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tlpMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CrashReporter";
            this.Padding = new System.Windows.Forms.Padding(12, 11, 12, 11);
            this.Text = "Chummer5a Crash Reporter";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmCrashReporter_FormClosing);
            this.Load += new System.EventHandler(this.frmCrashReporter_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabUserStory.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tlpUserStory.ResumeLayout(false);
            this.tlpUserStory.PerformLayout();
            this.pnlDesc.ResumeLayout(false);
            this.pnlDesc.PerformLayout();
            this.yabUserStory.ResumeLayout(false);
            this.yabUserStory.PerformLayout();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel statusCollectionProgess;
		private System.Windows.Forms.Button btnNo;
		private System.Windows.Forms.Button btnSend;
		private System.Windows.Forms.TabControl tabUserStory;
		private System.Windows.Forms.TabPage tabGeneral;
		private System.Windows.Forms.Button cmdSubmitIssue;
		private System.Windows.Forms.Label lblDescriptionWarning;
		private System.Windows.Forms.TabPage yabUserStory;
		private System.Windows.Forms.TextBox txtUserStory;
		private System.Windows.Forms.Label lblIntroText;
		private System.Windows.Forms.LinkLabel lblContents;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txtIdSelectable;
		private System.Windows.Forms.Label lblDesc;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.TableLayoutPanel tlpUserStory;
        private System.Windows.Forms.TableLayoutPanel tlpButtons;
        private System.Windows.Forms.Panel pnlDesc;
        private System.Windows.Forms.TextBox txtIdSelectable2;
    }
}

