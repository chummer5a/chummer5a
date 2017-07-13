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
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cboSpec = new System.Windows.Forms.ComboBox();
            this.lblAmmoLabel = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.chkKarma = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.Location = new System.Drawing.Point(255, 77);
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
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(174, 77);
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
            this.cboSpec.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSpec.DropDownWidth = 252;
            this.cboSpec.FormattingEnabled = true;
            this.cboSpec.Location = new System.Drawing.Point(54, 45);
            this.cboSpec.Name = "cboSpec";
            this.cboSpec.Size = new System.Drawing.Size(276, 21);
            this.cboSpec.TabIndex = 2;
            this.cboSpec.DropDown += new System.EventHandler(this.cboSpec_DropDown);
            this.cboSpec.SelectedIndexChanged += new System.EventHandler(this.cboSpec_SelectedIndexChanged);
            // 
            // lblAmmoLabel
            // 
            this.lblAmmoLabel.AutoSize = true;
            this.lblAmmoLabel.Location = new System.Drawing.Point(9, 48);
            this.lblAmmoLabel.Name = "lblAmmoLabel";
            this.lblAmmoLabel.Size = new System.Drawing.Size(38, 13);
            this.lblAmmoLabel.TabIndex = 1;
            this.lblAmmoLabel.Tag = "Label_Name";
            this.lblAmmoLabel.Text = "Name:";
            // 
            // lblDescription
            // 
            this.lblDescription.Location = new System.Drawing.Point(12, 9);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(319, 33);
            this.lblDescription.TabIndex = 0;
            this.lblDescription.Tag = "Label_SelectSkillSpecialization_Description";
            this.lblDescription.Text = "Select a Skill Specialization to add to your character.";
            // 
            // chkKarma
            // 
            this.chkKarma.AutoSize = true;
            this.chkKarma.Location = new System.Drawing.Point(69, 81);
            this.chkKarma.Name = "chkKarma";
            this.chkKarma.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chkKarma.Size = new System.Drawing.Size(99, 17);
            this.chkKarma.TabIndex = 15;
            this.chkKarma.Tag = "String_BuyWithKarma";
            this.chkKarma.Text = "Buy with Karma";
            this.chkKarma.UseVisualStyleBackColor = true;
            // 
            // frmSelectSpec
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(342, 108);
            this.ControlBox = false;
            this.Controls.Add(this.chkKarma);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cboSpec);
            this.Controls.Add(this.lblAmmoLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectSpec";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectSkillSpecialization";
            this.Text = "Select a Skill Specialization";
            this.Load += new System.EventHandler(this.frmSelectSpec_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.ComboBox cboSpec;
        private System.Windows.Forms.Label lblAmmoLabel;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.CheckBox chkKarma;
    }
}