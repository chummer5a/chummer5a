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
            this.cboCategory = new ElasticComboBox();
            this.lblPowerPoints = new System.Windows.Forms.Label();
            this.lblPowerPointsLabel = new System.Windows.Forms.Label();
            this.cmdOKAdd = new System.Windows.Forms.Button();
            this.lblKarma = new System.Windows.Forms.Label();
            this.lblKarmaLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.lblRatingNALabel = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearchLabel = new System.Windows.Forms.Label();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.nudCritterPowerRating)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCritterPowerSource
            // 
            this.lblCritterPowerSource.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblCritterPowerSource, 2);
            this.lblCritterPowerSource.Location = new System.Drawing.Point(363, 184);
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
            this.lblCritterPowerSourceLabel.Location = new System.Drawing.Point(313, 184);
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
            this.tableLayoutPanel1.SetColumnSpan(this.lblCritterPowerDuration, 2);
            this.lblCritterPowerDuration.Location = new System.Drawing.Point(363, 133);
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
            this.lblCritterPowerDurationLabel.Location = new System.Drawing.Point(307, 133);
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
            this.tableLayoutPanel1.SetColumnSpan(this.lblCritterPowerRange, 2);
            this.lblCritterPowerRange.Location = new System.Drawing.Point(363, 108);
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
            this.lblCritterPowerRangeLabel.Location = new System.Drawing.Point(315, 108);
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
            this.tableLayoutPanel1.SetColumnSpan(this.lblCritterPowerAction, 2);
            this.lblCritterPowerAction.Location = new System.Drawing.Point(363, 83);
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
            this.lblCritterPowerActionLabel.Location = new System.Drawing.Point(317, 83);
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
            this.tableLayoutPanel1.SetColumnSpan(this.lblCritterPowerType, 2);
            this.lblCritterPowerType.Location = new System.Drawing.Point(363, 58);
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
            this.lblCritterPowerTypeLabel.Location = new System.Drawing.Point(323, 58);
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
            this.tableLayoutPanel1.SetColumnSpan(this.lblCritterPowerCategory, 2);
            this.lblCritterPowerCategory.Location = new System.Drawing.Point(363, 33);
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
            this.lblCritterPowerCategoryLabel.Location = new System.Drawing.Point(305, 33);
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
            this.lblCritterPowerRatingLabel.Location = new System.Drawing.Point(316, 158);
            this.lblCritterPowerRatingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerRatingLabel.Name = "lblCritterPowerRatingLabel";
            this.lblCritterPowerRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblCritterPowerRatingLabel.TabIndex = 13;
            this.lblCritterPowerRatingLabel.Tag = "Label_Rating";
            this.lblCritterPowerRatingLabel.Text = "Rating:";
            // 
            // nudCritterPowerRating
            // 
            this.nudCritterPowerRating.Enabled = false;
            this.nudCritterPowerRating.Location = new System.Drawing.Point(3, 3);
            this.nudCritterPowerRating.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudCritterPowerRating.Name = "nudCritterPowerRating";
            this.nudCritterPowerRating.Size = new System.Drawing.Size(156, 20);
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
            this.trePowers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.trePowers, 2);
            this.trePowers.FullRowSelect = true;
            this.trePowers.HideSelection = false;
            this.trePowers.Location = new System.Drawing.Point(3, 30);
            this.trePowers.Name = "trePowers";
            this.tableLayoutPanel1.SetRowSpan(this.trePowers, 10);
            this.trePowers.ShowLines = false;
            this.trePowers.ShowPlusMinus = false;
            this.trePowers.ShowRootLines = false;
            this.trePowers.Size = new System.Drawing.Size(294, 384);
            this.trePowers.TabIndex = 0;
            this.trePowers.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.trePowers_AfterSelect);
            this.trePowers.DoubleClick += new System.EventHandler(this.trePowers_DoubleClick);
            // 
            // lblCategory
            // 
            this.lblCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(17, 6);
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
            this.cboCategory.Location = new System.Drawing.Point(75, 3);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(222, 21);
            this.cboCategory.TabIndex = 2;
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // lblPowerPoints
            // 
            this.lblPowerPoints.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblPowerPoints, 2);
            this.lblPowerPoints.Location = new System.Drawing.Point(363, 209);
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
            this.lblPowerPointsLabel.Location = new System.Drawing.Point(318, 209);
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
            this.tableLayoutPanel1.SetColumnSpan(this.lblKarma, 2);
            this.lblKarma.Location = new System.Drawing.Point(363, 234);
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
            this.lblKarmaLabel.Location = new System.Drawing.Point(317, 234);
            this.lblKarmaLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaLabel.Name = "lblKarmaLabel";
            this.lblKarmaLabel.Size = new System.Drawing.Size(40, 13);
            this.lblKarmaLabel.TabIndex = 22;
            this.lblKarmaLabel.Tag = "Label_Karma";
            this.lblKarmaLabel.Text = "Karma:";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 13F));
            this.tableLayoutPanel1.Controls.Add(this.txtSearch, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.trePowers, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblCategory, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 3, 6);
            this.tableLayoutPanel1.Controls.Add(this.cboCategory, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblSearchLabel, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblKarma, 3, 9);
            this.tableLayoutPanel1.Controls.Add(this.lblCritterPowerCategoryLabel, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblKarmaLabel, 2, 9);
            this.tableLayoutPanel1.Controls.Add(this.lblCritterPowerCategory, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblCritterPowerTypeLabel, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblPowerPoints, 3, 8);
            this.tableLayoutPanel1.Controls.Add(this.lblCritterPowerType, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblPowerPointsLabel, 2, 8);
            this.tableLayoutPanel1.Controls.Add(this.lblCritterPowerActionLabel, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblCritterPowerAction, 3, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblCritterPowerRangeLabel, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblCritterPowerRange, 3, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblCritterPowerDurationLabel, 2, 5);
            this.tableLayoutPanel1.Controls.Add(this.lblCritterPowerDuration, 3, 5);
            this.tableLayoutPanel1.Controls.Add(this.lblCritterPowerSource, 3, 7);
            this.tableLayoutPanel1.Controls.Add(this.lblCritterPowerSourceLabel, 2, 7);
            this.tableLayoutPanel1.Controls.Add(this.lblCritterPowerRatingLabel, 2, 6);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 2, 10);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
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
            this.tableLayoutPanel1.Size = new System.Drawing.Size(600, 417);
            this.tableLayoutPanel1.TabIndex = 24;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.nudCritterPowerRating);
            this.flowLayoutPanel1.Controls.Add(this.lblRatingNALabel);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(360, 152);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(162, 26);
            this.flowLayoutPanel1.TabIndex = 25;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // lblRatingNALabel
            // 
            this.lblRatingNALabel.AutoSize = true;
            this.lblRatingNALabel.Location = new System.Drawing.Point(165, 6);
            this.lblRatingNALabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 7);
            this.lblRatingNALabel.Name = "lblRatingNALabel";
            this.lblRatingNALabel.Size = new System.Drawing.Size(27, 13);
            this.lblRatingNALabel.TabIndex = 16;
            this.lblRatingNALabel.Tag = "String_NotApplicable";
            this.lblRatingNALabel.Text = "N/A";
            this.lblRatingNALabel.Visible = false;
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.txtSearch, 2);
            this.txtSearch.Location = new System.Drawing.Point(363, 3);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(234, 20);
            this.txtSearch.TabIndex = 26;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // lblSearchLabel
            // 
            this.lblSearchLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSearchLabel.AutoSize = true;
            this.lblSearchLabel.Location = new System.Drawing.Point(313, 6);
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
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel2, 3);
            this.flowLayoutPanel2.Controls.Add(this.cmdOK);
            this.flowLayoutPanel2.Controls.Add(this.cmdOKAdd);
            this.flowLayoutPanel2.Controls.Add(this.cmdCancel);
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(363, 394);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(237, 23);
            this.flowLayoutPanel2.TabIndex = 27;
            // 
            // frmSelectCritterPower
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectCritterPower";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectCritterPower";
            this.Text = "Select a Critter Power";
            this.Load += new System.EventHandler(this.frmSelectCritterPower_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudCritterPowerRating)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

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
        private Chummer.BufferedTableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label lblRatingNALabel;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearchLabel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
    }
}
