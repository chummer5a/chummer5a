namespace Chummer.UI.Table
{
    partial class CheckBoxTableCell<T>
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
            this._checkBox = new Chummer.ColorableCheckBox();
            this.SuspendLayout();
            // 
            // _checkBox
            // 
            this._checkBox.AutoSize = true;
            this._checkBox.Location = new System.Drawing.Point(0, 0);
            this._checkBox.Name = "_checkBox";
            this._checkBox.Size = new System.Drawing.Size(80, 17);
            this._checkBox.TabIndex = 0;
            this._checkBox.Text = "checkBox1";
            this._checkBox.UseVisualStyleBackColor = true;
            this._checkBox.CheckedChanged += new System.EventHandler(this.checked_changed);
            // 
            // CheckBoxTableCell
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this._checkBox);
            this.Name = "CheckBoxTableCell";
            this.Size = new System.Drawing.Size(83, 22);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Chummer.ColorableCheckBox _checkBox;
    }
}
