namespace Chummer
{
    partial class frmSelectComplexForm
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
                tipTooltip?.Dispose();
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
            this.lstComplexForms = new System.Windows.Forms.ListBox();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lblTargetLabel = new System.Windows.Forms.Label();
            this.lblTarget = new System.Windows.Forms.Label();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOKAdd = new System.Windows.Forms.Button();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearchLabel = new System.Windows.Forms.Label();
            this.tipTooltip = new TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip();
            this.lblDuration = new System.Windows.Forms.Label();
            this.lblDurationLabel = new System.Windows.Forms.Label();
            this.lblFV = new System.Windows.Forms.Label();
            this.lblFVLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lstComplexForms
            // 
            this.lstComplexForms.Location = new System.Drawing.Point(12, 12);
            this.lstComplexForms.Name = "lstComplexForms";
            this.lstComplexForms.Size = new System.Drawing.Size(241, 430);
            this.lstComplexForms.TabIndex = 6;
            this.lstComplexForms.SelectedIndexChanged += new System.EventHandler(this.lstComplexForms_SelectedIndexChanged);
            this.lstComplexForms.DoubleClick += new System.EventHandler(this.lstComplexForms_DoubleClick);
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
            // lblTargetLabel
            // 
            this.lblTargetLabel.AutoSize = true;
            this.lblTargetLabel.Location = new System.Drawing.Point(259, 60);
            this.lblTargetLabel.Name = "lblTargetLabel";
            this.lblTargetLabel.Size = new System.Drawing.Size(41, 13);
            this.lblTargetLabel.TabIndex = 2;
            this.lblTargetLabel.Tag = "Label_SelectComplexForm_Target";
            this.lblTargetLabel.Text = "Target:";
            // 
            // lblTarget
            // 
            this.lblTarget.AutoSize = true;
            this.lblTarget.Location = new System.Drawing.Point(373, 60);
            this.lblTarget.Name = "lblTarget";
            this.lblTarget.Size = new System.Drawing.Size(39, 13);
            this.lblTarget.TabIndex = 3;
            this.lblTarget.Text = "[None]";
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
            this.lblSource.Location = new System.Drawing.Point(310, 143);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 5;
            this.lblSource.Text = "[Source]";
            this.lblSource.Click += new System.EventHandler(CommonFunctions.OpenPDFFromControl);
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(259, 143);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 4;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(330, 12);
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
            this.lblSearchLabel.Location = new System.Drawing.Point(280, 15);
            this.lblSearchLabel.Name = "lblSearchLabel";
            this.lblSearchLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSearchLabel.TabIndex = 0;
            this.lblSearchLabel.Tag = "Label_Search";
            this.lblSearchLabel.Text = "&Search:";
            // 
            // tipTooltip
            // 
            this.tipTooltip.AllowLinksHandling = true;
            this.tipTooltip.AutoPopDelay = 10000;
            this.tipTooltip.BaseStylesheet = null;
            this.tipTooltip.InitialDelay = 250;
            this.tipTooltip.IsBalloon = true;
            this.tipTooltip.MaximumSize = new System.Drawing.Size(0, 0);
            this.tipTooltip.OwnerDraw = true;
            this.tipTooltip.ReshowDelay = 100;
            this.tipTooltip.TooltipCssClass = "htmltooltip";
            this.tipTooltip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.tipTooltip.ToolTipTitle = "Chummer Help";
            // 
            // lblDuration
            // 
            this.lblDuration.AutoSize = true;
            this.lblDuration.Location = new System.Drawing.Point(373, 82);
            this.lblDuration.Name = "lblDuration";
            this.lblDuration.Size = new System.Drawing.Size(39, 13);
            this.lblDuration.TabIndex = 11;
            this.lblDuration.Text = "[None]";
            // 
            // lblDurationLabel
            // 
            this.lblDurationLabel.AutoSize = true;
            this.lblDurationLabel.Location = new System.Drawing.Point(259, 82);
            this.lblDurationLabel.Name = "lblDurationLabel";
            this.lblDurationLabel.Size = new System.Drawing.Size(50, 13);
            this.lblDurationLabel.TabIndex = 10;
            this.lblDurationLabel.Tag = "Label_SelectComplexForm_Duration";
            this.lblDurationLabel.Text = "Duration:";
            // 
            // lblFV
            // 
            this.lblFV.AutoSize = true;
            this.lblFV.Location = new System.Drawing.Point(373, 105);
            this.lblFV.Name = "lblFV";
            this.lblFV.Size = new System.Drawing.Size(39, 13);
            this.lblFV.TabIndex = 13;
            this.lblFV.Text = "[None]";
            // 
            // lblFVLabel
            // 
            this.lblFVLabel.AutoSize = true;
            this.lblFVLabel.Location = new System.Drawing.Point(259, 105);
            this.lblFVLabel.Name = "lblFVLabel";
            this.lblFVLabel.Size = new System.Drawing.Size(23, 13);
            this.lblFVLabel.TabIndex = 12;
            this.lblFVLabel.Tag = "Label_SelectComplexForm_FV";
            this.lblFVLabel.Text = "FV:";
            // 
            // frmSelectComplexForm
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(516, 454);
            this.Controls.Add(this.lblFV);
            this.Controls.Add(this.lblFVLabel);
            this.Controls.Add(this.lblDuration);
            this.Controls.Add(this.lblDurationLabel);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.lblSearchLabel);
            this.Controls.Add(this.lblSource);
            this.Controls.Add(this.lblSourceLabel);
            this.Controls.Add(this.cmdOKAdd);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.lblTarget);
            this.Controls.Add(this.lblTargetLabel);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.lstComplexForms);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectComplexForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectComplexForm";
            this.Text = "Select a Complex Form";
            this.Load += new System.EventHandler(this.frmSelectComplexForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstComplexForms;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Label lblTargetLabel;
        private System.Windows.Forms.Label lblTarget;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOKAdd;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblSourceLabel;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearchLabel;
        private TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip tipTooltip;
        private System.Windows.Forms.Label lblDuration;
        private System.Windows.Forms.Label lblDurationLabel;
        private System.Windows.Forms.Label lblFV;
        private System.Windows.Forms.Label lblFVLabel;
    }
}
