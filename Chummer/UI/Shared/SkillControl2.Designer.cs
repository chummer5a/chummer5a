namespace Chummer.UI.Shared
{
	partial class SkillControl2
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
			this.lblName = new System.Windows.Forms.Label();
			this.lblAttribute = new System.Windows.Forms.Label();
			this.nudKarma = new System.Windows.Forms.NumericUpDown();
			this.nudSkill = new System.Windows.Forms.NumericUpDown();
			this.lblModifiedRating = new System.Windows.Forms.Label();
			this.cboSpec = new System.Windows.Forms.ComboBox();
			this.chkKarma = new System.Windows.Forms.CheckBox();
			this.cmdDelete = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.nudKarma)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudSkill)).BeginInit();
			this.SuspendLayout();
			// 
			// lblName
			// 
			this.lblName.AutoSize = true;
			this.lblName.Location = new System.Drawing.Point(0, 4);
			this.lblName.Name = "lblName";
			this.lblName.Size = new System.Drawing.Size(35, 13);
			this.lblName.TabIndex = 0;
			this.lblName.Text = "label1";
			// 
			// lblAttribute
			// 
			this.lblAttribute.AutoSize = true;
			this.lblAttribute.Location = new System.Drawing.Point(127, 4);
			this.lblAttribute.Name = "lblAttribute";
			this.lblAttribute.Size = new System.Drawing.Size(35, 13);
			this.lblAttribute.TabIndex = 3;
			this.lblAttribute.Text = "label1";
			// 
			// nudKarma
			// 
			this.nudKarma.Location = new System.Drawing.Point(210, 1);
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
			this.nudSkill.Location = new System.Drawing.Point(168, 1);
			this.nudSkill.Maximum = new decimal(new int[] {
            6,
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
			this.lblModifiedRating.Location = new System.Drawing.Point(256, 4);
			this.lblModifiedRating.Name = "lblModifiedRating";
			this.lblModifiedRating.Size = new System.Drawing.Size(13, 13);
			this.lblModifiedRating.TabIndex = 16;
			this.lblModifiedRating.Text = "0";
			// 
			// cboSpec
			// 
			this.cboSpec.FormattingEnabled = true;
			this.cboSpec.Location = new System.Drawing.Point(275, 1);
			this.cboSpec.Name = "cboSpec";
			this.cboSpec.Size = new System.Drawing.Size(177, 21);
			this.cboSpec.Sorted = true;
			this.cboSpec.TabIndex = 17;
			// 
			// chkKarma
			// 
			this.chkKarma.AutoSize = true;
			this.chkKarma.Location = new System.Drawing.Point(458, 4);
			this.chkKarma.Name = "chkKarma";
			this.chkKarma.Size = new System.Drawing.Size(15, 14);
			this.chkKarma.TabIndex = 18;
			this.chkKarma.UseVisualStyleBackColor = true;
			// 
			// cmdDelete
			// 
			this.cmdDelete.Location = new System.Drawing.Point(479, -1);
			this.cmdDelete.Name = "cmdDelete";
			this.cmdDelete.Size = new System.Drawing.Size(75, 23);
			this.cmdDelete.TabIndex = 19;
			this.cmdDelete.Tag = "String_Delete";
			this.cmdDelete.Text = "Delete";
			this.cmdDelete.UseVisualStyleBackColor = true;
			this.cmdDelete.Visible = false;
			// 
			// SkillControl2
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.cmdDelete);
			this.Controls.Add(this.chkKarma);
			this.Controls.Add(this.cboSpec);
			this.Controls.Add(this.lblModifiedRating);
			this.Controls.Add(this.nudSkill);
			this.Controls.Add(this.nudKarma);
			this.Controls.Add(this.lblAttribute);
			this.Controls.Add(this.lblName);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "SkillControl2";
			this.Size = new System.Drawing.Size(559, 23);
			this.Load += new System.EventHandler(this.SkillControl2_Load);
			((System.ComponentModel.ISupportInitialize)(this.nudKarma)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudSkill)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblName;
		private System.Windows.Forms.Label lblAttribute;
		private System.Windows.Forms.NumericUpDown nudKarma;
		private System.Windows.Forms.NumericUpDown nudSkill;
		private System.Windows.Forms.Label lblModifiedRating;
		private System.Windows.Forms.ComboBox cboSpec;
		private System.Windows.Forms.CheckBox chkKarma;
		private System.Windows.Forms.Button cmdDelete;
	}
}
