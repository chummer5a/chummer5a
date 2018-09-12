using System;

namespace Chummer
{
    partial class frmSelectArmorMod
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
            this.lblRatingLabel = new System.Windows.Forms.Label();
            this.lblCost = new System.Windows.Forms.Label();
            this.lblCostLabel = new System.Windows.Forms.Label();
            this.lblAvail = new System.Windows.Forms.Label();
            this.lblAvailLabel = new System.Windows.Forms.Label();
            this.lblA = new System.Windows.Forms.Label();
            this.lblALabel = new System.Windows.Forms.Label();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lstMod = new System.Windows.Forms.ListBox();
            this.nudRating = new System.Windows.Forms.NumericUpDown();
            this.cmdOKAdd = new System.Windows.Forms.Button();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.chkFreeItem = new System.Windows.Forms.CheckBox();
            this.lblCapacity = new System.Windows.Forms.Label();
            this.lblCapacityLabel = new System.Windows.Forms.Label();
            this.nudMarkup = new System.Windows.Forms.NumericUpDown();
            this.lblMarkupLabel = new System.Windows.Forms.Label();
            this.lblMarkupPercentLabel = new System.Windows.Forms.Label();
            this.lblTest = new System.Windows.Forms.Label();
            this.lblTestLabel = new System.Windows.Forms.Label();
            this.chkBlackMarketDiscount = new System.Windows.Forms.CheckBox();
            this.chkHideOverAvailLimit = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.chkShowOnlyAffordItems = new System.Windows.Forms.CheckBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.lblRatingNALabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudRating)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblRatingLabel
            // 
            this.lblRatingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblRatingLabel.AutoSize = true;
            this.lblRatingLabel.Location = new System.Drawing.Point(3, 81);
            this.lblRatingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRatingLabel.Name = "lblRatingLabel";
            this.lblRatingLabel.Size = new System.Drawing.Size(41, 14);
            this.lblRatingLabel.TabIndex = 5;
            this.lblRatingLabel.Tag = "Label_Rating";
            this.lblRatingLabel.Text = "Rating:";
            // 
            // lblCost
            // 
            this.lblCost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblCost.AutoSize = true;
            this.lblCost.Location = new System.Drawing.Point(88, 56);
            this.lblCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCost.Name = "lblCost";
            this.lblCost.Size = new System.Drawing.Size(34, 13);
            this.lblCost.TabIndex = 14;
            this.lblCost.Text = "[Cost]";
            // 
            // lblCostLabel
            // 
            this.lblCostLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblCostLabel.AutoSize = true;
            this.lblCostLabel.Location = new System.Drawing.Point(3, 56);
            this.lblCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCostLabel.Name = "lblCostLabel";
            this.lblCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblCostLabel.TabIndex = 13;
            this.lblCostLabel.Tag = "Label_Cost";
            this.lblCostLabel.Text = "Cost:";
            // 
            // lblAvail
            // 
            this.lblAvail.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblAvail.AutoSize = true;
            this.lblAvail.Location = new System.Drawing.Point(88, 31);
            this.lblAvail.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAvail.Name = "lblAvail";
            this.lblAvail.Size = new System.Drawing.Size(36, 13);
            this.lblAvail.TabIndex = 10;
            this.lblAvail.Text = "[Avail]";
            // 
            // lblAvailLabel
            // 
            this.lblAvailLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblAvailLabel.AutoSize = true;
            this.lblAvailLabel.Location = new System.Drawing.Point(3, 31);
            this.lblAvailLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAvailLabel.Name = "lblAvailLabel";
            this.lblAvailLabel.Size = new System.Drawing.Size(33, 13);
            this.lblAvailLabel.TabIndex = 9;
            this.lblAvailLabel.Tag = "Label_Avail";
            this.lblAvailLabel.Text = "Avail:";
            // 
            // lblA
            // 
            this.lblA.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblA.AutoSize = true;
            this.lblA.Location = new System.Drawing.Point(88, 107);
            this.lblA.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblA.Name = "lblA";
            this.lblA.Size = new System.Drawing.Size(20, 13);
            this.lblA.TabIndex = 2;
            this.lblA.Text = "[A]";
            // 
            // lblALabel
            // 
            this.lblALabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblALabel.AutoSize = true;
            this.lblALabel.Location = new System.Drawing.Point(3, 107);
            this.lblALabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblALabel.Name = "lblALabel";
            this.lblALabel.Size = new System.Drawing.Size(37, 13);
            this.lblALabel.TabIndex = 1;
            this.lblALabel.Tag = "Label_Armor";
            this.lblALabel.Text = "Armor:";
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(375, 406);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 23;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.Location = new System.Drawing.Point(537, 406);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 21;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lstMod
            // 
            this.lstMod.FormattingEnabled = true;
            this.lstMod.Location = new System.Drawing.Point(12, 12);
            this.lstMod.Name = "lstMod";
            this.lstMod.Size = new System.Drawing.Size(322, 420);
            this.lstMod.TabIndex = 0;
            this.lstMod.SelectedIndexChanged += new System.EventHandler(this.lstMod_SelectedIndexChanged);
            this.lstMod.DoubleClick += new System.EventHandler(this.lstMod_DoubleClick);
            // 
            // nudRating
            // 
            this.nudRating.Enabled = false;
            this.nudRating.Location = new System.Drawing.Point(3, 3);
            this.nudRating.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudRating.Name = "nudRating";
            this.nudRating.Size = new System.Drawing.Size(94, 20);
            this.nudRating.TabIndex = 6;
            this.nudRating.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudRating.ValueChanged += new System.EventHandler(this.nudRating_ValueChanged);
            // 
            // cmdOKAdd
            // 
            this.cmdOKAdd.Location = new System.Drawing.Point(456, 406);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(75, 23);
            this.cmdOKAdd.TabIndex = 22;
            this.cmdOKAdd.Tag = "String_AddMore";
            this.cmdOKAdd.Text = "&Add && More";
            this.cmdOKAdd.UseVisualStyleBackColor = true;
            this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
            // 
            // lblSource
            // 
            this.lblSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSource.AutoSize = true;
            this.lblSource.Location = new System.Drawing.Point(88, 208);
            this.lblSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 20;
            this.lblSource.Text = "[Source]";
            this.lblSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(3, 208);
            this.lblSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 19;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            // 
            // chkFreeItem
            // 
            this.chkFreeItem.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.chkFreeItem.AutoSize = true;
            this.chkFreeItem.Location = new System.Drawing.Point(3, 130);
            this.chkFreeItem.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkFreeItem.Name = "chkFreeItem";
            this.chkFreeItem.Size = new System.Drawing.Size(50, 17);
            this.chkFreeItem.TabIndex = 15;
            this.chkFreeItem.Tag = "Checkbox_Free";
            this.chkFreeItem.Text = "Free!";
            this.chkFreeItem.UseVisualStyleBackColor = true;
            this.chkFreeItem.CheckedChanged += new System.EventHandler(this.chkFreeItem_CheckedChanged);
            // 
            // lblCapacity
            // 
            this.lblCapacity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblCapacity.AutoSize = true;
            this.lblCapacity.Location = new System.Drawing.Point(88, 6);
            this.lblCapacity.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCapacity.Name = "lblCapacity";
            this.lblCapacity.Size = new System.Drawing.Size(19, 13);
            this.lblCapacity.TabIndex = 8;
            this.lblCapacity.Text = "[0]";
            // 
            // lblCapacityLabel
            // 
            this.lblCapacityLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblCapacityLabel.AutoSize = true;
            this.lblCapacityLabel.Location = new System.Drawing.Point(3, 6);
            this.lblCapacityLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCapacityLabel.Name = "lblCapacityLabel";
            this.lblCapacityLabel.Size = new System.Drawing.Size(51, 13);
            this.lblCapacityLabel.TabIndex = 7;
            this.lblCapacityLabel.Tag = "Label_Capacity";
            this.lblCapacityLabel.Text = "Capacity:";
            // 
            // nudMarkup
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.nudMarkup, 2);
            this.nudMarkup.DecimalPlaces = 2;
            this.nudMarkup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudMarkup.Location = new System.Drawing.Point(88, 179);
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
            this.nudMarkup.Size = new System.Drawing.Size(94, 20);
            this.nudMarkup.TabIndex = 17;
            this.nudMarkup.ValueChanged += new System.EventHandler(this.nudMarkup_ValueChanged);
            // 
            // lblMarkupLabel
            // 
            this.lblMarkupLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblMarkupLabel.AutoSize = true;
            this.lblMarkupLabel.Location = new System.Drawing.Point(3, 182);
            this.lblMarkupLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMarkupLabel.Name = "lblMarkupLabel";
            this.lblMarkupLabel.Size = new System.Drawing.Size(46, 14);
            this.lblMarkupLabel.TabIndex = 16;
            this.lblMarkupLabel.Tag = "Label_SelectGear_Markup";
            this.lblMarkupLabel.Text = "Markup:";
            // 
            // lblMarkupPercentLabel
            // 
            this.lblMarkupPercentLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblMarkupPercentLabel.AutoSize = true;
            this.lblMarkupPercentLabel.Location = new System.Drawing.Point(188, 182);
            this.lblMarkupPercentLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMarkupPercentLabel.Name = "lblMarkupPercentLabel";
            this.lblMarkupPercentLabel.Size = new System.Drawing.Size(15, 14);
            this.lblMarkupPercentLabel.TabIndex = 18;
            this.lblMarkupPercentLabel.Text = "%";
            // 
            // lblTest
            // 
            this.lblTest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblTest.AutoSize = true;
            this.lblTest.Location = new System.Drawing.Point(188, 31);
            this.lblTest.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTest.Name = "lblTest";
            this.lblTest.Size = new System.Drawing.Size(19, 13);
            this.lblTest.TabIndex = 12;
            this.lblTest.Text = "[0]";
            // 
            // lblTestLabel
            // 
            this.lblTestLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblTestLabel.AutoSize = true;
            this.lblTestLabel.Location = new System.Drawing.Point(143, 31);
            this.lblTestLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTestLabel.Name = "lblTestLabel";
            this.lblTestLabel.Size = new System.Drawing.Size(31, 13);
            this.lblTestLabel.TabIndex = 11;
            this.lblTestLabel.Tag = "Label_Test";
            this.lblTestLabel.Text = "Test:";
            // 
            // chkBlackMarketDiscount
            // 
            this.chkBlackMarketDiscount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.chkBlackMarketDiscount.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkBlackMarketDiscount, 4);
            this.chkBlackMarketDiscount.Location = new System.Drawing.Point(3, 155);
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
            // chkHideOverAvailLimit
            // 
            this.chkHideOverAvailLimit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.chkHideOverAvailLimit.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkHideOverAvailLimit, 4);
            this.chkHideOverAvailLimit.Location = new System.Drawing.Point(3, 231);
            this.chkHideOverAvailLimit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkHideOverAvailLimit.Name = "chkHideOverAvailLimit";
            this.chkHideOverAvailLimit.Size = new System.Drawing.Size(175, 17);
            this.chkHideOverAvailLimit.TabIndex = 65;
            this.chkHideOverAvailLimit.Tag = "Checkbox_HideOverAvailLimit";
            this.chkHideOverAvailLimit.Text = "Hide Items Over Avail Limit ({0})";
            this.chkHideOverAvailLimit.UseVisualStyleBackColor = true;
            this.chkHideOverAvailLimit.CheckedChanged += new System.EventHandler(this.chkHideOverAvailLimit_CheckedChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 85F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 55F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 87F));
            this.tableLayoutPanel1.Controls.Add(this.chkShowOnlyAffordItems, 0, 10);
            this.tableLayoutPanel1.Controls.Add(this.lblCapacityLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblSource, 1, 8);
            this.tableLayoutPanel1.Controls.Add(this.nudMarkup, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.lblSourceLabel, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.chkBlackMarketDiscount, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.lblMarkupLabel, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.lblCapacity, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblTest, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblAvailLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblTestLabel, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.chkFreeItem, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.lblAvail, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblCostLabel, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblCost, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblA, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblRatingLabel, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblALabel, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.chkHideOverAvailLimit, 0, 9);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblMarkupPercentLabel, 3, 7);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(340, 12);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 11;
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
            this.tableLayoutPanel1.Size = new System.Drawing.Size(272, 387);
            this.tableLayoutPanel1.TabIndex = 66;
            // 
            // chkShowOnlyAffordItems
            // 
            this.chkShowOnlyAffordItems.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.chkShowOnlyAffordItems.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkShowOnlyAffordItems, 4);
            this.chkShowOnlyAffordItems.Location = new System.Drawing.Point(3, 256);
            this.chkShowOnlyAffordItems.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkShowOnlyAffordItems.Name = "chkShowOnlyAffordItems";
            this.chkShowOnlyAffordItems.Size = new System.Drawing.Size(164, 127);
            this.chkShowOnlyAffordItems.TabIndex = 68;
            this.chkShowOnlyAffordItems.Tag = "Checkbox_ShowOnlyAffordItems";
            this.chkShowOnlyAffordItems.Text = "Show Only Items I Can Afford";
            this.chkShowOnlyAffordItems.UseVisualStyleBackColor = true;
            this.chkShowOnlyAffordItems.CheckedChanged += new System.EventHandler(this.chkHideOverAvailLimit_CheckedChanged);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 2);
            this.flowLayoutPanel1.Controls.Add(this.nudRating);
            this.flowLayoutPanel1.Controls.Add(this.lblRatingNALabel);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(85, 75);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(100, 26);
            this.flowLayoutPanel1.TabIndex = 69;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // lblRatingNALabel
            // 
            this.lblRatingNALabel.AutoSize = true;
            this.lblRatingNALabel.Location = new System.Drawing.Point(103, 6);
            this.lblRatingNALabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 7);
            this.lblRatingNALabel.Name = "lblRatingNALabel";
            this.lblRatingNALabel.Size = new System.Drawing.Size(27, 13);
            this.lblRatingNALabel.TabIndex = 14;
            this.lblRatingNALabel.Tag = "String_NotApplicable";
            this.lblRatingNALabel.Text = "N/A";
            this.lblRatingNALabel.Visible = false;
            // 
            // frmSelectArmorMod
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.cmdOKAdd);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.lstMod);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectArmorMod";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectArmorMod";
            this.Text = "Select an Armor Mod";
            this.Load += new System.EventHandler(this.frmSelectArmorMod_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudRating)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblRatingLabel;
        private System.Windows.Forms.Label lblCost;
        private System.Windows.Forms.Label lblCostLabel;
        private System.Windows.Forms.Label lblAvail;
        private System.Windows.Forms.Label lblAvailLabel;
        private System.Windows.Forms.Label lblA;
        private System.Windows.Forms.Label lblALabel;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.ListBox lstMod;
        private System.Windows.Forms.NumericUpDown nudRating;
        private System.Windows.Forms.Button cmdOKAdd;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblSourceLabel;
        private System.Windows.Forms.CheckBox chkFreeItem;
        private System.Windows.Forms.Label lblCapacity;
        private System.Windows.Forms.Label lblCapacityLabel;
        private System.Windows.Forms.NumericUpDown nudMarkup;
        private System.Windows.Forms.Label lblMarkupLabel;
        private System.Windows.Forms.Label lblMarkupPercentLabel;
        private System.Windows.Forms.Label lblTest;
        private System.Windows.Forms.Label lblTestLabel;
        private System.Windows.Forms.CheckBox chkBlackMarketDiscount;
        private System.Windows.Forms.CheckBox chkHideOverAvailLimit;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckBox chkShowOnlyAffordItems;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label lblRatingNALabel;
    }
}
