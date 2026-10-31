
using System;
using System.Threading;

namespace Chummer
{
    partial class CharacterRoster
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
                CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objMostRecentlyUsedsRefreshCancellationTokenSource, null);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                objOldCancellationTokenSource = Interlocked.Exchange(ref _objWatchFolderRefreshCancellationTokenSource, null);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                objOldCancellationTokenSource = Interlocked.Exchange(ref _objUpdateCharacterCancellationTokenSource, null);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                _objGenericFormClosingCancellationTokenSource?.Dispose();
                _watcherCharacterRosterFolderSaves?.Dispose();
                _objCharacterRosterFolderWatcherSemaphore?.Dispose();
                _objCachePurgeReaderWriterLock?.Dispose();
                if (components != null)
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CharacterRoster));
            this.tabCharacterTexts = new System.Windows.Forms.TabControl();
            this.tabCharacterDescription = new System.Windows.Forms.TabPage();
            this.txtCharacterDescription = new System.Windows.Forms.TextBox();
            this.tabCharacterConcept = new System.Windows.Forms.TabPage();
            this.txtCharacterConcept = new System.Windows.Forms.TextBox();
            this.tabCharacterBackground = new System.Windows.Forms.TabPage();
            this.txtCharacterBackground = new System.Windows.Forms.TextBox();
            this.tabCharacterNotes = new System.Windows.Forms.TabPage();
            this.txtCharacterNotes = new System.Windows.Forms.TextBox();
            this.tabGameNotes = new System.Windows.Forms.TabPage();
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
            this.lblFilePath = new Chummer.LabelWithToolTip();
            this.lblFilePathLabel = new System.Windows.Forms.Label();
            this.lblSettings = new System.Windows.Forms.Label();
            this.lblSettingsLabel = new System.Windows.Forms.Label();
            this.tlpRight = new System.Windows.Forms.TableLayoutPanel();
            this.picMugshot = new System.Windows.Forms.PictureBox();
            this.tlpCharacterRoster = new System.Windows.Forms.TableLayoutPanel();
            this.treCharacterList = new System.Windows.Forms.TreeView();
            this.cmsRoster = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsToggleFav = new Chummer.DpiFriendlyToolStripMenuItem();
            this.tsSort = new Chummer.DpiFriendlyToolStripMenuItem();
            this.tsOpen = new Chummer.DpiFriendlyToolStripMenuItem();
            this.tsOpenForPrinting = new Chummer.DpiFriendlyToolStripMenuItem();
            this.tsOpenForExport = new Chummer.DpiFriendlyToolStripMenuItem();
            this.tsDelete = new Chummer.DpiFriendlyToolStripMenuItem();
            this.tsCloseOpenCharacter = new Chummer.DpiFriendlyToolStripMenuItem();
            this.cmsRosterNoClose = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsToggleFavNoClose = new Chummer.DpiFriendlyToolStripMenuItem();
            this.tsSortNoClose = new Chummer.DpiFriendlyToolStripMenuItem();
            this.tsOpenNoClose = new Chummer.DpiFriendlyToolStripMenuItem();
            this.tsOpenForPrintingNoClose = new Chummer.DpiFriendlyToolStripMenuItem();
            this.tsOpenForExportNoClose = new Chummer.DpiFriendlyToolStripMenuItem();
            this.tsDeleteNoClose = new Chummer.DpiFriendlyToolStripMenuItem();
            this.tabCharacterTexts.SuspendLayout();
            this.tabCharacterDescription.SuspendLayout();
            this.tabCharacterConcept.SuspendLayout();
            this.tabCharacterBackground.SuspendLayout();
            this.tabCharacterNotes.SuspendLayout();
            this.tabGameNotes.SuspendLayout();
            this.tlpRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picMugshot)).BeginInit();
            this.tlpCharacterRoster.SuspendLayout();
            this.cmsRoster.SuspendLayout();
            this.cmsRosterNoClose.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabCharacterTexts
            // 
            this.tlpRight.SetColumnSpan(this.tabCharacterTexts, 3);
            this.tabCharacterTexts.Controls.Add(this.tabCharacterDescription);
            this.tabCharacterTexts.Controls.Add(this.tabCharacterConcept);
            this.tabCharacterTexts.Controls.Add(this.tabCharacterBackground);
            this.tabCharacterTexts.Controls.Add(this.tabCharacterNotes);
            this.tabCharacterTexts.Controls.Add(this.tabGameNotes);
            this.tabCharacterTexts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabCharacterTexts.Location = new System.Drawing.Point(3, 309);
            this.tabCharacterTexts.Name = "tabCharacterTexts";
            this.tabCharacterTexts.SelectedIndex = 0;
            this.tabCharacterTexts.Size = new System.Drawing.Size(454, 231);
            this.tabCharacterTexts.TabIndex = 22;
            // 
            // tabCharacterDescription
            // 
            this.tabCharacterDescription.Controls.Add(this.txtCharacterDescription);
            this.tabCharacterDescription.Location = new System.Drawing.Point(4, 22);
            this.tabCharacterDescription.Name = "tabCharacterDescription";
            this.tabCharacterDescription.Padding = new System.Windows.Forms.Padding(3);
            this.tabCharacterDescription.Size = new System.Drawing.Size(446, 205);
            this.tabCharacterDescription.TabIndex = 0;
            this.tabCharacterDescription.Tag = "Tab_Roster_Description";
            this.tabCharacterDescription.Text = "Description";
            this.tabCharacterDescription.UseVisualStyleBackColor = true;
            // 
            // txtCharacterDescription
            // 
            this.txtCharacterDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCharacterDescription.Location = new System.Drawing.Point(3, 3);
            this.txtCharacterDescription.MaxLength = 2147483647;
            this.txtCharacterDescription.Multiline = true;
            this.txtCharacterDescription.Name = "txtCharacterDescription";
            this.txtCharacterDescription.ReadOnly = true;
            this.txtCharacterDescription.Size = new System.Drawing.Size(440, 199);
            this.txtCharacterDescription.TabIndex = 0;
            // 
            // tabCharacterConcept
            // 
            this.tabCharacterConcept.Controls.Add(this.txtCharacterConcept);
            this.tabCharacterConcept.Location = new System.Drawing.Point(4, 22);
            this.tabCharacterConcept.Name = "tabCharacterConcept";
            this.tabCharacterConcept.Padding = new System.Windows.Forms.Padding(3);
            this.tabCharacterConcept.Size = new System.Drawing.Size(446, 205);
            this.tabCharacterConcept.TabIndex = 1;
            this.tabCharacterConcept.Tag = "Tab_Roster_Concept";
            this.tabCharacterConcept.Text = "Concept";
            this.tabCharacterConcept.UseVisualStyleBackColor = true;
            // 
            // txtCharacterConcept
            // 
            this.txtCharacterConcept.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCharacterConcept.Location = new System.Drawing.Point(3, 3);
            this.txtCharacterConcept.MaxLength = 2147483647;
            this.txtCharacterConcept.Multiline = true;
            this.txtCharacterConcept.Name = "txtCharacterConcept";
            this.txtCharacterConcept.ReadOnly = true;
            this.txtCharacterConcept.Size = new System.Drawing.Size(440, 199);
            this.txtCharacterConcept.TabIndex = 0;
            // 
            // tabCharacterBackground
            // 
            this.tabCharacterBackground.Controls.Add(this.txtCharacterBackground);
            this.tabCharacterBackground.Location = new System.Drawing.Point(4, 22);
            this.tabCharacterBackground.Name = "tabCharacterBackground";
            this.tabCharacterBackground.Padding = new System.Windows.Forms.Padding(3);
            this.tabCharacterBackground.Size = new System.Drawing.Size(446, 205);
            this.tabCharacterBackground.TabIndex = 2;
            this.tabCharacterBackground.Tag = "Tab_Roster_Background";
            this.tabCharacterBackground.Text = "Background";
            this.tabCharacterBackground.UseVisualStyleBackColor = true;
            // 
            // txtCharacterBackground
            // 
            this.txtCharacterBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCharacterBackground.Location = new System.Drawing.Point(3, 3);
            this.txtCharacterBackground.MaxLength = 2147483647;
            this.txtCharacterBackground.Multiline = true;
            this.txtCharacterBackground.Name = "txtCharacterBackground";
            this.txtCharacterBackground.ReadOnly = true;
            this.txtCharacterBackground.Size = new System.Drawing.Size(440, 199);
            this.txtCharacterBackground.TabIndex = 0;
            // 
            // tabCharacterNotes
            // 
            this.tabCharacterNotes.Controls.Add(this.txtCharacterNotes);
            this.tabCharacterNotes.Location = new System.Drawing.Point(4, 22);
            this.tabCharacterNotes.Name = "tabCharacterNotes";
            this.tabCharacterNotes.Padding = new System.Windows.Forms.Padding(3);
            this.tabCharacterNotes.Size = new System.Drawing.Size(446, 205);
            this.tabCharacterNotes.TabIndex = 3;
            this.tabCharacterNotes.Tag = "Tab_Roster_CharacterNotes";
            this.tabCharacterNotes.Text = "Character Notes";
            this.tabCharacterNotes.UseVisualStyleBackColor = true;
            // 
            // txtCharacterNotes
            // 
            this.txtCharacterNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCharacterNotes.Location = new System.Drawing.Point(3, 3);
            this.txtCharacterNotes.MaxLength = 2147483647;
            this.txtCharacterNotes.Multiline = true;
            this.txtCharacterNotes.Name = "txtCharacterNotes";
            this.txtCharacterNotes.ReadOnly = true;
            this.txtCharacterNotes.Size = new System.Drawing.Size(440, 199);
            this.txtCharacterNotes.TabIndex = 0;
            // 
            // tabGameNotes
            // 
            this.tabGameNotes.Controls.Add(this.txtGameNotes);
            this.tabGameNotes.Location = new System.Drawing.Point(4, 22);
            this.tabGameNotes.Name = "tabGameNotes";
            this.tabGameNotes.Padding = new System.Windows.Forms.Padding(3);
            this.tabGameNotes.Size = new System.Drawing.Size(446, 205);
            this.tabGameNotes.TabIndex = 4;
            this.tabGameNotes.Tag = "Tab_Roster_GameNotes";
            this.tabGameNotes.Text = "Game Notes";
            this.tabGameNotes.UseVisualStyleBackColor = true;
            // 
            // txtGameNotes
            // 
            this.txtGameNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtGameNotes.Location = new System.Drawing.Point(3, 3);
            this.txtGameNotes.MaxLength = 2147483647;
            this.txtGameNotes.Multiline = true;
            this.txtGameNotes.Name = "txtGameNotes";
            this.txtGameNotes.ReadOnly = true;
            this.txtGameNotes.Size = new System.Drawing.Size(440, 199);
            this.txtGameNotes.TabIndex = 0;
            // 
            // lblCharacterName
            // 
            this.lblCharacterName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCharacterName.AutoSize = true;
            this.lblCharacterName.Location = new System.Drawing.Point(96, 6);
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
            this.lblCharacterNameLabel.Location = new System.Drawing.Point(3, 6);
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
            this.lblMetatype.Location = new System.Drawing.Point(96, 81);
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
            this.lblMetatypeLabel.Location = new System.Drawing.Point(36, 81);
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
            this.lblCareerKarma.Location = new System.Drawing.Point(96, 106);
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
            this.lblCareerKarmaLabel.Location = new System.Drawing.Point(16, 106);
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
            this.lblPlayerName.Location = new System.Drawing.Point(96, 56);
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
            this.lblPlayerNameLabel.Location = new System.Drawing.Point(51, 56);
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
            this.lblCharacterAlias.Location = new System.Drawing.Point(96, 31);
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
            this.lblCharacterAliasLabel.Location = new System.Drawing.Point(58, 31);
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
            this.lblEssence.Location = new System.Drawing.Point(96, 131);
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
            this.lblEssenceLabel.Location = new System.Drawing.Point(39, 131);
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
            this.lblFilePath.Location = new System.Drawing.Point(96, 156);
            this.lblFilePath.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblFilePath.Name = "lblFilePath";
            this.lblFilePath.Size = new System.Drawing.Size(39, 13);
            this.lblFilePath.TabIndex = 38;
            this.lblFilePath.Text = "[None]";
            this.lblFilePath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblFilePath.ToolTipText = "";
            // 
            // lblFilePathLabel
            // 
            this.lblFilePathLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblFilePathLabel.AutoSize = true;
            this.lblFilePathLabel.Location = new System.Drawing.Point(36, 156);
            this.lblFilePathLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblFilePathLabel.Name = "lblFilePathLabel";
            this.lblFilePathLabel.Size = new System.Drawing.Size(54, 13);
            this.lblFilePathLabel.TabIndex = 37;
            this.lblFilePathLabel.Tag = "Label_Roster_File_Name";
            this.lblFilePathLabel.Text = "File Name";
            this.lblFilePathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblSettings
            // 
            this.lblSettings.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSettings.AutoSize = true;
            this.lblSettings.Location = new System.Drawing.Point(96, 181);
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
            this.lblSettingsLabel.Location = new System.Drawing.Point(23, 181);
            this.lblSettingsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSettingsLabel.Name = "lblSettingsLabel";
            this.lblSettingsLabel.Size = new System.Drawing.Size(67, 13);
            this.lblSettingsLabel.TabIndex = 39;
            this.lblSettingsLabel.Tag = "Label_Roster_Settings_File";
            this.lblSettingsLabel.Text = "Settings File:";
            this.lblSettingsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tlpRight
            // 
            this.tlpRight.AutoSize = true;
            this.tlpRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.ColumnCount = 3;
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpRight.Controls.Add(this.lblSettings, 1, 7);
            this.tlpRight.Controls.Add(this.tabCharacterTexts, 0, 9);
            this.tlpRight.Controls.Add(this.lblCharacterAliasLabel, 0, 1);
            this.tlpRight.Controls.Add(this.picMugshot, 2, 0);
            this.tlpRight.Controls.Add(this.lblSettingsLabel, 0, 7);
            this.tlpRight.Controls.Add(this.lblCharacterAlias, 1, 1);
            this.tlpRight.Controls.Add(this.lblFilePath, 1, 6);
            this.tlpRight.Controls.Add(this.lblPlayerNameLabel, 0, 2);
            this.tlpRight.Controls.Add(this.lblFilePathLabel, 0, 6);
            this.tlpRight.Controls.Add(this.lblPlayerName, 1, 2);
            this.tlpRight.Controls.Add(this.lblEssence, 1, 5);
            this.tlpRight.Controls.Add(this.lblMetatypeLabel, 0, 3);
            this.tlpRight.Controls.Add(this.lblEssenceLabel, 0, 5);
            this.tlpRight.Controls.Add(this.lblMetatype, 1, 3);
            this.tlpRight.Controls.Add(this.lblCareerKarma, 1, 4);
            this.tlpRight.Controls.Add(this.lblCareerKarmaLabel, 0, 4);
            this.tlpRight.Controls.Add(this.lblCharacterNameLabel, 0, 0);
            this.tlpRight.Controls.Add(this.lblCharacterName, 1, 0);
            this.tlpRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRight.Location = new System.Drawing.Point(306, 0);
            this.tlpRight.Margin = new System.Windows.Forms.Padding(0);
            this.tlpRight.Name = "tlpRight";
            this.tlpRight.RowCount = 10;
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRight.Size = new System.Drawing.Size(460, 543);
            this.tlpRight.TabIndex = 41;
            // 
            // picMugshot
            // 
            this.picMugshot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picMugshot.Location = new System.Drawing.Point(157, 3);
            this.picMugshot.Name = "picMugshot";
            this.tlpRight.SetRowSpan(this.picMugshot, 9);
            this.picMugshot.Size = new System.Drawing.Size(300, 300);
            this.picMugshot.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picMugshot.TabIndex = 21;
            this.picMugshot.TabStop = false;
            this.picMugshot.SizeChanged += new System.EventHandler(this.picMugshot_SizeChanged);
            // 
            // tlpCharacterRoster
            // 
            this.tlpCharacterRoster.AutoSize = true;
            this.tlpCharacterRoster.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpCharacterRoster.ColumnCount = 2;
            this.tlpCharacterRoster.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlpCharacterRoster.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tlpCharacterRoster.Controls.Add(this.treCharacterList, 0, 0);
            this.tlpCharacterRoster.Controls.Add(this.tlpRight, 1, 0);
            this.tlpCharacterRoster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCharacterRoster.Location = new System.Drawing.Point(9, 9);
            this.tlpCharacterRoster.Name = "tlpCharacterRoster";
            this.tlpCharacterRoster.RowCount = 1;
            this.tlpCharacterRoster.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCharacterRoster.Size = new System.Drawing.Size(766, 543);
            this.tlpCharacterRoster.TabIndex = 42;
            // 
            // treCharacterList
            // 
            this.treCharacterList.AllowDrop = true;
            this.treCharacterList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treCharacterList.Location = new System.Drawing.Point(3, 3);
            this.treCharacterList.Name = "treCharacterList";
            this.treCharacterList.ShowNodeToolTips = true;
            this.treCharacterList.Size = new System.Drawing.Size(300, 537);
            this.treCharacterList.TabIndex = 0;
            this.treCharacterList.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treCharacterList_AfterSelect);
            this.treCharacterList.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TreCharacterList_NodeMouseClick);
            this.treCharacterList.DoubleClick += new System.EventHandler(this.treCharacterList_DoubleClick);
            this.treCharacterList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treCharacterList_OnDefaultKeyDown);
            this.treCharacterList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            // 
            // cmsRoster
            // 
            this.cmsRoster.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsToggleFav,
            this.tsSort,
            this.tsOpen,
            this.tsOpenForPrinting,
            this.tsOpenForExport,
            this.tsDelete,
            this.tsCloseOpenCharacter});
            this.cmsRoster.Name = "cmsRoster";
            this.cmsRoster.Size = new System.Drawing.Size(214, 158);
            // 
            // tsToggleFav
            // 
            this.tsToggleFav.Image = global::Chummer.Properties.Resources.asterisk_orange_16;
            this.tsToggleFav.ImageDpi120 = global::Chummer.Properties.Resources.asterisk_orange_20;
            this.tsToggleFav.ImageDpi144 = global::Chummer.Properties.Resources.asterisk_orange_24;
            this.tsToggleFav.ImageDpi192 = global::Chummer.Properties.Resources.asterisk_orange_32;
            this.tsToggleFav.ImageDpi288 = global::Chummer.Properties.Resources.asterisk_orange_48;
            this.tsToggleFav.ImageDpi384 = global::Chummer.Properties.Resources.asterisk_orange_64;
            this.tsToggleFav.ImageDpi96 = global::Chummer.Properties.Resources.asterisk_orange_16;
            this.tsToggleFav.Name = "tsToggleFav";
            this.tsToggleFav.Size = new System.Drawing.Size(213, 22);
            this.tsToggleFav.Tag = "Menu_ToggleFavorite";
            this.tsToggleFav.Text = "&Toggle Favorite";
            this.tsToggleFav.Click += new System.EventHandler(this.tsToggleFav_Click);
            // 
            // tsSort
            // 
            this.tsSort.Image = global::Chummer.Properties.Resources.page_refresh_16;
            this.tsSort.ImageDpi120 = global::Chummer.Properties.Resources.page_refresh_20;
            this.tsSort.ImageDpi144 = global::Chummer.Properties.Resources.page_refresh_24;
            this.tsSort.ImageDpi192 = global::Chummer.Properties.Resources.page_refresh_32;
            this.tsSort.ImageDpi288 = global::Chummer.Properties.Resources.page_refresh_48;
            this.tsSort.ImageDpi384 = global::Chummer.Properties.Resources.page_refresh_64;
            this.tsSort.ImageDpi96 = global::Chummer.Properties.Resources.page_refresh_16;
            this.tsSort.Name = "tsSort";
            this.tsSort.Size = new System.Drawing.Size(213, 22);
            this.tsSort.Tag = "Menu_Sort";
            this.tsSort.Text = "&Sort this list Alphabetically";
            this.tsSort.Click += new System.EventHandler(this.tsSort_Click);
            // 
            // tsOpen
            // 
            this.tsOpen.Image = global::Chummer.Properties.Resources.folder_page_16;
            this.tsOpen.ImageDpi120 = global::Chummer.Properties.Resources.folder_page_20;
            this.tsOpen.ImageDpi144 = global::Chummer.Properties.Resources.folder_page_24;
            this.tsOpen.ImageDpi192 = global::Chummer.Properties.Resources.folder_page_32;
            this.tsOpen.ImageDpi288 = global::Chummer.Properties.Resources.folder_page_48;
            this.tsOpen.ImageDpi384 = global::Chummer.Properties.Resources.folder_page_64;
            this.tsOpen.ImageDpi96 = global::Chummer.Properties.Resources.folder_page_16;
            this.tsOpen.Name = "tsOpen";
            this.tsOpen.Size = new System.Drawing.Size(213, 22);
            this.tsOpen.Tag = "Menu_Main_Open";
            this.tsOpen.Text = "&Open";
            this.tsOpen.Click += new System.EventHandler(this.tsOpen_Click);
            // 
            // tsOpenForPrinting
            // 
            this.tsOpenForPrinting.Image = global::Chummer.Properties.Resources.folder_print_16;
            this.tsOpenForPrinting.ImageDpi120 = global::Chummer.Properties.Resources.folder_print_20;
            this.tsOpenForPrinting.ImageDpi144 = global::Chummer.Properties.Resources.folder_print_24;
            this.tsOpenForPrinting.ImageDpi192 = global::Chummer.Properties.Resources.folder_print_32;
            this.tsOpenForPrinting.ImageDpi288 = global::Chummer.Properties.Resources.folder_print_48;
            this.tsOpenForPrinting.ImageDpi384 = global::Chummer.Properties.Resources.folder_print_64;
            this.tsOpenForPrinting.ImageDpi96 = global::Chummer.Properties.Resources.folder_print_16;
            this.tsOpenForPrinting.Name = "tsOpenForPrinting";
            this.tsOpenForPrinting.Size = new System.Drawing.Size(213, 22);
            this.tsOpenForPrinting.Tag = "Menu_Main_OpenForPrinting";
            this.tsOpenForPrinting.Text = "Open for P&rinting";
            this.tsOpenForPrinting.Click += new System.EventHandler(this.tsOpenForPrinting_Click);
            // 
            // tsOpenForExport
            // 
            this.tsOpenForExport.Image = global::Chummer.Properties.Resources.folder_script_go_16;
            this.tsOpenForExport.ImageDpi120 = global::Chummer.Properties.Resources.folder_script_go_20;
            this.tsOpenForExport.ImageDpi144 = global::Chummer.Properties.Resources.folder_script_go_24;
            this.tsOpenForExport.ImageDpi192 = global::Chummer.Properties.Resources.folder_script_go_32;
            this.tsOpenForExport.ImageDpi288 = global::Chummer.Properties.Resources.folder_script_go_48;
            this.tsOpenForExport.ImageDpi384 = global::Chummer.Properties.Resources.folder_script_go_64;
            this.tsOpenForExport.ImageDpi96 = global::Chummer.Properties.Resources.folder_script_go_16;
            this.tsOpenForExport.Name = "tsOpenForExport";
            this.tsOpenForExport.Size = new System.Drawing.Size(213, 22);
            this.tsOpenForExport.Tag = "Menu_Main_OpenForExport";
            this.tsOpenForExport.Text = "Open for E&xport";
            this.tsOpenForExport.Click += new System.EventHandler(this.tsOpenForExport_Click);
            // 
            // tsDelete
            // 
            this.tsDelete.Image = global::Chummer.Properties.Resources.delete_16;
            this.tsDelete.ImageDpi120 = global::Chummer.Properties.Resources.delete_20;
            this.tsDelete.ImageDpi144 = global::Chummer.Properties.Resources.delete_24;
            this.tsDelete.ImageDpi192 = global::Chummer.Properties.Resources.delete_32;
            this.tsDelete.ImageDpi288 = global::Chummer.Properties.Resources.delete_48;
            this.tsDelete.ImageDpi384 = global::Chummer.Properties.Resources.delete_64;
            this.tsDelete.ImageDpi96 = global::Chummer.Properties.Resources.delete_16;
            this.tsDelete.Name = "tsDelete";
            this.tsDelete.Size = new System.Drawing.Size(213, 22);
            this.tsDelete.Tag = "Menu_Delete";
            this.tsDelete.Text = "&Delete from this list";
            this.tsDelete.Click += new System.EventHandler(this.tsDelete_Click);
            // 
            // tsCloseOpenCharacter
            // 
            this.tsCloseOpenCharacter.Image = global::Chummer.Properties.Resources.door_out_16;
            this.tsCloseOpenCharacter.ImageDpi120 = global::Chummer.Properties.Resources.door_out_20;
            this.tsCloseOpenCharacter.ImageDpi144 = global::Chummer.Properties.Resources.door_out_24;
            this.tsCloseOpenCharacter.ImageDpi192 = global::Chummer.Properties.Resources.door_out_32;
            this.tsCloseOpenCharacter.ImageDpi288 = global::Chummer.Properties.Resources.door_out_48;
            this.tsCloseOpenCharacter.ImageDpi384 = global::Chummer.Properties.Resources.door_out_64;
            this.tsCloseOpenCharacter.ImageDpi96 = global::Chummer.Properties.Resources.door_out_16;
            this.tsCloseOpenCharacter.Name = "tsCloseOpenCharacter";
            this.tsCloseOpenCharacter.Size = new System.Drawing.Size(213, 22);
            this.tsCloseOpenCharacter.Tag = "Menu_Close";
            this.tsCloseOpenCharacter.Text = "&Close this character";
            this.tsCloseOpenCharacter.Click += new System.EventHandler(this.tsCloseOpenCharacter_Click);
            // 
            // cmsRosterNoClose
            // 
            this.cmsRosterNoClose.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsToggleFavNoClose,
            this.tsSortNoClose,
            this.tsOpenNoClose,
            this.tsOpenForPrintingNoClose,
            this.tsOpenForExportNoClose,
            this.tsDeleteNoClose});
            this.cmsRosterNoClose.Name = "cmsRoster";
            this.cmsRosterNoClose.Size = new System.Drawing.Size(214, 136);
            // 
            // tsToggleFavNoClose
            // 
            this.tsToggleFavNoClose.Image = global::Chummer.Properties.Resources.asterisk_orange_16;
            this.tsToggleFavNoClose.ImageDpi120 = global::Chummer.Properties.Resources.asterisk_orange_20;
            this.tsToggleFavNoClose.ImageDpi144 = global::Chummer.Properties.Resources.asterisk_orange_24;
            this.tsToggleFavNoClose.ImageDpi192 = global::Chummer.Properties.Resources.asterisk_orange_32;
            this.tsToggleFavNoClose.ImageDpi288 = global::Chummer.Properties.Resources.asterisk_orange_48;
            this.tsToggleFavNoClose.ImageDpi384 = global::Chummer.Properties.Resources.asterisk_orange_64;
            this.tsToggleFavNoClose.ImageDpi96 = global::Chummer.Properties.Resources.asterisk_orange_16;
            this.tsToggleFavNoClose.Name = "tsToggleFavNoClose";
            this.tsToggleFavNoClose.Size = new System.Drawing.Size(213, 22);
            this.tsToggleFavNoClose.Tag = "Menu_ToggleFavorite";
            this.tsToggleFavNoClose.Text = "&Toggle Favorite";
            // 
            // tsSortNoClose
            // 
            this.tsSortNoClose.Image = global::Chummer.Properties.Resources.page_refresh_16;
            this.tsSortNoClose.ImageDpi120 = global::Chummer.Properties.Resources.page_refresh_20;
            this.tsSortNoClose.ImageDpi144 = global::Chummer.Properties.Resources.page_refresh_24;
            this.tsSortNoClose.ImageDpi192 = global::Chummer.Properties.Resources.page_refresh_32;
            this.tsSortNoClose.ImageDpi288 = global::Chummer.Properties.Resources.page_refresh_48;
            this.tsSortNoClose.ImageDpi384 = global::Chummer.Properties.Resources.page_refresh_64;
            this.tsSortNoClose.ImageDpi96 = global::Chummer.Properties.Resources.page_refresh_16;
            this.tsSortNoClose.Name = "tsSortNoClose";
            this.tsSortNoClose.Size = new System.Drawing.Size(213, 22);
            this.tsSortNoClose.Tag = "Menu_Sort";
            this.tsSortNoClose.Text = "&Sort this list Alphabetically";
            // 
            // tsOpenNoClose
            // 
            this.tsOpenNoClose.Image = global::Chummer.Properties.Resources.folder_page_16;
            this.tsOpenNoClose.ImageDpi120 = global::Chummer.Properties.Resources.folder_page_20;
            this.tsOpenNoClose.ImageDpi144 = global::Chummer.Properties.Resources.folder_page_24;
            this.tsOpenNoClose.ImageDpi192 = global::Chummer.Properties.Resources.folder_page_32;
            this.tsOpenNoClose.ImageDpi288 = global::Chummer.Properties.Resources.folder_page_48;
            this.tsOpenNoClose.ImageDpi384 = global::Chummer.Properties.Resources.folder_page_64;
            this.tsOpenNoClose.ImageDpi96 = global::Chummer.Properties.Resources.folder_page_16;
            this.tsOpenNoClose.Name = "tsOpenNoClose";
            this.tsOpenNoClose.Size = new System.Drawing.Size(213, 22);
            this.tsOpenNoClose.Tag = "Menu_Main_Open";
            this.tsOpenNoClose.Text = "&Open";
            // 
            // tsOpenForPrintingNoClose
            // 
            this.tsOpenForPrintingNoClose.Image = global::Chummer.Properties.Resources.folder_print_16;
            this.tsOpenForPrintingNoClose.ImageDpi120 = global::Chummer.Properties.Resources.folder_print_20;
            this.tsOpenForPrintingNoClose.ImageDpi144 = global::Chummer.Properties.Resources.folder_print_24;
            this.tsOpenForPrintingNoClose.ImageDpi192 = global::Chummer.Properties.Resources.folder_print_32;
            this.tsOpenForPrintingNoClose.ImageDpi288 = global::Chummer.Properties.Resources.folder_print_48;
            this.tsOpenForPrintingNoClose.ImageDpi384 = global::Chummer.Properties.Resources.folder_print_64;
            this.tsOpenForPrintingNoClose.ImageDpi96 = global::Chummer.Properties.Resources.folder_print_16;
            this.tsOpenForPrintingNoClose.Name = "tsOpenForPrintingNoClose";
            this.tsOpenForPrintingNoClose.Size = new System.Drawing.Size(213, 22);
            this.tsOpenForPrintingNoClose.Tag = "Menu_Main_OpenForPrinting";
            this.tsOpenForPrintingNoClose.Text = "Open for P&rinting";
            // 
            // tsOpenForExportNoClose
            // 
            this.tsOpenForExportNoClose.Image = global::Chummer.Properties.Resources.folder_script_go_16;
            this.tsOpenForExportNoClose.ImageDpi120 = global::Chummer.Properties.Resources.folder_script_go_20;
            this.tsOpenForExportNoClose.ImageDpi144 = global::Chummer.Properties.Resources.folder_script_go_24;
            this.tsOpenForExportNoClose.ImageDpi192 = global::Chummer.Properties.Resources.folder_script_go_32;
            this.tsOpenForExportNoClose.ImageDpi288 = global::Chummer.Properties.Resources.folder_script_go_48;
            this.tsOpenForExportNoClose.ImageDpi384 = global::Chummer.Properties.Resources.folder_script_go_64;
            this.tsOpenForExportNoClose.ImageDpi96 = global::Chummer.Properties.Resources.folder_script_go_16;
            this.tsOpenForExportNoClose.Name = "tsOpenForExportNoClose";
            this.tsOpenForExportNoClose.Size = new System.Drawing.Size(213, 22);
            this.tsOpenForExportNoClose.Tag = "Menu_Main_OpenForExport";
            this.tsOpenForExportNoClose.Text = "Open for E&xport";
            // 
            // tsDeleteNoClose
            // 
            this.tsDeleteNoClose.Image = global::Chummer.Properties.Resources.delete_16;
            this.tsDeleteNoClose.ImageDpi120 = global::Chummer.Properties.Resources.delete_20;
            this.tsDeleteNoClose.ImageDpi144 = global::Chummer.Properties.Resources.delete_24;
            this.tsDeleteNoClose.ImageDpi192 = global::Chummer.Properties.Resources.delete_32;
            this.tsDeleteNoClose.ImageDpi288 = global::Chummer.Properties.Resources.delete_48;
            this.tsDeleteNoClose.ImageDpi384 = global::Chummer.Properties.Resources.delete_64;
            this.tsDeleteNoClose.ImageDpi96 = global::Chummer.Properties.Resources.delete_16;
            this.tsDeleteNoClose.Name = "tsDeleteNoClose";
            this.tsDeleteNoClose.Size = new System.Drawing.Size(213, 22);
            this.tsDeleteNoClose.Tag = "Menu_Delete";
            this.tsDeleteNoClose.Text = "&Delete from this list";
            // 
            // CharacterRoster
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.tlpCharacterRoster);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CharacterRoster";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "String_CharacterRoster";
            this.Text = "Character Roster";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CharacterRoster_FormClosing);
            this.Load += new System.EventHandler(this.CharacterRoster_Load);
            this.tabCharacterTexts.ResumeLayout(false);
            this.tabCharacterDescription.ResumeLayout(false);
            this.tabCharacterDescription.PerformLayout();
            this.tabCharacterConcept.ResumeLayout(false);
            this.tabCharacterConcept.PerformLayout();
            this.tabCharacterBackground.ResumeLayout(false);
            this.tabCharacterBackground.PerformLayout();
            this.tabCharacterNotes.ResumeLayout(false);
            this.tabCharacterNotes.PerformLayout();
            this.tabGameNotes.ResumeLayout(false);
            this.tabGameNotes.PerformLayout();
            this.tlpRight.ResumeLayout(false);
            this.tlpRight.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picMugshot)).EndInit();
            this.tlpCharacterRoster.ResumeLayout(false);
            this.tlpCharacterRoster.PerformLayout();
            this.cmsRoster.ResumeLayout(false);
            this.cmsRosterNoClose.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TabControl tabCharacterTexts;
        private System.Windows.Forms.TabPage tabCharacterDescription;
        private System.Windows.Forms.TabPage tabCharacterConcept;
        private System.Windows.Forms.TabPage tabCharacterBackground;
        private System.Windows.Forms.TabPage tabCharacterNotes;
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
        private Chummer.LabelWithToolTip lblFilePath;
        private System.Windows.Forms.Label lblFilePathLabel;
        private System.Windows.Forms.TabPage tabGameNotes;

        private System.Windows.Forms.TableLayoutPanel tlpRight;
        private System.Windows.Forms.Label lblSettings;
        private System.Windows.Forms.Label lblSettingsLabel;
        private System.Windows.Forms.PictureBox picMugshot;
        private System.Windows.Forms.TextBox txtCharacterDescription;
        private System.Windows.Forms.TextBox txtCharacterConcept;
        private System.Windows.Forms.TextBox txtCharacterBackground;
        private System.Windows.Forms.TextBox txtCharacterNotes;
        private System.Windows.Forms.TextBox txtGameNotes;
        private System.Windows.Forms.TableLayoutPanel tlpCharacterRoster;
        public System.Windows.Forms.TreeView treCharacterList;
        private System.Windows.Forms.ContextMenuStrip cmsRoster;
        private DpiFriendlyToolStripMenuItem tsToggleFav;
        private DpiFriendlyToolStripMenuItem tsSort;
        private DpiFriendlyToolStripMenuItem tsOpen;
        private DpiFriendlyToolStripMenuItem tsOpenForPrinting;
        private DpiFriendlyToolStripMenuItem tsOpenForExport;
        private DpiFriendlyToolStripMenuItem tsDelete;
        private DpiFriendlyToolStripMenuItem tsCloseOpenCharacter;
        private System.Windows.Forms.ContextMenuStrip cmsRosterNoClose;
        private DpiFriendlyToolStripMenuItem tsToggleFavNoClose;
        private DpiFriendlyToolStripMenuItem tsSortNoClose;
        private DpiFriendlyToolStripMenuItem tsOpenNoClose;
        private DpiFriendlyToolStripMenuItem tsOpenForPrintingNoClose;
        private DpiFriendlyToolStripMenuItem tsOpenForExportNoClose;
        private DpiFriendlyToolStripMenuItem tsDeleteNoClose;
    }
}

