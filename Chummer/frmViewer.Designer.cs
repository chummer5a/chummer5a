using System;

namespace Chummer
{
    partial class frmViewer
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.cmdSaveHTML = new SplitButton();
            this.cmsSaveButton = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsSaveAsXml = new System.Windows.Forms.ToolStripMenuItem();
            this.tsSaveAsPdf = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsPrintButton = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsPrintPreview = new System.Windows.Forms.ToolStripMenuItem();
            this.cmdPrint = new SplitButton();
            this.SaveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.cboXSLT = new System.Windows.Forms.ComboBox();
            this.lblCharacterSheet = new System.Windows.Forms.Label();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.cmsSaveButton.SuspendLayout();
            this.cmsPrintButton.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdSaveHTML
            // 
            this.cmdSaveHTML.AutoSize = true;
            this.cmdSaveHTML.ContextMenuStrip = this.cmsSaveButton;
            this.cmdSaveHTML.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.cmdSaveHTML.Location = new System.Drawing.Point(98, 12);
            this.cmdSaveHTML.Name = "cmdSaveHTML";
            this.cmdSaveHTML.Size = new System.Drawing.Size(107, 23);
            this.cmdSaveHTML.SplitMenuStrip = this.cmsSaveButton;
            this.cmdSaveHTML.TabIndex = 1;
            this.cmdSaveHTML.Tag = "Button_Viewer_SaveAsHtml";
            this.cmdSaveHTML.Text = "Save as HTML";
            this.cmdSaveHTML.UseVisualStyleBackColor = true;
            this.cmdSaveHTML.Click += new System.EventHandler(this.cmdSaveHTML_Click);
            // 
            // cmsSaveButton
            // 
            this.cmsSaveButton.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsSaveAsXml,
            this.tsSaveAsPdf});
            this.cmsSaveButton.Name = "cmsPrintButton";
            this.cmsSaveButton.Size = new System.Drawing.Size(140, 48);
            // 
            // tsSaveAsXml
            // 
            this.tsSaveAsXml.Name = "tsSaveAsXml";
            this.tsSaveAsXml.Size = new System.Drawing.Size(139, 22);
            this.tsSaveAsXml.Tag = "Button_Viewer_SaveAsXml";
            this.tsSaveAsXml.Text = "Save as XML";
            this.tsSaveAsXml.Click += new EventHandler(this.tsSaveAsXml_Click);
            // 
            // tsSaveAsPdf
            // 
            this.tsSaveAsPdf.Name = "tsSaveAsPdf";
            this.tsSaveAsPdf.Size = new System.Drawing.Size(139, 22);
            this.tsSaveAsPdf.Tag = "Button_Viewer_SaveAsPdf";
            this.tsSaveAsPdf.Text = "Save as PDF";
            this.tsSaveAsPdf.Click += new EventHandler(this.tsSaveAsPdf_Click);
            // 
            // cmsPrintButton
            // 
            this.cmsPrintButton.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsPrintPreview});
            this.cmsPrintButton.Name = "cmsPrintButton";
            this.cmsPrintButton.Size = new System.Drawing.Size(144, 26);
            this.cmsPrintButton.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
            // 
            // tsPrintPreview
            // 
            this.tsPrintPreview.Name = "tsPrintPreview";
            this.tsPrintPreview.Size = new System.Drawing.Size(143, 22);
            this.tsPrintPreview.Tag = "Menu_FilePrintPreview";
            this.tsPrintPreview.Text = "&Print Preview";
            this.tsPrintPreview.Click += new System.EventHandler(this.tsPrintPreview_Click);
            // 
            // cmdPrint
            // 
            this.cmdPrint.AutoSize = true;
            this.cmdPrint.ContextMenuStrip = this.cmsPrintButton;
            this.cmdPrint.Location = new System.Drawing.Point(12, 12);
            this.cmdPrint.Name = "cmdPrint";
            this.cmdPrint.Size = new System.Drawing.Size(80, 23);
            this.cmdPrint.SplitMenuStrip = this.cmsPrintButton;
            this.cmdPrint.TabIndex = 103;
            this.cmdPrint.Tag = "Menu_FilePrint";
            this.cmdPrint.Text = "&Print";
            this.cmdPrint.UseVisualStyleBackColor = true;
            this.cmdPrint.Click += new System.EventHandler(this.cmdPrint_Click);
            // 
            // cboXSLT
            // 
            this.cboXSLT.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboXSLT.FormattingEnabled = true;
            this.cboXSLT.Location = new System.Drawing.Point(509, 14);
            this.cboXSLT.Name = "cboXSLT";
            this.cboXSLT.Size = new System.Drawing.Size(216, 21);
            this.cboXSLT.TabIndex = 4;
            this.cboXSLT.SelectedIndexChanged += new System.EventHandler(this.cboXSLT_SelectedIndexChanged);
            // 
            // lblCharacterSheet
            // 
            this.lblCharacterSheet.AutoSize = true;
            this.lblCharacterSheet.Location = new System.Drawing.Point(416, 17);
            this.lblCharacterSheet.Name = "lblCharacterSheet";
            this.lblCharacterSheet.Size = new System.Drawing.Size(87, 13);
            this.lblCharacterSheet.TabIndex = 3;
            this.lblCharacterSheet.Tag = "Label_Viewer_CharacterSheet";
            this.lblCharacterSheet.Text = "Character Sheet:";
            // 
            // webBrowser1
            // 
            this.webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser1.Location = new System.Drawing.Point(12, 41);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.ScriptErrorsSuppressed = true;
            this.webBrowser1.Size = new System.Drawing.Size(761, 583);
            this.webBrowser1.TabIndex = 5;
            this.webBrowser1.WebBrowserShortcutsEnabled = false;
            // 
            // frmViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 637);
            this.Controls.Add(this.lblCharacterSheet);
            this.Controls.Add(this.cboXSLT);
            this.Controls.Add(this.cmdSaveHTML);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.cmdPrint);
            this.Name = "frmViewer";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_CharacterViewer";
            this.Text = "Character Viewer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmViewer_FormClosing);
            this.Load += new System.EventHandler(this.frmViewer_Load);
            this.cmsSaveButton.ResumeLayout(false);
            this.cmsPrintButton.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        internal SplitButton cmdPrint;
        private System.Windows.Forms.ContextMenuStrip cmsPrintButton;
        private System.Windows.Forms.ToolStripMenuItem tsPrintPreview;
        internal System.Windows.Forms.SaveFileDialog SaveFileDialog1;
        private System.Windows.Forms.ComboBox cboXSLT;
        private System.Windows.Forms.Label lblCharacterSheet;
        private System.Windows.Forms.ContextMenuStrip cmsSaveButton;
        internal SplitButton cmdSaveHTML;
        private System.Windows.Forms.ToolStripMenuItem tsSaveAsXml;
        private System.Windows.Forms.ToolStripMenuItem tsSaveAsPdf;
        private System.Windows.Forms.WebBrowser webBrowser1;
    }

}