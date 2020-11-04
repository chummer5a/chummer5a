namespace ChummerHub.Client.UI
{
    using Chummer;
    partial class ucSINnerGroupCreate
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tbGroupname = new System.Windows.Forms.TextBox();
            this.tbAdminRole = new System.Windows.Forms.TextBox();
            this.tbPassword = new System.Windows.Forms.MaskedTextBox();
            this.tbParentGroupId = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbGroupId = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cboLanguage1 = new Chummer.ElasticComboBox();
            this.imgLanguageFlag = new System.Windows.Forms.PictureBox();
            this.lGroupCreatorUser = new System.Windows.Forms.Label();
            this.tbGroupCreatorUsername = new System.Windows.Forms.TextBox();
            this.bOk = new System.Windows.Forms.Button();
            this.cbIsPublic = new System.Windows.Forms.CheckBox();
            this.bParentGroupId = new System.Windows.Forms.Button();
            this.tbDescription = new System.Windows.Forms.TextBox();
            this.lDescription = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgLanguageFlag)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.tbGroupname, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tbAdminRole, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.tbPassword, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.tbParentGroupId, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.tbGroupId, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.cboLanguage1, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.imgLanguageFlag, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lGroupCreatorUser, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.tbGroupCreatorUsername, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.lDescription, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.tbDescription, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.bOk, 2, 9);
            this.tableLayoutPanel1.Controls.Add(this.cbIsPublic, 1, 8);
            this.tableLayoutPanel1.Controls.Add(this.bParentGroupId, 1, 4);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.MinimumSize = new System.Drawing.Size(350, 200);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 11;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(438, 307);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 34);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(104, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Preferred Language:";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(43, 61);
            this.label3.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Admin Role:";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(51, 87);
            this.label4.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Password:";
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(22, 115);
            this.label5.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(85, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Parent Group Id:";
            // 
            // tbGroupname
            // 
            this.tbGroupname.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.tbGroupname, 2);
            this.tbGroupname.Location = new System.Drawing.Point(113, 3);
            this.tbGroupname.Name = "tbGroupname";
            this.tbGroupname.Size = new System.Drawing.Size(322, 20);
            this.tbGroupname.TabIndex = 5;
            // 
            // tbAdminRole
            // 
            this.tbAdminRole.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.tbAdminRole, 2);
            this.tbAdminRole.Location = new System.Drawing.Point(113, 58);
            this.tbAdminRole.Name = "tbAdminRole";
            this.tbAdminRole.Size = new System.Drawing.Size(322, 20);
            this.tbAdminRole.TabIndex = 7;
            // 
            // tbPassword
            // 
            this.tbPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.tbPassword, 2);
            this.tbPassword.Location = new System.Drawing.Point(113, 84);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.Size = new System.Drawing.Size(322, 20);
            this.tbPassword.TabIndex = 8;
            // 
            // tbParentGroupId
            // 
            this.tbParentGroupId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbParentGroupId.Location = new System.Drawing.Point(142, 111);
            this.tbParentGroupId.Name = "tbParentGroupId";
            this.tbParentGroupId.ReadOnly = true;
            this.tbParentGroupId.Size = new System.Drawing.Size(293, 20);
            this.tbParentGroupId.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(56, 142);
            this.label6.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(51, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Group Id:";
            // 
            // tbGroupId
            // 
            this.tbGroupId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.tbGroupId, 2);
            this.tbGroupId.Location = new System.Drawing.Point(113, 139);
            this.tbGroupId.Name = "tbGroupId";
            this.tbGroupId.ReadOnly = true;
            this.tbGroupId.Size = new System.Drawing.Size(322, 20);
            this.tbGroupId.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(37, 6);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Group Name:";
            // 
            // cboLanguage1
            // 
            this.cboLanguage1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboLanguage1.FormattingEnabled = true;
            this.cboLanguage1.Location = new System.Drawing.Point(142, 30);
            this.cboLanguage1.Name = "cboLanguage1";
            this.cboLanguage1.Size = new System.Drawing.Size(293, 21);
            this.cboLanguage1.TabIndex = 13;
            this.cboLanguage1.TooltipText = "";
            this.cboLanguage1.SelectedIndexChanged += new System.EventHandler(this.CboLanguage1_SelectedIndexChanged_1);
            // 
            // imgLanguageFlag
            // 
            this.imgLanguageFlag.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imgLanguageFlag.Location = new System.Drawing.Point(113, 29);
            this.imgLanguageFlag.Name = "imgLanguageFlag";
            this.imgLanguageFlag.Size = new System.Drawing.Size(23, 23);
            this.imgLanguageFlag.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.imgLanguageFlag.TabIndex = 50;
            this.imgLanguageFlag.TabStop = false;
            // 
            // lGroupCreatorUser
            // 
            this.lGroupCreatorUser.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lGroupCreatorUser.AutoSize = true;
            this.lGroupCreatorUser.Location = new System.Drawing.Point(6, 168);
            this.lGroupCreatorUser.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lGroupCreatorUser.Name = "lGroupCreatorUser";
            this.lGroupCreatorUser.Size = new System.Drawing.Size(101, 13);
            this.lGroupCreatorUser.TabIndex = 52;
            this.lGroupCreatorUser.Text = "Creator (Username):";
            // 
            // tbGroupCreatorUsername
            // 
            this.tbGroupCreatorUsername.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.tbGroupCreatorUsername, 2);
            this.tbGroupCreatorUsername.Location = new System.Drawing.Point(113, 165);
            this.tbGroupCreatorUsername.Name = "tbGroupCreatorUsername";
            this.tbGroupCreatorUsername.Size = new System.Drawing.Size(322, 20);
            this.tbGroupCreatorUsername.TabIndex = 53;
            // 
            // bOk
            // 
            this.bOk.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.bOk.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.bOk, 3);
            this.bOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.bOk.Location = new System.Drawing.Point(181, 281);
            this.bOk.Name = "bOk";
            this.bOk.Size = new System.Drawing.Size(75, 23);
            this.bOk.TabIndex = 12;
            this.bOk.Text = "OK";
            this.bOk.UseVisualStyleBackColor = true;
            this.bOk.Click += new System.EventHandler(this.BOk_Click);
            // 
            // cbIsPublic
            // 
            this.cbIsPublic.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cbIsPublic.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.cbIsPublic, 2);
            this.cbIsPublic.Location = new System.Drawing.Point(113, 258);
            this.cbIsPublic.Name = "cbIsPublic";
            this.cbIsPublic.Size = new System.Drawing.Size(66, 17);
            this.cbIsPublic.TabIndex = 51;
            this.cbIsPublic.Text = "Is Public";
            this.cbIsPublic.UseVisualStyleBackColor = true;
            // 
            // bParentGroupId
            // 
            this.bParentGroupId.AutoSize = true;
            this.bParentGroupId.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bParentGroupId.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bParentGroupId.Location = new System.Drawing.Point(113, 110);
            this.bParentGroupId.Name = "bParentGroupId";
            this.bParentGroupId.Size = new System.Drawing.Size(23, 23);
            this.bParentGroupId.TabIndex = 56;
            this.bParentGroupId.Text = "?";
            this.bParentGroupId.UseVisualStyleBackColor = true;
            this.bParentGroupId.Click += new System.EventHandler(this.bParentGroupId_Click);
            // 
            // tbDescription
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.tbDescription, 2);
            this.tbDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbDescription.Location = new System.Drawing.Point(113, 191);
            this.tbDescription.Multiline = true;
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.Size = new System.Drawing.Size(322, 61);
            this.tbDescription.TabIndex = 55;
            // 
            // lDescription
            // 
            this.lDescription.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lDescription.AutoSize = true;
            this.lDescription.Location = new System.Drawing.Point(44, 215);
            this.lDescription.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lDescription.Name = "lDescription";
            this.lDescription.Size = new System.Drawing.Size(63, 13);
            this.lDescription.TabIndex = 54;
            this.lDescription.Text = "Description:";
            // 
            // ucSINnerGroupCreate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "ucSINnerGroupCreate";
            this.Size = new System.Drawing.Size(438, 307);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgLanguageFlag)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbGroupname;
        private System.Windows.Forms.TextBox tbAdminRole;
        private System.Windows.Forms.MaskedTextBox tbPassword;
        private System.Windows.Forms.TextBox tbParentGroupId;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbGroupId;
        private System.Windows.Forms.Button bOk;
        private ElasticComboBox cboLanguage1;
        private System.Windows.Forms.PictureBox imgLanguageFlag;
        private System.Windows.Forms.CheckBox cbIsPublic;
        private System.Windows.Forms.Label lGroupCreatorUser;
        private System.Windows.Forms.TextBox tbGroupCreatorUsername;
        private System.Windows.Forms.Button bParentGroupId;
        private System.Windows.Forms.Label lDescription;
        private System.Windows.Forms.TextBox tbDescription;
    }
}
