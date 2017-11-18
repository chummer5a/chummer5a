namespace Chummer
{
    partial class frmOmaeUploadSheet
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmOmaeUploadSheet));
            this.cmdBrowse = new System.Windows.Forms.Button();
            this.cmdUpload = new System.Windows.Forms.Button();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.lblDescriptionLabel = new System.Windows.Forms.Label();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.lblFilePathLabel = new System.Windows.Forms.Label();
            this.lblNameLabel = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // cmdBrowse
            // 
            this.cmdBrowse.Location = new System.Drawing.Point(471, 4);
            this.cmdBrowse.Name = "cmdBrowse";
            this.cmdBrowse.Size = new System.Drawing.Size(25, 23);
            this.cmdBrowse.TabIndex = 2;
            this.cmdBrowse.Text = "...";
            this.cmdBrowse.UseVisualStyleBackColor = true;
            this.cmdBrowse.Click += new System.EventHandler(this.cmdBrowse_Click);
            // 
            // cmdUpload
            // 
            this.cmdUpload.Location = new System.Drawing.Point(116, 119);
            this.cmdUpload.Name = "cmdUpload";
            this.cmdUpload.Size = new System.Drawing.Size(75, 23);
            this.cmdUpload.TabIndex = 7;
            this.cmdUpload.Tag = "Button_OmaeUpload_Upload";
            this.cmdUpload.Text = "Upload";
            this.cmdUpload.UseVisualStyleBackColor = true;
            this.cmdUpload.Click += new System.EventHandler(this.cmdUpload_Click);
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(116, 60);
            this.txtDescription.MaxLength = 1000;
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(380, 53);
            this.txtDescription.TabIndex = 6;
            // 
            // lblDescriptionLabel
            // 
            this.lblDescriptionLabel.AutoSize = true;
            this.lblDescriptionLabel.Location = new System.Drawing.Point(12, 60);
            this.lblDescriptionLabel.Name = "lblDescriptionLabel";
            this.lblDescriptionLabel.Size = new System.Drawing.Size(63, 13);
            this.lblDescriptionLabel.TabIndex = 5;
            this.lblDescriptionLabel.Tag = "Label_OmaeUpload_Description";
            this.lblDescriptionLabel.Text = "Description:";
            // 
            // txtFilePath
            // 
            this.txtFilePath.AcceptsReturn = true;
            this.txtFilePath.Location = new System.Drawing.Point(116, 6);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.ReadOnly = true;
            this.txtFilePath.Size = new System.Drawing.Size(349, 20);
            this.txtFilePath.TabIndex = 1;
            // 
            // lblFilePathLabel
            // 
            this.lblFilePathLabel.AutoSize = true;
            this.lblFilePathLabel.Location = new System.Drawing.Point(12, 9);
            this.lblFilePathLabel.Name = "lblFilePathLabel";
            this.lblFilePathLabel.Size = new System.Drawing.Size(75, 13);
            this.lblFilePathLabel.TabIndex = 0;
            this.lblFilePathLabel.Tag = "Label_OmaeUpload_FileName";
            this.lblFilePathLabel.Text = "File to Upload:";
            // 
            // lblNameLabel
            // 
            this.lblNameLabel.AutoSize = true;
            this.lblNameLabel.Location = new System.Drawing.Point(12, 35);
            this.lblNameLabel.Name = "lblNameLabel";
            this.lblNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblNameLabel.TabIndex = 3;
            this.lblNameLabel.Tag = "Label_Name";
            this.lblNameLabel.Text = "Name:";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(116, 32);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(380, 20);
            this.txtName.TabIndex = 4;
            // 
            // frmOmaeUploadSheet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(508, 152);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.lblNameLabel);
            this.Controls.Add(this.cmdBrowse);
            this.Controls.Add(this.cmdUpload);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.lblDescriptionLabel);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.lblFilePathLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmOmaeUploadSheet";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_OmaeUploadSheet";
            this.Text = "Upload Character Sheet to Omae";
            this.Load += new System.EventHandler(this.frmOmaeUploadSheet_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdBrowse;
        private System.Windows.Forms.Button cmdUpload;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label lblDescriptionLabel;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Label lblFilePathLabel;
        private System.Windows.Forms.Label lblNameLabel;
        private System.Windows.Forms.TextBox txtName;
    }
}