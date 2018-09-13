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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(456, 406);
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
            this.cmdOK.Location = new System.Drawing.Point(537, 406);
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
            this.lblVisbility.Location = new System.Drawing.Point(3, 33);
            this.lblVisbility.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVisbility.Name = "lblVisbility";
            this.lblVisbility.Size = new System.Drawing.Size(44, 13);
            this.lblVisbility.TabIndex = 21;
            this.lblVisbility.Tag = "Label_Visibility";
            this.lblVisbility.Text = "Visbility:";
            // 
            // cboVisibility
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.cboVisibility, 3);
            this.cboVisibility.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboVisibility.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboVisibility.FormattingEnabled = true;
            this.cboVisibility.Location = new System.Drawing.Point(76, 57);
            this.cboVisibility.Name = "cboVisibility";
            this.cboVisibility.Size = new System.Drawing.Size(215, 21);
            this.cboVisibility.TabIndex = 22;
            this.cboVisibility.SelectedIndexChanged += new System.EventHandler(this.comboBox_SelectedIndexChanged);
            // 
            // lblControl
            // 
            this.lblControl.AutoSize = true;
            this.lblControl.Location = new System.Drawing.Point(3, 60);
            this.lblControl.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblControl.Name = "lblControl";
            this.lblControl.Size = new System.Drawing.Size(43, 13);
            this.lblControl.TabIndex = 23;
            this.lblControl.Tag = "Label_Control";
            this.lblControl.Text = "Control:";
            // 
            // cboControl
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.cboControl, 3);
            this.cboControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboControl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboControl.FormattingEnabled = true;
            this.cboControl.Location = new System.Drawing.Point(76, 30);
            this.cboControl.Name = "cboControl";
            this.cboControl.Size = new System.Drawing.Size(215, 21);
            this.cboControl.TabIndex = 24;
            this.cboControl.SelectedIndexChanged += new System.EventHandler(this.comboBox_SelectedIndexChanged);
            // 
            // lblSize
            // 
            this.lblSize.AutoSize = true;
            this.lblSize.Location = new System.Drawing.Point(3, 6);
            this.lblSize.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(30, 13);
            this.lblSize.TabIndex = 25;
            this.lblSize.Tag = "Label_Size";
            this.lblSize.Text = "Size:";
            // 
            // cboSize
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.cboSize, 3);
            this.cboSize.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSize.FormattingEnabled = true;
            this.cboSize.Location = new System.Drawing.Point(76, 3);
            this.cboSize.Name = "cboSize";
            this.cboSize.Size = new System.Drawing.Size(215, 21);
            this.cboSize.TabIndex = 26;
            this.cboSize.SelectedIndexChanged += new System.EventHandler(this.cboSize_SelectedIndexChanged);
            // 
            // lblFlexibility
            // 
            this.lblFlexibility.AutoSize = true;
            this.lblFlexibility.Location = new System.Drawing.Point(3, 87);
            this.lblFlexibility.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblFlexibility.Name = "lblFlexibility";
            this.lblFlexibility.Size = new System.Drawing.Size(51, 13);
            this.lblFlexibility.TabIndex = 27;
            this.lblFlexibility.Tag = "Label_Flexibility";
            this.lblFlexibility.Text = "Flexibility:";
            // 
            // cboFlexibility
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.cboFlexibility, 3);
            this.cboFlexibility.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboFlexibility.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFlexibility.FormattingEnabled = true;
            this.cboFlexibility.Location = new System.Drawing.Point(76, 84);
            this.cboFlexibility.Name = "cboFlexibility";
            this.cboFlexibility.Size = new System.Drawing.Size(215, 21);
            this.cboFlexibility.TabIndex = 28;
            this.cboFlexibility.SelectedIndexChanged += new System.EventHandler(this.comboBox_SelectedIndexChanged);
            // 
            // lblAvailabilityLabel
            // 
            this.lblAvailabilityLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblAvailabilityLabel.AutoSize = true;
            this.lblAvailabilityLabel.Location = new System.Drawing.Point(3, 114);
            this.lblAvailabilityLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAvailabilityLabel.Name = "lblAvailabilityLabel";
            this.lblAvailabilityLabel.Size = new System.Drawing.Size(33, 13);
            this.lblAvailabilityLabel.TabIndex = 30;
            this.lblAvailabilityLabel.Tag = "Label_Avail";
            this.lblAvailabilityLabel.Text = "Avail:";
            // 
            // lblCostLabel
            // 
            this.lblCostLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblCostLabel.AutoSize = true;
            this.lblCostLabel.Location = new System.Drawing.Point(3, 164);
            this.lblCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCostLabel.Name = "lblCostLabel";
            this.lblCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblCostLabel.TabIndex = 29;
            this.lblCostLabel.Tag = "Label_Cost";
            this.lblCostLabel.Text = "Cost:";
            // 
            // lblAvailability
            // 
            this.lblAvailability.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblAvailability.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblAvailability, 3);
            this.lblAvailability.Location = new System.Drawing.Point(76, 114);
            this.lblAvailability.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAvailability.Name = "lblAvailability";
            this.lblAvailability.Size = new System.Drawing.Size(19, 13);
            this.lblAvailability.TabIndex = 32;
            this.lblAvailability.Tag = "";
            this.lblAvailability.Text = "+0";
            // 
            // lblCost
            // 
            this.lblCost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblCost.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblCost, 3);
            this.lblCost.Location = new System.Drawing.Point(76, 164);
            this.lblCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCost.Name = "lblCost";
            this.lblCost.Size = new System.Drawing.Size(19, 13);
            this.lblCost.TabIndex = 31;
            this.lblCost.Tag = "";
            this.lblCost.Text = "0¥";
            // 
            // lblSlots
            // 
            this.lblSlots.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSlots.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblSlots, 3);
            this.lblSlots.Location = new System.Drawing.Point(76, 139);
            this.lblSlots.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSlots.Name = "lblSlots";
            this.lblSlots.Size = new System.Drawing.Size(13, 13);
            this.lblSlots.TabIndex = 34;
            this.lblSlots.Tag = "";
            this.lblSlots.Text = "0";
            // 
            // lblSlotsLabel
            // 
            this.lblSlotsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSlotsLabel.AutoSize = true;
            this.lblSlotsLabel.Location = new System.Drawing.Point(3, 139);
            this.lblSlotsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSlotsLabel.Name = "lblSlotsLabel";
            this.lblSlotsLabel.Size = new System.Drawing.Size(33, 13);
            this.lblSlotsLabel.TabIndex = 33;
            this.lblSlotsLabel.Tag = "Label_Slots";
            this.lblSlotsLabel.Text = "Slots:";
            // 
            // chkFreeItem
            // 
            this.chkFreeItem.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.chkFreeItem.AutoSize = true;
            this.chkFreeItem.Location = new System.Drawing.Point(3, 187);
            this.chkFreeItem.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.nudMarkup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudMarkup.Location = new System.Drawing.Point(76, 211);
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
            this.nudMarkup.Size = new System.Drawing.Size(67, 20);
            this.nudMarkup.TabIndex = 63;
            this.nudMarkup.ValueChanged += new System.EventHandler(this.nudMarkup_ValueChanged);
            // 
            // lblMarkupPercentLabel
            // 
            this.lblMarkupPercentLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblMarkupPercentLabel.AutoSize = true;
            this.lblMarkupPercentLabel.Location = new System.Drawing.Point(149, 214);
            this.lblMarkupPercentLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMarkupPercentLabel.Name = "lblMarkupPercentLabel";
            this.lblMarkupPercentLabel.Size = new System.Drawing.Size(15, 14);
            this.lblMarkupPercentLabel.TabIndex = 64;
            this.lblMarkupPercentLabel.Text = "%";
            // 
            // lblMarkupLabel
            // 
            this.lblMarkupLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblMarkupLabel.AutoSize = true;
            this.lblMarkupLabel.Location = new System.Drawing.Point(3, 214);
            this.lblMarkupLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMarkupLabel.Name = "lblMarkupLabel";
            this.lblMarkupLabel.Size = new System.Drawing.Size(46, 14);
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
            this.treMods.Size = new System.Drawing.Size(300, 394);
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
            this.lblSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSource.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblSource, 3);
            this.lblSource.Location = new System.Drawing.Point(76, 240);
            this.lblSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 119);
            this.lblSource.TabIndex = 69;
            this.lblSource.Text = "[Source]";
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(3, 240);
            this.lblSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 119);
            this.lblSourceLabel.TabIndex = 68;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.lblSize, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblControl, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblVisbility, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblFlexibility, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.cboSize, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.cboFlexibility, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.cboControl, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblAvailability, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.cboVisibility, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblAvailabilityLabel, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblSourceLabel, 0, 9);
            this.tableLayoutPanel1.Controls.Add(this.lblMarkupLabel, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.lblSource, 1, 9);
            this.tableLayoutPanel1.Controls.Add(this.lblMarkupPercentLabel, 2, 8);
            this.tableLayoutPanel1.Controls.Add(this.nudMarkup, 1, 8);
            this.tableLayoutPanel1.Controls.Add(this.lblSlotsLabel, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.lblCostLabel, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.chkFreeItem, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.lblCost, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.lblSlots, 1, 5);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(318, 35);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 10;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(294, 365);
            this.tableLayoutPanel1.TabIndex = 70;
            // 
            // frmCreateWeaponMount
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.cmdDeleteMod);
            this.Controls.Add(this.cmdAddMod);
            this.Controls.Add(this.treMods);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "frmCreateWeaponMount";
            this.Tag = "Title_CreateWeaponMount";
            this.Text = "Create Weapon Mount";
            this.Load += new System.EventHandler(this.frmCreateWeaponMount_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

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
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
