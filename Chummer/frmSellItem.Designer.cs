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
            this.lblSellForLabel = new System.Windows.Forms.Label();
            this.nudPercent = new System.Windows.Forms.NumericUpDown();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lblPercentLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudPercent)).BeginInit();
            this.SuspendLayout();
            // 
            // lblSellForLabel
            // 
            this.lblSellForLabel.AutoSize = true;
            this.lblSellForLabel.Location = new System.Drawing.Point(21, 14);
            this.lblSellForLabel.Name = "lblSellForLabel";
            this.lblSellForLabel.Size = new System.Drawing.Size(62, 13);
            this.lblSellForLabel.TabIndex = 0;
            this.lblSellForLabel.Tag = "Label_SellItem_SellItemFor";
            this.lblSellForLabel.Text = "Sell Item for";
            // 
            // nudPercent
            // 
            this.nudPercent.Location = new System.Drawing.Point(89, 12);
            this.nudPercent.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.nudPercent.Name = "nudPercent";
            this.nudPercent.Size = new System.Drawing.Size(48, 20);
            this.nudPercent.TabIndex = 1;
            this.nudPercent.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(12, 55);
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
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.Location = new System.Drawing.Point(93, 55);
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
            this.lblPercentLabel.AutoSize = true;
            this.lblPercentLabel.Location = new System.Drawing.Point(143, 14);
            this.lblPercentLabel.Name = "lblPercentLabel";
            this.lblPercentLabel.Size = new System.Drawing.Size(15, 13);
            this.lblPercentLabel.TabIndex = 2;
            this.lblPercentLabel.Text = "%";
            // 
            // frmSellItem
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(179, 87);
            this.Controls.Add(this.lblPercentLabel);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.nudPercent);
            this.Controls.Add(this.lblSellForLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSellItem";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SellItem";
            this.Text = "Sell Item";
            ((System.ComponentModel.ISupportInitialize)(this.nudPercent)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblSellForLabel;
        private System.Windows.Forms.NumericUpDown nudPercent;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Label lblPercentLabel;
    }
}