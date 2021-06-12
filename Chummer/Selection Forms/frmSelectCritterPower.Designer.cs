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
            this.nudCritterPowerRating = new Chummer.NumericUpDownEx();
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
            this.tlpButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpRight = new Chummer.BufferedTableLayoutPanel(this.components);
            this.flpRating = new System.Windows.Forms.FlowLayoutPanel();
            this.lblRatingNALabel = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearchLabel = new System.Windows.Forms.Label();
            this.tlpLeft = new Chummer.BufferedTableLayoutPanel(this.components);
            this.bufferedTableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.nudCritterPowerRating)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            this.tlpRight.SuspendLayout();
            this.flpRating.SuspendLayout();
            this.tlpLeft.SuspendLayout();
            this.bufferedTableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCritterPowerSource
            // 
            this.lblCritterPowerSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCritterPowerSource.AutoSize = true;
            this.lblCritterPowerSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblCritterPowerSource.Location = new System.Drawing.Point(61, 157);
            this.lblCritterPowerSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerSource.Name = "lblCritterPowerSource";
            this.lblCritterPowerSource.Size = new System.Drawing.Size(47, 13);
            this.lblCritterPowerSource.TabIndex = 16;
            this.lblCritterPowerSource.Text = "[Source]";
            this.lblCritterPowerSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblCritterPowerSourceLabel
            // 
            this.lblCritterPowerSourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCritterPowerSourceLabel.AutoSize = true;
            this.lblCritterPowerSourceLabel.Location = new System.Drawing.Point(11, 157);
            this.lblCritterPowerSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerSourceLabel.Name = "lblCritterPowerSourceLabel";
            this.lblCritterPowerSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblCritterPowerSourceLabel.TabIndex = 15;
            this.lblCritterPowerSourceLabel.Tag = "Label_Source";
            this.lblCritterPowerSourceLabel.Text = "Source:";
            // 
            // lblCritterPowerDuration
            // 
            this.lblCritterPowerDuration.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCritterPowerDuration.AutoSize = true;
            this.lblCritterPowerDuration.Location = new System.Drawing.Point(61, 106);
            this.lblCritterPowerDuration.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerDuration.Name = "lblCritterPowerDuration";
            this.lblCritterPowerDuration.Size = new System.Drawing.Size(53, 13);
            this.lblCritterPowerDuration.TabIndex = 12;
            this.lblCritterPowerDuration.Text = "[Duration]";
            // 
            // lblCritterPowerDurationLabel
            // 
            this.lblCritterPowerDurationLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCritterPowerDurationLabel.AutoSize = true;
            this.lblCritterPowerDurationLabel.Location = new System.Drawing.Point(5, 106);
            this.lblCritterPowerDurationLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerDurationLabel.Name = "lblCritterPowerDurationLabel";
            this.lblCritterPowerDurationLabel.Size = new System.Drawing.Size(50, 13);
            this.lblCritterPowerDurationLabel.TabIndex = 11;
            this.lblCritterPowerDurationLabel.Tag = "Label_Duration";
            this.lblCritterPowerDurationLabel.Text = "Duration:";
            // 
            // lblCritterPowerRange
            // 
            this.lblCritterPowerRange.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCritterPowerRange.AutoSize = true;
            this.lblCritterPowerRange.Location = new System.Drawing.Point(61, 81);
            this.lblCritterPowerRange.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerRange.Name = "lblCritterPowerRange";
            this.lblCritterPowerRange.Size = new System.Drawing.Size(45, 13);
            this.lblCritterPowerRange.TabIndex = 10;
            this.lblCritterPowerRange.Text = "[Range]";
            // 
            // lblCritterPowerRangeLabel
            // 
            this.lblCritterPowerRangeLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCritterPowerRangeLabel.AutoSize = true;
            this.lblCritterPowerRangeLabel.Location = new System.Drawing.Point(13, 81);
            this.lblCritterPowerRangeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerRangeLabel.Name = "lblCritterPowerRangeLabel";
            this.lblCritterPowerRangeLabel.Size = new System.Drawing.Size(42, 13);
            this.lblCritterPowerRangeLabel.TabIndex = 9;
            this.lblCritterPowerRangeLabel.Tag = "Label_Range";
            this.lblCritterPowerRangeLabel.Text = "Range:";
            // 
            // lblCritterPowerAction
            // 
            this.lblCritterPowerAction.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCritterPowerAction.AutoSize = true;
            this.lblCritterPowerAction.Location = new System.Drawing.Point(61, 56);
            this.lblCritterPowerAction.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerAction.Name = "lblCritterPowerAction";
            this.lblCritterPowerAction.Size = new System.Drawing.Size(43, 13);
            this.lblCritterPowerAction.TabIndex = 8;
            this.lblCritterPowerAction.Text = "[Action]";
            // 
            // lblCritterPowerActionLabel
            // 
            this.lblCritterPowerActionLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCritterPowerActionLabel.AutoSize = true;
            this.lblCritterPowerActionLabel.Location = new System.Drawing.Point(15, 56);
            this.lblCritterPowerActionLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerActionLabel.Name = "lblCritterPowerActionLabel";
            this.lblCritterPowerActionLabel.Size = new System.Drawing.Size(40, 13);
            this.lblCritterPowerActionLabel.TabIndex = 7;
            this.lblCritterPowerActionLabel.Tag = "Label_Action";
            this.lblCritterPowerActionLabel.Text = "Action:";
            // 
            // lblCritterPowerType
            // 
            this.lblCritterPowerType.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCritterPowerType.AutoSize = true;
            this.lblCritterPowerType.Location = new System.Drawing.Point(61, 31);
            this.lblCritterPowerType.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerType.Name = "lblCritterPowerType";
            this.lblCritterPowerType.Size = new System.Drawing.Size(37, 13);
            this.lblCritterPowerType.TabIndex = 6;
            this.lblCritterPowerType.Text = "[Type]";
            // 
            // lblCritterPowerTypeLabel
            // 
            this.lblCritterPowerTypeLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCritterPowerTypeLabel.AutoSize = true;
            this.lblCritterPowerTypeLabel.Location = new System.Drawing.Point(21, 31);
            this.lblCritterPowerTypeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerTypeLabel.Name = "lblCritterPowerTypeLabel";
            this.lblCritterPowerTypeLabel.Size = new System.Drawing.Size(34, 13);
            this.lblCritterPowerTypeLabel.TabIndex = 5;
            this.lblCritterPowerTypeLabel.Tag = "Label_Type";
            this.lblCritterPowerTypeLabel.Text = "Type:";
            // 
            // lblCritterPowerCategory
            // 
            this.lblCritterPowerCategory.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCritterPowerCategory.AutoSize = true;
            this.lblCritterPowerCategory.Location = new System.Drawing.Point(61, 6);
            this.lblCritterPowerCategory.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerCategory.Name = "lblCritterPowerCategory";
            this.lblCritterPowerCategory.Size = new System.Drawing.Size(55, 13);
            this.lblCritterPowerCategory.TabIndex = 4;
            this.lblCritterPowerCategory.Text = "[Category]";
            // 
            // lblCritterPowerCategoryLabel
            // 
            this.lblCritterPowerCategoryLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCritterPowerCategoryLabel.AutoSize = true;
            this.lblCritterPowerCategoryLabel.Location = new System.Drawing.Point(3, 6);
            this.lblCritterPowerCategoryLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerCategoryLabel.Name = "lblCritterPowerCategoryLabel";
            this.lblCritterPowerCategoryLabel.Size = new System.Drawing.Size(52, 13);
            this.lblCritterPowerCategoryLabel.TabIndex = 3;
            this.lblCritterPowerCategoryLabel.Tag = "Label_Category";
            this.lblCritterPowerCategoryLabel.Text = "Category:";
            // 
            // lblCritterPowerRatingLabel
            // 
            this.lblCritterPowerRatingLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCritterPowerRatingLabel.AutoSize = true;
            this.lblCritterPowerRatingLabel.Location = new System.Drawing.Point(14, 131);
            this.lblCritterPowerRatingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerRatingLabel.Name = "lblCritterPowerRatingLabel";
            this.lblCritterPowerRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblCritterPowerRatingLabel.TabIndex = 13;
            this.lblCritterPowerRatingLabel.Tag = "Label_Rating";
            this.lblCritterPowerRatingLabel.Text = "Rating:";
            // 
            // nudCritterPowerRating
            // 
            this.nudCritterPowerRating.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudCritterPowerRating.AutoSize = true;
            this.nudCritterPowerRating.Enabled = false;
            this.nudCritterPowerRating.Location = new System.Drawing.Point(3, 3);
            this.nudCritterPowerRating.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudCritterPowerRating.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudCritterPowerRating.Name = "nudCritterPowerRating";
            this.nudCritterPowerRating.Size = new System.Drawing.Size(41, 20);
            this.nudCritterPowerRating.TabIndex = 14;
            this.nudCritterPowerRating.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
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
            this.cmdCancel.TabIndex = 20;
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
            this.cmdOK.TabIndex = 19;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // trePowers
            // 
            this.tlpLeft.SetColumnSpan(this.trePowers, 2);
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
            this.trePowers.DoubleClick += new System.EventHandler(this.cmdOK_Click);
            // 
            // lblCategory
            // 
            this.lblCategory.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(3, 7);
            this.lblCategory.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(52, 13);
            this.lblCategory.TabIndex = 1;
            this.lblCategory.Tag = "Label_Category";
            this.lblCategory.Text = "Category:";
            // 
            // cboCategory
            // 
            this.cboCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategory.FormattingEnabled = true;
            this.cboCategory.Location = new System.Drawing.Point(61, 3);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(237, 21);
            this.cboCategory.TabIndex = 2;
            this.cboCategory.TooltipText = "";
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // lblPowerPoints
            // 
            this.lblPowerPoints.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblPowerPoints.AutoSize = true;
            this.lblPowerPoints.Location = new System.Drawing.Point(61, 182);
            this.lblPowerPoints.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPowerPoints.Name = "lblPowerPoints";
            this.lblPowerPoints.Size = new System.Drawing.Size(75, 13);
            this.lblPowerPoints.TabIndex = 18;
            this.lblPowerPoints.Text = "[Power Points]";
            this.lblPowerPoints.Visible = false;
            // 
            // lblPowerPointsLabel
            // 
            this.lblPowerPointsLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblPowerPointsLabel.AutoSize = true;
            this.lblPowerPointsLabel.Location = new System.Drawing.Point(16, 182);
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
            this.cmdOKAdd.AutoSize = true;
            this.cmdOKAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOKAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOKAdd.Location = new System.Drawing.Point(81, 3);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(72, 23);
            this.cmdOKAdd.TabIndex = 21;
            this.cmdOKAdd.Tag = "String_AddMore";
            this.cmdOKAdd.Text = "&Add && More";
            this.cmdOKAdd.UseVisualStyleBackColor = true;
            this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
            // 
            // lblKarma
            // 
            this.lblKarma.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarma.AutoSize = true;
            this.lblKarma.Location = new System.Drawing.Point(61, 207);
            this.lblKarma.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarma.Name = "lblKarma";
            this.lblKarma.Size = new System.Drawing.Size(43, 13);
            this.lblKarma.TabIndex = 23;
            this.lblKarma.Text = "[Karma]";
            // 
            // lblKarmaLabel
            // 
            this.lblKarmaLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaLabel.AutoSize = true;
            this.lblKarmaLabel.Location = new System.Drawing.Point(15, 207);
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
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.Controls.Add(this.tlpButtons, 1, 2);
            this.tlpMain.Controls.Add(this.tlpLeft, 0, 0);
            this.tlpMain.Controls.Add(this.tlpRight, 1, 1);
            this.tlpMain.Controls.Add(this.bufferedTableLayoutPanel1, 1, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 3;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(606, 423);
            this.tlpMain.TabIndex = 24;
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
            this.tlpButtons.TabIndex = 29;
            // 
            // tlpRight
            // 
            this.tlpRight.AutoSize = true;
            this.tlpRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.ColumnCount = 2;
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.Controls.Add(this.lblKarma, 1, 8);
            this.tlpRight.Controls.Add(this.flpRating, 1, 5);
            this.tlpRight.Controls.Add(this.lblKarmaLabel, 0, 8);
            this.tlpRight.Controls.Add(this.lblPowerPoints, 1, 7);
            this.tlpRight.Controls.Add(this.lblPowerPointsLabel, 0, 7);
            this.tlpRight.Controls.Add(this.lblCritterPowerCategoryLabel, 0, 0);
            this.tlpRight.Controls.Add(this.lblCritterPowerSource, 1, 6);
            this.tlpRight.Controls.Add(this.lblCritterPowerTypeLabel, 0, 1);
            this.tlpRight.Controls.Add(this.lblCritterPowerSourceLabel, 0, 6);
            this.tlpRight.Controls.Add(this.lblCritterPowerType, 1, 1);
            this.tlpRight.Controls.Add(this.lblCritterPowerDuration, 1, 4);
            this.tlpRight.Controls.Add(this.lblCritterPowerDurationLabel, 0, 4);
            this.tlpRight.Controls.Add(this.lblCritterPowerRatingLabel, 0, 5);
            this.tlpRight.Controls.Add(this.lblCritterPowerRange, 1, 3);
            this.tlpRight.Controls.Add(this.lblCritterPowerRangeLabel, 0, 3);
            this.tlpRight.Controls.Add(this.lblCritterPowerAction, 1, 2);
            this.tlpRight.Controls.Add(this.lblCritterPowerActionLabel, 0, 2);
            this.tlpRight.Controls.Add(this.lblCritterPowerCategory, 1, 0);
            this.tlpRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRight.Location = new System.Drawing.Point(303, 26);
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
            this.tlpRight.Size = new System.Drawing.Size(303, 226);
            this.tlpRight.TabIndex = 30;
            // 
            // flpRating
            // 
            this.flpRating.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpRating.AutoSize = true;
            this.flpRating.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpRating.Controls.Add(this.nudCritterPowerRating);
            this.flpRating.Controls.Add(this.lblRatingNALabel);
            this.flpRating.Location = new System.Drawing.Point(58, 125);
            this.flpRating.Margin = new System.Windows.Forms.Padding(0);
            this.flpRating.Name = "flpRating";
            this.flpRating.Size = new System.Drawing.Size(80, 26);
            this.flpRating.TabIndex = 25;
            this.flpRating.WrapContents = false;
            // 
            // lblRatingNALabel
            // 
            this.lblRatingNALabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblRatingNALabel.AutoSize = true;
            this.lblRatingNALabel.Location = new System.Drawing.Point(50, 6);
            this.lblRatingNALabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRatingNALabel.Name = "lblRatingNALabel";
            this.lblRatingNALabel.Size = new System.Drawing.Size(27, 13);
            this.lblRatingNALabel.TabIndex = 16;
            this.lblRatingNALabel.Tag = "String_NotApplicable";
            this.lblRatingNALabel.Text = "N/A";
            this.lblRatingNALabel.Visible = false;
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(53, 3);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(247, 20);
            this.txtSearch.TabIndex = 26;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // lblSearchLabel
            // 
            this.lblSearchLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSearchLabel.AutoSize = true;
            this.lblSearchLabel.Location = new System.Drawing.Point(3, 6);
            this.lblSearchLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSearchLabel.Name = "lblSearchLabel";
            this.lblSearchLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSearchLabel.TabIndex = 25;
            this.lblSearchLabel.Tag = "Label_Search";
            this.lblSearchLabel.Text = "&Search:";
            // 
            // tlpLeft
            // 
            this.tlpLeft.AutoSize = true;
            this.tlpLeft.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpLeft.ColumnCount = 2;
            this.tlpLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLeft.Controls.Add(this.cboCategory, 1, 0);
            this.tlpLeft.Controls.Add(this.lblCategory, 0, 0);
            this.tlpLeft.Controls.Add(this.trePowers, 0, 1);
            this.tlpLeft.Location = new System.Drawing.Point(0, 0);
            this.tlpLeft.Margin = new System.Windows.Forms.Padding(0);
            this.tlpLeft.Name = "tlpLeft";
            this.tlpLeft.RowCount = 2;
            this.tlpMain.SetRowSpan(this.tlpLeft, 3);
            this.tlpLeft.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLeft.Size = new System.Drawing.Size(301, 423);
            this.tlpLeft.TabIndex = 31;
            // 
            // bufferedTableLayoutPanel1
            // 
            this.bufferedTableLayoutPanel1.AutoSize = true;
            this.bufferedTableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bufferedTableLayoutPanel1.ColumnCount = 2;
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bufferedTableLayoutPanel1.Controls.Add(this.txtSearch, 1, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.lblSearchLabel, 0, 0);
            this.bufferedTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedTableLayoutPanel1.Location = new System.Drawing.Point(303, 0);
            this.bufferedTableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.bufferedTableLayoutPanel1.Name = "bufferedTableLayoutPanel1";
            this.bufferedTableLayoutPanel1.RowCount = 1;
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bufferedTableLayoutPanel1.Size = new System.Drawing.Size(303, 26);
            this.bufferedTableLayoutPanel1.TabIndex = 32;
            // 
            // frmSelectCritterPower
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
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
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.tlpRight.ResumeLayout(false);
            this.tlpRight.PerformLayout();
            this.flpRating.ResumeLayout(false);
            this.flpRating.PerformLayout();
            this.tlpLeft.ResumeLayout(false);
            this.tlpLeft.PerformLayout();
            this.bufferedTableLayoutPanel1.ResumeLayout(false);
            this.bufferedTableLayoutPanel1.PerformLayout();
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
        private Chummer.NumericUpDownEx nudCritterPowerRating;
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
        private System.Windows.Forms.FlowLayoutPanel flpRating;
        private System.Windows.Forms.Label lblRatingNALabel;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearchLabel;
        private BufferedTableLayoutPanel tlpButtons;
        private BufferedTableLayoutPanel tlpRight;
        private BufferedTableLayoutPanel tlpLeft;
        private BufferedTableLayoutPanel bufferedTableLayoutPanel1;
    }
}
