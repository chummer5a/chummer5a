namespace Chummer
{
    partial class frmSelectMartialArtTechnique
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
            this.cmdOKAdd = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.lstTechniques = new System.Windows.Forms.ListBox();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.tipTooltip = new TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip();
            this.SuspendLayout();
            // 
            // cmdOKAdd
            // 
            this.cmdOKAdd.Location = new System.Drawing.Point(570, 81);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(75, 23);
            this.cmdOKAdd.TabIndex = 2;
            this.cmdOKAdd.Tag = "String_AddMore";
            this.cmdOKAdd.Text = "&Add && More";
            this.cmdOKAdd.UseVisualStyleBackColor = true;
            this.cmdOKAdd.Visible = false;
            this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(489, 110);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 3;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // lstTechniques
            // 
            this.lstTechniques.FormattingEnabled = true;
            this.lstTechniques.Location = new System.Drawing.Point(12, 12);
            this.lstTechniques.Name = "lstTechniques";
            this.lstTechniques.Size = new System.Drawing.Size(471, 121);
            this.lstTechniques.TabIndex = 0;
            this.lstTechniques.SelectedIndexChanged += new System.EventHandler(this.lstTechniques_SelectedIndexChanged);
            this.lstTechniques.DoubleClick += new System.EventHandler(this.lstTechniques_DoubleClick);
            // 
            // cmdOK
            // 
            this.cmdOK.Location = new System.Drawing.Point(570, 110);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 1;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.lblSource.Location = new System.Drawing.Point(541, 12);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 5;
            this.lblSource.Text = "[Source]";
            this.lblSource.Click += new System.EventHandler(CommonFunctions.OpenPDFFromControl);
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(490, 12);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 4;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
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
            // frmSelectMartialArtTechnique
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(656, 146);
            this.Controls.Add(this.lblSource);
            this.Controls.Add(this.lblSourceLabel);
            this.Controls.Add(this.cmdOKAdd);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.lstTechniques);
            this.Controls.Add(this.cmdOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectMartialArtTechnique";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Tag = "Title_SelectMartialArtTechnique";
            this.Text = "Select a Technique";
            this.Load += new System.EventHandler(this.frmSelectMartialArtTechnique_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdOKAdd;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.ListBox lstTechniques;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblSourceLabel;
        private TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip tipTooltip;
    }
}
