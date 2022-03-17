using System;

namespace Chummer
{
    partial class CharacterSheetViewer
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
                _lstCharacters.Dispose();
                _objRefresherCancellationTokenSource?.Dispose();
                _objOutputGeneratorCancellationTokenSource?.Dispose();
                _objGenericFormClosingCancellationTokenSource.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CharacterSheetViewer));
            this.cmsSaveButton = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsSaveAsXml = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.tsSaveAsHtml = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.cmsPrintButton = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsPrintPreview = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.SaveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.webViewer = new System.Windows.Forms.WebBrowser();
            this.cmdPrint = new SplitButton();
            this.cboXSLT = new Chummer.ElasticComboBox();
            this.cboLanguage = new Chummer.ElasticComboBox();
            this.cmdSaveAsPdf = new SplitButton();
            this.lblCharacterSheet = new System.Windows.Forms.Label();
            this.imgSheetLanguageFlag = new System.Windows.Forms.PictureBox();
            this.cmsSaveButton.SuspendLayout();
            this.cmsPrintButton.SuspendLayout();
            this.tlpMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgSheetLanguageFlag)).BeginInit();
            this.SuspendLayout();
            // 
            // cmsSaveButton
            // 
            this.cmsSaveButton.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsSaveAsXml,
            this.tsSaveAsHtml});
            this.cmsSaveButton.Name = "cmsPrintButton";
            this.cmsSaveButton.Size = new System.Drawing.Size(148, 48);
            // 
            // tsSaveAsXml
            // 
            this.tsSaveAsXml.Enabled = false;
            this.tsSaveAsXml.Image = null;
            this.tsSaveAsXml.ImageDpi120 = null;
            this.tsSaveAsXml.ImageDpi144 = null;
            this.tsSaveAsXml.ImageDpi192 = null;
            this.tsSaveAsXml.ImageDpi288 = null;
            this.tsSaveAsXml.ImageDpi384 = null;
            this.tsSaveAsXml.ImageDpi96 = null;
            this.tsSaveAsXml.Name = "tsSaveAsXml";
            this.tsSaveAsXml.Size = new System.Drawing.Size(147, 22);
            this.tsSaveAsXml.Tag = "Button_Viewer_SaveAsXml";
            this.tsSaveAsXml.Text = "Save as XML";
            this.tsSaveAsXml.Click += new System.EventHandler(this.tsSaveAsXml_Click);
            // 
            // tsSaveAsHtml
            // 
            this.tsSaveAsHtml.Enabled = false;
            this.tsSaveAsHtml.Image = null;
            this.tsSaveAsHtml.ImageDpi120 = null;
            this.tsSaveAsHtml.ImageDpi144 = null;
            this.tsSaveAsHtml.ImageDpi192 = null;
            this.tsSaveAsHtml.ImageDpi288 = null;
            this.tsSaveAsHtml.ImageDpi384 = null;
            this.tsSaveAsHtml.ImageDpi96 = null;
            this.tsSaveAsHtml.Name = "tsSaveAsHtml";
            this.tsSaveAsHtml.Size = new System.Drawing.Size(147, 22);
            this.tsSaveAsHtml.Tag = "Button_Viewer_SaveAsHtml";
            this.tsSaveAsHtml.Text = "Save as &HTML";
            this.tsSaveAsHtml.Click += new System.EventHandler(this.tsSaveAsHTML_Click);
            // 
            // cmsPrintButton
            // 
            this.cmsPrintButton.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsPrintPreview});
            this.cmsPrintButton.Name = "cmsPrintButton";
            this.cmsPrintButton.Size = new System.Drawing.Size(144, 26);
            // 
            // tsPrintPreview
            // 
            this.tsPrintPreview.Enabled = false;
            this.tsPrintPreview.Image = null;
            this.tsPrintPreview.ImageDpi120 = null;
            this.tsPrintPreview.ImageDpi144 = null;
            this.tsPrintPreview.ImageDpi192 = null;
            this.tsPrintPreview.ImageDpi288 = null;
            this.tsPrintPreview.ImageDpi384 = null;
            this.tsPrintPreview.ImageDpi96 = null;
            this.tsPrintPreview.Name = "tsPrintPreview";
            this.tsPrintPreview.Size = new System.Drawing.Size(143, 22);
            this.tsPrintPreview.Tag = "Menu_FilePrintPreview";
            this.tsPrintPreview.Text = "&Print Preview";
            this.tsPrintPreview.Click += new System.EventHandler(this.tsPrintPreview_Click);
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 6;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.webViewer, 0, 1);
            this.tlpMain.Controls.Add(this.cmdPrint, 0, 0);
            this.tlpMain.Controls.Add(this.cboXSLT, 5, 0);
            this.tlpMain.Controls.Add(this.cboLanguage, 4, 0);
            this.tlpMain.Controls.Add(this.cmdSaveAsPdf, 1, 0);
            this.tlpMain.Controls.Add(this.lblCharacterSheet, 2, 0);
            this.tlpMain.Controls.Add(this.imgSheetLanguageFlag, 3, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(766, 543);
            this.tlpMain.TabIndex = 105;
            // 
            // webViewer
            // 
            this.webViewer.AllowWebBrowserDrop = false;
            this.tlpMain.SetColumnSpan(this.webViewer, 6);
            this.webViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webViewer.IsWebBrowserContextMenuEnabled = false;
            this.webViewer.Location = new System.Drawing.Point(3, 32);
            this.webViewer.MinimumSize = new System.Drawing.Size(20, 20);
            this.webViewer.Name = "webViewer";
            this.webViewer.ScriptErrorsSuppressed = true;
            this.webViewer.Size = new System.Drawing.Size(760, 508);
            this.webViewer.TabIndex = 5;
            this.webViewer.WebBrowserShortcutsEnabled = false;
            this.webViewer.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webViewer_DocumentCompleted);
            // 
            // cmdPrint
            // 
            this.cmdPrint.AutoSize = true;
            this.cmdPrint.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdPrint.ContextMenuStrip = this.cmsPrintButton;
            this.cmdPrint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdPrint.Enabled = false;
            this.cmdPrint.Location = new System.Drawing.Point(3, 3);
            this.cmdPrint.Name = "cmdPrint";
            this.cmdPrint.Size = new System.Drawing.Size(56, 23);
            this.cmdPrint.SplitMenuStrip = this.cmsPrintButton;
            this.cmdPrint.TabIndex = 103;
            this.cmdPrint.Tag = "Menu_FilePrint";
            this.cmdPrint.Text = "&Print";
            this.cmdPrint.UseVisualStyleBackColor = true;
            this.cmdPrint.Click += new System.EventHandler(this.cmdPrint_Click);
            // 
            // cboXSLT
            // 
            this.cboXSLT.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboXSLT.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboXSLT.FormattingEnabled = true;
            this.cboXSLT.Location = new System.Drawing.Point(509, 4);
            this.cboXSLT.Name = "cboXSLT";
            this.cboXSLT.Size = new System.Drawing.Size(254, 21);
            this.cboXSLT.TabIndex = 4;
            this.cboXSLT.TooltipText = "";
            this.cboXSLT.SelectedIndexChanged += new System.EventHandler(this.cboXSLT_SelectedIndexChanged);
            // 
            // cboLanguage
            // 
            this.cboLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLanguage.FormattingEnabled = true;
            this.cboLanguage.Location = new System.Drawing.Point(341, 4);
            this.cboLanguage.Name = "cboLanguage";
            this.cboLanguage.Size = new System.Drawing.Size(162, 21);
            this.cboLanguage.TabIndex = 104;
            this.cboLanguage.TooltipText = "";
            this.cboLanguage.SelectedIndexChanged += new System.EventHandler(this.cboLanguage_SelectedIndexChanged);
            // 
            // cmdSaveAsPdf
            // 
            this.cmdSaveAsPdf.AutoSize = true;
            this.cmdSaveAsPdf.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdSaveAsPdf.ContextMenuStrip = this.cmsSaveButton;
            this.cmdSaveAsPdf.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdSaveAsPdf.Enabled = false;
            this.cmdSaveAsPdf.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.cmdSaveAsPdf.Location = new System.Drawing.Point(65, 3);
            this.cmdSaveAsPdf.Name = "cmdSaveAsPdf";
            this.cmdSaveAsPdf.Size = new System.Drawing.Size(98, 23);
            this.cmdSaveAsPdf.SplitMenuStrip = this.cmsSaveButton;
            this.cmdSaveAsPdf.TabIndex = 1;
            this.cmdSaveAsPdf.Tag = "Button_Viewer_SaveAsPdf";
            this.cmdSaveAsPdf.Text = "&Save as PDF";
            this.cmdSaveAsPdf.UseVisualStyleBackColor = true;
            this.cmdSaveAsPdf.Click += new System.EventHandler(this.cmdSaveAsPdf_Click);
            // 
            // lblCharacterSheet
            // 
            this.lblCharacterSheet.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCharacterSheet.AutoSize = true;
            this.lblCharacterSheet.Location = new System.Drawing.Point(226, 8);
            this.lblCharacterSheet.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCharacterSheet.Name = "lblCharacterSheet";
            this.lblCharacterSheet.Size = new System.Drawing.Size(87, 13);
            this.lblCharacterSheet.TabIndex = 3;
            this.lblCharacterSheet.Tag = "Label_Viewer_CharacterSheet";
            this.lblCharacterSheet.Text = "Character Sheet:";
            this.lblCharacterSheet.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // imgSheetLanguageFlag
            // 
            this.imgSheetLanguageFlag.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imgSheetLanguageFlag.Location = new System.Drawing.Point(319, 3);
            this.imgSheetLanguageFlag.Name = "imgSheetLanguageFlag";
            this.imgSheetLanguageFlag.Size = new System.Drawing.Size(16, 23);
            this.imgSheetLanguageFlag.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.imgSheetLanguageFlag.TabIndex = 105;
            this.imgSheetLanguageFlag.TabStop = false;
            // 
            // frmViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(800, 120);
            this.Name = "frmViewer";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_CharacterViewer";
            this.Text = "Character Viewer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CharacterSheetViewer_FormClosing);
            this.Load += new System.EventHandler(this.CharacterSheetViewer_Load);
            this.CursorChanged += new System.EventHandler(this.CharacterSheetViewer_CursorChanged);
            this.cmsSaveButton.ResumeLayout(false);
            this.cmsPrintButton.ResumeLayout(false);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgSheetLanguageFlag)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        internal SplitButton cmdPrint;
        private System.Windows.Forms.ContextMenuStrip cmsPrintButton;
        internal System.Windows.Forms.SaveFileDialog SaveFileDialog1;
        private ElasticComboBox cboXSLT;
        private System.Windows.Forms.Label lblCharacterSheet;
        private System.Windows.Forms.ContextMenuStrip cmsSaveButton;
        private SplitButton cmdSaveAsPdf;
        private System.Windows.Forms.WebBrowser webViewer;
        private ElasticComboBox cboLanguage;
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.PictureBox imgSheetLanguageFlag;
        private DpiFriendlyToolStripMenuItem tsSaveAsXml;
        internal DpiFriendlyToolStripMenuItem tsSaveAsHtml;
        private DpiFriendlyToolStripMenuItem tsPrintPreview;
    }

}
