namespace Chummer
{
    partial class frmOmaeUploadLanguage
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
            this.cmdBrowse = new System.Windows.Forms.Button();
            this.cmdUpload = new System.Windows.Forms.Button();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
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
            this.cmdUpload.Location = new System.Drawing.Point(116, 32);
            this.cmdUpload.Name = "cmdUpload";
            this.cmdUpload.Size = new System.Drawing.Size(75, 23);
            this.cmdUpload.TabIndex = 3;
            this.cmdUpload.Text = "Upload";
            this.cmdUpload.UseVisualStyleBackColor = true;
            this.cmdUpload.Click += new System.EventHandler(this.cmdUpload_Click);
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "File to Upload:";
            // 
            // frmOmaeUploadLanguage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(508, 67);
            this.Controls.Add(this.cmdBrowse);
            this.Controls.Add(this.cmdUpload);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmOmaeUploadLanguage";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Upload Language";
            this.Load += new System.EventHandler(this.frmOmaeUploadLanguage_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdBrowse;
        private System.Windows.Forms.Button cmdUpload;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Label label1;
    }
}