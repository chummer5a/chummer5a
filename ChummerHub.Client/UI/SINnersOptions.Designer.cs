namespace ChummerHub.Client.UI
{
    partial class SINnersOptions
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SINnersOptions));
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.cbUploadOnSave = new System.Windows.Forms.CheckBox();
            this.bMultiUpload = new System.Windows.Forms.Button();
            this.gbVisibility = new System.Windows.Forms.GroupBox();
            this.tlpVisibility = new System.Windows.Forms.TableLayoutPanel();
            this.cbVisibilityIsPublic = new System.Windows.Forms.CheckBox();
            this.cbVisibilityIsGroupVisible = new System.Windows.Forms.CheckBox();
            this.gpVisibilityToUserList = new System.Windows.Forms.GroupBox();
            this.tlpVisibilityToUsers = new System.Windows.Forms.TableLayoutPanel();
            this.clbVisibilityToUsers = new System.Windows.Forms.CheckedListBox();
            this.tbVisibilityAddEmail = new System.Windows.Forms.TextBox();
            this.bVisibilityAddEmail = new System.Windows.Forms.Button();
            this.bVisibilityRemove = new System.Windows.Forms.Button();
            this.tbHelptext = new System.Windows.Forms.TextBox();
            this.cbSINnerUrl = new System.Windows.Forms.ComboBox();
            this.lSINnerUrl = new System.Windows.Forms.Label();
            this.labelAccountStatus = new System.Windows.Forms.Label();
            this.bLogin = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tlpOptions = new System.Windows.Forms.TableLayoutPanel();
            this.flpButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.bBackup = new System.Windows.Forms.Button();
            this.bRestore = new System.Windows.Forms.Button();
            this.gbVisibility.SuspendLayout();
            this.tlpVisibility.SuspendLayout();
            this.gpVisibilityToUserList.SuspendLayout();
            this.tlpVisibilityToUsers.SuspendLayout();
            this.tlpOptions.SuspendLayout();
            this.flpButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            // 
            // cbUploadOnSave
            // 
            this.cbUploadOnSave.AutoSize = true;
            this.tlpOptions.SetColumnSpan(this.cbUploadOnSave, 3);
            this.cbUploadOnSave.Location = new System.Drawing.Point(4, 411);
            this.cbUploadOnSave.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbUploadOnSave.Name = "cbUploadOnSave";
            this.cbUploadOnSave.Size = new System.Drawing.Size(352, 24);
            this.cbUploadOnSave.TabIndex = 10;
            this.cbUploadOnSave.Text = "Upload on Save automatically (\"onlinemode\")";
            this.cbUploadOnSave.UseVisualStyleBackColor = true;
            this.cbUploadOnSave.CheckedChanged += new System.EventHandler(this.cbUploadOnSave_CheckedChanged);
            // 
            // bMultiUpload
            // 
            this.bMultiUpload.Location = new System.Drawing.Point(922, 50);
            this.bMultiUpload.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.bMultiUpload.MinimumSize = new System.Drawing.Size(150, 15);
            this.bMultiUpload.Name = "bMultiUpload";
            this.bMultiUpload.Size = new System.Drawing.Size(150, 46);
            this.bMultiUpload.TabIndex = 9;
            this.bMultiUpload.Text = "Multi-Upload";
            this.bMultiUpload.UseVisualStyleBackColor = true;
            this.bMultiUpload.Click += new System.EventHandler(this.bMultiUpload_Click);
            // 
            // gbVisibility
            // 
            this.gbVisibility.AutoSize = true;
            this.gbVisibility.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpOptions.SetColumnSpan(this.gbVisibility, 4);
            this.gbVisibility.Controls.Add(this.tlpVisibility);
            this.gbVisibility.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbVisibility.Location = new System.Drawing.Point(4, 106);
            this.gbVisibility.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbVisibility.Name = "gbVisibility";
            this.gbVisibility.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbVisibility.Size = new System.Drawing.Size(910, 295);
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
            this.tlpVisibility.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpVisibility.Controls.Add(this.cbVisibilityIsPublic, 0, 1);
            this.tlpVisibility.Controls.Add(this.cbVisibilityIsGroupVisible, 0, 0);
            this.tlpVisibility.Controls.Add(this.gpVisibilityToUserList, 1, 0);
            this.tlpVisibility.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpVisibility.Location = new System.Drawing.Point(4, 24);
            this.tlpVisibility.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tlpVisibility.MinimumSize = new System.Drawing.Size(60, 62);
            this.tlpVisibility.Name = "tlpVisibility";
            this.tlpVisibility.RowCount = 5;
            this.tlpVisibility.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVisibility.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVisibility.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVisibility.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVisibility.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVisibility.Size = new System.Drawing.Size(902, 266);
            this.tlpVisibility.TabIndex = 0;
            // 
            // cbVisibilityIsPublic
            // 
            this.cbVisibilityIsPublic.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbVisibilityIsPublic.AutoSize = true;
            this.cbVisibilityIsPublic.Location = new System.Drawing.Point(4, 39);
            this.cbVisibilityIsPublic.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbVisibilityIsPublic.Name = "cbVisibilityIsPublic";
            this.cbVisibilityIsPublic.Size = new System.Drawing.Size(258, 24);
            this.cbVisibilityIsPublic.TabIndex = 0;
            this.cbVisibilityIsPublic.Text = "discoverable (upcoming search)";
            this.cbVisibilityIsPublic.UseVisualStyleBackColor = true;
            this.cbVisibilityIsPublic.CheckedChanged += new System.EventHandler(this.cbVisibilityIsPublic_CheckedChanged);
            // 
            // cbVisibilityIsGroupVisible
            // 
            this.cbVisibilityIsGroupVisible.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbVisibilityIsGroupVisible.AutoSize = true;
            this.cbVisibilityIsGroupVisible.Location = new System.Drawing.Point(4, 5);
            this.cbVisibilityIsGroupVisible.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbVisibilityIsGroupVisible.Name = "cbVisibilityIsGroupVisible";
            this.cbVisibilityIsGroupVisible.Size = new System.Drawing.Size(258, 24);
            this.cbVisibilityIsGroupVisible.TabIndex = 1;
            this.cbVisibilityIsGroupVisible.Text = "visible to members of my group";
            this.cbVisibilityIsGroupVisible.UseVisualStyleBackColor = true;
            this.cbVisibilityIsGroupVisible.CheckedChanged += new System.EventHandler(this.cbVisibilityIsGroupVisible_CheckedChanged);
            // 
            // gpVisibilityToUserList
            // 
            this.gpVisibilityToUserList.AutoSize = true;
            this.gpVisibilityToUserList.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpVisibilityToUserList.Controls.Add(this.tlpVisibilityToUsers);
            this.gpVisibilityToUserList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpVisibilityToUserList.Location = new System.Drawing.Point(270, 5);
            this.gpVisibilityToUserList.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gpVisibilityToUserList.Name = "gpVisibilityToUserList";
            this.gpVisibilityToUserList.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tlpVisibility.SetRowSpan(this.gpVisibilityToUserList, 5);
            this.gpVisibilityToUserList.Size = new System.Drawing.Size(628, 256);
            this.gpVisibilityToUserList.TabIndex = 4;
            this.gpVisibilityToUserList.TabStop = false;
            this.gpVisibilityToUserList.Text = "Visible to Users (checked = may edit)";
            // 
            // tlpVisibilityToUsers
            // 
            this.tlpVisibilityToUsers.AutoSize = true;
            this.tlpVisibilityToUsers.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpVisibilityToUsers.ColumnCount = 3;
            this.tlpVisibilityToUsers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tlpVisibilityToUsers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpVisibilityToUsers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpVisibilityToUsers.Controls.Add(this.clbVisibilityToUsers, 0, 0);
            this.tlpVisibilityToUsers.Controls.Add(this.tbVisibilityAddEmail, 0, 1);
            this.tlpVisibilityToUsers.Controls.Add(this.bVisibilityAddEmail, 1, 1);
            this.tlpVisibilityToUsers.Controls.Add(this.bVisibilityRemove, 2, 1);
            this.tlpVisibilityToUsers.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpVisibilityToUsers.Location = new System.Drawing.Point(4, 24);
            this.tlpVisibilityToUsers.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tlpVisibilityToUsers.Name = "tlpVisibilityToUsers";
            this.tlpVisibilityToUsers.RowCount = 2;
            this.tlpVisibilityToUsers.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVisibilityToUsers.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVisibilityToUsers.Size = new System.Drawing.Size(620, 227);
            this.tlpVisibilityToUsers.TabIndex = 0;
            // 
            // clbVisibilityToUsers
            // 
            this.tlpVisibilityToUsers.SetColumnSpan(this.clbVisibilityToUsers, 3);
            this.clbVisibilityToUsers.Dock = System.Windows.Forms.DockStyle.Top;
            this.clbVisibilityToUsers.FormattingEnabled = true;
            this.clbVisibilityToUsers.Location = new System.Drawing.Point(4, 5);
            this.clbVisibilityToUsers.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.clbVisibilityToUsers.MinimumSize = new System.Drawing.Size(148, 90);
            this.clbVisibilityToUsers.Name = "clbVisibilityToUsers";
            this.clbVisibilityToUsers.Size = new System.Drawing.Size(612, 172);
            this.clbVisibilityToUsers.TabIndex = 0;
            this.clbVisibilityToUsers.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clbVisibilityToUsers_ItemCheck);
            // 
            // tbVisibilityAddEmail
            // 
            this.tbVisibilityAddEmail.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbVisibilityAddEmail.Location = new System.Drawing.Point(4, 191);
            this.tbVisibilityAddEmail.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tbVisibilityAddEmail.Name = "tbVisibilityAddEmail";
            this.tbVisibilityAddEmail.Size = new System.Drawing.Size(364, 26);
            this.tbVisibilityAddEmail.TabIndex = 1;
            // 
            // bVisibilityAddEmail
            // 
            this.bVisibilityAddEmail.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.bVisibilityAddEmail.Location = new System.Drawing.Point(387, 187);
            this.bVisibilityAddEmail.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.bVisibilityAddEmail.Name = "bVisibilityAddEmail";
            this.bVisibilityAddEmail.Size = new System.Drawing.Size(93, 35);
            this.bVisibilityAddEmail.TabIndex = 2;
            this.bVisibilityAddEmail.Text = "Add";
            this.bVisibilityAddEmail.UseVisualStyleBackColor = true;
            this.bVisibilityAddEmail.Click += new System.EventHandler(this.bVisibilityAddEmail_Click);
            // 
            // bVisibilityRemove
            // 
            this.bVisibilityRemove.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.bVisibilityRemove.Location = new System.Drawing.Point(511, 187);
            this.bVisibilityRemove.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.bVisibilityRemove.Name = "bVisibilityRemove";
            this.bVisibilityRemove.Size = new System.Drawing.Size(94, 35);
            this.bVisibilityRemove.TabIndex = 3;
            this.bVisibilityRemove.Text = "Remove";
            this.bVisibilityRemove.UseVisualStyleBackColor = true;
            this.bVisibilityRemove.Click += new System.EventHandler(this.bVisibilityRemove_Click);
            // 
            // tbHelptext
            // 
            this.tbHelptext.BackColor = System.Drawing.SystemColors.Control;
            this.tlpOptions.SetColumnSpan(this.tbHelptext, 5);
            this.tbHelptext.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbHelptext.Location = new System.Drawing.Point(4, 445);
            this.tbHelptext.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tbHelptext.Multiline = true;
            this.tbHelptext.Name = "tbHelptext";
            this.tbHelptext.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.tbHelptext.Size = new System.Drawing.Size(1135, 336);
            this.tbHelptext.TabIndex = 8;
            this.tbHelptext.Text = resources.GetString("tbHelptext.Text");
            // 
            // cbSINnerUrl
            // 
            this.tlpOptions.SetColumnSpan(this.cbSINnerUrl, 3);
            this.cbSINnerUrl.Dock = System.Windows.Forms.DockStyle.Left;
            this.cbSINnerUrl.FormattingEnabled = true;
            this.cbSINnerUrl.Location = new System.Drawing.Point(84, 50);
            this.cbSINnerUrl.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbSINnerUrl.Name = "cbSINnerUrl";
            this.cbSINnerUrl.Size = new System.Drawing.Size(830, 28);
            this.cbSINnerUrl.TabIndex = 5;
            // 
            // lSINnerUrl
            // 
            this.lSINnerUrl.AutoSize = true;
            this.lSINnerUrl.Dock = System.Windows.Forms.DockStyle.Left;
            this.lSINnerUrl.Location = new System.Drawing.Point(4, 45);
            this.lSINnerUrl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lSINnerUrl.Name = "lSINnerUrl";
            this.lSINnerUrl.Size = new System.Drawing.Size(33, 56);
            this.lSINnerUrl.TabIndex = 4;
            this.lSINnerUrl.Text = "Url:";
            this.lSINnerUrl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelAccountStatus
            // 
            this.labelAccountStatus.AutoSize = true;
            this.labelAccountStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelAccountStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAccountStatus.ForeColor = System.Drawing.Color.DarkMagenta;
            this.labelAccountStatus.Location = new System.Drawing.Point(84, 0);
            this.labelAccountStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelAccountStatus.Name = "labelAccountStatus";
            this.labelAccountStatus.Size = new System.Drawing.Size(81, 45);
            this.labelAccountStatus.TabIndex = 3;
            this.labelAccountStatus.Text = "unknown";
            this.labelAccountStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // bLogin
            // 
            this.bLogin.Location = new System.Drawing.Point(173, 5);
            this.bLogin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.bLogin.Name = "bLogin";
            this.bLogin.Size = new System.Drawing.Size(112, 35);
            this.bLogin.TabIndex = 0;
            this.bLogin.Text = "Login";
            this.bLogin.UseVisualStyleBackColor = true;
            this.bLogin.Click += new System.EventHandler(this.bLogin_ClickAsync);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(4, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 45);
            this.label1.TabIndex = 2;
            this.label1.Text = "Account:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
            this.tlpOptions.Controls.Add(this.label1, 0, 0);
            this.tlpOptions.Controls.Add(this.bLogin, 2, 0);
            this.tlpOptions.Controls.Add(this.labelAccountStatus, 1, 0);
            this.tlpOptions.Controls.Add(this.lSINnerUrl, 0, 1);
            this.tlpOptions.Controls.Add(this.cbSINnerUrl, 1, 1);
            this.tlpOptions.Controls.Add(this.tbHelptext, 0, 5);
            this.tlpOptions.Controls.Add(this.gbVisibility, 0, 2);
            this.tlpOptions.Controls.Add(this.bMultiUpload, 4, 1);
            this.tlpOptions.Controls.Add(this.cbUploadOnSave, 0, 4);
            this.tlpOptions.Controls.Add(this.flpButtons, 4, 2);
            this.tlpOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpOptions.Location = new System.Drawing.Point(0, 0);
            this.tlpOptions.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tlpOptions.MinimumSize = new System.Drawing.Size(1084, 769);
            this.tlpOptions.Name = "tlpOptions";
            this.tlpOptions.RowCount = 6;
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
            this.tlpOptions.Size = new System.Drawing.Size(1143, 786);
            this.tlpOptions.TabIndex = 1;
            // 
            // flpButtons
            // 
            this.flpButtons.AutoSize = true;
            this.flpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpButtons.Controls.Add(this.bBackup);
            this.flpButtons.Controls.Add(this.bRestore);
            this.flpButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpButtons.Location = new System.Drawing.Point(922, 106);
            this.flpButtons.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.flpButtons.Name = "flpButtons";
            this.flpButtons.Size = new System.Drawing.Size(217, 295);
            this.flpButtons.TabIndex = 11;
            // 
            // bBackup
            // 
            this.bBackup.AutoSize = true;
            this.bBackup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bBackup.Dock = System.Windows.Forms.DockStyle.Top;
            this.bBackup.Location = new System.Drawing.Point(4, 5);
            this.bBackup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.bBackup.Name = "bBackup";
            this.bBackup.Size = new System.Drawing.Size(73, 30);
            this.bBackup.TabIndex = 0;
            this.bBackup.Text = "Backup";
            this.bBackup.UseVisualStyleBackColor = true;
            this.bBackup.Click += new System.EventHandler(this.bBackup_Click);
            // 
            // bRestore
            // 
            this.bRestore.Location = new System.Drawing.Point(85, 5);
            this.bRestore.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.bRestore.Name = "bRestore";
            this.bRestore.Size = new System.Drawing.Size(112, 35);
            this.bRestore.TabIndex = 1;
            this.bRestore.Text = "Restore";
            this.bRestore.UseVisualStyleBackColor = true;
            this.bRestore.Click += new System.EventHandler(this.bRestore_Click);
            // 
            // SINnersOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tlpOptions);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MinimumSize = new System.Drawing.Size(1084, 786);
            this.Name = "SINnersOptions";
            this.Size = new System.Drawing.Size(1143, 786);
            this.gbVisibility.ResumeLayout(false);
            this.gbVisibility.PerformLayout();
            this.tlpVisibility.ResumeLayout(false);
            this.tlpVisibility.PerformLayout();
            this.gpVisibilityToUserList.ResumeLayout(false);
            this.gpVisibilityToUserList.PerformLayout();
            this.tlpVisibilityToUsers.ResumeLayout(false);
            this.tlpVisibilityToUsers.PerformLayout();
            this.tlpOptions.ResumeLayout(false);
            this.tlpOptions.PerformLayout();
            this.flpButtons.ResumeLayout(false);
            this.flpButtons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.CheckBox cbUploadOnSave;
        private System.Windows.Forms.TableLayoutPanel tlpOptions;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button bLogin;
        private System.Windows.Forms.Label labelAccountStatus;
        private System.Windows.Forms.Label lSINnerUrl;
        private System.Windows.Forms.ComboBox cbSINnerUrl;
        private System.Windows.Forms.TextBox tbHelptext;
        private System.Windows.Forms.GroupBox gbVisibility;
        private System.Windows.Forms.TableLayoutPanel tlpVisibility;
        private System.Windows.Forms.CheckBox cbVisibilityIsPublic;
        private System.Windows.Forms.CheckBox cbVisibilityIsGroupVisible;
        private System.Windows.Forms.GroupBox gpVisibilityToUserList;
        private System.Windows.Forms.TableLayoutPanel tlpVisibilityToUsers;
        private System.Windows.Forms.CheckedListBox clbVisibilityToUsers;
        private System.Windows.Forms.TextBox tbVisibilityAddEmail;
        private System.Windows.Forms.Button bVisibilityAddEmail;
        private System.Windows.Forms.Button bVisibilityRemove;
        private System.Windows.Forms.Button bMultiUpload;
        private System.Windows.Forms.FlowLayoutPanel flpButtons;
        private System.Windows.Forms.Button bBackup;
        private System.Windows.Forms.Button bRestore;
    }
}
