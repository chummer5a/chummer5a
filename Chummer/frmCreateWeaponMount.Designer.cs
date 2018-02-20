namespace Chummer
{
    partial class frmCreateWeaponMount
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
                tipTooltip?.Dispose();
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
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lblVisbility = new System.Windows.Forms.Label();
            this.cboVisibility = new System.Windows.Forms.ComboBox();
            this.lblControl = new System.Windows.Forms.Label();
            this.cboControl = new System.Windows.Forms.ComboBox();
            this.lblSize = new System.Windows.Forms.Label();
            this.cboSize = new System.Windows.Forms.ComboBox();
            this.lblFlexibility = new System.Windows.Forms.Label();
            this.cboFlexibility = new System.Windows.Forms.ComboBox();
            this.lblAvailabilityLabel = new System.Windows.Forms.Label();
            this.lblCostLabel = new System.Windows.Forms.Label();
            this.lblAvailability = new System.Windows.Forms.Label();
            this.lblCost = new System.Windows.Forms.Label();
            this.lblSlots = new System.Windows.Forms.Label();
            this.lblSlotsLabel = new System.Windows.Forms.Label();
            this.chkFreeItem = new System.Windows.Forms.CheckBox();
            this.nudMarkup = new System.Windows.Forms.NumericUpDown();
            this.lblMarkupPercentLabel = new System.Windows.Forms.Label();
            this.lblMarkupLabel = new System.Windows.Forms.Label();
            this.treMods = new System.Windows.Forms.TreeView();
            this.cmdDeleteMod = new System.Windows.Forms.Button();
            this.cmdAddMod = new System.Windows.Forms.Button();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.tipTooltip = new TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).BeginInit();
            this.SuspendLayout();
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(434, 223);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 19;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.Location = new System.Drawing.Point(515, 223);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 20;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lblVisbility
            // 
            this.lblVisbility.AutoSize = true;
            this.lblVisbility.Location = new System.Drawing.Point(267, 64);
            this.lblVisbility.Name = "lblVisbility";
            this.lblVisbility.Size = new System.Drawing.Size(44, 13);
            this.lblVisbility.TabIndex = 21;
            this.lblVisbility.Tag = "Label_Visibility";
            this.lblVisbility.Text = "Visbility:";
            // 
            // cboVisibility
            // 
            this.cboVisibility.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboVisibility.FormattingEnabled = true;
            this.cboVisibility.Location = new System.Drawing.Point(344, 61);
            this.cboVisibility.Name = "cboVisibility";
            this.cboVisibility.Size = new System.Drawing.Size(241, 21);
            this.cboVisibility.TabIndex = 22;
            this.cboVisibility.SelectedIndexChanged += new System.EventHandler(this.comboBox_SelectedIndexChanged);
            // 
            // lblControl
            // 
            this.lblControl.AutoSize = true;
            this.lblControl.Location = new System.Drawing.Point(267, 38);
            this.lblControl.Name = "lblControl";
            this.lblControl.Size = new System.Drawing.Size(43, 13);
            this.lblControl.TabIndex = 23;
            this.lblControl.Tag = "Label_Control";
            this.lblControl.Text = "Control:";
            // 
            // cboControl
            // 
            this.cboControl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboControl.FormattingEnabled = true;
            this.cboControl.Location = new System.Drawing.Point(344, 35);
            this.cboControl.Name = "cboControl";
            this.cboControl.Size = new System.Drawing.Size(241, 21);
            this.cboControl.TabIndex = 24;
            this.cboControl.SelectedIndexChanged += new System.EventHandler(this.comboBox_SelectedIndexChanged);
            // 
            // lblSize
            // 
            this.lblSize.AutoSize = true;
            this.lblSize.Location = new System.Drawing.Point(267, 12);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(30, 13);
            this.lblSize.TabIndex = 25;
            this.lblSize.Tag = "Label_Size";
            this.lblSize.Text = "Size:";
            // 
            // cboSize
            // 
            this.cboSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSize.FormattingEnabled = true;
            this.cboSize.Location = new System.Drawing.Point(344, 9);
            this.cboSize.Name = "cboSize";
            this.cboSize.Size = new System.Drawing.Size(241, 21);
            this.cboSize.TabIndex = 26;
            this.cboSize.SelectedIndexChanged += new System.EventHandler(this.cboSize_SelectedIndexChanged);
            // 
            // lblFlexibility
            // 
            this.lblFlexibility.AutoSize = true;
            this.lblFlexibility.Location = new System.Drawing.Point(267, 90);
            this.lblFlexibility.Name = "lblFlexibility";
            this.lblFlexibility.Size = new System.Drawing.Size(51, 13);
            this.lblFlexibility.TabIndex = 27;
            this.lblFlexibility.Tag = "Label_Flexibility";
            this.lblFlexibility.Text = "Flexibility:";
            // 
            // cboFlexibility
            // 
            this.cboFlexibility.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFlexibility.FormattingEnabled = true;
            this.cboFlexibility.Location = new System.Drawing.Point(344, 87);
            this.cboFlexibility.Name = "cboFlexibility";
            this.cboFlexibility.Size = new System.Drawing.Size(241, 21);
            this.cboFlexibility.TabIndex = 28;
            this.cboFlexibility.SelectedIndexChanged += new System.EventHandler(this.comboBox_SelectedIndexChanged);
            // 
            // lblAvailabilityLabel
            // 
            this.lblAvailabilityLabel.AutoSize = true;
            this.lblAvailabilityLabel.Location = new System.Drawing.Point(267, 116);
            this.lblAvailabilityLabel.Name = "lblAvailabilityLabel";
            this.lblAvailabilityLabel.Size = new System.Drawing.Size(33, 13);
            this.lblAvailabilityLabel.TabIndex = 30;
            this.lblAvailabilityLabel.Tag = "Label_Avail";
            this.lblAvailabilityLabel.Text = "Avail:";
            // 
            // lblCostLabel
            // 
            this.lblCostLabel.AutoSize = true;
            this.lblCostLabel.Location = new System.Drawing.Point(267, 142);
            this.lblCostLabel.Name = "lblCostLabel";
            this.lblCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblCostLabel.TabIndex = 29;
            this.lblCostLabel.Tag = "Label_Cost";
            this.lblCostLabel.Text = "Cost:";
            // 
            // lblAvailability
            // 
            this.lblAvailability.AutoSize = true;
            this.lblAvailability.Location = new System.Drawing.Point(341, 116);
            this.lblAvailability.Name = "lblAvailability";
            this.lblAvailability.Size = new System.Drawing.Size(19, 13);
            this.lblAvailability.TabIndex = 32;
            this.lblAvailability.Tag = "";
            this.lblAvailability.Text = "+0";
            // 
            // lblCost
            // 
            this.lblCost.AutoSize = true;
            this.lblCost.Location = new System.Drawing.Point(341, 142);
            this.lblCost.Name = "lblCost";
            this.lblCost.Size = new System.Drawing.Size(19, 13);
            this.lblCost.TabIndex = 31;
            this.lblCost.Tag = "";
            this.lblCost.Text = "0¥";
            // 
            // lblSlots
            // 
            this.lblSlots.AutoSize = true;
            this.lblSlots.Location = new System.Drawing.Point(341, 168);
            this.lblSlots.Name = "lblSlots";
            this.lblSlots.Size = new System.Drawing.Size(13, 13);
            this.lblSlots.TabIndex = 34;
            this.lblSlots.Tag = "";
            this.lblSlots.Text = "0";
            // 
            // lblSlotsLabel
            // 
            this.lblSlotsLabel.AutoSize = true;
            this.lblSlotsLabel.Location = new System.Drawing.Point(267, 168);
            this.lblSlotsLabel.Name = "lblSlotsLabel";
            this.lblSlotsLabel.Size = new System.Drawing.Size(33, 13);
            this.lblSlotsLabel.TabIndex = 33;
            this.lblSlotsLabel.Tag = "Label_Slots";
            this.lblSlotsLabel.Text = "Slots:";
            // 
            // chkFreeItem
            // 
            this.chkFreeItem.AutoSize = true;
            this.chkFreeItem.Location = new System.Drawing.Point(515, 141);
            this.chkFreeItem.Name = "chkFreeItem";
            this.chkFreeItem.Size = new System.Drawing.Size(50, 17);
            this.chkFreeItem.TabIndex = 59;
            this.chkFreeItem.Tag = "Checkbox_Free";
            this.chkFreeItem.Text = "Free!";
            this.chkFreeItem.UseVisualStyleBackColor = true;
            this.chkFreeItem.CheckedChanged += new System.EventHandler(this.chkFreeItem_CheckedChanged);
            // 
            // nudMarkup
            // 
            this.nudMarkup.DecimalPlaces = 2;
            this.nudMarkup.Location = new System.Drawing.Point(344, 226);
            this.nudMarkup.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudMarkup.Minimum = new decimal(new int[] {
            9999,
            0,
            0,
            -2147352576});
            this.nudMarkup.Name = "nudMarkup";
            this.nudMarkup.Size = new System.Drawing.Size(56, 20);
            this.nudMarkup.TabIndex = 63;
            this.nudMarkup.ValueChanged += new System.EventHandler(this.nudMarkup_ValueChanged);
            // 
            // lblMarkupPercentLabel
            // 
            this.lblMarkupPercentLabel.AutoSize = true;
            this.lblMarkupPercentLabel.Location = new System.Drawing.Point(399, 228);
            this.lblMarkupPercentLabel.Name = "lblMarkupPercentLabel";
            this.lblMarkupPercentLabel.Size = new System.Drawing.Size(15, 13);
            this.lblMarkupPercentLabel.TabIndex = 64;
            this.lblMarkupPercentLabel.Text = "%";
            // 
            // lblMarkupLabel
            // 
            this.lblMarkupLabel.AutoSize = true;
            this.lblMarkupLabel.Location = new System.Drawing.Point(267, 228);
            this.lblMarkupLabel.Name = "lblMarkupLabel";
            this.lblMarkupLabel.Size = new System.Drawing.Size(46, 13);
            this.lblMarkupLabel.TabIndex = 62;
            this.lblMarkupLabel.Tag = "Label_SelectGear_Markup";
            this.lblMarkupLabel.Text = "Markup:";
            // 
            // treMods
            // 
            this.treMods.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treMods.Location = new System.Drawing.Point(12, 35);
            this.treMods.Name = "treMods";
            this.treMods.Size = new System.Drawing.Size(249, 211);
            this.treMods.TabIndex = 65;
            this.treMods.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treMods_AfterSelect);
            // 
            // cmdDeleteMod
            // 
            this.cmdDeleteMod.Location = new System.Drawing.Point(140, 4);
            this.cmdDeleteMod.Name = "cmdDeleteMod";
            this.cmdDeleteMod.Size = new System.Drawing.Size(122, 23);
            this.cmdDeleteMod.TabIndex = 67;
            this.cmdDeleteMod.Tag = "Button_DeleteMod";
            this.cmdDeleteMod.Text = "Delete Mod";
            this.cmdDeleteMod.UseVisualStyleBackColor = true;
            this.cmdDeleteMod.Click += new System.EventHandler(this.cmdDeleteMod_Click);
            // 
            // cmdAddMod
            // 
            this.cmdAddMod.Location = new System.Drawing.Point(12, 4);
            this.cmdAddMod.Name = "cmdAddMod";
            this.cmdAddMod.Size = new System.Drawing.Size(122, 23);
            this.cmdAddMod.TabIndex = 66;
            this.cmdAddMod.Tag = "Button_AddMod";
            this.cmdAddMod.Text = "Add Mod";
            this.cmdAddMod.UseVisualStyleBackColor = true;
            this.cmdAddMod.Click += new System.EventHandler(this.cmdAddMod_Click);
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.lblSource.Location = new System.Drawing.Point(341, 194);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 69;
            this.lblSource.Text = "[Source]";
            this.lblSource.Click += new System.EventHandler(CommonFunctions.OpenPDFFromControl);
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(267, 194);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 68;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            // 
            // tipTooltip
            // 
            this.tipTooltip.AllowLinksHandling = true;
            this.tipTooltip.AutoPopDelay = 10000;
            this.tipTooltip.BaseStylesheet = null;
            this.tipTooltip.InitialDelay = 250;
            this.tipTooltip.IsBalloon = true;
            this.tipTooltip.MaximumSize = new System.Drawing.Size(0, 0);
            this.tipTooltip.OwnerDraw = true;
            this.tipTooltip.ReshowDelay = 100;
            this.tipTooltip.TooltipCssClass = "htmltooltip";
            this.tipTooltip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.tipTooltip.ToolTipTitle = "Chummer Help";
            // 
            // frmCreateWeaponMount
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(597, 258);
            this.Controls.Add(this.lblSource);
            this.Controls.Add(this.lblSourceLabel);
            this.Controls.Add(this.cmdDeleteMod);
            this.Controls.Add(this.cmdAddMod);
            this.Controls.Add(this.treMods);
            this.Controls.Add(this.nudMarkup);
            this.Controls.Add(this.lblMarkupPercentLabel);
            this.Controls.Add(this.lblMarkupLabel);
            this.Controls.Add(this.chkFreeItem);
            this.Controls.Add(this.lblSlots);
            this.Controls.Add(this.lblSlotsLabel);
            this.Controls.Add(this.lblAvailability);
            this.Controls.Add(this.lblCost);
            this.Controls.Add(this.lblAvailabilityLabel);
            this.Controls.Add(this.lblCostLabel);
            this.Controls.Add(this.lblFlexibility);
            this.Controls.Add(this.cboFlexibility);
            this.Controls.Add(this.lblSize);
            this.Controls.Add(this.cboSize);
            this.Controls.Add(this.lblControl);
            this.Controls.Add(this.cboControl);
            this.Controls.Add(this.lblVisbility);
            this.Controls.Add(this.cboVisibility);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "frmCreateWeaponMount";
            this.Tag = "Title_CreateWeaponMount";
            this.Text = "Create Weapon Mount";
            this.Load += new System.EventHandler(this.frmCreateWeaponMount_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Label lblVisbility;
        private System.Windows.Forms.ComboBox cboVisibility;
        private System.Windows.Forms.Label lblControl;
        private System.Windows.Forms.ComboBox cboControl;
        private System.Windows.Forms.Label lblSize;
        private System.Windows.Forms.ComboBox cboSize;
        private System.Windows.Forms.Label lblFlexibility;
        private System.Windows.Forms.ComboBox cboFlexibility;
        private System.Windows.Forms.Label lblAvailabilityLabel;
        private System.Windows.Forms.Label lblCostLabel;
        private System.Windows.Forms.Label lblAvailability;
        private System.Windows.Forms.Label lblCost;
        private System.Windows.Forms.Label lblSlots;
        private System.Windows.Forms.Label lblSlotsLabel;
        private System.Windows.Forms.CheckBox chkFreeItem;
        private System.Windows.Forms.NumericUpDown nudMarkup;
        private System.Windows.Forms.Label lblMarkupPercentLabel;
        private System.Windows.Forms.Label lblMarkupLabel;
        private System.Windows.Forms.TreeView treMods;
        private System.Windows.Forms.Button cmdDeleteMod;
        private System.Windows.Forms.Button cmdAddMod;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblSourceLabel;
        private TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip tipTooltip;
    }
}
