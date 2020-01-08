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
            this.lblIntroText = new System.Windows.Forms.Label();
            this.llblContents = new System.Windows.Forms.LinkLabel();
            this.label3 = new System.Windows.Forms.Label();
            this.txtIdSelectable = new System.Windows.Forms.TextBox();
            this.lblDesc = new System.Windows.Forms.Label();
            this.yabUserStory = new System.Windows.Forms.TabPage();
            this.txtUserStory = new System.Windows.Forms.TextBox();
            this.cmdSubmitIssue = new System.Windows.Forms.Button();
            this.lblDescriptionWarning = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flpLeftSide = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.statusStrip1.SuspendLayout();
            this.tabUserStory.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.yabUserStory.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flpLeftSide.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusCollectionProgess});
            this.statusStrip1.Location = new System.Drawing.Point(9, 410);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(606, 22);
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
            this.btnNo.AutoSize = true;
            this.btnNo.Location = new System.Drawing.Point(215, 3);
            this.btnNo.MaximumSize = new System.Drawing.Size(100, 35);
            this.btnNo.Name = "btnNo";
            this.btnNo.Size = new System.Drawing.Size(100, 35);
            this.btnNo.TabIndex = 4;
            this.btnNo.Text = "Close";
            this.btnNo.UseVisualStyleBackColor = true;
            this.btnNo.Click += new System.EventHandler(this.btnNo_Click);
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.AutoSize = true;
            this.btnSend.Enabled = false;
            this.btnSend.Location = new System.Drawing.Point(3, 3);
            this.btnSend.MaximumSize = new System.Drawing.Size(100, 35);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(100, 35);
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
            this.tabUserStory.Location = new System.Drawing.Point(3, 3);
            this.tabUserStory.Name = "tabUserStory";
            this.tabUserStory.SelectedIndex = 0;
            this.tabUserStory.Size = new System.Drawing.Size(600, 351);
            this.tabUserStory.TabIndex = 11;
            // 
            // tabGeneral
            // 
            this.tabGeneral.BackColor = System.Drawing.SystemColors.Control;
            this.tabGeneral.Controls.Add(this.tableLayoutPanel2);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(592, 325);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            // 
            // lblIntroText
            // 
            this.lblIntroText.AutoSize = true;
            this.lblIntroText.Location = new System.Drawing.Point(5, 5);
            this.lblIntroText.Margin = new System.Windows.Forms.Padding(5);
            this.lblIntroText.Name = "lblIntroText";
            this.lblIntroText.Size = new System.Drawing.Size(561, 117);
            this.lblIntroText.TabIndex = 18;
            this.lblIntroText.Text = resources.GetString("lblIntroText.Text");
            // 
            // llblContents
            // 
            this.llblContents.AutoSize = true;
            this.llblContents.Location = new System.Drawing.Point(3, 130);
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
            this.label3.Location = new System.Drawing.Point(3, 149);
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
            this.txtIdSelectable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtIdSelectable.Location = new System.Drawing.Point(3, 172);
            this.txtIdSelectable.Name = "txtIdSelectable";
            this.txtIdSelectable.ReadOnly = true;
            this.txtIdSelectable.Size = new System.Drawing.Size(574, 13);
            this.txtIdSelectable.TabIndex = 20;
            this.txtIdSelectable.TabStop = false;
            this.txtIdSelectable.Text = "[ID]";
            // 
            // lblDesc
            // 
            this.lblDesc.AutoSize = true;
            this.lblDesc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDesc.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDesc.Location = new System.Drawing.Point(3, 191);
            this.lblDesc.Margin = new System.Windows.Forms.Padding(3);
            this.lblDesc.Name = "lblDesc";
            this.lblDesc.Size = new System.Drawing.Size(574, 118);
            this.lblDesc.TabIndex = 16;
            this.lblDesc.Text = "[DESC]";
            // 
            // yabUserStory
            // 
            this.yabUserStory.Controls.Add(this.txtUserStory);
            this.yabUserStory.Location = new System.Drawing.Point(4, 22);
            this.yabUserStory.Name = "yabUserStory";
            this.yabUserStory.Padding = new System.Windows.Forms.Padding(3);
            this.yabUserStory.Size = new System.Drawing.Size(592, 325);
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
            this.txtUserStory.Size = new System.Drawing.Size(586, 319);
            this.txtUserStory.TabIndex = 0;
            this.txtUserStory.Text = "\r\n### Expected behaviour\r\n(Enter text here)\r\n\r\n### Actual behaviour\r\n(Enter text " +
    "here)\r\n\r\n### Steps to reproduce the behaviour\r\n(Enter text here)";
            this.txtUserStory.TextChanged += new System.EventHandler(this.txtDesc_TextChanged);
            // 
            // cmdSubmitIssue
            // 
            this.cmdSubmitIssue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdSubmitIssue.AutoSize = true;
            this.cmdSubmitIssue.Location = new System.Drawing.Point(109, 3);
            this.cmdSubmitIssue.MaximumSize = new System.Drawing.Size(100, 35);
            this.cmdSubmitIssue.Name = "cmdSubmitIssue";
            this.cmdSubmitIssue.Size = new System.Drawing.Size(100, 35);
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
            this.lblDescriptionWarning.Location = new System.Drawing.Point(3, 0);
            this.lblDescriptionWarning.Name = "lblDescriptionWarning";
            this.lblDescriptionWarning.Size = new System.Drawing.Size(221, 34);
            this.lblDescriptionWarning.TabIndex = 11;
            this.lblDescriptionWarning.Text = "Please describe the error on the \r\nDescription Tab before submitting";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tabUserStory, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flpLeftSide, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(9, 9);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(606, 398);
            this.tableLayoutPanel1.TabIndex = 13;
            // 
            // flpLeftSide
            // 
            this.flpLeftSide.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.flpLeftSide.AutoSize = true;
            this.flpLeftSide.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpLeftSide.Controls.Add(this.lblDescriptionWarning);
            this.flpLeftSide.FlowDirection = System.Windows.Forms.FlowDirection.BottomUp;
            this.flpLeftSide.Location = new System.Drawing.Point(3, 361);
            this.flpLeftSide.Name = "flpLeftSide";
            this.flpLeftSide.Size = new System.Drawing.Size(227, 34);
            this.flpLeftSide.TabIndex = 12;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.btnNo);
            this.flowLayoutPanel1.Controls.Add(this.cmdSubmitIssue);
            this.flowLayoutPanel1.Controls.Add(this.btnSend);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(288, 357);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(318, 41);
            this.flowLayoutPanel1.TabIndex = 13;
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
            this.tableLayoutPanel2.Location = new System.Drawing.Point(6, 7);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 5;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(580, 312);
            this.tableLayoutPanel2.TabIndex = 21;
            // 
            // frmCrashReporter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmCrashReporter";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.Text = "Chummer5a Crash Reporter";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmCrashReporter_FormClosing);
            this.Load += new System.EventHandler(this.frmCrashReporter_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabUserStory.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.yabUserStory.ResumeLayout(false);
            this.yabUserStory.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flpLeftSide.ResumeLayout(false);
            this.flpLeftSide.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
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
        private System.Windows.Forms.FlowLayoutPanel flpLeftSide;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    }
}

