namespace Chummer
{
    partial class frmSelectSpell
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
            this.treSpells = new System.Windows.Forms.TreeView();
            this.lblDescriptorsLabel = new System.Windows.Forms.Label();
            this.lblDescriptors = new System.Windows.Forms.Label();
            this.lblTypeLabel = new System.Windows.Forms.Label();
            this.lblType = new System.Windows.Forms.Label();
            this.lblRangeLabel = new System.Windows.Forms.Label();
            this.lblRange = new System.Windows.Forms.Label();
            this.lblDamageLabel = new System.Windows.Forms.Label();
            this.lblDamage = new System.Windows.Forms.Label();
            this.lblDurationLabel = new System.Windows.Forms.Label();
            this.lblDuration = new System.Windows.Forms.Label();
            this.lblDVLabel = new System.Windows.Forms.Label();
            this.lblDV = new System.Windows.Forms.Label();
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearchLabel = new System.Windows.Forms.Label();
            this.cmdOKAdd = new System.Windows.Forms.Button();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.chkLimited = new System.Windows.Forms.CheckBox();
            this.tipTooltip = new TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip();
            this.chkExtended = new System.Windows.Forms.CheckBox();
            this.chkAlchemical = new System.Windows.Forms.CheckBox();
            this.chkFreeBonus = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // treSpells
            // 
            this.treSpells.FullRowSelect = true;
            this.treSpells.HideSelection = false;
            this.treSpells.Location = new System.Drawing.Point(12, 12);
            this.treSpells.Name = "treSpells";
            this.treSpells.Size = new System.Drawing.Size(264, 536);
            this.treSpells.TabIndex = 17;
            this.treSpells.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treSpells_AfterSelect);
            this.treSpells.DoubleClick += new System.EventHandler(this.treSpells_DoubleClick);
            // 
            // lblDescriptorsLabel
            // 
            this.lblDescriptorsLabel.AutoSize = true;
            this.lblDescriptorsLabel.Location = new System.Drawing.Point(282, 60);
            this.lblDescriptorsLabel.Name = "lblDescriptorsLabel";
            this.lblDescriptorsLabel.Size = new System.Drawing.Size(63, 13);
            this.lblDescriptorsLabel.TabIndex = 2;
            this.lblDescriptorsLabel.Tag = "Label_Descriptors";
            this.lblDescriptorsLabel.Text = "Descriptors:";
            // 
            // lblDescriptors
            // 
            this.lblDescriptors.AutoSize = true;
            this.lblDescriptors.Location = new System.Drawing.Point(351, 60);
            this.lblDescriptors.Name = "lblDescriptors";
            this.lblDescriptors.Size = new System.Drawing.Size(66, 13);
            this.lblDescriptors.TabIndex = 3;
            this.lblDescriptors.Text = "[Descriptors]";
            // 
            // lblTypeLabel
            // 
            this.lblTypeLabel.AutoSize = true;
            this.lblTypeLabel.Location = new System.Drawing.Point(282, 83);
            this.lblTypeLabel.Name = "lblTypeLabel";
            this.lblTypeLabel.Size = new System.Drawing.Size(34, 13);
            this.lblTypeLabel.TabIndex = 4;
            this.lblTypeLabel.Tag = "Label_Type";
            this.lblTypeLabel.Text = "Type:";
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(351, 83);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(37, 13);
            this.lblType.TabIndex = 5;
            this.lblType.Text = "[Type]";
            // 
            // lblRangeLabel
            // 
            this.lblRangeLabel.AutoSize = true;
            this.lblRangeLabel.Location = new System.Drawing.Point(282, 105);
            this.lblRangeLabel.Name = "lblRangeLabel";
            this.lblRangeLabel.Size = new System.Drawing.Size(42, 13);
            this.lblRangeLabel.TabIndex = 6;
            this.lblRangeLabel.Tag = "Label_Range";
            this.lblRangeLabel.Text = "Range:";
            // 
            // lblRange
            // 
            this.lblRange.AutoSize = true;
            this.lblRange.Location = new System.Drawing.Point(351, 105);
            this.lblRange.Name = "lblRange";
            this.lblRange.Size = new System.Drawing.Size(45, 13);
            this.lblRange.TabIndex = 7;
            this.lblRange.Text = "[Range]";
            // 
            // lblDamageLabel
            // 
            this.lblDamageLabel.AutoSize = true;
            this.lblDamageLabel.Location = new System.Drawing.Point(282, 127);
            this.lblDamageLabel.Name = "lblDamageLabel";
            this.lblDamageLabel.Size = new System.Drawing.Size(50, 13);
            this.lblDamageLabel.TabIndex = 8;
            this.lblDamageLabel.Tag = "Label_Damage";
            this.lblDamageLabel.Text = "Damage:";
            // 
            // lblDamage
            // 
            this.lblDamage.AutoSize = true;
            this.lblDamage.Location = new System.Drawing.Point(351, 127);
            this.lblDamage.Name = "lblDamage";
            this.lblDamage.Size = new System.Drawing.Size(53, 13);
            this.lblDamage.TabIndex = 9;
            this.lblDamage.Text = "[Damage]";
            // 
            // lblDurationLabel
            // 
            this.lblDurationLabel.AutoSize = true;
            this.lblDurationLabel.Location = new System.Drawing.Point(282, 150);
            this.lblDurationLabel.Name = "lblDurationLabel";
            this.lblDurationLabel.Size = new System.Drawing.Size(50, 13);
            this.lblDurationLabel.TabIndex = 10;
            this.lblDurationLabel.Tag = "Label_Duration";
            this.lblDurationLabel.Text = "Duration:";
            // 
            // lblDuration
            // 
            this.lblDuration.AutoSize = true;
            this.lblDuration.Location = new System.Drawing.Point(351, 150);
            this.lblDuration.Name = "lblDuration";
            this.lblDuration.Size = new System.Drawing.Size(53, 13);
            this.lblDuration.TabIndex = 11;
            this.lblDuration.Text = "[Duration]";
            // 
            // lblDVLabel
            // 
            this.lblDVLabel.AutoSize = true;
            this.lblDVLabel.Location = new System.Drawing.Point(282, 172);
            this.lblDVLabel.Name = "lblDVLabel";
            this.lblDVLabel.Size = new System.Drawing.Size(25, 13);
            this.lblDVLabel.TabIndex = 12;
            this.lblDVLabel.Tag = "Label_DV";
            this.lblDVLabel.Text = "DV:";
            // 
            // lblDV
            // 
            this.lblDV.AutoSize = true;
            this.lblDV.Location = new System.Drawing.Point(351, 172);
            this.lblDV.Name = "lblDV";
            this.lblDV.Size = new System.Drawing.Size(28, 13);
            this.lblDV.TabIndex = 13;
            this.lblDV.Text = "[DV]";
            // 
            // cmdOK
            // 
            this.cmdOK.Location = new System.Drawing.Point(465, 525);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 18;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(384, 525);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 20;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(366, 9);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(174, 20);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
            this.txtSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyUp);
            // 
            // lblSearchLabel
            // 
            this.lblSearchLabel.AutoSize = true;
            this.lblSearchLabel.Location = new System.Drawing.Point(316, 12);
            this.lblSearchLabel.Name = "lblSearchLabel";
            this.lblSearchLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSearchLabel.TabIndex = 0;
            this.lblSearchLabel.Tag = "Label_Search";
            this.lblSearchLabel.Text = "&Search:";
            // 
            // cmdOKAdd
            // 
            this.cmdOKAdd.Location = new System.Drawing.Point(465, 496);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(75, 23);
            this.cmdOKAdd.TabIndex = 19;
            this.cmdOKAdd.Tag = "String_AddMore";
            this.cmdOKAdd.Text = "&Add && More";
            this.cmdOKAdd.UseVisualStyleBackColor = true;
            this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.lblSource.Location = new System.Drawing.Point(333, 252);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 16;
            this.lblSource.Text = "[Source]";
            this.lblSource.Click += new System.EventHandler(this.lblSource_Click);
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(282, 252);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 15;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            // 
            // chkLimited
            // 
            this.chkLimited.AutoSize = true;
            this.chkLimited.Location = new System.Drawing.Point(282, 198);
            this.chkLimited.Name = "chkLimited";
            this.chkLimited.Size = new System.Drawing.Size(85, 17);
            this.chkLimited.TabIndex = 14;
            this.chkLimited.Tag = "Checkbox_SelectSpell_LimitedSpell";
            this.chkLimited.Text = "Limited Spell";
            this.tipTooltip.SetToolTip(this.chkLimited, "Limited Spells require a Fetish to cast but add +2 dice to the Drain Resistance T" +
        "est after casting this Spell.");
            this.chkLimited.UseVisualStyleBackColor = true;
            this.chkLimited.CheckedChanged += new System.EventHandler(this.chkLimited_CheckedChanged);
            // 
            // tipTooltip
            // 
            this.tipTooltip.AllowLinksHandling = true;
            this.tipTooltip.AutoPopDelay = 10000;
            this.tipTooltip.BaseStylesheet = null;
            this.tipTooltip.InitialDelay = 250;
            this.tipTooltip.IsBalloon = true;
            this.tipTooltip.MaximumSize = new System.Drawing.Size(0, 0);
            this.tipTooltip.OwnerDraw = true;
            this.tipTooltip.ReshowDelay = 100;
            this.tipTooltip.TooltipCssClass = "htmltooltip";
            this.tipTooltip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.tipTooltip.ToolTipTitle = "Chummer Help";
            // 
            // chkExtended
            // 
            this.chkExtended.AutoSize = true;
            this.chkExtended.Enabled = false;
            this.chkExtended.Location = new System.Drawing.Point(282, 309);
            this.chkExtended.Name = "chkExtended";
            this.chkExtended.Size = new System.Drawing.Size(97, 17);
            this.chkExtended.TabIndex = 21;
            this.chkExtended.Tag = "Checkbox_SelectSpell_ExtendedSpell";
            this.chkExtended.Text = "Extended Spell";
            this.tipTooltip.SetToolTip(this.chkExtended, "Extended range Spells have a range of (Force x MAG x 10) meters but have their DV" +
        " increased by +2.");
            this.chkExtended.UseVisualStyleBackColor = true;
            this.chkExtended.Visible = false;
            this.chkExtended.CheckedChanged += new System.EventHandler(this.chkExtended_CheckedChanged);
            // 
            // chkAlchemical
            // 
            this.chkAlchemical.AutoSize = true;
            this.chkAlchemical.Location = new System.Drawing.Point(282, 221);
            this.chkAlchemical.Name = "chkAlchemical";
            this.chkAlchemical.Size = new System.Drawing.Size(134, 17);
            this.chkAlchemical.TabIndex = 15;
            this.chkAlchemical.Tag = "Checkbox_SelectSpell_Alchemical";
            this.chkAlchemical.Text = "Alchemical Preparation";
            this.tipTooltip.SetToolTip(this.chkAlchemical, "Extended range Spells have a range of (Force x MAG x 10) meters but have their DV" +
        " increased by +2.");
            this.chkAlchemical.UseVisualStyleBackColor = true;
            // 
            // chkFreeBonus
            // 
            this.chkFreeBonus.AutoSize = true;
            this.chkFreeBonus.Location = new System.Drawing.Point(282, 286);
            this.chkFreeBonus.Name = "chkFreeBonus";
            this.chkFreeBonus.Size = new System.Drawing.Size(50, 17);
            this.chkFreeBonus.TabIndex = 22;
            this.chkFreeBonus.Tag = "";
            this.chkFreeBonus.Text = "Free!";
            this.chkFreeBonus.UseVisualStyleBackColor = true;
            // 
            // frmSelectSpell
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(548, 560);
            this.Controls.Add(this.chkFreeBonus);
            this.Controls.Add(this.chkAlchemical);
            this.Controls.Add(this.chkExtended);
            this.Controls.Add(this.chkLimited);
            this.Controls.Add(this.lblSource);
            this.Controls.Add(this.lblSourceLabel);
            this.Controls.Add(this.cmdOKAdd);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.lblSearchLabel);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.lblDV);
            this.Controls.Add(this.lblDVLabel);
            this.Controls.Add(this.lblDuration);
            this.Controls.Add(this.lblDurationLabel);
            this.Controls.Add(this.lblDamage);
            this.Controls.Add(this.lblDamageLabel);
            this.Controls.Add(this.lblRange);
            this.Controls.Add(this.lblRangeLabel);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.lblTypeLabel);
            this.Controls.Add(this.lblDescriptors);
            this.Controls.Add(this.lblDescriptorsLabel);
            this.Controls.Add(this.treSpells);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectSpell";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectSpell";
            this.Text = "Select a Spell";
            this.Load += new System.EventHandler(this.frmSelectSpell_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treSpells;
        private System.Windows.Forms.Label lblDescriptorsLabel;
        private System.Windows.Forms.Label lblDescriptors;
        private System.Windows.Forms.Label lblTypeLabel;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.Label lblRangeLabel;
        private System.Windows.Forms.Label lblRange;
        private System.Windows.Forms.Label lblDamageLabel;
        private System.Windows.Forms.Label lblDamage;
        private System.Windows.Forms.Label lblDurationLabel;
        private System.Windows.Forms.Label lblDuration;
        private System.Windows.Forms.Label lblDVLabel;
        private System.Windows.Forms.Label lblDV;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearchLabel;
        private System.Windows.Forms.Button cmdOKAdd;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblSourceLabel;
        private System.Windows.Forms.CheckBox chkLimited;
        private TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip tipTooltip;
        private System.Windows.Forms.CheckBox chkExtended;
        private System.Windows.Forms.CheckBox chkAlchemical;
        private System.Windows.Forms.CheckBox chkFreeBonus;
    }
}