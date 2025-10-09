using System.Threading;

namespace Chummer
{
    partial class SelectCyberware
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
                CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objProcessGradeChangedCancellationTokenSource, null);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                objOldCancellationTokenSource = Interlocked.Exchange(ref _objUpdateCyberwareInfoCancellationTokenSource, null);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                objOldCancellationTokenSource = Interlocked.Exchange(ref _objDoRefreshSelectedCyberwareCancellationTokenSource, null);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                objOldCancellationTokenSource = Interlocked.Exchange(ref _objDoRefreshListCancellationTokenSource, null);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                _objGenericCancellationTokenSource.Dispose();
                Utils.StringHashSetPool.Return(ref _setBlackMarketMaps);
                Utils.StringHashSetPool.Return(ref _setDisallowedGrades);
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
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.chkHideOverAvailLimit = new Chummer.ColorableCheckBox();
            this.tlpFilterPanels = new System.Windows.Forms.TableLayoutPanel();
            this.gpbEssenceFilter = new System.Windows.Forms.GroupBox();
            this.tlpEssenceFilter = new System.Windows.Forms.TableLayoutPanel();
            this.lblMinimumEssence = new System.Windows.Forms.Label();
            this.lblMaximumEssence = new System.Windows.Forms.Label();
            this.lblExactEssence = new System.Windows.Forms.Label();
            this.nudMinimumEssence = new Chummer.NumericUpDownEx();
            this.nudMaximumEssence = new Chummer.NumericUpDownEx();
            this.nudExactEssence = new Chummer.NumericUpDownEx();
            this.chkUseCurrentEssence = new Chummer.ColorableCheckBox();
            this.gpbCostFilter = new System.Windows.Forms.GroupBox();
            this.tlpCostFilter = new System.Windows.Forms.TableLayoutPanel();
            this.lblMinimumCost = new System.Windows.Forms.Label();
            this.lblMaximumCost = new System.Windows.Forms.Label();
            this.lblExactCost = new System.Windows.Forms.Label();
            this.nudMinimumCost = new Chummer.NumericUpDownEx();
            this.nudMaximumCost = new Chummer.NumericUpDownEx();
            this.nudExactCost = new Chummer.NumericUpDownEx();
            this.chkUseCurrentNuyen = new Chummer.ColorableCheckBox();
            this.tlpLeft = new System.Windows.Forms.TableLayoutPanel();
            this.cboGrade = new Chummer.ElasticComboBox();
            this.lblCategory = new System.Windows.Forms.Label();
            this.cboCategory = new Chummer.ElasticComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lstCyberware = new System.Windows.Forms.ListBox();
            this.chkHideBannedGrades = new Chummer.ColorableCheckBox();
            this.tlpButtons = new System.Windows.Forms.TableLayoutPanel();
            this.cmdOKAdd = new System.Windows.Forms.Button();
            this.tlpRight = new System.Windows.Forms.TableLayoutPanel();
            this.lblEssence = new System.Windows.Forms.Label();
            this.flpMarkup = new System.Windows.Forms.FlowLayoutPanel();
            this.nudMarkup = new Chummer.NumericUpDownEx();
            this.lblMarkupPercentLabel = new System.Windows.Forms.Label();
            this.lblCyberwareNotesLabel = new System.Windows.Forms.Label();
            this.lblTest = new System.Windows.Forms.Label();
            this.lblMarkupLabel = new System.Windows.Forms.Label();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.lblTestLabel = new System.Windows.Forms.Label();
            this.lblCost = new System.Windows.Forms.Label();
            this.flpCheckBoxes = new System.Windows.Forms.FlowLayoutPanel();
            this.chkFree = new Chummer.ColorableCheckBox();
            this.chkBlackMarketDiscount = new Chummer.ColorableCheckBox();
            this.chkPrototypeTranshuman = new Chummer.ColorableCheckBox();
            this.lblCostLabel = new System.Windows.Forms.Label();
            this.lblEssenceLabel = new System.Windows.Forms.Label();
            this.lblRatingLabel = new System.Windows.Forms.Label();
            this.flpRating = new System.Windows.Forms.FlowLayoutPanel();
            this.nudRating = new Chummer.NumericUpDownEx();
            this.lblRatingNALabel = new System.Windows.Forms.Label();
            this.flpDiscount = new System.Windows.Forms.FlowLayoutPanel();
            this.lblESSDiscountLabel = new System.Windows.Forms.Label();
            this.nudESSDiscount = new Chummer.NumericUpDownEx();
            this.lblESSDiscountPercentLabel = new System.Windows.Forms.Label();
            this.lblCapacityLabel = new System.Windows.Forms.Label();
            this.lblCapacity = new System.Windows.Forms.Label();
            this.lblAvailLabel = new System.Windows.Forms.Label();
            this.lblAvail = new System.Windows.Forms.Label();
            this.lblMaximumCapacity = new System.Windows.Forms.Label();
            this.pnlNotes = new System.Windows.Forms.Panel();
            this.lblCyberwareNotes = new System.Windows.Forms.Label();
            this.tlpTopRight = new System.Windows.Forms.TableLayoutPanel();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearchLabel = new System.Windows.Forms.Label();
            this.tlpMain.SuspendLayout();
            this.tlpFilterPanels.SuspendLayout();
            this.gpbEssenceFilter.SuspendLayout();
            this.tlpEssenceFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinimumEssence)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaximumEssence)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudExactEssence)).BeginInit();
            this.gpbCostFilter.SuspendLayout();
            this.tlpCostFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinimumCost)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaximumCost)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudExactCost)).BeginInit();
            this.tlpLeft.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            this.tlpRight.SuspendLayout();
            this.flpMarkup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).BeginInit();
            this.flpCheckBoxes.SuspendLayout();
            this.flpRating.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRating)).BeginInit();
            this.flpDiscount.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudESSDiscount)).BeginInit();
            this.pnlNotes.SuspendLayout();
            this.tlpTopRight.SuspendLayout();
            this.SuspendLayout();
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
            this.cmdOK.TabIndex = 27;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
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
            this.cmdCancel.TabIndex = 29;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tlpMain.Controls.Add(this.chkHideOverAvailLimit, 1, 3);
            this.tlpMain.Controls.Add(this.tlpFilterPanels, 1, 4);
            this.tlpMain.Controls.Add(this.tlpLeft, 0, 0);
            this.tlpMain.Controls.Add(this.chkHideBannedGrades, 1, 2);
            this.tlpMain.Controls.Add(this.tlpButtons, 1, 5);
            this.tlpMain.Controls.Add(this.tlpRight, 1, 1);
            this.tlpMain.Controls.Add(this.tlpTopRight, 1, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 6;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(766, 543);
            this.tlpMain.TabIndex = 68;
            // 
            // chkHideOverAvailLimit
            // 
            this.chkHideOverAvailLimit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkHideOverAvailLimit.AutoSize = true;
            this.chkHideOverAvailLimit.DefaultColorScheme = true;
            this.chkHideOverAvailLimit.Location = new System.Drawing.Point(309, 373);
            this.chkHideOverAvailLimit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkHideOverAvailLimit.Name = "chkHideOverAvailLimit";
            this.chkHideOverAvailLimit.Size = new System.Drawing.Size(175, 17);
            this.chkHideOverAvailLimit.TabIndex = 65;
            this.chkHideOverAvailLimit.Tag = "Checkbox_HideOverAvailLimit";
            this.chkHideOverAvailLimit.Text = "Hide Items Over Avail Limit ({0})";
            this.chkHideOverAvailLimit.ToolTipText = "";
            this.chkHideOverAvailLimit.UseVisualStyleBackColor = true;
            this.chkHideOverAvailLimit.CheckedChanged += new System.EventHandler(this.RefreshCurrentList);
            // 
            // tlpFilterPanels
            // 
            this.tlpFilterPanels.AutoSize = true;
            this.tlpFilterPanels.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpFilterPanels.ColumnCount = 2;
            this.tlpFilterPanels.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpFilterPanels.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpFilterPanels.Controls.Add(this.gpbEssenceFilter, 0, 0);
            this.tlpFilterPanels.Controls.Add(this.gpbCostFilter, 1, 0);
            this.tlpFilterPanels.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpFilterPanels.Location = new System.Drawing.Point(309, 397);
            this.tlpFilterPanels.Name = "tlpFilterPanels";
            this.tlpFilterPanels.RowCount = 1;
            this.tlpFilterPanels.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpFilterPanels.Size = new System.Drawing.Size(454, 114);
            this.tlpFilterPanels.TabIndex = 73;
            // 
            // gpbEssenceFilter
            // 
            this.gpbEssenceFilter.AutoSize = true;
            this.gpbEssenceFilter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbEssenceFilter.Controls.Add(this.tlpEssenceFilter);
            this.gpbEssenceFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpbEssenceFilter.Location = new System.Drawing.Point(3, 3);
            this.gpbEssenceFilter.Name = "gpbEssenceFilter";
            this.gpbEssenceFilter.Size = new System.Drawing.Size(221, 108);
            this.gpbEssenceFilter.TabIndex = 73;
            this.gpbEssenceFilter.TabStop = false;
            this.gpbEssenceFilter.Tag = "Label_FilterByEssence";
            this.gpbEssenceFilter.Text = "Filter by Essence";
            // 
            // tlpEssenceFilter
            // 
            this.tlpEssenceFilter.AutoSize = true;
            this.tlpEssenceFilter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpEssenceFilter.ColumnCount = 3;
            this.tlpEssenceFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpEssenceFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpEssenceFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpEssenceFilter.Controls.Add(this.lblMinimumEssence, 0, 0);
            this.tlpEssenceFilter.Controls.Add(this.lblMaximumEssence, 0, 1);
            this.tlpEssenceFilter.Controls.Add(this.lblExactEssence, 0, 2);
            this.tlpEssenceFilter.Controls.Add(this.nudMinimumEssence, 1, 0);
            this.tlpEssenceFilter.Controls.Add(this.nudMaximumEssence, 1, 1);
            this.tlpEssenceFilter.Controls.Add(this.nudExactEssence, 1, 2);
            this.tlpEssenceFilter.Controls.Add(this.chkUseCurrentEssence, 2, 1);
            this.tlpEssenceFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpEssenceFilter.Location = new System.Drawing.Point(3, 16);
            this.tlpEssenceFilter.Name = "tlpEssenceFilter";
            this.tlpEssenceFilter.RowCount = 3;
            this.tlpEssenceFilter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpEssenceFilter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpEssenceFilter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpEssenceFilter.Size = new System.Drawing.Size(215, 89);
            this.tlpEssenceFilter.TabIndex = 0;
            // 
            // lblMinimumEssence
            // 
            this.lblMinimumEssence.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMinimumEssence.AutoSize = true;
            this.lblMinimumEssence.Location = new System.Drawing.Point(3, 6);
            this.lblMinimumEssence.Name = "lblMinimumEssence";
            this.lblMinimumEssence.Size = new System.Drawing.Size(51, 13);
            this.lblMinimumEssence.TabIndex = 0;
            this.lblMinimumEssence.Tag = "Label_Minimum";
            this.lblMinimumEssence.Text = "Minimum:";
            // 
            // lblMaximumEssence
            // 
            this.lblMaximumEssence.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMaximumEssence.AutoSize = true;
            this.lblMaximumEssence.Location = new System.Drawing.Point(3, 32);
            this.lblMaximumEssence.Name = "lblMaximumEssence";
            this.lblMaximumEssence.Size = new System.Drawing.Size(54, 13);
            this.lblMaximumEssence.TabIndex = 1;
            this.lblMaximumEssence.Tag = "Label_Maximum";
            this.lblMaximumEssence.Text = "Maximum:";
            // 
            // lblExactEssence
            // 
            this.lblExactEssence.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblExactEssence.AutoSize = true;
            this.lblExactEssence.Location = new System.Drawing.Point(3, 64);
            this.lblExactEssence.Name = "lblExactEssence";
            this.lblExactEssence.Size = new System.Drawing.Size(37, 13);
            this.lblExactEssence.TabIndex = 2;
            this.lblExactEssence.Tag = "Label_Exact";
            this.lblExactEssence.Text = "Exact:";
            // 
            // nudMinimumEssence
            // 
            this.nudMinimumEssence.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudMinimumEssence.AutoSize = true;
            this.nudMinimumEssence.DecimalPlaces = 2;
            this.nudMinimumEssence.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudMinimumEssence.Location = new System.Drawing.Point(63, 3);
            this.nudMinimumEssence.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudMinimumEssence.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMinimumEssence.Name = "nudMinimumEssence";
            this.nudMinimumEssence.Size = new System.Drawing.Size(50, 20);
            this.nudMinimumEssence.TabIndex = 2;
            this.nudMinimumEssence.ValueChanged += new System.EventHandler(this.EssenceCostFilter);
            // 
            // nudMaximumEssence
            // 
            this.nudMaximumEssence.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudMaximumEssence.AutoSize = true;
            this.nudMaximumEssence.DecimalPlaces = 2;
            this.nudMaximumEssence.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudMaximumEssence.Location = new System.Drawing.Point(63, 29);
            this.nudMaximumEssence.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudMaximumEssence.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMaximumEssence.Name = "nudMaximumEssence";
            this.nudMaximumEssence.Size = new System.Drawing.Size(50, 20);
            this.nudMaximumEssence.TabIndex = 3;
            this.nudMaximumEssence.ValueChanged += new System.EventHandler(this.EssenceCostFilter);
            // 
            // nudExactEssence
            // 
            this.nudExactEssence.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudExactEssence.AutoSize = true;
            this.nudExactEssence.DecimalPlaces = 2;
            this.nudExactEssence.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudExactEssence.Location = new System.Drawing.Point(63, 60);
            this.nudExactEssence.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudExactEssence.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudExactEssence.Name = "nudExactEssence";
            this.nudExactEssence.Size = new System.Drawing.Size(50, 20);
            this.nudExactEssence.TabIndex = 5;
            this.nudExactEssence.ValueChanged += new System.EventHandler(this.EssenceCostFilter);
            // 
            // chkUseCurrentEssence
            // 
            this.chkUseCurrentEssence.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkUseCurrentEssence.AutoSize = true;
            this.chkUseCurrentEssence.DefaultColorScheme = true;
            this.chkUseCurrentEssence.Location = new System.Drawing.Point(119, 30);
            this.chkUseCurrentEssence.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkUseCurrentEssence.Name = "chkUseCurrentEssence";
            this.chkUseCurrentEssence.Size = new System.Drawing.Size(104, 17);
            this.chkUseCurrentEssence.TabIndex = 6;
            this.chkUseCurrentEssence.Tag = "Checkbox_UseCurrentEssence";
            this.chkUseCurrentEssence.Text = "Current Essence";
            this.chkUseCurrentEssence.ToolTipText = "";
            this.chkUseCurrentEssence.UseVisualStyleBackColor = true;
            this.chkUseCurrentEssence.CheckedChanged += new System.EventHandler(this.chkUseCurrentEssence_CheckedChanged);
            // 
            // gpbCostFilter
            // 
            this.gpbCostFilter.AutoSize = true;
            this.gpbCostFilter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbCostFilter.Controls.Add(this.tlpCostFilter);
            this.gpbCostFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpbCostFilter.Location = new System.Drawing.Point(230, 3);
            this.gpbCostFilter.Name = "gpbCostFilter";
            this.gpbCostFilter.Size = new System.Drawing.Size(221, 108);
            this.gpbCostFilter.TabIndex = 74;
            this.gpbCostFilter.TabStop = false;
            this.gpbCostFilter.Tag = "Label_FilterByCost";
            this.gpbCostFilter.Text = "Filter by Cost (¥)";
            // 
            // tlpCostFilter
            // 
            this.tlpCostFilter.AutoSize = true;
            this.tlpCostFilter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpCostFilter.ColumnCount = 3;
            this.tlpCostFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCostFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCostFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCostFilter.Controls.Add(this.lblMinimumCost, 0, 0);
            this.tlpCostFilter.Controls.Add(this.lblMaximumCost, 0, 1);
            this.tlpCostFilter.Controls.Add(this.lblExactCost, 0, 2);
            this.tlpCostFilter.Controls.Add(this.nudMinimumCost, 1, 0);
            this.tlpCostFilter.Controls.Add(this.nudMaximumCost, 1, 1);
            this.tlpCostFilter.Controls.Add(this.nudExactCost, 1, 2);
            this.tlpCostFilter.Controls.Add(this.chkUseCurrentNuyen, 2, 1);
            this.tlpCostFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCostFilter.Location = new System.Drawing.Point(3, 16);
            this.tlpCostFilter.Name = "tlpCostFilter";
            this.tlpCostFilter.RowCount = 3;
            this.tlpCostFilter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCostFilter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCostFilter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCostFilter.Size = new System.Drawing.Size(215, 89);
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
            this.lblMaximumCost.Location = new System.Drawing.Point(3, 32);
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
            this.lblExactCost.Location = new System.Drawing.Point(3, 64);
            this.lblExactCost.Name = "lblExactCost";
            this.lblExactCost.Size = new System.Drawing.Size(37, 13);
            this.lblExactCost.TabIndex = 2;
            this.lblExactCost.Tag = "Label_Exact";
            this.lblExactCost.Text = "Exact:";
            // 
            // nudMinimumCost
            // 
            this.nudMinimumCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudMinimumCost.AutoSize = true;
            this.nudMinimumCost.Location = new System.Drawing.Point(63, 3);
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
            this.nudMinimumCost.Size = new System.Drawing.Size(59, 20);
            this.nudMinimumCost.TabIndex = 3;
            this.nudMinimumCost.ValueChanged += new System.EventHandler(this.EssenceCostFilter);
            // 
            // nudMaximumCost
            // 
            this.nudMaximumCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudMaximumCost.AutoSize = true;
            this.nudMaximumCost.Location = new System.Drawing.Point(63, 29);
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
            this.nudMaximumCost.Size = new System.Drawing.Size(59, 20);
            this.nudMaximumCost.TabIndex = 4;
            this.nudMaximumCost.ValueChanged += new System.EventHandler(this.EssenceCostFilter);
            // 
            // nudExactCost
            // 
            this.nudExactCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudExactCost.AutoSize = true;
            this.nudExactCost.Location = new System.Drawing.Point(63, 60);
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
            this.nudExactCost.Size = new System.Drawing.Size(59, 20);
            this.nudExactCost.TabIndex = 5;
            this.nudExactCost.ValueChanged += new System.EventHandler(this.EssenceCostFilter);
            // 
            // chkUseCurrentNuyen
            // 
            this.chkUseCurrentNuyen.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkUseCurrentNuyen.AutoSize = true;
            this.chkUseCurrentNuyen.DefaultColorScheme = true;
            this.chkUseCurrentNuyen.Location = new System.Drawing.Point(128, 30);
            this.chkUseCurrentNuyen.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkUseCurrentNuyen.Name = "chkUseCurrentNuyen";
            this.chkUseCurrentNuyen.Size = new System.Drawing.Size(69, 17);
            this.chkUseCurrentNuyen.TabIndex = 6;
            this.chkUseCurrentNuyen.Tag = "Checkbox_UseCurrentNuyen";
            this.chkUseCurrentNuyen.Text = "Current ¥";
            this.chkUseCurrentNuyen.ToolTipText = "";
            this.chkUseCurrentNuyen.UseVisualStyleBackColor = true;
            this.chkUseCurrentNuyen.CheckedChanged += new System.EventHandler(this.chkUseCurrentNuyen_CheckedChanged);
            // 
            // tlpLeft
            // 
            this.tlpLeft.ColumnCount = 2;
            this.tlpLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpLeft.Controls.Add(this.cboGrade, 1, 1);
            this.tlpLeft.Controls.Add(this.lblCategory, 0, 0);
            this.tlpLeft.Controls.Add(this.cboCategory, 1, 0);
            this.tlpLeft.Controls.Add(this.label1, 0, 1);
            this.tlpLeft.Controls.Add(this.lstCyberware, 0, 2);
            this.tlpLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpLeft.Location = new System.Drawing.Point(0, 0);
            this.tlpLeft.Margin = new System.Windows.Forms.Padding(0);
            this.tlpLeft.Name = "tlpLeft";
            this.tlpLeft.RowCount = 5;
            this.tlpMain.SetRowSpan(this.tlpLeft, 5);
            this.tlpLeft.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpLeft.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpLeft.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpLeft.Size = new System.Drawing.Size(306, 514);
            this.tlpLeft.TabIndex = 75;
            // 
            // cboGrade
            // 
            this.cboGrade.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboGrade.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGrade.FormattingEnabled = true;
            this.cboGrade.Location = new System.Drawing.Point(61, 30);
            this.cboGrade.Name = "cboGrade";
            this.cboGrade.Size = new System.Drawing.Size(242, 21);
            this.cboGrade.TabIndex = 25;
            this.cboGrade.SelectedIndexChanged += new System.EventHandler(this.cboGrade_SelectedIndexChanged);
            this.cboGrade.EnabledChanged += new System.EventHandler(this.cboGrade_EnabledChanged);
            // 
            // lblCategory
            // 
            this.lblCategory.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(3, 7);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(52, 13);
            this.lblCategory.TabIndex = 22;
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
            this.cboCategory.Size = new System.Drawing.Size(242, 21);
            this.cboCategory.TabIndex = 23;
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 34);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 24;
            this.label1.Tag = "Label_Grade";
            this.label1.Text = "Grade:";
            // 
            // lstCyberware
            // 
            this.tlpLeft.SetColumnSpan(this.lstCyberware, 2);
            this.lstCyberware.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstCyberware.FormattingEnabled = true;
            this.lstCyberware.Location = new System.Drawing.Point(3, 57);
            this.lstCyberware.Name = "lstCyberware";
            this.tlpLeft.SetRowSpan(this.lstCyberware, 3);
            this.lstCyberware.Size = new System.Drawing.Size(300, 483);
            this.lstCyberware.TabIndex = 26;
            this.lstCyberware.SelectedIndexChanged += new System.EventHandler(this.lstCyberware_SelectedIndexChanged);
            this.lstCyberware.DoubleClick += new System.EventHandler(this.cmdOK_Click);
            // 
            // chkHideBannedGrades
            // 
            this.chkHideBannedGrades.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkHideBannedGrades.AutoSize = true;
            this.chkHideBannedGrades.Checked = true;
            this.chkHideBannedGrades.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkHideBannedGrades.DefaultColorScheme = true;
            this.chkHideBannedGrades.Location = new System.Drawing.Point(309, 348);
            this.chkHideBannedGrades.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkHideBannedGrades.Name = "chkHideBannedGrades";
            this.chkHideBannedGrades.Size = new System.Drawing.Size(178, 17);
            this.chkHideBannedGrades.TabIndex = 67;
            this.chkHideBannedGrades.Tag = "Checkbox_HideBannedCyberwareGrades";
            this.chkHideBannedGrades.Text = "Hide Banned Cyberware Grades";
            this.chkHideBannedGrades.ToolTipText = "";
            this.chkHideBannedGrades.UseVisualStyleBackColor = true;
            this.chkHideBannedGrades.CheckedChanged += new System.EventHandler(this.chkHideBannedGrades_CheckedChanged);
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
            this.tlpButtons.Location = new System.Drawing.Point(508, 514);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButtons.Size = new System.Drawing.Size(258, 29);
            this.tlpButtons.TabIndex = 78;
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
            this.cmdOKAdd.TabIndex = 28;
            this.cmdOKAdd.Tag = "String_AddMore";
            this.cmdOKAdd.Text = "&Add && More";
            this.cmdOKAdd.UseVisualStyleBackColor = true;
            this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
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
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpRight.Controls.Add(this.lblEssence, 1, 2);
            this.tlpRight.Controls.Add(this.flpMarkup, 3, 6);
            this.tlpRight.Controls.Add(this.lblCyberwareNotesLabel, 0, 9);
            this.tlpRight.Controls.Add(this.lblTest, 3, 5);
            this.tlpRight.Controls.Add(this.lblMarkupLabel, 2, 6);
            this.tlpRight.Controls.Add(this.lblSource, 1, 8);
            this.tlpRight.Controls.Add(this.lblSourceLabel, 0, 8);
            this.tlpRight.Controls.Add(this.lblTestLabel, 2, 5);
            this.tlpRight.Controls.Add(this.lblCost, 1, 6);
            this.tlpRight.Controls.Add(this.flpCheckBoxes, 0, 7);
            this.tlpRight.Controls.Add(this.lblCostLabel, 0, 6);
            this.tlpRight.Controls.Add(this.lblEssenceLabel, 0, 2);
            this.tlpRight.Controls.Add(this.lblRatingLabel, 0, 1);
            this.tlpRight.Controls.Add(this.flpRating, 1, 1);
            this.tlpRight.Controls.Add(this.flpDiscount, 2, 2);
            this.tlpRight.Controls.Add(this.lblCapacityLabel, 0, 3);
            this.tlpRight.Controls.Add(this.lblCapacity, 1, 3);
            this.tlpRight.Controls.Add(this.lblAvailLabel, 0, 5);
            this.tlpRight.Controls.Add(this.lblAvail, 1, 5);
            this.tlpRight.Controls.Add(this.lblMaximumCapacity, 0, 4);
            this.tlpRight.Controls.Add(this.pnlNotes, 1, 9);
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
            this.tlpRight.Size = new System.Drawing.Size(460, 318);
            this.tlpRight.TabIndex = 79;
            // 
            // lblEssence
            // 
            this.lblEssence.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblEssence.AutoSize = true;
            this.lblEssence.Location = new System.Drawing.Point(60, 32);
            this.lblEssence.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblEssence.Name = "lblEssence";
            this.lblEssence.Size = new System.Drawing.Size(19, 13);
            this.lblEssence.TabIndex = 5;
            this.lblEssence.Text = "[0]";
            this.lblEssence.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flpMarkup
            // 
            this.flpMarkup.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpMarkup.AutoSize = true;
            this.flpMarkup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpMarkup.Controls.Add(this.nudMarkup);
            this.flpMarkup.Controls.Add(this.lblMarkupPercentLabel);
            this.flpMarkup.Location = new System.Drawing.Point(284, 127);
            this.flpMarkup.Margin = new System.Windows.Forms.Padding(0);
            this.flpMarkup.Name = "flpMarkup";
            this.flpMarkup.Size = new System.Drawing.Size(83, 26);
            this.flpMarkup.TabIndex = 16;
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
            99,
            0,
            0,
            -2147483648});
            this.nudMarkup.Name = "nudMarkup";
            this.nudMarkup.Size = new System.Drawing.Size(56, 20);
            this.nudMarkup.TabIndex = 41;
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
            this.lblMarkupPercentLabel.TabIndex = 42;
            this.lblMarkupPercentLabel.Text = "%";
            this.lblMarkupPercentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCyberwareNotesLabel
            // 
            this.lblCyberwareNotesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCyberwareNotesLabel.AutoSize = true;
            this.lblCyberwareNotesLabel.Location = new System.Drawing.Point(16, 209);
            this.lblCyberwareNotesLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberwareNotesLabel.Name = "lblCyberwareNotesLabel";
            this.lblCyberwareNotesLabel.Size = new System.Drawing.Size(38, 13);
            this.lblCyberwareNotesLabel.TabIndex = 30;
            this.lblCyberwareNotesLabel.Tag = "Menu_Notes";
            this.lblCyberwareNotesLabel.Text = "Notes:";
            this.lblCyberwareNotesLabel.Visible = false;
            // 
            // lblTest
            // 
            this.lblTest.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblTest.AutoSize = true;
            this.lblTest.Location = new System.Drawing.Point(287, 108);
            this.lblTest.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTest.Name = "lblTest";
            this.lblTest.Size = new System.Drawing.Size(19, 13);
            this.lblTest.TabIndex = 14;
            this.lblTest.Text = "[0]";
            this.lblTest.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMarkupLabel
            // 
            this.lblMarkupLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMarkupLabel.AutoSize = true;
            this.lblMarkupLabel.Location = new System.Drawing.Point(235, 133);
            this.lblMarkupLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMarkupLabel.Name = "lblMarkupLabel";
            this.lblMarkupLabel.Size = new System.Drawing.Size(46, 13);
            this.lblMarkupLabel.TabIndex = 40;
            this.lblMarkupLabel.Tag = "Label_SelectGear_Markup";
            this.lblMarkupLabel.Text = "Markup:";
            this.lblMarkupLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
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
            this.lblSource.TabIndex = 21;
            this.lblSource.Text = "[Source]";
            this.lblSource.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.lblSourceLabel.TabIndex = 20;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            this.lblSourceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblTestLabel
            // 
            this.lblTestLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblTestLabel.AutoSize = true;
            this.lblTestLabel.Location = new System.Drawing.Point(250, 108);
            this.lblTestLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTestLabel.Name = "lblTestLabel";
            this.lblTestLabel.Size = new System.Drawing.Size(31, 13);
            this.lblTestLabel.TabIndex = 13;
            this.lblTestLabel.Tag = "Label_Test";
            this.lblTestLabel.Text = "Test:";
            this.lblTestLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblCost
            // 
            this.lblCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCost.AutoSize = true;
            this.lblCost.Location = new System.Drawing.Point(60, 133);
            this.lblCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCost.Name = "lblCost";
            this.lblCost.Size = new System.Drawing.Size(19, 13);
            this.lblCost.TabIndex = 16;
            this.lblCost.Text = "[0]";
            this.lblCost.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flpCheckBoxes
            // 
            this.flpCheckBoxes.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpCheckBoxes.AutoSize = true;
            this.flpCheckBoxes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.SetColumnSpan(this.flpCheckBoxes, 4);
            this.flpCheckBoxes.Controls.Add(this.chkFree);
            this.flpCheckBoxes.Controls.Add(this.chkBlackMarketDiscount);
            this.flpCheckBoxes.Controls.Add(this.chkPrototypeTranshuman);
            this.flpCheckBoxes.Location = new System.Drawing.Point(0, 153);
            this.flpCheckBoxes.Margin = new System.Windows.Forms.Padding(0);
            this.flpCheckBoxes.Name = "flpCheckBoxes";
            this.flpCheckBoxes.Size = new System.Drawing.Size(364, 25);
            this.flpCheckBoxes.TabIndex = 77;
            // 
            // chkFree
            // 
            this.chkFree.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkFree.AutoSize = true;
            this.chkFree.DefaultColorScheme = true;
            this.chkFree.Location = new System.Drawing.Point(3, 4);
            this.chkFree.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkFree.Name = "chkFree";
            this.chkFree.Size = new System.Drawing.Size(50, 17);
            this.chkFree.TabIndex = 17;
            this.chkFree.Tag = "Checkbox_Free";
            this.chkFree.Text = "Free!";
            this.chkFree.ToolTipText = "";
            this.chkFree.UseVisualStyleBackColor = true;
            this.chkFree.CheckedChanged += new System.EventHandler(this.chkFree_CheckedChanged);
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
            this.chkBlackMarketDiscount.ToolTipText = "";
            this.chkBlackMarketDiscount.UseVisualStyleBackColor = true;
            this.chkBlackMarketDiscount.Visible = false;
            this.chkBlackMarketDiscount.CheckedChanged += new System.EventHandler(this.ProcessCyberwareInfoChanged);
            // 
            // chkPrototypeTranshuman
            // 
            this.chkPrototypeTranshuman.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkPrototypeTranshuman.AutoSize = true;
            this.chkPrototypeTranshuman.DefaultColorScheme = true;
            this.chkPrototypeTranshuman.Location = new System.Drawing.Point(228, 4);
            this.chkPrototypeTranshuman.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkPrototypeTranshuman.Name = "chkPrototypeTranshuman";
            this.chkPrototypeTranshuman.Size = new System.Drawing.Size(133, 17);
            this.chkPrototypeTranshuman.TabIndex = 66;
            this.chkPrototypeTranshuman.Tag = "Checkbox_PrototypeTranshuman";
            this.chkPrototypeTranshuman.Text = "Prototype Transhuman";
            this.chkPrototypeTranshuman.ToolTipText = "";
            this.chkPrototypeTranshuman.UseVisualStyleBackColor = true;
            this.chkPrototypeTranshuman.Visible = false;
            this.chkPrototypeTranshuman.CheckedChanged += new System.EventHandler(this.ProcessCyberwareInfoChanged);
            // 
            // lblCostLabel
            // 
            this.lblCostLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCostLabel.AutoSize = true;
            this.lblCostLabel.Location = new System.Drawing.Point(23, 133);
            this.lblCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCostLabel.Name = "lblCostLabel";
            this.lblCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblCostLabel.TabIndex = 15;
            this.lblCostLabel.Tag = "Label_Cost";
            this.lblCostLabel.Text = "Cost:";
            this.lblCostLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblEssenceLabel
            // 
            this.lblEssenceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblEssenceLabel.AutoSize = true;
            this.lblEssenceLabel.Location = new System.Drawing.Point(3, 32);
            this.lblEssenceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblEssenceLabel.Name = "lblEssenceLabel";
            this.lblEssenceLabel.Size = new System.Drawing.Size(51, 13);
            this.lblEssenceLabel.TabIndex = 4;
            this.lblEssenceLabel.Tag = "Label_Essence";
            this.lblEssenceLabel.Text = "Essence:";
            this.lblEssenceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblRatingLabel
            // 
            this.lblRatingLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblRatingLabel.AutoSize = true;
            this.lblRatingLabel.Location = new System.Drawing.Point(13, 6);
            this.lblRatingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRatingLabel.Name = "lblRatingLabel";
            this.lblRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblRatingLabel.TabIndex = 2;
            this.lblRatingLabel.Tag = "Label_Rating";
            this.lblRatingLabel.Text = "Rating:";
            this.lblRatingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // flpRating
            // 
            this.flpRating.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpRating.AutoSize = true;
            this.flpRating.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.SetColumnSpan(this.flpRating, 3);
            this.flpRating.Controls.Add(this.nudRating);
            this.flpRating.Controls.Add(this.lblRatingNALabel);
            this.flpRating.Location = new System.Drawing.Point(57, 0);
            this.flpRating.Margin = new System.Windows.Forms.Padding(0);
            this.flpRating.Name = "flpRating";
            this.flpRating.Size = new System.Drawing.Size(80, 26);
            this.flpRating.TabIndex = 73;
            this.flpRating.WrapContents = false;
            // 
            // nudRating
            // 
            this.nudRating.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudRating.AutoSize = true;
            this.nudRating.Location = new System.Drawing.Point(3, 3);
            this.nudRating.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudRating.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudRating.Name = "nudRating";
            this.nudRating.Size = new System.Drawing.Size(41, 20);
            this.nudRating.TabIndex = 3;
            this.nudRating.ValueChanged += new System.EventHandler(this.ProcessCyberwareInfoChanged);
            // 
            // lblRatingNALabel
            // 
            this.lblRatingNALabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblRatingNALabel.AutoSize = true;
            this.lblRatingNALabel.Location = new System.Drawing.Point(50, 6);
            this.lblRatingNALabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRatingNALabel.Name = "lblRatingNALabel";
            this.lblRatingNALabel.Size = new System.Drawing.Size(27, 13);
            this.lblRatingNALabel.TabIndex = 15;
            this.lblRatingNALabel.Tag = "String_NotApplicable";
            this.lblRatingNALabel.Text = "N/A";
            this.lblRatingNALabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblRatingNALabel.Visible = false;
            // 
            // flpDiscount
            // 
            this.flpDiscount.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpDiscount.AutoSize = true;
            this.flpDiscount.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.SetColumnSpan(this.flpDiscount, 2);
            this.flpDiscount.Controls.Add(this.lblESSDiscountLabel);
            this.flpDiscount.Controls.Add(this.nudESSDiscount);
            this.flpDiscount.Controls.Add(this.lblESSDiscountPercentLabel);
            this.flpDiscount.Location = new System.Drawing.Point(232, 26);
            this.flpDiscount.Margin = new System.Windows.Forms.Padding(0);
            this.flpDiscount.Name = "flpDiscount";
            this.flpDiscount.Size = new System.Drawing.Size(185, 26);
            this.flpDiscount.TabIndex = 76;
            this.flpDiscount.WrapContents = false;
            // 
            // lblESSDiscountLabel
            // 
            this.lblESSDiscountLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblESSDiscountLabel.AutoSize = true;
            this.lblESSDiscountLabel.Location = new System.Drawing.Point(3, 6);
            this.lblESSDiscountLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblESSDiscountLabel.Name = "lblESSDiscountLabel";
            this.lblESSDiscountLabel.Size = new System.Drawing.Size(96, 13);
            this.lblESSDiscountLabel.TabIndex = 6;
            this.lblESSDiscountLabel.Tag = "Label_SelectCyberware_ESSDiscount";
            this.lblESSDiscountLabel.Text = "Essence Discount:";
            this.lblESSDiscountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudESSDiscount
            // 
            this.nudESSDiscount.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudESSDiscount.AutoSize = true;
            this.nudESSDiscount.DecimalPlaces = 2;
            this.nudESSDiscount.Location = new System.Drawing.Point(105, 3);
            this.nudESSDiscount.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudESSDiscount.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.nudESSDiscount.Name = "nudESSDiscount";
            this.nudESSDiscount.Size = new System.Drawing.Size(56, 20);
            this.nudESSDiscount.TabIndex = 7;
            this.nudESSDiscount.ValueChanged += new System.EventHandler(this.ProcessCyberwareInfoChanged);
            // 
            // lblESSDiscountPercentLabel
            // 
            this.lblESSDiscountPercentLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblESSDiscountPercentLabel.AutoSize = true;
            this.lblESSDiscountPercentLabel.Location = new System.Drawing.Point(167, 6);
            this.lblESSDiscountPercentLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblESSDiscountPercentLabel.Name = "lblESSDiscountPercentLabel";
            this.lblESSDiscountPercentLabel.Size = new System.Drawing.Size(15, 13);
            this.lblESSDiscountPercentLabel.TabIndex = 8;
            this.lblESSDiscountPercentLabel.Text = "%";
            this.lblESSDiscountPercentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCapacityLabel
            // 
            this.lblCapacityLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCapacityLabel.AutoSize = true;
            this.lblCapacityLabel.Location = new System.Drawing.Point(3, 58);
            this.lblCapacityLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCapacityLabel.Name = "lblCapacityLabel";
            this.lblCapacityLabel.Size = new System.Drawing.Size(51, 13);
            this.lblCapacityLabel.TabIndex = 9;
            this.lblCapacityLabel.Tag = "Label_Capacity";
            this.lblCapacityLabel.Text = "Capacity:";
            this.lblCapacityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblCapacity
            // 
            this.lblCapacity.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCapacity.AutoSize = true;
            this.tlpRight.SetColumnSpan(this.lblCapacity, 3);
            this.lblCapacity.Location = new System.Drawing.Point(60, 58);
            this.lblCapacity.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCapacity.Name = "lblCapacity";
            this.lblCapacity.Size = new System.Drawing.Size(19, 13);
            this.lblCapacity.TabIndex = 10;
            this.lblCapacity.Text = "[0]";
            this.lblCapacity.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAvailLabel
            // 
            this.lblAvailLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblAvailLabel.AutoSize = true;
            this.lblAvailLabel.Location = new System.Drawing.Point(21, 108);
            this.lblAvailLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAvailLabel.Name = "lblAvailLabel";
            this.lblAvailLabel.Size = new System.Drawing.Size(33, 13);
            this.lblAvailLabel.TabIndex = 11;
            this.lblAvailLabel.Tag = "Label_Avail";
            this.lblAvailLabel.Text = "Avail:";
            this.lblAvailLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblAvail
            // 
            this.lblAvail.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblAvail.AutoSize = true;
            this.lblAvail.Location = new System.Drawing.Point(60, 108);
            this.lblAvail.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAvail.Name = "lblAvail";
            this.lblAvail.Size = new System.Drawing.Size(19, 13);
            this.lblAvail.TabIndex = 12;
            this.lblAvail.Text = "[0]";
            this.lblAvail.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMaximumCapacity
            // 
            this.lblMaximumCapacity.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMaximumCapacity.AutoSize = true;
            this.tlpRight.SetColumnSpan(this.lblMaximumCapacity, 4);
            this.lblMaximumCapacity.Location = new System.Drawing.Point(3, 83);
            this.lblMaximumCapacity.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMaximumCapacity.Name = "lblMaximumCapacity";
            this.lblMaximumCapacity.Size = new System.Drawing.Size(101, 13);
            this.lblMaximumCapacity.TabIndex = 19;
            this.lblMaximumCapacity.Text = "[Maximum Capacity]";
            // 
            // pnlNotes
            // 
            this.pnlNotes.AutoScroll = true;
            this.pnlNotes.AutoSize = true;
            this.tlpRight.SetColumnSpan(this.pnlNotes, 3);
            this.pnlNotes.Controls.Add(this.lblCyberwareNotes);
            this.pnlNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlNotes.Location = new System.Drawing.Point(57, 203);
            this.pnlNotes.Margin = new System.Windows.Forms.Padding(0);
            this.pnlNotes.Name = "pnlNotes";
            this.pnlNotes.Padding = new System.Windows.Forms.Padding(3, 6, 13, 3);
            this.pnlNotes.Size = new System.Drawing.Size(403, 115);
            this.pnlNotes.TabIndex = 78;
            // 
            // lblCyberwareNotes
            // 
            this.lblCyberwareNotes.AutoSize = true;
            this.lblCyberwareNotes.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblCyberwareNotes.Location = new System.Drawing.Point(3, 6);
            this.lblCyberwareNotes.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberwareNotes.Name = "lblCyberwareNotes";
            this.lblCyberwareNotes.Size = new System.Drawing.Size(41, 13);
            this.lblCyberwareNotes.TabIndex = 31;
            this.lblCyberwareNotes.Text = "[Notes]";
            this.lblCyberwareNotes.Visible = false;
            // 
            // tlpTopRight
            // 
            this.tlpTopRight.AutoSize = true;
            this.tlpTopRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpTopRight.ColumnCount = 2;
            this.tlpTopRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpTopRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTopRight.Controls.Add(this.txtSearch, 1, 0);
            this.tlpTopRight.Controls.Add(this.lblSearchLabel, 0, 0);
            this.tlpTopRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTopRight.Location = new System.Drawing.Point(306, 0);
            this.tlpTopRight.Margin = new System.Windows.Forms.Padding(0);
            this.tlpTopRight.Name = "tlpTopRight";
            this.tlpTopRight.RowCount = 1;
            this.tlpTopRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTopRight.Size = new System.Drawing.Size(460, 26);
            this.tlpTopRight.TabIndex = 80;
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
            // 
            // SelectCyberware
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
            this.Name = "SelectCyberware";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectCyberware";
            this.Text = "Select Cyberware";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SelectCyberware_Closing);
            this.Load += new System.EventHandler(this.SelectCyberware_Load);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpFilterPanels.ResumeLayout(false);
            this.tlpFilterPanels.PerformLayout();
            this.gpbEssenceFilter.ResumeLayout(false);
            this.gpbEssenceFilter.PerformLayout();
            this.tlpEssenceFilter.ResumeLayout(false);
            this.tlpEssenceFilter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinimumEssence)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaximumEssence)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudExactEssence)).EndInit();
            this.gpbCostFilter.ResumeLayout(false);
            this.gpbCostFilter.PerformLayout();
            this.tlpCostFilter.ResumeLayout(false);
            this.tlpCostFilter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinimumCost)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaximumCost)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudExactCost)).EndInit();
            this.tlpLeft.ResumeLayout(false);
            this.tlpLeft.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.tlpRight.ResumeLayout(false);
            this.tlpRight.PerformLayout();
            this.flpMarkup.ResumeLayout(false);
            this.flpMarkup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).EndInit();
            this.flpCheckBoxes.ResumeLayout(false);
            this.flpCheckBoxes.PerformLayout();
            this.flpRating.ResumeLayout(false);
            this.flpRating.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRating)).EndInit();
            this.flpDiscount.ResumeLayout(false);
            this.flpDiscount.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudESSDiscount)).EndInit();
            this.pnlNotes.ResumeLayout(false);
            this.pnlNotes.PerformLayout();
            this.tlpTopRight.ResumeLayout(false);
            this.tlpTopRight.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ElasticComboBox cboCategory;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.Label label1;
        private ElasticComboBox cboGrade;
        private System.Windows.Forms.ListBox lstCyberware;
        private System.Windows.Forms.Label lblEssenceLabel;
        private System.Windows.Forms.Label lblEssence;
        private System.Windows.Forms.Label lblCapacity;
        private System.Windows.Forms.Label lblCapacityLabel;
        private System.Windows.Forms.Label lblAvail;
        private System.Windows.Forms.Label lblAvailLabel;
        private System.Windows.Forms.Label lblCost;
        private System.Windows.Forms.Label lblCostLabel;
        private System.Windows.Forms.Label lblRatingLabel;
        private Chummer.NumericUpDownEx nudRating;
        private System.Windows.Forms.Label lblMaximumCapacity;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblSourceLabel;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearchLabel;
        private Chummer.ColorableCheckBox chkFree;
        private Chummer.NumericUpDownEx nudESSDiscount;
        private Chummer.ColorableCheckBox chkUseCurrentEssence;
        private Chummer.ColorableCheckBox chkUseCurrentNuyen;
        private System.Windows.Forms.Label lblESSDiscountLabel;
        private System.Windows.Forms.Label lblESSDiscountPercentLabel;
        private System.Windows.Forms.Label lblTest;
        private System.Windows.Forms.Label lblTestLabel;
        private System.Windows.Forms.Label lblCyberwareNotes;
        private System.Windows.Forms.Label lblCyberwareNotesLabel;
        private Chummer.ColorableCheckBox chkBlackMarketDiscount;
        private Chummer.NumericUpDownEx nudMarkup;
        private System.Windows.Forms.Label lblMarkupLabel;
        private System.Windows.Forms.Label lblMarkupPercentLabel;
        private Chummer.ColorableCheckBox chkHideOverAvailLimit;
        private Chummer.ColorableCheckBox chkPrototypeTranshuman;
        private Chummer.ColorableCheckBox chkHideBannedGrades;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.FlowLayoutPanel flpRating;
        private System.Windows.Forms.Label lblRatingNALabel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdOKAdd;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.FlowLayoutPanel flpMarkup;
        private System.Windows.Forms.FlowLayoutPanel flpDiscount;
        private System.Windows.Forms.FlowLayoutPanel flpCheckBoxes;
        private System.Windows.Forms.TableLayoutPanel tlpButtons;
        private System.Windows.Forms.TableLayoutPanel tlpRight;
        private System.Windows.Forms.TableLayoutPanel tlpLeft;
        private System.Windows.Forms.TableLayoutPanel tlpTopRight;
        private System.Windows.Forms.Panel pnlNotes;
        private System.Windows.Forms.TableLayoutPanel tlpFilterPanels;
        private System.Windows.Forms.GroupBox gpbEssenceFilter;
        private System.Windows.Forms.TableLayoutPanel tlpEssenceFilter;
        private System.Windows.Forms.Label lblMinimumEssence;
        private System.Windows.Forms.Label lblMaximumEssence;
        private System.Windows.Forms.Label lblExactEssence;
        private Chummer.NumericUpDownEx nudMinimumEssence;
        private Chummer.NumericUpDownEx nudMaximumEssence;
        private Chummer.NumericUpDownEx nudExactEssence;
        private System.Windows.Forms.GroupBox gpbCostFilter;
        private System.Windows.Forms.TableLayoutPanel tlpCostFilter;
        private System.Windows.Forms.Label lblMinimumCost;
        private System.Windows.Forms.Label lblMaximumCost;
        private System.Windows.Forms.Label lblExactCost;
        private Chummer.NumericUpDownEx nudMinimumCost;
        private Chummer.NumericUpDownEx nudMaximumCost;
        private Chummer.NumericUpDownEx nudExactCost;
    }
}
