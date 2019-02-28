namespace Chummer
{
    partial class frmOmaeCompress
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
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.lblFilePathLabel = new System.Windows.Forms.Label();
            this.cmdCompress = new System.Windows.Forms.Button();
            this.txtDestination = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtFilePath
            // 
            this.txtFilePath.AcceptsReturn = true;
            this.txtFilePath.Location = new System.Drawing.Point(117, 12);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.Size = new System.Drawing.Size(349, 20);
            this.txtFilePath.TabIndex = 1;
            // 
            // lblFilePathLabel
            // 
            this.lblFilePathLabel.AutoSize = true;
            this.lblFilePathLabel.Location = new System.Drawing.Point(13, 15);
            this.lblFilePathLabel.Name = "lblFilePathLabel";
            this.lblFilePathLabel.Size = new System.Drawing.Size(92, 13);
            this.lblFilePathLabel.TabIndex = 0;
            this.lblFilePathLabel.Tag = "Label_OmaeUpload_FileName";
            this.lblFilePathLabel.Text = "Files to Compress:";
            // 
            // cmdCompress
            // 
            this.cmdCompress.Location = new System.Drawing.Point(117, 64);
            this.cmdCompress.Name = "cmdCompress";
            this.cmdCompress.Size = new System.Drawing.Size(75, 23);
            this.cmdCompress.TabIndex = 4;
            this.cmdCompress.Tag = "Button_OmaeUpload_Upload";
            this.cmdCompress.Text = "Compress";
            this.cmdCompress.UseVisualStyleBackColor = true;
            this.cmdCompress.Click += new System.EventHandler(this.cmdCompress_Click);
            // 
            // txtDestination
            // 
            this.txtDestination.AcceptsReturn = true;
            this.txtDestination.Location = new System.Drawing.Point(117, 38);
            this.txtDestination.Name = "txtDestination";
            this.txtDestination.Size = new System.Drawing.Size(349, 20);
            this.txtDestination.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 2;
            this.label1.Tag = "Label_OmaeUpload_FileName";
            this.label1.Text = "Destination:";
            // 
            // frmOmaeCompress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(516, 99);
            this.Controls.Add(this.txtDestination);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmdCompress);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.lblFilePathLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmOmaeCompress";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Compress Files for Omae";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Label lblFilePathLabel;
        private System.Windows.Forms.Button cmdCompress;
        private System.Windows.Forms.TextBox txtDestination;
        private System.Windows.Forms.Label label1;
    }
}