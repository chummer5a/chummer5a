namespace ChummerHub.Client.UI
{
    partial class ucSINnersAdvanced
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
            this.groupBox_Tags = new System.Windows.Forms.GroupBox();
            this.MyTagTreeView = new System.Windows.Forms.TreeView();
            this.cmdPopulateTags = new System.Windows.Forms.Button();
            this.cmdPrepareModel = new System.Windows.Forms.Button();
            this.cmdPostSINnerMetadata = new System.Windows.Forms.Button();
            this.cmdUploadChummerFile = new System.Windows.Forms.Button();
            this.labelSINnerUrl = new System.Windows.Forms.Label();
            this.cbSINnerUrl = new System.Windows.Forms.ComboBox();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.flpButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.tlpUrl = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox_Tags.SuspendLayout();
            this.tlpMain.SuspendLayout();
            this.flpButtons.SuspendLayout();
            this.tlpUrl.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox_Tags
            // 
            this.groupBox_Tags.AutoSize = true;
            this.groupBox_Tags.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox_Tags.Controls.Add(this.MyTagTreeView);
            this.groupBox_Tags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox_Tags.Location = new System.Drawing.Point(3, 3);
            this.groupBox_Tags.Name = "groupBox_Tags";
            this.tlpMain.SetRowSpan(this.groupBox_Tags, 2);
            this.groupBox_Tags.Size = new System.Drawing.Size(323, 137);
            this.groupBox_Tags.TabIndex = 1;
            this.groupBox_Tags.TabStop = false;
            this.groupBox_Tags.Text = "Tags";
            // 
            // MyTagTreeView
            // 
            this.MyTagTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MyTagTreeView.Location = new System.Drawing.Point(3, 16);
            this.MyTagTreeView.Name = "MyTagTreeView";
            this.MyTagTreeView.Size = new System.Drawing.Size(317, 118);
            this.MyTagTreeView.TabIndex = 0;
            this.MyTagTreeView.VisibleChanged += new System.EventHandler(this.MyTagTreeView_VisibleChanged);
            // 
            // cmdPopulateTags
            // 
            this.cmdPopulateTags.AutoSize = true;
            this.cmdPopulateTags.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdPopulateTags.Dock = System.Windows.Forms.DockStyle.Top;
            this.cmdPopulateTags.Enabled = false;
            this.cmdPopulateTags.Location = new System.Drawing.Point(3, 3);
            this.cmdPopulateTags.Name = "cmdPopulateTags";
            this.cmdPopulateTags.Size = new System.Drawing.Size(88, 23);
            this.cmdPopulateTags.TabIndex = 2;
            this.cmdPopulateTags.Text = "Populate";
            this.cmdPopulateTags.UseVisualStyleBackColor = true;
            this.cmdPopulateTags.Click += new System.EventHandler(this.cmdPopulateTags_Click);
            // 
            // cmdPrepareModel
            // 
            this.cmdPrepareModel.AutoSize = true;
            this.cmdPrepareModel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdPrepareModel.Dock = System.Windows.Forms.DockStyle.Top;
            this.cmdPrepareModel.Enabled = false;
            this.cmdPrepareModel.Location = new System.Drawing.Point(3, 32);
            this.cmdPrepareModel.Name = "cmdPrepareModel";
            this.cmdPrepareModel.Size = new System.Drawing.Size(88, 23);
            this.cmdPrepareModel.TabIndex = 3;
            this.cmdPrepareModel.Text = "Prepare";
            this.cmdPrepareModel.UseVisualStyleBackColor = true;
            this.cmdPrepareModel.Click += new System.EventHandler(this.cmdPrepareModel_Click);
            // 
            // cmdPostSINnerMetadata
            // 
            this.cmdPostSINnerMetadata.AutoSize = true;
            this.cmdPostSINnerMetadata.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdPostSINnerMetadata.Dock = System.Windows.Forms.DockStyle.Top;
            this.cmdPostSINnerMetadata.Enabled = false;
            this.cmdPostSINnerMetadata.Location = new System.Drawing.Point(3, 61);
            this.cmdPostSINnerMetadata.Name = "cmdPostSINnerMetadata";
            this.cmdPostSINnerMetadata.Size = new System.Drawing.Size(88, 23);
            this.cmdPostSINnerMetadata.TabIndex = 4;
            this.cmdPostSINnerMetadata.Text = "Post MetaData";
            this.cmdPostSINnerMetadata.UseVisualStyleBackColor = true;
            this.cmdPostSINnerMetadata.Click += new System.EventHandler(this.cmdPostSINnerMetaData_Click);
            // 
            // cmdUploadChummerFile
            // 
            this.cmdUploadChummerFile.AutoSize = true;
            this.cmdUploadChummerFile.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdUploadChummerFile.Dock = System.Windows.Forms.DockStyle.Top;
            this.cmdUploadChummerFile.Enabled = false;
            this.cmdUploadChummerFile.Location = new System.Drawing.Point(3, 90);
            this.cmdUploadChummerFile.Name = "cmdUploadChummerFile";
            this.cmdUploadChummerFile.Size = new System.Drawing.Size(88, 23);
            this.cmdUploadChummerFile.TabIndex = 5;
            this.cmdUploadChummerFile.Text = "Upload File";
            this.cmdUploadChummerFile.UseVisualStyleBackColor = true;
            this.cmdUploadChummerFile.Click += new System.EventHandler(this.cmdUploadChummerFile_Click);
            // 
            // labelSINnerUrl
            // 
            this.labelSINnerUrl.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelSINnerUrl.AutoSize = true;
            this.labelSINnerUrl.Location = new System.Drawing.Point(3, 7);
            this.labelSINnerUrl.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.labelSINnerUrl.Name = "labelSINnerUrl";
            this.labelSINnerUrl.Size = new System.Drawing.Size(23, 13);
            this.labelSINnerUrl.TabIndex = 7;
            this.labelSINnerUrl.Text = "Url:";
            this.labelSINnerUrl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cbSINnerUrl
            // 
            this.cbSINnerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cbSINnerUrl.FormattingEnabled = true;
            this.cbSINnerUrl.Items.AddRange(new object[] {
            "https://chummer.azurewebsites.net/",
            "https://localhost:5001/",
            "https://chummer-beta.azurewebsites.net/"});
            this.cbSINnerUrl.Location = new System.Drawing.Point(32, 3);
            this.cbSINnerUrl.Name = "cbSINnerUrl";
            this.cbSINnerUrl.Size = new System.Drawing.Size(294, 21);
            this.cbSINnerUrl.TabIndex = 9;
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.Controls.Add(this.groupBox_Tags, 0, 0);
            this.tlpMain.Controls.Add(this.flpButtons, 1, 1);
            this.tlpMain.Controls.Add(this.tlpUrl, 1, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(658, 143);
            this.tlpMain.TabIndex = 10;
            // 
            // flpButtons
            // 
            this.flpButtons.AutoSize = true;
            this.flpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpButtons.Controls.Add(this.cmdPopulateTags);
            this.flpButtons.Controls.Add(this.cmdPrepareModel);
            this.flpButtons.Controls.Add(this.cmdPostSINnerMetadata);
            this.flpButtons.Controls.Add(this.cmdUploadChummerFile);
            this.flpButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpButtons.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpButtons.Location = new System.Drawing.Point(329, 27);
            this.flpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.flpButtons.Name = "flpButtons";
            this.flpButtons.Size = new System.Drawing.Size(329, 116);
            this.flpButtons.TabIndex = 2;
            // 
            // tlpUrl
            // 
            this.tlpUrl.AutoSize = true;
            this.tlpUrl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpUrl.ColumnCount = 2;
            this.tlpUrl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpUrl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpUrl.Controls.Add(this.cbSINnerUrl, 1, 0);
            this.tlpUrl.Controls.Add(this.labelSINnerUrl, 0, 0);
            this.tlpUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpUrl.Location = new System.Drawing.Point(329, 0);
            this.tlpUrl.Margin = new System.Windows.Forms.Padding(0);
            this.tlpUrl.Name = "tlpUrl";
            this.tlpUrl.RowCount = 1;
            this.tlpUrl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpUrl.Size = new System.Drawing.Size(329, 27);
            this.tlpUrl.TabIndex = 3;
            // 
            // ucSINnersAdvanced
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tlpMain);
            this.Name = "ucSINnersAdvanced";
            this.Size = new System.Drawing.Size(658, 143);
            this.groupBox_Tags.ResumeLayout(false);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.flpButtons.ResumeLayout(false);
            this.flpButtons.PerformLayout();
            this.tlpUrl.ResumeLayout(false);
            this.tlpUrl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox_Tags;
        private System.Windows.Forms.Button cmdPopulateTags;
        private System.Windows.Forms.Button cmdPrepareModel;
        private System.Windows.Forms.Button cmdPostSINnerMetadata;
        private System.Windows.Forms.TreeView MyTagTreeView;
        private System.Windows.Forms.Button cmdUploadChummerFile;
        private System.Windows.Forms.Label labelSINnerUrl;
        public System.Windows.Forms.ComboBox cbSINnerUrl;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.FlowLayoutPanel flpButtons;
        private System.Windows.Forms.TableLayoutPanel tlpUrl;
    }
}
