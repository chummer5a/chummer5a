using System.Windows;
using System.Windows.Forms;

namespace Chummer
{
    partial class frmKarmaMetatype
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
            this.tipTooltip = new TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip();
            this.pnlMetatypes = new System.Windows.Forms.Panel();
            this.chkBloodSpirit = new System.Windows.Forms.CheckBox();
            this.cboPossessionMethod = new System.Windows.Forms.ComboBox();
            this.chkPossessionBased = new System.Windows.Forms.CheckBox();
            this.nudForce = new System.Windows.Forms.NumericUpDown();
            this.lblForceLabel = new System.Windows.Forms.Label();
            this.cboCategory = new System.Windows.Forms.ComboBox();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.lblQualities = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblBP = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cboMetavariant = new System.Windows.Forms.ComboBox();
            this.cmdOK = new System.Windows.Forms.Button();
            this.Label19 = new System.Windows.Forms.Label();
            this.lblINI = new System.Windows.Forms.Label();
            this.Label17 = new System.Windows.Forms.Label();
            this.lblWIL = new System.Windows.Forms.Label();
            this.Label15 = new System.Windows.Forms.Label();
            this.lblLOG = new System.Windows.Forms.Label();
            this.Label13 = new System.Windows.Forms.Label();
            this.lblINT = new System.Windows.Forms.Label();
            this.Label11 = new System.Windows.Forms.Label();
            this.lblCHA = new System.Windows.Forms.Label();
            this.lblSTR = new System.Windows.Forms.Label();
            this.Label9 = new System.Windows.Forms.Label();
            this.lblREA = new System.Windows.Forms.Label();
            this.Label7 = new System.Windows.Forms.Label();
            this.lblAGI = new System.Windows.Forms.Label();
            this.Label5 = new System.Windows.Forms.Label();
            this.lblBOD = new System.Windows.Forms.Label();
            this.Label3 = new System.Windows.Forms.Label();
            this.lstMetatypes = new System.Windows.Forms.ListBox();
            this.pnlMetatypes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudForce)).BeginInit();
            this.SuspendLayout();
            // 
            // tipTooltip
            // 
            this.tipTooltip.AutoPopDelay = 10000;
            this.tipTooltip.InitialDelay = 250;
            this.tipTooltip.IsBalloon = true;
            this.tipTooltip.ReshowDelay = 100;
            this.tipTooltip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.tipTooltip.ToolTipTitle = "Chummer Help";
            // 
            // pnlMetatypes
            // 
            this.pnlMetatypes.Controls.Add(this.chkBloodSpirit);
            this.pnlMetatypes.Controls.Add(this.cboPossessionMethod);
            this.pnlMetatypes.Controls.Add(this.chkPossessionBased);
            this.pnlMetatypes.Controls.Add(this.nudForce);
            this.pnlMetatypes.Controls.Add(this.lblForceLabel);
            this.pnlMetatypes.Controls.Add(this.cboCategory);
            this.pnlMetatypes.Controls.Add(this.cmdCancel);
            this.pnlMetatypes.Controls.Add(this.lblQualities);
            this.pnlMetatypes.Controls.Add(this.label4);
            this.pnlMetatypes.Controls.Add(this.lblBP);
            this.pnlMetatypes.Controls.Add(this.label6);
            this.pnlMetatypes.Controls.Add(this.label2);
            this.pnlMetatypes.Controls.Add(this.cboMetavariant);
            this.pnlMetatypes.Controls.Add(this.cmdOK);
            this.pnlMetatypes.Controls.Add(this.Label19);
            this.pnlMetatypes.Controls.Add(this.lblINI);
            this.pnlMetatypes.Controls.Add(this.Label17);
            this.pnlMetatypes.Controls.Add(this.lblWIL);
            this.pnlMetatypes.Controls.Add(this.Label15);
            this.pnlMetatypes.Controls.Add(this.lblLOG);
            this.pnlMetatypes.Controls.Add(this.Label13);
            this.pnlMetatypes.Controls.Add(this.lblINT);
            this.pnlMetatypes.Controls.Add(this.Label11);
            this.pnlMetatypes.Controls.Add(this.lblCHA);
            this.pnlMetatypes.Controls.Add(this.lblSTR);
            this.pnlMetatypes.Controls.Add(this.Label9);
            this.pnlMetatypes.Controls.Add(this.lblREA);
            this.pnlMetatypes.Controls.Add(this.Label7);
            this.pnlMetatypes.Controls.Add(this.lblAGI);
            this.pnlMetatypes.Controls.Add(this.Label5);
            this.pnlMetatypes.Controls.Add(this.lblBOD);
            this.pnlMetatypes.Controls.Add(this.Label3);
            this.pnlMetatypes.Controls.Add(this.lstMetatypes);
            this.pnlMetatypes.Location = new System.Drawing.Point(12, 12);
            this.pnlMetatypes.Name = "pnlMetatypes";
            this.pnlMetatypes.Size = new System.Drawing.Size(513, 354);
            this.pnlMetatypes.TabIndex = 35;
            // 
            // chkBloodSpirit
            // 
            this.chkBloodSpirit.AutoSize = true;
            this.chkBloodSpirit.Location = new System.Drawing.Point(182, 237);
            this.chkBloodSpirit.Name = "chkBloodSpirit";
            this.chkBloodSpirit.Size = new System.Drawing.Size(79, 17);
            this.chkBloodSpirit.TabIndex = 69;
            this.chkBloodSpirit.Tag = "Checkbox_Metatype_BloodSpirit";
            this.chkBloodSpirit.Text = "Blood Spirit";
            this.chkBloodSpirit.UseVisualStyleBackColor = true;
            this.chkBloodSpirit.Visible = false;
            // 
            // cboPossessionMethod
            // 
            this.cboPossessionMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPossessionMethod.Enabled = false;
            this.cboPossessionMethod.FormattingEnabled = true;
            this.cboPossessionMethod.Location = new System.Drawing.Point(200, 283);
            this.cboPossessionMethod.Name = "cboPossessionMethod";
            this.cboPossessionMethod.Size = new System.Drawing.Size(174, 21);
            this.cboPossessionMethod.TabIndex = 65;
            this.cboPossessionMethod.Visible = false;
            // 
            // chkPossessionBased
            // 
            this.chkPossessionBased.AutoSize = true;
            this.chkPossessionBased.Location = new System.Drawing.Point(182, 260);
            this.chkPossessionBased.Name = "chkPossessionBased";
            this.chkPossessionBased.Size = new System.Drawing.Size(211, 17);
            this.chkPossessionBased.TabIndex = 64;
            this.chkPossessionBased.Tag = "Checkbox_Metatype_PossessionTradition";
            this.chkPossessionBased.Text = "Summoned by Possess-based Tradition";
            this.chkPossessionBased.UseVisualStyleBackColor = true;
            this.chkPossessionBased.Visible = false;
            // 
            // nudForce
            // 
            this.nudForce.Location = new System.Drawing.Point(404, 55);
            this.nudForce.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nudForce.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudForce.Name = "nudForce";
            this.nudForce.Size = new System.Drawing.Size(42, 20);
            this.nudForce.TabIndex = 57;
            this.nudForce.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudForce.Visible = false;
            // 
            // lblForceLabel
            // 
            this.lblForceLabel.AutoSize = true;
            this.lblForceLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblForceLabel.Location = new System.Drawing.Point(401, 44);
            this.lblForceLabel.Name = "lblForceLabel";
            this.lblForceLabel.Size = new System.Drawing.Size(48, 13);
            this.lblForceLabel.TabIndex = 56;
            this.lblForceLabel.Tag = "String_Force";
            this.lblForceLabel.Text = "FORCE";
            this.lblForceLabel.Visible = false;
            // 
            // cboCategory
            // 
            this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategory.FormattingEnabled = true;
            this.cboCategory.Location = new System.Drawing.Point(3, 3);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(158, 21);
            this.cboCategory.TabIndex = 66;
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(353, 323);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 68;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // lblQualities
            // 
            this.lblQualities.AutoSize = true;
            this.lblQualities.Location = new System.Drawing.Point(236, 156);
            this.lblQualities.Name = "lblQualities";
            this.lblQualities.Size = new System.Drawing.Size(33, 13);
            this.lblQualities.TabIndex = 63;
            this.lblQualities.Tag = "String_None";
            this.lblQualities.Text = "None";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(229, 143);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 62;
            this.label4.Tag = "String_Qualities";
            this.label4.Text = "Qualities";
            // 
            // lblBP
            // 
            this.lblBP.AutoSize = true;
            this.lblBP.Location = new System.Drawing.Point(179, 154);
            this.lblBP.Name = "lblBP";
            this.lblBP.Size = new System.Drawing.Size(13, 13);
            this.lblBP.TabIndex = 61;
            this.lblBP.Text = "0";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(179, 143);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(42, 13);
            this.label6.TabIndex = 60;
            this.label6.Tag = "String_Karma";
            this.label6.Text = "Karma";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(179, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 58;
            this.label2.Tag = "Label_Metavariant";
            this.label2.Text = "Metavariant";
            // 
            // cboMetavariant
            // 
            this.cboMetavariant.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMetavariant.FormattingEnabled = true;
            this.cboMetavariant.Location = new System.Drawing.Point(182, 111);
            this.cboMetavariant.Name = "cboMetavariant";
            this.cboMetavariant.Size = new System.Drawing.Size(219, 21);
            this.cboMetavariant.TabIndex = 59;
            this.cboMetavariant.SelectedIndexChanged += new System.EventHandler(this.cboMetavariant_SelectedIndexChanged);
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.Location = new System.Drawing.Point(434, 323);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 67;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // Label19
            // 
            this.Label19.AutoSize = true;
            this.Label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label19.Location = new System.Drawing.Point(350, 44);
            this.Label19.Name = "Label19";
            this.Label19.Size = new System.Drawing.Size(24, 13);
            this.Label19.TabIndex = 54;
            this.Label19.Tag = "String_AttributeINIShort";
            this.Label19.Text = "INI";
            // 
            // lblINI
            // 
            this.lblINI.AutoSize = true;
            this.lblINI.Location = new System.Drawing.Point(350, 57);
            this.lblINI.Name = "lblINI";
            this.lblINI.Size = new System.Drawing.Size(51, 13);
            this.lblINI.TabIndex = 55;
            this.lblINI.Text = "2/12 (18)";
            // 
            // Label17
            // 
            this.Label17.AutoSize = true;
            this.Label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label17.Location = new System.Drawing.Point(293, 44);
            this.Label17.Name = "Label17";
            this.Label17.Size = new System.Drawing.Size(30, 13);
            this.Label17.TabIndex = 52;
            this.Label17.Tag = "String_AttributeWILShort";
            this.Label17.Text = "WIL";
            // 
            // lblWIL
            // 
            this.lblWIL.AutoSize = true;
            this.lblWIL.Location = new System.Drawing.Point(293, 57);
            this.lblWIL.Name = "lblWIL";
            this.lblWIL.Size = new System.Drawing.Size(51, 13);
            this.lblWIL.TabIndex = 53;
            this.lblWIL.Text = "2/12 (18)";
            // 
            // Label15
            // 
            this.Label15.AutoSize = true;
            this.Label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label15.Location = new System.Drawing.Point(236, 44);
            this.Label15.Name = "Label15";
            this.Label15.Size = new System.Drawing.Size(32, 13);
            this.Label15.TabIndex = 50;
            this.Label15.Tag = "String_AttributeLOGShort";
            this.Label15.Text = "LOG";
            // 
            // lblLOG
            // 
            this.lblLOG.AutoSize = true;
            this.lblLOG.Location = new System.Drawing.Point(236, 57);
            this.lblLOG.Name = "lblLOG";
            this.lblLOG.Size = new System.Drawing.Size(51, 13);
            this.lblLOG.TabIndex = 51;
            this.lblLOG.Text = "2/12 (18)";
            // 
            // Label13
            // 
            this.Label13.AutoSize = true;
            this.Label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label13.Location = new System.Drawing.Point(179, 44);
            this.Label13.Name = "Label13";
            this.Label13.Size = new System.Drawing.Size(28, 13);
            this.Label13.TabIndex = 48;
            this.Label13.Tag = "String_AttributeINTShort";
            this.Label13.Text = "INT";
            // 
            // lblINT
            // 
            this.lblINT.AutoSize = true;
            this.lblINT.Location = new System.Drawing.Point(179, 57);
            this.lblINT.Name = "lblINT";
            this.lblINT.Size = new System.Drawing.Size(51, 13);
            this.lblINT.TabIndex = 49;
            this.lblINT.Text = "2/12 (18)";
            // 
            // Label11
            // 
            this.Label11.AutoSize = true;
            this.Label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label11.Location = new System.Drawing.Point(401, 8);
            this.Label11.Name = "Label11";
            this.Label11.Size = new System.Drawing.Size(32, 13);
            this.Label11.TabIndex = 46;
            this.Label11.Tag = "String_AttributeCHAShort";
            this.Label11.Text = "CHA";
            // 
            // lblCHA
            // 
            this.lblCHA.AutoSize = true;
            this.lblCHA.Location = new System.Drawing.Point(401, 21);
            this.lblCHA.Name = "lblCHA";
            this.lblCHA.Size = new System.Drawing.Size(51, 13);
            this.lblCHA.TabIndex = 47;
            this.lblCHA.Text = "2/12 (18)";
            // 
            // lblSTR
            // 
            this.lblSTR.AutoSize = true;
            this.lblSTR.Location = new System.Drawing.Point(350, 21);
            this.lblSTR.Name = "lblSTR";
            this.lblSTR.Size = new System.Drawing.Size(51, 13);
            this.lblSTR.TabIndex = 45;
            this.lblSTR.Text = "2/12 (18)";
            // 
            // Label9
            // 
            this.Label9.AutoSize = true;
            this.Label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label9.Location = new System.Drawing.Point(350, 8);
            this.Label9.Name = "Label9";
            this.Label9.Size = new System.Drawing.Size(32, 13);
            this.Label9.TabIndex = 44;
            this.Label9.Tag = "String_AttributeSTRShort";
            this.Label9.Text = "STR";
            // 
            // lblREA
            // 
            this.lblREA.AutoSize = true;
            this.lblREA.Location = new System.Drawing.Point(293, 21);
            this.lblREA.Name = "lblREA";
            this.lblREA.Size = new System.Drawing.Size(51, 13);
            this.lblREA.TabIndex = 43;
            this.lblREA.Text = "2/12 (18)";
            // 
            // Label7
            // 
            this.Label7.AutoSize = true;
            this.Label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label7.Location = new System.Drawing.Point(293, 8);
            this.Label7.Name = "Label7";
            this.Label7.Size = new System.Drawing.Size(32, 13);
            this.Label7.TabIndex = 42;
            this.Label7.Tag = "String_AttributeREAShort";
            this.Label7.Text = "REA";
            // 
            // lblAGI
            // 
            this.lblAGI.AutoSize = true;
            this.lblAGI.Location = new System.Drawing.Point(236, 21);
            this.lblAGI.Name = "lblAGI";
            this.lblAGI.Size = new System.Drawing.Size(51, 13);
            this.lblAGI.TabIndex = 41;
            this.lblAGI.Text = "2/12 (18)";
            // 
            // Label5
            // 
            this.Label5.AutoSize = true;
            this.Label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label5.Location = new System.Drawing.Point(236, 8);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(28, 13);
            this.Label5.TabIndex = 40;
            this.Label5.Tag = "String_AttributeAGIShort";
            this.Label5.Text = "AGI";
            // 
            // lblBOD
            // 
            this.lblBOD.AutoSize = true;
            this.lblBOD.Location = new System.Drawing.Point(179, 21);
            this.lblBOD.Name = "lblBOD";
            this.lblBOD.Size = new System.Drawing.Size(51, 13);
            this.lblBOD.TabIndex = 39;
            this.lblBOD.Text = "2/12 (18)";
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label3.Location = new System.Drawing.Point(179, 8);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(33, 13);
            this.Label3.TabIndex = 38;
            this.Label3.Tag = "String_AttributeBODShort";
            this.Label3.Text = "BOD";
            // 
            // lstMetatypes
            // 
            this.lstMetatypes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstMetatypes.FormattingEnabled = true;
            this.lstMetatypes.Location = new System.Drawing.Point(3, 30);
            this.lstMetatypes.Name = "lstMetatypes";
            this.lstMetatypes.Size = new System.Drawing.Size(158, 316);
            this.lstMetatypes.Sorted = true;
            this.lstMetatypes.TabIndex = 35;
            this.lstMetatypes.SelectedIndexChanged += new System.EventHandler(this.lstMetatypes_SelectedIndexChanged);
            this.lstMetatypes.MouseDoubleClick += new MouseEventHandler(this.lstMetatypes_DoubleClick);
            // 
            // frmKarmaMetatype
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(538, 376);
            this.ControlBox = false;
            this.Controls.Add(this.pnlMetatypes);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmKarmaMetatype";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_Metatype";
            this.Text = "Select a Metatype";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmMetatype_FormClosed);
            this.Load += new System.EventHandler(this.frmMetatype_Load);
            this.pnlMetatypes.ResumeLayout(false);
            this.pnlMetatypes.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudForce)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip tipTooltip;
        private System.Windows.Forms.Panel pnlMetatypes;
        private System.Windows.Forms.CheckBox chkBloodSpirit;
        private System.Windows.Forms.ComboBox cboPossessionMethod;
        private System.Windows.Forms.CheckBox chkPossessionBased;
        private System.Windows.Forms.NumericUpDown nudForce;
        private System.Windows.Forms.Label lblForceLabel;
        private System.Windows.Forms.ComboBox cboCategory;
        internal System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Label lblQualities;
        private System.Windows.Forms.Label label4;
        internal System.Windows.Forms.Label lblBP;
        internal System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboMetavariant;
        internal System.Windows.Forms.Button cmdOK;
        internal System.Windows.Forms.Label Label19;
        internal System.Windows.Forms.Label lblINI;
        internal System.Windows.Forms.Label Label17;
        internal System.Windows.Forms.Label lblWIL;
        internal System.Windows.Forms.Label Label15;
        internal System.Windows.Forms.Label lblLOG;
        internal System.Windows.Forms.Label Label13;
        internal System.Windows.Forms.Label lblINT;
        internal System.Windows.Forms.Label Label11;
        internal System.Windows.Forms.Label lblCHA;
        internal System.Windows.Forms.Label lblSTR;
        internal System.Windows.Forms.Label Label9;
        internal System.Windows.Forms.Label lblREA;
        internal System.Windows.Forms.Label Label7;
        internal System.Windows.Forms.Label lblAGI;
        internal System.Windows.Forms.Label Label5;
        internal System.Windows.Forms.Label lblBOD;
        internal System.Windows.Forms.Label Label3;
        internal System.Windows.Forms.ListBox lstMetatypes;
    }
}