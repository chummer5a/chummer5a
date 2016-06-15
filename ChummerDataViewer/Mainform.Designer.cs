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
			this.nothingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.txtSearch = new System.Windows.Forms.TextBox();
			this.cboAutomation = new System.Windows.Forms.ComboBox();
			this.menuStrip1.SuspendLayout();
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
            this.nothingToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(895, 24);
			this.menuStrip1.TabIndex = 2;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// nothingToolStripMenuItem
			// 
			this.nothingToolStripMenuItem.Name = "nothingToolStripMenuItem";
			this.nothingToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
			this.nothingToolStripMenuItem.Text = "Nothing";
			// 
			// txtSearch
			// 
			this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtSearch.Location = new System.Drawing.Point(12, 28);
			this.txtSearch.Name = "txtSearch";
			this.txtSearch.Size = new System.Drawing.Size(743, 20);
			this.txtSearch.TabIndex = 3;
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
			this.cboAutomation.Location = new System.Drawing.Point(761, 26);
			this.cboAutomation.Name = "cboAutomation";
			this.cboAutomation.Size = new System.Drawing.Size(121, 21);
			this.cboAutomation.TabIndex = 4;
			// 
			// Mainform
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(895, 433);
			this.Controls.Add(this.cboAutomation);
			this.Controls.Add(this.txtSearch);
			this.Controls.Add(this.tsBackground);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "Mainform";
			this.Text = "Mainform";
			this.Load += new System.EventHandler(this.Mainform_Load);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip tsBackground;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem nothingToolStripMenuItem;
		private System.Windows.Forms.TextBox txtSearch;
		private System.Windows.Forms.ComboBox cboAutomation;
	}
}

