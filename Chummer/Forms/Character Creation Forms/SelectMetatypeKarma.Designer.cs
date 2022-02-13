using System;
using System.Windows;
using System.Windows.Forms;

namespace Chummer
{
    partial class SelectMetatypeKarma
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
            if (disposing)
            {
                components?.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectMetatypeKarma));
            this.tlpMetatypes = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lstMetatypes = new System.Windows.Forms.ListBox();
            this.cboCategory = new Chummer.ElasticComboBox();
            this.lblQualitiesLabel = new System.Windows.Forms.Label();
            this.pnlQualities = new System.Windows.Forms.Panel();
            this.lblQualities = new System.Windows.Forms.Label();
            this.lblCHA = new System.Windows.Forms.Label();
            this.lblINT = new System.Windows.Forms.Label();
            this.lblLOG = new System.Windows.Forms.Label();
            this.lblWIL = new System.Windows.Forms.Label();
            this.lblWILLabel = new System.Windows.Forms.Label();
            this.lblLOGLabel = new System.Windows.Forms.Label();
            this.lblINTLabel = new System.Windows.Forms.Label();
            this.lblCHALabel = new System.Windows.Forms.Label();
            this.lblSTR = new System.Windows.Forms.Label();
            this.lblSTRLabel = new System.Windows.Forms.Label();
            this.lblREA = new System.Windows.Forms.Label();
            this.lblAGI = new System.Windows.Forms.Label();
            this.lblBOD = new System.Windows.Forms.Label();
            this.lblREALabel = new System.Windows.Forms.Label();
            this.lblAGILabel = new System.Windows.Forms.Label();
            this.lblKarmaLabel = new System.Windows.Forms.Label();
            this.lblKarma = new System.Windows.Forms.Label();
            this.lblBODLabel = new System.Windows.Forms.Label();
            this.tlpSpirits = new Chummer.BufferedTableLayoutPanel(this.components);
            this.chkPossessionBased = new Chummer.ColorableCheckBox(this.components);
            this.cboPossessionMethod = new Chummer.ElasticComboBox();
            this.lblForceLabel = new System.Windows.Forms.Label();
            this.nudForce = new Chummer.NumericUpDownEx();
            this.tlpMetavariant = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblMetavariantLabel = new System.Windows.Forms.Label();
            this.cboMetavariant = new Chummer.ElasticComboBox();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.lblSource = new System.Windows.Forms.Label();
            this.tlpButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lblSearchLabel = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.tlpMetatypes.SuspendLayout();
            this.pnlQualities.SuspendLayout();
            this.tlpSpirits.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudForce)).BeginInit();
            this.tlpMetavariant.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMetatypes
            // 
            this.tlpMetatypes.ColumnCount = 7;
            this.tlpMetatypes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 43F));
            this.tlpMetatypes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 9.5F));
            this.tlpMetatypes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 9.5F));
            this.tlpMetatypes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 9.5F));
            this.tlpMetatypes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 9.5F));
            this.tlpMetatypes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 9.5F));
            this.tlpMetatypes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 9.5F));
            this.tlpMetatypes.Controls.Add(this.txtSearch, 2, 0);
            this.tlpMetatypes.Controls.Add(this.lblSearchLabel, 1, 0);
            this.tlpMetatypes.Controls.Add(this.lstMetatypes, 0, 1);
            this.tlpMetatypes.Controls.Add(this.cboCategory, 0, 0);
            this.tlpMetatypes.Controls.Add(this.lblQualitiesLabel, 1, 7);
            this.tlpMetatypes.Controls.Add(this.pnlQualities, 2, 7);
            this.tlpMetatypes.Controls.Add(this.lblCHA, 6, 3);
            this.tlpMetatypes.Controls.Add(this.lblINT, 6, 4);
            this.tlpMetatypes.Controls.Add(this.lblLOG, 6, 5);
            this.tlpMetatypes.Controls.Add(this.lblWIL, 6, 6);
            this.tlpMetatypes.Controls.Add(this.lblWILLabel, 5, 6);
            this.tlpMetatypes.Controls.Add(this.lblLOGLabel, 5, 5);
            this.tlpMetatypes.Controls.Add(this.lblINTLabel, 5, 4);
            this.tlpMetatypes.Controls.Add(this.lblCHALabel, 5, 3);
            this.tlpMetatypes.Controls.Add(this.lblSTR, 4, 6);
            this.tlpMetatypes.Controls.Add(this.lblSTRLabel, 3, 6);
            this.tlpMetatypes.Controls.Add(this.lblREA, 4, 5);
            this.tlpMetatypes.Controls.Add(this.lblAGI, 4, 4);
            this.tlpMetatypes.Controls.Add(this.lblBOD, 4, 3);
            this.tlpMetatypes.Controls.Add(this.lblREALabel, 3, 5);
            this.tlpMetatypes.Controls.Add(this.lblAGILabel, 3, 4);
            this.tlpMetatypes.Controls.Add(this.lblKarmaLabel, 1, 3);
            this.tlpMetatypes.Controls.Add(this.lblKarma, 2, 3);
            this.tlpMetatypes.Controls.Add(this.lblBODLabel, 3, 3);
            this.tlpMetatypes.Controls.Add(this.tlpSpirits, 1, 2);
            this.tlpMetatypes.Controls.Add(this.tlpMetavariant, 1, 1);
            this.tlpMetatypes.Controls.Add(this.lblSourceLabel, 1, 6);
            this.tlpMetatypes.Controls.Add(this.lblSource, 2, 6);
            this.tlpMetatypes.Controls.Add(this.tlpButtons, 1, 8);
            this.tlpMetatypes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMetatypes.Location = new System.Drawing.Point(9, 9);
            this.tlpMetatypes.Name = "tlpMetatypes";
            this.tlpMetatypes.RowCount = 9;
            this.tlpMetatypes.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMetatypes.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMetatypes.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMetatypes.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMetatypes.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMetatypes.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMetatypes.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMetatypes.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMetatypes.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMetatypes.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMetatypes.Size = new System.Drawing.Size(766, 543);
            this.tlpMetatypes.TabIndex = 70;
            // 
            // lstMetatypes
            // 
            this.lstMetatypes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstMetatypes.FormattingEnabled = true;
            this.lstMetatypes.Location = new System.Drawing.Point(3, 30);
            this.lstMetatypes.Margin = new System.Windows.Forms.Padding(3, 3, 23, 3);
            this.lstMetatypes.Name = "lstMetatypes";
            this.tlpMetatypes.SetRowSpan(this.lstMetatypes, 8);
            this.lstMetatypes.Size = new System.Drawing.Size(303, 510);
            this.lstMetatypes.Sorted = true;
            this.lstMetatypes.TabIndex = 35;
            this.lstMetatypes.SelectedIndexChanged += new System.EventHandler(this.lstMetatypes_SelectedIndexChanged);
            this.lstMetatypes.DoubleClick += new System.EventHandler(this.cmdOK_Click);
            // 
            // cboCategory
            // 
            this.cboCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategory.FormattingEnabled = true;
            this.cboCategory.Location = new System.Drawing.Point(3, 3);
            this.cboCategory.Margin = new System.Windows.Forms.Padding(3, 3, 23, 3);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(303, 21);
            this.cboCategory.TabIndex = 66;
            this.cboCategory.TooltipText = "";
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // lblQualitiesLabel
            // 
            this.lblQualitiesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblQualitiesLabel.AutoSize = true;
            this.lblQualitiesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblQualitiesLabel.Location = new System.Drawing.Point(338, 187);
            this.lblQualitiesLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblQualitiesLabel.Name = "lblQualitiesLabel";
            this.lblQualitiesLabel.Size = new System.Drawing.Size(60, 13);
            this.lblQualitiesLabel.TabIndex = 62;
            this.lblQualitiesLabel.Tag = "String_Qualities";
            this.lblQualitiesLabel.Text = "Qualities:";
            // 
            // pnlQualities
            // 
            this.pnlQualities.AutoScroll = true;
            this.tlpMetatypes.SetColumnSpan(this.pnlQualities, 5);
            this.pnlQualities.Controls.Add(this.lblQualities);
            this.pnlQualities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlQualities.Location = new System.Drawing.Point(401, 181);
            this.pnlQualities.Margin = new System.Windows.Forms.Padding(0);
            this.pnlQualities.Name = "pnlQualities";
            this.pnlQualities.Padding = new System.Windows.Forms.Padding(3, 6, 13, 6);
            this.pnlQualities.Size = new System.Drawing.Size(365, 333);
            this.pnlQualities.TabIndex = 71;
            // 
            // lblQualities
            // 
            this.lblQualities.AutoSize = true;
            this.lblQualities.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblQualities.Location = new System.Drawing.Point(3, 6);
            this.lblQualities.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblQualities.Name = "lblQualities";
            this.lblQualities.Size = new System.Drawing.Size(33, 13);
            this.lblQualities.TabIndex = 63;
            this.lblQualities.Tag = "String_None";
            this.lblQualities.Text = "None";
            // 
            // lblCHA
            // 
            this.lblCHA.AutoSize = true;
            this.lblCHA.Location = new System.Drawing.Point(692, 87);
            this.lblCHA.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCHA.Name = "lblCHA";
            this.lblCHA.Size = new System.Drawing.Size(51, 13);
            this.lblCHA.TabIndex = 47;
            this.lblCHA.Text = "2/12 (18)";
            // 
            // lblINT
            // 
            this.lblINT.AutoSize = true;
            this.lblINT.Location = new System.Drawing.Point(692, 112);
            this.lblINT.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblINT.Name = "lblINT";
            this.lblINT.Size = new System.Drawing.Size(51, 13);
            this.lblINT.TabIndex = 49;
            this.lblINT.Text = "2/12 (18)";
            // 
            // lblLOG
            // 
            this.lblLOG.AutoSize = true;
            this.lblLOG.Location = new System.Drawing.Point(692, 137);
            this.lblLOG.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLOG.Name = "lblLOG";
            this.lblLOG.Size = new System.Drawing.Size(51, 13);
            this.lblLOG.TabIndex = 51;
            this.lblLOG.Text = "2/12 (18)";
            // 
            // lblWIL
            // 
            this.lblWIL.AutoSize = true;
            this.lblWIL.Location = new System.Drawing.Point(692, 162);
            this.lblWIL.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWIL.Name = "lblWIL";
            this.lblWIL.Size = new System.Drawing.Size(51, 13);
            this.lblWIL.TabIndex = 53;
            this.lblWIL.Text = "2/12 (18)";
            // 
            // lblWILLabel
            // 
            this.lblWILLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblWILLabel.AutoSize = true;
            this.lblWILLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWILLabel.Location = new System.Drawing.Point(656, 162);
            this.lblWILLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWILLabel.Name = "lblWILLabel";
            this.lblWILLabel.Size = new System.Drawing.Size(30, 13);
            this.lblWILLabel.TabIndex = 52;
            this.lblWILLabel.Tag = "String_AttributeWILShort";
            this.lblWILLabel.Text = "WIL";
            // 
            // lblLOGLabel
            // 
            this.lblLOGLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblLOGLabel.AutoSize = true;
            this.lblLOGLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLOGLabel.Location = new System.Drawing.Point(654, 137);
            this.lblLOGLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLOGLabel.Name = "lblLOGLabel";
            this.lblLOGLabel.Size = new System.Drawing.Size(32, 13);
            this.lblLOGLabel.TabIndex = 50;
            this.lblLOGLabel.Tag = "String_AttributeLOGShort";
            this.lblLOGLabel.Text = "LOG";
            // 
            // lblINTLabel
            // 
            this.lblINTLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblINTLabel.AutoSize = true;
            this.lblINTLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblINTLabel.Location = new System.Drawing.Point(658, 112);
            this.lblINTLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblINTLabel.Name = "lblINTLabel";
            this.lblINTLabel.Size = new System.Drawing.Size(28, 13);
            this.lblINTLabel.TabIndex = 48;
            this.lblINTLabel.Tag = "String_AttributeINTShort";
            this.lblINTLabel.Text = "INT";
            // 
            // lblCHALabel
            // 
            this.lblCHALabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCHALabel.AutoSize = true;
            this.lblCHALabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCHALabel.Location = new System.Drawing.Point(654, 87);
            this.lblCHALabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCHALabel.Name = "lblCHALabel";
            this.lblCHALabel.Size = new System.Drawing.Size(32, 13);
            this.lblCHALabel.TabIndex = 46;
            this.lblCHALabel.Tag = "String_AttributeCHAShort";
            this.lblCHALabel.Text = "CHA";
            // 
            // lblSTR
            // 
            this.lblSTR.AutoSize = true;
            this.lblSTR.Location = new System.Drawing.Point(548, 162);
            this.lblSTR.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSTR.Name = "lblSTR";
            this.lblSTR.Size = new System.Drawing.Size(51, 13);
            this.lblSTR.TabIndex = 45;
            this.lblSTR.Text = "2/12 (18)";
            // 
            // lblSTRLabel
            // 
            this.lblSTRLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSTRLabel.AutoSize = true;
            this.lblSTRLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSTRLabel.Location = new System.Drawing.Point(510, 162);
            this.lblSTRLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSTRLabel.Name = "lblSTRLabel";
            this.lblSTRLabel.Size = new System.Drawing.Size(32, 13);
            this.lblSTRLabel.TabIndex = 44;
            this.lblSTRLabel.Tag = "String_AttributeSTRShort";
            this.lblSTRLabel.Text = "STR";
            // 
            // lblREA
            // 
            this.lblREA.AutoSize = true;
            this.lblREA.Location = new System.Drawing.Point(548, 137);
            this.lblREA.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblREA.Name = "lblREA";
            this.lblREA.Size = new System.Drawing.Size(51, 13);
            this.lblREA.TabIndex = 43;
            this.lblREA.Text = "2/12 (18)";
            // 
            // lblAGI
            // 
            this.lblAGI.AutoSize = true;
            this.lblAGI.Location = new System.Drawing.Point(548, 112);
            this.lblAGI.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAGI.Name = "lblAGI";
            this.lblAGI.Size = new System.Drawing.Size(51, 13);
            this.lblAGI.TabIndex = 41;
            this.lblAGI.Text = "2/12 (18)";
            // 
            // lblBOD
            // 
            this.lblBOD.AutoSize = true;
            this.lblBOD.Location = new System.Drawing.Point(548, 87);
            this.lblBOD.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBOD.Name = "lblBOD";
            this.lblBOD.Size = new System.Drawing.Size(51, 13);
            this.lblBOD.TabIndex = 39;
            this.lblBOD.Text = "2/12 (18)";
            // 
            // lblREALabel
            // 
            this.lblREALabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblREALabel.AutoSize = true;
            this.lblREALabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblREALabel.Location = new System.Drawing.Point(510, 137);
            this.lblREALabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblREALabel.Name = "lblREALabel";
            this.lblREALabel.Size = new System.Drawing.Size(32, 13);
            this.lblREALabel.TabIndex = 42;
            this.lblREALabel.Tag = "String_AttributeREAShort";
            this.lblREALabel.Text = "REA";
            // 
            // lblAGILabel
            // 
            this.lblAGILabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblAGILabel.AutoSize = true;
            this.lblAGILabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAGILabel.Location = new System.Drawing.Point(514, 112);
            this.lblAGILabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAGILabel.Name = "lblAGILabel";
            this.lblAGILabel.Size = new System.Drawing.Size(28, 13);
            this.lblAGILabel.TabIndex = 40;
            this.lblAGILabel.Tag = "String_AttributeAGIShort";
            this.lblAGILabel.Text = "AGI";
            // 
            // lblKarmaLabel
            // 
            this.lblKarmaLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKarmaLabel.AutoSize = true;
            this.lblKarmaLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblKarmaLabel.Location = new System.Drawing.Point(352, 87);
            this.lblKarmaLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaLabel.Name = "lblKarmaLabel";
            this.lblKarmaLabel.Size = new System.Drawing.Size(46, 13);
            this.lblKarmaLabel.TabIndex = 60;
            this.lblKarmaLabel.Tag = "Label_Karma";
            this.lblKarmaLabel.Text = "Karma:";
            // 
            // lblKarma
            // 
            this.lblKarma.AutoSize = true;
            this.lblKarma.Location = new System.Drawing.Point(404, 87);
            this.lblKarma.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarma.Name = "lblKarma";
            this.lblKarma.Size = new System.Drawing.Size(13, 13);
            this.lblKarma.TabIndex = 61;
            this.lblKarma.Text = "0";
            // 
            // lblBODLabel
            // 
            this.lblBODLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblBODLabel.AutoSize = true;
            this.lblBODLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBODLabel.Location = new System.Drawing.Point(509, 87);
            this.lblBODLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBODLabel.Name = "lblBODLabel";
            this.lblBODLabel.Size = new System.Drawing.Size(33, 13);
            this.lblBODLabel.TabIndex = 38;
            this.lblBODLabel.Tag = "String_AttributeBODShort";
            this.lblBODLabel.Text = "BOD";
            // 
            // tlpSpirits
            // 
            this.tlpSpirits.AutoSize = true;
            this.tlpSpirits.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpSpirits.ColumnCount = 4;
            this.tlpMetatypes.SetColumnSpan(this.tlpSpirits, 6);
            this.tlpSpirits.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpSpirits.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpSpirits.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpSpirits.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSpirits.Controls.Add(this.chkPossessionBased, 2, 0);
            this.tlpSpirits.Controls.Add(this.cboPossessionMethod, 3, 0);
            this.tlpSpirits.Controls.Add(this.lblForceLabel, 0, 0);
            this.tlpSpirits.Controls.Add(this.nudForce, 1, 0);
            this.tlpSpirits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSpirits.Location = new System.Drawing.Point(329, 54);
            this.tlpSpirits.Margin = new System.Windows.Forms.Padding(0);
            this.tlpSpirits.Name = "tlpSpirits";
            this.tlpSpirits.RowCount = 1;
            this.tlpSpirits.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSpirits.Size = new System.Drawing.Size(437, 27);
            this.tlpSpirits.TabIndex = 70;
            // 
            // chkPossessionBased
            // 
            this.chkPossessionBased.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkPossessionBased.AutoSize = true;
            this.chkPossessionBased.DefaultColorScheme = true;
            this.chkPossessionBased.Location = new System.Drawing.Point(98, 5);
            this.chkPossessionBased.Name = "chkPossessionBased";
            this.chkPossessionBased.Size = new System.Drawing.Size(211, 17);
            this.chkPossessionBased.TabIndex = 64;
            this.chkPossessionBased.Tag = "Checkbox_Metatype_PossessionTradition";
            this.chkPossessionBased.Text = "Summoned by Possess-based Tradition";
            this.chkPossessionBased.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkPossessionBased.UseVisualStyleBackColor = true;
            this.chkPossessionBased.Visible = false;
            this.chkPossessionBased.CheckedChanged += new System.EventHandler(this.chkPossessionBased_CheckedChanged);
            // 
            // cboPossessionMethod
            // 
            this.cboPossessionMethod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboPossessionMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPossessionMethod.Enabled = false;
            this.cboPossessionMethod.FormattingEnabled = true;
            this.cboPossessionMethod.Location = new System.Drawing.Point(315, 3);
            this.cboPossessionMethod.Name = "cboPossessionMethod";
            this.cboPossessionMethod.Size = new System.Drawing.Size(119, 21);
            this.cboPossessionMethod.TabIndex = 65;
            this.cboPossessionMethod.TooltipText = "";
            this.cboPossessionMethod.Visible = false;
            // 
            // lblForceLabel
            // 
            this.lblForceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblForceLabel.AutoSize = true;
            this.lblForceLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblForceLabel.Location = new System.Drawing.Point(3, 7);
            this.lblForceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblForceLabel.Name = "lblForceLabel";
            this.lblForceLabel.Size = new System.Drawing.Size(48, 13);
            this.lblForceLabel.TabIndex = 56;
            this.lblForceLabel.Tag = "String_Force";
            this.lblForceLabel.Text = "FORCE";
            this.lblForceLabel.Visible = false;
            // 
            // nudForce
            // 
            this.nudForce.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudForce.AutoSize = true;
            this.nudForce.Location = new System.Drawing.Point(57, 3);
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
            this.nudForce.Size = new System.Drawing.Size(35, 20);
            this.nudForce.TabIndex = 57;
            this.nudForce.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudForce.Visible = false;
            // 
            // tlpMetavariant
            // 
            this.tlpMetavariant.AutoSize = true;
            this.tlpMetavariant.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMetavariant.ColumnCount = 2;
            this.tlpMetatypes.SetColumnSpan(this.tlpMetavariant, 6);
            this.tlpMetavariant.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMetavariant.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMetavariant.Controls.Add(this.lblMetavariantLabel, 0, 0);
            this.tlpMetavariant.Controls.Add(this.cboMetavariant, 1, 0);
            this.tlpMetavariant.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMetavariant.Location = new System.Drawing.Point(329, 27);
            this.tlpMetavariant.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMetavariant.Name = "tlpMetavariant";
            this.tlpMetavariant.RowCount = 1;
            this.tlpMetavariant.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMetavariant.Size = new System.Drawing.Size(437, 27);
            this.tlpMetavariant.TabIndex = 73;
            // 
            // lblMetavariantLabel
            // 
            this.lblMetavariantLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMetavariantLabel.AutoSize = true;
            this.lblMetavariantLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMetavariantLabel.Location = new System.Drawing.Point(3, 7);
            this.lblMetavariantLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetavariantLabel.Name = "lblMetavariantLabel";
            this.lblMetavariantLabel.Size = new System.Drawing.Size(78, 13);
            this.lblMetavariantLabel.TabIndex = 58;
            this.lblMetavariantLabel.Tag = "Label_Metavariant";
            this.lblMetavariantLabel.Text = "Metavariant:";
            // 
            // cboMetavariant
            // 
            this.cboMetavariant.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboMetavariant.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMetavariant.FormattingEnabled = true;
            this.cboMetavariant.Location = new System.Drawing.Point(87, 3);
            this.cboMetavariant.Name = "cboMetavariant";
            this.cboMetavariant.Size = new System.Drawing.Size(347, 21);
            this.cboMetavariant.TabIndex = 59;
            this.cboMetavariant.TooltipText = "";
            this.cboMetavariant.SelectedIndexChanged += new System.EventHandler(this.cboMetavariant_SelectedIndexChanged);
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSourceLabel.Location = new System.Drawing.Point(347, 162);
            this.lblSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(51, 13);
            this.lblSourceLabel.TabIndex = 110;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            this.lblSourceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblSource
            // 
            this.lblSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSource.AutoSize = true;
            this.lblSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSource.Location = new System.Drawing.Point(404, 162);
            this.lblSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 111;
            this.lblSource.Text = "[Source]";
            this.lblSource.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // tlpButtons
            // 
            this.tlpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpButtons.AutoSize = true;
            this.tlpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpButtons.ColumnCount = 2;
            this.tlpMetatypes.SetColumnSpan(this.tlpButtons, 6);
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.Controls.Add(this.cmdCancel, 0, 0);
            this.tlpButtons.Controls.Add(this.cmdOK, 1, 0);
            this.tlpButtons.Location = new System.Drawing.Point(654, 514);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.Size = new System.Drawing.Size(112, 29);
            this.tlpButtons.TabIndex = 112;
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
            this.cmdCancel.TabIndex = 12;
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
            this.cmdOK.TabIndex = 11;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lblSearchLabel
            // 
            this.lblSearchLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSearchLabel.AutoSize = true;
            this.lblSearchLabel.Location = new System.Drawing.Point(354, 7);
            this.lblSearchLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSearchLabel.Name = "lblSearchLabel";
            this.lblSearchLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSearchLabel.TabIndex = 113;
            this.lblSearchLabel.Tag = "Label_Search";
            this.lblSearchLabel.Text = "&Search:";
            this.lblSearchLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMetatypes.SetColumnSpan(this.txtSearch, 5);
            this.txtSearch.Location = new System.Drawing.Point(404, 3);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(359, 20);
            this.txtSearch.TabIndex = 114;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // frmKarmaMetatype
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.ControlBox = false;
            this.Controls.Add(this.tlpMetatypes);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectMetatypeKarma";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_Metatype";
            this.Text = "Select a Metatype";
            this.Load += new System.EventHandler(this.frmMetatype_Load);
            this.tlpMetatypes.ResumeLayout(false);
            this.tlpMetatypes.PerformLayout();
            this.pnlQualities.ResumeLayout(false);
            this.pnlQualities.PerformLayout();
            this.tlpSpirits.ResumeLayout(false);
            this.tlpSpirits.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudForce)).EndInit();
            this.tlpMetavariant.ResumeLayout(false);
            this.tlpMetavariant.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private ElasticComboBox cboPossessionMethod;
        private Chummer.ColorableCheckBox chkPossessionBased;
        private Chummer.NumericUpDownEx nudForce;
        private System.Windows.Forms.Label lblForceLabel;
        private ElasticComboBox cboCategory;
        private System.Windows.Forms.Label lblQualities;
        internal System.Windows.Forms.Label lblKarma;
        private ElasticComboBox cboMetavariant;
        internal System.Windows.Forms.Label lblWILLabel;
        internal System.Windows.Forms.Label lblWIL;
        internal System.Windows.Forms.Label lblLOGLabel;
        internal System.Windows.Forms.Label lblLOG;
        internal System.Windows.Forms.Label lblINTLabel;
        internal System.Windows.Forms.Label lblINT;
        internal System.Windows.Forms.Label lblCHALabel;
        internal System.Windows.Forms.Label lblCHA;
        internal System.Windows.Forms.Label lblSTR;
        internal System.Windows.Forms.Label lblSTRLabel;
        internal System.Windows.Forms.Label lblREA;
        internal System.Windows.Forms.Label lblREALabel;
        internal System.Windows.Forms.Label lblAGI;
        internal System.Windows.Forms.Label lblAGILabel;
        internal System.Windows.Forms.Label lblBOD;
        internal System.Windows.Forms.Label lblBODLabel;
        internal System.Windows.Forms.ListBox lstMetatypes;
        private BufferedTableLayoutPanel tlpMetatypes;
        private BufferedTableLayoutPanel tlpSpirits;
        private Label lblMetavariantLabel;
        internal Label lblKarmaLabel;
        private Label lblQualitiesLabel;
        private Panel pnlQualities;
        private BufferedTableLayoutPanel tlpMetavariant;
        internal Label lblSourceLabel;
        internal Label lblSource;
        private BufferedTableLayoutPanel tlpButtons;
        internal Button cmdCancel;
        internal Button cmdOK;
        private Label lblSearchLabel;
        private TextBox txtSearch;
    }
}
