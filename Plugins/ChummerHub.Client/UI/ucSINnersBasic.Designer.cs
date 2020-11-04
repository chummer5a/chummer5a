namespace ChummerHub.Client.UI
{
    partial class ucSINnersBasic
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
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
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
            this.bGenerateNewId = new System.Windows.Forms.Button();
            this.bVisibility = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tbID = new System.Windows.Forms.TextBox();
            this.tlpMain.SuspendLayout();
            this.gpTags.SuspendLayout();
            this.tlpTags.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TagValuePowerRating)).BeginInit();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 3;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.label1, 0, 1);
            this.tlpMain.Controls.Add(this.lGourpForSinner, 1, 1);
            this.tlpMain.Controls.Add(this.bGroupSearch, 2, 1);
            this.tlpMain.Controls.Add(this.lUploadStatus, 1, 0);
            this.tlpMain.Controls.Add(this.lStatuslabel, 0, 0);
            this.tlpMain.Controls.Add(this.bUpload, 2, 0);
            this.tlpMain.Controls.Add(this.gpTags, 0, 4);
            this.tlpMain.Controls.Add(this.bGenerateNewId, 2, 3);
            this.tlpMain.Controls.Add(this.bVisibility, 1, 2);
            this.tlpMain.Controls.Add(this.label2, 0, 3);
            this.tlpMain.Controls.Add(this.tbID, 1, 3);
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 5;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(302, 266);
            this.tlpMain.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 37);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Group:";
            // 
            // lGourpForSinner
            // 
            this.lGourpForSinner.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lGourpForSinner.AutoSize = true;
            this.lGourpForSinner.Location = new System.Drawing.Point(49, 37);
            this.lGourpForSinner.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lGourpForSinner.Name = "lGourpForSinner";
            this.lGourpForSinner.Size = new System.Drawing.Size(92, 13);
            this.lGourpForSinner.TabIndex = 6;
            this.lGourpForSinner.Text = "no group selected";
            this.lGourpForSinner.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // bGroupSearch
            // 
            this.bGroupSearch.AutoSize = true;
            this.bGroupSearch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bGroupSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bGroupSearch.Location = new System.Drawing.Point(211, 32);
            this.bGroupSearch.Name = "bGroupSearch";
            this.bGroupSearch.Size = new System.Drawing.Size(88, 23);
            this.bGroupSearch.TabIndex = 7;
            this.bGroupSearch.Text = "Search Groups";
            this.bGroupSearch.UseVisualStyleBackColor = true;
            this.bGroupSearch.Click += new System.EventHandler(this.bGroupSearch_Click);
            // 
            // lUploadStatus
            // 
            this.lUploadStatus.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lUploadStatus.AutoSize = true;
            this.lUploadStatus.Location = new System.Drawing.Point(49, 8);
            this.lUploadStatus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lUploadStatus.Name = "lUploadStatus";
            this.lUploadStatus.Size = new System.Drawing.Size(45, 13);
            this.lUploadStatus.TabIndex = 10;
            this.lUploadStatus.Text = "unkown";
            this.lUploadStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lStatuslabel
            // 
            this.lStatuslabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lStatuslabel.AutoSize = true;
            this.lStatuslabel.Location = new System.Drawing.Point(3, 8);
            this.lStatuslabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lStatuslabel.Name = "lStatuslabel";
            this.lStatuslabel.Size = new System.Drawing.Size(40, 13);
            this.lStatuslabel.TabIndex = 11;
            this.lStatuslabel.Text = "Status:";
            this.lStatuslabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // bUpload
            // 
            this.bUpload.AutoSize = true;
            this.bUpload.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bUpload.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bUpload.Enabled = false;
            this.bUpload.Location = new System.Drawing.Point(211, 3);
            this.bUpload.Name = "bUpload";
            this.bUpload.Size = new System.Drawing.Size(88, 23);
            this.bUpload.TabIndex = 5;
            this.bUpload.Text = "Upload";
            this.bUpload.UseVisualStyleBackColor = true;
            this.bUpload.Click += new System.EventHandler(this.bUpload_Click);
            // 
            // gpTags
            // 
            this.gpTags.AutoSize = true;
            this.gpTags.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.SetColumnSpan(this.gpTags, 3);
            this.gpTags.Controls.Add(this.tlpTags);
            this.gpTags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpTags.Location = new System.Drawing.Point(3, 119);
            this.gpTags.Name = "gpTags";
            this.gpTags.Size = new System.Drawing.Size(296, 144);
            this.gpTags.TabIndex = 12;
            this.gpTags.TabStop = false;
            this.gpTags.Text = "Tags";
            // 
            // tlpTags
            // 
            this.tlpTags.AutoSize = true;
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
            this.tlpTags.Location = new System.Drawing.Point(3, 16);
            this.tlpTags.Name = "tlpTags";
            this.tlpTags.RowCount = 5;
            this.tlpTags.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTags.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTags.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTags.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTags.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTags.Size = new System.Drawing.Size(290, 125);
            this.tlpTags.TabIndex = 0;
            this.tlpTags.MouseLeave += new System.EventHandler(this.TlpTags_MouseLeave);
            // 
            // cbTagSRM_ready
            // 
            this.cbTagSRM_ready.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cbTagSRM_ready.AutoSize = true;
            this.cbTagSRM_ready.Location = new System.Drawing.Point(3, 26);
            this.cbTagSRM_ready.Name = "cbTagSRM_ready";
            this.cbTagSRM_ready.Size = new System.Drawing.Size(84, 17);
            this.cbTagSRM_ready.TabIndex = 10;
            this.cbTagSRM_ready.Text = "SRM Ready";
            this.cbTagSRM_ready.UseVisualStyleBackColor = true;
            // 
            // cbTagCustom
            // 
            this.cbTagCustom.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cbTagCustom.AutoSize = true;
            this.cbTagCustom.Location = new System.Drawing.Point(3, 103);
            this.cbTagCustom.Name = "cbTagCustom";
            this.cbTagCustom.Size = new System.Drawing.Size(61, 17);
            this.cbTagCustom.TabIndex = 8;
            this.cbTagCustom.Text = "Custom";
            this.cbTagCustom.UseVisualStyleBackColor = true;
            // 
            // TagValueCustomName
            // 
            this.TagValueCustomName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.TagValueCustomName.Location = new System.Drawing.Point(99, 102);
            this.TagValueCustomName.Name = "TagValueCustomName";
            this.TagValueCustomName.Size = new System.Drawing.Size(188, 20);
            this.TagValueCustomName.TabIndex = 9;
            // 
            // cbTagArchetype
            // 
            this.cbTagArchetype.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cbTagArchetype.AutoSize = true;
            this.cbTagArchetype.Location = new System.Drawing.Point(3, 51);
            this.cbTagArchetype.Name = "cbTagArchetype";
            this.cbTagArchetype.Size = new System.Drawing.Size(74, 17);
            this.cbTagArchetype.TabIndex = 11;
            this.cbTagArchetype.Text = "Archetype";
            this.cbTagArchetype.UseVisualStyleBackColor = true;
            // 
            // TagValueArchetype
            // 
            this.TagValueArchetype.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.TagValueArchetype.FormattingEnabled = true;
            this.TagValueArchetype.Location = new System.Drawing.Point(99, 49);
            this.TagValueArchetype.Name = "TagValueArchetype";
            this.TagValueArchetype.Size = new System.Drawing.Size(188, 21);
            this.TagValueArchetype.TabIndex = 12;
            // 
            // cbTagPowerRating
            // 
            this.cbTagPowerRating.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cbTagPowerRating.AutoSize = true;
            this.cbTagPowerRating.Location = new System.Drawing.Point(3, 77);
            this.cbTagPowerRating.Name = "cbTagPowerRating";
            this.cbTagPowerRating.Size = new System.Drawing.Size(90, 17);
            this.cbTagPowerRating.TabIndex = 13;
            this.cbTagPowerRating.Text = "Power Rating";
            this.cbTagPowerRating.UseVisualStyleBackColor = true;
            // 
            // cbTagIsNPC
            // 
            this.cbTagIsNPC.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cbTagIsNPC.AutoSize = true;
            this.cbTagIsNPC.Location = new System.Drawing.Point(3, 3);
            this.cbTagIsNPC.Name = "cbTagIsNPC";
            this.cbTagIsNPC.Size = new System.Drawing.Size(59, 17);
            this.cbTagIsNPC.TabIndex = 16;
            this.cbTagIsNPC.Text = "Is NPC";
            this.cbTagIsNPC.UseVisualStyleBackColor = true;
            // 
            // TagValuePowerRating
            // 
            this.TagValuePowerRating.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.TagValuePowerRating.AutoSize = true;
            this.TagValuePowerRating.Location = new System.Drawing.Point(99, 76);
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
            this.TagValuePowerRating.Size = new System.Drawing.Size(29, 20);
            this.TagValuePowerRating.TabIndex = 17;
            this.TagValuePowerRating.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.TagValuePowerRating.ValueChanged += new System.EventHandler(this.OnGroupBoxTagsClick);
            // 
            // bGenerateNewId
            // 
            this.bGenerateNewId.AutoSize = true;
            this.bGenerateNewId.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bGenerateNewId.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bGenerateNewId.Location = new System.Drawing.Point(211, 90);
            this.bGenerateNewId.Name = "bGenerateNewId";
            this.bGenerateNewId.Size = new System.Drawing.Size(88, 23);
            this.bGenerateNewId.TabIndex = 14;
            this.bGenerateNewId.Text = "New Id";
            this.bGenerateNewId.UseVisualStyleBackColor = true;
            this.bGenerateNewId.Click += new System.EventHandler(this.BGenerateNewId_Click);
            // 
            // bVisibility
            // 
            this.bVisibility.AutoSize = true;
            this.bVisibility.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bVisibility.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bVisibility.Location = new System.Drawing.Point(49, 61);
            this.bVisibility.Name = "bVisibility";
            this.bVisibility.Size = new System.Drawing.Size(156, 23);
            this.bVisibility.TabIndex = 13;
            this.bVisibility.Text = "Visibility";
            this.bVisibility.UseVisualStyleBackColor = true;
            this.bVisibility.Click += new System.EventHandler(this.BVisibility_Click);
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 95);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(19, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Id:";
            // 
            // tbID
            // 
            this.tbID.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbID.Location = new System.Drawing.Point(49, 91);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(156, 20);
            this.tbID.TabIndex = 16;
            // 
            // ucSINnersBasic
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScroll = true;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tlpMain);
            this.Name = "ucSINnersBasic";
            this.Size = new System.Drawing.Size(305, 269);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.gpTags.ResumeLayout(false);
            this.gpTags.PerformLayout();
            this.tlpTags.ResumeLayout(false);
            this.tlpTags.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TagValuePowerRating)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpMain;
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
        private System.Windows.Forms.Button bVisibility;
        private System.Windows.Forms.Button bGenerateNewId;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbID;
    }
}
