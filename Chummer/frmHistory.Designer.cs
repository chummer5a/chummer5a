namespace Chummer
{
    partial class frmHistory
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
            this.txtRevisionHistory = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtRevisionHistory
            // 
            this.txtRevisionHistory.BackColor = System.Drawing.Color.White;
            this.txtRevisionHistory.Location = new System.Drawing.Point(12, 12);
            this.txtRevisionHistory.Multiline = true;
            this.txtRevisionHistory.Name = "txtRevisionHistory";
            this.txtRevisionHistory.ReadOnly = true;
            this.txtRevisionHistory.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtRevisionHistory.Size = new System.Drawing.Size(760, 438);
            this.txtRevisionHistory.TabIndex = 0;
            // 
            // frmHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 462);
            this.Controls.Add(this.txtRevisionHistory);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmHistory";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_History";
            this.Text = "Chummer Revision History";
            this.Load += new System.EventHandler(this.frmHistory_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtRevisionHistory;
    }
}