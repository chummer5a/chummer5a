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
			this.cartesianChart1 = new LiveCharts.WinForms.CartesianChart();
			this.SuspendLayout();
			// 
			// cartesianChart1
			// 
			this.cartesianChart1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cartesianChart1.Location = new System.Drawing.Point(0, 0);
			this.cartesianChart1.Name = "cartesianChart1";
			this.cartesianChart1.Size = new System.Drawing.Size(284, 261);
			this.cartesianChart1.TabIndex = 0;
			this.cartesianChart1.Text = "cartesianChart1";
			// 
			// ExpenseChart
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.cartesianChart1);
			this.Name = "ExpenseChart";
			this.Size = new System.Drawing.Size(284, 261);
			this.ResumeLayout(false);

        }

        #endregion

        private LiveCharts.WinForms.CartesianChart cartesianChart1;
    }
}
