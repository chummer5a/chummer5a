namespace Chummer
{
    partial class frmLifestyleNuyen
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
            this.lblDescription = new System.Windows.Forms.Label();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lblDice = new System.Windows.Forms.Label();
            this.lblResult = new System.Windows.Forms.Label();
            this.nudDiceResult = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.nudDiceResult)).BeginInit();
            this.SuspendLayout();
            // 
            // lblDescription
            // 
            this.lblDescription.Location = new System.Drawing.Point(12, 9);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(362, 45);
            this.lblDescription.TabIndex = 0;
            this.lblDescription.Tag = "Label_LifestyleNuyen_Description";
            this.lblDescription.Text = "Roll the number of dice shown below and enter the result to determine your charac" +
    "ter\'s starting Nueyn amount.";
            // 
            // cmdOK
            // 
            this.cmdOK.Location = new System.Drawing.Point(156, 83);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 4;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lblDice
            // 
            this.lblDice.AutoSize = true;
            this.lblDice.Location = new System.Drawing.Point(69, 59);
            this.lblDice.Name = "lblDice";
            this.lblDice.Size = new System.Drawing.Size(81, 13);
            this.lblDice.TabIndex = 1;
            this.lblDice.Tag = "Label_LifestyleNuyen_ResultOf";
            this.lblDice.Text = "Result of 4D6: (";
            this.lblDice.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblResult
            // 
            this.lblResult.AutoSize = true;
            this.lblResult.Location = new System.Drawing.Point(205, 59);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(37, 13);
            this.lblResult.TabIndex = 3;
            this.lblResult.Tag = "Label_LifestyleNuyen_ResultOf";
            this.lblResult.Text = "Result";
            // 
            // nudDiceResult
            // 
            this.nudDiceResult.Location = new System.Drawing.Point(156, 57);
            this.nudDiceResult.Name = "nudDiceResult";
            this.nudDiceResult.Size = new System.Drawing.Size(43, 20);
            this.nudDiceResult.TabIndex = 2;
            this.nudDiceResult.ValueChanged += new System.EventHandler(this.nudDiceResult_ValueChanged);
            // 
            // frmLifestyleNuyen
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(386, 112);
            this.Controls.Add(this.nudDiceResult);
            this.Controls.Add(this.lblResult);
            this.Controls.Add(this.lblDice);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.lblDescription);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmLifestyleNuyen";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_LifestyleNuyen";
            this.Text = "Starting Nuyen";
            this.Load += new System.EventHandler(this.frmLifestyleNuyen_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudDiceResult)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Label lblDice;
        private System.Windows.Forms.Label lblResult;
        private System.Windows.Forms.NumericUpDown nudDiceResult;
    }
}