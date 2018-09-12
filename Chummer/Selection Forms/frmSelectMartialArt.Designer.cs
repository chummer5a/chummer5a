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
            this.cmdCancel = new System.Windows.Forms.Button();
            this.lstMartialArts = new System.Windows.Forms.ListBox();
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdOKAdd = new System.Windows.Forms.Button();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblKarmaCostLabel = new System.Windows.Forms.Label();
            this.lblKarmaCost = new System.Windows.Forms.Label();
            this.lblIncludedTechniquesLabel = new System.Windows.Forms.Label();
            this.lblIncludedTechniques = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearchLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(375, 406);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 5;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // lstMartialArts
            // 
            this.lstMartialArts.FormattingEnabled = true;
            this.lstMartialArts.Location = new System.Drawing.Point(12, 12);
            this.lstMartialArts.Name = "lstMartialArts";
            this.lstMartialArts.Size = new System.Drawing.Size(300, 420);
            this.lstMartialArts.TabIndex = 0;
            this.lstMartialArts.SelectedIndexChanged += new System.EventHandler(this.lstMartialArts_SelectedIndexChanged);
            this.lstMartialArts.DoubleClick += new System.EventHandler(this.lstMartialArts_DoubleClick);
            // 
            // cmdOK
            // 
            this.cmdOK.Location = new System.Drawing.Point(537, 406);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 3;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdOKAdd
            // 
            this.cmdOKAdd.Location = new System.Drawing.Point(456, 406);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(75, 23);
            this.cmdOKAdd.TabIndex = 4;
            this.cmdOKAdd.Tag = "String_AddMore";
            this.cmdOKAdd.Text = "&Add && More";
            this.cmdOKAdd.UseVisualStyleBackColor = true;
            this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
            // 
            // lblSource
            // 
            this.lblSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSource.AutoSize = true;
            this.lblSource.Location = new System.Drawing.Point(66, 57);
            this.lblSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 2;
            this.lblSource.Text = "[Source]";
            this.lblSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(3, 57);
            this.lblSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 1;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 63F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.lblSource, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblSearchLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblSourceLabel, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblKarmaCostLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblKarmaCost, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblIncludedTechniquesLabel, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblIncludedTechniques, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.txtSearch, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(319, 13);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(293, 387);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // lblKarmaCostLabel
            // 
            this.lblKarmaCostLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblKarmaCostLabel.AutoSize = true;
            this.lblKarmaCostLabel.Location = new System.Drawing.Point(3, 32);
            this.lblKarmaCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaCostLabel.Name = "lblKarmaCostLabel";
            this.lblKarmaCostLabel.Size = new System.Drawing.Size(40, 13);
            this.lblKarmaCostLabel.TabIndex = 3;
            this.lblKarmaCostLabel.Tag = "Label_Karma";
            this.lblKarmaCostLabel.Text = "Karma:";
            // 
            // lblKarmaCost
            // 
            this.lblKarmaCost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblKarmaCost.AutoSize = true;
            this.lblKarmaCost.Location = new System.Drawing.Point(66, 32);
            this.lblKarmaCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaCost.Name = "lblKarmaCost";
            this.lblKarmaCost.Size = new System.Drawing.Size(19, 13);
            this.lblKarmaCost.TabIndex = 4;
            this.lblKarmaCost.Text = "[0]";
            // 
            // lblIncludedTechniquesLabel
            // 
            this.lblIncludedTechniquesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblIncludedTechniquesLabel.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblIncludedTechniquesLabel, 2);
            this.lblIncludedTechniquesLabel.Location = new System.Drawing.Point(3, 82);
            this.lblIncludedTechniquesLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblIncludedTechniquesLabel.Name = "lblIncludedTechniquesLabel";
            this.lblIncludedTechniquesLabel.Size = new System.Drawing.Size(216, 13);
            this.lblIncludedTechniquesLabel.TabIndex = 5;
            this.lblIncludedTechniquesLabel.Tag = "Label_SelectMartialArt_IncludedTechniques";
            this.lblIncludedTechniquesLabel.Text = "Enables Learning the Following Techniques:";
            // 
            // lblIncludedTechniques
            // 
            this.lblIncludedTechniques.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblIncludedTechniques.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblIncludedTechniques, 2);
            this.lblIncludedTechniques.Location = new System.Drawing.Point(3, 107);
            this.lblIncludedTechniques.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblIncludedTechniques.Name = "lblIncludedTechniques";
            this.lblIncludedTechniques.Size = new System.Drawing.Size(39, 274);
            this.lblIncludedTechniques.TabIndex = 6;
            this.lblIncludedTechniques.Text = "[None]";
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(66, 3);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(224, 20);
            this.txtSearch.TabIndex = 71;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // lblSearchLabel
            // 
            this.lblSearchLabel.AutoSize = true;
            this.lblSearchLabel.Location = new System.Drawing.Point(3, 6);
            this.lblSearchLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSearchLabel.Name = "lblSearchLabel";
            this.lblSearchLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSearchLabel.TabIndex = 70;
            this.lblSearchLabel.Tag = "Label_Search";
            this.lblSearchLabel.Text = "&Search:";
            // 
            // frmSelectMartialArt
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.cmdOKAdd);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.lstMartialArts);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectMartialArt";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectMartialArt";
            this.Text = "Select a Martial Art";
            this.Load += new System.EventHandler(this.frmSelectMartialArt_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.ListBox lstMartialArts;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdOKAdd;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblSourceLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblKarmaCostLabel;
        private System.Windows.Forms.Label lblKarmaCost;
        private System.Windows.Forms.Label lblIncludedTechniquesLabel;
        private System.Windows.Forms.Label lblIncludedTechniques;
        private System.Windows.Forms.Label lblSearchLabel;
        private System.Windows.Forms.TextBox txtSearch;
    }
}
