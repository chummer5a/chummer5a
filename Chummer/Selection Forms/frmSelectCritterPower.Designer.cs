namespace Chummer
{
    partial class frmSelectCritterPower
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
            this.lblCritterPowerSource = new System.Windows.Forms.Label();
            this.lblCritterPowerSourceLabel = new System.Windows.Forms.Label();
            this.lblCritterPowerDuration = new System.Windows.Forms.Label();
            this.lblCritterPowerDurationLabel = new System.Windows.Forms.Label();
            this.lblCritterPowerRange = new System.Windows.Forms.Label();
            this.lblCritterPowerRangeLabel = new System.Windows.Forms.Label();
            this.lblCritterPowerAction = new System.Windows.Forms.Label();
            this.lblCritterPowerActionLabel = new System.Windows.Forms.Label();
            this.lblCritterPowerType = new System.Windows.Forms.Label();
            this.lblCritterPowerTypeLabel = new System.Windows.Forms.Label();
            this.lblCritterPowerCategory = new System.Windows.Forms.Label();
            this.lblCritterPowerCategoryLabel = new System.Windows.Forms.Label();
            this.lblCritterPowerRatingLabel = new System.Windows.Forms.Label();
            this.nudCritterPowerRating = new System.Windows.Forms.NumericUpDown();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.trePowers = new System.Windows.Forms.TreeView();
            this.lblCategory = new System.Windows.Forms.Label();
            this.cboCategory = new Chummer.ElasticComboBox();
            this.lblPowerPoints = new System.Windows.Forms.Label();
            this.lblPowerPointsLabel = new System.Windows.Forms.Label();
            this.cmdOKAdd = new System.Windows.Forms.Button();
            this.lblKarma = new System.Windows.Forms.Label();
            this.lblKarmaLabel = new System.Windows.Forms.Label();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.lblRatingNALabel = new System.Windows.Forms.Label();
            this.lblSearchLabel = new System.Windows.Forms.Label();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.nudCritterPowerRating)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCritterPowerSource
            // 
            this.lblCritterPowerSource.AutoSize = true;
            this.lblCritterPowerSource.Location = new System.Drawing.Point(362, 183);
            this.lblCritterPowerSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerSource.Name = "lblCritterPowerSource";
            this.lblCritterPowerSource.Size = new System.Drawing.Size(47, 13);
            this.lblCritterPowerSource.TabIndex = 16;
            this.lblCritterPowerSource.Text = "[Source]";
            this.lblCritterPowerSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblCritterPowerSourceLabel
            // 
            this.lblCritterPowerSourceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCritterPowerSourceLabel.AutoSize = true;
            this.lblCritterPowerSourceLabel.Location = new System.Drawing.Point(312, 183);
            this.lblCritterPowerSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerSourceLabel.Name = "lblCritterPowerSourceLabel";
            this.lblCritterPowerSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblCritterPowerSourceLabel.TabIndex = 15;
            this.lblCritterPowerSourceLabel.Tag = "Label_Source";
            this.lblCritterPowerSourceLabel.Text = "Source:";
            // 
            // lblCritterPowerDuration
            // 
            this.lblCritterPowerDuration.AutoSize = true;
            this.lblCritterPowerDuration.Location = new System.Drawing.Point(362, 132);
            this.lblCritterPowerDuration.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerDuration.Name = "lblCritterPowerDuration";
            this.lblCritterPowerDuration.Size = new System.Drawing.Size(53, 13);
            this.lblCritterPowerDuration.TabIndex = 12;
            this.lblCritterPowerDuration.Text = "[Duration]";
            // 
            // lblCritterPowerDurationLabel
            // 
            this.lblCritterPowerDurationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCritterPowerDurationLabel.AutoSize = true;
            this.lblCritterPowerDurationLabel.Location = new System.Drawing.Point(306, 132);
            this.lblCritterPowerDurationLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerDurationLabel.Name = "lblCritterPowerDurationLabel";
            this.lblCritterPowerDurationLabel.Size = new System.Drawing.Size(50, 13);
            this.lblCritterPowerDurationLabel.TabIndex = 11;
            this.lblCritterPowerDurationLabel.Tag = "Label_Duration";
            this.lblCritterPowerDurationLabel.Text = "Duration:";
            // 
            // lblCritterPowerRange
            // 
            this.lblCritterPowerRange.AutoSize = true;
            this.lblCritterPowerRange.Location = new System.Drawing.Point(362, 107);
            this.lblCritterPowerRange.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerRange.Name = "lblCritterPowerRange";
            this.lblCritterPowerRange.Size = new System.Drawing.Size(45, 13);
            this.lblCritterPowerRange.TabIndex = 10;
            this.lblCritterPowerRange.Text = "[Range]";
            // 
            // lblCritterPowerRangeLabel
            // 
            this.lblCritterPowerRangeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCritterPowerRangeLabel.AutoSize = true;
            this.lblCritterPowerRangeLabel.Location = new System.Drawing.Point(314, 107);
            this.lblCritterPowerRangeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerRangeLabel.Name = "lblCritterPowerRangeLabel";
            this.lblCritterPowerRangeLabel.Size = new System.Drawing.Size(42, 13);
            this.lblCritterPowerRangeLabel.TabIndex = 9;
            this.lblCritterPowerRangeLabel.Tag = "Label_Range";
            this.lblCritterPowerRangeLabel.Text = "Range:";
            // 
            // lblCritterPowerAction
            // 
            this.lblCritterPowerAction.AutoSize = true;
            this.lblCritterPowerAction.Location = new System.Drawing.Point(362, 82);
            this.lblCritterPowerAction.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerAction.Name = "lblCritterPowerAction";
            this.lblCritterPowerAction.Size = new System.Drawing.Size(43, 13);
            this.lblCritterPowerAction.TabIndex = 8;
            this.lblCritterPowerAction.Text = "[Action]";
            // 
            // lblCritterPowerActionLabel
            // 
            this.lblCritterPowerActionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCritterPowerActionLabel.AutoSize = true;
            this.lblCritterPowerActionLabel.Location = new System.Drawing.Point(316, 82);
            this.lblCritterPowerActionLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerActionLabel.Name = "lblCritterPowerActionLabel";
            this.lblCritterPowerActionLabel.Size = new System.Drawing.Size(40, 13);
            this.lblCritterPowerActionLabel.TabIndex = 7;
            this.lblCritterPowerActionLabel.Tag = "Label_Action";
            this.lblCritterPowerActionLabel.Text = "Action:";
            // 
            // lblCritterPowerType
            // 
            this.lblCritterPowerType.AutoSize = true;
            this.lblCritterPowerType.Location = new System.Drawing.Point(362, 57);
            this.lblCritterPowerType.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerType.Name = "lblCritterPowerType";
            this.lblCritterPowerType.Size = new System.Drawing.Size(37, 13);
            this.lblCritterPowerType.TabIndex = 6;
            this.lblCritterPowerType.Text = "[Type]";
            // 
            // lblCritterPowerTypeLabel
            // 
            this.lblCritterPowerTypeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCritterPowerTypeLabel.AutoSize = true;
            this.lblCritterPowerTypeLabel.Location = new System.Drawing.Point(322, 57);
            this.lblCritterPowerTypeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerTypeLabel.Name = "lblCritterPowerTypeLabel";
            this.lblCritterPowerTypeLabel.Size = new System.Drawing.Size(34, 13);
            this.lblCritterPowerTypeLabel.TabIndex = 5;
            this.lblCritterPowerTypeLabel.Tag = "Label_Type";
            this.lblCritterPowerTypeLabel.Text = "Type:";
            // 
            // lblCritterPowerCategory
            // 
            this.lblCritterPowerCategory.AutoSize = true;
            this.lblCritterPowerCategory.Location = new System.Drawing.Point(362, 32);
            this.lblCritterPowerCategory.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerCategory.Name = "lblCritterPowerCategory";
            this.lblCritterPowerCategory.Size = new System.Drawing.Size(55, 13);
            this.lblCritterPowerCategory.TabIndex = 4;
            this.lblCritterPowerCategory.Text = "[Category]";
            // 
            // lblCritterPowerCategoryLabel
            // 
            this.lblCritterPowerCategoryLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCritterPowerCategoryLabel.AutoSize = true;
            this.lblCritterPowerCategoryLabel.Location = new System.Drawing.Point(304, 32);
            this.lblCritterPowerCategoryLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerCategoryLabel.Name = "lblCritterPowerCategoryLabel";
            this.lblCritterPowerCategoryLabel.Size = new System.Drawing.Size(52, 13);
            this.lblCritterPowerCategoryLabel.TabIndex = 3;
            this.lblCritterPowerCategoryLabel.Tag = "Label_Category";
            this.lblCritterPowerCategoryLabel.Text = "Category:";
            // 
            // lblCritterPowerRatingLabel
            // 
            this.lblCritterPowerRatingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCritterPowerRatingLabel.AutoSize = true;
            this.lblCritterPowerRatingLabel.Location = new System.Drawing.Point(315, 157);
            this.lblCritterPowerRatingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerRatingLabel.Name = "lblCritterPowerRatingLabel";
            this.lblCritterPowerRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblCritterPowerRatingLabel.TabIndex = 13;
            this.lblCritterPowerRatingLabel.Tag = "Label_Rating";
            this.lblCritterPowerRatingLabel.Text = "Rating:";
            // 
            // nudCritterPowerRating
            // 
            this.nudCritterPowerRating.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudCritterPowerRating.Enabled = false;
            this.nudCritterPowerRating.Location = new System.Drawing.Point(3, 3);
            this.nudCritterPowerRating.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudCritterPowerRating.Name = "nudCritterPowerRating";
            this.nudCritterPowerRating.Size = new System.Drawing.Size(75, 20);
            this.nudCritterPowerRating.TabIndex = 14;
            this.nudCritterPowerRating.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
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
            this.cmdCancel.TabIndex = 20;
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
            this.cmdOK.TabIndex = 19;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // trePowers
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.trePowers, 2);
            this.trePowers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trePowers.FullRowSelect = true;
            this.trePowers.HideSelection = false;
            this.trePowers.Location = new System.Drawing.Point(3, 30);
            this.trePowers.Name = "trePowers";
            this.trePowers.ShowLines = false;
            this.trePowers.ShowPlusMinus = false;
            this.trePowers.ShowRootLines = false;
            this.trePowers.Size = new System.Drawing.Size(295, 390);
            this.trePowers.TabIndex = 0;
            this.trePowers.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.trePowers_AfterSelect);
            this.trePowers.DoubleClick += new System.EventHandler(this.trePowers_DoubleClick);
            // 
            // lblCategory
            // 
            this.lblCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(20, 6);
            this.lblCategory.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(52, 13);
            this.lblCategory.TabIndex = 1;
            this.lblCategory.Tag = "Label_Category";
            this.lblCategory.Text = "Category:";
            // 
            // cboCategory
            // 
            this.cboCategory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategory.FormattingEnabled = true;
            this.cboCategory.Location = new System.Drawing.Point(78, 3);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(220, 21);
            this.cboCategory.TabIndex = 2;
            this.cboCategory.TooltipText = "";
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // lblPowerPoints
            // 
            this.lblPowerPoints.AutoSize = true;
            this.lblPowerPoints.Location = new System.Drawing.Point(362, 208);
            this.lblPowerPoints.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPowerPoints.Name = "lblPowerPoints";
            this.lblPowerPoints.Size = new System.Drawing.Size(75, 13);
            this.lblPowerPoints.TabIndex = 18;
            this.lblPowerPoints.Text = "[Power Points]";
            this.lblPowerPoints.Visible = false;
            // 
            // lblPowerPointsLabel
            // 
            this.lblPowerPointsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPowerPointsLabel.AutoSize = true;
            this.lblPowerPointsLabel.Location = new System.Drawing.Point(317, 208);
            this.lblPowerPointsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPowerPointsLabel.Name = "lblPowerPointsLabel";
            this.lblPowerPointsLabel.Size = new System.Drawing.Size(39, 13);
            this.lblPowerPointsLabel.TabIndex = 17;
            this.lblPowerPointsLabel.Tag = "Label_Points";
            this.lblPowerPointsLabel.Text = "Points:";
            this.lblPowerPointsLabel.Visible = false;
            // 
            // cmdOKAdd
            // 
            this.cmdOKAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOKAdd.AutoSize = true;
            this.cmdOKAdd.Location = new System.Drawing.Point(81, 0);
            this.cmdOKAdd.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(75, 23);
            this.cmdOKAdd.TabIndex = 21;
            this.cmdOKAdd.Tag = "String_AddMore";
            this.cmdOKAdd.Text = "&Add && More";
            this.cmdOKAdd.UseVisualStyleBackColor = true;
            this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
            // 
            // lblKarma
            // 
            this.lblKarma.AutoSize = true;
            this.lblKarma.Location = new System.Drawing.Point(362, 233);
            this.lblKarma.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarma.Name = "lblKarma";
            this.lblKarma.Size = new System.Drawing.Size(43, 13);
            this.lblKarma.TabIndex = 23;
            this.lblKarma.Text = "[Karma]";
            // 
            // lblKarmaLabel
            // 
            this.lblKarmaLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKarmaLabel.AutoSize = true;
            this.lblKarmaLabel.Location = new System.Drawing.Point(316, 233);
            this.lblKarmaLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaLabel.Name = "lblKarmaLabel";
            this.lblKarmaLabel.Size = new System.Drawing.Size(40, 13);
            this.lblKarmaLabel.TabIndex = 22;
            this.lblKarmaLabel.Tag = "Label_Karma";
            this.lblKarmaLabel.Text = "Karma:";
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 3;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 301F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.txtSearch, 2, 0);
            this.tlpMain.Controls.Add(this.flowLayoutPanel1, 2, 6);
            this.tlpMain.Controls.Add(this.lblSearchLabel, 1, 0);
            this.tlpMain.Controls.Add(this.lblKarma, 2, 9);
            this.tlpMain.Controls.Add(this.lblCritterPowerCategoryLabel, 1, 1);
            this.tlpMain.Controls.Add(this.lblKarmaLabel, 1, 9);
            this.tlpMain.Controls.Add(this.lblCritterPowerCategory, 2, 1);
            this.tlpMain.Controls.Add(this.lblCritterPowerTypeLabel, 1, 2);
            this.tlpMain.Controls.Add(this.lblPowerPoints, 2, 8);
            this.tlpMain.Controls.Add(this.lblCritterPowerType, 2, 2);
            this.tlpMain.Controls.Add(this.lblPowerPointsLabel, 1, 8);
            this.tlpMain.Controls.Add(this.lblCritterPowerActionLabel, 1, 3);
            this.tlpMain.Controls.Add(this.lblCritterPowerAction, 2, 3);
            this.tlpMain.Controls.Add(this.lblCritterPowerRangeLabel, 1, 4);
            this.tlpMain.Controls.Add(this.lblCritterPowerRange, 2, 4);
            this.tlpMain.Controls.Add(this.lblCritterPowerDurationLabel, 1, 5);
            this.tlpMain.Controls.Add(this.lblCritterPowerDuration, 2, 5);
            this.tlpMain.Controls.Add(this.lblCritterPowerSource, 2, 7);
            this.tlpMain.Controls.Add(this.lblCritterPowerSourceLabel, 1, 7);
            this.tlpMain.Controls.Add(this.lblCritterPowerRatingLabel, 1, 6);
            this.tlpMain.Controls.Add(this.flowLayoutPanel2, 1, 10);
            this.tlpMain.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 11;
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
            this.tlpMain.TabIndex = 24;
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(362, 3);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(241, 20);
            this.txtSearch.TabIndex = 26;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.nudCritterPowerRating);
            this.flowLayoutPanel1.Controls.Add(this.lblRatingNALabel);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(359, 151);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(247, 26);
            this.flowLayoutPanel1.TabIndex = 25;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // lblRatingNALabel
            // 
            this.lblRatingNALabel.AutoSize = true;
            this.lblRatingNALabel.Location = new System.Drawing.Point(84, 6);
            this.lblRatingNALabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 7);
            this.lblRatingNALabel.Name = "lblRatingNALabel";
            this.lblRatingNALabel.Size = new System.Drawing.Size(27, 13);
            this.lblRatingNALabel.TabIndex = 16;
            this.lblRatingNALabel.Tag = "String_NotApplicable";
            this.lblRatingNALabel.Text = "N/A";
            this.lblRatingNALabel.Visible = false;
            // 
            // lblSearchLabel
            // 
            this.lblSearchLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSearchLabel.AutoSize = true;
            this.lblSearchLabel.Location = new System.Drawing.Point(312, 6);
            this.lblSearchLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSearchLabel.Name = "lblSearchLabel";
            this.lblSearchLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSearchLabel.TabIndex = 25;
            this.lblSearchLabel.Tag = "Label_Search";
            this.lblSearchLabel.Text = "&Search:";
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel2.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.flowLayoutPanel2, 2);
            this.flowLayoutPanel2.Controls.Add(this.cmdOK);
            this.flowLayoutPanel2.Controls.Add(this.cmdOKAdd);
            this.flowLayoutPanel2.Controls.Add(this.cmdCancel);
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(366, 397);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(237, 23);
            this.flowLayoutPanel2.TabIndex = 27;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.tableLayoutPanel2.Controls.Add(this.lblCategory, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.cboCategory, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.trePowers, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tlpMain.SetRowSpan(this.tableLayoutPanel2, 11);
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(301, 423);
            this.tableLayoutPanel2.TabIndex = 28;
            // 
            // frmSelectCritterPower
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.tlpMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectCritterPower";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectCritterPower";
            this.Text = "Select a Critter Power";
            this.Load += new System.EventHandler(this.frmSelectCritterPower_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudCritterPowerRating)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblCritterPowerSource;
        private System.Windows.Forms.Label lblCritterPowerSourceLabel;
        private System.Windows.Forms.Label lblCritterPowerDuration;
        private System.Windows.Forms.Label lblCritterPowerDurationLabel;
        private System.Windows.Forms.Label lblCritterPowerRange;
        private System.Windows.Forms.Label lblCritterPowerRangeLabel;
        private System.Windows.Forms.Label lblCritterPowerAction;
        private System.Windows.Forms.Label lblCritterPowerActionLabel;
        private System.Windows.Forms.Label lblCritterPowerType;
        private System.Windows.Forms.Label lblCritterPowerTypeLabel;
        private System.Windows.Forms.Label lblCritterPowerCategory;
        private System.Windows.Forms.Label lblCritterPowerCategoryLabel;
        private System.Windows.Forms.Label lblCritterPowerRatingLabel;
        private System.Windows.Forms.NumericUpDown nudCritterPowerRating;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.TreeView trePowers;
        private System.Windows.Forms.Label lblCategory;
        private ElasticComboBox cboCategory;
        private System.Windows.Forms.Label lblPowerPoints;
        private System.Windows.Forms.Label lblPowerPointsLabel;
        private System.Windows.Forms.Button cmdOKAdd;
        private System.Windows.Forms.Label lblKarma;
        private System.Windows.Forms.Label lblKarmaLabel;
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label lblRatingNALabel;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearchLabel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    }
}
