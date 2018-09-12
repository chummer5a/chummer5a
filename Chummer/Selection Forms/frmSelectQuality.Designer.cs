namespace Chummer
{
    partial class frmSelectQuality
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
            this.lstQualities = new System.Windows.Forms.ListBox();
            this.lblCategory = new System.Windows.Forms.Label();
            this.cboCategory = new System.Windows.Forms.ComboBox();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.cmdOKAdd = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.chkLimitList = new System.Windows.Forms.CheckBox();
            this.lblBP = new System.Windows.Forms.Label();
            this.lblBPLabel = new System.Windows.Forms.Label();
            this.chkFree = new System.Windows.Forms.CheckBox();
            this.chkMetagenetic = new System.Windows.Forms.CheckBox();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearchLabel = new System.Windows.Forms.Label();
            this.chkNotMetagenetic = new System.Windows.Forms.CheckBox();
            this.nudMinimumBP = new System.Windows.Forms.NumericUpDown();
            this.nudValueBP = new System.Windows.Forms.NumericUpDown();
            this.nudMaximumBP = new System.Windows.Forms.NumericUpDown();
            this.lblMinimumBP = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinimumBP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudValueBP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaximumBP)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstQualities
            // 
            this.lstQualities.FormattingEnabled = true;
            this.lstQualities.Location = new System.Drawing.Point(12, 36);
            this.lstQualities.Name = "lstQualities";
            this.lstQualities.Size = new System.Drawing.Size(300, 394);
            this.lstQualities.TabIndex = 9;
            this.lstQualities.SelectedIndexChanged += new System.EventHandler(this.lstQualities_SelectedIndexChanged);
            this.lstQualities.DoubleClick += new System.EventHandler(this.lstQualities_DoubleClick);
            // 
            // lblCategory
            // 
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(12, 9);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(52, 13);
            this.lblCategory.TabIndex = 10;
            this.lblCategory.Tag = "Label_Category";
            this.lblCategory.Text = "Category:";
            // 
            // cboCategory
            // 
            this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategory.FormattingEnabled = true;
            this.cboCategory.Location = new System.Drawing.Point(70, 6);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(242, 21);
            this.cboCategory.TabIndex = 11;
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.lblSource.Location = new System.Drawing.Point(100, 31);
            this.lblSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 5;
            this.lblSource.Text = "[Source]";
            this.lblSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(3, 31);
            this.lblSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 4;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            // 
            // cmdOKAdd
            // 
            this.cmdOKAdd.Location = new System.Drawing.Point(456, 406);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(75, 23);
            this.cmdOKAdd.TabIndex = 13;
            this.cmdOKAdd.Tag = "String_AddMore";
            this.cmdOKAdd.Text = "&Add && More";
            this.cmdOKAdd.UseVisualStyleBackColor = true;
            this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(375, 406);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 14;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.Location = new System.Drawing.Point(537, 406);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 12;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // chkLimitList
            // 
            this.chkLimitList.AutoSize = true;
            this.chkLimitList.Checked = true;
            this.chkLimitList.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tableLayoutPanel1.SetColumnSpan(this.chkLimitList, 3);
            this.chkLimitList.Location = new System.Drawing.Point(3, 130);
            this.chkLimitList.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkLimitList.Name = "chkLimitList";
            this.chkLimitList.Size = new System.Drawing.Size(169, 17);
            this.chkLimitList.TabIndex = 6;
            this.chkLimitList.Tag = "Checkbox_SelectQuality_LimitList";
            this.chkLimitList.Text = "Show only Qualities I can take";
            this.chkLimitList.UseVisualStyleBackColor = true;
            this.chkLimitList.CheckedChanged += new System.EventHandler(this.chkLimitList_CheckedChanged);
            // 
            // lblBP
            // 
            this.lblBP.AutoSize = true;
            this.lblBP.Location = new System.Drawing.Point(100, 6);
            this.lblBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBP.Name = "lblBP";
            this.lblBP.Size = new System.Drawing.Size(27, 13);
            this.lblBP.TabIndex = 3;
            this.lblBP.Text = "[BP]";
            // 
            // lblBPLabel
            // 
            this.lblBPLabel.AutoSize = true;
            this.lblBPLabel.Location = new System.Drawing.Point(3, 6);
            this.lblBPLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBPLabel.Name = "lblBPLabel";
            this.lblBPLabel.Size = new System.Drawing.Size(40, 13);
            this.lblBPLabel.TabIndex = 2;
            this.lblBPLabel.Tag = "Label_Karma";
            this.lblBPLabel.Text = "Karma:";
            // 
            // chkFree
            // 
            this.chkFree.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkFree, 3);
            this.chkFree.Location = new System.Drawing.Point(3, 155);
            this.chkFree.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkFree.Name = "chkFree";
            this.chkFree.Size = new System.Drawing.Size(50, 17);
            this.chkFree.TabIndex = 8;
            this.chkFree.Tag = "Checkbox_Free";
            this.chkFree.Text = "Free!";
            this.chkFree.UseVisualStyleBackColor = true;
            this.chkFree.CheckedChanged += new System.EventHandler(this.chkFree_CheckedChanged);
            // 
            // chkMetagenetic
            // 
            this.chkMetagenetic.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkMetagenetic, 3);
            this.chkMetagenetic.Location = new System.Drawing.Point(3, 180);
            this.chkMetagenetic.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkMetagenetic.Name = "chkMetagenetic";
            this.chkMetagenetic.Size = new System.Drawing.Size(180, 17);
            this.chkMetagenetic.TabIndex = 7;
            this.chkMetagenetic.Tag = "Checkbox_SelectQuality_Metagenetic";
            this.chkMetagenetic.Text = "Show only Metagenetic Qualities";
            this.chkMetagenetic.UseVisualStyleBackColor = true;
            this.chkMetagenetic.CheckedChanged += new System.EventHandler(this.chkMetagenetic_CheckedChanged);
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(396, 6);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(216, 20);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
            this.txtSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyUp);
            // 
            // lblSearchLabel
            // 
            this.lblSearchLabel.AutoSize = true;
            this.lblSearchLabel.Location = new System.Drawing.Point(346, 9);
            this.lblSearchLabel.Name = "lblSearchLabel";
            this.lblSearchLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSearchLabel.TabIndex = 0;
            this.lblSearchLabel.Tag = "Label_Search";
            this.lblSearchLabel.Text = "&Search:";
            // 
            // chkNotMetagenetic
            // 
            this.chkNotMetagenetic.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkNotMetagenetic, 3);
            this.chkNotMetagenetic.Location = new System.Drawing.Point(3, 205);
            this.chkNotMetagenetic.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkNotMetagenetic.Name = "chkNotMetagenetic";
            this.chkNotMetagenetic.Size = new System.Drawing.Size(184, 17);
            this.chkNotMetagenetic.TabIndex = 15;
            this.chkNotMetagenetic.Tag = "Checkbox_SelectQuality_Not_Metagenetic";
            this.chkNotMetagenetic.Text = "Don\'t show Metagenetic Qualities";
            this.chkNotMetagenetic.UseVisualStyleBackColor = true;
            this.chkNotMetagenetic.CheckedChanged += new System.EventHandler(this.chkNotMetagenetic_CheckedChanged);
            // 
            // nudMinimumBP
            // 
            this.nudMinimumBP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudMinimumBP.Location = new System.Drawing.Point(3, 103);
            this.nudMinimumBP.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.nudMinimumBP.Name = "nudMinimumBP";
            this.nudMinimumBP.Size = new System.Drawing.Size(91, 20);
            this.nudMinimumBP.TabIndex = 16;
            this.nudMinimumBP.TextChanged += new System.EventHandler(this.KarmaFilter);
            this.nudMinimumBP.ValueChanged += new System.EventHandler(this.KarmaFilter);
            // 
            // nudValueBP
            // 
            this.nudValueBP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudValueBP.Location = new System.Drawing.Point(100, 103);
            this.nudValueBP.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.nudValueBP.Name = "nudValueBP";
            this.nudValueBP.Size = new System.Drawing.Size(92, 20);
            this.nudValueBP.TabIndex = 17;
            this.nudValueBP.TextChanged += new System.EventHandler(this.KarmaFilter);
            this.nudValueBP.ValueChanged += new System.EventHandler(this.KarmaFilter);
            // 
            // nudMaximumBP
            // 
            this.nudMaximumBP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudMaximumBP.Location = new System.Drawing.Point(198, 103);
            this.nudMaximumBP.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.nudMaximumBP.Name = "nudMaximumBP";
            this.nudMaximumBP.Size = new System.Drawing.Size(93, 20);
            this.nudMaximumBP.TabIndex = 18;
            this.nudMaximumBP.TextChanged += new System.EventHandler(this.KarmaFilter);
            this.nudMaximumBP.ValueChanged += new System.EventHandler(this.KarmaFilter);
            // 
            // lblMinimumBP
            // 
            this.lblMinimumBP.AutoSize = true;
            this.lblMinimumBP.Location = new System.Drawing.Point(3, 81);
            this.lblMinimumBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMinimumBP.Name = "lblMinimumBP";
            this.lblMinimumBP.Size = new System.Drawing.Size(51, 13);
            this.lblMinimumBP.TabIndex = 19;
            this.lblMinimumBP.Tag = "Label_CreateImprovementMinimum";
            this.lblMinimumBP.Text = "Minimum:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(100, 81);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 13);
            this.label2.TabIndex = 20;
            this.label2.Tag = "Label_CreateImprovementExactly";
            this.label2.Text = "Exactly:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(198, 81);
            this.label3.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 13);
            this.label3.TabIndex = 21;
            this.label3.Tag = "Label_CreateImprovementMaximum";
            this.label3.Text = "Maximum:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 56);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 22;
            this.label1.Tag = "Label_FilterByKarma";
            this.label1.Text = "Filter by Karma:";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel1.Controls.Add(this.chkNotMetagenetic, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.nudMaximumBP, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.label3, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.nudValueBP, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.chkMetagenetic, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.chkFree, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.nudMinimumBP, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.chkLimitList, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.label2, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblBPLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblMinimumBP, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblBP, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblSourceLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblSource, 1, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(318, 36);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 9;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(294, 364);
            this.tableLayoutPanel1.TabIndex = 23;
            // 
            // frmSelectQuality
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.lblSearchLabel);
            this.Controls.Add(this.cmdOKAdd);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.lblCategory);
            this.Controls.Add(this.cboCategory);
            this.Controls.Add(this.lstQualities);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectQuality";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectQuality";
            this.Text = "Select a Quality";
            this.Load += new System.EventHandler(this.frmSelectQuality_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudMinimumBP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudValueBP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaximumBP)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstQualities;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.ComboBox cboCategory;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblSourceLabel;
        private System.Windows.Forms.Button cmdOKAdd;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.CheckBox chkLimitList;
        private System.Windows.Forms.Label lblBP;
        private System.Windows.Forms.Label lblBPLabel;
        private System.Windows.Forms.CheckBox chkFree;
        private System.Windows.Forms.CheckBox chkMetagenetic;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearchLabel;
        private System.Windows.Forms.CheckBox chkNotMetagenetic;
        private System.Windows.Forms.NumericUpDown nudMinimumBP;
        private System.Windows.Forms.NumericUpDown nudValueBP;
        private System.Windows.Forms.NumericUpDown nudMaximumBP;
        private System.Windows.Forms.Label lblMinimumBP;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
