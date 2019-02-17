namespace Chummer
{
    partial class frmInitiative
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
            this.ucInit = new Chummer.InitiativeUserControl();
            this.SuspendLayout();
            // 
            // ucInit
            // 
            this.ucInit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucInit.Location = new System.Drawing.Point(0, 0);
            this.ucInit.Name = "ucInit";
            this.ucInit.Size = new System.Drawing.Size(427, 318);
            this.ucInit.TabIndex = 0;
            this.ucInit.Tag = string.Empty;
            // 
            // frmInitiative
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(427, 318);
            this.Controls.Add(this.ucInit);
            this.Name = "frmInitiative";
            this.Tag = "String_AttributeINILong";
            this.Text = "[Initiative]";
            this.ResumeLayout(false);

        }

        #endregion

        private InitiativeUserControl ucInit;
    }
}