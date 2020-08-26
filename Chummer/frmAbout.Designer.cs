namespace Chummer
{
    public partial class frmAbout
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            this.components = new System.ComponentModel.Container();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.lblCompanyName = new System.Windows.Forms.Label();
            this.lblCopyright = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.lblProductName = new System.Windows.Forms.Label();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.txtContributors = new System.Windows.Forms.TextBox();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.txtDisclaimer = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.cmdOK = new System.Windows.Forms.Button();
            this.tlpMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtDescription
            // 
            this.txtDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDescription.Location = new System.Drawing.Point(271, 103);
            this.txtDescription.MaxLength = 2147483647;
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ReadOnly = true;
            this.txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDescription.Size = new System.Drawing.Size(492, 201);
            this.txtDescription.TabIndex = 23;
            this.txtDescription.Text = "[Description]";
            this.txtDescription.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_KeyDown);
            // 
            // lblCompanyName
            // 
            this.lblCompanyName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCompanyName.AutoSize = true;
            this.lblCompanyName.Location = new System.Drawing.Point(271, 81);
            this.lblCompanyName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCompanyName.Name = "lblCompanyName";
            this.lblCompanyName.Size = new System.Drawing.Size(88, 13);
            this.lblCompanyName.TabIndex = 22;
            this.lblCompanyName.Text = "[Company Name]";
            // 
            // lblCopyright
            // 
            this.lblCopyright.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCopyright.AutoSize = true;
            this.lblCopyright.Location = new System.Drawing.Point(271, 56);
            this.lblCopyright.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCopyright.Name = "lblCopyright";
            this.lblCopyright.Size = new System.Drawing.Size(57, 13);
            this.lblCopyright.TabIndex = 21;
            this.lblCopyright.Text = "[Copyright]";
            this.lblCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblVersion
            // 
            this.lblVersion.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new System.Drawing.Point(271, 31);
            this.lblVersion.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(48, 13);
            this.lblVersion.TabIndex = 0;
            this.lblVersion.Text = "[Version]";
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblProductName
            // 
            this.lblProductName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblProductName.AutoSize = true;
            this.lblProductName.Location = new System.Drawing.Point(271, 6);
            this.lblProductName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblProductName.Name = "lblProductName";
            this.lblProductName.Size = new System.Drawing.Size(81, 13);
            this.lblProductName.TabIndex = 19;
            this.lblProductName.Text = "[Product Name]";
            this.lblProductName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65F));
            this.tlpMain.Controls.Add(this.txtContributors, 0, 5);
            this.tlpMain.Controls.Add(this.picLogo, 0, 0);
            this.tlpMain.Controls.Add(this.lblProductName, 1, 0);
            this.tlpMain.Controls.Add(this.lblVersion, 1, 1);
            this.tlpMain.Controls.Add(this.lblCopyright, 1, 2);
            this.tlpMain.Controls.Add(this.lblCompanyName, 1, 3);
            this.tlpMain.Controls.Add(this.txtDescription, 1, 4);
            this.tlpMain.Controls.Add(this.txtDisclaimer, 1, 5);
            this.tlpMain.Controls.Add(this.flowLayoutPanel1, 1, 6);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 7;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(766, 543);
            this.tlpMain.TabIndex = 0;
            // 
            // txtContributors
            // 
            this.txtContributors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtContributors.Location = new System.Drawing.Point(3, 310);
            this.txtContributors.MaxLength = 2147483647;
            this.txtContributors.Multiline = true;
            this.txtContributors.Name = "txtContributors";
            this.txtContributors.ReadOnly = true;
            this.txtContributors.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtContributors.Size = new System.Drawing.Size(262, 201);
            this.txtContributors.TabIndex = 25;
            this.txtContributors.Tag = "About_Label_Contributors";
            this.txtContributors.Text = "Thank You to All GitHub Contributors!";
            // 
            // picLogo
            // 
            this.picLogo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picLogo.Image = global::Chummer.Properties.Resources.troll;
            this.picLogo.Location = new System.Drawing.Point(3, 3);
            this.picLogo.Name = "picLogo";
            this.tlpMain.SetRowSpan(this.picLogo, 5);
            this.picLogo.Size = new System.Drawing.Size(262, 301);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLogo.TabIndex = 12;
            this.picLogo.TabStop = false;
            // 
            // txtDisclaimer
            // 
            this.txtDisclaimer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDisclaimer.Location = new System.Drawing.Point(271, 310);
            this.txtDisclaimer.MaxLength = 2147483647;
            this.txtDisclaimer.Multiline = true;
            this.txtDisclaimer.Name = "txtDisclaimer";
            this.txtDisclaimer.ReadOnly = true;
            this.txtDisclaimer.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDisclaimer.Size = new System.Drawing.Size(492, 201);
            this.txtDisclaimer.TabIndex = 26;
            this.txtDisclaimer.Tag = "About_Label_Disclaimer";
            this.txtDisclaimer.Text = "Disclaimer";
            this.txtDisclaimer.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_KeyDown);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.cmdOK);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(680, 514);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(86, 29);
            this.flowLayoutPanel1.TabIndex = 28;
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdOK.AutoSize = true;
            this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdOK.Location = new System.Drawing.Point(3, 3);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(80, 23);
            this.cmdOK.TabIndex = 27;
            this.cmdOK.Text = "&OK";
            // 
            // frmAbout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmAbout";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "frmAbout";
            this.Load += new System.EventHandler(this.frmAbout_Load);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label lblCompanyName;
        private System.Windows.Forms.Label lblCopyright;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Label lblProductName;
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.TextBox txtDisclaimer;
        private System.Windows.Forms.TextBox txtContributors;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
