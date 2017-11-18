namespace Chummer
{
    partial class frmSelectArt
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
            this.chkLimitList = new System.Windows.Forms.CheckBox();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.lstArt = new System.Windows.Forms.ListBox();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.tipTooltip = new TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip();
            this.SuspendLayout();
            // 
            // chkLimitList
            // 
            this.chkLimitList.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkLimitList.Checked = true;
            this.chkLimitList.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLimitList.Location = new System.Drawing.Point(258, 44);
            this.chkLimitList.Name = "chkLimitList";
            this.chkLimitList.Size = new System.Drawing.Size(186, 52);
            this.chkLimitList.TabIndex = 10;
            this.chkLimitList.Tag = "Checkbox_SelectArt_LimitList";
            this.chkLimitList.Text = "Show only Arts I can take";
            this.chkLimitList.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkLimitList.UseVisualStyleBackColor = true;
            this.chkLimitList.CheckedChanged += new System.EventHandler(this.chkLimitList_CheckedChanged);
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.lblSource.Location = new System.Drawing.Point(309, 12);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 9;
            this.lblSource.Text = "[Source]";
            this.lblSource.Click += new System.EventHandler(this.lblSource_Click);
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(258, 12);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 8;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            // 
            // lstArt
            // 
            this.lstArt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstArt.FormattingEnabled = true;
            this.lstArt.Location = new System.Drawing.Point(12, 12);
            this.lstArt.Name = "lstArt";
            this.lstArt.Size = new System.Drawing.Size(240, 238);
            this.lstArt.TabIndex = 7;
            this.lstArt.SelectedIndexChanged += new System.EventHandler(this.lstArt_SelectedIndexChanged);
            this.lstArt.DoubleClick += new System.EventHandler(this.lstArt_DoubleClick);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(312, 225);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 12;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.Location = new System.Drawing.Point(393, 225);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 11;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
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
            // frmSelectArt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(480, 258);
            this.Controls.Add(this.chkLimitList);
            this.Controls.Add(this.lblSource);
            this.Controls.Add(this.lblSourceLabel);
            this.Controls.Add(this.lstArt);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "frmSelectArt";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectArt";
            this.Text = "Select an Art";
            this.Load += new System.EventHandler(this.frmSelectArt_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.CheckBox chkLimitList;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblSourceLabel;
        private System.Windows.Forms.ListBox lstArt;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip tipTooltip;
    }
}