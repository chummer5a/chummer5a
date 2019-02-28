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
            this.bUpload = new System.Windows.Forms.Button();
            this.cbSRMReady = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lGourpForSinner = new System.Windows.Forms.Label();
            this.bGroupSearch = new System.Windows.Forms.Button();
            this.cbTagArchetype = new System.Windows.Forms.CheckBox();
            this.tbArchetypeName = new System.Windows.Forms.TextBox();
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
            this.tabLayoutPanel.Controls.Add(this.cbSRMReady, 0, 0);
            this.tabLayoutPanel.Controls.Add(this.label1, 0, 1);
            this.tabLayoutPanel.Controls.Add(this.lGourpForSinner, 1, 1);
            this.tabLayoutPanel.Controls.Add(this.bGroupSearch, 2, 1);
            this.tabLayoutPanel.Controls.Add(this.bUpload, 2, 4);
            this.tabLayoutPanel.Controls.Add(this.tbArchetypeName, 1, 2);
            this.tabLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tabLayoutPanel.Name = "tabLayoutPanel";
            this.tabLayoutPanel.RowCount = 6;
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tabLayoutPanel.Size = new System.Drawing.Size(409, 210);
            this.tabLayoutPanel.TabIndex = 0;
            // 
            // bUpload
            // 
            this.bUpload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.tabLayoutPanel.SetColumnSpan(this.bUpload, 3);
            this.bUpload.Enabled = false;
            this.bUpload.Location = new System.Drawing.Point(167, 187);
            this.bUpload.Name = "bUpload";
            this.bUpload.Size = new System.Drawing.Size(75, 20);
            this.bUpload.TabIndex = 5;
            this.bUpload.Text = "Upload";
            this.bUpload.UseVisualStyleBackColor = true;
            this.bUpload.Click += new System.EventHandler(this.bUpload_Click);
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
            this.label1.Location = new System.Drawing.Point(3, 31);
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
            this.lGourpForSinner.Location = new System.Drawing.Point(88, 23);
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
            this.bGroupSearch.Location = new System.Drawing.Point(186, 26);
            this.bGroupSearch.Name = "bGroupSearch";
            this.bGroupSearch.Size = new System.Drawing.Size(86, 23);
            this.bGroupSearch.TabIndex = 7;
            this.bGroupSearch.Text = "search Groups";
            this.bGroupSearch.UseVisualStyleBackColor = true;
            this.bGroupSearch.Click += new System.EventHandler(this.bGroupSearch_Click);
            // 
            // cbTagArchetype
            // 
            this.cbTagArchetype.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cbTagArchetype.AutoSize = true;
            this.cbTagArchetype.Location = new System.Drawing.Point(3, 99);
            this.cbTagArchetype.Name = "cbTagArchetype";
            this.cbTagArchetype.Size = new System.Drawing.Size(74, 17);
            this.cbTagArchetype.TabIndex = 8;
            this.cbTagArchetype.Text = "Archetype";
            this.cbTagArchetype.UseVisualStyleBackColor = true;
            this.cbTagArchetype.Click += new System.EventHandler(this.cbTagArchetype_Click);
            // 
            // tbArchetypeName
            // 
            this.tbArchetypeName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tabLayoutPanel.SetColumnSpan(this.tbArchetypeName, 2);
            this.tbArchetypeName.Location = new System.Drawing.Point(88, 98);
            this.tbArchetypeName.Name = "tbArchetypeName";
            this.tbArchetypeName.Size = new System.Drawing.Size(318, 20);
            this.tbArchetypeName.TabIndex = 9;
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
    }
}
