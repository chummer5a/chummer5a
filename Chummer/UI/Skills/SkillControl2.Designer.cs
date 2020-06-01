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
                _italic?.Dispose();
                _italicName?.Dispose();
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
            this.lblName = new Chummer.LabelWithToolTip();
            this.cmsSkillLabel = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsSkillLabelNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.lblAttribute = new System.Windows.Forms.Label();
            this.nudKarma = new Chummer.NumericUpDownEx();
            this.nudSkill = new Chummer.NumericUpDownEx();
            this.lblModifiedRating = new Chummer.LabelWithToolTip();
            this.cboSpec = new ElasticComboBox();
            this.chkKarma = new System.Windows.Forms.CheckBox();
            this.cmdDelete = new System.Windows.Forms.Button();
            this.lblCareerRating = new System.Windows.Forms.Label();
            this.btnCareerIncrease = new Chummer.ButtonWithToolTip();
            this.lblCareerSpec = new System.Windows.Forms.Label();
            this.btnAddSpec = new Chummer.ButtonWithToolTip();
            this.btnAttribute = new System.Windows.Forms.Button();
            this.cboSelectAttribute = new ElasticComboBox();
            this.cmsSkillLabel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill)).BeginInit();
            this.SuspendLayout();
            // 
            // lblName
            // 
            this.lblName.ContextMenuStrip = this.cmsSkillLabel;
            this.lblName.Location = new System.Drawing.Point(0, 5);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(35, 13);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "label1";
            this.lblName.ToolTipText = "";
            this.lblName.Click += new System.EventHandler(this.lblName_Click);
            // 
            // cmsSkillLabel
            // 
            this.cmsSkillLabel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsSkillLabelNotes});
            this.cmsSkillLabel.Name = "cmsWeapon";
            this.cmsSkillLabel.Size = new System.Drawing.Size(106, 26);
            // 
            // tsSkillLabelNotes
            // 
            this.tsSkillLabelNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsSkillLabelNotes.Name = "tsSkillLabelNotes";
            this.tsSkillLabelNotes.Size = new System.Drawing.Size(105, 22);
            this.tsSkillLabelNotes.Tag = "Menu_Notes";
            this.tsSkillLabelNotes.Text = "&Notes";
            this.tsSkillLabelNotes.Click += new System.EventHandler(this.tsSkillLabelNotes_Click);
            // 
            // lblAttribute
            // 
            this.lblAttribute.AutoSize = true;
            this.lblAttribute.Location = new System.Drawing.Point(133, 4);
            this.lblAttribute.Name = "lblAttribute";
            this.lblAttribute.Size = new System.Drawing.Size(29, 13);
            this.lblAttribute.TabIndex = 3;
            this.lblAttribute.Text = "ATR";
            // 
            // nudKarma
            // 
            this.nudKarma.InterceptMouseWheel = Chummer.NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver;
            this.nudKarma.Location = new System.Drawing.Point(210, 2);
            this.nudKarma.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudKarma.Name = "nudKarma";
            this.nudKarma.Size = new System.Drawing.Size(40, 20);
            this.nudKarma.TabIndex = 14;
            // 
            // nudSkill
            // 
            this.nudSkill.InterceptMouseWheel = Chummer.NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver;
            this.nudSkill.Location = new System.Drawing.Point(168, 2);
            this.nudSkill.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudSkill.Name = "nudSkill";
            this.nudSkill.Size = new System.Drawing.Size(40, 20);
            this.nudSkill.TabIndex = 15;
            // 
            // lblModifiedRating
            // 
            this.lblModifiedRating.AutoSize = true;
            this.lblModifiedRating.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblModifiedRating.Location = new System.Drawing.Point(256, 5);
            this.lblModifiedRating.Name = "lblModifiedRating";
            this.lblModifiedRating.Size = new System.Drawing.Size(14, 13);
            this.lblModifiedRating.TabIndex = 16;
            this.lblModifiedRating.Text = "0";
            this.lblModifiedRating.ToolTipText = "";
            // 
            // cboSpec
            // 
            this.cboSpec.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSpec.FormattingEnabled = true;
            this.cboSpec.Location = new System.Drawing.Point(310, 1);
            this.cboSpec.Name = "cboSpec";
            this.cboSpec.Size = new System.Drawing.Size(402, 21);
            this.cboSpec.Sorted = true;
            this.cboSpec.TabIndex = 17;
            // 
            // chkKarma
            // 
            this.chkKarma.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkKarma.AutoSize = true;
            this.chkKarma.Location = new System.Drawing.Point(723, 5);
            this.chkKarma.Name = "chkKarma";
            this.chkKarma.Size = new System.Drawing.Size(15, 14);
            this.chkKarma.TabIndex = 18;
            this.chkKarma.UseVisualStyleBackColor = true;
            // 
            // cmdDelete
            // 
            this.cmdDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdDelete.Location = new System.Drawing.Point(718, 1);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(71, 22);
            this.cmdDelete.TabIndex = 19;
            this.cmdDelete.Tag = "String_Delete";
            this.cmdDelete.Text = "Delete";
            this.cmdDelete.UseVisualStyleBackColor = true;
            // 
            // lblCareerRating
            // 
            this.lblCareerRating.AutoSize = true;
            this.lblCareerRating.Location = new System.Drawing.Point(169, 5);
            this.lblCareerRating.Name = "lblCareerRating";
            this.lblCareerRating.Size = new System.Drawing.Size(19, 13);
            this.lblCareerRating.TabIndex = 20;
            this.lblCareerRating.Text = "00";
            // 
            // btnCareerIncrease
            // 
            this.btnCareerIncrease.Image = global::Chummer.Properties.Resources.add;
            this.btnCareerIncrease.Location = new System.Drawing.Point(214, 0);
            this.btnCareerIncrease.Name = "btnCareerIncrease";
            this.btnCareerIncrease.Size = new System.Drawing.Size(24, 24);
            this.btnCareerIncrease.TabIndex = 21;
            this.btnCareerIncrease.ToolTipText = "";
            this.btnCareerIncrease.UseVisualStyleBackColor = true;
            this.btnCareerIncrease.Click += new System.EventHandler(this.btnCareerIncrease_Click);
            // 
            // lblCareerSpec
            // 
            this.lblCareerSpec.AutoSize = true;
            this.lblCareerSpec.Location = new System.Drawing.Point(290, 5);
            this.lblCareerSpec.Name = "lblCareerSpec";
            this.lblCareerSpec.Size = new System.Drawing.Size(35, 13);
            this.lblCareerSpec.TabIndex = 22;
            this.lblCareerSpec.Text = "label1";
            // 
            // btnAddSpec
            // 
            this.btnAddSpec.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddSpec.Image = global::Chummer.Properties.Resources.add;
            this.btnAddSpec.Location = new System.Drawing.Point(765, 0);
            this.btnAddSpec.Name = "btnAddSpec";
            this.btnAddSpec.Size = new System.Drawing.Size(24, 24);
            this.btnAddSpec.TabIndex = 23;
            this.btnAddSpec.ToolTipText = "";
            this.btnAddSpec.UseVisualStyleBackColor = true;
            this.btnAddSpec.Click += new System.EventHandler(this.btnAddSpec_Click);
            // 
            // btnAttribute
            // 
            this.btnAttribute.FlatAppearance.BorderSize = 0;
            this.btnAttribute.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAttribute.Location = new System.Drawing.Point(128, 0);
            this.btnAttribute.Margin = new System.Windows.Forms.Padding(1);
            this.btnAttribute.Name = "btnAttribute";
            this.btnAttribute.Size = new System.Drawing.Size(39, 23);
            this.btnAttribute.TabIndex = 24;
            this.btnAttribute.Text = "ATR";
            this.btnAttribute.UseVisualStyleBackColor = true;
            this.btnAttribute.Click += new System.EventHandler(this.btnAttribute_Click);
            // 
            // cboSelectAttribute
            // 
            this.cboSelectAttribute.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSelectAttribute.FormattingEnabled = true;
            this.cboSelectAttribute.Location = new System.Drawing.Point(128, 1);
            this.cboSelectAttribute.Name = "cboSelectAttribute";
            this.cboSelectAttribute.Size = new System.Drawing.Size(39, 21);
            this.cboSelectAttribute.TabIndex = 25;
            this.cboSelectAttribute.DropDownClosed += new System.EventHandler(this.cboSelectAttribute_Closed);
            // 
            // SkillControl2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnAddSpec);
            this.Controls.Add(this.cboSelectAttribute);
            this.Controls.Add(this.btnAttribute);
            this.Controls.Add(this.lblCareerSpec);
            this.Controls.Add(this.btnCareerIncrease);
            this.Controls.Add(this.lblCareerRating);
            this.Controls.Add(this.cboSpec);
            this.Controls.Add(this.lblModifiedRating);
            this.Controls.Add(this.nudSkill);
            this.Controls.Add(this.nudKarma);
            this.Controls.Add(this.lblAttribute);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.cmdDelete);
            this.Controls.Add(this.chkKarma);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "SkillControl2";
            this.Size = new System.Drawing.Size(789, 24);
            this.cmsSkillLabel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill)).EndInit();
            this.MouseLeave += new System.EventHandler(this.OnMouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private LabelWithToolTip lblName;
        private System.Windows.Forms.Label lblAttribute;
        private NumericUpDownEx nudKarma;
        private NumericUpDownEx nudSkill;
        private LabelWithToolTip lblModifiedRating;
        private ElasticComboBox cboSpec;
        private System.Windows.Forms.CheckBox chkKarma;
        private System.Windows.Forms.Button cmdDelete;
        private System.Windows.Forms.Label lblCareerRating;
        private ButtonWithToolTip btnCareerIncrease;
        private System.Windows.Forms.Label lblCareerSpec;
        private ButtonWithToolTip btnAddSpec;
        private System.Windows.Forms.Button btnAttribute;
        private ElasticComboBox cboSelectAttribute;
        private ContextMenuStrip cmsSkillLabel;
        private ToolStripMenuItem tsSkillLabelNotes;
    }
}
