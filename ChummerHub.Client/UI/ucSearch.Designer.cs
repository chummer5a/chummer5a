namespace ChummerHub.Client.UI
{
    partial class ucSearch
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
            this.bSearch = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.cLBavailableTags = new System.Windows.Forms.CheckedListBox();
            this.tvSearchTags = new System.Windows.Forms.TreeView();
            this.bAssignTags = new System.Windows.Forms.Button();
            this.propGirdTag = new System.Windows.Forms.PropertyGrid();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // bSearch
            // 
            this.bSearch.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.bSearch.Location = new System.Drawing.Point(554, 399);
            this.bSearch.Name = "bSearch";
            this.bSearch.Size = new System.Drawing.Size(75, 23);
            this.bSearch.TabIndex = 0;
            this.bSearch.Text = "button1";
            this.bSearch.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.bSearch, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.cLBavailableTags, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.tvSearchTags, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.bAssignTags, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.propGirdTag, 2, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(775, 425);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // cLBavailableTags
            // 
            this.cLBavailableTags.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cLBavailableTags.FormattingEnabled = true;
            this.cLBavailableTags.Location = new System.Drawing.Point(411, 3);
            this.cLBavailableTags.Name = "cLBavailableTags";
            this.cLBavailableTags.Size = new System.Drawing.Size(361, 184);
            this.cLBavailableTags.TabIndex = 1;
            // 
            // tvSearchTags
            // 
            this.tvSearchTags.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tvSearchTags.Location = new System.Drawing.Point(3, 3);
            this.tvSearchTags.Name = "tvSearchTags";
            this.tableLayoutPanel1.SetRowSpan(this.tvSearchTags, 2);
            this.tvSearchTags.Size = new System.Drawing.Size(361, 390);
            this.tvSearchTags.TabIndex = 2;
            // 
            // bAssignTags
            // 
            this.bAssignTags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.bAssignTags.Location = new System.Drawing.Point(370, 87);
            this.bAssignTags.Name = "bAssignTags";
            this.bAssignTags.Size = new System.Drawing.Size(35, 23);
            this.bAssignTags.TabIndex = 3;
            this.bAssignTags.Text = "<-";
            this.bAssignTags.UseVisualStyleBackColor = true;
            // 
            // propGirdTag
            // 
            this.propGirdTag.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propGirdTag.Location = new System.Drawing.Point(411, 201);
            this.propGirdTag.Name = "propGirdTag";
            this.propGirdTag.Size = new System.Drawing.Size(361, 192);
            this.propGirdTag.TabIndex = 4;
            // 
            // ucSearch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "ucSearch";
            this.Size = new System.Drawing.Size(781, 433);
            this.Load += new System.EventHandler(this.ucSearch_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button bSearch;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckedListBox cLBavailableTags;
        private System.Windows.Forms.TreeView tvSearchTags;
        private System.Windows.Forms.Button bAssignTags;
        private System.Windows.Forms.PropertyGrid propGirdTag;
    }
}
