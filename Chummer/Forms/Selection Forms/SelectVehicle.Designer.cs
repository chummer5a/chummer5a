namespace Chummer
{
    partial class SelectVehicle
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
                Utils.ListItemListPool.Return(_lstCategory);
                Utils.StringHashSetPool.Return(_setDealerConnectionMaps);
                Utils.StringHashSetPool.Return(_setBlackMarketMaps);
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabListView = new System.Windows.Forms.TabPage();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.chkHideOverAvailLimit = new Chummer.ColorableCheckBox(this.components);
            this.chkShowOnlyAffordItems = new Chummer.ColorableCheckBox(this.components);
            this.tableLayoutPanel2 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lstVehicle = new System.Windows.Forms.ListBox();
            this.tlpRight = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblSource = new System.Windows.Forms.Label();
            this.lblVehicleHandlingLabel = new System.Windows.Forms.Label();
            this.lblVehicleCost = new System.Windows.Forms.Label();
            this.lblVehicleSeats = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.flpDiscount = new System.Windows.Forms.FlowLayoutPanel();
            this.chkUsedVehicle = new Chummer.ColorableCheckBox(this.components);
            this.lblUsedVehicleDiscountLabel = new System.Windows.Forms.Label();
            this.nudUsedVehicleDiscount = new Chummer.NumericUpDownEx();
            this.lblUsedVehicleDiscountPercentLabel = new System.Windows.Forms.Label();
            this.lblVehicleCostLabel = new System.Windows.Forms.Label();
            this.flpCheckBoxes = new System.Windows.Forms.FlowLayoutPanel();
            this.chkFreeItem = new Chummer.ColorableCheckBox(this.components);
            this.chkBlackMarketDiscount = new Chummer.ColorableCheckBox(this.components);
            this.flpMarkup = new System.Windows.Forms.FlowLayoutPanel();
            this.nudMarkup = new Chummer.NumericUpDownEx();
            this.lblMarkupPercentLabel = new System.Windows.Forms.Label();
            this.lblMarkupLabel = new System.Windows.Forms.Label();
            this.lblVehicleSeatsLabel = new System.Windows.Forms.Label();
            this.lblTest = new System.Windows.Forms.Label();
            this.lblVehicleHandling = new System.Windows.Forms.Label();
            this.lblVehicleArmorLabel = new System.Windows.Forms.Label();
            this.lblVehicleArmor = new System.Windows.Forms.Label();
            this.lblTestLabel = new System.Windows.Forms.Label();
            this.lblVehicleAccelLabel = new System.Windows.Forms.Label();
            this.lblVehicleAccel = new System.Windows.Forms.Label();
            this.lblVehicleBody = new System.Windows.Forms.Label();
            this.lblVehicleBodyLabel = new System.Windows.Forms.Label();
            this.lblVehicleSpeed = new System.Windows.Forms.Label();
            this.lblVehicleSpeedLabel = new System.Windows.Forms.Label();
            this.lblVehicleAvail = new System.Windows.Forms.Label();
            this.lblVehicleAvailLabel = new System.Windows.Forms.Label();
            this.lblVehicleSensor = new System.Windows.Forms.Label();
            this.lblVehicleSensorLabel = new System.Windows.Forms.Label();
            this.lblVehiclePilotLabel = new System.Windows.Forms.Label();
            this.lblVehiclePilot = new System.Windows.Forms.Label();
            this.tabBrowse = new System.Windows.Forms.TabPage();
            this.dgvVehicles = new System.Windows.Forms.DataGridView();
            this.bufferedTableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblSearchLabel = new System.Windows.Forms.Label();
            this.cboCategory = new Chummer.ElasticComboBox();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.tlpButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOKAdd = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lblCategory = new System.Windows.Forms.Label();
            this.dgvc_Guid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvc_Name = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dgvc_Accel = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dgvc_Armor = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dgvc_Body = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dgvc_Handling = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dgvc_Pilot = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dgvc_Sensor = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dgvc_Speed = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dgvc_Seats = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dgvc_Gear = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dgvc_Mods = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dgvc_Weapons = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvc_WeaponMounts = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Label_Avail = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.Label_Source = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.dgvc_Cost = new Chummer.DataGridViewTextBoxColumnTranslated();
            this.tabControl1.SuspendLayout();
            this.tabListView.SuspendLayout();
            this.tlpMain.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tlpRight.SuspendLayout();
            this.flpDiscount.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudUsedVehicleDiscount)).BeginInit();
            this.flpCheckBoxes.SuspendLayout();
            this.flpMarkup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).BeginInit();
            this.tabBrowse.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvVehicles)).BeginInit();
            this.bufferedTableLayoutPanel1.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.bufferedTableLayoutPanel1.SetColumnSpan(this.tabControl1, 4);
            this.tabControl1.Controls.Add(this.tabListView);
            this.tabControl1.Controls.Add(this.tabBrowse);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(3, 30);
            this.tabControl1.Name = "tabControl1";
            this.bufferedTableLayoutPanel1.SetRowSpan(this.tabControl1, 2);
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(779, 652);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.RefreshCurrentList);
            // 
            // tabListView
            // 
            this.tabListView.Controls.Add(this.tlpMain);
            this.tabListView.Location = new System.Drawing.Point(4, 22);
            this.tabListView.Name = "tabListView";
            this.tabListView.Padding = new System.Windows.Forms.Padding(3);
            this.tabListView.Size = new System.Drawing.Size(771, 626);
            this.tabListView.TabIndex = 0;
            this.tabListView.Tag = "Title_ListView";
            this.tabListView.Text = "List View";
            this.tabListView.UseVisualStyleBackColor = true;
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tlpMain.Controls.Add(this.chkHideOverAvailLimit, 1, 3);
            this.tlpMain.Controls.Add(this.chkShowOnlyAffordItems, 1, 4);
            this.tlpMain.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tlpMain.Controls.Add(this.tlpRight, 1, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(3, 3);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 6;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(765, 620);
            this.tlpMain.TabIndex = 67;
            // 
            // chkHideOverAvailLimit
            // 
            this.chkHideOverAvailLimit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkHideOverAvailLimit.AutoSize = true;
            this.chkHideOverAvailLimit.DefaultColorScheme = true;
            this.chkHideOverAvailLimit.Location = new System.Drawing.Point(309, 459);
            this.chkHideOverAvailLimit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkHideOverAvailLimit.Name = "chkHideOverAvailLimit";
            this.chkHideOverAvailLimit.Size = new System.Drawing.Size(175, 17);
            this.chkHideOverAvailLimit.TabIndex = 65;
            this.chkHideOverAvailLimit.Tag = "Checkbox_HideOverAvailLimit";
            this.chkHideOverAvailLimit.Text = "Hide Items Over Avail Limit ({0})";
            this.chkHideOverAvailLimit.UseVisualStyleBackColor = true;
            this.chkHideOverAvailLimit.CheckedChanged += new System.EventHandler(this.RefreshCurrentList);
            // 
            // chkShowOnlyAffordItems
            // 
            this.chkShowOnlyAffordItems.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkShowOnlyAffordItems.AutoSize = true;
            this.chkShowOnlyAffordItems.DefaultColorScheme = true;
            this.chkShowOnlyAffordItems.Location = new System.Drawing.Point(309, 484);
            this.chkShowOnlyAffordItems.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkShowOnlyAffordItems.Name = "chkShowOnlyAffordItems";
            this.chkShowOnlyAffordItems.Size = new System.Drawing.Size(164, 17);
            this.chkShowOnlyAffordItems.TabIndex = 67;
            this.chkShowOnlyAffordItems.Tag = "Checkbox_ShowOnlyAffordItems";
            this.chkShowOnlyAffordItems.Text = "Show Only Items I Can Afford";
            this.chkShowOnlyAffordItems.UseVisualStyleBackColor = true;
            this.chkShowOnlyAffordItems.CheckedChanged += new System.EventHandler(this.RefreshCurrentList);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.lstVehicle, 0, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tlpMain.SetRowSpan(this.tableLayoutPanel2, 6);
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(306, 620);
            this.tableLayoutPanel2.TabIndex = 69;
            // 
            // lstVehicle
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.lstVehicle, 2);
            this.lstVehicle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstVehicle.FormattingEnabled = true;
            this.lstVehicle.Location = new System.Drawing.Point(3, 3);
            this.lstVehicle.Name = "lstVehicle";
            this.lstVehicle.Size = new System.Drawing.Size(300, 614);
            this.lstVehicle.TabIndex = 32;
            this.lstVehicle.SelectedIndexChanged += new System.EventHandler(this.lstVehicle_SelectedIndexChanged);
            // 
            // tlpRight
            // 
            this.tlpRight.AutoSize = true;
            this.tlpRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.ColumnCount = 4;
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpRight.Controls.Add(this.lblSource, 1, 8);
            this.tlpRight.Controls.Add(this.lblVehicleHandlingLabel, 0, 0);
            this.tlpRight.Controls.Add(this.lblVehicleCost, 1, 5);
            this.tlpRight.Controls.Add(this.lblVehicleSeats, 3, 3);
            this.tlpRight.Controls.Add(this.lblSourceLabel, 0, 8);
            this.tlpRight.Controls.Add(this.flpDiscount, 0, 7);
            this.tlpRight.Controls.Add(this.lblVehicleCostLabel, 0, 5);
            this.tlpRight.Controls.Add(this.flpCheckBoxes, 0, 6);
            this.tlpRight.Controls.Add(this.flpMarkup, 3, 5);
            this.tlpRight.Controls.Add(this.lblMarkupLabel, 2, 5);
            this.tlpRight.Controls.Add(this.lblVehicleSeatsLabel, 2, 3);
            this.tlpRight.Controls.Add(this.lblTest, 3, 4);
            this.tlpRight.Controls.Add(this.lblVehicleHandling, 1, 0);
            this.tlpRight.Controls.Add(this.lblVehicleArmorLabel, 2, 2);
            this.tlpRight.Controls.Add(this.lblVehicleArmor, 3, 2);
            this.tlpRight.Controls.Add(this.lblTestLabel, 2, 4);
            this.tlpRight.Controls.Add(this.lblVehicleAccelLabel, 2, 0);
            this.tlpRight.Controls.Add(this.lblVehicleAccel, 3, 0);
            this.tlpRight.Controls.Add(this.lblVehicleBody, 1, 2);
            this.tlpRight.Controls.Add(this.lblVehicleBodyLabel, 0, 2);
            this.tlpRight.Controls.Add(this.lblVehicleSpeed, 1, 1);
            this.tlpRight.Controls.Add(this.lblVehicleSpeedLabel, 0, 1);
            this.tlpRight.Controls.Add(this.lblVehicleAvail, 1, 4);
            this.tlpRight.Controls.Add(this.lblVehicleAvailLabel, 0, 4);
            this.tlpRight.Controls.Add(this.lblVehicleSensor, 1, 3);
            this.tlpRight.Controls.Add(this.lblVehicleSensorLabel, 0, 3);
            this.tlpRight.Controls.Add(this.lblVehiclePilotLabel, 2, 1);
            this.tlpRight.Controls.Add(this.lblVehiclePilot, 3, 1);
            this.tlpRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRight.Location = new System.Drawing.Point(306, 0);
            this.tlpRight.Margin = new System.Windows.Forms.Padding(0);
            this.tlpRight.Name = "tlpRight";
            this.tlpRight.RowCount = 9;
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpRight.Size = new System.Drawing.Size(459, 227);
            this.tlpRight.TabIndex = 74;
            // 
            // lblSource
            // 
            this.lblSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSource.AutoSize = true;
            this.tlpRight.SetColumnSpan(this.lblSource, 3);
            this.lblSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSource.Location = new System.Drawing.Point(61, 208);
            this.lblSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 31;
            this.lblSource.Text = "[Source]";
            // 
            // lblVehicleHandlingLabel
            // 
            this.lblVehicleHandlingLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblVehicleHandlingLabel.AutoSize = true;
            this.lblVehicleHandlingLabel.Location = new System.Drawing.Point(3, 6);
            this.lblVehicleHandlingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleHandlingLabel.Name = "lblVehicleHandlingLabel";
            this.lblVehicleHandlingLabel.Size = new System.Drawing.Size(52, 13);
            this.lblVehicleHandlingLabel.TabIndex = 2;
            this.lblVehicleHandlingLabel.Tag = "Label_Handling";
            this.lblVehicleHandlingLabel.Text = "Handling:";
            // 
            // lblVehicleCost
            // 
            this.lblVehicleCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblVehicleCost.AutoSize = true;
            this.lblVehicleCost.Location = new System.Drawing.Point(61, 131);
            this.lblVehicleCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleCost.Name = "lblVehicleCost";
            this.lblVehicleCost.Size = new System.Drawing.Size(34, 13);
            this.lblVehicleCost.TabIndex = 21;
            this.lblVehicleCost.Text = "[Cost]";
            // 
            // lblVehicleSeats
            // 
            this.lblVehicleSeats.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblVehicleSeats.AutoSize = true;
            this.lblVehicleSeats.Location = new System.Drawing.Point(287, 81);
            this.lblVehicleSeats.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSeats.Name = "lblVehicleSeats";
            this.lblVehicleSeats.Size = new System.Drawing.Size(40, 13);
            this.lblVehicleSeats.TabIndex = 40;
            this.lblVehicleSeats.Text = "[Seats]";
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(11, 208);
            this.lblSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 30;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            // 
            // flpDiscount
            // 
            this.flpDiscount.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpDiscount.AutoSize = true;
            this.flpDiscount.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.SetColumnSpan(this.flpDiscount, 4);
            this.flpDiscount.Controls.Add(this.chkUsedVehicle);
            this.flpDiscount.Controls.Add(this.lblUsedVehicleDiscountLabel);
            this.flpDiscount.Controls.Add(this.nudUsedVehicleDiscount);
            this.flpDiscount.Controls.Add(this.lblUsedVehicleDiscountPercentLabel);
            this.flpDiscount.Location = new System.Drawing.Point(0, 176);
            this.flpDiscount.Margin = new System.Windows.Forms.Padding(0);
            this.flpDiscount.Name = "flpDiscount";
            this.flpDiscount.Size = new System.Drawing.Size(230, 26);
            this.flpDiscount.TabIndex = 70;
            // 
            // chkUsedVehicle
            // 
            this.chkUsedVehicle.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkUsedVehicle.AutoSize = true;
            this.chkUsedVehicle.DefaultColorScheme = true;
            this.chkUsedVehicle.Location = new System.Drawing.Point(3, 4);
            this.chkUsedVehicle.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkUsedVehicle.Name = "chkUsedVehicle";
            this.chkUsedVehicle.Size = new System.Drawing.Size(89, 17);
            this.chkUsedVehicle.TabIndex = 22;
            this.chkUsedVehicle.Tag = "Checkbox_SelectVehicle_UsedVehicle";
            this.chkUsedVehicle.Text = "Used Vehicle";
            this.chkUsedVehicle.UseVisualStyleBackColor = true;
            this.chkUsedVehicle.Visible = false;
            this.chkUsedVehicle.CheckedChanged += new System.EventHandler(this.ProcessVehicleCostsChanged);
            // 
            // lblUsedVehicleDiscountLabel
            // 
            this.lblUsedVehicleDiscountLabel.AutoSize = true;
            this.lblUsedVehicleDiscountLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblUsedVehicleDiscountLabel.Location = new System.Drawing.Point(98, 6);
            this.lblUsedVehicleDiscountLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblUsedVehicleDiscountLabel.Name = "lblUsedVehicleDiscountLabel";
            this.lblUsedVehicleDiscountLabel.Size = new System.Drawing.Size(52, 14);
            this.lblUsedVehicleDiscountLabel.TabIndex = 23;
            this.lblUsedVehicleDiscountLabel.Tag = "Label_SelectVehicle_Discount";
            this.lblUsedVehicleDiscountLabel.Text = "Discount:";
            this.lblUsedVehicleDiscountLabel.Visible = false;
            // 
            // nudUsedVehicleDiscount
            // 
            this.nudUsedVehicleDiscount.AutoSize = true;
            this.nudUsedVehicleDiscount.DecimalPlaces = 2;
            this.nudUsedVehicleDiscount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudUsedVehicleDiscount.Location = new System.Drawing.Point(156, 3);
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
            this.nudUsedVehicleDiscount.Size = new System.Drawing.Size(50, 20);
            this.nudUsedVehicleDiscount.TabIndex = 24;
            this.nudUsedVehicleDiscount.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nudUsedVehicleDiscount.Visible = false;
            this.nudUsedVehicleDiscount.ValueChanged += new System.EventHandler(this.ProcessVehicleCostsChanged);
            // 
            // lblUsedVehicleDiscountPercentLabel
            // 
            this.lblUsedVehicleDiscountPercentLabel.AutoSize = true;
            this.lblUsedVehicleDiscountPercentLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblUsedVehicleDiscountPercentLabel.Location = new System.Drawing.Point(212, 6);
            this.lblUsedVehicleDiscountPercentLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblUsedVehicleDiscountPercentLabel.Name = "lblUsedVehicleDiscountPercentLabel";
            this.lblUsedVehicleDiscountPercentLabel.Size = new System.Drawing.Size(15, 14);
            this.lblUsedVehicleDiscountPercentLabel.TabIndex = 25;
            this.lblUsedVehicleDiscountPercentLabel.Text = "%";
            this.lblUsedVehicleDiscountPercentLabel.Visible = false;
            // 
            // lblVehicleCostLabel
            // 
            this.lblVehicleCostLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblVehicleCostLabel.AutoSize = true;
            this.lblVehicleCostLabel.Location = new System.Drawing.Point(24, 131);
            this.lblVehicleCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleCostLabel.Name = "lblVehicleCostLabel";
            this.lblVehicleCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblVehicleCostLabel.TabIndex = 20;
            this.lblVehicleCostLabel.Tag = "Label_Cost";
            this.lblVehicleCostLabel.Text = "Cost:";
            // 
            // flpCheckBoxes
            // 
            this.flpCheckBoxes.AutoSize = true;
            this.tlpRight.SetColumnSpan(this.flpCheckBoxes, 4);
            this.flpCheckBoxes.Controls.Add(this.chkFreeItem);
            this.flpCheckBoxes.Controls.Add(this.chkBlackMarketDiscount);
            this.flpCheckBoxes.Location = new System.Drawing.Point(0, 151);
            this.flpCheckBoxes.Margin = new System.Windows.Forms.Padding(0);
            this.flpCheckBoxes.Name = "flpCheckBoxes";
            this.flpCheckBoxes.Size = new System.Drawing.Size(225, 25);
            this.flpCheckBoxes.TabIndex = 72;
            // 
            // chkFreeItem
            // 
            this.chkFreeItem.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkFreeItem.AutoSize = true;
            this.chkFreeItem.DefaultColorScheme = true;
            this.chkFreeItem.Location = new System.Drawing.Point(3, 4);
            this.chkFreeItem.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkFreeItem.Name = "chkFreeItem";
            this.chkFreeItem.Size = new System.Drawing.Size(50, 17);
            this.chkFreeItem.TabIndex = 26;
            this.chkFreeItem.Tag = "Checkbox_Free";
            this.chkFreeItem.Text = "Free!";
            this.chkFreeItem.UseVisualStyleBackColor = true;
            this.chkFreeItem.CheckedChanged += new System.EventHandler(this.chkFreeItem_CheckedChanged);
            // 
            // chkBlackMarketDiscount
            // 
            this.chkBlackMarketDiscount.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkBlackMarketDiscount.AutoSize = true;
            this.chkBlackMarketDiscount.DefaultColorScheme = true;
            this.chkBlackMarketDiscount.Location = new System.Drawing.Point(59, 4);
            this.chkBlackMarketDiscount.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkBlackMarketDiscount.Name = "chkBlackMarketDiscount";
            this.chkBlackMarketDiscount.Size = new System.Drawing.Size(163, 17);
            this.chkBlackMarketDiscount.TabIndex = 38;
            this.chkBlackMarketDiscount.Tag = "Checkbox_BlackMarketDiscount";
            this.chkBlackMarketDiscount.Text = "Black Market Discount (10%)";
            this.chkBlackMarketDiscount.UseVisualStyleBackColor = true;
            this.chkBlackMarketDiscount.Visible = false;
            this.chkBlackMarketDiscount.CheckedChanged += new System.EventHandler(this.ProcessVehicleCostsChanged);
            // 
            // flpMarkup
            // 
            this.flpMarkup.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpMarkup.AutoSize = true;
            this.flpMarkup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpMarkup.Controls.Add(this.nudMarkup);
            this.flpMarkup.Controls.Add(this.lblMarkupPercentLabel);
            this.flpMarkup.Location = new System.Drawing.Point(284, 125);
            this.flpMarkup.Margin = new System.Windows.Forms.Padding(0);
            this.flpMarkup.Name = "flpMarkup";
            this.flpMarkup.Size = new System.Drawing.Size(83, 26);
            this.flpMarkup.TabIndex = 71;
            this.flpMarkup.WrapContents = false;
            // 
            // nudMarkup
            // 
            this.nudMarkup.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudMarkup.AutoSize = true;
            this.nudMarkup.DecimalPlaces = 2;
            this.nudMarkup.Location = new System.Drawing.Point(3, 3);
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
            this.nudMarkup.TabIndex = 28;
            this.nudMarkup.ValueChanged += new System.EventHandler(this.ProcessVehicleCostsChanged);
            // 
            // lblMarkupPercentLabel
            // 
            this.lblMarkupPercentLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMarkupPercentLabel.AutoSize = true;
            this.lblMarkupPercentLabel.Location = new System.Drawing.Point(65, 6);
            this.lblMarkupPercentLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMarkupPercentLabel.Name = "lblMarkupPercentLabel";
            this.lblMarkupPercentLabel.Size = new System.Drawing.Size(15, 13);
            this.lblMarkupPercentLabel.TabIndex = 29;
            this.lblMarkupPercentLabel.Text = "%";
            // 
            // lblMarkupLabel
            // 
            this.lblMarkupLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMarkupLabel.AutoSize = true;
            this.lblMarkupLabel.Location = new System.Drawing.Point(235, 131);
            this.lblMarkupLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMarkupLabel.Name = "lblMarkupLabel";
            this.lblMarkupLabel.Size = new System.Drawing.Size(46, 13);
            this.lblMarkupLabel.TabIndex = 27;
            this.lblMarkupLabel.Tag = "Label_SelectGear_Markup";
            this.lblMarkupLabel.Text = "Markup:";
            // 
            // lblVehicleSeatsLabel
            // 
            this.lblVehicleSeatsLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblVehicleSeatsLabel.AutoSize = true;
            this.lblVehicleSeatsLabel.Location = new System.Drawing.Point(244, 81);
            this.lblVehicleSeatsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSeatsLabel.Name = "lblVehicleSeatsLabel";
            this.lblVehicleSeatsLabel.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleSeatsLabel.TabIndex = 39;
            this.lblVehicleSeatsLabel.Tag = "Label_Seats";
            this.lblVehicleSeatsLabel.Text = "Seats:";
            // 
            // lblTest
            // 
            this.lblTest.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblTest.AutoSize = true;
            this.lblTest.Location = new System.Drawing.Point(287, 106);
            this.lblTest.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTest.Name = "lblTest";
            this.lblTest.Size = new System.Drawing.Size(19, 13);
            this.lblTest.TabIndex = 19;
            this.lblTest.Text = "[0]";
            // 
            // lblVehicleHandling
            // 
            this.lblVehicleHandling.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblVehicleHandling.AutoSize = true;
            this.lblVehicleHandling.Location = new System.Drawing.Point(61, 6);
            this.lblVehicleHandling.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleHandling.Name = "lblVehicleHandling";
            this.lblVehicleHandling.Size = new System.Drawing.Size(55, 13);
            this.lblVehicleHandling.TabIndex = 3;
            this.lblVehicleHandling.Text = "[Handling]";
            // 
            // lblVehicleArmorLabel
            // 
            this.lblVehicleArmorLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblVehicleArmorLabel.AutoSize = true;
            this.lblVehicleArmorLabel.Location = new System.Drawing.Point(244, 56);
            this.lblVehicleArmorLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleArmorLabel.Name = "lblVehicleArmorLabel";
            this.lblVehicleArmorLabel.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleArmorLabel.TabIndex = 12;
            this.lblVehicleArmorLabel.Tag = "Label_Armor";
            this.lblVehicleArmorLabel.Text = "Armor:";
            // 
            // lblVehicleArmor
            // 
            this.lblVehicleArmor.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblVehicleArmor.AutoSize = true;
            this.lblVehicleArmor.Location = new System.Drawing.Point(287, 56);
            this.lblVehicleArmor.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleArmor.Name = "lblVehicleArmor";
            this.lblVehicleArmor.Size = new System.Drawing.Size(40, 13);
            this.lblVehicleArmor.TabIndex = 13;
            this.lblVehicleArmor.Text = "[Armor]";
            // 
            // lblTestLabel
            // 
            this.lblTestLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblTestLabel.AutoSize = true;
            this.lblTestLabel.Location = new System.Drawing.Point(250, 106);
            this.lblTestLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTestLabel.Name = "lblTestLabel";
            this.lblTestLabel.Size = new System.Drawing.Size(31, 13);
            this.lblTestLabel.TabIndex = 18;
            this.lblTestLabel.Tag = "Label_Test";
            this.lblTestLabel.Text = "Test:";
            // 
            // lblVehicleAccelLabel
            // 
            this.lblVehicleAccelLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblVehicleAccelLabel.AutoSize = true;
            this.lblVehicleAccelLabel.Location = new System.Drawing.Point(244, 6);
            this.lblVehicleAccelLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleAccelLabel.Name = "lblVehicleAccelLabel";
            this.lblVehicleAccelLabel.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleAccelLabel.TabIndex = 4;
            this.lblVehicleAccelLabel.Tag = "Label_Accel";
            this.lblVehicleAccelLabel.Text = "Accel:";
            // 
            // lblVehicleAccel
            // 
            this.lblVehicleAccel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblVehicleAccel.AutoSize = true;
            this.lblVehicleAccel.Location = new System.Drawing.Point(287, 6);
            this.lblVehicleAccel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleAccel.Name = "lblVehicleAccel";
            this.lblVehicleAccel.Size = new System.Drawing.Size(40, 13);
            this.lblVehicleAccel.TabIndex = 5;
            this.lblVehicleAccel.Text = "[Accel]";
            // 
            // lblVehicleBody
            // 
            this.lblVehicleBody.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblVehicleBody.AutoSize = true;
            this.lblVehicleBody.Location = new System.Drawing.Point(61, 56);
            this.lblVehicleBody.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleBody.Name = "lblVehicleBody";
            this.lblVehicleBody.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleBody.TabIndex = 11;
            this.lblVehicleBody.Text = "[Body]";
            // 
            // lblVehicleBodyLabel
            // 
            this.lblVehicleBodyLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblVehicleBodyLabel.AutoSize = true;
            this.lblVehicleBodyLabel.Location = new System.Drawing.Point(21, 56);
            this.lblVehicleBodyLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleBodyLabel.Name = "lblVehicleBodyLabel";
            this.lblVehicleBodyLabel.Size = new System.Drawing.Size(34, 13);
            this.lblVehicleBodyLabel.TabIndex = 10;
            this.lblVehicleBodyLabel.Tag = "Label_Body";
            this.lblVehicleBodyLabel.Text = "Body:";
            // 
            // lblVehicleSpeed
            // 
            this.lblVehicleSpeed.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblVehicleSpeed.AutoSize = true;
            this.lblVehicleSpeed.Location = new System.Drawing.Point(61, 31);
            this.lblVehicleSpeed.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSpeed.Name = "lblVehicleSpeed";
            this.lblVehicleSpeed.Size = new System.Drawing.Size(44, 13);
            this.lblVehicleSpeed.TabIndex = 7;
            this.lblVehicleSpeed.Text = "[Speed]";
            // 
            // lblVehicleSpeedLabel
            // 
            this.lblVehicleSpeedLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblVehicleSpeedLabel.AutoSize = true;
            this.lblVehicleSpeedLabel.Location = new System.Drawing.Point(14, 31);
            this.lblVehicleSpeedLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSpeedLabel.Name = "lblVehicleSpeedLabel";
            this.lblVehicleSpeedLabel.Size = new System.Drawing.Size(41, 13);
            this.lblVehicleSpeedLabel.TabIndex = 6;
            this.lblVehicleSpeedLabel.Tag = "Label_Speed";
            this.lblVehicleSpeedLabel.Text = "Speed:";
            // 
            // lblVehicleAvail
            // 
            this.lblVehicleAvail.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblVehicleAvail.AutoSize = true;
            this.lblVehicleAvail.Location = new System.Drawing.Point(61, 106);
            this.lblVehicleAvail.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleAvail.Name = "lblVehicleAvail";
            this.lblVehicleAvail.Size = new System.Drawing.Size(36, 13);
            this.lblVehicleAvail.TabIndex = 17;
            this.lblVehicleAvail.Text = "[Avail]";
            // 
            // lblVehicleAvailLabel
            // 
            this.lblVehicleAvailLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblVehicleAvailLabel.AutoSize = true;
            this.lblVehicleAvailLabel.Location = new System.Drawing.Point(22, 106);
            this.lblVehicleAvailLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleAvailLabel.Name = "lblVehicleAvailLabel";
            this.lblVehicleAvailLabel.Size = new System.Drawing.Size(33, 13);
            this.lblVehicleAvailLabel.TabIndex = 16;
            this.lblVehicleAvailLabel.Tag = "Label_Avail";
            this.lblVehicleAvailLabel.Text = "Avail:";
            // 
            // lblVehicleSensor
            // 
            this.lblVehicleSensor.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblVehicleSensor.AutoSize = true;
            this.lblVehicleSensor.Location = new System.Drawing.Point(61, 81);
            this.lblVehicleSensor.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSensor.Name = "lblVehicleSensor";
            this.lblVehicleSensor.Size = new System.Drawing.Size(46, 13);
            this.lblVehicleSensor.TabIndex = 15;
            this.lblVehicleSensor.Text = "[Sensor]";
            // 
            // lblVehicleSensorLabel
            // 
            this.lblVehicleSensorLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblVehicleSensorLabel.AutoSize = true;
            this.lblVehicleSensorLabel.Location = new System.Drawing.Point(12, 81);
            this.lblVehicleSensorLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSensorLabel.Name = "lblVehicleSensorLabel";
            this.lblVehicleSensorLabel.Size = new System.Drawing.Size(43, 13);
            this.lblVehicleSensorLabel.TabIndex = 14;
            this.lblVehicleSensorLabel.Tag = "Label_Sensor";
            this.lblVehicleSensorLabel.Text = "Sensor:";
            // 
            // lblVehiclePilotLabel
            // 
            this.lblVehiclePilotLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblVehiclePilotLabel.AutoSize = true;
            this.lblVehiclePilotLabel.Location = new System.Drawing.Point(251, 31);
            this.lblVehiclePilotLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehiclePilotLabel.Name = "lblVehiclePilotLabel";
            this.lblVehiclePilotLabel.Size = new System.Drawing.Size(30, 13);
            this.lblVehiclePilotLabel.TabIndex = 8;
            this.lblVehiclePilotLabel.Tag = "Label_Pilot";
            this.lblVehiclePilotLabel.Text = "Pilot:";
            // 
            // lblVehiclePilot
            // 
            this.lblVehiclePilot.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblVehiclePilot.AutoSize = true;
            this.lblVehiclePilot.Location = new System.Drawing.Point(287, 31);
            this.lblVehiclePilot.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehiclePilot.Name = "lblVehiclePilot";
            this.lblVehiclePilot.Size = new System.Drawing.Size(33, 13);
            this.lblVehiclePilot.TabIndex = 9;
            this.lblVehiclePilot.Text = "[Pilot]";
            // 
            // tabBrowse
            // 
            this.tabBrowse.Controls.Add(this.dgvVehicles);
            this.tabBrowse.Location = new System.Drawing.Point(4, 22);
            this.tabBrowse.Name = "tabBrowse";
            this.tabBrowse.Padding = new System.Windows.Forms.Padding(3);
            this.tabBrowse.Size = new System.Drawing.Size(771, 626);
            this.tabBrowse.TabIndex = 1;
            this.tabBrowse.Tag = "Title_Browse";
            this.tabBrowse.Text = "Browse";
            this.tabBrowse.UseVisualStyleBackColor = true;
            // 
            // dgvVehicles
            // 
            this.dgvVehicles.AllowUserToAddRows = false;
            this.dgvVehicles.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.dgvVehicles.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvVehicles.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvVehicles.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvVehicles.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvVehicles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvVehicles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgvc_Guid,
            this.dgvc_Name,
            this.dgvc_Accel,
            this.dgvc_Armor,
            this.dgvc_Body,
            this.dgvc_Handling,
            this.dgvc_Pilot,
            this.dgvc_Sensor,
            this.dgvc_Speed,
            this.dgvc_Seats,
            this.dgvc_Gear,
            this.dgvc_Mods,
            this.dgvc_Weapons,
            this.dgvc_WeaponMounts,
            this.Label_Avail,
            this.Label_Source,
            this.dgvc_Cost});
            this.dgvVehicles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvVehicles.Location = new System.Drawing.Point(3, 3);
            this.dgvVehicles.MultiSelect = false;
            this.dgvVehicles.Name = "dgvVehicles";
            this.dgvVehicles.ReadOnly = true;
            this.dgvVehicles.RowHeadersVisible = false;
            this.dgvVehicles.RowTemplate.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            this.dgvVehicles.RowTemplate.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvVehicles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvVehicles.Size = new System.Drawing.Size(765, 620);
            this.dgvVehicles.TabIndex = 37;
            // 
            // bufferedTableLayoutPanel1
            // 
            this.bufferedTableLayoutPanel1.ColumnCount = 4;
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblSearchLabel, 2, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.cboCategory, 1, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.txtSearch, 3, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.tlpButtons, 3, 3);
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblCategory, 0, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.tabControl1, 0, 1);
            this.bufferedTableLayoutPanel1.Location = new System.Drawing.Point(9, 9);
            this.bufferedTableLayoutPanel1.Name = "bufferedTableLayoutPanel1";
            this.bufferedTableLayoutPanel1.RowCount = 4;
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel1.Size = new System.Drawing.Size(785, 714);
            this.bufferedTableLayoutPanel1.TabIndex = 1;
            // 
            // lblSearchLabel
            // 
            this.lblSearchLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSearchLabel.AutoSize = true;
            this.lblSearchLabel.Location = new System.Drawing.Point(309, 7);
            this.lblSearchLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSearchLabel.Name = "lblSearchLabel";
            this.lblSearchLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSearchLabel.TabIndex = 0;
            this.lblSearchLabel.Tag = "Label_Search";
            this.lblSearchLabel.Text = "&Search:";
            // 
            // cboCategory
            // 
            this.cboCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategory.FormattingEnabled = true;
            this.cboCategory.Location = new System.Drawing.Point(61, 3);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(242, 21);
            this.cboCategory.TabIndex = 34;
            this.cboCategory.TooltipText = "";
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.RefreshCurrentList);
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(359, 3);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(423, 20);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
            this.txtSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyUp);
            // 
            // tlpButtons
            // 
            this.tlpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpButtons.AutoSize = true;
            this.tlpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpButtons.ColumnCount = 3;
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpButtons.Controls.Add(this.cmdCancel, 0, 0);
            this.tlpButtons.Controls.Add(this.cmdOKAdd, 1, 0);
            this.tlpButtons.Controls.Add(this.cmdOK, 2, 0);
            this.tlpButtons.Location = new System.Drawing.Point(551, 685);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButtons.Size = new System.Drawing.Size(234, 29);
            this.tlpButtons.TabIndex = 73;
            // 
            // cmdCancel
            // 
            this.cmdCancel.AutoSize = true;
            this.cmdCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdCancel.Location = new System.Drawing.Point(3, 3);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(72, 23);
            this.cmdCancel.TabIndex = 37;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOKAdd
            // 
            this.cmdOKAdd.AutoSize = true;
            this.cmdOKAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOKAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOKAdd.Location = new System.Drawing.Point(81, 3);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(72, 23);
            this.cmdOKAdd.TabIndex = 36;
            this.cmdOKAdd.Tag = "String_AddMore";
            this.cmdOKAdd.Text = "&Add && More";
            this.cmdOKAdd.UseVisualStyleBackColor = true;
            this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.AutoSize = true;
            this.cmdOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOK.Location = new System.Drawing.Point(159, 3);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(72, 23);
            this.cmdOK.TabIndex = 35;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lblCategory
            // 
            this.lblCategory.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(3, 7);
            this.lblCategory.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(52, 13);
            this.lblCategory.TabIndex = 33;
            this.lblCategory.Tag = "Label_Category";
            this.lblCategory.Text = "Category:";
            // 
            // dgvc_Guid
            // 
            this.dgvc_Guid.DataPropertyName = "VehicleGuid";
            this.dgvc_Guid.HeaderText = "dgvc_Guid";
            this.dgvc_Guid.Name = "dgvc_Guid";
            this.dgvc_Guid.ReadOnly = true;
            this.dgvc_Guid.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Guid.Visible = false;
            // 
            // dgvc_Name
            // 
            this.dgvc_Name.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvc_Name.DataPropertyName = "VehicleName";
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Name.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvc_Name.HeaderText = "Name";
            this.dgvc_Name.Name = "dgvc_Name";
            this.dgvc_Name.ReadOnly = true;
            this.dgvc_Name.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Name.TranslationTag = null;
            this.dgvc_Name.Width = 60;
            // 
            // dgvc_Accel
            // 
            this.dgvc_Accel.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvc_Accel.DataPropertyName = "Accel";
            this.dgvc_Accel.FillWeight = 30F;
            this.dgvc_Accel.HeaderText = "Acceleration";
            this.dgvc_Accel.Name = "dgvc_Accel";
            this.dgvc_Accel.ReadOnly = true;
            this.dgvc_Accel.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Accel.TranslationTag = null;
            this.dgvc_Accel.Width = 91;
            // 
            // dgvc_Armor
            // 
            this.dgvc_Armor.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvc_Armor.DataPropertyName = "Armor";
            this.dgvc_Armor.FillWeight = 50F;
            this.dgvc_Armor.HeaderText = "Armor";
            this.dgvc_Armor.Name = "dgvc_Armor";
            this.dgvc_Armor.ReadOnly = true;
            this.dgvc_Armor.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Armor.TranslationTag = null;
            this.dgvc_Armor.Width = 59;
            // 
            // dgvc_Body
            // 
            this.dgvc_Body.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvc_Body.DataPropertyName = "Body";
            this.dgvc_Body.FillWeight = 50F;
            this.dgvc_Body.HeaderText = "Body";
            this.dgvc_Body.Name = "dgvc_Body";
            this.dgvc_Body.ReadOnly = true;
            this.dgvc_Body.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Body.TranslationTag = null;
            this.dgvc_Body.Width = 56;
            // 
            // dgvc_Handling
            // 
            this.dgvc_Handling.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvc_Handling.DataPropertyName = "Handling";
            this.dgvc_Handling.FillWeight = 30F;
            this.dgvc_Handling.HeaderText = "Handling";
            this.dgvc_Handling.Name = "dgvc_Handling";
            this.dgvc_Handling.ReadOnly = true;
            this.dgvc_Handling.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Handling.TranslationTag = null;
            this.dgvc_Handling.Width = 74;
            // 
            // dgvc_Pilot
            // 
            this.dgvc_Pilot.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvc_Pilot.DataPropertyName = "Pilot";
            this.dgvc_Pilot.FillWeight = 30F;
            this.dgvc_Pilot.HeaderText = "Pilot";
            this.dgvc_Pilot.Name = "dgvc_Pilot";
            this.dgvc_Pilot.ReadOnly = true;
            this.dgvc_Pilot.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Pilot.TranslationTag = null;
            this.dgvc_Pilot.Width = 52;
            // 
            // dgvc_Sensor
            // 
            this.dgvc_Sensor.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvc_Sensor.DataPropertyName = "Sensor";
            this.dgvc_Sensor.FillWeight = 60F;
            this.dgvc_Sensor.HeaderText = "Sensor";
            this.dgvc_Sensor.Name = "dgvc_Sensor";
            this.dgvc_Sensor.ReadOnly = true;
            this.dgvc_Sensor.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Sensor.TranslationTag = null;
            this.dgvc_Sensor.Width = 65;
            // 
            // dgvc_Speed
            // 
            this.dgvc_Speed.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvc_Speed.DataPropertyName = "Speed";
            this.dgvc_Speed.FillWeight = 60F;
            this.dgvc_Speed.HeaderText = "Speed";
            this.dgvc_Speed.Name = "dgvc_Speed";
            this.dgvc_Speed.ReadOnly = true;
            this.dgvc_Speed.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Speed.TranslationTag = null;
            this.dgvc_Speed.Width = 63;
            // 
            // dgvc_Seats
            // 
            this.dgvc_Seats.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvc_Seats.DataPropertyName = "Seats";
            this.dgvc_Seats.FillWeight = 40F;
            this.dgvc_Seats.HeaderText = "Seats";
            this.dgvc_Seats.Name = "dgvc_Seats";
            this.dgvc_Seats.ReadOnly = true;
            this.dgvc_Seats.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Seats.TranslationTag = null;
            this.dgvc_Seats.Width = 59;
            // 
            // dgvc_Gear
            // 
            this.dgvc_Gear.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvc_Gear.DataPropertyName = "Gear";
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Gear.DefaultCellStyle = dataGridViewCellStyle4;
            this.dgvc_Gear.HeaderText = "Gear";
            this.dgvc_Gear.Name = "dgvc_Gear";
            this.dgvc_Gear.ReadOnly = true;
            this.dgvc_Gear.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Gear.TranslationTag = null;
            this.dgvc_Gear.Width = 55;
            // 
            // dgvc_Mods
            // 
            this.dgvc_Mods.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvc_Mods.DataPropertyName = "Mods";
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Mods.DefaultCellStyle = dataGridViewCellStyle5;
            this.dgvc_Mods.HeaderText = "Mods";
            this.dgvc_Mods.Name = "dgvc_Mods";
            this.dgvc_Mods.ReadOnly = true;
            this.dgvc_Mods.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Mods.TranslationTag = null;
            this.dgvc_Mods.Width = 58;
            // 
            // dgvc_Weapons
            // 
            this.dgvc_Weapons.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvc_Weapons.DataPropertyName = "Weapons";
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Weapons.DefaultCellStyle = dataGridViewCellStyle6;
            this.dgvc_Weapons.HeaderText = "Weapons";
            this.dgvc_Weapons.Name = "dgvc_Weapons";
            this.dgvc_Weapons.ReadOnly = true;
            this.dgvc_Weapons.Width = 78;
            // 
            // dgvc_WeaponMounts
            // 
            this.dgvc_WeaponMounts.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvc_WeaponMounts.DataPropertyName = "WeaponMounts";
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_WeaponMounts.DefaultCellStyle = dataGridViewCellStyle7;
            this.dgvc_WeaponMounts.HeaderText = "Weapon Mounts";
            this.dgvc_WeaponMounts.Name = "dgvc_WeaponMounts";
            this.dgvc_WeaponMounts.ReadOnly = true;
            this.dgvc_WeaponMounts.Width = 102;
            // 
            // Label_Avail
            // 
            this.Label_Avail.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Label_Avail.DataPropertyName = "Avail";
            this.Label_Avail.FillWeight = 30F;
            this.Label_Avail.HeaderText = "Avail";
            this.Label_Avail.Name = "Label_Avail";
            this.Label_Avail.ReadOnly = true;
            this.Label_Avail.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Label_Avail.TranslationTag = null;
            this.Label_Avail.Width = 55;
            // 
            // Label_Source
            // 
            this.Label_Source.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Label_Source.DataPropertyName = "Source";
            this.Label_Source.HeaderText = "Source";
            this.Label_Source.Name = "Label_Source";
            this.Label_Source.ReadOnly = true;
            this.Label_Source.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Label_Source.TranslationTag = null;
            this.Label_Source.Width = 66;
            // 
            // dgvc_Cost
            // 
            this.dgvc_Cost.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvc_Cost.DataPropertyName = "Cost";
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopRight;
            dataGridViewCellStyle8.Format = "#,0.##¥";
            dataGridViewCellStyle8.NullValue = null;
            this.dgvc_Cost.DefaultCellStyle = dataGridViewCellStyle8;
            this.dgvc_Cost.FillWeight = 60F;
            this.dgvc_Cost.HeaderText = "Cost";
            this.dgvc_Cost.Name = "dgvc_Cost";
            this.dgvc_Cost.ReadOnly = true;
            this.dgvc_Cost.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvc_Cost.TranslationTag = null;
            this.dgvc_Cost.Width = 53;
            // 
            // SelectVehicle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(803, 732);
            this.Controls.Add(this.bufferedTableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectVehicle";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Tag = "Title_SelectVehicle";
            this.Text = "Select a Vehicle";
            this.Load += new System.EventHandler(this.SelectVehicle_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabListView.ResumeLayout(false);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tlpRight.ResumeLayout(false);
            this.tlpRight.PerformLayout();
            this.flpDiscount.ResumeLayout(false);
            this.flpDiscount.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudUsedVehicleDiscount)).EndInit();
            this.flpCheckBoxes.ResumeLayout(false);
            this.flpCheckBoxes.PerformLayout();
            this.flpMarkup.ResumeLayout(false);
            this.flpMarkup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).EndInit();
            this.tabBrowse.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvVehicles)).EndInit();
            this.bufferedTableLayoutPanel1.ResumeLayout(false);
            this.bufferedTableLayoutPanel1.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private BufferedTableLayoutPanel bufferedTableLayoutPanel1;
        private System.Windows.Forms.Label lblSearchLabel;
        private ElasticComboBox cboCategory;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.TabPage tabListView;
        private BufferedTableLayoutPanel tlpMain;
        private ColorableCheckBox chkHideOverAvailLimit;
        private ColorableCheckBox chkShowOnlyAffordItems;
        private BufferedTableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.ListBox lstVehicle;
        private BufferedTableLayoutPanel tlpButtons;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOKAdd;
        private System.Windows.Forms.Button cmdOK;
        private BufferedTableLayoutPanel tlpRight;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblVehicleHandlingLabel;
        private System.Windows.Forms.Label lblVehicleCost;
        private System.Windows.Forms.Label lblVehicleSeats;
        private System.Windows.Forms.Label lblSourceLabel;
        private System.Windows.Forms.FlowLayoutPanel flpDiscount;
        private ColorableCheckBox chkUsedVehicle;
        private System.Windows.Forms.Label lblUsedVehicleDiscountLabel;
        private NumericUpDownEx nudUsedVehicleDiscount;
        private System.Windows.Forms.Label lblUsedVehicleDiscountPercentLabel;
        private System.Windows.Forms.Label lblVehicleCostLabel;
        private System.Windows.Forms.FlowLayoutPanel flpCheckBoxes;
        private ColorableCheckBox chkFreeItem;
        private ColorableCheckBox chkBlackMarketDiscount;
        private System.Windows.Forms.FlowLayoutPanel flpMarkup;
        private NumericUpDownEx nudMarkup;
        private System.Windows.Forms.Label lblMarkupPercentLabel;
        private System.Windows.Forms.Label lblMarkupLabel;
        private System.Windows.Forms.Label lblVehicleSeatsLabel;
        private System.Windows.Forms.Label lblTest;
        private System.Windows.Forms.Label lblVehicleHandling;
        private System.Windows.Forms.Label lblVehicleArmorLabel;
        private System.Windows.Forms.Label lblVehicleArmor;
        private System.Windows.Forms.Label lblTestLabel;
        private System.Windows.Forms.Label lblVehicleAccelLabel;
        private System.Windows.Forms.Label lblVehicleAccel;
        private System.Windows.Forms.Label lblVehicleBody;
        private System.Windows.Forms.Label lblVehicleBodyLabel;
        private System.Windows.Forms.Label lblVehicleSpeed;
        private System.Windows.Forms.Label lblVehicleSpeedLabel;
        private System.Windows.Forms.Label lblVehicleAvail;
        private System.Windows.Forms.Label lblVehicleAvailLabel;
        private System.Windows.Forms.Label lblVehicleSensor;
        private System.Windows.Forms.Label lblVehicleSensorLabel;
        private System.Windows.Forms.Label lblVehiclePilotLabel;
        private System.Windows.Forms.Label lblVehiclePilot;
        private System.Windows.Forms.TabPage tabBrowse;
        private System.Windows.Forms.DataGridView dgvVehicles;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvc_Guid;
        private DataGridViewTextBoxColumnTranslated dgvc_Name;
        private DataGridViewTextBoxColumnTranslated dgvc_Accel;
        private DataGridViewTextBoxColumnTranslated dgvc_Armor;
        private DataGridViewTextBoxColumnTranslated dgvc_Body;
        private DataGridViewTextBoxColumnTranslated dgvc_Handling;
        private DataGridViewTextBoxColumnTranslated dgvc_Pilot;
        private DataGridViewTextBoxColumnTranslated dgvc_Sensor;
        private DataGridViewTextBoxColumnTranslated dgvc_Speed;
        private DataGridViewTextBoxColumnTranslated dgvc_Seats;
        private DataGridViewTextBoxColumnTranslated dgvc_Gear;
        private DataGridViewTextBoxColumnTranslated dgvc_Mods;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvc_Weapons;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvc_WeaponMounts;
        private DataGridViewTextBoxColumnTranslated Label_Avail;
        private DataGridViewTextBoxColumnTranslated Label_Source;
        private DataGridViewTextBoxColumnTranslated dgvc_Cost;
    }
}
