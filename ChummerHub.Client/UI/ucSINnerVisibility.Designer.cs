namespace ChummerHub.Client.UI
{
    partial class ucSINnerVisibility
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
            this.gpVisibilityToUserList = new System.Windows.Forms.GroupBox();
            this.tlpVisibilityToUsers = new System.Windows.Forms.TableLayoutPanel();
            this.clbVisibilityToUsers = new System.Windows.Forms.CheckedListBox();
            this.tbVisibilityAddEmail = new System.Windows.Forms.TextBox();
            this.bVisibilityAddEmail = new System.Windows.Forms.Button();
            this.bVisibilityRemove = new System.Windows.Forms.Button();
            this.cbVisibleInGroups = new System.Windows.Forms.CheckBox();
            this.gpVisibilityToUserList.SuspendLayout();
            this.tlpVisibilityToUsers.SuspendLayout();
            this.SuspendLayout();
            // 
            // gpVisibilityToUserList
            // 
            this.gpVisibilityToUserList.AutoSize = true;
            this.gpVisibilityToUserList.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpVisibilityToUserList.Controls.Add(this.tlpVisibilityToUsers);
            this.gpVisibilityToUserList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpVisibilityToUserList.Location = new System.Drawing.Point(0, 0);
            this.gpVisibilityToUserList.MinimumSize = new System.Drawing.Size(50, 50);
            this.gpVisibilityToUserList.Name = "gpVisibilityToUserList";
            this.gpVisibilityToUserList.Size = new System.Drawing.Size(320, 231);
            this.gpVisibilityToUserList.TabIndex = 5;
            this.gpVisibilityToUserList.TabStop = false;
            this.gpVisibilityToUserList.Text = "Visible to Users (checked = may edit)";
            // 
            // tlpVisibilityToUsers
            // 
            this.tlpVisibilityToUsers.AutoSize = true;
            this.tlpVisibilityToUsers.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpVisibilityToUsers.ColumnCount = 3;
            this.tlpVisibilityToUsers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpVisibilityToUsers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpVisibilityToUsers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpVisibilityToUsers.Controls.Add(this.clbVisibilityToUsers, 0, 0);
            this.tlpVisibilityToUsers.Controls.Add(this.tbVisibilityAddEmail, 0, 1);
            this.tlpVisibilityToUsers.Controls.Add(this.bVisibilityAddEmail, 1, 1);
            this.tlpVisibilityToUsers.Controls.Add(this.bVisibilityRemove, 2, 1);
            this.tlpVisibilityToUsers.Controls.Add(this.cbVisibleInGroups, 0, 2);
            this.tlpVisibilityToUsers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpVisibilityToUsers.Location = new System.Drawing.Point(3, 16);
            this.tlpVisibilityToUsers.Name = "tlpVisibilityToUsers";
            this.tlpVisibilityToUsers.RowCount = 3;
            this.tlpVisibilityToUsers.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpVisibilityToUsers.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVisibilityToUsers.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVisibilityToUsers.Size = new System.Drawing.Size(314, 212);
            this.tlpVisibilityToUsers.TabIndex = 0;
            // 
            // clbVisibilityToUsers
            // 
            this.tlpVisibilityToUsers.SetColumnSpan(this.clbVisibilityToUsers, 3);
            this.clbVisibilityToUsers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.clbVisibilityToUsers.FormattingEnabled = true;
            this.clbVisibilityToUsers.Location = new System.Drawing.Point(3, 3);
            this.clbVisibilityToUsers.MinimumSize = new System.Drawing.Size(75, 60);
            this.clbVisibilityToUsers.Name = "clbVisibilityToUsers";
            this.clbVisibilityToUsers.Size = new System.Drawing.Size(308, 154);
            this.clbVisibilityToUsers.TabIndex = 0;
            // 
            // tbVisibilityAddEmail
            // 
            this.tbVisibilityAddEmail.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbVisibilityAddEmail.Location = new System.Drawing.Point(3, 164);
            this.tbVisibilityAddEmail.Name = "tbVisibilityAddEmail";
            this.tbVisibilityAddEmail.Size = new System.Drawing.Size(171, 20);
            this.tbVisibilityAddEmail.TabIndex = 1;
            // 
            // bVisibilityAddEmail
            // 
            this.bVisibilityAddEmail.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.bVisibilityAddEmail.Location = new System.Drawing.Point(180, 163);
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
            this.bVisibilityRemove.Location = new System.Drawing.Point(248, 163);
            this.bVisibilityRemove.Name = "bVisibilityRemove";
            this.bVisibilityRemove.Size = new System.Drawing.Size(63, 23);
            this.bVisibilityRemove.TabIndex = 3;
            this.bVisibilityRemove.Text = "Remove";
            this.bVisibilityRemove.UseVisualStyleBackColor = true;
            this.bVisibilityRemove.Click += new System.EventHandler(this.bVisibilityRemove_Click);
            // 
            // cbVisibleInGroups
            // 
            this.cbVisibleInGroups.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cbVisibleInGroups.AutoSize = true;
            this.tlpVisibilityToUsers.SetColumnSpan(this.cbVisibleInGroups, 3);
            this.cbVisibleInGroups.Location = new System.Drawing.Point(106, 192);
            this.cbVisibleInGroups.Name = "cbVisibleInGroups";
            this.cbVisibleInGroups.Size = new System.Drawing.Size(101, 17);
            this.cbVisibleInGroups.TabIndex = 4;
            this.cbVisibleInGroups.Text = "visible in groups";
            this.cbVisibleInGroups.UseVisualStyleBackColor = true;
            this.cbVisibleInGroups.Click += new System.EventHandler(this.CbVisibleInGroups_Click);
            // 
            // ucSINnerVisibility
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.gpVisibilityToUserList);
            this.Name = "ucSINnerVisibility";
            this.Size = new System.Drawing.Size(320, 231);
            this.gpVisibilityToUserList.ResumeLayout(false);
            this.gpVisibilityToUserList.PerformLayout();
            this.tlpVisibilityToUsers.ResumeLayout(false);
            this.tlpVisibilityToUsers.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gpVisibilityToUserList;
        private System.Windows.Forms.TableLayoutPanel tlpVisibilityToUsers;
        private System.Windows.Forms.CheckedListBox clbVisibilityToUsers;
        private System.Windows.Forms.TextBox tbVisibilityAddEmail;
        private System.Windows.Forms.Button bVisibilityAddEmail;
        private System.Windows.Forms.Button bVisibilityRemove;
        private System.Windows.Forms.CheckBox cbVisibleInGroups;
    }
}
