namespace ChummerHub.Client.UI
{
    partial class frmSINnerResponse
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
            this.siNnerResponseUI1 = new ChummerHub.Client.UI.ucSINnerResponseUI();
            this.SuspendLayout();
            // 
            // siNnerResponseUI1
            // 
            this.siNnerResponseUI1.AutoSize = true;
            this.siNnerResponseUI1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.siNnerResponseUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.siNnerResponseUI1.Location = new System.Drawing.Point(0, 0);
            this.siNnerResponseUI1.MinimumSize = new System.Drawing.Size(200, 200);
            this.siNnerResponseUI1.Name = "siNnerResponseUI1";
            this.siNnerResponseUI1.Size = new System.Drawing.Size(575, 495);
            this.siNnerResponseUI1.TabIndex = 0;
            // 
            // frmSINnerResponse
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(575, 495);
            this.Controls.Add(this.siNnerResponseUI1);
            this.Name = "frmSINnerResponse";
            this.Text = "frmSINnerResponse";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ucSINnerResponseUI siNnerResponseUI1;
    }
}