namespace ChummerHub.Client.UI
{
    partial class SINnersOptions
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
            this.bLogin = new System.Windows.Forms.Button();
            this.tlpOptions = new System.Windows.Forms.TableLayoutPanel();
            this.bLogout = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.labelAccountStatus = new System.Windows.Forms.Label();
            this.lSINnerUrl = new System.Windows.Forms.Label();
            this.cbSINnerUrl = new System.Windows.Forms.ComboBox();
            this.tlpOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // bLogin
            // 
            this.bLogin.Location = new System.Drawing.Point(123, 3);
            this.bLogin.Name = "bLogin";
            this.bLogin.Size = new System.Drawing.Size(75, 23);
            this.bLogin.TabIndex = 0;
            this.bLogin.Text = "Login";
            this.bLogin.UseVisualStyleBackColor = true;
            this.bLogin.Click += new System.EventHandler(this.bLogin_ClickAsync);
            // 
            // tlpOptions
            // 
            this.tlpOptions.AutoSize = true;
            this.tlpOptions.ColumnCount = 5;
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpOptions.Controls.Add(this.bLogin, 2, 0);
            this.tlpOptions.Controls.Add(this.bLogout, 3, 0);
            this.tlpOptions.Controls.Add(this.label1, 0, 0);
            this.tlpOptions.Controls.Add(this.labelAccountStatus, 1, 0);
            this.tlpOptions.Controls.Add(this.lSINnerUrl, 0, 1);
            this.tlpOptions.Controls.Add(this.cbSINnerUrl, 1, 1);
            this.tlpOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpOptions.Location = new System.Drawing.Point(0, 0);
            this.tlpOptions.Name = "tlpOptions";
            this.tlpOptions.RowCount = 3;
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpOptions.Size = new System.Drawing.Size(565, 579);
            this.tlpOptions.TabIndex = 1;
            // 
            // bLogout
            // 
            this.bLogout.Location = new System.Drawing.Point(204, 3);
            this.bLogout.Name = "bLogout";
            this.bLogout.Size = new System.Drawing.Size(75, 23);
            this.bLogout.TabIndex = 1;
            this.bLogout.Text = "Logout";
            this.bLogout.UseVisualStyleBackColor = true;
            this.bLogout.Click += new System.EventHandler(this.bLogout_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 29);
            this.label1.TabIndex = 2;
            this.label1.Text = "Account:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelAccountStatus
            // 
            this.labelAccountStatus.AutoSize = true;
            this.labelAccountStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelAccountStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAccountStatus.ForeColor = System.Drawing.Color.DarkMagenta;
            this.labelAccountStatus.Location = new System.Drawing.Point(59, 0);
            this.labelAccountStatus.Name = "labelAccountStatus";
            this.labelAccountStatus.Size = new System.Drawing.Size(58, 29);
            this.labelAccountStatus.TabIndex = 3;
            this.labelAccountStatus.Text = "unknown";
            this.labelAccountStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lSINnerUrl
            // 
            this.lSINnerUrl.AutoSize = true;
            this.lSINnerUrl.Dock = System.Windows.Forms.DockStyle.Left;
            this.lSINnerUrl.Location = new System.Drawing.Point(3, 29);
            this.lSINnerUrl.Name = "lSINnerUrl";
            this.lSINnerUrl.Size = new System.Drawing.Size(23, 27);
            this.lSINnerUrl.TabIndex = 4;
            this.lSINnerUrl.Text = "Url:";
            this.lSINnerUrl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cbSINnerUrl
            // 
            this.tlpOptions.SetColumnSpan(this.cbSINnerUrl, 4);
            this.cbSINnerUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbSINnerUrl.FormattingEnabled = true;
            this.cbSINnerUrl.Location = new System.Drawing.Point(59, 32);
            this.cbSINnerUrl.Name = "cbSINnerUrl";
            this.cbSINnerUrl.Size = new System.Drawing.Size(503, 21);
            this.cbSINnerUrl.TabIndex = 5;
            // 
            // SINnersOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tlpOptions);
            this.Name = "SINnersOptions";
            this.Size = new System.Drawing.Size(565, 579);
            this.tlpOptions.ResumeLayout(false);
            this.tlpOptions.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bLogin;
        private System.Windows.Forms.TableLayoutPanel tlpOptions;
        private System.Windows.Forms.Button bLogout;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelAccountStatus;
        private System.Windows.Forms.Label lSINnerUrl;
        private System.Windows.Forms.ComboBox cbSINnerUrl;
    }
}
