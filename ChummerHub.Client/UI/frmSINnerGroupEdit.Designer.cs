namespace ChummerHub.Client.UI
{
    partial class frmSINnerGroupEdit
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
            this.siNnerGroupCreate1 = new ChummerHub.Client.UI.SINnerGroupCreate();
            this.SuspendLayout();
            // 
            // siNnerGroupCreate1
            // 
            this.siNnerGroupCreate1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.siNnerGroupCreate1.Location = new System.Drawing.Point(0, 0);
            this.siNnerGroupCreate1.Name = "siNnerGroupCreate1";
            this.siNnerGroupCreate1.Size = new System.Drawing.Size(305, 195);
            this.siNnerGroupCreate1.TabIndex = 0;
            // 
            // frmSINnerGroupEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(305, 195);
            this.Controls.Add(this.siNnerGroupCreate1);
            this.Name = "frmSINnerGroupEdit";
            this.Text = "frmSINnerGroupEdit";
            this.ResumeLayout(false);

        }

        #endregion

        private SINnerGroupCreate siNnerGroupCreate1;

      
    }
}
