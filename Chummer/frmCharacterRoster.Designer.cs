
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
                watcherCharacterRosterFolder?.Dispose();
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
            this.tabCharacterText = new System.Windows.Forms.TabControl();
            this.panCharacterBio = new System.Windows.Forms.TabPage();
            this.rtbCharacterBio = new System.Windows.Forms.RichTextBox();
            this.panCharacterConcept = new System.Windows.Forms.TabPage();
            this.rtbCharacterConcept = new System.Windows.Forms.RichTextBox();
            this.panCharacterBackground = new System.Windows.Forms.TabPage();
            this.rtbCharacterBackground = new System.Windows.Forms.RichTextBox();
            this.panCharacterNotes = new System.Windows.Forms.TabPage();
            this.rtbCharacterNotes = new System.Windows.Forms.RichTextBox();
            this.panGameNotes = new System.Windows.Forms.TabPage();
            this.rtbGameNotes = new System.Windows.Forms.RichTextBox();
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
            this.lblSettings = new System.Windows.Forms.Label();
            this.lblSettingsLabel = new System.Windows.Forms.Label();
            this.tlpCharacterRoster = new Chummer.BufferedTableLayoutPanel(this.components);
            this.picMugshot = new System.Windows.Forms.PictureBox();
            this.tabCharacterText.SuspendLayout();
            this.panCharacterBio.SuspendLayout();
            this.panCharacterConcept.SuspendLayout();
            this.panCharacterBackground.SuspendLayout();
            this.panCharacterNotes.SuspendLayout();
            this.panGameNotes.SuspendLayout();
            this.tlpCharacterRoster.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picMugshot)).BeginInit();
            this.SuspendLayout();
            // 
            // tabCharacterText
            // 
            this.tlpCharacterRoster.SetColumnSpan(this.tabCharacterText, 3);
            this.tabCharacterText.Controls.Add(this.panCharacterBio);
            this.tabCharacterText.Controls.Add(this.panCharacterConcept);
            this.tabCharacterText.Controls.Add(this.panCharacterBackground);
            this.tabCharacterText.Controls.Add(this.panCharacterNotes);
            this.tabCharacterText.Controls.Add(this.panGameNotes);
            this.tabCharacterText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabCharacterText.Location = new System.Drawing.Point(304, 309);
            this.tabCharacterText.Name = "tabCharacterText";
            this.tabCharacterText.SelectedIndex = 0;
            this.tabCharacterText.Size = new System.Drawing.Size(459, 231);
            this.tabCharacterText.TabIndex = 22;
            // 
            // panCharacterBio
            // 
            this.panCharacterBio.Controls.Add(this.rtbCharacterBio);
            this.panCharacterBio.Location = new System.Drawing.Point(4, 22);
            this.panCharacterBio.Name = "panCharacterBio";
            this.panCharacterBio.Padding = new System.Windows.Forms.Padding(3);
            this.panCharacterBio.Size = new System.Drawing.Size(451, 205);
            this.panCharacterBio.TabIndex = 0;
            this.panCharacterBio.Tag = "Tab_Roster_Description";
            this.panCharacterBio.Text = "Description";
            this.panCharacterBio.UseVisualStyleBackColor = true;
            // 
            // rtbCharacterBio
            // 
            this.rtbCharacterBio.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtbCharacterBio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbCharacterBio.Location = new System.Drawing.Point(3, 3);
            this.rtbCharacterBio.Name = "rtbCharacterBio";
            this.rtbCharacterBio.ReadOnly = true;
            this.rtbCharacterBio.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbCharacterBio.Size = new System.Drawing.Size(445, 199);
            this.rtbCharacterBio.TabIndex = 0;
            this.rtbCharacterBio.Text = "";
            // 
            // panCharacterConcept
            // 
            this.panCharacterConcept.Controls.Add(this.rtbCharacterConcept);
            this.panCharacterConcept.Location = new System.Drawing.Point(4, 22);
            this.panCharacterConcept.Name = "panCharacterConcept";
            this.panCharacterConcept.Padding = new System.Windows.Forms.Padding(3);
            this.panCharacterConcept.Size = new System.Drawing.Size(451, 205);
            this.panCharacterConcept.TabIndex = 1;
            this.panCharacterConcept.Tag = "Tab_Roster_Concept";
            this.panCharacterConcept.Text = "Concept";
            this.panCharacterConcept.UseVisualStyleBackColor = true;
            // 
            // rtbCharacterConcept
            // 
            this.rtbCharacterConcept.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtbCharacterConcept.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbCharacterConcept.Location = new System.Drawing.Point(3, 3);
            this.rtbCharacterConcept.Name = "rtbCharacterConcept";
            this.rtbCharacterConcept.ReadOnly = true;
            this.rtbCharacterConcept.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbCharacterConcept.Size = new System.Drawing.Size(445, 199);
            this.rtbCharacterConcept.TabIndex = 0;
            this.rtbCharacterConcept.Text = "";
            // 
            // panCharacterBackground
            // 
            this.panCharacterBackground.Controls.Add(this.rtbCharacterBackground);
            this.panCharacterBackground.Location = new System.Drawing.Point(4, 22);
            this.panCharacterBackground.Name = "panCharacterBackground";
            this.panCharacterBackground.Padding = new System.Windows.Forms.Padding(3);
            this.panCharacterBackground.Size = new System.Drawing.Size(451, 205);
            this.panCharacterBackground.TabIndex = 2;
            this.panCharacterBackground.Tag = "Tab_Roster_Background";
            this.panCharacterBackground.Text = "Background";
            this.panCharacterBackground.UseVisualStyleBackColor = true;
            // 
            // rtbCharacterBackground
            // 
            this.rtbCharacterBackground.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtbCharacterBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbCharacterBackground.Location = new System.Drawing.Point(3, 3);
            this.rtbCharacterBackground.Name = "rtbCharacterBackground";
            this.rtbCharacterBackground.ReadOnly = true;
            this.rtbCharacterBackground.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbCharacterBackground.Size = new System.Drawing.Size(445, 199);
            this.rtbCharacterBackground.TabIndex = 0;
            this.rtbCharacterBackground.Text = "";
            // 
            // panCharacterNotes
            // 
            this.panCharacterNotes.Controls.Add(this.rtbCharacterNotes);
            this.panCharacterNotes.Location = new System.Drawing.Point(4, 22);
            this.panCharacterNotes.Name = "panCharacterNotes";
            this.panCharacterNotes.Padding = new System.Windows.Forms.Padding(3);
            this.panCharacterNotes.Size = new System.Drawing.Size(451, 205);
            this.panCharacterNotes.TabIndex = 3;
            this.panCharacterNotes.Tag = "Tab_Roster_CharacterNotes";
            this.panCharacterNotes.Text = "Character Notes";
            this.panCharacterNotes.UseVisualStyleBackColor = true;
            // 
            // rtbCharacterNotes
            // 
            this.rtbCharacterNotes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtbCharacterNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbCharacterNotes.Location = new System.Drawing.Point(3, 3);
            this.rtbCharacterNotes.Name = "rtbCharacterNotes";
            this.rtbCharacterNotes.ReadOnly = true;
            this.rtbCharacterNotes.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbCharacterNotes.Size = new System.Drawing.Size(445, 199);
            this.rtbCharacterNotes.TabIndex = 0;
            this.rtbCharacterNotes.Text = "";
            // 
            // panGameNotes
            // 
            this.panGameNotes.Controls.Add(this.rtbGameNotes);
            this.panGameNotes.Location = new System.Drawing.Point(4, 22);
            this.panGameNotes.Name = "panGameNotes";
            this.panGameNotes.Padding = new System.Windows.Forms.Padding(3);
            this.panGameNotes.Size = new System.Drawing.Size(451, 205);
            this.panGameNotes.TabIndex = 4;
            this.panGameNotes.Tag = "Tab_Roster_GameNotes";
            this.panGameNotes.Text = "Game Notes";
            this.panGameNotes.UseVisualStyleBackColor = true;
            // 
            // rtbGameNotes
            // 
            this.rtbGameNotes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtbGameNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbGameNotes.Location = new System.Drawing.Point(3, 3);
            this.rtbGameNotes.Name = "rtbGameNotes";
            this.rtbGameNotes.ReadOnly = true;
            this.rtbGameNotes.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbGameNotes.Size = new System.Drawing.Size(445, 199);
            this.rtbGameNotes.TabIndex = 0;
            this.rtbGameNotes.Text = "";
            // 
            // lblCharacterName
            // 
            this.lblCharacterName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCharacterName.AutoSize = true;
            this.lblCharacterName.Location = new System.Drawing.Point(397, 6);
            this.lblCharacterName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCharacterName.Name = "lblCharacterName";
            this.lblCharacterName.Size = new System.Drawing.Size(39, 13);
            this.lblCharacterName.TabIndex = 24;
            this.lblCharacterName.Text = "[None]";
            this.lblCharacterName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCharacterNameLabel
            // 
            this.lblCharacterNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCharacterNameLabel.AutoSize = true;
            this.lblCharacterNameLabel.Location = new System.Drawing.Point(304, 6);
            this.lblCharacterNameLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCharacterNameLabel.Name = "lblCharacterNameLabel";
            this.lblCharacterNameLabel.Size = new System.Drawing.Size(87, 13);
            this.lblCharacterNameLabel.TabIndex = 23;
            this.lblCharacterNameLabel.Tag = "Label_CharacterName";
            this.lblCharacterNameLabel.Text = "Character Name:";
            this.lblCharacterNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblMetatype
            // 
            this.lblMetatype.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMetatype.AutoSize = true;
            this.lblMetatype.Location = new System.Drawing.Point(397, 81);
            this.lblMetatype.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetatype.Name = "lblMetatype";
            this.lblMetatype.Size = new System.Drawing.Size(39, 13);
            this.lblMetatype.TabIndex = 26;
            this.lblMetatype.Text = "[None]";
            this.lblMetatype.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMetatypeLabel
            // 
            this.lblMetatypeLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMetatypeLabel.AutoSize = true;
            this.lblMetatypeLabel.Location = new System.Drawing.Point(337, 81);
            this.lblMetatypeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetatypeLabel.Name = "lblMetatypeLabel";
            this.lblMetatypeLabel.Size = new System.Drawing.Size(54, 13);
            this.lblMetatypeLabel.TabIndex = 25;
            this.lblMetatypeLabel.Tag = "Label_Metatype";
            this.lblMetatypeLabel.Text = "Metatype:";
            this.lblMetatypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblCareerKarma
            // 
            this.lblCareerKarma.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCareerKarma.AutoSize = true;
            this.lblCareerKarma.Location = new System.Drawing.Point(397, 106);
            this.lblCareerKarma.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCareerKarma.Name = "lblCareerKarma";
            this.lblCareerKarma.Size = new System.Drawing.Size(39, 13);
            this.lblCareerKarma.TabIndex = 28;
            this.lblCareerKarma.Text = "[None]";
            this.lblCareerKarma.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCareerKarmaLabel
            // 
            this.lblCareerKarmaLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCareerKarmaLabel.AutoSize = true;
            this.lblCareerKarmaLabel.Location = new System.Drawing.Point(317, 106);
            this.lblCareerKarmaLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCareerKarmaLabel.Name = "lblCareerKarmaLabel";
            this.lblCareerKarmaLabel.Size = new System.Drawing.Size(74, 13);
            this.lblCareerKarmaLabel.TabIndex = 27;
            this.lblCareerKarmaLabel.Tag = "String_CareerKarma";
            this.lblCareerKarmaLabel.Text = "Career Karma:";
            this.lblCareerKarmaLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblPlayerName
            // 
            this.lblPlayerName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblPlayerName.AutoSize = true;
            this.lblPlayerName.Location = new System.Drawing.Point(397, 56);
            this.lblPlayerName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPlayerName.Name = "lblPlayerName";
            this.lblPlayerName.Size = new System.Drawing.Size(39, 13);
            this.lblPlayerName.TabIndex = 32;
            this.lblPlayerName.Text = "[None]";
            this.lblPlayerName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPlayerNameLabel
            // 
            this.lblPlayerNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblPlayerNameLabel.AutoSize = true;
            this.lblPlayerNameLabel.Location = new System.Drawing.Point(352, 56);
            this.lblPlayerNameLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPlayerNameLabel.Name = "lblPlayerNameLabel";
            this.lblPlayerNameLabel.Size = new System.Drawing.Size(39, 13);
            this.lblPlayerNameLabel.TabIndex = 31;
            this.lblPlayerNameLabel.Tag = "Label_Player";
            this.lblPlayerNameLabel.Text = "Player:";
            this.lblPlayerNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblCharacterAlias
            // 
            this.lblCharacterAlias.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCharacterAlias.AutoSize = true;
            this.lblCharacterAlias.Location = new System.Drawing.Point(397, 31);
            this.lblCharacterAlias.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCharacterAlias.Name = "lblCharacterAlias";
            this.lblCharacterAlias.Size = new System.Drawing.Size(39, 13);
            this.lblCharacterAlias.TabIndex = 34;
            this.lblCharacterAlias.Text = "[None]";
            this.lblCharacterAlias.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCharacterAliasLabel
            // 
            this.lblCharacterAliasLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCharacterAliasLabel.AutoSize = true;
            this.lblCharacterAliasLabel.Location = new System.Drawing.Point(359, 31);
            this.lblCharacterAliasLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCharacterAliasLabel.Name = "lblCharacterAliasLabel";
            this.lblCharacterAliasLabel.Size = new System.Drawing.Size(32, 13);
            this.lblCharacterAliasLabel.TabIndex = 33;
            this.lblCharacterAliasLabel.Tag = "Label_Alias";
            this.lblCharacterAliasLabel.Text = "Alias:";
            this.lblCharacterAliasLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblEssence
            // 
            this.lblEssence.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblEssence.AutoSize = true;
            this.lblEssence.Location = new System.Drawing.Point(397, 131);
            this.lblEssence.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblEssence.Name = "lblEssence";
            this.lblEssence.Size = new System.Drawing.Size(39, 13);
            this.lblEssence.TabIndex = 36;
            this.lblEssence.Text = "[None]";
            this.lblEssence.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblEssenceLabel
            // 
            this.lblEssenceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblEssenceLabel.AutoSize = true;
            this.lblEssenceLabel.Location = new System.Drawing.Point(340, 131);
            this.lblEssenceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblEssenceLabel.Name = "lblEssenceLabel";
            this.lblEssenceLabel.Size = new System.Drawing.Size(51, 13);
            this.lblEssenceLabel.TabIndex = 35;
            this.lblEssenceLabel.Tag = "Label_Essence";
            this.lblEssenceLabel.Text = "Essence:";
            this.lblEssenceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblFilePath
            // 
            this.lblFilePath.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblFilePath.AutoSize = true;
            this.lblFilePath.Location = new System.Drawing.Point(397, 156);
            this.lblFilePath.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblFilePath.Name = "lblFilePath";
            this.lblFilePath.Size = new System.Drawing.Size(39, 13);
            this.lblFilePath.TabIndex = 38;
            this.lblFilePath.Text = "[None]";
            this.lblFilePath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblFilePathLabel
            // 
            this.lblFilePathLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblFilePathLabel.AutoSize = true;
            this.lblFilePathLabel.Location = new System.Drawing.Point(337, 156);
            this.lblFilePathLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblFilePathLabel.Name = "lblFilePathLabel";
            this.lblFilePathLabel.Size = new System.Drawing.Size(54, 13);
            this.lblFilePathLabel.TabIndex = 37;
            this.lblFilePathLabel.Tag = "Label_Roster_File_Name";
            this.lblFilePathLabel.Text = "File Name";
            this.lblFilePathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // treCharacterList
            // 
            this.treCharacterList.AllowDrop = true;
            this.treCharacterList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treCharacterList.Location = new System.Drawing.Point(3, 3);
            this.treCharacterList.Name = "treCharacterList";
            this.tlpCharacterRoster.SetRowSpan(this.treCharacterList, 10);
            this.treCharacterList.ShowNodeToolTips = true;
            this.treCharacterList.Size = new System.Drawing.Size(295, 537);
            this.treCharacterList.TabIndex = 0;
            this.treCharacterList.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treCharacterList_AfterSelect);
            this.treCharacterList.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TreCharacterList_NodeMouseClick);
            this.treCharacterList.DoubleClick += new System.EventHandler(this.treCharacterList_DoubleClick);
            this.treCharacterList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treCharacterList_OnDefaultKeyDown);
            this.treCharacterList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            // 
            // lblSettings
            // 
            this.lblSettings.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSettings.AutoSize = true;
            this.lblSettings.Location = new System.Drawing.Point(397, 181);
            this.lblSettings.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSettings.Name = "lblSettings";
            this.lblSettings.Size = new System.Drawing.Size(39, 13);
            this.lblSettings.TabIndex = 40;
            this.lblSettings.Text = "[None]";
            this.lblSettings.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSettingsLabel
            // 
            this.lblSettingsLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSettingsLabel.AutoSize = true;
            this.lblSettingsLabel.Location = new System.Drawing.Point(324, 181);
            this.lblSettingsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSettingsLabel.Name = "lblSettingsLabel";
            this.lblSettingsLabel.Size = new System.Drawing.Size(67, 13);
            this.lblSettingsLabel.TabIndex = 39;
            this.lblSettingsLabel.Tag = "Label_Roster_Settings_File";
            this.lblSettingsLabel.Text = "Settings File:";
            this.lblSettingsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tlpCharacterRoster
            // 
            this.tlpCharacterRoster.AutoSize = true;
            this.tlpCharacterRoster.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpCharacterRoster.ColumnCount = 4;
            this.tlpCharacterRoster.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCharacterRoster.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCharacterRoster.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCharacterRoster.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCharacterRoster.Controls.Add(this.lblSettings, 2, 7);
            this.tlpCharacterRoster.Controls.Add(this.tabCharacterText, 1, 9);
            this.tlpCharacterRoster.Controls.Add(this.lblCharacterAliasLabel, 1, 1);
            this.tlpCharacterRoster.Controls.Add(this.treCharacterList, 0, 0);
            this.tlpCharacterRoster.Controls.Add(this.picMugshot, 3, 0);
            this.tlpCharacterRoster.Controls.Add(this.lblSettingsLabel, 1, 7);
            this.tlpCharacterRoster.Controls.Add(this.lblCharacterAlias, 2, 1);
            this.tlpCharacterRoster.Controls.Add(this.lblFilePath, 2, 6);
            this.tlpCharacterRoster.Controls.Add(this.lblPlayerNameLabel, 1, 2);
            this.tlpCharacterRoster.Controls.Add(this.lblFilePathLabel, 1, 6);
            this.tlpCharacterRoster.Controls.Add(this.lblPlayerName, 2, 2);
            this.tlpCharacterRoster.Controls.Add(this.lblEssence, 2, 5);
            this.tlpCharacterRoster.Controls.Add(this.lblMetatypeLabel, 1, 3);
            this.tlpCharacterRoster.Controls.Add(this.lblEssenceLabel, 1, 5);
            this.tlpCharacterRoster.Controls.Add(this.lblMetatype, 2, 3);
            this.tlpCharacterRoster.Controls.Add(this.lblCareerKarma, 2, 4);
            this.tlpCharacterRoster.Controls.Add(this.lblCareerKarmaLabel, 1, 4);
            this.tlpCharacterRoster.Controls.Add(this.lblCharacterNameLabel, 1, 0);
            this.tlpCharacterRoster.Controls.Add(this.lblCharacterName, 2, 0);
            this.tlpCharacterRoster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCharacterRoster.Location = new System.Drawing.Point(9, 9);
            this.tlpCharacterRoster.Name = "tlpCharacterRoster";
            this.tlpCharacterRoster.RowCount = 10;
            this.tlpCharacterRoster.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterRoster.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterRoster.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterRoster.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterRoster.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterRoster.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterRoster.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterRoster.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterRoster.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterRoster.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCharacterRoster.Size = new System.Drawing.Size(766, 543);
            this.tlpCharacterRoster.TabIndex = 41;
            // 
            // picMugshot
            // 
            this.picMugshot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picMugshot.Location = new System.Drawing.Point(463, 3);
            this.picMugshot.Name = "picMugshot";
            this.tlpCharacterRoster.SetRowSpan(this.picMugshot, 9);
            this.picMugshot.Size = new System.Drawing.Size(300, 300);
            this.picMugshot.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picMugshot.TabIndex = 21;
            this.picMugshot.TabStop = false;
            this.picMugshot.SizeChanged += new System.EventHandler(this.picMugshot_SizeChanged);
            // 
            // frmCharacterRoster
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.ControlBox = false;
            this.Controls.Add(this.tlpCharacterRoster);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "frmCharacterRoster";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.Tag = "String_CharacterRoster";
            this.Text = "Character Roster";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmCharacterRoster_FormClosing);
            this.Load += new System.EventHandler(this.frmCharacterRoster_Load);
            this.tabCharacterText.ResumeLayout(false);
            this.panCharacterBio.ResumeLayout(false);
            this.panCharacterConcept.ResumeLayout(false);
            this.panCharacterBackground.ResumeLayout(false);
            this.panCharacterNotes.ResumeLayout(false);
            this.panGameNotes.ResumeLayout(false);
            this.tlpCharacterRoster.ResumeLayout(false);
            this.tlpCharacterRoster.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picMugshot)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TabControl tabCharacterText;
        private System.Windows.Forms.TabPage panCharacterBio;
        private System.Windows.Forms.TabPage panCharacterConcept;
        private System.Windows.Forms.TabPage panCharacterBackground;
        private System.Windows.Forms.TabPage panCharacterNotes;
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
      
        private Chummer.BufferedTableLayoutPanel tlpCharacterRoster;
        private System.Windows.Forms.Label lblSettings;
        private System.Windows.Forms.Label lblSettingsLabel;
        private System.Windows.Forms.PictureBox picMugshot;
        public System.Windows.Forms.TreeView treCharacterList;
        private System.Windows.Forms.RichTextBox rtbCharacterBio;
        private System.Windows.Forms.RichTextBox rtbCharacterConcept;
        private System.Windows.Forms.RichTextBox rtbCharacterBackground;
        private System.Windows.Forms.RichTextBox rtbCharacterNotes;
        private System.Windows.Forms.RichTextBox rtbGameNotes;
    }
}

