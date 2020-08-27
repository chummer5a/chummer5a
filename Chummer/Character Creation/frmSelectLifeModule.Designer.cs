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
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.flpStage = new System.Windows.Forms.FlowLayoutPanel();
            this.tlpButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpMain.SuspendLayout();
            this.flpStage.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // treModules
            // 
            this.treModules.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treModules.Location = new System.Drawing.Point(3, 3);
            this.treModules.Name = "treModules";
            this.tlpMain.SetRowSpan(this.treModules, 7);
            this.treModules.ShowLines = false;
            this.treModules.Size = new System.Drawing.Size(295, 417);
            this.treModules.TabIndex = 0;
            this.treModules.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treModules_AfterSelect);
            this.treModules.DoubleClick += new System.EventHandler(this.treModules_DoubleClick);
            // 
            // cmdOKAdd
            // 
            this.cmdOKAdd.AutoSize = true;
            this.cmdOKAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOKAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOKAdd.Location = new System.Drawing.Point(81, 3);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(72, 23);
            this.cmdOKAdd.TabIndex = 16;
            this.cmdOKAdd.Tag = "String_AddMore";
            this.cmdOKAdd.Text = "&Add && More";
            this.cmdOKAdd.UseVisualStyleBackColor = true;
            this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.AutoSize = true;
            this.cmdCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdCancel.Location = new System.Drawing.Point(3, 3);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(72, 23);
            this.cmdCancel.TabIndex = 17;
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
            this.cmdOK.Location = new System.Drawing.Point(159, 3);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(72, 23);
            this.cmdOK.TabIndex = 15;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(354, 3);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(249, 20);
            this.txtSearch.TabIndex = 18;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // lblSearch
            // 
            this.lblSearch.Anchor = System.Windows.Forms.AnchorStyles.Right;
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
            this.lblBP.Anchor = System.Windows.Forms.AnchorStyles.Left;
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
            this.lblBPLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
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
            this.lblSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSource.AutoSize = true;
            this.lblSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSource.Location = new System.Drawing.Point(354, 57);
            this.lblSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 23;
            this.lblSource.Text = "[Source]";
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
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
            this.lblStageLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblStageLabel.AutoSize = true;
            this.lblStageLabel.Location = new System.Drawing.Point(310, 83);
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
            this.lblStage.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblStage.Location = new System.Drawing.Point(258, 6);
            this.lblStage.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblStage.Name = "lblStage";
            this.lblStage.Size = new System.Drawing.Size(41, 15);
            this.lblStage.TabIndex = 25;
            this.lblStage.Text = "[Stage]";
            // 
            // chkLimitList
            // 
            this.chkLimitList.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkLimitList.AutoSize = true;
            this.chkLimitList.Checked = true;
            this.chkLimitList.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tlpMain.SetColumnSpan(this.chkLimitList, 2);
            this.chkLimitList.Location = new System.Drawing.Point(304, 374);
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
            this.cboStage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
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
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 3;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.txtSearch, 2, 0);
            this.tlpMain.Controls.Add(this.lblSearch, 1, 0);
            this.tlpMain.Controls.Add(this.chkLimitList, 1, 5);
            this.tlpMain.Controls.Add(this.treModules, 0, 0);
            this.tlpMain.Controls.Add(this.lblStageLabel, 1, 3);
            this.tlpMain.Controls.Add(this.lblBP, 2, 1);
            this.tlpMain.Controls.Add(this.lblBPLabel, 1, 1);
            this.tlpMain.Controls.Add(this.lblSource, 2, 2);
            this.tlpMain.Controls.Add(this.lblSourceLabel, 1, 2);
            this.tlpMain.Controls.Add(this.flpStage, 2, 3);
            this.tlpMain.Controls.Add(this.tlpButtons, 1, 6);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 7;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(606, 423);
            this.tlpMain.TabIndex = 28;
            // 
            // flpStage
            // 
            this.flpStage.AutoSize = true;
            this.flpStage.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpStage.Controls.Add(this.cboStage);
            this.flpStage.Controls.Add(this.lblStage);
            this.flpStage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpStage.Location = new System.Drawing.Point(351, 76);
            this.flpStage.Margin = new System.Windows.Forms.Padding(0);
            this.flpStage.Name = "flpStage";
            this.flpStage.Size = new System.Drawing.Size(255, 27);
            this.flpStage.TabIndex = 27;
            this.flpStage.WrapContents = false;
            // 
            // tlpButtons
            // 
            this.tlpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpButtons.AutoSize = true;
            this.tlpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpButtons.ColumnCount = 3;
            this.tlpMain.SetColumnSpan(this.tlpButtons, 2);
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpButtons.Controls.Add(this.cmdCancel, 0, 0);
            this.tlpButtons.Controls.Add(this.cmdOKAdd, 1, 0);
            this.tlpButtons.Controls.Add(this.cmdOK, 2, 0);
            this.tlpButtons.Location = new System.Drawing.Point(372, 394);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButtons.Size = new System.Drawing.Size(234, 29);
            this.tlpButtons.TabIndex = 29;
            // 
            // frmSelectLifeModule
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.tlpMain);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectLifeModule";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Select Life Module";
            this.Load += new System.EventHandler(this.frmSelectLifeModule_Load);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.flpStage.ResumeLayout(false);
            this.flpStage.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
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
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.FlowLayoutPanel flpStage;
        private BufferedTableLayoutPanel tlpButtons;
    }
}
