namespace ChummerHub.Client.UI
{
    partial class ucSINnersUserControl
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
                TabSINnersBasic?.Dispose();
                TabSINnersAdvanced?.Dispose();
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
            this.tabSINner = new System.Windows.Forms.TabControl();
            this.tabPageBasic = new System.Windows.Forms.TabPage();
            this.tabPageAdvanced = new System.Windows.Forms.TabPage();
            this.tabSINner.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabSINner
            // 
            this.tabSINner.Controls.Add(this.tabPageBasic);
            this.tabSINner.Controls.Add(this.tabPageAdvanced);
            this.tabSINner.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabSINner.Location = new System.Drawing.Point(0, 0);
            this.tabSINner.Name = "tabSINner";
            this.tabSINner.SelectedIndex = 0;
            this.tabSINner.Size = new System.Drawing.Size(800, 600);
            this.tabSINner.TabIndex = 0;
            // 
            // tabPageBasic
            // 
            this.tabPageBasic.AutoScroll = true;
            this.tabPageBasic.Location = new System.Drawing.Point(4, 22);
            this.tabPageBasic.Name = "tabPageBasic";
            this.tabPageBasic.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageBasic.Size = new System.Drawing.Size(792, 574);
            this.tabPageBasic.TabIndex = 0;
            this.tabPageBasic.Text = "Basic";
            this.tabPageBasic.UseVisualStyleBackColor = true;
            // 
            // tabPageAdvanced
            // 
            this.tabPageAdvanced.AutoScroll = true;
            this.tabPageAdvanced.Location = new System.Drawing.Point(4, 22);
            this.tabPageAdvanced.Name = "tabPageAdvanced";
            this.tabPageAdvanced.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAdvanced.Size = new System.Drawing.Size(792, 574);
            this.tabPageAdvanced.TabIndex = 1;
            this.tabPageAdvanced.Text = "Advanced";
            this.tabPageAdvanced.UseVisualStyleBackColor = true;
            // 
            // ucSINnersUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.Controls.Add(this.tabSINner);
            this.Name = "ucSINnersUserControl";
            this.Size = new System.Drawing.Size(800, 600);
            this.tabSINner.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabSINner;
        private System.Windows.Forms.TabPage tabPageBasic;
        private System.Windows.Forms.TabPage tabPageAdvanced;
    }
}
