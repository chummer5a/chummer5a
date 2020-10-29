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
                _objGraphics?.Dispose();
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
            this.nudKarma = new Chummer.NumericUpDownEx();
            this.nudSkill = new Chummer.NumericUpDownEx();
            this.btnCareerIncrease = new Chummer.ButtonWithToolTip();
            this.lblName = new Chummer.LabelWithToolTip();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
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
            this.lblGroupRating.Location = new System.Drawing.Point(157, 5);
            this.lblGroupRating.MinimumSize = new System.Drawing.Size(25, 0);
            this.lblGroupRating.Name = "lblGroupRating";
            this.lblGroupRating.Size = new System.Drawing.Size(25, 13);
            this.lblGroupRating.TabIndex = 7;
            this.lblGroupRating.Text = "00";
            this.lblGroupRating.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudKarma
            // 
            this.nudKarma.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.nudKarma.AutoSize = true;
            this.nudKarma.InterceptMouseWheel = Chummer.NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver;
            this.nudKarma.Location = new System.Drawing.Point(116, 2);
            this.nudKarma.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
            this.nudSkill.Location = new System.Drawing.Point(75, 2);
            this.nudSkill.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.nudSkill.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudSkill.Name = "nudSkill";
            this.nudSkill.Size = new System.Drawing.Size(35, 20);
            this.nudSkill.TabIndex = 2;
            // 
            // btnCareerIncrease
            // 
            this.btnCareerIncrease.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnCareerIncrease.AutoSize = true;
            this.btnCareerIncrease.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnCareerIncrease.Image = global::Chummer.Properties.Resources.add;
            this.btnCareerIncrease.Location = new System.Drawing.Point(188, 0);
            this.btnCareerIncrease.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.btnCareerIncrease.Name = "btnCareerIncrease";
            this.btnCareerIncrease.Padding = new System.Windows.Forms.Padding(1);
            this.btnCareerIncrease.Size = new System.Drawing.Size(24, 24);
            this.btnCareerIncrease.TabIndex = 22;
            this.btnCareerIncrease.ToolTipText = "";
            this.btnCareerIncrease.UseVisualStyleBackColor = true;
            this.btnCareerIncrease.Click += new System.EventHandler(this.btnCareerIncrease_Click);
            // 
            // lblName
            // 
            this.lblName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(3, 5);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(66, 13);
            this.lblName.TabIndex = 6;
            this.lblName.Text = "[groupname]";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblName.ToolTipText = "";
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 6;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.nudSkill, 2, 0);
            this.tlpMain.Controls.Add(this.nudKarma, 3, 0);
            this.tlpMain.Controls.Add(this.lblGroupRating, 4, 0);
            this.tlpMain.Controls.Add(this.btnCareerIncrease, 5, 0);
            this.tlpMain.Controls.Add(this.lblName, 0, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 1;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(215, 24);
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
            this.Size = new System.Drawing.Size(215, 24);
            this.MouseLeave += new System.EventHandler(this.OnMouseLeave);
            this.DpiChangedAfterParent += new System.EventHandler(this.SkillGroupControl_DpiChangedAfterParent);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
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
        private BufferedTableLayoutPanel tlpMain;
    }
}
