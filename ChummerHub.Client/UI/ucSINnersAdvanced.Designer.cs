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
            this.groupBox_Tags.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox_Tags
            // 
            this.groupBox_Tags.Controls.Add(this.MyTagTreeView);
            this.groupBox_Tags.Location = new System.Drawing.Point(3, 3);
            this.groupBox_Tags.Name = "groupBox_Tags";
            this.groupBox_Tags.Size = new System.Drawing.Size(313, 443);
            this.groupBox_Tags.TabIndex = 1;
            this.groupBox_Tags.TabStop = false;
            this.groupBox_Tags.Text = "Tags";
            // 
            // MyTagTreeView
            // 
            this.MyTagTreeView.Location = new System.Drawing.Point(7, 20);
            this.MyTagTreeView.Name = "MyTagTreeView";
            this.MyTagTreeView.Size = new System.Drawing.Size(300, 417);
            this.MyTagTreeView.TabIndex = 0;
            this.MyTagTreeView.VisibleChanged += new System.EventHandler(this.MyTagTreeView_VisibleChanged);
            // 
            // cmdPopulateTags
            // 
            this.cmdPopulateTags.Enabled = false;
            this.cmdPopulateTags.Location = new System.Drawing.Point(322, 46);
            this.cmdPopulateTags.Name = "cmdPopulateTags";
            this.cmdPopulateTags.Size = new System.Drawing.Size(93, 23);
            this.cmdPopulateTags.TabIndex = 2;
            this.cmdPopulateTags.Text = "Populate";
            this.cmdPopulateTags.UseVisualStyleBackColor = true;
            this.cmdPopulateTags.Click += new System.EventHandler(this.cmdPopulateTags_Click);
            // 
            // cmdPrepareModel
            // 
            this.cmdPrepareModel.Enabled = false;
            this.cmdPrepareModel.Location = new System.Drawing.Point(323, 76);
            this.cmdPrepareModel.Name = "cmdPrepareModel";
            this.cmdPrepareModel.Size = new System.Drawing.Size(92, 23);
            this.cmdPrepareModel.TabIndex = 3;
            this.cmdPrepareModel.Text = "Prepare";
            this.cmdPrepareModel.UseVisualStyleBackColor = true;
            this.cmdPrepareModel.Click += new System.EventHandler(this.cmdPrepareModel_Click);
            // 
            // cmdPostSINnerMetadata
            // 
            this.cmdPostSINnerMetadata.Enabled = false;
            this.cmdPostSINnerMetadata.Location = new System.Drawing.Point(323, 106);
            this.cmdPostSINnerMetadata.Name = "cmdPostSINnerMetadata";
            this.cmdPostSINnerMetadata.Size = new System.Drawing.Size(92, 23);
            this.cmdPostSINnerMetadata.TabIndex = 4;
            this.cmdPostSINnerMetadata.Text = "Post MetaData";
            this.cmdPostSINnerMetadata.UseVisualStyleBackColor = true;
            this.cmdPostSINnerMetadata.Click += new System.EventHandler(this.cmdPostSINnerMetaData_Click);
            // 
            // cmdUploadChummerFile
            // 
            this.cmdUploadChummerFile.Enabled = false;
            this.cmdUploadChummerFile.Location = new System.Drawing.Point(323, 136);
            this.cmdUploadChummerFile.Name = "cmdUploadChummerFile";
            this.cmdUploadChummerFile.Size = new System.Drawing.Size(92, 23);
            this.cmdUploadChummerFile.TabIndex = 5;
            this.cmdUploadChummerFile.Text = "Upload File";
            this.cmdUploadChummerFile.UseVisualStyleBackColor = true;
            this.cmdUploadChummerFile.Click += new System.EventHandler(this.cmdUploadChummerFile_Click);
            // 
            // labelSINnerUrl
            // 
            this.labelSINnerUrl.AutoSize = true;
            this.labelSINnerUrl.Location = new System.Drawing.Point(325, 23);
            this.labelSINnerUrl.Name = "labelSINnerUrl";
            this.labelSINnerUrl.Size = new System.Drawing.Size(23, 13);
            this.labelSINnerUrl.TabIndex = 7;
            this.labelSINnerUrl.Text = "Url:";
            this.labelSINnerUrl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cbSINnerUrl
            // 
            this.cbSINnerUrl.FormattingEnabled = true;
            this.cbSINnerUrl.Items.AddRange(new object[] {
            "https://sinners.azurewebsites.net/",
            "https://localhost:5001/",
            "https://sinners-beta.azurewebsites.net/"});
            this.cbSINnerUrl.Location = new System.Drawing.Point(354, 19);
            this.cbSINnerUrl.Name = "cbSINnerUrl";
            this.cbSINnerUrl.Size = new System.Drawing.Size(294, 21);
            this.cbSINnerUrl.TabIndex = 9;
            // 
            // SINnersAdvanced
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.cbSINnerUrl);
            this.Controls.Add(this.labelSINnerUrl);
            this.Controls.Add(this.cmdUploadChummerFile);
            this.Controls.Add(this.cmdPostSINnerMetadata);
            this.Controls.Add(this.cmdPrepareModel);
            this.Controls.Add(this.cmdPopulateTags);
            this.Controls.Add(this.groupBox_Tags);
            this.Name = "SINnersAdvanced";
            this.Size = new System.Drawing.Size(651, 449);
            this.groupBox_Tags.ResumeLayout(false);
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
    }
}
