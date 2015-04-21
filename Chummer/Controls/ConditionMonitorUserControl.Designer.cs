namespace Chummer
{
    partial class ConditionMonitorUserControl
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
            this._progressBarPhysical = new System.Windows.Forms.ProgressBar();
            this._progressBarStun = new System.Windows.Forms.ProgressBar();
            this._btnPhysical = new System.Windows.Forms.Button();
            this._btnApplyStun = new System.Windows.Forms.Button();
            this._lblPhysical = new System.Windows.Forms.Label();
            this._lblStun = new System.Windows.Forms.Label();
            this._nudPhysical = new System.Windows.Forms.NumericUpDown();
            this.nudStun = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this._nudPhysical)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudStun)).BeginInit();
            this.SuspendLayout();
            // 
            // _progressBarPhysical
            // 
            this._progressBarPhysical.Location = new System.Drawing.Point(3, 28);
            this._progressBarPhysical.Name = "_progressBarPhysical";
            this._progressBarPhysical.Size = new System.Drawing.Size(204, 41);
            this._progressBarPhysical.TabIndex = 8;
            // 
            // _progressBarStun
            // 
            this._progressBarStun.Location = new System.Drawing.Point(217, 28);
            this._progressBarStun.Name = "_progressBarStun";
            this._progressBarStun.Size = new System.Drawing.Size(200, 41);
            this._progressBarStun.TabIndex = 10;
            // 
            // _btnPhysical
            // 
            this._btnPhysical.Location = new System.Drawing.Point(117, 75);
            this._btnPhysical.Name = "_btnPhysical";
            this._btnPhysical.Size = new System.Drawing.Size(90, 23);
            this._btnPhysical.TabIndex = 7;
            this._btnPhysical.Text = "Apply Physical";
            this._btnPhysical.UseVisualStyleBackColor = true;
            this._btnPhysical.Click += new System.EventHandler(this._btnPhysical_Click);
            // 
            // _btnApplyStun
            // 
            this._btnApplyStun.Location = new System.Drawing.Point(327, 75);
            this._btnApplyStun.Name = "_btnApplyStun";
            this._btnApplyStun.Size = new System.Drawing.Size(90, 23);
            this._btnApplyStun.TabIndex = 11;
            this._btnApplyStun.Text = "Apply Stun";
            this._btnApplyStun.UseVisualStyleBackColor = true;
            this._btnApplyStun.Click += new System.EventHandler(this._btnApplyStun_Click);
            // 
            // _lblPhysical
            // 
            this._lblPhysical.AutoSize = true;
            this._lblPhysical.Location = new System.Drawing.Point(75, 4);
            this._lblPhysical.Name = "_lblPhysical";
            this._lblPhysical.Size = new System.Drawing.Size(72, 13);
            this._lblPhysical.TabIndex = 12;
            this._lblPhysical.Tag = "Label_CMPhysical";
            this._lblPhysical.Text = "{Physical:HP}";
            // 
            // _lblStun
            // 
            this._lblStun.AutoSize = true;
            this._lblStun.Location = new System.Drawing.Point(285, 4);
            this._lblStun.Name = "_lblStun";
            this._lblStun.Size = new System.Drawing.Size(55, 13);
            this._lblStun.TabIndex = 13;
            this._lblStun.Tag = "Label_CMStun";
            this._lblStun.Text = "{Stun:HP}";
            // 
            // _nudPhysical
            // 
            this._nudPhysical.Location = new System.Drawing.Point(3, 78);
            this._nudPhysical.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this._nudPhysical.Name = "_nudPhysical";
            this._nudPhysical.Size = new System.Drawing.Size(108, 20);
            this._nudPhysical.TabIndex = 14;
            // 
            // nudStun
            // 
            this.nudStun.Location = new System.Drawing.Point(217, 78);
            this.nudStun.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.nudStun.Name = "nudStun";
            this.nudStun.Size = new System.Drawing.Size(104, 20);
            this.nudStun.TabIndex = 15;
            // 
            // ConditionMonitorUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.nudStun);
            this.Controls.Add(this._nudPhysical);
            this.Controls.Add(this._lblStun);
            this.Controls.Add(this._lblPhysical);
            this.Controls.Add(this._progressBarPhysical);
            this.Controls.Add(this._progressBarStun);
            this.Controls.Add(this._btnPhysical);
            this.Controls.Add(this._btnApplyStun);
            this.Name = "ConditionMonitorUserControl";
            this.Size = new System.Drawing.Size(420, 420);
            ((System.ComponentModel.ISupportInitialize)(this._nudPhysical)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudStun)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar _progressBarPhysical;
        private System.Windows.Forms.ProgressBar _progressBarStun;
        private System.Windows.Forms.Button _btnPhysical;
        private System.Windows.Forms.Button _btnApplyStun;
        private System.Windows.Forms.Label _lblPhysical;
        private System.Windows.Forms.Label _lblStun;
        private System.Windows.Forms.NumericUpDown _nudPhysical;
        private System.Windows.Forms.NumericUpDown nudStun;


    }
}
