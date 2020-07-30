namespace Chummer
{
    partial class frmSellItem
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
            this.lblSellForLabel = new System.Windows.Forms.Label();
            this.nudPercent = new System.Windows.Forms.NumericUpDown();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lblPercentLabel = new System.Windows.Forms.Label();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.flpButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.flpSellAmount = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.nudPercent)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.flpButtons.SuspendLayout();
            this.flpSellAmount.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblSellForLabel
            // 
            this.lblSellForLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSellForLabel.AutoSize = true;
            this.lblSellForLabel.Location = new System.Drawing.Point(3, 6);
            this.lblSellForLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSellForLabel.Name = "lblSellForLabel";
            this.lblSellForLabel.Size = new System.Drawing.Size(62, 13);
            this.lblSellForLabel.TabIndex = 0;
            this.lblSellForLabel.Tag = "Label_SellItem_SellItemFor";
            this.lblSellForLabel.Text = "Sell Item for";
            this.lblSellForLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudPercent
            // 
            this.nudPercent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudPercent.DecimalPlaces = 2;
            this.nudPercent.Location = new System.Drawing.Point(71, 3);
            this.nudPercent.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            131072});
            this.nudPercent.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudPercent.Name = "nudPercent";
            this.nudPercent.Size = new System.Drawing.Size(57, 20);
            this.nudPercent.TabIndex = 1;
            this.nudPercent.Value = new decimal(new int[] {
            50,
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
            this.cmdCancel.TabIndex = 4;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.AutoSize = true;
            this.cmdOK.Location = new System.Drawing.Point(81, 0);
            this.cmdOK.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 3;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lblPercentLabel
            // 
            this.lblPercentLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblPercentLabel.AutoSize = true;
            this.lblPercentLabel.Location = new System.Drawing.Point(134, 6);
            this.lblPercentLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPercentLabel.Name = "lblPercentLabel";
            this.lblPercentLabel.Size = new System.Drawing.Size(15, 13);
            this.lblPercentLabel.TabIndex = 2;
            this.lblPercentLabel.Text = "%";
            this.lblPercentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.flpButtons, 0, 1);
            this.tlpMain.Controls.Add(this.flpSellAmount, 0, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(206, 63);
            this.tlpMain.TabIndex = 5;
            // 
            // flpButtons
            // 
            this.flpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flpButtons.AutoSize = true;
            this.flpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpButtons.Controls.Add(this.cmdOK);
            this.flpButtons.Controls.Add(this.cmdCancel);
            this.flpButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpButtons.Location = new System.Drawing.Point(47, 37);
            this.flpButtons.Name = "flpButtons";
            this.flpButtons.Size = new System.Drawing.Size(156, 23);
            this.flpButtons.TabIndex = 3;
            // 
            // flpSellAmount
            // 
            this.flpSellAmount.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpSellAmount.AutoSize = true;
            this.flpSellAmount.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpSellAmount.Controls.Add(this.lblSellForLabel);
            this.flpSellAmount.Controls.Add(this.nudPercent);
            this.flpSellAmount.Controls.Add(this.lblPercentLabel);
            this.flpSellAmount.Location = new System.Drawing.Point(0, 4);
            this.flpSellAmount.Margin = new System.Windows.Forms.Padding(0);
            this.flpSellAmount.Name = "flpSellAmount";
            this.flpSellAmount.Size = new System.Drawing.Size(152, 26);
            this.flpSellAmount.TabIndex = 4;
            // 
            // frmSellItem
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(224, 81);
            this.Controls.Add(this.tlpMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSellItem";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SellItem";
            this.Text = "Sell Item";
            ((System.ComponentModel.ISupportInitialize)(this.nudPercent)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.flpButtons.ResumeLayout(false);
            this.flpButtons.PerformLayout();
            this.flpSellAmount.ResumeLayout(false);
            this.flpSellAmount.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblSellForLabel;
        private System.Windows.Forms.NumericUpDown nudPercent;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Label lblPercentLabel;
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.FlowLayoutPanel flpButtons;
        private System.Windows.Forms.FlowLayoutPanel flpSellAmount;
    }
}
