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
            this._spinner = new Chummer.NumericUpDownEx();
            ((System.ComponentModel.ISupportInitialize)(this._spinner)).BeginInit();
            this.SuspendLayout();
            // 
            // _spinner
            // 
            this._spinner.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._spinner.Location = new System.Drawing.Point(0, 0);
            this._spinner.Name = "_spinner";
            this._spinner.Size = new System.Drawing.Size(50, 20);
            this._spinner.TabIndex = 0;
            this._spinner.ValueChanged += new System.EventHandler(this.value_changed);
            // 
            // SpinnerTableCell
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this._spinner);
            this.Name = "SpinnerTableCell";
            this.Size = new System.Drawing.Size(50, 23);
            this.Load += OnLoad;
            ((System.ComponentModel.ISupportInitialize)(this._spinner)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Chummer.NumericUpDownEx _spinner;
    }
}
