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
            this.components = new System.ComponentModel.Container();
            this.lblGroupRating = new System.Windows.Forms.Label();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.flpRightCareer = new System.Windows.Forms.FlowLayoutPanel();
            this.flpRightCreate = new System.Windows.Forms.FlowLayoutPanel();
            this.lblName = new Chummer.LabelWithToolTip();
            this.btnCareerIncrease = new Chummer.ButtonWithToolTip();
            this.nudKarma = new Chummer.NumericUpDownEx();
            this.nudSkill = new Chummer.NumericUpDownEx();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.pnlRight.SuspendLayout();
            this.flpRightCareer.SuspendLayout();
            this.flpRightCreate.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblGroupRating
            // 
            this.lblGroupRating.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblGroupRating.AutoSize = true;
            this.lblGroupRating.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGroupRating.Location = new System.Drawing.Point(3, 5);
            this.lblGroupRating.MinimumSize = new System.Drawing.Size(21, 0);
            this.lblGroupRating.Name = "lblGroupRating";
            this.lblGroupRating.Size = new System.Drawing.Size(21, 13);
            this.lblGroupRating.TabIndex = 7;
            this.lblGroupRating.Text = "00";
            this.lblGroupRating.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlRight
            // 
            this.pnlRight.AutoSize = true;
            this.pnlRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlRight.Controls.Add(this.flpRightCareer);
            this.pnlRight.Controls.Add(this.flpRightCreate);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Location = new System.Drawing.Point(72, 0);
            this.pnlRight.Margin = new System.Windows.Forms.Padding(0);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Size = new System.Drawing.Size(82, 24);
            this.pnlRight.TabIndex = 8;
            // 
            // flpRightCareer
            // 
            this.flpRightCareer.AutoSize = true;
            this.flpRightCareer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpRightCareer.Controls.Add(this.btnCareerIncrease);
            this.flpRightCareer.Controls.Add(this.lblGroupRating);
            this.flpRightCareer.Dock = System.Windows.Forms.DockStyle.Right;
            this.flpRightCareer.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpRightCareer.Location = new System.Drawing.Point(25, 0);
            this.flpRightCareer.Name = "flpRightCareer";
            this.flpRightCareer.Size = new System.Drawing.Size(57, 24);
            this.flpRightCareer.TabIndex = 0;
            this.flpRightCareer.WrapContents = false;
            // 
            // flpRightCreate
            // 
            this.flpRightCreate.AutoSize = true;
            this.flpRightCreate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpRightCreate.Controls.Add(this.nudKarma);
            this.flpRightCreate.Controls.Add(this.nudSkill);
            this.flpRightCreate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpRightCreate.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpRightCreate.Location = new System.Drawing.Point(0, 0);
            this.flpRightCreate.Margin = new System.Windows.Forms.Padding(0);
            this.flpRightCreate.Name = "flpRightCreate";
            this.flpRightCreate.Size = new System.Drawing.Size(82, 24);
            this.flpRightCreate.TabIndex = 7;
            this.flpRightCreate.WrapContents = false;
            // 
            // lblName
            // 
            this.lblName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(3, 5);
            this.lblName.Margin = new System.Windows.Forms.Padding(3, 5, 3, 6);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(66, 13);
            this.lblName.TabIndex = 6;
            this.lblName.Text = "[groupname]";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblName.ToolTipText = "";
            // 
            // btnCareerIncrease
            // 
            this.btnCareerIncrease.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnCareerIncrease.AutoSize = true;
            this.btnCareerIncrease.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnCareerIncrease.Image = global::Chummer.Properties.Resources.add;
            this.btnCareerIncrease.Location = new System.Drawing.Point(30, 0);
            this.btnCareerIncrease.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.btnCareerIncrease.Name = "btnCareerIncrease";
            this.btnCareerIncrease.Padding = new System.Windows.Forms.Padding(1);
            this.btnCareerIncrease.Size = new System.Drawing.Size(24, 24);
            this.btnCareerIncrease.TabIndex = 22;
            this.btnCareerIncrease.ToolTipText = "";
            this.btnCareerIncrease.UseVisualStyleBackColor = true;
            this.btnCareerIncrease.Click += new System.EventHandler(this.btnCareerIncrease_Click);
            // 
            // nudKarma
            // 
            this.nudKarma.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.nudKarma.AutoSize = true;
            this.nudKarma.InterceptMouseWheel = Chummer.NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver;
            this.nudKarma.Location = new System.Drawing.Point(44, 0);
            this.nudKarma.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.nudKarma.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudKarma.Name = "nudKarma";
            this.nudKarma.Size = new System.Drawing.Size(35, 20);
            this.nudKarma.TabIndex = 5;
            // 
            // nudSkill
            // 
            this.nudSkill.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.nudSkill.AutoSize = true;
            this.nudSkill.InterceptMouseWheel = Chummer.NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver;
            this.nudSkill.Location = new System.Drawing.Point(3, 0);
            this.nudSkill.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.nudSkill.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudSkill.Name = "nudSkill";
            this.nudSkill.Size = new System.Drawing.Size(35, 20);
            this.nudSkill.TabIndex = 2;
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.lblName, 0, 0);
            this.tlpMain.Controls.Add(this.pnlRight, 1, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 1;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(154, 24);
            this.tlpMain.TabIndex = 25;
            // 
            // SkillGroupControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "SkillGroupControl";
            this.Size = new System.Drawing.Size(154, 24);
            this.MouseLeave += new System.EventHandler(this.OnMouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            this.pnlRight.ResumeLayout(false);
            this.pnlRight.PerformLayout();
            this.flpRightCareer.ResumeLayout(false);
            this.flpRightCareer.PerformLayout();
            this.flpRightCreate.ResumeLayout(false);
            this.flpRightCreate.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private NumericUpDownEx nudSkill;
        private NumericUpDownEx nudKarma;
        private LabelWithToolTip lblName;
        private System.Windows.Forms.Label lblGroupRating;
        private ButtonWithToolTip btnCareerIncrease;
        private System.Windows.Forms.FlowLayoutPanel flpRightCreate;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.FlowLayoutPanel flpRightCareer;
        private BufferedTableLayoutPanel tlpMain;
    }
}
