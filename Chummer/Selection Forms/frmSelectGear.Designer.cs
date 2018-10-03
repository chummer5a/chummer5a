namespace Chummer
{
    partial class frmSelectGear
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
            this.lstGear = new System.Windows.Forms.ListBox();
            this.lblCategory = new System.Windows.Forms.Label();
            this.cboCategory = new Chummer.ElasticComboBox();
            this.nudRating = new System.Windows.Forms.NumericUpDown();
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
            this.nudGearQty = new System.Windows.Forms.NumericUpDown();
            this.lblGearQtyLabel = new System.Windows.Forms.Label();
            this.chkFreeItem = new System.Windows.Forms.CheckBox();
            this.chkDoItYourself = new System.Windows.Forms.CheckBox();
            this.nudMarkup = new System.Windows.Forms.NumericUpDown();
            this.lblMarkupLabel = new System.Windows.Forms.Label();
            this.chkStack = new System.Windows.Forms.CheckBox();
            this.lblTest = new System.Windows.Forms.Label();
            this.lblTestLabel = new System.Windows.Forms.Label();
            this.chkBlackMarketDiscount = new System.Windows.Forms.CheckBox();
            this.chkHideOverAvailLimit = new System.Windows.Forms.CheckBox();
            this.chkShowOnlyAffordItems = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel();
            this.lblMarkupPercentLabel = new System.Windows.Forms.Label();
            this.lblCapacityLabel = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.lblRatingNALabel = new System.Windows.Forms.Label();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.nudRating)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGearQty)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(0, 0);
            this.cmdCancel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 38;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.Location = new System.Drawing.Point(162, 0);
            this.cmdOK.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 36;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lstGear
            // 
            this.lstGear.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.lstGear, 2);
            this.lstGear.FormattingEnabled = true;
            this.lstGear.Location = new System.Drawing.Point(3, 30);
            this.lstGear.Name = "lstGear";
            this.tableLayoutPanel1.SetRowSpan(this.lstGear, 14);
            this.lstGear.Size = new System.Drawing.Size(294, 381);
            this.lstGear.TabIndex = 35;
            this.lstGear.SelectedIndexChanged += new System.EventHandler(this.lstGear_SelectedIndexChanged);
            this.lstGear.DoubleClick += new System.EventHandler(this.lstGear_DoubleClick);
            // 
            // lblCategory
            // 
            this.lblCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(17, 6);
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
            this.cboCategory.Location = new System.Drawing.Point(75, 3);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(222, 21);
            this.cboCategory.TabIndex = 34;
            this.cboCategory.TooltipText = "";
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // nudRating
            // 
            this.nudRating.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudRating.Enabled = false;
            this.nudRating.Location = new System.Drawing.Point(3, 3);
            this.nudRating.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.nudRating.Name = "nudRating";
            this.nudRating.Size = new System.Drawing.Size(156, 20);
            this.nudRating.TabIndex = 11;
            this.nudRating.ValueChanged += new System.EventHandler(this.nudRating_ValueChanged);
            // 
            // lblRatingLabel
            // 
            this.lblRatingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRatingLabel.AutoSize = true;
            this.lblRatingLabel.Location = new System.Drawing.Point(316, 108);
            this.lblRatingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRatingLabel.Name = "lblRatingLabel";
            this.lblRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblRatingLabel.TabIndex = 10;
            this.lblRatingLabel.Tag = "Label_Rating";
            this.lblRatingLabel.Text = "Rating:";
            // 
            // lblCost
            // 
            this.lblCost.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblCost, 4);
            this.lblCost.Location = new System.Drawing.Point(363, 83);
            this.lblCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCost.Name = "lblCost";
            this.lblCost.Size = new System.Drawing.Size(19, 13);
            this.lblCost.TabIndex = 9;
            this.lblCost.Text = "[0]";
            // 
            // lblCostLabel
            // 
            this.lblCostLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCostLabel.AutoSize = true;
            this.lblCostLabel.Location = new System.Drawing.Point(326, 83);
            this.lblCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCostLabel.Name = "lblCostLabel";
            this.lblCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblCostLabel.TabIndex = 8;
            this.lblCostLabel.Tag = "Label_Cost";
            this.lblCostLabel.Text = "Cost:";
            // 
            // lblAvail
            // 
            this.lblAvail.AutoSize = true;
            this.lblAvail.Location = new System.Drawing.Point(363, 58);
            this.lblAvail.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAvail.Name = "lblAvail";
            this.lblAvail.Size = new System.Drawing.Size(19, 13);
            this.lblAvail.TabIndex = 5;
            this.lblAvail.Text = "[0]";
            // 
            // lblAvailLabel
            // 
            this.lblAvailLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAvailLabel.AutoSize = true;
            this.lblAvailLabel.Location = new System.Drawing.Point(324, 58);
            this.lblAvailLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAvailLabel.Name = "lblAvailLabel";
            this.lblAvailLabel.Size = new System.Drawing.Size(33, 13);
            this.lblAvailLabel.TabIndex = 4;
            this.lblAvailLabel.Tag = "Label_Avail";
            this.lblAvailLabel.Text = "Avail:";
            // 
            // lblMaximumCapacity
            // 
            this.lblMaximumCapacity.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblMaximumCapacity, 3);
            this.lblMaximumCapacity.Location = new System.Drawing.Point(303, 274);
            this.lblMaximumCapacity.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMaximumCapacity.Name = "lblMaximumCapacity";
            this.lblMaximumCapacity.Size = new System.Drawing.Size(101, 13);
            this.lblMaximumCapacity.TabIndex = 30;
            this.lblMaximumCapacity.Text = "[Maximum Capacity]";
            // 
            // lblSearchLabel
            // 
            this.lblSearchLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSearchLabel.AutoSize = true;
            this.lblSearchLabel.Location = new System.Drawing.Point(313, 6);
            this.lblSearchLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSearchLabel.Name = "lblSearchLabel";
            this.lblSearchLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSearchLabel.TabIndex = 0;
            this.lblSearchLabel.Tag = "Label_Search";
            this.lblSearchLabel.Text = "&Search:";
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.txtSearch, 4);
            this.txtSearch.Location = new System.Drawing.Point(363, 3);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(234, 20);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
            this.txtSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyUp);
            // 
            // cmdOKAdd
            // 
            this.cmdOKAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOKAdd.Location = new System.Drawing.Point(81, 0);
            this.cmdOKAdd.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(75, 23);
            this.cmdOKAdd.TabIndex = 37;
            this.cmdOKAdd.Tag = "String_AddMore";
            this.cmdOKAdd.Text = "&Add && More";
            this.cmdOKAdd.UseVisualStyleBackColor = true;
            this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
            // 
            // lblGearDeviceRating
            // 
            this.lblGearDeviceRating.AutoSize = true;
            this.lblGearDeviceRating.Location = new System.Drawing.Point(363, 236);
            this.lblGearDeviceRating.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearDeviceRating.Name = "lblGearDeviceRating";
            this.lblGearDeviceRating.Size = new System.Drawing.Size(19, 13);
            this.lblGearDeviceRating.TabIndex = 23;
            this.lblGearDeviceRating.Text = "[0]";
            // 
            // lblGearDeviceRatingLabel
            // 
            this.lblGearDeviceRatingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGearDeviceRatingLabel.AutoSize = true;
            this.lblGearDeviceRatingLabel.Location = new System.Drawing.Point(313, 236);
            this.lblGearDeviceRatingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearDeviceRatingLabel.Name = "lblGearDeviceRatingLabel";
            this.lblGearDeviceRatingLabel.Size = new System.Drawing.Size(44, 26);
            this.lblGearDeviceRatingLabel.TabIndex = 22;
            this.lblGearDeviceRatingLabel.Tag = "Label_DeviceRating";
            this.lblGearDeviceRatingLabel.Text = "Device Rating:";
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblSource, 2);
            this.lblSource.Location = new System.Drawing.Point(363, 299);
            this.lblSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 32;
            this.lblSource.Text = "[Source]";
            this.lblSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(313, 299);
            this.lblSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 31;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            // 
            // lblCapacity
            // 
            this.lblCapacity.AutoSize = true;
            this.lblCapacity.Location = new System.Drawing.Point(363, 33);
            this.lblCapacity.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCapacity.Name = "lblCapacity";
            this.lblCapacity.Size = new System.Drawing.Size(19, 13);
            this.lblCapacity.TabIndex = 3;
            this.lblCapacity.Text = "[0]";
            // 
            // nudGearQty
            // 
            this.nudGearQty.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.nudGearQty, 2);
            this.nudGearQty.Location = new System.Drawing.Point(363, 131);
            this.nudGearQty.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudGearQty.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudGearQty.Name = "nudGearQty";
            this.nudGearQty.Size = new System.Drawing.Size(156, 20);
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
            this.lblGearQtyLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGearQtyLabel.AutoSize = true;
            this.lblGearQtyLabel.Location = new System.Drawing.Point(331, 134);
            this.lblGearQtyLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearQtyLabel.Name = "lblGearQtyLabel";
            this.lblGearQtyLabel.Size = new System.Drawing.Size(26, 13);
            this.lblGearQtyLabel.TabIndex = 13;
            this.lblGearQtyLabel.Tag = "Label_Qty";
            this.lblGearQtyLabel.Text = "Qty:";
            // 
            // chkFreeItem
            // 
            this.chkFreeItem.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkFreeItem.AutoSize = true;
            this.chkFreeItem.Location = new System.Drawing.Point(303, 158);
            this.chkFreeItem.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkFreeItem.Name = "chkFreeItem";
            this.chkFreeItem.Size = new System.Drawing.Size(54, 17);
            this.chkFreeItem.TabIndex = 16;
            this.chkFreeItem.Tag = "Checkbox_Free";
            this.chkFreeItem.Text = "Free!";
            this.chkFreeItem.UseVisualStyleBackColor = true;
            this.chkFreeItem.CheckedChanged += new System.EventHandler(this.chkFreeItem_CheckedChanged);
            // 
            // chkDoItYourself
            // 
            this.chkDoItYourself.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkDoItYourself.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkDoItYourself, 4);
            this.chkDoItYourself.Location = new System.Drawing.Point(363, 183);
            this.chkDoItYourself.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkDoItYourself.Name = "chkDoItYourself";
            this.chkDoItYourself.Size = new System.Drawing.Size(234, 17);
            this.chkDoItYourself.TabIndex = 17;
            this.chkDoItYourself.Tag = "Label_SelectGear_DoItYourself";
            this.chkDoItYourself.Text = "Do It Yourself";
            this.chkDoItYourself.UseVisualStyleBackColor = true;
            this.chkDoItYourself.Visible = false;
            this.chkDoItYourself.CheckedChanged += new System.EventHandler(this.chkDoItYourself_CheckedChanged);
            // 
            // nudMarkup
            // 
            this.nudMarkup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.nudMarkup, 2);
            this.nudMarkup.DecimalPlaces = 2;
            this.nudMarkup.Location = new System.Drawing.Point(363, 207);
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
            this.nudMarkup.Size = new System.Drawing.Size(156, 20);
            this.nudMarkup.TabIndex = 20;
            this.nudMarkup.ValueChanged += new System.EventHandler(this.nudMarkup_ValueChanged);
            // 
            // lblMarkupLabel
            // 
            this.lblMarkupLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMarkupLabel.AutoSize = true;
            this.lblMarkupLabel.Location = new System.Drawing.Point(311, 210);
            this.lblMarkupLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMarkupLabel.Name = "lblMarkupLabel";
            this.lblMarkupLabel.Size = new System.Drawing.Size(46, 13);
            this.lblMarkupLabel.TabIndex = 19;
            this.lblMarkupLabel.Tag = "Label_SelectGear_Markup";
            this.lblMarkupLabel.Text = "Markup:";
            // 
            // chkStack
            // 
            this.chkStack.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkStack.AutoSize = true;
            this.chkStack.Checked = true;
            this.chkStack.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tableLayoutPanel1.SetColumnSpan(this.chkStack, 2);
            this.chkStack.Location = new System.Drawing.Point(525, 132);
            this.chkStack.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkStack.Name = "chkStack";
            this.chkStack.Size = new System.Drawing.Size(72, 18);
            this.chkStack.TabIndex = 15;
            this.chkStack.Tag = "Label_SelectGear_Stack";
            this.chkStack.Text = "Stack";
            this.chkStack.UseVisualStyleBackColor = true;
            // 
            // lblTest
            // 
            this.lblTest.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblTest, 2);
            this.lblTest.Location = new System.Drawing.Point(525, 58);
            this.lblTest.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTest.Name = "lblTest";
            this.lblTest.Size = new System.Drawing.Size(19, 13);
            this.lblTest.TabIndex = 7;
            this.lblTest.Text = "[0]";
            // 
            // lblTestLabel
            // 
            this.lblTestLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTestLabel.AutoSize = true;
            this.lblTestLabel.Location = new System.Drawing.Point(488, 58);
            this.lblTestLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTestLabel.Name = "lblTestLabel";
            this.lblTestLabel.Size = new System.Drawing.Size(31, 13);
            this.lblTestLabel.TabIndex = 6;
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
            this.chkBlackMarketDiscount.Location = new System.Drawing.Point(363, 157);
            this.chkBlackMarketDiscount.Name = "chkBlackMarketDiscount";
            this.chkBlackMarketDiscount.Size = new System.Drawing.Size(234, 19);
            this.chkBlackMarketDiscount.TabIndex = 40;
            this.chkBlackMarketDiscount.Tag = "Checkbox_BlackMarketDiscount";
            this.chkBlackMarketDiscount.Text = "Black Market Discount (10%)";
            this.chkBlackMarketDiscount.UseVisualStyleBackColor = true;
            this.chkBlackMarketDiscount.Visible = false;
            this.chkBlackMarketDiscount.CheckedChanged += new System.EventHandler(this.chkBlackMarketDiscount_CheckedChanged);
            // 
            // chkHideOverAvailLimit
            // 
            this.chkHideOverAvailLimit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkHideOverAvailLimit.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkHideOverAvailLimit, 5);
            this.chkHideOverAvailLimit.Location = new System.Drawing.Point(303, 322);
            this.chkHideOverAvailLimit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkHideOverAvailLimit.Name = "chkHideOverAvailLimit";
            this.chkHideOverAvailLimit.Size = new System.Drawing.Size(294, 17);
            this.chkHideOverAvailLimit.TabIndex = 65;
            this.chkHideOverAvailLimit.Tag = "Checkbox_HideOverAvailLimit";
            this.chkHideOverAvailLimit.Text = "Hide Items Over Avail Limit ({0})";
            this.chkHideOverAvailLimit.UseVisualStyleBackColor = true;
            this.chkHideOverAvailLimit.CheckedChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // chkShowOnlyAffordItems
            // 
            this.chkShowOnlyAffordItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkShowOnlyAffordItems.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkShowOnlyAffordItems, 5);
            this.chkShowOnlyAffordItems.Location = new System.Drawing.Point(303, 347);
            this.chkShowOnlyAffordItems.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkShowOnlyAffordItems.Name = "chkShowOnlyAffordItems";
            this.chkShowOnlyAffordItems.Size = new System.Drawing.Size(294, 17);
            this.chkShowOnlyAffordItems.TabIndex = 66;
            this.chkShowOnlyAffordItems.Tag = "Checkbox_ShowOnlyAffordItems";
            this.chkShowOnlyAffordItems.Text = "Show Only Items I Can Afford";
            this.chkShowOnlyAffordItems.UseVisualStyleBackColor = true;
            this.chkShowOnlyAffordItems.CheckedChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 7;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 13F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.Controls.Add(this.lstGear, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtSearch, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblCategory, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.chkShowOnlyAffordItems, 2, 13);
            this.tableLayoutPanel1.Controls.Add(this.cboCategory, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblSearchLabel, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblMarkupPercentLabel, 5, 8);
            this.tableLayoutPanel1.Controls.Add(this.lblCapacity, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.chkHideOverAvailLimit, 2, 12);
            this.tableLayoutPanel1.Controls.Add(this.lblCapacityLabel, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblSource, 3, 11);
            this.tableLayoutPanel1.Controls.Add(this.lblSourceLabel, 2, 11);
            this.tableLayoutPanel1.Controls.Add(this.lblMarkupLabel, 2, 8);
            this.tableLayoutPanel1.Controls.Add(this.lblAvailLabel, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblGearDeviceRatingLabel, 2, 9);
            this.tableLayoutPanel1.Controls.Add(this.lblMaximumCapacity, 2, 10);
            this.tableLayoutPanel1.Controls.Add(this.lblAvail, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblTestLabel, 4, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblCostLabel, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.nudGearQty, 3, 5);
            this.tableLayoutPanel1.Controls.Add(this.lblRatingLabel, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblGearQtyLabel, 2, 5);
            this.tableLayoutPanel1.Controls.Add(this.chkStack, 5, 5);
            this.tableLayoutPanel1.Controls.Add(this.lblTest, 5, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblGearDeviceRating, 3, 9);
            this.tableLayoutPanel1.Controls.Add(this.nudMarkup, 3, 8);
            this.tableLayoutPanel1.Controls.Add(this.lblCost, 3, 3);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 3, 4);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 2, 14);
            this.tableLayoutPanel1.Controls.Add(this.chkFreeItem, 2, 6);
            this.tableLayoutPanel1.Controls.Add(this.chkBlackMarketDiscount, 3, 6);
            this.tableLayoutPanel1.Controls.Add(this.chkDoItYourself, 3, 7);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 15;
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
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(600, 417);
            this.tableLayoutPanel1.TabIndex = 67;
            // 
            // lblMarkupPercentLabel
            // 
            this.lblMarkupPercentLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblMarkupPercentLabel.AutoSize = true;
            this.lblMarkupPercentLabel.Location = new System.Drawing.Point(522, 210);
            this.lblMarkupPercentLabel.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
            this.lblMarkupPercentLabel.Name = "lblMarkupPercentLabel";
            this.lblMarkupPercentLabel.Size = new System.Drawing.Size(15, 14);
            this.lblMarkupPercentLabel.TabIndex = 21;
            this.lblMarkupPercentLabel.Text = "%";
            // 
            // lblCapacityLabel
            // 
            this.lblCapacityLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCapacityLabel.AutoSize = true;
            this.lblCapacityLabel.Location = new System.Drawing.Point(306, 33);
            this.lblCapacityLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCapacityLabel.Name = "lblCapacityLabel";
            this.lblCapacityLabel.Size = new System.Drawing.Size(51, 13);
            this.lblCapacityLabel.TabIndex = 2;
            this.lblCapacityLabel.Tag = "Label_Capacity";
            this.lblCapacityLabel.Text = "Capacity:";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 2);
            this.flowLayoutPanel1.Controls.Add(this.nudRating);
            this.flowLayoutPanel1.Controls.Add(this.lblRatingNALabel);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(360, 102);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(162, 26);
            this.flowLayoutPanel1.TabIndex = 67;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // lblRatingNALabel
            // 
            this.lblRatingNALabel.AutoSize = true;
            this.lblRatingNALabel.Location = new System.Drawing.Point(165, 6);
            this.lblRatingNALabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 7);
            this.lblRatingNALabel.Name = "lblRatingNALabel";
            this.lblRatingNALabel.Size = new System.Drawing.Size(27, 13);
            this.lblRatingNALabel.TabIndex = 12;
            this.lblRatingNALabel.Tag = "String_NotApplicable";
            this.lblRatingNALabel.Text = "N/A";
            this.lblRatingNALabel.Visible = false;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel2, 5);
            this.flowLayoutPanel2.Controls.Add(this.cmdOK);
            this.flowLayoutPanel2.Controls.Add(this.cmdOKAdd);
            this.flowLayoutPanel2.Controls.Add(this.cmdCancel);
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(363, 394);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(237, 23);
            this.flowLayoutPanel2.TabIndex = 68;
            // 
            // frmSelectGear
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectGear";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectGear";
            this.Text = "Select Gear";
            this.Load += new System.EventHandler(this.frmSelectGear_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudRating)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGearQty)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.ListBox lstGear;
        private System.Windows.Forms.Label lblCategory;
        private ElasticComboBox cboCategory;
        private System.Windows.Forms.NumericUpDown nudRating;
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
        private System.Windows.Forms.NumericUpDown nudGearQty;
        private System.Windows.Forms.Label lblGearQtyLabel;
        private System.Windows.Forms.CheckBox chkFreeItem;
        private System.Windows.Forms.CheckBox chkDoItYourself;
        private System.Windows.Forms.NumericUpDown nudMarkup;
        private System.Windows.Forms.Label lblMarkupLabel;
        private System.Windows.Forms.CheckBox chkStack;
        private System.Windows.Forms.Label lblTest;
        private System.Windows.Forms.Label lblTestLabel;
        private System.Windows.Forms.CheckBox chkBlackMarketDiscount;
        private System.Windows.Forms.CheckBox chkHideOverAvailLimit;
        private System.Windows.Forms.CheckBox chkShowOnlyAffordItems;
        private Chummer.BufferedTableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblCapacityLabel;
        private System.Windows.Forms.Label lblMarkupPercentLabel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label lblRatingNALabel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
    }
}
