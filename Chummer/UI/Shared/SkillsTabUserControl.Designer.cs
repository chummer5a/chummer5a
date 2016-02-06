namespace Chummer.UI.Shared
{
	partial class SkillsTabUserControl
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
			this.splitSkills = new System.Windows.Forms.SplitContainer();
			this.btnExotic = new System.Windows.Forms.Button();
			this.cboDisplayFilter = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.btnKnowledge = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.splitSkills)).BeginInit();
			this.splitSkills.Panel1.SuspendLayout();
			this.splitSkills.Panel2.SuspendLayout();
			this.splitSkills.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitSkills
			// 
			this.splitSkills.BackColor = System.Drawing.SystemColors.InactiveCaption;
			this.splitSkills.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitSkills.Location = new System.Drawing.Point(0, 0);
			this.splitSkills.Name = "splitSkills";
			this.splitSkills.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitSkills.Panel1
			// 
			this.splitSkills.Panel1.BackColor = System.Drawing.SystemColors.Control;
			this.splitSkills.Panel1.Controls.Add(this.btnExotic);
			this.splitSkills.Panel1.Controls.Add(this.cboDisplayFilter);
			this.splitSkills.Panel1.Controls.Add(this.label1);
			this.splitSkills.Panel1.Resize += new System.EventHandler(this.Panel1_Resize);
			// 
			// splitSkills.Panel2
			// 
			this.splitSkills.Panel2.BackColor = System.Drawing.SystemColors.Control;
			this.splitSkills.Panel2.Controls.Add(this.btnKnowledge);
			this.splitSkills.Size = new System.Drawing.Size(836, 611);
			this.splitSkills.SplitterDistance = 435;
			this.splitSkills.TabIndex = 0;
			// 
			// btnExotic
			// 
			this.btnExotic.Location = new System.Drawing.Point(744, 3);
			this.btnExotic.Name = "btnExotic";
			this.btnExotic.Size = new System.Drawing.Size(89, 23);
			this.btnExotic.TabIndex = 2;
			this.btnExotic.Text = "Add Exotic Skill";
			this.btnExotic.UseVisualStyleBackColor = true;
			this.btnExotic.Click += new System.EventHandler(this.btnExotic_Click);
			// 
			// cboDisplayFilter
			// 
			this.cboDisplayFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboDisplayFilter.FormattingEnabled = true;
			this.cboDisplayFilter.IntegralHeight = false;
			this.cboDisplayFilter.Location = new System.Drawing.Point(537, 3);
			this.cboDisplayFilter.Name = "cboDisplayFilter";
			this.cboDisplayFilter.Size = new System.Drawing.Size(201, 21);
			this.cboDisplayFilter.TabIndex = 1;
			this.cboDisplayFilter.SelectedIndexChanged += new System.EventHandler(this.cboDisplayFilter_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(0, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(63, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Skill Groups";
			// 
			// btnKnowledge
			// 
			this.btnKnowledge.Location = new System.Drawing.Point(3, 21);
			this.btnKnowledge.Name = "btnKnowledge";
			this.btnKnowledge.Size = new System.Drawing.Size(75, 23);
			this.btnKnowledge.TabIndex = 0;
			this.btnKnowledge.Text = "button1";
			this.btnKnowledge.UseVisualStyleBackColor = true;
			this.btnKnowledge.Click += new System.EventHandler(this.btnKnowledge_Click);
			// 
			// SkillsTabUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitSkills);
			this.Name = "SkillsTabUserControl";
			this.Size = new System.Drawing.Size(836, 611);
			this.Load += new System.EventHandler(this.SkillsTabUserControl_Load);
			this.splitSkills.Panel1.ResumeLayout(false);
			this.splitSkills.Panel1.PerformLayout();
			this.splitSkills.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitSkills)).EndInit();
			this.splitSkills.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitSkills;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cboDisplayFilter;
		private System.Windows.Forms.Button btnExotic;
		private System.Windows.Forms.Button btnKnowledge;
	}
}
