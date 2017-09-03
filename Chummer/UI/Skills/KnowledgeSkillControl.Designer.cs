namespace Chummer.UI.Skills
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
            this.lblModifiedRating = new System.Windows.Forms.Label();
            this.cboSpec = new System.Windows.Forms.ComboBox();
            this.chkKarma = new System.Windows.Forms.CheckBox();
            this.cmdDelete = new System.Windows.Forms.Button();
            this.cboSkill = new System.Windows.Forms.ComboBox();
            this.cboType = new System.Windows.Forms.ComboBox();
            this.lblRating = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.lblSpec = new System.Windows.Forms.Label();
            this.btnCareerIncrease = new System.Windows.Forms.Button();
            this.btnAddSpec = new System.Windows.Forms.Button();
            this.nudSkill = new Chummer.helpers.NumericUpDownEx();
            this.nudKarma = new Chummer.helpers.NumericUpDownEx();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).BeginInit();
            this.SuspendLayout();
            // 
            // lblModifiedRating
            // 
            this.lblModifiedRating.AutoSize = true;
            this.lblModifiedRating.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblModifiedRating.Location = new System.Drawing.Point(294, 4);
            this.lblModifiedRating.Name = "lblModifiedRating";
            this.lblModifiedRating.Size = new System.Drawing.Size(47, 13);
            this.lblModifiedRating.TabIndex = 16;
            this.lblModifiedRating.Text = "10 (12)";
            // 
            // cboSpec
            // 
            this.cboSpec.FormattingEnabled = true;
            this.cboSpec.Location = new System.Drawing.Point(344, 0);
            this.cboSpec.Name = "cboSpec";
            this.cboSpec.Size = new System.Drawing.Size(177, 21);
            this.cboSpec.TabIndex = 17;
            this.cboSpec.TextChanged += new System.EventHandler(this.cboSpec_TextChanged);
            // 
            // chkKarma
            // 
            this.chkKarma.AutoSize = true;
            this.chkKarma.Location = new System.Drawing.Point(527, 4);
            this.chkKarma.Name = "chkKarma";
            this.chkKarma.Size = new System.Drawing.Size(15, 14);
            this.chkKarma.TabIndex = 18;
            this.chkKarma.UseVisualStyleBackColor = true;
            // 
            // cmdDelete
            // 
            this.cmdDelete.Location = new System.Drawing.Point(702, 1);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(75, 21);
            this.cmdDelete.TabIndex = 19;
            this.cmdDelete.Tag = "String_Delete";
            this.cmdDelete.Text = "Delete";
            this.cmdDelete.UseVisualStyleBackColor = true;
            // 
            // cboSkill
            // 
            this.cboSkill.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cboSkill.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cboSkill.FormattingEnabled = true;
            this.cboSkill.Location = new System.Drawing.Point(3, 1);
            this.cboSkill.Name = "cboSkill";
            this.cboSkill.Size = new System.Drawing.Size(190, 21);
            this.cboSkill.Sorted = true;
            this.cboSkill.TabIndex = 20;
            this.cboSkill.SelectedIndexChanged += new System.EventHandler(this.cboSkill_SelectedIndexChanged);
            // 
            // cboType
            // 
            this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboType.FormattingEnabled = true;
            this.cboType.Location = new System.Drawing.Point(557, 0);
            this.cboType.Name = "cboType";
            this.cboType.Size = new System.Drawing.Size(139, 21);
            this.cboType.Sorted = true;
            this.cboType.TabIndex = 21;
            // 
            // lblRating
            // 
            this.lblRating.AutoSize = true;
            this.lblRating.Location = new System.Drawing.Point(210, 4);
            this.lblRating.Name = "lblRating";
            this.lblRating.Size = new System.Drawing.Size(13, 13);
            this.lblRating.TabIndex = 22;
            this.lblRating.Text = "0";
            this.lblRating.Visible = false;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(4, 4);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(44, 13);
            this.lblName.TabIndex = 23;
            this.lblName.Text = "[NAME]";
            this.lblName.Visible = false;
            // 
            // lblSpec
            // 
            this.lblSpec.AutoSize = true;
            this.lblSpec.Location = new System.Drawing.Point(347, 3);
            this.lblSpec.Name = "lblSpec";
            this.lblSpec.Size = new System.Drawing.Size(41, 13);
            this.lblSpec.TabIndex = 24;
            this.lblSpec.Text = "[SPEC]";
            this.lblSpec.Visible = false;
            // 
            // btnCareerIncrease
            // 
            this.btnCareerIncrease.Image = global::Chummer.Properties.Resources.add;
            this.btnCareerIncrease.Location = new System.Drawing.Point(229, -2);
            this.btnCareerIncrease.Name = "btnCareerIncrease";
            this.btnCareerIncrease.Size = new System.Drawing.Size(24, 24);
            this.btnCareerIncrease.TabIndex = 25;
            this.btnCareerIncrease.UseVisualStyleBackColor = true;
            this.btnCareerIncrease.Visible = false;
            this.btnCareerIncrease.Click += new System.EventHandler(this.btnCareerIncrease_Click);
            // 
            // btnAddSpec
            // 
            this.btnAddSpec.Image = global::Chummer.Properties.Resources.add;
            this.btnAddSpec.Location = new System.Drawing.Point(527, -2);
            this.btnAddSpec.Name = "btnAddSpec";
            this.btnAddSpec.Size = new System.Drawing.Size(24, 24);
            this.btnAddSpec.TabIndex = 26;
            this.btnAddSpec.UseVisualStyleBackColor = true;
            this.btnAddSpec.Visible = false;
            this.btnAddSpec.Click += new System.EventHandler(this.btnAddSpec_Click);
            // 
            // nudSkill
            // 
            this.nudSkill.InterceptMouseWheel = Chummer.helpers.NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver;
            this.nudSkill.Location = new System.Drawing.Point(206, 1);
            this.nudSkill.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudSkill.Name = "nudSkill";
            this.nudSkill.ShowUpDownButtons = Chummer.helpers.NumericUpDownEx.ShowUpDownButtonsMode.Always;
            this.nudSkill.Size = new System.Drawing.Size(40, 20);
            this.nudSkill.TabIndex = 15;
            this.nudSkill.ValueChanged += new System.EventHandler(this.RatingChanged);
            // 
            // nudKarma
            // 
            this.nudKarma.InterceptMouseWheel = Chummer.helpers.NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver;
            this.nudKarma.Location = new System.Drawing.Point(248, 1);
            this.nudKarma.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudKarma.Name = "nudKarma";
            this.nudKarma.ShowUpDownButtons = Chummer.helpers.NumericUpDownEx.ShowUpDownButtonsMode.Always;
            this.nudKarma.Size = new System.Drawing.Size(40, 20);
            this.nudKarma.TabIndex = 14;
            this.nudKarma.ValueChanged += new System.EventHandler(this.RatingChanged);
            // 
            // KnowledgeSkillControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnAddSpec);
            this.Controls.Add(this.btnCareerIncrease);
            this.Controls.Add(this.lblSpec);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.lblRating);
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
            this.Size = new System.Drawing.Size(800, 23);
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Chummer.helpers.NumericUpDownEx nudKarma;
        private Chummer.helpers.NumericUpDownEx nudSkill;
        private System.Windows.Forms.Label lblModifiedRating;
        private System.Windows.Forms.ComboBox cboSpec;
        private System.Windows.Forms.CheckBox chkKarma;
        private System.Windows.Forms.Button cmdDelete;
        private System.Windows.Forms.ComboBox cboSkill;
        private System.Windows.Forms.ComboBox cboType;
        private System.Windows.Forms.Label lblRating;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblSpec;
        private System.Windows.Forms.Button btnCareerIncrease;
        private System.Windows.Forms.Button btnAddSpec;
    }
}
