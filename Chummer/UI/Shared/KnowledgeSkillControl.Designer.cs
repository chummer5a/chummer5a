namespace Chummer.UI.Shared
{
	partial class KnowledgeSkillControl
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
			this.nudKarma = new System.Windows.Forms.NumericUpDown();
			this.nudSkill = new System.Windows.Forms.NumericUpDown();
			this.lblModifiedRating = new System.Windows.Forms.Label();
			this.cboSpec = new System.Windows.Forms.ComboBox();
			this.chkKarma = new System.Windows.Forms.CheckBox();
			this.cmdDelete = new System.Windows.Forms.Button();
			this.cboSkill = new System.Windows.Forms.ComboBox();
			this.cboType = new System.Windows.Forms.ComboBox();
			((System.ComponentModel.ISupportInitialize)(this.nudKarma)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudSkill)).BeginInit();
			this.SuspendLayout();
			// 
			// nudKarma
			// 
			this.nudKarma.Location = new System.Drawing.Point(248, 1);
			this.nudKarma.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
			this.nudKarma.Name = "nudKarma";
			this.nudKarma.Size = new System.Drawing.Size(40, 20);
			this.nudKarma.TabIndex = 14;
			// 
			// nudSkill
			// 
			this.nudSkill.Location = new System.Drawing.Point(206, 1);
			this.nudSkill.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
			this.nudSkill.Name = "nudSkill";
			this.nudSkill.Size = new System.Drawing.Size(40, 20);
			this.nudSkill.TabIndex = 15;
			// 
			// lblModifiedRating
			// 
			this.lblModifiedRating.AutoSize = true;
			this.lblModifiedRating.Location = new System.Drawing.Point(294, 4);
			this.lblModifiedRating.Name = "lblModifiedRating";
			this.lblModifiedRating.Size = new System.Drawing.Size(13, 13);
			this.lblModifiedRating.TabIndex = 16;
			this.lblModifiedRating.Text = "0";
			// 
			// cboSpec
			// 
			this.cboSpec.FormattingEnabled = true;
			this.cboSpec.Location = new System.Drawing.Point(313, 1);
			this.cboSpec.Name = "cboSpec";
			this.cboSpec.Size = new System.Drawing.Size(177, 21);
			this.cboSpec.Sorted = true;
			this.cboSpec.TabIndex = 17;
			// 
			// chkKarma
			// 
			this.chkKarma.AutoSize = true;
			this.chkKarma.Location = new System.Drawing.Point(496, 4);
			this.chkKarma.Name = "chkKarma";
			this.chkKarma.Size = new System.Drawing.Size(15, 14);
			this.chkKarma.TabIndex = 18;
			this.chkKarma.UseVisualStyleBackColor = true;
			// 
			// cmdDelete
			// 
			this.cmdDelete.Location = new System.Drawing.Point(662, 1);
			this.cmdDelete.Name = "cmdDelete";
			this.cmdDelete.Size = new System.Drawing.Size(75, 21);
			this.cmdDelete.TabIndex = 19;
			this.cmdDelete.Tag = "String_Delete";
			this.cmdDelete.Text = "Delete";
			this.cmdDelete.UseVisualStyleBackColor = true;
			// 
			// cboSkill
			// 
			this.cboSkill.FormattingEnabled = true;
			this.cboSkill.Location = new System.Drawing.Point(3, 1);
			this.cboSkill.Name = "cboSkill";
			this.cboSkill.Size = new System.Drawing.Size(190, 21);
			this.cboSkill.Sorted = true;
			this.cboSkill.TabIndex = 20;
			// 
			// cboType
			// 
			this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboType.FormattingEnabled = true;
			this.cboType.Location = new System.Drawing.Point(517, 1);
			this.cboType.Name = "cboType";
			this.cboType.Size = new System.Drawing.Size(139, 21);
			this.cboType.Sorted = true;
			this.cboType.TabIndex = 21;
			// 
			// KnowledgeSkillControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.cboType);
			this.Controls.Add(this.cboSkill);
			this.Controls.Add(this.cmdDelete);
			this.Controls.Add(this.chkKarma);
			this.Controls.Add(this.cboSpec);
			this.Controls.Add(this.lblModifiedRating);
			this.Controls.Add(this.nudSkill);
			this.Controls.Add(this.nudKarma);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "KnowledgeSkillControl";
			this.Size = new System.Drawing.Size(793, 23);
			this.Load += new System.EventHandler(this.KnowledgeSkillControl_Load);
			this.DoubleClick += new System.EventHandler(this.KnowledgeSkillControl_DoubleClick);
			((System.ComponentModel.ISupportInitialize)(this.nudKarma)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudSkill)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.NumericUpDown nudKarma;
		private System.Windows.Forms.NumericUpDown nudSkill;
		private System.Windows.Forms.Label lblModifiedRating;
		private System.Windows.Forms.ComboBox cboSpec;
		private System.Windows.Forms.CheckBox chkKarma;
		private System.Windows.Forms.Button cmdDelete;
		private System.Windows.Forms.ComboBox cboSkill;
		private System.Windows.Forms.ComboBox cboType;
	}
}
