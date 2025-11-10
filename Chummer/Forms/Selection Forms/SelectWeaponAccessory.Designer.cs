namespace Chummer
{
    partial class SelectWeaponAccessory
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
                Utils.StringHashSetPool.Return(ref _setBlackMarketMaps);
                if (components != null)
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
            this.chkFreeItem = new Chummer.ColorableCheckBox();
            this.nudMarkup = new Chummer.NumericUpDownEx();
            this.lblMarkupLabel = new System.Windows.Forms.Label();
            this.lblMarkupPercentLabel = new System.Windows.Forms.Label();
            this.lblTest = new System.Windows.Forms.Label();
            this.lblTestLabel = new System.Windows.Forms.Label();
            this.nudRating = new Chummer.NumericUpDownEx();
            this.lblRatingLabel = new System.Windows.Forms.Label();
            this.cboMount = new Chummer.ElasticComboBox();
            this.chkBlackMarketDiscount = new Chummer.ColorableCheckBox();
            this.cboExtraMount = new Chummer.ElasticComboBox();
            this.lblExtraMountLabel = new System.Windows.Forms.Label();
            this.chkHideOverAvailLimit = new Chummer.ColorableCheckBox();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.chkShowOnlyAffordItems = new Chummer.ColorableCheckBox();
            this.gpbCostFilter = new System.Windows.Forms.GroupBox();
            this.tlpCostFilter = new System.Windows.Forms.TableLayoutPanel();
            this.lblMinimumCost = new System.Windows.Forms.Label();
            this.lblMaximumCost = new System.Windows.Forms.Label();
            this.lblExactCost = new System.Windows.Forms.Label();
            this.nudMinimumCost = new Chummer.NumericUpDownEx();
            this.nudMaximumCost = new Chummer.NumericUpDownEx();
            this.nudExactCost = new Chummer.NumericUpDownEx();
            this.tlpButtons = new System.Windows.Forms.TableLayoutPanel();
            this.tlpRight = new System.Windows.Forms.TableLayoutPanel();
            this.flpCheckBoxes = new System.Windows.Forms.FlowLayoutPanel();
            this.flpMarkup = new System.Windows.Forms.FlowLayoutPanel();
            this.flpRating = new System.Windows.Forms.FlowLayoutPanel();
            this.lblRatingNALabel = new System.Windows.Forms.Label();
            this.tlpTopRight = new System.Windows.Forms.TableLayoutPanel();
            this.lblSearchLabel = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.gpbShoppingCart = new System.Windows.Forms.GroupBox();
            this.tlpShoppingCart = new System.Windows.Forms.TableLayoutPanel();
            this.lstShoppingCart = new System.Windows.Forms.ListBox();
            this.cmdAddToCart = new System.Windows.Forms.Button();
            this.cmdRemoveFromCart = new System.Windows.Forms.Button();
            this.cmdPurchaseAll = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRating)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            this.tlpRight.SuspendLayout();
            this.flpCheckBoxes.SuspendLayout();
            this.flpMarkup.SuspendLayout();
            this.flpRating.SuspendLayout();
            this.tlpTopRight.SuspendLayout();
            this.gpbShoppingCart.SuspendLayout();
            this.tlpShoppingCart.SuspendLayout();
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
            this.cmdCancel.MinimumSize = new System.Drawing.Size(80, 0);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(80, 23);
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
            this.cmdOK.Location = new System.Drawing.Point(175, 3);
            this.cmdOK.MinimumSize = new System.Drawing.Size(80, 0);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(80, 23);
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
            this.lstAccessory.DoubleClick += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdOKAdd
            // 
            this.cmdOKAdd.AutoSize = true;
            this.cmdOKAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOKAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOKAdd.Location = new System.Drawing.Point(89, 3);
            this.cmdOKAdd.MinimumSize = new System.Drawing.Size(80, 0);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(80, 23);
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
            this.lblSource.Location = new System.Drawing.Point(76, 284);
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
            this.lblSourceLabel.Location = new System.Drawing.Point(26, 284);
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
            this.tlpMain.Controls.Add(this.gpbCostFilter, 1, 5);
            this.tlpMain.Controls.Add(this.tlpButtons, 1, 6);
            this.tlpMain.Controls.Add(this.tlpRight, 1, 1);
            this.tlpMain.Controls.Add(this.tlpTopRight, 1, 0);
            this.tlpMain.Controls.Add(this.gpbShoppingCart, 1, 2);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 7;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
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
            // gpbCostFilter
            // 
            this.gpbCostFilter.AutoSize = true;
            this.gpbCostFilter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbCostFilter.Controls.Add(this.tlpCostFilter);
            this.gpbCostFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpbCostFilter.Location = new System.Drawing.Point(309, 350);
            this.gpbCostFilter.Name = "gpbCostFilter";
            this.gpbCostFilter.Size = new System.Drawing.Size(200, 100);
            this.gpbCostFilter.TabIndex = 72;
            this.gpbCostFilter.TabStop = false;
            this.gpbCostFilter.Tag = "Label_FilterByCost";
            this.gpbCostFilter.Text = "Filter by Cost (¥)";
            // 
            // tlpCostFilter
            // 
            this.tlpCostFilter.AutoSize = true;
            this.tlpCostFilter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpCostFilter.ColumnCount = 2;
            this.tlpCostFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCostFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCostFilter.Controls.Add(this.lblMinimumCost, 0, 0);
            this.tlpCostFilter.Controls.Add(this.lblMaximumCost, 0, 1);
            this.tlpCostFilter.Controls.Add(this.lblExactCost, 0, 2);
            this.tlpCostFilter.Controls.Add(this.nudMinimumCost, 1, 0);
            this.tlpCostFilter.Controls.Add(this.nudMaximumCost, 1, 1);
            this.tlpCostFilter.Controls.Add(this.nudExactCost, 1, 2);
            this.tlpCostFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCostFilter.Location = new System.Drawing.Point(3, 16);
            this.tlpCostFilter.Name = "tlpCostFilter";
            this.tlpCostFilter.RowCount = 3;
            this.tlpCostFilter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCostFilter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCostFilter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCostFilter.Size = new System.Drawing.Size(194, 81);
            this.tlpCostFilter.TabIndex = 0;
            // 
            // lblMinimumCost
            // 
            this.lblMinimumCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMinimumCost.AutoSize = true;
            this.lblMinimumCost.Location = new System.Drawing.Point(3, 6);
            this.lblMinimumCost.Name = "lblMinimumCost";
            this.lblMinimumCost.Size = new System.Drawing.Size(51, 13);
            this.lblMinimumCost.TabIndex = 0;
            this.lblMinimumCost.Tag = "Label_Minimum";
            this.lblMinimumCost.Text = "Minimum:";
            // 
            // lblMaximumCost
            // 
            this.lblMaximumCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMaximumCost.AutoSize = true;
            this.lblMaximumCost.Location = new System.Drawing.Point(3, 33);
            this.lblMaximumCost.Name = "lblMaximumCost";
            this.lblMaximumCost.Size = new System.Drawing.Size(54, 13);
            this.lblMaximumCost.TabIndex = 1;
            this.lblMaximumCost.Tag = "Label_Maximum";
            this.lblMaximumCost.Text = "Maximum:";
            // 
            // lblExactCost
            // 
            this.lblExactCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblExactCost.AutoSize = true;
            this.lblExactCost.Location = new System.Drawing.Point(3, 60);
            this.lblExactCost.Name = "lblExactCost";
            this.lblExactCost.Size = new System.Drawing.Size(35, 13);
            this.lblExactCost.TabIndex = 2;
            this.lblExactCost.Tag = "Label_Exact";
            this.lblExactCost.Text = "Exact:";
            // 
            // nudMinimumCost
            // 
            this.nudMinimumCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudMinimumCost.AutoSize = true;
            this.nudMinimumCost.DecimalPlaces = 2;
            this.nudMinimumCost.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudMinimumCost.Location = new System.Drawing.Point(60, 3);
            this.nudMinimumCost.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.nudMinimumCost.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMinimumCost.Name = "nudMinimumCost";
            this.nudMinimumCost.Size = new System.Drawing.Size(60, 20);
            this.nudMinimumCost.TabIndex = 3;
            this.nudMinimumCost.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMinimumCost.ValueChanged += new System.EventHandler(this.CostFilter);
            // 
            // nudMaximumCost
            // 
            this.nudMaximumCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudMaximumCost.AutoSize = true;
            this.nudMaximumCost.DecimalPlaces = 2;
            this.nudMaximumCost.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudMaximumCost.Location = new System.Drawing.Point(60, 30);
            this.nudMaximumCost.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.nudMaximumCost.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMaximumCost.Name = "nudMaximumCost";
            this.nudMaximumCost.Size = new System.Drawing.Size(60, 20);
            this.nudMaximumCost.TabIndex = 4;
            this.nudMaximumCost.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMaximumCost.ValueChanged += new System.EventHandler(this.CostFilter);
            // 
            // nudExactCost
            // 
            this.nudExactCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudExactCost.AutoSize = true;
            this.nudExactCost.DecimalPlaces = 2;
            this.nudExactCost.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudExactCost.Location = new System.Drawing.Point(60, 57);
            this.nudExactCost.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.nudExactCost.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudExactCost.Name = "nudExactCost";
            this.nudExactCost.Size = new System.Drawing.Size(60, 20);
            this.nudExactCost.TabIndex = 5;
            this.nudExactCost.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudExactCost.ValueChanged += new System.EventHandler(this.CostFilter);
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
            this.tlpButtons.Location = new System.Drawing.Point(348, 394);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButtons.Size = new System.Drawing.Size(258, 29);
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
            this.tlpRight.Size = new System.Drawing.Size(303, 400);
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
            // gpbShoppingCart
            // 
            this.gpbShoppingCart.AutoSize = true;
            this.gpbShoppingCart.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbShoppingCart.Controls.Add(this.tlpShoppingCart);
            this.gpbShoppingCart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpbShoppingCart.Location = new System.Drawing.Point(309, 26);
            this.gpbShoppingCart.Name = "gpbShoppingCart";
            this.gpbShoppingCart.Size = new System.Drawing.Size(294, 150);
            this.gpbShoppingCart.TabIndex = 80;
            this.gpbShoppingCart.TabStop = false;
            this.gpbShoppingCart.Tag = "Label_ShoppingCart";
            this.gpbShoppingCart.Text = "Shopping Cart";
            this.gpbShoppingCart.Visible = false;
            // 
            // tlpShoppingCart
            // 
            this.tlpShoppingCart.AutoSize = true;
            this.tlpShoppingCart.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpShoppingCart.ColumnCount = 2;
            this.tlpShoppingCart.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpShoppingCart.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpShoppingCart.Controls.Add(this.lstShoppingCart, 0, 0);
            this.tlpShoppingCart.Controls.Add(this.cmdAddToCart, 1, 1);
            this.tlpShoppingCart.Controls.Add(this.cmdRemoveFromCart, 1, 2);
            this.tlpShoppingCart.Controls.Add(this.cmdPurchaseAll, 1, 3);
            this.tlpShoppingCart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpShoppingCart.Location = new System.Drawing.Point(3, 16);
            this.tlpShoppingCart.Name = "tlpShoppingCart";
            this.tlpShoppingCart.RowCount = 4;
            this.tlpShoppingCart.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpShoppingCart.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tlpShoppingCart.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tlpShoppingCart.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tlpShoppingCart.Size = new System.Drawing.Size(288, 131);
            this.tlpShoppingCart.TabIndex = 0;
            // 
            // lstShoppingCart
            // 
            this.lstShoppingCart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstShoppingCart.FormattingEnabled = true;
            this.lstShoppingCart.Location = new System.Drawing.Point(3, 3);
            this.lstShoppingCart.Name = "lstShoppingCart";
            this.tlpShoppingCart.SetRowSpan(this.lstShoppingCart, 4);
            this.lstShoppingCart.Size = new System.Drawing.Size(200, 125);
            this.lstShoppingCart.TabIndex = 0;
            // 
            // cmdAddToCart
            // 
            this.cmdAddToCart.AutoSize = true;
            this.cmdAddToCart.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddToCart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdAddToCart.Location = new System.Drawing.Point(209, 44);
            this.cmdAddToCart.MinimumSize = new System.Drawing.Size(80, 23);
            this.cmdAddToCart.Name = "cmdAddToCart";
            this.cmdAddToCart.Size = new System.Drawing.Size(76, 29);
            this.cmdAddToCart.TabIndex = 1;
            this.cmdAddToCart.Tag = "Button_AddToCart";
            this.cmdAddToCart.Text = "Add to Cart";
            this.cmdAddToCart.UseVisualStyleBackColor = true;
            this.cmdAddToCart.Click += new System.EventHandler(this.cmdAddToCart_Click);
            // 
            // cmdRemoveFromCart
            // 
            this.cmdRemoveFromCart.AutoSize = true;
            this.cmdRemoveFromCart.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRemoveFromCart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdRemoveFromCart.Location = new System.Drawing.Point(209, 73);
            this.cmdRemoveFromCart.MinimumSize = new System.Drawing.Size(80, 23);
            this.cmdRemoveFromCart.Name = "cmdRemoveFromCart";
            this.cmdRemoveFromCart.Size = new System.Drawing.Size(76, 29);
            this.cmdRemoveFromCart.TabIndex = 2;
            this.cmdRemoveFromCart.Tag = "Button_RemoveFromCart";
            this.cmdRemoveFromCart.Text = "Remove";
            this.cmdRemoveFromCart.UseVisualStyleBackColor = true;
            this.cmdRemoveFromCart.Click += new System.EventHandler(this.cmdRemoveFromCart_Click);
            // 
            // cmdPurchaseAll
            // 
            this.cmdPurchaseAll.AutoSize = true;
            this.cmdPurchaseAll.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdPurchaseAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdPurchaseAll.Location = new System.Drawing.Point(209, 102);
            this.cmdPurchaseAll.MinimumSize = new System.Drawing.Size(80, 23);
            this.cmdPurchaseAll.Name = "cmdPurchaseAll";
            this.cmdPurchaseAll.Size = new System.Drawing.Size(76, 29);
            this.cmdPurchaseAll.TabIndex = 3;
            this.cmdPurchaseAll.Tag = "Button_PurchaseAll";
            this.cmdPurchaseAll.Text = "Purchase All";
            this.cmdPurchaseAll.UseVisualStyleBackColor = true;
            this.cmdPurchaseAll.Click += new System.EventHandler(this.cmdPurchaseAll_Click);
            // 
            // SelectWeaponAccessory
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
            this.Name = "SelectWeaponAccessory";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectWeaponAccessory";
            this.Text = "Select an Accessory";
            this.Load += new System.EventHandler(this.SelectWeaponAccessory_Load);
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
            this.gpbShoppingCart.ResumeLayout(false);
            this.gpbShoppingCart.PerformLayout();
            this.tlpShoppingCart.ResumeLayout(false);
            this.tlpShoppingCart.PerformLayout();
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
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private Chummer.ColorableCheckBox chkShowOnlyAffordItems;
        private System.Windows.Forms.FlowLayoutPanel flpRating;
        private System.Windows.Forms.Label lblRatingNALabel;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearchLabel;
        private System.Windows.Forms.FlowLayoutPanel flpCheckBoxes;
        private System.Windows.Forms.FlowLayoutPanel flpMarkup;
        private System.Windows.Forms.TableLayoutPanel tlpButtons;
        private System.Windows.Forms.GroupBox gpbCostFilter;
        private System.Windows.Forms.TableLayoutPanel tlpCostFilter;
        private System.Windows.Forms.Label lblMinimumCost;
        private System.Windows.Forms.Label lblMaximumCost;
        private System.Windows.Forms.Label lblExactCost;
        private Chummer.NumericUpDownEx nudMinimumCost;
        private Chummer.NumericUpDownEx nudMaximumCost;
        private Chummer.NumericUpDownEx nudExactCost;
        private System.Windows.Forms.TableLayoutPanel tlpRight;
        private System.Windows.Forms.TableLayoutPanel tlpTopRight;
        private System.Windows.Forms.GroupBox gpbShoppingCart;
        private System.Windows.Forms.TableLayoutPanel tlpShoppingCart;
        private System.Windows.Forms.ListBox lstShoppingCart;
        private System.Windows.Forms.Button cmdAddToCart;
        private System.Windows.Forms.Button cmdRemoveFromCart;
        private System.Windows.Forms.Button cmdPurchaseAll;
    }
}
