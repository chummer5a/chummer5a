namespace Chummer.UI.Table
{
    partial class SpinnerTableCell<T>
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
            if (disposing)
            {
                _objUpdateSemaphore.Dispose();
                ValueUpdater = null; // to help the GC
                if (components != null)
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
            this.nudSpinner = new Chummer.NumericUpDownEx();
            ((System.ComponentModel.ISupportInitialize)(this.nudSpinner)).BeginInit();
            this.SuspendLayout();
            // 
            // nudSpinner
            // 
            this.nudSpinner.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudSpinner.Location = new System.Drawing.Point(0, 0);
            this.nudSpinner.Name = "nudSpinner";
            this.nudSpinner.Size = new System.Drawing.Size(50, 20);
            this.nudSpinner.TabIndex = 0;
            this.nudSpinner.ValueChanged += new System.EventHandler(this.nudSpinner_ValueChanged);
            // 
            // SpinnerTableCell
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.nudSpinner);
            this.Name = "SpinnerTableCell";
            this.Size = new System.Drawing.Size(50, 23);
            this.Load += OnLoad;
            ((System.ComponentModel.ISupportInitialize)(this.nudSpinner)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Chummer.NumericUpDownEx nudSpinner;
    }
}
