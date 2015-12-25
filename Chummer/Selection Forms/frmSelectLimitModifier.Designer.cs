namespace Chummer
{
    partial class frmSelectLimitModifier
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
            this.cmdCancel = new System.Windows.Forms.Button();
            this.txtName = new System.Windows.Forms.TextBox();
            this.cmdOK = new System.Windows.Forms.Button();
            this.nudBonus = new System.Windows.Forms.NumericUpDown();
            this.lblNameLabel = new System.Windows.Forms.Label();
            this.lblBonusLabel = new System.Windows.Forms.Label();
            this.lblCondition = new System.Windows.Forms.Label();
            this.txtCondition = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudBonus)).BeginInit();
            this.SuspendLayout();
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(252, 36);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 4;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(79, 12);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(167, 20);
            this.txtName.TabIndex = 0;
            // 
            // cmdOK
            // 
            this.cmdOK.Location = new System.Drawing.Point(252, 9);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 3;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // nudBonus
            // 
            this.nudBonus.Location = new System.Drawing.Point(79, 64);
            this.nudBonus.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nudBonus.Minimum = new decimal(new int[] {
            20,
            0,
            0,
            -2147483648});
            this.nudBonus.Name = "nudBonus";
            this.nudBonus.Size = new System.Drawing.Size(40, 20);
            this.nudBonus.TabIndex = 2;
            this.nudBonus.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblNameLabel
            // 
            this.lblNameLabel.AutoSize = true;
            this.lblNameLabel.Location = new System.Drawing.Point(10, 15);
            this.lblNameLabel.Name = "lblNameLabel";
            this.lblNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblNameLabel.TabIndex = 9;
            this.lblNameLabel.Tag = "Label_Name";
            this.lblNameLabel.Text = "Name:";
            // 
            // lblBonusLabel
            // 
            this.lblBonusLabel.AutoSize = true;
            this.lblBonusLabel.Location = new System.Drawing.Point(10, 66);
            this.lblBonusLabel.Name = "lblBonusLabel";
            this.lblBonusLabel.Size = new System.Drawing.Size(40, 13);
            this.lblBonusLabel.TabIndex = 10;
            this.lblBonusLabel.Tag = "Label_Bonus";
            this.lblBonusLabel.Text = "Bonus:";
            // 
            // lblCondition
            // 
            this.lblCondition.AutoSize = true;
            this.lblCondition.Location = new System.Drawing.Point(10, 41);
            this.lblCondition.Name = "lblCondition";
            this.lblCondition.Size = new System.Drawing.Size(54, 13);
            this.lblCondition.TabIndex = 12;
            this.lblCondition.Tag = "Label_Condition";
            this.lblCondition.Text = "Condition:";
            // 
            // txtCondition
            // 
            this.txtCondition.Location = new System.Drawing.Point(79, 38);
            this.txtCondition.Name = "txtCondition";
            this.txtCondition.Size = new System.Drawing.Size(167, 20);
            this.txtCondition.TabIndex = 1;
            // 
            // frmSelectLimitModifier
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(339, 98);
            this.Controls.Add(this.lblCondition);
            this.Controls.Add(this.txtCondition);
            this.Controls.Add(this.lblBonusLabel);
            this.Controls.Add(this.lblNameLabel);
            this.Controls.Add(this.nudBonus);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.cmdOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "frmSelectLimitModifier";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "String_EnterLimitModifier";
            this.Text = "Enter a Limit Modifier";
            ((System.ComponentModel.ISupportInitialize)(this.nudBonus)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Button cmdOK;
        internal System.Windows.Forms.NumericUpDown nudBonus;
        internal System.Windows.Forms.Label lblNameLabel;
        internal System.Windows.Forms.Label lblBonusLabel;
        internal System.Windows.Forms.Label lblCondition;
        private System.Windows.Forms.TextBox txtCondition;
    }
}