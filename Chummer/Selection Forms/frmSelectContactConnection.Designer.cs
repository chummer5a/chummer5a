namespace Chummer
{
    partial class frmSelectContactConnection
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
            this.cboMembership = new System.Windows.Forms.ComboBox();
            this.lblMembershipLabel = new System.Windows.Forms.Label();
            this.cboAreaOfInfluence = new System.Windows.Forms.ComboBox();
            this.lblAreaOfInfluenceLabel = new System.Windows.Forms.Label();
            this.cboMagicalResources = new System.Windows.Forms.ComboBox();
            this.lblMagicalResourcesLabel = new System.Windows.Forms.Label();
            this.cboMatrixResources = new System.Windows.Forms.ComboBox();
            this.lblMatrixResourcesLabel = new System.Windows.Forms.Label();
            this.lblTotalConnectionModifierLabel = new System.Windows.Forms.Label();
            this.lblTotalConnectionModifier = new System.Windows.Forms.Label();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lblGroupNameLabel = new System.Windows.Forms.Label();
            this.txtGroupName = new System.Windows.Forms.TextBox();
            this.cmdChangeColour = new System.Windows.Forms.Button();
            this.chkFreeContact = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // cboMembership
            // 
            this.cboMembership.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMembership.FormattingEnabled = true;
            this.cboMembership.Location = new System.Drawing.Point(152, 38);
            this.cboMembership.Name = "cboMembership";
            this.cboMembership.Size = new System.Drawing.Size(198, 21);
            this.cboMembership.TabIndex = 3;
            this.cboMembership.DropDown += new System.EventHandler(this.cboField_DropDown);
            this.cboMembership.SelectedIndexChanged += new System.EventHandler(this.cboMembership_SelectedIndexChanged);
            // 
            // lblMembershipLabel
            // 
            this.lblMembershipLabel.AutoSize = true;
            this.lblMembershipLabel.Location = new System.Drawing.Point(12, 41);
            this.lblMembershipLabel.Name = "lblMembershipLabel";
            this.lblMembershipLabel.Size = new System.Drawing.Size(67, 13);
            this.lblMembershipLabel.TabIndex = 2;
            this.lblMembershipLabel.Tag = "Label_SelectContactConnection_Membership";
            this.lblMembershipLabel.Text = "Membership:";
            // 
            // cboAreaOfInfluence
            // 
            this.cboAreaOfInfluence.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAreaOfInfluence.FormattingEnabled = true;
            this.cboAreaOfInfluence.Location = new System.Drawing.Point(152, 65);
            this.cboAreaOfInfluence.Name = "cboAreaOfInfluence";
            this.cboAreaOfInfluence.Size = new System.Drawing.Size(198, 21);
            this.cboAreaOfInfluence.TabIndex = 5;
            this.cboAreaOfInfluence.DropDown += new System.EventHandler(this.cboField_DropDown);
            this.cboAreaOfInfluence.SelectedIndexChanged += new System.EventHandler(this.cboAreaOfInfluence_SelectedIndexChanged);
            // 
            // lblAreaOfInfluenceLabel
            // 
            this.lblAreaOfInfluenceLabel.AutoSize = true;
            this.lblAreaOfInfluenceLabel.Location = new System.Drawing.Point(12, 68);
            this.lblAreaOfInfluenceLabel.Name = "lblAreaOfInfluenceLabel";
            this.lblAreaOfInfluenceLabel.Size = new System.Drawing.Size(91, 13);
            this.lblAreaOfInfluenceLabel.TabIndex = 4;
            this.lblAreaOfInfluenceLabel.Tag = "Label_SelectContactConnection_AreaOfInfluence";
            this.lblAreaOfInfluenceLabel.Text = "Area of Influence:";
            // 
            // cboMagicalResources
            // 
            this.cboMagicalResources.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMagicalResources.FormattingEnabled = true;
            this.cboMagicalResources.Location = new System.Drawing.Point(152, 92);
            this.cboMagicalResources.Name = "cboMagicalResources";
            this.cboMagicalResources.Size = new System.Drawing.Size(198, 21);
            this.cboMagicalResources.TabIndex = 7;
            this.cboMagicalResources.DropDown += new System.EventHandler(this.cboField_DropDown);
            this.cboMagicalResources.SelectedIndexChanged += new System.EventHandler(this.cboMagicalResources_SelectedIndexChanged);
            // 
            // lblMagicalResourcesLabel
            // 
            this.lblMagicalResourcesLabel.AutoSize = true;
            this.lblMagicalResourcesLabel.Location = new System.Drawing.Point(12, 95);
            this.lblMagicalResourcesLabel.Name = "lblMagicalResourcesLabel";
            this.lblMagicalResourcesLabel.Size = new System.Drawing.Size(101, 13);
            this.lblMagicalResourcesLabel.TabIndex = 6;
            this.lblMagicalResourcesLabel.Tag = "Label_SelectContactConnection_MagicalResources";
            this.lblMagicalResourcesLabel.Text = "Magical Resources:";
            // 
            // cboMatrixResources
            // 
            this.cboMatrixResources.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMatrixResources.FormattingEnabled = true;
            this.cboMatrixResources.Location = new System.Drawing.Point(152, 119);
            this.cboMatrixResources.Name = "cboMatrixResources";
            this.cboMatrixResources.Size = new System.Drawing.Size(198, 21);
            this.cboMatrixResources.TabIndex = 9;
            this.cboMatrixResources.DropDown += new System.EventHandler(this.cboField_DropDown);
            this.cboMatrixResources.SelectedIndexChanged += new System.EventHandler(this.cboMatrixResources_SelectedIndexChanged);
            // 
            // lblMatrixResourcesLabel
            // 
            this.lblMatrixResourcesLabel.AutoSize = true;
            this.lblMatrixResourcesLabel.Location = new System.Drawing.Point(12, 122);
            this.lblMatrixResourcesLabel.Name = "lblMatrixResourcesLabel";
            this.lblMatrixResourcesLabel.Size = new System.Drawing.Size(92, 13);
            this.lblMatrixResourcesLabel.TabIndex = 8;
            this.lblMatrixResourcesLabel.Tag = "Label_SelectContactConnection_MatrixResources";
            this.lblMatrixResourcesLabel.Text = "Matrix Resources:";
            // 
            // lblTotalConnectionModifierLabel
            // 
            this.lblTotalConnectionModifierLabel.AutoSize = true;
            this.lblTotalConnectionModifierLabel.Location = new System.Drawing.Point(12, 153);
            this.lblTotalConnectionModifierLabel.Name = "lblTotalConnectionModifierLabel";
            this.lblTotalConnectionModifierLabel.Size = new System.Drawing.Size(131, 13);
            this.lblTotalConnectionModifierLabel.TabIndex = 10;
            this.lblTotalConnectionModifierLabel.Tag = "Label_SelectContactConnection_TotalModifier";
            this.lblTotalConnectionModifierLabel.Text = "Total Connection Modifier:";
            // 
            // lblTotalConnectionModifier
            // 
            this.lblTotalConnectionModifier.AutoSize = true;
            this.lblTotalConnectionModifier.Location = new System.Drawing.Point(149, 153);
            this.lblTotalConnectionModifier.Name = "lblTotalConnectionModifier";
            this.lblTotalConnectionModifier.Size = new System.Drawing.Size(13, 13);
            this.lblTotalConnectionModifier.TabIndex = 11;
            this.lblTotalConnectionModifier.Text = "0";
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(194, 243);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 15;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.Location = new System.Drawing.Point(275, 243);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 14;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lblGroupNameLabel
            // 
            this.lblGroupNameLabel.AutoSize = true;
            this.lblGroupNameLabel.Location = new System.Drawing.Point(12, 15);
            this.lblGroupNameLabel.Name = "lblGroupNameLabel";
            this.lblGroupNameLabel.Size = new System.Drawing.Size(130, 13);
            this.lblGroupNameLabel.TabIndex = 0;
            this.lblGroupNameLabel.Tag = "Label_SelectContactConnection_GroupName";
            this.lblGroupNameLabel.Text = "Group Name/Occupation:";
            // 
            // txtGroupName
            // 
            this.txtGroupName.Location = new System.Drawing.Point(152, 12);
            this.txtGroupName.Name = "txtGroupName";
            this.txtGroupName.Size = new System.Drawing.Size(198, 20);
            this.txtGroupName.TabIndex = 1;
            this.txtGroupName.TextChanged += new System.EventHandler(this.txtGroupName_TextChanged);
            // 
            // cmdChangeColour
            // 
            this.cmdChangeColour.Location = new System.Drawing.Point(194, 214);
            this.cmdChangeColour.Name = "cmdChangeColour";
            this.cmdChangeColour.Size = new System.Drawing.Size(156, 23);
            this.cmdChangeColour.TabIndex = 13;
            this.cmdChangeColour.Tag = "Button_SelectContactConnection_ChangeColor";
            this.cmdChangeColour.Text = "Change Colour";
            this.cmdChangeColour.UseVisualStyleBackColor = true;
            this.cmdChangeColour.Click += new System.EventHandler(this.cmdChangeColour_Click);
            // 
            // chkFreeContact
            // 
            this.chkFreeContact.AutoSize = true;
            this.chkFreeContact.Location = new System.Drawing.Point(15, 179);
            this.chkFreeContact.Name = "chkFreeContact";
            this.chkFreeContact.Size = new System.Drawing.Size(87, 17);
            this.chkFreeContact.TabIndex = 12;
            this.chkFreeContact.Tag = "Checkbox_SelectContactConnection_FreeContact";
            this.chkFreeContact.Text = "Free Contact";
            this.chkFreeContact.UseVisualStyleBackColor = true;
            this.chkFreeContact.CheckedChanged += new System.EventHandler(this.chkFreeContact_CheckedChanged);
            // 
            // frmSelectContactConnection
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(362, 274);
            this.Controls.Add(this.chkFreeContact);
            this.Controls.Add(this.cmdChangeColour);
            this.Controls.Add(this.txtGroupName);
            this.Controls.Add(this.lblGroupNameLabel);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.lblTotalConnectionModifier);
            this.Controls.Add(this.lblTotalConnectionModifierLabel);
            this.Controls.Add(this.cboMatrixResources);
            this.Controls.Add(this.lblMatrixResourcesLabel);
            this.Controls.Add(this.cboMagicalResources);
            this.Controls.Add(this.lblMagicalResourcesLabel);
            this.Controls.Add(this.cboAreaOfInfluence);
            this.Controls.Add(this.lblAreaOfInfluenceLabel);
            this.Controls.Add(this.cboMembership);
            this.Controls.Add(this.lblMembershipLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectContactConnection";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectContactConnection";
            this.Text = "Advanced Contact Options";
            this.Load += new System.EventHandler(this.frmSelectContactConnection_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cboMembership;
        private System.Windows.Forms.Label lblMembershipLabel;
        private System.Windows.Forms.ComboBox cboAreaOfInfluence;
        private System.Windows.Forms.Label lblAreaOfInfluenceLabel;
        private System.Windows.Forms.ComboBox cboMagicalResources;
        private System.Windows.Forms.Label lblMagicalResourcesLabel;
        private System.Windows.Forms.ComboBox cboMatrixResources;
        private System.Windows.Forms.Label lblMatrixResourcesLabel;
        private System.Windows.Forms.Label lblTotalConnectionModifierLabel;
        private System.Windows.Forms.Label lblTotalConnectionModifier;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Label lblGroupNameLabel;
        private System.Windows.Forms.TextBox txtGroupName;
        private System.Windows.Forms.Button cmdChangeColour;
        private System.Windows.Forms.CheckBox chkFreeContact;

    }
}