using System;

namespace Chummer.UI.Skills
{
    partial class SkillsTabUserControl
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
                _lstActiveSkills?.Dispose();
                _lstSkillGroups?.Dispose();
                _lstKnowledgeSkills?.Dispose();
                UnbindSkillsTabUserControl();
                if (!(ParentForm is CharacterShared frmParent) || frmParent.CharacterObject != _objCharacter)
                    _objCharacter?.Dispose();
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
            this.components = new System.ComponentModel.Container();
            this.splitSkills = new System.Windows.Forms.SplitContainer();
            this.tlpTopPanel = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpSkillGroups = new System.Windows.Forms.TableLayoutPanel();
            this.lblSkillGroups = new System.Windows.Forms.Label();
            this.flpSkillGroupLabels = new System.Windows.Forms.FlowLayoutPanel();
            this.lblGroupKarma = new System.Windows.Forms.Label();
            this.lblGroupsSp = new System.Windows.Forms.Label();
            this.tlpActiveSkills = new System.Windows.Forms.TableLayoutPanel();
            this.lblActiveSkills = new System.Windows.Forms.Label();
            this.lblBuyWithKarma = new System.Windows.Forms.Label();
            this.flpActiveSkillsLabels = new System.Windows.Forms.FlowLayoutPanel();
            this.lblActiveSp = new System.Windows.Forms.Label();
            this.lblActiveKarma = new System.Windows.Forms.Label();
            this.btnResetCustomDisplayAttribute = new System.Windows.Forms.Button();
            this.tlpActiveSkillsButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cboSort = new Chummer.ElasticComboBox();
            this.cboDisplayFilter = new Chummer.ElasticComboBox();
            this.btnExotic = new System.Windows.Forms.Button();
            this.tlpBottomPanel = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblKnowledgeSkills = new System.Windows.Forms.Label();
            this.lblKnoBwk = new System.Windows.Forms.Label();
            this.flpKnowledgeSkillsLabels = new System.Windows.Forms.FlowLayoutPanel();
            this.lblKnoSp = new System.Windows.Forms.Label();
            this.lblKnoKarma = new System.Windows.Forms.Label();
            this.lblCustomKnowledgeSkillsReminder = new System.Windows.Forms.Label();
            this.tlpKnowledgeSkillsHeader = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cboDisplayFilterKnowledge = new Chummer.ElasticComboBox();
            this.cboSortKnowledge = new Chummer.ElasticComboBox();
            this.btnKnowledge = new System.Windows.Forms.Button();
            this.lblKnowledgeSkillPoints = new System.Windows.Forms.Label();
            this.lblKnowledgeSkillPointsTitle = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitSkills)).BeginInit();
            this.splitSkills.Panel1.SuspendLayout();
            this.splitSkills.Panel2.SuspendLayout();
            this.splitSkills.SuspendLayout();
            this.tlpTopPanel.SuspendLayout();
            this.tlpSkillGroups.SuspendLayout();
            this.flpSkillGroupLabels.SuspendLayout();
            this.tlpActiveSkills.SuspendLayout();
            this.flpActiveSkillsLabels.SuspendLayout();
            this.tlpActiveSkillsButtons.SuspendLayout();
            this.tlpBottomPanel.SuspendLayout();
            this.flpKnowledgeSkillsLabels.SuspendLayout();
            this.tlpKnowledgeSkillsHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitSkills
            // 
            this.splitSkills.BackColor = System.Drawing.Color.LightBlue;
            this.splitSkills.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitSkills.Location = new System.Drawing.Point(0, 0);
            this.splitSkills.Margin = new System.Windows.Forms.Padding(0);
            this.splitSkills.Name = "splitSkills";
            this.splitSkills.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitSkills.Panel1
            // 
            this.splitSkills.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.splitSkills.Panel1.Controls.Add(this.tlpTopPanel);
            this.splitSkills.Panel1.Resize += new System.EventHandler(this.Panel1_Resize);
            // 
            // splitSkills.Panel2
            // 
            this.splitSkills.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.splitSkills.Panel2.Controls.Add(this.tlpBottomPanel);
            this.splitSkills.Panel2.Resize += new System.EventHandler(this.Panel2_Resize);
            this.splitSkills.Size = new System.Drawing.Size(800, 611);
            this.splitSkills.SplitterDistance = 420;
            this.splitSkills.TabIndex = 0;
            this.splitSkills.Resize += new System.EventHandler(this.splitSkills_Resize);
            // 
            // tlpTopPanel
            // 
            this.tlpTopPanel.ColumnCount = 2;
            this.tlpTopPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpTopPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.tlpTopPanel.Controls.Add(this.tlpSkillGroups, 0, 0);
            this.tlpTopPanel.Controls.Add(this.tlpActiveSkills, 1, 0);
            this.tlpTopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTopPanel.Location = new System.Drawing.Point(0, 0);
            this.tlpTopPanel.Margin = new System.Windows.Forms.Padding(0);
            this.tlpTopPanel.Name = "tlpTopPanel";
            this.tlpTopPanel.RowCount = 1;
            this.tlpTopPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTopPanel.Size = new System.Drawing.Size(800, 420);
            this.tlpTopPanel.TabIndex = 58;
            // 
            // tlpSkillGroups
            // 
            this.tlpSkillGroups.ColumnCount = 2;
            this.tlpSkillGroups.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpSkillGroups.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSkillGroups.Controls.Add(this.lblSkillGroups, 0, 0);
            this.tlpSkillGroups.Controls.Add(this.flpSkillGroupLabels, 1, 0);
            this.tlpSkillGroups.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSkillGroups.Location = new System.Drawing.Point(0, 0);
            this.tlpSkillGroups.Margin = new System.Windows.Forms.Padding(0, 0, 9, 0);
            this.tlpSkillGroups.Name = "tlpSkillGroups";
            this.tlpSkillGroups.RowCount = 2;
            this.tlpSkillGroups.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSkillGroups.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSkillGroups.Size = new System.Drawing.Size(191, 420);
            this.tlpSkillGroups.TabIndex = 56;
            // 
            // lblSkillGroups
            // 
            this.lblSkillGroups.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSkillGroups.AutoSize = true;
            this.lblSkillGroups.Location = new System.Drawing.Point(3, 0);
            this.lblSkillGroups.MinimumSize = new System.Drawing.Size(0, 22);
            this.lblSkillGroups.Name = "lblSkillGroups";
            this.lblSkillGroups.Size = new System.Drawing.Size(63, 22);
            this.lblSkillGroups.TabIndex = 0;
            this.lblSkillGroups.Tag = "Label_SkillGroups";
            this.lblSkillGroups.Text = "Skill Groups";
            this.lblSkillGroups.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // flpSkillGroupLabels
            // 
            this.flpSkillGroupLabels.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flpSkillGroupLabels.AutoSize = true;
            this.flpSkillGroupLabels.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpSkillGroupLabels.Controls.Add(this.lblGroupKarma);
            this.flpSkillGroupLabels.Controls.Add(this.lblGroupsSp);
            this.flpSkillGroupLabels.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpSkillGroupLabels.Location = new System.Drawing.Point(106, 9);
            this.flpSkillGroupLabels.Margin = new System.Windows.Forms.Padding(0);
            this.flpSkillGroupLabels.Name = "flpSkillGroupLabels";
            this.flpSkillGroupLabels.Size = new System.Drawing.Size(85, 13);
            this.flpSkillGroupLabels.TabIndex = 1;
            this.flpSkillGroupLabels.WrapContents = false;
            // 
            // lblGroupKarma
            // 
            this.lblGroupKarma.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGroupKarma.AutoSize = true;
            this.lblGroupKarma.Location = new System.Drawing.Point(45, 0);
            this.lblGroupKarma.Name = "lblGroupKarma";
            this.lblGroupKarma.Size = new System.Drawing.Size(37, 13);
            this.lblGroupKarma.TabIndex = 52;
            this.lblGroupKarma.Tag = "String_Karma";
            this.lblGroupKarma.Text = "Karma";
            this.lblGroupKarma.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // lblGroupsSp
            // 
            this.lblGroupsSp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGroupsSp.AutoSize = true;
            this.lblGroupsSp.Location = new System.Drawing.Point(3, 0);
            this.lblGroupsSp.Name = "lblGroupsSp";
            this.lblGroupsSp.Size = new System.Drawing.Size(36, 13);
            this.lblGroupsSp.TabIndex = 51;
            this.lblGroupsSp.Tag = "String_Points";
            this.lblGroupsSp.Text = "Points";
            this.lblGroupsSp.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // tlpActiveSkills
            // 
            this.tlpActiveSkills.ColumnCount = 3;
            this.tlpActiveSkills.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpActiveSkills.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpActiveSkills.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpActiveSkills.Controls.Add(this.lblActiveSkills, 0, 1);
            this.tlpActiveSkills.Controls.Add(this.lblBuyWithKarma, 2, 1);
            this.tlpActiveSkills.Controls.Add(this.flpActiveSkillsLabels, 1, 1);
            this.tlpActiveSkills.Controls.Add(this.tlpActiveSkillsButtons, 1, 0);
            this.tlpActiveSkills.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpActiveSkills.Location = new System.Drawing.Point(209, 0);
            this.tlpActiveSkills.Margin = new System.Windows.Forms.Padding(9, 0, 0, 0);
            this.tlpActiveSkills.Name = "tlpActiveSkills";
            this.tlpActiveSkills.RowCount = 3;
            this.tlpActiveSkills.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpActiveSkills.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpActiveSkills.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpActiveSkills.Size = new System.Drawing.Size(591, 420);
            this.tlpActiveSkills.TabIndex = 57;
            // 
            // lblActiveSkills
            // 
            this.lblActiveSkills.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblActiveSkills.AutoSize = true;
            this.lblActiveSkills.Location = new System.Drawing.Point(3, 30);
            this.lblActiveSkills.MinimumSize = new System.Drawing.Size(0, 22);
            this.lblActiveSkills.Name = "lblActiveSkills";
            this.lblActiveSkills.Size = new System.Drawing.Size(64, 22);
            this.lblActiveSkills.TabIndex = 3;
            this.lblActiveSkills.Tag = "Label_ActiveSkills";
            this.lblActiveSkills.Text = "Active Skills";
            this.lblActiveSkills.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // lblBuyWithKarma
            // 
            this.lblBuyWithKarma.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBuyWithKarma.AutoSize = true;
            this.lblBuyWithKarma.Location = new System.Drawing.Point(505, 39);
            this.lblBuyWithKarma.Name = "lblBuyWithKarma";
            this.lblBuyWithKarma.Size = new System.Drawing.Size(83, 13);
            this.lblBuyWithKarma.TabIndex = 50;
            this.lblBuyWithKarma.Tag = "String_BuyWithKarma";
            this.lblBuyWithKarma.Text = "Buy With Karma";
            this.lblBuyWithKarma.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // flpActiveSkillsLabels
            // 
            this.flpActiveSkillsLabels.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.flpActiveSkillsLabels.AutoSize = true;
            this.flpActiveSkillsLabels.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpActiveSkillsLabels.Controls.Add(this.lblActiveSp);
            this.flpActiveSkillsLabels.Controls.Add(this.lblActiveKarma);
            this.flpActiveSkillsLabels.Controls.Add(this.btnResetCustomDisplayAttribute);
            this.flpActiveSkillsLabels.Location = new System.Drawing.Point(70, 29);
            this.flpActiveSkillsLabels.Margin = new System.Windows.Forms.Padding(0);
            this.flpActiveSkillsLabels.Name = "flpActiveSkillsLabels";
            this.flpActiveSkillsLabels.Size = new System.Drawing.Size(144, 23);
            this.flpActiveSkillsLabels.TabIndex = 52;
            this.flpActiveSkillsLabels.WrapContents = false;
            // 
            // lblActiveSp
            // 
            this.lblActiveSp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblActiveSp.AutoSize = true;
            this.lblActiveSp.Location = new System.Drawing.Point(3, 10);
            this.lblActiveSp.Name = "lblActiveSp";
            this.lblActiveSp.Size = new System.Drawing.Size(36, 13);
            this.lblActiveSp.TabIndex = 46;
            this.lblActiveSp.Tag = "String_Points";
            this.lblActiveSp.Text = "Points";
            this.lblActiveSp.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // lblActiveKarma
            // 
            this.lblActiveKarma.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblActiveKarma.AutoSize = true;
            this.lblActiveKarma.Location = new System.Drawing.Point(45, 10);
            this.lblActiveKarma.Name = "lblActiveKarma";
            this.lblActiveKarma.Size = new System.Drawing.Size(37, 13);
            this.lblActiveKarma.TabIndex = 47;
            this.lblActiveKarma.Tag = "String_Karma";
            this.lblActiveKarma.Text = "Karma";
            this.lblActiveKarma.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // btnResetCustomDisplayAttribute
            // 
            this.btnResetCustomDisplayAttribute.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnResetCustomDisplayAttribute.AutoSize = true;
            this.btnResetCustomDisplayAttribute.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnResetCustomDisplayAttribute.Location = new System.Drawing.Point(85, 0);
            this.btnResetCustomDisplayAttribute.Margin = new System.Windows.Forms.Padding(0);
            this.btnResetCustomDisplayAttribute.Name = "btnResetCustomDisplayAttribute";
            this.btnResetCustomDisplayAttribute.Size = new System.Drawing.Size(59, 23);
            this.btnResetCustomDisplayAttribute.TabIndex = 53;
            this.btnResetCustomDisplayAttribute.Tag = "Button_ResetAll";
            this.btnResetCustomDisplayAttribute.Text = "Reset All";
            this.btnResetCustomDisplayAttribute.UseVisualStyleBackColor = true;
            this.btnResetCustomDisplayAttribute.Visible = false;
            this.btnResetCustomDisplayAttribute.Click += new System.EventHandler(this.btnResetCustomDisplayAttribute_Click);
            // 
            // tlpActiveSkillsButtons
            // 
            this.tlpActiveSkillsButtons.AutoSize = true;
            this.tlpActiveSkillsButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpActiveSkillsButtons.ColumnCount = 3;
            this.tlpActiveSkills.SetColumnSpan(this.tlpActiveSkillsButtons, 2);
            this.tlpActiveSkillsButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlpActiveSkillsButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tlpActiveSkillsButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpActiveSkillsButtons.Controls.Add(this.cboSort, 0, 0);
            this.tlpActiveSkillsButtons.Controls.Add(this.cboDisplayFilter, 1, 0);
            this.tlpActiveSkillsButtons.Controls.Add(this.btnExotic, 2, 0);
            this.tlpActiveSkillsButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpActiveSkillsButtons.Location = new System.Drawing.Point(70, 0);
            this.tlpActiveSkillsButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpActiveSkillsButtons.Name = "tlpActiveSkillsButtons";
            this.tlpActiveSkillsButtons.RowCount = 1;
            this.tlpActiveSkillsButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpActiveSkillsButtons.Size = new System.Drawing.Size(521, 29);
            this.tlpActiveSkillsButtons.TabIndex = 53;
            // 
            // cboSort
            // 
            this.cboSort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSort.FormattingEnabled = true;
            this.cboSort.IntegralHeight = false;
            this.cboSort.Location = new System.Drawing.Point(3, 4);
            this.cboSort.Name = "cboSort";
            this.cboSort.Size = new System.Drawing.Size(164, 21);
            this.cboSort.TabIndex = 4;
            this.cboSort.TooltipText = "";
            this.cboSort.SelectedIndexChanged += new System.EventHandler(this.cboSort_SelectedIndexChanged);
            // 
            // cboDisplayFilter
            // 
            this.cboDisplayFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboDisplayFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDisplayFilter.FormattingEnabled = true;
            this.cboDisplayFilter.IntegralHeight = false;
            this.cboDisplayFilter.Location = new System.Drawing.Point(173, 4);
            this.cboDisplayFilter.Name = "cboDisplayFilter";
            this.cboDisplayFilter.Size = new System.Drawing.Size(249, 21);
            this.cboDisplayFilter.TabIndex = 1;
            this.cboDisplayFilter.TooltipText = "";
            this.cboDisplayFilter.SelectedIndexChanged += new System.EventHandler(this.cboDisplayFilter_SelectedIndexChanged);
            this.cboDisplayFilter.TextUpdate += new System.EventHandler(this.cboDisplayFilter_TextUpdate);
            // 
            // btnExotic
            // 
            this.btnExotic.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnExotic.AutoSize = true;
            this.btnExotic.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnExotic.Location = new System.Drawing.Point(428, 3);
            this.btnExotic.Name = "btnExotic";
            this.btnExotic.Size = new System.Drawing.Size(90, 23);
            this.btnExotic.TabIndex = 2;
            this.btnExotic.Tag = "Button_AddExoticSkill";
            this.btnExotic.Text = "Add Exotic Skill";
            this.btnExotic.UseVisualStyleBackColor = true;
            this.btnExotic.Click += new System.EventHandler(this.btnExotic_Click);
            // 
            // tlpBottomPanel
            // 
            this.tlpBottomPanel.AutoSize = true;
            this.tlpBottomPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpBottomPanel.ColumnCount = 3;
            this.tlpBottomPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBottomPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBottomPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBottomPanel.Controls.Add(this.lblKnowledgeSkills, 0, 1);
            this.tlpBottomPanel.Controls.Add(this.lblKnoBwk, 2, 1);
            this.tlpBottomPanel.Controls.Add(this.flpKnowledgeSkillsLabels, 1, 1);
            this.tlpBottomPanel.Controls.Add(this.lblCustomKnowledgeSkillsReminder, 0, 3);
            this.tlpBottomPanel.Controls.Add(this.tlpKnowledgeSkillsHeader, 0, 0);
            this.tlpBottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBottomPanel.Location = new System.Drawing.Point(0, 0);
            this.tlpBottomPanel.Margin = new System.Windows.Forms.Padding(0);
            this.tlpBottomPanel.Name = "tlpBottomPanel";
            this.tlpBottomPanel.RowCount = 4;
            this.tlpBottomPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBottomPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBottomPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBottomPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBottomPanel.Size = new System.Drawing.Size(800, 187);
            this.tlpBottomPanel.TabIndex = 59;
            // 
            // lblKnowledgeSkills
            // 
            this.lblKnowledgeSkills.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblKnowledgeSkills.AutoSize = true;
            this.lblKnowledgeSkills.Location = new System.Drawing.Point(3, 29);
            this.lblKnowledgeSkills.MinimumSize = new System.Drawing.Size(0, 22);
            this.lblKnowledgeSkills.Name = "lblKnowledgeSkills";
            this.lblKnowledgeSkills.Size = new System.Drawing.Size(87, 22);
            this.lblKnowledgeSkills.TabIndex = 4;
            this.lblKnowledgeSkills.Tag = "Label_KnowledgeSkills";
            this.lblKnowledgeSkills.Text = "Knowledge Skills";
            this.lblKnowledgeSkills.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // lblKnoBwk
            // 
            this.lblKnoBwk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.lblKnoBwk.AutoSize = true;
            this.lblKnoBwk.Location = new System.Drawing.Point(714, 38);
            this.lblKnoBwk.Name = "lblKnoBwk";
            this.lblKnoBwk.Size = new System.Drawing.Size(83, 13);
            this.lblKnoBwk.TabIndex = 53;
            this.lblKnoBwk.Tag = "String_BuyWithKarma";
            this.lblKnoBwk.Text = "Buy With Karma";
            this.lblKnoBwk.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // flpKnowledgeSkillsLabels
            // 
            this.flpKnowledgeSkillsLabels.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.flpKnowledgeSkillsLabels.AutoSize = true;
            this.flpKnowledgeSkillsLabels.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpKnowledgeSkillsLabels.Controls.Add(this.lblKnoSp);
            this.flpKnowledgeSkillsLabels.Controls.Add(this.lblKnoKarma);
            this.flpKnowledgeSkillsLabels.Location = new System.Drawing.Point(93, 38);
            this.flpKnowledgeSkillsLabels.Margin = new System.Windows.Forms.Padding(0);
            this.flpKnowledgeSkillsLabels.Name = "flpKnowledgeSkillsLabels";
            this.flpKnowledgeSkillsLabels.Size = new System.Drawing.Size(85, 13);
            this.flpKnowledgeSkillsLabels.TabIndex = 59;
            // 
            // lblKnoSp
            // 
            this.lblKnoSp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblKnoSp.AutoSize = true;
            this.lblKnoSp.Location = new System.Drawing.Point(3, 0);
            this.lblKnoSp.Name = "lblKnoSp";
            this.lblKnoSp.Size = new System.Drawing.Size(36, 13);
            this.lblKnoSp.TabIndex = 53;
            this.lblKnoSp.Tag = "String_Points";
            this.lblKnoSp.Text = "Points";
            this.lblKnoSp.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // lblKnoKarma
            // 
            this.lblKnoKarma.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblKnoKarma.AutoSize = true;
            this.lblKnoKarma.Location = new System.Drawing.Point(45, 0);
            this.lblKnoKarma.Name = "lblKnoKarma";
            this.lblKnoKarma.Size = new System.Drawing.Size(37, 13);
            this.lblKnoKarma.TabIndex = 54;
            this.lblKnoKarma.Tag = "String_Karma";
            this.lblKnoKarma.Text = "Karma";
            this.lblKnoKarma.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // lblCustomKnowledgeSkillsReminder
            // 
            this.lblCustomKnowledgeSkillsReminder.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.lblCustomKnowledgeSkillsReminder.AutoSize = true;
            this.tlpBottomPanel.SetColumnSpan(this.lblCustomKnowledgeSkillsReminder, 3);
            this.lblCustomKnowledgeSkillsReminder.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCustomKnowledgeSkillsReminder.Location = new System.Drawing.Point(201, 168);
            this.lblCustomKnowledgeSkillsReminder.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCustomKnowledgeSkillsReminder.Name = "lblCustomKnowledgeSkillsReminder";
            this.lblCustomKnowledgeSkillsReminder.Size = new System.Drawing.Size(398, 13);
            this.lblCustomKnowledgeSkillsReminder.TabIndex = 55;
            this.lblCustomKnowledgeSkillsReminder.Tag = "Label_CustomKnowledgeSkillsReminder";
            this.lblCustomKnowledgeSkillsReminder.Text = "Remember, you can always write in custom skills and specializations!";
            this.lblCustomKnowledgeSkillsReminder.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // tlpKnowledgeSkillsHeader
            // 
            this.tlpKnowledgeSkillsHeader.AutoSize = true;
            this.tlpKnowledgeSkillsHeader.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpKnowledgeSkillsHeader.ColumnCount = 5;
            this.tlpBottomPanel.SetColumnSpan(this.tlpKnowledgeSkillsHeader, 3);
            this.tlpKnowledgeSkillsHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpKnowledgeSkillsHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlpKnowledgeSkillsHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tlpKnowledgeSkillsHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpKnowledgeSkillsHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpKnowledgeSkillsHeader.Controls.Add(this.cboDisplayFilterKnowledge, 2, 0);
            this.tlpKnowledgeSkillsHeader.Controls.Add(this.cboSortKnowledge, 1, 0);
            this.tlpKnowledgeSkillsHeader.Controls.Add(this.btnKnowledge, 0, 0);
            this.tlpKnowledgeSkillsHeader.Controls.Add(this.lblKnowledgeSkillPoints, 4, 0);
            this.tlpKnowledgeSkillsHeader.Controls.Add(this.lblKnowledgeSkillPointsTitle, 3, 0);
            this.tlpKnowledgeSkillsHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpKnowledgeSkillsHeader.Location = new System.Drawing.Point(0, 0);
            this.tlpKnowledgeSkillsHeader.Margin = new System.Windows.Forms.Padding(0);
            this.tlpKnowledgeSkillsHeader.Name = "tlpKnowledgeSkillsHeader";
            this.tlpKnowledgeSkillsHeader.RowCount = 1;
            this.tlpKnowledgeSkillsHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpKnowledgeSkillsHeader.Size = new System.Drawing.Size(800, 29);
            this.tlpKnowledgeSkillsHeader.TabIndex = 60;
            // 
            // cboDisplayFilterKnowledge
            // 
            this.cboDisplayFilterKnowledge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboDisplayFilterKnowledge.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDisplayFilterKnowledge.FormattingEnabled = true;
            this.cboDisplayFilterKnowledge.IntegralHeight = false;
            this.cboDisplayFilterKnowledge.Location = new System.Drawing.Point(265, 4);
            this.cboDisplayFilterKnowledge.Name = "cboDisplayFilterKnowledge";
            this.cboDisplayFilterKnowledge.Size = new System.Drawing.Size(291, 21);
            this.cboDisplayFilterKnowledge.TabIndex = 54;
            this.cboDisplayFilterKnowledge.TooltipText = "";
            this.cboDisplayFilterKnowledge.SelectedIndexChanged += new System.EventHandler(this.cboDisplayFilterKnowledge_SelectedIndexChanged);
            this.cboDisplayFilterKnowledge.TextUpdate += new System.EventHandler(this.cboDisplayFilterKnowledge_TextUpdate);
            // 
            // cboSortKnowledge
            // 
            this.cboSortKnowledge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSortKnowledge.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSortKnowledge.FormattingEnabled = true;
            this.cboSortKnowledge.IntegralHeight = false;
            this.cboSortKnowledge.Location = new System.Drawing.Point(67, 4);
            this.cboSortKnowledge.Name = "cboSortKnowledge";
            this.cboSortKnowledge.Size = new System.Drawing.Size(192, 21);
            this.cboSortKnowledge.TabIndex = 55;
            this.cboSortKnowledge.TooltipText = "";
            this.cboSortKnowledge.SelectedIndexChanged += new System.EventHandler(this.cboSortKnowledge_SelectedIndexChanged);
            // 
            // btnKnowledge
            // 
            this.btnKnowledge.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnKnowledge.AutoSize = true;
            this.btnKnowledge.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnKnowledge.Location = new System.Drawing.Point(3, 3);
            this.btnKnowledge.Name = "btnKnowledge";
            this.btnKnowledge.Size = new System.Drawing.Size(58, 23);
            this.btnKnowledge.TabIndex = 0;
            this.btnKnowledge.Tag = "Button_AddSkill";
            this.btnKnowledge.Text = "&Add Skill";
            this.btnKnowledge.UseVisualStyleBackColor = true;
            this.btnKnowledge.Click += new System.EventHandler(this.btnKnowledge_Click);
            // 
            // lblKnowledgeSkillPoints
            // 
            this.lblKnowledgeSkillPoints.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKnowledgeSkillPoints.AutoSize = true;
            this.lblKnowledgeSkillPoints.Location = new System.Drawing.Point(762, 8);
            this.lblKnowledgeSkillPoints.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKnowledgeSkillPoints.Name = "lblKnowledgeSkillPoints";
            this.lblKnowledgeSkillPoints.Size = new System.Drawing.Size(34, 13);
            this.lblKnowledgeSkillPoints.TabIndex = 38;
            this.lblKnowledgeSkillPoints.Text = "0 of 0";
            this.lblKnowledgeSkillPoints.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKnowledgeSkillPointsTitle
            // 
            this.lblKnowledgeSkillPointsTitle.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblKnowledgeSkillPointsTitle.AutoSize = true;
            this.lblKnowledgeSkillPointsTitle.Location = new System.Drawing.Point(562, 8);
            this.lblKnowledgeSkillPointsTitle.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKnowledgeSkillPointsTitle.Name = "lblKnowledgeSkillPointsTitle";
            this.lblKnowledgeSkillPointsTitle.Size = new System.Drawing.Size(194, 13);
            this.lblKnowledgeSkillPointsTitle.TabIndex = 37;
            this.lblKnowledgeSkillPointsTitle.Tag = "Label_FreeKnowledgeSkills";
            this.lblKnowledgeSkillPointsTitle.Text = "Free Knowledge Skill Points Remaining:";
            this.lblKnowledgeSkillPointsTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SkillsTabUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.splitSkills);
            this.DoubleBuffered = true;
            this.Name = "SkillsTabUserControl";
            this.Size = new System.Drawing.Size(800, 611);
            this.Load += new System.EventHandler(this.SkillsTabUserControl_Load);
            this.splitSkills.Panel1.ResumeLayout(false);
            this.splitSkills.Panel2.ResumeLayout(false);
            this.splitSkills.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitSkills)).EndInit();
            this.splitSkills.ResumeLayout(false);
            this.tlpTopPanel.ResumeLayout(false);
            this.tlpSkillGroups.ResumeLayout(false);
            this.tlpSkillGroups.PerformLayout();
            this.flpSkillGroupLabels.ResumeLayout(false);
            this.flpSkillGroupLabels.PerformLayout();
            this.tlpActiveSkills.ResumeLayout(false);
            this.tlpActiveSkills.PerformLayout();
            this.flpActiveSkillsLabels.ResumeLayout(false);
            this.flpActiveSkillsLabels.PerformLayout();
            this.tlpActiveSkillsButtons.ResumeLayout(false);
            this.tlpActiveSkillsButtons.PerformLayout();
            this.tlpBottomPanel.ResumeLayout(false);
            this.tlpBottomPanel.PerformLayout();
            this.flpKnowledgeSkillsLabels.ResumeLayout(false);
            this.flpKnowledgeSkillsLabels.PerformLayout();
            this.tlpKnowledgeSkillsHeader.ResumeLayout(false);
            this.tlpKnowledgeSkillsHeader.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitSkills;
        private System.Windows.Forms.Label lblSkillGroups;
        private ElasticComboBox cboDisplayFilter;
        private System.Windows.Forms.Button btnExotic;
        private System.Windows.Forms.Button btnKnowledge;
        private System.Windows.Forms.Label lblActiveSkills;
        private System.Windows.Forms.Label lblKnowledgeSkillPoints;
        private System.Windows.Forms.Label lblKnowledgeSkillPointsTitle;
        private System.Windows.Forms.Label lblKnowledgeSkills;
        private ElasticComboBox cboSort;
        private System.Windows.Forms.Label lblBuyWithKarma;
        private System.Windows.Forms.Label lblActiveKarma;
        private System.Windows.Forms.Label lblActiveSp;
        private System.Windows.Forms.Label lblGroupKarma;
        private System.Windows.Forms.Label lblGroupsSp;
        private System.Windows.Forms.Label lblKnoKarma;
        private System.Windows.Forms.Label lblKnoSp;
        private System.Windows.Forms.Label lblKnoBwk;
        private System.Windows.Forms.Button btnResetCustomDisplayAttribute;
        private System.Windows.Forms.Label lblCustomKnowledgeSkillsReminder;
        private ElasticComboBox cboSortKnowledge;
        private ElasticComboBox cboDisplayFilterKnowledge;
        private System.Windows.Forms.TableLayoutPanel tlpSkillGroups;
        private System.Windows.Forms.FlowLayoutPanel flpSkillGroupLabels;
        private System.Windows.Forms.TableLayoutPanel tlpActiveSkills;
        private BufferedTableLayoutPanel tlpTopPanel;
        private System.Windows.Forms.FlowLayoutPanel flpActiveSkillsLabels;
        private BufferedTableLayoutPanel tlpBottomPanel;
        private System.Windows.Forms.FlowLayoutPanel flpKnowledgeSkillsLabels;
        private BufferedTableLayoutPanel tlpActiveSkillsButtons;
        private BufferedTableLayoutPanel tlpKnowledgeSkillsHeader;
    }
}
