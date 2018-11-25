namespace Chummer
{
    partial class frmSelectSpec
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
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cboSpec = new Chummer.ElasticComboBox();
            this.lblAmmoLabel = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.chkKarma = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
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
            // cboSpec
            // 
            this.cboSpec.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.cboSpec, 2);
            this.cboSpec.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSpec.DropDownWidth = 252;
            this.cboSpec.FormattingEnabled = true;
            this.cboSpec.Location = new System.Drawing.Point(47, 30);
            this.cboSpec.Name = "cboSpec";
            this.cboSpec.Size = new System.Drawing.Size(276, 21);
            this.cboSpec.TabIndex = 2;
            this.cboSpec.TooltipText = "";
            this.cboSpec.SelectedIndexChanged += new System.EventHandler(this.cboSpec_SelectedIndexChanged);
            // 
            // lblAmmoLabel
            // 
            this.lblAmmoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAmmoLabel.AutoSize = true;
            this.lblAmmoLabel.Location = new System.Drawing.Point(3, 33);
            this.lblAmmoLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAmmoLabel.Name = "lblAmmoLabel";
            this.lblAmmoLabel.Size = new System.Drawing.Size(38, 13);
            this.lblAmmoLabel.TabIndex = 1;
            this.lblAmmoLabel.Tag = "Label_Name";
            this.lblAmmoLabel.Text = "Name:";
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblDescription, 3);
            this.lblDescription.Location = new System.Drawing.Point(3, 6);
            this.lblDescription.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(255, 13);
            this.lblDescription.TabIndex = 0;
            this.lblDescription.Tag = "Label_SelectSkillSpecialization_Description";
            this.lblDescription.Text = "Select a Skill Specialization to add to your character.";
            // 
            // chkKarma
            // 
            this.chkKarma.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkKarma.AutoSize = true;
            this.chkKarma.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.tableLayoutPanel1.SetColumnSpan(this.chkKarma, 2);
            this.chkKarma.Location = new System.Drawing.Point(3, 57);
            this.chkKarma.Name = "chkKarma";
            this.chkKarma.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chkKarma.Size = new System.Drawing.Size(131, 23);
            this.chkKarma.TabIndex = 15;
            this.chkKarma.Tag = "String_BuyWithKarma";
            this.chkKarma.Text = "Buy with Karma";
            this.chkKarma.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkKarma.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 66.66666F));
            this.tableLayoutPanel1.Controls.Add(this.lblDescription, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.cboSpec, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblAmmoLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.chkKarma, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 2, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(9, 9);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(326, 83);
            this.tableLayoutPanel1.TabIndex = 16;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.cmdOK);
            this.flowLayoutPanel1.Controls.Add(this.cmdCancel);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(164, 54);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(162, 29);
            this.flowLayoutPanel1.TabIndex = 16;
            // 
            // frmSelectSpec
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(344, 101);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(360, 10000);
            this.MinimizeBox = false;
            this.Name = "frmSelectSpec";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectSkillSpecialization";
            this.Text = "Select a Skill Specialization";
            this.Load += new System.EventHandler(this.frmSelectSpec_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private Chummer.ElasticComboBox cboSpec;
        private System.Windows.Forms.Label lblAmmoLabel;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.CheckBox chkKarma;
        private Chummer.BufferedTableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
