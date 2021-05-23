
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
            this.tplManageManifestMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tplManageManifestButton = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdAccept = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.tplManifestVersion = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tabControlManifest = new System.Windows.Forms.TabControl();
            this.tabDescription = new System.Windows.Forms.TabPage();
            this.tplDescription = new Chummer.BufferedTableLayoutPanel(this.components);
            this.rtboxDescription = new System.Windows.Forms.RichTextBox();
            this.cboLanguage = new Chummer.ElasticComboBox();
            this.cmdDiscarcDescription = new System.Windows.Forms.Button();
            this.cmdAcceptDescription = new System.Windows.Forms.Button();
            this.tabInformation = new System.Windows.Forms.TabPage();
            this.bufferedTableLayoutPanel2 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.treAuthors = new System.Windows.Forms.TreeView();
            this.tboxAuthor = new System.Windows.Forms.TextBox();
            this.cmdAddAuthor = new System.Windows.Forms.Button();
            this.cmdRemoveAuthor = new System.Windows.Forms.Button();
            this.lblVersion = new System.Windows.Forms.Label();
            this.tboxVersion = new System.Windows.Forms.TextBox();
            this.tabDependencies = new System.Windows.Forms.TabPage();
            this.tplDependencyTab = new Chummer.BufferedTableLayoutPanel(this.components);
            this.treDependencies = new System.Windows.Forms.TreeView();
            this.treCustomDataDirectories = new System.Windows.Forms.TreeView();
            this.treExclusivities = new System.Windows.Forms.TreeView();
            this.label1 = new System.Windows.Forms.Label();
            this.cmdAddExclusivity = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cmdAddDependency = new System.Windows.Forms.Button();
            this.cmdRemoveExclusivity = new System.Windows.Forms.Button();
            this.cmdRemoveDependency = new System.Windows.Forms.Button();
            this.tplManageManifestMain.SuspendLayout();
            this.tplManageManifestButton.SuspendLayout();
            this.tabControlManifest.SuspendLayout();
            this.tabDescription.SuspendLayout();
            this.tplDescription.SuspendLayout();
            this.tabInformation.SuspendLayout();
            this.bufferedTableLayoutPanel2.SuspendLayout();
            this.tabDependencies.SuspendLayout();
            this.tplDependencyTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // tplManageManifestMain
            // 
            this.tplManageManifestMain.AutoSize = true;
            this.tplManageManifestMain.ColumnCount = 1;
            this.tplManageManifestMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tplManageManifestMain.Controls.Add(this.tplManageManifestButton, 0, 1);
            this.tplManageManifestMain.Controls.Add(this.tplManifestVersion, 0, 1);
            this.tplManageManifestMain.Controls.Add(this.tabControlManifest, 0, 0);
            this.tplManageManifestMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tplManageManifestMain.Location = new System.Drawing.Point(0, 0);
            this.tplManageManifestMain.Name = "tplManageManifestMain";
            this.tplManageManifestMain.RowCount = 1;
            this.tplManageManifestMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tplManageManifestMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tplManageManifestMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tplManageManifestMain.Size = new System.Drawing.Size(800, 450);
            this.tplManageManifestMain.TabIndex = 1;
            // 
            // tplManageManifestButton
            // 
            this.tplManageManifestButton.AutoSize = true;
            this.tplManageManifestButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tplManageManifestButton.ColumnCount = 4;
            this.tplManageManifestButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tplManageManifestButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tplManageManifestButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tplManageManifestButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tplManageManifestButton.Controls.Add(this.cmdAccept, 3, 0);
            this.tplManageManifestButton.Controls.Add(this.cmdCancel, 2, 0);
            this.tplManageManifestButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tplManageManifestButton.Location = new System.Drawing.Point(3, 415);
            this.tplManageManifestButton.Name = "tplManageManifestButton";
            this.tplManageManifestButton.RowCount = 1;
            this.tplManageManifestButton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tplManageManifestButton.Size = new System.Drawing.Size(794, 26);
            this.tplManageManifestButton.TabIndex = 3;
            // 
            // cmdAccept
            // 
            this.cmdAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdAccept.Location = new System.Drawing.Point(717, 3);
            this.cmdAccept.Name = "cmdAccept";
            this.cmdAccept.Size = new System.Drawing.Size(74, 20);
            this.cmdAccept.TabIndex = 0;
            this.cmdAccept.Text = "OK";
            this.cmdAccept.UseVisualStyleBackColor = true;
            this.cmdAccept.Click += new System.EventHandler(this.cmdOk_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.Location = new System.Drawing.Point(636, 3);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 20);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // tplManifestVersion
            // 
            this.tplManifestVersion.AutoSize = true;
            this.tplManifestVersion.ColumnCount = 2;
            this.tplManifestVersion.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 82.82829F));
            this.tplManifestVersion.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 17.17172F));
            this.tplManifestVersion.Location = new System.Drawing.Point(3, 447);
            this.tplManifestVersion.Name = "tplManifestVersion";
            this.tplManifestVersion.RowCount = 1;
            this.tplManifestVersion.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tplManifestVersion.Size = new System.Drawing.Size(0, 0);
            this.tplManifestVersion.TabIndex = 4;
            // 
            // tabControlManifest
            // 
            this.tabControlManifest.Controls.Add(this.tabDescription);
            this.tabControlManifest.Controls.Add(this.tabInformation);
            this.tabControlManifest.Controls.Add(this.tabDependencies);
            this.tabControlManifest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlManifest.Location = new System.Drawing.Point(3, 3);
            this.tabControlManifest.Name = "tabControlManifest";
            this.tabControlManifest.SelectedIndex = 0;
            this.tabControlManifest.Size = new System.Drawing.Size(794, 406);
            this.tabControlManifest.TabIndex = 1;
            // 
            // tabDescription
            // 
            this.tabDescription.Controls.Add(this.tplDescription);
            this.tabDescription.Location = new System.Drawing.Point(4, 22);
            this.tabDescription.Name = "tabDescription";
            this.tabDescription.Padding = new System.Windows.Forms.Padding(3);
            this.tabDescription.Size = new System.Drawing.Size(786, 380);
            this.tabDescription.TabIndex = 1;
            this.tabDescription.Text = "Description";
            this.tabDescription.UseVisualStyleBackColor = true;
            // 
            // tplDescription
            // 
            this.tplDescription.ColumnCount = 3;
            this.tplDescription.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tplDescription.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tplDescription.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tplDescription.Controls.Add(this.rtboxDescription, 0, 1);
            this.tplDescription.Controls.Add(this.cboLanguage, 0, 0);
            this.tplDescription.Controls.Add(this.cmdDiscarcDescription, 1, 0);
            this.tplDescription.Controls.Add(this.cmdAcceptDescription, 2, 0);
            this.tplDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tplDescription.Location = new System.Drawing.Point(3, 3);
            this.tplDescription.Name = "tplDescription";
            this.tplDescription.RowCount = 2;
            this.tplDescription.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tplDescription.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tplDescription.Size = new System.Drawing.Size(780, 374);
            this.tplDescription.TabIndex = 0;
            // 
            // rtboxDescription
            // 
            this.tplDescription.SetColumnSpan(this.rtboxDescription, 3);
            this.rtboxDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtboxDescription.Location = new System.Drawing.Point(3, 32);
            this.rtboxDescription.Name = "rtboxDescription";
            this.rtboxDescription.Size = new System.Drawing.Size(774, 339);
            this.rtboxDescription.TabIndex = 0;
            this.rtboxDescription.Text = "";
            // 
            // cboLanguage
            // 
            this.cboLanguage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLanguage.FormattingEnabled = true;
            this.cboLanguage.Location = new System.Drawing.Point(3, 3);
            this.cboLanguage.Name = "cboLanguage";
            this.cboLanguage.Size = new System.Drawing.Size(658, 21);
            this.cboLanguage.TabIndex = 1;
            this.cboLanguage.TooltipText = "";
            // 
            // cmdDiscarcDescription
            // 
            this.cmdDiscarcDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdDiscarcDescription.AutoSize = true;
            this.cmdDiscarcDescription.Location = new System.Drawing.Point(667, 3);
            this.cmdDiscarcDescription.Name = "cmdDiscarcDescription";
            this.cmdDiscarcDescription.Size = new System.Drawing.Size(53, 23);
            this.cmdDiscarcDescription.TabIndex = 2;
            this.cmdDiscarcDescription.Text = "Discard";
            this.cmdDiscarcDescription.UseVisualStyleBackColor = true;
            // 
            // cmdAcceptDescription
            // 
            this.cmdAcceptDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdAcceptDescription.AutoSize = true;
            this.cmdAcceptDescription.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAcceptDescription.Location = new System.Drawing.Point(726, 3);
            this.cmdAcceptDescription.Name = "cmdAcceptDescription";
            this.cmdAcceptDescription.Size = new System.Drawing.Size(51, 23);
            this.cmdAcceptDescription.TabIndex = 3;
            this.cmdAcceptDescription.Text = "Accept";
            this.cmdAcceptDescription.UseVisualStyleBackColor = true;
            // 
            // tabInformation
            // 
            this.tabInformation.Controls.Add(this.bufferedTableLayoutPanel2);
            this.tabInformation.Location = new System.Drawing.Point(4, 22);
            this.tabInformation.Name = "tabInformation";
            this.tabInformation.Padding = new System.Windows.Forms.Padding(3);
            this.tabInformation.Size = new System.Drawing.Size(786, 380);
            this.tabInformation.TabIndex = 2;
            this.tabInformation.Text = "Information";
            this.tabInformation.UseVisualStyleBackColor = true;
            // 
            // bufferedTableLayoutPanel2
            // 
            this.bufferedTableLayoutPanel2.ColumnCount = 3;
            this.bufferedTableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bufferedTableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel2.Controls.Add(this.treAuthors, 0, 1);
            this.bufferedTableLayoutPanel2.Controls.Add(this.tboxAuthor, 0, 0);
            this.bufferedTableLayoutPanel2.Controls.Add(this.cmdAddAuthor, 1, 0);
            this.bufferedTableLayoutPanel2.Controls.Add(this.cmdRemoveAuthor, 2, 0);
            this.bufferedTableLayoutPanel2.Controls.Add(this.lblVersion, 1, 3);
            this.bufferedTableLayoutPanel2.Controls.Add(this.tboxVersion, 2, 3);
            this.bufferedTableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedTableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.bufferedTableLayoutPanel2.Name = "bufferedTableLayoutPanel2";
            this.bufferedTableLayoutPanel2.RowCount = 4;
            this.bufferedTableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.bufferedTableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.bufferedTableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.bufferedTableLayoutPanel2.Size = new System.Drawing.Size(780, 374);
            this.bufferedTableLayoutPanel2.TabIndex = 0;
            // 
            // treAuthors
            // 
            this.bufferedTableLayoutPanel2.SetColumnSpan(this.treAuthors, 3);
            this.treAuthors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treAuthors.Location = new System.Drawing.Point(3, 32);
            this.treAuthors.Name = "treAuthors";
            this.bufferedTableLayoutPanel2.SetRowSpan(this.treAuthors, 2);
            this.treAuthors.Size = new System.Drawing.Size(774, 318);
            this.treAuthors.TabIndex = 4;
            // 
            // tboxAuthor
            // 
            this.tboxAuthor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tboxAuthor.Location = new System.Drawing.Point(3, 3);
            this.tboxAuthor.Name = "tboxAuthor";
            this.tboxAuthor.Size = new System.Drawing.Size(569, 20);
            this.tboxAuthor.TabIndex = 0;
            // 
            // cmdAddAuthor
            // 
            this.cmdAddAuthor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdAddAuthor.AutoSize = true;
            this.cmdAddAuthor.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddAuthor.Image = global::Chummer.Properties.Resources.add;
            this.cmdAddAuthor.Location = new System.Drawing.Point(578, 3);
            this.cmdAddAuthor.Name = "cmdAddAuthor";
            this.cmdAddAuthor.Size = new System.Drawing.Size(86, 23);
            this.cmdAddAuthor.TabIndex = 2;
            this.cmdAddAuthor.Text = "Add Author";
            this.cmdAddAuthor.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.cmdAddAuthor.UseVisualStyleBackColor = true;
            // 
            // cmdRemoveAuthor
            // 
            this.cmdRemoveAuthor.AutoSize = true;
            this.cmdRemoveAuthor.Image = global::Chummer.Properties.Resources.delete;
            this.cmdRemoveAuthor.Location = new System.Drawing.Point(670, 3);
            this.cmdRemoveAuthor.Name = "cmdRemoveAuthor";
            this.cmdRemoveAuthor.Size = new System.Drawing.Size(107, 23);
            this.cmdRemoveAuthor.TabIndex = 3;
            this.cmdRemoveAuthor.Text = "Remove Author";
            this.cmdRemoveAuthor.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.cmdRemoveAuthor.UseVisualStyleBackColor = true;
            // 
            // lblVersion
            // 
            this.lblVersion.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new System.Drawing.Point(619, 357);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(45, 13);
            this.lblVersion.TabIndex = 1;
            this.lblVersion.Text = "Version:";
            // 
            // tboxVersion
            // 
            this.tboxVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tboxVersion.Location = new System.Drawing.Point(670, 356);
            this.tboxVersion.Name = "tboxVersion";
            this.tboxVersion.Size = new System.Drawing.Size(107, 20);
            this.tboxVersion.TabIndex = 0;
            // 
            // tabDependencies
            // 
            this.tabDependencies.Controls.Add(this.tplDependencyTab);
            this.tabDependencies.Location = new System.Drawing.Point(4, 22);
            this.tabDependencies.Name = "tabDependencies";
            this.tabDependencies.Padding = new System.Windows.Forms.Padding(3);
            this.tabDependencies.Size = new System.Drawing.Size(786, 380);
            this.tabDependencies.TabIndex = 3;
            this.tabDependencies.Text = "Dependencies and Exclusivities";
            this.tabDependencies.UseVisualStyleBackColor = true;
            // 
            // tplDependencyTab
            // 
            this.tplDependencyTab.ColumnCount = 5;
            this.tplDependencyTab.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tplDependencyTab.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tplDependencyTab.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tplDependencyTab.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tplDependencyTab.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tplDependencyTab.Controls.Add(this.treDependencies, 4, 1);
            this.tplDependencyTab.Controls.Add(this.treCustomDataDirectories, 2, 1);
            this.tplDependencyTab.Controls.Add(this.treExclusivities, 0, 1);
            this.tplDependencyTab.Controls.Add(this.label1, 0, 0);
            this.tplDependencyTab.Controls.Add(this.cmdAddExclusivity, 1, 1);
            this.tplDependencyTab.Controls.Add(this.label2, 2, 0);
            this.tplDependencyTab.Controls.Add(this.label3, 4, 0);
            this.tplDependencyTab.Controls.Add(this.cmdAddDependency, 3, 1);
            this.tplDependencyTab.Controls.Add(this.cmdRemoveExclusivity, 1, 2);
            this.tplDependencyTab.Controls.Add(this.cmdRemoveDependency, 3, 2);
            this.tplDependencyTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tplDependencyTab.Location = new System.Drawing.Point(3, 3);
            this.tplDependencyTab.Name = "tplDependencyTab";
            this.tplDependencyTab.RowCount = 3;
            this.tplDependencyTab.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tplDependencyTab.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tplDependencyTab.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tplDependencyTab.Size = new System.Drawing.Size(780, 374);
            this.tplDependencyTab.TabIndex = 1;
            // 
            // treDependencies
            // 
            this.treDependencies.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treDependencies.Location = new System.Drawing.Point(604, 16);
            this.treDependencies.Name = "treDependencies";
            this.tplDependencyTab.SetRowSpan(this.treDependencies, 2);
            this.treDependencies.Size = new System.Drawing.Size(173, 355);
            this.treDependencies.TabIndex = 2;
            // 
            // treCustomDataDirectories
            // 
            this.treCustomDataDirectories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treCustomDataDirectories.Location = new System.Drawing.Point(268, 16);
            this.treCustomDataDirectories.Name = "treCustomDataDirectories";
            this.tplDependencyTab.SetRowSpan(this.treCustomDataDirectories, 2);
            this.treCustomDataDirectories.Size = new System.Drawing.Size(230, 355);
            this.treCustomDataDirectories.TabIndex = 1;
            // 
            // treExclusivities
            // 
            this.treExclusivities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treExclusivities.Location = new System.Drawing.Point(3, 16);
            this.treExclusivities.Name = "treExclusivities";
            this.tplDependencyTab.SetRowSpan(this.treExclusivities, 2);
            this.treExclusivities.Size = new System.Drawing.Size(171, 355);
            this.treExclusivities.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(171, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Exclusivities";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // cmdAddExclusivity
            // 
            this.cmdAddExclusivity.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cmdAddExclusivity.AutoSize = true;
            this.cmdAddExclusivity.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddExclusivity.Image = global::Chummer.Properties.Resources.add;
            this.cmdAddExclusivity.Location = new System.Drawing.Point(180, 167);
            this.cmdAddExclusivity.Name = "cmdAddExclusivity";
            this.cmdAddExclusivity.Size = new System.Drawing.Size(82, 23);
            this.cmdAddExclusivity.TabIndex = 6;
            this.cmdAddExclusivity.Text = "Exclusivity";
            this.cmdAddExclusivity.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.cmdAddExclusivity.UseVisualStyleBackColor = true;
            this.cmdAddExclusivity.Click += new System.EventHandler(this.cmdAddExclusivity_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(268, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(230, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Custom Data Directories";
            this.label2.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(604, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(173, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Dependencies";
            this.label3.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // cmdAddDependency
            // 
            this.cmdAddDependency.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cmdAddDependency.AutoSize = true;
            this.cmdAddDependency.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddDependency.Image = global::Chummer.Properties.Resources.add;
            this.cmdAddDependency.Location = new System.Drawing.Point(504, 167);
            this.cmdAddDependency.Name = "cmdAddDependency";
            this.cmdAddDependency.Size = new System.Drawing.Size(94, 23);
            this.cmdAddDependency.TabIndex = 12;
            this.cmdAddDependency.Text = "Dependency";
            this.cmdAddDependency.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.cmdAddDependency.UseVisualStyleBackColor = true;
            this.cmdAddDependency.Click += new System.EventHandler(this.cmdAddDependency_Click);
            // 
            // cmdRemoveExclusivity
            // 
            this.cmdRemoveExclusivity.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cmdRemoveExclusivity.AutoSize = true;
            this.cmdRemoveExclusivity.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRemoveExclusivity.Image = global::Chummer.Properties.Resources.delete;
            this.cmdRemoveExclusivity.Location = new System.Drawing.Point(180, 196);
            this.cmdRemoveExclusivity.Name = "cmdRemoveExclusivity";
            this.cmdRemoveExclusivity.Size = new System.Drawing.Size(82, 23);
            this.cmdRemoveExclusivity.TabIndex = 13;
            this.cmdRemoveExclusivity.Text = "Exclusivity";
            this.cmdRemoveExclusivity.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.cmdRemoveExclusivity.UseVisualStyleBackColor = true;
            this.cmdRemoveExclusivity.Click += new System.EventHandler(this.cmdRemoveExclusivity_Click);
            // 
            // cmdRemoveDependency
            // 
            this.cmdRemoveDependency.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cmdRemoveDependency.AutoSize = true;
            this.cmdRemoveDependency.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRemoveDependency.Image = global::Chummer.Properties.Resources.delete;
            this.cmdRemoveDependency.Location = new System.Drawing.Point(504, 196);
            this.cmdRemoveDependency.Name = "cmdRemoveDependency";
            this.cmdRemoveDependency.Size = new System.Drawing.Size(94, 23);
            this.cmdRemoveDependency.TabIndex = 14;
            this.cmdRemoveDependency.Text = "Dependency";
            this.cmdRemoveDependency.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.cmdRemoveDependency.UseVisualStyleBackColor = true;
            this.cmdRemoveDependency.Click += new System.EventHandler(this.cmdRemoveDependency_Click);
            // 
            // frmSelectDependencies
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tplManageManifestMain);
            this.Name = "frmSelectDependencies";
            this.Text = "frmSelectDependencies";
            this.Load += new System.EventHandler(this.frmSelectDependencies_Load);
            this.tplManageManifestMain.ResumeLayout(false);
            this.tplManageManifestMain.PerformLayout();
            this.tplManageManifestButton.ResumeLayout(false);
            this.tabControlManifest.ResumeLayout(false);
            this.tabDescription.ResumeLayout(false);
            this.tplDescription.ResumeLayout(false);
            this.tplDescription.PerformLayout();
            this.tabInformation.ResumeLayout(false);
            this.bufferedTableLayoutPanel2.ResumeLayout(false);
            this.bufferedTableLayoutPanel2.PerformLayout();
            this.tabDependencies.ResumeLayout(false);
            this.tplDependencyTab.ResumeLayout(false);
            this.tplDependencyTab.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BufferedTableLayoutPanel tplManageManifestMain;
        private BufferedTableLayoutPanel tplDescription;
        private System.Windows.Forms.RichTextBox rtboxDescription;
        private ElasticComboBox cboLanguage;
        private System.Windows.Forms.Button cmdDiscarcDescription;
        private System.Windows.Forms.Button cmdAcceptDescription;
        private BufferedTableLayoutPanel bufferedTableLayoutPanel2;
        private System.Windows.Forms.Button cmdRemoveAuthor;
        private System.Windows.Forms.Button cmdAddAuthor;
        private System.Windows.Forms.TreeView treAuthors;
        private System.Windows.Forms.TextBox tboxAuthor;
        private BufferedTableLayoutPanel tplManageManifestButton;
        private System.Windows.Forms.Button cmdAccept;
        private System.Windows.Forms.Button cmdCancel;
        private BufferedTableLayoutPanel tplManifestVersion;
        private System.Windows.Forms.TextBox tboxVersion;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.TabControl tabControlManifest;
        private System.Windows.Forms.TabPage tabDescription;
        private System.Windows.Forms.TabPage tabInformation;
        private System.Windows.Forms.TabPage tabDependencies;
        private BufferedTableLayoutPanel tplDependencyTab;
        private System.Windows.Forms.TreeView treDependencies;
        private System.Windows.Forms.TreeView treCustomDataDirectories;
        private System.Windows.Forms.TreeView treExclusivities;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button cmdAddExclusivity;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button cmdAddDependency;
        private System.Windows.Forms.Button cmdRemoveExclusivity;
        private System.Windows.Forms.Button cmdRemoveDependency;
    }
}
