namespace Chummer
{
    partial class frmHeroLabImporter
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmHeroLabImporter));
            this.treCharacterList = new System.Windows.Forms.TreeView();
            this.cmdSelectFile = new System.Windows.Forms.Button();
            this.cmdImport = new System.Windows.Forms.Button();
            this.lblEssence = new System.Windows.Forms.Label();
            this.lblEssenceLabel = new System.Windows.Forms.Label();
            this.lblCharacterAlias = new System.Windows.Forms.Label();
            this.lblCharacterAliasLabel = new System.Windows.Forms.Label();
            this.lblPlayerName = new System.Windows.Forms.Label();
            this.lblPlayerNameLabel = new System.Windows.Forms.Label();
            this.lblCareerKarma = new System.Windows.Forms.Label();
            this.lblCareerKarmaLabel = new System.Windows.Forms.Label();
            this.lblMetatype = new System.Windows.Forms.Label();
            this.lblMetatypeLabel = new System.Windows.Forms.Label();
            this.lblCharacterName = new System.Windows.Forms.Label();
            this.lblCharacterNameLabel = new System.Windows.Forms.Label();
            this.picMugshot = new System.Windows.Forms.PictureBox();
            this.lblHeroLabTrademark = new System.Windows.Forms.Label();
            this.tabCharacterText = new System.Windows.Forms.TabControl();
            this.panCharacterBio = new System.Windows.Forms.TabPage();
            this.txtCharacterBio = new System.Windows.Forms.TextBox();
            this.panCharacterConcept = new System.Windows.Forms.TabPage();
            this.txtCharacterConcept = new System.Windows.Forms.TextBox();
            this.panCharacterBackground = new System.Windows.Forms.TabPage();
            this.txtCharacterBackground = new System.Windows.Forms.TextBox();
            this.panCharacterNotes = new System.Windows.Forms.TabPage();
            this.txtCharacterNotes = new System.Windows.Forms.TextBox();
            this.panGameNotes = new System.Windows.Forms.TabPage();
            this.txtGameNotes = new System.Windows.Forms.TextBox();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.flpButtons = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.picMugshot)).BeginInit();
            this.tabCharacterText.SuspendLayout();
            this.panCharacterBio.SuspendLayout();
            this.panCharacterConcept.SuspendLayout();
            this.panCharacterBackground.SuspendLayout();
            this.panCharacterNotes.SuspendLayout();
            this.panGameNotes.SuspendLayout();
            this.tlpMain.SuspendLayout();
            this.flpButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // treCharacterList
            // 
            this.treCharacterList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treCharacterList.Location = new System.Drawing.Point(3, 3);
            this.treCharacterList.Name = "treCharacterList";
            this.tlpMain.SetRowSpan(this.treCharacterList, 7);
            this.treCharacterList.ShowNodeToolTips = true;
            this.treCharacterList.Size = new System.Drawing.Size(295, 508);
            this.treCharacterList.TabIndex = 0;
            this.treCharacterList.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treCharacterList_AfterSelect);
            this.treCharacterList.DoubleClick += new System.EventHandler(this.treCharacterList_DoubleClick);
            // 
            // cmdSelectFile
            // 
            this.cmdSelectFile.AutoSize = true;
            this.cmdSelectFile.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdSelectFile.Location = new System.Drawing.Point(3, 3);
            this.cmdSelectFile.Name = "cmdSelectFile";
            this.cmdSelectFile.Size = new System.Drawing.Size(92, 23);
            this.cmdSelectFile.TabIndex = 1;
            this.cmdSelectFile.Tag = "String_SelectFile";
            this.cmdSelectFile.Text = "Select POR File";
            this.cmdSelectFile.UseVisualStyleBackColor = true;
            this.cmdSelectFile.Click += new System.EventHandler(this.cmdSelectFile_Click);
            // 
            // cmdImport
            // 
            this.cmdImport.AutoSize = true;
            this.cmdImport.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdImport.Enabled = false;
            this.cmdImport.Location = new System.Drawing.Point(101, 3);
            this.cmdImport.Name = "cmdImport";
            this.cmdImport.Size = new System.Drawing.Size(95, 23);
            this.cmdImport.TabIndex = 2;
            this.cmdImport.Tag = "String_ImportCharacter";
            this.cmdImport.Text = "Import Character";
            this.cmdImport.UseVisualStyleBackColor = true;
            this.cmdImport.Click += new System.EventHandler(this.cmdImport_Click);
            // 
            // lblEssence
            // 
            this.lblEssence.AutoSize = true;
            this.lblEssence.Location = new System.Drawing.Point(397, 131);
            this.lblEssence.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblEssence.Name = "lblEssence";
            this.lblEssence.Size = new System.Drawing.Size(39, 13);
            this.lblEssence.TabIndex = 51;
            this.lblEssence.Text = "[None]";
            // 
            // lblEssenceLabel
            // 
            this.lblEssenceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblEssenceLabel.AutoSize = true;
            this.lblEssenceLabel.Location = new System.Drawing.Point(340, 131);
            this.lblEssenceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblEssenceLabel.Name = "lblEssenceLabel";
            this.lblEssenceLabel.Size = new System.Drawing.Size(51, 13);
            this.lblEssenceLabel.TabIndex = 50;
            this.lblEssenceLabel.Tag = "Label_Essence";
            this.lblEssenceLabel.Text = "Essence:";
            this.lblEssenceLabel.Visible = false;
            // 
            // lblCharacterAlias
            // 
            this.lblCharacterAlias.AutoSize = true;
            this.lblCharacterAlias.Location = new System.Drawing.Point(397, 31);
            this.lblCharacterAlias.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCharacterAlias.Name = "lblCharacterAlias";
            this.lblCharacterAlias.Size = new System.Drawing.Size(39, 13);
            this.lblCharacterAlias.TabIndex = 49;
            this.lblCharacterAlias.Text = "[None]";
            // 
            // lblCharacterAliasLabel
            // 
            this.lblCharacterAliasLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCharacterAliasLabel.AutoSize = true;
            this.lblCharacterAliasLabel.Location = new System.Drawing.Point(359, 31);
            this.lblCharacterAliasLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCharacterAliasLabel.Name = "lblCharacterAliasLabel";
            this.lblCharacterAliasLabel.Size = new System.Drawing.Size(32, 13);
            this.lblCharacterAliasLabel.TabIndex = 48;
            this.lblCharacterAliasLabel.Tag = "Label_Alias";
            this.lblCharacterAliasLabel.Text = "Alias:";
            this.lblCharacterAliasLabel.Visible = false;
            // 
            // lblPlayerName
            // 
            this.lblPlayerName.AutoSize = true;
            this.lblPlayerName.Location = new System.Drawing.Point(397, 56);
            this.lblPlayerName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPlayerName.Name = "lblPlayerName";
            this.lblPlayerName.Size = new System.Drawing.Size(39, 13);
            this.lblPlayerName.TabIndex = 47;
            this.lblPlayerName.Text = "[None]";
            // 
            // lblPlayerNameLabel
            // 
            this.lblPlayerNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPlayerNameLabel.AutoSize = true;
            this.lblPlayerNameLabel.Location = new System.Drawing.Point(352, 56);
            this.lblPlayerNameLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPlayerNameLabel.Name = "lblPlayerNameLabel";
            this.lblPlayerNameLabel.Size = new System.Drawing.Size(39, 13);
            this.lblPlayerNameLabel.TabIndex = 46;
            this.lblPlayerNameLabel.Tag = "Label_Player";
            this.lblPlayerNameLabel.Text = "Player:";
            this.lblPlayerNameLabel.Visible = false;
            // 
            // lblCareerKarma
            // 
            this.lblCareerKarma.AutoSize = true;
            this.lblCareerKarma.Location = new System.Drawing.Point(397, 106);
            this.lblCareerKarma.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCareerKarma.Name = "lblCareerKarma";
            this.lblCareerKarma.Size = new System.Drawing.Size(39, 13);
            this.lblCareerKarma.TabIndex = 45;
            this.lblCareerKarma.Text = "[None]";
            // 
            // lblCareerKarmaLabel
            // 
            this.lblCareerKarmaLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCareerKarmaLabel.AutoSize = true;
            this.lblCareerKarmaLabel.Location = new System.Drawing.Point(317, 106);
            this.lblCareerKarmaLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCareerKarmaLabel.Name = "lblCareerKarmaLabel";
            this.lblCareerKarmaLabel.Size = new System.Drawing.Size(74, 13);
            this.lblCareerKarmaLabel.TabIndex = 44;
            this.lblCareerKarmaLabel.Tag = "String_CareerKarma";
            this.lblCareerKarmaLabel.Text = "Career Karma:";
            this.lblCareerKarmaLabel.Visible = false;
            // 
            // lblMetatype
            // 
            this.lblMetatype.AutoSize = true;
            this.lblMetatype.Location = new System.Drawing.Point(397, 81);
            this.lblMetatype.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetatype.Name = "lblMetatype";
            this.lblMetatype.Size = new System.Drawing.Size(39, 13);
            this.lblMetatype.TabIndex = 43;
            this.lblMetatype.Text = "[None]";
            // 
            // lblMetatypeLabel
            // 
            this.lblMetatypeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMetatypeLabel.AutoSize = true;
            this.lblMetatypeLabel.Location = new System.Drawing.Point(337, 81);
            this.lblMetatypeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetatypeLabel.Name = "lblMetatypeLabel";
            this.lblMetatypeLabel.Size = new System.Drawing.Size(54, 13);
            this.lblMetatypeLabel.TabIndex = 42;
            this.lblMetatypeLabel.Tag = "Label_Metatype";
            this.lblMetatypeLabel.Text = "Metatype:";
            this.lblMetatypeLabel.Visible = false;
            // 
            // lblCharacterName
            // 
            this.lblCharacterName.AutoSize = true;
            this.lblCharacterName.Location = new System.Drawing.Point(397, 6);
            this.lblCharacterName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCharacterName.Name = "lblCharacterName";
            this.lblCharacterName.Size = new System.Drawing.Size(39, 13);
            this.lblCharacterName.TabIndex = 41;
            this.lblCharacterName.Text = "[None]";
            // 
            // lblCharacterNameLabel
            // 
            this.lblCharacterNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCharacterNameLabel.AutoSize = true;
            this.lblCharacterNameLabel.Location = new System.Drawing.Point(304, 6);
            this.lblCharacterNameLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCharacterNameLabel.Name = "lblCharacterNameLabel";
            this.lblCharacterNameLabel.Size = new System.Drawing.Size(87, 13);
            this.lblCharacterNameLabel.TabIndex = 40;
            this.lblCharacterNameLabel.Tag = "Label_CharacterName";
            this.lblCharacterNameLabel.Text = "Character Name:";
            this.lblCharacterNameLabel.Visible = false;
            // 
            // picMugshot
            // 
            this.picMugshot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picMugshot.Location = new System.Drawing.Point(463, 3);
            this.picMugshot.MinimumSize = new System.Drawing.Size(21, 31);
            this.picMugshot.Name = "picMugshot";
            this.tlpMain.SetRowSpan(this.picMugshot, 6);
            this.picMugshot.Size = new System.Drawing.Size(300, 300);
            this.picMugshot.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picMugshot.TabIndex = 39;
            this.picMugshot.TabStop = false;
            this.picMugshot.SizeChanged += new System.EventHandler(this.picMugshot_SizeChanged);
            // 
            // lblHeroLabTrademark
            // 
            this.lblHeroLabTrademark.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblHeroLabTrademark.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.lblHeroLabTrademark, 3);
            this.lblHeroLabTrademark.Location = new System.Drawing.Point(349, 524);
            this.lblHeroLabTrademark.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblHeroLabTrademark.Name = "lblHeroLabTrademark";
            this.lblHeroLabTrademark.Size = new System.Drawing.Size(414, 13);
            this.lblHeroLabTrademark.TabIndex = 52;
            this.lblHeroLabTrademark.Tag = "Label_HeroLabTrademarks";
            this.lblHeroLabTrademark.Text = "Hero Lab and the Hero Lab logo are Registered Trademarks of LWD Technology, Inc.";
            // 
            // tabCharacterText
            // 
            this.tlpMain.SetColumnSpan(this.tabCharacterText, 3);
            this.tabCharacterText.Controls.Add(this.panCharacterBio);
            this.tabCharacterText.Controls.Add(this.panCharacterConcept);
            this.tabCharacterText.Controls.Add(this.panCharacterBackground);
            this.tabCharacterText.Controls.Add(this.panCharacterNotes);
            this.tabCharacterText.Controls.Add(this.panGameNotes);
            this.tabCharacterText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabCharacterText.Location = new System.Drawing.Point(304, 309);
            this.tabCharacterText.Name = "tabCharacterText";
            this.tabCharacterText.SelectedIndex = 0;
            this.tabCharacterText.Size = new System.Drawing.Size(459, 202);
            this.tabCharacterText.TabIndex = 54;
            // 
            // panCharacterBio
            // 
            this.panCharacterBio.Controls.Add(this.txtCharacterBio);
            this.panCharacterBio.Location = new System.Drawing.Point(4, 22);
            this.panCharacterBio.Name = "panCharacterBio";
            this.panCharacterBio.Padding = new System.Windows.Forms.Padding(3);
            this.panCharacterBio.Size = new System.Drawing.Size(451, 176);
            this.panCharacterBio.TabIndex = 0;
            this.panCharacterBio.Tag = "Tab_Roster_Description";
            this.panCharacterBio.Text = "Description";
            this.panCharacterBio.UseVisualStyleBackColor = true;
            // 
            // txtCharacterBio
            // 
            this.txtCharacterBio.BackColor = System.Drawing.SystemColors.Window;
            this.txtCharacterBio.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCharacterBio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCharacterBio.Location = new System.Drawing.Point(3, 3);
            this.txtCharacterBio.Multiline = true;
            this.txtCharacterBio.Name = "txtCharacterBio";
            this.txtCharacterBio.ReadOnly = true;
            this.txtCharacterBio.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtCharacterBio.Size = new System.Drawing.Size(445, 170);
            this.txtCharacterBio.TabIndex = 1;
            // 
            // panCharacterConcept
            // 
            this.panCharacterConcept.Controls.Add(this.txtCharacterConcept);
            this.panCharacterConcept.Location = new System.Drawing.Point(4, 22);
            this.panCharacterConcept.Name = "panCharacterConcept";
            this.panCharacterConcept.Padding = new System.Windows.Forms.Padding(3);
            this.panCharacterConcept.Size = new System.Drawing.Size(451, 176);
            this.panCharacterConcept.TabIndex = 1;
            this.panCharacterConcept.Tag = "Tab_Roster_Concept";
            this.panCharacterConcept.Text = "Concept";
            this.panCharacterConcept.UseVisualStyleBackColor = true;
            // 
            // txtCharacterConcept
            // 
            this.txtCharacterConcept.BackColor = System.Drawing.SystemColors.Window;
            this.txtCharacterConcept.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCharacterConcept.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCharacterConcept.Location = new System.Drawing.Point(3, 3);
            this.txtCharacterConcept.Multiline = true;
            this.txtCharacterConcept.Name = "txtCharacterConcept";
            this.txtCharacterConcept.ReadOnly = true;
            this.txtCharacterConcept.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtCharacterConcept.Size = new System.Drawing.Size(445, 170);
            this.txtCharacterConcept.TabIndex = 2;
            this.txtCharacterConcept.Tag = "";
            // 
            // panCharacterBackground
            // 
            this.panCharacterBackground.Controls.Add(this.txtCharacterBackground);
            this.panCharacterBackground.Location = new System.Drawing.Point(4, 22);
            this.panCharacterBackground.Name = "panCharacterBackground";
            this.panCharacterBackground.Padding = new System.Windows.Forms.Padding(3);
            this.panCharacterBackground.Size = new System.Drawing.Size(451, 176);
            this.panCharacterBackground.TabIndex = 2;
            this.panCharacterBackground.Tag = "Tab_Roster_Background";
            this.panCharacterBackground.Text = "Background";
            this.panCharacterBackground.UseVisualStyleBackColor = true;
            // 
            // txtCharacterBackground
            // 
            this.txtCharacterBackground.BackColor = System.Drawing.SystemColors.Window;
            this.txtCharacterBackground.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCharacterBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCharacterBackground.Location = new System.Drawing.Point(3, 3);
            this.txtCharacterBackground.Multiline = true;
            this.txtCharacterBackground.Name = "txtCharacterBackground";
            this.txtCharacterBackground.ReadOnly = true;
            this.txtCharacterBackground.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtCharacterBackground.Size = new System.Drawing.Size(445, 170);
            this.txtCharacterBackground.TabIndex = 0;
            this.txtCharacterBackground.Tag = "";
            // 
            // panCharacterNotes
            // 
            this.panCharacterNotes.Controls.Add(this.txtCharacterNotes);
            this.panCharacterNotes.Location = new System.Drawing.Point(4, 22);
            this.panCharacterNotes.Name = "panCharacterNotes";
            this.panCharacterNotes.Padding = new System.Windows.Forms.Padding(3);
            this.panCharacterNotes.Size = new System.Drawing.Size(451, 176);
            this.panCharacterNotes.TabIndex = 3;
            this.panCharacterNotes.Tag = "Tab_Roster_CharacterNotes";
            this.panCharacterNotes.Text = "Character Notes";
            this.panCharacterNotes.UseVisualStyleBackColor = true;
            // 
            // txtCharacterNotes
            // 
            this.txtCharacterNotes.BackColor = System.Drawing.SystemColors.Window;
            this.txtCharacterNotes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCharacterNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCharacterNotes.Location = new System.Drawing.Point(3, 3);
            this.txtCharacterNotes.Multiline = true;
            this.txtCharacterNotes.Name = "txtCharacterNotes";
            this.txtCharacterNotes.ReadOnly = true;
            this.txtCharacterNotes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtCharacterNotes.Size = new System.Drawing.Size(445, 170);
            this.txtCharacterNotes.TabIndex = 2;
            this.txtCharacterNotes.Tag = "";
            // 
            // panGameNotes
            // 
            this.panGameNotes.Controls.Add(this.txtGameNotes);
            this.panGameNotes.Location = new System.Drawing.Point(4, 22);
            this.panGameNotes.Name = "panGameNotes";
            this.panGameNotes.Padding = new System.Windows.Forms.Padding(3);
            this.panGameNotes.Size = new System.Drawing.Size(451, 176);
            this.panGameNotes.TabIndex = 4;
            this.panGameNotes.Tag = "Tab_Roster_GameNotes";
            this.panGameNotes.Text = "Game Notes";
            this.panGameNotes.UseVisualStyleBackColor = true;
            // 
            // txtGameNotes
            // 
            this.txtGameNotes.BackColor = System.Drawing.SystemColors.Window;
            this.txtGameNotes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtGameNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtGameNotes.Location = new System.Drawing.Point(3, 3);
            this.txtGameNotes.Multiline = true;
            this.txtGameNotes.Name = "txtGameNotes";
            this.txtGameNotes.ReadOnly = true;
            this.txtGameNotes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtGameNotes.Size = new System.Drawing.Size(445, 170);
            this.txtGameNotes.TabIndex = 3;
            this.txtGameNotes.Tag = "";
            // 
            // tlpMain
            // 
            this.tlpMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMain.AutoSize = true;
            this.tlpMain.ColumnCount = 4;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 301F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.tabCharacterText, 1, 6);
            this.tlpMain.Controls.Add(this.treCharacterList, 0, 0);
            this.tlpMain.Controls.Add(this.lblHeroLabTrademark, 1, 7);
            this.tlpMain.Controls.Add(this.picMugshot, 3, 0);
            this.tlpMain.Controls.Add(this.lblEssence, 2, 5);
            this.tlpMain.Controls.Add(this.lblCharacterName, 2, 0);
            this.tlpMain.Controls.Add(this.lblEssenceLabel, 1, 5);
            this.tlpMain.Controls.Add(this.lblCharacterNameLabel, 1, 0);
            this.tlpMain.Controls.Add(this.lblCareerKarma, 2, 4);
            this.tlpMain.Controls.Add(this.lblPlayerNameLabel, 1, 2);
            this.tlpMain.Controls.Add(this.lblCareerKarmaLabel, 1, 4);
            this.tlpMain.Controls.Add(this.lblCharacterAliasLabel, 1, 1);
            this.tlpMain.Controls.Add(this.lblMetatype, 2, 3);
            this.tlpMain.Controls.Add(this.lblCharacterAlias, 2, 1);
            this.tlpMain.Controls.Add(this.lblMetatypeLabel, 1, 3);
            this.tlpMain.Controls.Add(this.lblPlayerName, 2, 2);
            this.tlpMain.Controls.Add(this.flpButtons, 0, 7);
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 8;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(766, 543);
            this.tlpMain.TabIndex = 55;
            // 
            // flpButtons
            // 
            this.flpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.flpButtons.AutoSize = true;
            this.flpButtons.Controls.Add(this.cmdSelectFile);
            this.flpButtons.Controls.Add(this.cmdImport);
            this.flpButtons.Location = new System.Drawing.Point(0, 514);
            this.flpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.flpButtons.Name = "flpButtons";
            this.flpButtons.Size = new System.Drawing.Size(199, 29);
            this.flpButtons.TabIndex = 53;
            // 
            // frmHeroLabImporter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.tlpMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmHeroLabImporter";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.Tag = "Title_HeroLabImporter";
            this.Text = "Hero Lab Importer";
            ((System.ComponentModel.ISupportInitialize)(this.picMugshot)).EndInit();
            this.tabCharacterText.ResumeLayout(false);
            this.panCharacterBio.ResumeLayout(false);
            this.panCharacterBio.PerformLayout();
            this.panCharacterConcept.ResumeLayout(false);
            this.panCharacterConcept.PerformLayout();
            this.panCharacterBackground.ResumeLayout(false);
            this.panCharacterBackground.PerformLayout();
            this.panCharacterNotes.ResumeLayout(false);
            this.panCharacterNotes.PerformLayout();
            this.panGameNotes.ResumeLayout(false);
            this.panGameNotes.PerformLayout();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.flpButtons.ResumeLayout(false);
            this.flpButtons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treCharacterList;
        private System.Windows.Forms.Button cmdSelectFile;
        private System.Windows.Forms.Button cmdImport;
        private System.Windows.Forms.Label lblEssence;
        private System.Windows.Forms.Label lblEssenceLabel;
        private System.Windows.Forms.Label lblCharacterAlias;
        private System.Windows.Forms.Label lblCharacterAliasLabel;
        private System.Windows.Forms.Label lblPlayerName;
        private System.Windows.Forms.Label lblPlayerNameLabel;
        private System.Windows.Forms.Label lblCareerKarma;
        private System.Windows.Forms.Label lblCareerKarmaLabel;
        private System.Windows.Forms.Label lblMetatype;
        private System.Windows.Forms.Label lblMetatypeLabel;
        private System.Windows.Forms.Label lblCharacterName;
        private System.Windows.Forms.Label lblCharacterNameLabel;
        private System.Windows.Forms.PictureBox picMugshot;
        private System.Windows.Forms.Label lblHeroLabTrademark;
        private System.Windows.Forms.TabControl tabCharacterText;
        private System.Windows.Forms.TabPage panCharacterBio;
        private System.Windows.Forms.TextBox txtCharacterBio;
        private System.Windows.Forms.TabPage panCharacterConcept;
        private System.Windows.Forms.TextBox txtCharacterConcept;
        private System.Windows.Forms.TabPage panCharacterBackground;
        private System.Windows.Forms.TextBox txtCharacterBackground;
        private System.Windows.Forms.TabPage panCharacterNotes;
        private System.Windows.Forms.TextBox txtCharacterNotes;
        private System.Windows.Forms.TabPage panGameNotes;
        private System.Windows.Forms.TextBox txtGameNotes;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.FlowLayoutPanel flpButtons;
    }
}
