namespace Chummer
{
    partial class SkillControl
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
            this.cboSpec = new System.Windows.Forms.ComboBox();
            this.cmdDelete = new System.Windows.Forms.Button();
            this.lblModifiedRating = new System.Windows.Forms.Label();
            this.tipTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.cboKnowledgeSkillCategory = new System.Windows.Forms.ComboBox();
            this.cboSkillName = new System.Windows.Forms.ComboBox();
            this.lblSkillName = new System.Windows.Forms.Label();
            this.lblAttribute = new System.Windows.Forms.Label();
            this.cmdImproveSkill = new System.Windows.Forms.Button();
            this.lblSkillRating = new System.Windows.Forms.Label();
            this.cmdChangeSpec = new System.Windows.Forms.Button();
            this.cmdRoll = new System.Windows.Forms.Button();
            this.cmdBreakGroup = new System.Windows.Forms.Button();
            this.nudKarma = new System.Windows.Forms.NumericUpDown();
            this.chkKarma = new System.Windows.Forms.CheckBox();
            this.lblSpec = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).BeginInit();
            this.SuspendLayout();
            // 
            // nudSkill
            // 
            this.nudSkill.Location = new System.Drawing.Point(205, 1);
            this.nudSkill.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.nudSkill.Name = "nudSkill";
            this.nudSkill.Size = new System.Drawing.Size(40, 20);
            this.nudSkill.TabIndex = 3;
            this.nudSkill.ValueChanged += new System.EventHandler(this.nudSkill_ValueChanged);
            // 
            // cboSpec
            // 
            this.cboSpec.FormattingEnabled = true;
            this.cboSpec.Location = new System.Drawing.Point(358, 0);
            this.cboSpec.Name = "cboSpec";
            this.cboSpec.Size = new System.Drawing.Size(177, 21);
            this.cboSpec.Sorted = true;
            this.cboSpec.TabIndex = 7;
            this.cboSpec.TextChanged += new System.EventHandler(this.cboSpec_TextChanged);
            this.cboSpec.Leave += new System.EventHandler(this.cboSpec_Leave);
            // 
            // cmdDelete
            // 
            this.cmdDelete.Location = new System.Drawing.Point(742, -1);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(75, 23);
            this.cmdDelete.TabIndex = 9;
            this.cmdDelete.Tag = "String_Delete";
            this.cmdDelete.Text = "Delete";
            this.cmdDelete.UseVisualStyleBackColor = true;
            this.cmdDelete.Visible = false;
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // lblModifiedRating
            // 
            this.lblModifiedRating.AutoSize = true;
            this.lblModifiedRating.Location = new System.Drawing.Point(311, 4);
            this.lblModifiedRating.Name = "lblModifiedRating";
            this.lblModifiedRating.Size = new System.Drawing.Size(13, 13);
            this.lblModifiedRating.TabIndex = 6;
            this.lblModifiedRating.Text = "0";
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
            // cboKnowledgeSkillCategory
            // 
            this.cboKnowledgeSkillCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboKnowledgeSkillCategory.FormattingEnabled = true;
            this.cboKnowledgeSkillCategory.Location = new System.Drawing.Point(571, 0);
            this.cboKnowledgeSkillCategory.Name = "cboKnowledgeSkillCategory";
            this.cboKnowledgeSkillCategory.Size = new System.Drawing.Size(162, 21);
            this.cboKnowledgeSkillCategory.TabIndex = 8;
            this.cboKnowledgeSkillCategory.Visible = false;
            this.cboKnowledgeSkillCategory.SelectedIndexChanged += new System.EventHandler(this.cboKnonwledgeSkillCategory_SelectedIndexChanged);
            // 
            // cboSkillName
            // 
            this.cboSkillName.DropDownWidth = 250;
            this.cboSkillName.FormattingEnabled = true;
            this.cboSkillName.Location = new System.Drawing.Point(3, 1);
            this.cboSkillName.Name = "cboSkillName";
            this.cboSkillName.Size = new System.Drawing.Size(196, 21);
            this.cboSkillName.Sorted = true;
            this.cboSkillName.TabIndex = 1;
            this.cboSkillName.Visible = false;
            this.cboSkillName.SelectedIndexChanged += new System.EventHandler(this.cboSkillName_SelectedIndexChanged);
            this.cboSkillName.TextChanged += new System.EventHandler(this.cboSkillName_TextChanged);
            // 
            // lblSkillName
            // 
            this.lblSkillName.AutoSize = true;
            this.lblSkillName.Location = new System.Drawing.Point(3, 5);
            this.lblSkillName.Name = "lblSkillName";
            this.lblSkillName.Size = new System.Drawing.Size(35, 13);
            this.lblSkillName.TabIndex = 0;
            this.lblSkillName.Text = "label1";
            this.lblSkillName.Click += new System.EventHandler(this.lblSkillName_Click);
            // 
            // lblAttribute
            // 
            this.lblAttribute.AutoSize = true;
            this.lblAttribute.Location = new System.Drawing.Point(151, 5);
            this.lblAttribute.Name = "lblAttribute";
            this.lblAttribute.Size = new System.Drawing.Size(35, 13);
            this.lblAttribute.TabIndex = 2;
            this.lblAttribute.Text = "label1";
            // 
            // cmdImproveSkill
            // 
            this.cmdImproveSkill.FlatAppearance.BorderSize = 0;
            this.cmdImproveSkill.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdImproveSkill.Image = global::Chummer.Properties.Resources.add;
            this.cmdImproveSkill.Location = new System.Drawing.Point(225, -1);
            this.cmdImproveSkill.Name = "cmdImproveSkill";
            this.cmdImproveSkill.Size = new System.Drawing.Size(24, 24);
            this.cmdImproveSkill.TabIndex = 5;
            this.cmdImproveSkill.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.cmdImproveSkill.UseVisualStyleBackColor = true;
            this.cmdImproveSkill.Visible = false;
            this.cmdImproveSkill.Click += new System.EventHandler(this.cmdImproveSkill_Click);
            // 
            // lblSkillRating
            // 
            this.lblSkillRating.AutoSize = true;
            this.lblSkillRating.Location = new System.Drawing.Point(205, 5);
            this.lblSkillRating.Name = "lblSkillRating";
            this.lblSkillRating.Size = new System.Drawing.Size(13, 13);
            this.lblSkillRating.TabIndex = 4;
            this.lblSkillRating.Text = "0";
            this.lblSkillRating.Visible = false;
            // 
            // cmdChangeSpec
            // 
            this.cmdChangeSpec.FlatAppearance.BorderSize = 0;
            this.cmdChangeSpec.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdChangeSpec.Image = global::Chummer.Properties.Resources.add;
            this.cmdChangeSpec.Location = new System.Drawing.Point(541, -2);
            this.cmdChangeSpec.Name = "cmdChangeSpec";
            this.cmdChangeSpec.Size = new System.Drawing.Size(24, 24);
            this.cmdChangeSpec.TabIndex = 10;
            this.cmdChangeSpec.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.cmdChangeSpec.UseVisualStyleBackColor = true;
            this.cmdChangeSpec.Visible = false;
            this.cmdChangeSpec.Click += new System.EventHandler(this.cmdChangeSpec_Click);
            // 
            // cmdRoll
            // 
            this.cmdRoll.FlatAppearance.BorderSize = 0;
            this.cmdRoll.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdRoll.Image = global::Chummer.Properties.Resources.die;
            this.cmdRoll.Location = new System.Drawing.Point(358, -2);
            this.cmdRoll.Name = "cmdRoll";
            this.cmdRoll.Size = new System.Drawing.Size(24, 24);
            this.cmdRoll.TabIndex = 11;
            this.cmdRoll.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.cmdRoll.UseVisualStyleBackColor = true;
            this.cmdRoll.Visible = false;
            this.cmdRoll.Click += new System.EventHandler(this.cmdRoll_Click);
            // 
            // cmdBreakGroup
            // 
            this.cmdBreakGroup.Location = new System.Drawing.Point(205, 0);
            this.cmdBreakGroup.Name = "cmdBreakGroup";
            this.cmdBreakGroup.Size = new System.Drawing.Size(44, 23);
            this.cmdBreakGroup.TabIndex = 12;
            this.cmdBreakGroup.Tag = "String_Break";
            this.cmdBreakGroup.Text = "Break";
            this.cmdBreakGroup.UseVisualStyleBackColor = true;
            this.cmdBreakGroup.Visible = false;
            this.cmdBreakGroup.Click += new System.EventHandler(this.cmdBreakGroup_Click);
            // 
            // nudKarma
            // 
            this.nudKarma.Location = new System.Drawing.Point(255, 1);
            this.nudKarma.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudKarma.Name = "nudKarma";
            this.nudKarma.Size = new System.Drawing.Size(40, 20);
            this.nudKarma.TabIndex = 13;
            this.nudKarma.ValueChanged += new System.EventHandler(this.nudKarma_ValueChanged);
            // 
            // chkKarma
            // 
            this.chkKarma.AutoSize = true;
            this.chkKarma.Location = new System.Drawing.Point(541, 4);
            this.chkKarma.Name = "chkKarma";
            this.chkKarma.Size = new System.Drawing.Size(15, 14);
            this.chkKarma.TabIndex = 14;
            this.chkKarma.UseVisualStyleBackColor = true;
            this.chkKarma.CheckedChanged += new System.EventHandler(this.chkKarma_CheckedChanged);
            // 
            // lblSpec
            // 
            this.lblSpec.AutoSize = true;
            this.lblSpec.Location = new System.Drawing.Point(358, 5);
            this.lblSpec.MaximumSize = new System.Drawing.Size(177, 0);
            this.lblSpec.Name = "lblSpec";
            this.lblSpec.Size = new System.Drawing.Size(35, 13);
            this.lblSpec.TabIndex = 15;
            this.lblSpec.Text = "label1";
            this.lblSpec.Visible = false;
            // 
            // SkillControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblSpec);
            this.Controls.Add(this.chkKarma);
            this.Controls.Add(this.nudKarma);
            this.Controls.Add(this.cmdBreakGroup);
            this.Controls.Add(this.cmdRoll);
            this.Controls.Add(this.cmdChangeSpec);
            this.Controls.Add(this.lblSkillRating);
            this.Controls.Add(this.cmdImproveSkill);
            this.Controls.Add(this.lblAttribute);
            this.Controls.Add(this.lblSkillName);
            this.Controls.Add(this.cboSkillName);
            this.Controls.Add(this.cboKnowledgeSkillCategory);
            this.Controls.Add(this.lblModifiedRating);
            this.Controls.Add(this.cmdDelete);
            this.Controls.Add(this.cboSpec);
            this.Controls.Add(this.nudSkill);
            this.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.Name = "SkillControl";
            this.Size = new System.Drawing.Size(820, 23);
            this.Load += new System.EventHandler(this.SkillControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.NumericUpDown nudSkill;
		private System.Windows.Forms.ComboBox cboSpec;
        private System.Windows.Forms.Button cmdDelete;
		private System.Windows.Forms.Label lblModifiedRating;
		private System.Windows.Forms.ToolTip tipTooltip;
		private System.Windows.Forms.ComboBox cboKnowledgeSkillCategory;
		private System.Windows.Forms.ComboBox cboSkillName;
		private System.Windows.Forms.Label lblSkillName;
		private System.Windows.Forms.Label lblAttribute;
		private System.Windows.Forms.Button cmdImproveSkill;
		private System.Windows.Forms.Label lblSkillRating;
		private System.Windows.Forms.Button cmdChangeSpec;
		private System.Windows.Forms.Button cmdRoll;
		private System.Windows.Forms.Button cmdBreakGroup;
        private System.Windows.Forms.NumericUpDown nudKarma;
        private System.Windows.Forms.CheckBox chkKarma;
        private System.Windows.Forms.Label lblSpec;
    }
}
