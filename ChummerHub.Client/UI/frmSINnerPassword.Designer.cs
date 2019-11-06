namespace ChummerHub.Client.UI
{
    partial class frmSINnerPassword
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
            this.tlpPassword = new System.Windows.Forms.TableLayoutPanel();
            this.bOk = new System.Windows.Forms.Button();
            this.pbIcon = new System.Windows.Forms.PictureBox();
            this.lPasswordText = new System.Windows.Forms.Label();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.tlpPassword.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // tlpPassword
            // 
            this.tlpPassword.AutoSize = true;
            this.tlpPassword.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpPassword.ColumnCount = 2;
            this.tlpPassword.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpPassword.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPassword.Controls.Add(this.bOk, 1, 2);
            this.tlpPassword.Controls.Add(this.pbIcon, 0, 0);
            this.tlpPassword.Controls.Add(this.lPasswordText, 1, 0);
            this.tlpPassword.Controls.Add(this.tbPassword, 1, 1);
            this.tlpPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPassword.Location = new System.Drawing.Point(0, 0);
            this.tlpPassword.Name = "tlpPassword";
            this.tlpPassword.RowCount = 3;
            this.tlpPassword.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpPassword.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpPassword.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpPassword.Size = new System.Drawing.Size(393, 125);
            this.tlpPassword.TabIndex = 0;
            // 
            // bOk
            // 
            this.bOk.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.bOk.Location = new System.Drawing.Point(212, 97);
            this.bOk.Name = "bOk";
            this.bOk.Size = new System.Drawing.Size(75, 23);
            this.bOk.TabIndex = 0;
            this.bOk.Text = "Ok";
            this.bOk.UseVisualStyleBackColor = true;
            this.bOk.Click += new System.EventHandler(this.bOk_Click);
            // 
            // pbIcon
            // 
            this.pbIcon.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbIcon.Location = new System.Drawing.Point(3, 3);
            this.pbIcon.Name = "pbIcon";
            this.tlpPassword.SetRowSpan(this.pbIcon, 3);
            this.pbIcon.Size = new System.Drawing.Size(100, 119);
            this.pbIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbIcon.TabIndex = 1;
            this.pbIcon.TabStop = false;
            // 
            // lPasswordText
            // 
            this.lPasswordText.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lPasswordText.AutoSize = true;
            this.lPasswordText.Location = new System.Drawing.Point(109, 24);
            this.lPasswordText.Name = "lPasswordText";
            this.lPasswordText.Size = new System.Drawing.Size(56, 13);
            this.lPasswordText.TabIndex = 2;
            this.lPasswordText.Text = "Password:";
            // 
            // tbPassword
            // 
            this.tbPassword.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tbPassword.Location = new System.Drawing.Point(109, 67);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.Size = new System.Drawing.Size(194, 20);
            this.tbPassword.TabIndex = 3;
            this.tbPassword.UseSystemPasswordChar = true;
            // 
            // frmSINnerPassword
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(393, 125);
            this.Controls.Add(this.tlpPassword);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "frmSINnerPassword";
            this.Text = "frmSINnerPassword";
            this.tlpPassword.ResumeLayout(false);
            this.tlpPassword.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpPassword;
        private System.Windows.Forms.Button bOk;
        private System.Windows.Forms.PictureBox pbIcon;
        private System.Windows.Forms.Label lPasswordText;
        private System.Windows.Forms.TextBox tbPassword;
    }
}