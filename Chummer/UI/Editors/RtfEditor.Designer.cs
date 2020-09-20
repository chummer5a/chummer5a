using System;

namespace Chummer.UI.Editors
{
    partial class RtfEditor
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
            this.tsControls = new System.Windows.Forms.ToolStrip();
            this.tsbBold = new System.Windows.Forms.ToolStripButton();
            this.tsbItalic = new System.Windows.Forms.ToolStripButton();
            this.tsbUnderline = new System.Windows.Forms.ToolStripButton();
            this.tsbStrikeout = new System.Windows.Forms.ToolStripButton();
            this.tss1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbFont = new System.Windows.Forms.ToolStripButton();
            this.tsbForeColor = new System.Windows.Forms.ToolStripButton();
            this.tsbBackColor = new System.Windows.Forms.ToolStripButton();
            this.tss2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbAlignLeft = new System.Windows.Forms.ToolStripButton();
            this.tsbAlignCenter = new System.Windows.Forms.ToolStripButton();
            this.tsbAlignRight = new System.Windows.Forms.ToolStripButton();
            this.tss3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbUnorderedList = new System.Windows.Forms.ToolStripButton();
            this.tsbIncreaseIndent = new System.Windows.Forms.ToolStripButton();
            this.tsbDecreaseIndent = new System.Windows.Forms.ToolStripButton();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.rtbContent = new System.Windows.Forms.RichTextBox();
            this.tsControls.SuspendLayout();
            this.tlpMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // tsControls
            // 
            this.tsControls.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsControls.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbBold,
            this.tsbItalic,
            this.tsbUnderline,
            this.tsbStrikeout,
            this.tss1,
            this.tsbFont,
            this.tsbForeColor,
            this.tsbBackColor,
            this.tss2,
            this.tsbAlignLeft,
            this.tsbAlignCenter,
            this.tsbAlignRight,
            this.tss3,
            this.tsbUnorderedList,
            this.tsbIncreaseIndent,
            this.tsbDecreaseIndent});
            this.tsControls.Location = new System.Drawing.Point(0, 0);
            this.tsControls.Name = "tsControls";
            this.tsControls.Padding = new System.Windows.Forms.Padding(3);
            this.tsControls.Size = new System.Drawing.Size(325, 29);
            this.tsControls.Stretch = true;
            this.tsControls.TabIndex = 0;
            // 
            // tsbBold
            // 
            this.tsbBold.CheckOnClick = true;
            this.tsbBold.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbBold.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tsbBold.Image = global::Chummer.Properties.Resources.text_bold;
            this.tsbBold.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbBold.Name = "tsbBold";
            this.tsbBold.Size = new System.Drawing.Size(23, 20);
            this.tsbBold.Tag = "String_Bold";
            this.tsbBold.Text = "Bold";
            this.tsbBold.Click += new System.EventHandler(this.UpdateFont);
            // 
            // tsbItalic
            // 
            this.tsbItalic.CheckOnClick = true;
            this.tsbItalic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbItalic.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tsbItalic.Image = global::Chummer.Properties.Resources.text_italic;
            this.tsbItalic.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbItalic.Name = "tsbItalic";
            this.tsbItalic.Size = new System.Drawing.Size(23, 20);
            this.tsbItalic.Tag = "String_Italic";
            this.tsbItalic.Text = "Italic";
            this.tsbItalic.Click += new System.EventHandler(this.UpdateFont);
            // 
            // tsbUnderline
            // 
            this.tsbUnderline.CheckOnClick = true;
            this.tsbUnderline.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbUnderline.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tsbUnderline.Image = global::Chummer.Properties.Resources.text_underline;
            this.tsbUnderline.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbUnderline.Name = "tsbUnderline";
            this.tsbUnderline.Size = new System.Drawing.Size(23, 20);
            this.tsbUnderline.Tag = "String_Underline";
            this.tsbUnderline.Text = "Underline";
            this.tsbUnderline.Click += new System.EventHandler(this.UpdateFont);
            // 
            // tsbStrikeout
            // 
            this.tsbStrikeout.CheckOnClick = true;
            this.tsbStrikeout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbStrikeout.Image = global::Chummer.Properties.Resources.text_strikethrough;
            this.tsbStrikeout.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbStrikeout.Name = "tsbStrikeout";
            this.tsbStrikeout.Size = new System.Drawing.Size(23, 20);
            this.tsbStrikeout.Tag = "String_Strikethrough";
            this.tsbStrikeout.Text = "Strikethrough";
            this.tsbStrikeout.Click += new System.EventHandler(this.UpdateFont);
            // 
            // tss1
            // 
            this.tss1.Name = "tss1";
            this.tss1.Size = new System.Drawing.Size(6, 23);
            // 
            // tsbFont
            // 
            this.tsbFont.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbFont.Image = global::Chummer.Properties.Resources.font;
            this.tsbFont.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbFont.Name = "tsbFont";
            this.tsbFont.Size = new System.Drawing.Size(23, 20);
            this.tsbFont.Tag = "String_Font";
            this.tsbFont.Text = "Font";
            this.tsbFont.Click += new System.EventHandler(this.tsbFont_Click);
            // 
            // tsbForeColor
            // 
            this.tsbForeColor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbForeColor.Image = global::Chummer.Properties.Resources.color_wheel;
            this.tsbForeColor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbForeColor.Name = "tsbForeColor";
            this.tsbForeColor.Size = new System.Drawing.Size(23, 20);
            this.tsbForeColor.Tag = "String_ForeColor";
            this.tsbForeColor.Text = "Foreground Color";
            this.tsbForeColor.Click += new System.EventHandler(this.tsbForeColor_Click);
            // 
            // tsbBackColor
            // 
            this.tsbBackColor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbBackColor.Image = global::Chummer.Properties.Resources.paintcan;
            this.tsbBackColor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbBackColor.Name = "tsbBackColor";
            this.tsbBackColor.Size = new System.Drawing.Size(23, 20);
            this.tsbBackColor.Tag = "String_BackColor";
            this.tsbBackColor.Text = "Background Color";
            this.tsbBackColor.Click += new System.EventHandler(this.tsbBackColor_Click);
            // 
            // tss2
            // 
            this.tss2.Name = "tss2";
            this.tss2.Size = new System.Drawing.Size(6, 23);
            // 
            // tsbAlignLeft
            // 
            this.tsbAlignLeft.CheckOnClick = true;
            this.tsbAlignLeft.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbAlignLeft.Image = global::Chummer.Properties.Resources.text_align_left;
            this.tsbAlignLeft.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbAlignLeft.Name = "tsbAlignLeft";
            this.tsbAlignLeft.Size = new System.Drawing.Size(23, 20);
            this.tsbAlignLeft.Tag = "String_AlignLeft";
            this.tsbAlignLeft.Text = "Align Left";
            this.tsbAlignLeft.CheckedChanged += new System.EventHandler(this.tsbAlignLeft_CheckedChanged);
            this.tsbAlignLeft.Click += new System.EventHandler(this.tsbAlignLeft_Click);
            // 
            // tsbAlignCenter
            // 
            this.tsbAlignCenter.CheckOnClick = true;
            this.tsbAlignCenter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbAlignCenter.Image = global::Chummer.Properties.Resources.text_align_center;
            this.tsbAlignCenter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbAlignCenter.Name = "tsbAlignCenter";
            this.tsbAlignCenter.Size = new System.Drawing.Size(23, 20);
            this.tsbAlignCenter.Tag = "String_AlignCenter";
            this.tsbAlignCenter.Text = "Align Center";
            this.tsbAlignCenter.CheckedChanged += new System.EventHandler(this.tsbAlignCenter_CheckedChanged);
            this.tsbAlignCenter.Click += new System.EventHandler(this.tsbAlignCenter_Click);
            // 
            // tsbAlignRight
            // 
            this.tsbAlignRight.CheckOnClick = true;
            this.tsbAlignRight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbAlignRight.Image = global::Chummer.Properties.Resources.text_align_right;
            this.tsbAlignRight.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbAlignRight.Name = "tsbAlignRight";
            this.tsbAlignRight.Size = new System.Drawing.Size(23, 20);
            this.tsbAlignRight.Tag = "String_AlignRight";
            this.tsbAlignRight.Text = "Align Right";
            this.tsbAlignRight.CheckedChanged += new System.EventHandler(this.tsbAlignRight_CheckedChanged);
            this.tsbAlignRight.Click += new System.EventHandler(this.tsbAlignRight_Click);
            // 
            // tss3
            // 
            this.tss3.Name = "tss3";
            this.tss3.Size = new System.Drawing.Size(6, 23);
            // 
            // tsbUnorderedList
            // 
            this.tsbUnorderedList.CheckOnClick = true;
            this.tsbUnorderedList.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbUnorderedList.Image = global::Chummer.Properties.Resources.text_list_bullets;
            this.tsbUnorderedList.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbUnorderedList.Name = "tsbUnorderedList";
            this.tsbUnorderedList.Size = new System.Drawing.Size(23, 20);
            this.tsbUnorderedList.Tag = "String_UnorderedList";
            this.tsbUnorderedList.Text = "Toggle List";
            this.tsbUnorderedList.Click += new System.EventHandler(this.tsbUnorderedList_Click);
            // 
            // tsbIncreaseIndent
            // 
            this.tsbIncreaseIndent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbIncreaseIndent.Image = global::Chummer.Properties.Resources.text_indent;
            this.tsbIncreaseIndent.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbIncreaseIndent.Name = "tsbIncreaseIndent";
            this.tsbIncreaseIndent.Size = new System.Drawing.Size(23, 20);
            this.tsbIncreaseIndent.Tag = "String_IncreaseIndent";
            this.tsbIncreaseIndent.Text = "Increase Indent";
            this.tsbIncreaseIndent.Click += new System.EventHandler(this.tsbIncreaseIndent_Click);
            // 
            // tsbDecreaseIndent
            // 
            this.tsbDecreaseIndent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbDecreaseIndent.Image = global::Chummer.Properties.Resources.text_indent_remove;
            this.tsbDecreaseIndent.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbDecreaseIndent.Name = "tsbDecreaseIndent";
            this.tsbDecreaseIndent.Size = new System.Drawing.Size(23, 20);
            this.tsbDecreaseIndent.Tag = "String_DecreaseIndent";
            this.tsbDecreaseIndent.Text = "Decrease Indent";
            this.tsbDecreaseIndent.Click += new System.EventHandler(this.tsbDecreaseIndent_Click);
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.tsControls, 0, 0);
            this.tlpMain.Controls.Add(this.rtbContent, 0, 1);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(325, 60);
            this.tlpMain.TabIndex = 1;
            // 
            // rtbContent
            // 
            this.rtbContent.AcceptsTab = true;
            this.rtbContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtbContent.BulletIndent = 8;
            this.rtbContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbContent.Location = new System.Drawing.Point(3, 32);
            this.rtbContent.Name = "rtbContent";
            this.rtbContent.Size = new System.Drawing.Size(319, 25);
            this.rtbContent.TabIndex = 1;
            this.rtbContent.TabStop = false;
            this.rtbContent.Text = "";
            this.rtbContent.SelectionChanged += new System.EventHandler(this.UpdateButtons);
            this.rtbContent.Enter += new System.EventHandler(this.rtbContent_Enter);
            this.rtbContent.KeyDown += new System.Windows.Forms.KeyEventHandler(this.rtbContent_KeyDown);
            this.rtbContent.Leave += new System.EventHandler(this.rtbContent_Leave);
            // 
            // RtfEditor
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.MinimumSize = new System.Drawing.Size(0, 60);
            this.Name = "RtfEditor";
            this.Size = new System.Drawing.Size(325, 60);
            this.tsControls.ResumeLayout(false);
            this.tsControls.PerformLayout();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStrip tsControls;
        private System.Windows.Forms.ToolStripButton tsbBold;
        private System.Windows.Forms.ToolStripButton tsbItalic;
        private System.Windows.Forms.ToolStripButton tsbUnderline;
        private System.Windows.Forms.ToolStripSeparator tss2;
        private System.Windows.Forms.ToolStripButton tsbUnorderedList;
        private System.Windows.Forms.ToolStripButton tsbAlignLeft;
        private System.Windows.Forms.ToolStripButton tsbAlignCenter;
        private System.Windows.Forms.ToolStripButton tsbAlignRight;
        private System.Windows.Forms.ToolStripSeparator tss3;
        private System.Windows.Forms.ToolStripButton tsbIncreaseIndent;
        private System.Windows.Forms.ToolStripButton tsbDecreaseIndent;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.ToolStripButton tsbFont;
        private System.Windows.Forms.ToolStripSeparator tss1;
        private System.Windows.Forms.ToolStripButton tsbForeColor;
        private System.Windows.Forms.RichTextBox rtbContent;
        private System.Windows.Forms.ToolStripButton tsbBackColor;
        private System.Windows.Forms.ToolStripButton tsbStrikeout;
    }
}
