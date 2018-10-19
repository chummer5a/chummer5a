namespace ChummerHub.Client.UI
{
    partial class SINnersAdvanced
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
            this.cmdUploadSINner = new System.Windows.Forms.Button();
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
            this.cmdPopulateTags.Location = new System.Drawing.Point(322, 16);
            this.cmdPopulateTags.Name = "cmdPopulateTags";
            this.cmdPopulateTags.Size = new System.Drawing.Size(75, 23);
            this.cmdPopulateTags.TabIndex = 2;
            this.cmdPopulateTags.Text = "Populate";
            this.cmdPopulateTags.UseVisualStyleBackColor = true;
            this.cmdPopulateTags.Click += new System.EventHandler(this.cmdPopulateTags_Click);
            // 
            // cmdPrepareModel
            // 
            this.cmdPrepareModel.Location = new System.Drawing.Point(323, 46);
            this.cmdPrepareModel.Name = "cmdPrepareModel";
            this.cmdPrepareModel.Size = new System.Drawing.Size(75, 23);
            this.cmdPrepareModel.TabIndex = 3;
            this.cmdPrepareModel.Text = "Prepare";
            this.cmdPrepareModel.UseVisualStyleBackColor = true;
            this.cmdPrepareModel.Click += new System.EventHandler(this.cmdPrepareModel_Click);
            // 
            // cmdUploadSINner
            // 
            this.cmdUploadSINner.Location = new System.Drawing.Point(323, 76);
            this.cmdUploadSINner.Name = "cmdUploadSINner";
            this.cmdUploadSINner.Size = new System.Drawing.Size(75, 23);
            this.cmdUploadSINner.TabIndex = 4;
            this.cmdUploadSINner.Text = "Upload";
            this.cmdUploadSINner.UseVisualStyleBackColor = true;
            this.cmdUploadSINner.Click += new System.EventHandler(this.cmdUploadSINner_Click);
            // 
            // SINnersAdvanced
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cmdUploadSINner);
            this.Controls.Add(this.cmdPrepareModel);
            this.Controls.Add(this.cmdPopulateTags);
            this.Controls.Add(this.groupBox_Tags);
            this.Name = "SINnersAdvanced";
            this.Size = new System.Drawing.Size(658, 549);
            this.groupBox_Tags.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox_Tags;
        private System.Windows.Forms.Button cmdPopulateTags;
        private System.Windows.Forms.Button cmdPrepareModel;
        private System.Windows.Forms.Button cmdUploadSINner;
        private System.Windows.Forms.TreeView MyTagTreeView;
    }
}
