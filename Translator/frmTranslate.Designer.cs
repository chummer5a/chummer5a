namespace Translator
{
    partial class frmTranslate
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTranslate));
            this.cboFile = new System.Windows.Forms.ComboBox();
            this.dgvSection = new System.Windows.Forms.DataGridView();
            this.dgvTranslate = new System.Windows.Forms.DataGridView();
            this.key = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.english = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.text = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Translated = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.chkOnlyTranslation = new System.Windows.Forms.CheckBox();
            this.cboSection = new System.Windows.Forms.ComboBox();
            this.sectionEnglish = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sectionText = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sectionBook = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sectionPage = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sectionTranslated = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.pbTranslateProgressBar = new System.Windows.Forms.ProgressBar();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.pnlMain = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSection)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTranslate)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // cboFile
            // 
            this.cboFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboFile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFile.FormattingEnabled = true;
            this.cboFile.Location = new System.Drawing.Point(3, 4);
            this.cboFile.Name = "cboFile";
            this.cboFile.Size = new System.Drawing.Size(322, 21);
            this.cboFile.TabIndex = 8;
            this.cboFile.SelectedIndexChanged += new System.EventHandler(this.cboFile_SelectedIndexChanged);
            // 
            // dgvSection
            // 
            this.dgvSection.AllowUserToAddRows = false;
            this.dgvSection.AllowUserToDeleteRows = false;
            this.dgvSection.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvSection.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvSection.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSection.Location = new System.Drawing.Point(3, 3);
            this.dgvSection.MultiSelect = false;
            this.dgvSection.Name = "dgvSection";
            this.dgvSection.RowHeadersVisible = false;
            this.dgvSection.Size = new System.Drawing.Size(1240, 599);
            this.dgvSection.TabIndex = 13;
            this.dgvSection.Visible = false;
            this.dgvSection.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvSection_CellMouseUp);
            this.dgvSection.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvSection_CellValueChanged);
            this.dgvSection.Sorted += new System.EventHandler(this.dgvSection_Sorted);
            this.dgvSection.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmTranslate_KeyDown);
            // 
            // dgvTranslate
            // 
            this.dgvTranslate.AllowUserToAddRows = false;
            this.dgvTranslate.AllowUserToDeleteRows = false;
            this.dgvTranslate.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvTranslate.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvTranslate.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTranslate.Location = new System.Drawing.Point(3, 3);
            this.dgvTranslate.MultiSelect = false;
            this.dgvTranslate.Name = "dgvTranslate";
            this.dgvTranslate.RowHeadersVisible = false;
            this.dgvTranslate.Size = new System.Drawing.Size(1240, 599);
            this.dgvTranslate.TabIndex = 14;
            this.dgvTranslate.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvTranslate_CellValueChanged);
            this.dgvTranslate.Sorted += new System.EventHandler(this.dgvTranslate_Sorted);
            this.dgvTranslate.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmTranslate_KeyDown);
            // 
            // key
            // 
            this.key.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.key.DataPropertyName = "key";
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.key.DefaultCellStyle = dataGridViewCellStyle1;
            this.key.HeaderText = "Key";
            this.key.Name = "key";
            this.key.ReadOnly = true;
            this.key.Width = 300;
            // 
            // english
            // 
            this.english.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.english.DataPropertyName = "english";
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.english.DefaultCellStyle = dataGridViewCellStyle2;
            this.english.HeaderText = "English";
            this.english.Name = "english";
            this.english.ReadOnly = true;
            this.english.Width = 400;
            // 
            // text
            // 
            this.text.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.text.DataPropertyName = "text";
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.text.DefaultCellStyle = dataGridViewCellStyle3;
            this.text.HeaderText = "Text";
            this.text.Name = "text";
            this.text.Width = 400;
            // 
            // Translated
            // 
            this.Translated.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Translated.DataPropertyName = "translated";
            this.Translated.HeaderText = "Translated";
            this.Translated.Name = "Translated";
            this.Translated.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.Translated.Width = 70;
            // 
            // chkOnlyTranslation
            // 
            this.chkOnlyTranslation.AutoSize = true;
            this.chkOnlyTranslation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkOnlyTranslation.Location = new System.Drawing.Point(659, 3);
            this.chkOnlyTranslation.Name = "chkOnlyTranslation";
            this.chkOnlyTranslation.Size = new System.Drawing.Size(199, 23);
            this.chkOnlyTranslation.TabIndex = 10;
            this.chkOnlyTranslation.Text = "Only Show Text Needing Translation";
            this.chkOnlyTranslation.UseVisualStyleBackColor = true;
            this.chkOnlyTranslation.CheckedChanged += new System.EventHandler(this.chkOnlyTranslation_CheckedChanged);
            // 
            // cboSection
            // 
            this.cboSection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSection.FormattingEnabled = true;
            this.cboSection.Location = new System.Drawing.Point(331, 4);
            this.cboSection.Name = "cboSection";
            this.cboSection.Size = new System.Drawing.Size(322, 21);
            this.cboSection.TabIndex = 9;
            this.cboSection.Visible = false;
            this.cboSection.SelectedIndexChanged += new System.EventHandler(this.cboSection_SelectedIndexChanged);
            // 
            // sectionEnglish
            // 
            this.sectionEnglish.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.sectionEnglish.DataPropertyName = "english";
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.sectionEnglish.DefaultCellStyle = dataGridViewCellStyle4;
            this.sectionEnglish.HeaderText = "English";
            this.sectionEnglish.Name = "sectionEnglish";
            this.sectionEnglish.ReadOnly = true;
            this.sectionEnglish.Width = 480;
            // 
            // sectionText
            // 
            this.sectionText.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.sectionText.DataPropertyName = "text";
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.sectionText.DefaultCellStyle = dataGridViewCellStyle5;
            this.sectionText.HeaderText = "Text";
            this.sectionText.Name = "sectionText";
            this.sectionText.Width = 480;
            // 
            // sectionBook
            // 
            this.sectionBook.DataPropertyName = "book";
            this.sectionBook.HeaderText = "Book";
            this.sectionBook.Name = "sectionBook";
            this.sectionBook.ReadOnly = true;
            this.sectionBook.Width = 70;
            // 
            // sectionPage
            // 
            this.sectionPage.DataPropertyName = "page";
            this.sectionPage.HeaderText = "Page";
            this.sectionPage.Name = "sectionPage";
            this.sectionPage.Width = 70;
            // 
            // sectionTranslated
            // 
            this.sectionTranslated.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.sectionTranslated.DataPropertyName = "translated";
            this.sectionTranslated.HeaderText = "Translated";
            this.sectionTranslated.Name = "sectionTranslated";
            this.sectionTranslated.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.sectionTranslated.Width = 70;
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(864, 4);
            this.txtSearch.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(322, 20);
            this.txtSearch.TabIndex = 11;
            this.txtSearch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSearch_KeyPressed);
            // 
            // btnSearch
            // 
            this.btnSearch.AutoSize = true;
            this.btnSearch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSearch.Location = new System.Drawing.Point(1192, 3);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(51, 23);
            this.btnSearch.TabIndex = 12;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // pbTranslateProgressBar
            // 
            this.tlpMain.SetColumnSpan(this.pbTranslateProgressBar, 5);
            this.pbTranslateProgressBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbTranslateProgressBar.Location = new System.Drawing.Point(3, 637);
            this.pbTranslateProgressBar.Name = "pbTranslateProgressBar";
            this.pbTranslateProgressBar.Size = new System.Drawing.Size(1240, 23);
            this.pbTranslateProgressBar.Step = 1;
            this.pbTranslateProgressBar.TabIndex = 148;
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 5;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Controls.Add(this.txtSearch, 3, 0);
            this.tlpMain.Controls.Add(this.btnSearch, 4, 0);
            this.tlpMain.Controls.Add(this.pbTranslateProgressBar, 0, 2);
            this.tlpMain.Controls.Add(this.cboFile, 0, 0);
            this.tlpMain.Controls.Add(this.chkOnlyTranslation, 2, 0);
            this.tlpMain.Controls.Add(this.cboSection, 1, 0);
            this.tlpMain.Controls.Add(this.pnlMain, 0, 1);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 3;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(1246, 663);
            this.tlpMain.TabIndex = 149;
            // 
            // pnlMain
            // 
            this.pnlMain.AutoSize = true;
            this.pnlMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.SetColumnSpan(this.pnlMain, 5);
            this.pnlMain.Controls.Add(this.dgvTranslate);
            this.pnlMain.Controls.Add(this.dgvSection);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 29);
            this.pnlMain.Margin = new System.Windows.Forms.Padding(0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(1246, 605);
            this.pnlMain.TabIndex = 150;
            // 
            // frmTranslate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 681);
            this.Controls.Add(this.tlpMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmTranslate";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.Text = "frmTranslate";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmTranslate_FormClosing);
            this.Load += new System.EventHandler(this.frmTranslate_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmTranslate_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSection)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTranslate)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.pnlMain.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cboFile;
        private System.Windows.Forms.DataGridView dgvSection;
        private System.Windows.Forms.DataGridView dgvTranslate;
        private System.Windows.Forms.DataGridViewTextBoxColumn key;
        private System.Windows.Forms.DataGridViewTextBoxColumn english;
        private System.Windows.Forms.DataGridViewTextBoxColumn text;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Translated;
        private System.Windows.Forms.CheckBox chkOnlyTranslation;
        private System.Windows.Forms.ComboBox cboSection;
        private System.Windows.Forms.DataGridViewTextBoxColumn sectionEnglish;
        private System.Windows.Forms.DataGridViewTextBoxColumn sectionText;
        private System.Windows.Forms.DataGridViewTextBoxColumn sectionBook;
        private System.Windows.Forms.DataGridViewTextBoxColumn sectionPage;
        private System.Windows.Forms.DataGridViewCheckBoxColumn sectionTranslated;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.ProgressBar pbTranslateProgressBar;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.Panel pnlMain;
    }
}
