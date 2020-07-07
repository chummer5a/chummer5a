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
            if (disposing)
            {
                components?.Dispose();
                _workerRefresher?.Dispose();
                _workerOutputGenerator?.Dispose();
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
            this.tsSaveAsHtml = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsSaveButton = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsSaveAsXml = new System.Windows.Forms.ToolStripMenuItem();
            this.cmdSaveAsPdf = new SplitButton();
            this.cmsPrintButton = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsPrintPreview = new System.Windows.Forms.ToolStripMenuItem();
            this.cmdPrint = new SplitButton();
            this.SaveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.cboXSLT = new Chummer.ElasticComboBox();
            this.lblCharacterSheet = new System.Windows.Forms.Label();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.cboLanguage = new Chummer.ElasticComboBox();
            this.tableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.imgSheetLanguageFlag = new System.Windows.Forms.PictureBox();
            this.cmsSaveButton.SuspendLayout();
            this.cmsPrintButton.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgSheetLanguageFlag)).BeginInit();
            this.SuspendLayout();
            // 
            // tsSaveAsHtml
            // 
            this.tsSaveAsHtml.Enabled = false;
            this.tsSaveAsHtml.Name = "tsSaveAsHtml";
            this.tsSaveAsHtml.Size = new System.Drawing.Size(147, 22);
            this.tsSaveAsHtml.Tag = "Button_Viewer_SaveAsHtml";
            this.tsSaveAsHtml.Text = "Save as &HTML";
            this.tsSaveAsHtml.Click += new System.EventHandler(this.tsSaveAsHTML_Click);
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
            this.tsSaveAsXml.Name = "tsSaveAsXml";
            this.tsSaveAsXml.Size = new System.Drawing.Size(147, 22);
            this.tsSaveAsXml.Tag = "Button_Viewer_SaveAsXml";
            this.tsSaveAsXml.Text = "Save as XML";
            this.tsSaveAsXml.Click += new System.EventHandler(this.tsSaveAsXml_Click);
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
            this.tsPrintPreview.Name = "tsPrintPreview";
            this.tsPrintPreview.Size = new System.Drawing.Size(143, 22);
            this.tsPrintPreview.Tag = "Menu_FilePrintPreview";
            this.tsPrintPreview.Text = "&Print Preview";
            this.tsPrintPreview.Click += new System.EventHandler(this.tsPrintPreview_Click);
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
            // webBrowser1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.webBrowser1, 6);
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(3, 32);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.ScriptErrorsSuppressed = true;
            this.webBrowser1.Size = new System.Drawing.Size(760, 508);
            this.webBrowser1.TabIndex = 5;
            this.webBrowser1.WebBrowserShortcutsEnabled = false;
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
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 6;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.webBrowser1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.cmdPrint, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.cboXSLT, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.cboLanguage, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.cmdSaveAsPdf, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblCharacterSheet, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.imgSheetLanguageFlag, 3, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(9, 9);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(766, 543);
            this.tableLayoutPanel1.TabIndex = 105;
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
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(800, 120);
            this.Name = "frmViewer";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_CharacterViewer";
            this.Text = "Character Viewer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmViewer_FormClosing);
            this.Load += new System.EventHandler(this.frmViewer_Load);
            this.cmsSaveButton.ResumeLayout(false);
            this.cmsPrintButton.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgSheetLanguageFlag)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        internal SplitButton cmdPrint;
        private System.Windows.Forms.ContextMenuStrip cmsPrintButton;
        private System.Windows.Forms.ToolStripMenuItem tsPrintPreview;
        internal System.Windows.Forms.SaveFileDialog SaveFileDialog1;
        private ElasticComboBox cboXSLT;
        private System.Windows.Forms.Label lblCharacterSheet;
        private System.Windows.Forms.ContextMenuStrip cmsSaveButton;
        internal System.Windows.Forms.ToolStripMenuItem tsSaveAsHtml;
        private System.Windows.Forms.ToolStripMenuItem tsSaveAsXml;
        private SplitButton cmdSaveAsPdf;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private ElasticComboBox cboLanguage;
        private Chummer.BufferedTableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.PictureBox imgSheetLanguageFlag;
    }

}
