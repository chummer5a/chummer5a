namespace Chummer
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
            this.nudSkill = new System.Windows.Forms.NumericUpDown();
            this.txtGroupName = new System.Windows.Forms.TextBox();
            this.tipTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.cmdImproveSkill = new System.Windows.Forms.Button();
            this.lblGroupRating = new System.Windows.Forms.Label();
            this.nudKarma = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).BeginInit();
            this.SuspendLayout();
            // 
            // nudSkill
            // 
            this.nudSkill.Location = new System.Drawing.Point(154, 1);
            this.nudSkill.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.nudSkill.Name = "nudSkill";
            this.nudSkill.Size = new System.Drawing.Size(40, 20);
            this.nudSkill.TabIndex = 1;
            this.nudSkill.ValueChanged += new System.EventHandler(this.nudSkill_ValueChanged);
            // 
            // txtGroupName
            // 
            this.txtGroupName.BackColor = System.Drawing.SystemColors.Control;
            this.txtGroupName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtGroupName.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.txtGroupName.Location = new System.Drawing.Point(0, 3);
            this.txtGroupName.Name = "txtGroupName";
            this.txtGroupName.ReadOnly = true;
            this.txtGroupName.Size = new System.Drawing.Size(148, 13);
            this.txtGroupName.TabIndex = 0;
            this.txtGroupName.Click += new System.EventHandler(this.txtGroupName_Click);
            this.txtGroupName.Enter += new System.EventHandler(this.txtSkillName_Enter);
            // 
            // tipTooltip
            // 
            this.tipTooltip.AutoPopDelay = 10000;
            this.tipTooltip.InitialDelay = 250;
            this.tipTooltip.IsBalloon = true;
            this.tipTooltip.ReshowDelay = 100;
            this.tipTooltip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.tipTooltip.ToolTipTitle = "Chummer Help";
            // 
            // cmdImproveSkill
            // 
            this.cmdImproveSkill.FlatAppearance.BorderSize = 0;
            this.cmdImproveSkill.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdImproveSkill.Image = global::Chummer.Properties.Resources.add;
            this.cmdImproveSkill.Location = new System.Drawing.Point(200, 0);
            this.cmdImproveSkill.Name = "cmdImproveSkill";
            this.cmdImproveSkill.Size = new System.Drawing.Size(24, 24);
            this.cmdImproveSkill.TabIndex = 3;
            this.cmdImproveSkill.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.cmdImproveSkill.UseVisualStyleBackColor = true;
            this.cmdImproveSkill.Visible = false;
            this.cmdImproveSkill.Click += new System.EventHandler(this.cmdImproveSkill_Click);
            // 
            // lblGroupRating
            // 
            this.lblGroupRating.AutoSize = true;
            this.lblGroupRating.Location = new System.Drawing.Point(154, 3);
            this.lblGroupRating.Name = "lblGroupRating";
            this.lblGroupRating.Size = new System.Drawing.Size(19, 13);
            this.lblGroupRating.TabIndex = 2;
            this.lblGroupRating.Text = "[0]";
            this.lblGroupRating.Visible = false;
            // 
            // nudKarma
            // 
            this.nudKarma.Location = new System.Drawing.Point(200, 1);
            this.nudKarma.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.nudKarma.Name = "nudKarma";
            this.nudKarma.Size = new System.Drawing.Size(40, 20);
            this.nudKarma.TabIndex = 4;
            this.nudKarma.ValueChanged += new System.EventHandler(this.nudKarma_ValueChanged);
            // 
            // SkillGroupControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.nudKarma);
            this.Controls.Add(this.lblGroupRating);
            this.Controls.Add(this.cmdImproveSkill);
            this.Controls.Add(this.txtGroupName);
            this.Controls.Add(this.nudSkill);
            this.Name = "SkillGroupControl";
            this.Size = new System.Drawing.Size(370, 23);
            this.Load += new System.EventHandler(this.SkillControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.NumericUpDown nudSkill;
        private System.Windows.Forms.TextBox txtGroupName;
		private System.Windows.Forms.ToolTip tipTooltip;
		private System.Windows.Forms.Button cmdImproveSkill;
		private System.Windows.Forms.Label lblGroupRating;
        private System.Windows.Forms.NumericUpDown nudKarma;
    }
}
