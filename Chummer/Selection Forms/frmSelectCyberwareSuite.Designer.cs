namespace Chummer
{
    partial class frmSelectCyberwareSuite
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
            this.cmdOK = new System.Windows.Forms.Button();
            this.lstCyberware = new System.Windows.Forms.ListBox();
            this.lblGradeLabel = new System.Windows.Forms.Label();
            this.lblGrade = new System.Windows.Forms.Label();
            this.lblEssence = new System.Windows.Forms.Label();
            this.lblEssenceLabel = new System.Windows.Forms.Label();
            this.lblCost = new System.Windows.Forms.Label();
            this.lblCostLabel = new System.Windows.Forms.Label();
            this.lblCyberwareLabel = new System.Windows.Forms.Label();
            this.lblCyberware = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(452, 440);
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
            this.cmdOK.Location = new System.Drawing.Point(533, 440);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 9;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lstCyberware
            // 
            this.lstCyberware.FormattingEnabled = true;
            this.lstCyberware.Location = new System.Drawing.Point(12, 12);
            this.lstCyberware.Name = "lstCyberware";
            this.lstCyberware.Size = new System.Drawing.Size(240, 446);
            this.lstCyberware.Sorted = true;
            this.lstCyberware.TabIndex = 0;
            this.lstCyberware.SelectedIndexChanged += new System.EventHandler(this.lstCyberware_SelectedIndexChanged);
            this.lstCyberware.DoubleClick += new System.EventHandler(this.lstCyberware_DoubleClick);
            // 
            // lblGradeLabel
            // 
            this.lblGradeLabel.AutoSize = true;
            this.lblGradeLabel.Location = new System.Drawing.Point(258, 12);
            this.lblGradeLabel.Name = "lblGradeLabel";
            this.lblGradeLabel.Size = new System.Drawing.Size(39, 13);
            this.lblGradeLabel.TabIndex = 1;
            this.lblGradeLabel.Tag = "Label_Grade";
            this.lblGradeLabel.Text = "Grade:";
            // 
            // lblGrade
            // 
            this.lblGrade.AutoSize = true;
            this.lblGrade.Location = new System.Drawing.Point(315, 12);
            this.lblGrade.Name = "lblGrade";
            this.lblGrade.Size = new System.Drawing.Size(42, 13);
            this.lblGrade.TabIndex = 2;
            this.lblGrade.Text = "[Grade]";
            // 
            // lblEssence
            // 
            this.lblEssence.AutoSize = true;
            this.lblEssence.Location = new System.Drawing.Point(315, 35);
            this.lblEssence.Name = "lblEssence";
            this.lblEssence.Size = new System.Drawing.Size(54, 13);
            this.lblEssence.TabIndex = 4;
            this.lblEssence.Text = "[Essence]";
            // 
            // lblEssenceLabel
            // 
            this.lblEssenceLabel.AutoSize = true;
            this.lblEssenceLabel.Location = new System.Drawing.Point(258, 35);
            this.lblEssenceLabel.Name = "lblEssenceLabel";
            this.lblEssenceLabel.Size = new System.Drawing.Size(51, 13);
            this.lblEssenceLabel.TabIndex = 3;
            this.lblEssenceLabel.Tag = "Label_Essence";
            this.lblEssenceLabel.Text = "Essence:";
            // 
            // lblCost
            // 
            this.lblCost.AutoSize = true;
            this.lblCost.Location = new System.Drawing.Point(315, 58);
            this.lblCost.Name = "lblCost";
            this.lblCost.Size = new System.Drawing.Size(34, 13);
            this.lblCost.TabIndex = 6;
            this.lblCost.Text = "[Cost]";
            // 
            // lblCostLabel
            // 
            this.lblCostLabel.AutoSize = true;
            this.lblCostLabel.Location = new System.Drawing.Point(258, 58);
            this.lblCostLabel.Name = "lblCostLabel";
            this.lblCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblCostLabel.TabIndex = 5;
            this.lblCostLabel.Tag = "Label_Cost";
            this.lblCostLabel.Text = "Cost:";
            // 
            // lblCyberwareLabel
            // 
            this.lblCyberwareLabel.AutoSize = true;
            this.lblCyberwareLabel.Location = new System.Drawing.Point(258, 93);
            this.lblCyberwareLabel.Name = "lblCyberwareLabel";
            this.lblCyberwareLabel.Size = new System.Drawing.Size(144, 13);
            this.lblCyberwareLabel.TabIndex = 7;
            this.lblCyberwareLabel.Tag = "Label_SelectCyberwareSuite_PartsInSuite";
            this.lblCyberwareLabel.Text = "Parts in this Cyberware Suite:";
            // 
            // lblCyberware
            // 
            this.lblCyberware.Location = new System.Drawing.Point(296, 116);
            this.lblCyberware.Name = "lblCyberware";
            this.lblCyberware.Size = new System.Drawing.Size(311, 292);
            this.lblCyberware.TabIndex = 8;
            this.lblCyberware.Text = "[Cyberware]";
            // 
            // frmSelectCyberwareSuite
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(620, 475);
            this.Controls.Add(this.lblCyberware);
            this.Controls.Add(this.lblCyberwareLabel);
            this.Controls.Add(this.lblCost);
            this.Controls.Add(this.lblCostLabel);
            this.Controls.Add(this.lblEssence);
            this.Controls.Add(this.lblEssenceLabel);
            this.Controls.Add(this.lblGrade);
            this.Controls.Add(this.lblGradeLabel);
            this.Controls.Add(this.lstCyberware);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectCyberwareSuite";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectCyberwareSuite";
            this.Text = "Select a Cyberware Suite";
            this.Load += new System.EventHandler(this.frmSelectCyberwareSuite_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.ListBox lstCyberware;
        private System.Windows.Forms.Label lblGradeLabel;
        private System.Windows.Forms.Label lblGrade;
        private System.Windows.Forms.Label lblEssence;
        private System.Windows.Forms.Label lblEssenceLabel;
        private System.Windows.Forms.Label lblCost;
        private System.Windows.Forms.Label lblCostLabel;
        private System.Windows.Forms.Label lblCyberwareLabel;
        private System.Windows.Forms.Label lblCyberware;
    }
}