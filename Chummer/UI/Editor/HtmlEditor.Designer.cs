using System;

namespace Chummer.UI.Editor
{
    partial class HtmlEditor
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
            this.webContent = new System.Windows.Forms.WebBrowser();
            this.tsControls = new System.Windows.Forms.ToolStrip();
            this.tsbBold = new System.Windows.Forms.ToolStripButton();
            this.tsbItalic = new System.Windows.Forms.ToolStripButton();
            this.tsbUnderline = new System.Windows.Forms.ToolStripButton();
            this.tss2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbForeColor = new System.Windows.Forms.ToolStripButton();
            this.tss3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbImage = new System.Windows.Forms.ToolStripButton();
            this.tsbHyperlink = new System.Windows.Forms.ToolStripButton();
            this.tss4 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbAlignLeft = new System.Windows.Forms.ToolStripButton();
            this.tsbAlignCenter = new System.Windows.Forms.ToolStripButton();
            this.tsbAlignRight = new System.Windows.Forms.ToolStripButton();
            this.tsbAlignJustify = new System.Windows.Forms.ToolStripButton();
            this.tss5 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbUnorderedList = new System.Windows.Forms.ToolStripButton();
            this.tsbOrderedList = new System.Windows.Forms.ToolStripButton();
            this.tsbIncreaseIndent = new System.Windows.Forms.ToolStripButton();
            this.tsbDecreaseIndent = new System.Windows.Forms.ToolStripButton();
            this.tsbBackColor = new System.Windows.Forms.ToolStripButton();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.cboFont = new System.Windows.Forms.ToolStripComboBox();
            this.tss1 = new System.Windows.Forms.ToolStripSeparator();
            this.cboFontSize = new System.Windows.Forms.ToolStripComboBox();
            this.tsControls.SuspendLayout();
            this.tlpMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // webContent
            // 
            this.webContent.AllowNavigation = false;
            this.webContent.AllowWebBrowserDrop = false;
            this.webContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webContent.IsWebBrowserContextMenuEnabled = false;
            this.webContent.Location = new System.Drawing.Point(3, 28);
            this.webContent.Name = "webContent";
            this.webContent.Size = new System.Drawing.Size(640, 329);
            this.webContent.TabIndex = 0;
            this.webContent.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webContent_DocumentCompleted);
            this.webContent.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(this.webContent_Navigated);
            this.webContent.GotFocus += new System.EventHandler(this.webContent_GotFocus);
            // 
            // tsControls
            // 
            this.tsControls.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsControls.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cboFont,
            this.cboFontSize,
            this.tss1,
            this.tsbBold,
            this.tsbItalic,
            this.tsbUnderline,
            this.tss2,
            this.tsbForeColor,
            this.tsbBackColor,
            this.tss3,
            this.tsbHyperlink,
            this.tsbImage,
            this.tss4,
            this.tsbAlignLeft,
            this.tsbAlignCenter,
            this.tsbAlignRight,
            this.tsbAlignJustify,
            this.tss5,
            this.tsbUnorderedList,
            this.tsbOrderedList,
            this.tsbIncreaseIndent,
            this.tsbDecreaseIndent});
            this.tsControls.Location = new System.Drawing.Point(0, 0);
            this.tsControls.Name = "tsControls";
            this.tsControls.Size = new System.Drawing.Size(646, 25);
            this.tsControls.Stretch = true;
            this.tsControls.TabIndex = 0;
            // 
            // tsbBold
            // 
            this.tsbBold.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbBold.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tsbBold.Image = global::Chummer.Properties.Resources.text_bold;
            this.tsbBold.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbBold.Name = "tsbBold";
            this.tsbBold.Size = new System.Drawing.Size(23, 22);
            this.tsbBold.Click += new System.EventHandler(this.tsbBold_Click);
            // 
            // tsbItalic
            // 
            this.tsbItalic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbItalic.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tsbItalic.Image = global::Chummer.Properties.Resources.text_italic;
            this.tsbItalic.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbItalic.Name = "tsbItalic";
            this.tsbItalic.Size = new System.Drawing.Size(23, 22);
            this.tsbItalic.Click += new System.EventHandler(this.tsbItalic_Click);
            // 
            // tsbUnderline
            // 
            this.tsbUnderline.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbUnderline.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tsbUnderline.Image = global::Chummer.Properties.Resources.text_underline;
            this.tsbUnderline.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbUnderline.Name = "tsbUnderline";
            this.tsbUnderline.Size = new System.Drawing.Size(23, 22);
            this.tsbUnderline.Click += new System.EventHandler(this.tsbUnderline_Click);
            // 
            // tss2
            // 
            this.tss2.Name = "tss2";
            this.tss2.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbForeColor
            // 
            this.tsbForeColor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbForeColor.Image = global::Chummer.Properties.Resources.font;
            this.tsbForeColor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbForeColor.Name = "tsbForeColor";
            this.tsbForeColor.Size = new System.Drawing.Size(23, 22);
            this.tsbForeColor.Click += new System.EventHandler(this.tsbForeColor_Click);
            // 
            // tss3
            // 
            this.tss3.Name = "tss3";
            this.tss3.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbImage
            // 
            this.tsbImage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbImage.Image = global::Chummer.Properties.Resources.picture;
            this.tsbImage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbImage.Name = "tsbImage";
            this.tsbImage.Size = new System.Drawing.Size(23, 22);
            this.tsbImage.Click += new System.EventHandler(this.tsbImage_Click);
            // 
            // tsbHyperlink
            // 
            this.tsbHyperlink.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbHyperlink.Image = global::Chummer.Properties.Resources.link;
            this.tsbHyperlink.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbHyperlink.Name = "tsbHyperlink";
            this.tsbHyperlink.Size = new System.Drawing.Size(23, 22);
            this.tsbHyperlink.Click += new System.EventHandler(this.tsbHyperlink_Click);
            // 
            // tss4
            // 
            this.tss4.Name = "tss4";
            this.tss4.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbAlignLeft
            // 
            this.tsbAlignLeft.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbAlignLeft.Image = global::Chummer.Properties.Resources.text_align_left;
            this.tsbAlignLeft.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbAlignLeft.Name = "tsbAlignLeft";
            this.tsbAlignLeft.Size = new System.Drawing.Size(23, 22);
            this.tsbAlignLeft.Click += new System.EventHandler(this.tsbAlignLeft_Click);
            // 
            // tsbAlignCenter
            // 
            this.tsbAlignCenter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbAlignCenter.Image = global::Chummer.Properties.Resources.text_align_center;
            this.tsbAlignCenter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbAlignCenter.Name = "tsbAlignCenter";
            this.tsbAlignCenter.Size = new System.Drawing.Size(23, 22);
            this.tsbAlignCenter.Click += new System.EventHandler(this.tsbAlignCenter_Click);
            // 
            // tsbAlignRight
            // 
            this.tsbAlignRight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbAlignRight.Image = global::Chummer.Properties.Resources.text_align_right;
            this.tsbAlignRight.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbAlignRight.Name = "tsbAlignRight";
            this.tsbAlignRight.Size = new System.Drawing.Size(23, 22);
            this.tsbAlignRight.Click += new System.EventHandler(this.tsbAlignRight_Click);
            // 
            // tsbAlignJustify
            // 
            this.tsbAlignJustify.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbAlignJustify.Image = global::Chummer.Properties.Resources.text_align_justify;
            this.tsbAlignJustify.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbAlignJustify.Name = "tsbAlignJustify";
            this.tsbAlignJustify.Size = new System.Drawing.Size(23, 22);
            this.tsbAlignJustify.Text = "toolStripButton4";
            this.tsbAlignJustify.Click += new System.EventHandler(this.tsbAlignJustify_Click);
            // 
            // tss5
            // 
            this.tss5.Name = "tss5";
            this.tss5.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbUnorderedList
            // 
            this.tsbUnorderedList.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbUnorderedList.Image = global::Chummer.Properties.Resources.text_list_bullets;
            this.tsbUnorderedList.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbUnorderedList.Name = "tsbUnorderedList";
            this.tsbUnorderedList.Size = new System.Drawing.Size(23, 22);
            this.tsbUnorderedList.Click += new System.EventHandler(this.tsbUnorderedList_Click);
            // 
            // tsbOrderedList
            // 
            this.tsbOrderedList.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbOrderedList.Image = global::Chummer.Properties.Resources.text_list_numbers;
            this.tsbOrderedList.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbOrderedList.Name = "tsbOrderedList";
            this.tsbOrderedList.Size = new System.Drawing.Size(23, 22);
            this.tsbOrderedList.ToolTipText = "OList";
            this.tsbOrderedList.Click += new System.EventHandler(this.tsbOrderedList_Click);
            // 
            // tsbIncreaseIndent
            // 
            this.tsbIncreaseIndent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbIncreaseIndent.Image = global::Chummer.Properties.Resources.text_indent;
            this.tsbIncreaseIndent.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbIncreaseIndent.Name = "tsbIncreaseIndent";
            this.tsbIncreaseIndent.Size = new System.Drawing.Size(23, 22);
            this.tsbIncreaseIndent.Click += new System.EventHandler(this.tsbIncreaseIndent_Click);
            // 
            // tsbDecreaseIndent
            // 
            this.tsbDecreaseIndent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbDecreaseIndent.Image = global::Chummer.Properties.Resources.text_indent_remove;
            this.tsbDecreaseIndent.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbDecreaseIndent.Name = "tsbDecreaseIndent";
            this.tsbDecreaseIndent.Size = new System.Drawing.Size(23, 22);
            this.tsbDecreaseIndent.Click += new System.EventHandler(this.tsbDecreaseIndent_Click);
            // 
            // tsbBackColor
            // 
            this.tsbBackColor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbBackColor.Image = global::Chummer.Properties.Resources.color_wheel;
            this.tsbBackColor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbBackColor.Name = "tsbBackColor";
            this.tsbBackColor.Size = new System.Drawing.Size(23, 22);
            this.tsbBackColor.Click += new System.EventHandler(this.tsbBackColor_Click);
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.tsControls, 0, 0);
            this.tlpMain.Controls.Add(this.webContent, 0, 1);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(640, 360);
            this.tlpMain.TabIndex = 1;
            // 
            // cboFont
            // 
            this.cboFont.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cboFont.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.cboFont.Name = "cboFont";
            this.cboFont.Size = new System.Drawing.Size(125, 25);
            this.cboFont.Leave += new System.EventHandler(this.cboFont_Leave);
            // 
            // tss1
            // 
            this.tss1.Name = "tss1";
            this.tss1.Size = new System.Drawing.Size(6, 25);
            // 
            // cboFontSize
            // 
            this.cboFontSize.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7"});
            this.cboFontSize.Name = "cboFontSize";
            this.cboFontSize.Size = new System.Drawing.Size(75, 25);
            this.cboFontSize.Leave += new System.EventHandler(this.cboFontSize_Leave);
            this.cboFontSize.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cboFontSize_KeyPress);
            // 
            // HtmlEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tlpMain);
            this.Name = "HtmlEditor";
            this.Size = new System.Drawing.Size(640, 360);
            this.Enter += new System.EventHandler(this.HtmlEditor_Enter);
            this.Leave += new System.EventHandler(this.HtmlEditor_Leave);
            this.tsControls.ResumeLayout(false);
            this.tsControls.PerformLayout();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.WebBrowser webContent;
        private System.Windows.Forms.ToolStrip tsControls;
        private System.Windows.Forms.ToolStripButton tsbBold;
        private System.Windows.Forms.ToolStripButton tsbItalic;
        private System.Windows.Forms.ToolStripButton tsbUnderline;
        private System.Windows.Forms.ToolStripSeparator tss3;
        private System.Windows.Forms.ToolStripButton tsbUnorderedList;
        private System.Windows.Forms.ToolStripButton tsbOrderedList;
        private System.Windows.Forms.ToolStripSeparator tss4;
        private System.Windows.Forms.ToolStripButton tsbAlignLeft;
        private System.Windows.Forms.ToolStripButton tsbAlignCenter;
        private System.Windows.Forms.ToolStripButton tsbAlignRight;
        private System.Windows.Forms.ToolStripButton tsbAlignJustify;
        private System.Windows.Forms.ToolStripSeparator tss5;
        private System.Windows.Forms.ToolStripButton tsbIncreaseIndent;
        private System.Windows.Forms.ToolStripButton tsbDecreaseIndent;
        private System.Windows.Forms.ToolStripButton tsbImage;
        private System.Windows.Forms.ToolStripButton tsbHyperlink;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.ToolStripButton tsbForeColor;
        private System.Windows.Forms.ToolStripSeparator tss2;
        private System.Windows.Forms.ToolStripButton tsbBackColor;
        private System.Windows.Forms.ToolStripComboBox cboFont;
        private System.Windows.Forms.ToolStripComboBox cboFontSize;
        private System.Windows.Forms.ToolStripSeparator tss1;
    }
}
