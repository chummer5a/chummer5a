namespace Chummer
{
    partial class frmOmaeUploadData
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmOmaeUploadData));
            this.treFiles = new System.Windows.Forms.TreeView();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.lblDescriptionLabel = new System.Windows.Forms.Label();
            this.cmdUpload = new System.Windows.Forms.Button();
            this.lblNameLabel = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // treFiles
            // 
            this.treFiles.CheckBoxes = true;
            this.treFiles.Location = new System.Drawing.Point(12, 12);
            this.treFiles.Name = "treFiles";
            this.treFiles.ShowLines = false;
            this.treFiles.ShowRootLines = false;
            this.treFiles.Size = new System.Drawing.Size(262, 195);
            this.treFiles.TabIndex = 0;
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(349, 35);
            this.txtDescription.MaxLength = 1000;
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(380, 53);
            this.txtDescription.TabIndex = 4;
            // 
            // lblDescriptionLabel
            // 
            this.lblDescriptionLabel.AutoSize = true;
            this.lblDescriptionLabel.Location = new System.Drawing.Point(280, 38);
            this.lblDescriptionLabel.Name = "lblDescriptionLabel";
            this.lblDescriptionLabel.Size = new System.Drawing.Size(63, 13);
            this.lblDescriptionLabel.TabIndex = 3;
            this.lblDescriptionLabel.Tag = "Label_OmaeUpload_Description";
            this.lblDescriptionLabel.Text = "Description:";
            // 
            // cmdUpload
            // 
            this.cmdUpload.Location = new System.Drawing.Point(349, 184);
            this.cmdUpload.Name = "cmdUpload";
            this.cmdUpload.Size = new System.Drawing.Size(75, 23);
            this.cmdUpload.TabIndex = 5;
            this.cmdUpload.Tag = "Button_OmaeUpload_Upload";
            this.cmdUpload.Text = "Upload";
            this.cmdUpload.UseVisualStyleBackColor = true;
            this.cmdUpload.Click += new System.EventHandler(this.cmdUpload_Click);
            // 
            // lblNameLabel
            // 
            this.lblNameLabel.AutoSize = true;
            this.lblNameLabel.Location = new System.Drawing.Point(280, 12);
            this.lblNameLabel.Name = "lblNameLabel";
            this.lblNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblNameLabel.TabIndex = 1;
            this.lblNameLabel.Tag = "Label_Name";
            this.lblNameLabel.Text = "Name:";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(349, 9);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(380, 20);
            this.txtName.TabIndex = 2;
            // 
            // frmOmaeUploadData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(741, 220);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.lblNameLabel);
            this.Controls.Add(this.cmdUpload);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.lblDescriptionLabel);
            this.Controls.Add(this.treFiles);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmOmaeUploadData";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_OmaeUploadData";
            this.Text = "Upload Custom Data to Omae";
            this.Load += new System.EventHandler(this.frmOmaeUploadData_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treFiles;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label lblDescriptionLabel;
        private System.Windows.Forms.Button cmdUpload;
        private System.Windows.Forms.Label lblNameLabel;
        private System.Windows.Forms.TextBox txtName;

    }
}