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
                _objGraphics?.Dispose();
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
            this.chkKarma = new Chummer.ColorableCheckBox(this.components);
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
            this.tlpLeft = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblName = new System.Windows.Forms.Label();
            this.tlpMiddle = new System.Windows.Forms.TableLayoutPanel();
            this.chkNativeLanguage = new Chummer.ColorableCheckBox(this.components);
            this.tlpRight = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.tlpLeft.SuspendLayout();
            this.tlpMiddle.SuspendLayout();
            this.tlpRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblModifiedRating
            // 
            this.lblModifiedRating.AutoSize = true;
            this.lblModifiedRating.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblModifiedRating.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblModifiedRating.Location = new System.Drawing.Point(3, 0);
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
            this.cboSpec.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cboSpec.FormattingEnabled = true;
            this.cboSpec.Location = new System.Drawing.Point(169, 1);
            this.cboSpec.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cboSpec.Name = "cboSpec";
            this.cboSpec.Size = new System.Drawing.Size(65, 21);
            this.cboSpec.TabIndex = 17;
            this.cboSpec.TabStop = false;
            this.cboSpec.TooltipText = "";
            // 
            // chkKarma
            // 
            this.chkKarma.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkKarma.AutoSize = true;
            this.chkKarma.DefaultColorScheme = true;
            this.chkKarma.Location = new System.Drawing.Point(240, 5);
            this.chkKarma.Name = "chkKarma";
            this.chkKarma.Size = new System.Drawing.Size(15, 14);
            this.chkKarma.TabIndex = 18;
            this.chkKarma.UseVisualStyleBackColor = true;
            // 
            // cmdDelete
            // 
            this.cmdDelete.AutoSize = true;
            this.cmdDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDelete.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdDelete.Location = new System.Drawing.Point(165, 0);
            this.cmdDelete.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(48, 24);
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
            this.cboName.Size = new System.Drawing.Size(163, 21);
            this.cboName.Sorted = true;
            this.cboName.TabIndex = 20;
            this.cboName.TabStop = false;
            this.cboName.TooltipText = "";
            this.cboName.TextChanged += new System.EventHandler(this.cboName_TextChanged);
            // 
            // cboType
            // 
            this.cboType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboType.FormattingEnabled = true;
            this.cboType.Location = new System.Drawing.Point(3, 1);
            this.cboType.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cboType.Name = "cboType";
            this.cboType.Size = new System.Drawing.Size(156, 21);
            this.cboType.Sorted = true;
            this.cboType.TabIndex = 21;
            this.cboType.TooltipText = "";
            // 
            // lblRating
            // 
            this.lblRating.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblRating.AutoSize = true;
            this.lblRating.Location = new System.Drawing.Point(301, 5);
            this.lblRating.MinimumSize = new System.Drawing.Size(25, 0);
            this.lblRating.Name = "lblRating";
            this.lblRating.Size = new System.Drawing.Size(25, 13);
            this.lblRating.TabIndex = 22;
            this.lblRating.Text = "00";
            this.lblRating.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSpec
            // 
            this.lblSpec.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSpec.AutoSize = true;
            this.lblSpec.Location = new System.Drawing.Point(122, 5);
            this.lblSpec.Name = "lblSpec";
            this.lblSpec.Size = new System.Drawing.Size(41, 13);
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
            this.btnCareerIncrease.Location = new System.Drawing.Point(332, 0);
            this.btnCareerIncrease.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.btnCareerIncrease.Name = "btnCareerIncrease";
            this.btnCareerIncrease.Padding = new System.Windows.Forms.Padding(1);
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
            this.btnAddSpec.Location = new System.Drawing.Point(261, 0);
            this.btnAddSpec.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.btnAddSpec.Name = "btnAddSpec";
            this.btnAddSpec.Padding = new System.Windows.Forms.Padding(1);
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
            this.nudSkill.Location = new System.Drawing.Point(219, 2);
            this.nudSkill.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
            this.nudKarma.Location = new System.Drawing.Point(260, 2);
            this.nudKarma.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
            this.tlpMain.ColumnCount = 7;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Controls.Add(this.nudSkill, 1, 0);
            this.tlpMain.Controls.Add(this.nudKarma, 2, 0);
            this.tlpMain.Controls.Add(this.lblRating, 3, 0);
            this.tlpMain.Controls.Add(this.btnCareerIncrease, 4, 0);
            this.tlpMain.Controls.Add(this.tlpLeft, 0, 0);
            this.tlpMain.Controls.Add(this.tlpMiddle, 5, 0);
            this.tlpMain.Controls.Add(this.tlpRight, 6, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 1;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(863, 24);
            this.tlpMain.TabIndex = 27;
            // 
            // tlpLeft
            // 
            this.tlpLeft.AutoSize = true;
            this.tlpLeft.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpLeft.ColumnCount = 2;
            this.tlpLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLeft.Controls.Add(this.lblName, 0, 0);
            this.tlpLeft.Controls.Add(this.cboName, 1, 0);
            this.tlpLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpLeft.Location = new System.Drawing.Point(0, 0);
            this.tlpLeft.Margin = new System.Windows.Forms.Padding(0);
            this.tlpLeft.Name = "tlpLeft";
            this.tlpLeft.RowCount = 1;
            this.tlpLeft.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpLeft.Size = new System.Drawing.Size(216, 24);
            this.tlpLeft.TabIndex = 28;
            // 
            // lblName
            // 
            this.lblName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(3, 5);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(41, 13);
            this.lblName.TabIndex = 21;
            this.lblName.Text = "[Name]";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tlpMiddle
            // 
            this.tlpMiddle.AutoSize = true;
            this.tlpMiddle.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMiddle.ColumnCount = 6;
            this.tlpMiddle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMiddle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMiddle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMiddle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMiddle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMiddle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMiddle.Controls.Add(this.btnAddSpec, 5, 0);
            this.tlpMiddle.Controls.Add(this.chkKarma, 4, 0);
            this.tlpMiddle.Controls.Add(this.lblModifiedRating, 0, 0);
            this.tlpMiddle.Controls.Add(this.chkNativeLanguage, 1, 0);
            this.tlpMiddle.Controls.Add(this.lblSpec, 2, 0);
            this.tlpMiddle.Controls.Add(this.cboSpec, 3, 0);
            this.tlpMiddle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMiddle.Location = new System.Drawing.Point(359, 0);
            this.tlpMiddle.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMiddle.Name = "tlpMiddle";
            this.tlpMiddle.RowCount = 1;
            this.tlpMiddle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMiddle.Size = new System.Drawing.Size(288, 24);
            this.tlpMiddle.TabIndex = 29;
            // 
            // chkNativeLanguage
            // 
            this.chkNativeLanguage.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkNativeLanguage.AutoSize = true;
            this.chkNativeLanguage.DefaultColorScheme = true;
            this.chkNativeLanguage.Location = new System.Drawing.Point(59, 3);
            this.chkNativeLanguage.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.chkNativeLanguage.Name = "chkNativeLanguage";
            this.chkNativeLanguage.Size = new System.Drawing.Size(57, 17);
            this.chkNativeLanguage.TabIndex = 19;
            this.chkNativeLanguage.Tag = "Skill_NativeLanguageLong";
            this.chkNativeLanguage.Text = "Native";
            this.chkNativeLanguage.UseVisualStyleBackColor = true;
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
            this.tlpRight.Location = new System.Drawing.Point(647, 0);
            this.tlpRight.Margin = new System.Windows.Forms.Padding(0);
            this.tlpRight.Name = "tlpRight";
            this.tlpRight.RowCount = 1;
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRight.Size = new System.Drawing.Size(216, 24);
            this.tlpRight.TabIndex = 30;
            // 
            // KnowledgeSkillControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "KnowledgeSkillControl";
            this.Size = new System.Drawing.Size(863, 24);
            this.MouseLeave += new System.EventHandler(this.OnMouseLeave);
            this.DpiChangedAfterParent += new System.EventHandler(this.KnowledgeSkillControl_DpiChangedAfterParent);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpLeft.ResumeLayout(false);
            this.tlpLeft.PerformLayout();
            this.tlpMiddle.ResumeLayout(false);
            this.tlpMiddle.PerformLayout();
            this.tlpRight.ResumeLayout(false);
            this.tlpRight.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private NumericUpDownEx nudKarma;
        private NumericUpDownEx nudSkill;
        private LabelWithToolTip lblModifiedRating;
        private ElasticComboBox cboSpec;
        private Chummer.ColorableCheckBox chkKarma;
        private System.Windows.Forms.Button cmdDelete;
        private ElasticComboBox cboName;
        private ElasticComboBox cboType;
        private System.Windows.Forms.Label lblRating;
        private System.Windows.Forms.Label lblSpec;
        private ButtonWithToolTip btnCareerIncrease;
        private ButtonWithToolTip btnAddSpec;
        private BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.Label lblName;
        private BufferedTableLayoutPanel tlpLeft;
        private ColorableCheckBox chkNativeLanguage;
        private System.Windows.Forms.TableLayoutPanel tlpMiddle;
        private System.Windows.Forms.TableLayoutPanel tlpRight;
    }
}
