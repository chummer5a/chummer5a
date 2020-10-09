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
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.flpButtonsCareer = new System.Windows.Forms.FlowLayoutPanel();
            this.btnCareerIncrease = new Chummer.ButtonWithToolTip();
            this.lblCareerRating = new System.Windows.Forms.Label();
            this.flpButtonsCreate = new System.Windows.Forms.FlowLayoutPanel();
            this.nudKarma = new Chummer.NumericUpDownEx();
            this.nudSkill = new Chummer.NumericUpDownEx();
            this.cmdDelete = new System.Windows.Forms.Button();
            this.lblName = new Chummer.LabelWithToolTip();
            this.lblModifiedRating = new Chummer.LabelWithToolTip();
            this.pnlSpecs = new System.Windows.Forms.Panel();
            this.tlpSpecsCareer = new System.Windows.Forms.TableLayoutPanel();
            this.lblCareerSpec = new System.Windows.Forms.Label();
            this.btnAddSpec = new Chummer.ButtonWithToolTip();
            this.tlpSpecsCreate = new System.Windows.Forms.TableLayoutPanel();
            this.chkKarma = new Chummer.ColorableCheckBox(this.components);
            this.cboSpec = new Chummer.ElasticComboBox();
            this.pnlAttributes = new System.Windows.Forms.Panel();
            this.btnAttribute = new System.Windows.Forms.Button();
            this.cboSelectAttribute = new Chummer.ElasticComboBox();
            this.cmsSkillLabel.SuspendLayout();
            this.tlpMain.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.flpButtonsCareer.SuspendLayout();
            this.flpButtonsCreate.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill)).BeginInit();
            this.pnlSpecs.SuspendLayout();
            this.tlpSpecsCareer.SuspendLayout();
            this.tlpSpecsCreate.SuspendLayout();
            this.pnlAttributes.SuspendLayout();
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
            this.tlpMain.ColumnCount = 6;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.pnlButtons, 2, 0);
            this.tlpMain.Controls.Add(this.cmdDelete, 5, 0);
            this.tlpMain.Controls.Add(this.lblName, 0, 0);
            this.tlpMain.Controls.Add(this.lblModifiedRating, 3, 0);
            this.tlpMain.Controls.Add(this.pnlSpecs, 4, 0);
            this.tlpMain.Controls.Add(this.pnlAttributes, 1, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 1;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(1009, 24);
            this.tlpMain.TabIndex = 28;
            // 
            // pnlButtons
            // 
            this.pnlButtons.AutoSize = true;
            this.pnlButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlButtons.Controls.Add(this.flpButtonsCareer);
            this.pnlButtons.Controls.Add(this.flpButtonsCreate);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlButtons.Location = new System.Drawing.Point(87, 0);
            this.pnlButtons.Margin = new System.Windows.Forms.Padding(0);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(82, 24);
            this.pnlButtons.TabIndex = 32;
            // 
            // flpButtonsCareer
            // 
            this.flpButtonsCareer.AutoSize = true;
            this.flpButtonsCareer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpButtonsCareer.Controls.Add(this.btnCareerIncrease);
            this.flpButtonsCareer.Controls.Add(this.lblCareerRating);
            this.flpButtonsCareer.Dock = System.Windows.Forms.DockStyle.Right;
            this.flpButtonsCareer.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpButtonsCareer.Location = new System.Drawing.Point(21, 0);
            this.flpButtonsCareer.Margin = new System.Windows.Forms.Padding(0);
            this.flpButtonsCareer.Name = "flpButtonsCareer";
            this.flpButtonsCareer.Size = new System.Drawing.Size(61, 24);
            this.flpButtonsCareer.TabIndex = 1;
            this.flpButtonsCareer.WrapContents = false;
            // 
            // btnCareerIncrease
            // 
            this.btnCareerIncrease.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnCareerIncrease.AutoSize = true;
            this.btnCareerIncrease.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnCareerIncrease.Image = global::Chummer.Properties.Resources.add;
            this.btnCareerIncrease.Location = new System.Drawing.Point(34, 0);
            this.btnCareerIncrease.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.btnCareerIncrease.Name = "btnCareerIncrease";
            this.btnCareerIncrease.Padding = new System.Windows.Forms.Padding(1);
            this.btnCareerIncrease.Size = new System.Drawing.Size(24, 24);
            this.btnCareerIncrease.TabIndex = 21;
            this.btnCareerIncrease.ToolTipText = "";
            this.btnCareerIncrease.UseVisualStyleBackColor = true;
            this.btnCareerIncrease.Click += new System.EventHandler(this.btnCareerIncrease_Click);
            // 
            // lblCareerRating
            // 
            this.lblCareerRating.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCareerRating.AutoSize = true;
            this.lblCareerRating.Location = new System.Drawing.Point(3, 5);
            this.lblCareerRating.MinimumSize = new System.Drawing.Size(25, 0);
            this.lblCareerRating.Name = "lblCareerRating";
            this.lblCareerRating.Size = new System.Drawing.Size(25, 13);
            this.lblCareerRating.TabIndex = 20;
            this.lblCareerRating.Text = "00";
            this.lblCareerRating.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
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
            this.flpButtonsCreate.TabIndex = 0;
            this.flpButtonsCreate.WrapContents = false;
            // 
            // nudKarma
            // 
            this.nudKarma.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.nudKarma.AutoSize = true;
            this.nudKarma.InterceptMouseWheel = Chummer.NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver;
            this.nudKarma.Location = new System.Drawing.Point(44, 2);
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
            // nudSkill
            // 
            this.nudSkill.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.nudSkill.AutoSize = true;
            this.nudSkill.InterceptMouseWheel = Chummer.NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver;
            this.nudSkill.Location = new System.Drawing.Point(3, 2);
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
            // cmdDelete
            // 
            this.cmdDelete.AutoSize = true;
            this.cmdDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDelete.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdDelete.Location = new System.Drawing.Point(958, 0);
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
            // lblModifiedRating
            // 
            this.lblModifiedRating.AutoSize = true;
            this.lblModifiedRating.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblModifiedRating.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblModifiedRating.Location = new System.Drawing.Point(172, 0);
            this.lblModifiedRating.MinimumSize = new System.Drawing.Size(50, 0);
            this.lblModifiedRating.Name = "lblModifiedRating";
            this.lblModifiedRating.Size = new System.Drawing.Size(50, 24);
            this.lblModifiedRating.TabIndex = 16;
            this.lblModifiedRating.Text = "00 (00)";
            this.lblModifiedRating.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblModifiedRating.ToolTipText = "";
            // 
            // pnlSpecs
            // 
            this.pnlSpecs.AutoSize = true;
            this.pnlSpecs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlSpecs.Controls.Add(this.tlpSpecsCareer);
            this.pnlSpecs.Controls.Add(this.tlpSpecsCreate);
            this.pnlSpecs.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSpecs.Location = new System.Drawing.Point(225, 0);
            this.pnlSpecs.Margin = new System.Windows.Forms.Padding(0);
            this.pnlSpecs.Name = "pnlSpecs";
            this.pnlSpecs.Size = new System.Drawing.Size(730, 24);
            this.pnlSpecs.TabIndex = 31;
            // 
            // tlpSpecsCareer
            // 
            this.tlpSpecsCareer.AutoSize = true;
            this.tlpSpecsCareer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpSpecsCareer.ColumnCount = 2;
            this.tlpSpecsCareer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSpecsCareer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpSpecsCareer.Controls.Add(this.lblCareerSpec, 0, 0);
            this.tlpSpecsCareer.Controls.Add(this.btnAddSpec, 1, 0);
            this.tlpSpecsCareer.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpSpecsCareer.Location = new System.Drawing.Point(0, 0);
            this.tlpSpecsCareer.Margin = new System.Windows.Forms.Padding(0);
            this.tlpSpecsCareer.Name = "tlpSpecsCareer";
            this.tlpSpecsCareer.RowCount = 1;
            this.tlpSpecsCareer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSpecsCareer.Size = new System.Drawing.Size(730, 24);
            this.tlpSpecsCareer.TabIndex = 32;
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
            // btnAddSpec
            // 
            this.btnAddSpec.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnAddSpec.AutoSize = true;
            this.btnAddSpec.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAddSpec.Image = global::Chummer.Properties.Resources.add;
            this.btnAddSpec.Location = new System.Drawing.Point(703, 0);
            this.btnAddSpec.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.btnAddSpec.Name = "btnAddSpec";
            this.btnAddSpec.Padding = new System.Windows.Forms.Padding(1);
            this.btnAddSpec.Size = new System.Drawing.Size(24, 24);
            this.btnAddSpec.TabIndex = 23;
            this.btnAddSpec.ToolTipText = "";
            this.btnAddSpec.UseVisualStyleBackColor = true;
            this.btnAddSpec.Click += new System.EventHandler(this.btnAddSpec_Click);
            // 
            // tlpSpecsCreate
            // 
            this.tlpSpecsCreate.AutoSize = true;
            this.tlpSpecsCreate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpSpecsCreate.ColumnCount = 2;
            this.tlpSpecsCreate.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSpecsCreate.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpSpecsCreate.Controls.Add(this.chkKarma, 1, 0);
            this.tlpSpecsCreate.Controls.Add(this.cboSpec, 0, 0);
            this.tlpSpecsCreate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSpecsCreate.Location = new System.Drawing.Point(0, 0);
            this.tlpSpecsCreate.Margin = new System.Windows.Forms.Padding(0);
            this.tlpSpecsCreate.Name = "tlpSpecsCreate";
            this.tlpSpecsCreate.RowCount = 1;
            this.tlpSpecsCreate.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSpecsCreate.Size = new System.Drawing.Size(730, 24);
            this.tlpSpecsCreate.TabIndex = 30;
            // 
            // chkKarma
            // 
            this.chkKarma.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkKarma.AutoSize = true;
            this.chkKarma.Location = new System.Drawing.Point(715, 6);
            this.chkKarma.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.chkKarma.Name = "chkKarma";
            this.chkKarma.Size = new System.Drawing.Size(12, 11);
            this.chkKarma.TabIndex = 18;
            this.chkKarma.UseVisualStyleBackColor = true;
            // 
            // cboSpec
            // 
            this.cboSpec.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSpec.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cboSpec.FormattingEnabled = true;
            this.cboSpec.Location = new System.Drawing.Point(3, 1);
            this.cboSpec.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cboSpec.Name = "cboSpec";
            this.cboSpec.Size = new System.Drawing.Size(706, 21);
            this.cboSpec.Sorted = true;
            this.cboSpec.TabIndex = 17;
            this.cboSpec.TabStop = false;
            this.cboSpec.TooltipText = "";
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
            this.pnlAttributes.Size = new System.Drawing.Size(40, 24);
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
            this.btnAttribute.Size = new System.Drawing.Size(40, 24);
            this.btnAttribute.TabIndex = 24;
            this.btnAttribute.Text = "ATR";
            this.btnAttribute.UseVisualStyleBackColor = true;
            this.btnAttribute.Click += new System.EventHandler(this.btnAttribute_Click);
            // 
            // cboSelectAttribute
            // 
            this.cboSelectAttribute.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboSelectAttribute.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSelectAttribute.FormattingEnabled = true;
            this.cboSelectAttribute.Location = new System.Drawing.Point(0, 0);
            this.cboSelectAttribute.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cboSelectAttribute.Name = "cboSelectAttribute";
            this.cboSelectAttribute.Size = new System.Drawing.Size(40, 21);
            this.cboSelectAttribute.TabIndex = 25;
            this.cboSelectAttribute.TooltipText = "";
            this.cboSelectAttribute.Visible = false;
            this.cboSelectAttribute.DropDownClosed += new System.EventHandler(this.cboSelectAttribute_Closed);
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
            this.Size = new System.Drawing.Size(1009, 24);
            this.MouseLeave += new System.EventHandler(this.OnMouseLeave);
            this.DpiChangedAfterParent += new System.EventHandler(this.SkillControl2_DpiChangedAfterParent);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            this.cmsSkillLabel.ResumeLayout(false);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.pnlButtons.PerformLayout();
            this.flpButtonsCareer.ResumeLayout(false);
            this.flpButtonsCareer.PerformLayout();
            this.flpButtonsCreate.ResumeLayout(false);
            this.flpButtonsCreate.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill)).EndInit();
            this.pnlSpecs.ResumeLayout(false);
            this.pnlSpecs.PerformLayout();
            this.tlpSpecsCareer.ResumeLayout(false);
            this.tlpSpecsCareer.PerformLayout();
            this.tlpSpecsCreate.ResumeLayout(false);
            this.tlpSpecsCreate.PerformLayout();
            this.pnlAttributes.ResumeLayout(false);
            this.pnlAttributes.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private LabelWithToolTip lblName;
        private NumericUpDownEx nudKarma;
        private NumericUpDownEx nudSkill;
        private LabelWithToolTip lblModifiedRating;
        private ElasticComboBox cboSpec;
        private Chummer.ColorableCheckBox chkKarma;
        private System.Windows.Forms.Button cmdDelete;
        private System.Windows.Forms.Label lblCareerRating;
        private ButtonWithToolTip btnCareerIncrease;
        private System.Windows.Forms.Label lblCareerSpec;
        private ButtonWithToolTip btnAddSpec;
        private System.Windows.Forms.Button btnAttribute;
        private ElasticComboBox cboSelectAttribute;
        private ContextMenuStrip cmsSkillLabel;
        private ToolStripMenuItem tsSkillLabelNotes;
        private BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.TableLayoutPanel tlpSpecsCreate;
        private Panel pnlSpecs;
        private System.Windows.Forms.TableLayoutPanel tlpSpecsCareer;
        private Panel pnlButtons;
        private FlowLayoutPanel flpButtonsCreate;
        private FlowLayoutPanel flpButtonsCareer;
        private Panel pnlAttributes;
    }
}
