namespace Chummer
{
    partial class frmSelectLifeModule
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
            this.treModules = new System.Windows.Forms.TreeView();
            this.cmdOKAdd = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.lblBP = new System.Windows.Forms.Label();
            this.lblBPLabel = new System.Windows.Forms.Label();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.lblStageLabel = new System.Windows.Forms.Label();
            this.lblStage = new System.Windows.Forms.Label();
            this.chkLimitList = new System.Windows.Forms.CheckBox();
            this.cboStage = new Chummer.ElasticComboBox();
            this.tableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // treModules
            // 
            this.treModules.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treModules.Location = new System.Drawing.Point(3, 3);
            this.treModules.Name = "treModules";
            this.tableLayoutPanel1.SetRowSpan(this.treModules, 6);
            this.treModules.ShowLines = false;
            this.treModules.Size = new System.Drawing.Size(295, 417);
            this.treModules.TabIndex = 0;
            this.treModules.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treModules_AfterSelect);
            this.treModules.DoubleClick += new System.EventHandler(this.treModules_DoubleClick);
            // 
            // cmdOKAdd
            // 
            this.cmdOKAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOKAdd.AutoSize = true;
            this.cmdOKAdd.Location = new System.Drawing.Point(81, 0);
            this.cmdOKAdd.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(75, 23);
            this.cmdOKAdd.TabIndex = 16;
            this.cmdOKAdd.Tag = "String_AddMore";
            this.cmdOKAdd.Text = "&Add && More";
            this.cmdOKAdd.UseVisualStyleBackColor = true;
            this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.AutoSize = true;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(0, 0);
            this.cmdCancel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 17;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.AutoSize = true;
            this.cmdOK.Location = new System.Drawing.Point(162, 0);
            this.cmdOK.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 15;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // txtSearch
            // 
            this.txtSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSearch.Location = new System.Drawing.Point(354, 3);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(249, 20);
            this.txtSearch.TabIndex = 18;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // lblSearch
            // 
            this.lblSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new System.Drawing.Point(304, 6);
            this.lblSearch.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(44, 13);
            this.lblSearch.TabIndex = 19;
            this.lblSearch.Tag = "Label_Search";
            this.lblSearch.Text = "Search:";
            // 
            // lblBP
            // 
            this.lblBP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblBP.AutoSize = true;
            this.lblBP.Location = new System.Drawing.Point(354, 32);
            this.lblBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBP.Name = "lblBP";
            this.lblBP.Size = new System.Drawing.Size(27, 13);
            this.lblBP.TabIndex = 21;
            this.lblBP.Text = "[BP]";
            // 
            // lblBPLabel
            // 
            this.lblBPLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBPLabel.AutoSize = true;
            this.lblBPLabel.Location = new System.Drawing.Point(308, 32);
            this.lblBPLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBPLabel.Name = "lblBPLabel";
            this.lblBPLabel.Size = new System.Drawing.Size(40, 13);
            this.lblBPLabel.TabIndex = 20;
            this.lblBPLabel.Tag = "Label_Karma";
            this.lblBPLabel.Text = "Karma:";
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.lblSource.Location = new System.Drawing.Point(354, 57);
            this.lblSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 23;
            this.lblSource.Text = "[Source]";
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(304, 57);
            this.lblSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 22;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            // 
            // lblStageLabel
            // 
            this.lblStageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStageLabel.AutoSize = true;
            this.lblStageLabel.Location = new System.Drawing.Point(310, 82);
            this.lblStageLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblStageLabel.Name = "lblStageLabel";
            this.lblStageLabel.Size = new System.Drawing.Size(38, 13);
            this.lblStageLabel.TabIndex = 24;
            this.lblStageLabel.Tag = "Label_Stage";
            this.lblStageLabel.Text = "Stage:";
            // 
            // lblStage
            // 
            this.lblStage.AutoSize = true;
            this.lblStage.Location = new System.Drawing.Point(258, 6);
            this.lblStage.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblStage.Name = "lblStage";
            this.lblStage.Size = new System.Drawing.Size(41, 13);
            this.lblStage.TabIndex = 25;
            this.lblStage.Text = "[Stage]";
            // 
            // chkLimitList
            // 
            this.chkLimitList.AutoSize = true;
            this.chkLimitList.Checked = true;
            this.chkLimitList.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tableLayoutPanel1.SetColumnSpan(this.chkLimitList, 2);
            this.chkLimitList.Location = new System.Drawing.Point(304, 105);
            this.chkLimitList.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkLimitList.Name = "chkLimitList";
            this.chkLimitList.Size = new System.Drawing.Size(189, 17);
            this.chkLimitList.TabIndex = 26;
            this.chkLimitList.Tag = "Checkbox_SelectLifeModule_LimitList";
            this.chkLimitList.Text = "Show only Life Modules I can take";
            this.chkLimitList.UseVisualStyleBackColor = true;
            this.chkLimitList.Click += new System.EventHandler(this.chkLimitList_Click);
            // 
            // cboStage
            // 
            this.cboStage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboStage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStage.FormattingEnabled = true;
            this.cboStage.Location = new System.Drawing.Point(3, 3);
            this.cboStage.Name = "cboStage";
            this.cboStage.Size = new System.Drawing.Size(249, 21);
            this.cboStage.TabIndex = 27;
            this.cboStage.TooltipText = "";
            this.cboStage.Visible = false;
            this.cboStage.SelectionChangeCommitted += new System.EventHandler(this.cboStage_SelectionChangeCommitted);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 301F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.txtSearch, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblSearch, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.chkLimitList, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.treModules, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblStageLabel, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblBP, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblBPLabel, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblSource, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblSourceLabel, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 2, 5);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(9, 9);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(606, 423);
            this.tableLayoutPanel1.TabIndex = 28;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.cboStage);
            this.flowLayoutPanel1.Controls.Add(this.lblStage);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(351, 76);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(255, 25);
            this.flowLayoutPanel1.TabIndex = 27;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.Controls.Add(this.cmdOK);
            this.flowLayoutPanel2.Controls.Add(this.cmdOKAdd);
            this.flowLayoutPanel2.Controls.Add(this.cmdCancel);
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(366, 397);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(237, 23);
            this.flowLayoutPanel2.TabIndex = 28;
            // 
            // frmSelectLifeModule
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(640, 10000);
            this.MinimizeBox = false;
            this.Name = "frmSelectLifeModule";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Select Life Module";
            this.Load += new System.EventHandler(this.frmSelectLifeModule_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treModules;
        private System.Windows.Forms.Button cmdOKAdd;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.Label lblBP;
        private System.Windows.Forms.Label lblBPLabel;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblSourceLabel;
        private System.Windows.Forms.Label lblStageLabel;
        private System.Windows.Forms.Label lblStage;
        private System.Windows.Forms.CheckBox chkLimitList;
        private ElasticComboBox cboStage;
        private Chummer.BufferedTableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
    }
}
