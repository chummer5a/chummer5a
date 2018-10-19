namespace Chummer
{
    partial class frmInitRoller
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
            this.nudDiceResult = new System.Windows.Forms.NumericUpDown();
            this.lblDice = new System.Windows.Forms.Label();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lblDescription = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.nudDiceResult)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // nudDiceResult
            // 
            this.nudDiceResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudDiceResult.Location = new System.Drawing.Point(158, 31);
            this.nudDiceResult.Name = "nudDiceResult";
            this.nudDiceResult.Size = new System.Drawing.Size(50, 20);
            this.nudDiceResult.TabIndex = 7;
            // 
            // lblDice
            // 
            this.lblDice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDice.AutoSize = true;
            this.lblDice.Location = new System.Drawing.Point(77, 34);
            this.lblDice.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDice.Name = "lblDice";
            this.lblDice.Size = new System.Drawing.Size(75, 13);
            this.lblDice.TabIndex = 6;
            this.lblDice.Tag = "Label_InitRoller_ResultOf";
            this.lblDice.Text = "Result of 5D6:";
            this.lblDice.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cmdOK.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.cmdOK, 3);
            this.cmdOK.Location = new System.Drawing.Point(145, 57);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 9;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(3, 6);
            this.lblDescription.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(88, 13);
            this.lblDescription.TabIndex = 5;
            this.lblDescription.Tag = "InitRoller_Description";
            this.lblDescription.Text = "[Text Goes Here]";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.cmdOK, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.nudDiceResult, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblDescription, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblDice, 0, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(9, 9);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(366, 83);
            this.tableLayoutPanel1.TabIndex = 10;
            // 
            // frmInitRoller
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(384, 101);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(400, 10000);
            this.MinimizeBox = false;
            this.Name = "frmInitRoller";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "InitRoller_Header";
            this.Text = "{InitativeRoller}";
            this.Load += new System.EventHandler(this.frmInitRoller_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudDiceResult)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown nudDiceResult;
        private System.Windows.Forms.Label lblDice;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
