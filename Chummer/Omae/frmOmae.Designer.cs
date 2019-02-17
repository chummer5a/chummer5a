namespace Chummer
{
    partial class frmOmae
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmOmae));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.cmdRegister = new System.Windows.Forms.Button();
            this.cmdLogin = new System.Windows.Forms.Button();
            this.cmdUpload = new System.Windows.Forms.Button();
            this.cmdSearch = new System.Windows.Forms.Button();
            this.panOmae = new System.Windows.Forms.Panel();
            this.tipTooltip = new TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip();
            this.cmdPasswordReset = new System.Windows.Forms.Button();
            this.cmdMyAccount = new System.Windows.Forms.Button();
            this.cmdUploadLanguage = new System.Windows.Forms.Button();
            this.panLogin = new System.Windows.Forms.Panel();
            this.chkAutoLogin = new System.Windows.Forms.CheckBox();
            this.panLoggedIn = new System.Windows.Forms.Panel();
            this.lblLoggedIn = new System.Windows.Forms.Label();
            this.cboCharacterTypes = new System.Windows.Forms.ComboBox();
            this.cboSortOrder = new System.Windows.Forms.ComboBox();
            this.lblSearchFor = new System.Windows.Forms.Label();
            this.lblSortedBy = new System.Windows.Forms.Label();
            this.lblFilter = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.panFilter = new System.Windows.Forms.Panel();
            this.cboFilterMode = new System.Windows.Forms.ComboBox();
            this.lblFilterMode = new System.Windows.Forms.Label();
            this.cmdFilterClear = new System.Windows.Forms.Button();
            this.cmdFilterToggle = new System.Windows.Forms.Button();
            this.cboFilterQuality3 = new System.Windows.Forms.ComboBox();
            this.lblFilterQuality3 = new System.Windows.Forms.Label();
            this.cboFilterQuality2 = new System.Windows.Forms.ComboBox();
            this.lblFilterQuality2 = new System.Windows.Forms.Label();
            this.cboFilterQuality1 = new System.Windows.Forms.ComboBox();
            this.lblFilterQuality1 = new System.Windows.Forms.Label();
            this.txtFilterUser = new System.Windows.Forms.TextBox();
            this.lblFilterUser = new System.Windows.Forms.Label();
            this.cboFilterMetavariant = new System.Windows.Forms.ComboBox();
            this.lblFilterMetavariant = new System.Windows.Forms.Label();
            this.cboFilterMetatype = new System.Windows.Forms.ComboBox();
            this.lblFilterMetatype = new System.Windows.Forms.Label();
            this.cmdCompress = new System.Windows.Forms.Button();
            this.cmdCompressData = new System.Windows.Forms.Button();
            this.panLogin.SuspendLayout();
            this.panLoggedIn.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.panFilter.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 12);
            this.label1.TabIndex = 0;
            this.label1.Tag = "Label_Omae_Username";
            this.label1.Text = "Username:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(158, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 12);
            this.label2.TabIndex = 2;
            this.label2.Tag = "Label_Omae_Password";
            this.label2.Text = "Password:";
            // 
            // txtUserName
            // 
            this.txtUserName.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUserName.Location = new System.Drawing.Point(60, 2);
            this.txtUserName.MaxLength = 50;
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(92, 18);
            this.txtUserName.TabIndex = 1;
            // 
            // txtPassword
            // 
            this.txtPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPassword.Location = new System.Drawing.Point(214, 2);
            this.txtPassword.MaxLength = 40;
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(92, 18);
            this.txtPassword.TabIndex = 3;
            // 
            // cmdRegister
            // 
            this.cmdRegister.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdRegister.Location = new System.Drawing.Point(504, 1);
            this.cmdRegister.Name = "cmdRegister";
            this.cmdRegister.Size = new System.Drawing.Size(75, 20);
            this.cmdRegister.TabIndex = 6;
            this.cmdRegister.Tag = "Button_Omae_Register";
            this.cmdRegister.Text = "Register";
            this.tipTooltip.SetToolTip(this.cmdRegister, "Register an Omae account. You only need to register if you want to upload charact" +
        "ers. No registration is need to browse and download.");
            this.cmdRegister.UseVisualStyleBackColor = true;
            this.cmdRegister.Click += new System.EventHandler(this.cmdRegister_Click);
            // 
            // cmdLogin
            // 
            this.cmdLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdLogin.Location = new System.Drawing.Point(423, 1);
            this.cmdLogin.Name = "cmdLogin";
            this.cmdLogin.Size = new System.Drawing.Size(75, 20);
            this.cmdLogin.TabIndex = 5;
            this.cmdLogin.Tag = "Button_Omae_Login";
            this.cmdLogin.Text = "Login";
            this.tipTooltip.SetToolTip(this.cmdLogin, "Login to Omae.");
            this.cmdLogin.UseVisualStyleBackColor = true;
            this.cmdLogin.Click += new System.EventHandler(this.cmdLogin_Click);
            // 
            // cmdUpload
            // 
            this.cmdUpload.Location = new System.Drawing.Point(531, 34);
            this.cmdUpload.Name = "cmdUpload";
            this.cmdUpload.Size = new System.Drawing.Size(139, 23);
            this.cmdUpload.TabIndex = 6;
            this.cmdUpload.Tag = "Button_Omae_Upload";
            this.cmdUpload.Text = "&Upload a Character";
            this.cmdUpload.UseVisualStyleBackColor = true;
            this.cmdUpload.Click += new System.EventHandler(this.cmdUpload_Click);
            // 
            // cmdSearch
            // 
            this.cmdSearch.Location = new System.Drawing.Point(397, 33);
            this.cmdSearch.Name = "cmdSearch";
            this.cmdSearch.Size = new System.Drawing.Size(75, 23);
            this.cmdSearch.TabIndex = 5;
            this.cmdSearch.Tag = "Button_Omae_Search";
            this.cmdSearch.Text = "&Search";
            this.cmdSearch.UseVisualStyleBackColor = true;
            this.cmdSearch.Click += new System.EventHandler(this.cmdSearch_Click);
            // 
            // panOmae
            // 
            this.panOmae.AutoScroll = true;
            this.panOmae.Location = new System.Drawing.Point(3, 40);
            this.panOmae.Name = "panOmae";
            this.panOmae.Size = new System.Drawing.Size(760, 394);
            this.panOmae.TabIndex = 7;
            // 
            // tipTooltip
            // 
            this.tipTooltip.AutoPopDelay = 10000;
            this.tipTooltip.InitialDelay = 250;
            this.tipTooltip.IsBalloon = true;
            this.tipTooltip.ReshowDelay = 100;
            this.tipTooltip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.tipTooltip.ToolTipTitle = "Omae Help";
            // 
            // cmdPasswordReset
            // 
            this.cmdPasswordReset.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdPasswordReset.Location = new System.Drawing.Point(585, 1);
            this.cmdPasswordReset.Name = "cmdPasswordReset";
            this.cmdPasswordReset.Size = new System.Drawing.Size(75, 20);
            this.cmdPasswordReset.TabIndex = 7;
            this.cmdPasswordReset.Tag = "Button_Omae_PasswordReset";
            this.cmdPasswordReset.Text = "PW Reset";
            this.tipTooltip.SetToolTip(this.cmdPasswordReset, "Forgot your password? Click here to have a new password emailed to you.");
            this.cmdPasswordReset.UseVisualStyleBackColor = true;
            this.cmdPasswordReset.Click += new System.EventHandler(this.cmdPasswordReset_Click);
            // 
            // cmdMyAccount
            // 
            this.cmdMyAccount.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdMyAccount.Location = new System.Drawing.Point(205, 1);
            this.cmdMyAccount.Name = "cmdMyAccount";
            this.cmdMyAccount.Size = new System.Drawing.Size(75, 20);
            this.cmdMyAccount.TabIndex = 8;
            this.cmdMyAccount.Tag = "Button_Omae_MyAccount";
            this.cmdMyAccount.Text = "My Account";
            this.tipTooltip.SetToolTip(this.cmdMyAccount, "Edit your Omae account information.");
            this.cmdMyAccount.UseVisualStyleBackColor = true;
            this.cmdMyAccount.Click += new System.EventHandler(this.cmdMyAccount_Click);
            // 
            // cmdUploadLanguage
            // 
            this.cmdUploadLanguage.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdUploadLanguage.Location = new System.Drawing.Point(573, 1);
            this.cmdUploadLanguage.Name = "cmdUploadLanguage";
            this.cmdUploadLanguage.Size = new System.Drawing.Size(89, 20);
            this.cmdUploadLanguage.TabIndex = 9;
            this.cmdUploadLanguage.Text = "Upload Language";
            this.cmdUploadLanguage.UseVisualStyleBackColor = true;
            this.cmdUploadLanguage.Visible = false;
            this.cmdUploadLanguage.Click += new System.EventHandler(this.cmdUploadLanguage_Click);
            // 
            // panLogin
            // 
            this.panLogin.Controls.Add(this.cmdPasswordReset);
            this.panLogin.Controls.Add(this.chkAutoLogin);
            this.panLogin.Controls.Add(this.label1);
            this.panLogin.Controls.Add(this.label2);
            this.panLogin.Controls.Add(this.txtUserName);
            this.panLogin.Controls.Add(this.txtPassword);
            this.panLogin.Controls.Add(this.cmdLogin);
            this.panLogin.Controls.Add(this.cmdRegister);
            this.panLogin.Location = new System.Drawing.Point(12, 5);
            this.panLogin.Name = "panLogin";
            this.panLogin.Size = new System.Drawing.Size(662, 23);
            this.panLogin.TabIndex = 0;
            // 
            // chkAutoLogin
            // 
            this.chkAutoLogin.AutoSize = true;
            this.chkAutoLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkAutoLogin.Location = new System.Drawing.Point(312, 3);
            this.chkAutoLogin.Name = "chkAutoLogin";
            this.chkAutoLogin.Size = new System.Drawing.Size(105, 16);
            this.chkAutoLogin.TabIndex = 4;
            this.chkAutoLogin.Tag = "Checkbox_Omae_AutoLogin";
            this.chkAutoLogin.Text = "Login Automatically";
            this.chkAutoLogin.UseVisualStyleBackColor = true;
            // 
            // panLoggedIn
            // 
            this.panLoggedIn.Controls.Add(this.cmdUploadLanguage);
            this.panLoggedIn.Controls.Add(this.cmdMyAccount);
            this.panLoggedIn.Controls.Add(this.lblLoggedIn);
            this.panLoggedIn.Location = new System.Drawing.Point(12, 5);
            this.panLoggedIn.Name = "panLoggedIn";
            this.panLoggedIn.Size = new System.Drawing.Size(662, 23);
            this.panLoggedIn.TabIndex = 11;
            this.panLoggedIn.Visible = false;
            // 
            // lblLoggedIn
            // 
            this.lblLoggedIn.AutoSize = true;
            this.lblLoggedIn.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLoggedIn.Location = new System.Drawing.Point(3, 3);
            this.lblLoggedIn.Name = "lblLoggedIn";
            this.lblLoggedIn.Size = new System.Drawing.Size(109, 12);
            this.lblLoggedIn.TabIndex = 1;
            this.lblLoggedIn.Text = "Logged in as [UserName]";
            // 
            // cboCharacterTypes
            // 
            this.cboCharacterTypes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCharacterTypes.FormattingEnabled = true;
            this.cboCharacterTypes.Location = new System.Drawing.Point(76, 34);
            this.cboCharacterTypes.Name = "cboCharacterTypes";
            this.cboCharacterTypes.Size = new System.Drawing.Size(149, 21);
            this.cboCharacterTypes.TabIndex = 2;
            this.cboCharacterTypes.SelectedIndexChanged += new System.EventHandler(this.cboCharacterTypes_SelectedIndexChanged);
            // 
            // cboSortOrder
            // 
            this.cboSortOrder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSortOrder.FormattingEnabled = true;
            this.cboSortOrder.Location = new System.Drawing.Point(287, 34);
            this.cboSortOrder.Name = "cboSortOrder";
            this.cboSortOrder.Size = new System.Drawing.Size(104, 21);
            this.cboSortOrder.TabIndex = 4;
            // 
            // lblSearchFor
            // 
            this.lblSearchFor.AutoSize = true;
            this.lblSearchFor.Location = new System.Drawing.Point(14, 37);
            this.lblSearchFor.Name = "lblSearchFor";
            this.lblSearchFor.Size = new System.Drawing.Size(56, 13);
            this.lblSearchFor.TabIndex = 1;
            this.lblSearchFor.Tag = "Label_Omae_SearchFor";
            this.lblSearchFor.Text = "Search for";
            // 
            // lblSortedBy
            // 
            this.lblSortedBy.AutoSize = true;
            this.lblSortedBy.Location = new System.Drawing.Point(231, 37);
            this.lblSortedBy.Name = "lblSortedBy";
            this.lblSortedBy.Size = new System.Drawing.Size(50, 13);
            this.lblSortedBy.TabIndex = 3;
            this.lblSortedBy.Tag = "Label_Omae_SortedBy";
            this.lblSortedBy.Text = "sorted by";
            // 
            // lblFilter
            // 
            this.lblFilter.AutoSize = true;
            this.lblFilter.Location = new System.Drawing.Point(-1, 10);
            this.lblFilter.Name = "lblFilter";
            this.lblFilter.Size = new System.Drawing.Size(46, 13);
            this.lblFilter.TabIndex = 0;
            this.lblFilter.Tag = "Label_Omae_Filtering";
            this.lblFilter.Text = "Filtering:";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Controls.Add(this.panFilter);
            this.flowLayoutPanel1.Controls.Add(this.panOmae);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(12, 61);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(770, 434);
            this.flowLayoutPanel1.TabIndex = 12;
            this.flowLayoutPanel1.Resize += new System.EventHandler(this.flowLayoutPanel1_Resize);
            // 
            // panFilter
            // 
            this.panFilter.Controls.Add(this.cboFilterMode);
            this.panFilter.Controls.Add(this.lblFilterMode);
            this.panFilter.Controls.Add(this.cmdFilterClear);
            this.panFilter.Controls.Add(this.cmdFilterToggle);
            this.panFilter.Controls.Add(this.cboFilterQuality3);
            this.panFilter.Controls.Add(this.lblFilterQuality3);
            this.panFilter.Controls.Add(this.cboFilterQuality2);
            this.panFilter.Controls.Add(this.lblFilterQuality2);
            this.panFilter.Controls.Add(this.cboFilterQuality1);
            this.panFilter.Controls.Add(this.lblFilterQuality1);
            this.panFilter.Controls.Add(this.txtFilterUser);
            this.panFilter.Controls.Add(this.lblFilterUser);
            this.panFilter.Controls.Add(this.cboFilterMetavariant);
            this.panFilter.Controls.Add(this.lblFilterMetavariant);
            this.panFilter.Controls.Add(this.cboFilterMetatype);
            this.panFilter.Controls.Add(this.lblFilterMetatype);
            this.panFilter.Controls.Add(this.lblFilter);
            this.panFilter.Location = new System.Drawing.Point(3, 3);
            this.panFilter.Name = "panFilter";
            this.panFilter.Size = new System.Drawing.Size(760, 31);
            this.panFilter.TabIndex = 0;
            // 
            // cboFilterMode
            // 
            this.cboFilterMode.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cboFilterMode.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cboFilterMode.DropDownWidth = 225;
            this.cboFilterMode.FormattingEnabled = true;
            this.cboFilterMode.Location = new System.Drawing.Point(76, 92);
            this.cboFilterMode.Name = "cboFilterMode";
            this.cboFilterMode.Size = new System.Drawing.Size(141, 21);
            this.cboFilterMode.Sorted = true;
            this.cboFilterMode.TabIndex = 16;
            // 
            // lblFilterMode
            // 
            this.lblFilterMode.AutoSize = true;
            this.lblFilterMode.Location = new System.Drawing.Point(4, 95);
            this.lblFilterMode.Name = "lblFilterMode";
            this.lblFilterMode.Size = new System.Drawing.Size(74, 13);
            this.lblFilterMode.TabIndex = 15;
            this.lblFilterMode.Tag = "Label_Omae_CurrentMode";
            this.lblFilterMode.Text = "Current Mode:";
            // 
            // cmdFilterClear
            // 
            this.cmdFilterClear.Location = new System.Drawing.Point(130, 5);
            this.cmdFilterClear.Name = "cmdFilterClear";
            this.cmdFilterClear.Size = new System.Drawing.Size(75, 23);
            this.cmdFilterClear.TabIndex = 14;
            this.cmdFilterClear.Tag = "Button_Omae_ClearFilter";
            this.cmdFilterClear.Text = "Clear Filter";
            this.cmdFilterClear.UseVisualStyleBackColor = true;
            this.cmdFilterClear.Click += new System.EventHandler(this.cmdFilterClear_Click);
            // 
            // cmdFilterToggle
            // 
            this.cmdFilterToggle.Location = new System.Drawing.Point(51, 5);
            this.cmdFilterToggle.Name = "cmdFilterToggle";
            this.cmdFilterToggle.Size = new System.Drawing.Size(75, 23);
            this.cmdFilterToggle.TabIndex = 13;
            this.cmdFilterToggle.Tag = "Button_Omae_ShowFilter";
            this.cmdFilterToggle.Text = "Show Filter";
            this.cmdFilterToggle.UseVisualStyleBackColor = true;
            this.cmdFilterToggle.Click += new System.EventHandler(this.cmdFilterToggle_Click);
            // 
            // cboFilterQuality3
            // 
            this.cboFilterQuality3.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cboFilterQuality3.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cboFilterQuality3.DropDownWidth = 225;
            this.cboFilterQuality3.FormattingEnabled = true;
            this.cboFilterQuality3.Location = new System.Drawing.Point(514, 65);
            this.cboFilterQuality3.Name = "cboFilterQuality3";
            this.cboFilterQuality3.Size = new System.Drawing.Size(141, 21);
            this.cboFilterQuality3.Sorted = true;
            this.cboFilterQuality3.TabIndex = 12;
            // 
            // lblFilterQuality3
            // 
            this.lblFilterQuality3.AutoSize = true;
            this.lblFilterQuality3.Location = new System.Drawing.Point(442, 68);
            this.lblFilterQuality3.Name = "lblFilterQuality3";
            this.lblFilterQuality3.Size = new System.Drawing.Size(54, 13);
            this.lblFilterQuality3.TabIndex = 11;
            this.lblFilterQuality3.Tag = "Label_Omae_OrQuality";
            this.lblFilterQuality3.Text = "or Quality:";
            // 
            // cboFilterQuality2
            // 
            this.cboFilterQuality2.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cboFilterQuality2.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cboFilterQuality2.DropDownWidth = 225;
            this.cboFilterQuality2.FormattingEnabled = true;
            this.cboFilterQuality2.Location = new System.Drawing.Point(295, 65);
            this.cboFilterQuality2.Name = "cboFilterQuality2";
            this.cboFilterQuality2.Size = new System.Drawing.Size(141, 21);
            this.cboFilterQuality2.Sorted = true;
            this.cboFilterQuality2.TabIndex = 10;
            // 
            // lblFilterQuality2
            // 
            this.lblFilterQuality2.AutoSize = true;
            this.lblFilterQuality2.Location = new System.Drawing.Point(223, 68);
            this.lblFilterQuality2.Name = "lblFilterQuality2";
            this.lblFilterQuality2.Size = new System.Drawing.Size(54, 13);
            this.lblFilterQuality2.TabIndex = 9;
            this.lblFilterQuality2.Tag = "Label_Omae_OrQuality";
            this.lblFilterQuality2.Text = "or Quality:";
            // 
            // cboFilterQuality1
            // 
            this.cboFilterQuality1.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cboFilterQuality1.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cboFilterQuality1.DropDownWidth = 225;
            this.cboFilterQuality1.FormattingEnabled = true;
            this.cboFilterQuality1.Location = new System.Drawing.Point(76, 65);
            this.cboFilterQuality1.Name = "cboFilterQuality1";
            this.cboFilterQuality1.Size = new System.Drawing.Size(141, 21);
            this.cboFilterQuality1.Sorted = true;
            this.cboFilterQuality1.TabIndex = 8;
            // 
            // lblFilterQuality1
            // 
            this.lblFilterQuality1.AutoSize = true;
            this.lblFilterQuality1.Location = new System.Drawing.Point(4, 68);
            this.lblFilterQuality1.Name = "lblFilterQuality1";
            this.lblFilterQuality1.Size = new System.Drawing.Size(42, 13);
            this.lblFilterQuality1.TabIndex = 7;
            this.lblFilterQuality1.Tag = "Label_Omae_Quality";
            this.lblFilterQuality1.Text = "Quality:";
            // 
            // txtFilterUser
            // 
            this.txtFilterUser.Location = new System.Drawing.Point(514, 38);
            this.txtFilterUser.Name = "txtFilterUser";
            this.txtFilterUser.Size = new System.Drawing.Size(141, 20);
            this.txtFilterUser.TabIndex = 6;
            // 
            // lblFilterUser
            // 
            this.lblFilterUser.AutoSize = true;
            this.lblFilterUser.Location = new System.Drawing.Point(442, 41);
            this.lblFilterUser.Name = "lblFilterUser";
            this.lblFilterUser.Size = new System.Drawing.Size(44, 13);
            this.lblFilterUser.TabIndex = 5;
            this.lblFilterUser.Tag = "Label_Omae_Creator";
            this.lblFilterUser.Text = "Creator:";
            // 
            // cboFilterMetavariant
            // 
            this.cboFilterMetavariant.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cboFilterMetavariant.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cboFilterMetavariant.DropDownWidth = 225;
            this.cboFilterMetavariant.FormattingEnabled = true;
            this.cboFilterMetavariant.Location = new System.Drawing.Point(295, 38);
            this.cboFilterMetavariant.Name = "cboFilterMetavariant";
            this.cboFilterMetavariant.Size = new System.Drawing.Size(141, 21);
            this.cboFilterMetavariant.Sorted = true;
            this.cboFilterMetavariant.TabIndex = 4;
            // 
            // lblFilterMetavariant
            // 
            this.lblFilterMetavariant.AutoSize = true;
            this.lblFilterMetavariant.Location = new System.Drawing.Point(223, 41);
            this.lblFilterMetavariant.Name = "lblFilterMetavariant";
            this.lblFilterMetavariant.Size = new System.Drawing.Size(66, 13);
            this.lblFilterMetavariant.TabIndex = 3;
            this.lblFilterMetavariant.Tag = "Label_Metavariant";
            this.lblFilterMetavariant.Text = "Metavariant:";
            // 
            // cboFilterMetatype
            // 
            this.cboFilterMetatype.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cboFilterMetatype.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cboFilterMetatype.DropDownWidth = 225;
            this.cboFilterMetatype.FormattingEnabled = true;
            this.cboFilterMetatype.Location = new System.Drawing.Point(76, 38);
            this.cboFilterMetatype.Name = "cboFilterMetatype";
            this.cboFilterMetatype.Size = new System.Drawing.Size(141, 21);
            this.cboFilterMetatype.Sorted = true;
            this.cboFilterMetatype.TabIndex = 2;
            // 
            // lblFilterMetatype
            // 
            this.lblFilterMetatype.AutoSize = true;
            this.lblFilterMetatype.Location = new System.Drawing.Point(4, 41);
            this.lblFilterMetatype.Name = "lblFilterMetatype";
            this.lblFilterMetatype.Size = new System.Drawing.Size(54, 13);
            this.lblFilterMetatype.TabIndex = 1;
            this.lblFilterMetatype.Tag = "Label_Metatype";
            this.lblFilterMetatype.Text = "Metatype:";
            // 
            // cmdCompress
            // 
            this.cmdCompress.Location = new System.Drawing.Point(686, 34);
            this.cmdCompress.Name = "cmdCompress";
            this.cmdCompress.Size = new System.Drawing.Size(89, 23);
            this.cmdCompress.TabIndex = 13;
            this.cmdCompress.Tag = "Button_Omae_Search";
            this.cmdCompress.Text = "Compress Files";
            this.cmdCompress.UseVisualStyleBackColor = true;
            this.cmdCompress.Visible = false;
            this.cmdCompress.Click += new System.EventHandler(this.cmdCompress_Click);
            // 
            // cmdCompressData
            // 
            this.cmdCompressData.Location = new System.Drawing.Point(686, 10);
            this.cmdCompressData.Name = "cmdCompressData";
            this.cmdCompressData.Size = new System.Drawing.Size(89, 23);
            this.cmdCompressData.TabIndex = 14;
            this.cmdCompressData.Tag = "Button_Omae_Search";
            this.cmdCompressData.Text = "Compress Data";
            this.cmdCompressData.UseVisualStyleBackColor = true;
            this.cmdCompressData.Visible = false;
            this.cmdCompressData.Click += new System.EventHandler(this.cmdCompressData_Click);
            // 
            // frmOmae
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(794, 507);
            this.Controls.Add(this.cmdCompressData);
            this.Controls.Add(this.cmdCompress);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.lblSortedBy);
            this.Controls.Add(this.lblSearchFor);
            this.Controls.Add(this.cboSortOrder);
            this.Controls.Add(this.cboCharacterTypes);
            this.Controls.Add(this.cmdSearch);
            this.Controls.Add(this.cmdUpload);
            this.Controls.Add(this.panLogin);
            this.Controls.Add(this.panLoggedIn);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(810, 545);
            this.Name = "frmOmae";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Tag = "Title_Omae";
            this.Text = "Chummer: Omae Character Exchange";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmOmae_FormClosing);
            this.Load += new System.EventHandler(this.frmOmae_Load);
            this.Resize += new System.EventHandler(this.frmOmae_Resize);
            this.panLogin.ResumeLayout(false);
            this.panLogin.PerformLayout();
            this.panLoggedIn.ResumeLayout(false);
            this.panLoggedIn.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.panFilter.ResumeLayout(false);
            this.panFilter.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button cmdRegister;
        private System.Windows.Forms.Button cmdLogin;
        private System.Windows.Forms.Button cmdUpload;
        private System.Windows.Forms.Button cmdSearch;
        private System.Windows.Forms.Panel panOmae;
        private TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip tipTooltip;
        private System.Windows.Forms.Panel panLogin;
        private System.Windows.Forms.Panel panLoggedIn;
        private System.Windows.Forms.Label lblLoggedIn;
        private System.Windows.Forms.ComboBox cboCharacterTypes;
        private System.Windows.Forms.ComboBox cboSortOrder;
        private System.Windows.Forms.Label lblSearchFor;
        private System.Windows.Forms.Label lblSortedBy;
        private System.Windows.Forms.CheckBox chkAutoLogin;
        private System.Windows.Forms.Label lblFilter;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel panFilter;
        private System.Windows.Forms.Button cmdFilterClear;
        private System.Windows.Forms.Button cmdFilterToggle;
        private System.Windows.Forms.ComboBox cboFilterQuality3;
        private System.Windows.Forms.Label lblFilterQuality3;
        private System.Windows.Forms.ComboBox cboFilterQuality2;
        private System.Windows.Forms.Label lblFilterQuality2;
        private System.Windows.Forms.ComboBox cboFilterQuality1;
        private System.Windows.Forms.Label lblFilterQuality1;
        private System.Windows.Forms.TextBox txtFilterUser;
        private System.Windows.Forms.Label lblFilterUser;
        private System.Windows.Forms.ComboBox cboFilterMetavariant;
        private System.Windows.Forms.Label lblFilterMetavariant;
        private System.Windows.Forms.ComboBox cboFilterMetatype;
        private System.Windows.Forms.Label lblFilterMetatype;
        private System.Windows.Forms.ComboBox cboFilterMode;
        private System.Windows.Forms.Label lblFilterMode;
        private System.Windows.Forms.Button cmdPasswordReset;
        private System.Windows.Forms.Button cmdMyAccount;
        private System.Windows.Forms.Button cmdUploadLanguage;
        private System.Windows.Forms.Button cmdCompress;
        private System.Windows.Forms.Button cmdCompressData;
    }
}