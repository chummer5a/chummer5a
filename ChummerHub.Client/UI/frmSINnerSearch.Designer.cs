namespace ChummerHub.Client.UI
{
    partial class frmSINnerSearch
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
            this.tabSearch = new System.Windows.Forms.TabControl();
            this.tabSearchPage = new System.Windows.Forms.TabPage();
            this.tabResultPage = new System.Windows.Forms.TabPage();
            this.ucSearch1 = new ChummerHub.Client.UI.ucSearch();
            this.tabSearch.SuspendLayout();
            this.tabSearchPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabSearch
            // 
            this.tabSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabSearch.Controls.Add(this.tabSearchPage);
            this.tabSearch.Controls.Add(this.tabResultPage);
            this.tabSearch.Location = new System.Drawing.Point(1, 1);
            this.tabSearch.Name = "tabSearch";
            this.tabSearch.SelectedIndex = 0;
            this.tabSearch.Size = new System.Drawing.Size(807, 468);
            this.tabSearch.TabIndex = 0;
            // 
            // tabSearchPage
            // 
            this.tabSearchPage.Controls.Add(this.ucSearch1);
            this.tabSearchPage.Location = new System.Drawing.Point(4, 22);
            this.tabSearchPage.Name = "tabSearchPage";
            this.tabSearchPage.Padding = new System.Windows.Forms.Padding(3);
            this.tabSearchPage.Size = new System.Drawing.Size(799, 442);
            this.tabSearchPage.TabIndex = 0;
            this.tabSearchPage.Text = "Search";
            this.tabSearchPage.UseVisualStyleBackColor = true;
            // 
            // tabResultPage
            // 
            this.tabResultPage.Location = new System.Drawing.Point(4, 22);
            this.tabResultPage.Name = "tabResultPage";
            this.tabResultPage.Padding = new System.Windows.Forms.Padding(3);
            this.tabResultPage.Size = new System.Drawing.Size(906, 611);
            this.tabResultPage.TabIndex = 1;
            this.tabResultPage.Text = "Result";
            this.tabResultPage.UseVisualStyleBackColor = true;
            // 
            // ucSearch1
            // 
            this.ucSearch1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ucSearch1.Location = new System.Drawing.Point(8, 7);
            this.ucSearch1.Name = "ucSearch1";
            this.ucSearch1.Size = new System.Drawing.Size(785, 429);
            this.ucSearch1.TabIndex = 0;
            // 
            // frmSINnerSearch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(820, 481);
            this.Controls.Add(this.tabSearch);
            this.Name = "frmSINnerSearch";
            this.Text = "frmSINnerSearch";
            this.tabSearch.ResumeLayout(false);
            this.tabSearchPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabSearch;
        private System.Windows.Forms.TabPage tabSearchPage;
        private System.Windows.Forms.TabPage tabResultPage;
        private ucSearch ucSearch1;
    }
}