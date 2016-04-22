namespace Chummer.UI.Skills
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
			this.lblActiveSkills = new System.Windows.Forms.Label();
			this.lblKnowledgeSkills = new System.Windows.Forms.Label();
			this.lblKnowledgeSkillPointsTitle = new System.Windows.Forms.Label();
			this.lblKnowledgeSkillPoints = new System.Windows.Forms.Label();
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
			this.splitSkills.Margin = new System.Windows.Forms.Padding(0);
			this.splitSkills.MinimumSize = new System.Drawing.Size(830, 0);
			this.splitSkills.Name = "splitSkills";
			this.splitSkills.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitSkills.Panel1
			// 
			this.splitSkills.Panel1.BackColor = System.Drawing.SystemColors.Control;
			this.splitSkills.Panel1.Controls.Add(this.lblActiveSkills);
			this.splitSkills.Panel1.Controls.Add(this.btnExotic);
			this.splitSkills.Panel1.Controls.Add(this.cboDisplayFilter);
			this.splitSkills.Panel1.Controls.Add(this.label1);
			this.splitSkills.Panel1.Resize += new System.EventHandler(this.Panel1_Resize);
			// 
			// splitSkills.Panel2
			// 
			this.splitSkills.Panel2.BackColor = System.Drawing.SystemColors.Control;
			this.splitSkills.Panel2.Controls.Add(this.lblKnowledgeSkillPoints);
			this.splitSkills.Panel2.Controls.Add(this.lblKnowledgeSkillPointsTitle);
			this.splitSkills.Panel2.Controls.Add(this.lblKnowledgeSkills);
			this.splitSkills.Panel2.Controls.Add(this.btnKnowledge);
			this.splitSkills.Panel2.Resize += new System.EventHandler(this.Panel2_Resize);
			this.splitSkills.Size = new System.Drawing.Size(830, 611);
			this.splitSkills.SplitterDistance = 450;
			this.splitSkills.TabIndex = 0;
			// 
			// btnExotic
			// 
			this.btnExotic.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnExotic.Location = new System.Drawing.Point(738, 3);
			this.btnExotic.Name = "btnExotic";
			this.btnExotic.Size = new System.Drawing.Size(89, 23);
			this.btnExotic.TabIndex = 2;
			this.btnExotic.Tag = "Button_AddExoticSkill";
			this.btnExotic.Text = "Add Exotic Skill";
			this.btnExotic.UseVisualStyleBackColor = true;
			this.btnExotic.Click += new System.EventHandler(this.btnExotic_Click);
			// 
			// cboDisplayFilter
			// 
			this.cboDisplayFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cboDisplayFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboDisplayFilter.FormattingEnabled = true;
			this.cboDisplayFilter.IntegralHeight = false;
			this.cboDisplayFilter.Location = new System.Drawing.Point(531, 3);
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
			this.label1.Tag = "Label_SkillGroups";
			this.label1.Text = "Skill Groups";
			// 
			// btnKnowledge
			// 
			this.btnKnowledge.Location = new System.Drawing.Point(3, 21);
			this.btnKnowledge.Name = "btnKnowledge";
			this.btnKnowledge.Size = new System.Drawing.Size(75, 23);
			this.btnKnowledge.TabIndex = 0;
			this.btnKnowledge.Tag = "Button_AddSkill";
			this.btnKnowledge.Text = "&Add Skill";
			this.btnKnowledge.UseVisualStyleBackColor = true;
			this.btnKnowledge.Click += new System.EventHandler(this.btnKnowledge_Click);
			// 
			// lblActiveSkills
			// 
			this.lblActiveSkills.AutoSize = true;
			this.lblActiveSkills.Location = new System.Drawing.Point(256, 21);
			this.lblActiveSkills.Name = "lblActiveSkills";
			this.lblActiveSkills.Size = new System.Drawing.Size(64, 13);
			this.lblActiveSkills.TabIndex = 3;
			this.lblActiveSkills.Tag = "Label_ActiveSkills";
			this.lblActiveSkills.Text = "Active Skills";
			// 
			// lblKnowledgeSkills
			// 
			this.lblKnowledgeSkills.AutoSize = true;
			this.lblKnowledgeSkills.Location = new System.Drawing.Point(0, 5);
			this.lblKnowledgeSkills.Name = "lblKnowledgeSkills";
			this.lblKnowledgeSkills.Size = new System.Drawing.Size(87, 13);
			this.lblKnowledgeSkills.TabIndex = 4;
			this.lblKnowledgeSkills.Tag = "Label_KnowledgeSkills";
			this.lblKnowledgeSkills.Text = "Knowledge Skills";
			// 
			// lblKnowledgeSkillPointsTitle
			// 
			this.lblKnowledgeSkillPointsTitle.AutoSize = true;
			this.lblKnowledgeSkillPointsTitle.Location = new System.Drawing.Point(110, 5);
			this.lblKnowledgeSkillPointsTitle.Name = "lblKnowledgeSkillPointsTitle";
			this.lblKnowledgeSkillPointsTitle.Size = new System.Drawing.Size(194, 13);
			this.lblKnowledgeSkillPointsTitle.TabIndex = 37;
			this.lblKnowledgeSkillPointsTitle.Tag = "Label_FreeKnowledgeSkills";
			this.lblKnowledgeSkillPointsTitle.Text = "Free Knowledge Skill Points Remaining:";
			// 
			// lblKnowledgeSkillPoints
			// 
			this.lblKnowledgeSkillPoints.AutoSize = true;
			this.lblKnowledgeSkillPoints.Location = new System.Drawing.Point(310, 5);
			this.lblKnowledgeSkillPoints.Name = "lblKnowledgeSkillPoints";
			this.lblKnowledgeSkillPoints.Size = new System.Drawing.Size(34, 13);
			this.lblKnowledgeSkillPoints.TabIndex = 38;
			this.lblKnowledgeSkillPoints.Text = "0 of 0";
			// 
			// SkillsTabUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitSkills);
			this.Name = "SkillsTabUserControl";
			this.Size = new System.Drawing.Size(830, 611);
			this.Load += new System.EventHandler(this.SkillsTabUserControl_Load);
			this.splitSkills.Panel1.ResumeLayout(false);
			this.splitSkills.Panel1.PerformLayout();
			this.splitSkills.Panel2.ResumeLayout(false);
			this.splitSkills.Panel2.PerformLayout();
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
		private System.Windows.Forms.Label lblActiveSkills;
		private System.Windows.Forms.Label lblKnowledgeSkillPoints;
		private System.Windows.Forms.Label lblKnowledgeSkillPointsTitle;
		private System.Windows.Forms.Label lblKnowledgeSkills;
	}
}
