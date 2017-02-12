using System;

namespace Chummer
{
    partial class frmSelectAIProgram
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
            this.cmdOK = new System.Windows.Forms.Button();
            this.lblRequiresProgramLabel = new System.Windows.Forms.Label();
            this.lblRequiresProgram = new System.Windows.Forms.Label();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOKAdd = new System.Windows.Forms.Button();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearchLabel = new System.Windows.Forms.Label();
            this.tipTooltip = new TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip();
            this.chkLimitList = new System.Windows.Forms.CheckBox();
            this.lblCategory = new System.Windows.Forms.Label();
            this.cboCategory = new System.Windows.Forms.ComboBox();
            this.lstAIPrograms = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // cmdOK
            // 
            this.cmdOK.Location = new System.Drawing.Point(429, 419);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 7;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lblRequiresProgramLabel
            // 
            this.lblRequiresProgramLabel.AutoSize = true;
            this.lblRequiresProgramLabel.Location = new System.Drawing.Point(273, 60);
            this.lblRequiresProgramLabel.Name = "lblRequiresProgramLabel";
            this.lblRequiresProgramLabel.Size = new System.Drawing.Size(52, 13);
            this.lblRequiresProgramLabel.TabIndex = 2;
            this.lblRequiresProgramLabel.Tag = "String_Requires";
            this.lblRequiresProgramLabel.Text = "Requires:";
            // 
            // lblRequiresProgram
            // 
            this.lblRequiresProgram.AutoSize = true;
            this.lblRequiresProgram.Location = new System.Drawing.Point(373, 60);
            this.lblRequiresProgram.Name = "lblRequiresProgram";
            this.lblRequiresProgram.Size = new System.Drawing.Size(39, 13);
            this.lblRequiresProgram.TabIndex = 3;
            this.lblRequiresProgram.Text = "[None]";
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(348, 419);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 9;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOKAdd
            // 
            this.cmdOKAdd.Location = new System.Drawing.Point(429, 390);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(75, 23);
            this.cmdOKAdd.TabIndex = 8;
            this.cmdOKAdd.Tag = "String_AddMore";
            this.cmdOKAdd.Text = "&Add && More";
            this.cmdOKAdd.UseVisualStyleBackColor = true;
            this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.lblSource.Location = new System.Drawing.Point(324, 143);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 5;
            this.lblSource.Text = "[Source]";
            this.lblSource.Click += new System.EventHandler(this.lblSource_Click);
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(273, 143);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 4;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(330, 10);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(174, 20);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
            this.txtSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyUp);
            // 
            // lblSearchLabel
            // 
            this.lblSearchLabel.AutoSize = true;
            this.lblSearchLabel.Location = new System.Drawing.Point(280, 13);
            this.lblSearchLabel.Name = "lblSearchLabel";
            this.lblSearchLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSearchLabel.TabIndex = 0;
            this.lblSearchLabel.Tag = "Label_Search";
            this.lblSearchLabel.Text = "&Search:";
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
            // chkLimitList
            // 
            this.chkLimitList.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkLimitList.Location = new System.Drawing.Point(276, 169);
            this.chkLimitList.Name = "chkLimitList";
            this.chkLimitList.Size = new System.Drawing.Size(228, 62);
            this.chkLimitList.TabIndex = 10;
            this.chkLimitList.Tag = "Checkbox_SelectAIProgram_LimitList";
            this.chkLimitList.Text = "Show only Advanced Programs I can take";
            this.chkLimitList.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkLimitList.UseVisualStyleBackColor = true;
            this.chkLimitList.CheckedChanged += new System.EventHandler(this.chkLimitList_CheckedChanged);
            // 
            // lblCategory
            // 
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(13, 13);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(52, 13);
            this.lblCategory.TabIndex = 35;
            this.lblCategory.Tag = "Label_Category";
            this.lblCategory.Text = "Category:";
            // 
            // cboCategory
            // 
            this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategory.FormattingEnabled = true;
            this.cboCategory.Location = new System.Drawing.Point(71, 10);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(196, 21);
            this.cboCategory.TabIndex = 36;
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // lstAIPrograms
            // 
            this.lstAIPrograms.FormattingEnabled = true;
            this.lstAIPrograms.Location = new System.Drawing.Point(12, 36);
            this.lstAIPrograms.Name = "lstAIPrograms";
            this.lstAIPrograms.Size = new System.Drawing.Size(255, 381);
            this.lstAIPrograms.TabIndex = 37;
            this.lstAIPrograms.SelectedIndexChanged += new System.EventHandler(this.lstAIPrograms_SelectedIndexChanged);
            this.lstAIPrograms.DoubleClick += new EventHandler(this.trePrograms_DoubleClick);
            // 
            // frmSelectAIProgram
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(516, 454);
            this.Controls.Add(this.lstAIPrograms);
            this.Controls.Add(this.lblCategory);
            this.Controls.Add(this.cboCategory);
            this.Controls.Add(this.chkLimitList);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.lblSearchLabel);
            this.Controls.Add(this.lblSource);
            this.Controls.Add(this.lblSourceLabel);
            this.Controls.Add(this.cmdOKAdd);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.lblRequiresProgram);
            this.Controls.Add(this.lblRequiresProgramLabel);
            this.Controls.Add(this.cmdOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectAIProgram";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectAIProgram";
            this.Text = "Select a Program";
            this.Load += new System.EventHandler(this.frmSelectProgram_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Label lblRequiresProgramLabel;
        private System.Windows.Forms.Label lblRequiresProgram;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOKAdd;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblSourceLabel;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearchLabel;
        private TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip tipTooltip;
        private System.Windows.Forms.CheckBox chkLimitList;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.ComboBox cboCategory;
        private System.Windows.Forms.ListBox lstAIPrograms;
    }
}