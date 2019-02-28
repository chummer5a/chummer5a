namespace ChummerHub.Client.UI
{
    partial class SINnerGroupSearch
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
            this.tlpGroupSearch = new System.Windows.Forms.TableLayoutPanel();
            this.gpSearchForGroups = new System.Windows.Forms.GroupBox();
            this.tlpSearchGroups = new System.Windows.Forms.TableLayoutPanel();
            this.lSearchGroupname = new System.Windows.Forms.Label();
            this.tbSearchByAlias = new System.Windows.Forms.TextBox();
            this.lSearchByAlias = new System.Windows.Forms.Label();
            this.lSearchByUsername = new System.Windows.Forms.Label();
            this.tbSearchByUsername = new System.Windows.Forms.TextBox();
            this.bSearch = new System.Windows.Forms.Button();
            this.bCreateGroup = new System.Windows.Forms.Button();
            this.tbSearchGroupname = new System.Windows.Forms.TextBox();
            this.bJoinGroup = new System.Windows.Forms.Button();
            this.gbGroupMembers = new System.Windows.Forms.GroupBox();
            this.lbGroupMembers = new System.Windows.Forms.ListBox();
            this.gpGroupsFound = new System.Windows.Forms.GroupBox();
            this.lbGroupSearchResult = new System.Windows.Forms.ListBox();
            this.tlpGroupSearch.SuspendLayout();
            this.gpSearchForGroups.SuspendLayout();
            this.tlpSearchGroups.SuspendLayout();
            this.gbGroupMembers.SuspendLayout();
            this.gpGroupsFound.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpGroupSearch
            // 
            this.tlpGroupSearch.AutoScroll = true;
            this.tlpGroupSearch.AutoSize = true;
            this.tlpGroupSearch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpGroupSearch.ColumnCount = 2;
            this.tlpGroupSearch.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpGroupSearch.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpGroupSearch.Controls.Add(this.gpSearchForGroups, 0, 0);
            this.tlpGroupSearch.Controls.Add(this.gbGroupMembers, 1, 1);
            this.tlpGroupSearch.Controls.Add(this.gpGroupsFound, 0, 1);
            this.tlpGroupSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpGroupSearch.Location = new System.Drawing.Point(0, 0);
            this.tlpGroupSearch.Name = "tlpGroupSearch";
            this.tlpGroupSearch.RowCount = 2;
            this.tlpGroupSearch.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGroupSearch.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGroupSearch.Size = new System.Drawing.Size(500, 425);
            this.tlpGroupSearch.TabIndex = 0;
            // 
            // gpSearchForGroups
            // 
            this.gpSearchForGroups.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gpSearchForGroups.AutoSize = true;
            this.gpSearchForGroups.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpGroupSearch.SetColumnSpan(this.gpSearchForGroups, 2);
            this.gpSearchForGroups.Controls.Add(this.tlpSearchGroups);
            this.gpSearchForGroups.Location = new System.Drawing.Point(3, 3);
            this.gpSearchForGroups.Name = "gpSearchForGroups";
            this.gpSearchForGroups.Size = new System.Drawing.Size(494, 106);
            this.gpSearchForGroups.TabIndex = 1;
            this.gpSearchForGroups.TabStop = false;
            this.gpSearchForGroups.Text = "search for groups";
            // 
            // tlpSearchGroups
            // 
            this.tlpSearchGroups.AutoScroll = true;
            this.tlpSearchGroups.AutoSize = true;
            this.tlpSearchGroups.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpSearchGroups.ColumnCount = 3;
            this.tlpSearchGroups.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpSearchGroups.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSearchGroups.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpSearchGroups.Controls.Add(this.lSearchGroupname, 0, 2);
            this.tlpSearchGroups.Controls.Add(this.tbSearchByAlias, 1, 1);
            this.tlpSearchGroups.Controls.Add(this.lSearchByAlias, 0, 1);
            this.tlpSearchGroups.Controls.Add(this.lSearchByUsername, 0, 0);
            this.tlpSearchGroups.Controls.Add(this.tbSearchByUsername, 1, 0);
            this.tlpSearchGroups.Controls.Add(this.bSearch, 2, 0);
            this.tlpSearchGroups.Controls.Add(this.bCreateGroup, 2, 1);
            this.tlpSearchGroups.Controls.Add(this.tbSearchGroupname, 1, 2);
            this.tlpSearchGroups.Controls.Add(this.bJoinGroup, 2, 2);
            this.tlpSearchGroups.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSearchGroups.Location = new System.Drawing.Point(3, 16);
            this.tlpSearchGroups.Name = "tlpSearchGroups";
            this.tlpSearchGroups.RowCount = 3;
            this.tlpSearchGroups.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpSearchGroups.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpSearchGroups.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpSearchGroups.Size = new System.Drawing.Size(488, 87);
            this.tlpSearchGroups.TabIndex = 0;
            // 
            // lSearchGroupname
            // 
            this.lSearchGroupname.AutoSize = true;
            this.lSearchGroupname.Dock = System.Windows.Forms.DockStyle.Left;
            this.lSearchGroupname.Location = new System.Drawing.Point(3, 58);
            this.lSearchGroupname.Name = "lSearchGroupname";
            this.lSearchGroupname.Size = new System.Drawing.Size(65, 29);
            this.lSearchGroupname.TabIndex = 6;
            this.lSearchGroupname.Text = "Groupname:";
            this.lSearchGroupname.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbSearchByAlias
            // 
            this.tbSearchByAlias.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSearchByAlias.Location = new System.Drawing.Point(97, 33);
            this.tbSearchByAlias.Name = "tbSearchByAlias";
            this.tbSearchByAlias.Size = new System.Drawing.Size(281, 20);
            this.tbSearchByAlias.TabIndex = 3;
            // 
            // lSearchByAlias
            // 
            this.lSearchByAlias.AutoSize = true;
            this.lSearchByAlias.Dock = System.Windows.Forms.DockStyle.Left;
            this.lSearchByAlias.Location = new System.Drawing.Point(3, 29);
            this.lSearchByAlias.Name = "lSearchByAlias";
            this.lSearchByAlias.Size = new System.Drawing.Size(32, 29);
            this.lSearchByAlias.TabIndex = 1;
            this.lSearchByAlias.Text = "Alias:";
            this.lSearchByAlias.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lSearchByUsername
            // 
            this.lSearchByUsername.AutoSize = true;
            this.lSearchByUsername.Dock = System.Windows.Forms.DockStyle.Left;
            this.lSearchByUsername.Location = new System.Drawing.Point(3, 0);
            this.lSearchByUsername.Name = "lSearchByUsername";
            this.lSearchByUsername.Size = new System.Drawing.Size(88, 29);
            this.lSearchByUsername.TabIndex = 0;
            this.lSearchByUsername.Text = "Username/Email:";
            this.lSearchByUsername.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbSearchByUsername
            // 
            this.tbSearchByUsername.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSearchByUsername.Location = new System.Drawing.Point(97, 4);
            this.tbSearchByUsername.Name = "tbSearchByUsername";
            this.tbSearchByUsername.Size = new System.Drawing.Size(281, 20);
            this.tbSearchByUsername.TabIndex = 2;
            // 
            // bSearch
            // 
            this.bSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.bSearch.AutoSize = true;
            this.bSearch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bSearch.Location = new System.Drawing.Point(384, 3);
            this.bSearch.Name = "bSearch";
            this.bSearch.Size = new System.Drawing.Size(101, 23);
            this.bSearch.TabIndex = 4;
            this.bSearch.Text = "Search";
            this.bSearch.UseVisualStyleBackColor = true;
            this.bSearch.Click += new System.EventHandler(this.bSearch_Click);
            // 
            // bCreateGroup
            // 
            this.bCreateGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.bCreateGroup.AutoSize = true;
            this.bCreateGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bCreateGroup.Location = new System.Drawing.Point(384, 32);
            this.bCreateGroup.Name = "bCreateGroup";
            this.bCreateGroup.Size = new System.Drawing.Size(101, 23);
            this.bCreateGroup.TabIndex = 5;
            this.bCreateGroup.Text = "Create new group";
            this.bCreateGroup.UseVisualStyleBackColor = true;
            this.bCreateGroup.Click += new System.EventHandler(this.bCreateGroup_Click);
            // 
            // tbSearchGroupname
            // 
            this.tbSearchGroupname.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSearchGroupname.Location = new System.Drawing.Point(97, 62);
            this.tbSearchGroupname.Name = "tbSearchGroupname";
            this.tbSearchGroupname.Size = new System.Drawing.Size(281, 20);
            this.tbSearchGroupname.TabIndex = 7;
            // 
            // bJoinGroup
            // 
            this.bJoinGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bJoinGroup.Location = new System.Drawing.Point(384, 61);
            this.bJoinGroup.Name = "bJoinGroup";
            this.bJoinGroup.Size = new System.Drawing.Size(101, 23);
            this.bJoinGroup.TabIndex = 8;
            this.bJoinGroup.Text = "Join Group";
            this.bJoinGroup.UseVisualStyleBackColor = true;
            this.bJoinGroup.Click += new System.EventHandler(this.bJoinGroup_Click);
            // 
            // gbGroupMembers
            // 
            this.gbGroupMembers.Controls.Add(this.lbGroupMembers);
            this.gbGroupMembers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbGroupMembers.Location = new System.Drawing.Point(253, 115);
            this.gbGroupMembers.Name = "gbGroupMembers";
            this.gbGroupMembers.Size = new System.Drawing.Size(244, 307);
            this.gbGroupMembers.TabIndex = 3;
            this.gbGroupMembers.TabStop = false;
            this.gbGroupMembers.Text = "Members";
            // 
            // lbGroupMembers
            // 
            this.lbGroupMembers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbGroupMembers.FormattingEnabled = true;
            this.lbGroupMembers.Location = new System.Drawing.Point(3, 16);
            this.lbGroupMembers.Name = "lbGroupMembers";
            this.lbGroupMembers.Size = new System.Drawing.Size(238, 288);
            this.lbGroupMembers.Sorted = true;
            this.lbGroupMembers.TabIndex = 4;
            // 
            // gpGroupsFound
            // 
            this.gpGroupsFound.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpGroupsFound.Controls.Add(this.lbGroupSearchResult);
            this.gpGroupsFound.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpGroupsFound.Location = new System.Drawing.Point(3, 115);
            this.gpGroupsFound.Name = "gpGroupsFound";
            this.gpGroupsFound.Size = new System.Drawing.Size(244, 307);
            this.gpGroupsFound.TabIndex = 4;
            this.gpGroupsFound.TabStop = false;
            this.gpGroupsFound.Text = "Groups found";
            // 
            // lbGroupSearchResult
            // 
            this.lbGroupSearchResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbGroupSearchResult.FormattingEnabled = true;
            this.lbGroupSearchResult.Location = new System.Drawing.Point(3, 16);
            this.lbGroupSearchResult.Name = "lbGroupSearchResult";
            this.lbGroupSearchResult.Size = new System.Drawing.Size(238, 288);
            this.lbGroupSearchResult.Sorted = true;
            this.lbGroupSearchResult.TabIndex = 3;
            this.lbGroupSearchResult.SelectedIndexChanged += new System.EventHandler(this.lbGroupSearchResult_SelectedIndexChanged);
            // 
            // SINnerGroupSearch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tlpGroupSearch);
            this.Name = "SINnerGroupSearch";
            this.Size = new System.Drawing.Size(500, 425);
            this.tlpGroupSearch.ResumeLayout(false);
            this.tlpGroupSearch.PerformLayout();
            this.gpSearchForGroups.ResumeLayout(false);
            this.gpSearchForGroups.PerformLayout();
            this.tlpSearchGroups.ResumeLayout(false);
            this.tlpSearchGroups.PerformLayout();
            this.gbGroupMembers.ResumeLayout(false);
            this.gpGroupsFound.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpGroupSearch;
        private System.Windows.Forms.GroupBox gpSearchForGroups;
        private System.Windows.Forms.TableLayoutPanel tlpSearchGroups;
        private System.Windows.Forms.TextBox tbSearchByAlias;
        private System.Windows.Forms.Label lSearchByAlias;
        private System.Windows.Forms.Label lSearchByUsername;
        private System.Windows.Forms.TextBox tbSearchByUsername;
        private System.Windows.Forms.Button bSearch;
        private System.Windows.Forms.Button bCreateGroup;
        private System.Windows.Forms.Label lSearchGroupname;
        private System.Windows.Forms.TextBox tbSearchGroupname;
        private System.Windows.Forms.Button bJoinGroup;
        private System.Windows.Forms.GroupBox gbGroupMembers;
        private System.Windows.Forms.ListBox lbGroupMembers;
        private System.Windows.Forms.GroupBox gpGroupsFound;
        private System.Windows.Forms.ListBox lbGroupSearchResult;
    }
}
