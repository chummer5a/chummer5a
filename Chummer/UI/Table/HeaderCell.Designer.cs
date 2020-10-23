namespace Chummer.UI.Table
{
    partial class HeaderCell
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
                components?.Dispose();
                _objGraphics?.Dispose();
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
            this._lblCellText = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _lblCellText
            // 
            this._lblCellText.AutoSize = true;
            this._lblCellText.Location = new System.Drawing.Point(0, 0);
            this._lblCellText.Name = "_lblCellText";
            this._lblCellText.Size = new System.Drawing.Size(35, 13);
            this._lblCellText.TabIndex = 0;
            this._lblCellText.Text = "label1";
            // 
            // HeaderCell
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlText;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this._lblCellText);
            this.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.Name = "HeaderCell";
            this.Size = new System.Drawing.Size(148, 15);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _lblCellText;
    }
}
