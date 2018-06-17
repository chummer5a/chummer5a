namespace Chummer.UI.Skills
{
    partial class SkillGroupControl
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
            if (disposing)
            {
                components?.Dispose();
                UnbindSkillGroupControl();
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
            this.lblName = new LabelWithToolTip();
            this.lblGroupRating = new System.Windows.Forms.Label();
            this.btnCareerIncrease = new ButtonWithToolTip();
            this.nudKarma = new Chummer.NumericUpDownEx();
            this.nudSkill = new Chummer.NumericUpDownEx();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill)).BeginInit();
            this.SuspendLayout();
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(0, 4);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(66, 13);
            this.lblName.TabIndex = 6;
            this.lblName.Text = "[groupname]";
            // 
            // lblGroupRating
            // 
            this.lblGroupRating.AutoSize = true;
            this.lblGroupRating.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGroupRating.Location = new System.Drawing.Point(131, 4);
            this.lblGroupRating.Name = "lblGroupRating";
            this.lblGroupRating.Size = new System.Drawing.Size(41, 13);
            this.lblGroupRating.TabIndex = 7;
            this.lblGroupRating.Text = "label1";
            // 
            // btnCareerIncrease
            // 
            this.btnCareerIncrease.Image = global::Chummer.Properties.Resources.add;
            this.btnCareerIncrease.Location = new System.Drawing.Point(180, -2);
            this.btnCareerIncrease.Name = "btnCareerIncrease";
            this.btnCareerIncrease.Size = new System.Drawing.Size(24, 24);
            this.btnCareerIncrease.TabIndex = 22;
            this.btnCareerIncrease.UseVisualStyleBackColor = true;
            this.btnCareerIncrease.Click += new System.EventHandler(this.btnCareerIncrease_Click);
            // 
            // nudKarma
            // 
            this.nudKarma.InterceptMouseWheel = Chummer.NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver;
            this.nudKarma.Location = new System.Drawing.Point(180, 1);
            this.nudKarma.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudKarma.Name = "nudKarma";
            this.nudKarma.Size = new System.Drawing.Size(40, 20);
            this.nudKarma.TabIndex = 5;
            // 
            // nudSkill
            // 
            this.nudSkill.InterceptMouseWheel = Chummer.NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver;
            this.nudSkill.Location = new System.Drawing.Point(134, 1);
            this.nudSkill.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudSkill.Name = "nudSkill";
            this.nudSkill.Size = new System.Drawing.Size(40, 20);
            this.nudSkill.TabIndex = 2;
            // 
            // SkillGroupControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnCareerIncrease);
            this.Controls.Add(this.lblGroupRating);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.nudKarma);
            this.Controls.Add(this.nudSkill);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "SkillGroupControl";
            this.Size = new System.Drawing.Size(225, 23);
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private NumericUpDownEx nudSkill;
        private NumericUpDownEx nudKarma;
        private LabelWithToolTip lblName;
        private System.Windows.Forms.Label lblGroupRating;
        private ButtonWithToolTip btnCareerIncrease;
    }
}
