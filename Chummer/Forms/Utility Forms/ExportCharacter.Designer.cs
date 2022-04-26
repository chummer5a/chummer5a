namespace Chummer
{
    partial class ExportCharacter
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
                _objCharacterXmlGeneratorCancellationTokenSource?.Dispose();
                _objXmlGeneratorCancellationTokenSource?.Dispose();
                _dicCache.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportCharacter));
            this.lblExport = new System.Windows.Forms.Label();
            this.cboXSLT = new Chummer.ElasticComboBox();
            this.cmdExportClose = new System.Windows.Forms.Button();
            this.dlgSaveFile = new System.Windows.Forms.SaveFileDialog();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cboLanguage = new Chummer.ElasticComboBox();
            this.imgSheetLanguageFlag = new System.Windows.Forms.PictureBox();
            this.tlpButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdExport = new System.Windows.Forms.Button();
            this.txtText = new System.Windows.Forms.TextBox();
            this.tlpMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgSheetLanguageFlag)).BeginInit();
            this.tlpButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblExport
            // 
            this.lblExport.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblExport.AutoSize = true;
            this.lblExport.Location = new System.Drawing.Point(3, 8);
            this.lblExport.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblExport.Name = "lblExport";
            this.lblExport.Size = new System.Drawing.Size(52, 13);
            this.lblExport.TabIndex = 0;
            this.lblExport.Tag = "Label_ExportTo";
            this.lblExport.Text = "Export to:";
            this.lblExport.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboXSLT
            // 
            this.cboXSLT.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboXSLT.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboXSLT.FormattingEnabled = true;
            this.cboXSLT.Location = new System.Drawing.Point(266, 4);
            this.cboXSLT.Name = "cboXSLT";
            this.cboXSLT.Size = new System.Drawing.Size(177, 21);
            this.cboXSLT.TabIndex = 1;
            this.cboXSLT.TooltipText = "";
            this.cboXSLT.SelectedIndexChanged += new System.EventHandler(this.cboXSLT_SelectedIndexChanged);
            // 
            // cmdExportClose
            // 
            this.cmdExportClose.AutoSize = true;
            this.cmdExportClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdExportClose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdExportClose.Enabled = false;
            this.cmdExportClose.Location = new System.Drawing.Point(106, 3);
            this.cmdExportClose.Name = "cmdExportClose";
            this.cmdExportClose.Size = new System.Drawing.Size(97, 23);
            this.cmdExportClose.TabIndex = 2;
            this.cmdExportClose.Tag = "String_Export_And_Close";
            this.cmdExportClose.Text = "Export and Close";
            this.cmdExportClose.UseVisualStyleBackColor = true;
            this.cmdExportClose.Click += new System.EventHandler(this.cmdExportClose_Click);
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 4;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.Controls.Add(this.cboLanguage, 2, 0);
            this.tlpMain.Controls.Add(this.imgSheetLanguageFlag, 1, 0);
            this.tlpMain.Controls.Add(this.lblExport, 0, 0);
            this.tlpMain.Controls.Add(this.tlpButtons, 0, 2);
            this.tlpMain.Controls.Add(this.txtText, 0, 1);
            this.tlpMain.Controls.Add(this.cboXSLT, 3, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 3;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Size = new System.Drawing.Size(446, 263);
            this.tlpMain.TabIndex = 5;
            // 
            // cboLanguage
            // 
            this.cboLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLanguage.FormattingEnabled = true;
            this.cboLanguage.Location = new System.Drawing.Point(83, 4);
            this.cboLanguage.Name = "cboLanguage";
            this.cboLanguage.Size = new System.Drawing.Size(177, 21);
            this.cboLanguage.TabIndex = 107;
            this.cboLanguage.TooltipText = "";
            this.cboLanguage.SelectedIndexChanged += new System.EventHandler(this.cboLanguage_SelectedIndexChanged);
            // 
            // imgSheetLanguageFlag
            // 
            this.imgSheetLanguageFlag.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imgSheetLanguageFlag.Location = new System.Drawing.Point(61, 3);
            this.imgSheetLanguageFlag.Name = "imgSheetLanguageFlag";
            this.imgSheetLanguageFlag.Size = new System.Drawing.Size(16, 23);
            this.imgSheetLanguageFlag.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.imgSheetLanguageFlag.TabIndex = 106;
            this.imgSheetLanguageFlag.TabStop = false;
            // 
            // tlpButtons
            // 
            this.tlpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpButtons.AutoSize = true;
            this.tlpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpButtons.ColumnCount = 2;
            this.tlpMain.SetColumnSpan(this.tlpButtons, 4);
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.Controls.Add(this.cmdExport, 0, 0);
            this.tlpButtons.Controls.Add(this.cmdExportClose, 1, 0);
            this.tlpButtons.Location = new System.Drawing.Point(240, 234);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButtons.Size = new System.Drawing.Size(206, 29);
            this.tlpButtons.TabIndex = 6;
            // 
            // cmdExport
            // 
            this.cmdExport.AutoSize = true;
            this.cmdExport.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdExport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdExport.Enabled = false;
            this.cmdExport.Location = new System.Drawing.Point(3, 3);
            this.cmdExport.Name = "cmdExport";
            this.cmdExport.Size = new System.Drawing.Size(97, 23);
            this.cmdExport.TabIndex = 4;
            this.cmdExport.Tag = "String_Export";
            this.cmdExport.Text = "Export";
            this.cmdExport.UseVisualStyleBackColor = true;
            this.cmdExport.Click += new System.EventHandler(this.cmdExport_Click);
            // 
            // txtText
            // 
            this.tlpMain.SetColumnSpan(this.txtText, 4);
            this.txtText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtText.Location = new System.Drawing.Point(3, 32);
            this.txtText.MaxLength = 2147483647;
            this.txtText.Multiline = true;
            this.txtText.Name = "txtText";
            this.txtText.ReadOnly = true;
            this.txtText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtText.Size = new System.Drawing.Size(440, 199);
            this.txtText.TabIndex = 7;
            this.txtText.Tag = "String_Generating_Data";
            this.txtText.Text = "Generating Data...";
            this.txtText.Leave += new System.EventHandler(this.txtText_Leave);
            this.txtText.MouseUp += new System.Windows.Forms.MouseEventHandler(this.txtText_MouseUp);
            // 
            // ExportCharacter
            // 
            this.AcceptButton = this.cmdExportClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(464, 281);
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExportCharacter";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_ExportCharacter";
            this.Text = "Export Character";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ExportCharacter_FormClosing);
            this.Load += new System.EventHandler(this.ExportCharacter_Load);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgSheetLanguageFlag)).EndInit();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblExport;
        private ElasticComboBox cboXSLT;
        private System.Windows.Forms.Button cmdExportClose;
        internal System.Windows.Forms.SaveFileDialog dlgSaveFile;
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private BufferedTableLayoutPanel tlpButtons;
        private System.Windows.Forms.TextBox txtText;
        private System.Windows.Forms.PictureBox imgSheetLanguageFlag;
        private ElasticComboBox cboLanguage;
        private System.Windows.Forms.Button cmdExport;
    }
}
