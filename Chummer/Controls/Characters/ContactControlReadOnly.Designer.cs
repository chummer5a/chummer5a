namespace Chummer
{
    partial class ContactControlReadOnly
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
                UnbindContactControl();
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
            this.components = new System.ComponentModel.Container();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblLoyalty = new System.Windows.Forms.Label();
            this.txtHobbiesVice = new System.Windows.Forms.TextBox();
            this.txtPreferredPayment = new System.Windows.Forms.TextBox();
            this.txtPersonalLife = new System.Windows.Forms.TextBox();
            this.txtAge = new System.Windows.Forms.TextBox();
            this.txtGender = new System.Windows.Forms.TextBox();
            this.txtMetatype = new System.Windows.Forms.TextBox();
            this.txtType = new System.Windows.Forms.TextBox();
            this.txtArchetype = new System.Windows.Forms.TextBox();
            this.txtLocation = new System.Windows.Forms.TextBox();
            this.lblNameLabel = new System.Windows.Forms.Label();
            this.lblLocationLabel = new System.Windows.Forms.Label();
            this.lblArchtypeLabel = new System.Windows.Forms.Label();
            this.lblConnectionLabel = new System.Windows.Forms.Label();
            this.lblLoyaltyLabel = new System.Windows.Forms.Label();
            this.flpCheckboxes = new System.Windows.Forms.FlowLayoutPanel();
            this.cmdNotes = new Chummer.ButtonWithToolTip(this.components);
            this.cmdLink = new Chummer.ButtonWithToolTip(this.components);
            this.chkFree = new Chummer.ColorableCheckBox(this.components);
            this.chkGroup = new Chummer.ColorableCheckBox(this.components);
            this.chkBlackmail = new Chummer.ColorableCheckBox(this.components);
            this.chkFamily = new Chummer.ColorableCheckBox(this.components);
            this.lblTypeLabel = new System.Windows.Forms.Label();
            this.lblMetatypeLabel = new System.Windows.Forms.Label();
            this.lblGenderLabel = new System.Windows.Forms.Label();
            this.lblAgeLabel = new System.Windows.Forms.Label();
            this.lblPersonalLifeLabel = new System.Windows.Forms.Label();
            this.lblPreferredPaymentLabel = new System.Windows.Forms.Label();
            this.lblHobbiesViceLabel = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblConnection = new System.Windows.Forms.Label();
            this.tlpMain.SuspendLayout();
            this.flpCheckboxes.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.lblLoyalty, 1, 4);
            this.tlpMain.Controls.Add(this.txtHobbiesVice, 1, 12);
            this.tlpMain.Controls.Add(this.txtPreferredPayment, 1, 11);
            this.tlpMain.Controls.Add(this.txtPersonalLife, 1, 10);
            this.tlpMain.Controls.Add(this.txtAge, 1, 9);
            this.tlpMain.Controls.Add(this.txtGender, 1, 8);
            this.tlpMain.Controls.Add(this.txtMetatype, 1, 7);
            this.tlpMain.Controls.Add(this.txtType, 1, 6);
            this.tlpMain.Controls.Add(this.txtArchetype, 1, 2);
            this.tlpMain.Controls.Add(this.txtLocation, 1, 1);
            this.tlpMain.Controls.Add(this.lblNameLabel, 0, 0);
            this.tlpMain.Controls.Add(this.lblLocationLabel, 0, 1);
            this.tlpMain.Controls.Add(this.lblArchtypeLabel, 0, 2);
            this.tlpMain.Controls.Add(this.lblConnectionLabel, 0, 3);
            this.tlpMain.Controls.Add(this.lblLoyaltyLabel, 0, 4);
            this.tlpMain.Controls.Add(this.flpCheckboxes, 0, 5);
            this.tlpMain.Controls.Add(this.lblTypeLabel, 0, 6);
            this.tlpMain.Controls.Add(this.lblMetatypeLabel, 0, 7);
            this.tlpMain.Controls.Add(this.lblGenderLabel, 0, 8);
            this.tlpMain.Controls.Add(this.lblAgeLabel, 0, 9);
            this.tlpMain.Controls.Add(this.lblPersonalLifeLabel, 0, 10);
            this.tlpMain.Controls.Add(this.lblPreferredPaymentLabel, 0, 11);
            this.tlpMain.Controls.Add(this.lblHobbiesViceLabel, 0, 12);
            this.tlpMain.Controls.Add(this.txtName, 1, 0);
            this.tlpMain.Controls.Add(this.lblConnection, 1, 3);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 13;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(324, 357);
            this.tlpMain.TabIndex = 0;
            // 
            // lblLoyalty
            // 
            this.lblLoyalty.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblLoyalty.AutoSize = true;
            this.lblLoyalty.Location = new System.Drawing.Point(106, 112);
            this.lblLoyalty.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLoyalty.Name = "lblLoyalty";
            this.lblLoyalty.Size = new System.Drawing.Size(19, 13);
            this.lblLoyalty.TabIndex = 68;
            this.lblLoyalty.Text = "[0]";
            // 
            // txtHobbiesVice
            // 
            this.txtHobbiesVice.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtHobbiesVice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtHobbiesVice.Location = new System.Drawing.Point(106, 335);
            this.txtHobbiesVice.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.txtHobbiesVice.Multiline = true;
            this.txtHobbiesVice.Name = "txtHobbiesVice";
            this.txtHobbiesVice.ReadOnly = true;
            this.txtHobbiesVice.Size = new System.Drawing.Size(215, 16);
            this.txtHobbiesVice.TabIndex = 66;
            this.txtHobbiesVice.Text = "[Hobbies/Vice]";
            // 
            // txtPreferredPayment
            // 
            this.txtPreferredPayment.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtPreferredPayment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPreferredPayment.Location = new System.Drawing.Point(106, 307);
            this.txtPreferredPayment.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.txtPreferredPayment.Multiline = true;
            this.txtPreferredPayment.Name = "txtPreferredPayment";
            this.txtPreferredPayment.ReadOnly = true;
            this.txtPreferredPayment.Size = new System.Drawing.Size(215, 16);
            this.txtPreferredPayment.TabIndex = 65;
            this.txtPreferredPayment.Text = "[Preferred Payment]";
            // 
            // txtPersonalLife
            // 
            this.txtPersonalLife.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtPersonalLife.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPersonalLife.Location = new System.Drawing.Point(106, 279);
            this.txtPersonalLife.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.txtPersonalLife.Multiline = true;
            this.txtPersonalLife.Name = "txtPersonalLife";
            this.txtPersonalLife.ReadOnly = true;
            this.txtPersonalLife.Size = new System.Drawing.Size(215, 16);
            this.txtPersonalLife.TabIndex = 64;
            this.txtPersonalLife.Text = "[Personal Life]";
            // 
            // txtAge
            // 
            this.txtAge.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtAge.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtAge.Location = new System.Drawing.Point(106, 251);
            this.txtAge.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.txtAge.Multiline = true;
            this.txtAge.Name = "txtAge";
            this.txtAge.ReadOnly = true;
            this.txtAge.Size = new System.Drawing.Size(215, 16);
            this.txtAge.TabIndex = 63;
            this.txtAge.Text = "[Age]";
            // 
            // txtGender
            // 
            this.txtGender.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtGender.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtGender.Location = new System.Drawing.Point(106, 223);
            this.txtGender.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.txtGender.Multiline = true;
            this.txtGender.Name = "txtGender";
            this.txtGender.ReadOnly = true;
            this.txtGender.Size = new System.Drawing.Size(215, 16);
            this.txtGender.TabIndex = 62;
            this.txtGender.Text = "[Gender]";
            // 
            // txtMetatype
            // 
            this.txtMetatype.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMetatype.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMetatype.Location = new System.Drawing.Point(106, 195);
            this.txtMetatype.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.txtMetatype.Multiline = true;
            this.txtMetatype.Name = "txtMetatype";
            this.txtMetatype.ReadOnly = true;
            this.txtMetatype.Size = new System.Drawing.Size(215, 16);
            this.txtMetatype.TabIndex = 61;
            this.txtMetatype.Text = "[Metatype]";
            // 
            // txtType
            // 
            this.txtType.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtType.Location = new System.Drawing.Point(106, 167);
            this.txtType.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.txtType.Multiline = true;
            this.txtType.Name = "txtType";
            this.txtType.ReadOnly = true;
            this.txtType.Size = new System.Drawing.Size(215, 16);
            this.txtType.TabIndex = 60;
            this.txtType.Text = "[Type]";
            // 
            // txtArchetype
            // 
            this.txtArchetype.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtArchetype.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtArchetype.Location = new System.Drawing.Point(106, 59);
            this.txtArchetype.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.txtArchetype.Multiline = true;
            this.txtArchetype.Name = "txtArchetype";
            this.txtArchetype.ReadOnly = true;
            this.txtArchetype.Size = new System.Drawing.Size(215, 16);
            this.txtArchetype.TabIndex = 59;
            this.txtArchetype.Text = "[Archetype]";
            // 
            // txtLocation
            // 
            this.txtLocation.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtLocation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLocation.Location = new System.Drawing.Point(106, 31);
            this.txtLocation.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.txtLocation.Multiline = true;
            this.txtLocation.Name = "txtLocation";
            this.txtLocation.ReadOnly = true;
            this.txtLocation.Size = new System.Drawing.Size(215, 16);
            this.txtLocation.TabIndex = 58;
            this.txtLocation.Text = "[Location]";
            // 
            // lblNameLabel
            // 
            this.lblNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblNameLabel.AutoSize = true;
            this.lblNameLabel.Location = new System.Drawing.Point(62, 6);
            this.lblNameLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblNameLabel.Name = "lblNameLabel";
            this.lblNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblNameLabel.TabIndex = 44;
            this.lblNameLabel.Tag = "Label_Name";
            this.lblNameLabel.Text = "Name:";
            // 
            // lblLocationLabel
            // 
            this.lblLocationLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblLocationLabel.AutoSize = true;
            this.lblLocationLabel.Location = new System.Drawing.Point(49, 32);
            this.lblLocationLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLocationLabel.Name = "lblLocationLabel";
            this.lblLocationLabel.Size = new System.Drawing.Size(51, 13);
            this.lblLocationLabel.TabIndex = 45;
            this.lblLocationLabel.Tag = "Label_Location";
            this.lblLocationLabel.Text = "Location:";
            // 
            // lblArchtypeLabel
            // 
            this.lblArchtypeLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblArchtypeLabel.AutoSize = true;
            this.lblArchtypeLabel.Location = new System.Drawing.Point(42, 60);
            this.lblArchtypeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArchtypeLabel.Name = "lblArchtypeLabel";
            this.lblArchtypeLabel.Size = new System.Drawing.Size(58, 13);
            this.lblArchtypeLabel.TabIndex = 46;
            this.lblArchtypeLabel.Tag = "Label_Archetype";
            this.lblArchtypeLabel.Text = "Archetype:";
            // 
            // lblConnectionLabel
            // 
            this.lblConnectionLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblConnectionLabel.AutoSize = true;
            this.lblConnectionLabel.Location = new System.Drawing.Point(36, 87);
            this.lblConnectionLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblConnectionLabel.Name = "lblConnectionLabel";
            this.lblConnectionLabel.Size = new System.Drawing.Size(64, 13);
            this.lblConnectionLabel.TabIndex = 47;
            this.lblConnectionLabel.Tag = "Label_Contact_Connection";
            this.lblConnectionLabel.Text = "Connection:";
            // 
            // lblLoyaltyLabel
            // 
            this.lblLoyaltyLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblLoyaltyLabel.AutoSize = true;
            this.lblLoyaltyLabel.Location = new System.Drawing.Point(57, 112);
            this.lblLoyaltyLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLoyaltyLabel.Name = "lblLoyaltyLabel";
            this.lblLoyaltyLabel.Size = new System.Drawing.Size(43, 13);
            this.lblLoyaltyLabel.TabIndex = 48;
            this.lblLoyaltyLabel.Tag = "Label_Contact_Loyalty";
            this.lblLoyaltyLabel.Text = "Loyalty:";
            // 
            // flpCheckboxes
            // 
            this.flpCheckboxes.AutoSize = true;
            this.flpCheckboxes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.SetColumnSpan(this.flpCheckboxes, 2);
            this.flpCheckboxes.Controls.Add(this.cmdNotes);
            this.flpCheckboxes.Controls.Add(this.cmdLink);
            this.flpCheckboxes.Controls.Add(this.chkFree);
            this.flpCheckboxes.Controls.Add(this.chkGroup);
            this.flpCheckboxes.Controls.Add(this.chkBlackmail);
            this.flpCheckboxes.Controls.Add(this.chkFamily);
            this.flpCheckboxes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpCheckboxes.Location = new System.Drawing.Point(0, 131);
            this.flpCheckboxes.Margin = new System.Windows.Forms.Padding(0);
            this.flpCheckboxes.Name = "flpCheckboxes";
            this.flpCheckboxes.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.flpCheckboxes.Size = new System.Drawing.Size(324, 30);
            this.flpCheckboxes.TabIndex = 49;
            this.flpCheckboxes.WrapContents = false;
            // 
            // cmdNotes
            // 
            this.cmdNotes.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmdNotes.AutoSize = true;
            this.cmdNotes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdNotes.FlatAppearance.BorderSize = 0;
            this.cmdNotes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.cmdNotes.ImageDpi120 = null;
            this.cmdNotes.ImageDpi144 = null;
            this.cmdNotes.ImageDpi192 = global::Chummer.Properties.Resources.note_edit1;
            this.cmdNotes.ImageDpi288 = null;
            this.cmdNotes.ImageDpi384 = null;
            this.cmdNotes.ImageDpi96 = global::Chummer.Properties.Resources.note_edit;
            this.cmdNotes.Location = new System.Drawing.Point(9, 3);
            this.cmdNotes.MinimumSize = new System.Drawing.Size(24, 24);
            this.cmdNotes.Name = "cmdNotes";
            this.cmdNotes.Padding = new System.Windows.Forms.Padding(1);
            this.cmdNotes.Size = new System.Drawing.Size(24, 24);
            this.cmdNotes.TabIndex = 37;
            this.cmdNotes.ToolTipText = "";
            this.cmdNotes.UseVisualStyleBackColor = true;
            // 
            // cmdLink
            // 
            this.cmdLink.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmdLink.AutoSize = true;
            this.cmdLink.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdLink.FlatAppearance.BorderSize = 0;
            this.cmdLink.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdLink.Image = global::Chummer.Properties.Resources.link;
            this.cmdLink.ImageDpi120 = null;
            this.cmdLink.ImageDpi144 = null;
            this.cmdLink.ImageDpi192 = global::Chummer.Properties.Resources.link1;
            this.cmdLink.ImageDpi288 = null;
            this.cmdLink.ImageDpi384 = null;
            this.cmdLink.ImageDpi96 = global::Chummer.Properties.Resources.link;
            this.cmdLink.Location = new System.Drawing.Point(39, 3);
            this.cmdLink.MinimumSize = new System.Drawing.Size(24, 24);
            this.cmdLink.Name = "cmdLink";
            this.cmdLink.Padding = new System.Windows.Forms.Padding(1);
            this.cmdLink.Size = new System.Drawing.Size(24, 24);
            this.cmdLink.TabIndex = 38;
            this.cmdLink.ToolTipText = "";
            this.cmdLink.UseVisualStyleBackColor = true;
            this.cmdLink.Click += new System.EventHandler(this.cmdLink_Click);
            // 
            // chkFree
            // 
            this.chkFree.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkFree.AutoSize = true;
            this.chkFree.DefaultColorScheme = true;
            this.chkFree.Enabled = false;
            this.chkFree.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.chkFree.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ControlDark;
            this.chkFree.Location = new System.Drawing.Point(69, 7);
            this.chkFree.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            this.chkFree.Name = "chkFree";
            this.chkFree.Size = new System.Drawing.Size(47, 17);
            this.chkFree.TabIndex = 0;
            this.chkFree.Tag = "Checkbox_Contact_Free";
            this.chkFree.Text = "Free";
            this.chkFree.UseVisualStyleBackColor = true;
            // 
            // chkGroup
            // 
            this.chkGroup.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkGroup.AutoSize = true;
            this.chkGroup.DefaultColorScheme = true;
            this.chkGroup.Enabled = false;
            this.chkGroup.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.chkGroup.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ControlDark;
            this.chkGroup.Location = new System.Drawing.Point(122, 7);
            this.chkGroup.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            this.chkGroup.Name = "chkGroup";
            this.chkGroup.Size = new System.Drawing.Size(55, 17);
            this.chkGroup.TabIndex = 1;
            this.chkGroup.Tag = "Checkbox_Contact_Group";
            this.chkGroup.Text = "Group";
            this.chkGroup.UseVisualStyleBackColor = true;
            // 
            // chkBlackmail
            // 
            this.chkBlackmail.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkBlackmail.AutoSize = true;
            this.chkBlackmail.DefaultColorScheme = true;
            this.chkBlackmail.Enabled = false;
            this.chkBlackmail.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.chkBlackmail.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ControlDark;
            this.chkBlackmail.Location = new System.Drawing.Point(183, 7);
            this.chkBlackmail.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            this.chkBlackmail.Name = "chkBlackmail";
            this.chkBlackmail.Size = new System.Drawing.Size(71, 17);
            this.chkBlackmail.TabIndex = 2;
            this.chkBlackmail.Tag = "Checkbox_Contact_Blackmail";
            this.chkBlackmail.Text = "Blackmail";
            this.chkBlackmail.UseVisualStyleBackColor = true;
            // 
            // chkFamily
            // 
            this.chkFamily.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkFamily.AutoSize = true;
            this.chkFamily.DefaultColorScheme = true;
            this.chkFamily.Enabled = false;
            this.chkFamily.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.chkFamily.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ControlDark;
            this.chkFamily.Location = new System.Drawing.Point(260, 7);
            this.chkFamily.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            this.chkFamily.Name = "chkFamily";
            this.chkFamily.Size = new System.Drawing.Size(55, 17);
            this.chkFamily.TabIndex = 3;
            this.chkFamily.Tag = "Checkbox_Contact_Family";
            this.chkFamily.Text = "Family";
            this.chkFamily.UseVisualStyleBackColor = true;
            // 
            // lblTypeLabel
            // 
            this.lblTypeLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblTypeLabel.AutoSize = true;
            this.lblTypeLabel.Location = new System.Drawing.Point(66, 168);
            this.lblTypeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTypeLabel.Name = "lblTypeLabel";
            this.lblTypeLabel.Size = new System.Drawing.Size(34, 13);
            this.lblTypeLabel.TabIndex = 50;
            this.lblTypeLabel.Tag = "Label_Type";
            this.lblTypeLabel.Text = "Type:";
            // 
            // lblMetatypeLabel
            // 
            this.lblMetatypeLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMetatypeLabel.AutoSize = true;
            this.lblMetatypeLabel.Location = new System.Drawing.Point(46, 196);
            this.lblMetatypeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetatypeLabel.Name = "lblMetatypeLabel";
            this.lblMetatypeLabel.Size = new System.Drawing.Size(54, 13);
            this.lblMetatypeLabel.TabIndex = 51;
            this.lblMetatypeLabel.Tag = "Label_Metatype";
            this.lblMetatypeLabel.Text = "Metatype:";
            // 
            // lblGenderLabel
            // 
            this.lblGenderLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblGenderLabel.AutoSize = true;
            this.lblGenderLabel.Location = new System.Drawing.Point(55, 224);
            this.lblGenderLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGenderLabel.Name = "lblGenderLabel";
            this.lblGenderLabel.Size = new System.Drawing.Size(45, 13);
            this.lblGenderLabel.TabIndex = 52;
            this.lblGenderLabel.Tag = "Label_Gender";
            this.lblGenderLabel.Text = "Gender:";
            // 
            // lblAgeLabel
            // 
            this.lblAgeLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblAgeLabel.AutoSize = true;
            this.lblAgeLabel.Location = new System.Drawing.Point(71, 252);
            this.lblAgeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAgeLabel.Name = "lblAgeLabel";
            this.lblAgeLabel.Size = new System.Drawing.Size(29, 13);
            this.lblAgeLabel.TabIndex = 53;
            this.lblAgeLabel.Tag = "Label_Age";
            this.lblAgeLabel.Text = "Age:";
            // 
            // lblPersonalLifeLabel
            // 
            this.lblPersonalLifeLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblPersonalLifeLabel.AutoSize = true;
            this.lblPersonalLifeLabel.Location = new System.Drawing.Point(29, 280);
            this.lblPersonalLifeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPersonalLifeLabel.Name = "lblPersonalLifeLabel";
            this.lblPersonalLifeLabel.Size = new System.Drawing.Size(71, 13);
            this.lblPersonalLifeLabel.TabIndex = 54;
            this.lblPersonalLifeLabel.Tag = "Label_Contact_PersonalLife";
            this.lblPersonalLifeLabel.Text = "Personal Life:";
            // 
            // lblPreferredPaymentLabel
            // 
            this.lblPreferredPaymentLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblPreferredPaymentLabel.AutoSize = true;
            this.lblPreferredPaymentLabel.Location = new System.Drawing.Point(3, 308);
            this.lblPreferredPaymentLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPreferredPaymentLabel.Name = "lblPreferredPaymentLabel";
            this.lblPreferredPaymentLabel.Size = new System.Drawing.Size(97, 13);
            this.lblPreferredPaymentLabel.TabIndex = 55;
            this.lblPreferredPaymentLabel.Tag = "Label_Contact_PreferredPayment";
            this.lblPreferredPaymentLabel.Text = "Preferred Payment:";
            // 
            // lblHobbiesViceLabel
            // 
            this.lblHobbiesViceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblHobbiesViceLabel.AutoSize = true;
            this.lblHobbiesViceLabel.Location = new System.Drawing.Point(25, 336);
            this.lblHobbiesViceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblHobbiesViceLabel.Name = "lblHobbiesViceLabel";
            this.lblHobbiesViceLabel.Size = new System.Drawing.Size(75, 13);
            this.lblHobbiesViceLabel.TabIndex = 56;
            this.lblHobbiesViceLabel.Tag = "Label_Contact_HobbiesVice";
            this.lblHobbiesViceLabel.Text = "Hobbies/Vice:";
            // 
            // txtName
            // 
            this.txtName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtName.Location = new System.Drawing.Point(106, 6);
            this.txtName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.txtName.Multiline = true;
            this.txtName.Name = "txtName";
            this.txtName.ReadOnly = true;
            this.txtName.Size = new System.Drawing.Size(215, 16);
            this.txtName.TabIndex = 57;
            this.txtName.Text = "[Name]";
            // 
            // lblConnection
            // 
            this.lblConnection.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblConnection.AutoSize = true;
            this.lblConnection.Location = new System.Drawing.Point(106, 87);
            this.lblConnection.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblConnection.Name = "lblConnection";
            this.lblConnection.Size = new System.Drawing.Size(19, 13);
            this.lblConnection.TabIndex = 67;
            this.lblConnection.Text = "[0]";
            // 
            // ContactControlReadOnly
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.Name = "ContactControlReadOnly";
            this.Size = new System.Drawing.Size(324, 357);
            this.Load += new System.EventHandler(this.ContactControlReadOnly_Load);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.flpCheckboxes.ResumeLayout(false);
            this.flpCheckboxes.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.Label lblNameLabel;
        private System.Windows.Forms.Label lblLocationLabel;
        private System.Windows.Forms.Label lblArchtypeLabel;
        private System.Windows.Forms.Label lblConnectionLabel;
        private System.Windows.Forms.Label lblLoyaltyLabel;
        private System.Windows.Forms.FlowLayoutPanel flpCheckboxes;
        private ColorableCheckBox chkFree;
        private ColorableCheckBox chkGroup;
        private ColorableCheckBox chkBlackmail;
        private ColorableCheckBox chkFamily;
        private ButtonWithToolTip cmdNotes;
        private ButtonWithToolTip cmdLink;
        private System.Windows.Forms.Label lblTypeLabel;
        private System.Windows.Forms.Label lblMetatypeLabel;
        private System.Windows.Forms.Label lblGenderLabel;
        private System.Windows.Forms.Label lblAgeLabel;
        private System.Windows.Forms.Label lblPersonalLifeLabel;
        private System.Windows.Forms.Label lblPreferredPaymentLabel;
        private System.Windows.Forms.Label lblHobbiesViceLabel;
        private System.Windows.Forms.TextBox txtPersonalLife;
        private System.Windows.Forms.TextBox txtAge;
        private System.Windows.Forms.TextBox txtGender;
        private System.Windows.Forms.TextBox txtMetatype;
        private System.Windows.Forms.TextBox txtType;
        private System.Windows.Forms.TextBox txtArchetype;
        private System.Windows.Forms.TextBox txtLocation;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.TextBox txtHobbiesVice;
        private System.Windows.Forms.TextBox txtPreferredPayment;
        private System.Windows.Forms.Label lblLoyalty;
        private System.Windows.Forms.Label lblConnection;
    }
}
