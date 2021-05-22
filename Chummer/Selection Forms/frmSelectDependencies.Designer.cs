
namespace Chummer
{
    partial class frmSelectDependencies
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
            this.components = new System.ComponentModel.Container();
            this.tblSelectDependenciesMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdRemoveDependency = new System.Windows.Forms.Button();
            this.cmdRemoveExclusivity = new System.Windows.Forms.Button();
            this.cmdAddDependency = new System.Windows.Forms.Button();
            this.treExclusivities = new System.Windows.Forms.TreeView();
            this.treDependencies = new System.Windows.Forms.TreeView();
            this.treCustomDataFiles = new System.Windows.Forms.TreeView();
            this.cmdAddExclusivity = new System.Windows.Forms.Button();
            this.lblExclusivities = new System.Windows.Forms.Label();
            this.lblCustomDataDirectories = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.bufferedTableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdAccept = new System.Windows.Forms.Button();
            this.tblSelectDependenciesMain.SuspendLayout();
            this.bufferedTableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tblSelectDependenciesMain
            // 
            this.tblSelectDependenciesMain.ColumnCount = 5;
            this.tblSelectDependenciesMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tblSelectDependenciesMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tblSelectDependenciesMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tblSelectDependenciesMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tblSelectDependenciesMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tblSelectDependenciesMain.Controls.Add(this.cmdRemoveDependency, 3, 2);
            this.tblSelectDependenciesMain.Controls.Add(this.cmdRemoveExclusivity, 1, 2);
            this.tblSelectDependenciesMain.Controls.Add(this.cmdAddDependency, 3, 1);
            this.tblSelectDependenciesMain.Controls.Add(this.treExclusivities, 0, 1);
            this.tblSelectDependenciesMain.Controls.Add(this.treDependencies, 4, 1);
            this.tblSelectDependenciesMain.Controls.Add(this.treCustomDataFiles, 2, 1);
            this.tblSelectDependenciesMain.Controls.Add(this.cmdAddExclusivity, 1, 1);
            this.tblSelectDependenciesMain.Controls.Add(this.lblExclusivities, 0, 0);
            this.tblSelectDependenciesMain.Controls.Add(this.lblCustomDataDirectories, 2, 0);
            this.tblSelectDependenciesMain.Controls.Add(this.label1, 4, 0);
            this.tblSelectDependenciesMain.Controls.Add(this.bufferedTableLayoutPanel1, 4, 3);
            this.tblSelectDependenciesMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblSelectDependenciesMain.Location = new System.Drawing.Point(0, 0);
            this.tblSelectDependenciesMain.Name = "tblSelectDependenciesMain";
            this.tblSelectDependenciesMain.RowCount = 4;
            this.tblSelectDependenciesMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblSelectDependenciesMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblSelectDependenciesMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblSelectDependenciesMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblSelectDependenciesMain.Size = new System.Drawing.Size(800, 450);
            this.tblSelectDependenciesMain.TabIndex = 0;
            // 
            // cmdRemoveDependency
            // 
            this.cmdRemoveDependency.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdRemoveDependency.AutoSize = true;
            this.cmdRemoveDependency.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRemoveDependency.Image = global::Chummer.Properties.Resources.delete;
            this.cmdRemoveDependency.Location = new System.Drawing.Point(551, 216);
            this.cmdRemoveDependency.Name = "cmdRemoveDependency";
            this.cmdRemoveDependency.Size = new System.Drawing.Size(22, 22);
            this.cmdRemoveDependency.TabIndex = 6;
            this.cmdRemoveDependency.UseVisualStyleBackColor = true;
            this.cmdRemoveDependency.Click += new System.EventHandler(this.cmdRemoveDependency_Click);
            // 
            // cmdRemoveExclusivity
            // 
            this.cmdRemoveExclusivity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdRemoveExclusivity.AutoSize = true;
            this.cmdRemoveExclusivity.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRemoveExclusivity.Image = global::Chummer.Properties.Resources.delete;
            this.cmdRemoveExclusivity.Location = new System.Drawing.Point(226, 216);
            this.cmdRemoveExclusivity.Name = "cmdRemoveExclusivity";
            this.cmdRemoveExclusivity.Size = new System.Drawing.Size(22, 22);
            this.cmdRemoveExclusivity.TabIndex = 5;
            this.cmdRemoveExclusivity.UseVisualStyleBackColor = true;
            this.cmdRemoveExclusivity.Click += new System.EventHandler(this.cmdRemoveExclusivity_Click);
            // 
            // cmdAddDependency
            // 
            this.cmdAddDependency.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdAddDependency.AutoSize = true;
            this.cmdAddDependency.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddDependency.Image = global::Chummer.Properties.Resources.add;
            this.cmdAddDependency.Location = new System.Drawing.Point(551, 188);
            this.cmdAddDependency.Name = "cmdAddDependency";
            this.cmdAddDependency.Size = new System.Drawing.Size(22, 22);
            this.cmdAddDependency.TabIndex = 4;
            this.cmdAddDependency.UseVisualStyleBackColor = true;
            this.cmdAddDependency.Click += new System.EventHandler(this.cmdAddDependency_Click);
            // 
            // treExclusivities
            // 
            this.treExclusivities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treExclusivities.Location = new System.Drawing.Point(3, 16);
            this.treExclusivities.Name = "treExclusivities";
            this.tblSelectDependenciesMain.SetRowSpan(this.treExclusivities, 2);
            this.treExclusivities.Size = new System.Drawing.Size(217, 394);
            this.treExclusivities.TabIndex = 0;
            // 
            // treDependencies
            // 
            this.treDependencies.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treDependencies.Location = new System.Drawing.Point(579, 16);
            this.treDependencies.Name = "treDependencies";
            this.tblSelectDependenciesMain.SetRowSpan(this.treDependencies, 2);
            this.treDependencies.Size = new System.Drawing.Size(218, 394);
            this.treDependencies.TabIndex = 1;
            // 
            // treCustomDataFiles
            // 
            this.treCustomDataFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treCustomDataFiles.Location = new System.Drawing.Point(254, 16);
            this.treCustomDataFiles.Name = "treCustomDataFiles";
            this.tblSelectDependenciesMain.SetRowSpan(this.treCustomDataFiles, 2);
            this.treCustomDataFiles.Size = new System.Drawing.Size(291, 394);
            this.treCustomDataFiles.TabIndex = 2;
            // 
            // cmdAddExclusivity
            // 
            this.cmdAddExclusivity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdAddExclusivity.AutoSize = true;
            this.cmdAddExclusivity.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddExclusivity.Image = global::Chummer.Properties.Resources.add;
            this.cmdAddExclusivity.Location = new System.Drawing.Point(226, 188);
            this.cmdAddExclusivity.Name = "cmdAddExclusivity";
            this.cmdAddExclusivity.Size = new System.Drawing.Size(22, 22);
            this.cmdAddExclusivity.TabIndex = 3;
            this.cmdAddExclusivity.UseVisualStyleBackColor = true;
            this.cmdAddExclusivity.Click += new System.EventHandler(this.cmdAddExclusivity_Click);
            // 
            // lblExclusivities
            // 
            this.lblExclusivities.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblExclusivities.AutoSize = true;
            this.lblExclusivities.Location = new System.Drawing.Point(3, 0);
            this.lblExclusivities.Name = "lblExclusivities";
            this.lblExclusivities.Size = new System.Drawing.Size(217, 13);
            this.lblExclusivities.TabIndex = 7;
            this.lblExclusivities.Tag = "Title_DirectoryExclusivities";
            this.lblExclusivities.Text = "Exclusivities";
            this.lblExclusivities.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblCustomDataDirectories
            // 
            this.lblCustomDataDirectories.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCustomDataDirectories.AutoSize = true;
            this.lblCustomDataDirectories.Location = new System.Drawing.Point(254, 0);
            this.lblCustomDataDirectories.Name = "lblCustomDataDirectories";
            this.lblCustomDataDirectories.Size = new System.Drawing.Size(291, 13);
            this.lblCustomDataDirectories.TabIndex = 8;
            this.lblCustomDataDirectories.Tag = "Tab_Options_CustomDataDirectories";
            this.lblCustomDataDirectories.Text = "Custom Data Directories";
            this.lblCustomDataDirectories.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(579, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(218, 13);
            this.label1.TabIndex = 9;
            this.label1.Tag = "Title_DirectoryDependencies";
            this.label1.Text = "Dependencies";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // bufferedTableLayoutPanel1
            // 
            this.bufferedTableLayoutPanel1.ColumnCount = 2;
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 62.84404F));
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 37.15596F));
            this.bufferedTableLayoutPanel1.Controls.Add(this.cmdCancel, 0, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.cmdAccept, 1, 0);
            this.bufferedTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedTableLayoutPanel1.Location = new System.Drawing.Point(579, 416);
            this.bufferedTableLayoutPanel1.Name = "bufferedTableLayoutPanel1";
            this.bufferedTableLayoutPanel1.RowCount = 1;
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel1.Size = new System.Drawing.Size(218, 31);
            this.bufferedTableLayoutPanel1.TabIndex = 10;
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdCancel.Location = new System.Drawing.Point(59, 4);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 0;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // cmdAccept
            // 
            this.cmdAccept.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdAccept.Location = new System.Drawing.Point(140, 4);
            this.cmdAccept.Name = "cmdAccept";
            this.cmdAccept.Size = new System.Drawing.Size(75, 23);
            this.cmdAccept.TabIndex = 1;
            this.cmdAccept.Tag = "String_OK";
            this.cmdAccept.Text = "OK";
            this.cmdAccept.UseVisualStyleBackColor = true;
            // 
            // frmSelectDependencies
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tblSelectDependenciesMain);
            this.Name = "frmSelectDependencies";
            this.Text = "frmSelectDependencies";
            this.Load += new System.EventHandler(this.frmSelectDependencies_Load);
            this.tblSelectDependenciesMain.ResumeLayout(false);
            this.tblSelectDependenciesMain.PerformLayout();
            this.bufferedTableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private BufferedTableLayoutPanel tblSelectDependenciesMain;
        private System.Windows.Forms.Button cmdRemoveDependency;
        private System.Windows.Forms.Button cmdRemoveExclusivity;
        private System.Windows.Forms.Button cmdAddDependency;
        private System.Windows.Forms.TreeView treExclusivities;
        private System.Windows.Forms.TreeView treDependencies;
        private System.Windows.Forms.TreeView treCustomDataFiles;
        private System.Windows.Forms.Button cmdAddExclusivity;
        private System.Windows.Forms.Label lblExclusivities;
        private System.Windows.Forms.Label lblCustomDataDirectories;
        private System.Windows.Forms.Label label1;
        private BufferedTableLayoutPanel bufferedTableLayoutPanel1;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdAccept;
    }
}
