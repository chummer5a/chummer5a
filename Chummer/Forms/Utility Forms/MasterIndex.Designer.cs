namespace Chummer
{
    partial class MasterIndex
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
                _dicCachedNotes?.Dispose();
                foreach (ListItem objExistingItem in _lstItems)
                    ((MasterIndexEntry)objExistingItem.Value).Dispose();
                Utils.ListItemListPool.Return(_lstFileNamesWithItems);
                Utils.ListItemListPool.Return(_lstItems);
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MasterIndex));
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpRight = new Chummer.BufferedTableLayoutPanel(this.components);
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.lblSource = new Chummer.LabelWithToolTip();
            this.lblSourceClickReminder = new System.Windows.Forms.Label();
            this.txtNotes = new System.Windows.Forms.TextBox();
            this.tlpTopLeft = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblFile = new System.Windows.Forms.Label();
            this.cboFile = new Chummer.ElasticComboBox();
            this.lstItems = new System.Windows.Forms.ListBox();
            this.tlpGameplayOption = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdEditCharacterSetting = new Chummer.ButtonWithToolTip(this.components);
            this.lblCharacterSetting = new System.Windows.Forms.Label();
            this.cboCharacterSetting = new Chummer.ElasticComboBox();
            this.tlpMain.SuspendLayout();
            this.tlpRight.SuspendLayout();
            this.tlpTopLeft.SuspendLayout();
            this.tlpGameplayOption.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.Controls.Add(this.tlpRight, 1, 0);
            this.tlpMain.Controls.Add(this.tlpTopLeft, 0, 0);
            this.tlpMain.Controls.Add(this.lstItems, 0, 1);
            this.tlpMain.Controls.Add(this.tlpGameplayOption, 0, 2);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 3;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(766, 543);
            this.tlpMain.TabIndex = 0;
            // 
            // tlpRight
            // 
            this.tlpRight.AutoSize = true;
            this.tlpRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.ColumnCount = 3;
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRight.Controls.Add(this.txtSearch, 1, 0);
            this.tlpRight.Controls.Add(this.lblSearch, 0, 0);
            this.tlpRight.Controls.Add(this.lblSourceLabel, 0, 1);
            this.tlpRight.Controls.Add(this.lblSource, 1, 1);
            this.tlpRight.Controls.Add(this.lblSourceClickReminder, 2, 1);
            this.tlpRight.Controls.Add(this.txtNotes, 0, 2);
            this.tlpRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRight.Location = new System.Drawing.Point(383, 0);
            this.tlpRight.Margin = new System.Windows.Forms.Padding(0);
            this.tlpRight.Name = "tlpRight";
            this.tlpRight.RowCount = 3;
            this.tlpMain.SetRowSpan(this.tlpRight, 2);
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRight.Size = new System.Drawing.Size(383, 514);
            this.tlpRight.TabIndex = 1;
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpRight.SetColumnSpan(this.txtSearch, 2);
            this.txtSearch.Location = new System.Drawing.Point(53, 3);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(327, 20);
            this.txtSearch.TabIndex = 0;
            this.txtSearch.TextChanged += new System.EventHandler(this.RefreshList);
            // 
            // lblSearch
            // 
            this.lblSearch.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new System.Drawing.Point(3, 6);
            this.lblSearch.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(44, 13);
            this.lblSearch.TabIndex = 1;
            this.lblSearch.Tag = "String_Search";
            this.lblSearch.Text = "Search:";
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(3, 32);
            this.lblSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 2;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            this.lblSourceLabel.Visible = false;
            // 
            // lblSource
            // 
            this.lblSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSource.AutoSize = true;
            this.lblSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSource.Location = new System.Drawing.Point(53, 32);
            this.lblSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(49, 13);
            this.lblSource.TabIndex = 3;
            this.lblSource.Text = "SR5 000";
            this.lblSource.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblSource.ToolTipText = "";
            this.lblSource.Visible = false;
            this.lblSource.Click += new System.EventHandler(this.lblSource_Click);
            // 
            // lblSourceClickReminder
            // 
            this.lblSourceClickReminder.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSourceClickReminder.AutoSize = true;
            this.lblSourceClickReminder.Location = new System.Drawing.Point(108, 32);
            this.lblSourceClickReminder.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSourceClickReminder.Name = "lblSourceClickReminder";
            this.lblSourceClickReminder.Size = new System.Drawing.Size(142, 13);
            this.lblSourceClickReminder.TabIndex = 5;
            this.lblSourceClickReminder.Tag = "Label_MasterIndex_SourceClickReminder";
            this.lblSourceClickReminder.Text = "<- Click to Open Linked PDF";
            this.lblSourceClickReminder.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblSourceClickReminder.Visible = false;
            // 
            // txtNotes
            // 
            this.tlpRight.SetColumnSpan(this.txtNotes, 3);
            this.txtNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtNotes.Location = new System.Drawing.Point(3, 54);
            this.txtNotes.MaxLength = 2147483647;
            this.txtNotes.Multiline = true;
            this.txtNotes.Name = "txtNotes";
            this.txtNotes.ReadOnly = true;
            this.txtNotes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtNotes.Size = new System.Drawing.Size(377, 457);
            this.txtNotes.TabIndex = 6;
            this.txtNotes.Visible = false;
            // 
            // tlpTopLeft
            // 
            this.tlpTopLeft.AutoSize = true;
            this.tlpTopLeft.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpTopLeft.ColumnCount = 2;
            this.tlpTopLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpTopLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTopLeft.Controls.Add(this.lblFile, 0, 0);
            this.tlpTopLeft.Controls.Add(this.cboFile, 1, 0);
            this.tlpTopLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTopLeft.Location = new System.Drawing.Point(0, 0);
            this.tlpTopLeft.Margin = new System.Windows.Forms.Padding(0);
            this.tlpTopLeft.Name = "tlpTopLeft";
            this.tlpTopLeft.RowCount = 1;
            this.tlpTopLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTopLeft.Size = new System.Drawing.Size(383, 27);
            this.tlpTopLeft.TabIndex = 0;
            // 
            // lblFile
            // 
            this.lblFile.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblFile.AutoSize = true;
            this.lblFile.Location = new System.Drawing.Point(3, 7);
            this.lblFile.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblFile.Name = "lblFile";
            this.lblFile.Size = new System.Drawing.Size(52, 13);
            this.lblFile.TabIndex = 2;
            this.lblFile.Tag = "Label_DataFile";
            this.lblFile.Text = "Data File:";
            // 
            // cboFile
            // 
            this.cboFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboFile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFile.FormattingEnabled = true;
            this.cboFile.Location = new System.Drawing.Point(61, 3);
            this.cboFile.Name = "cboFile";
            this.cboFile.Size = new System.Drawing.Size(319, 21);
            this.cboFile.TabIndex = 0;
            this.cboFile.TooltipText = "";
            this.cboFile.SelectedIndexChanged += new System.EventHandler(this.RefreshList);
            // 
            // lstItems
            // 
            this.lstItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstItems.FormattingEnabled = true;
            this.lstItems.Location = new System.Drawing.Point(3, 30);
            this.lstItems.Name = "lstItems";
            this.lstItems.Size = new System.Drawing.Size(377, 481);
            this.lstItems.TabIndex = 2;
            this.lstItems.SelectedIndexChanged += new System.EventHandler(this.lstItems_SelectedIndexChanged);
            // 
            // tlpGameplayOption
            // 
            this.tlpGameplayOption.AutoSize = true;
            this.tlpGameplayOption.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpGameplayOption.ColumnCount = 3;
            this.tlpMain.SetColumnSpan(this.tlpGameplayOption, 2);
            this.tlpGameplayOption.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpGameplayOption.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGameplayOption.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpGameplayOption.Controls.Add(this.cmdEditCharacterSetting, 2, 0);
            this.tlpGameplayOption.Controls.Add(this.lblCharacterSetting, 0, 0);
            this.tlpGameplayOption.Controls.Add(this.cboCharacterSetting, 1, 0);
            this.tlpGameplayOption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpGameplayOption.Location = new System.Drawing.Point(0, 514);
            this.tlpGameplayOption.Margin = new System.Windows.Forms.Padding(0);
            this.tlpGameplayOption.Name = "tlpGameplayOption";
            this.tlpGameplayOption.RowCount = 1;
            this.tlpGameplayOption.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGameplayOption.Size = new System.Drawing.Size(766, 29);
            this.tlpGameplayOption.TabIndex = 3;
            // 
            // cmdEditCharacterSetting
            // 
            this.cmdEditCharacterSetting.AutoSize = true;
            this.cmdEditCharacterSetting.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdEditCharacterSetting.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdEditCharacterSetting.Image = null;
            this.cmdEditCharacterSetting.ImageDpi120 = null;
            this.cmdEditCharacterSetting.ImageDpi144 = null;
            this.cmdEditCharacterSetting.ImageDpi192 = null;
            this.cmdEditCharacterSetting.ImageDpi288 = null;
            this.cmdEditCharacterSetting.ImageDpi384 = null;
            this.cmdEditCharacterSetting.ImageDpi96 = null;
            this.cmdEditCharacterSetting.Location = new System.Drawing.Point(706, 3);
            this.cmdEditCharacterSetting.Name = "cmdEditCharacterSetting";
            this.cmdEditCharacterSetting.Size = new System.Drawing.Size(57, 23);
            this.cmdEditCharacterSetting.TabIndex = 20;
            this.cmdEditCharacterSetting.Text = "Modify...";
            this.cmdEditCharacterSetting.ToolTipText = "";
            this.cmdEditCharacterSetting.UseVisualStyleBackColor = true;
            this.cmdEditCharacterSetting.Click += new System.EventHandler(this.cmdEditCharacterSetting_Click);
            // 
            // lblCharacterSetting
            // 
            this.lblCharacterSetting.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCharacterSetting.AutoSize = true;
            this.lblCharacterSetting.Location = new System.Drawing.Point(3, 8);
            this.lblCharacterSetting.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCharacterSetting.Name = "lblCharacterSetting";
            this.lblCharacterSetting.Size = new System.Drawing.Size(65, 13);
            this.lblCharacterSetting.TabIndex = 19;
            this.lblCharacterSetting.Tag = "Label_SelectBP_UseSetting";
            this.lblCharacterSetting.Text = "Use Setting:";
            // 
            // cboCharacterSetting
            // 
            this.cboCharacterSetting.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboCharacterSetting.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCharacterSetting.FormattingEnabled = true;
            this.cboCharacterSetting.Location = new System.Drawing.Point(74, 4);
            this.cboCharacterSetting.Name = "cboCharacterSetting";
            this.cboCharacterSetting.Size = new System.Drawing.Size(626, 21);
            this.cboCharacterSetting.TabIndex = 21;
            this.cboCharacterSetting.TooltipText = "";
            this.cboCharacterSetting.SelectedIndexChanged += new System.EventHandler(this.cboCharacterSetting_SelectedIndexChanged);
            // 
            // frmMasterIndex
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.tlpMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MasterIndex";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.Tag = "Title_MasterIndex";
            this.Text = "Master Index";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MasterIndex_FormClosing);
            this.Load += new System.EventHandler(this.MasterIndex_Load);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpRight.ResumeLayout(false);
            this.tlpRight.PerformLayout();
            this.tlpTopLeft.ResumeLayout(false);
            this.tlpTopLeft.PerformLayout();
            this.tlpGameplayOption.ResumeLayout(false);
            this.tlpGameplayOption.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BufferedTableLayoutPanel tlpMain;
        private BufferedTableLayoutPanel tlpRight;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearch;
        private BufferedTableLayoutPanel tlpTopLeft;
        private System.Windows.Forms.Label lblFile;
        private ElasticComboBox cboFile;
        private System.Windows.Forms.ListBox lstItems;
        private System.Windows.Forms.Label lblSourceLabel;
        private LabelWithToolTip lblSource;
        private System.Windows.Forms.Label lblSourceClickReminder;
        private System.Windows.Forms.TextBox txtNotes;
        private BufferedTableLayoutPanel tlpGameplayOption;
        private System.Windows.Forms.Label lblCharacterSetting;
        private ButtonWithToolTip cmdEditCharacterSetting;
        private ElasticComboBox cboCharacterSetting;
    }
}
