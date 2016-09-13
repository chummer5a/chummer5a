namespace Chummer.UI.Options
{
	partial class OptionsNumberControl
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
			this.nudRule = new System.Windows.Forms.NumericUpDown();
			this.lblRuleDescriptionLabel = new System.Windows.Forms.Label();
			this.lblRuleMultiplierLabel = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.nudRule)).BeginInit();
			this.SuspendLayout();
			// 
			// nudRule
			// 
			this.nudRule.Location = new System.Drawing.Point(104, 6);
			this.nudRule.Name = "nudRule";
			this.nudRule.Size = new System.Drawing.Size(64, 20);
			this.nudRule.TabIndex = 0;
			// 
			// lblRuleDescriptionLabel
			// 
			this.lblRuleDescriptionLabel.Location = new System.Drawing.Point(8, 8);
			this.lblRuleDescriptionLabel.Name = "lblRuleDescriptionLabel";
			this.lblRuleDescriptionLabel.Size = new System.Drawing.Size(88, 16);
			this.lblRuleDescriptionLabel.TabIndex = 1;
			this.lblRuleDescriptionLabel.Text = "label1";
			// 
			// lblRuleMultiplierLabel
			// 
			this.lblRuleMultiplierLabel.Location = new System.Drawing.Point(176, 8);
			this.lblRuleMultiplierLabel.Name = "lblRuleMultiplierLabel";
			this.lblRuleMultiplierLabel.Size = new System.Drawing.Size(96, 16);
			this.lblRuleMultiplierLabel.TabIndex = 2;
			this.lblRuleMultiplierLabel.Text = "label2";
			// 
			// OptionsNumberControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.Controls.Add(this.lblRuleMultiplierLabel);
			this.Controls.Add(this.lblRuleDescriptionLabel);
			this.Controls.Add(this.nudRule);
			this.Name = "OptionsNumberControl";
			this.Size = new System.Drawing.Size(279, 31);
			((System.ComponentModel.ISupportInitialize)(this.nudRule)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.NumericUpDown nudRule;
		private System.Windows.Forms.Label lblRuleDescriptionLabel;
		private System.Windows.Forms.Label lblRuleMultiplierLabel;
	}
}
