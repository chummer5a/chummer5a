namespace ChummerHub.Client.UI
{
    partial class frmSINnerShare
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
            this.ucSINnerShare1 = new ChummerHub.Client.UI.ucSINnerShare();
            this.SuspendLayout();
            // 
            // ucSINnerShare1
            // 
            this.ucSINnerShare1.AutoSize = true;
            this.ucSINnerShare1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ucSINnerShare1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucSINnerShare1.Location = new System.Drawing.Point(0, 0);
            this.ucSINnerShare1.Name = "ucSINnerShare1";
            this.ucSINnerShare1.Size = new System.Drawing.Size(619, 278);
            this.ucSINnerShare1.TabIndex = 0;
            // 
            // frmSINnerShare
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(619, 278);
            this.Controls.Add(this.ucSINnerShare1);
            this.Name = "frmSINnerShare";
            this.Text = "frmSINnerShare";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ucSINnerShare ucSINnerShare1;
    }
}
