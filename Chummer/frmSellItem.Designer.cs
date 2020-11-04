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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSellItem));
            this.lblSellForLabel = new System.Windows.Forms.Label();
            this.nudPercent = new Chummer.NumericUpDownEx();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lblPercentLabel = new System.Windows.Forms.Label();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.flpSellAmount = new System.Windows.Forms.FlowLayoutPanel();
            this.tlpButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.nudPercent)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.flpSellAmount.SuspendLayout();
            this.tlpButtons.SuspendLayout();
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
            this.nudPercent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudPercent.AutoSize = true;
            this.nudPercent.DecimalPlaces = 2;
            this.nudPercent.Location = new System.Drawing.Point(71, 3);
            this.nudPercent.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            131072});
            this.nudPercent.Name = "nudPercent";
            this.nudPercent.Size = new System.Drawing.Size(62, 20);
            this.nudPercent.TabIndex = 1;
            this.nudPercent.Value = new decimal(new int[] {
            50,
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
            this.cmdCancel.Size = new System.Drawing.Size(50, 23);
            this.cmdCancel.TabIndex = 4;
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
            this.cmdOK.Location = new System.Drawing.Point(59, 3);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(50, 23);
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
            this.lblPercentLabel.Location = new System.Drawing.Point(139, 6);
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
            this.tlpMain.Controls.Add(this.flpSellAmount, 0, 0);
            this.tlpMain.Controls.Add(this.tlpButtons, 0, 1);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(206, 63);
            this.tlpMain.TabIndex = 5;
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
            this.flpSellAmount.Size = new System.Drawing.Size(157, 26);
            this.flpSellAmount.TabIndex = 4;
            // 
            // tlpButtons
            // 
            this.tlpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpButtons.AutoSize = true;
            this.tlpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpButtons.ColumnCount = 2;
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.Controls.Add(this.cmdCancel, 0, 0);
            this.tlpButtons.Controls.Add(this.cmdOK, 1, 0);
            this.tlpButtons.Location = new System.Drawing.Point(94, 34);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.Size = new System.Drawing.Size(112, 29);
            this.tlpButtons.TabIndex = 5;
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
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
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
            this.flpSellAmount.ResumeLayout(false);
            this.flpSellAmount.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblSellForLabel;
        private Chummer.NumericUpDownEx nudPercent;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Label lblPercentLabel;
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.FlowLayoutPanel flpSellAmount;
        private BufferedTableLayoutPanel tlpButtons;
    }
}
