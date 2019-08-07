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
            this.flpTempFolder = new System.Windows.Forms.FlowLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.tbTempDownloadPath = new System.Windows.Forms.TextBox();
            this.bMultiUpload = new System.Windows.Forms.Button();
            this.bLogin = new System.Windows.Forms.Button();
            this.tlpAccount = new System.Windows.Forms.TableLayoutPanel();
            this.cbRoles = new System.Windows.Forms.ComboBox();
            this.lUsername = new System.Windows.Forms.Label();
            this.gpPublicMode = new System.Windows.Forms.GroupBox();
            this.tlpPublicMode = new System.Windows.Forms.TableLayoutPanel();
            this.bRegisterUriScheme = new System.Windows.Forms.Button();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.backgroundWorker2 = new System.ComponentModel.BackgroundWorker();
            this.tlpAllOptions.SuspendLayout();
            this.gpRadioOnlyPublic.SuspendLayout();
            this.rbListUserMode.SuspendLayout();
            this.gbRegisteredMode.SuspendLayout();
            this.tlpOptions.SuspendLayout();
            this.gbVisibility.SuspendLayout();
            this.tlpVisibility.SuspendLayout();
            this.flpTempFolder.SuspendLayout();
            this.tlpAccount.SuspendLayout();
            this.gpPublicMode.SuspendLayout();
            this.tlpPublicMode.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpAllOptions
            // 
            this.tlpAllOptions.AutoSize = true;
            this.tlpAllOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpAllOptions.ColumnCount = 2;
            this.tlpAllOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 525F));
            this.tlpAllOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpAllOptions.Controls.Add(this.gpRadioOnlyPublic, 0, 0);
            this.tlpAllOptions.Controls.Add(this.gbRegisteredMode, 0, 1);
            this.tlpAllOptions.Controls.Add(this.gpPublicMode, 1, 0);
            this.tlpAllOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpAllOptions.Location = new System.Drawing.Point(0, 0);
            this.tlpAllOptions.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tlpAllOptions.Name = "tlpAllOptions";
            this.tlpAllOptions.RowCount = 2;
            this.tlpAllOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 154F));
            this.tlpAllOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpAllOptions.Size = new System.Drawing.Size(981, 776);
            this.tlpAllOptions.TabIndex = 0;
            // 
            // gpRadioOnlyPublic
            // 
            this.gpRadioOnlyPublic.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpRadioOnlyPublic.Controls.Add(this.rbListUserMode);
            this.gpRadioOnlyPublic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpRadioOnlyPublic.Location = new System.Drawing.Point(4, 5);
            this.gpRadioOnlyPublic.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gpRadioOnlyPublic.Name = "gpRadioOnlyPublic";
            this.gpRadioOnlyPublic.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gpRadioOnlyPublic.Size = new System.Drawing.Size(517, 144);
            this.gpRadioOnlyPublic.TabIndex = 3;
            this.gpRadioOnlyPublic.TabStop = false;
            this.gpRadioOnlyPublic.Text = "Anonymous or Registered User";
            // 
            // rbListUserMode
            // 
            this.rbListUserMode.AutoScrollMinSize = new System.Drawing.Size(509, 96);
            this.rbListUserMode.Dock = System.Windows.Forms.DockStyle.Fill;
            radioButtonListItem1.Checked = true;
            radioButtonListItem1.Subtext = "use only functions available for none registered users";
            radioButtonListItem1.Tag = "public";
            radioButtonListItem1.Text = "Public Mode";
            radioButtonListItem1.ToolTipText = "";
            radioButtonListItem2.Subtext = "use enchanced functionality (requires registration)";
            radioButtonListItem2.Tag = "registered";
            radioButtonListItem2.Text = "Registered Mode";
            this.rbListUserMode.Items.AddRange(new GroupControls.RadioButtonListItem[] {
            radioButtonListItem1,
            radioButtonListItem2});
            this.rbListUserMode.Location = new System.Drawing.Point(4, 24);
            this.rbListUserMode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rbListUserMode.Name = "rbListUserMode";
            this.rbListUserMode.Size = new System.Drawing.Size(509, 115);
            this.rbListUserMode.TabIndex = 0;
            // 
            // gbRegisteredMode
            // 
            this.gbRegisteredMode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbRegisteredMode.AutoSize = true;
            this.gbRegisteredMode.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpAllOptions.SetColumnSpan(this.gbRegisteredMode, 2);
            this.gbRegisteredMode.Controls.Add(this.tlpOptions);
            this.gbRegisteredMode.Location = new System.Drawing.Point(4, 159);
            this.gbRegisteredMode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbRegisteredMode.Name = "gbRegisteredMode";
            this.gbRegisteredMode.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbRegisteredMode.Size = new System.Drawing.Size(973, 612);
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
            this.tlpOptions.Controls.Add(this.flpTempFolder, 0, 6);
            this.tlpOptions.Controls.Add(this.bMultiUpload, 2, 2);
            this.tlpOptions.Controls.Add(this.bLogin, 2, 0);
            this.tlpOptions.Controls.Add(this.tlpAccount, 1, 0);
            this.tlpOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpOptions.Location = new System.Drawing.Point(4, 24);
            this.tlpOptions.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
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
            this.tlpOptions.Size = new System.Drawing.Size(965, 583);
            this.tlpOptions.TabIndex = 2;
            // 
            // gbVisibility
            // 
            this.gbVisibility.AutoSize = true;
            this.gbVisibility.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpOptions.SetColumnSpan(this.gbVisibility, 2);
            this.gbVisibility.Controls.Add(this.tlpVisibility);
            this.gbVisibility.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbVisibility.Location = new System.Drawing.Point(4, 105);
            this.gbVisibility.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbVisibility.MaximumSize = new System.Drawing.Size(600, 154);
            this.gbVisibility.MinimumSize = new System.Drawing.Size(150, 0);
            this.gbVisibility.Name = "gbVisibility";
            this.gbVisibility.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbVisibility.Size = new System.Drawing.Size(596, 91);
            this.gbVisibility.TabIndex = 7;
            this.gbVisibility.TabStop = false;
            this.gbVisibility.Text = "Visibility of uploaded SINner";
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
            this.tlpVisibility.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpVisibility.Location = new System.Drawing.Point(4, 24);
            this.tlpVisibility.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tlpVisibility.MinimumSize = new System.Drawing.Size(60, 62);
            this.tlpVisibility.Name = "tlpVisibility";
            this.tlpVisibility.RowCount = 1;
            this.tlpVisibility.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVisibility.Size = new System.Drawing.Size(588, 62);
            this.tlpVisibility.TabIndex = 0;
            // 
            // cbVisibilityIsPublic
            // 
            this.cbVisibilityIsPublic.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbVisibilityIsPublic.AutoSize = true;
            this.cbVisibilityIsPublic.Location = new System.Drawing.Point(4, 5);
            this.cbVisibilityIsPublic.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbVisibilityIsPublic.Name = "cbVisibilityIsPublic";
            this.cbVisibilityIsPublic.Size = new System.Drawing.Size(258, 52);
            this.cbVisibilityIsPublic.TabIndex = 0;
            this.cbVisibilityIsPublic.Text = "discoverable (upcoming search)";
            this.cbVisibilityIsPublic.UseVisualStyleBackColor = true;
            // 
            // bEditDefaultVisibility
            // 
            this.bEditDefaultVisibility.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.bEditDefaultVisibility.Location = new System.Drawing.Point(270, 13);
            this.bEditDefaultVisibility.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.bEditDefaultVisibility.Name = "bEditDefaultVisibility";
            this.bEditDefaultVisibility.Size = new System.Drawing.Size(314, 35);
            this.bEditDefaultVisibility.TabIndex = 2;
            this.bEditDefaultVisibility.Text = "set default Users";
            this.bEditDefaultVisibility.UseVisualStyleBackColor = true;
            this.bEditDefaultVisibility.Click += new System.EventHandler(this.BEditDefaultVisibility_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(4, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 62);
            this.label1.TabIndex = 2;
            this.label1.Text = "Account:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // bTempPathBrowse
            // 
            this.bTempPathBrowse.AutoSize = true;
            this.bTempPathBrowse.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bTempPathBrowse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bTempPathBrowse.Location = new System.Drawing.Point(608, 240);
            this.bTempPathBrowse.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.bTempPathBrowse.Name = "bTempPathBrowse";
            this.bTempPathBrowse.Size = new System.Drawing.Size(290, 36);
            this.bTempPathBrowse.TabIndex = 15;
            this.bTempPathBrowse.Text = "Browse";
            this.bTempPathBrowse.UseVisualStyleBackColor = true;
            this.bTempPathBrowse.Click += new System.EventHandler(this.BTempPathBrowse_Click);
            // 
            // lSINnerUrl
            // 
            this.lSINnerUrl.AutoSize = true;
            this.lSINnerUrl.Dock = System.Windows.Forms.DockStyle.Left;
            this.lSINnerUrl.Location = new System.Drawing.Point(4, 62);
            this.lSINnerUrl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lSINnerUrl.Name = "lSINnerUrl";
            this.lSINnerUrl.Size = new System.Drawing.Size(33, 38);
            this.lSINnerUrl.TabIndex = 4;
            this.lSINnerUrl.Text = "Url:";
            this.lSINnerUrl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cbSINnerUrl
            // 
            this.cbSINnerUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbSINnerUrl.FormattingEnabled = true;
            this.cbSINnerUrl.Location = new System.Drawing.Point(84, 67);
            this.cbSINnerUrl.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbSINnerUrl.MinimumSize = new System.Drawing.Size(298, 0);
            this.cbSINnerUrl.Name = "cbSINnerUrl";
            this.cbSINnerUrl.Size = new System.Drawing.Size(516, 28);
            this.cbSINnerUrl.TabIndex = 5;
            this.cbSINnerUrl.SelectedIndexChanged += new System.EventHandler(this.CbSINnerUrl_SelectedValueChanged);
            // 
            // tbHelptext
            // 
            this.tbHelptext.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbHelptext.BackColor = System.Drawing.SystemColors.Control;
            this.tlpOptions.SetColumnSpan(this.tbHelptext, 3);
            this.tbHelptext.Location = new System.Drawing.Point(4, 286);
            this.tbHelptext.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tbHelptext.Multiline = true;
            this.tbHelptext.Name = "tbHelptext";
            this.tbHelptext.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.tbHelptext.Size = new System.Drawing.Size(894, 292);
            this.tbHelptext.TabIndex = 8;
            this.tbHelptext.Text = resources.GetString("tbHelptext.Text");
            // 
            // cbUploadOnSave
            // 
            this.cbUploadOnSave.AutoSize = true;
            this.tlpOptions.SetColumnSpan(this.cbUploadOnSave, 3);
            this.cbUploadOnSave.Location = new System.Drawing.Point(4, 206);
            this.cbUploadOnSave.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbUploadOnSave.Name = "cbUploadOnSave";
            this.cbUploadOnSave.Size = new System.Drawing.Size(352, 24);
            this.cbUploadOnSave.TabIndex = 10;
            this.cbUploadOnSave.Text = "Upload on Save automatically (\"onlinemode\")";
            this.cbUploadOnSave.UseVisualStyleBackColor = true;
            // 
            // flpTempFolder
            // 
            this.flpTempFolder.AutoSize = true;
            this.flpTempFolder.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpOptions.SetColumnSpan(this.flpTempFolder, 2);
            this.flpTempFolder.Controls.Add(this.label3);
            this.flpTempFolder.Controls.Add(this.tbTempDownloadPath);
            this.flpTempFolder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpTempFolder.Location = new System.Drawing.Point(4, 240);
            this.flpTempFolder.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.flpTempFolder.Name = "flpTempFolder";
            this.flpTempFolder.Size = new System.Drawing.Size(596, 36);
            this.flpTempFolder.TabIndex = 17;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 8);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(120, 20);
            this.label3.TabIndex = 13;
            this.label3.Text = "Downloadfolder";
            // 
            // tbTempDownloadPath
            // 
            this.tbTempDownloadPath.Dock = System.Windows.Forms.DockStyle.Top;
            this.tbTempDownloadPath.Location = new System.Drawing.Point(132, 5);
            this.tbTempDownloadPath.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tbTempDownloadPath.Name = "tbTempDownloadPath";
            this.tbTempDownloadPath.ReadOnly = true;
            this.tbTempDownloadPath.Size = new System.Drawing.Size(434, 26);
            this.tbTempDownloadPath.TabIndex = 14;
            // 
            // bMultiUpload
            // 
            this.bMultiUpload.AutoSize = true;
            this.bMultiUpload.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bMultiUpload.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bMultiUpload.Location = new System.Drawing.Point(608, 105);
            this.bMultiUpload.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.bMultiUpload.MinimumSize = new System.Drawing.Size(150, 15);
            this.bMultiUpload.Name = "bMultiUpload";
            this.bMultiUpload.Size = new System.Drawing.Size(290, 91);
            this.bMultiUpload.TabIndex = 9;
            this.bMultiUpload.Text = "Multi-Upload";
            this.bMultiUpload.UseVisualStyleBackColor = true;
            this.bMultiUpload.Click += new System.EventHandler(this.bMultiUpload_Click);
            // 
            // bLogin
            // 
            this.bLogin.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bLogin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bLogin.Location = new System.Drawing.Point(608, 5);
            this.bLogin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.bLogin.Name = "bLogin";
            this.bLogin.Size = new System.Drawing.Size(290, 52);
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
            this.tlpAccount.Dock = System.Windows.Forms.DockStyle.Left;
            this.tlpAccount.Location = new System.Drawing.Point(84, 5);
            this.tlpAccount.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tlpAccount.Name = "tlpAccount";
            this.tlpAccount.RowCount = 1;
            this.tlpAccount.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpAccount.Size = new System.Drawing.Size(492, 52);
            this.tlpAccount.TabIndex = 18;
            // 
            // cbRoles
            // 
            this.cbRoles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cbRoles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbRoles.FormattingEnabled = true;
            this.cbRoles.Location = new System.Drawing.Point(55, 12);
            this.cbRoles.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbRoles.Name = "cbRoles";
            this.cbRoles.Size = new System.Drawing.Size(433, 28);
            this.cbRoles.TabIndex = 16;
            // 
            // lUsername
            // 
            this.lUsername.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lUsername.AutoSize = true;
            this.lUsername.Location = new System.Drawing.Point(4, 16);
            this.lUsername.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lUsername.Name = "lUsername";
            this.lUsername.Size = new System.Drawing.Size(43, 20);
            this.lUsername.TabIndex = 17;
            this.lUsername.Text = "User";
            // 
            // gpPublicMode
            // 
            this.gpPublicMode.AutoSize = true;
            this.gpPublicMode.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpPublicMode.Controls.Add(this.tlpPublicMode);
            this.gpPublicMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpPublicMode.Location = new System.Drawing.Point(529, 5);
            this.gpPublicMode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gpPublicMode.MaximumSize = new System.Drawing.Size(450, 246);
            this.gpPublicMode.Name = "gpPublicMode";
            this.gpPublicMode.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gpPublicMode.Size = new System.Drawing.Size(448, 144);
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
            this.tlpPublicMode.Controls.Add(this.bRegisterUriScheme, 0, 0);
            this.tlpPublicMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPublicMode.Location = new System.Drawing.Point(4, 24);
            this.tlpPublicMode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tlpPublicMode.Name = "tlpPublicMode";
            this.tlpPublicMode.RowCount = 2;
            this.tlpPublicMode.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpPublicMode.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpPublicMode.Size = new System.Drawing.Size(440, 115);
            this.tlpPublicMode.TabIndex = 0;
            // 
            // bRegisterUriScheme
            // 
            this.bRegisterUriScheme.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bRegisterUriScheme.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bRegisterUriScheme.Location = new System.Drawing.Point(4, 5);
            this.bRegisterUriScheme.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.bRegisterUriScheme.Name = "bRegisterUriScheme";
            this.bRegisterUriScheme.Size = new System.Drawing.Size(212, 47);
            this.bRegisterUriScheme.TabIndex = 20;
            this.bRegisterUriScheme.Text = "Register Url-Scheme";
            this.bRegisterUriScheme.UseVisualStyleBackColor = true;
            this.bRegisterUriScheme.Click += new System.EventHandler(this.BRegisterUriScheme_Click);
            // 
            // ucSINnersOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tlpAllOptions);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "ucSINnersOptions";
            this.Size = new System.Drawing.Size(981, 776);
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
            this.flpTempFolder.ResumeLayout(false);
            this.flpTempFolder.PerformLayout();
            this.tlpAccount.ResumeLayout(false);
            this.tlpAccount.PerformLayout();
            this.gpPublicMode.ResumeLayout(false);
            this.gpPublicMode.PerformLayout();
            this.tlpPublicMode.ResumeLayout(false);
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
        private System.Windows.Forms.FlowLayoutPanel flpTempFolder;
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
        private System.Windows.Forms.Button bRegisterUriScheme;
    }
}
