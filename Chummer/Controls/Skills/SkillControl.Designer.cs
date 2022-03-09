using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Chummer.UI.Skills
{
    public sealed partial class SkillControl
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
            this.tsSkillLabelNotes = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblName = new Chummer.LabelWithToolTip();
            this.pnlAttributes = new System.Windows.Forms.Panel();
            this.btnAttribute = new System.Windows.Forms.Button();
            this.lblModifiedRating = new Chummer.LabelWithToolTip();
            this.tlpRight = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmsSkillLabel.SuspendLayout();
            this.tlpMain.SuspendLayout();
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
            this.tsSkillLabelNotes.ImageDpi120 = null;
            this.tsSkillLabelNotes.ImageDpi144 = null;
            this.tsSkillLabelNotes.ImageDpi192 = global::Chummer.Properties.Resources.note_edit1;
            this.tsSkillLabelNotes.ImageDpi288 = null;
            this.tsSkillLabelNotes.ImageDpi384 = null;
            this.tsSkillLabelNotes.ImageDpi96 = global::Chummer.Properties.Resources.note_edit;
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
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.lblName, 0, 0);
            this.tlpMain.Controls.Add(this.pnlAttributes, 1, 0);
            this.tlpMain.Controls.Add(this.lblModifiedRating, 4, 0);
            this.tlpMain.Controls.Add(this.tlpRight, 5, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 1;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(143, 25);
            this.tlpMain.TabIndex = 28;
            // 
            // lblName
            // 
            this.lblName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblName.AutoSize = true;
            this.lblName.ContextMenuStrip = this.cmsSkillLabel;
            this.lblName.Location = new System.Drawing.Point(3, 6);
            this.lblName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
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
            this.pnlAttributes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlAttributes.Location = new System.Drawing.Point(47, 0);
            this.pnlAttributes.Margin = new System.Windows.Forms.Padding(0);
            this.pnlAttributes.MinimumSize = new System.Drawing.Size(40, 0);
            this.pnlAttributes.Name = "pnlAttributes";
            this.pnlAttributes.Size = new System.Drawing.Size(40, 25);
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
            this.btnAttribute.Size = new System.Drawing.Size(40, 25);
            this.btnAttribute.TabIndex = 24;
            this.btnAttribute.Text = "ATR";
            this.btnAttribute.UseVisualStyleBackColor = true;
            this.btnAttribute.Click += new System.EventHandler(this.btnAttribute_Click);
            // 
            // lblModifiedRating
            // 
            this.lblModifiedRating.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblModifiedRating.AutoSize = true;
            this.lblModifiedRating.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblModifiedRating.Location = new System.Drawing.Point(90, 6);
            this.lblModifiedRating.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblModifiedRating.MinimumSize = new System.Drawing.Size(50, 0);
            this.lblModifiedRating.Name = "lblModifiedRating";
            this.lblModifiedRating.Size = new System.Drawing.Size(50, 13);
            this.lblModifiedRating.TabIndex = 16;
            this.lblModifiedRating.Text = "00 (00)";
            this.lblModifiedRating.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblModifiedRating.ToolTipText = "";
            // 
            // tlpRight
            // 
            this.tlpRight.AutoSize = true;
            this.tlpRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.ColumnCount = 2;
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRight.Location = new System.Drawing.Point(143, 0);
            this.tlpRight.Margin = new System.Windows.Forms.Padding(0);
            this.tlpRight.Name = "tlpRight";
            this.tlpRight.RowCount = 1;
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRight.Size = new System.Drawing.Size(1, 25);
            this.tlpRight.TabIndex = 34;
            // 
            // SkillControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "SkillControl";
            this.Size = new System.Drawing.Size(143, 25);
            this.MouseLeave += new System.EventHandler(this.OnMouseLeave);
            this.DpiChangedAfterParent += new System.EventHandler(this.SkillControl_DpiChangedAfterParent);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            this.cmsSkillLabel.ResumeLayout(false);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.pnlAttributes.ResumeLayout(false);
            this.pnlAttributes.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private LabelWithToolTip lblName;
        private System.Windows.Forms.Button btnAttribute;
        private ContextMenuStrip cmsSkillLabel;
        private BufferedTableLayoutPanel tlpMain;
        private Panel pnlAttributes;
        private LabelWithToolTip lblModifiedRating;
        private BufferedTableLayoutPanel tlpRight;
        private DpiFriendlyToolStripMenuItem tsSkillLabelNotes;
    }
}
