namespace Chummer
{
    partial class frmExpense
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
            ((System.ComponentModel.ISupportInitialize)(this.nudAmount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPercent)).BeginInit();
            this.SuspendLayout();
            // 
            // lblKarma
            // 
            this.lblKarma.AutoSize = true;
            this.lblKarma.Location = new System.Drawing.Point(12, 14);
            this.lblKarma.Name = "lblKarma";
            this.lblKarma.Size = new System.Drawing.Size(79, 13);
            this.lblKarma.TabIndex = 0;
            this.lblKarma.Tag = "Label_Expense_KarmaAmount";
            this.lblKarma.Text = "Karma Amount:";
            // 
            // nudAmount
            // 
            this.nudAmount.Location = new System.Drawing.Point(98, 12);
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
            this.nudAmount.Size = new System.Drawing.Size(69, 20);
            this.nudAmount.TabIndex = 1;
            this.nudAmount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(12, 41);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(63, 13);
            this.lblDescription.TabIndex = 6;
            this.lblDescription.Tag = "Label_Expense_Description";
            this.lblDescription.Text = "Description:";
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(98, 38);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(437, 20);
            this.txtDescription.TabIndex = 7;
            this.txtDescription.Text = "Mission Reward";
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(379, 80);
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
            this.cmdOK.Location = new System.Drawing.Point(460, 80);
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
            this.chkRefund.AutoSize = true;
            this.chkRefund.Location = new System.Drawing.Point(98, 64);
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
            this.datDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.datDate.Location = new System.Drawing.Point(284, 12);
            this.datDate.Name = "datDate";
            this.datDate.Size = new System.Drawing.Size(251, 20);
            this.datDate.TabIndex = 5;
            // 
            // lblDateLabel
            // 
            this.lblDateLabel.AutoSize = true;
            this.lblDateLabel.Location = new System.Drawing.Point(245, 14);
            this.lblDateLabel.Name = "lblDateLabel";
            this.lblDateLabel.Size = new System.Drawing.Size(33, 13);
            this.lblDateLabel.TabIndex = 4;
            this.lblDateLabel.Tag = "Label_Expense_Date";
            this.lblDateLabel.Text = "Date:";
            // 
            // nudPercent
            // 
            this.nudPercent.Location = new System.Drawing.Point(173, 12);
            this.nudPercent.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudPercent.Name = "nudPercent";
            this.nudPercent.Size = new System.Drawing.Size(48, 20);
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
            this.lblPercent.AutoSize = true;
            this.lblPercent.Location = new System.Drawing.Point(219, 14);
            this.lblPercent.Name = "lblPercent";
            this.lblPercent.Size = new System.Drawing.Size(15, 13);
            this.lblPercent.TabIndex = 3;
            this.lblPercent.Text = "%";
            this.lblPercent.Visible = false;
            // 
            // chkKarmaNuyenExchange
            // 
            this.chkKarmaNuyenExchange.AutoSize = true;
            this.chkKarmaNuyenExchange.Location = new System.Drawing.Point(98, 84);
            this.chkKarmaNuyenExchange.Name = "chkKarmaNuyenExchange";
            this.chkKarmaNuyenExchange.Size = new System.Drawing.Size(161, 17);
            this.chkKarmaNuyenExchange.TabIndex = 11;
            this.chkKarmaNuyenExchange.Tag = "";
            this.chkKarmaNuyenExchange.Text = "Working for the Man/People";
            this.chkKarmaNuyenExchange.UseVisualStyleBackColor = true;
            this.chkKarmaNuyenExchange.CheckedChanged += new System.EventHandler(this.chkKarmaNuyenExchange_CheckedChanged);
            // 
            // frmExpense
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(547, 115);
            this.Controls.Add(this.chkKarmaNuyenExchange);
            this.Controls.Add(this.nudPercent);
            this.Controls.Add(this.lblDateLabel);
            this.Controls.Add(this.datDate);
            this.Controls.Add(this.chkRefund);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.nudAmount);
            this.Controls.Add(this.lblKarma);
            this.Controls.Add(this.lblPercent);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmExpense";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_Expense_Karma";
            this.Text = "Karmic Change";
            this.Load += new System.EventHandler(this.frmExpanse_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudAmount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPercent)).EndInit();
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
    }
}