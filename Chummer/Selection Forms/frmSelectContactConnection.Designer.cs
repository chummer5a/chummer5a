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
            this.components = new System.ComponentModel.Container();
            this.cboMembership = new Chummer.ElasticComboBox();
            this.lblMembershipLabel = new System.Windows.Forms.Label();
            this.cboAreaOfInfluence = new Chummer.ElasticComboBox();
            this.lblAreaOfInfluenceLabel = new System.Windows.Forms.Label();
            this.cboMagicalResources = new Chummer.ElasticComboBox();
            this.lblMagicalResourcesLabel = new System.Windows.Forms.Label();
            this.cboMatrixResources = new Chummer.ElasticComboBox();
            this.lblMatrixResourcesLabel = new System.Windows.Forms.Label();
            this.lblTotalConnectionModifierLabel = new System.Windows.Forms.Label();
            this.lblTotalConnectionModifier = new System.Windows.Forms.Label();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lblGroupNameLabel = new System.Windows.Forms.Label();
            this.txtGroupName = new System.Windows.Forms.TextBox();
            this.cmdChangeColour = new System.Windows.Forms.Button();
            this.chkFreeContact = new System.Windows.Forms.CheckBox();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tlpMain.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cboMembership
            // 
            this.cboMembership.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboMembership.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMembership.FormattingEnabled = true;
            this.cboMembership.Location = new System.Drawing.Point(140, 29);
            this.cboMembership.Name = "cboMembership";
            this.cboMembership.Size = new System.Drawing.Size(223, 21);
            this.cboMembership.TabIndex = 3;
            this.cboMembership.TooltipText = "";
            this.cboMembership.SelectedIndexChanged += new System.EventHandler(this.cboMembership_SelectedIndexChanged);
            // 
            // lblMembershipLabel
            // 
            this.lblMembershipLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMembershipLabel.AutoSize = true;
            this.lblMembershipLabel.Location = new System.Drawing.Point(67, 32);
            this.lblMembershipLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMembershipLabel.Name = "lblMembershipLabel";
            this.lblMembershipLabel.Size = new System.Drawing.Size(67, 13);
            this.lblMembershipLabel.TabIndex = 2;
            this.lblMembershipLabel.Tag = "Label_SelectContactConnection_Membership";
            this.lblMembershipLabel.Text = "Membership:";
            // 
            // cboAreaOfInfluence
            // 
            this.cboAreaOfInfluence.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboAreaOfInfluence.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAreaOfInfluence.FormattingEnabled = true;
            this.cboAreaOfInfluence.Location = new System.Drawing.Point(140, 56);
            this.cboAreaOfInfluence.Name = "cboAreaOfInfluence";
            this.cboAreaOfInfluence.Size = new System.Drawing.Size(223, 21);
            this.cboAreaOfInfluence.TabIndex = 5;
            this.cboAreaOfInfluence.TooltipText = "";
            this.cboAreaOfInfluence.SelectedIndexChanged += new System.EventHandler(this.cboAreaOfInfluence_SelectedIndexChanged);
            // 
            // lblAreaOfInfluenceLabel
            // 
            this.lblAreaOfInfluenceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAreaOfInfluenceLabel.AutoSize = true;
            this.lblAreaOfInfluenceLabel.Location = new System.Drawing.Point(43, 59);
            this.lblAreaOfInfluenceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAreaOfInfluenceLabel.Name = "lblAreaOfInfluenceLabel";
            this.lblAreaOfInfluenceLabel.Size = new System.Drawing.Size(91, 13);
            this.lblAreaOfInfluenceLabel.TabIndex = 4;
            this.lblAreaOfInfluenceLabel.Tag = "Label_SelectContactConnection_AreaOfInfluence";
            this.lblAreaOfInfluenceLabel.Text = "Area of Influence:";
            // 
            // cboMagicalResources
            // 
            this.cboMagicalResources.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboMagicalResources.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMagicalResources.FormattingEnabled = true;
            this.cboMagicalResources.Location = new System.Drawing.Point(140, 83);
            this.cboMagicalResources.Name = "cboMagicalResources";
            this.cboMagicalResources.Size = new System.Drawing.Size(223, 21);
            this.cboMagicalResources.TabIndex = 7;
            this.cboMagicalResources.TooltipText = "";
            this.cboMagicalResources.SelectedIndexChanged += new System.EventHandler(this.cboMagicalResources_SelectedIndexChanged);
            // 
            // lblMagicalResourcesLabel
            // 
            this.lblMagicalResourcesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMagicalResourcesLabel.AutoSize = true;
            this.lblMagicalResourcesLabel.Location = new System.Drawing.Point(33, 86);
            this.lblMagicalResourcesLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMagicalResourcesLabel.Name = "lblMagicalResourcesLabel";
            this.lblMagicalResourcesLabel.Size = new System.Drawing.Size(101, 13);
            this.lblMagicalResourcesLabel.TabIndex = 6;
            this.lblMagicalResourcesLabel.Tag = "Label_SelectContactConnection_MagicalResources";
            this.lblMagicalResourcesLabel.Text = "Magical Resources:";
            // 
            // cboMatrixResources
            // 
            this.cboMatrixResources.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboMatrixResources.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMatrixResources.FormattingEnabled = true;
            this.cboMatrixResources.Location = new System.Drawing.Point(140, 110);
            this.cboMatrixResources.Name = "cboMatrixResources";
            this.cboMatrixResources.Size = new System.Drawing.Size(223, 21);
            this.cboMatrixResources.TabIndex = 9;
            this.cboMatrixResources.TooltipText = "";
            this.cboMatrixResources.SelectedIndexChanged += new System.EventHandler(this.cboMatrixResources_SelectedIndexChanged);
            // 
            // lblMatrixResourcesLabel
            // 
            this.lblMatrixResourcesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMatrixResourcesLabel.AutoSize = true;
            this.lblMatrixResourcesLabel.Location = new System.Drawing.Point(42, 113);
            this.lblMatrixResourcesLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMatrixResourcesLabel.Name = "lblMatrixResourcesLabel";
            this.lblMatrixResourcesLabel.Size = new System.Drawing.Size(92, 13);
            this.lblMatrixResourcesLabel.TabIndex = 8;
            this.lblMatrixResourcesLabel.Tag = "Label_SelectContactConnection_MatrixResources";
            this.lblMatrixResourcesLabel.Text = "Matrix Resources:";
            // 
            // lblTotalConnectionModifierLabel
            // 
            this.lblTotalConnectionModifierLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTotalConnectionModifierLabel.AutoSize = true;
            this.lblTotalConnectionModifierLabel.Location = new System.Drawing.Point(3, 140);
            this.lblTotalConnectionModifierLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTotalConnectionModifierLabel.Name = "lblTotalConnectionModifierLabel";
            this.lblTotalConnectionModifierLabel.Size = new System.Drawing.Size(131, 13);
            this.lblTotalConnectionModifierLabel.TabIndex = 10;
            this.lblTotalConnectionModifierLabel.Tag = "Label_SelectContactConnection_TotalModifier";
            this.lblTotalConnectionModifierLabel.Text = "Total Connection Modifier:";
            // 
            // lblTotalConnectionModifier
            // 
            this.lblTotalConnectionModifier.AutoSize = true;
            this.lblTotalConnectionModifier.Location = new System.Drawing.Point(140, 140);
            this.lblTotalConnectionModifier.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTotalConnectionModifier.Name = "lblTotalConnectionModifier";
            this.lblTotalConnectionModifier.Size = new System.Drawing.Size(13, 13);
            this.lblTotalConnectionModifier.TabIndex = 11;
            this.lblTotalConnectionModifier.Text = "0";
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.AutoSize = true;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(0, 0);
            this.cmdCancel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
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
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.AutoSize = true;
            this.cmdOK.Location = new System.Drawing.Point(78, 0);
            this.cmdOK.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
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
            this.lblGroupNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGroupNameLabel.AutoSize = true;
            this.lblGroupNameLabel.Location = new System.Drawing.Point(4, 6);
            this.lblGroupNameLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGroupNameLabel.Name = "lblGroupNameLabel";
            this.lblGroupNameLabel.Size = new System.Drawing.Size(130, 13);
            this.lblGroupNameLabel.TabIndex = 0;
            this.lblGroupNameLabel.Tag = "Label_SelectContactConnection_GroupName";
            this.lblGroupNameLabel.Text = "Group Name/Occupation:";
            // 
            // txtGroupName
            // 
            this.txtGroupName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGroupName.Location = new System.Drawing.Point(140, 3);
            this.txtGroupName.Name = "txtGroupName";
            this.txtGroupName.Size = new System.Drawing.Size(223, 20);
            this.txtGroupName.TabIndex = 1;
            this.txtGroupName.TextChanged += new System.EventHandler(this.txtGroupName_TextChanged);
            // 
            // cmdChangeColour
            // 
            this.cmdChangeColour.AutoSize = true;
            this.cmdChangeColour.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdChangeColour.Location = new System.Drawing.Point(140, 162);
            this.cmdChangeColour.Name = "cmdChangeColour";
            this.cmdChangeColour.Size = new System.Drawing.Size(87, 23);
            this.cmdChangeColour.TabIndex = 13;
            this.cmdChangeColour.Tag = "Button_SelectContactConnection_ChangeColor";
            this.cmdChangeColour.Text = "Change Colour";
            this.cmdChangeColour.UseVisualStyleBackColor = true;
            this.cmdChangeColour.Click += new System.EventHandler(this.cmdChangeColour_Click);
            // 
            // chkFreeContact
            // 
            this.chkFreeContact.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.chkFreeContact.AutoSize = true;
            this.chkFreeContact.Location = new System.Drawing.Point(47, 165);
            this.chkFreeContact.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkFreeContact.Name = "chkFreeContact";
            this.chkFreeContact.Size = new System.Drawing.Size(87, 17);
            this.chkFreeContact.TabIndex = 12;
            this.chkFreeContact.Tag = "Checkbox_SelectContactConnection_FreeContact";
            this.chkFreeContact.Text = "Free Contact";
            this.chkFreeContact.UseVisualStyleBackColor = true;
            this.chkFreeContact.CheckedChanged += new System.EventHandler(this.chkFreeContact_CheckedChanged);
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.chkFreeContact, 0, 6);
            this.tlpMain.Controls.Add(this.lblGroupNameLabel, 0, 0);
            this.tlpMain.Controls.Add(this.lblMembershipLabel, 0, 1);
            this.tlpMain.Controls.Add(this.txtGroupName, 1, 0);
            this.tlpMain.Controls.Add(this.lblAreaOfInfluenceLabel, 0, 2);
            this.tlpMain.Controls.Add(this.lblTotalConnectionModifier, 1, 5);
            this.tlpMain.Controls.Add(this.lblMagicalResourcesLabel, 0, 3);
            this.tlpMain.Controls.Add(this.lblTotalConnectionModifierLabel, 0, 5);
            this.tlpMain.Controls.Add(this.lblMatrixResourcesLabel, 0, 4);
            this.tlpMain.Controls.Add(this.cboMatrixResources, 1, 4);
            this.tlpMain.Controls.Add(this.cboMembership, 1, 1);
            this.tlpMain.Controls.Add(this.cboMagicalResources, 1, 3);
            this.tlpMain.Controls.Add(this.cboAreaOfInfluence, 1, 2);
            this.tlpMain.Controls.Add(this.flowLayoutPanel1, 0, 8);
            this.tlpMain.Controls.Add(this.cmdChangeColour, 1, 6);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 9;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(366, 223);
            this.tlpMain.TabIndex = 16;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.SetColumnSpan(this.flowLayoutPanel1, 2);
            this.flowLayoutPanel1.Controls.Add(this.cmdOK);
            this.flowLayoutPanel1.Controls.Add(this.cmdCancel);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(210, 200);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(156, 23);
            this.flowLayoutPanel1.TabIndex = 14;
            // 
            // frmSelectContactConnection
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(384, 241);
            this.Controls.Add(this.tlpMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectContactConnection";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectContactConnection";
            this.Text = "Advanced Contact Options";
            this.Load += new System.EventHandler(this.frmSelectContactConnection_Load);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ElasticComboBox cboMembership;
        private System.Windows.Forms.Label lblMembershipLabel;
        private ElasticComboBox cboAreaOfInfluence;
        private System.Windows.Forms.Label lblAreaOfInfluenceLabel;
        private ElasticComboBox cboMagicalResources;
        private System.Windows.Forms.Label lblMagicalResourcesLabel;
        private ElasticComboBox cboMatrixResources;
        private System.Windows.Forms.Label lblMatrixResourcesLabel;
        private System.Windows.Forms.Label lblTotalConnectionModifierLabel;
        private System.Windows.Forms.Label lblTotalConnectionModifier;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Label lblGroupNameLabel;
        private System.Windows.Forms.TextBox txtGroupName;
        private System.Windows.Forms.Button cmdChangeColour;
        private System.Windows.Forms.CheckBox chkFreeContact;
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
