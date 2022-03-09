namespace Chummer
{
    partial class SelectGear
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
                Utils.StringHashSetPool.Return(_setAllowedCategories);
                Utils.StringHashSetPool.Return(_setAllowedNames);
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
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lstGear = new System.Windows.Forms.ListBox();
            this.lblCategory = new System.Windows.Forms.Label();
            this.cboCategory = new Chummer.ElasticComboBox();
            this.nudRating = new Chummer.NumericUpDownEx();
            this.lblRatingLabel = new System.Windows.Forms.Label();
            this.lblCost = new System.Windows.Forms.Label();
            this.lblCostLabel = new System.Windows.Forms.Label();
            this.lblAvail = new System.Windows.Forms.Label();
            this.lblAvailLabel = new System.Windows.Forms.Label();
            this.lblMaximumCapacity = new System.Windows.Forms.Label();
            this.lblSearchLabel = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.cmdOKAdd = new System.Windows.Forms.Button();
            this.lblGearDeviceRating = new System.Windows.Forms.Label();
            this.lblGearDeviceRatingLabel = new System.Windows.Forms.Label();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.lblCapacity = new System.Windows.Forms.Label();
            this.nudGearQty = new Chummer.NumericUpDownEx();
            this.lblGearQtyLabel = new System.Windows.Forms.Label();
            this.chkFreeItem = new Chummer.ColorableCheckBox(this.components);
            this.chkDoItYourself = new Chummer.ColorableCheckBox(this.components);
            this.nudMarkup = new Chummer.NumericUpDownEx();
            this.lblMarkupLabel = new System.Windows.Forms.Label();
            this.chkStack = new Chummer.ColorableCheckBox(this.components);
            this.lblTest = new System.Windows.Forms.Label();
            this.lblTestLabel = new System.Windows.Forms.Label();
            this.chkBlackMarketDiscount = new Chummer.ColorableCheckBox(this.components);
            this.chkHideOverAvailLimit = new Chummer.ColorableCheckBox(this.components);
            this.chkShowOnlyAffordItems = new Chummer.ColorableCheckBox(this.components);
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpLeft = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpRight = new Chummer.BufferedTableLayoutPanel(this.components);
            this.flpCheckBoxes = new System.Windows.Forms.FlowLayoutPanel();
            this.flpQty = new System.Windows.Forms.FlowLayoutPanel();
            this.flpRating = new System.Windows.Forms.FlowLayoutPanel();
            this.lblRatingNALabel = new System.Windows.Forms.Label();
            this.lblCapacityLabel = new System.Windows.Forms.Label();
            this.flpMarkup = new System.Windows.Forms.FlowLayoutPanel();
            this.lblMarkupPercentLabel = new System.Windows.Forms.Label();
            this.tlpTopRight = new Chummer.BufferedTableLayoutPanel(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.nudRating)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGearQty)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.tlpLeft.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            this.tlpRight.SuspendLayout();
            this.flpCheckBoxes.SuspendLayout();
            this.flpQty.SuspendLayout();
            this.flpRating.SuspendLayout();
            this.flpMarkup.SuspendLayout();
            this.tlpTopRight.SuspendLayout();
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
            this.cmdCancel.Size = new System.Drawing.Size(72, 23);
            this.cmdCancel.TabIndex = 38;
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
            this.cmdOK.TabIndex = 36;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lstGear
            // 
            this.tlpLeft.SetColumnSpan(this.lstGear, 2);
            this.lstGear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstGear.FormattingEnabled = true;
            this.lstGear.Location = new System.Drawing.Point(3, 30);
            this.lstGear.Name = "lstGear";
            this.lstGear.Size = new System.Drawing.Size(300, 510);
            this.lstGear.TabIndex = 35;
            this.lstGear.SelectedIndexChanged += new System.EventHandler(this.lstGear_SelectedIndexChanged);
            this.lstGear.DoubleClick += new System.EventHandler(this.cmdOK_Click);
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
            this.lblCategory.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
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
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // nudRating
            // 
            this.nudRating.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudRating.AutoSize = true;
            this.nudRating.Enabled = false;
            this.nudRating.Location = new System.Drawing.Point(3, 3);
            this.nudRating.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.nudRating.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudRating.Name = "nudRating";
            this.nudRating.Size = new System.Drawing.Size(59, 20);
            this.nudRating.TabIndex = 11;
            this.nudRating.ValueChanged += new System.EventHandler(this.nudRating_ValueChanged);
            // 
            // lblRatingLabel
            // 
            this.lblRatingLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblRatingLabel.AutoSize = true;
            this.lblRatingLabel.Location = new System.Drawing.Point(13, 6);
            this.lblRatingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRatingLabel.Name = "lblRatingLabel";
            this.lblRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblRatingLabel.TabIndex = 10;
            this.lblRatingLabel.Tag = "Label_Rating";
            this.lblRatingLabel.Text = "Rating:";
            this.lblRatingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblCost
            // 
            this.lblCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCost.AutoSize = true;
            this.lblCost.Location = new System.Drawing.Point(60, 133);
            this.lblCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCost.Name = "lblCost";
            this.lblCost.Size = new System.Drawing.Size(19, 13);
            this.lblCost.TabIndex = 9;
            this.lblCost.Text = "[0]";
            this.lblCost.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCostLabel
            // 
            this.lblCostLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCostLabel.AutoSize = true;
            this.lblCostLabel.Location = new System.Drawing.Point(23, 133);
            this.lblCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCostLabel.Name = "lblCostLabel";
            this.lblCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblCostLabel.TabIndex = 8;
            this.lblCostLabel.Tag = "Label_Cost";
            this.lblCostLabel.Text = "Cost:";
            this.lblCostLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblAvail
            // 
            this.lblAvail.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblAvail.AutoSize = true;
            this.lblAvail.Location = new System.Drawing.Point(60, 108);
            this.lblAvail.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAvail.Name = "lblAvail";
            this.lblAvail.Size = new System.Drawing.Size(19, 13);
            this.lblAvail.TabIndex = 5;
            this.lblAvail.Text = "[0]";
            this.lblAvail.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAvailLabel
            // 
            this.lblAvailLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblAvailLabel.AutoSize = true;
            this.lblAvailLabel.Location = new System.Drawing.Point(21, 108);
            this.lblAvailLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAvailLabel.Name = "lblAvailLabel";
            this.lblAvailLabel.Size = new System.Drawing.Size(33, 13);
            this.lblAvailLabel.TabIndex = 4;
            this.lblAvailLabel.Tag = "Label_Avail";
            this.lblAvailLabel.Text = "Avail:";
            this.lblAvailLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblMaximumCapacity
            // 
            this.lblMaximumCapacity.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMaximumCapacity.AutoSize = true;
            this.tlpRight.SetColumnSpan(this.lblMaximumCapacity, 4);
            this.lblMaximumCapacity.Location = new System.Drawing.Point(3, 57);
            this.lblMaximumCapacity.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMaximumCapacity.Name = "lblMaximumCapacity";
            this.lblMaximumCapacity.Size = new System.Drawing.Size(101, 13);
            this.lblMaximumCapacity.TabIndex = 30;
            this.lblMaximumCapacity.Text = "[Maximum Capacity]";
            // 
            // lblSearchLabel
            // 
            this.lblSearchLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSearchLabel.AutoSize = true;
            this.lblSearchLabel.Location = new System.Drawing.Point(3, 6);
            this.lblSearchLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSearchLabel.Name = "lblSearchLabel";
            this.lblSearchLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSearchLabel.TabIndex = 0;
            this.lblSearchLabel.Tag = "Label_Search";
            this.lblSearchLabel.Text = "&Search:";
            this.lblSearchLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(53, 3);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(404, 20);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
            this.txtSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyUp);
            // 
            // cmdOKAdd
            // 
            this.cmdOKAdd.AutoSize = true;
            this.cmdOKAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOKAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOKAdd.Location = new System.Drawing.Point(81, 3);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(72, 23);
            this.cmdOKAdd.TabIndex = 37;
            this.cmdOKAdd.Tag = "String_AddMore";
            this.cmdOKAdd.Text = "&Add && More";
            this.cmdOKAdd.UseVisualStyleBackColor = true;
            this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
            // 
            // lblGearDeviceRating
            // 
            this.lblGearDeviceRating.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblGearDeviceRating.AutoSize = true;
            this.lblGearDeviceRating.Location = new System.Drawing.Point(303, 6);
            this.lblGearDeviceRating.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearDeviceRating.Name = "lblGearDeviceRating";
            this.lblGearDeviceRating.Size = new System.Drawing.Size(19, 13);
            this.lblGearDeviceRating.TabIndex = 23;
            this.lblGearDeviceRating.Text = "[0]";
            // 
            // lblGearDeviceRatingLabel
            // 
            this.lblGearDeviceRatingLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblGearDeviceRatingLabel.AutoSize = true;
            this.lblGearDeviceRatingLabel.Location = new System.Drawing.Point(219, 6);
            this.lblGearDeviceRatingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearDeviceRatingLabel.Name = "lblGearDeviceRatingLabel";
            this.lblGearDeviceRatingLabel.Size = new System.Drawing.Size(78, 13);
            this.lblGearDeviceRatingLabel.TabIndex = 22;
            this.lblGearDeviceRatingLabel.Tag = "Label_DeviceRating";
            this.lblGearDeviceRatingLabel.Text = "Device Rating:";
            this.lblGearDeviceRatingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblSource
            // 
            this.lblSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSource.AutoSize = true;
            this.lblSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSource.Location = new System.Drawing.Point(60, 184);
            this.lblSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 32;
            this.lblSource.Text = "[Source]";
            this.lblSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(10, 184);
            this.lblSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 31;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            // 
            // lblCapacity
            // 
            this.lblCapacity.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCapacity.AutoSize = true;
            this.tlpRight.SetColumnSpan(this.lblCapacity, 3);
            this.lblCapacity.Location = new System.Drawing.Point(60, 32);
            this.lblCapacity.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCapacity.Name = "lblCapacity";
            this.lblCapacity.Size = new System.Drawing.Size(19, 13);
            this.lblCapacity.TabIndex = 3;
            this.lblCapacity.Text = "[0]";
            this.lblCapacity.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // nudGearQty
            // 
            this.nudGearQty.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudGearQty.AutoSize = true;
            this.nudGearQty.Location = new System.Drawing.Point(3, 3);
            this.nudGearQty.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.nudGearQty.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudGearQty.Name = "nudGearQty";
            this.nudGearQty.Size = new System.Drawing.Size(77, 20);
            this.nudGearQty.TabIndex = 14;
            this.nudGearQty.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudGearQty.ValueChanged += new System.EventHandler(this.nudGearQty_ValueChanged);
            // 
            // lblGearQtyLabel
            // 
            this.lblGearQtyLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblGearQtyLabel.AutoSize = true;
            this.lblGearQtyLabel.Location = new System.Drawing.Point(28, 82);
            this.lblGearQtyLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearQtyLabel.Name = "lblGearQtyLabel";
            this.lblGearQtyLabel.Size = new System.Drawing.Size(26, 13);
            this.lblGearQtyLabel.TabIndex = 13;
            this.lblGearQtyLabel.Tag = "Label_Qty";
            this.lblGearQtyLabel.Text = "Qty:";
            this.lblGearQtyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
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
            this.chkFreeItem.TabIndex = 16;
            this.chkFreeItem.Tag = "Checkbox_Free";
            this.chkFreeItem.Text = "Free!";
            this.chkFreeItem.UseVisualStyleBackColor = true;
            this.chkFreeItem.CheckedChanged += new System.EventHandler(this.chkFreeItem_CheckedChanged);
            // 
            // chkDoItYourself
            // 
            this.chkDoItYourself.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkDoItYourself.AutoSize = true;
            this.chkDoItYourself.DefaultColorScheme = true;
            this.chkDoItYourself.Location = new System.Drawing.Point(228, 4);
            this.chkDoItYourself.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkDoItYourself.Name = "chkDoItYourself";
            this.chkDoItYourself.Size = new System.Drawing.Size(90, 17);
            this.chkDoItYourself.TabIndex = 17;
            this.chkDoItYourself.Tag = "Label_SelectGear_DoItYourself";
            this.chkDoItYourself.Text = "Do It Yourself";
            this.chkDoItYourself.UseVisualStyleBackColor = true;
            this.chkDoItYourself.Visible = false;
            this.chkDoItYourself.CheckedChanged += new System.EventHandler(this.chkDoItYourself_CheckedChanged);
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
            this.nudMarkup.TabIndex = 20;
            this.nudMarkup.ValueChanged += new System.EventHandler(this.nudMarkup_ValueChanged);
            // 
            // lblMarkupLabel
            // 
            this.lblMarkupLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMarkupLabel.AutoSize = true;
            this.lblMarkupLabel.Location = new System.Drawing.Point(251, 133);
            this.lblMarkupLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMarkupLabel.Name = "lblMarkupLabel";
            this.lblMarkupLabel.Size = new System.Drawing.Size(46, 13);
            this.lblMarkupLabel.TabIndex = 19;
            this.lblMarkupLabel.Tag = "Label_SelectGear_Markup";
            this.lblMarkupLabel.Text = "Markup:";
            this.lblMarkupLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkStack
            // 
            this.chkStack.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkStack.AutoSize = true;
            this.chkStack.Checked = true;
            this.chkStack.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkStack.DefaultColorScheme = true;
            this.chkStack.Location = new System.Drawing.Point(86, 4);
            this.chkStack.Name = "chkStack";
            this.chkStack.Size = new System.Drawing.Size(54, 17);
            this.chkStack.TabIndex = 15;
            this.chkStack.Tag = "Label_SelectGear_Stack";
            this.chkStack.Text = "Stack";
            this.chkStack.UseVisualStyleBackColor = true;
            // 
            // lblTest
            // 
            this.lblTest.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblTest.AutoSize = true;
            this.lblTest.Location = new System.Drawing.Point(303, 108);
            this.lblTest.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTest.Name = "lblTest";
            this.lblTest.Size = new System.Drawing.Size(19, 13);
            this.lblTest.TabIndex = 7;
            this.lblTest.Text = "[0]";
            this.lblTest.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTestLabel
            // 
            this.lblTestLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblTestLabel.AutoSize = true;
            this.lblTestLabel.Location = new System.Drawing.Point(266, 108);
            this.lblTestLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTestLabel.Name = "lblTestLabel";
            this.lblTestLabel.Size = new System.Drawing.Size(31, 13);
            this.lblTestLabel.TabIndex = 6;
            this.lblTestLabel.Tag = "Label_Test";
            this.lblTestLabel.Text = "Test:";
            this.lblTestLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
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
            this.chkBlackMarketDiscount.TabIndex = 40;
            this.chkBlackMarketDiscount.Tag = "Checkbox_BlackMarketDiscount";
            this.chkBlackMarketDiscount.Text = "Black Market Discount (10%)";
            this.chkBlackMarketDiscount.UseVisualStyleBackColor = true;
            this.chkBlackMarketDiscount.Visible = false;
            this.chkBlackMarketDiscount.CheckedChanged += new System.EventHandler(this.chkBlackMarketDiscount_CheckedChanged);
            // 
            // chkHideOverAvailLimit
            // 
            this.chkHideOverAvailLimit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkHideOverAvailLimit.AutoSize = true;
            this.chkHideOverAvailLimit.DefaultColorScheme = true;
            this.chkHideOverAvailLimit.Location = new System.Drawing.Point(309, 468);
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
            this.chkShowOnlyAffordItems.Location = new System.Drawing.Point(309, 493);
            this.chkShowOnlyAffordItems.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkShowOnlyAffordItems.Name = "chkShowOnlyAffordItems";
            this.chkShowOnlyAffordItems.Size = new System.Drawing.Size(164, 17);
            this.chkShowOnlyAffordItems.TabIndex = 66;
            this.chkShowOnlyAffordItems.Tag = "Checkbox_ShowOnlyAffordItems";
            this.chkShowOnlyAffordItems.Text = "Show Only Items I Can Afford";
            this.chkShowOnlyAffordItems.UseVisualStyleBackColor = true;
            this.chkShowOnlyAffordItems.CheckedChanged += new System.EventHandler(this.RefreshCurrentList);
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tlpMain.Controls.Add(this.chkShowOnlyAffordItems, 1, 4);
            this.tlpMain.Controls.Add(this.chkHideOverAvailLimit, 1, 3);
            this.tlpMain.Controls.Add(this.tlpLeft, 0, 0);
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
            this.tlpMain.Size = new System.Drawing.Size(766, 543);
            this.tlpMain.TabIndex = 67;
            // 
            // tlpLeft
            // 
            this.tlpLeft.ColumnCount = 2;
            this.tlpLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpLeft.Controls.Add(this.lblCategory, 0, 0);
            this.tlpLeft.Controls.Add(this.cboCategory, 1, 0);
            this.tlpLeft.Controls.Add(this.lstGear, 0, 1);
            this.tlpLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpLeft.Location = new System.Drawing.Point(0, 0);
            this.tlpLeft.Margin = new System.Windows.Forms.Padding(0);
            this.tlpLeft.Name = "tlpLeft";
            this.tlpLeft.RowCount = 2;
            this.tlpMain.SetRowSpan(this.tlpLeft, 6);
            this.tlpLeft.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpLeft.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpLeft.Size = new System.Drawing.Size(306, 543);
            this.tlpLeft.TabIndex = 69;
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
            this.tlpButtons.Location = new System.Drawing.Point(532, 514);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButtons.Size = new System.Drawing.Size(234, 29);
            this.tlpButtons.TabIndex = 73;
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
            this.tlpRight.Controls.Add(this.lblGearDeviceRatingLabel, 2, 1);
            this.tlpRight.Controls.Add(this.lblSourceLabel, 0, 8);
            this.tlpRight.Controls.Add(this.lblGearDeviceRating, 3, 1);
            this.tlpRight.Controls.Add(this.lblGearQtyLabel, 0, 4);
            this.tlpRight.Controls.Add(this.flpCheckBoxes, 0, 7);
            this.tlpRight.Controls.Add(this.flpQty, 1, 4);
            this.tlpRight.Controls.Add(this.lblTest, 3, 5);
            this.tlpRight.Controls.Add(this.lblTestLabel, 2, 5);
            this.tlpRight.Controls.Add(this.lblAvailLabel, 0, 5);
            this.tlpRight.Controls.Add(this.lblRatingLabel, 0, 1);
            this.tlpRight.Controls.Add(this.lblAvail, 1, 5);
            this.tlpRight.Controls.Add(this.flpRating, 1, 1);
            this.tlpRight.Controls.Add(this.lblCapacityLabel, 0, 2);
            this.tlpRight.Controls.Add(this.lblCapacity, 1, 2);
            this.tlpRight.Controls.Add(this.lblMaximumCapacity, 0, 3);
            this.tlpRight.Controls.Add(this.lblCostLabel, 0, 6);
            this.tlpRight.Controls.Add(this.lblCost, 1, 6);
            this.tlpRight.Controls.Add(this.lblMarkupLabel, 2, 6);
            this.tlpRight.Controls.Add(this.flpMarkup, 3, 6);
            this.tlpRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRight.Location = new System.Drawing.Point(306, 26);
            this.tlpRight.Margin = new System.Windows.Forms.Padding(0);
            this.tlpRight.Name = "tlpRight";
            this.tlpRight.RowCount = 10;
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.Size = new System.Drawing.Size(460, 253);
            this.tlpRight.TabIndex = 74;
            // 
            // flpCheckBoxes
            // 
            this.flpCheckBoxes.AutoSize = true;
            this.flpCheckBoxes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.SetColumnSpan(this.flpCheckBoxes, 4);
            this.flpCheckBoxes.Controls.Add(this.chkFreeItem);
            this.flpCheckBoxes.Controls.Add(this.chkBlackMarketDiscount);
            this.flpCheckBoxes.Controls.Add(this.chkDoItYourself);
            this.flpCheckBoxes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpCheckBoxes.Location = new System.Drawing.Point(0, 153);
            this.flpCheckBoxes.Margin = new System.Windows.Forms.Padding(0);
            this.flpCheckBoxes.Name = "flpCheckBoxes";
            this.flpCheckBoxes.Size = new System.Drawing.Size(460, 25);
            this.flpCheckBoxes.TabIndex = 72;
            // 
            // flpQty
            // 
            this.flpQty.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpQty.AutoSize = true;
            this.flpQty.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.SetColumnSpan(this.flpQty, 3);
            this.flpQty.Controls.Add(this.nudGearQty);
            this.flpQty.Controls.Add(this.chkStack);
            this.flpQty.Location = new System.Drawing.Point(57, 76);
            this.flpQty.Margin = new System.Windows.Forms.Padding(0);
            this.flpQty.Name = "flpQty";
            this.flpQty.Size = new System.Drawing.Size(143, 26);
            this.flpQty.TabIndex = 70;
            this.flpQty.WrapContents = false;
            // 
            // flpRating
            // 
            this.flpRating.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpRating.AutoSize = true;
            this.flpRating.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpRating.Controls.Add(this.nudRating);
            this.flpRating.Controls.Add(this.lblRatingNALabel);
            this.flpRating.Location = new System.Drawing.Point(57, 0);
            this.flpRating.Margin = new System.Windows.Forms.Padding(0);
            this.flpRating.Name = "flpRating";
            this.flpRating.Size = new System.Drawing.Size(98, 26);
            this.flpRating.TabIndex = 67;
            this.flpRating.WrapContents = false;
            // 
            // lblRatingNALabel
            // 
            this.lblRatingNALabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblRatingNALabel.AutoSize = true;
            this.lblRatingNALabel.Location = new System.Drawing.Point(68, 6);
            this.lblRatingNALabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRatingNALabel.Name = "lblRatingNALabel";
            this.lblRatingNALabel.Size = new System.Drawing.Size(27, 13);
            this.lblRatingNALabel.TabIndex = 12;
            this.lblRatingNALabel.Tag = "String_NotApplicable";
            this.lblRatingNALabel.Text = "N/A";
            this.lblRatingNALabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblRatingNALabel.Visible = false;
            // 
            // lblCapacityLabel
            // 
            this.lblCapacityLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCapacityLabel.AutoSize = true;
            this.lblCapacityLabel.Location = new System.Drawing.Point(3, 32);
            this.lblCapacityLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCapacityLabel.Name = "lblCapacityLabel";
            this.lblCapacityLabel.Size = new System.Drawing.Size(51, 13);
            this.lblCapacityLabel.TabIndex = 2;
            this.lblCapacityLabel.Tag = "Label_Capacity";
            this.lblCapacityLabel.Text = "Capacity:";
            this.lblCapacityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // flpMarkup
            // 
            this.flpMarkup.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpMarkup.AutoSize = true;
            this.flpMarkup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpMarkup.Controls.Add(this.nudMarkup);
            this.flpMarkup.Controls.Add(this.lblMarkupPercentLabel);
            this.flpMarkup.Location = new System.Drawing.Point(300, 127);
            this.flpMarkup.Margin = new System.Windows.Forms.Padding(0);
            this.flpMarkup.Name = "flpMarkup";
            this.flpMarkup.Size = new System.Drawing.Size(83, 26);
            this.flpMarkup.TabIndex = 71;
            // 
            // lblMarkupPercentLabel
            // 
            this.lblMarkupPercentLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMarkupPercentLabel.AutoSize = true;
            this.lblMarkupPercentLabel.Location = new System.Drawing.Point(65, 6);
            this.lblMarkupPercentLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMarkupPercentLabel.Name = "lblMarkupPercentLabel";
            this.lblMarkupPercentLabel.Size = new System.Drawing.Size(15, 13);
            this.lblMarkupPercentLabel.TabIndex = 21;
            this.lblMarkupPercentLabel.Text = "%";
            this.lblMarkupPercentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.tlpTopRight.Location = new System.Drawing.Point(306, 0);
            this.tlpTopRight.Margin = new System.Windows.Forms.Padding(0);
            this.tlpTopRight.Name = "tlpTopRight";
            this.tlpTopRight.RowCount = 1;
            this.tlpTopRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTopRight.Size = new System.Drawing.Size(460, 26);
            this.tlpTopRight.TabIndex = 75;
            // 
            // frmSelectGear
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectGear";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectGear";
            this.Text = "Select Gear";
            this.Load += new System.EventHandler(this.SelectGear_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudRating)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGearQty)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpLeft.ResumeLayout(false);
            this.tlpLeft.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.tlpRight.ResumeLayout(false);
            this.tlpRight.PerformLayout();
            this.flpCheckBoxes.ResumeLayout(false);
            this.flpCheckBoxes.PerformLayout();
            this.flpQty.ResumeLayout(false);
            this.flpQty.PerformLayout();
            this.flpRating.ResumeLayout(false);
            this.flpRating.PerformLayout();
            this.flpMarkup.ResumeLayout(false);
            this.flpMarkup.PerformLayout();
            this.tlpTopRight.ResumeLayout(false);
            this.tlpTopRight.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.ListBox lstGear;
        private System.Windows.Forms.Label lblCategory;
        private ElasticComboBox cboCategory;
        private Chummer.NumericUpDownEx nudRating;
        private System.Windows.Forms.Label lblRatingLabel;
        private System.Windows.Forms.Label lblCost;
        private System.Windows.Forms.Label lblCostLabel;
        private System.Windows.Forms.Label lblAvail;
        private System.Windows.Forms.Label lblAvailLabel;
        private System.Windows.Forms.Label lblMaximumCapacity;
        private System.Windows.Forms.Label lblSearchLabel;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button cmdOKAdd;
        private System.Windows.Forms.Label lblGearDeviceRating;
        private System.Windows.Forms.Label lblGearDeviceRatingLabel;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblSourceLabel;
        private System.Windows.Forms.Label lblCapacity;
        private Chummer.NumericUpDownEx nudGearQty;
        private System.Windows.Forms.Label lblGearQtyLabel;
        private Chummer.ColorableCheckBox chkFreeItem;
        private Chummer.ColorableCheckBox chkDoItYourself;
        private Chummer.NumericUpDownEx nudMarkup;
        private System.Windows.Forms.Label lblMarkupLabel;
        private Chummer.ColorableCheckBox chkStack;
        private System.Windows.Forms.Label lblTest;
        private System.Windows.Forms.Label lblTestLabel;
        private Chummer.ColorableCheckBox chkBlackMarketDiscount;
        private Chummer.ColorableCheckBox chkHideOverAvailLimit;
        private Chummer.ColorableCheckBox chkShowOnlyAffordItems;
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.Label lblCapacityLabel;
        private System.Windows.Forms.Label lblMarkupPercentLabel;
        private System.Windows.Forms.FlowLayoutPanel flpRating;
        private System.Windows.Forms.Label lblRatingNALabel;
        private System.Windows.Forms.FlowLayoutPanel flpQty;
        private System.Windows.Forms.FlowLayoutPanel flpMarkup;
        private System.Windows.Forms.FlowLayoutPanel flpCheckBoxes;
        private BufferedTableLayoutPanel tlpButtons;
        private BufferedTableLayoutPanel tlpRight;
        private BufferedTableLayoutPanel tlpLeft;
        private BufferedTableLayoutPanel tlpTopRight;
    }
}
