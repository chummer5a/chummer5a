namespace ChummerHub.Client.UI
{
    partial class SINnersBasic
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
            this.tabLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.cbSRMReady = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.tbGroupname = new System.Windows.Forms.TextBox();
            this.bUpload = new System.Windows.Forms.Button();
            this.tabLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabLayoutPanel
            // 
            this.tabLayoutPanel.ColumnCount = 2;
            this.tabLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tabLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tabLayoutPanel.Controls.Add(this.bUpload, 0, 3);
            this.tabLayoutPanel.Controls.Add(this.cbSRMReady, 0, 0);
            this.tabLayoutPanel.Controls.Add(this.label1, 0, 1);
            this.tabLayoutPanel.Controls.Add(this.textBox1, 0, 2);
            this.tabLayoutPanel.Controls.Add(this.tbGroupname, 1, 1);
            this.tabLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tabLayoutPanel.Name = "tabLayoutPanel";
            this.tabLayoutPanel.RowCount = 4;
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayoutPanel.Size = new System.Drawing.Size(368, 205);
            this.tabLayoutPanel.TabIndex = 0;
            this.tabLayoutPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.tabLayoutPanel_Paint);
            // 
            // cbSRMReady
            // 
            this.cbSRMReady.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cbSRMReady.AutoSize = true;
            this.cbSRMReady.Location = new System.Drawing.Point(3, 3);
            this.cbSRMReady.Name = "cbSRMReady";
            this.cbSRMReady.Size = new System.Drawing.Size(79, 17);
            this.cbSRMReady.TabIndex = 0;
            this.cbSRMReady.Text = "SRM ready";
            this.cbSRMReady.UseVisualStyleBackColor = true;
            this.cbSRMReady.Click += new System.EventHandler(this.cbSRMReady_Click);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Groupname:";
            // 
            // textBox1
            // 
            this.tabLayoutPanel.SetColumnSpan(this.textBox1, 2);
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(3, 52);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(390, 121);
            this.textBox1.TabIndex = 4;
            this.textBox1.Text = "As you probalby can tell, I\'m not an UI dev. This is just a demo to call the WebS" +
    "ervice. I hope some of you awesome UI guys pick this up.";
            // 
            // tbGroupname
            // 
            this.tbGroupname.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tbGroupname.Location = new System.Drawing.Point(88, 26);
            this.tbGroupname.Name = "tbGroupname";
            this.tbGroupname.Size = new System.Drawing.Size(190, 20);
            this.tbGroupname.TabIndex = 2;
            this.tbGroupname.TextChanged += new System.EventHandler(this.tbGroupname_TextChanged);
            // 
            // bUpload
            // 
            this.bUpload.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tabLayoutPanel.SetColumnSpan(this.bUpload, 2);
            this.bUpload.Enabled = false;
            this.bUpload.Location = new System.Drawing.Point(160, 179);
            this.bUpload.Name = "bUpload";
            this.bUpload.Size = new System.Drawing.Size(75, 23);
            this.bUpload.TabIndex = 5;
            this.bUpload.Text = "Upload";
            this.bUpload.UseVisualStyleBackColor = true;
            this.bUpload.Click += new System.EventHandler(this.bUpload_Click);
            // 
            // SINnersBasic
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tabLayoutPanel);
            this.Name = "SINnersBasic";
            this.Size = new System.Drawing.Size(371, 208);
            this.tabLayoutPanel.ResumeLayout(false);
            this.tabLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tabLayoutPanel;
        private System.Windows.Forms.CheckBox cbSRMReady;
        private System.Windows.Forms.TextBox tbGroupname;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button bUpload;
    }
}
