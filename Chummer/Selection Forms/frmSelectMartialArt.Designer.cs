namespace Chummer
{
    partial class frmSelectMartialArt
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
            this.cmdCancel = new System.Windows.Forms.Button();
            this.lstMartialArts = new System.Windows.Forms.ListBox();
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdOKAdd = new System.Windows.Forms.Button();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblSearchLabel = new System.Windows.Forms.Label();
            this.lblKarmaCostLabel = new System.Windows.Forms.Label();
            this.lblKarmaCost = new System.Windows.Forms.Label();
            this.lblIncludedTechniques = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.tlpButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpRight = new Chummer.BufferedTableLayoutPanel(this.components);
            this.gpbIncludedTechniques = new System.Windows.Forms.GroupBox();
            this.pnlIncludedTechniques = new System.Windows.Forms.Panel();
            this.tlpMain.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            this.tlpRight.SuspendLayout();
            this.gpbIncludedTechniques.SuspendLayout();
            this.pnlIncludedTechniques.SuspendLayout();
            this.SuspendLayout();
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
            this.cmdCancel.TabIndex = 5;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // lstMartialArts
            // 
            this.lstMartialArts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstMartialArts.FormattingEnabled = true;
            this.lstMartialArts.Location = new System.Drawing.Point(3, 3);
            this.lstMartialArts.Name = "lstMartialArts";
            this.tlpMain.SetRowSpan(this.lstMartialArts, 3);
            this.lstMartialArts.Size = new System.Drawing.Size(297, 417);
            this.lstMartialArts.TabIndex = 0;
            this.lstMartialArts.SelectedIndexChanged += new System.EventHandler(this.lstMartialArts_SelectedIndexChanged);
            this.lstMartialArts.DoubleClick += new System.EventHandler(this.lstMartialArts_DoubleClick);
            // 
            // cmdOK
            // 
            this.cmdOK.AutoSize = true;
            this.cmdOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOK.Location = new System.Drawing.Point(159, 3);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(72, 23);
            this.cmdOK.TabIndex = 3;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdOKAdd
            // 
            this.cmdOKAdd.AutoSize = true;
            this.cmdOKAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOKAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOKAdd.Location = new System.Drawing.Point(81, 3);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(72, 23);
            this.cmdOKAdd.TabIndex = 4;
            this.cmdOKAdd.Tag = "String_AddMore";
            this.cmdOKAdd.Text = "&Add && More";
            this.cmdOKAdd.UseVisualStyleBackColor = true;
            this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.lblSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSource.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblSource.Location = new System.Drawing.Point(53, 57);
            this.lblSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 2;
            this.lblSource.Text = "[Source]";
            this.lblSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblSourceLabel.Location = new System.Drawing.Point(3, 57);
            this.lblSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 1;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.Controls.Add(this.lstMartialArts, 0, 0);
            this.tlpMain.Controls.Add(this.tlpButtons, 1, 2);
            this.tlpMain.Controls.Add(this.tlpRight, 1, 0);
            this.tlpMain.Controls.Add(this.gpbIncludedTechniques, 1, 1);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 3;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Size = new System.Drawing.Size(606, 423);
            this.tlpMain.TabIndex = 6;
            // 
            // lblSearchLabel
            // 
            this.lblSearchLabel.AutoSize = true;
            this.lblSearchLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblSearchLabel.Location = new System.Drawing.Point(3, 6);
            this.lblSearchLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSearchLabel.Name = "lblSearchLabel";
            this.lblSearchLabel.Size = new System.Drawing.Size(44, 14);
            this.lblSearchLabel.TabIndex = 70;
            this.lblSearchLabel.Tag = "Label_Search";
            this.lblSearchLabel.Text = "&Search:";
            // 
            // lblKarmaCostLabel
            // 
            this.lblKarmaCostLabel.AutoSize = true;
            this.lblKarmaCostLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblKarmaCostLabel.Location = new System.Drawing.Point(7, 32);
            this.lblKarmaCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaCostLabel.Name = "lblKarmaCostLabel";
            this.lblKarmaCostLabel.Size = new System.Drawing.Size(40, 13);
            this.lblKarmaCostLabel.TabIndex = 3;
            this.lblKarmaCostLabel.Tag = "Label_Karma";
            this.lblKarmaCostLabel.Text = "Karma:";
            // 
            // lblKarmaCost
            // 
            this.lblKarmaCost.AutoSize = true;
            this.lblKarmaCost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblKarmaCost.Location = new System.Drawing.Point(53, 32);
            this.lblKarmaCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaCost.Name = "lblKarmaCost";
            this.lblKarmaCost.Size = new System.Drawing.Size(247, 13);
            this.lblKarmaCost.TabIndex = 4;
            this.lblKarmaCost.Text = "[0]";
            // 
            // lblIncludedTechniques
            // 
            this.lblIncludedTechniques.AutoSize = true;
            this.lblIncludedTechniques.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblIncludedTechniques.Location = new System.Drawing.Point(3, 6);
            this.lblIncludedTechniques.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblIncludedTechniques.Name = "lblIncludedTechniques";
            this.lblIncludedTechniques.Size = new System.Drawing.Size(39, 13);
            this.lblIncludedTechniques.TabIndex = 6;
            this.lblIncludedTechniques.Text = "[None]";
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(53, 3);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(247, 20);
            this.txtSearch.TabIndex = 71;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // tlpButtons
            // 
            this.tlpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpButtons.AutoSize = true;
            this.tlpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpButtons.ColumnCount = 3;
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
            this.tlpButtons.TabIndex = 73;
            // 
            // tlpRight
            // 
            this.tlpRight.AutoSize = true;
            this.tlpRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.ColumnCount = 2;
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRight.Controls.Add(this.lblSearchLabel, 0, 0);
            this.tlpRight.Controls.Add(this.lblSource, 1, 2);
            this.tlpRight.Controls.Add(this.txtSearch, 1, 0);
            this.tlpRight.Controls.Add(this.lblSourceLabel, 0, 2);
            this.tlpRight.Controls.Add(this.lblKarmaCostLabel, 0, 1);
            this.tlpRight.Controls.Add(this.lblKarmaCost, 1, 1);
            this.tlpRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRight.Location = new System.Drawing.Point(303, 0);
            this.tlpRight.Margin = new System.Windows.Forms.Padding(0);
            this.tlpRight.Name = "tlpRight";
            this.tlpRight.RowCount = 3;
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.Size = new System.Drawing.Size(303, 76);
            this.tlpRight.TabIndex = 74;
            // 
            // gpbIncludedTechniques
            // 
            this.gpbIncludedTechniques.Controls.Add(this.pnlIncludedTechniques);
            this.gpbIncludedTechniques.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpbIncludedTechniques.Location = new System.Drawing.Point(306, 79);
            this.gpbIncludedTechniques.Name = "gpbIncludedTechniques";
            this.gpbIncludedTechniques.Size = new System.Drawing.Size(297, 312);
            this.gpbIncludedTechniques.TabIndex = 75;
            this.gpbIncludedTechniques.TabStop = false;
            this.gpbIncludedTechniques.Text = "Enables Learning the Following Techniques:";
            // 
            // pnlIncludedTechniques
            // 
            this.pnlIncludedTechniques.AutoScroll = true;
            this.pnlIncludedTechniques.Controls.Add(this.lblIncludedTechniques);
            this.pnlIncludedTechniques.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlIncludedTechniques.Location = new System.Drawing.Point(3, 16);
            this.pnlIncludedTechniques.Name = "pnlIncludedTechniques";
            this.pnlIncludedTechniques.Padding = new System.Windows.Forms.Padding(3, 6, 13, 6);
            this.pnlIncludedTechniques.Size = new System.Drawing.Size(291, 293);
            this.pnlIncludedTechniques.TabIndex = 0;
            // 
            // frmSelectMartialArt
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.tlpMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectMartialArt";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectMartialArt";
            this.Text = "Select a Martial Art";
            this.Load += new System.EventHandler(this.frmSelectMartialArt_Load);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.tlpRight.ResumeLayout(false);
            this.tlpRight.PerformLayout();
            this.gpbIncludedTechniques.ResumeLayout(false);
            this.pnlIncludedTechniques.ResumeLayout(false);
            this.pnlIncludedTechniques.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.ListBox lstMartialArts;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdOKAdd;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblSourceLabel;
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.Label lblKarmaCostLabel;
        private System.Windows.Forms.Label lblKarmaCost;
        private System.Windows.Forms.Label lblIncludedTechniques;
        private System.Windows.Forms.Label lblSearchLabel;
        private System.Windows.Forms.TextBox txtSearch;
        private BufferedTableLayoutPanel tlpButtons;
        private BufferedTableLayoutPanel tlpRight;
        private System.Windows.Forms.GroupBox gpbIncludedTechniques;
        private System.Windows.Forms.Panel pnlIncludedTechniques;
    }
}
