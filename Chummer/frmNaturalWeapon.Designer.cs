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
            this.components = new System.ComponentModel.Container();
            this.cboSkill = new Chummer.ElasticComboBox();
            this.lblName = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblActiveSkill = new System.Windows.Forms.Label();
            this.lblDV = new System.Windows.Forms.Label();
            this.cboDVBase = new Chummer.ElasticComboBox();
            this.nudDVMod = new System.Windows.Forms.NumericUpDown();
            this.lblDVPlus = new System.Windows.Forms.Label();
            this.nudAP = new System.Windows.Forms.NumericUpDown();
            this.lblAP = new System.Windows.Forms.Label();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.nudReach = new System.Windows.Forms.NumericUpDown();
            this.lblReach = new System.Windows.Forms.Label();
            this.cboDVType = new Chummer.ElasticComboBox();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.nudDVMod)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudReach)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // cboSkill
            // 
            this.cboSkill.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSkill.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSkill.FormattingEnabled = true;
            this.cboSkill.Location = new System.Drawing.Point(71, 29);
            this.cboSkill.Name = "cboSkill";
            this.cboSkill.Size = new System.Drawing.Size(372, 21);
            this.cboSkill.TabIndex = 3;
            this.cboSkill.TooltipText = "";
            // 
            // lblName
            // 
            this.lblName.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(27, 6);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(38, 13);
            this.lblName.TabIndex = 0;
            this.lblName.Tag = "Label_Name";
            this.lblName.Text = "Name:";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtName
            // 
            this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtName.Location = new System.Drawing.Point(71, 3);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(372, 20);
            this.txtName.TabIndex = 1;
            // 
            // lblActiveSkill
            // 
            this.lblActiveSkill.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblActiveSkill.AutoSize = true;
            this.lblActiveSkill.Location = new System.Drawing.Point(3, 33);
            this.lblActiveSkill.Name = "lblActiveSkill";
            this.lblActiveSkill.Size = new System.Drawing.Size(62, 13);
            this.lblActiveSkill.TabIndex = 2;
            this.lblActiveSkill.Tag = "Label_ActiveSkill";
            this.lblActiveSkill.Text = "Active Skill:";
            this.lblActiveSkill.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblDV
            // 
            this.lblDV.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDV.AutoSize = true;
            this.lblDV.Location = new System.Drawing.Point(40, 59);
            this.lblDV.Name = "lblDV";
            this.lblDV.Size = new System.Drawing.Size(25, 13);
            this.lblDV.TabIndex = 4;
            this.lblDV.Tag = "Label_DV";
            this.lblDV.Text = "DV:";
            this.lblDV.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboDVBase
            // 
            this.cboDVBase.Dock = System.Windows.Forms.DockStyle.Left;
            this.cboDVBase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDVBase.FormattingEnabled = true;
            this.cboDVBase.Location = new System.Drawing.Point(3, 3);
            this.cboDVBase.Name = "cboDVBase";
            this.cboDVBase.Size = new System.Drawing.Size(93, 21);
            this.cboDVBase.TabIndex = 5;
            this.cboDVBase.TooltipText = "";
            // 
            // nudDVMod
            // 
            this.nudDVMod.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudDVMod.AutoSize = true;
            this.nudDVMod.Location = new System.Drawing.Point(121, 3);
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
            this.nudDVMod.Size = new System.Drawing.Size(35, 20);
            this.nudDVMod.TabIndex = 7;
            // 
            // lblDVPlus
            // 
            this.lblDVPlus.AutoSize = true;
            this.lblDVPlus.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblDVPlus.Location = new System.Drawing.Point(102, 3);
            this.lblDVPlus.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.lblDVPlus.Name = "lblDVPlus";
            this.lblDVPlus.Size = new System.Drawing.Size(13, 17);
            this.lblDVPlus.TabIndex = 6;
            this.lblDVPlus.Text = "+";
            this.lblDVPlus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // nudAP
            // 
            this.nudAP.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudAP.AutoSize = true;
            this.nudAP.Location = new System.Drawing.Point(71, 82);
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
            this.nudAP.Size = new System.Drawing.Size(35, 20);
            this.nudAP.TabIndex = 9;
            // 
            // lblAP
            // 
            this.lblAP.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblAP.AutoSize = true;
            this.lblAP.Location = new System.Drawing.Point(41, 85);
            this.lblAP.Name = "lblAP";
            this.lblAP.Size = new System.Drawing.Size(24, 13);
            this.lblAP.TabIndex = 8;
            this.lblAP.Tag = "Label_AP";
            this.lblAP.Text = "AP:";
            this.lblAP.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(0, 0);
            this.cmdCancel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
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
            this.cmdOK.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdOK.AutoSize = true;
            this.cmdOK.Location = new System.Drawing.Point(81, 0);
            this.cmdOK.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
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
            this.nudReach.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudReach.AutoSize = true;
            this.nudReach.Location = new System.Drawing.Point(71, 108);
            this.nudReach.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudReach.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.nudReach.Name = "nudReach";
            this.nudReach.Size = new System.Drawing.Size(35, 20);
            this.nudReach.TabIndex = 13;
            // 
            // lblReach
            // 
            this.lblReach.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblReach.AutoSize = true;
            this.lblReach.Location = new System.Drawing.Point(23, 111);
            this.lblReach.Name = "lblReach";
            this.lblReach.Size = new System.Drawing.Size(42, 13);
            this.lblReach.TabIndex = 12;
            this.lblReach.Tag = "Label_Reach";
            this.lblReach.Text = "Reach:";
            this.lblReach.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboDVType
            // 
            this.cboDVType.Dock = System.Windows.Forms.DockStyle.Left;
            this.cboDVType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDVType.FormattingEnabled = true;
            this.cboDVType.Location = new System.Drawing.Point(162, 3);
            this.cboDVType.Name = "cboDVType";
            this.cboDVType.Size = new System.Drawing.Size(61, 21);
            this.cboDVType.TabIndex = 14;
            this.cboDVType.TooltipText = "";
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.lblName, 0, 0);
            this.tlpMain.Controls.Add(this.nudReach, 1, 4);
            this.tlpMain.Controls.Add(this.lblActiveSkill, 0, 1);
            this.tlpMain.Controls.Add(this.nudAP, 1, 3);
            this.tlpMain.Controls.Add(this.lblReach, 0, 4);
            this.tlpMain.Controls.Add(this.txtName, 1, 0);
            this.tlpMain.Controls.Add(this.cboSkill, 1, 1);
            this.tlpMain.Controls.Add(this.lblDV, 0, 2);
            this.tlpMain.Controls.Add(this.lblAP, 0, 3);
            this.tlpMain.Controls.Add(this.flowLayoutPanel1, 1, 2);
            this.tlpMain.Controls.Add(this.flowLayoutPanel2, 0, 5);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 6;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Size = new System.Drawing.Size(446, 183);
            this.tlpMain.TabIndex = 15;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.cboDVBase);
            this.flowLayoutPanel1.Controls.Add(this.lblDVPlus);
            this.flowLayoutPanel1.Controls.Add(this.nudDVMod);
            this.flowLayoutPanel1.Controls.Add(this.cboDVType);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(68, 53);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(378, 26);
            this.flowLayoutPanel1.TabIndex = 14;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.SetColumnSpan(this.flowLayoutPanel2, 2);
            this.flowLayoutPanel2.Controls.Add(this.cmdOK);
            this.flowLayoutPanel2.Controls.Add(this.cmdCancel);
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(290, 160);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(156, 23);
            this.flowLayoutPanel2.TabIndex = 15;
            // 
            // frmNaturalWeapon
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(464, 201);
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmNaturalWeapon";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.Tag = "Title_CreateNaturalWeapon";
            this.Text = "Create Natural Weapon";
            this.Load += new System.EventHandler(this.frmNaturalWeapon_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudDVMod)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudReach)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ElasticComboBox cboSkill;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblActiveSkill;
        private System.Windows.Forms.Label lblDV;
        private ElasticComboBox cboDVBase;
        private System.Windows.Forms.NumericUpDown nudDVMod;
        private System.Windows.Forms.Label lblDVPlus;
        private System.Windows.Forms.NumericUpDown nudAP;
        private System.Windows.Forms.Label lblAP;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.NumericUpDown nudReach;
        private System.Windows.Forms.Label lblReach;
        private ElasticComboBox cboDVType;
        private BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
    }
}
