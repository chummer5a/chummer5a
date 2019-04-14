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
            this.label1 = new System.Windows.Forms.Label();
            this.lGourpForSinner = new System.Windows.Forms.Label();
            this.bGroupSearch = new System.Windows.Forms.Button();
            this.lUploadStatus = new System.Windows.Forms.Label();
            this.lStatuslabel = new System.Windows.Forms.Label();
            this.bUpload = new System.Windows.Forms.Button();
            this.gpTags = new System.Windows.Forms.GroupBox();
            this.tlpTags = new System.Windows.Forms.TableLayoutPanel();
            this.cbTagSRM_ready = new System.Windows.Forms.CheckBox();
            this.cbTagCustom = new System.Windows.Forms.CheckBox();
            this.TagValueCustomName = new System.Windows.Forms.TextBox();
            this.cbTagArchetype = new System.Windows.Forms.CheckBox();
            this.TagValueArchetype = new System.Windows.Forms.ComboBox();
            this.cbTagPowerRating = new System.Windows.Forms.CheckBox();
            this.cbTagIsNPC = new System.Windows.Forms.CheckBox();
            this.TagValuePowerRating = new System.Windows.Forms.NumericUpDown();
            this.tabLayoutPanel.SuspendLayout();
            this.gpTags.SuspendLayout();
            this.tlpTags.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TagValuePowerRating)).BeginInit();
            this.SuspendLayout();
            // 
            // tabLayoutPanel
            // 
            this.tabLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tabLayoutPanel.ColumnCount = 3;
            this.tabLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tabLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tabLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tabLayoutPanel.Controls.Add(this.label1, 0, 1);
            this.tabLayoutPanel.Controls.Add(this.lGourpForSinner, 1, 1);
            this.tabLayoutPanel.Controls.Add(this.bGroupSearch, 2, 1);
            this.tabLayoutPanel.Controls.Add(this.lUploadStatus, 1, 0);
            this.tabLayoutPanel.Controls.Add(this.lStatuslabel, 0, 0);
            this.tabLayoutPanel.Controls.Add(this.bUpload, 2, 0);
            this.tabLayoutPanel.Controls.Add(this.gpTags, 0, 2);
            this.tabLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tabLayoutPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabLayoutPanel.Name = "tabLayoutPanel";
            this.tabLayoutPanel.RowCount = 3;
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
            this.tabLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
            this.tabLayoutPanel.Size = new System.Drawing.Size(450, 455);
            this.tabLayoutPanel.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 51);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 8, 4, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Group:";
            // 
            // lGourpForSinner
            // 
            this.lGourpForSinner.AutoSize = true;
            this.lGourpForSinner.Dock = System.Windows.Forms.DockStyle.Left;
            this.lGourpForSinner.Location = new System.Drawing.Point(72, 41);
            this.lGourpForSinner.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lGourpForSinner.Name = "lGourpForSinner";
            this.lGourpForSinner.Size = new System.Drawing.Size(136, 40);
            this.lGourpForSinner.TabIndex = 6;
            this.lGourpForSinner.Text = "no group selected";
            this.lGourpForSinner.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // bGroupSearch
            // 
            this.bGroupSearch.AutoSize = true;
            this.bGroupSearch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bGroupSearch.Dock = System.Windows.Forms.DockStyle.Left;
            this.bGroupSearch.Location = new System.Drawing.Point(216, 46);
            this.bGroupSearch.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.bGroupSearch.Name = "bGroupSearch";
            this.bGroupSearch.Size = new System.Drawing.Size(124, 30);
            this.bGroupSearch.TabIndex = 7;
            this.bGroupSearch.Text = "search Groups";
            this.bGroupSearch.UseVisualStyleBackColor = true;
            this.bGroupSearch.Click += new System.EventHandler(this.bGroupSearch_Click);
            // 
            // lUploadStatus
            // 
            this.lUploadStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lUploadStatus.AutoSize = true;
            this.lUploadStatus.Location = new System.Drawing.Point(72, 10);
            this.lUploadStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lUploadStatus.Name = "lUploadStatus";
            this.lUploadStatus.Size = new System.Drawing.Size(136, 20);
            this.lUploadStatus.TabIndex = 10;
            this.lUploadStatus.Text = "unkown";
            this.lUploadStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lStatuslabel
            // 
            this.lStatuslabel.AutoSize = true;
            this.lStatuslabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.lStatuslabel.Location = new System.Drawing.Point(4, 0);
            this.lStatuslabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lStatuslabel.Name = "lStatuslabel";
            this.lStatuslabel.Size = new System.Drawing.Size(60, 41);
            this.lStatuslabel.TabIndex = 11;
            this.lStatuslabel.Text = "Status:";
            this.lStatuslabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // bUpload
            // 
            this.bUpload.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.bUpload.Enabled = false;
            this.bUpload.Location = new System.Drawing.Point(216, 5);
            this.bUpload.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.bUpload.Name = "bUpload";
            this.bUpload.Size = new System.Drawing.Size(112, 31);
            this.bUpload.TabIndex = 5;
            this.bUpload.Text = "Upload";
            this.bUpload.UseVisualStyleBackColor = true;
            this.bUpload.Click += new System.EventHandler(this.bUpload_Click);
            // 
            // gpTags
            // 
            this.gpTags.AutoSize = true;
            this.gpTags.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tabLayoutPanel.SetColumnSpan(this.gpTags, 3);
            this.gpTags.Controls.Add(this.tlpTags);
            this.gpTags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpTags.Location = new System.Drawing.Point(4, 86);
            this.gpTags.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gpTags.Name = "gpTags";
            this.gpTags.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gpTags.Size = new System.Drawing.Size(442, 364);
            this.gpTags.TabIndex = 12;
            this.gpTags.TabStop = false;
            this.gpTags.Text = "Tags";
            // 
            // tlpTags
            // 
            this.tlpTags.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpTags.ColumnCount = 2;
            this.tlpTags.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpTags.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTags.Controls.Add(this.cbTagSRM_ready, 0, 1);
            this.tlpTags.Controls.Add(this.cbTagCustom, 0, 4);
            this.tlpTags.Controls.Add(this.TagValueCustomName, 1, 4);
            this.tlpTags.Controls.Add(this.cbTagArchetype, 0, 2);
            this.tlpTags.Controls.Add(this.TagValueArchetype, 1, 2);
            this.tlpTags.Controls.Add(this.cbTagPowerRating, 0, 3);
            this.tlpTags.Controls.Add(this.cbTagIsNPC, 0, 0);
            this.tlpTags.Controls.Add(this.TagValuePowerRating, 1, 3);
            this.tlpTags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTags.Location = new System.Drawing.Point(4, 24);
            this.tlpTags.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tlpTags.Name = "tlpTags";
            this.tlpTags.RowCount = 5;
            this.tlpTags.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTags.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTags.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTags.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTags.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTags.Size = new System.Drawing.Size(434, 335);
            this.tlpTags.TabIndex = 0;
            // 
            // cbTagSRM_ready
            // 
            this.cbTagSRM_ready.AutoSize = true;
            this.cbTagSRM_ready.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbTagSRM_ready.Location = new System.Drawing.Point(8, 48);
            this.cbTagSRM_ready.Margin = new System.Windows.Forms.Padding(8);
            this.cbTagSRM_ready.Name = "cbTagSRM_ready";
            this.cbTagSRM_ready.Size = new System.Drawing.Size(123, 24);
            this.cbTagSRM_ready.TabIndex = 10;
            this.cbTagSRM_ready.Text = "SRM ready";
            this.cbTagSRM_ready.UseVisualStyleBackColor = true;
            this.cbTagSRM_ready.CheckedChanged += new System.EventHandler(this.OnGroupBoxTagsClick);
            // 
            // cbTagCustom
            // 
            this.cbTagCustom.AutoSize = true;
            this.cbTagCustom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbTagCustom.Location = new System.Drawing.Point(8, 168);
            this.cbTagCustom.Margin = new System.Windows.Forms.Padding(8);
            this.cbTagCustom.Name = "cbTagCustom";
            this.cbTagCustom.Size = new System.Drawing.Size(123, 159);
            this.cbTagCustom.TabIndex = 8;
            this.cbTagCustom.Text = "Custom";
            this.cbTagCustom.UseVisualStyleBackColor = true;
            this.cbTagCustom.CheckedChanged += new System.EventHandler(this.OnGroupBoxTagsClick);
            // 
            // TagValueCustomName
            // 
            this.TagValueCustomName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.TagValueCustomName.Location = new System.Drawing.Point(143, 234);
            this.TagValueCustomName.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TagValueCustomName.Name = "TagValueCustomName";
            this.TagValueCustomName.Size = new System.Drawing.Size(287, 26);
            this.TagValueCustomName.TabIndex = 9;
            this.TagValueCustomName.TextChanged += new System.EventHandler(this.OnGroupBoxTagsClick);
            // 
            // cbTagArchetype
            // 
            this.cbTagArchetype.AutoSize = true;
            this.cbTagArchetype.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbTagArchetype.Location = new System.Drawing.Point(8, 88);
            this.cbTagArchetype.Margin = new System.Windows.Forms.Padding(8);
            this.cbTagArchetype.Name = "cbTagArchetype";
            this.cbTagArchetype.Size = new System.Drawing.Size(123, 24);
            this.cbTagArchetype.TabIndex = 11;
            this.cbTagArchetype.Text = "Archetype";
            this.cbTagArchetype.UseVisualStyleBackColor = true;
            this.cbTagArchetype.CheckedChanged += new System.EventHandler(this.OnGroupBoxTagsClick);
            // 
            // TagValueArchetype
            // 
            this.TagValueArchetype.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.TagValueArchetype.FormattingEnabled = true;
            this.TagValueArchetype.Location = new System.Drawing.Point(143, 86);
            this.TagValueArchetype.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TagValueArchetype.Name = "TagValueArchetype";
            this.TagValueArchetype.Size = new System.Drawing.Size(287, 28);
            this.TagValueArchetype.TabIndex = 12;
            this.TagValueArchetype.SelectedIndexChanged += new System.EventHandler(this.OnGroupBoxTagsClick);
            // 
            // cbTagPowerRating
            // 
            this.cbTagPowerRating.AutoSize = true;
            this.cbTagPowerRating.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbTagPowerRating.Location = new System.Drawing.Point(8, 128);
            this.cbTagPowerRating.Margin = new System.Windows.Forms.Padding(8);
            this.cbTagPowerRating.Name = "cbTagPowerRating";
            this.cbTagPowerRating.Size = new System.Drawing.Size(123, 24);
            this.cbTagPowerRating.TabIndex = 13;
            this.cbTagPowerRating.Text = "Power rating";
            this.cbTagPowerRating.UseVisualStyleBackColor = true;
            this.cbTagPowerRating.CheckedChanged += new System.EventHandler(this.OnGroupBoxTagsClick);
            // 
            // cbTagIsNPC
            // 
            this.cbTagIsNPC.AutoSize = true;
            this.cbTagIsNPC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbTagIsNPC.Location = new System.Drawing.Point(8, 8);
            this.cbTagIsNPC.Margin = new System.Windows.Forms.Padding(8);
            this.cbTagIsNPC.Name = "cbTagIsNPC";
            this.cbTagIsNPC.Size = new System.Drawing.Size(123, 24);
            this.cbTagIsNPC.TabIndex = 16;
            this.cbTagIsNPC.Text = "Is NPC";
            this.cbTagIsNPC.UseVisualStyleBackColor = true;
            this.cbTagIsNPC.CheckedChanged += new System.EventHandler(this.OnGroupBoxTagsClick);
            // 
            // TagValuePowerRating
            // 
            this.TagValuePowerRating.Dock = System.Windows.Forms.DockStyle.Left;
            this.TagValuePowerRating.Location = new System.Drawing.Point(143, 125);
            this.TagValuePowerRating.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TagValuePowerRating.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.TagValuePowerRating.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.TagValuePowerRating.Name = "TagValuePowerRating";
            this.TagValuePowerRating.Size = new System.Drawing.Size(56, 26);
            this.TagValuePowerRating.TabIndex = 17;
            this.TagValuePowerRating.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.TagValuePowerRating.ValueChanged += new System.EventHandler(this.OnGroupBoxTagsClick);
            // 
            // SINnersBasic
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tabLayoutPanel);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "SINnersBasic";
            this.Size = new System.Drawing.Size(454, 460);
            this.tabLayoutPanel.ResumeLayout(false);
            this.tabLayoutPanel.PerformLayout();
            this.gpTags.ResumeLayout(false);
            this.tlpTags.ResumeLayout(false);
            this.tlpTags.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TagValuePowerRating)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tabLayoutPanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button bUpload;
        private System.Windows.Forms.Label lGourpForSinner;
        private System.Windows.Forms.Button bGroupSearch;

        private System.Windows.Forms.Label lUploadStatus;
        private System.Windows.Forms.Label lStatuslabel;
        private System.Windows.Forms.GroupBox gpTags;
        private System.Windows.Forms.TableLayoutPanel tlpTags;
        private System.Windows.Forms.CheckBox cbTagSRM_ready;
        private System.Windows.Forms.CheckBox cbTagCustom;
        private System.Windows.Forms.TextBox TagValueCustomName;
        private System.Windows.Forms.CheckBox cbTagArchetype;
        private System.Windows.Forms.ComboBox TagValueArchetype;
        private System.Windows.Forms.CheckBox cbTagPowerRating;
        private System.Windows.Forms.CheckBox cbTagIsNPC;
        private System.Windows.Forms.NumericUpDown TagValuePowerRating;
    }
}
