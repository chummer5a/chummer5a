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
            this.splitSkills = new System.Windows.Forms.SplitContainer();
            this.btnResetCustomDisplayAttribute = new System.Windows.Forms.Button();
            this.lblGroupKarma = new System.Windows.Forms.Label();
            this.lblGroupsSp = new System.Windows.Forms.Label();
            this.lblBuyWithKarma = new System.Windows.Forms.Label();
            this.lblActiveKarma = new System.Windows.Forms.Label();
            this.lblActiveSp = new System.Windows.Forms.Label();
            this.cboSort = new System.Windows.Forms.ComboBox();
            this.lblActiveSkills = new System.Windows.Forms.Label();
            this.btnExotic = new System.Windows.Forms.Button();
            this.cboDisplayFilter = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblCustomKnowledgeSkillsReminder = new System.Windows.Forms.Label();
            this.lblKnoBwk = new System.Windows.Forms.Label();
            this.lblKnoKarma = new System.Windows.Forms.Label();
            this.lblKnoSp = new System.Windows.Forms.Label();
            this.lblKnowledgeSkillPoints = new System.Windows.Forms.Label();
            this.lblKnowledgeSkillPointsTitle = new System.Windows.Forms.Label();
            this.lblKnowledgeSkills = new System.Windows.Forms.Label();
            this.btnKnowledge = new System.Windows.Forms.Button();
            this.cboSortKnowledge = new System.Windows.Forms.ComboBox();
            this.cboDisplayFilterKnowledge = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitSkills)).BeginInit();
            this.splitSkills.Panel1.SuspendLayout();
            this.splitSkills.Panel2.SuspendLayout();
            this.splitSkills.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitSkills
            // 
            this.splitSkills.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.splitSkills.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitSkills.Location = new System.Drawing.Point(0, 0);
            this.splitSkills.Margin = new System.Windows.Forms.Padding(0);
            this.splitSkills.MinimumSize = new System.Drawing.Size(830, 0);
            this.splitSkills.Name = "splitSkills";
            this.splitSkills.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitSkills.Panel1
            // 
            this.splitSkills.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.splitSkills.Panel1.Controls.Add(this.btnResetCustomDisplayAttribute);
            this.splitSkills.Panel1.Controls.Add(this.lblGroupKarma);
            this.splitSkills.Panel1.Controls.Add(this.lblGroupsSp);
            this.splitSkills.Panel1.Controls.Add(this.lblBuyWithKarma);
            this.splitSkills.Panel1.Controls.Add(this.lblActiveKarma);
            this.splitSkills.Panel1.Controls.Add(this.lblActiveSp);
            this.splitSkills.Panel1.Controls.Add(this.cboSort);
            this.splitSkills.Panel1.Controls.Add(this.lblActiveSkills);
            this.splitSkills.Panel1.Controls.Add(this.btnExotic);
            this.splitSkills.Panel1.Controls.Add(this.cboDisplayFilter);
            this.splitSkills.Panel1.Controls.Add(this.label1);
            this.splitSkills.Panel1.Resize += new System.EventHandler(this.Panel1_Resize);
            // 
            // splitSkills.Panel2
            // 
            this.splitSkills.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.splitSkills.Panel2.Controls.Add(this.cboSortKnowledge);
            this.splitSkills.Panel2.Controls.Add(this.cboDisplayFilterKnowledge);
            this.splitSkills.Panel2.Controls.Add(this.lblCustomKnowledgeSkillsReminder);
            this.splitSkills.Panel2.Controls.Add(this.lblKnoBwk);
            this.splitSkills.Panel2.Controls.Add(this.lblKnoKarma);
            this.splitSkills.Panel2.Controls.Add(this.lblKnoSp);
            this.splitSkills.Panel2.Controls.Add(this.lblKnowledgeSkillPoints);
            this.splitSkills.Panel2.Controls.Add(this.lblKnowledgeSkillPointsTitle);
            this.splitSkills.Panel2.Controls.Add(this.lblKnowledgeSkills);
            this.splitSkills.Panel2.Controls.Add(this.btnKnowledge);
            this.splitSkills.Panel2.Resize += new System.EventHandler(this.Panel2_Resize);
            this.splitSkills.Size = new System.Drawing.Size(830, 611);
            this.splitSkills.SplitterDistance = 450;
            this.splitSkills.TabIndex = 0;
            // 
            // btnResetCustomDisplayAttribute
            // 
            this.btnResetCustomDisplayAttribute.FlatAppearance.BorderSize = 0;
            this.btnResetCustomDisplayAttribute.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnResetCustomDisplayAttribute.Location = new System.Drawing.Point(373, 28);
            this.btnResetCustomDisplayAttribute.Margin = new System.Windows.Forms.Padding(0);
            this.btnResetCustomDisplayAttribute.Name = "btnResetCustomDisplayAttribute";
            this.btnResetCustomDisplayAttribute.Size = new System.Drawing.Size(75, 15);
            this.btnResetCustomDisplayAttribute.TabIndex = 53;
            this.btnResetCustomDisplayAttribute.Text = "Reset all";
            this.btnResetCustomDisplayAttribute.UseVisualStyleBackColor = true;
            this.btnResetCustomDisplayAttribute.Visible = false;
            this.btnResetCustomDisplayAttribute.Click += new System.EventHandler(this.btnResetCustomDisplayAttribute_Click);
            // 
            // lblGroupKarma
            // 
            this.lblGroupKarma.AutoSize = true;
            this.lblGroupKarma.Location = new System.Drawing.Point(177, 0);
            this.lblGroupKarma.Name = "lblGroupKarma";
            this.lblGroupKarma.Size = new System.Drawing.Size(37, 13);
            this.lblGroupKarma.TabIndex = 52;
            this.lblGroupKarma.Tag = "String_Karma";
            this.lblGroupKarma.Text = "Karma";
            this.lblGroupKarma.Visible = false;
            // 
            // lblGroupsSp
            // 
            this.lblGroupsSp.AutoSize = true;
            this.lblGroupsSp.Location = new System.Drawing.Point(136, 0);
            this.lblGroupsSp.Name = "lblGroupsSp";
            this.lblGroupsSp.Size = new System.Drawing.Size(36, 13);
            this.lblGroupsSp.TabIndex = 51;
            this.lblGroupsSp.Tag = "String_Points";
            this.lblGroupsSp.Text = "Points";
            this.lblGroupsSp.Visible = false;
            // 
            // lblBuyWithKarma
            // 
            this.lblBuyWithKarma.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBuyWithKarma.AutoSize = true;
            this.lblBuyWithKarma.Location = new System.Drawing.Point(715, 29);
            this.lblBuyWithKarma.Name = "lblBuyWithKarma";
            this.lblBuyWithKarma.Size = new System.Drawing.Size(83, 13);
            this.lblBuyWithKarma.TabIndex = 50;
            this.lblBuyWithKarma.Tag = "String_BuyWithKarma";
            this.lblBuyWithKarma.Text = "Buy With Karma";
            this.lblBuyWithKarma.Visible = false;
            // 
            // lblActiveKarma
            // 
            this.lblActiveKarma.AutoSize = true;
            this.lblActiveKarma.Location = new System.Drawing.Point(473, 27);
            this.lblActiveKarma.Name = "lblActiveKarma";
            this.lblActiveKarma.Size = new System.Drawing.Size(37, 13);
            this.lblActiveKarma.TabIndex = 47;
            this.lblActiveKarma.Tag = "String_Karma";
            this.lblActiveKarma.Text = "Karma";
            this.lblActiveKarma.Visible = false;
            // 
            // lblActiveSp
            // 
            this.lblActiveSp.AutoSize = true;
            this.lblActiveSp.Location = new System.Drawing.Point(432, 27);
            this.lblActiveSp.Name = "lblActiveSp";
            this.lblActiveSp.Size = new System.Drawing.Size(36, 13);
            this.lblActiveSp.TabIndex = 46;
            this.lblActiveSp.Tag = "String_Points";
            this.lblActiveSp.Text = "Points";
            this.lblActiveSp.Visible = false;
            // 
            // cboSort
            // 
            this.cboSort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSort.FormattingEnabled = true;
            this.cboSort.IntegralHeight = false;
            this.cboSort.Location = new System.Drawing.Point(394, 3);
            this.cboSort.Name = "cboSort";
            this.cboSort.Size = new System.Drawing.Size(133, 21);
            this.cboSort.TabIndex = 4;
            this.cboSort.SelectedIndexChanged += new System.EventHandler(this.cboSort_SelectedIndexChanged);
            // 
            // lblActiveSkills
            // 
            this.lblActiveSkills.AutoSize = true;
            this.lblActiveSkills.Location = new System.Drawing.Point(256, 27);
            this.lblActiveSkills.Name = "lblActiveSkills";
            this.lblActiveSkills.Size = new System.Drawing.Size(64, 13);
            this.lblActiveSkills.TabIndex = 3;
            this.lblActiveSkills.Tag = "Label_ActiveSkills";
            this.lblActiveSkills.Text = "Active Skills";
            // 
            // btnExotic
            // 
            this.btnExotic.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExotic.Location = new System.Drawing.Point(738, 3);
            this.btnExotic.Name = "btnExotic";
            this.btnExotic.Size = new System.Drawing.Size(89, 23);
            this.btnExotic.TabIndex = 2;
            this.btnExotic.Tag = "Button_AddExoticSkill";
            this.btnExotic.Text = "Add Exotic Skill";
            this.btnExotic.UseVisualStyleBackColor = true;
            this.btnExotic.Click += new System.EventHandler(this.btnExotic_Click);
            // 
            // cboDisplayFilter
            // 
            this.cboDisplayFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboDisplayFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDisplayFilter.FormattingEnabled = true;
            this.cboDisplayFilter.IntegralHeight = false;
            this.cboDisplayFilter.Location = new System.Drawing.Point(531, 3);
            this.cboDisplayFilter.Name = "cboDisplayFilter";
            this.cboDisplayFilter.Size = new System.Drawing.Size(201, 21);
            this.cboDisplayFilter.TabIndex = 1;
            this.cboDisplayFilter.SelectedIndexChanged += new System.EventHandler(this.cboDisplayFilter_SelectedIndexChanged);
            this.cboDisplayFilter.TextUpdate += new System.EventHandler(this.cboDisplayFilter_TextUpdate);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 0;
            this.label1.Tag = "Label_SkillGroups";
            this.label1.Text = "Skill Groups";
            // 
            // lblCustomKnowledgeSkillsReminder
            // 
            this.lblCustomKnowledgeSkillsReminder.AutoSize = true;
            this.lblCustomKnowledgeSkillsReminder.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCustomKnowledgeSkillsReminder.Location = new System.Drawing.Point(365, 5);
            this.lblCustomKnowledgeSkillsReminder.Name = "lblCustomKnowledgeSkillsReminder";
            this.lblCustomKnowledgeSkillsReminder.Size = new System.Drawing.Size(398, 13);
            this.lblCustomKnowledgeSkillsReminder.TabIndex = 55;
            this.lblCustomKnowledgeSkillsReminder.Tag = "Label_CustomKnowledgeSkillsReminder";
            this.lblCustomKnowledgeSkillsReminder.Text = "Remember, you can always write in custom skills and specializations!";
            // 
            // lblKnoBwk
            // 
            this.lblKnoBwk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKnoBwk.AutoSize = true;
            this.lblKnoBwk.Location = new System.Drawing.Point(365, 36);
            this.lblKnoBwk.Name = "lblKnoBwk";
            this.lblKnoBwk.Size = new System.Drawing.Size(83, 13);
            this.lblKnoBwk.TabIndex = 53;
            this.lblKnoBwk.Tag = "String_BuyWithKarma";
            this.lblKnoBwk.Text = "Buy With Karma";
            this.lblKnoBwk.Visible = false;
            // 
            // lblKnoKarma
            // 
            this.lblKnoKarma.AutoSize = true;
            this.lblKnoKarma.Location = new System.Drawing.Point(250, 36);
            this.lblKnoKarma.Name = "lblKnoKarma";
            this.lblKnoKarma.Size = new System.Drawing.Size(37, 13);
            this.lblKnoKarma.TabIndex = 54;
            this.lblKnoKarma.Tag = "String_Karma";
            this.lblKnoKarma.Text = "Karma";
            this.lblKnoKarma.Visible = false;
            // 
            // lblKnoSp
            // 
            this.lblKnoSp.AutoSize = true;
            this.lblKnoSp.Location = new System.Drawing.Point(209, 36);
            this.lblKnoSp.Name = "lblKnoSp";
            this.lblKnoSp.Size = new System.Drawing.Size(36, 13);
            this.lblKnoSp.TabIndex = 53;
            this.lblKnoSp.Tag = "String_Points";
            this.lblKnoSp.Text = "Points";
            this.lblKnoSp.Visible = false;
            // 
            // lblKnowledgeSkillPoints
            // 
            this.lblKnowledgeSkillPoints.AutoSize = true;
            this.lblKnowledgeSkillPoints.Location = new System.Drawing.Point(310, 5);
            this.lblKnowledgeSkillPoints.Name = "lblKnowledgeSkillPoints";
            this.lblKnowledgeSkillPoints.Size = new System.Drawing.Size(34, 13);
            this.lblKnowledgeSkillPoints.TabIndex = 38;
            this.lblKnowledgeSkillPoints.Text = "0 of 0";
            // 
            // lblKnowledgeSkillPointsTitle
            // 
            this.lblKnowledgeSkillPointsTitle.AutoSize = true;
            this.lblKnowledgeSkillPointsTitle.Location = new System.Drawing.Point(110, 5);
            this.lblKnowledgeSkillPointsTitle.Name = "lblKnowledgeSkillPointsTitle";
            this.lblKnowledgeSkillPointsTitle.Size = new System.Drawing.Size(194, 13);
            this.lblKnowledgeSkillPointsTitle.TabIndex = 37;
            this.lblKnowledgeSkillPointsTitle.Tag = "Label_FreeKnowledgeSkills";
            this.lblKnowledgeSkillPointsTitle.Text = "Free Knowledge Skill Points Remaining:";
            // 
            // lblKnowledgeSkills
            // 
            this.lblKnowledgeSkills.AutoSize = true;
            this.lblKnowledgeSkills.Location = new System.Drawing.Point(0, 5);
            this.lblKnowledgeSkills.Name = "lblKnowledgeSkills";
            this.lblKnowledgeSkills.Size = new System.Drawing.Size(87, 13);
            this.lblKnowledgeSkills.TabIndex = 4;
            this.lblKnowledgeSkills.Tag = "Label_KnowledgeSkills";
            this.lblKnowledgeSkills.Text = "Knowledge Skills";
            // 
            // btnKnowledge
            // 
            this.btnKnowledge.Location = new System.Drawing.Point(3, 21);
            this.btnKnowledge.Name = "btnKnowledge";
            this.btnKnowledge.Size = new System.Drawing.Size(75, 23);
            this.btnKnowledge.TabIndex = 0;
            this.btnKnowledge.Tag = "Button_AddSkill";
            this.btnKnowledge.Text = "&Add Skill";
            this.btnKnowledge.UseVisualStyleBackColor = true;
            this.btnKnowledge.Click += new System.EventHandler(this.btnKnowledge_Click);
            // 
            // cboSortKnowledge
            // 
            this.cboSortKnowledge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSortKnowledge.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSortKnowledge.FormattingEnabled = true;
            this.cboSortKnowledge.IntegralHeight = false;
            this.cboSortKnowledge.Location = new System.Drawing.Point(476, 23);
            this.cboSortKnowledge.Name = "cboSortKnowledge";
            this.cboSortKnowledge.Size = new System.Drawing.Size(133, 21);
            this.cboSortKnowledge.TabIndex = 55;
            this.cboSortKnowledge.SelectedIndexChanged += new System.EventHandler(this.cboSortKnowledge_SelectedIndexChanged);
            // 
            // cboDisplayFilterKnowledge
            // 
            this.cboDisplayFilterKnowledge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboDisplayFilterKnowledge.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDisplayFilterKnowledge.FormattingEnabled = true;
            this.cboDisplayFilterKnowledge.IntegralHeight = false;
            this.cboDisplayFilterKnowledge.Location = new System.Drawing.Point(613, 23);
            this.cboDisplayFilterKnowledge.Name = "cboDisplayFilterKnowledge";
            this.cboDisplayFilterKnowledge.Size = new System.Drawing.Size(201, 21);
            this.cboDisplayFilterKnowledge.TabIndex = 54;
            this.cboDisplayFilterKnowledge.SelectedIndexChanged += new System.EventHandler(this.cboDisplayFilterKnowledge_SelectedIndexChanged);
            this.cboDisplayFilterKnowledge.TextUpdate += new EventHandler(this.cboDisplayFilterKnowledge_TextUpdate);
            // 
            // SkillsTabUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitSkills);
            this.Name = "SkillsTabUserControl";
            this.Size = new System.Drawing.Size(830, 611);
            this.Load += new System.EventHandler(this.SkillsTabUserControl_Load);
            this.splitSkills.Panel1.ResumeLayout(false);
            this.splitSkills.Panel1.PerformLayout();
            this.splitSkills.Panel2.ResumeLayout(false);
            this.splitSkills.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitSkills)).EndInit();
            this.splitSkills.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitSkills;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboDisplayFilter;
        private System.Windows.Forms.Button btnExotic;
        private System.Windows.Forms.Button btnKnowledge;
        private System.Windows.Forms.Label lblActiveSkills;
        private System.Windows.Forms.Label lblKnowledgeSkillPoints;
        private System.Windows.Forms.Label lblKnowledgeSkillPointsTitle;
        private System.Windows.Forms.Label lblKnowledgeSkills;
        private System.Windows.Forms.ComboBox cboSort;
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
        private System.Windows.Forms.ComboBox cboSortKnowledge;
        private System.Windows.Forms.ComboBox cboDisplayFilterKnowledge;
    }
}
