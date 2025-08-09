namespace Chummer
{
    public sealed partial class DataExporter
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataExporter));
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.lblLanguage = new System.Windows.Forms.Label();
            this.imgSheetLanguageFlag = new System.Windows.Forms.PictureBox();
            this.gpbCustomData = new System.Windows.Forms.GroupBox();
            this.pnlCustomData = new System.Windows.Forms.Panel();
            this.lblCustomData = new System.Windows.Forms.Label();
            this.tlpButtons = new System.Windows.Forms.TableLayoutPanel();
            this.cmdExport = new System.Windows.Forms.Button();
            this.cmdExportClose = new System.Windows.Forms.Button();
            this.cmdEditCharacterSetting = new System.Windows.Forms.Button();
            this.lblCharacterSetting = new System.Windows.Forms.Label();
            this.pgbExportProgress = new System.Windows.Forms.ProgressBar();
            this.cboCharacterSetting = new Chummer.ElasticComboBox();
            this.cboLanguage = new Chummer.ElasticComboBox();
            this.cboBuildMethod = new Chummer.ElasticComboBox();
            this.nudMaxAvail = new Chummer.NumericUpDownEx();
            this.cboGamePlay = new Chummer.ElasticComboBox();
            this.lblStartingKarma = new System.Windows.Forms.Label();
            this.dlgSaveFile = new System.Windows.Forms.SaveFileDialog();
            this.tlpMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgSheetLanguageFlag)).BeginInit();
            this.gpbCustomData.SuspendLayout();
            this.pnlCustomData.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxAvail)).BeginInit();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 5;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.lblLanguage, 0, 1);
            this.tlpMain.Controls.Add(this.imgSheetLanguageFlag, 1, 1);
            this.tlpMain.Controls.Add(this.gpbCustomData, 0, 2);
            this.tlpMain.Controls.Add(this.tlpButtons, 3, 3);
            this.tlpMain.Controls.Add(this.cmdEditCharacterSetting, 4, 0);
            this.tlpMain.Controls.Add(this.lblCharacterSetting, 0, 0);
            this.tlpMain.Controls.Add(this.pgbExportProgress, 0, 3);
            this.tlpMain.Controls.Add(this.cboCharacterSetting, 1, 0);
            this.tlpMain.Controls.Add(this.cboLanguage, 2, 1);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 4;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(606, 423);
            this.tlpMain.TabIndex = 16;
            // 
            // lblLanguage
            // 
            this.lblLanguage.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblLanguage.AutoSize = true;
            this.lblLanguage.Location = new System.Drawing.Point(10, 37);
            this.lblLanguage.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Size = new System.Drawing.Size(58, 13);
            this.lblLanguage.TabIndex = 108;
            this.lblLanguage.Tag = "Label_Options_Language";
            this.lblLanguage.Text = "Language:";
            // 
            // imgSheetLanguageFlag
            // 
            this.imgSheetLanguageFlag.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imgSheetLanguageFlag.Location = new System.Drawing.Point(74, 32);
            this.imgSheetLanguageFlag.Name = "imgSheetLanguageFlag";
            this.imgSheetLanguageFlag.Size = new System.Drawing.Size(16, 23);
            this.imgSheetLanguageFlag.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.imgSheetLanguageFlag.TabIndex = 106;
            this.imgSheetLanguageFlag.TabStop = false;
            // 
            // gpbCustomData
            // 
            this.gpbCustomData.AutoSize = true;
            this.gpbCustomData.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.SetColumnSpan(this.gpbCustomData, 5);
            this.gpbCustomData.Controls.Add(this.pnlCustomData);
            this.gpbCustomData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpbCustomData.Location = new System.Drawing.Point(3, 61);
            this.gpbCustomData.Name = "gpbCustomData";
            this.gpbCustomData.Size = new System.Drawing.Size(600, 330);
            this.gpbCustomData.TabIndex = 34;
            this.gpbCustomData.TabStop = false;
            this.gpbCustomData.Tag = "Label_SelectBP_CustomData";
            this.gpbCustomData.Text = "Custom Data";
            // 
            // pnlCustomData
            // 
            this.pnlCustomData.AutoScroll = true;
            this.pnlCustomData.Controls.Add(this.lblCustomData);
            this.pnlCustomData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCustomData.Location = new System.Drawing.Point(3, 16);
            this.pnlCustomData.Margin = new System.Windows.Forms.Padding(0);
            this.pnlCustomData.Name = "pnlCustomData";
            this.pnlCustomData.Padding = new System.Windows.Forms.Padding(3, 6, 13, 6);
            this.pnlCustomData.Size = new System.Drawing.Size(594, 311);
            this.pnlCustomData.TabIndex = 31;
            // 
            // lblCustomData
            // 
            this.lblCustomData.AutoSize = true;
            this.lblCustomData.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCustomData.Location = new System.Drawing.Point(3, 6);
            this.lblCustomData.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCustomData.Name = "lblCustomData";
            this.lblCustomData.Size = new System.Drawing.Size(110, 13);
            this.lblCustomData.TabIndex = 27;
            this.lblCustomData.Text = "[Custom Data Names]";
            // 
            // tlpButtons
            // 
            this.tlpButtons.AutoSize = true;
            this.tlpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpButtons.ColumnCount = 2;
            this.tlpMain.SetColumnSpan(this.tlpButtons, 2);
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.Controls.Add(this.cmdExport, 0, 0);
            this.tlpButtons.Controls.Add(this.cmdExportClose, 1, 0);
            this.tlpButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpButtons.Location = new System.Drawing.Point(400, 394);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButtons.Size = new System.Drawing.Size(206, 29);
            this.tlpButtons.TabIndex = 20;
            // 
            // cmdExport
            // 
            this.cmdExport.AutoSize = true;
            this.cmdExport.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdExport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdExport.Enabled = false;
            this.cmdExport.Location = new System.Drawing.Point(3, 3);
            this.cmdExport.MinimumSize = new System.Drawing.Size(80, 0);
            this.cmdExport.Name = "cmdExport";
            this.cmdExport.Size = new System.Drawing.Size(97, 23);
            this.cmdExport.TabIndex = 4;
            this.cmdExport.Tag = "String_Export";
            this.cmdExport.Text = "Export";
            this.cmdExport.UseVisualStyleBackColor = true;
            this.cmdExport.Click += new System.EventHandler(this.cmdExport_Click);
            // 
            // cmdExportClose
            // 
            this.cmdExportClose.AutoSize = true;
            this.cmdExportClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdExportClose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdExportClose.Enabled = false;
            this.cmdExportClose.Location = new System.Drawing.Point(106, 3);
            this.cmdExportClose.MinimumSize = new System.Drawing.Size(80, 0);
            this.cmdExportClose.Name = "cmdExportClose";
            this.cmdExportClose.Size = new System.Drawing.Size(97, 23);
            this.cmdExportClose.TabIndex = 2;
            this.cmdExportClose.Tag = "String_Export_And_Close";
            this.cmdExportClose.Text = "Export and Close";
            this.cmdExportClose.UseVisualStyleBackColor = true;
            // 
            // cmdEditCharacterSetting
            // 
            this.cmdEditCharacterSetting.AutoSize = true;
            this.cmdEditCharacterSetting.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdEditCharacterSetting.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdEditCharacterSetting.Location = new System.Drawing.Point(523, 3);
            this.cmdEditCharacterSetting.MinimumSize = new System.Drawing.Size(80, 0);
            this.cmdEditCharacterSetting.Name = "cmdEditCharacterSetting";
            this.cmdEditCharacterSetting.Size = new System.Drawing.Size(80, 23);
            this.cmdEditCharacterSetting.TabIndex = 17;
            this.cmdEditCharacterSetting.Tag = "String_ModifyEllipses";
            this.cmdEditCharacterSetting.Text = "Modify...";
            this.cmdEditCharacterSetting.UseVisualStyleBackColor = true;
            this.cmdEditCharacterSetting.Click += new System.EventHandler(this.cmdEditCharacterOption_Click);
            // 
            // lblCharacterSetting
            // 
            this.lblCharacterSetting.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCharacterSetting.AutoSize = true;
            this.lblCharacterSetting.Location = new System.Drawing.Point(3, 8);
            this.lblCharacterSetting.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCharacterSetting.Name = "lblCharacterSetting";
            this.lblCharacterSetting.Size = new System.Drawing.Size(65, 13);
            this.lblCharacterSetting.TabIndex = 18;
            this.lblCharacterSetting.Tag = "Label_SelectBP_UseSetting";
            this.lblCharacterSetting.Text = "Use Setting:";
            // 
            // pgbExportProgress
            // 
            this.tlpMain.SetColumnSpan(this.pgbExportProgress, 3);
            this.pgbExportProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgbExportProgress.Location = new System.Drawing.Point(3, 397);
            this.pgbExportProgress.Name = "pgbExportProgress";
            this.pgbExportProgress.Size = new System.Drawing.Size(394, 23);
            this.pgbExportProgress.TabIndex = 35;
            // 
            // cboCharacterSetting
            // 
            this.cboCharacterSetting.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMain.SetColumnSpan(this.cboCharacterSetting, 3);
            this.cboCharacterSetting.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCharacterSetting.FormattingEnabled = true;
            this.cboCharacterSetting.Location = new System.Drawing.Point(74, 4);
            this.cboCharacterSetting.Name = "cboCharacterSetting";
            this.cboCharacterSetting.Size = new System.Drawing.Size(443, 21);
            this.cboCharacterSetting.TabIndex = 8;
            this.cboCharacterSetting.TooltipText = "";
            this.cboCharacterSetting.SelectedIndexChanged += new System.EventHandler(this.cboCharacterSetting_SelectedIndexChanged);
            // 
            // cboLanguage
            // 
            this.cboLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMain.SetColumnSpan(this.cboLanguage, 3);
            this.cboLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLanguage.FormattingEnabled = true;
            this.cboLanguage.Location = new System.Drawing.Point(96, 33);
            this.cboLanguage.Name = "cboLanguage";
            this.cboLanguage.Size = new System.Drawing.Size(507, 21);
            this.cboLanguage.TabIndex = 107;
            this.cboLanguage.TooltipText = "";
            this.cboLanguage.SelectedIndexChanged += new System.EventHandler(this.cboLanguage_SelectedIndexChanged);
            // 
            // cboBuildMethod
            // 
            this.cboBuildMethod.Location = new System.Drawing.Point(0, 0);
            this.cboBuildMethod.Name = "cboBuildMethod";
            this.cboBuildMethod.Size = new System.Drawing.Size(121, 21);
            this.cboBuildMethod.TabIndex = 0;
            this.cboBuildMethod.TooltipText = "";
            // 
            // nudMaxAvail
            // 
            this.nudMaxAvail.Location = new System.Drawing.Point(0, 0);
            this.nudMaxAvail.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudMaxAvail.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMaxAvail.Name = "nudMaxAvail";
            this.nudMaxAvail.Size = new System.Drawing.Size(120, 20);
            this.nudMaxAvail.TabIndex = 0;
            // 
            // cboGamePlay
            // 
            this.cboGamePlay.Location = new System.Drawing.Point(0, 0);
            this.cboGamePlay.Name = "cboGamePlay";
            this.cboGamePlay.Size = new System.Drawing.Size(121, 21);
            this.cboGamePlay.TabIndex = 0;
            this.cboGamePlay.TooltipText = "";
            // 
            // lblStartingKarma
            // 
            this.lblStartingKarma.Location = new System.Drawing.Point(0, 0);
            this.lblStartingKarma.Name = "lblStartingKarma";
            this.lblStartingKarma.Size = new System.Drawing.Size(100, 23);
            this.lblStartingKarma.TabIndex = 0;
            // 
            // dlgSaveFile
            // 
            this.dlgSaveFile.DefaultExt = "zip";
            // 
            // DataExporter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.tlpMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DataExporter";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_ExportData";
            this.Text = "Export Data Files to Archive";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DataExporter_Closing);
            this.Load += new System.EventHandler(this.DataExporter_Load);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgSheetLanguageFlag)).EndInit();
            this.gpbCustomData.ResumeLayout(false);
            this.pnlCustomData.ResumeLayout(false);
            this.pnlCustomData.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxAvail)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.Button cmdEditCharacterSetting;
        private System.Windows.Forms.Label lblCharacterSetting;
        private ElasticComboBox cboBuildMethod;
        private NumericUpDownEx nudMaxAvail;
        private ElasticComboBox cboGamePlay;
        private System.Windows.Forms.Label lblStartingKarma;
        private System.Windows.Forms.TableLayoutPanel tlpButtons;
        private System.Windows.Forms.Button cmdExport;
        private System.Windows.Forms.Button cmdExportClose;
        private System.Windows.Forms.GroupBox gpbCustomData;
        private System.Windows.Forms.Panel pnlCustomData;
        private System.Windows.Forms.Label lblCustomData;
        private ElasticComboBox cboCharacterSetting;
        internal System.Windows.Forms.SaveFileDialog dlgSaveFile;
        private System.Windows.Forms.ProgressBar pgbExportProgress;
        private System.Windows.Forms.PictureBox imgSheetLanguageFlag;
        private ElasticComboBox cboLanguage;
        private System.Windows.Forms.Label lblLanguage;
    }
}
