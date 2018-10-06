namespace Chummer
{
    partial class frmSelectVehicle
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
            this.components = new System.ComponentModel.Container();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearchLabel = new System.Windows.Forms.Label();
            this.lblVehiclePilot = new System.Windows.Forms.Label();
            this.lblVehiclePilotLabel = new System.Windows.Forms.Label();
            this.lblVehicleArmor = new System.Windows.Forms.Label();
            this.lblVehicleArmorLabel = new System.Windows.Forms.Label();
            this.lblVehicleBody = new System.Windows.Forms.Label();
            this.lblVehicleBodyLabel = new System.Windows.Forms.Label();
            this.lblVehicleSpeed = new System.Windows.Forms.Label();
            this.lblVehicleSpeedLabel = new System.Windows.Forms.Label();
            this.lblVehicleCost = new System.Windows.Forms.Label();
            this.lblVehicleCostLabel = new System.Windows.Forms.Label();
            this.lblVehicleAvail = new System.Windows.Forms.Label();
            this.lblVehicleAvailLabel = new System.Windows.Forms.Label();
            this.lblVehicleAccel = new System.Windows.Forms.Label();
            this.lblVehicleAccelLabel = new System.Windows.Forms.Label();
            this.lblVehicleHandling = new System.Windows.Forms.Label();
            this.lblVehicleHandlingLabel = new System.Windows.Forms.Label();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lstVehicle = new System.Windows.Forms.ListBox();
            this.lblCategory = new System.Windows.Forms.Label();
            this.cboCategory = new Chummer.ElasticComboBox();
            this.lblVehicleSensor = new System.Windows.Forms.Label();
            this.lblVehicleSensorLabel = new System.Windows.Forms.Label();
            this.cmdOKAdd = new System.Windows.Forms.Button();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.chkFreeItem = new System.Windows.Forms.CheckBox();
            this.chkUsedVehicle = new System.Windows.Forms.CheckBox();
            this.lblUsedVehicleDiscountLabel = new System.Windows.Forms.Label();
            this.nudUsedVehicleDiscount = new System.Windows.Forms.NumericUpDown();
            this.lblUsedVehicleDiscountPercentLabel = new System.Windows.Forms.Label();
            this.nudMarkup = new System.Windows.Forms.NumericUpDown();
            this.lblMarkupLabel = new System.Windows.Forms.Label();
            this.lblMarkupPercentLabel = new System.Windows.Forms.Label();
            this.lblTest = new System.Windows.Forms.Label();
            this.lblTestLabel = new System.Windows.Forms.Label();
            this.chkBlackMarketDiscount = new System.Windows.Forms.CheckBox();
            this.lblVehicleSeatsLabel = new System.Windows.Forms.Label();
            this.lblVehicleSeats = new System.Windows.Forms.Label();
            this.chkHideOverAvailLimit = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.chkShowOnlyAffordItems = new System.Windows.Forms.CheckBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.nudUsedVehicleDiscount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.txtSearch, 4);
            this.txtSearch.Location = new System.Drawing.Point(365, 3);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(238, 20);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
            this.txtSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyUp);
            // 
            // lblSearchLabel
            // 
            this.lblSearchLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSearchLabel.AutoSize = true;
            this.lblSearchLabel.Location = new System.Drawing.Point(315, 6);
            this.lblSearchLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSearchLabel.Name = "lblSearchLabel";
            this.lblSearchLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSearchLabel.TabIndex = 0;
            this.lblSearchLabel.Tag = "Label_Search";
            this.lblSearchLabel.Text = "&Search:";
            // 
            // lblVehiclePilot
            // 
            this.lblVehiclePilot.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblVehiclePilot, 2);
            this.lblVehiclePilot.Location = new System.Drawing.Point(529, 57);
            this.lblVehiclePilot.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehiclePilot.Name = "lblVehiclePilot";
            this.lblVehiclePilot.Size = new System.Drawing.Size(33, 13);
            this.lblVehiclePilot.TabIndex = 9;
            this.lblVehiclePilot.Text = "[Pilot]";
            // 
            // lblVehiclePilotLabel
            // 
            this.lblVehiclePilotLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehiclePilotLabel.AutoSize = true;
            this.lblVehiclePilotLabel.Location = new System.Drawing.Point(493, 57);
            this.lblVehiclePilotLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehiclePilotLabel.Name = "lblVehiclePilotLabel";
            this.lblVehiclePilotLabel.Size = new System.Drawing.Size(30, 13);
            this.lblVehiclePilotLabel.TabIndex = 8;
            this.lblVehiclePilotLabel.Tag = "Label_Pilot";
            this.lblVehiclePilotLabel.Text = "Pilot:";
            // 
            // lblVehicleArmor
            // 
            this.lblVehicleArmor.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblVehicleArmor, 2);
            this.lblVehicleArmor.Location = new System.Drawing.Point(529, 82);
            this.lblVehicleArmor.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleArmor.Name = "lblVehicleArmor";
            this.lblVehicleArmor.Size = new System.Drawing.Size(40, 13);
            this.lblVehicleArmor.TabIndex = 13;
            this.lblVehicleArmor.Text = "[Armor]";
            // 
            // lblVehicleArmorLabel
            // 
            this.lblVehicleArmorLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleArmorLabel.AutoSize = true;
            this.lblVehicleArmorLabel.Location = new System.Drawing.Point(486, 82);
            this.lblVehicleArmorLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleArmorLabel.Name = "lblVehicleArmorLabel";
            this.lblVehicleArmorLabel.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleArmorLabel.TabIndex = 12;
            this.lblVehicleArmorLabel.Tag = "Label_Armor";
            this.lblVehicleArmorLabel.Text = "Armor:";
            // 
            // lblVehicleBody
            // 
            this.lblVehicleBody.AutoSize = true;
            this.lblVehicleBody.Location = new System.Drawing.Point(365, 82);
            this.lblVehicleBody.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleBody.Name = "lblVehicleBody";
            this.lblVehicleBody.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleBody.TabIndex = 11;
            this.lblVehicleBody.Text = "[Body]";
            // 
            // lblVehicleBodyLabel
            // 
            this.lblVehicleBodyLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleBodyLabel.AutoSize = true;
            this.lblVehicleBodyLabel.Location = new System.Drawing.Point(325, 82);
            this.lblVehicleBodyLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleBodyLabel.Name = "lblVehicleBodyLabel";
            this.lblVehicleBodyLabel.Size = new System.Drawing.Size(34, 13);
            this.lblVehicleBodyLabel.TabIndex = 10;
            this.lblVehicleBodyLabel.Tag = "Label_Body";
            this.lblVehicleBodyLabel.Text = "Body:";
            // 
            // lblVehicleSpeed
            // 
            this.lblVehicleSpeed.AutoSize = true;
            this.lblVehicleSpeed.Location = new System.Drawing.Point(365, 57);
            this.lblVehicleSpeed.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSpeed.Name = "lblVehicleSpeed";
            this.lblVehicleSpeed.Size = new System.Drawing.Size(44, 13);
            this.lblVehicleSpeed.TabIndex = 7;
            this.lblVehicleSpeed.Text = "[Speed]";
            // 
            // lblVehicleSpeedLabel
            // 
            this.lblVehicleSpeedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleSpeedLabel.AutoSize = true;
            this.lblVehicleSpeedLabel.Location = new System.Drawing.Point(318, 57);
            this.lblVehicleSpeedLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSpeedLabel.Name = "lblVehicleSpeedLabel";
            this.lblVehicleSpeedLabel.Size = new System.Drawing.Size(41, 13);
            this.lblVehicleSpeedLabel.TabIndex = 6;
            this.lblVehicleSpeedLabel.Tag = "Label_Speed";
            this.lblVehicleSpeedLabel.Text = "Speed:";
            // 
            // lblVehicleCost
            // 
            this.lblVehicleCost.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblVehicleCost, 2);
            this.lblVehicleCost.Location = new System.Drawing.Point(365, 157);
            this.lblVehicleCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleCost.Name = "lblVehicleCost";
            this.lblVehicleCost.Size = new System.Drawing.Size(34, 13);
            this.lblVehicleCost.TabIndex = 21;
            this.lblVehicleCost.Text = "[Cost]";
            // 
            // lblVehicleCostLabel
            // 
            this.lblVehicleCostLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleCostLabel.AutoSize = true;
            this.lblVehicleCostLabel.Location = new System.Drawing.Point(328, 157);
            this.lblVehicleCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleCostLabel.Name = "lblVehicleCostLabel";
            this.lblVehicleCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblVehicleCostLabel.TabIndex = 20;
            this.lblVehicleCostLabel.Tag = "Label_Cost";
            this.lblVehicleCostLabel.Text = "Cost:";
            // 
            // lblVehicleAvail
            // 
            this.lblVehicleAvail.AutoSize = true;
            this.lblVehicleAvail.Location = new System.Drawing.Point(365, 132);
            this.lblVehicleAvail.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleAvail.Name = "lblVehicleAvail";
            this.lblVehicleAvail.Size = new System.Drawing.Size(36, 13);
            this.lblVehicleAvail.TabIndex = 17;
            this.lblVehicleAvail.Text = "[Avail]";
            // 
            // lblVehicleAvailLabel
            // 
            this.lblVehicleAvailLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleAvailLabel.AutoSize = true;
            this.lblVehicleAvailLabel.Location = new System.Drawing.Point(326, 132);
            this.lblVehicleAvailLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleAvailLabel.Name = "lblVehicleAvailLabel";
            this.lblVehicleAvailLabel.Size = new System.Drawing.Size(33, 13);
            this.lblVehicleAvailLabel.TabIndex = 16;
            this.lblVehicleAvailLabel.Tag = "Label_Avail";
            this.lblVehicleAvailLabel.Text = "Avail:";
            // 
            // lblVehicleAccel
            // 
            this.lblVehicleAccel.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblVehicleAccel, 2);
            this.lblVehicleAccel.Location = new System.Drawing.Point(529, 32);
            this.lblVehicleAccel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleAccel.Name = "lblVehicleAccel";
            this.lblVehicleAccel.Size = new System.Drawing.Size(40, 13);
            this.lblVehicleAccel.TabIndex = 5;
            this.lblVehicleAccel.Text = "[Accel]";
            // 
            // lblVehicleAccelLabel
            // 
            this.lblVehicleAccelLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleAccelLabel.AutoSize = true;
            this.lblVehicleAccelLabel.Location = new System.Drawing.Point(486, 32);
            this.lblVehicleAccelLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleAccelLabel.Name = "lblVehicleAccelLabel";
            this.lblVehicleAccelLabel.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleAccelLabel.TabIndex = 4;
            this.lblVehicleAccelLabel.Tag = "Label_Accel";
            this.lblVehicleAccelLabel.Text = "Accel:";
            // 
            // lblVehicleHandling
            // 
            this.lblVehicleHandling.AutoSize = true;
            this.lblVehicleHandling.Location = new System.Drawing.Point(365, 32);
            this.lblVehicleHandling.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleHandling.Name = "lblVehicleHandling";
            this.lblVehicleHandling.Size = new System.Drawing.Size(55, 13);
            this.lblVehicleHandling.TabIndex = 3;
            this.lblVehicleHandling.Text = "[Handling]";
            // 
            // lblVehicleHandlingLabel
            // 
            this.lblVehicleHandlingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleHandlingLabel.AutoSize = true;
            this.lblVehicleHandlingLabel.Location = new System.Drawing.Point(307, 32);
            this.lblVehicleHandlingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleHandlingLabel.Name = "lblVehicleHandlingLabel";
            this.lblVehicleHandlingLabel.Size = new System.Drawing.Size(52, 13);
            this.lblVehicleHandlingLabel.TabIndex = 2;
            this.lblVehicleHandlingLabel.Tag = "Label_Handling";
            this.lblVehicleHandlingLabel.Text = "Handling:";
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
            this.cmdCancel.TabIndex = 37;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.AutoSize = true;
            this.cmdOK.Location = new System.Drawing.Point(162, 0);
            this.cmdOK.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 35;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lstVehicle
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.lstVehicle, 2);
            this.lstVehicle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstVehicle.FormattingEnabled = true;
            this.lstVehicle.Location = new System.Drawing.Point(3, 30);
            this.lstVehicle.Name = "lstVehicle";
            this.lstVehicle.Size = new System.Drawing.Size(295, 390);
            this.lstVehicle.TabIndex = 32;
            this.lstVehicle.SelectedIndexChanged += new System.EventHandler(this.lstVehicle_SelectedIndexChanged);
            this.lstVehicle.DoubleClick += new System.EventHandler(this.lstVehicle_DoubleClick);
            // 
            // lblCategory
            // 
            this.lblCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(3, 6);
            this.lblCategory.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(52, 13);
            this.lblCategory.TabIndex = 33;
            this.lblCategory.Tag = "Label_Category";
            this.lblCategory.Text = "Category:";
            // 
            // cboCategory
            // 
            this.cboCategory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategory.FormattingEnabled = true;
            this.cboCategory.Location = new System.Drawing.Point(61, 3);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(237, 21);
            this.cboCategory.TabIndex = 34;
            this.cboCategory.TooltipText = "";
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // lblVehicleSensor
            // 
            this.lblVehicleSensor.AutoSize = true;
            this.lblVehicleSensor.Location = new System.Drawing.Point(365, 107);
            this.lblVehicleSensor.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSensor.Name = "lblVehicleSensor";
            this.lblVehicleSensor.Size = new System.Drawing.Size(46, 13);
            this.lblVehicleSensor.TabIndex = 15;
            this.lblVehicleSensor.Text = "[Sensor]";
            // 
            // lblVehicleSensorLabel
            // 
            this.lblVehicleSensorLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleSensorLabel.AutoSize = true;
            this.lblVehicleSensorLabel.Location = new System.Drawing.Point(316, 107);
            this.lblVehicleSensorLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSensorLabel.Name = "lblVehicleSensorLabel";
            this.lblVehicleSensorLabel.Size = new System.Drawing.Size(43, 13);
            this.lblVehicleSensorLabel.TabIndex = 14;
            this.lblVehicleSensorLabel.Tag = "Label_Sensor";
            this.lblVehicleSensorLabel.Text = "Sensor:";
            // 
            // cmdOKAdd
            // 
            this.cmdOKAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOKAdd.AutoSize = true;
            this.cmdOKAdd.Location = new System.Drawing.Point(81, 0);
            this.cmdOKAdd.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(75, 23);
            this.cmdOKAdd.TabIndex = 36;
            this.cmdOKAdd.Tag = "String_AddMore";
            this.cmdOKAdd.Text = "&Add && More";
            this.cmdOKAdd.UseVisualStyleBackColor = true;
            this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblSource, 2);
            this.lblSource.Location = new System.Drawing.Point(365, 259);
            this.lblSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 31;
            this.lblSource.Text = "[Source]";
            this.lblSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(315, 259);
            this.lblSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 30;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            // 
            // chkFreeItem
            // 
            this.chkFreeItem.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkFreeItem.AutoSize = true;
            this.chkFreeItem.Location = new System.Drawing.Point(304, 180);
            this.chkFreeItem.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkFreeItem.Name = "chkFreeItem";
            this.chkFreeItem.Size = new System.Drawing.Size(55, 17);
            this.chkFreeItem.TabIndex = 26;
            this.chkFreeItem.Tag = "Checkbox_Free";
            this.chkFreeItem.Text = "Free!";
            this.chkFreeItem.UseVisualStyleBackColor = true;
            this.chkFreeItem.CheckedChanged += new System.EventHandler(this.chkFreeItem_CheckedChanged);
            // 
            // chkUsedVehicle
            // 
            this.chkUsedVehicle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkUsedVehicle.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkUsedVehicle, 2);
            this.chkUsedVehicle.Location = new System.Drawing.Point(304, 205);
            this.chkUsedVehicle.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkUsedVehicle.Name = "chkUsedVehicle";
            this.chkUsedVehicle.Size = new System.Drawing.Size(137, 18);
            this.chkUsedVehicle.TabIndex = 22;
            this.chkUsedVehicle.Tag = "Checkbox_SelectVehicle_UsedVehicle";
            this.chkUsedVehicle.Text = "Used Vehicle";
            this.chkUsedVehicle.UseVisualStyleBackColor = true;
            this.chkUsedVehicle.Visible = false;
            this.chkUsedVehicle.CheckedChanged += new System.EventHandler(this.chkUsedVehicle_CheckedChanged);
            // 
            // lblUsedVehicleDiscountLabel
            // 
            this.lblUsedVehicleDiscountLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblUsedVehicleDiscountLabel.AutoSize = true;
            this.lblUsedVehicleDiscountLabel.Location = new System.Drawing.Point(471, 207);
            this.lblUsedVehicleDiscountLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblUsedVehicleDiscountLabel.Name = "lblUsedVehicleDiscountLabel";
            this.lblUsedVehicleDiscountLabel.Size = new System.Drawing.Size(52, 13);
            this.lblUsedVehicleDiscountLabel.TabIndex = 23;
            this.lblUsedVehicleDiscountLabel.Tag = "Label_SelectVehicle_Discount";
            this.lblUsedVehicleDiscountLabel.Text = "Discount:";
            this.lblUsedVehicleDiscountLabel.Visible = false;
            // 
            // nudUsedVehicleDiscount
            // 
            this.nudUsedVehicleDiscount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudUsedVehicleDiscount.DecimalPlaces = 2;
            this.nudUsedVehicleDiscount.Location = new System.Drawing.Point(529, 204);
            this.nudUsedVehicleDiscount.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.nudUsedVehicleDiscount.Minimum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nudUsedVehicleDiscount.Name = "nudUsedVehicleDiscount";
            this.nudUsedVehicleDiscount.Size = new System.Drawing.Size(55, 20);
            this.nudUsedVehicleDiscount.TabIndex = 24;
            this.nudUsedVehicleDiscount.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nudUsedVehicleDiscount.Visible = false;
            this.nudUsedVehicleDiscount.ValueChanged += new System.EventHandler(this.nudUsedVehicleDiscount_ValueChanged);
            // 
            // lblUsedVehicleDiscountPercentLabel
            // 
            this.lblUsedVehicleDiscountPercentLabel.AutoSize = true;
            this.lblUsedVehicleDiscountPercentLabel.Location = new System.Drawing.Point(590, 207);
            this.lblUsedVehicleDiscountPercentLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblUsedVehicleDiscountPercentLabel.Name = "lblUsedVehicleDiscountPercentLabel";
            this.lblUsedVehicleDiscountPercentLabel.Size = new System.Drawing.Size(13, 13);
            this.lblUsedVehicleDiscountPercentLabel.TabIndex = 25;
            this.lblUsedVehicleDiscountPercentLabel.Text = "%";
            this.lblUsedVehicleDiscountPercentLabel.Visible = false;
            // 
            // nudMarkup
            // 
            this.nudMarkup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudMarkup.DecimalPlaces = 2;
            this.nudMarkup.Location = new System.Drawing.Point(365, 230);
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
            this.nudMarkup.Size = new System.Drawing.Size(76, 20);
            this.nudMarkup.TabIndex = 28;
            this.nudMarkup.ValueChanged += new System.EventHandler(this.nudMarkup_ValueChanged);
            // 
            // lblMarkupLabel
            // 
            this.lblMarkupLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMarkupLabel.AutoSize = true;
            this.lblMarkupLabel.Location = new System.Drawing.Point(313, 233);
            this.lblMarkupLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMarkupLabel.Name = "lblMarkupLabel";
            this.lblMarkupLabel.Size = new System.Drawing.Size(46, 13);
            this.lblMarkupLabel.TabIndex = 27;
            this.lblMarkupLabel.Tag = "Label_SelectGear_Markup";
            this.lblMarkupLabel.Text = "Markup:";
            // 
            // lblMarkupPercentLabel
            // 
            this.lblMarkupPercentLabel.AutoSize = true;
            this.lblMarkupPercentLabel.Location = new System.Drawing.Point(447, 233);
            this.lblMarkupPercentLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMarkupPercentLabel.Name = "lblMarkupPercentLabel";
            this.lblMarkupPercentLabel.Size = new System.Drawing.Size(15, 13);
            this.lblMarkupPercentLabel.TabIndex = 29;
            this.lblMarkupPercentLabel.Text = "%";
            // 
            // lblTest
            // 
            this.lblTest.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblTest, 2);
            this.lblTest.Location = new System.Drawing.Point(529, 132);
            this.lblTest.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTest.Name = "lblTest";
            this.lblTest.Size = new System.Drawing.Size(19, 13);
            this.lblTest.TabIndex = 19;
            this.lblTest.Text = "[0]";
            // 
            // lblTestLabel
            // 
            this.lblTestLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTestLabel.AutoSize = true;
            this.lblTestLabel.Location = new System.Drawing.Point(492, 132);
            this.lblTestLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTestLabel.Name = "lblTestLabel";
            this.lblTestLabel.Size = new System.Drawing.Size(31, 13);
            this.lblTestLabel.TabIndex = 18;
            this.lblTestLabel.Tag = "Label_Test";
            this.lblTestLabel.Text = "Test:";
            // 
            // chkBlackMarketDiscount
            // 
            this.chkBlackMarketDiscount.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkBlackMarketDiscount.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkBlackMarketDiscount, 4);
            this.chkBlackMarketDiscount.Location = new System.Drawing.Point(365, 180);
            this.chkBlackMarketDiscount.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkBlackMarketDiscount.Name = "chkBlackMarketDiscount";
            this.chkBlackMarketDiscount.Size = new System.Drawing.Size(238, 17);
            this.chkBlackMarketDiscount.TabIndex = 38;
            this.chkBlackMarketDiscount.Tag = "Checkbox_BlackMarketDiscount";
            this.chkBlackMarketDiscount.Text = "Black Market Discount (10%)";
            this.chkBlackMarketDiscount.UseVisualStyleBackColor = true;
            this.chkBlackMarketDiscount.Visible = false;
            this.chkBlackMarketDiscount.CheckedChanged += new System.EventHandler(this.chkBlackMarketDiscount_CheckedChanged);
            // 
            // lblVehicleSeatsLabel
            // 
            this.lblVehicleSeatsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleSeatsLabel.AutoSize = true;
            this.lblVehicleSeatsLabel.Location = new System.Drawing.Point(486, 107);
            this.lblVehicleSeatsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSeatsLabel.Name = "lblVehicleSeatsLabel";
            this.lblVehicleSeatsLabel.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleSeatsLabel.TabIndex = 39;
            this.lblVehicleSeatsLabel.Tag = "Label_Seats";
            this.lblVehicleSeatsLabel.Text = "Seats:";
            // 
            // lblVehicleSeats
            // 
            this.lblVehicleSeats.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblVehicleSeats, 2);
            this.lblVehicleSeats.Location = new System.Drawing.Point(529, 107);
            this.lblVehicleSeats.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSeats.Name = "lblVehicleSeats";
            this.lblVehicleSeats.Size = new System.Drawing.Size(40, 13);
            this.lblVehicleSeats.TabIndex = 40;
            this.lblVehicleSeats.Text = "[Seats]";
            // 
            // chkHideOverAvailLimit
            // 
            this.chkHideOverAvailLimit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkHideOverAvailLimit.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkHideOverAvailLimit, 5);
            this.chkHideOverAvailLimit.Location = new System.Drawing.Point(304, 282);
            this.chkHideOverAvailLimit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkHideOverAvailLimit.Name = "chkHideOverAvailLimit";
            this.chkHideOverAvailLimit.Size = new System.Drawing.Size(299, 17);
            this.chkHideOverAvailLimit.TabIndex = 65;
            this.chkHideOverAvailLimit.Tag = "Checkbox_HideOverAvailLimit";
            this.chkHideOverAvailLimit.Text = "Hide Items Over Avail Limit ({0})";
            this.chkHideOverAvailLimit.UseVisualStyleBackColor = true;
            this.chkHideOverAvailLimit.CheckedChanged += new System.EventHandler(this.chkHideOverAvailLimit_CheckedChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 6;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 301F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.chkHideOverAvailLimit, 1, 11);
            this.tableLayoutPanel1.Controls.Add(this.txtSearch, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblVehicleHandlingLabel, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblSearchLabel, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblSource, 2, 10);
            this.tableLayoutPanel1.Controls.Add(this.nudMarkup, 2, 9);
            this.tableLayoutPanel1.Controls.Add(this.lblSourceLabel, 1, 10);
            this.tableLayoutPanel1.Controls.Add(this.lblVehicleSeats, 4, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblMarkupLabel, 1, 9);
            this.tableLayoutPanel1.Controls.Add(this.lblTest, 4, 5);
            this.tableLayoutPanel1.Controls.Add(this.lblVehicleHandling, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblTestLabel, 3, 5);
            this.tableLayoutPanel1.Controls.Add(this.lblVehicleSeatsLabel, 3, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblVehicleSpeedLabel, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblVehicleSpeed, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.chkFreeItem, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.lblVehicleBodyLabel, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblVehicleBody, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblVehicleAccelLabel, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblUsedVehicleDiscountPercentLabel, 5, 8);
            this.tableLayoutPanel1.Controls.Add(this.lblVehicleAccel, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblVehiclePilotLabel, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblVehicleCost, 2, 6);
            this.tableLayoutPanel1.Controls.Add(this.lblVehicleArmorLabel, 3, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblVehicleCostLabel, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.lblVehicleArmor, 4, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblVehicleSensorLabel, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblVehicleSensor, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblVehicleAvailLabel, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.lblVehicleAvail, 2, 5);
            this.tableLayoutPanel1.Controls.Add(this.chkUsedVehicle, 1, 8);
            this.tableLayoutPanel1.Controls.Add(this.chkBlackMarketDiscount, 2, 7);
            this.tableLayoutPanel1.Controls.Add(this.nudUsedVehicleDiscount, 4, 8);
            this.tableLayoutPanel1.Controls.Add(this.lblUsedVehicleDiscountLabel, 3, 8);
            this.tableLayoutPanel1.Controls.Add(this.lblVehiclePilot, 4, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblMarkupPercentLabel, 3, 9);
            this.tableLayoutPanel1.Controls.Add(this.chkShowOnlyAffordItems, 1, 12);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 13);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(9, 9);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 14;
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
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(606, 423);
            this.tableLayoutPanel1.TabIndex = 66;
            // 
            // chkShowOnlyAffordItems
            // 
            this.chkShowOnlyAffordItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkShowOnlyAffordItems.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkShowOnlyAffordItems, 5);
            this.chkShowOnlyAffordItems.Location = new System.Drawing.Point(304, 307);
            this.chkShowOnlyAffordItems.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkShowOnlyAffordItems.Name = "chkShowOnlyAffordItems";
            this.chkShowOnlyAffordItems.Size = new System.Drawing.Size(299, 17);
            this.chkShowOnlyAffordItems.TabIndex = 67;
            this.chkShowOnlyAffordItems.Tag = "Checkbox_ShowOnlyAffordItems";
            this.chkShowOnlyAffordItems.Text = "Show Only Items I Can Afford";
            this.chkShowOnlyAffordItems.UseVisualStyleBackColor = true;
            this.chkShowOnlyAffordItems.CheckedChanged += new System.EventHandler(this.chkShowOnlyAffordItems_CheckedChanged);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 5);
            this.flowLayoutPanel1.Controls.Add(this.cmdOK);
            this.flowLayoutPanel1.Controls.Add(this.cmdOKAdd);
            this.flowLayoutPanel1.Controls.Add(this.cmdCancel);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(366, 397);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(237, 23);
            this.flowLayoutPanel1.TabIndex = 68;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.lblCategory, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.cboCategory, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.lstVehicle, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel1.SetRowSpan(this.tableLayoutPanel2, 14);
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(301, 423);
            this.tableLayoutPanel2.TabIndex = 69;
            // 
            // frmSelectVehicle
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(640, 10000);
            this.MinimizeBox = false;
            this.Name = "frmSelectVehicle";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Tag = "Title_SelectVehicle";
            this.Text = "Select a Vehicle";
            this.Load += new System.EventHandler(this.frmSelectVehicle_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudUsedVehicleDiscount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearchLabel;
        private System.Windows.Forms.Label lblVehiclePilot;
        private System.Windows.Forms.Label lblVehiclePilotLabel;
        private System.Windows.Forms.Label lblVehicleArmor;
        private System.Windows.Forms.Label lblVehicleArmorLabel;
        private System.Windows.Forms.Label lblVehicleBody;
        private System.Windows.Forms.Label lblVehicleBodyLabel;
        private System.Windows.Forms.Label lblVehicleSpeed;
        private System.Windows.Forms.Label lblVehicleSpeedLabel;
        private System.Windows.Forms.Label lblVehicleCost;
        private System.Windows.Forms.Label lblVehicleCostLabel;
        private System.Windows.Forms.Label lblVehicleAvail;
        private System.Windows.Forms.Label lblVehicleAvailLabel;
        private System.Windows.Forms.Label lblVehicleAccel;
        private System.Windows.Forms.Label lblVehicleAccelLabel;
        private System.Windows.Forms.Label lblVehicleHandling;
        private System.Windows.Forms.Label lblVehicleHandlingLabel;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.ListBox lstVehicle;
        private System.Windows.Forms.Label lblCategory;
        private ElasticComboBox cboCategory;
        private System.Windows.Forms.Label lblVehicleSensor;
        private System.Windows.Forms.Label lblVehicleSensorLabel;
        private System.Windows.Forms.Button cmdOKAdd;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblSourceLabel;
        private System.Windows.Forms.CheckBox chkFreeItem;
        private System.Windows.Forms.CheckBox chkUsedVehicle;
        private System.Windows.Forms.Label lblUsedVehicleDiscountLabel;
        private System.Windows.Forms.NumericUpDown nudUsedVehicleDiscount;
        private System.Windows.Forms.Label lblUsedVehicleDiscountPercentLabel;
        private System.Windows.Forms.NumericUpDown nudMarkup;
        private System.Windows.Forms.Label lblMarkupLabel;
        private System.Windows.Forms.Label lblMarkupPercentLabel;
        private System.Windows.Forms.Label lblTest;
        private System.Windows.Forms.Label lblTestLabel;
        private System.Windows.Forms.CheckBox chkBlackMarketDiscount;
        private System.Windows.Forms.Label lblVehicleSeatsLabel;
        private System.Windows.Forms.Label lblVehicleSeats;
        private System.Windows.Forms.CheckBox chkHideOverAvailLimit;
        private Chummer.BufferedTableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckBox chkShowOnlyAffordItems;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    }
}
