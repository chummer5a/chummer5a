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
            this.bLogin = new System.Windows.Forms.Button();
            this.tlpOptions = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.labelAccountStatus = new System.Windows.Forms.Label();
            this.lSINnerUrl = new System.Windows.Forms.Label();
            this.cbSINnerUrl = new System.Windows.Forms.ComboBox();
            this.tbHelptext = new System.Windows.Forms.TextBox();
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
            this.lGroupname = new System.Windows.Forms.Label();
            this.tbGroupname = new System.Windows.Forms.TextBox();
            this.bMultiUpload = new System.Windows.Forms.Button();
            this.cbUploadOnSave = new System.Windows.Forms.CheckBox();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.tlpOptions.SuspendLayout();
            this.gbVisibility.SuspendLayout();
            this.tlpVisibility.SuspendLayout();
            this.gpVisibilityToUserList.SuspendLayout();
            this.tlpVisibilityToUsers.SuspendLayout();
            this.SuspendLayout();
            // 
            // bLogin
            // 
            this.bLogin.Location = new System.Drawing.Point(123, 3);
            this.bLogin.Name = "bLogin";
            this.bLogin.Size = new System.Drawing.Size(75, 23);
            this.bLogin.TabIndex = 0;
            this.bLogin.Text = "Login";
            this.bLogin.UseVisualStyleBackColor = true;
            this.bLogin.Click += new System.EventHandler(this.bLogin_ClickAsync);
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
            this.tlpOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpOptions.Location = new System.Drawing.Point(0, 0);
            this.tlpOptions.MinimumSize = new System.Drawing.Size(723, 500);
            this.tlpOptions.Name = "tlpOptions";
            this.tlpOptions.RowCount = 6;
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpOptions.Size = new System.Drawing.Size(723, 511);
            this.tlpOptions.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 29);
            this.label1.TabIndex = 2;
            this.label1.Text = "Account:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelAccountStatus
            // 
            this.labelAccountStatus.AutoSize = true;
            this.labelAccountStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelAccountStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAccountStatus.ForeColor = System.Drawing.Color.DarkMagenta;
            this.labelAccountStatus.Location = new System.Drawing.Point(59, 0);
            this.labelAccountStatus.Name = "labelAccountStatus";
            this.labelAccountStatus.Size = new System.Drawing.Size(58, 29);
            this.labelAccountStatus.TabIndex = 3;
            this.labelAccountStatus.Text = "unknown";
            this.labelAccountStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lSINnerUrl
            // 
            this.lSINnerUrl.AutoSize = true;
            this.lSINnerUrl.Dock = System.Windows.Forms.DockStyle.Left;
            this.lSINnerUrl.Location = new System.Drawing.Point(3, 29);
            this.lSINnerUrl.Name = "lSINnerUrl";
            this.lSINnerUrl.Size = new System.Drawing.Size(23, 36);
            this.lSINnerUrl.TabIndex = 4;
            this.lSINnerUrl.Text = "Url:";
            this.lSINnerUrl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cbSINnerUrl
            // 
            this.tlpOptions.SetColumnSpan(this.cbSINnerUrl, 3);
            this.cbSINnerUrl.Dock = System.Windows.Forms.DockStyle.Left;
            this.cbSINnerUrl.FormattingEnabled = true;
            this.cbSINnerUrl.Location = new System.Drawing.Point(59, 32);
            this.cbSINnerUrl.Name = "cbSINnerUrl";
            this.cbSINnerUrl.Size = new System.Drawing.Size(555, 21);
            this.cbSINnerUrl.TabIndex = 5;
            // 
            // tbHelptext
            // 
            this.tbHelptext.BackColor = System.Drawing.SystemColors.Control;
            this.tlpOptions.SetColumnSpan(this.tbHelptext, 5);
            this.tbHelptext.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbHelptext.Location = new System.Drawing.Point(3, 315);
            this.tbHelptext.Multiline = true;
            this.tbHelptext.Name = "tbHelptext";
            this.tbHelptext.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.tbHelptext.Size = new System.Drawing.Size(717, 193);
            this.tbHelptext.TabIndex = 8;
            this.tbHelptext.Text = resources.GetString("tbHelptext.Text");
            // 
            // gbVisibility
            // 
            this.gbVisibility.AutoSize = true;
            this.gbVisibility.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpOptions.SetColumnSpan(this.gbVisibility, 4);
            this.gbVisibility.Controls.Add(this.tlpVisibility);
            this.gbVisibility.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbVisibility.Location = new System.Drawing.Point(3, 68);
            this.gbVisibility.Name = "gbVisibility";
            this.gbVisibility.Size = new System.Drawing.Size(611, 218);
            this.gbVisibility.TabIndex = 7;
            this.gbVisibility.TabStop = false;
            this.gbVisibility.Text = "Visibility of uploaded SINners";
            // 
            // tlpVisibility
            // 
            this.tlpVisibility.AutoSize = true;
            this.tlpVisibility.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpVisibility.ColumnCount = 2;
            this.tlpVisibility.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpVisibility.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpVisibility.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpVisibility.Controls.Add(this.cbVisibilityIsPublic, 0, 1);
            this.tlpVisibility.Controls.Add(this.cbVisibilityIsGroupVisible, 0, 0);
            this.tlpVisibility.Controls.Add(this.gpVisibilityToUserList, 1, 0);
            this.tlpVisibility.Controls.Add(this.lGroupname, 0, 2);
            this.tlpVisibility.Controls.Add(this.tbGroupname, 0, 3);
            this.tlpVisibility.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpVisibility.Location = new System.Drawing.Point(3, 16);
            this.tlpVisibility.MinimumSize = new System.Drawing.Size(40, 40);
            this.tlpVisibility.Name = "tlpVisibility";
            this.tlpVisibility.RowCount = 5;
            this.tlpVisibility.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVisibility.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVisibility.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVisibility.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVisibility.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVisibility.Size = new System.Drawing.Size(605, 199);
            this.tlpVisibility.TabIndex = 0;
            // 
            // cbVisibilityIsPublic
            // 
            this.cbVisibilityIsPublic.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbVisibilityIsPublic.AutoSize = true;
            this.cbVisibilityIsPublic.Location = new System.Drawing.Point(3, 26);
            this.cbVisibilityIsPublic.Name = "cbVisibilityIsPublic";
            this.cbVisibilityIsPublic.Size = new System.Drawing.Size(170, 17);
            this.cbVisibilityIsPublic.TabIndex = 0;
            this.cbVisibilityIsPublic.Text = "visible to everyone";
            this.cbVisibilityIsPublic.UseVisualStyleBackColor = true;
            this.cbVisibilityIsPublic.CheckedChanged += new System.EventHandler(this.cbVisibilityIsPublic_CheckedChanged);
            // 
            // cbVisibilityIsGroupVisible
            // 
            this.cbVisibilityIsGroupVisible.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbVisibilityIsGroupVisible.AutoSize = true;
            this.cbVisibilityIsGroupVisible.Location = new System.Drawing.Point(3, 3);
            this.cbVisibilityIsGroupVisible.Name = "cbVisibilityIsGroupVisible";
            this.cbVisibilityIsGroupVisible.Size = new System.Drawing.Size(170, 17);
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
            this.gpVisibilityToUserList.Location = new System.Drawing.Point(179, 3);
            this.gpVisibilityToUserList.Name = "gpVisibilityToUserList";
            this.tlpVisibility.SetRowSpan(this.gpVisibilityToUserList, 5);
            this.gpVisibilityToUserList.Size = new System.Drawing.Size(423, 193);
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
            this.tlpVisibilityToUsers.Location = new System.Drawing.Point(3, 16);
            this.tlpVisibilityToUsers.Name = "tlpVisibilityToUsers";
            this.tlpVisibilityToUsers.RowCount = 2;
            this.tlpVisibilityToUsers.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVisibilityToUsers.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVisibilityToUsers.Size = new System.Drawing.Size(417, 174);
            this.tlpVisibilityToUsers.TabIndex = 0;
            // 
            // clbVisibilityToUsers
            // 
            this.tlpVisibilityToUsers.SetColumnSpan(this.clbVisibilityToUsers, 3);
            this.clbVisibilityToUsers.Dock = System.Windows.Forms.DockStyle.Top;
            this.clbVisibilityToUsers.FormattingEnabled = true;
            this.clbVisibilityToUsers.Location = new System.Drawing.Point(3, 3);
            this.clbVisibilityToUsers.MinimumSize = new System.Drawing.Size(100, 60);
            this.clbVisibilityToUsers.Name = "clbVisibilityToUsers";
            this.clbVisibilityToUsers.Size = new System.Drawing.Size(411, 139);
            this.clbVisibilityToUsers.TabIndex = 0;
            this.clbVisibilityToUsers.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clbVisibilityToUsers_ItemCheck);
            // 
            // tbVisibilityAddEmail
            // 
            this.tbVisibilityAddEmail.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbVisibilityAddEmail.Location = new System.Drawing.Point(3, 149);
            this.tbVisibilityAddEmail.Name = "tbVisibilityAddEmail";
            this.tbVisibilityAddEmail.Size = new System.Drawing.Size(244, 20);
            this.tbVisibilityAddEmail.TabIndex = 1;
            // 
            // bVisibilityAddEmail
            // 
            this.bVisibilityAddEmail.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.bVisibilityAddEmail.Location = new System.Drawing.Point(260, 148);
            this.bVisibilityAddEmail.Name = "bVisibilityAddEmail";
            this.bVisibilityAddEmail.Size = new System.Drawing.Size(62, 23);
            this.bVisibilityAddEmail.TabIndex = 2;
            this.bVisibilityAddEmail.Text = "Add";
            this.bVisibilityAddEmail.UseVisualStyleBackColor = true;
            this.bVisibilityAddEmail.Click += new System.EventHandler(this.bVisibilityAddEmail_Click);
            // 
            // bVisibilityRemove
            // 
            this.bVisibilityRemove.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.bVisibilityRemove.Location = new System.Drawing.Point(343, 148);
            this.bVisibilityRemove.Name = "bVisibilityRemove";
            this.bVisibilityRemove.Size = new System.Drawing.Size(63, 23);
            this.bVisibilityRemove.TabIndex = 3;
            this.bVisibilityRemove.Text = "Remove";
            this.bVisibilityRemove.UseVisualStyleBackColor = true;
            this.bVisibilityRemove.Click += new System.EventHandler(this.bVisibilityRemove_Click);
            // 
            // lGroupname
            // 
            this.lGroupname.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lGroupname.AutoSize = true;
            this.lGroupname.Location = new System.Drawing.Point(3, 46);
            this.lGroupname.MinimumSize = new System.Drawing.Size(20, 20);
            this.lGroupname.Name = "lGroupname";
            this.lGroupname.Size = new System.Drawing.Size(170, 20);
            this.lGroupname.TabIndex = 2;
            this.lGroupname.Text = "Groupname:";
            this.lGroupname.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbGroupname
            // 
            this.tbGroupname.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbGroupname.Location = new System.Drawing.Point(3, 69);
            this.tbGroupname.Name = "tbGroupname";
            this.tbGroupname.Size = new System.Drawing.Size(170, 20);
            this.tbGroupname.TabIndex = 3;
            this.tbGroupname.TextChanged += new System.EventHandler(this.tbGroupname_TextChanged);
            // 
            // bMultiUpload
            // 
            this.bMultiUpload.Location = new System.Drawing.Point(620, 32);
            this.bMultiUpload.MinimumSize = new System.Drawing.Size(100, 10);
            this.bMultiUpload.Name = "bMultiUpload";
            this.bMultiUpload.Size = new System.Drawing.Size(100, 30);
            this.bMultiUpload.TabIndex = 9;
            this.bMultiUpload.Text = "Multi-Upload";
            this.bMultiUpload.UseVisualStyleBackColor = true;
            this.bMultiUpload.Click += new System.EventHandler(this.bMultiUpload_Click);
            // 
            // cbUploadOnSave
            // 
            this.cbUploadOnSave.AutoSize = true;
            this.tlpOptions.SetColumnSpan(this.cbUploadOnSave, 3);
            this.cbUploadOnSave.Location = new System.Drawing.Point(3, 292);
            this.cbUploadOnSave.Name = "cbUploadOnSave";
            this.cbUploadOnSave.Size = new System.Drawing.Size(240, 17);
            this.cbUploadOnSave.TabIndex = 10;
            this.cbUploadOnSave.Text = "Upload on Save automatically (\"onlinemode\")";
            this.cbUploadOnSave.UseVisualStyleBackColor = true;
            this.cbUploadOnSave.CheckedChanged += new System.EventHandler(this.cbUploadOnSave_CheckedChanged);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            // 
            // SINnersOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tlpOptions);
            this.MinimumSize = new System.Drawing.Size(723, 511);
            this.Name = "SINnersOptions";
            this.Size = new System.Drawing.Size(723, 511);
            this.tlpOptions.ResumeLayout(false);
            this.tlpOptions.PerformLayout();
            this.gbVisibility.ResumeLayout(false);
            this.gbVisibility.PerformLayout();
            this.tlpVisibility.ResumeLayout(false);
            this.tlpVisibility.PerformLayout();
            this.gpVisibilityToUserList.ResumeLayout(false);
            this.gpVisibilityToUserList.PerformLayout();
            this.tlpVisibilityToUsers.ResumeLayout(false);
            this.tlpVisibilityToUsers.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bLogin;
        private System.Windows.Forms.TableLayoutPanel tlpOptions;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelAccountStatus;
        private System.Windows.Forms.Label lSINnerUrl;
        private System.Windows.Forms.ComboBox cbSINnerUrl;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.TextBox tbHelptext;
        private System.Windows.Forms.Button bMultiUpload;
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
        private System.Windows.Forms.Label lGroupname;
        private System.Windows.Forms.TextBox tbGroupname;
        private System.Windows.Forms.CheckBox cbUploadOnSave;
    }
}
