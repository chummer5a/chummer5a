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
            this.cbTagArchetype = new System.Windows.Forms.CheckBox();
            this.cbSRMReady = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lGourpForSinner = new System.Windows.Forms.Label();
            this.bGroupSearch = new System.Windows.Forms.Button();
            this.bUpload = new System.Windows.Forms.Button();
            this.tbArchetypeName = new System.Windows.Forms.TextBox();
            this.lUploadStatus = new System.Windows.Forms.Label();
            this.lStatuslabel = new System.Windows.Forms.Label();
            this.tabLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabLayoutPanel
            // 
            this.tabLayoutPanel.ColumnCount = 3;
            this.tabLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tabLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tabLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tabLayoutPanel.Controls.Add(this.cbTagArchetype, 0, 2);
            this.tabLayoutPanel.Controls.Add(this.label1, 0, 1);
            this.tabLayoutPanel.Controls.Add(this.lGourpForSinner, 1, 1);
            this.tabLayoutPanel.Controls.Add(this.bGroupSearch, 2, 1);
            this.tabLayoutPanel.Controls.Add(this.tbArchetypeName, 1, 2);
            this.tabLayoutPanel.Controls.Add(this.lUploadStatus, 1, 0);
            this.tabLayoutPanel.Controls.Add(this.cbSRMReady, 0, 4);
            this.tabLayoutPanel.Controls.Add(this.lStatuslabel, 0, 0);
            this.tabLayoutPanel.Controls.Add(this.bUpload, 2, 0);
            this.tabLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tabLayoutPanel.Name = "tabLayoutPanel";
            this.tabLayoutPanel.RowCount = 6;
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayoutPanel.Size = new System.Drawing.Size(409, 210);
            this.tabLayoutPanel.TabIndex = 0;
            // 
            // cbTagArchetype
            // 
            this.cbTagArchetype.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cbTagArchetype.AutoSize = true;
            this.cbTagArchetype.Location = new System.Drawing.Point(3, 59);
            this.cbTagArchetype.Name = "cbTagArchetype";
            this.cbTagArchetype.Size = new System.Drawing.Size(74, 17);
            this.cbTagArchetype.TabIndex = 8;
            this.cbTagArchetype.Text = "Archetype";
            this.cbTagArchetype.UseVisualStyleBackColor = true;
            this.cbTagArchetype.Click += new System.EventHandler(this.cbTagArchetype_Click);
            // 
            // cbSRMReady
            // 
            this.cbSRMReady.AutoSize = true;
            this.cbSRMReady.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbSRMReady.Location = new System.Drawing.Point(5, 86);
            this.cbSRMReady.Margin = new System.Windows.Forms.Padding(5);
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
            this.label1.Location = new System.Drawing.Point(3, 34);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Group:";
            // 
            // lGourpForSinner
            // 
            this.lGourpForSinner.AutoSize = true;
            this.lGourpForSinner.Dock = System.Windows.Forms.DockStyle.Left;
            this.lGourpForSinner.Location = new System.Drawing.Point(92, 26);
            this.lGourpForSinner.Name = "lGourpForSinner";
            this.lGourpForSinner.Size = new System.Drawing.Size(92, 29);
            this.lGourpForSinner.TabIndex = 6;
            this.lGourpForSinner.Text = "no group selected";
            this.lGourpForSinner.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // bGroupSearch
            // 
            this.bGroupSearch.AutoSize = true;
            this.bGroupSearch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bGroupSearch.Dock = System.Windows.Forms.DockStyle.Left;
            this.bGroupSearch.Location = new System.Drawing.Point(190, 29);
            this.bGroupSearch.Name = "bGroupSearch";
            this.bGroupSearch.Size = new System.Drawing.Size(86, 23);
            this.bGroupSearch.TabIndex = 7;
            this.bGroupSearch.Text = "search Groups";
            this.bGroupSearch.UseVisualStyleBackColor = true;
            this.bGroupSearch.Click += new System.EventHandler(this.bGroupSearch_Click);
            // 
            // bUpload
            // 
            this.bUpload.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.bUpload.Enabled = false;
            this.bUpload.Location = new System.Drawing.Point(190, 3);
            this.bUpload.Name = "bUpload";
            this.bUpload.Size = new System.Drawing.Size(75, 20);
            this.bUpload.TabIndex = 5;
            this.bUpload.Text = "Upload";
            this.bUpload.UseVisualStyleBackColor = true;
            this.bUpload.Click += new System.EventHandler(this.bUpload_Click);
            // 
            // tbArchetypeName
            // 
            this.tbArchetypeName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tabLayoutPanel.SetColumnSpan(this.tbArchetypeName, 2);
            this.tbArchetypeName.Location = new System.Drawing.Point(92, 58);
            this.tbArchetypeName.Name = "tbArchetypeName";
            this.tbArchetypeName.Size = new System.Drawing.Size(318, 20);
            this.tbArchetypeName.TabIndex = 9;
            // 
            // lUploadStatus
            // 
            this.lUploadStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lUploadStatus.AutoSize = true;
            this.lUploadStatus.Location = new System.Drawing.Point(92, 6);
            this.lUploadStatus.Name = "lUploadStatus";
            this.lUploadStatus.Size = new System.Drawing.Size(92, 13);
            this.lUploadStatus.TabIndex = 10;
            this.lUploadStatus.Text = "unkown";
            this.lUploadStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lStatuslabel
            // 
            this.lStatuslabel.AutoSize = true;
            this.lStatuslabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.lStatuslabel.Location = new System.Drawing.Point(3, 0);
            this.lStatuslabel.Name = "lStatuslabel";
            this.lStatuslabel.Size = new System.Drawing.Size(40, 26);
            this.lStatuslabel.TabIndex = 11;
            this.lStatuslabel.Text = "Status:";
            this.lStatuslabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.Size = new System.Drawing.Size(412, 213);
            this.tabLayoutPanel.ResumeLayout(false);
            this.tabLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tabLayoutPanel;
        private System.Windows.Forms.CheckBox cbSRMReady;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button bUpload;
        private System.Windows.Forms.Label lGourpForSinner;
        private System.Windows.Forms.Button bGroupSearch;
        private System.Windows.Forms.CheckBox cbTagArchetype;
        private System.Windows.Forms.TextBox tbArchetypeName;
        private System.Windows.Forms.Label lUploadStatus;
        private System.Windows.Forms.Label lStatuslabel;
    }
}
