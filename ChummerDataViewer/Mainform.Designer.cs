namespace ChummerDataViewer
{
	partial class Mainform
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
			this.tsBackground = new System.Windows.Forms.ToolStrip();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteDatabaserequiresRestartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.txtSearch = new System.Windows.Forms.TextBox();
			this.cboAutomation = new System.Windows.Forms.ComboBox();
			this.tabsContainer = new System.Windows.Forms.TabControl();
			this.tabReports = new System.Windows.Forms.TabPage();
			this.cboBuild = new System.Windows.Forms.ComboBox();
			this.cboVersion = new System.Windows.Forms.ComboBox();
			this.tabStats = new System.Windows.Forms.TabPage();
			this.menuStrip1.SuspendLayout();
			this.tabsContainer.SuspendLayout();
			this.tabReports.SuspendLayout();
			this.SuspendLayout();
			// 
			// tsBackground
			// 
			this.tsBackground.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.tsBackground.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tsBackground.Location = new System.Drawing.Point(0, 408);
			this.tsBackground.Name = "tsBackground";
			this.tsBackground.Size = new System.Drawing.Size(895, 25);
			this.tsBackground.TabIndex = 0;
			this.tsBackground.Text = "toolStrip1";
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(895, 24);
			this.menuStrip1.TabIndex = 2;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteDatabaserequiresRestartToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// deleteDatabaserequiresRestartToolStripMenuItem
			// 
			this.deleteDatabaserequiresRestartToolStripMenuItem.Name = "deleteDatabaserequiresRestartToolStripMenuItem";
			this.deleteDatabaserequiresRestartToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
			this.deleteDatabaserequiresRestartToolStripMenuItem.Text = "Delete database (requires Restart)";
			this.deleteDatabaserequiresRestartToolStripMenuItem.Click += new System.EventHandler(this.deleteDatabaserequiresRestartToolStripMenuItem_Click);
			// 
			// txtSearch
			// 
			this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtSearch.Location = new System.Drawing.Point(8, 6);
			this.txtSearch.Name = "txtSearch";
			this.txtSearch.Size = new System.Drawing.Size(746, 20);
			this.txtSearch.TabIndex = 3;
			this.txtSearch.TextChanged += new System.EventHandler(this.SearchParameterChanged);
			// 
			// cboAutomation
			// 
			this.cboAutomation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cboAutomation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboAutomation.FormattingEnabled = true;
			this.cboAutomation.Items.AddRange(new object[] {
            "No automation",
            "Automatic Download",
            "Automatic Unpacking"});
			this.cboAutomation.Location = new System.Drawing.Point(760, 6);
			this.cboAutomation.Name = "cboAutomation";
			this.cboAutomation.Size = new System.Drawing.Size(121, 21);
			this.cboAutomation.TabIndex = 4;
			this.cboAutomation.SelectedIndexChanged += new System.EventHandler(this.cboAutomation_SelectedIndexChanged);
			// 
			// tabsContainer
			// 
			this.tabsContainer.Controls.Add(this.tabReports);
			this.tabsContainer.Controls.Add(this.tabStats);
			this.tabsContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabsContainer.Location = new System.Drawing.Point(0, 24);
			this.tabsContainer.Name = "tabsContainer";
			this.tabsContainer.SelectedIndex = 0;
			this.tabsContainer.Size = new System.Drawing.Size(895, 384);
			this.tabsContainer.TabIndex = 5;
			// 
			// tabReports
			// 
			this.tabReports.BackColor = System.Drawing.SystemColors.Control;
			this.tabReports.Controls.Add(this.cboBuild);
			this.tabReports.Controls.Add(this.cboVersion);
			this.tabReports.Controls.Add(this.txtSearch);
			this.tabReports.Controls.Add(this.cboAutomation);
			this.tabReports.Location = new System.Drawing.Point(4, 22);
			this.tabReports.Name = "tabReports";
			this.tabReports.Padding = new System.Windows.Forms.Padding(3);
			this.tabReports.Size = new System.Drawing.Size(887, 358);
			this.tabReports.TabIndex = 0;
			this.tabReports.Text = "Crash Reports";
			// 
			// cboBuild
			// 
			this.cboBuild.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboBuild.FormattingEnabled = true;
			this.cboBuild.Location = new System.Drawing.Point(307, 32);
			this.cboBuild.Name = "cboBuild";
			this.cboBuild.Size = new System.Drawing.Size(60, 21);
			this.cboBuild.TabIndex = 6;
			this.cboBuild.SelectedIndexChanged += new System.EventHandler(this.SearchParameterChanged);
			// 
			// cboVersion
			// 
			this.cboVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboVersion.FormattingEnabled = true;
			this.cboVersion.Location = new System.Drawing.Point(378, 32);
			this.cboVersion.Name = "cboVersion";
			this.cboVersion.Size = new System.Drawing.Size(60, 21);
			this.cboVersion.TabIndex = 5;
			this.cboVersion.SelectedIndexChanged += new System.EventHandler(this.SearchParameterChanged);
			// 
			// tabStats
			// 
			this.tabStats.BackColor = System.Drawing.SystemColors.Control;
			this.tabStats.Location = new System.Drawing.Point(4, 22);
			this.tabStats.Name = "tabStats";
			this.tabStats.Padding = new System.Windows.Forms.Padding(3);
			this.tabStats.Size = new System.Drawing.Size(887, 358);
			this.tabStats.TabIndex = 1;
			this.tabStats.Text = "Statistics (Comming soon)";
			// 
			// Mainform
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(895, 433);
			this.Controls.Add(this.tabsContainer);
			this.Controls.Add(this.tsBackground);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "Mainform";
			this.Text = "Mainform";
			this.Shown += new System.EventHandler(this.Mainform_Shown);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.tabsContainer.ResumeLayout(false);
			this.tabReports.ResumeLayout(false);
			this.tabReports.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip tsBackground;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.TextBox txtSearch;
		private System.Windows.Forms.ComboBox cboAutomation;
		private System.Windows.Forms.TabControl tabsContainer;
		private System.Windows.Forms.TabPage tabReports;
		private System.Windows.Forms.TabPage tabStats;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem deleteDatabaserequiresRestartToolStripMenuItem;
		private System.Windows.Forms.ComboBox cboBuild;
		private System.Windows.Forms.ComboBox cboVersion;
	}
}

