namespace Chummer
{
    partial class SelectCalendarStart
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
            this.lblYear = new System.Windows.Forms.Label();
            this.nudYear = new Chummer.NumericUpDownEx();
            this.lblWeek = new System.Windows.Forms.Label();
            this.nudWeek = new Chummer.NumericUpDownEx();
            this.lblCalendarStart = new System.Windows.Forms.Label();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.lblDateSpan = new System.Windows.Forms.Label();
            this.tlpButtons = new System.Windows.Forms.TableLayoutPanel();
            this.lblWeekSpansFollowingDates = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudYear)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWeek)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblYear
            // 
            this.lblYear.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblYear.AutoSize = true;
            this.lblYear.Location = new System.Drawing.Point(3, 44);
            this.lblYear.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblYear.Name = "lblYear";
            this.lblYear.Size = new System.Drawing.Size(32, 13);
            this.lblYear.TabIndex = 1;
            this.lblYear.Tag = "Label_Year";
            this.lblYear.Text = "Year:";
            // 
            // nudYear
            // 
            this.nudYear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudYear.AutoSize = true;
            this.nudYear.Location = new System.Drawing.Point(41, 41);
            this.nudYear.Maximum = new decimal(new int[] {
            9000,
            0,
            0,
            0});
            this.nudYear.Minimum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.nudYear.Name = "nudYear";
            this.nudYear.Size = new System.Drawing.Size(95, 20);
            this.nudYear.TabIndex = 2;
            this.nudYear.Value = new decimal(new int[] {
            2072,
            0,
            0,
            0});
            // 
            // lblWeek
            // 
            this.lblWeek.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblWeek.AutoSize = true;
            this.lblWeek.Location = new System.Drawing.Point(142, 44);
            this.lblWeek.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeek.Name = "lblWeek";
            this.lblWeek.Size = new System.Drawing.Size(39, 13);
            this.lblWeek.TabIndex = 5;
            this.lblWeek.Tag = "Label_Week";
            this.lblWeek.Text = "Week:";
            // 
            // nudWeek
            // 
            this.nudWeek.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudWeek.AutoSize = true;
            this.nudWeek.Location = new System.Drawing.Point(187, 41);
            this.nudWeek.Maximum = new decimal(new int[] {
            52,
            0,
            0,
            0});
            this.nudWeek.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudWeek.Name = "nudWeek";
            this.nudWeek.Size = new System.Drawing.Size(96, 20);
            this.nudWeek.TabIndex = 6;
            this.nudWeek.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblCalendarStart
            // 
            this.lblCalendarStart.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.lblCalendarStart, 4);
            this.lblCalendarStart.Location = new System.Drawing.Point(3, 6);
            this.lblCalendarStart.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCalendarStart.Name = "lblCalendarStart";
            this.lblCalendarStart.Size = new System.Drawing.Size(260, 26);
            this.lblCalendarStart.TabIndex = 0;
            this.lblCalendarStart.Tag = "Label_CalendarStart";
            this.lblCalendarStart.Text = "Enter the year and week number at which to start the calendar.";
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
            this.cmdCancel.TabIndex = 8;
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
            this.cmdOK.Location = new System.Drawing.Point(89, 3);
            this.cmdOK.MinimumSize = new System.Drawing.Size(80, 0);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(80, 23);
            this.cmdOK.TabIndex = 7;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 4;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.Controls.Add(this.lblDateSpan, 0, 3);
            this.tlpMain.Controls.Add(this.lblCalendarStart, 0, 0);
            this.tlpMain.Controls.Add(this.lblYear, 0, 1);
            this.tlpMain.Controls.Add(this.nudWeek, 3, 1);
            this.tlpMain.Controls.Add(this.nudYear, 1, 1);
            this.tlpMain.Controls.Add(this.lblWeek, 2, 1);
            this.tlpMain.Controls.Add(this.tlpButtons, 0, 4);
            this.tlpMain.Controls.Add(this.lblWeekSpansFollowingDates, 0, 2);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 5;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Size = new System.Drawing.Size(286, 163);
            this.tlpMain.TabIndex = 9;
            // 
            // lblDateSpan
            // 
            this.lblDateSpan.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblDateSpan.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.lblDateSpan, 4);
            this.lblDateSpan.Location = new System.Drawing.Point(56, 95);
            this.lblDateSpan.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDateSpan.Name = "lblDateSpan";
            this.lblDateSpan.Size = new System.Drawing.Size(174, 13);
            this.lblDateSpan.TabIndex = 10;
            this.lblDateSpan.Tag = "String_DateSpan";
            this.lblDateSpan.Text = "January 4, 2072 - January 10, 2072";
            // 
            // tlpButtons
            // 
            this.tlpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpButtons.AutoSize = true;
            this.tlpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpButtons.ColumnCount = 2;
            this.tlpMain.SetColumnSpan(this.tlpButtons, 4);
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.Controls.Add(this.cmdCancel, 0, 0);
            this.tlpButtons.Controls.Add(this.cmdOK, 1, 0);
            this.tlpButtons.Location = new System.Drawing.Point(114, 134);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButtons.Size = new System.Drawing.Size(172, 29);
            this.tlpButtons.TabIndex = 8;
            // 
            // lblWeekSpansFollowingDates
            // 
            this.lblWeekSpansFollowingDates.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.lblWeekSpansFollowingDates, 4);
            this.lblWeekSpansFollowingDates.Location = new System.Drawing.Point(3, 70);
            this.lblWeekSpansFollowingDates.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeekSpansFollowingDates.Name = "lblWeekSpansFollowingDates";
            this.lblWeekSpansFollowingDates.Size = new System.Drawing.Size(181, 13);
            this.lblWeekSpansFollowingDates.TabIndex = 9;
            this.lblWeekSpansFollowingDates.Tag = "Label_WeekSpansFollowingDates";
            this.lblWeekSpansFollowingDates.Text = "This week spans the following dates:";
            // 
            // SelectCalendarStart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(304, 181);
            this.Controls.Add(this.tlpMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectCalendarStart";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_CalendarStart";
            this.Text = "Calendar";
            this.Load += new System.EventHandler(this.UpdateDateSpan);
            ((System.ComponentModel.ISupportInitialize)(this.nudYear)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWeek)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblYear;
        private Chummer.NumericUpDownEx nudYear;
        private System.Windows.Forms.Label lblWeek;
        private Chummer.NumericUpDownEx nudWeek;
        private System.Windows.Forms.Label lblCalendarStart;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.TableLayoutPanel tlpButtons;
        private System.Windows.Forms.Label lblWeekSpansFollowingDates;
        private System.Windows.Forms.Label lblDateSpan;
    }
}
