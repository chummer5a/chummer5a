namespace ChummerHub.Client.UI
{
    partial class ucSINnersOptions
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
                frmWebBrowser?.Dispose();
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
            GroupControls.RadioButtonListItem radioButtonListItem1 = new GroupControls.RadioButtonListItem();
            GroupControls.RadioButtonListItem radioButtonListItem2 = new GroupControls.RadioButtonListItem();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ucSINnersOptions));
            this.tlpAllOptions = new System.Windows.Forms.TableLayoutPanel();
            this.gpRadioOnlyPublic = new System.Windows.Forms.GroupBox();
            this.rbListUserMode = new GroupControls.RadioButtonList();
            this.gbRegisteredMode = new System.Windows.Forms.GroupBox();
            this.tlpOptions = new System.Windows.Forms.TableLayoutPanel();
            this.gbVisibility = new System.Windows.Forms.GroupBox();
            this.tlpVisibility = new System.Windows.Forms.TableLayoutPanel();
            this.cbVisibilityIsPublic = new System.Windows.Forms.CheckBox();
            this.bEditDefaultVisibility = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.bTempPathBrowse = new System.Windows.Forms.Button();
            this.lSINnerUrl = new System.Windows.Forms.Label();
            this.cbSINnerUrl = new System.Windows.Forms.ComboBox();
            this.tbHelptext = new System.Windows.Forms.TextBox();
            this.cbUploadOnSave = new System.Windows.Forms.CheckBox();
            this.bMultiUpload = new System.Windows.Forms.Button();
            this.bLogin = new System.Windows.Forms.Button();
            this.tlpAccount = new System.Windows.Forms.TableLayoutPanel();
            this.cbRoles = new System.Windows.Forms.ComboBox();
            this.lUsername = new System.Windows.Forms.Label();
            this.tlpTempFolder = new System.Windows.Forms.TableLayoutPanel();
            this.tbTempDownloadPath = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.gpPublicMode = new System.Windows.Forms.GroupBox();
            this.tlpPublicMode = new System.Windows.Forms.TableLayoutPanel();
            this.cbIgnoreWarnings = new System.Windows.Forms.CheckBox();
            this.cbOpenChummerFromSharedLinks = new System.Windows.Forms.CheckBox();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.backgroundWorker2 = new System.ComponentModel.BackgroundWorker();
            this.tlpAllOptions.SuspendLayout();
            this.gpRadioOnlyPublic.SuspendLayout();
            this.rbListUserMode.SuspendLayout();
            this.gbRegisteredMode.SuspendLayout();
            this.tlpOptions.SuspendLayout();
            this.gbVisibility.SuspendLayout();
            this.tlpVisibility.SuspendLayout();
            this.tlpAccount.SuspendLayout();
            this.tlpTempFolder.SuspendLayout();
            this.gpPublicMode.SuspendLayout();
            this.tlpPublicMode.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpAllOptions
            // 
            this.tlpAllOptions.AutoSize = true;
            this.tlpAllOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpAllOptions.ColumnCount = 2;
            this.tlpAllOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 350F));
            this.tlpAllOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpAllOptions.Controls.Add(this.gpRadioOnlyPublic, 0, 0);
            this.tlpAllOptions.Controls.Add(this.gbRegisteredMode, 0, 1);
            this.tlpAllOptions.Controls.Add(this.gpPublicMode, 1, 0);
            this.tlpAllOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpAllOptions.Location = new System.Drawing.Point(0, 0);
            this.tlpAllOptions.Name = "tlpAllOptions";
            this.tlpAllOptions.RowCount = 2;
            this.tlpAllOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tlpAllOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpAllOptions.Size = new System.Drawing.Size(673, 484);
            this.tlpAllOptions.TabIndex = 0;
            // 
            // gpRadioOnlyPublic
            // 
            this.gpRadioOnlyPublic.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpRadioOnlyPublic.Controls.Add(this.rbListUserMode);
            this.gpRadioOnlyPublic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpRadioOnlyPublic.Location = new System.Drawing.Point(3, 3);
            this.gpRadioOnlyPublic.Name = "gpRadioOnlyPublic";
            this.gpRadioOnlyPublic.Size = new System.Drawing.Size(344, 94);
            this.gpRadioOnlyPublic.TabIndex = 3;
            this.gpRadioOnlyPublic.TabStop = false;
            this.gpRadioOnlyPublic.Text = "Anonymous or Registered User";
            // 
            // rbListUserMode
            // 
            this.rbListUserMode.AutoScrollMinSize = new System.Drawing.Size(338, 68);
            this.rbListUserMode.Dock = System.Windows.Forms.DockStyle.Fill;
            radioButtonListItem1.Checked = true;
            radioButtonListItem1.Subtext = "Only use functions available for non-registered users";
            radioButtonListItem1.Tag = "public";
            radioButtonListItem1.Text = "Public Mode";
            radioButtonListItem1.ToolTipText = "";
            radioButtonListItem2.Subtext = "Use enhanced functionality (requires registration)";
            radioButtonListItem2.Tag = "registered";
            radioButtonListItem2.Text = "Registered Mode";
            this.rbListUserMode.Items.AddRange(new GroupControls.RadioButtonListItem[] {
            radioButtonListItem1,
            radioButtonListItem2});
            this.rbListUserMode.Location = new System.Drawing.Point(3, 16);
            this.rbListUserMode.Name = "rbListUserMode";
            this.rbListUserMode.Size = new System.Drawing.Size(338, 68);
            this.rbListUserMode.TabIndex = 0;
            // 
            // gbRegisteredMode
            // 
            this.gbRegisteredMode.AutoSize = true;
            this.gbRegisteredMode.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpAllOptions.SetColumnSpan(this.gbRegisteredMode, 2);
            this.gbRegisteredMode.Controls.Add(this.tlpOptions);
            this.gbRegisteredMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbRegisteredMode.Location = new System.Drawing.Point(3, 103);
            this.gbRegisteredMode.Name = "gbRegisteredMode";
            this.gbRegisteredMode.Size = new System.Drawing.Size(667, 378);
            this.gbRegisteredMode.TabIndex = 4;
            this.gbRegisteredMode.TabStop = false;
            this.gbRegisteredMode.Text = "Registered Mode";
            // 
            // tlpOptions
            // 
            this.tlpOptions.AutoSize = true;
            this.tlpOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpOptions.ColumnCount = 5;
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpOptions.Controls.Add(this.gbVisibility, 0, 2);
            this.tlpOptions.Controls.Add(this.label1, 0, 0);
            this.tlpOptions.Controls.Add(this.bTempPathBrowse, 2, 6);
            this.tlpOptions.Controls.Add(this.lSINnerUrl, 0, 1);
            this.tlpOptions.Controls.Add(this.cbSINnerUrl, 1, 1);
            this.tlpOptions.Controls.Add(this.tbHelptext, 0, 7);
            this.tlpOptions.Controls.Add(this.cbUploadOnSave, 0, 4);
            this.tlpOptions.Controls.Add(this.bMultiUpload, 2, 2);
            this.tlpOptions.Controls.Add(this.bLogin, 2, 0);
            this.tlpOptions.Controls.Add(this.tlpAccount, 1, 0);
            this.tlpOptions.Controls.Add(this.tlpTempFolder, 0, 6);
            this.tlpOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpOptions.Location = new System.Drawing.Point(3, 16);
            this.tlpOptions.Name = "tlpOptions";
            this.tlpOptions.RowCount = 7;
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.Size = new System.Drawing.Size(661, 359);
            this.tlpOptions.TabIndex = 2;
            // 
            // gbVisibility
            // 
            this.gbVisibility.AutoSize = true;
            this.gbVisibility.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpOptions.SetColumnSpan(this.gbVisibility, 2);
            this.gbVisibility.Controls.Add(this.tlpVisibility);
            this.gbVisibility.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbVisibility.Location = new System.Drawing.Point(3, 59);
            this.gbVisibility.Name = "gbVisibility";
            this.gbVisibility.Size = new System.Drawing.Size(401, 48);
            this.gbVisibility.TabIndex = 7;
            this.gbVisibility.TabStop = false;
            this.gbVisibility.Text = "Visibility of Uploaded SINners";
            // 
            // tlpVisibility
            // 
            this.tlpVisibility.AutoSize = true;
            this.tlpVisibility.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpVisibility.ColumnCount = 2;
            this.tlpVisibility.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpVisibility.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpVisibility.Controls.Add(this.cbVisibilityIsPublic, 0, 0);
            this.tlpVisibility.Controls.Add(this.bEditDefaultVisibility, 1, 0);
            this.tlpVisibility.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpVisibility.Location = new System.Drawing.Point(3, 16);
            this.tlpVisibility.Name = "tlpVisibility";
            this.tlpVisibility.RowCount = 1;
            this.tlpVisibility.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVisibility.Size = new System.Drawing.Size(395, 29);
            this.tlpVisibility.TabIndex = 0;
            // 
            // cbVisibilityIsPublic
            // 
            this.cbVisibilityIsPublic.AutoSize = true;
            this.cbVisibilityIsPublic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbVisibilityIsPublic.Location = new System.Drawing.Point(3, 3);
            this.cbVisibilityIsPublic.Name = "cbVisibilityIsPublic";
            this.cbVisibilityIsPublic.Size = new System.Drawing.Size(182, 23);
            this.cbVisibilityIsPublic.TabIndex = 0;
            this.cbVisibilityIsPublic.Text = "Discoverable (Upcoming Search)";
            this.cbVisibilityIsPublic.UseVisualStyleBackColor = true;
            // 
            // bEditDefaultVisibility
            // 
            this.bEditDefaultVisibility.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.bEditDefaultVisibility.AutoSize = true;
            this.bEditDefaultVisibility.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bEditDefaultVisibility.Location = new System.Drawing.Point(191, 3);
            this.bEditDefaultVisibility.Name = "bEditDefaultVisibility";
            this.bEditDefaultVisibility.Size = new System.Drawing.Size(201, 23);
            this.bEditDefaultVisibility.TabIndex = 2;
            this.bEditDefaultVisibility.Text = "Set Defaults for All Users";
            this.bEditDefaultVisibility.UseVisualStyleBackColor = true;
            this.bEditDefaultVisibility.Click += new System.EventHandler(this.BEditDefaultVisibility_Click);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Account:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // bTempPathBrowse
            // 
            this.bTempPathBrowse.AutoSize = true;
            this.bTempPathBrowse.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bTempPathBrowse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bTempPathBrowse.Location = new System.Drawing.Point(410, 136);
            this.bTempPathBrowse.Name = "bTempPathBrowse";
            this.bTempPathBrowse.Size = new System.Drawing.Size(248, 23);
            this.bTempPathBrowse.TabIndex = 15;
            this.bTempPathBrowse.Text = "Browse";
            this.bTempPathBrowse.UseVisualStyleBackColor = true;
            this.bTempPathBrowse.Click += new System.EventHandler(this.BTempPathBrowse_Click);
            // 
            // lSINnerUrl
            // 
            this.lSINnerUrl.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lSINnerUrl.AutoSize = true;
            this.lSINnerUrl.Location = new System.Drawing.Point(30, 36);
            this.lSINnerUrl.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lSINnerUrl.Name = "lSINnerUrl";
            this.lSINnerUrl.Size = new System.Drawing.Size(23, 13);
            this.lSINnerUrl.TabIndex = 4;
            this.lSINnerUrl.Text = "Url:";
            this.lSINnerUrl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cbSINnerUrl
            // 
            this.cbSINnerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cbSINnerUrl.FormattingEnabled = true;
            this.cbSINnerUrl.Location = new System.Drawing.Point(59, 32);
            this.cbSINnerUrl.MinimumSize = new System.Drawing.Size(200, 0);
            this.cbSINnerUrl.Name = "cbSINnerUrl";
            this.cbSINnerUrl.Size = new System.Drawing.Size(345, 21);
            this.cbSINnerUrl.TabIndex = 5;
            this.cbSINnerUrl.SelectedIndexChanged += new System.EventHandler(this.CbSINnerUrl_SelectedValueChanged);
            // 
            // tbHelptext
            // 
            this.tbHelptext.BackColor = System.Drawing.SystemColors.Control;
            this.tlpOptions.SetColumnSpan(this.tbHelptext, 3);
            this.tbHelptext.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbHelptext.Location = new System.Drawing.Point(3, 165);
            this.tbHelptext.Multiline = true;
            this.tbHelptext.Name = "tbHelptext";
            this.tbHelptext.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.tbHelptext.Size = new System.Drawing.Size(655, 191);
            this.tbHelptext.TabIndex = 8;
            this.tbHelptext.Text = resources.GetString("tbHelptext.Text");
            // 
            // cbUploadOnSave
            // 
            this.cbUploadOnSave.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cbUploadOnSave.AutoSize = true;
            this.tlpOptions.SetColumnSpan(this.cbUploadOnSave, 3);
            this.cbUploadOnSave.Location = new System.Drawing.Point(3, 113);
            this.cbUploadOnSave.Name = "cbUploadOnSave";
            this.cbUploadOnSave.Size = new System.Drawing.Size(241, 17);
            this.cbUploadOnSave.TabIndex = 10;
            this.cbUploadOnSave.Text = "Upload on Save Automatically (\"onlinemode\")";
            this.cbUploadOnSave.UseVisualStyleBackColor = true;
            // 
            // bMultiUpload
            // 
            this.bMultiUpload.AutoSize = true;
            this.bMultiUpload.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bMultiUpload.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bMultiUpload.Location = new System.Drawing.Point(410, 59);
            this.bMultiUpload.Name = "bMultiUpload";
            this.bMultiUpload.Size = new System.Drawing.Size(248, 48);
            this.bMultiUpload.TabIndex = 9;
            this.bMultiUpload.Text = "Multi-Upload";
            this.bMultiUpload.UseVisualStyleBackColor = true;
            this.bMultiUpload.Click += new System.EventHandler(this.bMultiUpload_Click);
            // 
            // bLogin
            // 
            this.bLogin.AutoSize = true;
            this.bLogin.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bLogin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bLogin.Location = new System.Drawing.Point(410, 3);
            this.bLogin.Name = "bLogin";
            this.bLogin.Size = new System.Drawing.Size(248, 23);
            this.bLogin.TabIndex = 0;
            this.bLogin.Text = "Login";
            this.bLogin.UseVisualStyleBackColor = true;
            this.bLogin.Click += new System.EventHandler(this.bLogin_ClickAsync);
            // 
            // tlpAccount
            // 
            this.tlpAccount.AutoSize = true;
            this.tlpAccount.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpAccount.ColumnCount = 2;
            this.tlpAccount.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpAccount.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpAccount.Controls.Add(this.cbRoles, 1, 0);
            this.tlpAccount.Controls.Add(this.lUsername, 0, 0);
            this.tlpAccount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpAccount.Location = new System.Drawing.Point(56, 0);
            this.tlpAccount.Margin = new System.Windows.Forms.Padding(0);
            this.tlpAccount.Name = "tlpAccount";
            this.tlpAccount.RowCount = 1;
            this.tlpAccount.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpAccount.Size = new System.Drawing.Size(351, 29);
            this.tlpAccount.TabIndex = 18;
            // 
            // cbRoles
            // 
            this.cbRoles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cbRoles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbRoles.FormattingEnabled = true;
            this.cbRoles.Location = new System.Drawing.Point(38, 4);
            this.cbRoles.Name = "cbRoles";
            this.cbRoles.Size = new System.Drawing.Size(310, 21);
            this.cbRoles.TabIndex = 16;
            // 
            // lUsername
            // 
            this.lUsername.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lUsername.AutoSize = true;
            this.lUsername.Location = new System.Drawing.Point(3, 8);
            this.lUsername.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lUsername.Name = "lUsername";
            this.lUsername.Size = new System.Drawing.Size(29, 13);
            this.lUsername.TabIndex = 17;
            this.lUsername.Text = "User";
            this.lUsername.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tlpTempFolder
            // 
            this.tlpTempFolder.AutoSize = true;
            this.tlpTempFolder.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpTempFolder.ColumnCount = 2;
            this.tlpOptions.SetColumnSpan(this.tlpTempFolder, 2);
            this.tlpTempFolder.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpTempFolder.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTempFolder.Controls.Add(this.tbTempDownloadPath, 1, 0);
            this.tlpTempFolder.Controls.Add(this.label3, 0, 0);
            this.tlpTempFolder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTempFolder.Location = new System.Drawing.Point(0, 133);
            this.tlpTempFolder.Margin = new System.Windows.Forms.Padding(0);
            this.tlpTempFolder.Name = "tlpTempFolder";
            this.tlpTempFolder.RowCount = 1;
            this.tlpTempFolder.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTempFolder.Size = new System.Drawing.Size(407, 29);
            this.tlpTempFolder.TabIndex = 19;
            // 
            // tbTempDownloadPath
            // 
            this.tbTempDownloadPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbTempDownloadPath.Location = new System.Drawing.Point(96, 4);
            this.tbTempDownloadPath.Name = "tbTempDownloadPath";
            this.tbTempDownloadPath.ReadOnly = true;
            this.tbTempDownloadPath.Size = new System.Drawing.Size(308, 20);
            this.tbTempDownloadPath.TabIndex = 14;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 8);
            this.label3.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Download Folder";
            // 
            // gpPublicMode
            // 
            this.gpPublicMode.AutoSize = true;
            this.gpPublicMode.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpPublicMode.Controls.Add(this.tlpPublicMode);
            this.gpPublicMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpPublicMode.Location = new System.Drawing.Point(353, 3);
            this.gpPublicMode.Name = "gpPublicMode";
            this.gpPublicMode.Size = new System.Drawing.Size(317, 94);
            this.gpPublicMode.TabIndex = 5;
            this.gpPublicMode.TabStop = false;
            this.gpPublicMode.Text = "Public Mode";
            // 
            // tlpPublicMode
            // 
            this.tlpPublicMode.AutoSize = true;
            this.tlpPublicMode.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpPublicMode.ColumnCount = 2;
            this.tlpPublicMode.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpPublicMode.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpPublicMode.Controls.Add(this.cbIgnoreWarnings, 0, 0);
            this.tlpPublicMode.Controls.Add(this.cbOpenChummerFromSharedLinks, 0, 1);
            this.tlpPublicMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPublicMode.Location = new System.Drawing.Point(3, 16);
            this.tlpPublicMode.Name = "tlpPublicMode";
            this.tlpPublicMode.RowCount = 2;
            this.tlpPublicMode.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpPublicMode.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpPublicMode.Size = new System.Drawing.Size(311, 75);
            this.tlpPublicMode.TabIndex = 0;
            // 
            // cbIgnoreWarnings
            // 
            this.cbIgnoreWarnings.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cbIgnoreWarnings.AutoSize = true;
            this.tlpPublicMode.SetColumnSpan(this.cbIgnoreWarnings, 2);
            this.cbIgnoreWarnings.Location = new System.Drawing.Point(3, 10);
            this.cbIgnoreWarnings.Name = "cbIgnoreWarnings";
            this.cbIgnoreWarnings.Size = new System.Drawing.Size(237, 17);
            this.cbIgnoreWarnings.TabIndex = 0;
            this.cbIgnoreWarnings.Text = "Ignore Warnings When Opening a Character";
            this.cbIgnoreWarnings.UseVisualStyleBackColor = true;
            // 
            // cbOpenChummerFromSharedLinks
            // 
            this.cbOpenChummerFromSharedLinks.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cbOpenChummerFromSharedLinks.AutoSize = true;
            this.tlpPublicMode.SetColumnSpan(this.cbOpenChummerFromSharedLinks, 2);
            this.cbOpenChummerFromSharedLinks.Location = new System.Drawing.Point(3, 47);
            this.cbOpenChummerFromSharedLinks.Name = "cbOpenChummerFromSharedLinks";
            this.cbOpenChummerFromSharedLinks.Size = new System.Drawing.Size(187, 17);
            this.cbOpenChummerFromSharedLinks.TabIndex = 1;
            this.cbOpenChummerFromSharedLinks.Text = "Open Chummer from Shared Links";
            this.cbOpenChummerFromSharedLinks.UseVisualStyleBackColor = true;
            // 
            // ucSINnersOptions
            // 
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tlpAllOptions);
            this.Name = "ucSINnersOptions";
            this.Size = new System.Drawing.Size(673, 484);
            this.tlpAllOptions.ResumeLayout(false);
            this.tlpAllOptions.PerformLayout();
            this.gpRadioOnlyPublic.ResumeLayout(false);
            this.gpRadioOnlyPublic.PerformLayout();
            this.rbListUserMode.ResumeLayout(true);
            this.gbRegisteredMode.ResumeLayout(false);
            this.gbRegisteredMode.PerformLayout();
            this.tlpOptions.ResumeLayout(false);
            this.tlpOptions.PerformLayout();
            this.gbVisibility.ResumeLayout(false);
            this.gbVisibility.PerformLayout();
            this.tlpVisibility.ResumeLayout(false);
            this.tlpVisibility.PerformLayout();
            this.tlpAccount.ResumeLayout(false);
            this.tlpAccount.PerformLayout();
            this.tlpTempFolder.ResumeLayout(false);
            this.tlpTempFolder.PerformLayout();
            this.gpPublicMode.ResumeLayout(false);
            this.gpPublicMode.PerformLayout();
            this.tlpPublicMode.ResumeLayout(false);
            this.tlpPublicMode.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpAllOptions;
        private System.Windows.Forms.TableLayoutPanel tlpOptions;
        private System.Windows.Forms.GroupBox gbVisibility;
        private System.Windows.Forms.TableLayoutPanel tlpVisibility;
        private System.Windows.Forms.CheckBox cbVisibilityIsPublic;
        private System.Windows.Forms.Button bEditDefaultVisibility;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button bTempPathBrowse;
        private System.Windows.Forms.Label lSINnerUrl;
        private System.Windows.Forms.ComboBox cbSINnerUrl;
        private System.Windows.Forms.TextBox tbHelptext;
        private System.Windows.Forms.CheckBox cbUploadOnSave;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbTempDownloadPath;
        private System.Windows.Forms.Button bMultiUpload;
        private System.Windows.Forms.Button bLogin;
        private System.Windows.Forms.TableLayoutPanel tlpAccount;
        private System.Windows.Forms.ComboBox cbRoles;
        private System.Windows.Forms.Label lUsername;
        private System.Windows.Forms.GroupBox gpRadioOnlyPublic;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private GroupControls.RadioButtonList rbListUserMode;
        private System.Windows.Forms.GroupBox gbRegisteredMode;
        private System.Windows.Forms.GroupBox gpPublicMode;
        private System.ComponentModel.BackgroundWorker backgroundWorker2;
        private System.Windows.Forms.TableLayoutPanel tlpPublicMode;
        private System.Windows.Forms.CheckBox cbIgnoreWarnings;
        private System.Windows.Forms.CheckBox cbOpenChummerFromSharedLinks;
        private System.Windows.Forms.TableLayoutPanel tlpTempFolder;
    }
}
