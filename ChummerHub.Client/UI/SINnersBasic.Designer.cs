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
            this.tbGroupname = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.bUpload = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.tabLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabLayoutPanel
            // 
            this.tabLayoutPanel.ColumnCount = 4;
            this.tabLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tabLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tabLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tabLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 315F));
            this.tabLayoutPanel.Controls.Add(this.cbSRMReady, 0, 0);
            this.tabLayoutPanel.Controls.Add(this.tbGroupname, 2, 1);
            this.tabLayoutPanel.Controls.Add(this.label1, 0, 1);
            this.tabLayoutPanel.Controls.Add(this.bUpload, 2, 4);
            this.tabLayoutPanel.Controls.Add(this.textBox1, 0, 3);
            this.tabLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tabLayoutPanel.Name = "tabLayoutPanel";
            this.tabLayoutPanel.RowCount = 4;
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tabLayoutPanel.Size = new System.Drawing.Size(420, 303);
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
            // tbGroupname
            // 
            this.tbGroupname.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tabLayoutPanel.SetColumnSpan(this.tbGroupname, 2);
            this.tbGroupname.Location = new System.Drawing.Point(91, 26);
            this.tbGroupname.Name = "tbGroupname";
            this.tbGroupname.Size = new System.Drawing.Size(190, 20);
            this.tbGroupname.TabIndex = 2;
            this.tbGroupname.TextChanged += new System.EventHandler(this.tbGroupname_TextChanged);
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
            // bUpload
            // 
            this.tabLayoutPanel.SetColumnSpan(this.bUpload, 2);
            this.bUpload.Location = new System.Drawing.Point(91, 72);
            this.bUpload.Name = "bUpload";
            this.bUpload.Size = new System.Drawing.Size(190, 23);
            this.bUpload.TabIndex = 3;
            this.bUpload.Text = "Upload to SINners";
            this.bUpload.UseVisualStyleBackColor = true;
            this.bUpload.Click += new System.EventHandler(this.bUpload_Click);
            // 
            // textBox1
            // 
            this.tabLayoutPanel.SetColumnSpan(this.textBox1, 4);
            this.textBox1.Location = new System.Drawing.Point(3, 52);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(417, 20);
            this.textBox1.TabIndex = 4;
            this.textBox1.Text = "As you probalby can tell, I\'m not an UI dev. This is just a demo to call the WebS" +
    "ervice. I hope some of you awesome UI guys pick this up.";
            // 
            // SINnersBasic
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabLayoutPanel);
            this.Name = "SINnersBasic";
            this.Size = new System.Drawing.Size(420, 303);
            this.tabLayoutPanel.ResumeLayout(false);
            this.tabLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tabLayoutPanel;
        private System.Windows.Forms.CheckBox cbSRMReady;
        private System.Windows.Forms.TextBox tbGroupname;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button bUpload;
        private System.Windows.Forms.TextBox textBox1;
    }
}
