namespace Chummer
{
    partial class frmOptions
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
            if(disposing)
            {
                components?.Dispose();
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
            this.tlpOptions = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tabOptions = new System.Windows.Forms.TabControl();
            this.tabGlobal = new System.Windows.Forms.TabPage();
            this.tlpGlobal = new Chummer.BufferedTableLayoutPanel(this.components);
            this.grpSelectedSourcebook = new System.Windows.Forms.GroupBox();
            this.tlpSelectedSourcebook = new Chummer.BufferedTableLayoutPanel(this.components);
            this.txtPDFLocation = new System.Windows.Forms.TextBox();
            this.lblPDFLocation = new System.Windows.Forms.Label();
            this.cmdPDFLocation = new System.Windows.Forms.Button();
            this.lblPDFOffset = new System.Windows.Forms.Label();
            this.flpPDFOffset = new System.Windows.Forms.FlowLayoutPanel();
            this.nudPDFOffset = new System.Windows.Forms.NumericUpDown();
            this.cmdPDFTest = new System.Windows.Forms.Button();
            this.tlpGlobalOptions = new System.Windows.Forms.TableLayoutPanel();
            this.chkAllowEasterEggs = new System.Windows.Forms.CheckBox();
            this.chkDatesIncludeTime = new System.Windows.Forms.CheckBox();
            this.chkLiveCustomData = new System.Windows.Forms.CheckBox();
            this.chkSingleDiceRoller = new System.Windows.Forms.CheckBox();
            this.chkStartupFullscreen = new System.Windows.Forms.CheckBox();
            this.chkLifeModule = new System.Windows.Forms.CheckBox();
            this.chkAutomaticUpdate = new System.Windows.Forms.CheckBox();
            this.chkUseLogging = new System.Windows.Forms.CheckBox();
            this.chkConfirmKarmaExpense = new System.Windows.Forms.CheckBox();
            this.chkConfirmDelete = new System.Windows.Forms.CheckBox();
            this.chkHideItemsOverAvail = new System.Windows.Forms.CheckBox();
            this.flpEnablePlugins = new System.Windows.Forms.FlowLayoutPanel();
            this.chkEnablePlugins = new System.Windows.Forms.CheckBox();
            this.cmdPluginsHelp = new System.Windows.Forms.Button();
            this.chkAllowHoverIncrement = new System.Windows.Forms.CheckBox();
            this.chkAllowSkillDiceRolling = new System.Windows.Forms.CheckBox();
            this.chkSearchInCategoryOnly = new System.Windows.Forms.CheckBox();
            this.chkCreateBackupOnCareer = new System.Windows.Forms.CheckBox();
            this.tlpGlobalOptionsTop = new Chummer.BufferedTableLayoutPanel(this.components);
            this.grpDateFormat = new System.Windows.Forms.GroupBox();
            this.tlpDateFormat = new Chummer.BufferedTableLayoutPanel(this.components);
            this.txtDateFormat = new System.Windows.Forms.TextBox();
            this.txtDateFormatView = new System.Windows.Forms.TextBox();
            this.grpTimeFormat = new System.Windows.Forms.GroupBox();
            this.tlpTimeFormat = new Chummer.BufferedTableLayoutPanel(this.components);
            this.txtTimeFormat = new System.Windows.Forms.TextBox();
            this.txtTimeFormatView = new System.Windows.Forms.TextBox();
            this.chkCustomDateTimeFormats = new System.Windows.Forms.CheckBox();
            this.lblXSLT = new System.Windows.Forms.Label();
            this.lblLanguage = new System.Windows.Forms.Label();
            this.cboSheetLanguage = new Chummer.ElasticComboBox();
            this.cboXSLT = new Chummer.ElasticComboBox();
            this.cboLanguage = new Chummer.ElasticComboBox();
            this.cmdVerify = new System.Windows.Forms.Button();
            this.cmdVerifyData = new System.Windows.Forms.Button();
            this.imgSheetLanguageFlag = new System.Windows.Forms.PictureBox();
            this.imgLanguageFlag = new System.Windows.Forms.PictureBox();
            this.tlpLoggingOptions = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cboUseLoggingApplicationInsights = new System.Windows.Forms.ComboBox();
            this.cmdUseLoggingHelp = new Chummer.ButtonWithToolTip();
            this.lblDefaultCharacterOption = new System.Windows.Forms.Label();
            this.cboDefaultCharacterOption = new System.Windows.Forms.ComboBox();
            this.lblCharacterRosterLabel = new System.Windows.Forms.Label();
            this.tlpCharacterRosterPath = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdCharacterRoster = new System.Windows.Forms.Button();
            this.txtCharacterRosterPath = new System.Windows.Forms.TextBox();
            this.lblPDFParametersLabel = new System.Windows.Forms.Label();
            this.cboPDFParameters = new Chummer.ElasticComboBox();
            this.lblPDFAppPath = new System.Windows.Forms.Label();
            this.tlpPDFAppPath = new Chummer.BufferedTableLayoutPanel(this.components);
            this.txtPDFAppPath = new System.Windows.Forms.TextBox();
            this.cmdPDFAppPath = new System.Windows.Forms.Button();
            this.flpBrowserVersion = new System.Windows.Forms.FlowLayoutPanel();
            this.lblBrowserVersion = new System.Windows.Forms.Label();
            this.nudBrowserVersion = new System.Windows.Forms.NumericUpDown();
            this.chkHideCharts = new System.Windows.Forms.CheckBox();
            this.chkLiveUpdateCleanCharacterFiles = new System.Windows.Forms.CheckBox();
            this.chkPreferNightlyBuilds = new System.Windows.Forms.CheckBox();
            this.chkHideCharacterRoster = new System.Windows.Forms.CheckBox();
            this.chkPrintToFileFirst = new System.Windows.Forms.CheckBox();
            this.lblMugshotCompression = new System.Windows.Forms.Label();
            this.tlpMugshotCompression = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblMugshotCompressionQuality = new System.Windows.Forms.Label();
            this.nudMugshotCompressionQuality = new System.Windows.Forms.NumericUpDown();
            this.cboMugshotCompression = new Chummer.ElasticComboBox();
            this.chkHideMasterIndex = new System.Windows.Forms.CheckBox();
            this.gpbEditSourcebookInfo = new System.Windows.Forms.GroupBox();
            this.lstGlobalSourcebookInfos = new System.Windows.Forms.ListBox();
            this.tabCustomDataDirectories = new System.Windows.Forms.TabPage();
            this.tlpOptionalRules = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblCustomDataDirectoriesLabel = new System.Windows.Forms.Label();
            this.cmdAddCustomDirectory = new System.Windows.Forms.Button();
            this.treCustomDataDirectories = new System.Windows.Forms.TreeView();
            this.cmdRenameCustomDataDirectory = new System.Windows.Forms.Button();
            this.cmdRemoveCustomDirectory = new System.Windows.Forms.Button();
            this.tabGitHubIssues = new System.Windows.Forms.TabPage();
            this.cmdUploadPastebin = new System.Windows.Forms.Button();
            this.tabPlugins = new System.Windows.Forms.TabPage();
            this.bufferedTableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.grpAvailablePlugins = new System.Windows.Forms.GroupBox();
            this.clbPlugins = new System.Windows.Forms.CheckedListBox();
            this.pnlPluginOption = new System.Windows.Forms.Panel();
            this.flpOKCancel = new System.Windows.Forms.FlowLayoutPanel();
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.tlpOptions.SuspendLayout();
            this.tabOptions.SuspendLayout();
            this.tabGlobal.SuspendLayout();
            this.tlpGlobal.SuspendLayout();
            this.grpSelectedSourcebook.SuspendLayout();
            this.tlpSelectedSourcebook.SuspendLayout();
            this.flpPDFOffset.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPDFOffset)).BeginInit();
            this.tlpGlobalOptions.SuspendLayout();
            this.flpEnablePlugins.SuspendLayout();
            this.tlpGlobalOptionsTop.SuspendLayout();
            this.grpDateFormat.SuspendLayout();
            this.tlpDateFormat.SuspendLayout();
            this.grpTimeFormat.SuspendLayout();
            this.tlpTimeFormat.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgSheetLanguageFlag)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgLanguageFlag)).BeginInit();
            this.tlpLoggingOptions.SuspendLayout();
            this.tlpCharacterRosterPath.SuspendLayout();
            this.tlpPDFAppPath.SuspendLayout();
            this.flpBrowserVersion.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudBrowserVersion)).BeginInit();
            this.tlpMugshotCompression.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMugshotCompressionQuality)).BeginInit();
            this.gpbEditSourcebookInfo.SuspendLayout();
            this.tabCustomDataDirectories.SuspendLayout();
            this.tlpOptionalRules.SuspendLayout();
            this.tabGitHubIssues.SuspendLayout();
            this.tabPlugins.SuspendLayout();
            this.bufferedTableLayoutPanel1.SuspendLayout();
            this.grpAvailablePlugins.SuspendLayout();
            this.flpOKCancel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpOptions
            // 
            this.tlpOptions.AutoSize = true;
            this.tlpOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpOptions.ColumnCount = 1;
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpOptions.Controls.Add(this.tabOptions, 0, 0);
            this.tlpOptions.Controls.Add(this.flpOKCancel, 0, 1);
            this.tlpOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpOptions.Location = new System.Drawing.Point(9, 9);
            this.tlpOptions.Name = "tlpOptions";
            this.tlpOptions.RowCount = 2;
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.Size = new System.Drawing.Size(1246, 663);
            this.tlpOptions.TabIndex = 7;
            // 
            // tabOptions
            // 
            this.tabOptions.Controls.Add(this.tabGlobal);
            this.tabOptions.Controls.Add(this.tabCustomDataDirectories);
            this.tabOptions.Controls.Add(this.tabGitHubIssues);
            this.tabOptions.Controls.Add(this.tabPlugins);
            this.tabOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabOptions.Location = new System.Drawing.Point(3, 3);
            this.tabOptions.Name = "tabOptions";
            this.tabOptions.SelectedIndex = 0;
            this.tabOptions.Size = new System.Drawing.Size(1240, 628);
            this.tabOptions.TabIndex = 4;
            // 
            // tabGlobal
            // 
            this.tabGlobal.BackColor = System.Drawing.SystemColors.Control;
            this.tabGlobal.Controls.Add(this.tlpGlobal);
            this.tabGlobal.Location = new System.Drawing.Point(4, 22);
            this.tabGlobal.Name = "tabGlobal";
            this.tabGlobal.Padding = new System.Windows.Forms.Padding(9);
            this.tabGlobal.Size = new System.Drawing.Size(1232, 602);
            this.tabGlobal.TabIndex = 5;
            this.tabGlobal.Tag = "Tab_Options_Global";
            this.tabGlobal.Text = "Global Options";
            // 
            // tlpGlobal
            // 
            this.tlpGlobal.AutoSize = true;
            this.tlpGlobal.ColumnCount = 2;
            this.tlpGlobal.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 301F));
            this.tlpGlobal.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGlobal.Controls.Add(this.grpSelectedSourcebook, 1, 1);
            this.tlpGlobal.Controls.Add(this.tlpGlobalOptions, 1, 0);
            this.tlpGlobal.Controls.Add(this.gpbEditSourcebookInfo, 0, 0);
            this.tlpGlobal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpGlobal.Location = new System.Drawing.Point(9, 9);
            this.tlpGlobal.Name = "tlpGlobal";
            this.tlpGlobal.RowCount = 2;
            this.tlpGlobal.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGlobal.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobal.Size = new System.Drawing.Size(1214, 584);
            this.tlpGlobal.TabIndex = 39;
            // 
            // grpSelectedSourcebook
            // 
            this.grpSelectedSourcebook.AutoSize = true;
            this.grpSelectedSourcebook.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.grpSelectedSourcebook.Controls.Add(this.tlpSelectedSourcebook);
            this.grpSelectedSourcebook.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpSelectedSourcebook.Enabled = false;
            this.grpSelectedSourcebook.Location = new System.Drawing.Point(304, 504);
            this.grpSelectedSourcebook.Name = "grpSelectedSourcebook";
            this.grpSelectedSourcebook.Size = new System.Drawing.Size(907, 77);
            this.grpSelectedSourcebook.TabIndex = 27;
            this.grpSelectedSourcebook.TabStop = false;
            this.grpSelectedSourcebook.Tag = "Label_Options_SelectedSourcebook";
            this.grpSelectedSourcebook.Text = "Selected Sourcebook:";
            // 
            // tlpSelectedSourcebook
            // 
            this.tlpSelectedSourcebook.AutoSize = true;
            this.tlpSelectedSourcebook.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpSelectedSourcebook.ColumnCount = 3;
            this.tlpSelectedSourcebook.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpSelectedSourcebook.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSelectedSourcebook.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpSelectedSourcebook.Controls.Add(this.txtPDFLocation, 1, 0);
            this.tlpSelectedSourcebook.Controls.Add(this.lblPDFLocation, 0, 0);
            this.tlpSelectedSourcebook.Controls.Add(this.cmdPDFLocation, 2, 0);
            this.tlpSelectedSourcebook.Controls.Add(this.lblPDFOffset, 0, 1);
            this.tlpSelectedSourcebook.Controls.Add(this.flpPDFOffset, 1, 1);
            this.tlpSelectedSourcebook.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSelectedSourcebook.Location = new System.Drawing.Point(3, 16);
            this.tlpSelectedSourcebook.Name = "tlpSelectedSourcebook";
            this.tlpSelectedSourcebook.RowCount = 2;
            this.tlpSelectedSourcebook.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSelectedSourcebook.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSelectedSourcebook.Size = new System.Drawing.Size(901, 58);
            this.tlpSelectedSourcebook.TabIndex = 18;
            // 
            // txtPDFLocation
            // 
            this.txtPDFLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPDFLocation.Location = new System.Drawing.Point(84, 4);
            this.txtPDFLocation.Name = "txtPDFLocation";
            this.txtPDFLocation.ReadOnly = true;
            this.txtPDFLocation.Size = new System.Drawing.Size(782, 20);
            this.txtPDFLocation.TabIndex = 13;
            this.txtPDFLocation.TextChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblPDFLocation
            // 
            this.lblPDFLocation.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblPDFLocation.AutoSize = true;
            this.lblPDFLocation.Location = new System.Drawing.Point(3, 8);
            this.lblPDFLocation.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPDFLocation.Name = "lblPDFLocation";
            this.lblPDFLocation.Size = new System.Drawing.Size(75, 13);
            this.lblPDFLocation.TabIndex = 12;
            this.lblPDFLocation.Tag = "Label_Options_PDFLocation";
            this.lblPDFLocation.Text = "PDF Location:";
            this.lblPDFLocation.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmdPDFLocation
            // 
            this.cmdPDFLocation.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmdPDFLocation.AutoSize = true;
            this.cmdPDFLocation.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdPDFLocation.Location = new System.Drawing.Point(872, 3);
            this.cmdPDFLocation.Name = "cmdPDFLocation";
            this.cmdPDFLocation.Size = new System.Drawing.Size(26, 23);
            this.cmdPDFLocation.TabIndex = 14;
            this.cmdPDFLocation.Text = "...";
            this.cmdPDFLocation.UseVisualStyleBackColor = true;
            this.cmdPDFLocation.Click += new System.EventHandler(this.cmdPDFLocation_Click);
            // 
            // lblPDFOffset
            // 
            this.lblPDFOffset.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblPDFOffset.AutoSize = true;
            this.lblPDFOffset.Location = new System.Drawing.Point(12, 37);
            this.lblPDFOffset.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPDFOffset.Name = "lblPDFOffset";
            this.lblPDFOffset.Size = new System.Drawing.Size(66, 13);
            this.lblPDFOffset.TabIndex = 15;
            this.lblPDFOffset.Tag = "Label_Options_PDFOffset";
            this.lblPDFOffset.Text = "Page Offset:";
            this.lblPDFOffset.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // flpPDFOffset
            // 
            this.flpPDFOffset.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpPDFOffset.AutoSize = true;
            this.flpPDFOffset.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpSelectedSourcebook.SetColumnSpan(this.flpPDFOffset, 2);
            this.flpPDFOffset.Controls.Add(this.nudPDFOffset);
            this.flpPDFOffset.Controls.Add(this.cmdPDFTest);
            this.flpPDFOffset.Location = new System.Drawing.Point(81, 29);
            this.flpPDFOffset.Margin = new System.Windows.Forms.Padding(0);
            this.flpPDFOffset.Name = "flpPDFOffset";
            this.flpPDFOffset.Size = new System.Drawing.Size(175, 29);
            this.flpPDFOffset.TabIndex = 16;
            // 
            // nudPDFOffset
            // 
            this.nudPDFOffset.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudPDFOffset.AutoSize = true;
            this.nudPDFOffset.Location = new System.Drawing.Point(3, 4);
            this.nudPDFOffset.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudPDFOffset.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.nudPDFOffset.Name = "nudPDFOffset";
            this.nudPDFOffset.Size = new System.Drawing.Size(41, 20);
            this.nudPDFOffset.TabIndex = 16;
            this.nudPDFOffset.ValueChanged += new System.EventHandler(this.nudPDFOffset_ValueChanged);
            // 
            // cmdPDFTest
            // 
            this.cmdPDFTest.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmdPDFTest.AutoSize = true;
            this.cmdPDFTest.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdPDFTest.Location = new System.Drawing.Point(50, 3);
            this.cmdPDFTest.Name = "cmdPDFTest";
            this.cmdPDFTest.Size = new System.Drawing.Size(122, 23);
            this.cmdPDFTest.TabIndex = 17;
            this.cmdPDFTest.Tag = "Button_Options_PDFTest";
            this.cmdPDFTest.Text = "Test - Open to Page 5";
            this.cmdPDFTest.UseVisualStyleBackColor = true;
            this.cmdPDFTest.Click += new System.EventHandler(this.cmdPDFTest_Click);
            // 
            // tlpGlobalOptions
            // 
            this.tlpGlobalOptions.AutoScroll = true;
            this.tlpGlobalOptions.AutoSize = true;
            this.tlpGlobalOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpGlobalOptions.ColumnCount = 4;
            this.tlpGlobalOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpGlobalOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tlpGlobalOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpGlobalOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tlpGlobalOptions.Controls.Add(this.chkAllowEasterEggs, 0, 4);
            this.tlpGlobalOptions.Controls.Add(this.chkDatesIncludeTime, 0, 8);
            this.tlpGlobalOptions.Controls.Add(this.chkLiveCustomData, 0, 5);
            this.tlpGlobalOptions.Controls.Add(this.chkSingleDiceRoller, 0, 7);
            this.tlpGlobalOptions.Controls.Add(this.chkStartupFullscreen, 0, 6);
            this.tlpGlobalOptions.Controls.Add(this.chkLifeModule, 0, 1);
            this.tlpGlobalOptions.Controls.Add(this.chkAutomaticUpdate, 0, 3);
            this.tlpGlobalOptions.Controls.Add(this.chkUseLogging, 0, 2);
            this.tlpGlobalOptions.Controls.Add(this.chkConfirmKarmaExpense, 2, 2);
            this.tlpGlobalOptions.Controls.Add(this.chkConfirmDelete, 2, 1);
            this.tlpGlobalOptions.Controls.Add(this.chkHideItemsOverAvail, 2, 3);
            this.tlpGlobalOptions.Controls.Add(this.flpEnablePlugins, 2, 4);
            this.tlpGlobalOptions.Controls.Add(this.chkAllowHoverIncrement, 2, 5);
            this.tlpGlobalOptions.Controls.Add(this.chkAllowSkillDiceRolling, 2, 6);
            this.tlpGlobalOptions.Controls.Add(this.chkSearchInCategoryOnly, 2, 7);
            this.tlpGlobalOptions.Controls.Add(this.tlpGlobalOptionsTop, 0, 0);
            this.tlpGlobalOptions.Controls.Add(this.tlpLoggingOptions, 1, 2);
            this.tlpGlobalOptions.Controls.Add(this.lblDefaultCharacterOption, 2, 10);
            this.tlpGlobalOptions.Controls.Add(this.cboDefaultCharacterOption, 3, 10);
            this.tlpGlobalOptions.Controls.Add(this.lblCharacterRosterLabel, 2, 11);
            this.tlpGlobalOptions.Controls.Add(this.tlpCharacterRosterPath, 3, 11);
            this.tlpGlobalOptions.Controls.Add(this.lblPDFParametersLabel, 2, 13);
            this.tlpGlobalOptions.Controls.Add(this.cboPDFParameters, 3, 13);
            this.tlpGlobalOptions.Controls.Add(this.lblPDFAppPath, 2, 14);
            this.tlpGlobalOptions.Controls.Add(this.tlpPDFAppPath, 3, 14);
            this.tlpGlobalOptions.Controls.Add(this.chkHideCharts, 0, 9);
            this.tlpGlobalOptions.Controls.Add(this.chkLiveUpdateCleanCharacterFiles, 0, 10);
            this.tlpGlobalOptions.Controls.Add(this.chkPreferNightlyBuilds, 0, 11);
            this.tlpGlobalOptions.Controls.Add(this.chkPrintToFileFirst, 0, 13);
            this.tlpGlobalOptions.Controls.Add(this.lblMugshotCompression, 0, 14);
            this.tlpGlobalOptions.Controls.Add(this.tlpMugshotCompression, 1, 14);
            this.tlpGlobalOptions.Controls.Add(this.flpBrowserVersion, 2, 12);
            this.tlpGlobalOptions.Controls.Add(this.chkCreateBackupOnCareer, 0, 12);
            this.tlpGlobalOptions.Controls.Add(this.chkHideMasterIndex, 2, 8);
            this.tlpGlobalOptions.Controls.Add(this.chkHideCharacterRoster, 2, 9);
            this.tlpGlobalOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpGlobalOptions.Location = new System.Drawing.Point(301, 0);
            this.tlpGlobalOptions.Margin = new System.Windows.Forms.Padding(0);
            this.tlpGlobalOptions.Name = "tlpGlobalOptions";
            this.tlpGlobalOptions.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.tlpGlobalOptions.RowCount = 16;
            this.tlpGlobalOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobalOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobalOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobalOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobalOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobalOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobalOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobalOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobalOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobalOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobalOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobalOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobalOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobalOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobalOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobalOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1F));
            this.tlpGlobalOptions.Size = new System.Drawing.Size(913, 501);
            this.tlpGlobalOptions.TabIndex = 67;
            // 
            // chkAllowEasterEggs
            // 
            this.chkAllowEasterEggs.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkAllowEasterEggs.AutoSize = true;
            this.tlpGlobalOptions.SetColumnSpan(this.chkAllowEasterEggs, 2);
            this.chkAllowEasterEggs.Location = new System.Drawing.Point(3, 216);
            this.chkAllowEasterEggs.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkAllowEasterEggs.Name = "chkAllowEasterEggs";
            this.chkAllowEasterEggs.Size = new System.Drawing.Size(111, 17);
            this.chkAllowEasterEggs.TabIndex = 52;
            this.chkAllowEasterEggs.Tag = "Checkbox_Options_AllowEasterEggs";
            this.chkAllowEasterEggs.Text = "Allow Easter Eggs";
            this.chkAllowEasterEggs.UseVisualStyleBackColor = true;
            this.chkAllowEasterEggs.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkDatesIncludeTime
            // 
            this.chkDatesIncludeTime.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkDatesIncludeTime.AutoSize = true;
            this.tlpGlobalOptions.SetColumnSpan(this.chkDatesIncludeTime, 2);
            this.chkDatesIncludeTime.Location = new System.Drawing.Point(3, 318);
            this.chkDatesIncludeTime.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkDatesIncludeTime.Name = "chkDatesIncludeTime";
            this.chkDatesIncludeTime.Size = new System.Drawing.Size(189, 17);
            this.chkDatesIncludeTime.TabIndex = 9;
            this.chkDatesIncludeTime.Tag = "Checkbox_Options_DatesIncludeTime";
            this.chkDatesIncludeTime.Text = "Expense dates should include time";
            this.chkDatesIncludeTime.UseVisualStyleBackColor = true;
            this.chkDatesIncludeTime.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkLiveCustomData
            // 
            this.chkLiveCustomData.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkLiveCustomData.AutoSize = true;
            this.tlpGlobalOptions.SetColumnSpan(this.chkLiveCustomData, 2);
            this.chkLiveCustomData.Location = new System.Drawing.Point(3, 243);
            this.chkLiveCustomData.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkLiveCustomData.Name = "chkLiveCustomData";
            this.chkLiveCustomData.Size = new System.Drawing.Size(307, 17);
            this.chkLiveCustomData.TabIndex = 28;
            this.chkLiveCustomData.Tag = "Checkbox_Options_Live_CustomData";
            this.chkLiveCustomData.Text = "Allow Live Custom Data Updates from customdata Directory";
            this.chkLiveCustomData.UseVisualStyleBackColor = true;
            this.chkLiveCustomData.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkSingleDiceRoller
            // 
            this.chkSingleDiceRoller.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkSingleDiceRoller.AutoSize = true;
            this.tlpGlobalOptions.SetColumnSpan(this.chkSingleDiceRoller, 2);
            this.chkSingleDiceRoller.Location = new System.Drawing.Point(3, 293);
            this.chkSingleDiceRoller.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkSingleDiceRoller.Name = "chkSingleDiceRoller";
            this.chkSingleDiceRoller.Size = new System.Drawing.Size(251, 17);
            this.chkSingleDiceRoller.TabIndex = 8;
            this.chkSingleDiceRoller.Tag = "Checkbox_Options_SingleDiceRoller";
            this.chkSingleDiceRoller.Text = "Use a single instance of the Dice Roller window";
            this.chkSingleDiceRoller.UseVisualStyleBackColor = true;
            this.chkSingleDiceRoller.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkStartupFullscreen
            // 
            this.chkStartupFullscreen.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkStartupFullscreen.AutoSize = true;
            this.tlpGlobalOptions.SetColumnSpan(this.chkStartupFullscreen, 2);
            this.chkStartupFullscreen.Location = new System.Drawing.Point(3, 268);
            this.chkStartupFullscreen.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkStartupFullscreen.Name = "chkStartupFullscreen";
            this.chkStartupFullscreen.Size = new System.Drawing.Size(154, 17);
            this.chkStartupFullscreen.TabIndex = 7;
            this.chkStartupFullscreen.Tag = "Checkbox_Options_StartupFullscreen";
            this.chkStartupFullscreen.Text = "Start Chummer in fullscreen";
            this.chkStartupFullscreen.UseVisualStyleBackColor = true;
            this.chkStartupFullscreen.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkLifeModule
            // 
            this.chkLifeModule.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkLifeModule.AutoSize = true;
            this.tlpGlobalOptions.SetColumnSpan(this.chkLifeModule, 2);
            this.chkLifeModule.Location = new System.Drawing.Point(3, 135);
            this.chkLifeModule.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkLifeModule.Name = "chkLifeModule";
            this.chkLifeModule.Size = new System.Drawing.Size(117, 17);
            this.chkLifeModule.TabIndex = 22;
            this.chkLifeModule.Tag = "Checkbox_Options_UseLifeModule";
            this.chkLifeModule.Text = "Life modules visible";
            this.chkLifeModule.UseVisualStyleBackColor = true;
            this.chkLifeModule.CheckedChanged += new System.EventHandler(this.chkLifeModules_CheckedChanged);
            // 
            // chkAutomaticUpdate
            // 
            this.chkAutomaticUpdate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkAutomaticUpdate.AutoSize = true;
            this.tlpGlobalOptions.SetColumnSpan(this.chkAutomaticUpdate, 2);
            this.chkAutomaticUpdate.Location = new System.Drawing.Point(3, 189);
            this.chkAutomaticUpdate.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkAutomaticUpdate.Name = "chkAutomaticUpdate";
            this.chkAutomaticUpdate.Size = new System.Drawing.Size(116, 17);
            this.chkAutomaticUpdate.TabIndex = 5;
            this.chkAutomaticUpdate.Tag = "Checkbox_Options_AutomaticUpdates";
            this.chkAutomaticUpdate.Text = "Automatic Updates";
            this.chkAutomaticUpdate.UseVisualStyleBackColor = true;
            this.chkAutomaticUpdate.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkUseLogging
            // 
            this.chkUseLogging.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkUseLogging.AutoSize = true;
            this.chkUseLogging.Location = new System.Drawing.Point(3, 162);
            this.chkUseLogging.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkUseLogging.Name = "chkUseLogging";
            this.chkUseLogging.Size = new System.Drawing.Size(86, 17);
            this.chkUseLogging.TabIndex = 4;
            this.chkUseLogging.Tag = "Checkbox_Options_UseLogging";
            this.chkUseLogging.Text = "Use Logging";
            this.chkUseLogging.UseVisualStyleBackColor = true;
            this.chkUseLogging.CheckedChanged += new System.EventHandler(this.chkUseLogging_CheckedChanged);
            // 
            // chkConfirmKarmaExpense
            // 
            this.chkConfirmKarmaExpense.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkConfirmKarmaExpense.AutoSize = true;
            this.tlpGlobalOptions.SetColumnSpan(this.chkConfirmKarmaExpense, 2);
            this.chkConfirmKarmaExpense.Location = new System.Drawing.Point(445, 162);
            this.chkConfirmKarmaExpense.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkConfirmKarmaExpense.Name = "chkConfirmKarmaExpense";
            this.chkConfirmKarmaExpense.Size = new System.Drawing.Size(215, 17);
            this.chkConfirmKarmaExpense.TabIndex = 39;
            this.chkConfirmKarmaExpense.Tag = "Checkbox_Options_ConfirmKarmaExpense";
            this.chkConfirmKarmaExpense.Text = "Ask for confirmation for Karma expenses";
            this.chkConfirmKarmaExpense.UseVisualStyleBackColor = true;
            this.chkConfirmKarmaExpense.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkConfirmDelete
            // 
            this.chkConfirmDelete.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkConfirmDelete.AutoSize = true;
            this.tlpGlobalOptions.SetColumnSpan(this.chkConfirmDelete, 2);
            this.chkConfirmDelete.Location = new System.Drawing.Point(445, 135);
            this.chkConfirmDelete.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkConfirmDelete.Name = "chkConfirmDelete";
            this.chkConfirmDelete.Size = new System.Drawing.Size(215, 17);
            this.chkConfirmDelete.TabIndex = 38;
            this.chkConfirmDelete.Tag = "Checkbox_Options_ConfirmDelete";
            this.chkConfirmDelete.Text = "Ask for confirmation when deleting items";
            this.chkConfirmDelete.UseVisualStyleBackColor = true;
            this.chkConfirmDelete.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkHideItemsOverAvail
            // 
            this.chkHideItemsOverAvail.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkHideItemsOverAvail.AutoSize = true;
            this.chkHideItemsOverAvail.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkHideItemsOverAvail.Checked = true;
            this.chkHideItemsOverAvail.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tlpGlobalOptions.SetColumnSpan(this.chkHideItemsOverAvail, 2);
            this.chkHideItemsOverAvail.Location = new System.Drawing.Point(445, 189);
            this.chkHideItemsOverAvail.Name = "chkHideItemsOverAvail";
            this.chkHideItemsOverAvail.Size = new System.Drawing.Size(353, 17);
            this.chkHideItemsOverAvail.TabIndex = 40;
            this.chkHideItemsOverAvail.Tag = "Checkbox_Option_HideItemsOverAvailLimit";
            this.chkHideItemsOverAvail.Text = "Hide items that are over the Availability Limit during character creation";
            this.chkHideItemsOverAvail.UseVisualStyleBackColor = true;
            // 
            // flpEnablePlugins
            // 
            this.flpEnablePlugins.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpEnablePlugins.AutoSize = true;
            this.flpEnablePlugins.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpGlobalOptions.SetColumnSpan(this.flpEnablePlugins, 2);
            this.flpEnablePlugins.Controls.Add(this.chkEnablePlugins);
            this.flpEnablePlugins.Controls.Add(this.cmdPluginsHelp);
            this.flpEnablePlugins.Location = new System.Drawing.Point(442, 210);
            this.flpEnablePlugins.Margin = new System.Windows.Forms.Padding(0);
            this.flpEnablePlugins.Name = "flpEnablePlugins";
            this.flpEnablePlugins.Size = new System.Drawing.Size(199, 29);
            this.flpEnablePlugins.TabIndex = 62;
            // 
            // chkEnablePlugins
            // 
            this.chkEnablePlugins.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkEnablePlugins.AutoSize = true;
            this.chkEnablePlugins.Location = new System.Drawing.Point(3, 3);
            this.chkEnablePlugins.Name = "chkEnablePlugins";
            this.chkEnablePlugins.Size = new System.Drawing.Size(164, 23);
            this.chkEnablePlugins.TabIndex = 51;
            this.chkEnablePlugins.Tag = "Checkbox_Options_EnablePlugins";
            this.chkEnablePlugins.Text = "Enable Plugins (experimental)";
            this.chkEnablePlugins.UseVisualStyleBackColor = true;
            this.chkEnablePlugins.CheckedChanged += new System.EventHandler(this.chkEnablePlugins_CheckedChanged);
            // 
            // cmdPluginsHelp
            // 
            this.cmdPluginsHelp.AutoSize = true;
            this.cmdPluginsHelp.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdPluginsHelp.Location = new System.Drawing.Point(173, 3);
            this.cmdPluginsHelp.MaximumSize = new System.Drawing.Size(23, 23);
            this.cmdPluginsHelp.MinimumSize = new System.Drawing.Size(23, 23);
            this.cmdPluginsHelp.Name = "cmdPluginsHelp";
            this.cmdPluginsHelp.Size = new System.Drawing.Size(23, 23);
            this.cmdPluginsHelp.TabIndex = 57;
            this.cmdPluginsHelp.Text = "?";
            this.cmdPluginsHelp.UseVisualStyleBackColor = true;
            this.cmdPluginsHelp.Click += new System.EventHandler(this.cmdPluginsHelp_Click);
            // 
            // chkAllowHoverIncrement
            // 
            this.chkAllowHoverIncrement.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkAllowHoverIncrement.AutoSize = true;
            this.chkAllowHoverIncrement.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkAllowHoverIncrement.Checked = true;
            this.chkAllowHoverIncrement.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tlpGlobalOptions.SetColumnSpan(this.chkAllowHoverIncrement, 2);
            this.chkAllowHoverIncrement.Location = new System.Drawing.Point(445, 243);
            this.chkAllowHoverIncrement.Name = "chkAllowHoverIncrement";
            this.chkAllowHoverIncrement.Size = new System.Drawing.Size(410, 17);
            this.chkAllowHoverIncrement.TabIndex = 41;
            this.chkAllowHoverIncrement.Tag = "Checkbox_Options_AllowHoverIncrement";
            this.chkAllowHoverIncrement.Text = "Allow incrementingvalues of numericupdown controls by hovering over the control";
            this.chkAllowHoverIncrement.UseVisualStyleBackColor = true;
            this.chkAllowHoverIncrement.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkAllowSkillDiceRolling
            // 
            this.chkAllowSkillDiceRolling.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkAllowSkillDiceRolling.AutoSize = true;
            this.tlpGlobalOptions.SetColumnSpan(this.chkAllowSkillDiceRolling, 2);
            this.chkAllowSkillDiceRolling.Location = new System.Drawing.Point(445, 268);
            this.chkAllowSkillDiceRolling.Name = "chkAllowSkillDiceRolling";
            this.chkAllowSkillDiceRolling.Size = new System.Drawing.Size(170, 17);
            this.chkAllowSkillDiceRolling.TabIndex = 69;
            this.chkAllowSkillDiceRolling.Tag = "Checkbox_Option_AllowSkillDiceRolling";
            this.chkAllowSkillDiceRolling.Text = "Allow dice rolling for dice pools";
            this.chkAllowSkillDiceRolling.UseVisualStyleBackColor = true;
            this.chkAllowSkillDiceRolling.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkSearchInCategoryOnly
            // 
            this.chkSearchInCategoryOnly.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkSearchInCategoryOnly.AutoSize = true;
            this.chkSearchInCategoryOnly.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkSearchInCategoryOnly.Checked = true;
            this.chkSearchInCategoryOnly.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tlpGlobalOptions.SetColumnSpan(this.chkSearchInCategoryOnly, 2);
            this.chkSearchInCategoryOnly.Location = new System.Drawing.Point(445, 293);
            this.chkSearchInCategoryOnly.Name = "chkSearchInCategoryOnly";
            this.chkSearchInCategoryOnly.Size = new System.Drawing.Size(325, 17);
            this.chkSearchInCategoryOnly.TabIndex = 21;
            this.chkSearchInCategoryOnly.Tag = "Checkbox_Options_SearchInCategoryOnly";
            this.chkSearchInCategoryOnly.Text = "Searching in selection forms is restricted to the current Category";
            this.chkSearchInCategoryOnly.UseVisualStyleBackColor = true;
            this.chkSearchInCategoryOnly.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkCreateBackupOnCareer
            // 
            this.chkCreateBackupOnCareer.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkCreateBackupOnCareer.AutoSize = true;
            this.chkCreateBackupOnCareer.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.tlpGlobalOptions.SetColumnSpan(this.chkCreateBackupOnCareer, 2);
            this.chkCreateBackupOnCareer.Location = new System.Drawing.Point(3, 435);
            this.chkCreateBackupOnCareer.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkCreateBackupOnCareer.Name = "chkCreateBackupOnCareer";
            this.chkCreateBackupOnCareer.Size = new System.Drawing.Size(333, 17);
            this.chkCreateBackupOnCareer.TabIndex = 24;
            this.chkCreateBackupOnCareer.Tag = "Checkbox_Option_CreateBackupOnCareer";
            this.chkCreateBackupOnCareer.Text = "Create backup of characters before moving them to Career Mode";
            this.chkCreateBackupOnCareer.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkCreateBackupOnCareer.UseVisualStyleBackColor = true;
            this.chkCreateBackupOnCareer.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // tlpGlobalOptionsTop
            // 
            this.tlpGlobalOptionsTop.AutoSize = true;
            this.tlpGlobalOptionsTop.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpGlobalOptionsTop.ColumnCount = 5;
            this.tlpGlobalOptions.SetColumnSpan(this.tlpGlobalOptionsTop, 4);
            this.tlpGlobalOptionsTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpGlobalOptionsTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpGlobalOptionsTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpGlobalOptionsTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpGlobalOptionsTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpGlobalOptionsTop.Controls.Add(this.grpDateFormat, 2, 2);
            this.tlpGlobalOptionsTop.Controls.Add(this.grpTimeFormat, 3, 2);
            this.tlpGlobalOptionsTop.Controls.Add(this.chkCustomDateTimeFormats, 0, 2);
            this.tlpGlobalOptionsTop.Controls.Add(this.lblXSLT, 0, 1);
            this.tlpGlobalOptionsTop.Controls.Add(this.lblLanguage, 0, 0);
            this.tlpGlobalOptionsTop.Controls.Add(this.cboSheetLanguage, 2, 1);
            this.tlpGlobalOptionsTop.Controls.Add(this.cboXSLT, 3, 1);
            this.tlpGlobalOptionsTop.Controls.Add(this.cboLanguage, 2, 0);
            this.tlpGlobalOptionsTop.Controls.Add(this.cmdVerify, 3, 0);
            this.tlpGlobalOptionsTop.Controls.Add(this.cmdVerifyData, 4, 0);
            this.tlpGlobalOptionsTop.Controls.Add(this.imgSheetLanguageFlag, 1, 1);
            this.tlpGlobalOptionsTop.Controls.Add(this.imgLanguageFlag, 1, 0);
            this.tlpGlobalOptionsTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpGlobalOptionsTop.Location = new System.Drawing.Point(0, 0);
            this.tlpGlobalOptionsTop.Margin = new System.Windows.Forms.Padding(0);
            this.tlpGlobalOptionsTop.Name = "tlpGlobalOptionsTop";
            this.tlpGlobalOptionsTop.RowCount = 3;
            this.tlpGlobalOptionsTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpGlobalOptionsTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpGlobalOptionsTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGlobalOptionsTop.Size = new System.Drawing.Size(886, 131);
            this.tlpGlobalOptionsTop.TabIndex = 73;
            // 
            // grpDateFormat
            // 
            this.grpDateFormat.AutoSize = true;
            this.grpDateFormat.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.grpDateFormat.Controls.Add(this.tlpDateFormat);
            this.grpDateFormat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpDateFormat.Enabled = false;
            this.grpDateFormat.Location = new System.Drawing.Point(155, 60);
            this.grpDateFormat.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.grpDateFormat.Name = "grpDateFormat";
            this.grpDateFormat.Size = new System.Drawing.Size(361, 71);
            this.grpDateFormat.TabIndex = 59;
            this.grpDateFormat.TabStop = false;
            this.grpDateFormat.Tag = "Label_Options_DateFormat";
            this.grpDateFormat.Text = "Date Format";
            // 
            // tlpDateFormat
            // 
            this.tlpDateFormat.AutoSize = true;
            this.tlpDateFormat.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpDateFormat.ColumnCount = 1;
            this.tlpDateFormat.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpDateFormat.Controls.Add(this.txtDateFormat, 0, 1);
            this.tlpDateFormat.Controls.Add(this.txtDateFormatView, 0, 0);
            this.tlpDateFormat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpDateFormat.Location = new System.Drawing.Point(3, 16);
            this.tlpDateFormat.Name = "tlpDateFormat";
            this.tlpDateFormat.RowCount = 2;
            this.tlpDateFormat.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDateFormat.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDateFormat.Size = new System.Drawing.Size(355, 52);
            this.tlpDateFormat.TabIndex = 0;
            // 
            // txtDateFormat
            // 
            this.txtDateFormat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDateFormat.Location = new System.Drawing.Point(3, 29);
            this.txtDateFormat.Name = "txtDateFormat";
            this.txtDateFormat.Size = new System.Drawing.Size(349, 20);
            this.txtDateFormat.TabIndex = 48;
            this.txtDateFormat.TextChanged += new System.EventHandler(this.txtDateFormat_TextChanged);
            // 
            // txtDateFormatView
            // 
            this.txtDateFormatView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDateFormatView.Location = new System.Drawing.Point(3, 3);
            this.txtDateFormatView.Name = "txtDateFormatView";
            this.txtDateFormatView.ReadOnly = true;
            this.txtDateFormatView.Size = new System.Drawing.Size(349, 20);
            this.txtDateFormatView.TabIndex = 47;
            // 
            // grpTimeFormat
            // 
            this.grpTimeFormat.AutoSize = true;
            this.grpTimeFormat.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpGlobalOptionsTop.SetColumnSpan(this.grpTimeFormat, 2);
            this.grpTimeFormat.Controls.Add(this.tlpTimeFormat);
            this.grpTimeFormat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpTimeFormat.Enabled = false;
            this.grpTimeFormat.Location = new System.Drawing.Point(522, 60);
            this.grpTimeFormat.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.grpTimeFormat.Name = "grpTimeFormat";
            this.grpTimeFormat.Size = new System.Drawing.Size(361, 71);
            this.grpTimeFormat.TabIndex = 61;
            this.grpTimeFormat.TabStop = false;
            this.grpTimeFormat.Tag = "Label_Options_TimeFormat";
            this.grpTimeFormat.Text = "Time Format";
            // 
            // tlpTimeFormat
            // 
            this.tlpTimeFormat.AutoSize = true;
            this.tlpTimeFormat.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpTimeFormat.ColumnCount = 1;
            this.tlpTimeFormat.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpTimeFormat.Controls.Add(this.txtTimeFormat, 0, 1);
            this.tlpTimeFormat.Controls.Add(this.txtTimeFormatView, 0, 0);
            this.tlpTimeFormat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTimeFormat.Location = new System.Drawing.Point(3, 16);
            this.tlpTimeFormat.Name = "tlpTimeFormat";
            this.tlpTimeFormat.RowCount = 2;
            this.tlpTimeFormat.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTimeFormat.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTimeFormat.Size = new System.Drawing.Size(355, 52);
            this.tlpTimeFormat.TabIndex = 0;
            // 
            // txtTimeFormat
            // 
            this.txtTimeFormat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTimeFormat.Location = new System.Drawing.Point(3, 29);
            this.txtTimeFormat.Name = "txtTimeFormat";
            this.txtTimeFormat.Size = new System.Drawing.Size(349, 20);
            this.txtTimeFormat.TabIndex = 48;
            this.txtTimeFormat.TextChanged += new System.EventHandler(this.txtTimeFormat_TextChanged);
            // 
            // txtTimeFormatView
            // 
            this.txtTimeFormatView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTimeFormatView.Location = new System.Drawing.Point(3, 3);
            this.txtTimeFormatView.Name = "txtTimeFormatView";
            this.txtTimeFormatView.ReadOnly = true;
            this.txtTimeFormatView.Size = new System.Drawing.Size(349, 20);
            this.txtTimeFormatView.TabIndex = 47;
            // 
            // chkCustomDateTimeFormats
            // 
            this.chkCustomDateTimeFormats.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkCustomDateTimeFormats.AutoSize = true;
            this.tlpGlobalOptionsTop.SetColumnSpan(this.chkCustomDateTimeFormats, 2);
            this.chkCustomDateTimeFormats.Location = new System.Drawing.Point(3, 87);
            this.chkCustomDateTimeFormats.Name = "chkCustomDateTimeFormats";
            this.chkCustomDateTimeFormats.Size = new System.Drawing.Size(122, 17);
            this.chkCustomDateTimeFormats.TabIndex = 60;
            this.chkCustomDateTimeFormats.Tag = "Checkbox_Options_CustomTimeFormat";
            this.chkCustomDateTimeFormats.Text = "Custom Time Format";
            this.chkCustomDateTimeFormats.UseVisualStyleBackColor = true;
            this.chkCustomDateTimeFormats.CheckedChanged += new System.EventHandler(this.chkCustomDateTimeFormats_CheckedChanged);
            // 
            // lblXSLT
            // 
            this.lblXSLT.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblXSLT.AutoSize = true;
            this.lblXSLT.Location = new System.Drawing.Point(3, 38);
            this.lblXSLT.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblXSLT.Name = "lblXSLT";
            this.lblXSLT.Size = new System.Drawing.Size(124, 13);
            this.lblXSLT.TabIndex = 7;
            this.lblXSLT.Tag = "Label_Options_DefaultCharacterSheet";
            this.lblXSLT.Text = "Default Character Sheet:";
            this.lblXSLT.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblLanguage
            // 
            this.lblLanguage.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblLanguage.AutoSize = true;
            this.lblLanguage.Location = new System.Drawing.Point(69, 8);
            this.lblLanguage.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Size = new System.Drawing.Size(58, 13);
            this.lblLanguage.TabIndex = 0;
            this.lblLanguage.Tag = "Label_Options_Language";
            this.lblLanguage.Text = "Language:";
            this.lblLanguage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboSheetLanguage
            // 
            this.cboSheetLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSheetLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSheetLanguage.FormattingEnabled = true;
            this.cboSheetLanguage.Location = new System.Drawing.Point(155, 34);
            this.cboSheetLanguage.Name = "cboSheetLanguage";
            this.cboSheetLanguage.Size = new System.Drawing.Size(361, 21);
            this.cboSheetLanguage.TabIndex = 34;
            this.cboSheetLanguage.TooltipText = "";
            this.cboSheetLanguage.SelectedIndexChanged += new System.EventHandler(this.cboSheetLanguage_SelectedIndexChanged);
            // 
            // cboXSLT
            // 
            this.cboXSLT.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpGlobalOptionsTop.SetColumnSpan(this.cboXSLT, 2);
            this.cboXSLT.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboXSLT.FormattingEnabled = true;
            this.cboXSLT.Location = new System.Drawing.Point(522, 34);
            this.cboXSLT.Name = "cboXSLT";
            this.cboXSLT.Size = new System.Drawing.Size(361, 21);
            this.cboXSLT.TabIndex = 8;
            this.cboXSLT.TooltipText = "";
            // 
            // cboLanguage
            // 
            this.cboLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLanguage.FormattingEnabled = true;
            this.cboLanguage.Location = new System.Drawing.Point(155, 4);
            this.cboLanguage.Name = "cboLanguage";
            this.cboLanguage.Size = new System.Drawing.Size(361, 21);
            this.cboLanguage.TabIndex = 1;
            this.cboLanguage.TooltipText = "";
            this.cboLanguage.SelectedIndexChanged += new System.EventHandler(this.cboLanguage_SelectedIndexChanged);
            // 
            // cmdVerify
            // 
            this.cmdVerify.AutoSize = true;
            this.cmdVerify.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdVerify.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdVerify.Enabled = false;
            this.cmdVerify.Location = new System.Drawing.Point(522, 3);
            this.cmdVerify.Name = "cmdVerify";
            this.cmdVerify.Size = new System.Drawing.Size(177, 24);
            this.cmdVerify.TabIndex = 2;
            this.cmdVerify.Text = "Verify";
            this.cmdVerify.UseVisualStyleBackColor = true;
            this.cmdVerify.Click += new System.EventHandler(this.cmdVerify_Click);
            // 
            // cmdVerifyData
            // 
            this.cmdVerifyData.AutoSize = true;
            this.cmdVerifyData.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdVerifyData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdVerifyData.Enabled = false;
            this.cmdVerifyData.Location = new System.Drawing.Point(705, 3);
            this.cmdVerifyData.Name = "cmdVerifyData";
            this.cmdVerifyData.Size = new System.Drawing.Size(178, 24);
            this.cmdVerifyData.TabIndex = 3;
            this.cmdVerifyData.Text = "Verify Data File";
            this.cmdVerifyData.UseVisualStyleBackColor = true;
            this.cmdVerifyData.Click += new System.EventHandler(this.cmdVerifyData_Click);
            // 
            // imgSheetLanguageFlag
            // 
            this.imgSheetLanguageFlag.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imgSheetLanguageFlag.Location = new System.Drawing.Point(133, 33);
            this.imgSheetLanguageFlag.Name = "imgSheetLanguageFlag";
            this.imgSheetLanguageFlag.Size = new System.Drawing.Size(16, 24);
            this.imgSheetLanguageFlag.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.imgSheetLanguageFlag.TabIndex = 50;
            this.imgSheetLanguageFlag.TabStop = false;
            // 
            // imgLanguageFlag
            // 
            this.imgLanguageFlag.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imgLanguageFlag.Location = new System.Drawing.Point(133, 3);
            this.imgLanguageFlag.Name = "imgLanguageFlag";
            this.imgLanguageFlag.Size = new System.Drawing.Size(16, 24);
            this.imgLanguageFlag.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.imgLanguageFlag.TabIndex = 49;
            this.imgLanguageFlag.TabStop = false;
            // 
            // tlpLoggingOptions
            // 
            this.tlpLoggingOptions.AutoSize = true;
            this.tlpLoggingOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpLoggingOptions.ColumnCount = 2;
            this.tlpLoggingOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLoggingOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpLoggingOptions.Controls.Add(this.cboUseLoggingApplicationInsights, 0, 0);
            this.tlpLoggingOptions.Controls.Add(this.cmdUseLoggingHelp, 1, 0);
            this.tlpLoggingOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpLoggingOptions.Location = new System.Drawing.Point(177, 156);
            this.tlpLoggingOptions.Margin = new System.Windows.Forms.Padding(0);
            this.tlpLoggingOptions.Name = "tlpLoggingOptions";
            this.tlpLoggingOptions.RowCount = 1;
            this.tlpLoggingOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLoggingOptions.Size = new System.Drawing.Size(265, 29);
            this.tlpLoggingOptions.TabIndex = 75;
            // 
            // cboUseLoggingApplicationInsights
            // 
            this.cboUseLoggingApplicationInsights.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboUseLoggingApplicationInsights.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboUseLoggingApplicationInsights.Location = new System.Drawing.Point(3, 4);
            this.cboUseLoggingApplicationInsights.Name = "cboUseLoggingApplicationInsights";
            this.cboUseLoggingApplicationInsights.Size = new System.Drawing.Size(230, 21);
            this.cboUseLoggingApplicationInsights.TabIndex = 55;
            this.cboUseLoggingApplicationInsights.SelectedIndexChanged += new System.EventHandler(this.cboUseLoggingApplicationInsights_SelectedIndexChanged);
            // 
            // cmdUseLoggingHelp
            // 
            this.cmdUseLoggingHelp.AutoSize = true;
            this.cmdUseLoggingHelp.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdUseLoggingHelp.Location = new System.Drawing.Point(239, 3);
            this.cmdUseLoggingHelp.MinimumSize = new System.Drawing.Size(23, 23);
            this.cmdUseLoggingHelp.Name = "cmdUseLoggingHelp";
            this.cmdUseLoggingHelp.Size = new System.Drawing.Size(23, 23);
            this.cmdUseLoggingHelp.TabIndex = 56;
            this.cmdUseLoggingHelp.Text = "?";
            this.cmdUseLoggingHelp.ToolTipText = "";
            this.cmdUseLoggingHelp.UseVisualStyleBackColor = true;
            this.cmdUseLoggingHelp.Click += new System.EventHandler(this.cboUseLoggingHelp_Click);
            // 
            // lblDefaultCharacterOption
            // 
            this.lblDefaultCharacterOption.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDefaultCharacterOption.AutoSize = true;
            this.lblDefaultCharacterOption.Location = new System.Drawing.Point(499, 370);
            this.lblDefaultCharacterOption.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDefaultCharacterOption.Name = "lblDefaultCharacterOption";
            this.lblDefaultCharacterOption.Size = new System.Drawing.Size(117, 26);
            this.lblDefaultCharacterOption.TabIndex = 70;
            this.lblDefaultCharacterOption.Tag = "Label_Options_DefaultCharacterOption";
            this.lblDefaultCharacterOption.Text = "Default Setting for New Characters:";
            this.lblDefaultCharacterOption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboDefaultCharacterOption
            // 
            this.cboDefaultCharacterOption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboDefaultCharacterOption.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDefaultCharacterOption.FormattingEnabled = true;
            this.cboDefaultCharacterOption.Location = new System.Drawing.Point(622, 372);
            this.cboDefaultCharacterOption.Name = "cboDefaultCharacterOption";
            this.cboDefaultCharacterOption.Size = new System.Drawing.Size(261, 21);
            this.cboDefaultCharacterOption.TabIndex = 7;
            this.cboDefaultCharacterOption.SelectedIndexChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblCharacterRosterLabel
            // 
            this.lblCharacterRosterLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCharacterRosterLabel.AutoSize = true;
            this.lblCharacterRosterLabel.Location = new System.Drawing.Point(459, 410);
            this.lblCharacterRosterLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCharacterRosterLabel.Name = "lblCharacterRosterLabel";
            this.lblCharacterRosterLabel.Size = new System.Drawing.Size(157, 13);
            this.lblCharacterRosterLabel.TabIndex = 44;
            this.lblCharacterRosterLabel.Tag = "Label_Options_CharacterRoster";
            this.lblCharacterRosterLabel.Text = "Character Roster Watch Folder:";
            this.lblCharacterRosterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tlpCharacterRosterPath
            // 
            this.tlpCharacterRosterPath.AutoSize = true;
            this.tlpCharacterRosterPath.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpCharacterRosterPath.ColumnCount = 2;
            this.tlpCharacterRosterPath.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCharacterRosterPath.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCharacterRosterPath.Controls.Add(this.cmdCharacterRoster, 1, 0);
            this.tlpCharacterRosterPath.Controls.Add(this.txtCharacterRosterPath, 0, 0);
            this.tlpCharacterRosterPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCharacterRosterPath.Location = new System.Drawing.Point(619, 402);
            this.tlpCharacterRosterPath.Margin = new System.Windows.Forms.Padding(0);
            this.tlpCharacterRosterPath.Name = "tlpCharacterRosterPath";
            this.tlpCharacterRosterPath.RowCount = 1;
            this.tlpCharacterRosterPath.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCharacterRosterPath.Size = new System.Drawing.Size(267, 29);
            this.tlpCharacterRosterPath.TabIndex = 71;
            // 
            // cmdCharacterRoster
            // 
            this.cmdCharacterRoster.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmdCharacterRoster.AutoSize = true;
            this.cmdCharacterRoster.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCharacterRoster.Location = new System.Drawing.Point(238, 3);
            this.cmdCharacterRoster.Name = "cmdCharacterRoster";
            this.cmdCharacterRoster.Size = new System.Drawing.Size(26, 23);
            this.cmdCharacterRoster.TabIndex = 47;
            this.cmdCharacterRoster.Text = "...";
            this.cmdCharacterRoster.UseVisualStyleBackColor = true;
            this.cmdCharacterRoster.Click += new System.EventHandler(this.cmdCharacterRoster_Click);
            // 
            // txtCharacterRosterPath
            // 
            this.txtCharacterRosterPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCharacterRosterPath.Location = new System.Drawing.Point(3, 4);
            this.txtCharacterRosterPath.Name = "txtCharacterRosterPath";
            this.txtCharacterRosterPath.ReadOnly = true;
            this.txtCharacterRosterPath.Size = new System.Drawing.Size(229, 20);
            this.txtCharacterRosterPath.TabIndex = 45;
            this.txtCharacterRosterPath.TextChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblPDFParametersLabel
            // 
            this.lblPDFParametersLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblPDFParametersLabel.AutoSize = true;
            this.lblPDFParametersLabel.Location = new System.Drawing.Point(529, 464);
            this.lblPDFParametersLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPDFParametersLabel.Name = "lblPDFParametersLabel";
            this.lblPDFParametersLabel.Size = new System.Drawing.Size(87, 13);
            this.lblPDFParametersLabel.TabIndex = 19;
            this.lblPDFParametersLabel.Tag = "Label_Options_PDFParameters";
            this.lblPDFParametersLabel.Text = "PDF Parameters:";
            this.lblPDFParametersLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboPDFParameters
            // 
            this.cboPDFParameters.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboPDFParameters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPDFParameters.FormattingEnabled = true;
            this.cboPDFParameters.Location = new System.Drawing.Point(622, 460);
            this.cboPDFParameters.Name = "cboPDFParameters";
            this.cboPDFParameters.Size = new System.Drawing.Size(261, 21);
            this.cboPDFParameters.TabIndex = 26;
            this.cboPDFParameters.TooltipText = "";
            this.cboPDFParameters.SelectedIndexChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblPDFAppPath
            // 
            this.lblPDFAppPath.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblPDFAppPath.AutoSize = true;
            this.lblPDFAppPath.Location = new System.Drawing.Point(475, 492);
            this.lblPDFAppPath.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPDFAppPath.Name = "lblPDFAppPath";
            this.lblPDFAppPath.Size = new System.Drawing.Size(141, 13);
            this.lblPDFAppPath.TabIndex = 9;
            this.lblPDFAppPath.Tag = "Label_Options_PDFApplicationPath";
            this.lblPDFAppPath.Text = "Location of PDF application:";
            this.lblPDFAppPath.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tlpPDFAppPath
            // 
            this.tlpPDFAppPath.AutoSize = true;
            this.tlpPDFAppPath.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpPDFAppPath.ColumnCount = 2;
            this.tlpPDFAppPath.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPDFAppPath.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpPDFAppPath.Controls.Add(this.txtPDFAppPath, 0, 0);
            this.tlpPDFAppPath.Controls.Add(this.cmdPDFAppPath, 1, 0);
            this.tlpPDFAppPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPDFAppPath.Location = new System.Drawing.Point(619, 484);
            this.tlpPDFAppPath.Margin = new System.Windows.Forms.Padding(0);
            this.tlpPDFAppPath.Name = "tlpPDFAppPath";
            this.tlpPDFAppPath.RowCount = 1;
            this.tlpPDFAppPath.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPDFAppPath.Size = new System.Drawing.Size(267, 29);
            this.tlpPDFAppPath.TabIndex = 76;
            // 
            // txtPDFAppPath
            // 
            this.txtPDFAppPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPDFAppPath.Location = new System.Drawing.Point(3, 4);
            this.txtPDFAppPath.Name = "txtPDFAppPath";
            this.txtPDFAppPath.ReadOnly = true;
            this.txtPDFAppPath.Size = new System.Drawing.Size(229, 20);
            this.txtPDFAppPath.TabIndex = 10;
            this.txtPDFAppPath.TextChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // cmdPDFAppPath
            // 
            this.cmdPDFAppPath.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmdPDFAppPath.AutoSize = true;
            this.cmdPDFAppPath.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdPDFAppPath.Location = new System.Drawing.Point(238, 3);
            this.cmdPDFAppPath.Name = "cmdPDFAppPath";
            this.cmdPDFAppPath.Size = new System.Drawing.Size(26, 23);
            this.cmdPDFAppPath.TabIndex = 11;
            this.cmdPDFAppPath.Text = "...";
            this.cmdPDFAppPath.UseVisualStyleBackColor = true;
            this.cmdPDFAppPath.Click += new System.EventHandler(this.cmdPDFAppPath_Click);
            // 
            // flpBrowserVersion
            // 
            this.flpBrowserVersion.AutoSize = true;
            this.flpBrowserVersion.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpGlobalOptions.SetColumnSpan(this.flpBrowserVersion, 2);
            this.flpBrowserVersion.Controls.Add(this.lblBrowserVersion);
            this.flpBrowserVersion.Controls.Add(this.nudBrowserVersion);
            this.flpBrowserVersion.Location = new System.Drawing.Point(442, 431);
            this.flpBrowserVersion.Margin = new System.Windows.Forms.Padding(0);
            this.flpBrowserVersion.Name = "flpBrowserVersion";
            this.flpBrowserVersion.Size = new System.Drawing.Size(250, 26);
            this.flpBrowserVersion.TabIndex = 77;
            // 
            // lblBrowserVersion
            // 
            this.lblBrowserVersion.AutoSize = true;
            this.lblBrowserVersion.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblBrowserVersion.Location = new System.Drawing.Point(3, 3);
            this.lblBrowserVersion.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.lblBrowserVersion.Name = "lblBrowserVersion";
            this.lblBrowserVersion.Size = new System.Drawing.Size(190, 17);
            this.lblBrowserVersion.TabIndex = 53;
            this.lblBrowserVersion.Tag = "Label_Options_BrowserVersion";
            this.lblBrowserVersion.Text = "Preview uses Internet Explorer version:";
            this.lblBrowserVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudBrowserVersion
            // 
            this.nudBrowserVersion.Location = new System.Drawing.Point(199, 3);
            this.nudBrowserVersion.Maximum = new decimal(new int[] {
            11,
            0,
            0,
            0});
            this.nudBrowserVersion.Minimum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.nudBrowserVersion.Name = "nudBrowserVersion";
            this.nudBrowserVersion.Size = new System.Drawing.Size(48, 20);
            this.nudBrowserVersion.TabIndex = 54;
            this.nudBrowserVersion.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.nudBrowserVersion.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkHideCharts
            // 
            this.chkHideCharts.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkHideCharts.AutoSize = true;
            this.tlpGlobalOptions.SetColumnSpan(this.chkHideCharts, 2);
            this.chkHideCharts.Location = new System.Drawing.Point(3, 343);
            this.chkHideCharts.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkHideCharts.Name = "chkHideCharts";
            this.chkHideCharts.Size = new System.Drawing.Size(215, 17);
            this.chkHideCharts.TabIndex = 58;
            this.chkHideCharts.Tag = "Checkbox_Options_HideCharts";
            this.chkHideCharts.Text = "Hide charts that cause issues with Wine";
            this.chkHideCharts.UseVisualStyleBackColor = true;
            this.chkHideCharts.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkLiveUpdateCleanCharacterFiles
            // 
            this.chkLiveUpdateCleanCharacterFiles.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkLiveUpdateCleanCharacterFiles.AutoSize = true;
            this.tlpGlobalOptions.SetColumnSpan(this.chkLiveUpdateCleanCharacterFiles, 2);
            this.chkLiveUpdateCleanCharacterFiles.Location = new System.Drawing.Point(3, 368);
            this.chkLiveUpdateCleanCharacterFiles.Name = "chkLiveUpdateCleanCharacterFiles";
            this.chkLiveUpdateCleanCharacterFiles.Size = new System.Drawing.Size(286, 30);
            this.chkLiveUpdateCleanCharacterFiles.TabIndex = 33;
            this.chkLiveUpdateCleanCharacterFiles.Tag = "Checkbox_Options_LiveUpdateCleanCharacterFiles";
            this.chkLiveUpdateCleanCharacterFiles.Text = "Automatically load changes from open characters\' save\r\nfiles if there are no pend" +
    "ing changes to be saved";
            this.chkLiveUpdateCleanCharacterFiles.UseVisualStyleBackColor = true;
            this.chkLiveUpdateCleanCharacterFiles.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkPreferNightlyBuilds
            // 
            this.chkPreferNightlyBuilds.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkPreferNightlyBuilds.AutoSize = true;
            this.chkPreferNightlyBuilds.Location = new System.Drawing.Point(3, 408);
            this.chkPreferNightlyBuilds.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkPreferNightlyBuilds.Name = "chkPreferNightlyBuilds";
            this.chkPreferNightlyBuilds.Size = new System.Drawing.Size(120, 17);
            this.chkPreferNightlyBuilds.TabIndex = 25;
            this.chkPreferNightlyBuilds.Tag = "Checkbox_Options_PreferNightlyBuilds";
            this.chkPreferNightlyBuilds.Text = "Prefer Nightly Builds";
            this.chkPreferNightlyBuilds.UseVisualStyleBackColor = true;
            this.chkPreferNightlyBuilds.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkHideCharacterRoster
            // 
            this.chkHideCharacterRoster.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkHideCharacterRoster.AutoSize = true;
            this.chkHideCharacterRoster.Location = new System.Drawing.Point(445, 343);
            this.chkHideCharacterRoster.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkHideCharacterRoster.Name = "chkHideCharacterRoster";
            this.chkHideCharacterRoster.Size = new System.Drawing.Size(149, 17);
            this.chkHideCharacterRoster.TabIndex = 35;
            this.chkHideCharacterRoster.Tag = "Checkbox_Options_HideCharacterRoster";
            this.chkHideCharacterRoster.Text = "Hide the Character Roster";
            this.chkHideCharacterRoster.UseVisualStyleBackColor = true;
            this.chkHideCharacterRoster.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // chkPrintToFileFirst
            // 
            this.chkPrintToFileFirst.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkPrintToFileFirst.AutoSize = true;
            this.chkPrintToFileFirst.Location = new System.Drawing.Point(3, 462);
            this.chkPrintToFileFirst.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkPrintToFileFirst.Name = "chkPrintToFileFirst";
            this.chkPrintToFileFirst.Size = new System.Drawing.Size(130, 17);
            this.chkPrintToFileFirst.TabIndex = 43;
            this.chkPrintToFileFirst.Tag = "Checkbox_Option_PrintToFileFirst";
            this.chkPrintToFileFirst.Text = "Apply Linux printing fix";
            this.chkPrintToFileFirst.UseVisualStyleBackColor = true;
            this.chkPrintToFileFirst.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // lblMugshotCompression
            // 
            this.lblMugshotCompression.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMugshotCompression.AutoSize = true;
            this.lblMugshotCompression.Location = new System.Drawing.Point(48, 492);
            this.lblMugshotCompression.Name = "lblMugshotCompression";
            this.lblMugshotCompression.Size = new System.Drawing.Size(126, 13);
            this.lblMugshotCompression.TabIndex = 65;
            this.lblMugshotCompression.Tag = "Label_Options_MugshotCompression";
            this.lblMugshotCompression.Text = "Mugshot Storage Format:";
            this.lblMugshotCompression.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tlpMugshotCompression
            // 
            this.tlpMugshotCompression.AutoSize = true;
            this.tlpMugshotCompression.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMugshotCompression.ColumnCount = 3;
            this.tlpMugshotCompression.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMugshotCompression.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMugshotCompression.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMugshotCompression.Controls.Add(this.lblMugshotCompressionQuality, 1, 0);
            this.tlpMugshotCompression.Controls.Add(this.nudMugshotCompressionQuality, 2, 0);
            this.tlpMugshotCompression.Controls.Add(this.cboMugshotCompression, 0, 0);
            this.tlpMugshotCompression.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMugshotCompression.Location = new System.Drawing.Point(177, 484);
            this.tlpMugshotCompression.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMugshotCompression.Name = "tlpMugshotCompression";
            this.tlpMugshotCompression.RowCount = 1;
            this.tlpMugshotCompression.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMugshotCompression.Size = new System.Drawing.Size(265, 29);
            this.tlpMugshotCompression.TabIndex = 72;
            // 
            // lblMugshotCompressionQuality
            // 
            this.lblMugshotCompressionQuality.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMugshotCompressionQuality.AutoSize = true;
            this.lblMugshotCompressionQuality.Location = new System.Drawing.Point(141, 8);
            this.lblMugshotCompressionQuality.Name = "lblMugshotCompressionQuality";
            this.lblMugshotCompressionQuality.Size = new System.Drawing.Size(74, 13);
            this.lblMugshotCompressionQuality.TabIndex = 67;
            this.lblMugshotCompressionQuality.Tag = "Label_Options_ImageQuality";
            this.lblMugshotCompressionQuality.Text = "Image Quality:";
            // 
            // nudMugshotCompressionQuality
            // 
            this.nudMugshotCompressionQuality.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudMugshotCompressionQuality.AutoSize = true;
            this.nudMugshotCompressionQuality.Location = new System.Drawing.Point(221, 4);
            this.nudMugshotCompressionQuality.Name = "nudMugshotCompressionQuality";
            this.nudMugshotCompressionQuality.Size = new System.Drawing.Size(41, 20);
            this.nudMugshotCompressionQuality.TabIndex = 68;
            this.nudMugshotCompressionQuality.Value = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.nudMugshotCompressionQuality.ValueChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // cboMugshotCompression
            // 
            this.cboMugshotCompression.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboMugshotCompression.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMugshotCompression.FormattingEnabled = true;
            this.cboMugshotCompression.Location = new System.Drawing.Point(3, 4);
            this.cboMugshotCompression.Name = "cboMugshotCompression";
            this.cboMugshotCompression.Size = new System.Drawing.Size(132, 21);
            this.cboMugshotCompression.TabIndex = 66;
            this.cboMugshotCompression.TooltipText = "";
            this.cboMugshotCompression.SelectedIndexChanged += new System.EventHandler(this.cboMugshotCompression_SelectedIndexChanged);
            // 
            // chkHideMasterIndex
            // 
            this.chkHideMasterIndex.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkHideMasterIndex.AutoSize = true;
            this.chkHideMasterIndex.Location = new System.Drawing.Point(406, 339);
            this.chkHideMasterIndex.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkHideMasterIndex.Name = "chkHideMasterIndex";
            this.chkHideMasterIndex.Size = new System.Drawing.Size(130, 17);
            this.chkHideMasterIndex.TabIndex = 69;
            this.chkHideMasterIndex.Tag = "Checkbox_Options_HideMasterIndex";
            this.chkHideMasterIndex.Text = "Hide the Master Index";
            this.chkHideMasterIndex.UseVisualStyleBackColor = true;
            // 
            // gpbEditSourcebookInfo
            // 
            this.gpbEditSourcebookInfo.AutoSize = true;
            this.gpbEditSourcebookInfo.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbEditSourcebookInfo.Controls.Add(this.lstGlobalSourcebookInfos);
            this.gpbEditSourcebookInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpbEditSourcebookInfo.Location = new System.Drawing.Point(3, 3);
            this.gpbEditSourcebookInfo.Name = "gpbEditSourcebookInfo";
            this.tlpGlobal.SetRowSpan(this.gpbEditSourcebookInfo, 2);
            this.gpbEditSourcebookInfo.Size = new System.Drawing.Size(295, 578);
            this.gpbEditSourcebookInfo.TabIndex = 65;
            this.gpbEditSourcebookInfo.TabStop = false;
            this.gpbEditSourcebookInfo.Tag = "Label_Options_EditSourcebookInfo";
            this.gpbEditSourcebookInfo.Text = "Edit Sourcebook Info";
            // 
            // lstGlobalSourcebookInfos
            // 
            this.lstGlobalSourcebookInfos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstGlobalSourcebookInfos.FormattingEnabled = true;
            this.lstGlobalSourcebookInfos.Location = new System.Drawing.Point(3, 16);
            this.lstGlobalSourcebookInfos.Name = "lstGlobalSourcebookInfos";
            this.lstGlobalSourcebookInfos.Size = new System.Drawing.Size(289, 559);
            this.lstGlobalSourcebookInfos.TabIndex = 48;
            this.lstGlobalSourcebookInfos.SelectedIndexChanged += new System.EventHandler(this.lstGlobalSourcebookInfos_SelectedIndexChanged);
            // 
            // tabCustomDataDirectories
            // 
            this.tabCustomDataDirectories.BackColor = System.Drawing.SystemColors.Control;
            this.tabCustomDataDirectories.Controls.Add(this.tlpOptionalRules);
            this.tabCustomDataDirectories.Location = new System.Drawing.Point(4, 22);
            this.tabCustomDataDirectories.Name = "tabCustomDataDirectories";
            this.tabCustomDataDirectories.Padding = new System.Windows.Forms.Padding(9);
            this.tabCustomDataDirectories.Size = new System.Drawing.Size(1232, 602);
            this.tabCustomDataDirectories.TabIndex = 2;
            this.tabCustomDataDirectories.Tag = "Tab_Options_CustomDataDirectories";
            this.tabCustomDataDirectories.Text = "Custom Data Directories";
            // 
            // tlpOptionalRules
            // 
            this.tlpOptionalRules.AutoSize = true;
            this.tlpOptionalRules.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpOptionalRules.ColumnCount = 4;
            this.tlpOptionalRules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tlpOptionalRules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tlpOptionalRules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tlpOptionalRules.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpOptionalRules.Controls.Add(this.lblCustomDataDirectoriesLabel, 0, 0);
            this.tlpOptionalRules.Controls.Add(this.cmdAddCustomDirectory, 1, 0);
            this.tlpOptionalRules.Controls.Add(this.treCustomDataDirectories, 0, 1);
            this.tlpOptionalRules.Controls.Add(this.cmdRenameCustomDataDirectory, 2, 0);
            this.tlpOptionalRules.Controls.Add(this.cmdRemoveCustomDirectory, 3, 0);
            this.tlpOptionalRules.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpOptionalRules.Location = new System.Drawing.Point(9, 9);
            this.tlpOptionalRules.Name = "tlpOptionalRules";
            this.tlpOptionalRules.RowCount = 2;
            this.tlpOptionalRules.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptionalRules.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpOptionalRules.Size = new System.Drawing.Size(1214, 584);
            this.tlpOptionalRules.TabIndex = 44;
            // 
            // lblCustomDataDirectoriesLabel
            // 
            this.lblCustomDataDirectoriesLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCustomDataDirectoriesLabel.AutoSize = true;
            this.lblCustomDataDirectoriesLabel.Location = new System.Drawing.Point(3, 8);
            this.lblCustomDataDirectoriesLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCustomDataDirectoriesLabel.Name = "lblCustomDataDirectoriesLabel";
            this.lblCustomDataDirectoriesLabel.Size = new System.Drawing.Size(351, 13);
            this.lblCustomDataDirectoriesLabel.TabIndex = 36;
            this.lblCustomDataDirectoriesLabel.Tag = "Label_Options_CustomDataDirectories";
            this.lblCustomDataDirectoriesLabel.Text = "Custom Data Directory Entries (Changes Are Only Applied After a Restart)";
            this.lblCustomDataDirectoriesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmdAddCustomDirectory
            // 
            this.cmdAddCustomDirectory.AutoSize = true;
            this.cmdAddCustomDirectory.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddCustomDirectory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdAddCustomDirectory.Location = new System.Drawing.Point(931, 3);
            this.cmdAddCustomDirectory.Name = "cmdAddCustomDirectory";
            this.cmdAddCustomDirectory.Size = new System.Drawing.Size(126, 23);
            this.cmdAddCustomDirectory.TabIndex = 38;
            this.cmdAddCustomDirectory.Tag = "Button_AddCustomDirectory";
            this.cmdAddCustomDirectory.Text = "Add Directory";
            this.cmdAddCustomDirectory.UseVisualStyleBackColor = true;
            this.cmdAddCustomDirectory.Click += new System.EventHandler(this.cmdAddCustomDirectory_Click);
            // 
            // treCustomDataDirectories
            // 
            this.tlpOptionalRules.SetColumnSpan(this.treCustomDataDirectories, 4);
            this.treCustomDataDirectories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treCustomDataDirectories.Location = new System.Drawing.Point(3, 32);
            this.treCustomDataDirectories.Name = "treCustomDataDirectories";
            this.treCustomDataDirectories.ShowLines = false;
            this.treCustomDataDirectories.ShowPlusMinus = false;
            this.treCustomDataDirectories.ShowRootLines = false;
            this.treCustomDataDirectories.Size = new System.Drawing.Size(1208, 549);
            this.treCustomDataDirectories.TabIndex = 40;
            // 
            // cmdRenameCustomDataDirectory
            // 
            this.cmdRenameCustomDataDirectory.AutoSize = true;
            this.cmdRenameCustomDataDirectory.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRenameCustomDataDirectory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdRenameCustomDataDirectory.Location = new System.Drawing.Point(1063, 3);
            this.cmdRenameCustomDataDirectory.Name = "cmdRenameCustomDataDirectory";
            this.cmdRenameCustomDataDirectory.Size = new System.Drawing.Size(126, 23);
            this.cmdRenameCustomDataDirectory.TabIndex = 41;
            this.cmdRenameCustomDataDirectory.Tag = "Button_RenameCustomDataDirectory";
            this.cmdRenameCustomDataDirectory.Text = "Rename Entry";
            this.cmdRenameCustomDataDirectory.UseVisualStyleBackColor = true;
            this.cmdRenameCustomDataDirectory.Click += new System.EventHandler(this.cmdRenameCustomDataDirectory_Click);
            // 
            // cmdRemoveCustomDirectory
            // 
            this.cmdRemoveCustomDirectory.AutoSize = true;
            this.cmdRemoveCustomDirectory.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRemoveCustomDirectory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdRemoveCustomDirectory.Location = new System.Drawing.Point(1195, 3);
            this.cmdRemoveCustomDirectory.Name = "cmdRemoveCustomDirectory";
            this.cmdRemoveCustomDirectory.Size = new System.Drawing.Size(16, 23);
            this.cmdRemoveCustomDirectory.TabIndex = 39;
            this.cmdRemoveCustomDirectory.Tag = "Button_RemoveCustomDirectory";
            this.cmdRemoveCustomDirectory.Text = "Remove Directory";
            this.cmdRemoveCustomDirectory.UseVisualStyleBackColor = true;
            this.cmdRemoveCustomDirectory.Click += new System.EventHandler(this.cmdRemoveCustomDirectory_Click);
            // 
            // tabGitHubIssues
            // 
            this.tabGitHubIssues.BackColor = System.Drawing.SystemColors.Control;
            this.tabGitHubIssues.Controls.Add(this.cmdUploadPastebin);
            this.tabGitHubIssues.Location = new System.Drawing.Point(4, 22);
            this.tabGitHubIssues.Name = "tabGitHubIssues";
            this.tabGitHubIssues.Padding = new System.Windows.Forms.Padding(3);
            this.tabGitHubIssues.Size = new System.Drawing.Size(1232, 602);
            this.tabGitHubIssues.TabIndex = 4;
            this.tabGitHubIssues.Tag = "Tab_Options_GitHubIssues";
            this.tabGitHubIssues.Text = "GitHub Issues";
            // 
            // cmdUploadPastebin
            // 
            this.cmdUploadPastebin.AutoSize = true;
            this.cmdUploadPastebin.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdUploadPastebin.Enabled = false;
            this.cmdUploadPastebin.Location = new System.Drawing.Point(6, 6);
            this.cmdUploadPastebin.Name = "cmdUploadPastebin";
            this.cmdUploadPastebin.Size = new System.Drawing.Size(123, 23);
            this.cmdUploadPastebin.TabIndex = 1;
            this.cmdUploadPastebin.Tag = "Button_Options_UploadPastebin";
            this.cmdUploadPastebin.Text = "Upload file to Pastebin";
            this.cmdUploadPastebin.UseVisualStyleBackColor = true;
            this.cmdUploadPastebin.Click += new System.EventHandler(this.cmdUploadPastebin_Click);
            // 
            // tabPlugins
            // 
            this.tabPlugins.BackColor = System.Drawing.SystemColors.Control;
            this.tabPlugins.Controls.Add(this.bufferedTableLayoutPanel1);
            this.tabPlugins.Location = new System.Drawing.Point(4, 22);
            this.tabPlugins.Name = "tabPlugins";
            this.tabPlugins.Padding = new System.Windows.Forms.Padding(9);
            this.tabPlugins.Size = new System.Drawing.Size(1232, 602);
            this.tabPlugins.TabIndex = 6;
            this.tabPlugins.Tag = "Tab_Options_Plugins";
            this.tabPlugins.Text = "Plugins";
            // 
            // bufferedTableLayoutPanel1
            // 
            this.bufferedTableLayoutPanel1.AutoSize = true;
            this.bufferedTableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bufferedTableLayoutPanel1.ColumnCount = 2;
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 301F));
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bufferedTableLayoutPanel1.Controls.Add(this.grpAvailablePlugins, 0, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.pnlPluginOption, 1, 0);
            this.bufferedTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedTableLayoutPanel1.Location = new System.Drawing.Point(9, 9);
            this.bufferedTableLayoutPanel1.MinimumSize = new System.Drawing.Size(823, 516);
            this.bufferedTableLayoutPanel1.Name = "bufferedTableLayoutPanel1";
            this.bufferedTableLayoutPanel1.RowCount = 1;
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 584F));
            this.bufferedTableLayoutPanel1.Size = new System.Drawing.Size(1214, 584);
            this.bufferedTableLayoutPanel1.TabIndex = 0;
            // 
            // grpAvailablePlugins
            // 
            this.grpAvailablePlugins.AutoSize = true;
            this.grpAvailablePlugins.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.grpAvailablePlugins.Controls.Add(this.clbPlugins);
            this.grpAvailablePlugins.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpAvailablePlugins.Location = new System.Drawing.Point(3, 3);
            this.grpAvailablePlugins.Name = "grpAvailablePlugins";
            this.grpAvailablePlugins.Size = new System.Drawing.Size(295, 578);
            this.grpAvailablePlugins.TabIndex = 0;
            this.grpAvailablePlugins.TabStop = false;
            this.grpAvailablePlugins.Tag = "String_AvailablePlugins";
            this.grpAvailablePlugins.Text = "Available Plugins";
            // 
            // clbPlugins
            // 
            this.clbPlugins.Dock = System.Windows.Forms.DockStyle.Fill;
            this.clbPlugins.FormattingEnabled = true;
            this.clbPlugins.Location = new System.Drawing.Point(3, 16);
            this.clbPlugins.Name = "clbPlugins";
            this.clbPlugins.Size = new System.Drawing.Size(289, 559);
            this.clbPlugins.TabIndex = 0;
            this.clbPlugins.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clbPlugins_ItemCheck);
            this.clbPlugins.SelectedValueChanged += new System.EventHandler(this.clbPlugins_SelectedValueChanged);
            this.clbPlugins.VisibleChanged += new System.EventHandler(this.clbPlugins_VisibleChanged);
            // 
            // pnlPluginOption
            // 
            this.pnlPluginOption.AutoScroll = true;
            this.pnlPluginOption.AutoSize = true;
            this.pnlPluginOption.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlPluginOption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPluginOption.Location = new System.Drawing.Point(301, 0);
            this.pnlPluginOption.Margin = new System.Windows.Forms.Padding(0);
            this.pnlPluginOption.Name = "pnlPluginOption";
            this.pnlPluginOption.Size = new System.Drawing.Size(913, 584);
            this.pnlPluginOption.TabIndex = 1;
            // 
            // flpOKCancel
            // 
            this.flpOKCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flpOKCancel.AutoSize = true;
            this.flpOKCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpOKCancel.Controls.Add(this.cmdOK);
            this.flpOKCancel.Controls.Add(this.cmdCancel);
            this.flpOKCancel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpOKCancel.Location = new System.Drawing.Point(1077, 637);
            this.flpOKCancel.Name = "flpOKCancel";
            this.flpOKCancel.Size = new System.Drawing.Size(166, 23);
            this.flpOKCancel.TabIndex = 5;
            // 
            // cmdOK
            // 
            this.cmdOK.AutoSize = true;
            this.cmdOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdOK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOK.Location = new System.Drawing.Point(86, 0);
            this.cmdOK.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.cmdOK.MinimumSize = new System.Drawing.Size(80, 0);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(80, 23);
            this.cmdOK.TabIndex = 5;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.AutoSize = true;
            this.cmdCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(0, 0);
            this.cmdCancel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.cmdCancel.MinimumSize = new System.Drawing.Size(80, 0);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(80, 23);
            this.cmdCancel.TabIndex = 6;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // frmOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1264, 681);
            this.Controls.Add(this.tlpOptions);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(48, 50);
            this.Name = "frmOptions";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_Options";
            this.Text = "Options";
            this.Load += new System.EventHandler(this.frmOptions_Load);
            this.tlpOptions.ResumeLayout(false);
            this.tlpOptions.PerformLayout();
            this.tabOptions.ResumeLayout(false);
            this.tabGlobal.ResumeLayout(false);
            this.tabGlobal.PerformLayout();
            this.tlpGlobal.ResumeLayout(false);
            this.tlpGlobal.PerformLayout();
            this.grpSelectedSourcebook.ResumeLayout(false);
            this.grpSelectedSourcebook.PerformLayout();
            this.tlpSelectedSourcebook.ResumeLayout(false);
            this.tlpSelectedSourcebook.PerformLayout();
            this.flpPDFOffset.ResumeLayout(false);
            this.flpPDFOffset.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPDFOffset)).EndInit();
            this.tlpGlobalOptions.ResumeLayout(false);
            this.tlpGlobalOptions.PerformLayout();
            this.flpEnablePlugins.ResumeLayout(false);
            this.flpEnablePlugins.PerformLayout();
            this.tlpGlobalOptionsTop.ResumeLayout(false);
            this.tlpGlobalOptionsTop.PerformLayout();
            this.grpDateFormat.ResumeLayout(false);
            this.grpDateFormat.PerformLayout();
            this.tlpDateFormat.ResumeLayout(false);
            this.tlpDateFormat.PerformLayout();
            this.grpTimeFormat.ResumeLayout(false);
            this.grpTimeFormat.PerformLayout();
            this.tlpTimeFormat.ResumeLayout(false);
            this.tlpTimeFormat.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgSheetLanguageFlag)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgLanguageFlag)).EndInit();
            this.tlpLoggingOptions.ResumeLayout(false);
            this.tlpLoggingOptions.PerformLayout();
            this.tlpCharacterRosterPath.ResumeLayout(false);
            this.tlpCharacterRosterPath.PerformLayout();
            this.tlpPDFAppPath.ResumeLayout(false);
            this.tlpPDFAppPath.PerformLayout();
            this.flpBrowserVersion.ResumeLayout(false);
            this.flpBrowserVersion.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudBrowserVersion)).EndInit();
            this.tlpMugshotCompression.ResumeLayout(false);
            this.tlpMugshotCompression.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMugshotCompressionQuality)).EndInit();
            this.gpbEditSourcebookInfo.ResumeLayout(false);
            this.tabCustomDataDirectories.ResumeLayout(false);
            this.tabCustomDataDirectories.PerformLayout();
            this.tlpOptionalRules.ResumeLayout(false);
            this.tlpOptionalRules.PerformLayout();
            this.tabGitHubIssues.ResumeLayout(false);
            this.tabGitHubIssues.PerformLayout();
            this.tabPlugins.ResumeLayout(false);
            this.tabPlugins.PerformLayout();
            this.bufferedTableLayoutPanel1.ResumeLayout(false);
            this.bufferedTableLayoutPanel1.PerformLayout();
            this.grpAvailablePlugins.ResumeLayout(false);
            this.flpOKCancel.ResumeLayout(false);
            this.flpOKCancel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BufferedTableLayoutPanel tlpOptions;
        private System.Windows.Forms.TabControl tabOptions;
        private System.Windows.Forms.TabPage tabGlobal;
        private BufferedTableLayoutPanel tlpGlobal;
        private System.Windows.Forms.GroupBox grpSelectedSourcebook;
        private BufferedTableLayoutPanel tlpSelectedSourcebook;
        private System.Windows.Forms.TextBox txtPDFLocation;
        private System.Windows.Forms.Label lblPDFLocation;
        private System.Windows.Forms.Button cmdPDFLocation;
        private System.Windows.Forms.Label lblPDFOffset;
        private System.Windows.Forms.FlowLayoutPanel flpPDFOffset;
        private System.Windows.Forms.NumericUpDown nudPDFOffset;
        private System.Windows.Forms.Button cmdPDFTest;
        private System.Windows.Forms.TableLayoutPanel tlpGlobalOptions;
        private System.Windows.Forms.CheckBox chkHideCharts;
        private System.Windows.Forms.Label lblPDFParametersLabel;
        private System.Windows.Forms.CheckBox chkHideMasterIndex;
        private System.Windows.Forms.CheckBox chkHideCharacterRoster;
        private System.Windows.Forms.GroupBox grpTimeFormat;
        private BufferedTableLayoutPanel tlpTimeFormat;
        private System.Windows.Forms.TextBox txtTimeFormat;
        private System.Windows.Forms.TextBox txtTimeFormatView;
        private System.Windows.Forms.CheckBox chkSearchInCategoryOnly;
        private System.Windows.Forms.CheckBox chkAllowEasterEggs;
        private System.Windows.Forms.ComboBox cboDefaultCharacterOption;
        private System.Windows.Forms.NumericUpDown nudBrowserVersion;
        private System.Windows.Forms.CheckBox chkPreferNightlyBuilds;
        private System.Windows.Forms.Label lblBrowserVersion;
        private System.Windows.Forms.CheckBox chkLiveUpdateCleanCharacterFiles;
        private System.Windows.Forms.Label lblLanguage;
        private System.Windows.Forms.CheckBox chkDatesIncludeTime;
        private System.Windows.Forms.CheckBox chkLiveCustomData;
        private System.Windows.Forms.CheckBox chkSingleDiceRoller;
        private System.Windows.Forms.CheckBox chkStartupFullscreen;
        private System.Windows.Forms.Label lblXSLT;
        private System.Windows.Forms.PictureBox imgLanguageFlag;
        private System.Windows.Forms.CheckBox chkLifeModule;
        private System.Windows.Forms.CheckBox chkAutomaticUpdate;
        private System.Windows.Forms.CheckBox chkAllowHoverIncrement;
        private System.Windows.Forms.CheckBox chkUseLogging;
        private System.Windows.Forms.PictureBox imgSheetLanguageFlag;
        private ElasticComboBox cboSheetLanguage;
        private ElasticComboBox cboLanguage;
        private System.Windows.Forms.Button cmdVerify;
        private System.Windows.Forms.Button cmdVerifyData;
        private ElasticComboBox cboXSLT;
        private System.Windows.Forms.CheckBox chkCustomDateTimeFormats;
        private System.Windows.Forms.CheckBox chkConfirmKarmaExpense;
        private System.Windows.Forms.CheckBox chkConfirmDelete;
        private System.Windows.Forms.ComboBox cboUseLoggingApplicationInsights;
        private ButtonWithToolTip cmdUseLoggingHelp;
        private System.Windows.Forms.CheckBox chkHideItemsOverAvail;
        private System.Windows.Forms.FlowLayoutPanel flpEnablePlugins;
        private System.Windows.Forms.CheckBox chkEnablePlugins;
        private System.Windows.Forms.Button cmdPluginsHelp;
        private System.Windows.Forms.TextBox txtCharacterRosterPath;
        private System.Windows.Forms.Button cmdCharacterRoster;
        private System.Windows.Forms.GroupBox grpDateFormat;
        private BufferedTableLayoutPanel tlpDateFormat;
        private System.Windows.Forms.TextBox txtDateFormat;
        private System.Windows.Forms.TextBox txtDateFormatView;
        private System.Windows.Forms.CheckBox chkCreateBackupOnCareer;
        private System.Windows.Forms.GroupBox gpbEditSourcebookInfo;
        private System.Windows.Forms.ListBox lstGlobalSourcebookInfos;
        private System.Windows.Forms.TabPage tabCustomDataDirectories;
        private BufferedTableLayoutPanel tlpOptionalRules;
        private System.Windows.Forms.Label lblCustomDataDirectoriesLabel;
        private System.Windows.Forms.Button cmdAddCustomDirectory;
        private System.Windows.Forms.TreeView treCustomDataDirectories;
        private System.Windows.Forms.Button cmdRenameCustomDataDirectory;
        private System.Windows.Forms.Button cmdRemoveCustomDirectory;
        private System.Windows.Forms.TabPage tabGitHubIssues;
        private System.Windows.Forms.Button cmdUploadPastebin;
        private System.Windows.Forms.TabPage tabPlugins;
        private BufferedTableLayoutPanel bufferedTableLayoutPanel1;
        private System.Windows.Forms.GroupBox grpAvailablePlugins;
        private System.Windows.Forms.CheckedListBox clbPlugins;
        private System.Windows.Forms.Panel pnlPluginOption;
        private System.Windows.Forms.FlowLayoutPanel flpOKCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.CheckBox chkAllowSkillDiceRolling;
        private BufferedTableLayoutPanel tlpCharacterRosterPath;
        private System.Windows.Forms.CheckBox chkPrintToFileFirst;
        private BufferedTableLayoutPanel tlpMugshotCompression;
        private System.Windows.Forms.Label lblMugshotCompressionQuality;
        private System.Windows.Forms.NumericUpDown nudMugshotCompressionQuality;
        private ElasticComboBox cboMugshotCompression;
        private System.Windows.Forms.Label lblPDFAppPath;
        private BufferedTableLayoutPanel tlpGlobalOptionsTop;
        private BufferedTableLayoutPanel tlpLoggingOptions;
        private System.Windows.Forms.Label lblDefaultCharacterOption;
        private System.Windows.Forms.Label lblCharacterRosterLabel;
        private System.Windows.Forms.Label lblMugshotCompression;
        private ElasticComboBox cboPDFParameters;
        private BufferedTableLayoutPanel tlpPDFAppPath;
        private System.Windows.Forms.TextBox txtPDFAppPath;
        private System.Windows.Forms.Button cmdPDFAppPath;
        private System.Windows.Forms.FlowLayoutPanel flpBrowserVersion;
    }
}
