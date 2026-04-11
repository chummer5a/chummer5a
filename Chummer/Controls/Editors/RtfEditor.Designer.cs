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
            if (disposing)
            {
                foreach (System.Windows.Forms.Control objControl in Controls)
                    objControl.ResetBindings();
                if (components != null)
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
            this.components = new System.ComponentModel.Container();
            this.tsControls = new System.Windows.Forms.ToolStrip();
            this.tsbBold = new Chummer.DpiFriendlyToolStripButton();
            this.tsbItalic = new Chummer.DpiFriendlyToolStripButton();
            this.tsbUnderline = new Chummer.DpiFriendlyToolStripButton();
            this.tsbStrikeout = new Chummer.DpiFriendlyToolStripButton();
            this.tss1 = new Chummer.ColorableToolStripSeparator();
            this.tsbFont = new Chummer.DpiFriendlyToolStripButton();
            this.tsbForeColor = new Chummer.DpiFriendlyToolStripButton();
            this.tsbBackColor = new Chummer.DpiFriendlyToolStripButton();
            this.tss2 = new Chummer.ColorableToolStripSeparator();
            this.tsbSuperscript = new Chummer.DpiFriendlyToolStripButton();
            this.tsbSubscript = new Chummer.DpiFriendlyToolStripButton();
            this.tss3 = new Chummer.ColorableToolStripSeparator();
            this.tsbAlignLeft = new Chummer.DpiFriendlyToolStripButton();
            this.tsbAlignCenter = new Chummer.DpiFriendlyToolStripButton();
            this.tsbAlignRight = new Chummer.DpiFriendlyToolStripButton();
            this.tss4 = new Chummer.ColorableToolStripSeparator();
            this.tsbUnorderedList = new Chummer.DpiFriendlyToolStripButton();
            this.tsbIncreaseIndent = new Chummer.DpiFriendlyToolStripButton();
            this.tsbDecreaseIndent = new Chummer.DpiFriendlyToolStripButton();
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
            this.tsbSuperscript,
            this.tsbSubscript,
            this.tss3,
            this.tsbAlignLeft,
            this.tsbAlignCenter,
            this.tsbAlignRight,
            this.tss4,
            this.tsbUnorderedList,
            this.tsbIncreaseIndent,
            this.tsbDecreaseIndent});
            this.tsControls.Location = new System.Drawing.Point(0, 0);
            this.tsControls.Name = "tsControls";
            this.tsControls.Padding = new System.Windows.Forms.Padding(3);
            this.tsControls.Size = new System.Drawing.Size(408, 29);
            this.tsControls.Stretch = true;
            this.tsControls.TabIndex = 0;
            // 
            // tsbBold
            // 
            this.tsbBold.CheckOnClick = true;
            this.tsbBold.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbBold.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tsbBold.Image = global::Chummer.Properties.Resources.text_bold_16;
            this.tsbBold.ImageDpi120 = global::Chummer.Properties.Resources.text_bold_20;
            this.tsbBold.ImageDpi144 = global::Chummer.Properties.Resources.text_bold_24;
            this.tsbBold.ImageDpi192 = global::Chummer.Properties.Resources.text_bold_32;
            this.tsbBold.ImageDpi288 = global::Chummer.Properties.Resources.text_bold_48;
            this.tsbBold.ImageDpi384 = global::Chummer.Properties.Resources.text_bold_64;
            this.tsbBold.ImageDpi96 = global::Chummer.Properties.Resources.text_bold_16;
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
            this.tsbItalic.Image = global::Chummer.Properties.Resources.text_italic_16;
            this.tsbItalic.ImageDpi120 = global::Chummer.Properties.Resources.text_italic_20;
            this.tsbItalic.ImageDpi144 = global::Chummer.Properties.Resources.text_italic_24;
            this.tsbItalic.ImageDpi192 = global::Chummer.Properties.Resources.text_italic_32;
            this.tsbItalic.ImageDpi288 = global::Chummer.Properties.Resources.text_italic_48;
            this.tsbItalic.ImageDpi384 = global::Chummer.Properties.Resources.text_italic_64;
            this.tsbItalic.ImageDpi96 = global::Chummer.Properties.Resources.text_italic_16;
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
            this.tsbUnderline.Image = global::Chummer.Properties.Resources.text_underline_16;
            this.tsbUnderline.ImageDpi120 = global::Chummer.Properties.Resources.text_underline_20;
            this.tsbUnderline.ImageDpi144 = global::Chummer.Properties.Resources.text_underline_24;
            this.tsbUnderline.ImageDpi192 = global::Chummer.Properties.Resources.text_underline_32;
            this.tsbUnderline.ImageDpi288 = global::Chummer.Properties.Resources.text_underline_48;
            this.tsbUnderline.ImageDpi384 = global::Chummer.Properties.Resources.text_underline_64;
            this.tsbUnderline.ImageDpi96 = global::Chummer.Properties.Resources.text_underline_16;
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
            this.tsbStrikeout.Image = global::Chummer.Properties.Resources.text_strikethrough_16;
            this.tsbStrikeout.ImageDpi120 = global::Chummer.Properties.Resources.text_strikethrough_20;
            this.tsbStrikeout.ImageDpi144 = global::Chummer.Properties.Resources.text_strikethrough_24;
            this.tsbStrikeout.ImageDpi192 = global::Chummer.Properties.Resources.text_strikethrough_32;
            this.tsbStrikeout.ImageDpi288 = global::Chummer.Properties.Resources.text_strikethrough_48;
            this.tsbStrikeout.ImageDpi384 = global::Chummer.Properties.Resources.text_strikethrough_64;
            this.tsbStrikeout.ImageDpi96 = global::Chummer.Properties.Resources.text_strikethrough_16;
            this.tsbStrikeout.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbStrikeout.Name = "tsbStrikeout";
            this.tsbStrikeout.Size = new System.Drawing.Size(23, 20);
            this.tsbStrikeout.Tag = "String_Strikethrough";
            this.tsbStrikeout.Text = "Strikethrough";
            this.tsbStrikeout.Click += new System.EventHandler(this.UpdateFont);
            // 
            // tss1
            // 
            this.tss1.DefaultColorScheme = true;
            this.tss1.Name = "tss1";
            this.tss1.Size = new System.Drawing.Size(6, 23);
            // 
            // tsbFont
            // 
            this.tsbFont.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbFont.Image = global::Chummer.Properties.Resources.font_16;
            this.tsbFont.ImageDpi120 = global::Chummer.Properties.Resources.font_20;
            this.tsbFont.ImageDpi144 = global::Chummer.Properties.Resources.font_24;
            this.tsbFont.ImageDpi192 = global::Chummer.Properties.Resources.font_32;
            this.tsbFont.ImageDpi288 = global::Chummer.Properties.Resources.font_48;
            this.tsbFont.ImageDpi384 = global::Chummer.Properties.Resources.font_64;
            this.tsbFont.ImageDpi96 = global::Chummer.Properties.Resources.font_16;
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
            this.tsbForeColor.Image = global::Chummer.Properties.Resources.color_wheel_16;
            this.tsbForeColor.ImageDpi120 = global::Chummer.Properties.Resources.color_wheel_20;
            this.tsbForeColor.ImageDpi144 = global::Chummer.Properties.Resources.color_wheel_24;
            this.tsbForeColor.ImageDpi192 = global::Chummer.Properties.Resources.color_wheel_32;
            this.tsbForeColor.ImageDpi288 = global::Chummer.Properties.Resources.color_wheel_48;
            this.tsbForeColor.ImageDpi384 = global::Chummer.Properties.Resources.color_wheel_64;
            this.tsbForeColor.ImageDpi96 = global::Chummer.Properties.Resources.color_wheel_16;
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
            this.tsbBackColor.Image = global::Chummer.Properties.Resources.paintcan_16;
            this.tsbBackColor.ImageDpi120 = global::Chummer.Properties.Resources.paintcan_20;
            this.tsbBackColor.ImageDpi144 = global::Chummer.Properties.Resources.paintcan_24;
            this.tsbBackColor.ImageDpi192 = global::Chummer.Properties.Resources.paintcan_32;
            this.tsbBackColor.ImageDpi288 = global::Chummer.Properties.Resources.paintcan_48;
            this.tsbBackColor.ImageDpi384 = global::Chummer.Properties.Resources.paintcan_64;
            this.tsbBackColor.ImageDpi96 = global::Chummer.Properties.Resources.paintcan_16;
            this.tsbBackColor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbBackColor.Name = "tsbBackColor";
            this.tsbBackColor.Size = new System.Drawing.Size(23, 20);
            this.tsbBackColor.Tag = "String_BackColor";
            this.tsbBackColor.Text = "Background Color";
            this.tsbBackColor.Click += new System.EventHandler(this.tsbBackColor_Click);
            // 
            // tss2
            // 
            this.tss2.DefaultColorScheme = true;
            this.tss2.Name = "tss2";
            this.tss2.Size = new System.Drawing.Size(6, 23);
            // 
            // tsbSuperscript
            // 
            this.tsbSuperscript.CheckOnClick = true;
            this.tsbSuperscript.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbSuperscript.Image = global::Chummer.Properties.Resources.text_superscript_16;
            this.tsbSuperscript.ImageDpi120 = global::Chummer.Properties.Resources.text_superscript_20;
            this.tsbSuperscript.ImageDpi144 = global::Chummer.Properties.Resources.text_superscript_24;
            this.tsbSuperscript.ImageDpi192 = global::Chummer.Properties.Resources.text_superscript_32;
            this.tsbSuperscript.ImageDpi288 = global::Chummer.Properties.Resources.text_superscript_48;
            this.tsbSuperscript.ImageDpi384 = global::Chummer.Properties.Resources.text_superscript_64;
            this.tsbSuperscript.ImageDpi96 = global::Chummer.Properties.Resources.text_superscript_16;
            this.tsbSuperscript.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbSuperscript.Name = "tsbSuperscript";
            this.tsbSuperscript.Size = new System.Drawing.Size(23, 20);
            this.tsbSuperscript.Text = "Superscript";
            this.tsbSuperscript.Click += new System.EventHandler(this.tsbSuperscript_Click);
            // 
            // tsbSubscript
            // 
            this.tsbSubscript.CheckOnClick = true;
            this.tsbSubscript.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbSubscript.Image = global::Chummer.Properties.Resources.text_subscript_16;
            this.tsbSubscript.ImageDpi120 = global::Chummer.Properties.Resources.text_subscript_20;
            this.tsbSubscript.ImageDpi144 = global::Chummer.Properties.Resources.text_subscript_24;
            this.tsbSubscript.ImageDpi192 = global::Chummer.Properties.Resources.text_subscript_32;
            this.tsbSubscript.ImageDpi288 = global::Chummer.Properties.Resources.text_subscript_48;
            this.tsbSubscript.ImageDpi384 = global::Chummer.Properties.Resources.text_subscript_64;
            this.tsbSubscript.ImageDpi96 = global::Chummer.Properties.Resources.text_subscript_16;
            this.tsbSubscript.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbSubscript.Name = "tsbSubscript";
            this.tsbSubscript.Size = new System.Drawing.Size(23, 20);
            this.tsbSubscript.Text = "Subscript";
            this.tsbSubscript.Click += new System.EventHandler(this.tsbSubscript_Click);
            // 
            // tss3
            // 
            this.tss3.DefaultColorScheme = true;
            this.tss3.Name = "tss3";
            this.tss3.Size = new System.Drawing.Size(6, 23);
            // 
            // tsbAlignLeft
            // 
            this.tsbAlignLeft.CheckOnClick = true;
            this.tsbAlignLeft.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbAlignLeft.Image = global::Chummer.Properties.Resources.text_align_left_16;
            this.tsbAlignLeft.ImageDpi120 = global::Chummer.Properties.Resources.text_align_left_20;
            this.tsbAlignLeft.ImageDpi144 = global::Chummer.Properties.Resources.text_align_left_24;
            this.tsbAlignLeft.ImageDpi192 = global::Chummer.Properties.Resources.text_align_left_32;
            this.tsbAlignLeft.ImageDpi288 = global::Chummer.Properties.Resources.text_align_left_48;
            this.tsbAlignLeft.ImageDpi384 = global::Chummer.Properties.Resources.text_align_left_64;
            this.tsbAlignLeft.ImageDpi96 = global::Chummer.Properties.Resources.text_align_left_16;
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
            this.tsbAlignCenter.Image = global::Chummer.Properties.Resources.text_align_center_16;
            this.tsbAlignCenter.ImageDpi120 = global::Chummer.Properties.Resources.text_align_center_20;
            this.tsbAlignCenter.ImageDpi144 = global::Chummer.Properties.Resources.text_align_center_24;
            this.tsbAlignCenter.ImageDpi192 = global::Chummer.Properties.Resources.text_align_center_32;
            this.tsbAlignCenter.ImageDpi288 = global::Chummer.Properties.Resources.text_align_center_48;
            this.tsbAlignCenter.ImageDpi384 = global::Chummer.Properties.Resources.text_align_center_64;
            this.tsbAlignCenter.ImageDpi96 = global::Chummer.Properties.Resources.text_align_center_16;
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
            this.tsbAlignRight.Image = global::Chummer.Properties.Resources.text_align_right_16;
            this.tsbAlignRight.ImageDpi120 = global::Chummer.Properties.Resources.text_align_right_20;
            this.tsbAlignRight.ImageDpi144 = global::Chummer.Properties.Resources.text_align_right_24;
            this.tsbAlignRight.ImageDpi192 = global::Chummer.Properties.Resources.text_align_right_32;
            this.tsbAlignRight.ImageDpi288 = global::Chummer.Properties.Resources.text_align_right_48;
            this.tsbAlignRight.ImageDpi384 = global::Chummer.Properties.Resources.text_align_right_64;
            this.tsbAlignRight.ImageDpi96 = global::Chummer.Properties.Resources.text_align_right_16;
            this.tsbAlignRight.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbAlignRight.Name = "tsbAlignRight";
            this.tsbAlignRight.Size = new System.Drawing.Size(23, 20);
            this.tsbAlignRight.Tag = "String_AlignRight";
            this.tsbAlignRight.Text = "Align Right";
            this.tsbAlignRight.CheckedChanged += new System.EventHandler(this.tsbAlignRight_CheckedChanged);
            this.tsbAlignRight.Click += new System.EventHandler(this.tsbAlignRight_Click);
            // 
            // tss4
            // 
            this.tss4.DefaultColorScheme = true;
            this.tss4.Name = "tss4";
            this.tss4.Size = new System.Drawing.Size(6, 23);
            // 
            // tsbUnorderedList
            // 
            this.tsbUnorderedList.CheckOnClick = true;
            this.tsbUnorderedList.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbUnorderedList.Image = global::Chummer.Properties.Resources.text_list_bullets_16;
            this.tsbUnorderedList.ImageDpi120 = global::Chummer.Properties.Resources.text_list_bullets_20;
            this.tsbUnorderedList.ImageDpi144 = global::Chummer.Properties.Resources.text_list_bullets_24;
            this.tsbUnorderedList.ImageDpi192 = global::Chummer.Properties.Resources.text_list_bullets_32;
            this.tsbUnorderedList.ImageDpi288 = global::Chummer.Properties.Resources.text_list_bullets_48;
            this.tsbUnorderedList.ImageDpi384 = global::Chummer.Properties.Resources.text_list_bullets_64;
            this.tsbUnorderedList.ImageDpi96 = global::Chummer.Properties.Resources.text_list_bullets_16;
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
            this.tsbIncreaseIndent.Image = global::Chummer.Properties.Resources.text_indent_16;
            this.tsbIncreaseIndent.ImageDpi120 = global::Chummer.Properties.Resources.text_indent_20;
            this.tsbIncreaseIndent.ImageDpi144 = global::Chummer.Properties.Resources.text_indent_24;
            this.tsbIncreaseIndent.ImageDpi192 = global::Chummer.Properties.Resources.text_indent_32;
            this.tsbIncreaseIndent.ImageDpi288 = global::Chummer.Properties.Resources.text_indent_48;
            this.tsbIncreaseIndent.ImageDpi384 = global::Chummer.Properties.Resources.text_indent_64;
            this.tsbIncreaseIndent.ImageDpi96 = global::Chummer.Properties.Resources.text_indent_16;
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
            this.tsbDecreaseIndent.Image = global::Chummer.Properties.Resources.text_indent_remove_16;
            this.tsbDecreaseIndent.ImageDpi120 = global::Chummer.Properties.Resources.text_indent_remove_20;
            this.tsbDecreaseIndent.ImageDpi144 = global::Chummer.Properties.Resources.text_indent_remove_24;
            this.tsbDecreaseIndent.ImageDpi192 = global::Chummer.Properties.Resources.text_indent_remove_32;
            this.tsbDecreaseIndent.ImageDpi288 = global::Chummer.Properties.Resources.text_indent_remove_48;
            this.tsbDecreaseIndent.ImageDpi384 = global::Chummer.Properties.Resources.text_indent_remove_64;
            this.tsbDecreaseIndent.ImageDpi96 = global::Chummer.Properties.Resources.text_indent_remove_16;
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
            this.tlpMain.Size = new System.Drawing.Size(408, 60);
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
            this.rtbContent.Size = new System.Drawing.Size(402, 25);
            this.rtbContent.TabIndex = 1;
            this.rtbContent.TabStop = false;
            this.rtbContent.Text = "";
            this.rtbContent.SelectionChanged += new System.EventHandler(this.UpdateButtons);
            this.rtbContent.TextChanged += new System.EventHandler(this.rtbContent_TextChanged);
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
            this.Size = new System.Drawing.Size(408, 60);
            this.tsControls.ResumeLayout(false);
            this.tsControls.PerformLayout();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStrip tsControls;
        private Chummer.ColorableToolStripSeparator tss3;
        private Chummer.ColorableToolStripSeparator tss4;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private Chummer.ColorableToolStripSeparator tss1;
        private System.Windows.Forms.RichTextBox rtbContent;
        private Chummer.ColorableToolStripSeparator tss2;
        private DpiFriendlyToolStripButton tsbBold;
        private DpiFriendlyToolStripButton tsbItalic;
        private DpiFriendlyToolStripButton tsbUnderline;
        private DpiFriendlyToolStripButton tsbStrikeout;
        private DpiFriendlyToolStripButton tsbFont;
        private DpiFriendlyToolStripButton tsbForeColor;
        private DpiFriendlyToolStripButton tsbBackColor;
        private DpiFriendlyToolStripButton tsbSuperscript;
        private DpiFriendlyToolStripButton tsbSubscript;
        private DpiFriendlyToolStripButton tsbAlignLeft;
        private DpiFriendlyToolStripButton tsbAlignCenter;
        private DpiFriendlyToolStripButton tsbAlignRight;
        private DpiFriendlyToolStripButton tsbUnorderedList;
        private DpiFriendlyToolStripButton tsbIncreaseIndent;
        private DpiFriendlyToolStripButton tsbDecreaseIndent;
    }
}
