namespace Chummer
{
    public sealed partial class frmExpense
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
            if (disposing && (components != null))
            {
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
            this.components = new System.ComponentModel.Container();
            this.lblKarma = new System.Windows.Forms.Label();
            this.nudAmount = new System.Windows.Forms.NumericUpDown();
            this.lblDescription = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.chkRefund = new System.Windows.Forms.CheckBox();
            this.datDate = new System.Windows.Forms.DateTimePicker();
            this.lblDateLabel = new System.Windows.Forms.Label();
            this.nudPercent = new System.Windows.Forms.NumericUpDown();
            this.lblPercent = new System.Windows.Forms.Label();
            this.chkKarmaNuyenExchange = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.flpAmount = new System.Windows.Forms.FlowLayoutPanel();
            this.flpButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.chkForceCareerVisible = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudAmount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPercent)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.flpAmount.SuspendLayout();
            this.flpButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblKarma
            // 
            this.lblKarma.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarma.AutoSize = true;
            this.lblKarma.Location = new System.Drawing.Point(3, 6);
            this.lblKarma.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarma.Name = "lblKarma";
            this.lblKarma.Size = new System.Drawing.Size(79, 13);
            this.lblKarma.TabIndex = 0;
            this.lblKarma.Tag = "Label_Expense_KarmaAmount";
            this.lblKarma.Text = "Karma Amount:";
            this.lblKarma.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudAmount
            // 
            this.nudAmount.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudAmount.AutoSize = true;
            this.nudAmount.Location = new System.Drawing.Point(3, 3);
            this.nudAmount.Maximum = new decimal(new int[] {
            9999999,
            0,
            0,
            0});
            this.nudAmount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudAmount.Name = "nudAmount";
            this.nudAmount.Size = new System.Drawing.Size(65, 20);
            this.nudAmount.TabIndex = 1;
            this.nudAmount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblDescription
            // 
            this.lblDescription.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(19, 58);
            this.lblDescription.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(63, 13);
            this.lblDescription.TabIndex = 6;
            this.lblDescription.Tag = "Label_Expense_Description";
            this.lblDescription.Text = "Description:";
            this.lblDescription.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtDescription
            // 
            this.txtDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDescription.Location = new System.Drawing.Point(88, 55);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(355, 20);
            this.txtDescription.TabIndex = 7;
            this.txtDescription.Text = "Mission Reward";
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdCancel.AutoSize = true;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(0, 0);
            this.cmdCancel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 10;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdOK.AutoSize = true;
            this.cmdOK.Location = new System.Drawing.Point(81, 0);
            this.cmdOK.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 9;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // chkRefund
            // 
            this.chkRefund.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkRefund.AutoSize = true;
            this.chkRefund.Location = new System.Drawing.Point(88, 81);
            this.chkRefund.Name = "chkRefund";
            this.chkRefund.Size = new System.Drawing.Size(275, 17);
            this.chkRefund.TabIndex = 8;
            this.chkRefund.Tag = "Checkbox_Expense_Refund";
            this.chkRefund.Text = "Refund (does not count towards Total Career Karma)";
            this.chkRefund.UseVisualStyleBackColor = true;
            // 
            // datDate
            // 
            this.datDate.CustomFormat = "ddddd, MMMM dd, yyyy hh:mm tt";
            this.datDate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.datDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.datDate.Location = new System.Drawing.Point(88, 29);
            this.datDate.Name = "datDate";
            this.datDate.Size = new System.Drawing.Size(355, 20);
            this.datDate.TabIndex = 5;
            // 
            // lblDateLabel
            // 
            this.lblDateLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDateLabel.AutoSize = true;
            this.lblDateLabel.Location = new System.Drawing.Point(49, 32);
            this.lblDateLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDateLabel.Name = "lblDateLabel";
            this.lblDateLabel.Size = new System.Drawing.Size(33, 13);
            this.lblDateLabel.TabIndex = 4;
            this.lblDateLabel.Tag = "Label_Expense_Date";
            this.lblDateLabel.Text = "Date:";
            this.lblDateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudPercent
            // 
            this.nudPercent.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudPercent.AutoSize = true;
            this.nudPercent.DecimalPlaces = 2;
            this.nudPercent.Location = new System.Drawing.Point(74, 3);
            this.nudPercent.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudPercent.Name = "nudPercent";
            this.nudPercent.Size = new System.Drawing.Size(56, 20);
            this.nudPercent.TabIndex = 2;
            this.nudPercent.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudPercent.Visible = false;
            // 
            // lblPercent
            // 
            this.lblPercent.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblPercent.AutoSize = true;
            this.lblPercent.Location = new System.Drawing.Point(136, 6);
            this.lblPercent.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPercent.Name = "lblPercent";
            this.lblPercent.Size = new System.Drawing.Size(15, 13);
            this.lblPercent.TabIndex = 3;
            this.lblPercent.Text = "%";
            this.lblPercent.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblPercent.Visible = false;
            // 
            // chkKarmaNuyenExchange
            // 
            this.chkKarmaNuyenExchange.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkKarmaNuyenExchange.AutoSize = true;
            this.chkKarmaNuyenExchange.Location = new System.Drawing.Point(88, 104);
            this.chkKarmaNuyenExchange.Name = "chkKarmaNuyenExchange";
            this.chkKarmaNuyenExchange.Size = new System.Drawing.Size(161, 17);
            this.chkKarmaNuyenExchange.TabIndex = 11;
            this.chkKarmaNuyenExchange.Tag = "String_WorkingForTheManPeople";
            this.chkKarmaNuyenExchange.Text = "Working for the Man/People";
            this.chkKarmaNuyenExchange.UseVisualStyleBackColor = true;
            this.chkKarmaNuyenExchange.CheckedChanged += new System.EventHandler(this.chkKarmaNuyenExchange_CheckedChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.chkKarmaNuyenExchange, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblKarma, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblDateLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.chkRefund, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.datDate, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblDescription, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.txtDescription, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.flpAmount, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.flpButtons, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.chkForceCareerVisible, 1, 5);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(9, 9);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 7;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(446, 183);
            this.tableLayoutPanel1.TabIndex = 12;
            // 
            // flpAmount
            // 
            this.flpAmount.AutoSize = true;
            this.flpAmount.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpAmount.Controls.Add(this.nudAmount);
            this.flpAmount.Controls.Add(this.nudPercent);
            this.flpAmount.Controls.Add(this.lblPercent);
            this.flpAmount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpAmount.Location = new System.Drawing.Point(85, 0);
            this.flpAmount.Margin = new System.Windows.Forms.Padding(0);
            this.flpAmount.Name = "flpAmount";
            this.flpAmount.Size = new System.Drawing.Size(361, 26);
            this.flpAmount.TabIndex = 13;
            // 
            // flpButtons
            // 
            this.flpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flpButtons.AutoSize = true;
            this.flpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.SetColumnSpan(this.flpButtons, 2);
            this.flpButtons.Controls.Add(this.cmdOK);
            this.flpButtons.Controls.Add(this.cmdCancel);
            this.flpButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpButtons.Location = new System.Drawing.Point(287, 157);
            this.flpButtons.Name = "flpButtons";
            this.flpButtons.Size = new System.Drawing.Size(156, 23);
            this.flpButtons.TabIndex = 12;
            // 
            // chkForceCareerVisible
            // 
            this.chkForceCareerVisible.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkForceCareerVisible.AutoSize = true;
            this.chkForceCareerVisible.Location = new System.Drawing.Point(88, 127);
            this.chkForceCareerVisible.Name = "chkForceCareerVisible";
            this.chkForceCareerVisible.Size = new System.Drawing.Size(167, 17);
            this.chkForceCareerVisible.TabIndex = 11;
            this.chkForceCareerVisible.Tag = "Checkbox_Expense_ForceCareerVisible";
            this.chkForceCareerVisible.Text = "Show in Career Karma/Nuyen";
            this.chkForceCareerVisible.UseVisualStyleBackColor = true;
            // 
            // frmExpense
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(464, 201);
            this.Controls.Add(this.tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmExpense";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_Expense_Karma";
            this.Text = "Karmic Change";
            this.Load += new System.EventHandler(this.frmExpanse_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudAmount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPercent)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flpAmount.ResumeLayout(false);
            this.flpAmount.PerformLayout();
            this.flpButtons.ResumeLayout(false);
            this.flpButtons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblKarma;
        private System.Windows.Forms.NumericUpDown nudAmount;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.CheckBox chkRefund;
        private System.Windows.Forms.DateTimePicker datDate;
        private System.Windows.Forms.Label lblDateLabel;
        private System.Windows.Forms.NumericUpDown nudPercent;
        private System.Windows.Forms.Label lblPercent;
        private System.Windows.Forms.CheckBox chkKarmaNuyenExchange;
        private Chummer.BufferedTableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flpButtons;
        private System.Windows.Forms.FlowLayoutPanel flpAmount;
        private System.Windows.Forms.CheckBox chkForceCareerVisible;
    }
}
