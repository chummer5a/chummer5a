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
            this.components = new System.ComponentModel.Container();
            this.nudSkill = new Chummer.helpers.NumericUpDownEx();
            this.nudKarma = new Chummer.helpers.NumericUpDownEx();
            this.lblName = new System.Windows.Forms.Label();
            this.lblGroupRating = new System.Windows.Forms.Label();
            this.btnCareerIncrease = new System.Windows.Forms.Button();
            this.tipToolTip = new TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).BeginInit();
            this.SuspendLayout();
            // 
            // nudSkill
            // 
            this.nudSkill.InterceptMouseWheel = Chummer.helpers.NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver;
            this.nudSkill.Location = new System.Drawing.Point(134, 1);
            this.nudSkill.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudSkill.Name = "nudSkill";
            this.nudSkill.ShowUpDownButtons = Chummer.helpers.NumericUpDownEx.ShowUpDownButtonsMode.Always;
            this.nudSkill.Size = new System.Drawing.Size(40, 20);
            this.nudSkill.TabIndex = 2;
            // 
            // nudKarma
            // 
            this.nudKarma.InterceptMouseWheel = Chummer.helpers.NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver;
            this.nudKarma.Location = new System.Drawing.Point(180, 1);
            this.nudKarma.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudKarma.Name = "nudKarma";
            this.nudKarma.ShowUpDownButtons = Chummer.helpers.NumericUpDownEx.ShowUpDownButtonsMode.Always;
            this.nudKarma.Size = new System.Drawing.Size(40, 20);
            this.nudKarma.TabIndex = 5;
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
            this.lblGroupRating.Visible = false;
            // 
            // btnCareerIncrease
            // 
            this.btnCareerIncrease.Image = global::Chummer.Properties.Resources.add;
            this.btnCareerIncrease.Location = new System.Drawing.Point(180, -2);
            this.btnCareerIncrease.Name = "btnCareerIncrease";
            this.btnCareerIncrease.Size = new System.Drawing.Size(24, 24);
            this.btnCareerIncrease.TabIndex = 22;
            this.btnCareerIncrease.UseVisualStyleBackColor = true;
            this.btnCareerIncrease.Visible = false;
            this.btnCareerIncrease.Click += new System.EventHandler(this.btnCareerIncrease_Click);
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
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Chummer.helpers.NumericUpDownEx nudSkill;
        private Chummer.helpers.NumericUpDownEx nudKarma;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblGroupRating;
        private System.Windows.Forms.Button btnCareerIncrease;
        private TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip tipToolTip;
    }
}
