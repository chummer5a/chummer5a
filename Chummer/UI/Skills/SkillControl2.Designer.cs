using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Chummer.UI.Skills
{
    partial class SkillControl2
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
            _italic.Dispose();
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
            this.lblName = new System.Windows.Forms.Label();
            this.cmsSkillLabel = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsSkillLabelNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.lblAttribute = new System.Windows.Forms.Label();
            this.nudKarma = new Chummer.helpers.NumericUpDownEx();
            this.nudSkill = new Chummer.helpers.NumericUpDownEx();
            this.lblModifiedRating = new System.Windows.Forms.Label();
            this.cboSpec = new System.Windows.Forms.ComboBox();
            this.chkKarma = new System.Windows.Forms.CheckBox();
            this.cmdDelete = new System.Windows.Forms.Button();
            this.lblCareerRating = new System.Windows.Forms.Label();
            this.btnCareerIncrease = new System.Windows.Forms.Button();
            this.lblCareerSpec = new System.Windows.Forms.Label();
            this.btnAddSpec = new System.Windows.Forms.Button();
            this.tipTooltip = new TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip();
            this.btnAttribute = new System.Windows.Forms.Button();
            this.cboSelectAttribute = new System.Windows.Forms.ComboBox();
            this.cmsSkillLabel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill)).BeginInit();
            this.SuspendLayout();
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.ContextMenuStrip = this.cmsSkillLabel;
            this.lblName.Location = new System.Drawing.Point(0, 4);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(35, 13);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "label1";
            this.lblName.Click += new System.EventHandler(this.lblName_Click);
            // 
            // cmsSkillLabel
            // 
            this.cmsSkillLabel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsSkillLabelNotes});
            this.cmsSkillLabel.Name = "cmsWeapon";
            this.cmsSkillLabel.Size = new System.Drawing.Size(106, 26);
            this.cmsSkillLabel.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
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
            this.nudKarma.InterceptMouseWheel = Chummer.helpers.NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver;
            this.nudKarma.Location = new System.Drawing.Point(210, 1);
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
            // nudSkill
            // 
            this.nudSkill.InterceptMouseWheel = Chummer.helpers.NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver;
            this.nudSkill.Location = new System.Drawing.Point(168, 1);
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
            // lblModifiedRating
            // 
            this.lblModifiedRating.AutoSize = true;
            this.lblModifiedRating.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblModifiedRating.Location = new System.Drawing.Point(256, 4);
            this.lblModifiedRating.Name = "lblModifiedRating";
            this.lblModifiedRating.Size = new System.Drawing.Size(14, 13);
            this.lblModifiedRating.TabIndex = 16;
            this.lblModifiedRating.Text = "0";
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
            this.cboSpec.TextChanged += new System.EventHandler(this.cboSpec_TextChanged);
            // 
            // chkKarma
            // 
            this.chkKarma.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkKarma.AutoSize = true;
            this.chkKarma.Location = new System.Drawing.Point(723, 4);
            this.chkKarma.Name = "chkKarma";
            this.chkKarma.Size = new System.Drawing.Size(15, 14);
            this.chkKarma.TabIndex = 18;
            this.chkKarma.UseVisualStyleBackColor = true;
            // 
            // cmdDelete
            // 
            this.cmdDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdDelete.Location = new System.Drawing.Point(718, 0);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(71, 23);
            this.cmdDelete.TabIndex = 19;
            this.cmdDelete.Tag = "String_Delete";
            this.cmdDelete.Text = "Delete";
            this.cmdDelete.UseVisualStyleBackColor = true;
            this.cmdDelete.Visible = false;
            // 
            // lblCareerRating
            // 
            this.lblCareerRating.AutoSize = true;
            this.lblCareerRating.Location = new System.Drawing.Point(169, 4);
            this.lblCareerRating.Name = "lblCareerRating";
            this.lblCareerRating.Size = new System.Drawing.Size(19, 13);
            this.lblCareerRating.TabIndex = 20;
            this.lblCareerRating.Text = "00";
            this.lblCareerRating.Visible = false;
            // 
            // btnCareerIncrease
            // 
            this.btnCareerIncrease.Image = global::Chummer.Properties.Resources.add;
            this.btnCareerIncrease.Location = new System.Drawing.Point(214, -2);
            this.btnCareerIncrease.Name = "btnCareerIncrease";
            this.btnCareerIncrease.Size = new System.Drawing.Size(24, 24);
            this.btnCareerIncrease.TabIndex = 21;
            this.btnCareerIncrease.UseVisualStyleBackColor = true;
            this.btnCareerIncrease.Visible = false;
            this.btnCareerIncrease.Click += new System.EventHandler(this.btnCareerIncrease_Click);
            // 
            // lblCareerSpec
            // 
            this.lblCareerSpec.AutoSize = true;
            this.lblCareerSpec.Location = new System.Drawing.Point(290, 4);
            this.lblCareerSpec.Name = "lblCareerSpec";
            this.lblCareerSpec.Size = new System.Drawing.Size(35, 13);
            this.lblCareerSpec.TabIndex = 22;
            this.lblCareerSpec.Text = "label1";
            this.lblCareerSpec.Visible = false;
            // 
            // btnAddSpec
            // 
            this.btnAddSpec.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddSpec.Image = global::Chummer.Properties.Resources.add;
            this.btnAddSpec.Location = new System.Drawing.Point(765, -2);
            this.btnAddSpec.Name = "btnAddSpec";
            this.btnAddSpec.Size = new System.Drawing.Size(24, 24);
            this.btnAddSpec.TabIndex = 23;
            this.btnAddSpec.UseVisualStyleBackColor = true;
            this.btnAddSpec.Visible = false;
            this.btnAddSpec.Click += new System.EventHandler(this.btnAddSpec_Click);
            // 
            // tipTooltip
            // 
            this.tipTooltip.AllowLinksHandling = true;
            this.tipTooltip.AutoPopDelay = 10000;
            this.tipTooltip.BaseStylesheet = null;
            this.tipTooltip.InitialDelay = 250;
            this.tipTooltip.IsBalloon = true;
            this.tipTooltip.MaximumSize = new System.Drawing.Size(0, 0);
            this.tipTooltip.OwnerDraw = true;
            this.tipTooltip.ReshowDelay = 100;
            this.tipTooltip.TooltipCssClass = "htmltooltip";
            this.tipTooltip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.tipTooltip.ToolTipTitle = "Chummer Help";
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
            this.btnAttribute.Visible = false;
            this.btnAttribute.Click += new System.EventHandler(this.btnAttribute_Click);
            // 
            // cboSelectAttribute
            // 
            this.cboSelectAttribute.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSelectAttribute.FormattingEnabled = true;
            this.cboSelectAttribute.Location = new System.Drawing.Point(128, 0);
            this.cboSelectAttribute.Name = "cboSelectAttribute";
            this.cboSelectAttribute.Size = new System.Drawing.Size(39, 21);
            this.cboSelectAttribute.TabIndex = 25;
            this.cboSelectAttribute.Visible = false;
            this.cboSelectAttribute.DropDownClosed += new System.EventHandler(this.cboSelectAttribute_Closed);
            // 
            // SkillControl2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cboSelectAttribute);
            this.Controls.Add(this.btnAttribute);
            this.Controls.Add(this.btnAddSpec);
            this.Controls.Add(this.lblCareerSpec);
            this.Controls.Add(this.btnCareerIncrease);
            this.Controls.Add(this.lblCareerRating);
            this.Controls.Add(this.cmdDelete);
            this.Controls.Add(this.chkKarma);
            this.Controls.Add(this.cboSpec);
            this.Controls.Add(this.lblModifiedRating);
            this.Controls.Add(this.nudSkill);
            this.Controls.Add(this.nudKarma);
            this.Controls.Add(this.lblAttribute);
            this.Controls.Add(this.lblName);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "SkillControl2";
            this.Size = new System.Drawing.Size(789, 23);
            this.cmsSkillLabel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblAttribute;
        private Chummer.helpers.NumericUpDownEx nudKarma;
        private Chummer.helpers.NumericUpDownEx nudSkill;
        private System.Windows.Forms.Label lblModifiedRating;
        private System.Windows.Forms.ComboBox cboSpec;
        private System.Windows.Forms.CheckBox chkKarma;
        private System.Windows.Forms.Button cmdDelete;
        private System.Windows.Forms.Label lblCareerRating;
        private System.Windows.Forms.Button btnCareerIncrease;
        private System.Windows.Forms.Label lblCareerSpec;
        private System.Windows.Forms.Button btnAddSpec;
        private TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip tipTooltip;
        private System.Windows.Forms.Button btnAttribute;
        private System.Windows.Forms.ComboBox cboSelectAttribute;
        private ContextMenuStrip cmsSkillLabel;
        private ToolStripMenuItem tsSkillLabelNotes;
    }
}
