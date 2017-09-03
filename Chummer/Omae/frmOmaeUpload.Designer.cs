namespace Chummer
{
    partial class frmOmaeUpload
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmOmaeUpload));
            this.lblFilePathLabel = new System.Windows.Forms.Label();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.lblCharacterNameLabel = new System.Windows.Forms.Label();
            this.lblDescriptionLabel = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.cmdUpload = new System.Windows.Forms.Button();
            this.cmdBrowse = new System.Windows.Forms.Button();
            this.lblCharacterTypeLabel = new System.Windows.Forms.Label();
            this.cboCharacterTypes = new System.Windows.Forms.ComboBox();
            this.cboCharacterName = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
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
            // txtFilePath
            // 
            this.txtFilePath.AcceptsReturn = true;
            this.txtFilePath.Location = new System.Drawing.Point(116, 6);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.ReadOnly = true;
            this.txtFilePath.Size = new System.Drawing.Size(349, 20);
            this.txtFilePath.TabIndex = 1;
            // 
            // lblCharacterNameLabel
            // 
            this.lblCharacterNameLabel.AutoSize = true;
            this.lblCharacterNameLabel.Location = new System.Drawing.Point(12, 35);
            this.lblCharacterNameLabel.Name = "lblCharacterNameLabel";
            this.lblCharacterNameLabel.Size = new System.Drawing.Size(87, 13);
            this.lblCharacterNameLabel.TabIndex = 3;
            this.lblCharacterNameLabel.Tag = "Label_OmaeUpload_CharacterName";
            this.lblCharacterNameLabel.Text = "Character Name:";
            // 
            // lblDescriptionLabel
            // 
            this.lblDescriptionLabel.AutoSize = true;
            this.lblDescriptionLabel.Location = new System.Drawing.Point(12, 84);
            this.lblDescriptionLabel.Name = "lblDescriptionLabel";
            this.lblDescriptionLabel.Size = new System.Drawing.Size(63, 13);
            this.lblDescriptionLabel.TabIndex = 7;
            this.lblDescriptionLabel.Tag = "Label_OmaeUpload_Description";
            this.lblDescriptionLabel.Text = "Description:";
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(116, 84);
            this.txtDescription.MaxLength = 1000;
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(380, 53);
            this.txtDescription.TabIndex = 8;
            // 
            // cmdUpload
            // 
            this.cmdUpload.Location = new System.Drawing.Point(116, 143);
            this.cmdUpload.Name = "cmdUpload";
            this.cmdUpload.Size = new System.Drawing.Size(75, 23);
            this.cmdUpload.TabIndex = 9;
            this.cmdUpload.Tag = "Button_OmaeUpload_Upload";
            this.cmdUpload.Text = "Upload";
            this.cmdUpload.UseVisualStyleBackColor = true;
            this.cmdUpload.Click += new System.EventHandler(this.cmdUpload_Click);
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
            // lblCharacterTypeLabel
            // 
            this.lblCharacterTypeLabel.AutoSize = true;
            this.lblCharacterTypeLabel.Location = new System.Drawing.Point(12, 60);
            this.lblCharacterTypeLabel.Name = "lblCharacterTypeLabel";
            this.lblCharacterTypeLabel.Size = new System.Drawing.Size(83, 13);
            this.lblCharacterTypeLabel.TabIndex = 5;
            this.lblCharacterTypeLabel.Tag = "Label_OmaeUpload_CharacterType";
            this.lblCharacterTypeLabel.Text = "Character Type:";
            // 
            // cboCharacterTypes
            // 
            this.cboCharacterTypes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCharacterTypes.FormattingEnabled = true;
            this.cboCharacterTypes.Location = new System.Drawing.Point(116, 57);
            this.cboCharacterTypes.Name = "cboCharacterTypes";
            this.cboCharacterTypes.Size = new System.Drawing.Size(168, 21);
            this.cboCharacterTypes.TabIndex = 6;
            // 
            // cboCharacterName
            // 
            this.cboCharacterName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCharacterName.FormattingEnabled = true;
            this.cboCharacterName.Location = new System.Drawing.Point(116, 32);
            this.cboCharacterName.Name = "cboCharacterName";
            this.cboCharacterName.Size = new System.Drawing.Size(380, 21);
            this.cboCharacterName.TabIndex = 4;
            // 
            // frmOmaeUpload
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(508, 175);
            this.Controls.Add(this.cboCharacterName);
            this.Controls.Add(this.lblCharacterTypeLabel);
            this.Controls.Add(this.cboCharacterTypes);
            this.Controls.Add(this.cmdBrowse);
            this.Controls.Add(this.cmdUpload);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.lblDescriptionLabel);
            this.Controls.Add(this.lblCharacterNameLabel);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.lblFilePathLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmOmaeUpload";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_OmaeUpload";
            this.Text = "Upload Character to Omae";
            this.Load += new System.EventHandler(this.frmOmaeUpload_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblFilePathLabel;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Label lblCharacterNameLabel;
        private System.Windows.Forms.Label lblDescriptionLabel;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Button cmdUpload;
        private System.Windows.Forms.Button cmdBrowse;
        private System.Windows.Forms.Label lblCharacterTypeLabel;
        private System.Windows.Forms.ComboBox cboCharacterTypes;
        private System.Windows.Forms.ComboBox cboCharacterName;
    }
}