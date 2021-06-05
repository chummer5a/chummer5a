namespace Chummer
{
    partial class frmSelectWeaponAccessory
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
            this.lblMountLabel = new System.Windows.Forms.Label();
            this.lblCost = new System.Windows.Forms.Label();
            this.lblCostLabel = new System.Windows.Forms.Label();
            this.lblAvail = new System.Windows.Forms.Label();
            this.lblAvailLabel = new System.Windows.Forms.Label();
            this.lblRC = new System.Windows.Forms.Label();
            this.lblRCLabel = new System.Windows.Forms.Label();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lstAccessory = new System.Windows.Forms.ListBox();
            this.cmdOKAdd = new System.Windows.Forms.Button();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.chkFreeItem = new Chummer.ColorableCheckBox(this.components);
            this.nudMarkup = new Chummer.NumericUpDownEx();
            this.lblMarkupLabel = new System.Windows.Forms.Label();
            this.lblMarkupPercentLabel = new System.Windows.Forms.Label();
            this.lblTest = new System.Windows.Forms.Label();
            this.lblTestLabel = new System.Windows.Forms.Label();
            this.nudRating = new Chummer.NumericUpDownEx();
            this.lblRatingLabel = new System.Windows.Forms.Label();
            this.cboMount = new Chummer.ElasticComboBox();
            this.chkBlackMarketDiscount = new Chummer.ColorableCheckBox(this.components);
            this.cboExtraMount = new Chummer.ElasticComboBox();
            this.lblExtraMountLabel = new System.Windows.Forms.Label();
            this.chkHideOverAvailLimit = new Chummer.ColorableCheckBox(this.components);
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.chkShowOnlyAffordItems = new Chummer.ColorableCheckBox(this.components);
            this.tlpButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpRight = new Chummer.BufferedTableLayoutPanel(this.components);
            this.flpCheckBoxes = new System.Windows.Forms.FlowLayoutPanel();
            this.flpMarkup = new System.Windows.Forms.FlowLayoutPanel();
            this.flpRating = new System.Windows.Forms.FlowLayoutPanel();
            this.lblRatingNALabel = new System.Windows.Forms.Label();
            this.tlpTopRight = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblSearchLabel = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRating)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            this.tlpRight.SuspendLayout();
            this.flpCheckBoxes.SuspendLayout();
            this.flpMarkup.SuspendLayout();
            this.flpRating.SuspendLayout();
            this.tlpTopRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblMountLabel
            // 
            this.lblMountLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMountLabel.AutoSize = true;
            this.lblMountLabel.Location = new System.Drawing.Point(30, 7);
            this.lblMountLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMountLabel.Name = "lblMountLabel";
            this.lblMountLabel.Size = new System.Drawing.Size(40, 13);
            this.lblMountLabel.TabIndex = 3;
            this.lblMountLabel.Tag = "Label_Mount";
            this.lblMountLabel.Text = "Mount:";
            // 
            // lblCost
            // 
            this.lblCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCost.AutoSize = true;
            this.lblCost.Location = new System.Drawing.Point(76, 136);
            this.lblCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCost.Name = "lblCost";
            this.lblCost.Size = new System.Drawing.Size(34, 13);
            this.lblCost.TabIndex = 10;
            this.lblCost.Text = "[Cost]";
            // 
            // lblCostLabel
            // 
            this.lblCostLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCostLabel.AutoSize = true;
            this.lblCostLabel.Location = new System.Drawing.Point(39, 136);
            this.lblCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCostLabel.Name = "lblCostLabel";
            this.lblCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblCostLabel.TabIndex = 9;
            this.lblCostLabel.Tag = "Label_Cost";
            this.lblCostLabel.Text = "Cost:";
            // 
            // lblAvail
            // 
            this.lblAvail.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblAvail.AutoSize = true;
            this.lblAvail.Location = new System.Drawing.Point(76, 111);
            this.lblAvail.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAvail.Name = "lblAvail";
            this.lblAvail.Size = new System.Drawing.Size(36, 13);
            this.lblAvail.TabIndex = 6;
            this.lblAvail.Text = "[Avail]";
            // 
            // lblAvailLabel
            // 
            this.lblAvailLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblAvailLabel.AutoSize = true;
            this.lblAvailLabel.Location = new System.Drawing.Point(37, 111);
            this.lblAvailLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAvailLabel.Name = "lblAvailLabel";
            this.lblAvailLabel.Size = new System.Drawing.Size(33, 13);
            this.lblAvailLabel.TabIndex = 5;
            this.lblAvailLabel.Tag = "Label_Avail";
            this.lblAvailLabel.Text = "Avail:";
            // 
            // lblRC
            // 
            this.lblRC.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblRC.AutoSize = true;
            this.tlpRight.SetColumnSpan(this.lblRC, 3);
            this.lblRC.Location = new System.Drawing.Point(76, 86);
            this.lblRC.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRC.Name = "lblRC";
            this.lblRC.Size = new System.Drawing.Size(28, 13);
            this.lblRC.TabIndex = 2;
            this.lblRC.Text = "[RC]";
            // 
            // lblRCLabel
            // 
            this.lblRCLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblRCLabel.AutoSize = true;
            this.lblRCLabel.Location = new System.Drawing.Point(45, 86);
            this.lblRCLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRCLabel.Name = "lblRCLabel";
            this.lblRCLabel.Size = new System.Drawing.Size(25, 13);
            this.lblRCLabel.TabIndex = 1;
            this.lblRCLabel.Tag = "Label_RC";
            this.lblRCLabel.Text = "RC:";
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
            this.cmdOK.Location = new System.Drawing.Point(159, 3);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(72, 23);
            this.cmdOK.TabIndex = 17;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lstAccessory
            // 
            this.lstAccessory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstAccessory.FormattingEnabled = true;
            this.lstAccessory.Location = new System.Drawing.Point(3, 3);
            this.lstAccessory.Name = "lstAccessory";
            this.tlpMain.SetRowSpan(this.lstAccessory, 6);
            this.lstAccessory.Size = new System.Drawing.Size(297, 417);
            this.lstAccessory.TabIndex = 0;
            this.lstAccessory.SelectedIndexChanged += new System.EventHandler(this.lstAccessory_SelectedIndexChanged);
            this.lstAccessory.DoubleClick += new System.EventHandler(this.lstAccessory_DoubleClick);
            // 
            // cmdOKAdd
            // 
            this.cmdOKAdd.AutoSize = true;
            this.cmdOKAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOKAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOKAdd.Location = new System.Drawing.Point(81, 3);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(72, 23);
            this.cmdOKAdd.TabIndex = 18;
            this.cmdOKAdd.Tag = "String_AddMore";
            this.cmdOKAdd.Text = "&Add && More";
            this.cmdOKAdd.UseVisualStyleBackColor = true;
            this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
            // 
            // lblSource
            // 
            this.lblSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSource.AutoSize = true;
            this.lblSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSource.Location = new System.Drawing.Point(76, 187);
            this.lblSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 16;
            this.lblSource.Text = "[Source]";
            this.lblSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(26, 187);
            this.lblSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 15;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
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
            this.chkFreeItem.TabIndex = 11;
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
            99,
            0,
            0,
            -2147483648});
            this.nudMarkup.Name = "nudMarkup";
            this.nudMarkup.Size = new System.Drawing.Size(56, 20);
            this.nudMarkup.TabIndex = 13;
            this.nudMarkup.ValueChanged += new System.EventHandler(this.nudMarkup_ValueChanged);
            // 
            // lblMarkupLabel
            // 
            this.lblMarkupLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMarkupLabel.AutoSize = true;
            this.lblMarkupLabel.Location = new System.Drawing.Point(165, 136);
            this.lblMarkupLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMarkupLabel.Name = "lblMarkupLabel";
            this.lblMarkupLabel.Size = new System.Drawing.Size(46, 13);
            this.lblMarkupLabel.TabIndex = 12;
            this.lblMarkupLabel.Tag = "Label_SelectGear_Markup";
            this.lblMarkupLabel.Text = "Markup:";
            // 
            // lblMarkupPercentLabel
            // 
            this.lblMarkupPercentLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMarkupPercentLabel.AutoSize = true;
            this.lblMarkupPercentLabel.Location = new System.Drawing.Point(65, 6);
            this.lblMarkupPercentLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMarkupPercentLabel.Name = "lblMarkupPercentLabel";
            this.lblMarkupPercentLabel.Size = new System.Drawing.Size(15, 13);
            this.lblMarkupPercentLabel.TabIndex = 14;
            this.lblMarkupPercentLabel.Text = "%";
            // 
            // lblTest
            // 
            this.lblTest.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblTest.AutoSize = true;
            this.lblTest.Location = new System.Drawing.Point(217, 111);
            this.lblTest.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTest.Name = "lblTest";
            this.lblTest.Size = new System.Drawing.Size(19, 13);
            this.lblTest.TabIndex = 8;
            this.lblTest.Text = "[0]";
            // 
            // lblTestLabel
            // 
            this.lblTestLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTestLabel.AutoSize = true;
            this.lblTestLabel.Location = new System.Drawing.Point(180, 111);
            this.lblTestLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTestLabel.Name = "lblTestLabel";
            this.lblTestLabel.Size = new System.Drawing.Size(31, 13);
            this.lblTestLabel.TabIndex = 7;
            this.lblTestLabel.Tag = "Label_Test";
            this.lblTestLabel.Text = "Test:";
            // 
            // nudRating
            // 
            this.nudRating.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudRating.AutoSize = true;
            this.nudRating.Enabled = false;
            this.nudRating.Location = new System.Drawing.Point(3, 3);
            this.nudRating.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudRating.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudRating.Name = "nudRating";
            this.nudRating.Size = new System.Drawing.Size(41, 20);
            this.nudRating.TabIndex = 14;
            this.nudRating.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudRating.ValueChanged += new System.EventHandler(this.nudRating_ValueChanged);
            // 
            // lblRatingLabel
            // 
            this.lblRatingLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblRatingLabel.AutoSize = true;
            this.lblRatingLabel.Location = new System.Drawing.Point(29, 60);
            this.lblRatingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRatingLabel.Name = "lblRatingLabel";
            this.lblRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblRatingLabel.TabIndex = 13;
            this.lblRatingLabel.Tag = "Label_Rating";
            this.lblRatingLabel.Text = "Rating:";
            // 
            // cboMount
            // 
            this.cboMount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpRight.SetColumnSpan(this.cboMount, 3);
            this.cboMount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMount.FormattingEnabled = true;
            this.cboMount.Location = new System.Drawing.Point(76, 3);
            this.cboMount.Name = "cboMount";
            this.cboMount.Size = new System.Drawing.Size(224, 21);
            this.cboMount.TabIndex = 20;
            this.cboMount.TooltipText = "";
            this.cboMount.SelectedIndexChanged += new System.EventHandler(this.cboMount_SelectedIndexChanged);
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
            this.chkBlackMarketDiscount.TabIndex = 39;
            this.chkBlackMarketDiscount.Tag = "Checkbox_BlackMarketDiscount";
            this.chkBlackMarketDiscount.Text = "Black Market Discount (10%)";
            this.chkBlackMarketDiscount.UseVisualStyleBackColor = true;
            this.chkBlackMarketDiscount.Visible = false;
            this.chkBlackMarketDiscount.CheckedChanged += new System.EventHandler(this.chkBlackMarketDiscount_CheckedChanged);
            // 
            // cboExtraMount
            // 
            this.cboExtraMount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpRight.SetColumnSpan(this.cboExtraMount, 3);
            this.cboExtraMount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboExtraMount.FormattingEnabled = true;
            this.cboExtraMount.Location = new System.Drawing.Point(76, 30);
            this.cboExtraMount.Name = "cboExtraMount";
            this.cboExtraMount.Size = new System.Drawing.Size(224, 21);
            this.cboExtraMount.TabIndex = 41;
            this.cboExtraMount.TooltipText = "";
            this.cboExtraMount.SelectedIndexChanged += new System.EventHandler(this.cboExtraMount_SelectedIndexChanged);
            // 
            // lblExtraMountLabel
            // 
            this.lblExtraMountLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblExtraMountLabel.AutoSize = true;
            this.lblExtraMountLabel.Location = new System.Drawing.Point(3, 34);
            this.lblExtraMountLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblExtraMountLabel.Name = "lblExtraMountLabel";
            this.lblExtraMountLabel.Size = new System.Drawing.Size(67, 13);
            this.lblExtraMountLabel.TabIndex = 40;
            this.lblExtraMountLabel.Tag = "Label_ExtraMount";
            this.lblExtraMountLabel.Text = "Extra Mount:";
            // 
            // chkHideOverAvailLimit
            // 
            this.chkHideOverAvailLimit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkHideOverAvailLimit.AutoSize = true;
            this.chkHideOverAvailLimit.DefaultColorScheme = true;
            this.chkHideOverAvailLimit.Location = new System.Drawing.Point(306, 348);
            this.chkHideOverAvailLimit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkHideOverAvailLimit.Name = "chkHideOverAvailLimit";
            this.chkHideOverAvailLimit.Size = new System.Drawing.Size(175, 17);
            this.chkHideOverAvailLimit.TabIndex = 66;
            this.chkHideOverAvailLimit.Tag = "Checkbox_HideOverAvailLimit";
            this.chkHideOverAvailLimit.Text = "Hide Items Over Avail Limit ({0})";
            this.chkHideOverAvailLimit.UseVisualStyleBackColor = true;
            this.chkHideOverAvailLimit.CheckedChanged += new System.EventHandler(this.RefreshCurrentList);
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.Controls.Add(this.lstAccessory, 0, 0);
            this.tlpMain.Controls.Add(this.chkHideOverAvailLimit, 1, 3);
            this.tlpMain.Controls.Add(this.chkShowOnlyAffordItems, 1, 4);
            this.tlpMain.Controls.Add(this.tlpButtons, 1, 5);
            this.tlpMain.Controls.Add(this.tlpRight, 1, 1);
            this.tlpMain.Controls.Add(this.tlpTopRight, 1, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 6;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(606, 423);
            this.tlpMain.TabIndex = 67;
            // 
            // chkShowOnlyAffordItems
            // 
            this.chkShowOnlyAffordItems.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkShowOnlyAffordItems.AutoSize = true;
            this.chkShowOnlyAffordItems.DefaultColorScheme = true;
            this.chkShowOnlyAffordItems.Location = new System.Drawing.Point(306, 373);
            this.chkShowOnlyAffordItems.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkShowOnlyAffordItems.Name = "chkShowOnlyAffordItems";
            this.chkShowOnlyAffordItems.Size = new System.Drawing.Size(164, 17);
            this.chkShowOnlyAffordItems.TabIndex = 72;
            this.chkShowOnlyAffordItems.Tag = "Checkbox_ShowOnlyAffordItems";
            this.chkShowOnlyAffordItems.Text = "Show Only Items I Can Afford";
            this.chkShowOnlyAffordItems.UseVisualStyleBackColor = true;
            this.chkShowOnlyAffordItems.CheckedChanged += new System.EventHandler(this.RefreshCurrentList);
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
            this.tlpButtons.Location = new System.Drawing.Point(372, 394);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButtons.Size = new System.Drawing.Size(234, 29);
            this.tlpButtons.TabIndex = 77;
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
            this.tlpRight.Controls.Add(this.lblSource, 1, 7);
            this.tlpRight.Controls.Add(this.lblMountLabel, 0, 0);
            this.tlpRight.Controls.Add(this.lblSourceLabel, 0, 7);
            this.tlpRight.Controls.Add(this.cboExtraMount, 1, 1);
            this.tlpRight.Controls.Add(this.lblRC, 1, 3);
            this.tlpRight.Controls.Add(this.flpCheckBoxes, 0, 6);
            this.tlpRight.Controls.Add(this.cboMount, 1, 0);
            this.tlpRight.Controls.Add(this.flpMarkup, 3, 5);
            this.tlpRight.Controls.Add(this.lblMarkupLabel, 2, 5);
            this.tlpRight.Controls.Add(this.lblCostLabel, 0, 5);
            this.tlpRight.Controls.Add(this.flpRating, 1, 2);
            this.tlpRight.Controls.Add(this.lblCost, 1, 5);
            this.tlpRight.Controls.Add(this.lblRatingLabel, 0, 2);
            this.tlpRight.Controls.Add(this.lblExtraMountLabel, 0, 1);
            this.tlpRight.Controls.Add(this.lblRCLabel, 0, 3);
            this.tlpRight.Controls.Add(this.lblTest, 3, 4);
            this.tlpRight.Controls.Add(this.lblTestLabel, 2, 4);
            this.tlpRight.Controls.Add(this.lblAvailLabel, 0, 4);
            this.tlpRight.Controls.Add(this.lblAvail, 1, 4);
            this.tlpRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRight.Location = new System.Drawing.Point(303, 26);
            this.tlpRight.Margin = new System.Windows.Forms.Padding(0);
            this.tlpRight.Name = "tlpRight";
            this.tlpRight.RowCount = 8;
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpRight.Size = new System.Drawing.Size(303, 206);
            this.tlpRight.TabIndex = 78;
            // 
            // flpCheckBoxes
            // 
            this.flpCheckBoxes.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpCheckBoxes.AutoSize = true;
            this.flpCheckBoxes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.SetColumnSpan(this.flpCheckBoxes, 4);
            this.flpCheckBoxes.Controls.Add(this.chkFreeItem);
            this.flpCheckBoxes.Controls.Add(this.chkBlackMarketDiscount);
            this.flpCheckBoxes.Location = new System.Drawing.Point(0, 156);
            this.flpCheckBoxes.Margin = new System.Windows.Forms.Padding(0);
            this.flpCheckBoxes.Name = "flpCheckBoxes";
            this.flpCheckBoxes.Size = new System.Drawing.Size(225, 25);
            this.flpCheckBoxes.TabIndex = 75;
            // 
            // flpMarkup
            // 
            this.flpMarkup.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpMarkup.AutoSize = true;
            this.flpMarkup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpMarkup.Controls.Add(this.nudMarkup);
            this.flpMarkup.Controls.Add(this.lblMarkupPercentLabel);
            this.flpMarkup.Location = new System.Drawing.Point(214, 130);
            this.flpMarkup.Margin = new System.Windows.Forms.Padding(0);
            this.flpMarkup.Name = "flpMarkup";
            this.flpMarkup.Size = new System.Drawing.Size(83, 26);
            this.flpMarkup.TabIndex = 76;
            this.flpMarkup.WrapContents = false;
            // 
            // flpRating
            // 
            this.flpRating.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpRating.AutoSize = true;
            this.flpRating.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.SetColumnSpan(this.flpRating, 3);
            this.flpRating.Controls.Add(this.nudRating);
            this.flpRating.Controls.Add(this.lblRatingNALabel);
            this.flpRating.Location = new System.Drawing.Point(73, 54);
            this.flpRating.Margin = new System.Windows.Forms.Padding(0);
            this.flpRating.Name = "flpRating";
            this.flpRating.Size = new System.Drawing.Size(80, 26);
            this.flpRating.TabIndex = 73;
            this.flpRating.WrapContents = false;
            // 
            // lblRatingNALabel
            // 
            this.lblRatingNALabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblRatingNALabel.AutoSize = true;
            this.lblRatingNALabel.Location = new System.Drawing.Point(50, 6);
            this.lblRatingNALabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 7);
            this.lblRatingNALabel.Name = "lblRatingNALabel";
            this.lblRatingNALabel.Size = new System.Drawing.Size(27, 13);
            this.lblRatingNALabel.TabIndex = 15;
            this.lblRatingNALabel.Tag = "String_NotApplicable";
            this.lblRatingNALabel.Text = "N/A";
            this.lblRatingNALabel.Visible = false;
            // 
            // tlpTopRight
            // 
            this.tlpTopRight.AutoSize = true;
            this.tlpTopRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpTopRight.ColumnCount = 2;
            this.tlpTopRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpTopRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTopRight.Controls.Add(this.lblSearchLabel, 0, 0);
            this.tlpTopRight.Controls.Add(this.txtSearch, 1, 0);
            this.tlpTopRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTopRight.Location = new System.Drawing.Point(303, 0);
            this.tlpTopRight.Margin = new System.Windows.Forms.Padding(0);
            this.tlpTopRight.Name = "tlpTopRight";
            this.tlpTopRight.RowCount = 1;
            this.tlpTopRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTopRight.Size = new System.Drawing.Size(303, 26);
            this.tlpTopRight.TabIndex = 79;
            // 
            // lblSearchLabel
            // 
            this.lblSearchLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSearchLabel.AutoSize = true;
            this.lblSearchLabel.Location = new System.Drawing.Point(3, 6);
            this.lblSearchLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSearchLabel.Name = "lblSearchLabel";
            this.lblSearchLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSearchLabel.TabIndex = 68;
            this.lblSearchLabel.Tag = "Label_Search";
            this.lblSearchLabel.Text = "&Search:";
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(53, 3);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(247, 20);
            this.txtSearch.TabIndex = 69;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // frmSelectWeaponAccessory
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.tlpMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectWeaponAccessory";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectWeaponAccessory";
            this.Text = "Select an Accessory";
            this.Load += new System.EventHandler(this.frmSelectWeaponAccessory_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRating)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.tlpRight.ResumeLayout(false);
            this.tlpRight.PerformLayout();
            this.flpCheckBoxes.ResumeLayout(false);
            this.flpCheckBoxes.PerformLayout();
            this.flpMarkup.ResumeLayout(false);
            this.flpMarkup.PerformLayout();
            this.flpRating.ResumeLayout(false);
            this.flpRating.PerformLayout();
            this.tlpTopRight.ResumeLayout(false);
            this.tlpTopRight.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblMountLabel;
        private System.Windows.Forms.Label lblCost;
        private System.Windows.Forms.Label lblCostLabel;
        private System.Windows.Forms.Label lblAvail;
        private System.Windows.Forms.Label lblAvailLabel;
        private System.Windows.Forms.Label lblRC;
        private System.Windows.Forms.Label lblRCLabel;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.ListBox lstAccessory;
        private System.Windows.Forms.Button cmdOKAdd;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblSourceLabel;
        private Chummer.ColorableCheckBox chkFreeItem;
        private Chummer.NumericUpDownEx nudMarkup;
        private Chummer.NumericUpDownEx nudRating;
        private System.Windows.Forms.Label lblRatingLabel;
        private System.Windows.Forms.Label lblMarkupLabel;
        private System.Windows.Forms.Label lblMarkupPercentLabel;
        private System.Windows.Forms.Label lblTest;
        private System.Windows.Forms.Label lblTestLabel;
        private ElasticComboBox cboMount;
        private Chummer.ColorableCheckBox chkBlackMarketDiscount;
        private ElasticComboBox cboExtraMount;
        private System.Windows.Forms.Label lblExtraMountLabel;
        private Chummer.ColorableCheckBox chkHideOverAvailLimit;
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private Chummer.ColorableCheckBox chkShowOnlyAffordItems;
        private System.Windows.Forms.FlowLayoutPanel flpRating;
        private System.Windows.Forms.Label lblRatingNALabel;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearchLabel;
        private System.Windows.Forms.FlowLayoutPanel flpCheckBoxes;
        private System.Windows.Forms.FlowLayoutPanel flpMarkup;
        private BufferedTableLayoutPanel tlpButtons;
        private BufferedTableLayoutPanel tlpRight;
        private BufferedTableLayoutPanel tlpTopRight;
    }
}
