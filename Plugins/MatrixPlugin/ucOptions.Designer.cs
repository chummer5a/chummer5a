namespace MatrixPlugin
{
    partial class ucOptions
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.MySampleLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // MySampleLabel
            // 
            this.MySampleLabel.AutoSize = true;
            this.MySampleLabel.Location = new System.Drawing.Point(4, 4);
            this.MySampleLabel.Name = "MySampleLabel";
            this.MySampleLabel.Size = new System.Drawing.Size(162, 13);
            this.MySampleLabel.TabIndex = 0;
            this.MySampleLabel.Text = "There is no options, just CheckIn";
            // 
            // ucOptions
            // 
            this.Controls.Add(this.MySampleLabel);
            this.Name = "ucOptions";
            this.Size = new System.Drawing.Size(359, 309);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label MySampleLabel;
    }
}
