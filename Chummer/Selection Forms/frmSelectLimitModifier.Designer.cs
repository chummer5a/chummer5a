namespace Chummer
{
    public sealed partial class frmSelectLimitModifier
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
            this.cmdCancel = new System.Windows.Forms.Button();
            this.txtName = new System.Windows.Forms.TextBox();
            this.cmdOK = new System.Windows.Forms.Button();
            this.nudBonus = new System.Windows.Forms.NumericUpDown();
            this.lblNameLabel = new System.Windows.Forms.Label();
            this.lblBonusLabel = new System.Windows.Forms.Label();
            this.lblCondition = new System.Windows.Forms.Label();
            this.txtCondition = new System.Windows.Forms.TextBox();
            this.cboLimit = new Chummer.ElasticComboBox();
            this.lblLimit = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.nudBonus)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.AutoSize = true;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(3, 3);
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
            this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtName.Location = new System.Drawing.Point(63, 30);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(260, 20);
            this.txtName.TabIndex = 0;
            this.txtName.TextChanged += new System.EventHandler(this.ToggleOKEnabled);
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.AutoSize = true;
            this.cmdOK.Location = new System.Drawing.Point(84, 3);
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
            this.nudBonus.Location = new System.Drawing.Point(63, 82);
            this.nudBonus.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.nudBonus.Name = "nudBonus";
            this.nudBonus.Size = new System.Drawing.Size(100, 20);
            this.nudBonus.TabIndex = 2;
            this.nudBonus.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblNameLabel
            // 
            this.lblNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNameLabel.AutoSize = true;
            this.lblNameLabel.Location = new System.Drawing.Point(19, 33);
            this.lblNameLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblNameLabel.Name = "lblNameLabel";
            this.lblNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblNameLabel.TabIndex = 9;
            this.lblNameLabel.Tag = "Label_Name";
            this.lblNameLabel.Text = "Name:";
            // 
            // lblBonusLabel
            // 
            this.lblBonusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBonusLabel.AutoSize = true;
            this.lblBonusLabel.Location = new System.Drawing.Point(17, 85);
            this.lblBonusLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBonusLabel.Name = "lblBonusLabel";
            this.lblBonusLabel.Size = new System.Drawing.Size(40, 13);
            this.lblBonusLabel.TabIndex = 10;
            this.lblBonusLabel.Tag = "Label_Bonus";
            this.lblBonusLabel.Text = "Bonus:";
            // 
            // lblCondition
            // 
            this.lblCondition.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCondition.AutoSize = true;
            this.lblCondition.Location = new System.Drawing.Point(3, 59);
            this.lblCondition.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCondition.Name = "lblCondition";
            this.lblCondition.Size = new System.Drawing.Size(54, 13);
            this.lblCondition.TabIndex = 12;
            this.lblCondition.Tag = "Label_Condition";
            this.lblCondition.Text = "Condition:";
            // 
            // txtCondition
            // 
            this.txtCondition.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCondition.Location = new System.Drawing.Point(63, 56);
            this.txtCondition.Name = "txtCondition";
            this.txtCondition.Size = new System.Drawing.Size(260, 20);
            this.txtCondition.TabIndex = 1;
            // 
            // cboLimit
            // 
            this.cboLimit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboLimit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLimit.FormattingEnabled = true;
            this.cboLimit.Location = new System.Drawing.Point(63, 3);
            this.cboLimit.Name = "cboLimit";
            this.cboLimit.Size = new System.Drawing.Size(260, 21);
            this.cboLimit.TabIndex = 13;
            this.cboLimit.TooltipText = "";
            this.cboLimit.SelectedIndexChanged += new System.EventHandler(this.ToggleOKEnabled);
            // 
            // lblLimit
            // 
            this.lblLimit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLimit.AutoSize = true;
            this.lblLimit.Location = new System.Drawing.Point(26, 6);
            this.lblLimit.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLimit.Name = "lblLimit";
            this.lblLimit.Size = new System.Drawing.Size(31, 13);
            this.lblLimit.TabIndex = 14;
            this.lblLimit.Tag = "String_Limit";
            this.lblLimit.Text = "Limit:";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.lblBonusLabel, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.txtCondition, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.nudBonus, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblCondition, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.cboLimit, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblLimit, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblNameLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtName, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 4);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(9, 9);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(326, 133);
            this.tableLayoutPanel1.TabIndex = 15;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 2);
            this.flowLayoutPanel1.Controls.Add(this.cmdOK);
            this.flowLayoutPanel1.Controls.Add(this.cmdCancel);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(164, 105);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(162, 28);
            this.flowLayoutPanel1.TabIndex = 15;
            // 
            // frmSelectLimitModifier
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(344, 151);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "frmSelectLimitModifier";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "String_EnterLimitModifier";
            this.Text = "Enter a Limit Modifier";
            ((System.ComponentModel.ISupportInitialize)(this.nudBonus)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
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
        private ElasticComboBox cboLimit;
        internal System.Windows.Forms.Label lblLimit;
        private Chummer.BufferedTableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
