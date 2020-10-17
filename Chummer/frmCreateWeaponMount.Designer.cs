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
            this.components = new System.ComponentModel.Container();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lblVisbility = new System.Windows.Forms.Label();
            this.cboVisibility = new Chummer.ElasticComboBox();
            this.lblControl = new System.Windows.Forms.Label();
            this.cboControl = new Chummer.ElasticComboBox();
            this.lblSize = new System.Windows.Forms.Label();
            this.cboSize = new Chummer.ElasticComboBox();
            this.lblFlexibility = new System.Windows.Forms.Label();
            this.cboFlexibility = new Chummer.ElasticComboBox();
            this.lblAvailabilityLabel = new System.Windows.Forms.Label();
            this.lblCostLabel = new System.Windows.Forms.Label();
            this.lblAvailability = new System.Windows.Forms.Label();
            this.lblCost = new System.Windows.Forms.Label();
            this.lblSlots = new System.Windows.Forms.Label();
            this.lblSlotsLabel = new System.Windows.Forms.Label();
            this.chkFreeItem = new Chummer.ColorableCheckBox();
            this.nudMarkup = new Chummer.NumericUpDownEx();
            this.lblMarkupPercentLabel = new System.Windows.Forms.Label();
            this.lblMarkupLabel = new System.Windows.Forms.Label();
            this.treMods = new System.Windows.Forms.TreeView();
            this.cmdDeleteMod = new System.Windows.Forms.Button();
            this.cmdAddMod = new System.Windows.Forms.Button();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpTopButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.flpMarkup = new System.Windows.Forms.FlowLayoutPanel();
            this.tlpButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.tlpTopButtons.SuspendLayout();
            this.flpMarkup.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdCancel
            // 
            this.cmdCancel.AutoSize = true;
            this.cmdCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdCancel.Location = new System.Drawing.Point(3, 3);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(50, 23);
            this.cmdCancel.TabIndex = 19;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.AutoSize = true;
            this.cmdOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOK.Location = new System.Drawing.Point(59, 3);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(50, 23);
            this.cmdOK.TabIndex = 20;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lblVisbility
            // 
            this.lblVisbility.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblVisbility.AutoSize = true;
            this.lblVisbility.Location = new System.Drawing.Point(311, 63);
            this.lblVisbility.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVisbility.Name = "lblVisbility";
            this.lblVisbility.Size = new System.Drawing.Size(44, 13);
            this.lblVisbility.TabIndex = 21;
            this.lblVisbility.Tag = "Label_Visibility";
            this.lblVisbility.Text = "Visbility:";
            // 
            // cboVisibility
            // 
            this.cboVisibility.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboVisibility.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboVisibility.FormattingEnabled = true;
            this.cboVisibility.Location = new System.Drawing.Point(361, 59);
            this.cboVisibility.Name = "cboVisibility";
            this.cboVisibility.Size = new System.Drawing.Size(242, 21);
            this.cboVisibility.TabIndex = 22;
            this.cboVisibility.TooltipText = "";
            this.cboVisibility.SelectedIndexChanged += new System.EventHandler(this.comboBox_SelectedIndexChanged);
            // 
            // lblControl
            // 
            this.lblControl.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblControl.AutoSize = true;
            this.lblControl.Location = new System.Drawing.Point(312, 90);
            this.lblControl.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblControl.Name = "lblControl";
            this.lblControl.Size = new System.Drawing.Size(43, 13);
            this.lblControl.TabIndex = 23;
            this.lblControl.Tag = "Label_Control";
            this.lblControl.Text = "Control:";
            // 
            // cboControl
            // 
            this.cboControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboControl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboControl.FormattingEnabled = true;
            this.cboControl.Location = new System.Drawing.Point(361, 86);
            this.cboControl.Name = "cboControl";
            this.cboControl.Size = new System.Drawing.Size(242, 21);
            this.cboControl.TabIndex = 24;
            this.cboControl.TooltipText = "";
            this.cboControl.SelectedIndexChanged += new System.EventHandler(this.comboBox_SelectedIndexChanged);
            // 
            // lblSize
            // 
            this.lblSize.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSize.AutoSize = true;
            this.lblSize.Location = new System.Drawing.Point(325, 36);
            this.lblSize.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(30, 13);
            this.lblSize.TabIndex = 25;
            this.lblSize.Tag = "Label_Size";
            this.lblSize.Text = "Size:";
            // 
            // cboSize
            // 
            this.cboSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSize.FormattingEnabled = true;
            this.cboSize.Location = new System.Drawing.Point(361, 32);
            this.cboSize.Name = "cboSize";
            this.cboSize.Size = new System.Drawing.Size(242, 21);
            this.cboSize.TabIndex = 26;
            this.cboSize.TooltipText = "";
            this.cboSize.SelectedIndexChanged += new System.EventHandler(this.cboSize_SelectedIndexChanged);
            // 
            // lblFlexibility
            // 
            this.lblFlexibility.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblFlexibility.AutoSize = true;
            this.lblFlexibility.Location = new System.Drawing.Point(304, 117);
            this.lblFlexibility.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblFlexibility.Name = "lblFlexibility";
            this.lblFlexibility.Size = new System.Drawing.Size(51, 13);
            this.lblFlexibility.TabIndex = 27;
            this.lblFlexibility.Tag = "Label_Flexibility";
            this.lblFlexibility.Text = "Flexibility:";
            // 
            // cboFlexibility
            // 
            this.cboFlexibility.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboFlexibility.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFlexibility.FormattingEnabled = true;
            this.cboFlexibility.Location = new System.Drawing.Point(361, 113);
            this.cboFlexibility.Name = "cboFlexibility";
            this.cboFlexibility.Size = new System.Drawing.Size(242, 21);
            this.cboFlexibility.TabIndex = 28;
            this.cboFlexibility.TooltipText = "";
            this.cboFlexibility.SelectedIndexChanged += new System.EventHandler(this.comboBox_SelectedIndexChanged);
            // 
            // lblAvailabilityLabel
            // 
            this.lblAvailabilityLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblAvailabilityLabel.AutoSize = true;
            this.lblAvailabilityLabel.Location = new System.Drawing.Point(322, 143);
            this.lblAvailabilityLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAvailabilityLabel.Name = "lblAvailabilityLabel";
            this.lblAvailabilityLabel.Size = new System.Drawing.Size(33, 13);
            this.lblAvailabilityLabel.TabIndex = 30;
            this.lblAvailabilityLabel.Tag = "Label_Avail";
            this.lblAvailabilityLabel.Text = "Avail:";
            this.lblAvailabilityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblCostLabel
            // 
            this.lblCostLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCostLabel.AutoSize = true;
            this.lblCostLabel.Location = new System.Drawing.Point(324, 193);
            this.lblCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCostLabel.Name = "lblCostLabel";
            this.lblCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblCostLabel.TabIndex = 29;
            this.lblCostLabel.Tag = "Label_Cost";
            this.lblCostLabel.Text = "Cost:";
            this.lblCostLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblAvailability
            // 
            this.lblAvailability.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblAvailability.AutoSize = true;
            this.lblAvailability.Location = new System.Drawing.Point(361, 143);
            this.lblAvailability.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAvailability.Name = "lblAvailability";
            this.lblAvailability.Size = new System.Drawing.Size(19, 13);
            this.lblAvailability.TabIndex = 32;
            this.lblAvailability.Tag = "";
            this.lblAvailability.Text = "+0";
            this.lblAvailability.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCost
            // 
            this.lblCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCost.AutoSize = true;
            this.lblCost.Location = new System.Drawing.Point(361, 193);
            this.lblCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCost.Name = "lblCost";
            this.lblCost.Size = new System.Drawing.Size(19, 13);
            this.lblCost.TabIndex = 31;
            this.lblCost.Tag = "";
            this.lblCost.Text = "0¥";
            this.lblCost.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSlots
            // 
            this.lblSlots.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSlots.AutoSize = true;
            this.lblSlots.Location = new System.Drawing.Point(361, 168);
            this.lblSlots.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSlots.Name = "lblSlots";
            this.lblSlots.Size = new System.Drawing.Size(13, 13);
            this.lblSlots.TabIndex = 34;
            this.lblSlots.Tag = "";
            this.lblSlots.Text = "0";
            this.lblSlots.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSlotsLabel
            // 
            this.lblSlotsLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSlotsLabel.AutoSize = true;
            this.lblSlotsLabel.Location = new System.Drawing.Point(322, 168);
            this.lblSlotsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSlotsLabel.Name = "lblSlotsLabel";
            this.lblSlotsLabel.Size = new System.Drawing.Size(33, 13);
            this.lblSlotsLabel.TabIndex = 33;
            this.lblSlotsLabel.Tag = "Label_Slots";
            this.lblSlotsLabel.Text = "Slots:";
            this.lblSlotsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkFreeItem
            // 
            this.chkFreeItem.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkFreeItem.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.chkFreeItem, 2);
            this.chkFreeItem.Location = new System.Drawing.Point(304, 215);
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
            this.nudMarkup.TabIndex = 63;
            this.nudMarkup.ValueChanged += new System.EventHandler(this.nudMarkup_ValueChanged);
            // 
            // lblMarkupPercentLabel
            // 
            this.lblMarkupPercentLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMarkupPercentLabel.AutoSize = true;
            this.lblMarkupPercentLabel.Location = new System.Drawing.Point(65, 6);
            this.lblMarkupPercentLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMarkupPercentLabel.Name = "lblMarkupPercentLabel";
            this.lblMarkupPercentLabel.Size = new System.Drawing.Size(15, 13);
            this.lblMarkupPercentLabel.TabIndex = 64;
            this.lblMarkupPercentLabel.Text = "%";
            this.lblMarkupPercentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMarkupLabel
            // 
            this.lblMarkupLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMarkupLabel.AutoSize = true;
            this.lblMarkupLabel.Location = new System.Drawing.Point(309, 241);
            this.lblMarkupLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMarkupLabel.Name = "lblMarkupLabel";
            this.lblMarkupLabel.Size = new System.Drawing.Size(46, 13);
            this.lblMarkupLabel.TabIndex = 62;
            this.lblMarkupLabel.Tag = "Label_SelectGear_Markup";
            this.lblMarkupLabel.Text = "Markup:";
            this.lblMarkupLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // treMods
            // 
            this.treMods.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treMods.Location = new System.Drawing.Point(3, 32);
            this.treMods.Name = "treMods";
            this.tlpMain.SetRowSpan(this.treMods, 11);
            this.treMods.Size = new System.Drawing.Size(295, 388);
            this.treMods.TabIndex = 65;
            this.treMods.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treMods_AfterSelect);
            // 
            // cmdDeleteMod
            // 
            this.cmdDeleteMod.AutoSize = true;
            this.cmdDeleteMod.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDeleteMod.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdDeleteMod.Location = new System.Drawing.Point(153, 3);
            this.cmdDeleteMod.Name = "cmdDeleteMod";
            this.cmdDeleteMod.Size = new System.Drawing.Size(145, 23);
            this.cmdDeleteMod.TabIndex = 67;
            this.cmdDeleteMod.Tag = "Button_DeleteMod";
            this.cmdDeleteMod.Text = "Delete Mod";
            this.cmdDeleteMod.UseVisualStyleBackColor = true;
            this.cmdDeleteMod.Click += new System.EventHandler(this.cmdDeleteMod_Click);
            // 
            // cmdAddMod
            // 
            this.cmdAddMod.AutoSize = true;
            this.cmdAddMod.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddMod.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdAddMod.Location = new System.Drawing.Point(3, 3);
            this.cmdAddMod.Name = "cmdAddMod";
            this.cmdAddMod.Size = new System.Drawing.Size(144, 23);
            this.cmdAddMod.TabIndex = 66;
            this.cmdAddMod.Tag = "Button_AddMod";
            this.cmdAddMod.Text = "Add Mod";
            this.cmdAddMod.UseVisualStyleBackColor = true;
            this.cmdAddMod.Click += new System.EventHandler(this.cmdAddMod_Click);
            // 
            // lblSource
            // 
            this.lblSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSource.AutoSize = true;
            this.lblSource.Location = new System.Drawing.Point(361, 267);
            this.lblSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 69;
            this.lblSource.Text = "[Source]";
            this.lblSource.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(311, 267);
            this.lblSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 68;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            this.lblSourceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 3;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Controls.Add(this.lblSize, 1, 1);
            this.tlpMain.Controls.Add(this.lblControl, 1, 3);
            this.tlpMain.Controls.Add(this.treMods, 0, 1);
            this.tlpMain.Controls.Add(this.lblVisbility, 1, 2);
            this.tlpMain.Controls.Add(this.lblFlexibility, 1, 4);
            this.tlpMain.Controls.Add(this.cboSize, 2, 1);
            this.tlpMain.Controls.Add(this.cboFlexibility, 2, 4);
            this.tlpMain.Controls.Add(this.lblAvailability, 2, 5);
            this.tlpMain.Controls.Add(this.lblAvailabilityLabel, 1, 5);
            this.tlpMain.Controls.Add(this.lblSourceLabel, 1, 10);
            this.tlpMain.Controls.Add(this.lblMarkupLabel, 1, 9);
            this.tlpMain.Controls.Add(this.lblSource, 2, 10);
            this.tlpMain.Controls.Add(this.lblSlotsLabel, 1, 6);
            this.tlpMain.Controls.Add(this.lblCostLabel, 1, 7);
            this.tlpMain.Controls.Add(this.chkFreeItem, 1, 8);
            this.tlpMain.Controls.Add(this.lblCost, 2, 7);
            this.tlpMain.Controls.Add(this.lblSlots, 2, 6);
            this.tlpMain.Controls.Add(this.cboVisibility, 2, 2);
            this.tlpMain.Controls.Add(this.cboControl, 2, 3);
            this.tlpMain.Controls.Add(this.tlpTopButtons, 0, 0);
            this.tlpMain.Controls.Add(this.flpMarkup, 2, 9);
            this.tlpMain.Controls.Add(this.tlpButtons, 1, 11);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 12;
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
            this.tlpMain.Size = new System.Drawing.Size(606, 423);
            this.tlpMain.TabIndex = 70;
            // 
            // tlpTopButtons
            // 
            this.tlpTopButtons.AutoSize = true;
            this.tlpTopButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpTopButtons.ColumnCount = 2;
            this.tlpTopButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpTopButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpTopButtons.Controls.Add(this.cmdAddMod, 0, 0);
            this.tlpTopButtons.Controls.Add(this.cmdDeleteMod, 1, 0);
            this.tlpTopButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTopButtons.Location = new System.Drawing.Point(0, 0);
            this.tlpTopButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpTopButtons.Name = "tlpTopButtons";
            this.tlpTopButtons.RowCount = 1;
            this.tlpTopButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpTopButtons.Size = new System.Drawing.Size(301, 29);
            this.tlpTopButtons.TabIndex = 71;
            // 
            // flpMarkup
            // 
            this.flpMarkup.AutoSize = true;
            this.flpMarkup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpMarkup.Controls.Add(this.nudMarkup);
            this.flpMarkup.Controls.Add(this.lblMarkupPercentLabel);
            this.flpMarkup.Location = new System.Drawing.Point(358, 235);
            this.flpMarkup.Margin = new System.Windows.Forms.Padding(0);
            this.flpMarkup.Name = "flpMarkup";
            this.flpMarkup.Size = new System.Drawing.Size(83, 26);
            this.flpMarkup.TabIndex = 72;
            // 
            // tlpButtons
            // 
            this.tlpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpButtons.AutoSize = true;
            this.tlpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpButtons.ColumnCount = 2;
            this.tlpMain.SetColumnSpan(this.tlpButtons, 2);
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.Controls.Add(this.cmdCancel, 0, 0);
            this.tlpButtons.Controls.Add(this.cmdOK, 1, 0);
            this.tlpButtons.Location = new System.Drawing.Point(494, 394);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.Size = new System.Drawing.Size(112, 29);
            this.tlpButtons.TabIndex = 73;
            // 
            // frmCreateWeaponMount
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "frmCreateWeaponMount";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.Tag = "Title_CreateWeaponMount";
            this.Text = "Create Weapon Mount";
            this.Load += new System.EventHandler(this.frmCreateWeaponMount_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpTopButtons.ResumeLayout(false);
            this.tlpTopButtons.PerformLayout();
            this.flpMarkup.ResumeLayout(false);
            this.flpMarkup.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Label lblVisbility;
        private ElasticComboBox cboVisibility;
        private System.Windows.Forms.Label lblControl;
        private ElasticComboBox cboControl;
        private System.Windows.Forms.Label lblSize;
        private ElasticComboBox cboSize;
        private System.Windows.Forms.Label lblFlexibility;
        private ElasticComboBox cboFlexibility;
        private System.Windows.Forms.Label lblAvailabilityLabel;
        private System.Windows.Forms.Label lblCostLabel;
        private System.Windows.Forms.Label lblAvailability;
        private System.Windows.Forms.Label lblCost;
        private System.Windows.Forms.Label lblSlots;
        private System.Windows.Forms.Label lblSlotsLabel;
        private Chummer.ColorableCheckBox chkFreeItem;
        private Chummer.NumericUpDownEx nudMarkup;
        private System.Windows.Forms.Label lblMarkupPercentLabel;
        private System.Windows.Forms.Label lblMarkupLabel;
        private System.Windows.Forms.TreeView treMods;
        private System.Windows.Forms.Button cmdDeleteMod;
        private System.Windows.Forms.Button cmdAddMod;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblSourceLabel;
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private BufferedTableLayoutPanel tlpTopButtons;
        private System.Windows.Forms.FlowLayoutPanel flpMarkup;
        private BufferedTableLayoutPanel tlpButtons;
    }
}
