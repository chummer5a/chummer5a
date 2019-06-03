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
            this.lDescription = new System.Windows.Forms.Label();
            this.tbDescription = new System.Windows.Forms.TextBox();
            this.bOk = new System.Windows.Forms.Button();
            this.cbIsPublic = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgLanguageFlag)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.tbGroupname, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tbAdminRole, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.tbPassword, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.tbParentGroupId, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.tbGroupId, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.cboLanguage1, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.imgLanguageFlag, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lGroupCreatorUser, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.tbGroupCreatorUsername, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.lDescription, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.tbDescription, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.bOk, 2, 10);
            this.tableLayoutPanel1.Controls.Add(this.cbIsPublic, 1, 9);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.MinimumSize = new System.Drawing.Size(350, 200);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 12;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(438, 307);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "preferred language:";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 61);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "AdminRole:";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 87);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Password:";
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 113);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(85, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Parent Group Id:";
            // 
            // tbGroupname
            // 
            this.tbGroupname.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.tbGroupname, 2);
            this.tbGroupname.Location = new System.Drawing.Point(156, 3);
            this.tbGroupname.Name = "tbGroupname";
            this.tbGroupname.Size = new System.Drawing.Size(279, 20);
            this.tbGroupname.TabIndex = 5;
            // 
            // tbAdminRole
            // 
            this.tbAdminRole.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.tbAdminRole, 2);
            this.tbAdminRole.Location = new System.Drawing.Point(156, 58);
            this.tbAdminRole.Name = "tbAdminRole";
            this.tbAdminRole.Size = new System.Drawing.Size(279, 20);
            this.tbAdminRole.TabIndex = 7;
            // 
            // tbPassword
            // 
            this.tbPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.tbPassword, 2);
            this.tbPassword.Location = new System.Drawing.Point(156, 84);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.Size = new System.Drawing.Size(279, 20);
            this.tbPassword.TabIndex = 8;
            // 
            // tbParentGroupId
            // 
            this.tbParentGroupId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.tbParentGroupId, 2);
            this.tbParentGroupId.Location = new System.Drawing.Point(156, 110);
            this.tbParentGroupId.Name = "tbParentGroupId";
            this.tbParentGroupId.Size = new System.Drawing.Size(279, 20);
            this.tbParentGroupId.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 139);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(51, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Group Id:";
            // 
            // tbGroupId
            // 
            this.tbGroupId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.tbGroupId, 2);
            this.tbGroupId.Location = new System.Drawing.Point(156, 136);
            this.tbGroupId.Name = "tbGroupId";
            this.tbGroupId.Size = new System.Drawing.Size(279, 20);
            this.tbGroupId.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Groupname:";
            // 
            // cboLanguage1
            // 
            this.cboLanguage1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboLanguage1.FormattingEnabled = true;
            this.cboLanguage1.Location = new System.Drawing.Point(199, 29);
            this.cboLanguage1.Name = "cboLanguage1";
            this.cboLanguage1.Size = new System.Drawing.Size(236, 21);
            this.cboLanguage1.TabIndex = 13;
            this.cboLanguage1.TooltipText = "";
            this.cboLanguage1.SelectedIndexChanged += new System.EventHandler(this.CboLanguage1_SelectedIndexChanged_1);
            // 
            // imgLanguageFlag
            // 
            this.imgLanguageFlag.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.imgLanguageFlag.Location = new System.Drawing.Point(156, 29);
            this.imgLanguageFlag.Name = "imgLanguageFlag";
            this.imgLanguageFlag.Size = new System.Drawing.Size(37, 23);
            this.imgLanguageFlag.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.imgLanguageFlag.TabIndex = 50;
            this.imgLanguageFlag.TabStop = false;
            // 
            // lGroupCreatorUser
            // 
            this.lGroupCreatorUser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lGroupCreatorUser.AutoSize = true;
            this.lGroupCreatorUser.Location = new System.Drawing.Point(3, 165);
            this.lGroupCreatorUser.Name = "lGroupCreatorUser";
            this.lGroupCreatorUser.Size = new System.Drawing.Size(147, 13);
            this.lGroupCreatorUser.TabIndex = 52;
            this.lGroupCreatorUser.Text = "Creator (Username)";
            // 
            // tbGroupCreatorUsername
            // 
            this.tbGroupCreatorUsername.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.tbGroupCreatorUsername, 2);
            this.tbGroupCreatorUsername.Location = new System.Drawing.Point(156, 162);
            this.tbGroupCreatorUsername.Name = "tbGroupCreatorUsername";
            this.tbGroupCreatorUsername.Size = new System.Drawing.Size(279, 20);
            this.tbGroupCreatorUsername.TabIndex = 53;
            // 
            // lDescription
            // 
            this.lDescription.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lDescription.AutoSize = true;
            this.lDescription.Location = new System.Drawing.Point(3, 206);
            this.lDescription.Name = "lDescription";
            this.tableLayoutPanel1.SetRowSpan(this.lDescription, 2);
            this.lDescription.Size = new System.Drawing.Size(60, 13);
            this.lDescription.TabIndex = 54;
            this.lDescription.Text = "Description";
            // 
            // tbDescription
            // 
            this.tbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.tbDescription, 2);
            this.tbDescription.Location = new System.Drawing.Point(156, 188);
            this.tbDescription.MinimumSize = new System.Drawing.Size(100, 50);
            this.tbDescription.Multiline = true;
            this.tbDescription.Name = "tbDescription";
            this.tableLayoutPanel1.SetRowSpan(this.tbDescription, 2);
            this.tbDescription.Size = new System.Drawing.Size(279, 50);
            this.tbDescription.TabIndex = 55;
            // 
            // bOk
            // 
            this.bOk.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.bOk.AutoSize = true;
            this.bOk.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.SetColumnSpan(this.bOk, 3);
            this.bOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.bOk.Location = new System.Drawing.Point(173, 274);
            this.bOk.Name = "bOk";
            this.bOk.Padding = new System.Windows.Forms.Padding(30, 0, 30, 0);
            this.bOk.Size = new System.Drawing.Size(91, 23);
            this.bOk.TabIndex = 12;
            this.bOk.Text = "Ok";
            this.bOk.UseVisualStyleBackColor = true;
            this.bOk.Click += new System.EventHandler(this.BOk_Click);
            // 
            // cbIsPublic
            // 
            this.cbIsPublic.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cbIsPublic.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.cbIsPublic, 2);
            this.cbIsPublic.Location = new System.Drawing.Point(156, 244);
            this.cbIsPublic.Name = "cbIsPublic";
            this.cbIsPublic.Size = new System.Drawing.Size(66, 17);
            this.cbIsPublic.TabIndex = 51;
            this.cbIsPublic.Text = "Is Public";
            this.cbIsPublic.UseVisualStyleBackColor = true;
            // 
            // ucSINnerGroupCreate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
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
        private System.Windows.Forms.Label lDescription;
        private System.Windows.Forms.TextBox tbDescription;
    }
}
