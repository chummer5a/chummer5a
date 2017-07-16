namespace Chummer.UI.Shared
{
    partial class BindingListDisplay<TType>
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
            this.pnlDisplay = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // pnlDisplay
            // 
            this.pnlDisplay.Location = new System.Drawing.Point(0, 0);
            this.pnlDisplay.Name = "pnlDisplay";
            this.pnlDisplay.Size = new System.Drawing.Size(606, 433);
            this.pnlDisplay.TabIndex = 0;
            // 
            // BindingListDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.pnlDisplay);
            this.Name = "BindingListDisplay";
            this.Size = new System.Drawing.Size(606, 433);
            this.Load += new System.EventHandler(this.SkillsDisplay_Load);
            this.Scroll += new System.Windows.Forms.ScrollEventHandler(this.BindingListDisplay_Scroll);
            this.SizeChanged += new System.EventHandler(this.BindingListDisplay_SizeChanged);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlDisplay;
    }
}
