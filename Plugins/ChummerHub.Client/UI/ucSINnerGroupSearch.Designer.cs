using System.Windows.Forms;

namespace ChummerHub.Client.UI
{
    partial class ucSINnerGroupSearch
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
            if(disposing && (components != null))
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ucSINnerGroupSearch));
            this.tlpGroupSearch = new System.Windows.Forms.TableLayoutPanel();
            this.gpSearchForGroups = new System.Windows.Forms.GroupBox();
            this.tlpSearchGroups = new System.Windows.Forms.TableLayoutPanel();
            this.lSearchGroupname = new System.Windows.Forms.Label();
            this.bSearch = new System.Windows.Forms.Button();
            this.tbSearchGroupname = new System.Windows.Forms.TextBox();
            this.lSINner = new System.Windows.Forms.Label();
            this.lSINnerName = new System.Windows.Forms.Label();
            this.cbShowMembers = new System.Windows.Forms.CheckBox();
            this.tlpSearchGroupsBottom = new System.Windows.Forms.TableLayoutPanel();
            this.bCreateGroup = new System.Windows.Forms.Button();
            this.bViewGroup = new System.Windows.Forms.Button();
            this.bJoinGroup = new System.Windows.Forms.Button();
            this.gpGroupsFound = new System.Windows.Forms.GroupBox();
            this.tlpGroupsFoundPanel = new System.Windows.Forms.TableLayoutPanel();
            this.tvGroupSearchResult = new System.Windows.Forms.TreeView();
            this.lGroupExplanation = new System.Windows.Forms.Label();
            this.tlpGroupsFoundPanelButtons = new System.Windows.Forms.TableLayoutPanel();
            this.bGroupFoundLoadInCharacterRoster = new System.Windows.Forms.Button();
            this.bGroupsFoundDeleteGroup = new System.Windows.Forms.Button();
            this.tlpGroupSearch.SuspendLayout();
            this.gpSearchForGroups.SuspendLayout();
            this.tlpSearchGroups.SuspendLayout();
            this.tlpSearchGroupsBottom.SuspendLayout();
            this.gpGroupsFound.SuspendLayout();
            this.tlpGroupsFoundPanel.SuspendLayout();
            this.tlpGroupsFoundPanelButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpGroupSearch
            // 
            this.tlpGroupSearch.AutoScroll = true;
            this.tlpGroupSearch.AutoSize = true;
            this.tlpGroupSearch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpGroupSearch.ColumnCount = 1;
            this.tlpGroupSearch.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGroupSearch.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpGroupSearch.Controls.Add(this.gpSearchForGroups, 0, 0);
            this.tlpGroupSearch.Controls.Add(this.gpGroupsFound, 0, 1);
            this.tlpGroupSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpGroupSearch.Location = new System.Drawing.Point(0, 0);
            this.tlpGroupSearch.Name = "tlpGroupSearch";
            this.tlpGroupSearch.RowCount = 2;
            this.tlpGroupSearch.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGroupSearch.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGroupSearch.Size = new System.Drawing.Size(414, 334);
            this.tlpGroupSearch.TabIndex = 0;
            this.tlpGroupSearch.VisibleChanged += new System.EventHandler(this.TlpGroupSearch_VisibleChanged);
            // 
            // gpSearchForGroups
            // 
            this.gpSearchForGroups.AutoSize = true;
            this.gpSearchForGroups.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpSearchForGroups.Controls.Add(this.tlpSearchGroups);
            this.gpSearchForGroups.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpSearchForGroups.Location = new System.Drawing.Point(3, 3);
            this.gpSearchForGroups.Name = "gpSearchForGroups";
            this.gpSearchForGroups.Size = new System.Drawing.Size(408, 106);
            this.gpSearchForGroups.TabIndex = 1;
            this.gpSearchForGroups.TabStop = false;
            this.gpSearchForGroups.Text = "Search Groups";
            // 
            // tlpSearchGroups
            // 
            this.tlpSearchGroups.AutoSize = true;
            this.tlpSearchGroups.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpSearchGroups.ColumnCount = 3;
            this.tlpSearchGroups.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpSearchGroups.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSearchGroups.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpSearchGroups.Controls.Add(this.lSearchGroupname, 0, 1);
            this.tlpSearchGroups.Controls.Add(this.bSearch, 2, 1);
            this.tlpSearchGroups.Controls.Add(this.tbSearchGroupname, 1, 1);
            this.tlpSearchGroups.Controls.Add(this.lSINner, 0, 0);
            this.tlpSearchGroups.Controls.Add(this.lSINnerName, 1, 0);
            this.tlpSearchGroups.Controls.Add(this.cbShowMembers, 2, 0);
            this.tlpSearchGroups.Controls.Add(this.tlpSearchGroupsBottom, 0, 2);
            this.tlpSearchGroups.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSearchGroups.Location = new System.Drawing.Point(3, 16);
            this.tlpSearchGroups.Name = "tlpSearchGroups";
            this.tlpSearchGroups.RowCount = 3;
            this.tlpSearchGroups.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpSearchGroups.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpSearchGroups.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpSearchGroups.Size = new System.Drawing.Size(402, 87);
            this.tlpSearchGroups.TabIndex = 0;
            // 
            // lSearchGroupname
            // 
            this.lSearchGroupname.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lSearchGroupname.AutoSize = true;
            this.lSearchGroupname.Location = new System.Drawing.Point(3, 37);
            this.lSearchGroupname.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lSearchGroupname.Name = "lSearchGroupname";
            this.lSearchGroupname.Size = new System.Drawing.Size(70, 13);
            this.lSearchGroupname.TabIndex = 6;
            this.lSearchGroupname.Text = "Group Name:";
            this.lSearchGroupname.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // bSearch
            // 
            this.bSearch.AutoSize = true;
            this.bSearch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bSearch.Location = new System.Drawing.Point(300, 32);
            this.bSearch.Name = "bSearch";
            this.bSearch.Size = new System.Drawing.Size(99, 23);
            this.bSearch.TabIndex = 4;
            this.bSearch.Text = "Search";
            this.bSearch.UseVisualStyleBackColor = true;
            this.bSearch.Click += new System.EventHandler(this.bSearch_Click);
            // 
            // tbSearchGroupname
            // 
            this.tbSearchGroupname.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSearchGroupname.Location = new System.Drawing.Point(79, 33);
            this.tbSearchGroupname.Name = "tbSearchGroupname";
            this.tbSearchGroupname.Size = new System.Drawing.Size(215, 20);
            this.tbSearchGroupname.TabIndex = 7;
            this.tbSearchGroupname.TextChanged += new System.EventHandler(this.TbSearchGroupname_TextChanged);
            this.tbSearchGroupname.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TbSearchGroupname_KeyDown);
            // 
            // lSINner
            // 
            this.lSINner.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lSINner.AutoSize = true;
            this.lSINner.Location = new System.Drawing.Point(30, 8);
            this.lSINner.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lSINner.Name = "lSINner";
            this.lSINner.Size = new System.Drawing.Size(43, 13);
            this.lSINner.TabIndex = 0;
            this.lSINner.Text = "SINner:";
            // 
            // lSINnerName
            // 
            this.lSINnerName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lSINnerName.AutoSize = true;
            this.lSINnerName.Location = new System.Drawing.Point(79, 8);
            this.lSINnerName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lSINnerName.Name = "lSINnerName";
            this.lSINnerName.Size = new System.Drawing.Size(39, 13);
            this.lSINnerName.TabIndex = 10;
            this.lSINnerName.Text = "not set";
            // 
            // cbShowMembers
            // 
            this.cbShowMembers.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cbShowMembers.AutoSize = true;
            this.cbShowMembers.Location = new System.Drawing.Point(300, 6);
            this.cbShowMembers.Name = "cbShowMembers";
            this.cbShowMembers.Size = new System.Drawing.Size(99, 17);
            this.cbShowMembers.TabIndex = 11;
            this.cbShowMembers.Text = "Show Members";
            this.cbShowMembers.UseVisualStyleBackColor = true;
            this.cbShowMembers.CheckedChanged += new System.EventHandler(this.CbShowMembers_CheckedChanged);
            // 
            // tlpSearchGroupsBottom
            // 
            this.tlpSearchGroupsBottom.AutoSize = true;
            this.tlpSearchGroupsBottom.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpSearchGroupsBottom.ColumnCount = 3;
            this.tlpSearchGroups.SetColumnSpan(this.tlpSearchGroupsBottom, 3);
            this.tlpSearchGroupsBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpSearchGroupsBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpSearchGroupsBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpSearchGroupsBottom.Controls.Add(this.bCreateGroup, 0, 0);
            this.tlpSearchGroupsBottom.Controls.Add(this.bViewGroup, 1, 0);
            this.tlpSearchGroupsBottom.Controls.Add(this.bJoinGroup, 2, 0);
            this.tlpSearchGroupsBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSearchGroupsBottom.Location = new System.Drawing.Point(0, 58);
            this.tlpSearchGroupsBottom.Margin = new System.Windows.Forms.Padding(0);
            this.tlpSearchGroupsBottom.Name = "tlpSearchGroupsBottom";
            this.tlpSearchGroupsBottom.RowCount = 1;
            this.tlpSearchGroupsBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSearchGroupsBottom.Size = new System.Drawing.Size(402, 29);
            this.tlpSearchGroupsBottom.TabIndex = 12;
            // 
            // bCreateGroup
            // 
            this.bCreateGroup.AutoSize = true;
            this.bCreateGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bCreateGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bCreateGroup.Location = new System.Drawing.Point(3, 3);
            this.bCreateGroup.Name = "bCreateGroup";
            this.bCreateGroup.Size = new System.Drawing.Size(128, 23);
            this.bCreateGroup.TabIndex = 5;
            this.bCreateGroup.Text = "Create a Group";
            this.bCreateGroup.UseVisualStyleBackColor = true;
            this.bCreateGroup.Click += new System.EventHandler(this.bCreateGroup_Click);
            // 
            // bViewGroup
            // 
            this.bViewGroup.AutoSize = true;
            this.bViewGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bViewGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bViewGroup.Location = new System.Drawing.Point(137, 3);
            this.bViewGroup.Name = "bViewGroup";
            this.bViewGroup.Size = new System.Drawing.Size(128, 23);
            this.bViewGroup.TabIndex = 9;
            this.bViewGroup.Text = "View Group Description";
            this.bViewGroup.UseVisualStyleBackColor = true;
            this.bViewGroup.Click += new System.EventHandler(this.BViewGroup_Click);
            // 
            // bJoinGroup
            // 
            this.bJoinGroup.AutoSize = true;
            this.bJoinGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bJoinGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bJoinGroup.Location = new System.Drawing.Point(271, 3);
            this.bJoinGroup.Name = "bJoinGroup";
            this.bJoinGroup.Size = new System.Drawing.Size(128, 23);
            this.bJoinGroup.TabIndex = 8;
            this.bJoinGroup.Text = "Join Group";
            this.bJoinGroup.UseVisualStyleBackColor = true;
            this.bJoinGroup.Click += new System.EventHandler(this.bJoinGroup_Click);
            // 
            // gpGroupsFound
            // 
            this.gpGroupsFound.Controls.Add(this.tlpGroupsFoundPanel);
            this.gpGroupsFound.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpGroupsFound.Location = new System.Drawing.Point(3, 115);
            this.gpGroupsFound.Name = "gpGroupsFound";
            this.gpGroupsFound.Size = new System.Drawing.Size(408, 216);
            this.gpGroupsFound.TabIndex = 4;
            this.gpGroupsFound.TabStop = false;
            this.gpGroupsFound.Text = "Groups Found";
            // 
            // tlpGroupsFoundPanel
            // 
            this.tlpGroupsFoundPanel.ColumnCount = 1;
            this.tlpGroupsFoundPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpGroupsFoundPanel.Controls.Add(this.tvGroupSearchResult, 0, 0);
            this.tlpGroupsFoundPanel.Controls.Add(this.lGroupExplanation, 0, 1);
            this.tlpGroupsFoundPanel.Controls.Add(this.tlpGroupsFoundPanelButtons, 0, 2);
            this.tlpGroupsFoundPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpGroupsFoundPanel.Location = new System.Drawing.Point(3, 16);
            this.tlpGroupsFoundPanel.Name = "tlpGroupsFoundPanel";
            this.tlpGroupsFoundPanel.RowCount = 3;
            this.tlpGroupsFoundPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGroupsFoundPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGroupsFoundPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGroupsFoundPanel.Size = new System.Drawing.Size(402, 197);
            this.tlpGroupsFoundPanel.TabIndex = 4;
            // 
            // tvGroupSearchResult
            // 
            this.tvGroupSearchResult.AllowDrop = true;
            this.tvGroupSearchResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvGroupSearchResult.Location = new System.Drawing.Point(3, 3);
            this.tvGroupSearchResult.Name = "tvGroupSearchResult";
            this.tvGroupSearchResult.Size = new System.Drawing.Size(396, 100);
            this.tvGroupSearchResult.Sorted = true;
            this.tvGroupSearchResult.TabIndex = 3;
            this.tvGroupSearchResult.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.TvGroupSearchResult_ItemDrag);
            this.tvGroupSearchResult.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TvGroupSearchResult_AfterSelect);
            this.tvGroupSearchResult.DragDrop += new System.Windows.Forms.DragEventHandler(this.TvGroupSearchResult_DragDrop);
            this.tvGroupSearchResult.DragEnter += new System.Windows.Forms.DragEventHandler(this.TvGroupSearchResult_DragEnter);
            // 
            // lGroupExplanation
            // 
            this.lGroupExplanation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lGroupExplanation.Location = new System.Drawing.Point(3, 112);
            this.lGroupExplanation.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lGroupExplanation.Name = "lGroupExplanation";
            this.lGroupExplanation.Size = new System.Drawing.Size(396, 50);
            this.lGroupExplanation.TabIndex = 6;
            this.lGroupExplanation.Text = resources.GetString("lGroupExplanation.Text");
            // 
            // tlpGroupsFoundPanelButtons
            // 
            this.tlpGroupsFoundPanelButtons.AutoSize = true;
            this.tlpGroupsFoundPanelButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpGroupsFoundPanelButtons.ColumnCount = 2;
            this.tlpGroupsFoundPanelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpGroupsFoundPanelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpGroupsFoundPanelButtons.Controls.Add(this.bGroupFoundLoadInCharacterRoster, 0, 0);
            this.tlpGroupsFoundPanelButtons.Controls.Add(this.bGroupsFoundDeleteGroup, 1, 0);
            this.tlpGroupsFoundPanelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpGroupsFoundPanelButtons.Location = new System.Drawing.Point(0, 168);
            this.tlpGroupsFoundPanelButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpGroupsFoundPanelButtons.Name = "tlpGroupsFoundPanelButtons";
            this.tlpGroupsFoundPanelButtons.RowCount = 1;
            this.tlpGroupsFoundPanelButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpGroupsFoundPanelButtons.Size = new System.Drawing.Size(402, 29);
            this.tlpGroupsFoundPanelButtons.TabIndex = 7;
            // 
            // bGroupFoundLoadInCharacterRoster
            // 
            this.bGroupFoundLoadInCharacterRoster.AutoSize = true;
            this.bGroupFoundLoadInCharacterRoster.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bGroupFoundLoadInCharacterRoster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bGroupFoundLoadInCharacterRoster.Location = new System.Drawing.Point(3, 3);
            this.bGroupFoundLoadInCharacterRoster.Name = "bGroupFoundLoadInCharacterRoster";
            this.bGroupFoundLoadInCharacterRoster.Size = new System.Drawing.Size(195, 23);
            this.bGroupFoundLoadInCharacterRoster.TabIndex = 4;
            this.bGroupFoundLoadInCharacterRoster.Text = "Show in Character Roster";
            this.bGroupFoundLoadInCharacterRoster.UseVisualStyleBackColor = true;
            this.bGroupFoundLoadInCharacterRoster.Click += new System.EventHandler(this.BGroupFoundLoadInCharacterRoster_Click);
            // 
            // bGroupsFoundDeleteGroup
            // 
            this.bGroupsFoundDeleteGroup.AutoSize = true;
            this.bGroupsFoundDeleteGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bGroupsFoundDeleteGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bGroupsFoundDeleteGroup.Location = new System.Drawing.Point(204, 3);
            this.bGroupsFoundDeleteGroup.Name = "bGroupsFoundDeleteGroup";
            this.bGroupsFoundDeleteGroup.Size = new System.Drawing.Size(195, 23);
            this.bGroupsFoundDeleteGroup.TabIndex = 5;
            this.bGroupsFoundDeleteGroup.Text = "Delete Group";
            this.bGroupsFoundDeleteGroup.UseVisualStyleBackColor = true;
            this.bGroupsFoundDeleteGroup.Click += new System.EventHandler(this.BGroupsFoundDeleteGroup_Click);
            // 
            // ucSINnerGroupSearch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tlpGroupSearch);
            this.Name = "ucSINnerGroupSearch";
            this.Size = new System.Drawing.Size(414, 334);
            this.VisibleChanged += new System.EventHandler(this.SINnerGroupSearch_VisibleChanged);
            this.tlpGroupSearch.ResumeLayout(false);
            this.tlpGroupSearch.PerformLayout();
            this.gpSearchForGroups.ResumeLayout(false);
            this.gpSearchForGroups.PerformLayout();
            this.tlpSearchGroups.ResumeLayout(false);
            this.tlpSearchGroups.PerformLayout();
            this.tlpSearchGroupsBottom.ResumeLayout(false);
            this.tlpSearchGroupsBottom.PerformLayout();
            this.gpGroupsFound.ResumeLayout(false);
            this.tlpGroupsFoundPanel.ResumeLayout(false);
            this.tlpGroupsFoundPanel.PerformLayout();
            this.tlpGroupsFoundPanelButtons.ResumeLayout(false);
            this.tlpGroupsFoundPanelButtons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpGroupSearch;
        private System.Windows.Forms.GroupBox gpSearchForGroups;
        private System.Windows.Forms.TableLayoutPanel tlpSearchGroups;
        private System.Windows.Forms.Button bSearch;
        private System.Windows.Forms.Button bCreateGroup;
        private System.Windows.Forms.Label lSearchGroupname;
        private System.Windows.Forms.TextBox tbSearchGroupname;
        private System.Windows.Forms.Button bJoinGroup;
        private System.Windows.Forms.GroupBox gpGroupsFound;
        private System.Windows.Forms.TreeView tvGroupSearchResult;
        private TableLayoutPanel tlpGroupsFoundPanel;
        private Button bViewGroup;
        private Label lSINner;
        private Label lSINnerName;
        private CheckBox cbShowMembers;
        private Label lGroupExplanation;
        private TableLayoutPanel tlpGroupsFoundPanelButtons;
        private Button bGroupFoundLoadInCharacterRoster;
        private Button bGroupsFoundDeleteGroup;
        private TableLayoutPanel tlpSearchGroupsBottom;
    }
}
