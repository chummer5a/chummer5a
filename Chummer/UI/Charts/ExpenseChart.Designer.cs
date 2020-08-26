namespace Chummer.UI.Charts
{
    partial class ExpenseChart
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
                if (!(ParentForm is CharacterShared frmParent) || frmParent.CharacterObject != _objCharacter)
                    _objCharacter?.Dispose();
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
            this.chtCartesian = new LiveCharts.WinForms.CartesianChart();
            this.SuspendLayout();
            // 
            // chtCartesian
            // 
            this.chtCartesian.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chtCartesian.Location = new System.Drawing.Point(0, 0);
            this.chtCartesian.Name = "chtCartesian";
            this.chtCartesian.Size = new System.Drawing.Size(284, 261);
            this.chtCartesian.TabIndex = 0;
            // 
            // ExpenseChart
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.chtCartesian);
            this.DoubleBuffered = true;
            this.Name = "ExpenseChart";
            this.Size = new System.Drawing.Size(284, 261);
            this.Load += new System.EventHandler(this.ExpenseChart_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private LiveCharts.WinForms.CartesianChart chtCartesian;
    }
}
