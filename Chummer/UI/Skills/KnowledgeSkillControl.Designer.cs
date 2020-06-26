namespace Chummer.UI.Skills
{
    public sealed partial class KnowledgeSkillControl
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
                UnbindKnowledgeSkillControl();
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
            this.lblModifiedRating = new Chummer.LabelWithToolTip();
            this.cboSpec = new Chummer.ElasticComboBox();
            this.chkKarma = new System.Windows.Forms.CheckBox();
            this.cmdDelete = new System.Windows.Forms.Button();
            this.cboName = new Chummer.ElasticComboBox();
            this.cboType = new Chummer.ElasticComboBox();
            this.lblRating = new System.Windows.Forms.Label();
            this.lblSpec = new System.Windows.Forms.Label();
            this.btnCareerIncrease = new Chummer.ButtonWithToolTip();
            this.btnAddSpec = new Chummer.ButtonWithToolTip();
            this.nudSkill = new Chummer.NumericUpDownEx();
            this.nudKarma = new Chummer.NumericUpDownEx();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpRight = new System.Windows.Forms.TableLayoutPanel();
            this.pnlSpecs = new System.Windows.Forms.Panel();
            this.tlpSpecsCareer = new System.Windows.Forms.TableLayoutPanel();
            this.tlpSpecsCreate = new System.Windows.Forms.TableLayoutPanel();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.flpButtonsCareer = new System.Windows.Forms.FlowLayoutPanel();
            this.flpButtonsCreate = new System.Windows.Forms.FlowLayoutPanel();
            this.tlpName = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblName = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.tlpRight.SuspendLayout();
            this.pnlSpecs.SuspendLayout();
            this.tlpSpecsCareer.SuspendLayout();
            this.tlpSpecsCreate.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.flpButtonsCareer.SuspendLayout();
            this.flpButtonsCreate.SuspendLayout();
            this.tlpName.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblModifiedRating
            // 
            this.lblModifiedRating.AutoSize = true;
            this.lblModifiedRating.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblModifiedRating.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblModifiedRating.Location = new System.Drawing.Point(456, 0);
            this.lblModifiedRating.MinimumSize = new System.Drawing.Size(50, 0);
            this.lblModifiedRating.Name = "lblModifiedRating";
            this.lblModifiedRating.Size = new System.Drawing.Size(50, 24);
            this.lblModifiedRating.TabIndex = 16;
            this.lblModifiedRating.Text = "00 (00)";
            this.lblModifiedRating.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblModifiedRating.ToolTipText = "";
            // 
            // cboSpec
            // 
            this.cboSpec.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSpec.FormattingEnabled = true;
            this.cboSpec.Location = new System.Drawing.Point(3, 1);
            this.cboSpec.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cboSpec.Name = "cboSpec";
            this.cboSpec.Size = new System.Drawing.Size(450, 21);
            this.cboSpec.TabIndex = 17;
            this.cboSpec.TabStop = false;
            this.cboSpec.TooltipText = "";
            // 
            // chkKarma
            // 
            this.chkKarma.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.chkKarma.AutoSize = true;
            this.chkKarma.Location = new System.Drawing.Point(459, 5);
            this.chkKarma.Name = "chkKarma";
            this.chkKarma.Size = new System.Drawing.Size(15, 14);
            this.chkKarma.TabIndex = 18;
            this.chkKarma.UseVisualStyleBackColor = true;
            // 
            // cmdDelete
            // 
            this.cmdDelete.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmdDelete.AutoSize = true;
            this.cmdDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDelete.Location = new System.Drawing.Point(161, 0);
            this.cmdDelete.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(48, 23);
            this.cmdDelete.TabIndex = 19;
            this.cmdDelete.Tag = "String_Delete";
            this.cmdDelete.Text = "Delete";
            this.cmdDelete.UseVisualStyleBackColor = true;
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // cboName
            // 
            this.cboName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboName.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cboName.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cboName.DropDownWidth = 200;
            this.cboName.FormattingEnabled = true;
            this.cboName.Location = new System.Drawing.Point(50, 2);
            this.cboName.Margin = new System.Windows.Forms.Padding(3, 1, 3, 0);
            this.cboName.Name = "cboName";
            this.cboName.Size = new System.Drawing.Size(318, 21);
            this.cboName.Sorted = true;
            this.cboName.TabIndex = 20;
            this.cboName.TabStop = false;
            this.cboName.TooltipText = "";
            // 
            // cboType
            // 
            this.cboType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboType.FormattingEnabled = true;
            this.cboType.Location = new System.Drawing.Point(3, 1);
            this.cboType.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cboType.Name = "cboType";
            this.cboType.Size = new System.Drawing.Size(152, 21);
            this.cboType.Sorted = true;
            this.cboType.TabIndex = 21;
            this.cboType.TooltipText = "";
            // 
            // lblRating
            // 
            this.lblRating.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblRating.AutoSize = true;
            this.lblRating.Location = new System.Drawing.Point(29, 5);
            this.lblRating.MinimumSize = new System.Drawing.Size(20, 0);
            this.lblRating.Name = "lblRating";
            this.lblRating.Size = new System.Drawing.Size(20, 13);
            this.lblRating.TabIndex = 22;
            this.lblRating.Text = "00";
            this.lblRating.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSpec
            // 
            this.lblSpec.AutoSize = true;
            this.lblSpec.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblSpec.Location = new System.Drawing.Point(3, 0);
            this.lblSpec.Name = "lblSpec";
            this.lblSpec.Size = new System.Drawing.Size(41, 24);
            this.lblSpec.TabIndex = 24;
            this.lblSpec.Text = "[SPEC]";
            this.lblSpec.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnCareerIncrease
            // 
            this.btnCareerIncrease.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnCareerIncrease.AutoSize = true;
            this.btnCareerIncrease.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnCareerIncrease.Image = global::Chummer.Properties.Resources.add;
            this.btnCareerIncrease.Location = new System.Drawing.Point(55, 0);
            this.btnCareerIncrease.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.btnCareerIncrease.MinimumSize = new System.Drawing.Size(24, 24);
            this.btnCareerIncrease.Name = "btnCareerIncrease";
            this.btnCareerIncrease.Size = new System.Drawing.Size(24, 24);
            this.btnCareerIncrease.TabIndex = 25;
            this.btnCareerIncrease.ToolTipText = "";
            this.btnCareerIncrease.UseVisualStyleBackColor = true;
            this.btnCareerIncrease.Click += new System.EventHandler(this.btnCareerIncrease_Click);
            // 
            // btnAddSpec
            // 
            this.btnAddSpec.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnAddSpec.AutoSize = true;
            this.btnAddSpec.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAddSpec.Image = global::Chummer.Properties.Resources.add;
            this.btnAddSpec.Location = new System.Drawing.Point(450, 0);
            this.btnAddSpec.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.btnAddSpec.MinimumSize = new System.Drawing.Size(24, 24);
            this.btnAddSpec.Name = "btnAddSpec";
            this.btnAddSpec.Size = new System.Drawing.Size(24, 24);
            this.btnAddSpec.TabIndex = 26;
            this.btnAddSpec.ToolTipText = "";
            this.btnAddSpec.UseVisualStyleBackColor = true;
            this.btnAddSpec.Click += new System.EventHandler(this.btnAddSpec_Click);
            // 
            // nudSkill
            // 
            this.nudSkill.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.nudSkill.AutoSize = true;
            this.nudSkill.InterceptMouseWheel = Chummer.NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver;
            this.nudSkill.Location = new System.Drawing.Point(3, 2);
            this.nudSkill.Margin = new System.Windows.Forms.Padding(3, 2, 3, 0);
            this.nudSkill.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudSkill.Name = "nudSkill";
            this.nudSkill.Size = new System.Drawing.Size(35, 20);
            this.nudSkill.TabIndex = 15;
            // 
            // nudKarma
            // 
            this.nudKarma.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.nudKarma.AutoSize = true;
            this.nudKarma.InterceptMouseWheel = Chummer.NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver;
            this.nudKarma.Location = new System.Drawing.Point(44, 2);
            this.nudKarma.Margin = new System.Windows.Forms.Padding(3, 2, 3, 0);
            this.nudKarma.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudKarma.Name = "nudKarma";
            this.nudKarma.Size = new System.Drawing.Size(35, 20);
            this.nudKarma.TabIndex = 14;
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 5;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Controls.Add(this.lblModifiedRating, 2, 0);
            this.tlpMain.Controls.Add(this.tlpRight, 4, 0);
            this.tlpMain.Controls.Add(this.pnlSpecs, 3, 0);
            this.tlpMain.Controls.Add(this.pnlButtons, 1, 0);
            this.tlpMain.Controls.Add(this.tlpName, 0, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 1;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(1198, 24);
            this.tlpMain.TabIndex = 27;
            // 
            // tlpRight
            // 
            this.tlpRight.AutoSize = true;
            this.tlpRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.ColumnCount = 2;
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.Controls.Add(this.cmdDelete, 1, 0);
            this.tlpRight.Controls.Add(this.cboType, 0, 0);
            this.tlpRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRight.Location = new System.Drawing.Point(986, 0);
            this.tlpRight.Margin = new System.Windows.Forms.Padding(0);
            this.tlpRight.Name = "tlpRight";
            this.tlpRight.RowCount = 1;
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRight.Size = new System.Drawing.Size(212, 24);
            this.tlpRight.TabIndex = 24;
            // 
            // pnlSpecs
            // 
            this.pnlSpecs.AutoSize = true;
            this.pnlSpecs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlSpecs.Controls.Add(this.tlpSpecsCareer);
            this.pnlSpecs.Controls.Add(this.tlpSpecsCreate);
            this.pnlSpecs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSpecs.Location = new System.Drawing.Point(509, 0);
            this.pnlSpecs.Margin = new System.Windows.Forms.Padding(0);
            this.pnlSpecs.Name = "pnlSpecs";
            this.pnlSpecs.Size = new System.Drawing.Size(477, 24);
            this.pnlSpecs.TabIndex = 25;
            // 
            // tlpSpecsCareer
            // 
            this.tlpSpecsCareer.AutoSize = true;
            this.tlpSpecsCareer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpSpecsCareer.ColumnCount = 2;
            this.tlpSpecsCareer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSpecsCareer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpSpecsCareer.Controls.Add(this.btnAddSpec, 1, 0);
            this.tlpSpecsCareer.Controls.Add(this.lblSpec, 0, 0);
            this.tlpSpecsCareer.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpSpecsCareer.Location = new System.Drawing.Point(0, 0);
            this.tlpSpecsCareer.Margin = new System.Windows.Forms.Padding(0);
            this.tlpSpecsCareer.Name = "tlpSpecsCareer";
            this.tlpSpecsCareer.RowCount = 1;
            this.tlpSpecsCareer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSpecsCareer.Size = new System.Drawing.Size(477, 24);
            this.tlpSpecsCareer.TabIndex = 0;
            // 
            // tlpSpecsCreate
            // 
            this.tlpSpecsCreate.AutoSize = true;
            this.tlpSpecsCreate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpSpecsCreate.ColumnCount = 4;
            this.tlpSpecsCreate.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSpecsCreate.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpSpecsCreate.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpSpecsCreate.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpSpecsCreate.Controls.Add(this.cboSpec, 0, 0);
            this.tlpSpecsCreate.Controls.Add(this.chkKarma, 1, 0);
            this.tlpSpecsCreate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSpecsCreate.Location = new System.Drawing.Point(0, 0);
            this.tlpSpecsCreate.Margin = new System.Windows.Forms.Padding(0);
            this.tlpSpecsCreate.Name = "tlpSpecsCreate";
            this.tlpSpecsCreate.RowCount = 1;
            this.tlpSpecsCreate.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSpecsCreate.Size = new System.Drawing.Size(477, 24);
            this.tlpSpecsCreate.TabIndex = 22;
            // 
            // pnlButtons
            // 
            this.pnlButtons.AutoSize = true;
            this.pnlButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlButtons.Controls.Add(this.flpButtonsCareer);
            this.pnlButtons.Controls.Add(this.flpButtonsCreate);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlButtons.Location = new System.Drawing.Point(371, 0);
            this.pnlButtons.Margin = new System.Windows.Forms.Padding(0);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(82, 24);
            this.pnlButtons.TabIndex = 26;
            // 
            // flpButtonsCareer
            // 
            this.flpButtonsCareer.AutoSize = true;
            this.flpButtonsCareer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpButtonsCareer.Controls.Add(this.btnCareerIncrease);
            this.flpButtonsCareer.Controls.Add(this.lblRating);
            this.flpButtonsCareer.Dock = System.Windows.Forms.DockStyle.Top;
            this.flpButtonsCareer.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpButtonsCareer.Location = new System.Drawing.Point(0, 0);
            this.flpButtonsCareer.Margin = new System.Windows.Forms.Padding(0);
            this.flpButtonsCareer.Name = "flpButtonsCareer";
            this.flpButtonsCareer.Size = new System.Drawing.Size(82, 24);
            this.flpButtonsCareer.TabIndex = 0;
            this.flpButtonsCareer.WrapContents = false;
            // 
            // flpButtonsCreate
            // 
            this.flpButtonsCreate.AutoSize = true;
            this.flpButtonsCreate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpButtonsCreate.Controls.Add(this.nudKarma);
            this.flpButtonsCreate.Controls.Add(this.nudSkill);
            this.flpButtonsCreate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpButtonsCreate.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpButtonsCreate.Location = new System.Drawing.Point(0, 0);
            this.flpButtonsCreate.Margin = new System.Windows.Forms.Padding(0);
            this.flpButtonsCreate.Name = "flpButtonsCreate";
            this.flpButtonsCreate.Size = new System.Drawing.Size(82, 24);
            this.flpButtonsCreate.TabIndex = 23;
            this.flpButtonsCreate.WrapContents = false;
            // 
            // tlpName
            // 
            this.tlpName.AutoSize = true;
            this.tlpName.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpName.ColumnCount = 2;
            this.tlpName.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpName.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpName.Controls.Add(this.lblName, 0, 0);
            this.tlpName.Controls.Add(this.cboName, 1, 0);
            this.tlpName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpName.Location = new System.Drawing.Point(0, 0);
            this.tlpName.Margin = new System.Windows.Forms.Padding(0);
            this.tlpName.Name = "tlpName";
            this.tlpName.RowCount = 1;
            this.tlpName.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpName.Size = new System.Drawing.Size(371, 24);
            this.tlpName.TabIndex = 27;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblName.Location = new System.Drawing.Point(3, 0);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(41, 24);
            this.lblName.TabIndex = 21;
            this.lblName.Text = "[Name]";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // KnowledgeSkillControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumSize = new System.Drawing.Size(2, 24);
            this.Name = "KnowledgeSkillControl";
            this.Size = new System.Drawing.Size(1198, 24);
            this.MouseLeave += new System.EventHandler(this.OnMouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpRight.ResumeLayout(false);
            this.tlpRight.PerformLayout();
            this.pnlSpecs.ResumeLayout(false);
            this.pnlSpecs.PerformLayout();
            this.tlpSpecsCareer.ResumeLayout(false);
            this.tlpSpecsCareer.PerformLayout();
            this.tlpSpecsCreate.ResumeLayout(false);
            this.tlpSpecsCreate.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.pnlButtons.PerformLayout();
            this.flpButtonsCareer.ResumeLayout(false);
            this.flpButtonsCareer.PerformLayout();
            this.flpButtonsCreate.ResumeLayout(false);
            this.flpButtonsCreate.PerformLayout();
            this.tlpName.ResumeLayout(false);
            this.tlpName.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private NumericUpDownEx nudKarma;
        private NumericUpDownEx nudSkill;
        private LabelWithToolTip lblModifiedRating;
        private ElasticComboBox cboSpec;
        private System.Windows.Forms.CheckBox chkKarma;
        private System.Windows.Forms.Button cmdDelete;
        private ElasticComboBox cboName;
        private ElasticComboBox cboType;
        private System.Windows.Forms.Label lblRating;
        private System.Windows.Forms.Label lblSpec;
        private ButtonWithToolTip btnCareerIncrease;
        private ButtonWithToolTip btnAddSpec;
        private BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.TableLayoutPanel tlpSpecsCreate;
        private System.Windows.Forms.FlowLayoutPanel flpButtonsCreate;
        private System.Windows.Forms.TableLayoutPanel tlpRight;
        private System.Windows.Forms.Panel pnlSpecs;
        private System.Windows.Forms.TableLayoutPanel tlpSpecsCareer;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.FlowLayoutPanel flpButtonsCareer;
        private BufferedTableLayoutPanel tlpName;
        private System.Windows.Forms.Label lblName;
    }
}
