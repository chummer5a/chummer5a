using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Chummer.UI.Skills
{
    public sealed partial class SkillControl2
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
                _fntItalic?.Dispose();
                _fntItalicName?.Dispose();
                _objGraphics?.Dispose();
                UnbindSkillControl();
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
            this.cmsSkillLabel = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsSkillLabelNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.nudSkill = new Chummer.NumericUpDownEx();
            this.nudKarma = new Chummer.NumericUpDownEx();
            this.lblCareerRating = new System.Windows.Forms.Label();
            this.btnCareerIncrease = new Chummer.ButtonWithToolTip();
            this.lblModifiedRating = new Chummer.LabelWithToolTip();
            this.cmdDelete = new System.Windows.Forms.Button();
            this.lblName = new Chummer.LabelWithToolTip();
            this.pnlAttributes = new System.Windows.Forms.Panel();
            this.btnAttribute = new System.Windows.Forms.Button();
            this.cboSelectAttribute = new Chummer.ElasticComboBox();
            this.chkKarma = new Chummer.ColorableCheckBox(this.components);
            this.btnAddSpec = new Chummer.ButtonWithToolTip();
            this.tlpRight = new System.Windows.Forms.TableLayoutPanel();
            this.cboSpec = new Chummer.ElasticComboBox();
            this.lblCareerSpec = new System.Windows.Forms.Label();
            this.cmsSkillLabel.SuspendLayout();
            this.tlpMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).BeginInit();
            this.pnlAttributes.SuspendLayout();
            this.tlpRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmsSkillLabel
            // 
            this.cmsSkillLabel.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.cmsSkillLabel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsSkillLabelNotes});
            this.cmsSkillLabel.Name = "cmsWeapon";
            this.cmsSkillLabel.Size = new System.Drawing.Size(110, 30);
            // 
            // tsSkillLabelNotes
            // 
            this.tsSkillLabelNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsSkillLabelNotes.Name = "tsSkillLabelNotes";
            this.tsSkillLabelNotes.Size = new System.Drawing.Size(109, 26);
            this.tsSkillLabelNotes.Tag = "Menu_Notes";
            this.tsSkillLabelNotes.Text = "&Notes";
            this.tsSkillLabelNotes.Click += new System.EventHandler(this.tsSkillLabelNotes_Click);
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 8;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Controls.Add(this.nudSkill, 2, 0);
            this.tlpMain.Controls.Add(this.nudKarma, 3, 0);
            this.tlpMain.Controls.Add(this.lblCareerRating, 4, 0);
            this.tlpMain.Controls.Add(this.btnCareerIncrease, 5, 0);
            this.tlpMain.Controls.Add(this.lblModifiedRating, 6, 0);
            this.tlpMain.Controls.Add(this.lblName, 0, 0);
            this.tlpMain.Controls.Add(this.pnlAttributes, 1, 0);
            this.tlpMain.Controls.Add(this.tlpRight, 7, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 1;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(594, 24);
            this.tlpMain.TabIndex = 28;
            // 
            // nudSkill
            // 
            this.nudSkill.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.nudSkill.AutoSize = true;
            this.nudSkill.InterceptMouseWheel = Chummer.NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver;
            this.nudSkill.Location = new System.Drawing.Point(93, 2);
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
            this.nudKarma.Location = new System.Drawing.Point(134, 2);
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
            // lblCareerRating
            // 
            this.lblCareerRating.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCareerRating.AutoSize = true;
            this.lblCareerRating.Location = new System.Drawing.Point(175, 5);
            this.lblCareerRating.MinimumSize = new System.Drawing.Size(25, 0);
            this.lblCareerRating.Name = "lblCareerRating";
            this.lblCareerRating.Size = new System.Drawing.Size(25, 13);
            this.lblCareerRating.TabIndex = 20;
            this.lblCareerRating.Text = "00";
            this.lblCareerRating.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnCareerIncrease
            // 
            this.btnCareerIncrease.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnCareerIncrease.AutoSize = true;
            this.btnCareerIncrease.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnCareerIncrease.Image = global::Chummer.Properties.Resources.add;
            this.btnCareerIncrease.Location = new System.Drawing.Point(206, 0);
            this.btnCareerIncrease.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.btnCareerIncrease.Name = "btnCareerIncrease";
            this.btnCareerIncrease.Padding = new System.Windows.Forms.Padding(1);
            this.btnCareerIncrease.Size = new System.Drawing.Size(24, 24);
            this.btnCareerIncrease.TabIndex = 21;
            this.btnCareerIncrease.ToolTipText = "";
            this.btnCareerIncrease.UseVisualStyleBackColor = true;
            this.btnCareerIncrease.Click += new System.EventHandler(this.btnCareerIncrease_Click);
            // 
            // lblModifiedRating
            // 
            this.lblModifiedRating.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblModifiedRating.AutoSize = true;
            this.lblModifiedRating.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblModifiedRating.Location = new System.Drawing.Point(236, 5);
            this.lblModifiedRating.MinimumSize = new System.Drawing.Size(50, 0);
            this.lblModifiedRating.Name = "lblModifiedRating";
            this.lblModifiedRating.Size = new System.Drawing.Size(50, 13);
            this.lblModifiedRating.TabIndex = 16;
            this.lblModifiedRating.Text = "00 (00)";
            this.lblModifiedRating.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblModifiedRating.ToolTipText = "";
            // 
            // cmdDelete
            // 
            this.cmdDelete.AutoSize = true;
            this.cmdDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDelete.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdDelete.Location = new System.Drawing.Point(254, 0);
            this.cmdDelete.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(48, 24);
            this.cmdDelete.TabIndex = 19;
            this.cmdDelete.Tag = "String_Delete";
            this.cmdDelete.Text = "Delete";
            this.cmdDelete.UseVisualStyleBackColor = true;
            this.cmdDelete.Visible = false;
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // lblName
            // 
            this.lblName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblName.AutoSize = true;
            this.lblName.ContextMenuStrip = this.cmsSkillLabel;
            this.lblName.Location = new System.Drawing.Point(3, 5);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(41, 13);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "[Name]";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblName.ToolTipText = "";
            this.lblName.Click += new System.EventHandler(this.lblName_Click);
            // 
            // pnlAttributes
            // 
            this.pnlAttributes.AutoSize = true;
            this.pnlAttributes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlAttributes.Controls.Add(this.btnAttribute);
            this.pnlAttributes.Controls.Add(this.cboSelectAttribute);
            this.pnlAttributes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlAttributes.Location = new System.Drawing.Point(47, 0);
            this.pnlAttributes.Margin = new System.Windows.Forms.Padding(0);
            this.pnlAttributes.MinimumSize = new System.Drawing.Size(40, 0);
            this.pnlAttributes.Name = "pnlAttributes";
            this.pnlAttributes.Size = new System.Drawing.Size(43, 24);
            this.pnlAttributes.TabIndex = 33;
            // 
            // btnAttribute
            // 
            this.btnAttribute.AutoSize = true;
            this.btnAttribute.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAttribute.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAttribute.FlatAppearance.BorderSize = 0;
            this.btnAttribute.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAttribute.Location = new System.Drawing.Point(0, 0);
            this.btnAttribute.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.btnAttribute.Name = "btnAttribute";
            this.btnAttribute.Size = new System.Drawing.Size(43, 24);
            this.btnAttribute.TabIndex = 24;
            this.btnAttribute.Text = "ATR";
            this.btnAttribute.UseVisualStyleBackColor = true;
            this.btnAttribute.Click += new System.EventHandler(this.btnAttribute_Click);
            // 
            // cboSelectAttribute
            // 
            this.cboSelectAttribute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSelectAttribute.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSelectAttribute.FormattingEnabled = true;
            this.cboSelectAttribute.Location = new System.Drawing.Point(0, -12);
            this.cboSelectAttribute.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cboSelectAttribute.Name = "cboSelectAttribute";
            this.cboSelectAttribute.Size = new System.Drawing.Size(40, 21);
            this.cboSelectAttribute.TabIndex = 25;
            this.cboSelectAttribute.TooltipText = "";
            this.cboSelectAttribute.Visible = false;
            this.cboSelectAttribute.DropDownClosed += new System.EventHandler(this.cboSelectAttribute_Closed);
            // 
            // chkKarma
            // 
            this.chkKarma.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkKarma.AutoSize = true;
            this.chkKarma.DefaultColorScheme = true;
            this.chkKarma.Location = new System.Drawing.Point(203, 5);
            this.chkKarma.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.chkKarma.Name = "chkKarma";
            this.chkKarma.Size = new System.Drawing.Size(15, 14);
            this.chkKarma.TabIndex = 18;
            this.chkKarma.UseVisualStyleBackColor = true;
            // 
            // btnAddSpec
            // 
            this.btnAddSpec.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnAddSpec.AutoSize = true;
            this.btnAddSpec.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAddSpec.Image = global::Chummer.Properties.Resources.add;
            this.btnAddSpec.Location = new System.Drawing.Point(224, 0);
            this.btnAddSpec.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.btnAddSpec.Name = "btnAddSpec";
            this.btnAddSpec.Padding = new System.Windows.Forms.Padding(1);
            this.btnAddSpec.Size = new System.Drawing.Size(24, 24);
            this.btnAddSpec.TabIndex = 23;
            this.btnAddSpec.ToolTipText = "";
            this.btnAddSpec.UseVisualStyleBackColor = true;
            this.btnAddSpec.Click += new System.EventHandler(this.btnAddSpec_Click);
            // 
            // tlpRight
            // 
            this.tlpRight.AutoSize = true;
            this.tlpRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.ColumnCount = 5;
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.Controls.Add(this.cboSpec, 1, 0);
            this.tlpRight.Controls.Add(this.lblCareerSpec, 0, 0);
            this.tlpRight.Controls.Add(this.chkKarma, 2, 0);
            this.tlpRight.Controls.Add(this.btnAddSpec, 3, 0);
            this.tlpRight.Controls.Add(this.cmdDelete, 4, 0);
            this.tlpRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRight.Location = new System.Drawing.Point(289, 0);
            this.tlpRight.Margin = new System.Windows.Forms.Padding(0);
            this.tlpRight.Name = "tlpRight";
            this.tlpRight.RowCount = 1;
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRight.Size = new System.Drawing.Size(305, 24);
            this.tlpRight.TabIndex = 34;
            // 
            // cboSpec
            // 
            this.cboSpec.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSpec.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cboSpec.FormattingEnabled = true;
            this.cboSpec.Location = new System.Drawing.Point(92, 1);
            this.cboSpec.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cboSpec.Name = "cboSpec";
            this.cboSpec.Size = new System.Drawing.Size(105, 21);
            this.cboSpec.Sorted = true;
            this.cboSpec.TabIndex = 17;
            this.cboSpec.TabStop = false;
            this.cboSpec.TooltipText = "";
            // 
            // lblCareerSpec
            // 
            this.lblCareerSpec.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCareerSpec.AutoSize = true;
            this.lblCareerSpec.Location = new System.Drawing.Point(3, 5);
            this.lblCareerSpec.Name = "lblCareerSpec";
            this.lblCareerSpec.Size = new System.Drawing.Size(83, 13);
            this.lblCareerSpec.TabIndex = 22;
            this.lblCareerSpec.Text = "[Specializations]";
            this.lblCareerSpec.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SkillControl2
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "SkillControl2";
            this.Size = new System.Drawing.Size(594, 24);
            this.MouseLeave += new System.EventHandler(this.OnMouseLeave);
            this.DpiChangedAfterParent += new System.EventHandler(this.SkillControl2_DpiChangedAfterParent);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            this.cmsSkillLabel.ResumeLayout(false);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).EndInit();
            this.pnlAttributes.ResumeLayout(false);
            this.pnlAttributes.PerformLayout();
            this.tlpRight.ResumeLayout(false);
            this.tlpRight.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private LabelWithToolTip lblName;
        private ElasticComboBox cboSpec;
        private Chummer.ColorableCheckBox chkKarma;
        private System.Windows.Forms.Button cmdDelete;
        private System.Windows.Forms.Label lblCareerSpec;
        private ButtonWithToolTip btnAddSpec;
        private System.Windows.Forms.Button btnAttribute;
        private ElasticComboBox cboSelectAttribute;
        private ContextMenuStrip cmsSkillLabel;
        private ToolStripMenuItem tsSkillLabelNotes;
        private BufferedTableLayoutPanel tlpMain;
        private Panel pnlAttributes;
        private LabelWithToolTip lblModifiedRating;
        private ButtonWithToolTip btnCareerIncrease;
        private Label lblCareerRating;
        private NumericUpDownEx nudKarma;
        private NumericUpDownEx nudSkill;
        private TableLayoutPanel tlpRight;
    }
}
