namespace Chummer
{
    partial class OmaeRecord
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
            this.lblCharacterName = new System.Windows.Forms.Label();
            this.lblUserLabel = new System.Windows.Forms.Label();
            this.lblDate = new System.Windows.Forms.Label();
            this.lblTags = new System.Windows.Forms.Label();
            this.cmdDownload = new System.Windows.Forms.Button();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblCount = new System.Windows.Forms.Label();
            this.lblMetatype = new System.Windows.Forms.Label();
            this.cmdPostUpdate = new System.Windows.Forms.Button();
            this.lblUser = new System.Windows.Forms.Label();
            this.cmdDelete = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblCharacterName
            // 
            this.lblCharacterName.AutoSize = true;
            this.lblCharacterName.Location = new System.Drawing.Point(3, 0);
            this.lblCharacterName.Name = "lblCharacterName";
            this.lblCharacterName.Size = new System.Drawing.Size(140, 13);
            this.lblCharacterName.TabIndex = 0;
            this.lblCharacterName.Text = "Spud the Research Monkey";
            // 
            // lblUserLabel
            // 
            this.lblUserLabel.AutoSize = true;
            this.lblUserLabel.Location = new System.Drawing.Point(292, 0);
            this.lblUserLabel.Name = "lblUserLabel";
            this.lblUserLabel.Size = new System.Drawing.Size(58, 13);
            this.lblUserLabel.TabIndex = 1;
            this.lblUserLabel.Text = "Created by";
            // 
            // lblDate
            // 
            this.lblDate.AutoSize = true;
            this.lblDate.Location = new System.Drawing.Point(482, 0);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(103, 13);
            this.lblDate.TabIndex = 2;
            this.lblDate.Text = "Updated Jul 8, 2011";
            // 
            // lblTags
            // 
            this.lblTags.AutoSize = true;
            this.lblTags.Location = new System.Drawing.Point(292, 19);
            this.lblTags.Name = "lblTags";
            this.lblTags.Size = new System.Drawing.Size(55, 13);
            this.lblTags.TabIndex = 4;
            this.lblTags.Text = "Tags: Silly";
            this.lblTags.Visible = false;
            // 
            // cmdDownload
            // 
            this.cmdDownload.Location = new System.Drawing.Point(638, 3);
            this.cmdDownload.Name = "cmdDownload";
            this.cmdDownload.Size = new System.Drawing.Size(79, 29);
            this.cmdDownload.TabIndex = 5;
            this.cmdDownload.Tag = "Omae_Download";
            this.cmdDownload.Text = "Download";
            this.cmdDownload.UseVisualStyleBackColor = true;
            this.cmdDownload.Click += new System.EventHandler(this.cmdDownload_Click);
            // 
            // lblDescription
            // 
            this.lblDescription.Location = new System.Drawing.Point(3, 38);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(629, 44);
            this.lblDescription.TabIndex = 6;
            this.lblDescription.Text = "[3 line description]";
            // 
            // lblCount
            // 
            this.lblCount.AutoSize = true;
            this.lblCount.Location = new System.Drawing.Point(482, 19);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(104, 13);
            this.lblCount.TabIndex = 7;
            this.lblCount.Text = "Downloaded X times";
            // 
            // lblMetatype
            // 
            this.lblMetatype.AutoSize = true;
            this.lblMetatype.Location = new System.Drawing.Point(3, 19);
            this.lblMetatype.Name = "lblMetatype";
            this.lblMetatype.Size = new System.Drawing.Size(70, 13);
            this.lblMetatype.TabIndex = 8;
            this.lblMetatype.Text = "Metatype: [X]";
            // 
            // cmdPostUpdate
            // 
            this.cmdPostUpdate.Location = new System.Drawing.Point(638, 36);
            this.cmdPostUpdate.Name = "cmdPostUpdate";
            this.cmdPostUpdate.Size = new System.Drawing.Size(79, 22);
            this.cmdPostUpdate.TabIndex = 9;
            this.cmdPostUpdate.Tag = "Omae_PostUpdate";
            this.cmdPostUpdate.Text = "Post Update";
            this.cmdPostUpdate.UseVisualStyleBackColor = true;
            this.cmdPostUpdate.Visible = false;
            this.cmdPostUpdate.Click += new System.EventHandler(this.cmdPostUpdate_Click);
            // 
            // lblUser
            // 
            this.lblUser.AutoSize = true;
            this.lblUser.Location = new System.Drawing.Point(348, 0);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(35, 13);
            this.lblUser.TabIndex = 10;
            this.lblUser.Text = "[User]";
            // 
            // cmdDelete
            // 
            this.cmdDelete.Location = new System.Drawing.Point(638, 59);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(79, 22);
            this.cmdDelete.TabIndex = 11;
            this.cmdDelete.Tag = "String_Delete";
            this.cmdDelete.Text = "Delete";
            this.cmdDelete.UseVisualStyleBackColor = true;
            this.cmdDelete.Visible = false;
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // OmaeRecord
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.cmdDelete);
            this.Controls.Add(this.cmdPostUpdate);
            this.Controls.Add(this.lblMetatype);
            this.Controls.Add(this.lblCount);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.cmdDownload);
            this.Controls.Add(this.lblTags);
            this.Controls.Add(this.lblDate);
            this.Controls.Add(this.lblUserLabel);
            this.Controls.Add(this.lblCharacterName);
            this.Controls.Add(this.lblUser);
            this.Name = "OmaeRecord";
            this.Size = new System.Drawing.Size(720, 83);
            this.Load += new System.EventHandler(this.OmaeRecord_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.Label lblCharacterName;
        private System.Windows.Forms.Label lblUserLabel;
        private System.Windows.Forms.Label lblDate;
        private System.Windows.Forms.Label lblTags;
        private System.Windows.Forms.Button cmdDownload;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.Label lblMetatype;
        private System.Windows.Forms.Button cmdPostUpdate;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.Button cmdDelete;
    }
}
