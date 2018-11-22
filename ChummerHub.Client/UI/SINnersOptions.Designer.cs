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
            this.bLogin = new System.Windows.Forms.Button();
            this.tlpOptions = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.labelAccountStatus = new System.Windows.Forms.Label();
            this.lSINnerUrl = new System.Windows.Forms.Label();
            this.cbSINnerUrl = new System.Windows.Forms.ComboBox();
            this.gbVisibility = new System.Windows.Forms.GroupBox();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.tlpVisibility = new System.Windows.Forms.TableLayoutPanel();
            this.cbVisibilityIsPublic = new System.Windows.Forms.CheckBox();
            this.cbVisibilityIsGroupVisible = new System.Windows.Forms.CheckBox();
            this.lGroupname = new System.Windows.Forms.Label();
            this.tbGroupname = new System.Windows.Forms.TextBox();
            this.gpVisibilityToUserList = new System.Windows.Forms.GroupBox();
            this.tlpVisibilityToUsers = new System.Windows.Forms.TableLayoutPanel();
            this.lbVisibilityToUsers = new System.Windows.Forms.ListBox();
            this.tbVisibilityAddEmail = new System.Windows.Forms.TextBox();
            this.bVisibilityAddEmail = new System.Windows.Forms.Button();
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
            this.tlpOptions.ColumnCount = 5;
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpOptions.Controls.Add(this.bLogin, 2, 0);
            this.tlpOptions.Controls.Add(this.label1, 0, 0);
            this.tlpOptions.Controls.Add(this.labelAccountStatus, 1, 0);
            this.tlpOptions.Controls.Add(this.lSINnerUrl, 0, 1);
            this.tlpOptions.Controls.Add(this.cbSINnerUrl, 1, 1);
            this.tlpOptions.Controls.Add(this.gbVisibility, 0, 2);
            this.tlpOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpOptions.Location = new System.Drawing.Point(0, 0);
            this.tlpOptions.Name = "tlpOptions";
            this.tlpOptions.RowCount = 4;
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 66.66666F));
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpOptions.Size = new System.Drawing.Size(665, 579);
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
            this.lSINnerUrl.Size = new System.Drawing.Size(23, 27);
            this.lSINnerUrl.TabIndex = 4;
            this.lSINnerUrl.Text = "Url:";
            this.lSINnerUrl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cbSINnerUrl
            // 
            this.tlpOptions.SetColumnSpan(this.cbSINnerUrl, 4);
            this.cbSINnerUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbSINnerUrl.FormattingEnabled = true;
            this.cbSINnerUrl.Location = new System.Drawing.Point(59, 32);
            this.cbSINnerUrl.Name = "cbSINnerUrl";
            this.cbSINnerUrl.Size = new System.Drawing.Size(603, 21);
            this.cbSINnerUrl.TabIndex = 5;
            // 
            // gbVisibility
            // 
            this.gbVisibility.AutoSize = true;
            this.tlpOptions.SetColumnSpan(this.gbVisibility, 5);
            this.gbVisibility.Controls.Add(this.tlpVisibility);
            this.gbVisibility.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbVisibility.Location = new System.Drawing.Point(3, 59);
            this.gbVisibility.Name = "gbVisibility";
            this.gbVisibility.Size = new System.Drawing.Size(659, 342);
            this.gbVisibility.TabIndex = 7;
            this.gbVisibility.TabStop = false;
            this.gbVisibility.Text = "Visibility of uploaded SINners";
            // 
            // tlpVisibility
            // 
            this.tlpVisibility.AutoSize = true;
            this.tlpVisibility.ColumnCount = 3;
            this.tlpVisibility.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpVisibility.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpVisibility.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpVisibility.Controls.Add(this.cbVisibilityIsPublic, 0, 1);
            this.tlpVisibility.Controls.Add(this.cbVisibilityIsGroupVisible, 0, 0);
            this.tlpVisibility.Controls.Add(this.lGroupname, 1, 0);
            this.tlpVisibility.Controls.Add(this.tbGroupname, 2, 0);
            this.tlpVisibility.Controls.Add(this.gpVisibilityToUserList, 1, 1);
            this.tlpVisibility.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpVisibility.Location = new System.Drawing.Point(3, 16);
            this.tlpVisibility.MinimumSize = new System.Drawing.Size(40, 40);
            this.tlpVisibility.Name = "tlpVisibility";
            this.tlpVisibility.RowCount = 3;
            this.tlpVisibility.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVisibility.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVisibility.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVisibility.Size = new System.Drawing.Size(653, 323);
            this.tlpVisibility.TabIndex = 0;
            // 
            // cbVisibilityIsPublic
            // 
            this.cbVisibilityIsPublic.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbVisibilityIsPublic.AutoSize = true;
            this.cbVisibilityIsPublic.Location = new System.Drawing.Point(3, 29);
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
            this.cbVisibilityIsGroupVisible.Size = new System.Drawing.Size(170, 20);
            this.cbVisibilityIsGroupVisible.TabIndex = 1;
            this.cbVisibilityIsGroupVisible.Text = "visible to members of my group";
            this.cbVisibilityIsGroupVisible.UseVisualStyleBackColor = true;
            this.cbVisibilityIsGroupVisible.CheckedChanged += new System.EventHandler(this.cbVisibilityIsGroupVisible_CheckedChanged);
            // 
            // lGroupname
            // 
            this.lGroupname.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lGroupname.AutoSize = true;
            this.lGroupname.Location = new System.Drawing.Point(179, 0);
            this.lGroupname.Name = "lGroupname";
            this.lGroupname.Size = new System.Drawing.Size(65, 26);
            this.lGroupname.TabIndex = 2;
            this.lGroupname.Text = "Groupname:";
            this.lGroupname.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbGroupname
            // 
            this.tbGroupname.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbGroupname.Location = new System.Drawing.Point(250, 3);
            this.tbGroupname.Name = "tbGroupname";
            this.tbGroupname.Size = new System.Drawing.Size(400, 20);
            this.tbGroupname.TabIndex = 3;
            this.tbGroupname.TextChanged += new System.EventHandler(this.tbGroupname_TextChanged);
            // 
            // gpVisibilityToUserList
            // 
            this.tlpVisibility.SetColumnSpan(this.gpVisibilityToUserList, 2);
            this.gpVisibilityToUserList.Controls.Add(this.tlpVisibilityToUsers);
            this.gpVisibilityToUserList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpVisibilityToUserList.Location = new System.Drawing.Point(179, 29);
            this.gpVisibilityToUserList.Name = "gpVisibilityToUserList";
            this.tlpVisibility.SetRowSpan(this.gpVisibilityToUserList, 2);
            this.gpVisibilityToUserList.Size = new System.Drawing.Size(471, 291);
            this.gpVisibilityToUserList.TabIndex = 4;
            this.gpVisibilityToUserList.TabStop = false;
            this.gpVisibilityToUserList.Text = "Visible to Users";
            // 
            // tlpVisibilityToUsers
            // 
            this.tlpVisibilityToUsers.ColumnCount = 2;
            this.tlpVisibilityToUsers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tlpVisibilityToUsers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpVisibilityToUsers.Controls.Add(this.lbVisibilityToUsers, 0, 0);
            this.tlpVisibilityToUsers.Controls.Add(this.tbVisibilityAddEmail, 0, 1);
            this.tlpVisibilityToUsers.Controls.Add(this.bVisibilityAddEmail, 1, 1);
            this.tlpVisibilityToUsers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpVisibilityToUsers.Location = new System.Drawing.Point(3, 16);
            this.tlpVisibilityToUsers.Name = "tlpVisibilityToUsers";
            this.tlpVisibilityToUsers.RowCount = 2;
            this.tlpVisibilityToUsers.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpVisibilityToUsers.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVisibilityToUsers.Size = new System.Drawing.Size(465, 272);
            this.tlpVisibilityToUsers.TabIndex = 0;
            // 
            // lbVisibilityToUsers
            // 
            this.tlpVisibilityToUsers.SetColumnSpan(this.lbVisibilityToUsers, 2);
            this.lbVisibilityToUsers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbVisibilityToUsers.FormattingEnabled = true;
            this.lbVisibilityToUsers.Location = new System.Drawing.Point(3, 3);
            this.lbVisibilityToUsers.MinimumSize = new System.Drawing.Size(100, 60);
            this.lbVisibilityToUsers.Name = "lbVisibilityToUsers";
            this.lbVisibilityToUsers.Size = new System.Drawing.Size(459, 237);
            this.lbVisibilityToUsers.TabIndex = 0;
            // 
            // tbVisibilityAddEmail
            // 
            this.tbVisibilityAddEmail.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbVisibilityAddEmail.Location = new System.Drawing.Point(3, 247);
            this.tbVisibilityAddEmail.Name = "tbVisibilityAddEmail";
            this.tbVisibilityAddEmail.Size = new System.Drawing.Size(366, 20);
            this.tbVisibilityAddEmail.TabIndex = 1;
            // 
            // bVisibilityAddEmail
            // 
            this.bVisibilityAddEmail.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.bVisibilityAddEmail.Location = new System.Drawing.Point(381, 246);
            this.bVisibilityAddEmail.Name = "bVisibilityAddEmail";
            this.bVisibilityAddEmail.Size = new System.Drawing.Size(75, 23);
            this.bVisibilityAddEmail.TabIndex = 2;
            this.bVisibilityAddEmail.Text = "Add";
            this.bVisibilityAddEmail.UseVisualStyleBackColor = true;
            this.bVisibilityAddEmail.Click += new System.EventHandler(this.bVisibilityAddEmail_Click);
            // 
            // SINnersOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.tlpOptions);
            this.Name = "SINnersOptions";
            this.Size = new System.Drawing.Size(665, 579);
            this.tlpOptions.ResumeLayout(false);
            this.tlpOptions.PerformLayout();
            this.gbVisibility.ResumeLayout(false);
            this.gbVisibility.PerformLayout();
            this.tlpVisibility.ResumeLayout(false);
            this.tlpVisibility.PerformLayout();
            this.gpVisibilityToUserList.ResumeLayout(false);
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
        private System.Windows.Forms.GroupBox gbVisibility;
        private System.Windows.Forms.TableLayoutPanel tlpVisibility;
        private System.Windows.Forms.CheckBox cbVisibilityIsPublic;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.CheckBox cbVisibilityIsGroupVisible;
        private System.Windows.Forms.Label lGroupname;
        private System.Windows.Forms.TextBox tbGroupname;
        private System.Windows.Forms.GroupBox gpVisibilityToUserList;
        private System.Windows.Forms.TableLayoutPanel tlpVisibilityToUsers;
        private System.Windows.Forms.ListBox lbVisibilityToUsers;
        private System.Windows.Forms.TextBox tbVisibilityAddEmail;
        private System.Windows.Forms.Button bVisibilityAddEmail;
    }
}
