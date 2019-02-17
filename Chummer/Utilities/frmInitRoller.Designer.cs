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
            this.lblResult = new System.Windows.Forms.Label();
            this.lblDice = new System.Windows.Forms.Label();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lblDescription = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudDiceResult)).BeginInit();
            this.SuspendLayout();
            // 
            // nudDiceResult
            // 
            this.nudDiceResult.Location = new System.Drawing.Point(156, 57);
            this.nudDiceResult.Name = "nudDiceResult";
            this.nudDiceResult.Size = new System.Drawing.Size(43, 20);
            this.nudDiceResult.TabIndex = 7;
            // 
            // lblResult
            // 
            this.lblResult.AutoSize = true;
            this.lblResult.Location = new System.Drawing.Point(205, 59);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(37, 13);
            this.lblResult.TabIndex = 8;
            this.lblResult.Text = "Result";
            // 
            // lblDice
            // 
            this.lblDice.AutoSize = true;
            this.lblDice.Location = new System.Drawing.Point(69, 59);
            this.lblDice.Name = "lblDice";
            this.lblDice.Size = new System.Drawing.Size(81, 13);
            this.lblDice.TabIndex = 6;
            this.lblDice.Tag = "String_DiceHits_HitsOn";
            this.lblDice.Text = "Result of 4D6: (";
            this.lblDice.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cmdOK
            // 
            this.cmdOK.Location = new System.Drawing.Point(156, 83);
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
            this.lblDescription.Location = new System.Drawing.Point(12, 9);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(362, 45);
            this.lblDescription.TabIndex = 5;
            this.lblDescription.Tag = "InitRoller_Description";
            this.lblDescription.Text = "[Text Goes Here]";
            // 
            // frmInitRoller
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(386, 112);
            this.Controls.Add(this.nudDiceResult);
            this.Controls.Add(this.lblResult);
            this.Controls.Add(this.lblDice);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.lblDescription);
            this.Name = "frmInitRoller";
            this.Tag = "InitRoller_Header";
            this.Text = "{InitativeRoller}";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.frmInitRoller_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudDiceResult)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown nudDiceResult;
        private System.Windows.Forms.Label lblResult;
        private System.Windows.Forms.Label lblDice;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Label lblDescription;

    }
}