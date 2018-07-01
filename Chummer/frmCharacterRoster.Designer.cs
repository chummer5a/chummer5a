using System;

namespace Chummer
{
    partial class frmCharacterRoster
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
            if (disposing)
            {
                components?.Dispose();
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
            this.picMugshot = new System.Windows.Forms.PictureBox();
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
            this.lblCharacterName = new System.Windows.Forms.Label();
            this.lblCharacterNameLabel = new System.Windows.Forms.Label();
            this.lblMetatype = new System.Windows.Forms.Label();
            this.lblMetatypeLabel = new System.Windows.Forms.Label();
            this.lblCareerKarma = new System.Windows.Forms.Label();
            this.lblCareerKarmaLabel = new System.Windows.Forms.Label();
            this.lblPlayerName = new System.Windows.Forms.Label();
            this.lblPlayerNameLabel = new System.Windows.Forms.Label();
            this.lblCharacterAlias = new System.Windows.Forms.Label();
            this.lblCharacterAliasLabel = new System.Windows.Forms.Label();
            this.lblEssence = new System.Windows.Forms.Label();
            this.lblEssenceLabel = new System.Windows.Forms.Label();
            this.lblFilePath = new System.Windows.Forms.Label();
            this.lblFilePathLabel = new System.Windows.Forms.Label();
            this.treCharacterList = new System.Windows.Forms.TreeView();
            this.cmsRoster = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsToggleFav = new System.Windows.Forms.ToolStripMenuItem();
            this.tsSort = new System.Windows.Forms.ToolStripMenuItem();
            this.tsDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.tsCloseOpenCharacter = new System.Windows.Forms.ToolStripMenuItem();
            this.lblSettings = new System.Windows.Forms.Label();
            this.lblSettingsLabel = new System.Windows.Forms.Label();
            this.tlpCharacterBlock = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.picMugshot)).BeginInit();
            this.tabCharacterText.SuspendLayout();
            this.panCharacterBio.SuspendLayout();
            this.panCharacterConcept.SuspendLayout();
            this.panCharacterBackground.SuspendLayout();
            this.panCharacterNotes.SuspendLayout();
            this.panGameNotes.SuspendLayout();
            this.cmsRoster.SuspendLayout();
            this.tlpCharacterBlock.SuspendLayout();
            this.SuspendLayout();
            // 
            // picMugshot
            // 
            this.picMugshot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picMugshot.Location = new System.Drawing.Point(448, 8);
            this.picMugshot.Margin = new System.Windows.Forms.Padding(13);
            this.picMugshot.MinimumSize = new System.Drawing.Size(21, 31);
            this.picMugshot.Name = "picMugshot";
            this.picMugshot.Size = new System.Drawing.Size(324, 324);
            this.picMugshot.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picMugshot.TabIndex = 21;
            this.picMugshot.TabStop = false;
            this.picMugshot.SizeChanged += new System.EventHandler(this.picMugshot_SizeChanged);
            // 
            // tabCharacterText
            // 
            this.tabCharacterText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabCharacterText.Controls.Add(this.panCharacterBio);
            this.tabCharacterText.Controls.Add(this.panCharacterConcept);
            this.tabCharacterText.Controls.Add(this.panCharacterBackground);
            this.tabCharacterText.Controls.Add(this.panCharacterNotes);
            this.tabCharacterText.Controls.Add(this.panGameNotes);
            this.tabCharacterText.Location = new System.Drawing.Point(208, 336);
            this.tabCharacterText.Name = "tabCharacterText";
            this.tabCharacterText.SelectedIndex = 0;
            this.tabCharacterText.Size = new System.Drawing.Size(568, 208);
            this.tabCharacterText.TabIndex = 22;
            // 
            // panCharacterBio
            // 
            this.panCharacterBio.Controls.Add(this.txtCharacterBio);
            this.panCharacterBio.Location = new System.Drawing.Point(4, 22);
            this.panCharacterBio.Name = "panCharacterBio";
            this.panCharacterBio.Padding = new System.Windows.Forms.Padding(3);
            this.panCharacterBio.Size = new System.Drawing.Size(560, 182);
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
            this.txtCharacterBio.Size = new System.Drawing.Size(554, 176);
            this.txtCharacterBio.TabIndex = 1;
            // 
            // panCharacterConcept
            // 
            this.panCharacterConcept.Controls.Add(this.txtCharacterConcept);
            this.panCharacterConcept.Location = new System.Drawing.Point(4, 22);
            this.panCharacterConcept.Name = "panCharacterConcept";
            this.panCharacterConcept.Padding = new System.Windows.Forms.Padding(3);
            this.panCharacterConcept.Size = new System.Drawing.Size(560, 182);
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
            this.txtCharacterConcept.Size = new System.Drawing.Size(554, 176);
            this.txtCharacterConcept.TabIndex = 2;
            this.txtCharacterConcept.Tag = "";
            // 
            // panCharacterBackground
            // 
            this.panCharacterBackground.Controls.Add(this.txtCharacterBackground);
            this.panCharacterBackground.Location = new System.Drawing.Point(4, 22);
            this.panCharacterBackground.Name = "panCharacterBackground";
            this.panCharacterBackground.Padding = new System.Windows.Forms.Padding(3);
            this.panCharacterBackground.Size = new System.Drawing.Size(560, 182);
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
            this.txtCharacterBackground.Size = new System.Drawing.Size(554, 176);
            this.txtCharacterBackground.TabIndex = 0;
            this.txtCharacterBackground.Tag = "";
            // 
            // panCharacterNotes
            // 
            this.panCharacterNotes.Controls.Add(this.txtCharacterNotes);
            this.panCharacterNotes.Location = new System.Drawing.Point(4, 22);
            this.panCharacterNotes.Name = "panCharacterNotes";
            this.panCharacterNotes.Padding = new System.Windows.Forms.Padding(3);
            this.panCharacterNotes.Size = new System.Drawing.Size(560, 182);
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
            this.txtCharacterNotes.Size = new System.Drawing.Size(554, 176);
            this.txtCharacterNotes.TabIndex = 2;
            this.txtCharacterNotes.Tag = "";
            // 
            // panGameNotes
            // 
            this.panGameNotes.Controls.Add(this.txtGameNotes);
            this.panGameNotes.Location = new System.Drawing.Point(4, 22);
            this.panGameNotes.Name = "panGameNotes";
            this.panGameNotes.Padding = new System.Windows.Forms.Padding(3);
            this.panGameNotes.Size = new System.Drawing.Size(560, 182);
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
            this.txtGameNotes.Size = new System.Drawing.Size(554, 176);
            this.txtGameNotes.TabIndex = 3;
            this.txtGameNotes.Tag = "";
            // 
            // lblCharacterName
            // 
            this.lblCharacterName.AutoSize = true;
            this.lblCharacterName.Location = new System.Drawing.Point(96, 0);
            this.lblCharacterName.Name = "lblCharacterName";
            this.lblCharacterName.Size = new System.Drawing.Size(33, 13);
            this.lblCharacterName.TabIndex = 24;
            this.lblCharacterName.Text = "None";
            // 
            // lblCharacterNameLabel
            // 
            this.lblCharacterNameLabel.AutoSize = true;
            this.lblCharacterNameLabel.Location = new System.Drawing.Point(3, 0);
            this.lblCharacterNameLabel.Name = "lblCharacterNameLabel";
            this.lblCharacterNameLabel.Size = new System.Drawing.Size(87, 13);
            this.lblCharacterNameLabel.TabIndex = 23;
            this.lblCharacterNameLabel.Tag = "Label_CharacterName";
            this.lblCharacterNameLabel.Text = "Character Name:";
            // 
            // lblMetatype
            // 
            this.lblMetatype.AutoSize = true;
            this.lblMetatype.Location = new System.Drawing.Point(96, 60);
            this.lblMetatype.Name = "lblMetatype";
            this.lblMetatype.Size = new System.Drawing.Size(33, 13);
            this.lblMetatype.TabIndex = 26;
            this.lblMetatype.Text = "None";
            // 
            // lblMetatypeLabel
            // 
            this.lblMetatypeLabel.AutoSize = true;
            this.lblMetatypeLabel.Location = new System.Drawing.Point(3, 60);
            this.lblMetatypeLabel.Name = "lblMetatypeLabel";
            this.lblMetatypeLabel.Size = new System.Drawing.Size(54, 13);
            this.lblMetatypeLabel.TabIndex = 25;
            this.lblMetatypeLabel.Tag = "Label_Metatype";
            this.lblMetatypeLabel.Text = "Metatype:";
            // 
            // lblCareerKarma
            // 
            this.lblCareerKarma.AutoSize = true;
            this.lblCareerKarma.Location = new System.Drawing.Point(96, 80);
            this.lblCareerKarma.Name = "lblCareerKarma";
            this.lblCareerKarma.Size = new System.Drawing.Size(33, 13);
            this.lblCareerKarma.TabIndex = 28;
            this.lblCareerKarma.Text = "None";
            // 
            // lblCareerKarmaLabel
            // 
            this.lblCareerKarmaLabel.AutoSize = true;
            this.lblCareerKarmaLabel.Location = new System.Drawing.Point(3, 80);
            this.lblCareerKarmaLabel.Name = "lblCareerKarmaLabel";
            this.lblCareerKarmaLabel.Size = new System.Drawing.Size(74, 13);
            this.lblCareerKarmaLabel.TabIndex = 27;
            this.lblCareerKarmaLabel.Tag = "String_CareerKarma";
            this.lblCareerKarmaLabel.Text = "Career Karma:";
            // 
            // lblPlayerName
            // 
            this.lblPlayerName.AutoSize = true;
            this.lblPlayerName.Location = new System.Drawing.Point(96, 40);
            this.lblPlayerName.Name = "lblPlayerName";
            this.lblPlayerName.Size = new System.Drawing.Size(33, 13);
            this.lblPlayerName.TabIndex = 32;
            this.lblPlayerName.Text = "None";
            // 
            // lblPlayerNameLabel
            // 
            this.lblPlayerNameLabel.AutoSize = true;
            this.lblPlayerNameLabel.Location = new System.Drawing.Point(3, 40);
            this.lblPlayerNameLabel.Name = "lblPlayerNameLabel";
            this.lblPlayerNameLabel.Size = new System.Drawing.Size(39, 13);
            this.lblPlayerNameLabel.TabIndex = 31;
            this.lblPlayerNameLabel.Tag = "Label_Player";
            this.lblPlayerNameLabel.Text = "Player:";
            // 
            // lblCharacterAlias
            // 
            this.lblCharacterAlias.AutoSize = true;
            this.lblCharacterAlias.Location = new System.Drawing.Point(96, 20);
            this.lblCharacterAlias.Name = "lblCharacterAlias";
            this.lblCharacterAlias.Size = new System.Drawing.Size(33, 13);
            this.lblCharacterAlias.TabIndex = 34;
            this.lblCharacterAlias.Text = "None";
            // 
            // lblCharacterAliasLabel
            // 
            this.lblCharacterAliasLabel.AutoSize = true;
            this.lblCharacterAliasLabel.Location = new System.Drawing.Point(3, 20);
            this.lblCharacterAliasLabel.Name = "lblCharacterAliasLabel";
            this.lblCharacterAliasLabel.Size = new System.Drawing.Size(32, 13);
            this.lblCharacterAliasLabel.TabIndex = 33;
            this.lblCharacterAliasLabel.Tag = "Label_Alias";
            this.lblCharacterAliasLabel.Text = "Alias:";
            // 
            // lblEssence
            // 
            this.lblEssence.AutoSize = true;
            this.lblEssence.Location = new System.Drawing.Point(96, 100);
            this.lblEssence.Name = "lblEssence";
            this.lblEssence.Size = new System.Drawing.Size(33, 13);
            this.lblEssence.TabIndex = 36;
            this.lblEssence.Text = "None";
            // 
            // lblEssenceLabel
            // 
            this.lblEssenceLabel.AutoSize = true;
            this.lblEssenceLabel.Location = new System.Drawing.Point(3, 100);
            this.lblEssenceLabel.Name = "lblEssenceLabel";
            this.lblEssenceLabel.Size = new System.Drawing.Size(51, 13);
            this.lblEssenceLabel.TabIndex = 35;
            this.lblEssenceLabel.Tag = "Label_Essence";
            this.lblEssenceLabel.Text = "Essence:";
            // 
            // lblFilePath
            // 
            this.lblFilePath.AutoSize = true;
            this.lblFilePath.Location = new System.Drawing.Point(96, 120);
            this.lblFilePath.Name = "lblFilePath";
            this.lblFilePath.Size = new System.Drawing.Size(33, 13);
            this.lblFilePath.TabIndex = 38;
            this.lblFilePath.Text = "None";
            // 
            // lblFilePathLabel
            // 
            this.lblFilePathLabel.AutoSize = true;
            this.lblFilePathLabel.Location = new System.Drawing.Point(3, 120);
            this.lblFilePathLabel.Name = "lblFilePathLabel";
            this.lblFilePathLabel.Size = new System.Drawing.Size(54, 13);
            this.lblFilePathLabel.TabIndex = 37;
            this.lblFilePathLabel.Tag = "Label_Roster_File_Name";
            this.lblFilePathLabel.Text = "File Name";
            // 
            // treCharacterList
            // 
            this.treCharacterList.AllowDrop = true;
            this.treCharacterList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treCharacterList.Location = new System.Drawing.Point(8, 8);
            this.treCharacterList.Name = "treCharacterList";
            this.treCharacterList.ShowNodeToolTips = true;
            this.treCharacterList.Size = new System.Drawing.Size(192, 536);
            this.treCharacterList.TabIndex = 0;
            this.treCharacterList.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treCharacterList_AfterSelect);
            this.treCharacterList.DoubleClick += new System.EventHandler(this.treCharacterList_DoubleClick);
            this.treCharacterList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treCharacterList_KeyDown);
            this.treCharacterList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            // 
            // cmsRoster
            // 
            this.cmsRoster.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsToggleFav,
            this.tsSort,
            this.tsDelete,
            this.tsCloseOpenCharacter});
            this.cmsRoster.Name = "cmsRoster";
            this.cmsRoster.Size = new System.Drawing.Size(178, 92);
            // 
            // tsToggleFav
            // 
            this.tsToggleFav.Image = global::Chummer.Properties.Resources.asterisk_orange;
            this.tsToggleFav.Name = "tsToggleFav";
            this.tsToggleFav.Size = new System.Drawing.Size(177, 22);
            this.tsToggleFav.Tag = "Menu_ToggleFavorite";
            this.tsToggleFav.Text = "&Toggle Favorite";
            this.tsToggleFav.Click += new System.EventHandler(this.tsToggleFav_Click);
            // 
            // tsSort
            // 
            this.tsSort.Image = global::Chummer.Properties.Resources.page_refresh;
            this.tsSort.Name = "tsSort";
            this.tsSort.Size = new System.Drawing.Size(177, 22);
            this.tsSort.Tag = "Menu_Sort";
            this.tsSort.Text = "&Sort Alphabetically";
            this.tsSort.Click += new System.EventHandler(this.tsSort_Click);
            // 
            // tsDelete
            // 
            this.tsDelete.Image = global::Chummer.Properties.Resources.delete;
            this.tsDelete.Name = "tsDelete";
            this.tsDelete.Size = new System.Drawing.Size(177, 22);
            this.tsDelete.Tag = "Menu_Delete";
            this.tsDelete.Text = "&Delete from this list";
            this.tsDelete.Click += new System.EventHandler(this.tsDelete_Click);
            // 
            // tsCloseOpenCharacter
            // 
            this.tsCloseOpenCharacter.Image = global::Chummer.Properties.Resources.door_out;
            this.tsCloseOpenCharacter.Name = "tsCloseOpenCharacter";
            this.tsCloseOpenCharacter.Size = new System.Drawing.Size(177, 22);
            this.tsCloseOpenCharacter.Tag = "Menu_Close";
            this.tsCloseOpenCharacter.Text = "&Close this character";
            this.tsCloseOpenCharacter.Click += new System.EventHandler(this.tsCloseOpenCharacter_Click);
            // 
            // lblSettings
            // 
            this.lblSettings.AutoSize = true;
            this.lblSettings.Location = new System.Drawing.Point(96, 140);
            this.lblSettings.Name = "lblSettings";
            this.lblSettings.Size = new System.Drawing.Size(33, 13);
            this.lblSettings.TabIndex = 40;
            this.lblSettings.Text = "None";
            // 
            // lblSettingsLabel
            // 
            this.lblSettingsLabel.AutoSize = true;
            this.lblSettingsLabel.Location = new System.Drawing.Point(3, 140);
            this.lblSettingsLabel.Name = "lblSettingsLabel";
            this.lblSettingsLabel.Size = new System.Drawing.Size(67, 13);
            this.lblSettingsLabel.TabIndex = 39;
            this.lblSettingsLabel.Tag = "Label_Roster_Settings_File";
            this.lblSettingsLabel.Text = "Settings File:";
            // 
            // tlpCharacterBlock
            // 
            this.tlpCharacterBlock.AutoSize = true;
            this.tlpCharacterBlock.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpCharacterBlock.ColumnCount = 2;
            this.tlpCharacterBlock.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCharacterBlock.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCharacterBlock.Controls.Add(this.lblSettings, 1, 7);
            this.tlpCharacterBlock.Controls.Add(this.lblCharacterAliasLabel, 0, 1);
            this.tlpCharacterBlock.Controls.Add(this.lblSettingsLabel, 0, 7);
            this.tlpCharacterBlock.Controls.Add(this.lblCharacterAlias, 1, 1);
            this.tlpCharacterBlock.Controls.Add(this.lblFilePath, 1, 6);
            this.tlpCharacterBlock.Controls.Add(this.lblPlayerNameLabel, 0, 2);
            this.tlpCharacterBlock.Controls.Add(this.lblFilePathLabel, 0, 6);
            this.tlpCharacterBlock.Controls.Add(this.lblPlayerName, 1, 2);
            this.tlpCharacterBlock.Controls.Add(this.lblEssence, 1, 5);
            this.tlpCharacterBlock.Controls.Add(this.lblMetatypeLabel, 0, 3);
            this.tlpCharacterBlock.Controls.Add(this.lblEssenceLabel, 0, 5);
            this.tlpCharacterBlock.Controls.Add(this.lblMetatype, 1, 3);
            this.tlpCharacterBlock.Controls.Add(this.lblCareerKarma, 1, 4);
            this.tlpCharacterBlock.Controls.Add(this.lblCareerKarmaLabel, 0, 4);
            this.tlpCharacterBlock.Controls.Add(this.lblCharacterNameLabel, 0, 0);
            this.tlpCharacterBlock.Controls.Add(this.lblCharacterName, 1, 0);
            this.tlpCharacterBlock.Location = new System.Drawing.Point(208, 8);
            this.tlpCharacterBlock.Name = "tlpCharacterBlock";
            this.tlpCharacterBlock.RowCount = 8;
            this.tlpCharacterBlock.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpCharacterBlock.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpCharacterBlock.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpCharacterBlock.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpCharacterBlock.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpCharacterBlock.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpCharacterBlock.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpCharacterBlock.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpCharacterBlock.Size = new System.Drawing.Size(132, 160);
            this.tlpCharacterBlock.TabIndex = 41;
            // 
            // frmCharacterRoster
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(786, 552);
            this.ControlBox = false;
            this.Controls.Add(this.tlpCharacterBlock);
            this.Controls.Add(this.tabCharacterText);
            this.Controls.Add(this.picMugshot);
            this.Controls.Add(this.treCharacterList);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(586, 585);
            this.Name = "frmCharacterRoster";
            this.Text = "Character Roster";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmCharacterRoster_FormClosing);
            this.Load += new System.EventHandler(this.frmCharacterRoster_Load);
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
            this.cmsRoster.ResumeLayout(false);
            this.tlpCharacterBlock.ResumeLayout(false);
            this.tlpCharacterBlock.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treCharacterList;
        private System.Windows.Forms.PictureBox picMugshot;
        private System.Windows.Forms.TabControl tabCharacterText;
        private System.Windows.Forms.TabPage panCharacterBio;
        private System.Windows.Forms.TabPage panCharacterConcept;
        private System.Windows.Forms.TabPage panCharacterBackground;
        private System.Windows.Forms.TabPage panCharacterNotes;
        private System.Windows.Forms.TextBox txtCharacterBio;
        private System.Windows.Forms.TextBox txtCharacterBackground;
        private System.Windows.Forms.TextBox txtCharacterConcept;
        private System.Windows.Forms.TextBox txtCharacterNotes;
        private System.Windows.Forms.Label lblCharacterName;
        private System.Windows.Forms.Label lblCharacterNameLabel;
        private System.Windows.Forms.Label lblMetatype;
        private System.Windows.Forms.Label lblMetatypeLabel;
        private System.Windows.Forms.Label lblCareerKarma;
        private System.Windows.Forms.Label lblCareerKarmaLabel;
        private System.Windows.Forms.Label lblPlayerName;
        private System.Windows.Forms.Label lblPlayerNameLabel;
        private System.Windows.Forms.Label lblCharacterAlias;
        private System.Windows.Forms.Label lblCharacterAliasLabel;
        private System.Windows.Forms.Label lblEssence;
        private System.Windows.Forms.Label lblEssenceLabel;
        private System.Windows.Forms.Label lblFilePath;
        private System.Windows.Forms.Label lblFilePathLabel;
        private System.Windows.Forms.TabPage panGameNotes;
        private System.Windows.Forms.TextBox txtGameNotes;
        private System.Windows.Forms.ContextMenuStrip cmsRoster;
        private System.Windows.Forms.ToolStripMenuItem tsToggleFav;
        private System.Windows.Forms.ToolStripMenuItem tsSort;
        private System.Windows.Forms.ToolStripMenuItem tsDelete;
        private System.Windows.Forms.ToolStripMenuItem tsCloseOpenCharacter;
        private System.Windows.Forms.TableLayoutPanel tlpCharacterBlock;
        private System.Windows.Forms.Label lblSettings;
        private System.Windows.Forms.Label lblSettingsLabel;
    }
}
