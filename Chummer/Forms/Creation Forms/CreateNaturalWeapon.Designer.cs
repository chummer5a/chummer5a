namespace Chummer
{
    partial class CreateNaturalWeapon
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
            this.nudDVMod = new Chummer.NumericUpDownEx();
            this.lblDVPlus = new System.Windows.Forms.Label();
            this.nudAP = new Chummer.NumericUpDownEx();
            this.lblAP = new System.Windows.Forms.Label();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.nudReach = new Chummer.NumericUpDownEx();
            this.lblReach = new System.Windows.Forms.Label();
            this.cboDVType = new Chummer.ElasticComboBox();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.flpDV = new System.Windows.Forms.FlowLayoutPanel();
            this.tlpButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.nudDVMod)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudReach)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.flpDV.SuspendLayout();
            this.tlpButtons.SuspendLayout();
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
            this.lblDV.Location = new System.Drawing.Point(40, 60);
            this.lblDV.Name = "lblDV";
            this.lblDV.Size = new System.Drawing.Size(25, 13);
            this.lblDV.TabIndex = 4;
            this.lblDV.Tag = "Label_DV";
            this.lblDV.Text = "DV:";
            this.lblDV.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboDVBase
            // 
            this.cboDVBase.Anchor = System.Windows.Forms.AnchorStyles.Left;
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
            this.lblDVPlus.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDVPlus.AutoSize = true;
            this.lblDVPlus.Location = new System.Drawing.Point(102, 5);
            this.lblDVPlus.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.lblDVPlus.Name = "lblDVPlus";
            this.lblDVPlus.Size = new System.Drawing.Size(13, 13);
            this.lblDVPlus.TabIndex = 6;
            this.lblDVPlus.Text = "+";
            this.lblDVPlus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // nudAP
            // 
            this.nudAP.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudAP.AutoSize = true;
            this.nudAP.Location = new System.Drawing.Point(71, 83);
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
            this.lblAP.Location = new System.Drawing.Point(41, 86);
            this.lblAP.Name = "lblAP";
            this.lblAP.Size = new System.Drawing.Size(24, 13);
            this.lblAP.TabIndex = 8;
            this.lblAP.Tag = "Label_AP";
            this.lblAP.Text = "AP:";
            this.lblAP.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmdCancel
            // 
            this.cmdCancel.AutoSize = true;
            this.cmdCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdCancel.Location = new System.Drawing.Point(3, 3);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(50, 23);
            this.cmdCancel.TabIndex = 11;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.AutoSize = true;
            this.cmdOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOK.Location = new System.Drawing.Point(59, 3);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(50, 23);
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
            this.nudReach.Location = new System.Drawing.Point(71, 109);
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
            this.lblReach.Location = new System.Drawing.Point(23, 112);
            this.lblReach.Name = "lblReach";
            this.lblReach.Size = new System.Drawing.Size(42, 13);
            this.lblReach.TabIndex = 12;
            this.lblReach.Tag = "Label_Reach";
            this.lblReach.Text = "Reach:";
            this.lblReach.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboDVType
            // 
            this.cboDVType.Anchor = System.Windows.Forms.AnchorStyles.Left;
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
            this.tlpMain.Controls.Add(this.flpDV, 1, 2);
            this.tlpMain.Controls.Add(this.tlpButtons, 0, 5);
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
            // flpDV
            // 
            this.flpDV.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpDV.AutoSize = true;
            this.flpDV.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpDV.Controls.Add(this.cboDVBase);
            this.flpDV.Controls.Add(this.lblDVPlus);
            this.flpDV.Controls.Add(this.nudDVMod);
            this.flpDV.Controls.Add(this.cboDVType);
            this.flpDV.Location = new System.Drawing.Point(68, 53);
            this.flpDV.Margin = new System.Windows.Forms.Padding(0);
            this.flpDV.Name = "flpDV";
            this.flpDV.Size = new System.Drawing.Size(226, 27);
            this.flpDV.TabIndex = 14;
            this.flpDV.WrapContents = false;
            // 
            // tlpButtons
            // 
            this.tlpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpButtons.AutoSize = true;
            this.tlpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpButtons.ColumnCount = 2;
            this.tlpMain.SetColumnSpan(this.tlpButtons, 2);
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.Controls.Add(this.cmdCancel, 0, 0);
            this.tlpButtons.Controls.Add(this.cmdOK, 1, 0);
            this.tlpButtons.Location = new System.Drawing.Point(334, 154);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.Size = new System.Drawing.Size(112, 29);
            this.tlpButtons.TabIndex = 16;
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
            this.Load += new System.EventHandler(this.CreateNaturalWeapon_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudDVMod)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudReach)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.flpDV.ResumeLayout(false);
            this.flpDV.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
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
        private Chummer.NumericUpDownEx nudDVMod;
        private System.Windows.Forms.Label lblDVPlus;
        private Chummer.NumericUpDownEx nudAP;
        private System.Windows.Forms.Label lblAP;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private Chummer.NumericUpDownEx nudReach;
        private System.Windows.Forms.Label lblReach;
        private ElasticComboBox cboDVType;
        private BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.FlowLayoutPanel flpDV;
        private BufferedTableLayoutPanel tlpButtons;
    }
}
