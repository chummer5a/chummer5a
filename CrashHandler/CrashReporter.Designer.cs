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
            this.stpMain = new System.Windows.Forms.StatusStrip();
            this.tslStatusCollectionProgess = new System.Windows.Forms.ToolStripStatusLabel();
            this.cmdClose = new System.Windows.Forms.Button();
            this.tabUserStories = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.tlpUserStory = new System.Windows.Forms.TableLayoutPanel();
            this.txtIdSelectable2 = new System.Windows.Forms.TextBox();
            this.txtIdSelectable = new System.Windows.Forms.TextBox();
            this.lblDetails = new System.Windows.Forms.Label();
            this.lblContents = new System.Windows.Forms.LinkLabel();
            this.lblIntroText = new System.Windows.Forms.Label();
            this.tabUserStory = new System.Windows.Forms.TabPage();
            this.rtbUserStory = new System.Windows.Forms.RichTextBox();
            this.cmdSubmitIssue = new System.Windows.Forms.Button();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.tlpButtons = new System.Windows.Forms.TableLayoutPanel();
            this.cmdRestart = new System.Windows.Forms.Button();
            this.lblDescriptionWarning = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.cmdReportToDiscord = new System.Windows.Forms.Button();
            this.stpMain.SuspendLayout();
            this.tabUserStories.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.tlpUserStory.SuspendLayout();
            this.tabUserStory.SuspendLayout();
            this.tlpMain.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // stpMain
            // 
            this.stpMain.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.stpMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslStatusCollectionProgess});
            this.stpMain.Location = new System.Drawing.Point(0, 539);
            this.stpMain.Name = "stpMain";
            this.stpMain.Size = new System.Drawing.Size(784, 22);
            this.stpMain.TabIndex = 0;
            this.stpMain.Text = "statusStrip";
            // 
            // tslStatusCollectionProgess
            // 
            this.tslStatusCollectionProgess.Name = "tslStatusCollectionProgess";
            this.tslStatusCollectionProgess.Size = new System.Drawing.Size(57, 17);
            this.tslStatusCollectionProgess.Text = "Starting...";
            // 
            // cmdClose
            // 
            this.cmdClose.AutoSize = true;
            this.cmdClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdClose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdClose.Location = new System.Drawing.Point(345, 3);
            this.cmdClose.MinimumSize = new System.Drawing.Size(80, 0);
            this.cmdClose.Name = "cmdClose";
            this.cmdClose.Size = new System.Drawing.Size(108, 36);
            this.cmdClose.TabIndex = 4;
            this.cmdClose.Text = "Close Reporter and\r\nChummer";
            this.cmdClose.UseVisualStyleBackColor = true;
            this.cmdClose.Click += new System.EventHandler(this.cmdClose_Click);
            // 
            // tabUserStories
            // 
            this.tlpMain.SetColumnSpan(this.tabUserStories, 2);
            this.tabUserStories.Controls.Add(this.tabGeneral);
            this.tabUserStories.Controls.Add(this.tabUserStory);
            this.tabUserStories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabUserStories.Location = new System.Drawing.Point(3, 3);
            this.tabUserStories.Name = "tabUserStories";
            this.tabUserStories.SelectedIndex = 0;
            this.tabUserStories.Size = new System.Drawing.Size(778, 488);
            this.tabUserStories.TabIndex = 11;
            // 
            // tabGeneral
            // 
            this.tabGeneral.BackColor = System.Drawing.SystemColors.Control;
            this.tabGeneral.Controls.Add(this.tlpUserStory);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(770, 462);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            // 
            // tlpUserStory
            // 
            this.tlpUserStory.ColumnCount = 2;
            this.tlpUserStory.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpUserStory.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpUserStory.Controls.Add(this.txtDescription, 0, 4);
            this.tlpUserStory.Controls.Add(this.txtIdSelectable2, 1, 3);
            this.tlpUserStory.Controls.Add(this.txtIdSelectable, 0, 3);
            this.tlpUserStory.Controls.Add(this.lblDetails, 0, 2);
            this.tlpUserStory.Controls.Add(this.lblContents, 0, 1);
            this.tlpUserStory.Controls.Add(this.lblIntroText, 0, 0);
            this.tlpUserStory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpUserStory.Location = new System.Drawing.Point(3, 3);
            this.tlpUserStory.Name = "tlpUserStory";
            this.tlpUserStory.RowCount = 5;
            this.tlpUserStory.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpUserStory.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpUserStory.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpUserStory.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpUserStory.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpUserStory.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpUserStory.Size = new System.Drawing.Size(764, 456);
            this.tlpUserStory.TabIndex = 21;
            // 
            // txtIdSelectable2
            // 
            this.txtIdSelectable2.BackColor = System.Drawing.SystemColors.Control;
            this.txtIdSelectable2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtIdSelectable2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtIdSelectable2.Location = new System.Drawing.Point(388, 266);
            this.txtIdSelectable2.Margin = new System.Windows.Forms.Padding(6);
            this.txtIdSelectable2.Multiline = true;
            this.txtIdSelectable2.Name = "txtIdSelectable2";
            this.txtIdSelectable2.ReadOnly = true;
            this.txtIdSelectable2.Size = new System.Drawing.Size(370, 11);
            this.txtIdSelectable2.TabIndex = 22;
            this.txtIdSelectable2.TabStop = false;
            this.txtIdSelectable2.Text = "[ID]";
            // 
            // txtIdSelectable
            // 
            this.txtIdSelectable.BackColor = System.Drawing.SystemColors.Control;
            this.txtIdSelectable.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtIdSelectable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtIdSelectable.Location = new System.Drawing.Point(6, 266);
            this.txtIdSelectable.Margin = new System.Windows.Forms.Padding(6);
            this.txtIdSelectable.Multiline = true;
            this.txtIdSelectable.Name = "txtIdSelectable";
            this.txtIdSelectable.ReadOnly = true;
            this.txtIdSelectable.Size = new System.Drawing.Size(370, 11);
            this.txtIdSelectable.TabIndex = 20;
            this.txtIdSelectable.TabStop = false;
            this.txtIdSelectable.Text = "[ID]";
            // 
            // lblDetails
            // 
            this.lblDetails.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDetails.AutoSize = true;
            this.tlpUserStory.SetColumnSpan(this.lblDetails, 2);
            this.lblDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDetails.Location = new System.Drawing.Point(3, 237);
            this.lblDetails.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDetails.Name = "lblDetails";
            this.lblDetails.Size = new System.Drawing.Size(287, 17);
            this.lblDetails.TabIndex = 17;
            this.lblDetails.Text = "Details (Can Be Selected and Copied):";
            // 
            // lblContents
            // 
            this.lblContents.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblContents.AutoSize = true;
            this.tlpUserStory.SetColumnSpan(this.lblContents, 2);
            this.lblContents.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblContents.Enabled = false;
            this.lblContents.Location = new System.Drawing.Point(3, 206);
            this.lblContents.Margin = new System.Windows.Forms.Padding(3, 12, 3, 12);
            this.lblContents.Name = "lblContents";
            this.lblContents.Size = new System.Drawing.Size(448, 13);
            this.lblContents.TabIndex = 19;
            this.lblContents.TabStop = true;
            this.lblContents.Text = "The .zip archive containing all anonymized crash information is being generated, " +
    "please wait...";
            this.lblContents.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblContents_LinkClicked);
            // 
            // lblIntroText
            // 
            this.lblIntroText.AutoSize = true;
            this.tlpUserStory.SetColumnSpan(this.lblIntroText, 2);
            this.lblIntroText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblIntroText.Location = new System.Drawing.Point(3, 6);
            this.lblIntroText.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblIntroText.Name = "lblIntroText";
            this.lblIntroText.Size = new System.Drawing.Size(758, 182);
            this.lblIntroText.TabIndex = 18;
            this.lblIntroText.Text = resources.GetString("lblIntroText.Text");
            // 
            // tabUserStory
            // 
            this.tabUserStory.Controls.Add(this.rtbUserStory);
            this.tabUserStory.Location = new System.Drawing.Point(4, 22);
            this.tabUserStory.Name = "tabUserStory";
            this.tabUserStory.Padding = new System.Windows.Forms.Padding(3);
            this.tabUserStory.Size = new System.Drawing.Size(770, 462);
            this.tabUserStory.TabIndex = 2;
            this.tabUserStory.Text = "Error Description";
            this.tabUserStory.UseVisualStyleBackColor = true;
            // 
            // rtbUserStory
            // 
            this.rtbUserStory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbUserStory.Location = new System.Drawing.Point(3, 3);
            this.rtbUserStory.Name = "rtbUserStory";
            this.rtbUserStory.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbUserStory.Size = new System.Drawing.Size(764, 456);
            this.rtbUserStory.TabIndex = 1;
            this.rtbUserStory.Text = "### Expected behaviour\n(Enter text here)\n\n### Actual behaviour\n(Enter text here)\n" +
    "\n### Steps to reproduce the behaviour\n(Enter text here)\n";
            this.rtbUserStory.TextChanged += new System.EventHandler(this.rtbUserStory_TextChanged);
            // 
            // cmdSubmitIssue
            // 
            this.cmdSubmitIssue.AutoSize = true;
            this.cmdSubmitIssue.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdSubmitIssue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdSubmitIssue.Enabled = false;
            this.cmdSubmitIssue.Location = new System.Drawing.Point(117, 3);
            this.cmdSubmitIssue.MinimumSize = new System.Drawing.Size(80, 0);
            this.cmdSubmitIssue.Name = "cmdSubmitIssue";
            this.cmdSubmitIssue.Size = new System.Drawing.Size(108, 36);
            this.cmdSubmitIssue.TabIndex = 12;
            this.cmdSubmitIssue.Text = "Create New Issue\r\non GitHub";
            this.cmdSubmitIssue.UseVisualStyleBackColor = true;
            this.cmdSubmitIssue.Click += new System.EventHandler(this.cmdSubmitIssue_Click);
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.tabUserStories, 0, 0);
            this.tlpMain.Controls.Add(this.tlpButtons, 1, 1);
            this.tlpMain.Controls.Add(this.lblDescriptionWarning, 0, 1);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.Padding = new System.Windows.Forms.Padding(0, 0, 0, 25);
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(784, 561);
            this.tlpMain.TabIndex = 13;
            // 
            // tlpButtons
            // 
            this.tlpButtons.AutoSize = true;
            this.tlpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpButtons.ColumnCount = 4;
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpButtons.Controls.Add(this.cmdReportToDiscord, 0, 0);
            this.tlpButtons.Controls.Add(this.cmdRestart, 1, 0);
            this.tlpButtons.Controls.Add(this.cmdSubmitIssue, 1, 0);
            this.tlpButtons.Controls.Add(this.cmdClose, 3, 0);
            this.tlpButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpButtons.Location = new System.Drawing.Point(328, 494);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButtons.Size = new System.Drawing.Size(456, 42);
            this.tlpButtons.TabIndex = 14;
            // 
            // cmdRestart
            // 
            this.cmdRestart.AutoSize = true;
            this.cmdRestart.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRestart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdRestart.Location = new System.Drawing.Point(231, 3);
            this.cmdRestart.MinimumSize = new System.Drawing.Size(80, 0);
            this.cmdRestart.Name = "cmdRestart";
            this.cmdRestart.Size = new System.Drawing.Size(108, 36);
            this.cmdRestart.TabIndex = 13;
            this.cmdRestart.Text = "Close Reporter and\r\nRestart Chummer";
            this.cmdRestart.UseVisualStyleBackColor = true;
            this.cmdRestart.Click += new System.EventHandler(this.cmdRestart_Click);
            // 
            // lblDescriptionWarning
            // 
            this.lblDescriptionWarning.AutoSize = true;
            this.lblDescriptionWarning.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDescriptionWarning.Location = new System.Drawing.Point(3, 500);
            this.lblDescriptionWarning.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDescriptionWarning.Name = "lblDescriptionWarning";
            this.lblDescriptionWarning.Size = new System.Drawing.Size(322, 30);
            this.lblDescriptionWarning.TabIndex = 15;
            this.lblDescriptionWarning.Text = "Please make sure you have written some useful information in the Error Descriptio" +
    "n before creating an issue on GitHub.";
            // 
            // txtDescription
            // 
            this.txtDescription.BackColor = System.Drawing.SystemColors.Control;
            this.txtDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tlpUserStory.SetColumnSpan(this.txtDescription, 2);
            this.txtDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDescription.Location = new System.Drawing.Point(6, 289);
            this.txtDescription.Margin = new System.Windows.Forms.Padding(6);
            this.txtDescription.MaxLength = 2147483647;
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ReadOnly = true;
            this.txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDescription.Size = new System.Drawing.Size(752, 161);
            this.txtDescription.TabIndex = 25;
            this.txtDescription.TabStop = false;
            this.txtDescription.Text = "[DESC]";
            // 
            // cmdReportToDiscord
            // 
            this.cmdReportToDiscord.AutoSize = true;
            this.cmdReportToDiscord.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdReportToDiscord.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdReportToDiscord.Location = new System.Drawing.Point(3, 3);
            this.cmdReportToDiscord.MinimumSize = new System.Drawing.Size(80, 0);
            this.cmdReportToDiscord.Name = "cmdReportToDiscord";
            this.cmdReportToDiscord.Size = new System.Drawing.Size(108, 36);
            this.cmdReportToDiscord.TabIndex = 14;
            this.cmdReportToDiscord.Text = "Report to Chummer\r\nDiscord Server";
            this.cmdReportToDiscord.UseVisualStyleBackColor = true;
            this.cmdReportToDiscord.Click += new System.EventHandler(this.cmdReportToDiscord_Click);
            // 
            // CrashReporter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.stpMain);
            this.Controls.Add(this.tlpMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CrashReporter";
            this.Text = "Chummer5a Crash Reporter";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmCrashReporter_FormClosing);
            this.Load += new System.EventHandler(this.frmCrashReporter_Load);
            this.stpMain.ResumeLayout(false);
            this.stpMain.PerformLayout();
            this.tabUserStories.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tlpUserStory.ResumeLayout(false);
            this.tlpUserStory.PerformLayout();
            this.tabUserStory.ResumeLayout(false);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.StatusStrip stpMain;
		private System.Windows.Forms.ToolStripStatusLabel tslStatusCollectionProgess;
		private System.Windows.Forms.Button cmdClose;
		private System.Windows.Forms.TabControl tabUserStories;
		private System.Windows.Forms.TabPage tabGeneral;
		private System.Windows.Forms.Button cmdSubmitIssue;
		private System.Windows.Forms.TabPage tabUserStory;
		private System.Windows.Forms.Label lblIntroText;
		private System.Windows.Forms.LinkLabel lblContents;
		private System.Windows.Forms.Label lblDetails;
		private System.Windows.Forms.TextBox txtIdSelectable;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.TableLayoutPanel tlpUserStory;
        private System.Windows.Forms.TableLayoutPanel tlpButtons;
        private System.Windows.Forms.TextBox txtIdSelectable2;
        private System.Windows.Forms.Button cmdRestart;
        private System.Windows.Forms.Label lblDescriptionWarning;
        private System.Windows.Forms.RichTextBox rtbUserStory;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Button cmdReportToDiscord;
    }
}

