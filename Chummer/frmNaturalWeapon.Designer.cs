namespace Chummer
{
    partial class frmNaturalWeapon
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
            this.cboSkill = new System.Windows.Forms.ComboBox();
            this.lblName = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblActiveSkill = new System.Windows.Forms.Label();
            this.lblDV = new System.Windows.Forms.Label();
            this.cboDVBase = new System.Windows.Forms.ComboBox();
            this.nudDVMod = new System.Windows.Forms.NumericUpDown();
            this.lblDVPlus = new System.Windows.Forms.Label();
            this.nudAP = new System.Windows.Forms.NumericUpDown();
            this.lblAP = new System.Windows.Forms.Label();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.nudReach = new System.Windows.Forms.NumericUpDown();
            this.lblReach = new System.Windows.Forms.Label();
            this.cboDVType = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudDVMod)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudReach)).BeginInit();
            this.SuspendLayout();
            // 
            // cboSkill
            // 
            this.cboSkill.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSkill.FormattingEnabled = true;
            this.cboSkill.Location = new System.Drawing.Point(89, 38);
            this.cboSkill.Name = "cboSkill";
            this.cboSkill.Size = new System.Drawing.Size(234, 21);
            this.cboSkill.TabIndex = 3;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(11, 15);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(38, 13);
            this.lblName.TabIndex = 0;
            this.lblName.Tag = "Label_Name";
            this.lblName.Text = "Name:";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(89, 12);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(323, 20);
            this.txtName.TabIndex = 1;
            // 
            // lblActiveSkill
            // 
            this.lblActiveSkill.AutoSize = true;
            this.lblActiveSkill.Location = new System.Drawing.Point(12, 41);
            this.lblActiveSkill.Name = "lblActiveSkill";
            this.lblActiveSkill.Size = new System.Drawing.Size(62, 13);
            this.lblActiveSkill.TabIndex = 2;
            this.lblActiveSkill.Tag = "Label_ActiveSkill";
            this.lblActiveSkill.Text = "Active Skill:";
            // 
            // lblDV
            // 
            this.lblDV.AutoSize = true;
            this.lblDV.Location = new System.Drawing.Point(12, 68);
            this.lblDV.Name = "lblDV";
            this.lblDV.Size = new System.Drawing.Size(25, 13);
            this.lblDV.TabIndex = 4;
            this.lblDV.Tag = "Label_DV";
            this.lblDV.Text = "DV:";
            // 
            // cboDVBase
            // 
            this.cboDVBase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDVBase.FormattingEnabled = true;
            this.cboDVBase.Location = new System.Drawing.Point(89, 65);
            this.cboDVBase.Name = "cboDVBase";
            this.cboDVBase.Size = new System.Drawing.Size(93, 21);
            this.cboDVBase.TabIndex = 5;
            // 
            // nudDVMod
            // 
            this.nudDVMod.Location = new System.Drawing.Point(207, 66);
            this.nudDVMod.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudDVMod.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.nudDVMod.Name = "nudDVMod";
            this.nudDVMod.Size = new System.Drawing.Size(55, 20);
            this.nudDVMod.TabIndex = 7;
            // 
            // lblDVPlus
            // 
            this.lblDVPlus.AutoSize = true;
            this.lblDVPlus.Location = new System.Drawing.Point(188, 68);
            this.lblDVPlus.Name = "lblDVPlus";
            this.lblDVPlus.Size = new System.Drawing.Size(13, 13);
            this.lblDVPlus.TabIndex = 6;
            this.lblDVPlus.Text = "+";
            // 
            // nudAP
            // 
            this.nudAP.Location = new System.Drawing.Point(89, 92);
            this.nudAP.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudAP.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.nudAP.Name = "nudAP";
            this.nudAP.Size = new System.Drawing.Size(55, 20);
            this.nudAP.TabIndex = 9;
            // 
            // lblAP
            // 
            this.lblAP.AutoSize = true;
            this.lblAP.Location = new System.Drawing.Point(13, 94);
            this.lblAP.Name = "lblAP";
            this.lblAP.Size = new System.Drawing.Size(24, 13);
            this.lblAP.TabIndex = 8;
            this.lblAP.Tag = "Label_AP";
            this.lblAP.Text = "AP:";
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(254, 149);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 11;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.Location = new System.Drawing.Point(335, 149);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 10;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // nudReach
            // 
            this.nudReach.Location = new System.Drawing.Point(89, 118);
            this.nudReach.Maximum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.nudReach.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            -2147483648});
            this.nudReach.Name = "nudReach";
            this.nudReach.Size = new System.Drawing.Size(55, 20);
            this.nudReach.TabIndex = 13;
            // 
            // lblReach
            // 
            this.lblReach.AutoSize = true;
            this.lblReach.Location = new System.Drawing.Point(13, 120);
            this.lblReach.Name = "lblReach";
            this.lblReach.Size = new System.Drawing.Size(42, 13);
            this.lblReach.TabIndex = 12;
            this.lblReach.Tag = "Label_Reach";
            this.lblReach.Text = "Reach:";
            // 
            // cboDVType
            // 
            this.cboDVType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDVType.FormattingEnabled = true;
            this.cboDVType.Location = new System.Drawing.Point(268, 65);
            this.cboDVType.Name = "cboDVType";
            this.cboDVType.Size = new System.Drawing.Size(61, 21);
            this.cboDVType.TabIndex = 14;
            // 
            // frmNaturalWeapon
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(422, 179);
            this.Controls.Add(this.cboDVType);
            this.Controls.Add(this.nudReach);
            this.Controls.Add(this.lblReach);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.nudAP);
            this.Controls.Add(this.lblAP);
            this.Controls.Add(this.lblDVPlus);
            this.Controls.Add(this.nudDVMod);
            this.Controls.Add(this.cboDVBase);
            this.Controls.Add(this.lblDV);
            this.Controls.Add(this.lblActiveSkill);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.cboSkill);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmNaturalWeapon";
            this.ShowInTaskbar = false;
            this.Tag = "Title_CreateNaturalWeapon";
            this.Text = "Create Natural Weapon";
            this.Load += new System.EventHandler(this.frmNaturalWeapon_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudDVMod)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudReach)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cboSkill;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblActiveSkill;
        private System.Windows.Forms.Label lblDV;
        private System.Windows.Forms.ComboBox cboDVBase;
        private System.Windows.Forms.NumericUpDown nudDVMod;
        private System.Windows.Forms.Label lblDVPlus;
        private System.Windows.Forms.NumericUpDown nudAP;
        private System.Windows.Forms.Label lblAP;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.NumericUpDown nudReach;
        private System.Windows.Forms.Label lblReach;
        private System.Windows.Forms.ComboBox cboDVType;
    }
}