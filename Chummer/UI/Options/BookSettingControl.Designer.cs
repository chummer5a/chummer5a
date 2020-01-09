namespace Chummer.UI.Options
{
    partial class BookSettingControl
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
            this.lblName = new System.Windows.Forms.Label();
            this.chkEnabled = new System.Windows.Forms.CheckBox();
            this.lblPath = new System.Windows.Forms.Label();
            this.lblOffset = new System.Windows.Forms.Label();
            this.btnTest = new System.Windows.Forms.Button();
            this.nudOffset = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.nudOffset)).BeginInit();
            this.SuspendLayout();
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblName.Location = new System.Drawing.Point(0, 0);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(155, 20);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "[Book Name Here]";
            this.lblName.UseMnemonic = false;
            // 
            // chkEnabled
            // 
            this.chkEnabled.AutoSize = true;
            this.chkEnabled.Location = new System.Drawing.Point(13, 24);
            this.chkEnabled.Name = "chkEnabled";
            this.chkEnabled.Size = new System.Drawing.Size(71, 17);
            this.chkEnabled.TabIndex = 1;
            this.chkEnabled.Text = "[Enabled]";
            this.chkEnabled.UseVisualStyleBackColor = true;
            // 
            // lblPath
            // 
            this.lblPath.AutoSize = true;
            this.lblPath.Location = new System.Drawing.Point(8, 49);
            this.lblPath.Name = "lblPath";
            this.lblPath.Size = new System.Drawing.Size(75, 13);
            this.lblPath.TabIndex = 2;
            this.lblPath.Text = "PDF Location;";
            // 
            // lblOffset
            // 
            this.lblOffset.AutoSize = true;
            this.lblOffset.Location = new System.Drawing.Point(8, 77);
            this.lblOffset.Name = "lblOffset";
            this.lblOffset.Size = new System.Drawing.Size(66, 13);
            this.lblOffset.TabIndex = 5;
            this.lblOffset.Text = "Page Offset:";
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(126, 72);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(133, 23);
            this.btnTest.TabIndex = 6;
            this.btnTest.Text = "Test - Open to page 5";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // nudOffset
            // 
            this.nudOffset.Location = new System.Drawing.Point(80, 75);
            this.nudOffset.Name = "nudOffset";
            this.nudOffset.Size = new System.Drawing.Size(40, 20);
            this.nudOffset.TabIndex = 7;
            // 
            // BookSettingControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.nudOffset);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.lblOffset);
            this.Controls.Add(this.lblPath);
            this.Controls.Add(this.chkEnabled);
            this.Controls.Add(this.lblName);
            this.Name = "BookSettingControl";
            this.Size = new System.Drawing.Size(354, 101);
            ((System.ComponentModel.ISupportInitialize)(this.nudOffset)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.CheckBox chkEnabled;
        private System.Windows.Forms.Label lblPath;
        private System.Windows.Forms.Label lblOffset;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.NumericUpDown nudOffset;
    }
}
